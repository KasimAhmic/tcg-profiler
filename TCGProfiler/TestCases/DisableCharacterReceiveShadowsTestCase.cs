using TCGProfiler.Testing;

namespace TCGProfiler.TestCases;

public sealed class DisableCharacterReceiveShadowsTestCase
    : TestCase
{
    public DisableCharacterReceiveShadowsTestCase(
        bool enabled = true)
        : base(
            "CharacterReceiveShadows_Disabled",
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

            renderer.receiveShadows =
                false;
        }
    }
}