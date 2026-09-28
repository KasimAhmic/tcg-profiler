using System.Collections.Generic;
using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class ShadowDistanceTestCase : TestCase
{
    private readonly float _distance;

    public ShadowDistanceTestCase(
        float distance,
        bool enabled = true)
        : base(
            $"ShadowDistance_{distance:F0}",
            enabled)
    {
        _distance =
            distance;
    }

    public override void Setup(
        TcgProfiler profiler)
    {
        QualitySettings.shadowDistance =
            _distance;
    }

    public override Dictionary<string, string>
        GetMetadata(TcgProfiler profiler)
    {
        return new Dictionary<string, string>
        {
            ["ShadowDistance"] =
                _distance.ToString("F1")
        };
    }
}