using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Interfaces
{
    public interface ISysAuditService
    {
        (IReadOnlyList<IProperty> PkProps, string? RecordId, bool IsTemporary) GetPkInfo(EntityEntry entry);
        bool ShouldSkip(PropertyEntry prop);
        bool ValuesEqual(object? a, object? b);
        string? SafeToString(object? value, int maxLen = 4000);
    }
}
