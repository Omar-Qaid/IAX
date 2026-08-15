namespace IAX.IXApi.Shared.Application.NumberSequences;

public sealed class NumberSequenceMetadataDto
{
    public string SequenceKey { get; init; } = string.Empty;
    public string Mode { get; init; } = "automatic";
    public bool Manual { get; init; }
    public bool Available { get; init; }
    public bool Blocked { get; init; }
    public string? PreviewCode { get; init; }
    public string? Scope { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Shared boundary used by generic controllers. SysNumberSequences remains the
/// source of truth; feature modules do not depend on Administration internals.
/// </summary>
public interface INumberSequenceRuntime
{
    Task<NumberSequenceMetadataDto?> GetMetadataAsync(
        Type entityType,
        string? dataAreaId = null,
        CancellationToken cancellationToken = default);

    Task PrepareCreateAsync(
        object entity,
        CancellationToken cancellationToken = default);
}
