using System.Collections.Generic;
using TCGProfiler.Testing;
using UnityEngine.Rendering;

namespace TCGProfiler.TestCases;

public sealed class CharacterNoShadowsTestCase : TestCase
{
    private int _renderersChanged;

    public CharacterNoShadowsTestCase(
        bool enabled = true)
        : base(
            "CharacterNoShadows",
            enabled)
    {
    }

    public override void Setup(
        TcgProfiler profiler)
    {
        profiler.DiscoverCharacters();

        _renderersChanged = 0;

        foreach (var renderer in
                 profiler.CharacterRenderers)
        {
            if (renderer == null ||
                !renderer.enabled ||
                !renderer.gameObject.activeInHierarchy)
            {
                continue;
            }

            renderer.shadowCastingMode =
                ShadowCastingMode.Off;

            renderer.receiveShadows =
                false;

            _renderersChanged++;
        }
    }

    public override Dictionary<string, string>
        GetMetadata(TcgProfiler profiler)
    {
        return new Dictionary<string, string>
        {
            ["RenderersChanged"] =
                _renderersChanged.ToString()
        };
    }
}