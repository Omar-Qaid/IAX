using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("BankAccountStatement")]
    public class BankAccountStatement : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.AccountId)]
        public string AccountId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.AccountStatementNum)]
        public string AccountStatementNum { get; set; } = string.Empty;

        public DateTime AccountStatementDate { get; set; }

        // ==========================================================
        // Financials & Balances
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        public decimal EndingBalance { get; set; }

        // ==========================================================
        // Reconciliation & Status Lifecycles
        // ==========================================================
        // Basic Properties
        public DateTime ReconcileDate { get; set; }
        public DateTime CancelDate { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? Currency { get; set; }

        // Note: Link to BankAccountTable can be configured here if that 
        // entity exists within your Cash and Bank Management module.
//         [ForeignKey(nameof(AccountId))]
//         public virtual BankAccountTable? BankAccount { get; set; }

        #endregion
    }
}
