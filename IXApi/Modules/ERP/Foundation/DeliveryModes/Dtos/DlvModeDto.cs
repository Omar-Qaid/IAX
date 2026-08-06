using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Shared.Features
{
    public class DlvModeDto : EntityDto<long>
    {
        public string Code { get; set; } = string.Empty;
        public string Txt { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public WHSShipCarrierDlvType ShipCarrierDlvType { get; set; }
        public string MarkupGroup { get; set; } = string.Empty;
        public string McrExpedite { get; set; } = string.Empty;
        public long DomPriority { get; set; }
    }
}
