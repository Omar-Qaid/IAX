using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Shared.Features
{
    public class PaymTermDto : EntityDto<long>
    {
        public string PaymTermId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumOfMonths { get; set; }
        public int NumOfDays { get; set; }
        public int CutOffDay { get; set; }
        public int AdditionalMonths { get; set; }
        public string PaymDayId { get; set; } = string.Empty;
        public PaymMethod PaymMethod { get; set; }
        public long CashLedgerDimension { get; set; }
        public NoYes Cash { get; set; }
        public NoYes PostOffsettingAr { get; set; }
        public CreditCardPaymentType CreditCardPaymentType { get; set; }
        public NoYes CreditCardCreditCheck { get; set; }
        public string PaymSched { get; set; } = string.Empty;
        public NoYes CustomerUpdateDueDate { get; set; }
        public NoYes VendorUpdateDueDate { get; set; }
        public long CfmPaymentRequestTypePayment { get; set; }
        public long CfmPaymentRequestTypePrepayment { get; set; }
        public NoYes ShipCarrierCertifiedCheck { get; set; }
        public NoYes ShipCarrierAncillaryCharge { get; set; }
    }
}
