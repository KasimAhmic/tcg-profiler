using System.Collections.Generic;
using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class CharacterCheapMaterialTestCase : TestCase
{
    private Material? _cheapMaterial;
    private int _materialSlotsReplaced;

    private int _renderersChanged;

    public CharacterCheapMaterialTestCase(
        bool enabled = true)
        : base(
            "CharacterCheapMaterial",
            enabled)
    {
    }

    public override void Setup(
        TcgProfiler profiler)
    {
        profiler.DiscoverCharacters();

        _renderersChanged = 0;
        _materialSlotsReplaced = 0;

        /*
         * Use Unity's built-in unlit color shader if available.
         *
         * This is intentionally simple and is only for benchmarking.
         */
        var shader =
            Shader.Find(
                "Unlit/Color"
            );

        if (shader == null)
        {
            Debug.LogWarning(
                "[TCGProfiler] Could not find shader Unlit/Color."
            );

            return;
        }

        _cheapMaterial =
            new Material(shader);

        _cheapMaterial.color =
            Color.white;

        foreach (var renderer in
                 profiler.CharacterRenderers)
        {
            if (renderer == null ||
                !renderer.enabled ||
                !renderer.gameObject.activeInHierarchy)
                continue;

            var existing =
                renderer.sharedMaterials;

            if (existing.Length == 0) continue;

            var replacements =
                new Material[existing.Length];

            for (var i = 0;
                 i < replacements.Length;
                 i++)
                replacements[i] =
                    _cheapMaterial;

            renderer.sharedMaterials =
                replacements;

            _renderersChanged++;

            _materialSlotsReplaced +=
                replacements.Length;
        }

        Debug.Log(
            $"[TCGProfiler] CharacterCheapMaterial: " +
            $"changed {_renderersChanged} renderers / " +
            $"{_materialSlotsReplaced} material slots"
        );
    }

    public override Dictionary<string, string>
        GetMetadata(TcgProfiler profiler)
    {
        return new Dictionary<string, string>
        {
            ["RenderersChanged"] =
                _renderersChanged.ToString(),

            ["MaterialSlotsReplaced"] =
                _materialSlotsReplaced.ToString()
        };
    }

    public override void Teardown(
        TcgProfiler profiler)
    {
        if (_cheapMaterial != null)
        {
            Object.Destroy(
                _cheapMaterial
            );

            _cheapMaterial =
                null;
        }
    }
}