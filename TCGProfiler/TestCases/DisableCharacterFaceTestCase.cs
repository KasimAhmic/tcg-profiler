using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class DisableCharacterFaceTestCase
    : CharacterRendererDisableTestCase
{
    public DisableCharacterFaceTestCase(
        bool enabled = true)
        : base(
            "CharacterFace_Disabled",
            enabled)
    {
    }

    protected override bool ShouldDisable(
        Renderer renderer)
    {
        if (!IsActiveRenderer(renderer)) return false;

        return NameContainsAny(
            renderer,
            "Eye",
            "Eyelash",
            "Eyebrow",
            "Teeth",
            "Tongue"
        );
    }
}