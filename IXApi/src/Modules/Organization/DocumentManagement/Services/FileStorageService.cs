using IAX.IXApi.Modules.Organization.DocumentManagement.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace IAX.IXApi.Modules.Organization.DocumentManagement.Services;

public sealed class FileStorageService : IFileStorageProvider
{
    private readonly string _rootPath;

    public FileStorageService(IOptions<DocumentStorageOptions> options, IHostEnvironment environment)
    {
        var configuredPath = options.Value.RootPath;
        _rootPath = Path.GetFullPath(Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(environment.ContentRootPath, configuredPath));
    }

    public string Name => "Local";

    public async Task<StoredDocument> SaveAsync(Stream source, string extension, CancellationToken cancellationToken = default)
    {
        var safeExtension = NormalizeExtension(extension);
        var now = DateTime.UtcNow;
        var storageKey = Path.Combine(now.ToString("yyyy"), now.ToString("MM"), $"{Guid.NewGuid():N}{safeExtension}").Replace('\\', '/');
        var physicalPath = Resolve(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
        await using var destination = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[81920];
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            hash.AppendData(buffer, 0, bytesRead);
        }
        return new StoredDocument(storageKey, destination.Length, Convert.ToHexString(hash.GetHashAndReset()));
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = Resolve(storageKey);
        if (!File.Exists(path)) throw new FileNotFoundException("The stored document was not found.");
        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = Resolve(storageKey);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string Resolve(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || Path.IsPathRooted(storageKey)) throw new InvalidDataException("Invalid document storage key.");
        var path = Path.GetFullPath(Path.Combine(_rootPath, storageKey.Replace('/', Path.DirectorySeparatorChar)));
        var prefix = _rootPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Invalid document storage key.");
        return path;
    }

    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return string.Empty;
        var value = extension.StartsWith('.') ? extension : $".{extension}";
        if (value.Length > 16 || !value.Skip(1).All(char.IsLetterOrDigit)) throw new InvalidDataException("Invalid file extension.");
        return value.ToLowerInvariant();
    }
}
