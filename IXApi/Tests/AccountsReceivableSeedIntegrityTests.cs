using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IXApi.Tests;

[Collection("AccountsReceivable cycle stages")]
public sealed class AccountsReceivableSeedIntegrityTests
{
    [Fact]
    public async Task Inventory_close_sequences_are_prepared_and_consumed_counters_survive_reruns()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        using var provider = new ServiceCollection().AddAccountsReceivablePhaseOneSeeding().BuildServiceProvider();
        var runner = new SeedRunner(db,provider.GetServices<IDataSeeder>(),provider.GetServices<ISeedValidator>());
        await runner.RunAsync(2026,"otc-acceptance");
        foreach(var key in new[] { "InventClosing","SettleTransId" })
        {
            var sequence = await db.Set<SysNumberSequence>().SingleOrDefaultAsync(x => x.DataAreaId == "dat" && x.NumberSequence == key);
            Assert.NotNull(sequence);
            Assert.Equal(1,sequence.InUse);
            sequence.NextRec = 23;
        }
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await runner.RunAsync(2026,"otc-acceptance");
        foreach(var key in new[] { "InventClosing","SettleTransId" })
            Assert.Equal(23,(await db.Set<SysNumberSequence>().SingleAsync(x => x.NumberSequence == key)).NextRec);
    }

    [Theory]
    [InlineData("MISSING",false)]
    [InlineData("EMPTY",true)]
    public async Task Explicit_missing_customer_profile_cannot_borrow_another_profiles_accounts(string requestedProfile,bool createHeader)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        using var provider = new ServiceCollection().AddAccountsReceivablePhaseOneSeeding().BuildServiceProvider();
        await new SeedRunner(db,provider.GetServices<IDataSeeder>(),provider.GetServices<ISeedValidator>())
            .RunAsync(2026,"ar-profile-test");
        db.ChangeTracker.Clear();
        var customer = await db.Set<CustTable>().AsNoTracking().SingleAsync(x => x.AccountNum == "ARV2-dat-LOCAL");
        var support = new SubledgerPostingSupport(db);
        var defaultAccount = await support.ResolveCustomerSummaryAsync("dat",null,customer,default);
        var explicitAccount = await support.ResolveCustomerSummaryAsync("dat","ARV2",customer,default);
        Assert.Equal(GeneralLedgerSeedExample.CustomerControl,defaultAccount.MainAccountValue);
        Assert.Equal(defaultAccount.RecId,explicitAccount.RecId);
        Assert.Equal(defaultAccount.RecId,(await support.ResolveCustomerSummaryAsync("dat"," ",customer,default)).RecId);

        if(createHeader)
            db.Add(new CustLedger { DataAreaId = "dat",PostingProfile = requestedProfile,Name = "No rules",Settlement = NoYes.Yes });
        var bank = await db.Set<DimensionAttributeValueCombination>().SingleAsync(x => x.MainAccountValue == GeneralLedgerSeedExample.Bank);
        db.Add(new CustLedgerAccounts { DataAreaId = "dat",PostingProfile = "ARV2",AccountCode = AccountCode.Table,
            Num = customer.AccountNum,SummaryLedgerDimension = bank.RecId });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Equal(bank.RecId,(await support.ResolveCustomerSummaryAsync("dat","ARV2",customer,default)).RecId);

        db.Remove(await db.Set<CustLedgerAccounts>().SingleAsync(x => x.PostingProfile == "ARV2" && x.AccountCode == AccountCode.Table));
        await db.SaveChangesAsync();
        Assert.Equal(defaultAccount.RecId,(await support.ResolveCustomerSummaryAsync("dat","ARV2",customer,default)).RecId);
        var allRule = await db.Set<CustLedgerAccounts>().SingleAsync(x => x.PostingProfile == "ARV2");
        allRule.AccountCode = AccountCode.All;
        allRule.Num = string.Empty;
        await db.SaveChangesAsync();
        Assert.Equal(defaultAccount.RecId,(await support.ResolveCustomerSummaryAsync("dat","ARV2",customer,default)).RecId);

        var error = await Assert.ThrowsAsync<SubledgerPostingException>(() =>
            support.ResolveCustomerSummaryAsync("dat",requestedProfile,customer,default));
        Assert.Contains(requestedProfile,error.Message);
        Assert.Contains("not found in this company",error.Message);
    }

    [Fact]
    public void Shared_inventory_dimension_owner_is_an_explicit_seed_dependency()
    {
        Assert.Contains("ERPV2.DimensionAttributeValueCombinationSeeder",new SalesInventPostingSeeder().Dependencies);
        using var provider = new ServiceCollection().AddAccountsReceivablePhaseOneSeeding().BuildServiceProvider();
        var seeders = provider.GetServices<IDataSeeder>().ToArray();
        var ordered = new SeedDependencyResolver().Resolve(seeders.Reverse());
        Assert.Equal(seeders.Length,ordered.Count);
        Assert.True(ordered.ToList().FindIndex(x => x.Id == "ERPV2.DimensionAttributeValueCombinationSeeder")
            < ordered.ToList().FindIndex(x => x.Id == "ERPV2.SalesInventPostingSeeder"));
    }

    [Fact]
    public async Task Cross_module_corruptions_are_rejected_before_commit_and_every_write_rolls_back()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        await using var db = new Fixture(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
        await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript().Replace("N'", "'"));
        using var provider = new ServiceCollection().AddAccountsReceivablePhaseOneSeeding().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        var validators = scope.ServiceProvider.GetServices<ISeedValidator>().ToArray();
        var runner = new SeedRunner(db,seeders,validators);
        await runner.RunAsync(2026,"ar-integrity-test");
        var runId = Guid.NewGuid().ToString("N");
        void Log(string name,string status,string? detail = null)
        {
            var path = Environment.GetEnvironmentVariable("IXAPI_AR_INTEGRITY_REPORT")
                ?? Path.Combine(AppContext.BaseDirectory,"ar-integrity-results.jsonl");
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            File.AppendAllText(path,System.Text.Json.JsonSerializer.Serialize(new { runId,stage = name,status,detail,atUtc = DateTime.UtcNow }) + Environment.NewLine);
        }
        Log("initial-seed","PASSED");
        // Number allocation already consumed by an operator must survive every rerun and failed attempt.
        var sequence = await db.Set<SysNumberSequence>().SingleAsync(x => x.NumberSequence == "CustSettlement");
        sequence.NextRec = 23;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var baseline = await FingerprintAsync(db);
        foreach (var kind in new[] { "invoice-account", "currency", "payment-mode", "foreign-party", "postal-role",
            "sales-issue-account", "sales-cost-account", "tax-exemption", "manual-sequence", "nonincrementing-sequence",
            "return-disposition", "price-currency", "missing-party", "inactive-party", "customer-control", "discount-account",
            "journal-exhausted", "journal-below-minimum", "journal-missing-counter",
            "output-tax-account", "tax-base", "tax-calculation", "tax-rounding", "tax-currency",
            "price-party-scope", "price-item-scope", "price-minimum-quantity", "price-maximum-quantity",
            "price-search-disabled", "price-date-policy" })
        {
            Log(kind,"RUNNING");
            try
            {
            var corrupt = new CorruptSeeder(seeders.Select(x => x.Id).ToArray(),kind);
            var attempt = new SeedRunner(db,seeders.Append(corrupt),validators);
            var error = await Record.ExceptionAsync(() => attempt.RunAsync(2026,"ar-integrity-test"));
            Assert.NotNull(error);
            if(kind == "missing-party") Assert.IsType<DbUpdateException>(error);
            else
            {
                Assert.IsType<InvalidOperationException>(error);
                Assert.True(error.Message.Contains("AR phase-one integrity:") || error.Message.Contains("reference in dat")
                    || error.Message.Contains("DirPartyTable is deleted or inactive"),kind + ": " + error.Message);
                if(kind == "output-tax-account") Assert.Contains("output tax must reference",error.Message);
                if(kind is "tax-base" or "tax-calculation" or "tax-rounding" or "tax-currency")
                    Assert.Contains("calculation base/method/rounding/currency",error.Message);
                if(kind is "price-party-scope" or "price-item-scope" or "price-minimum-quantity" or "price-maximum-quantity")
                    Assert.Contains("party/item scope or quantity range",error.Message);
                if(kind == "price-search-disabled") Assert.Contains("price-agreement search is disabled",error.Message);
                if(kind == "price-date-policy") Assert.Contains("sales price date must use the delivery date",error.Message);
            }
            Assert.Equal(baseline,await FingerprintAsync(db));
            Assert.False(await db.Set<TaxExemptCodeTable>().IgnoreQueryFilters().AnyAsync(x => x.ExemptCode == "AR-BAD"));
            Assert.False(await db.Set<DirPartyTable>().IgnoreQueryFilters().AnyAsync(x => x.PartyNumber == "AR-FOREIGN"));
            Assert.False(await db.Set<CustPaymModeTable>().IgnoreQueryFilters().AnyAsync(x => x.PaymMode == "AR-ALT"));
            Assert.False(await db.Set<SysDataSeedLog>().AnyAsync(x => x.SeederId == corrupt.Id));
            db.ChangeTracker.Clear();
            await runner.RunAsync(2026,"ar-integrity-test");
            Assert.Equal(baseline,await FingerprintAsync(db));
            db.ChangeTracker.Clear();
            Log(kind,"PASSED","Rejected corruption, rolled back data/counter/history, and reran without duplicates.");
            }
            catch(Exception error) { Log(kind,"FAILED",error.ToString()); throw; }
        }
        // Existing user customization is preserved, never silently repaired by the seeder.
        var customer = await db.Set<CustTable>().SingleAsync(x => x.AccountNum == "ARV2-dat-LOCAL");
        customer.InvoiceAccount = "ARV2-dat-USD";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var conflict = await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(2026,"ar-integrity-test"));
        Assert.Contains("Conflicting CustTable.InvoiceAccount",conflict.Message);
        Assert.Equal("ARV2-dat-USD",(await db.Set<CustTable>().AsNoTracking().SingleAsync(x => x.AccountNum == "ARV2-dat-LOCAL")).InvoiceAccount);
        Log("preserve-user-customization","PASSED");
        Log("run","PASSED");
    }

    private static async Task<string> FingerprintAsync(ApplicationDbContext db)
    {
        var customers = await db.Set<CustTable>().AsNoTracking().OrderBy(x => x.AccountNum)
            .Select(x => new { x.RecId,x.AccountNum,x.InvoiceAccount,x.CurrencyCode,x.PaymModeId,x.Party }).ToListAsync();
        var postings = await db.Set<InventPosting>().AsNoTracking().OrderBy(x => x.RecId)
            .Select(x => new { x.RecId,x.InventAccountType,x.LedgerDimension }).ToListAsync();
        var sequences = await db.Set<SysNumberSequence>().AsNoTracking().OrderBy(x => x.NumberSequence)
            .Select(x => new { x.RecId,x.NumberSequence,x.NextRec,x.Manual,x.NoIncrement }).ToListAsync();
        var taxes = await db.Set<TaxTable>().AsNoTracking().OrderBy(x => x.RecId)
            .Select(x => new { x.RecId,x.TaxCode,x.TaxCurrencyCode,x.TaxBase,x.TaxCalcMethod,x.TaxRoundOff }).ToListAsync();
        var taxAccounts = await db.Set<TaxLedgerAccountGroup>().AsNoTracking().OrderBy(x => x.RecId)
            .Select(x => new { x.RecId,x.TaxOutgoingLedgerDimension }).ToListAsync();
        var prices = await db.Set<PriceDiscTable>().AsNoTracking().OrderBy(x => x.RecId)
            .Select(x => new { x.RecId,x.AccountCode,x.ItemCode,x.QuantityAmountFrom,x.QuantityAmountTo,
                x.Currency,x.UnitId,x.Amount,x.PriceUnit,x.InventDimId,x.FromDate,x.ToDate }).ToListAsync();
        var customerParameters = await db.Set<CustParameters>().AsNoTracking().OrderBy(x => x.RecId)
            .Select(x => new { x.RecId,x.PriceDiscSearchPrice }).ToListAsync();
        var salesParameters = await db.Set<SalesParameters>().AsNoTracking().OrderBy(x => x.RecId)
            .Select(x => new { x.RecId,x.PriceDateType }).ToListAsync();
        return System.Text.Json.JsonSerializer.Serialize(new { customers,postings,sequences,taxes,taxAccounts,prices,customerParameters,salesParameters,
            Parties=await db.Set<DirPartyTable>().IgnoreQueryFilters().CountAsync(),
            History=await db.Set<SysDataSeedLog>().CountAsync(),
            Exemptions=await db.Set<TaxExemptCodeTable>().IgnoreQueryFilters().CountAsync() });
    }

    private sealed class CorruptSeeder(string[] dependencies,string kind) : IDataSeeder
    {
        public string Id => "ERPV2.AR.IntegrityTest." + kind;
        public IReadOnlyCollection<string> Dependencies => dependencies;
        public async Task SeedAsync(SeedContext c,CancellationToken ct)
        {
            var customer = await c.RequireAsync<CustTable>(x => x.AccountNum == "ARV2-dat-LOCAL",ct);
            // This increment is a late write alongside the bad reference, not a counter reset.
            var sequence = await c.RequireAsync<SysNumberSequence>(x => x.NumberSequence == "CustSettlement",ct);
            sequence.NextRec += 17;
            switch(kind)
            {
                case "invoice-account": customer.InvoiceAccount = "ARV2-dat-USD"; break;
                case "currency": customer.CurrencyCode = "USD"; break;
                case "payment-mode":
                    var mode = await c.RequireAsync<CustPaymModeTable>(x => x.PaymMode == "ARV2",ct);
                    await c.EnsureAsync<CustPaymModeTable>(x => x.PaymMode == "AR-ALT",new() { PaymMode = "AR-ALT",Name = "Must roll back",
                        AccountType = mode.AccountType, PaymentLedgerDimension = mode.PaymentLedgerDimension, PaymJournalNameId = mode.PaymJournalNameId },ct);
                    customer.PaymModeId = "AR-ALT"; break;
                case "foreign-party":
                    var foreign = new DirPartyTable { DataAreaId = "oth",PartyNumber = "AR-FOREIGN",Name = "Foreign",NameAlias = "Foreign",LanguageId = "en-us",IsActive = NoYes.Yes };
                    c.Db.Add(foreign); await c.Db.SaveChangesAsync(ct); customer.Party = foreign.RecId; break;
                case "missing-party": customer.Party = long.MaxValue; break;
                case "inactive-party": (await c.RequireAsync<DirPartyTable>(x => x.RecId == customer.Party,ct)).IsActive = NoYes.No; break;
                case "postal-role":
                    var party = await c.RequireAsync<DirPartyTable>(x => x.RecId == customer.Party,ct);
                    (await c.RequireAsync<DirPartyLocation>(x => x.Party == party.RecId && x.Location == party.PrimaryAddressLocation,ct)).IsPostalAddress = NoYes.No; break;
                case "sales-issue-account": case "sales-cost-account":
                    var bank = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == GeneralLedgerSeedExample.Bank,ct);
                    var type = kind == "sales-issue-account" ? InventAccountType.SalesIssue : InventAccountType.SalesConsump;
                    (await c.RequireAsync<InventPosting>(x => x.InventAccountType == type && x.ItemRelation == "APV2",ct)).LedgerDimension = bank.RecId; break;
                case "tax-exemption":
                    await c.EnsureAsync<TaxExemptCodeTable>(x => x.ExemptCode == "AR-BAD",new() { ExemptCode = "AR-BAD",Description = "Must roll back" },ct);
                    (await c.RequireAsync<TaxOnItem>(x => x.TaxCode == "ARV2-15" && x.TaxItemGroup == "APV2",ct)).TaxExemptCode = "AR-BAD"; break;
                case "manual-sequence": sequence.Manual = 1; break;
                case "nonincrementing-sequence": sequence.NoIncrement = 1; break;
                case "output-tax-account":
                    var advance = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == GeneralLedgerSeedExample.CustomerAdvance,ct);
                    (await c.RequireAsync<TaxLedgerAccountGroup>(x => x.TaxAccountGroup == "ARV2",ct)).TaxOutgoingLedgerDimension = advance.RecId; break;
                case "tax-base": case "tax-calculation": case "tax-rounding": case "tax-currency":
                    var tax = await c.RequireAsync<TaxTable>(x => x.TaxCode == "ARV2-15",ct);
                    if(kind == "tax-base") tax.TaxBase = TaxBaseType.PctPerGross;
                    if(kind == "tax-calculation") tax.TaxCalcMethod = TaxCalcMethod.Total;
                    if(kind == "tax-rounding") tax.TaxRoundOff = 1m;
                    if(kind == "tax-currency") tax.TaxCurrencyCode = "USD";
                    break;
                case "journal-exhausted": case "journal-below-minimum": case "journal-missing-counter":
                    var journal = await c.RequireAsync<LedgerJournalName>(x => x.JournalName == "ARV2-PAY",ct);
                    var voucher = await c.RequireAsync<SysNumberSequence>(x => x.RecId == journal.NumberSequenceTable,ct);
                    voucher.NextRec = kind == "journal-exhausted" ? voucher.Highest + 1
                        : kind == "journal-below-minimum" ? 0 : null;
                    break;
                case "return-disposition":
                    (await c.RequireAsync<ReturnDispositionCode>(x => x.DispositionCodeId == "ARV2-RET",ct)).DispositionAction = DispositionAction.CreditOnly; break;
                case "price-currency":
                    (await c.RequireAsync<PriceDiscTable>(x => x.AccountRelation == customer.AccountNum && x.ItemRelation == "APV2-STOCK" && x.Relation == PriceType.PriceSales,ct)).Currency = "USD"; break;
                case "price-party-scope": case "price-item-scope": case "price-minimum-quantity": case "price-maximum-quantity":
                    var price = await c.RequireAsync<PriceDiscTable>(x => x.AccountRelation == customer.AccountNum
                        && x.ItemRelation == "APV2-STOCK" && x.Relation == PriceType.PriceSales,ct);
                    if(kind == "price-party-scope") price.AccountCode = PriceDiscPartyCodeType.GroupId;
                    if(kind == "price-item-scope") price.ItemCode = PriceDiscProductCodeType.GroupId;
                    if(kind == "price-minimum-quantity") price.QuantityAmountFrom = 1000m;
                    if(kind == "price-maximum-quantity") price.QuantityAmountTo = 10m;
                    break;
                case "price-search-disabled":
                    (await c.RequireAsync<CustParameters>(x => x.Key == 0,ct)).PriceDiscSearchPrice = NoYes.No; break;
                case "price-date-policy":
                    (await c.RequireAsync<SalesParameters>(x => x.DataAreaId == c.Company,ct)).PriceDateType = PriceDateType.Today; break;
                case "customer-control":
                    var bankControl = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == GeneralLedgerSeedExample.Bank,ct);
                    (await c.RequireAsync<CustLedgerAccounts>(x => x.PostingProfile == "ARV2" && x.AccountCode == AccountCode.Group,ct)).SummaryLedgerDimension = bankControl.RecId; break;
                case "discount-account":
                    var expense = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == GeneralLedgerSeedExample.MaintenanceExpense,ct);
                    (await c.RequireAsync<CashDisc>(x => x.CashDiscCode == "ARV2-2",ct)).CashDiscLedgerDimension = expense.RecId; break;
                default: throw new InvalidOperationException("Unknown corruption " + kind);
            }
            await c.Db.SaveChangesAsync(ct);
        }
    }

    private sealed class Fixture(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            foreach(var entity in builder.Model.GetEntityTypes())
            foreach(var property in entity.GetProperties())
            {
                var type=Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                property.SetColumnType(type == typeof(byte[]) ? "BLOB" : type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal) || type == typeof(Guid) ? "TEXT" : "INTEGER");
                property.SetDefaultValueSql(null);
                if(property.Name == "RowVersion") property.SetDefaultValueSql("X'00'");
            }
        }
    }
}
