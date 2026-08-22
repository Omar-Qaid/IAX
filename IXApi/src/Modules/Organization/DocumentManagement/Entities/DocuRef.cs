using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Organization.DocumentManagement.Entities;

public sealed class DocuRef : Entity<long>
{
    public int RefTableId { get; set; }
    public long RefRecId { get; set; }
    public string RefCompanyId { get; set; } = null!;
    public string ActualCompanyId { get; set; } = null!;
    public long Author { get; set; }
    public long Party { get; set; }
    public string TypeId { get; set; } = null!;
    public DocuType DocuType { get; set; } = null!;
    public long ValueRecId { get; set; }
    public DocuValue DocuValue { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int Restriction { get; set; }
    public string SmmEmailEntryId { get; set; } = string.Empty;
    public string SmmEmailStoreId { get; set; } = string.Empty;
    public int SmmTable { get; set; }
    public string EncyclopediaItemId { get; set; } = string.Empty;
    public string ContactPersonId { get; set; } = string.Empty;
    public long Partition { get; set; } = 5637144576;
    public int IsJustification { get; set; }
    public Guid DocumentId { get; set; } = Guid.NewGuid();
    public int DefaultAttachment { get; set; }
    public string EngChgEngineeringReference { get; set; } = string.Empty;
    public long EngChgEngineeringDocument { get; set; }
    public int IsEnabledForVirtualEntitySync { get; set; }
}
