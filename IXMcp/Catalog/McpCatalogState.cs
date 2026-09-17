namespace IAX.IXMcp.Catalog;

public sealed class McpCatalogState : IMcpCatalogState
{
    private RuntimeState runtime = new(CompiledCatalog.Empty, default, null);

    public bool IsReady => PublishedToolCount > 0 && RefreshError is null;

    public int PublishedToolCount => Snapshot.Tools.Count(tool => tool.ExecutionEnabled);

    public int CandidateToolCount => Snapshot.Tools.Count;

    public string Status => CandidateToolCount == 0
        ? "No validated catalog is loaded."
        : RefreshError is not null
            ? "The last catalog refresh failed; the previous snapshot is retained."
            : $"{CandidateToolCount} validated candidates are loaded; {PublishedToolCount} are admitted.";

    public CompiledCatalog Snapshot => Volatile.Read(ref runtime).Catalog;

    public DateTimeOffset LoadedAt => Volatile.Read(ref runtime).LoadedAt;

    public string? RefreshError => Volatile.Read(ref runtime).RefreshError;

    public void Replace(CompiledCatalog nextCatalog)
    {
        ArgumentNullException.ThrowIfNull(nextCatalog);
        Interlocked.Exchange(ref runtime, new RuntimeState(nextCatalog, DateTimeOffset.UtcNow, null));
    }

    public void RecordRefreshFailure(string error)
    {
        while (true)
        {
            var current = Volatile.Read(ref runtime);
            var next = current with { RefreshError = error };
            if (ReferenceEquals(Interlocked.CompareExchange(ref runtime, next, current), current))
            {
                return;
            }
        }
    }

    private sealed record RuntimeState(
        CompiledCatalog Catalog,
        DateTimeOffset LoadedAt,
        string? RefreshError);
}
