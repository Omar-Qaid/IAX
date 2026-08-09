using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Organization.Attachments
{
    public class OrgAttachmentDetail: Entity<long>
    {
        public long FileId { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string FileName { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.StringLength(50)]
        public string FileType { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.StringLength(1000)]
        public string FilePath { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string? Description { get; set; }
        public long AttachmentId { get; set; }
        public long FileSize { get; set; }

        // Navigation property for attachment
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(AttachmentId))]
        public virtual OrgAttachment Attachment { get; set; } = null!;
    }
}

