using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class DisableCharacterHairBeardTestCase
    : CharacterRendererDisableTestCase
{
    public DisableCharacterHairBeardTestCase(
        bool enabled = true)
        : base(
            "CharacterHairBeard_Disabled",
            enabled)
    {
    }

    protected override bool ShouldDisable(
        Renderer renderer)
    {
        if (!IsActiveRenderer(renderer)) return false;

        return NameContainsAny(
            renderer,
            "Hair",
            "Beard",
            "Mustache",
            "Moustache"
        );
    }
}