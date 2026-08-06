using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DocumentFormat.OpenXml.Office2010.CustomUI;
using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    [Table("CustGroup")]
    public class CustGroup : Entity<long>
    {
        //----------------------------------------- Core Properties
        [Required]
        [StringLength(FieldLengths.CustGroupId)]
        public string CustGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.PaymTermId)]
        public string PaymTermId { get; set; } = string.Empty;

        [StringLength(FieldLengths.TaxGroupId)]
        public string TaxGroupId { get; set; } = string.Empty;

        public long DefaultDimension { get; set; }

        //----------------------------------------- Financial
        public long AccountingCurrencyExchangeRateType { get; set; }

        public long ReportingCurrencyExchangeRateType { get; set; }

        public long CustAccountNumSeq { get; set; }

        public long CustWriteOffRefRecId { get; set; }

        //----------------------------------------- Bank
        public long BankCustPaymIdTable { get; set; }

        //----------------------------------------- Other Properties
        [StringLength(FieldLengths.PeriodId)]    
        public string ClearingPeriod { get; set; } = string.Empty;

        public NoYes PriceIncludeSalesTax { get; set; }

        #region Navigation Properties Row 
        #endregion

        #region Navigation Properties List
        #endregion
    }
}
