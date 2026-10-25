using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.Inventory
{
    public class InventItemSalesSetupDto : EntityDto<long>
    {
        public string ItemId { get; set; } = string.Empty;
        public string ItemDataAreaId { get; set; } = string.Empty;
        public string InventDimId { get; set; } = string.Empty;
        public string? InventDimIdDefault { get; set; }
        public decimal? StandardQty { get; set; }
        public decimal? LowestQty { get; set; }
        public decimal? HighestQty { get; set; }
        public decimal? MultipleQty { get; set; }
        public int? LeadTime { get; set; }
        public int? Stopped { get; set; }
        public int? Override { get; set; }
    }
}
