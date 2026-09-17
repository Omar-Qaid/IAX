using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("CompanyInfo")]
    public class CompanyInfo : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties

        public string DataArea { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public long Party { get; set; }

        public string? LanguageId { get; set; }

        public string? CurrencyCode { get; set; }

        public string? TaxLicenseNum { get; set; }

        public string? FederalTaxId { get; set; }

        public string? BankAccount { get; set; }

        public long? Calendar { get; set; }

        public string? TimeZone { get; set; }

        public byte[]? Logo { get; set; }
        public byte[]? ReportLogo { get; set; }

        public string? Memo { get; set; }

        public string? ArabicName { get; set; }

        public string? LocalizedRegion { get; set; }


        #region Navigation Properties Row
        public virtual DirPartyTable? DirPartyTable { get; set; }
        public virtual FiscalCalendar? FiscalCalendarTable { get; set; }
        public virtual Currency? Currency { get; set; }
        #endregion
    }
}
