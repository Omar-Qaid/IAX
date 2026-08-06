using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("Ledger")]
    public class Ledger : Entity<long>
    {
        //----------------------------------------- Core Information & Structs
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Description)]
        public string Description { get; set; } = string.Empty;

        public long ChartOfAccounts { get; set; }
        public long FiscalCalendar { get; set; }
        public long PrimaryForLegalEntity { get; set; }

        // ==========================================================
        // Currency Configurations
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.AccountingCurrency)]
        public string AccountingCurrency { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.ReportingCurrency)]
        public string ReportingCurrency { get; set; } = string.Empty;

        // ==========================================================
        // Exchange Rate Multi-Type Policies
        // ==========================================================
        // Basic Properties
        public long DefaultExchangeRateType { get; set; }
        public long ReportingCurrencyExchangeRateType { get; set; }
        public long BudgetExchangeRateType { get; set; }

        // ==========================================================
        // Budgeting & Period Management
        // ==========================================================
        // Basic Properties
        public DateTime MostRecentYearEndClose { get; set; }

        // Enum Properties
        public NoYes IsBudgetControlEnabled { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(FiscalCalendar))]
//         public virtual FiscalCalendar? Calendar { get; set; }

//         [ForeignKey(nameof(AccountingCurrency))]
//         public virtual Currency? BaseAccountingCurrency { get; set; }

//         [ForeignKey(nameof(ReportingCurrency))]
//         public virtual Currency? BaseReportingCurrency { get; set; }

//         [ForeignKey(nameof(DefaultExchangeRateType))]
//         public virtual ExchangeRateType? DefaultExchangeRateTypeNav { get; set; }

//         [ForeignKey(nameof(ReportingCurrencyExchangeRateType))]
//         public virtual ExchangeRateType? ReportingExchangeRateTypeNav { get; set; }

//         [ForeignKey(nameof(BudgetExchangeRateType))]
//         public virtual ExchangeRateType? BudgetExchangeRateTypeNav { get; set; }

        // Note: Links to LegalEntity (DirCompanyBase)
        // can be mapped here once those contextual components are generated.

//         [ForeignKey(nameof(ChartOfAccounts))]
//         public virtual LedgerChartOfAccounts? ChartOfAccountsDefinition { get; set; }

        #endregion
    }
}
