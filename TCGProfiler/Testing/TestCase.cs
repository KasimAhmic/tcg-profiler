using System.Collections.Generic;

namespace TCGProfiler.Testing;

public abstract class TestCase
{
    protected TestCase(
        string name,
        bool enabled = true,
        float soakDuration = 7.0f,
        float testDuration = 15.0f)
    {
        Name = name;
        Enabled = enabled;
        SoakDuration = soakDuration;
        TestDuration = testDuration;
    }

    public string Name { get; }

    public bool Enabled { get; set; }

    public float SoakDuration { get; set; }

    public float TestDuration { get; set; }

    public virtual void Setup(TcgProfiler profiler)
    {
    }

    // NEW
    public virtual void BeginMeasurement(TcgProfiler profiler)
    {
    }

    public virtual void Sample(
        TcgProfiler profiler,
        TestSample sample)
    {
    }

    public virtual void Complete(
        TcgProfiler profiler,
        TestResult result)
    {
    }

    public virtual void Teardown(TcgProfiler profiler)
    {
    }

    public virtual Dictionary<string, string> GetMetadata(
        TcgProfiler profiler)
    {
        return new Dictionary<string, string>();
    }
}