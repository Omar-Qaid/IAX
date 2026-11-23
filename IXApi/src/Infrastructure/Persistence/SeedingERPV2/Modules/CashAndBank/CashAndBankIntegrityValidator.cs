using System.Reflection;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.CashAndBank;

/// <summary>
/// Validate what a payment resolves before it can post to a bank account: an active account inside
/// its activation window, in a currency the company knows, filed under an existing bank group, and
/// pointing at a posting dimension and a payment journal name that both exist.
/// </summary>
public sealed class CashAndBankIntegrityValidator : ISeedValidator
{
    private static readonly MethodInfo LoadMethod = typeof(CashAndBankIntegrityValidator).GetMethod(nameof(Load), BindingFlags.NonPublic | BindingFlags.Static)!;

    public async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        var accounts = await c.Rows<BankAccountTable>().ToListAsync(ct);
        if (accounts.Count == 0) throw new InvalidOperationException($"No bank account was seeded in {c.Company}.");
        foreach (var account in accounts)
        {
            if (account.BankAccountStatus != BankAccountStatus.Active)
                throw new InvalidOperationException($"Bank account {account.AccountId} is not active and cannot take a payment.");
            if (account.ActiveTo != default && account.ActiveFrom != default && account.ActiveTo < account.ActiveFrom)
                throw new InvalidOperationException($"Bank account {account.AccountId} closes before it opens.");
            if (account.LedgerDimension <= 0)
                throw new InvalidOperationException($"Bank account {account.AccountId} has no posting dimension.");
            await c.RequireAsync<DimensionAttributeValueCombination>(x => x.RecId == account.LedgerDimension, ct);
            await c.RequireAsync<BankGroup>(x => x.BankGroupId == account.BankGroupId, ct);
            await c.RequireAsync<Currency>(x => x.CurrencyCode == account.CurrencyCode, ct);
            if (!string.IsNullOrWhiteSpace(account.VendPaymentJournalName))
                await c.RequireAsync<LedgerJournalName>(x => x.JournalName == account.VendPaymentJournalName, ct);
        }
        if (accounts.Select(x => x.AccountNum).Distinct(StringComparer.Ordinal).Count() != accounts.Count)
            throw new InvalidOperationException("Two bank accounts share an account number.");

        var cache = new Dictionary<IEntityType, object[]>();
        async Task<object[]> Rows(IEntityType type)
        {
            if (!cache.TryGetValue(type, out var rows))
            {
                rows = await (Task<object[]>)LoadMethod.MakeGenericMethod(type.ClrType).Invoke(null, [c, ct])!;
                cache[type] = rows;
            }
            return rows;
        }
        foreach (var type in c.Db.Model.GetEntityTypes().Where(x => CashAndBankTableCatalog.MasterTypes.Contains(x.ClrType.Name)))
        {
            var rows = await Rows(type);
            if (rows.Length == 0) throw new InvalidOperationException($"CashAndBank table {type.ClrType.Name} was not seeded in {c.Company}.");
            foreach (var fk in type.GetForeignKeys())
            {
                var parents = await Rows(fk.PrincipalEntityType);
                foreach (var row in rows)
                {
                    var key = fk.Properties.Select(p => p.PropertyInfo?.GetValue(row)).ToArray();
                    if (key.Any(x => x == null)) continue;
                    if (!parents.Any(parent => fk.PrincipalKey.Properties.Select(p => p.PropertyInfo?.GetValue(parent)).SequenceEqual(key)))
                        throw new InvalidOperationException($"CashAndBank reference {type.ClrType.Name} -> {fk.PrincipalEntityType.ClrType.Name} is missing or crosses company {c.Company}.");
                }
            }
        }
    }

    private static async Task<object[]> Load<T>(SeedContext c, CancellationToken ct) where T : class
    {
        var query = c.Db.Set<T>().IgnoreQueryFilters().AsNoTracking();
        if (c.Db.Model.FindEntityType(typeof(T))!.FindProperty("DataAreaId") != null)
            query = query.Where(x => EF.Property<string>(x, "DataAreaId") == c.Company);
        return (await query.ToListAsync(ct)).Cast<object>().ToArray();
    }
}
