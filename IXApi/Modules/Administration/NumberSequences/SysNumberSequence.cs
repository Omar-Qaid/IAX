using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    public enum SequenceResetCycle : byte
    {
        Never = 0,
        Yearly = 1,
        Monthly = 2,
        Daily = 3
    }

    [Table("SysNumberSequences")]
    [DataManagement]
    public class SysNumberSequence : LookupEntity<int>
    {
        public string EntityName { get; set; } = null!;
        public string? Prefix { get; set; }
        public string? Suffix { get; set; }
        public long SmallestValue { get; set; } = 1;
        public long LargestValue { get; set; } = 999999999;
        public long NextValue { get; set; } = 1;
        public int Step { get; set; } = 1;
        public string FormatPattern { get; set; } = "{PREFIX}-{SEQ}";
        public int PaddingLength { get; set; } = 5;
        public SequenceResetCycle ResetCycle { get; set; } = SequenceResetCycle.Never;
        public DateTime? LastResetAt { get; set; }
        public string? TenantId { get; set; }
    }
}

