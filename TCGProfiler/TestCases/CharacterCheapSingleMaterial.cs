using System.Collections.Generic;
using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class CharacterCheapSingleMaterialTestCase : TestCase
{
    private Material? _cheapMaterial;
    private int _originalMaterialSlots;

    private int _renderersChanged;

    public CharacterCheapSingleMaterialTestCase(
        bool enabled = true)
        : base(
            "CharacterCheapSingleMaterial",
            enabled)
    {
    }

    public override void Setup(
        TcgProfiler profiler)
    {
        profiler.DiscoverCharacters();

        _renderersChanged = 0;
        _originalMaterialSlots = 0;

        var shader =
            Shader.Find("Unlit/Color");

        if (shader == null)
        {
            Debug.LogWarning(
                "[TCGProfiler] Could not find shader Unlit/Color."
            );

            return;
        }

        _cheapMaterial =
            new Material(shader)
            {
                color = Color.white
            };

        foreach (var renderer in profiler.CharacterRenderers)
        {
            if (renderer == null ||
                !renderer.enabled ||
                !renderer.gameObject.activeInHierarchy)
                continue;

            var existing =
                renderer.sharedMaterials;

            if (existing.Length == 0) continue;

            _originalMaterialSlots +=
                existing.Length;

            renderer.sharedMaterials =
                new[]
                {
                    _cheapMaterial
                };

            _renderersChanged++;
        }

        Debug.Log(
            $"[TCGProfiler] CharacterCheapSingleMaterial: " +
            $"changed {_renderersChanged} renderers; " +
            $"collapsed {_originalMaterialSlots} original " +
            $"material slots to {_renderersChanged} slots"
        );
    }

    public override Dictionary<string, string>
        GetMetadata(TcgProfiler profiler)
    {
        return new Dictionary<string, string>
        {
            ["RenderersChanged"] =
                _renderersChanged.ToString(),

            ["OriginalMaterialSlots"] =
                _originalMaterialSlots.ToString(),

            ["ResultingMaterialSlots"] =
                _renderersChanged.ToString(),

            ["MaterialSlotsRemoved"] =
                (_originalMaterialSlots -
                 _renderersChanged)
                .ToString()
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

            _cheapMaterial = null;
        }
    }
}