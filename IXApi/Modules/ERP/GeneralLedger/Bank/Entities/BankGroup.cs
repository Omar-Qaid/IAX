using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("BankGroup")]
    public class BankGroup : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.BankGroupId)]
        public string BankGroupId { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.RegistrationNum)]
        public string RegistrationNum { get; set; } = string.Empty;

        // Enum Properties
        public BankCodeType BankCodeType { get; set; }

        // ==========================================================
        // Currency & Payment Identification
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.CurrencyCode)]
        public string CurrencyCode { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.CompanyPaymId)]
        public string CompanyPaymId { get; set; } = string.Empty;

        // ==========================================================
        // File Formats & Layout Templates
        // ==========================================================
        // Basic Properties
        public long BankStatementFormat { get; set; }
        public long TemplateRefRecId { get; set; }
        public long CurrencyTemplateRefRecId { get; set; }

        // ==========================================================
        // Regional Localization & Logistics
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.BankCorrAccount_W)]
        public string BankCorrAccount_W { get; set; } = string.Empty;

        public long Location { get; set; }


        #region Navigation Properties Row

//         [ForeignKey(nameof(CurrencyCode))]
//         public virtual Currency? Currency { get; set; }

//         [ForeignKey(nameof(Location))]
//         public virtual LogisticsLocation? LogisticsLocation { get; set; }

        #endregion
    }
}
