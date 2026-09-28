using System;
using System.Collections.Generic;
using System.Linq;

namespace TCGProfiler.Testing;

public sealed class TestResult
{
    public TestResult(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public List<TestSample> Samples { get; } = new();

    public Dictionary<string, string> Metadata { get; } = new();

    public double AverageFrameTimeMs =>
        Samples.Count == 0
            ? 0
            : Samples.Average(x => x.FrameTimeMs);

    public double AverageFps =>
        AverageFrameTimeMs <= 0
            ? 0
            : 1000.0 / AverageFrameTimeMs;

    public double MedianFrameTimeMs =>
        Percentile(
            Samples.Select(x => x.FrameTimeMs),
            0.50
        );

    public double P95FrameTimeMs =>
        Percentile(
            Samples.Select(x => x.FrameTimeMs),
            0.95
        );

    public double P99FrameTimeMs =>
        Percentile(
            Samples.Select(x => x.FrameTimeMs),
            0.99
        );

    /*
     * "1% low FPS" here is derived from the average
     * frame time of the slowest 1% of captured frames.
     */
    public double OnePercentLowFps
    {
        get
        {
            if (Samples.Count == 0) return 0;

            var sorted =
                Samples
                    .Select(x => x.FrameTimeMs)
                    .OrderByDescending(x => x)
                    .ToArray();

            var count =
                Math.Max(
                    1,
                    (int)Math.Ceiling(
                        sorted.Length * 0.01
                    )
                );

            var average =
                sorted
                    .Take(count)
                    .Average();

            return average <= 0
                ? 0
                : 1000.0 / average;
        }
    }

    public double AverageMainThreadMs =>
        AveragePositive(
            Samples.Select(x => x.MainThreadMs)
        );

    public double AverageDrawCalls =>
        AveragePositive(
            Samples.Select(x => (double)x.DrawCalls)
        );

    public double AverageBatches =>
        AveragePositive(
            Samples.Select(x => (double)x.Batches)
        );

    public double AverageSetPassCalls =>
        AveragePositive(
            Samples.Select(x => (double)x.SetPassCalls)
        );

    public double AverageTriangles =>
        AveragePositive(
            Samples.Select(x => (double)x.Triangles)
        );

    public double AverageVertices =>
        AveragePositive(
            Samples.Select(x => (double)x.Vertices)
        );

    private static double AveragePositive(
        IEnumerable<double> values)
    {
        var array =
            values
                .Where(x => x > 0)
                .ToArray();

        return array.Length == 0
            ? 0
            : array.Average();
    }

    private static double Percentile(
        IEnumerable<double> values,
        double percentile)
    {
        var sorted =
            values
                .OrderBy(x => x)
                .ToArray();

        if (sorted.Length == 0) return 0;

        var index =
            (sorted.Length - 1) *
            percentile;

        var lower =
            (int)Math.Floor(index);

        var upper =
            (int)Math.Ceiling(index);

        if (lower == upper) return sorted[lower];

        var fraction =
            index - lower;

        return
            sorted[lower] +
            (
                sorted[upper] -
                sorted[lower]
            ) * fraction;
    }
}