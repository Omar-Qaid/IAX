using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Shared.Features
{
    public class DlvTermDto : EntityDto<long>
    {
        public string Code { get; set; } = string.Empty;
        public string Txt { get; set; } = string.Empty;
        public decimal ShipCarrierFreeMinimum { get; set; }
        public int FreightChargeTerm { get; set; } 
        public int TaxLocationRole { get; set; }
        public NoYes ItmGoodsInTransitControl { get; set; }
        public NoYes ItmPortMandatory { get; set; }
    }
}
