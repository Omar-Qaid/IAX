using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IAX.IXApi.Tests.Support.Assertions;

/// <summary>Posted general-ledger balances, read from <see cref="GeneralJournalAccountEntry"/> as a ledger inquiry would.</summary>
public static class LedgerBalances
{
    /// <summary>Net accounting-currency balance of a main account in a company (debit positive, credit negative).</summary>
    public static async Task<decimal> OfMainAccountAsync(ApplicationDbContext db, string company, string mainAccount)
    {
        var dimensions = await db.Set<DimensionAttributeValueCombination>().AsNoTracking()
            .Where(x => x.DataAreaId == company && x.MainAccountValue == mainAccount).Select(x => x.RecId).ToArrayAsync();
        return (await db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.DataAreaId == company && dimensions.Contains(x.LedgerDimension)).ToListAsync())
            .Sum(x => x.AccountingCurrencyAmount);
    }

    /// <summary>Every voucher in the company balances: its debits equal its credits.</summary>
    public static async Task AssertEveryVoucherBalancesAsync(ApplicationDbContext db, string company)
    {
        var entries = await db.Set<GeneralJournalAccountEntry>().AsNoTracking().Where(x => x.DataAreaId == company).ToListAsync();
        Assert.NotEmpty(entries);
        Assert.All(entries.GroupBy(x => x.GeneralJournalEntry), voucher =>
            Assert.True(voucher.Sum(x => x.AccountingCurrencyAmount) == 0m, $"Voucher entry {voucher.Key} does not balance."));
    }
}
