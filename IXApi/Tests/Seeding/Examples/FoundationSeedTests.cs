using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace IAX.IXApi.Tests;
[Collection("AccountsPayable phase one")]
public sealed class FoundationSeedTests
{
    [Fact]
    public void Registration_is_idempotent_and_dependencies_resolve()
    {
        using var provider = new ServiceCollection().AddFoundationPhaseOneSeeding().AddFoundationPhaseOneSeeding().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        var ordered = new SeedDependencyResolver().Resolve(seeders);
        Assert.Equal(seeders.Length, ordered.Count);
        Assert.Equal(seeders.Length, seeders.Select(x => x.Id).Distinct().Count());
        Assert.Contains(ordered, x => x.Id == "ERPV2.Foundation.HcmWorkerSeeder");
        Assert.Contains(scope.ServiceProvider.GetServices<ISeedValidator>(), x => x is FoundationIntegrityValidator);
    }
    [Fact]
    public async Task All_master_tables_are_populated_and_reruns_preserve_keys_and_conflicts_roll_back()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        using var provider = new ServiceCollection().AddFoundationPhaseOneSeeding().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var runner = new SeedRunner(db, scope.ServiceProvider.GetServices<IDataSeeder>(), scope.ServiceProvider.GetServices<ISeedValidator>());
        await runner.RunAsync(2026, "foundation-test");
        var contactCodes = ContactPersonDemoData.Examples.Select(x => x.Code).ToArray();
        var demoContacts = await db.Set<ContactPerson>().Where(x => contactCodes.Contains(x.ContactPersonId)).ToListAsync();
        Assert.Equal(3, demoContacts.Count);
        Assert.Single(demoContacts.Where(x => x.Inactive == IAX.IXApi.Modules.Finance.Common.NoYes.Yes));
        var contactNameIds = new List<long>();
        foreach (var contact in demoContacts)
        {
            var example = ContactPersonDemoData.Examples.Single(x => x.Code == contact.ContactPersonId);
            var party = await db.Set<DirPartyTable>().SingleAsync(x => x.RecId == contact.Party);
            Assert.Equal(example.Name, party.Name);
            Assert.Equal(example.ArabicName, party.NameAlias);
            var personName = await db.Set<DirPersonName>().SingleAsync(x => x.Person == party.RecId);
            Assert.Equal(example.FirstName, personName.FirstName);
            Assert.Equal(example.LastName, personName.LastName);
            contactNameIds.Add(personName.RecId);
            var customer = await db.Set<IAX.IXApi.Modules.Finance.AccountsReceivable.CustTable>()
                .SingleAsync(x => x.AccountNum == "FNV2-" + db.GetDataAreaId() + "-CUSTOMER");
            Assert.Equal(customer.Party, contact.ContactForParty);
            Assert.Equal(example.CustomerContact ? customer.AccountNum : null, contact.CustAccount);
        }
        var mapped = db.Model.GetEntityTypes().Where(x => FoundationTableCatalog.MasterTypes.Contains(x.ClrType.Name)).ToArray();
        Assert.Equal(FoundationTableCatalog.MasterTypes.Count, mapped.Length);
        var counts = new Dictionary<string,long>();
        foreach (var type in mapped)
        {
            await using var cmd = database.Connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM \"" + type.GetTableName() + "\"";
            var count = Convert.ToInt64(await cmd.ExecuteScalarAsync());
            Assert.True(count > 0, type.ClrType.Name);
            counts[type.ClrType.Name] = count;
        }
        db.ChangeTracker.Clear();
        await runner.RunAsync(2026, "foundation-test");
        foreach (var type in mapped)
        {
            await using var cmd = database.Connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM \"" + type.GetTableName() + "\"";
            Assert.Equal(counts[type.ClrType.Name], Convert.ToInt64(await cmd.ExecuteScalarAsync()));
        }
        Assert.Equal(contactNameIds.OrderBy(x => x), await db.Set<DirPersonName>()
            .Where(x => contactNameIds.Contains(x.RecId)).OrderBy(x => x.RecId).Select(x => x.RecId).ToListAsync());
        Assert.Equal(3, await db.Set<DirPersonName>().CountAsync());
        var delivery = await db.Set<DlvMode>().SingleAsync(x => x.Code == "FNV2");
        delivery.Txt = "Operator change";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(2027, "foundation-test"));
        Assert.False(await db.Set<TaxReportPeriod>().AnyAsync(x => x.FromDate.Year == 2027));
    }
    [Fact]
    public async Task Late_cross_company_reference_is_rejected_and_rolled_back()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        using var provider = new ServiceCollection().AddFoundationPhaseOneSeeding().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        var validators = scope.ServiceProvider.GetServices<ISeedValidator>().ToArray();
        var runner = new SeedRunner(db, seeders, validators);
        await runner.RunAsync(2026, "foundation-test");
        var original = await db.Set<IAX.IXApi.Modules.Finance.Foundation.Structure.HcmPosition>().SingleAsync(x => x.Code == "FNV2-BUYER");
        var originalUnit = original.OrganizationUnitId;
        db.ChangeTracker.Clear();
        var corruptingRunner = new SeedRunner(db, seeders.Append(new CrossCompanyReference(seeders.Select(x => x.Id).ToArray())), validators);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => corruptingRunner.RunAsync(2026, "foundation-test"));
        Assert.Contains("crosses company", error.Message);
        Assert.Equal(originalUnit, (await db.Set<IAX.IXApi.Modules.Finance.Foundation.Structure.HcmPosition>().SingleAsync(x => x.Code == "FNV2-BUYER")).OrganizationUnitId);
        Assert.False(await db.Set<IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits.OrganizationUnit>().IgnoreQueryFilters().AnyAsync(x => x.Code == "FNV2-OTHER"));
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Unavailable_parent_is_rejected_and_late_changes_roll_back(bool deleted)
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        using var provider = new ServiceCollection().AddFoundationPhaseOneSeeding().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToArray();
        var validators = scope.ServiceProvider.GetServices<ISeedValidator>().ToArray();
        await new SeedRunner(db, seeders, validators).RunAsync(2026, "foundation-test");
        db.ChangeTracker.Clear();
        var runner = new SeedRunner(db, seeders.Append(new UnavailableParent(seeders.Select(x => x.Id).ToArray(), deleted)), validators);
        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(2027, "foundation-test"));
        db.ChangeTracker.Clear();
        var unit = await db.Set<IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits.OrganizationUnit>().IgnoreQueryFilters().SingleAsync(x => x.Code == "FNV2-FIN");
        Assert.False(unit.IsDeleted);
        Assert.True(unit.IsActive);
        Assert.False(await db.Set<TaxReportPeriod>().AnyAsync(x => x.FromDate.Year == 2027));
    }
    private sealed class UnavailableParent(string[] dependencies, bool deleted) : IDataSeeder
    {
        public string Id => "ERPV2.Foundation.UnavailableParent";
        public IReadOnlyCollection<string> Dependencies => dependencies;
        public async Task SeedAsync(SeedContext c, CancellationToken ct)
        {
            var unit = await c.Rows<IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits.OrganizationUnit>().SingleAsync(x => x.Code == "FNV2-FIN", ct);
            unit.IsDeleted = deleted;
            unit.IsActive = deleted;
            await c.Db.SaveChangesAsync(ct);
        }
    }
    private sealed class CrossCompanyReference(string[] dependencies) : IDataSeeder
    {
        public string Id => "ERPV2.Foundation.TestCorruption";
        public IReadOnlyCollection<string> Dependencies => dependencies;
        public async Task SeedAsync(SeedContext c, CancellationToken ct)
        {
            var foreign = new IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits.OrganizationUnit { DataAreaId = "oth", Code = "FNV2-OTHER", Name = "Other company" };
            c.Db.Add(foreign);
            await c.Db.SaveChangesAsync(ct);
            var position = await c.Rows<IAX.IXApi.Modules.Finance.Foundation.Structure.HcmPosition>().SingleAsync(x => x.Code == "FNV2-BUYER", ct);
            position.OrganizationUnitId = foreign.RecId;
            await c.Db.SaveChangesAsync(ct);
        }
    }
    [Fact]
    public async Task Tax_and_charge_examples_use_business_services_and_are_repeatable()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        var actor = new TestIdentity("foundation-test");
        using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db)
            .AddSingleton<IAX.IXApi.Shared.Application.Identity.ICompanyExecutionContext>(actor)
            .AddSingleton<IAX.IXApi.Infrastructure.Identity.ICurrentUserService>(actor)
            .AddFoundationTransactionExamples().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var examples = scope.ServiceProvider.GetRequiredService<IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.FoundationTransactionExamples>();
        var result = await examples.RunAsync(new(2026, "foundation-test"));
        Assert.Equal("FoundationTransactions", result.Scope);
        Assert.NotNull(result.Preparation?.MasterData);
        Assert.False(result.Preparation!.TransactionsExecuted);
        Assert.Single(result.TaxJournalDetails);
        Assert.Equal(2, result.Scenarios.Count);
        Assert.All(result.Scenarios, x => Assert.True(x.PostedVouchers > 0));
        var taxes = await db.Set<TaxTrans>().CountAsync();
        var charges = await db.Set<IAX.IXApi.Modules.Finance.AccountsReceivable.MarkupTrans>().CountAsync();
        Assert.True(taxes > 0);
        Assert.True(charges > 0);
        var journalTaxes = await db.Set<TaxJournalTrans>().ToListAsync();
        var detail = Assert.Single(journalTaxes);
        Assert.Equal("dat", detail.DataAreaId);
        Assert.Equal(IAX.IXApi.Modules.Finance.Common.FinanceTableNumbers.LedgerJournalTrans, detail.TransTableId);
        var source = await db.Set<LedgerJournalTrans>().SingleAsync(x => x.RecId == detail.TransRecId && x.DataAreaId == detail.DataAreaId);
        var posted = await db.Set<TaxTrans>().SingleAsync(x => x.SourceRecId == source.RecId && x.SourceTableId == detail.TransTableId && x.TaxCode == detail.TaxCode);
        Assert.Equal(source.TaxGroup, detail.TaxGroup);
        Assert.Equal(source.TaxItemGroup, detail.TaxItemGroup);
        Assert.Equal(posted.SourceBaseAmountCur, detail.SourceBaseAmountCur);
        Assert.Equal(posted.SourceTaxAmountCur, detail.SourceTaxAmountCur);
        Assert.Equal(posted.TaxBaseAmount, detail.TaxBaseAmount);
        Assert.Equal(posted.TaxAmount, detail.TaxAmount);
        Assert.Equal(52.50m, detail.TaxAmount);
        var association = await db.Set<TaxTransGeneralJournalAccountEntry>().SingleAsync();
        Assert.Equal(association.RecId, Assert.Single(result.TaxAccountingAssociationIds));
        Assert.Equal(posted.RecId, association.TaxTrans);
        var accountEntry = await db.Set<GeneralJournalAccountEntry>().SingleAsync(x => x.RecId == association.GeneralJournalAccountEntry && x.DataAreaId == association.DataAreaId);
        var voucher = await db.Set<GeneralJournalEntry>().SingleAsync(x => x.RecId == accountEntry.GeneralJournalEntry && x.DataAreaId == association.DataAreaId);
        Assert.Equal(posted.Voucher, voucher.SubledgerVoucher);
        Assert.Equal(posted.TransDate, voucher.AccountingDate);
        Assert.Equal(accountEntry.LedgerDimension, association.LedgerDimension);
        Assert.Equal(accountEntry.AccountingCurrencyAmount, association.AccountingCurrencyAmount);
        Assert.Equal(accountEntry.TransactionCurrencyAmount, association.TransactionCurrencyAmount);
        Assert.Equal(accountEntry.TransactionCurrencyCode, association.TransactionCurrency);
        Assert.Null(association.TaxUncommitted);
        Assert.Null(association.SubledgerJournalAccountEntry);
        Assert.Null(association.TaxTransRelationship);
        Assert.Null(association.MonetaryAmountType);
        Assert.True(await db.Set<DimensionAttributeValueCombination>().AnyAsync(x => x.DataAreaId == detail.DataAreaId && x.RecId == detail.OperationLedgerDimension));
        db.ChangeTracker.Clear();
        await examples.RunAsync(new(2026, "foundation-test"));
        Assert.Equal(journalTaxes.Count, await db.Set<TaxJournalTrans>().CountAsync());
        Assert.Equal(detail.RecId, await db.Set<TaxJournalTrans>().Select(x => x.RecId).SingleAsync());
        Assert.Equal(taxes, await db.Set<TaxTrans>().CountAsync());
        Assert.Equal(charges, await db.Set<IAX.IXApi.Modules.Finance.AccountsReceivable.MarkupTrans>().CountAsync());
        Assert.Equal(association.RecId, await db.Set<TaxTransGeneralJournalAccountEntry>().Select(x => x.RecId).SingleAsync());
        var associations = scope.ServiceProvider.GetRequiredService<IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax.TaxTransGeneralJournalAccountEntrySeeder>();
        await Assert.ThrowsAsync<InvalidOperationException>(() => associations.SeedAsync([posted.RecId, long.MaxValue]));
        Assert.Equal(association.RecId, await db.Set<TaxTransGeneralJournalAccountEntry>().Select(x => x.RecId).SingleAsync());
        var conflicting = await db.Set<TaxTransGeneralJournalAccountEntry>().SingleAsync();
        conflicting.LedgerDimension = long.MaxValue;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => associations.SeedAsync([posted.RecId]));
        Assert.Equal(long.MaxValue, (await db.Set<TaxTransGeneralJournalAccountEntry>().SingleAsync()).LedgerDimension);
        var invalidSource = await db.Set<TaxTrans>().SingleAsync(x => x.RecId == posted.RecId);
        invalidSource.SourceRecId = long.MaxValue;
        await db.SaveChangesAsync();
        var sourceError = await Assert.ThrowsAsync<InvalidOperationException>(() => associations.SeedAsync([posted.RecId]));
        Assert.Contains("source vendor invoice or journal line", sourceError.Message);
        (await db.Set<TaxTrans>().SingleAsync(x => x.RecId == posted.RecId)).SourceRecId = posted.SourceRecId;
        (await db.Set<TaxTransGeneralJournalAccountEntry>().SingleAsync()).LedgerDimension = accountEntry.LedgerDimension;
        var dimension = await db.Set<DimensionAttributeValueCombination>().SingleAsync(x => x.RecId == accountEntry.LedgerDimension);
        db.Add(IAX.IXApi.Modules.Finance.Common.SubledgerPostingSupport.AccountEntry("dat", accountEntry.GeneralJournalEntry, dimension,
            IAX.IXApi.Modules.Finance.Common.LedgerPostingType.Tax, accountEntry.TransactionCurrencyAmount,
            accountEntry.AccountingCurrencyAmount, accountEntry.ReportingCurrencyAmount, "Ambiguous test fixture", 0m, accountEntry.TransactionCurrencyCode));
        await db.SaveChangesAsync();
        var ambiguous = await Assert.ThrowsAsync<InvalidOperationException>(() => associations.SeedAsync([posted.RecId]));
        Assert.Contains("2 matching tax ledger entries", ambiguous.Message);
        Assert.Equal(association.RecId, await db.Set<TaxTransGeneralJournalAccountEntry>().Select(x => x.RecId).SingleAsync());
    }
    [Fact]
    public void Foundation_manifest_rejects_coverage_drift()
    {
        var catalog = new IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Data.FoundationExampleCatalog();
        catalog.Validate();
        catalog.Tables.Masters[0] = "UnknownTable";
        Assert.Throws<InvalidOperationException>(() => catalog.Validate());
        var referenceCatalog = new IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Data.FoundationExampleCatalog();
        referenceCatalog.References[0] = referenceCatalog.References[0] with { Value = "WrongFixture" };
        Assert.Throws<InvalidOperationException>(() => referenceCatalog.Validate());
    }
    [Fact]
    public async Task Foundation_preparation_reports_persisted_references_and_preserves_them_on_rerun()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db).AddFoundationConfigurationAndMasterDataExamples().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var examples = scope.ServiceProvider.GetRequiredService<FoundationCycleSeeder>();
        var first = await examples.PrepareAsync(new(2026,"foundation-test"));
        Assert.False(first.TransactionsExecuted);
        Assert.NotNull(first.MasterData);
        Assert.All(await db.Set<TaxReportPeriod>().ToListAsync(), interval => Assert.Equal(0, interval.VersionNum));
        Assert.Equal(4, first.PreparationSteps.Count);
        Assert.Equal(68, first.MasterData.Tables.Count);
        Assert.Equal(first.CompletedSeeders, first.SeederSteps.Select(x => x.SeederId));
        Assert.Equal(Enumerable.Range(1, first.CompletedSeeders.Count), first.SeederSteps.Select(x => x.Order));
        Assert.Equal(13, first.MasterData.References.Count);
        Assert.All(first.MasterData.Tables, table => Assert.Equal(table.RowCount, table.RecordIds.Count));
        var currency = first.MasterData.References.Single(x => x.Entity == "Currency");
        Assert.Equal((await db.Set<Currency>().SingleAsync(x => x.CurrencyCode == "SAR")).RecId.ToString(), currency.RecordId);
        Assert.Empty(await db.Set<TaxJournalTrans>().ToListAsync());
        db.ChangeTracker.Clear();
        var second = await examples.PrepareAsync(new(2026,"foundation-test"));
        Assert.Equal(first.MasterData.References, second.MasterData!.References);
    }
}
