using TCGProfiler.Testing;
using UnityEngine.Rendering;

namespace TCGProfiler.TestCases;

public sealed class DisableCharacterShadowCastingTestCase
    : TestCase
{
    public DisableCharacterShadowCastingTestCase(
        bool enabled = true)
        : base(
            "CharacterShadowCasting_Disabled",
            enabled)
    {
    }

    public override void Setup(
        TcgProfiler profiler)
    {
        profiler.DiscoverCharacters();

        foreach (var renderer in
                 profiler.CharacterRenderers)
        {
            if (renderer == null) continue;

            renderer.shadowCastingMode =
                ShadowCastingMode.Off;
        }
    }
}