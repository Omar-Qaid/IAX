using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Services
{
    public sealed class PendingAudit
    {
        public SysAuditLog Log { get; set; } = default!;
        public EntityEntry Entry { get; set; } = default!;
        public string? PkNames { get; set; }
    }

    [ScopedService]
    public class SysAuditService : ISysAuditService
    {
        public (IReadOnlyList<IProperty> PkProps, string? RecordId, bool IsTemporary) GetPkInfo(EntityEntry entry)
        {
            var pk = entry.Metadata.FindPrimaryKey();
            if (pk == null || pk.Properties.Count == 0)
                return (Array.Empty<IProperty>(), null, false);

            var props = pk.Properties;
            bool anyTemp = false;
            var parts = new List<string>(props.Count);

            foreach (var p in props)
            {
                var propEntry = entry.Property(p.Name);
                if (propEntry.IsTemporary)
                    anyTemp = true;

                var val = propEntry.CurrentValue;
                parts.Add(val?.ToString() ?? string.Empty);
            }

            if (anyTemp)
                return (props, null, true);

            var id = (parts.Count == 1) ? parts[0] : string.Join("|", parts);
            return (props, id, false);
        }
        public bool ShouldSkip(PropertyEntry prop)
        {
            // Skip navigations/shadows
            if (prop.Metadata.IsShadowProperty()) return false; // shadow values *can* be useful; keep if you want
            if (prop.Metadata.IsIndexerProperty()) return true;
            if (prop.Metadata.IsKey()) return true; // PK columns are already captured in RecordId

            var clr = prop.Metadata.ClrType;

            // Avoid logging large/binary data
            if (clr == typeof(byte[]) || clr == typeof(ReadOnlyMemory<byte>) || clr == typeof(Memory<byte>))
                return true;

            // Skip concurrency tokens (optional)
            if (prop.Metadata.IsConcurrencyToken) return true;

            var name = prop.Metadata.Name;
            if (name.Equals("CreatedBy", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("LastModifiedAt", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("LastModifiedBy", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("OwnerAccountId", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
        public bool ValuesEqual(object? a, object? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;

            // Normalize DateTime kinds
            if (a is DateTime da && b is DateTime db)
                return da.ToUniversalTime() == db.ToUniversalTime();

            // Normalize decimals/doubles as needed (customize if you want tolerance)
            return Equals(a, b);
        }
        public string? SafeToString(object? value, int maxLen = 4000)
        {
            if (value is null) return null;

            string s = value switch
            {
                DateTime dt => dt.ToString("o"), // ISO 8601 round-trip
                decimal dec => dec.ToString(System.Globalization.CultureInfo.InvariantCulture),
                double dbl => dbl.ToString(System.Globalization.CultureInfo.InvariantCulture),
                float flt => flt.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Guid g => g.ToString("D"),
                _ => value.ToString() ?? string.Empty
            };

            if (s.Length > maxLen) s = s[..maxLen];
            return s;
        }

    }
}
