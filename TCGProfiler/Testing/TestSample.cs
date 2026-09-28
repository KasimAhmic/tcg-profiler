namespace TCGProfiler.Testing;

public sealed class TestSample
{
    public double FrameTimeMs { get; set; }

    public double MainThreadMs { get; set; }

    public long DrawCalls { get; set; }

    public long Batches { get; set; }

    public long SetPassCalls { get; set; }

    public long Triangles { get; set; }

    public long Vertices { get; set; }
}