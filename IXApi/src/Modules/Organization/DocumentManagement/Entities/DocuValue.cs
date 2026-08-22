using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Organization.DocumentManagement.Entities;

public sealed class DocuValue : Entity<long>
{
    public byte[]? File { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string? Path { get; set; }
    public int Type { get; set; }
    public Guid FileId { get; set; } = Guid.NewGuid();
    public string? McrDocuSubject { get; set; }
    public string AccessInformation { get; set; } = string.Empty;
    public int StorageProviderId { get; set; }
    public string DocumentHashNumber { get; set; } = string.Empty;
    public long Partition { get; set; } = 5637144576;
}
