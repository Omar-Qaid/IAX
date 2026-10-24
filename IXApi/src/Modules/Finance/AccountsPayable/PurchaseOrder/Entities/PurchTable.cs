using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    [Table("PurchTable")]
    public class PurchTable : Entity<long>
    {
        [StringLength(FieldLengths.PurchId)]
        public string PurchId { get; set; } = string.Empty;
        [StringLength(FieldLengths.VendAccount)]
        public string OrderAccount { get; set; } = string.Empty;
        [StringLength(FieldLengths.VendAccount)]
        public string InvoiceAccount { get; set; } = string.Empty;
        public string PurchName { get; set; } = string.Empty;
        public PurchaseType PurchaseType { get; set; } = PurchaseType.Purch;
        public PurchStatus PurchStatus { get; set; } = PurchStatus.Backorder;
        /// <summary>Vendor return material authorization (RMA) number, for return orders.</summary>
        [StringLength(50)]
        public string ReturnItemNum { get; set; } = string.Empty;
        public DocumentStatus DocumentStatus { get; set; } = DocumentStatus.None;
        public string CurrencyCode { get; set; } = string.Empty;
        public string Payment { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public DateTime OrderDate { get; set; }
        public string InventSiteId { get; set; } = string.Empty;
        public string InventLocationId { get; set; } = string.Empty;
        public long DefaultDimension { get; set; }
    }
}
