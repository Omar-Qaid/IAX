using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.Inventory
{
    public class InventItemSetupSupplyTypeDto : EntityDto<long>
    {
        public string ItemId { get; set; } = string.Empty;
        public string ItemDataAreaId { get; set; } = string.Empty;
        public int DefaultOrderType { get; set; }
    }
}
