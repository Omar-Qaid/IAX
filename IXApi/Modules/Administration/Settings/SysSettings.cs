using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.Administration.Settings
{
    [Table("SysSettings")]
    [DataManagement]
    public class SysSettings : Entity<int>
    {
        [Required]
        [MaxLength(256)]
        public string AppName { get; set; } = "HBMC ERP";

        [Required]
        [MaxLength(10)]
        public string DefaultLanguage { get; set; } = "en";

        [Required]
        [MaxLength(50)]
        public string TimeZone { get; set; } = "UTC";

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "USD";

        [Required]
        [MaxLength(50)]
        public string DateFormat { get; set; } = "YYYY-MM-DD";

        public bool EnableAuditLog { get; set; } = true;

        public long MaxUploadSize { get; set; } = 10485760; // 10MB default

        public int PaginationSize { get; set; } = 10;

        public int DecimalPlaces { get; set; } = 2;
    }
}


