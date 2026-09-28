using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class DisableNonVisibleCharacterRenderersTestCase
    : CharacterRendererDisableTestCase
{
    public DisableNonVisibleCharacterRenderersTestCase(
        bool enabled = true)
        : base(
            "CharacterNonVisibleRenderers_Disabled",
            enabled)
    {
    }

    protected override bool ShouldDisable(
        Renderer renderer)
    {
        return
            IsActiveRenderer(renderer) &&
            !renderer.isVisible;
    }
}