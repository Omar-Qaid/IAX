using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    [Table("SpecTrans")]
    public class SpecTrans : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [StringLength(FieldLengths.Code)]
        public string Code { get; set; } = string.Empty;

        // ==========================================================
        // Source Transaction Reference (Ref Context)
        // ==========================================================
        // Basic Properties
        public long RefRecId { get; set; }
        public int RefTableId { get; set; }
        [StringLength(FieldLengths.Company)]
        public string RefCompany { get; set; } = string.Empty;

        // ==========================================================
        // Specification Reference (Spec Context)
        // ==========================================================
        // Basic Properties
        public long SpecRecId { get; set; }
        public int SpecTableId { get; set; }
        [StringLength(FieldLengths.Company)]
        public string SpecCompany { get; set; } = string.Empty;

        // ==========================================================
        // Financials, Amounts & Settlement
        // ==========================================================
        // Basic Properties
        public decimal Balance01 { get; set; }
        public decimal CashDiscToTake { get; set; }
        public decimal CrossRate { get; set; }
        public DateTime SelectedDateUsedToCalcCashDisc { get; set; }

        // Enum Properties
        public NoYes FullSettlement { get; set; }
        public NoYes Payment { get; set; }
        public CustPaymentStatus PaymentStatus { get; set; }
    }
}
