using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.ERP.Inventory.InventTable
{
    [Table("EcoResProduct")]
    public class EcoResProduct : Entity<long>
    {
    }
}
