using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class AnimatorCullingTestCase : TestCase
{
    private readonly AnimatorCullingMode _mode;

    public AnimatorCullingTestCase(
        AnimatorCullingMode mode,
        bool enabled = true)
        : base(
            $"AnimatorCulling_{mode}",
            enabled)
    {
        _mode = mode;
    }

    public override void Setup(
        TcgProfiler profiler)
    {
        profiler.DiscoverCharacters();

        foreach (var animator in
                 profiler.CharacterAnimators)
        {
            if (animator == null) continue;

            animator.cullingMode =
                _mode;
        }
    }
}