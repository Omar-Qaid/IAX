namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;

public sealed record CustOpenTransDto(string AccountNum, string CustomerName, string DataAreaId,
    string CustomerCurrency, string AccountingCurrency, decimal CustomerBalanceMst, bool Reconciled,
    IReadOnlyList<CustOpenTransRowDto> Transactions);

public sealed class CustOpenTransRowDto
{
    public long RecId { get; set; }
    public long TransRecId { get; set; }
    public string Invoice { get; set; } = string.Empty;
    public string Voucher { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string PaymentMode { get; set; } = string.Empty;
    public string PaymentReference { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = string.Empty;
    public string CashDiscCode { get; set; } = string.Empty;
    public string Worker { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string CostCenter { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string BusinessUnit { get; set; } = string.Empty;
    public decimal AmountCur { get; set; }
    public decimal AmountMst { get; set; }
    public decimal OriginalAmountCur { get; set; }
    public decimal OriginalAmountMst { get; set; }
    public decimal SettledAmountMst { get; set; }
    public decimal SettledAmountCur { get; set; }
    public decimal PossibleCashDisc { get; set; }
    public decimal CashDiscTaken { get; set; }
    public DateTime TransDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CashDiscDate { get; set; }
    public DateTime LastSettleDate { get; set; }
    public int UseCashDisc { get; set; }
    public bool Prepayment { get; set; }
}
