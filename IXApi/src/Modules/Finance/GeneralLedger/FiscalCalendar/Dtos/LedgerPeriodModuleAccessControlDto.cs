using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.GeneralLedger;

public class LedgerPeriodModuleAccessControlDto : EntityDto<long>
{
    public long LedgerFiscalCalendarPeriod { get; set; }
    public LedgerPeriodAccessLevel LedgerAccessLevel { get; set; }
    public string LedgerUserGroupInfo { get; set; } = string.Empty;
    public LedgerPeriodAccessLevel TaxAccessLevel { get; set; }
    public string TaxUserGroupInfo { get; set; } = string.Empty;
    public LedgerPeriodAccessLevel BankAccessLevel { get; set; }
    public string BankUserGroupInfo { get; set; } = string.Empty;
    public LedgerPeriodAccessLevel CustAccessLevel { get; set; }
    public string CustUserGroupInfo { get; set; } = string.Empty;
    public LedgerPeriodAccessLevel VendAccessLevel { get; set; }
    public string VendUserGroupInfo { get; set; } = string.Empty;
}
