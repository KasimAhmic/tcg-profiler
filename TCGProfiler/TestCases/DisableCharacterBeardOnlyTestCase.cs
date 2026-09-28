using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class DisableCharacterBeardOnlyTestCase
    : CharacterRendererDisableTestCase
{
    public DisableCharacterBeardOnlyTestCase(
        bool enabled = true)
        : base(
            "CharacterBeardOnly_Disabled",
            enabled)
    {
    }

    protected override bool ShouldDisable(
        Renderer renderer)
    {
        if (!IsActiveRenderer(renderer)) return false;

        return NameContainsAny(
            renderer,
            "Beard",
            "Mustache",
            "Moustache"
        );
    }
}