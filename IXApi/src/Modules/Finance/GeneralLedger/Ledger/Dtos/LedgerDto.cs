using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.GeneralLedger
{
    public class LedgerDto : EntityDto<long>
    {
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty;

        public long ChartOfAccounts { get; set; }
        public long FiscalCalendar { get; set; }
        public long PrimaryForLegalEntity { get; set; }

        [StringLength(FieldLengths.AccountingCurrency)]
        public string AccountingCurrency { get; set; } = string.Empty;

        [StringLength(FieldLengths.ReportingCurrency)]
        public string ReportingCurrency { get; set; } = string.Empty;

        public long DefaultExchangeRateType { get; set; }
        public long ReportingCurrencyExchangeRateType { get; set; }
    }
}

