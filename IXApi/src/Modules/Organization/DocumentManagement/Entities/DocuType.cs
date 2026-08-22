using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Organization.DocumentManagement.Entities;

public sealed class DocuType : Entity<long>
{
    public int ActionClassId { get; set; }
    public string? ArchivePath { get; set; }
    public int DocuStructureType { get; set; }
    public int FilePlace { get; set; }
    public int FileRemovalConfirmation { get; set; }
    public string TypeId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int TypeGroup { get; set; }
    public byte[]? Parameters { get; set; }
    public int RemoveOption { get; set; }
    public string? Host { get; set; }
    public string? Site { get; set; }
    public string? FolderPath { get; set; }
    public long Partition { get; set; } = 5637144576;
}
