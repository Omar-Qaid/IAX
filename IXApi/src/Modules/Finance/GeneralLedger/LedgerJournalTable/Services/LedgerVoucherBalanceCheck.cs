using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;

/// <summary>Read-only arithmetic checks; not posting validation or approval.</summary>
public static class LedgerVoucherBalanceCheck
{
    public static IReadOnlyList<string> Check(IEnumerable<LedgerJournalTrans> source)
    {
        var lines = source.ToList();
        var errors = new List<string>();
        if (lines.Count == 0) errors.Add("The journal has no lines.");
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line.Voucher)) errors.Add($"Line {line.LineNum}: voucher is required.");
            if (line.TransDate == default) errors.Add($"Line {line.LineNum}: transaction date is required.");
            if (string.IsNullOrWhiteSpace(line.CurrencyCode)) errors.Add($"Line {line.LineNum}: currency is required.");
            if ((line.AmountCurDebit != 0 && line.AmountCurCredit != 0) ||
                (line.AmountCurDebit == 0 && line.AmountCurCredit == 0))
                errors.Add($"Line {line.LineNum}: enter a nonzero debit or credit, not both.");
        }
        // Never net unrelated vouchers, dates, currencies or companies. Explicit offsets
        // balance their own line arithmetically; account validity is a separate check.
        foreach (var group in lines.GroupBy(x => new { x.DataAreaId, x.Company, x.Voucher, Date = x.TransDate.Date, x.CurrencyCode }))
        {
            var balance = group.Where(x => x.OffsetLedgerDimension == 0)
                .Sum(x => x.AmountCurDebit - x.AmountCurCredit);
            if (balance != 0) errors.Add($"Voucher {group.Key.Voucher}, {group.Key.Date:yyyy-MM-dd}, {group.Key.CurrencyCode}: unbalanced by {balance}.");
        }
        return errors;
    }
}
