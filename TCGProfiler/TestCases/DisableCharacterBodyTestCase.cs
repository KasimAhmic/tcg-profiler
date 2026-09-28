using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class DisableCharacterBodyTestCase
    : CharacterRendererDisableTestCase
{
    public DisableCharacterBodyTestCase(
        bool enabled = true)
        : base(
            "CharacterBody_Disabled",
            enabled)
    {
    }

    protected override bool ShouldDisable(
        Renderer renderer)
    {
        if (!IsActiveRenderer(renderer)) return false;

        return NameContainsAny(
            renderer,
            "Male_Combined",
            "Female_Combined",
            "Base_Body",
            "BaseBody"
        );
    }
}