namespace IAX.IXMcp.Catalog;

public sealed class McpCatalogState : IMcpCatalogState
{
    private CompiledCatalog catalog = CompiledCatalog.Empty;

    public bool IsReady => PublishedToolCount > 0;

    public int PublishedToolCount => Volatile.Read(ref catalog).Tools.Count(tool => tool.ExecutionEnabled);

    public int CandidateToolCount => Volatile.Read(ref catalog).Tools.Count;

    public string Status => CandidateToolCount == 0
        ? "No validated catalog is loaded."
        : $"{CandidateToolCount} validated candidate tools are loaded; none are admitted for execution.";

    public CompiledCatalog Snapshot => Volatile.Read(ref catalog);

    public void Replace(CompiledCatalog nextCatalog)
    {
        ArgumentNullException.ThrowIfNull(nextCatalog);
        Interlocked.Exchange(ref catalog, nextCatalog);
    }
}
