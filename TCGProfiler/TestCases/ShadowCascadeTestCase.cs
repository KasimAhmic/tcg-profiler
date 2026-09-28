using System.Collections.Generic;
using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class ShadowCascadeTestCase : TestCase
{
    private readonly int _cascades;

    public ShadowCascadeTestCase(
        int cascades,
        bool enabled = true)
        : base(
            $"ShadowCascades_{cascades}",
            enabled)
    {
        _cascades =
            cascades;
    }

    public override void Setup(
        TcgProfiler profiler)
    {
        QualitySettings.shadowCascades =
            _cascades;
    }

    public override Dictionary<string, string>
        GetMetadata(TcgProfiler profiler)
    {
        return new Dictionary<string, string>
        {
            ["ShadowCascades"] =
                _cascades.ToString()
        };
    }
}