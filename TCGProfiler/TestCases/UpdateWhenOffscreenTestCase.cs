using TCGProfiler.Testing;

namespace TCGProfiler.TestCases;

public sealed class UpdateWhenOffscreenTestCase
    : TestCase
{
    public UpdateWhenOffscreenTestCase(
        bool enabled = true)
        : base(
            "SkinnedMesh_UpdateWhenOffscreen_False",
            enabled)
    {
    }

    public override void Setup(
        TcgProfiler profiler)
    {
        profiler.DiscoverCharacters();

        foreach (var renderer in
                 profiler.CharacterSkinnedRenderers)
        {
            if (renderer == null) continue;

            renderer.updateWhenOffscreen =
                false;
        }
    }
}