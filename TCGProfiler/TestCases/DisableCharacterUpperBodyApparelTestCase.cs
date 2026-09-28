using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class DisableCharacterUpperBodyApparelTestCase
    : CharacterRendererDisableTestCase
{
    public DisableCharacterUpperBodyApparelTestCase(
        bool enabled = true)
        : base(
            "CharacterUpperBodyApparel_Disabled",
            enabled)
    {
    }

    protected override bool ShouldDisable(
        Renderer renderer)
    {
        if (!IsActiveRenderer(renderer)) return false;

        return NameContainsAny(
            renderer,
            "Shirt",
            "TShirt",
            "T-Shirt",
            "Hoodie",
            "Sweater",
            "Jacket",
            "Coat",
            "Vest"
        );
    }
}