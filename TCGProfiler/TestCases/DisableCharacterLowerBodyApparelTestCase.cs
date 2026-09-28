using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class DisableCharacterLowerBodyApparelTestCase
    : CharacterRendererDisableTestCase
{
    public DisableCharacterLowerBodyApparelTestCase(
        bool enabled = true)
        : base(
            "CharacterLowerBodyApparel_Disabled",
            enabled)
    {
    }

    protected override bool ShouldDisable(
        Renderer renderer)
    {
        if (!IsActiveRenderer(renderer)) return false;

        return NameContainsAny(
            renderer,
            "Pants",
            "Jeans",
            "Shorts",
            "Skirt",
            "Dress"
        );
    }
}