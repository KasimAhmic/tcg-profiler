using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class DisableVisibleCharacterRenderersTestCase
    : CharacterRendererDisableTestCase
{
    public DisableVisibleCharacterRenderersTestCase(
        bool enabled = true)
        : base(
            "CharacterVisibleRenderers_Disabled",
            enabled)
    {
    }

    protected override bool ShouldDisable(
        Renderer renderer)
    {
        return
            IsActiveRenderer(renderer) &&
            renderer.isVisible;
    }
}