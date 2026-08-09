using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    [Table("CustLedgerAccounts")]
    public class CustLedgerAccounts : Entity<long>
    {
        //----------------------------------------- Core Properties

        [Required]
        [StringLength(FieldLengths.PostingProfile)]
        public string PostingProfile { get; set; } = string.Empty;

        public AccountCode AccountCode { get; set; }

        [StringLength(FieldLengths.Num)]
        public string Num { get; set; } = string.Empty;

        [StringLength(FieldLengths.CollectionLetterCode)]
        public string CollectionLetterCourse { get; set; } = string.Empty;

        //----------------------------------------- Ledger Dimensions

        public long ClearingLedgerDimension { get; set; }

        public long DepositLedgerDimension { get; set; }

        public long EndorseLedgerDimension { get; set; }

        public long ExportSalesLedgerDimension { get; set; }

        public long LiabilitiesForDiscountLedgerDimension { get; set; }

        public long SummaryLedgerDimension { get; set; }

        public long VatPrepaymentsLedgerDimension { get; set; }

        public long WriteOffLedgerDimension { get; set; }

        //----------------------------------------- Other Properties

        public long CustInterest { get; set; }

        //----------------------------------------- Navigation Properties (Single)

        #region Navigation Properties Row

        [ForeignKey(nameof(PostingProfile))]
        public virtual CustLedger? CustLedgerTable { get; set; }


        [ForeignKey(nameof(Num))]
        public virtual CustGroup? CustGroupTable { get; set; }

        [ForeignKey(nameof(Num))]
        public virtual CustTable? CustTable { get; set; }


        //         [ForeignKey(nameof(ClearingLedgerDimension))]
        //         public virtual DimensionAttributeValueCombination? ClearingLedgerDimensionNavigation { get; set; }

        //         [ForeignKey(nameof(DepositLedgerDimension))]
        //         public virtual DimensionAttributeValueCombination? DepositLedgerDimensionNavigation { get; set; }

        //         [ForeignKey(nameof(EndorseLedgerDimension))]
        //         public virtual DimensionAttributeValueCombination? EndorseLedgerDimensionNavigation { get; set; }

        //         [ForeignKey(nameof(ExportSalesLedgerDimension))]
        //         public virtual DimensionAttributeValueCombination? ExportSalesLedgerDimensionNavigation { get; set; }

        //         [ForeignKey(nameof(LiabilitiesForDiscountLedgerDimension))]
        //         public virtual DimensionAttributeValueCombination? LiabilitiesForDiscountLedgerDimensionNavigation { get; set; }

        [ForeignKey(nameof(SummaryLedgerDimension))]
        public virtual DimensionAttributeValueCombination? SummaryDimensionAttributeValueCombinationTable { get; set; }

        //         [ForeignKey(nameof(VatPrepaymentsLedgerDimension))]
        //         public virtual DimensionAttributeValueCombination? VatPrepaymentsLedgerDimensionNavigation { get; set; }

        //         [ForeignKey(nameof(WriteOffLedgerDimension))]
        //         public virtual DimensionAttributeValueCombination? WriteOffLedgerDimensionNavigation { get; set; }

        #endregion

        //----------------------------------------- Navigation Properties (List)

        #region Navigation Properties List

        #endregion
    }
}

