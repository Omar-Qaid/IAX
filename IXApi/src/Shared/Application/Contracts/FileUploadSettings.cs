namespace IAX.IXApi.Shared.Application.Contracts
{
    public class FileUploadSettings
    {
        public required string FileVirtualDirectory { get; set; }

        public required string FileUploadDirectory { get; set; }

        public required int MaxImageWidth { get; set; }

        public required int MaxImageHeight { get; set; }
    }
}
