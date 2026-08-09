namespace IAX.IXApi.Modules.Organization.Attachments
{
    public class OrgAttachmentDetailDto
    {
        public long FileId { get; set; }
        public string FileName { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string? Description { get; set; }
        public long AttachmentId { get; set; }
        public long FileSize { get; set; }
    }
}
