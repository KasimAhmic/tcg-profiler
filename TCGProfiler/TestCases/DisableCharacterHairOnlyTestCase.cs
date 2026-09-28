using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class DisableCharacterHairOnlyTestCase
    : CharacterRendererDisableTestCase
{
    public DisableCharacterHairOnlyTestCase(
        bool enabled = true)
        : base(
            "CharacterHairOnly_Disabled",
            enabled)
    {
    }

    protected override bool ShouldDisable(
        Renderer renderer)
    {
        if (!IsActiveRenderer(renderer)) return false;

        return NameContainsAny(
            renderer,
            "Hair"
        );
    }
}