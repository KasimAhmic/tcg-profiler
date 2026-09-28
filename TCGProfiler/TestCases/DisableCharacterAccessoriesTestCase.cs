using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class DisableCharacterAccessoriesTestCase
    : CharacterRendererDisableTestCase
{
    public DisableCharacterAccessoriesTestCase(
        bool enabled = true)
        : base(
            "CharacterAccessories_Disabled",
            enabled)
    {
    }

    protected override bool ShouldDisable(
        Renderer renderer)
    {
        if (!IsActiveRenderer(renderer)) return false;

        return NameContainsAny(
            renderer,
            "Glasses",
            "Eyeglass",
            "Sunglass",
            "Hat",
            "Cap",
            "Earring",
            "Necklace",
            "Bracelet",
            "Watch",
            "Accessory"
        );
    }
}