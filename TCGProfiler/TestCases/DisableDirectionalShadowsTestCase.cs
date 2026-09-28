using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class DisableDirectionalShadowsTestCase
    : TestCase
{
    public DisableDirectionalShadowsTestCase(
        bool enabled = true)
        : base(
            "DirectionalShadows_Disabled",
            enabled)
    {
    }

    public override void Setup(
        TcgProfiler profiler)
    {
        var lights =
            Resources.FindObjectsOfTypeAll<Light>();

        foreach (var light in lights)
        {
            if (light == null ||
                !light.gameObject.scene.IsValid())
                continue;

            if (light.type ==
                LightType.Directional)
                light.shadows =
                    LightShadows.None;
        }
    }
}