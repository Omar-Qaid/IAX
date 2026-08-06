using IAX.IXApi.Modules.ERP.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    /// <summary>Sales pool for grouping orders by sales channel (AX SalesPool).</summary>
    [Table("SalesPool")]
    public class SalesPool : Entity<int>
    {
        [StringLength(FieldLengths.SalesPoolId)]
        public string SalesPoolId { get; set; } = string.Empty;
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;
    }
}
