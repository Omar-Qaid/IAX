namespace IAX.IXApi.Modules.Organization.DocumentManagement.Storage;

public sealed class DocumentStorageOptions
{
    public string Provider { get; set; } = "Local";
    public string RootPath { get; set; } = "App_Data/documents";
    public long MaxFileSizeBytes { get; set; } = 25 * 1024 * 1024;
    public int MaxAttachmentsPerRecord { get; set; } = 100;
    public string[] AllowedExtensions { get; set; } = [];
    public string[] AllowedMimeTypes { get; set; } = [];
}

public sealed record StoredDocument(string StorageKey, long Length, string Hash);

public interface IFileStorageProvider
{
    string Name { get; }
    Task<StoredDocument> SaveAsync(Stream source, string extension, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
