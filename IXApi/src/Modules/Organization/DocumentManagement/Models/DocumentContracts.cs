namespace IAX.IXApi.Modules.Organization.DocumentManagement.Models;

public sealed record DocuTypeDto(long Id, string TypeId, string Name, int TypeGroup, string Kind, int? FilePlace,
    string? Description, string[] AllowedExtensions, string[] AllowedMimeTypes, long? MaxFileSizeBytes);

public sealed record DocumentDto(long Id, int RefTableId, long RefRecId, string? RefCompanyId,
    string TypeId, string DocumentTypeName, int TypeGroup, string Kind, long? ValueRecId, string Name,
    string? FileName, string? OriginalFileName, string? FileType, string? ContentType, long? FileSize,
    string? Notes, string? Url, int? Restriction, string? CreatedBy, DateTime? CreatedAt,
    string? ModifiedBy, DateTime? ModifiedAt);

public sealed record DocumentPageDto(IReadOnlyList<DocumentDto> Items, int PageNumber, int PageSize, int TotalCount);

public sealed class CreateDocumentCommand
{
    public required int RefTableId { get; init; }
    public required long RefRecId { get; init; }
    public required string TypeId { get; init; }
    public string? Name { get; init; }
    public string? Notes { get; init; }
    public string? Url { get; init; }
    public string? FileName { get; init; }
    public string? MimeType { get; init; }
    public long FileSize { get; init; }
    public Stream? Content { get; init; }
}

public sealed class UpdateDocumentRequest
{
    public string? FileName { get; set; }
    public string? Name { get; set; }
    public string? Notes { get; set; }
    public string? Url { get; set; }
    public int? Restriction { get; set; }
}

public sealed record DocumentContent(Stream Stream, string FileName, string MimeType);
