using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BepInEx;
using TCGProfiler.TestCases;
using TCGProfiler.Testing;
using Unity.Profiling;
using UnityEngine;

namespace TCGProfiler;

public sealed class TcgProfiler : MonoBehaviour
{
    /*
     * ====================================================================
     * Test configuration
     * ====================================================================
     */

    private readonly List<TestCase> _tests = new();

    private QualitySnapshot? _baselineQuality;

    private SceneSnapshot? _baselineScene;
    private ProfilerRecorder _batches;

    private ProfilerRecorder _drawCalls;

    /*
     * Recorder counters.
     */

    private ProfilerRecorder _mainThread;

    /*
     * Suite state.
     */

    private bool _running;
    private ProfilerRecorder _setPassCalls;

    private string _summaryPath = string.Empty;
    private ProfilerRecorder _triangles;
    private ProfilerRecorder _vertices;

    /*
     * Character caches.
     */

    public List<Animator>
        CharacterAnimators { get; } = new();

    public List<Renderer>
        CharacterRenderers { get; } = new();

    public List<SkinnedMeshRenderer>
        CharacterSkinnedRenderers { get; } = new();

    public string CharacterRendererResultsPath { get; private set; } = string.Empty;

    public string CharacterRendererSummaryPath { get; private set; } = string.Empty;

    public string ActiveCharacterRendererResultsPath { get; private set; } = string.Empty;

    /*
     * Profiling.
     */

    public ManagedMethodProfiler
        ManagedMethodProfiler { get; } = new();

    public string MethodResultsPath { get; private set; } = string.Empty;

    /*
     * ====================================================================
     * Lifecycle
     * ====================================================================
     */

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        CreateTests();

        CreateRecorders();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10) &&
            !_running)
            StartCoroutine(
                RunSuite()
            );
    }

    private void OnDestroy()
    {
        DisposeRecorder(
            ref _mainThread
        );

        DisposeRecorder(
            ref _drawCalls
        );

        DisposeRecorder(
            ref _batches
        );

        DisposeRecorder(
            ref _setPassCalls
        );

        DisposeRecorder(
            ref _triangles
        );

        DisposeRecorder(
            ref _vertices
        );

        ManagedMethodProfiler.Disable();
    }

    /*
     * ====================================================================
     * Tests
     * ====================================================================
     */

    private void CreateTests()
    {
        _tests.Add(
            new BaselineTestCase()
        );

        _tests.Add(
            new CheapCharacterApparelShaderTestCase(
                enabled: true
            )
        );

        _tests.Add(
            new CheapCharacterSkinShaderTestCase(
                enabled: true
            )
        );

        _tests.Add(
            new CheapCharacterToonyColorsShaderTestCase(
                enabled: true
            )
        );

        _tests.Add(
            new CheapCharacterTransparentShaderTestCase(
                enabled: true
            )
        );

        _tests.Add(
            new CheapCharacterHairShaderTestCase(
                enabled: true
            )
        );

        _tests.Add(
            new CharacterCheapMaterialTestCase(
                enabled: true
            )
        );

        _tests.Add(
            new DisableCharacterRenderingTestCase(
                enabled: true
            )
        );
    }

    /*
     * ====================================================================
     * Suite
     * ====================================================================
     */

    private IEnumerator RunSuite()
    {
        _running = true;

        try
        {
            var timestamp =
                DateTime.Now.ToString(
                    "yyyyMMdd_HHmmss"
                );

            var directory =
                Path.Combine(
                    Paths.ConfigPath,
                    "TCGProfiler",
                    "Results"
                );

            Directory.CreateDirectory(
                directory
            );

            _summaryPath =
                Path.Combine(
                    directory,
                    $"TCGProfiler_{timestamp}_summary.csv"
                );

            MethodResultsPath =
                Path.Combine(
                    directory,
                    $"TCGProfiler_{timestamp}_methods.csv"
                );

            CharacterRendererResultsPath =
                Path.Combine(
                    directory,
                    $"TCGProfiler_{timestamp}_character_renderers.csv"
                );

            CharacterRendererSummaryPath =
                Path.Combine(
                    directory,
                    $"TCGProfiler_{timestamp}_character_renderer_summary.csv"
                );

            ActiveCharacterRendererResultsPath =
                Path.Combine(
                    directory,
                    $"TCGProfiler_{timestamp}_active_character_renderers.csv"
                );

            Debug.Log(
                "[TCGProfiler] Starting benchmark suite."
            );

            Debug.Log(
                $"[TCGProfiler] Results: {_summaryPath}"
            );

            /*
             * Capture the exact world state we intend
             * to return to between cases.
             */

            _baselineQuality =
                new QualitySnapshot();

            DiscoverCharacters();

            _baselineScene =
                new SceneSnapshot(this);

            WriteSummaryHeader();

            foreach (var test in _tests)
            {
                if (!test.Enabled) continue;

                yield return RunTest(
                    test
                );
            }

            RestoreBaseline();

            Debug.Log(
                "[TCGProfiler] Benchmark suite complete."
            );

            Debug.Log(
                $"[TCGProfiler] Summary: {_summaryPath}"
            );

            Debug.Log(
                $"[TCGProfiler] Managed methods: {MethodResultsPath}"
            );
        }
        finally
        {
            RestoreBaseline();

            _running = false;
        }
    }

    private IEnumerator RunTest(
        TestCase test)
    {
        Debug.Log(
            "[TCGProfiler] --------------------------------"
        );

        Debug.Log(
            $"[TCGProfiler] Preparing: {test.Name}"
        );

        /*
         * Every case begins at the exact suite baseline.
         */

        RestoreBaseline();

        /*
         * Let restoration settle for one frame.
         */

        yield return null;

        test.Setup(this);

        Debug.Log(
            $"[TCGProfiler] Soaking {test.Name} " +
            $"for {test.SoakDuration:F1}s..."
        );

        yield return WaitUnscaled(
            test.SoakDuration
        );

        test.BeginMeasurement(this);

        /*
         * Measurement starts only after the scene
         * has settled.
         */

        var result =
            new TestResult(
                test.Name
            );

        foreach (var pair in
                 test.GetMetadata(this))
            result.Metadata[pair.Key] =
                pair.Value;

        /*
         * Add actual effective settings too.
         *
         * This is important because a test may cause
         * Unity/game code to modify another property.
         */

        AddEffectiveSettings(
            result
        );

        Debug.Log(
            $"[TCGProfiler] Measuring {test.Name} " +
            $"for {test.TestDuration:F1}s..."
        );

        var elapsed = 0.0f;

        while (elapsed <
               test.TestDuration)
        {
            var sample =
                CaptureSample();

            test.Sample(
                this,
                sample
            );

            result.Samples.Add(
                sample
            );

            elapsed +=
                Time.unscaledDeltaTime;

            yield return null;
        }

        test.Complete(
            this,
            result
        );

        WriteResult(
            result
        );

        Debug.Log(
            $"[TCGProfiler] {test.Name}: " +
            $"{result.AverageFps:F2} FPS, " +
            $"{result.AverageFrameTimeMs:F2} ms, " +
            $"1% low {result.OnePercentLowFps:F2} FPS"
        );

        test.Teardown(
            this
        );

        RestoreBaseline();

        /*
         * One frame at restored state before moving on.
         */

        yield return null;
    }

    /*
     * ====================================================================
     * Sample capture
     * ====================================================================
     */

    private TestSample CaptureSample()
    {
        return new TestSample
        {
            FrameTimeMs =
                Time.unscaledDeltaTime *
                1000.0,

            MainThreadMs =
                ReadTime(
                    _mainThread
                ),

            DrawCalls =
                ReadCounter(
                    _drawCalls
                ),

            Batches =
                ReadCounter(
                    _batches
                ),

            SetPassCalls =
                ReadCounter(
                    _setPassCalls
                ),

            Triangles =
                ReadCounter(
                    _triangles
                ),

            Vertices =
                ReadCounter(
                    _vertices
                )
        };
    }

    /*
     * ====================================================================
     * Characters
     * ====================================================================
     */

    public void DiscoverCharacters()
    {
        CharacterAnimators.Clear();
        CharacterRenderers.Clear();
        CharacterSkinnedRenderers.Clear();

        var renderers =
            Resources.FindObjectsOfTypeAll<Renderer>();

        foreach (var renderer in renderers)
        {
            if (!IsSceneComponent(
                    renderer))
                continue;

            if (!IsCharacterTransform(
                    renderer.transform))
                continue;

            CharacterRenderers.Add(
                renderer
            );

            if (renderer is
                SkinnedMeshRenderer skinned)
                CharacterSkinnedRenderers.Add(
                    skinned
                );
        }

        var animators =
            Resources.FindObjectsOfTypeAll<Animator>();

        foreach (var animator in animators)
        {
            if (!IsSceneComponent(
                    animator))
                continue;

            if (!IsCharacterTransform(
                    animator.transform))
                continue;

            CharacterAnimators.Add(
                animator
            );
        }

        Debug.Log(
            $"[TCGProfiler] Characters: " +
            $"{CharacterAnimators.Count} animators, " +
            $"{CharacterRenderers.Count} renderers, " +
            $"{CharacterSkinnedRenderers.Count} skinned renderers"
        );
    }

    private static bool IsCharacterTransform(
        Transform transform)
    {
        var current =
            transform;

        while (current != null)
        {
            foreach (var component in
                     current.GetComponents<Component>())
            {
                if (component == null) continue;

                var fullName =
                    component
                        .GetType()
                        .FullName;

                if (fullName != null &&
                    fullName.StartsWith(
                        "CC.",
                        StringComparison.Ordinal))
                    return true;
            }

            current =
                current.parent;
        }

        return false;
    }

    /*
     * ====================================================================
     * Baseline restoration
     * ====================================================================
     */

    private void RestoreBaseline()
    {
        _baselineQuality?.Restore();

        _baselineScene?.Restore();
    }

    /*
     * ====================================================================
     * Output
     * ====================================================================
     */

    private void WriteSummaryHeader()
    {
        File.WriteAllText(
            _summaryPath,
            "Test," +
            "Samples," +
            "AverageFPS," +
            "OnePercentLowFPS," +
            "AverageFrameMs," +
            "MedianFrameMs," +
            "P95FrameMs," +
            "P99FrameMs," +
            "AverageMainThreadMs," +
            "AverageDrawCalls," +
            "AverageBatches," +
            "AverageSetPassCalls," +
            "AverageTriangles," +
            "AverageVertices," +
            "QualityLevel," +
            "QualityName," +
            "AntiAliasing," +
            "ShadowCascades," +
            "ShadowResolution," +
            "ShadowDistance," +
            "LODBias," +
            "PixelLightCount," +
            "Metadata" +
            Environment.NewLine
        );
    }

    private void WriteResult(
        TestResult result)
    {
        var metadata =
            string.Join(
                ";",
                result.Metadata
                    .Select(pair =>
                        $"{pair.Key}={pair.Value}"
                    )
            );

        var qualityIndex =
            QualitySettings.GetQualityLevel();

        var qualityName =
            qualityIndex >= 0 &&
            qualityIndex <
            QualitySettings.names.Length
                ? QualitySettings
                    .names[qualityIndex]
                : "Unknown";

        var line =
            string.Join(
                ",",
                Csv(result.Name),
                result.Samples.Count,
                F(result.AverageFps),
                F(result.OnePercentLowFps),
                F(result.AverageFrameTimeMs),
                F(result.MedianFrameTimeMs),
                F(result.P95FrameTimeMs),
                F(result.P99FrameTimeMs),
                F(result.AverageMainThreadMs),
                F(result.AverageDrawCalls),
                F(result.AverageBatches),
                F(result.AverageSetPassCalls),
                F(result.AverageTriangles),
                F(result.AverageVertices),
                qualityIndex,
                Csv(qualityName),
                QualitySettings.antiAliasing,
                QualitySettings.shadowCascades,
                Csv(
                    QualitySettings
                        .shadowResolution
                        .ToString()
                ),
                F(
                    QualitySettings
                        .shadowDistance
                ),
                F(
                    QualitySettings
                        .lodBias
                ),
                QualitySettings
                    .pixelLightCount,
                Csv(metadata)
            );

        File.AppendAllText(
            _summaryPath,
            line +
            Environment.NewLine
        );
    }

    private static string F(
        double value)
    {
        return value.ToString(
            "F4",
            CultureInfo.InvariantCulture
        );
    }

    private static string Csv(
        string value)
    {
        return
            "\"" +
            value.Replace(
                "\"",
                "\"\""
            ) +
            "\"";
    }

    /*
     * ====================================================================
     * Effective settings
     * ====================================================================
     */

    private void AddEffectiveSettings(
        TestResult result)
    {
        result.Metadata["EffectiveQuality"] =
            QualitySettings
                .GetQualityLevel()
                .ToString();

        result.Metadata["AA"] =
            QualitySettings
                .antiAliasing
                .ToString();

        result.Metadata["Cascades"] =
            QualitySettings
                .shadowCascades
                .ToString();

        result.Metadata["ShadowResolution"] =
            QualitySettings
                .shadowResolution
                .ToString();

        result.Metadata["ShadowDistance"] =
            QualitySettings
                .shadowDistance
                .ToString(
                    "F1",
                    CultureInfo.InvariantCulture
                );

        result.Metadata["Animators"] =
            CharacterAnimators
                .Count
                .ToString();

        result.Metadata["CharacterRenderers"] =
            CharacterRenderers
                .Count
                .ToString();
    }

    /*
     * ====================================================================
     * Recorder setup
     * ====================================================================
     */

    private void CreateRecorders()
    {
        _mainThread =
            StartRecorder(
                ProfilerCategory.Internal,
                "Main Thread"
            );

        _drawCalls =
            StartRecorder(
                ProfilerCategory.Render,
                "Draw Calls Count"
            );

        _batches =
            StartRecorder(
                ProfilerCategory.Render,
                "Batches Count"
            );

        _setPassCalls =
            StartRecorder(
                ProfilerCategory.Render,
                "SetPass Calls Count"
            );

        _triangles =
            StartRecorder(
                ProfilerCategory.Render,
                "Triangles Count"
            );

        _vertices =
            StartRecorder(
                ProfilerCategory.Render,
                "Vertices Count"
            );
    }

    private static ProfilerRecorder StartRecorder(
        ProfilerCategory category,
        string name)
    {
        try
        {
            return ProfilerRecorder.StartNew(
                category,
                name
            );
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"[TCGProfiler] Unable to start " +
                $"{name}: {exception.Message}"
            );

            return default;
        }
    }

    private static long ReadCounter(
        ProfilerRecorder recorder)
    {
        if (!recorder.Valid ||
            !recorder.IsRunning ||
            recorder.Count == 0)
            return 0;

        return recorder.LastValue;
    }

    private static double ReadTime(
        ProfilerRecorder recorder)
    {
        var value =
            ReadCounter(
                recorder
            );

        return value <= 0
            ? 0
            : value /
              1_000_000.0;
    }

    private static void DisposeRecorder(
        ref ProfilerRecorder recorder)
    {
        if (recorder.Valid) recorder.Dispose();

        recorder = default;
    }

    /*
     * ====================================================================
     * Helpers
     * ====================================================================
     */

    private static IEnumerator WaitUnscaled(
        float seconds)
    {
        var elapsed =
            0.0f;

        while (elapsed < seconds)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            yield return null;
        }
    }

    private static bool IsSceneComponent(
        Component component)
    {
        return
            component != null &&
            component.gameObject != null &&
            component.gameObject.scene.IsValid();
    }
}