using System.Collections.Generic;
using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class ShadowResolutionTestCase : TestCase
{
    private readonly ShadowResolution _resolution;

    public ShadowResolutionTestCase(
        ShadowResolution resolution,
        bool enabled = true)
        : base(
            $"ShadowResolution_{resolution}",
            enabled)
    {
        _resolution =
            resolution;
    }

    public override void Setup(
        TcgProfiler profiler)
    {
        QualitySettings.shadowResolution =
            _resolution;
    }

    public override Dictionary<string, string>
        GetMetadata(TcgProfiler profiler)
    {
        return new Dictionary<string, string>
        {
            ["ShadowResolution"] =
                _resolution.ToString()
        };
    }
}