using TCGProfiler.Testing;
using UnityEngine;
using UnityEngine.Rendering;

namespace TCGProfiler.TestCases;

public sealed class DisableNonVisibleShadowCastingCharacterRenderersTestCase
    : CharacterRendererDisableTestCase
{
    public DisableNonVisibleShadowCastingCharacterRenderersTestCase(
        bool enabled = true)
        : base(
            "CharacterNonVisibleShadowCasters_Disabled",
            enabled)
    {
    }

    protected override bool ShouldDisable(
        Renderer renderer)
    {
        if (!IsActiveRenderer(renderer)) return false;

        if (renderer.isVisible) return false;

        return
            renderer.shadowCastingMode !=
            ShadowCastingMode.Off;
    }
}