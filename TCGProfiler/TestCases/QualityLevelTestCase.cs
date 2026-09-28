using System.Collections.Generic;
using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class QualityLevelTestCase : TestCase
{
    private readonly int _qualityLevel;

    public QualityLevelTestCase(
        int qualityLevel,
        bool enabled = true)
        : base(
            $"Quality_{qualityLevel}",
            enabled)
    {
        _qualityLevel =
            qualityLevel;
    }

    public override void Setup(
        TcgProfiler profiler)
    {
        QualitySettings.SetQualityLevel(
            _qualityLevel,
            true
        );
    }

    public override Dictionary<string, string>
        GetMetadata(TcgProfiler profiler)
    {
        var names =
            QualitySettings.names;

        var name =
            _qualityLevel >= 0 &&
            _qualityLevel < names.Length
                ? names[_qualityLevel]
                : "Unknown";

        return new Dictionary<string, string>
        {
            ["QualityLevel"] =
                _qualityLevel.ToString(),

            ["QualityName"] =
                name
        };
    }
}