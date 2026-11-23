using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Organization;

/// <summary>
/// Validate what the document service resolves before it can file an attachment: every seeded
/// document type present and usable, its identifier unique across the installation as the alternate
/// key requires, and every attachment already stored pointing at a type and a stored value that
/// still exist.
/// </summary>
public sealed class OrganizationIntegrityValidator : ISeedValidator
{
    public async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        var types = await c.Db.Set<DocuType>().IgnoreQueryFilters().AsNoTracking().ToListAsync(ct);
        if (types.Count == 0) throw new InvalidOperationException("No document type was seeded.");
        foreach (var typeId in OrganizationSeedExample.References.Select(x => x.Value))
        {
            var matches = types.Where(x => string.Equals(x.TypeId, typeId, StringComparison.Ordinal) && !x.IsDeleted).ToList();
            if (matches.Count != 1)
                throw new InvalidOperationException($"Expected exactly one usable document type {typeId}; found {matches.Count}.");
            if (string.IsNullOrWhiteSpace(matches[0].Name))
                throw new InvalidOperationException($"Document type {typeId} has no name.");
            if (string.IsNullOrWhiteSpace(matches[0].DataAreaId))
                throw new InvalidOperationException($"Document type {typeId} carries no company.");
        }
        if (types.Select(x => x.TypeId).Distinct(StringComparer.Ordinal).Count() != types.Count)
            throw new InvalidOperationException("Two document types share a type identifier; the DocuType alternate key is unique across companies.");

        var typeIds = types.Where(x => !x.IsDeleted).Select(x => x.TypeId).ToHashSet(StringComparer.Ordinal);
        var attachments = await c.Rows<DocuRef>().AsNoTracking()
            .Select(x => new { x.RecId, x.TypeId, x.ValueRecId }).ToListAsync(ct);
        if (attachments.Count == 0) return;
        foreach (var attachment in attachments)
            if (!typeIds.Contains(attachment.TypeId))
                throw new InvalidOperationException($"Attachment {attachment.RecId} is filed under missing document type {attachment.TypeId}.");
        if (attachments.Select(x => x.ValueRecId).Distinct().Count() != attachments.Count)
            throw new InvalidOperationException("Two attachments share a stored value; DocuRef.ValueRecId is unique.");
        var values = await c.Rows<DocuValue>().AsNoTracking().Select(x => x.RecId).ToListAsync(ct);
        var stored = values.ToHashSet();
        foreach (var attachment in attachments.Where(x => !stored.Contains(x.ValueRecId)))
            throw new InvalidOperationException($"Attachment {attachment.RecId} points at missing stored value {attachment.ValueRecId}.");
    }
}
