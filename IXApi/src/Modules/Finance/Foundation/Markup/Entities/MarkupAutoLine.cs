using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities;

[Table("MarkupAutoLine")]
public class MarkupAutoLine : Entity<long>
{
    public string CurrencyCode { get; set; } = string.Empty;
    public int CustomsAssessableValue_IN { get; set; }
    public decimal FromAmount { get; set; }
    public int Keep { get; set; }
    public decimal LineNum { get; set; }
    public MarkupCategory MarkupCategory { get; set; }
    public string MarkupCode { get; set; } = string.Empty;
    public string MarkupCurrencyCode { get; set; } = string.Empty;
    public int MCRReturnMarkup { get; set; }
    public MarkupModuleCategory ModuleCategory { get; set; }
    public MarkupModuleType ModuleType { get; set; }
    public int NotionalCharges_IN { get; set; }
    public decimal NotionalPct_IN { get; set; }
    public long TableRecId { get; set; }
    public int TableTableId { get; set; }
    public string TaxGroup { get; set; } = string.Empty;
    public string TaxItemGroup { get; set; } = string.Empty;
    public decimal ToAmount { get; set; }
    public string Txt { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string InventSiteId { get; set; } = string.Empty;
    public string InventLocationId { get; set; } = string.Empty;
}
