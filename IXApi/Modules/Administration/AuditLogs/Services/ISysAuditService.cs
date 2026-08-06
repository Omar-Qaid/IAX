using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Services
{
    public interface ISysAuditService
    {
        (IReadOnlyList<IProperty> PkProps, string? RecordId, bool IsTemporary) GetPkInfo(EntityEntry entry);
        bool ShouldSkip(PropertyEntry prop);
        bool ValuesEqual(object? a, object? b);
        string? SafeToString(object? value, int maxLen = 4000);
    }
}
