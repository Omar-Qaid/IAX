using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Finance.Inventory.InventTable;

[Table("EcoResDistinctProduct")]
public sealed class EcoResDistinctProduct : SharedEntity<long>
{
}
