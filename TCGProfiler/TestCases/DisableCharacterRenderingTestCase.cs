using TCGProfiler.Testing;

namespace TCGProfiler.TestCases;

public sealed class DisableCharacterRenderingTestCase
    : TestCase
{
    public DisableCharacterRenderingTestCase(
        bool enabled = true)
        : base(
            "CharacterRendering_Disabled",
            enabled)
    {
    }

    public override void Setup(
        TcgProfiler profiler)
    {
        profiler.DiscoverCharacters();

        foreach (var renderer in
                 profiler.CharacterRenderers)
            if (renderer != null)
                renderer.enabled =
                    false;
    }
}