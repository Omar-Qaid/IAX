using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Identity.Users;

namespace IAX.IXApi.Modules.Administration.Settings
{
    [Table("SysUserSettings")]
    [DataManagement]
    public class SysUserSettings : Entity<int>
    {
        [Required]
        [MaxLength(256)]
        public string UserId { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string Theme { get; set; } = "light";

        [Required]
        [MaxLength(10)]
        public string Language { get; set; } = "en";

        public int PageSize { get; set; } = 10;

        public bool NotificationEnabled { get; set; } = true;

        [MaxLength(2000)]
        public string DashboardLayout { get; set; } = "default";

        [ForeignKey(nameof(UserId))]
        public virtual AspNetUser? User { get; set; }
    }
}


