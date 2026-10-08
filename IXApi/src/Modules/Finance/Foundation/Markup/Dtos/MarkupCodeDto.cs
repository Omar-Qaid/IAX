using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Foundation.Markup;

public sealed class MarkupCodeDto
{
    public string MarkupCode { get; set; } = string.Empty;
    public string Txt { get; set; } = string.Empty;
    public string TaxItemGroup { get; set; } = string.Empty;
    public NoYes McrBrokerContractFee { get; set; }
}
