using TCGProfiler.Testing;

namespace TCGProfiler.TestCases;

public sealed class ManagedCallbackProfilingTestCase
    : TestCase
{
    public ManagedCallbackProfilingTestCase(
        bool enabled = true)
        : base(
            "ManagedCallbacks",
            enabled,
            10.0f,
            20.0f)
    {
    }

    public override void Setup(
        TcgProfiler profiler)
    {
        profiler.ManagedMethodProfiler.Enable();
    }

    public override void BeginMeasurement(
        TcgProfiler profiler)
    {
        profiler.ManagedMethodProfiler.Reset();
    }

    public override void Complete(
        TcgProfiler profiler,
        TestResult result)
    {
        profiler.ManagedMethodProfiler
            .WriteResults(
                profiler.MethodResultsPath
            );
    }

    public override void Teardown(
        TcgProfiler profiler)
    {
        profiler.ManagedMethodProfiler.Disable();
    }
}