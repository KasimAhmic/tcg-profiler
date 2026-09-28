using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TCGProfiler.Testing;

public abstract class CharacterShaderReplacementTestCase : TestCase
{
    private Material? _cheapMaterial;
    private int _matchedMaterialSlots;

    private int _matchedRenderers;
    private int _visibleMatchedRenderers;

    protected CharacterShaderReplacementTestCase(
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

    protected abstract bool ShouldReplaceShader(
        string shaderName);

    public override void Setup(
        TcgProfiler profiler)
    {
        profiler.DiscoverCharacters();

        _matchedRenderers = 0;
        _visibleMatchedRenderers = 0;
        _matchedMaterialSlots = 0;

        var shader =
            Shader.Find("Unlit/Color");

        if (shader == null)
        {
            Debug.LogWarning(
                $"[TCGProfiler] {Name}: " +
                "could not find Unlit/Color shader."
            );

            return;
        }

        _cheapMaterial =
            new Material(shader)
            {
                color = Color.white
            };

        foreach (var renderer in
                 profiler.CharacterRenderers)
        {
            if (renderer == null ||
                !renderer.enabled ||
                !renderer.gameObject.activeInHierarchy)
                continue;

            var materials =
                renderer.sharedMaterials;

            if (materials.Length == 0) continue;

            Material[]? replacements = null;
            var replacedSlots = 0;

            for (var i = 0;
                 i < materials.Length;
                 i++)
            {
                var material =
                    materials[i];

                if (material == null ||
                    material.shader == null)
                    continue;

                if (!ShouldReplaceShader(
                        material.shader.name))
                    continue;

                /*
                 * Don't allocate a replacement array unless
                 * we actually find a matching slot.
                 */
                replacements ??=
                    (Material[])materials.Clone();

                replacements[i] =
                    _cheapMaterial;

                replacedSlots++;
            }

            if (replacements == null) continue;

            renderer.sharedMaterials =
                replacements;

            _matchedRenderers++;

            _matchedMaterialSlots +=
                replacedSlots;

            if (renderer.isVisible) _visibleMatchedRenderers++;
        }

        Debug.Log(
            $"[TCGProfiler] {Name}: " +
            $"{_matchedRenderers} renderers, " +
            $"{_visibleMatchedRenderers} visible, " +
            $"{_matchedMaterialSlots} material slots replaced"
        );
    }

    public override Dictionary<string, string>
        GetMetadata(TcgProfiler profiler)
    {
        return new Dictionary<string, string>
        {
            ["MatchedRenderers"] =
                _matchedRenderers.ToString(),

            ["VisibleMatchedRenderers"] =
                _visibleMatchedRenderers.ToString(),

            ["MatchedMaterialSlots"] =
                _matchedMaterialSlots.ToString()
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

    protected static bool EqualsShader(
        string actual,
        string expected)
    {
        return string.Equals(
            actual,
            expected,
            StringComparison.Ordinal
        );
    }

    protected static bool ShaderStartsWith(
        string actual,
        string prefix)
    {
        return actual.StartsWith(
            prefix,
            StringComparison.Ordinal
        );
    }
}