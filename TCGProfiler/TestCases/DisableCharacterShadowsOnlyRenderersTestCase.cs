using TCGProfiler.Testing;
using UnityEngine;
using UnityEngine.Rendering;

namespace TCGProfiler.TestCases;

public sealed class DisableCharacterShadowsOnlyRenderersTestCase
    : CharacterRendererDisableTestCase
{
    public DisableCharacterShadowsOnlyRenderersTestCase(
        bool enabled = true)
        : base(
            "CharacterShadowsOnlyRenderers_Disabled",
            enabled)
    {
    }

    protected override bool ShouldDisable(
        Renderer renderer)
    {
        return
            IsActiveRenderer(renderer) &&
            renderer.shadowCastingMode ==
            ShadowCastingMode.ShadowsOnly;
    }
}