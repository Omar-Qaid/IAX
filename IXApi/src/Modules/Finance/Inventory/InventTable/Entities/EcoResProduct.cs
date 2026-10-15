using IAX.IXApi.Shared.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Inventory.InventTable
{
    [Table("EcoResProduct")]
    public class EcoResProduct : SharedEntity<long>
    {
        [Required]
        [StringLength(FieldLengths.ProductNumber)]
        public string DisplayProductNumber { get; set; } = string.Empty;

        public ItemType ProductType { get; set; } = ItemType.Item;

        public EcoResProductSubtype ProductSubtype { get; set; } = EcoResProductSubtype.Product;

        [StringLength(FieldLengths.ProductSearchName)]
        public string SearchName { get; set; } = string.Empty;
    }
}

