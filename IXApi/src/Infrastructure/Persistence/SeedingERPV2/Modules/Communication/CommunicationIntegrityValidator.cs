using System.Text.RegularExpressions;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Communication;

/// <summary>
/// Validate what the notification service resolves before it can render a message: every seeded
/// template present and usable, its code unique across the installation, and every placeholder its
/// subject or body uses also declared in the variable list the service fills it from. An undeclared
/// placeholder is never substituted, so the recipient is sent the raw token.
/// </summary>
public sealed class CommunicationIntegrityValidator : ISeedValidator
{
    private static readonly Regex Placeholder = new(@"\{\{\s*([^}]*?)\s*\}\}", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    public async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        var templates = await c.Db.Set<SysNotificationTemplate>().IgnoreQueryFilters().AsNoTracking()
            .Where(x => !x.IsDeleted).ToListAsync(ct);
        if (templates.Count == 0) throw new InvalidOperationException("No notification template was seeded.");
        if (templates.Select(x => x.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count() != templates.Count)
            throw new InvalidOperationException("Two notification templates share a code; SysNotificationTemplate.Code is unique across companies.");

        foreach (var code in CommunicationSeedExample.References.Select(x => x.Value))
            if (!templates.Any(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Notification template {code} was not seeded; the event that raises it cannot be rendered.");

        foreach (var template in templates)
        {
            if (string.IsNullOrWhiteSpace(template.Name) || string.IsNullOrWhiteSpace(template.Subject) || string.IsNullOrWhiteSpace(template.Body))
                throw new InvalidOperationException($"Notification template {template.Code} is missing a name, a subject or a body.");
            if (string.IsNullOrWhiteSpace(template.Language))
                throw new InvalidOperationException($"Notification template {template.Code} declares no language.");

            var declared = (template.Variables ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var text in new[] { template.Subject, template.SubjectAR, template.Body, template.BodyAR })
            {
                if (string.IsNullOrEmpty(text)) continue;
                foreach (var match in Placeholder.Matches(text).Select(x => x.Groups[1].Value))
                {
                    if (string.IsNullOrWhiteSpace(match))
                        throw new InvalidOperationException($"Notification template {template.Code} carries an empty placeholder.");
                    if (!declared.Contains(match))
                        throw new InvalidOperationException($"Notification template {template.Code} uses placeholder {match}, which its variable list does not declare.");
                }
            }
        }
    }
}
