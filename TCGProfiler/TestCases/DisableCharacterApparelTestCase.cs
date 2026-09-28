using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class DisableCharacterApparelTestCase
    : CharacterRendererDisableTestCase
{
    public DisableCharacterApparelTestCase(
        bool enabled = true)
        : base(
            "CharacterApparel_Disabled",
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
            "Pants",
            "Jeans",
            "Shorts",
            "Skirt",
            "Dress",
            "Sneaker",
            "Shoe",
            "Boot",
            "Sock"
        );
    }
}