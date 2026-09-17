using System.Diagnostics.Metrics;

namespace IAX.IXMcp.Observability;

public sealed class McpTelemetry : IDisposable
{
    private readonly Meter meter = new("IAX.IXMcp", "1.0.0");

    public McpTelemetry()
    {
        Calls = meter.CreateCounter<long>("ixmcp.tool.calls");
        Duration = meter.CreateHistogram<double>("ixmcp.tool.duration", "ms");
    }

    public Counter<long> Calls { get; }

    public Histogram<double> Duration { get; }

    public void Dispose() => meter.Dispose();
}
