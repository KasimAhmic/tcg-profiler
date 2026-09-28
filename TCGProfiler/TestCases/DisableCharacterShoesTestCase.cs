using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class DisableCharacterShoesTestCase
    : CharacterRendererDisableTestCase
{
    public DisableCharacterShoesTestCase(
        bool enabled = true)
        : base(
            "CharacterShoes_Disabled",
            enabled)
    {
    }

    protected override bool ShouldDisable(
        Renderer renderer)
    {
        if (!IsActiveRenderer(renderer)) return false;

        return NameContainsAny(
            renderer,
            "Sneaker",
            "Shoe",
            "Boot"
        );
    }
}