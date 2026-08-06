using System;
using IAX.IXApi.Modules.ERP.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.ERP.AccountsReceivable
{
    public class CustLedgerAccountsDto : EntityDto<long>
    {
        public string DataAreaId { get; set; } = "dat";
        public string PostingProfile { get; set; } = string.Empty;
        public AccountCode AccountCode { get; set; }
        public string Num { get; set; } = string.Empty;
        public string CollectionLetterCourse { get; set; } = string.Empty;
        public long ClearingLedgerDimension { get; set; }
        public long DepositLedgerDimension { get; set; }
        public long EndorseLedgerDimension { get; set; }
        public long ExportSalesLedgerDimension { get; set; }
        public long LiabilitiesForDiscountLedgerDimension { get; set; }
        public long SummaryLedgerDimension { get; set; }
        public long VatPrepaymentsLedgerDimension { get; set; }
        public long WriteOffLedgerDimension { get; set; }
        public long CustInterest { get; set; }
    }
}
