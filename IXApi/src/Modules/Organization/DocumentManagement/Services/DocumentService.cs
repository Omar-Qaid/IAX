using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using IAX.IXApi.Modules.Organization.DocumentManagement.Models;
using IAX.IXApi.Modules.Organization.DocumentManagement.Storage;
using IAX.IXApi.Modules.Organization.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IAX.IXApi.Modules.Organization.DocumentManagement.Services;

public sealed class DocumentService : IDocumentService
{
    private readonly IOrganizationDataContext _db;
    private readonly IFileStorageProvider _storage;
    private readonly DocumentStorageOptions _options;
    private readonly ICurrentUserService _currentUser;

    public DocumentService(IOrganizationDataContext db, IFileStorageProvider storage, IOptions<DocumentStorageOptions> options, ICurrentUserService currentUser)
    {
        _db = db; _storage = storage; _options = options.Value; _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<DocuTypeDto>> GetTypesAsync(CancellationToken cancellationToken = default)
    {
        var values = await _db.DocuTypes.IgnoreQueryFilters().AsNoTracking().Where(x => x.IsActive && !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return values.Select(ToTypeDto).ToList();
    }

    public async Task<DocumentPageDto> GetForRecordAsync(int refTableId, long refRecId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        ValidateReference(refTableId, refRecId);
        pageNumber = Math.Max(1, pageNumber); pageSize = Math.Clamp(pageSize, 1, 100);
        var company = CurrentCompany();
        var query = Query().AsNoTracking().Where(x => x.RefTableId == refTableId && x.RefRecId == refRecId && x.RefCompanyId == company)
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.RecId);
        var total = await query.CountAsync(cancellationToken);
        var values = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new DocumentPageDto(values.Select(ToDto).ToList(), pageNumber, pageSize, total);
    }

    public async Task<DocumentDto?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var value = await Query().AsNoTracking().SingleOrDefaultAsync(x => x.RecId == id && x.RefCompanyId == CurrentCompany(), cancellationToken);
        return value == null ? null : ToDto(value);
    }

    public async Task<DocumentDto> CreateAsync(CreateDocumentCommand command, CancellationToken cancellationToken = default)
    {
        ValidateReference(command.RefTableId, command.RefRecId);
        var company = CurrentCompany();
        var type = await _db.DocuTypes.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.IsActive && !x.IsDeleted && x.TypeId == command.TypeId, cancellationToken)
            ?? throw new KeyNotFoundException("The selected document type was not found or is inactive.");
        var count = await _db.DocuRefs.CountAsync(x => x.RefTableId == command.RefTableId && x.RefRecId == command.RefRecId && x.RefCompanyId == company, cancellationToken);
        if (count >= _options.MaxAttachmentsPerRecord) throw new ArgumentException($"A record can contain at most {_options.MaxAttachmentsPerRecord} documents.");

        DocuValue value; StoredDocument? stored = null;
        var kind = Kind(type.TypeGroup);
        if (IsFile(kind))
        {
            if (command.Content == null || command.FileSize <= 0 || string.IsNullOrWhiteSpace(command.FileName)) throw new ArgumentException("A non-empty file is required.");
            var originalName = Path.GetFileName(command.FileName); var extension = Path.GetExtension(originalName).ToLowerInvariant();
            ValidateFile(extension, command.MimeType, command.FileSize);
            stored = await _storage.SaveAsync(command.Content, extension, cancellationToken);
            if (stored.Length > _options.MaxFileSizeBytes)
            {
                await _storage.DeleteAsync(stored.StorageKey, cancellationToken);
                throw new ArgumentException($"The file exceeds the maximum size of {_options.MaxFileSizeBytes} bytes.");
            }
            var duplicate = await _db.DocuRefs.AnyAsync(x => x.RefTableId == command.RefTableId && x.RefRecId == command.RefRecId && x.RefCompanyId == company && x.DocuValue.DocumentHashNumber == stored.Hash, cancellationToken);
            if (duplicate)
            {
                await _storage.DeleteAsync(stored.StorageKey, cancellationToken);
                throw new ArgumentException("The same document is already attached to this record.");
            }
            value = CreateValue(type, company, Clean(command.Name, 120) ?? originalName, originalName, extension, stored.StorageKey, stored.Hash);
        }
        else if (kind == "URL")
        {
            if (!Uri.TryCreate(command.Url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")) throw new ArgumentException("A valid HTTP or HTTPS URL is required.");
            value = CreateValue(type, company, Clean(command.Name, 120) ?? uri.Host, uri.Host, "url", null, HashText(uri.AbsoluteUri));
            value.AccessInformation = uri.AbsoluteUri;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(command.Name) || string.IsNullOrWhiteSpace(command.Notes)) throw new ArgumentException("Name and notes are required.");
            value = CreateValue(type, company, Clean(command.Name, 120)!, string.Empty, "note", null, HashText(command.Notes));
        }

        var docuRef = new DocuRef { RefTableId = command.RefTableId, RefRecId = command.RefRecId, RefCompanyId = company,
            ActualCompanyId = company, DataAreaId = company, TypeId = type.TypeId, DocuType = type, DocuValue = value,
            Name = Clean(command.Name, 120) ?? value.Name, Notes = Clean(command.Notes, 100_000) };
        try { _db.DocuRefs.Add(docuRef); await _db.SaveChangesAsync(cancellationToken); }
        catch { if (stored != null) await _storage.DeleteAsync(stored.StorageKey, cancellationToken); throw; }
        return ToDto(docuRef);
    }

    public async Task<DocumentDto?> UpdateAsync(long id, UpdateDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var docuRef = await Query().SingleOrDefaultAsync(x => x.RecId == id && x.RefCompanyId == CurrentCompany(), cancellationToken);
        if (docuRef == null) return null;
        var value = docuRef.DocuValue;
        var kind = Kind(docuRef.DocuType.TypeGroup);
        docuRef.Name = Clean(request.Name, 120) ?? docuRef.Name; docuRef.Notes = Clean(request.Notes, 100_000); docuRef.Restriction = request.Restriction ?? docuRef.Restriction;
        if (IsFile(kind) && !string.IsNullOrWhiteSpace(request.FileName)) value.FileName = Path.GetFileName(request.FileName);
        if (kind == "Note" && (string.IsNullOrWhiteSpace(docuRef.Name) || string.IsNullOrWhiteSpace(docuRef.Notes))) throw new ArgumentException("Name and notes are required.");
        if (kind == "URL")
        {
            var url = request.Url ?? value.AccessInformation;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")) throw new ArgumentException("A valid HTTP or HTTPS URL is required.");
            value.AccessInformation = uri.AbsoluteUri; value.FileName = docuRef.Name;
            value.DocumentHashNumber = HashText(uri.AbsoluteUri);
        }
        await _db.SaveChangesAsync(cancellationToken); return ToDto(docuRef);
    }

    public async Task<DocumentContent?> OpenContentAsync(long id, CancellationToken cancellationToken = default)
    {
        var docuRef = await Query().AsNoTracking().SingleOrDefaultAsync(x => x.RecId == id && x.RefCompanyId == CurrentCompany(), cancellationToken);
        var value = docuRef?.DocuValue;
        if (docuRef == null || !IsFile(Kind(docuRef.DocuType.TypeGroup)) || string.IsNullOrWhiteSpace(value?.Path)) return null;
        return new DocumentContent(await _storage.OpenReadAsync(value.Path, cancellationToken), value.FileName, MimeType(value.FileType));
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var docuRef = await Query().SingleOrDefaultAsync(x => x.RecId == id && x.RefCompanyId == CurrentCompany(), cancellationToken);
        if (docuRef == null) return false;
        var value = docuRef.DocuValue; var storagePath = value.Path;
        _db.DocuRefs.Remove(docuRef); _db.DocuValues.Remove(value);
        await _db.SaveChangesAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(storagePath)) await _storage.DeleteAsync(storagePath, cancellationToken);
        return true;
    }

    private IQueryable<DocuRef> Query() => _db.DocuRefs.Include(x => x.DocuType).Include(x => x.DocuValue);
    private string CurrentCompany() { var value = (_currentUser.GetDataAreaId() ?? "dat").Trim(); return value[..Math.Min(value.Length, 8)]; }
    private void ValidateFile(string extension, string? mimeType, long size)
    {
        if (size > _options.MaxFileSizeBytes) throw new ArgumentException($"The file exceeds the maximum size of {_options.MaxFileSizeBytes} bytes.");
        var extensions = _options.AllowedExtensions.Select(x => x.StartsWith('.') ? x : $".{x}").ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (extensions.Count > 0 && !extensions.Contains(extension)) throw new ArgumentException($"Files with extension '{extension}' are not allowed.");
        var mimeTypes = _options.AllowedMimeTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (mimeTypes.Count > 0 && (string.IsNullOrWhiteSpace(mimeType) || !mimeTypes.Contains(mimeType))) throw new ArgumentException("The file MIME type is not allowed.");
    }
    private static void ValidateReference(int refTableId, long refRecId) { if (refTableId <= 0 || refRecId <= 0) throw new ArgumentException("A valid RefTableId and RefRecId are required."); }
    private static string Kind(int typeGroup) => typeGroup switch { 1 => "Note", 2 => "URL", 3 => "Image", _ => "File" };
    private static bool IsFile(string kind) => kind is "File" or "Image";
    private static string? Clean(string? value, int max) { var text = value?.Trim(); return string.IsNullOrEmpty(text) ? null : text[..Math.Min(text.Length, max)]; }
    private static DocuValue CreateValue(DocuType type, string company, string name, string originalFileName, string fileType, string? path, string hash) => new()
    {
        FileName = originalFileName, OriginalFileName = originalFileName, Name = name, FileType = fileType,
        Path = path, Type = type.TypeGroup, DataAreaId = company, AccessInformation = string.Empty, StorageProviderId = 0,
        DocumentHashNumber = hash
    };
    private static string HashText(string? value) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value ?? string.Empty)));
    private static string MimeType(string extension) => extension.ToLowerInvariant() switch
    {
        ".pdf" or "pdf" => "application/pdf", ".png" or "png" => "image/png",
        ".jpg" or ".jpeg" or "jpg" or "jpeg" => "image/jpeg", ".gif" or "gif" => "image/gif",
        ".txt" or "txt" => "text/plain", ".csv" or "csv" => "text/csv",
        ".doc" or "doc" => "application/msword",
        ".docx" or "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".xls" or "xls" => "application/vnd.ms-excel",
        ".xlsx" or "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        _ => "application/octet-stream"
    };
    private DocuTypeDto ToTypeDto(DocuType x) => new(x.RecId, x.TypeId, x.Name, x.TypeGroup, Kind(x.TypeGroup), x.FilePlace, null, _options.AllowedExtensions, _options.AllowedMimeTypes, _options.MaxFileSizeBytes);
    private static DocumentDto ToDto(DocuRef x) => new(x.RecId, x.RefTableId, x.RefRecId, x.RefCompanyId, x.TypeId, x.DocuType.Name, x.DocuType.TypeGroup, Kind(x.DocuType.TypeGroup), x.ValueRecId,
        x.Name, x.DocuValue.FileName, x.DocuValue.OriginalFileName, x.DocuValue.FileType, MimeType(x.DocuValue.FileType),
        null, x.Notes, x.DocuValue.AccessInformation, x.Restriction, x.CreatedBy, x.CreatedAt, x.LastModifiedBy, x.LastModifiedAt);
}
