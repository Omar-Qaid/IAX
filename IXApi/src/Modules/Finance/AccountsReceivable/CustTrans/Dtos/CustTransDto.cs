using System;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    public record CustTransInquiryRow(long RecId, string Voucher, string TransactionType, DateTime TransDate, string Invoice, string Description, decimal AmountCur, decimal BalanceCur, string CurrencyCode, decimal AmountMst, decimal BalanceMst, string? Department, string? CostCenter, string? WorkerId);

    public class CustTransDto : EntityDto<long>
    {
    }
}

