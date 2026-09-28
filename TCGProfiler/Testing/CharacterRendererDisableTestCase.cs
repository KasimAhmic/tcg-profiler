using System;
using System.Collections.Generic;
using UnityEngine;

namespace TCGProfiler.Testing;

public abstract class CharacterRendererDisableTestCase : TestCase
{
    private int _activeRenderers;
    private int _matchedRenderers;
    private int _materialSlots;
    private int _skinnedRenderers;

    private long _triangles;
    private int _visibleRenderers;

    protected CharacterRendererDisableTestCase(
        string name,
        bool enabled = true,
        float soakDuration = 7.0f,
        float testDuration = 15.0f)
        : base(
            name,
            enabled,
            soakDuration,
            testDuration)
    {
    }

    protected abstract bool ShouldDisable(
        Renderer renderer);

    public override void Setup(
        TcgProfiler profiler)
    {
        profiler.DiscoverCharacters();

        _matchedRenderers = 0;
        _activeRenderers = 0;
        _visibleRenderers = 0;
        _skinnedRenderers = 0;
        _triangles = 0;
        _materialSlots = 0;

        foreach (var renderer in
                 profiler.CharacterRenderers)
        {
            if (renderer == null) continue;

            if (!ShouldDisable(renderer)) continue;

            _matchedRenderers++;

            var active =
                renderer.enabled &&
                renderer.gameObject.activeInHierarchy;

            if (active) _activeRenderers++;

            if (active && renderer.isVisible) _visibleRenderers++;

            if (renderer is SkinnedMeshRenderer) _skinnedRenderers++;

            _materialSlots +=
                renderer.sharedMaterials.Length;

            var mesh =
                GetMesh(renderer);

            if (mesh != null)
                for (var i = 0;
                     i < mesh.subMeshCount;
                     i++)
                {
                    if (mesh.GetTopology(i) !=
                        MeshTopology.Triangles)
                        continue;

                    _triangles +=
                        mesh.GetIndexCount(i) /
                        3L;
                }

            /*
             * There is no reason to mutate renderers that
             * are already disabled.
             */
            if (renderer.enabled) renderer.enabled = false;
        }

        Debug.Log(
            $"[TCGProfiler] {Name}: disabling " +
            $"{_matchedRenderers} renderers " +
            $"({_activeRenderers} active, " +
            $"{_visibleRenderers} visible)"
        );
    }

    public override Dictionary<string, string>
        GetMetadata(TcgProfiler profiler)
    {
        return new Dictionary<string, string>
        {
            ["MatchedRenderers"] =
                _matchedRenderers.ToString(),

            ["ActiveMatchedRenderers"] =
                _activeRenderers.ToString(),

            ["VisibleMatchedRenderers"] =
                _visibleRenderers.ToString(),

            ["SkinnedMatchedRenderers"] =
                _skinnedRenderers.ToString(),

            ["MatchedMaterialSlots"] =
                _materialSlots.ToString(),

            ["MatchedTriangles"] =
                _triangles.ToString()
        };
    }

    protected static Mesh? GetMesh(
        Renderer renderer)
    {
        if (renderer is SkinnedMeshRenderer skinned) return skinned.sharedMesh;

        var filter =
            renderer.GetComponent<MeshFilter>();

        return filter?.sharedMesh;
    }

    protected static string GetMeshName(
        Renderer renderer)
    {
        return GetMesh(renderer)?.name ??
               string.Empty;
    }

    protected static bool NameContainsAny(
        Renderer renderer,
        params string[] terms)
    {
        var objectName =
            renderer.gameObject.name;

        var meshName =
            GetMeshName(renderer);

        foreach (var term in terms)
        {
            if (objectName.IndexOf(
                    term,
                    StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (meshName.IndexOf(
                    term,
                    StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }

    protected static bool IsActiveRenderer(
        Renderer renderer)
    {
        return
            renderer.enabled &&
            renderer.gameObject.activeInHierarchy;
    }
}