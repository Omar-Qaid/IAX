namespace IAX.IXMcp.Catalog;

public interface IMcpCatalogState
{
    bool IsReady { get; }

    int PublishedToolCount { get; }

    int CandidateToolCount { get; }

    string Status { get; }
}
