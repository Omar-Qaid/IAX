using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>
/// One test per documented Foundation scenario (CycleErpExamples/Foundation/Scenarios/01-06), asserting the
/// scenario's expected results against persisted data. Scenario 06 (conflict, cross-company and rerun rollback)
/// is covered by <see cref="FoundationSeedTests"/>.
/// </summary>
[Collection(SeedingCollection.Name)]
public sealed class FoundationScenarioTests
{
    private const string Company = TestIdentity.DefaultCompany;

    [Fact]
    public async Task Scenario01_preparation_captures_every_master_table_and_posts_nothing()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        var prepared = await PrepareAsync(db);

        Assert.False(prepared.TransactionsExecuted);
        Assert.Equal(68, prepared.MasterData!.Tables.Count);
        Assert.All(prepared.MasterData.Tables, table => Assert.True(table.RowCount > 0, table.Entity));
        // Configuration and masters only: every transaction table stays empty.
        Assert.False(await db.Set<GeneralJournalEntry>().AnyAsync());
        Assert.False(await db.Set<GeneralJournalAccountEntry>().AnyAsync());
        Assert.False(await db.Set<TaxTrans>().AnyAsync());
        Assert.False(await db.Set<TaxJournalTrans>().AnyAsync());
        Assert.False(await db.Set<MarkupTrans>().AnyAsync());
        Assert.False(await db.Set<BankAccountTrans>().AnyAsync());
        Assert.False(await db.Set<InventTrans>().AnyAsync());
        Assert.False(await db.Set<VendTrans>().AnyAsync());
        Assert.False(await db.Set<CustTrans>().AnyAsync());
    }

    [Fact]
    public async Task Scenario02_worker_position_role_unit_and_managers_resolve_in_the_company_and_survive_rerun()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        await PrepareAsync(db);

        var unit = await db.Set<OrganizationUnit>().SingleAsync(x => x.Code == "FNV2-FIN");
        var role = await db.Set<OrganizationRole>().SingleAsync(x => x.Code == "FNV2-BUYER");
        var buyerPosition = await db.Set<HcmPosition>().SingleAsync(x => x.Code == "FNV2-BUYER");
        var managerPosition = await db.Set<HcmPosition>().SingleAsync(x => x.Code == "FNV2-MANAGER");
        var buyer = await db.Set<HcmWorker>().SingleAsync(x => x.PersonnelNumber == "FNV2-BUYER");
        var manager = await db.Set<HcmWorker>().SingleAsync(x => x.PersonnelNumber == "FNV2-MANAGER");
        var showroom = await db.Set<HcmShowroom>().SingleAsync(x => x.PersonnelNumber == "FNV2-SHOWROOM");

        Assert.Equal((unit.RecId, role.RecId), (buyerPosition.OrganizationUnitId, buyerPosition.RoleId));
        var managerRole = await db.Set<OrganizationRole>().SingleAsync(x => x.Code == "FNV2-MANAGER");
        Assert.Equal((unit.RecId, managerRole.RecId), (managerPosition.OrganizationUnitId, managerPosition.RoleId));
        var assignment = await db.Set<HcmWorkerOrganizationAssignment>().SingleAsync(x => x.HcmWorkerId == buyer.RecId);
        Assert.Equal((buyerPosition.RecId, unit.RecId, role.RecId), (assignment.PositionId, assignment.OrganizationUnitId, assignment.OrganizationRoleId));
        var line = await db.Set<HcmWorkerOrganizationAssignmentV1>().SingleAsync(x => x.HcmWorkerId == buyer.RecId);
        Assert.Equal(manager.RecId, line.HcmManagerWorkerId);
        var hierarchy = await db.Set<HcmReportingHierarchy>().SingleAsync(x => x.Code == "FNV2");
        var reporting = await db.Set<HcmPositionReportingLine>().SingleAsync(x => x.ReportingHierarchyId == hierarchy.RecId && x.SubordinatePositionId == buyerPosition.RecId);
        Assert.Equal(managerPosition.RecId, reporting.ManagerPositionId);
        Assert.True(await db.Set<HcmWorkerShowroomAssignment>().AnyAsync(x => x.HcmWorkerId == buyer.RecId && x.HcmShowroomId == showroom.RecId));
        var organization = await db.Set<OrganizationHierarchy>().SingleAsync(x => x.Code == "FNV2");
        Assert.True(await db.Set<OrganizationHierarchyNode>().AnyAsync(x => x.HierarchyId == organization.RecId && x.OrganizationUnitId == unit.RecId));
        Assert.All(new[] { unit.DataAreaId, buyerPosition.DataAreaId, buyer.DataAreaId, manager.DataAreaId, showroom.DataAreaId, assignment.DataAreaId, line.DataAreaId },
            area => Assert.Equal(Company, area));

        // Effective-dated fixtures keep their keys and dates on rerun.
        var before = (assignment.RecId, assignment.ValidFrom, line.RecId, line.ValidFrom, reporting.RecId, reporting.ValidFrom);
        db.ChangeTracker.Clear();
        await PrepareAsync(db);
        var assignmentAfter = await db.Set<HcmWorkerOrganizationAssignment>().SingleAsync(x => x.HcmWorkerId == buyer.RecId);
        var lineAfter = await db.Set<HcmWorkerOrganizationAssignmentV1>().SingleAsync(x => x.HcmWorkerId == buyer.RecId);
        var reportingAfter = await db.Set<HcmPositionReportingLine>().SingleAsync(x => x.SubordinatePositionId == buyerPosition.RecId);
        Assert.Equal(before, (assignmentAfter.RecId, assignmentAfter.ValidFrom, lineAfter.RecId, lineAfter.ValidFrom, reportingAfter.RecId, reportingAfter.ValidFrom));
    }

    [Fact]
    public async Task Scenario03_default_and_ledger_dimensions_link_to_the_intended_company_records()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        await PrepareAsync(db);

        var unit = await db.Set<OrganizationUnit>().SingleAsync(x => x.Code == "FNV2-FIN");
        var attribute = await db.Set<DimensionAttribute>().SingleAsync(x => x.Name == "FNV2Department");
        var value = await db.Set<DimensionAttributeValue>().SingleAsync(x => x.DimensionAttribute == attribute.RecId && x.DisplayValue == "FNV2-FIN");
        Assert.Equal((unit.RecId, Company), (value.EntityInstance, value.BackingRecordDataAreaId));
        var set = await db.Set<DimensionAttributeSet>().SingleAsync(x => x.BaseEnumTypeName == "FNV2");
        Assert.True(await db.Set<DimensionAttributeSetItem>().AnyAsync(x => x.DimensionAttributeSet == set.RecId && x.DimensionAttribute == attribute.RecId));

        // Default dimension: the customer's value set holds the department value.
        var values = await db.Set<DimensionAttributeValueSet>().SingleAsync(x => x.DepartmentValue == "FNV2-FIN");
        Assert.Equal(value.RecId, values.Department);
        Assert.True(await db.Set<DimensionAttributeValueSetItem>().AnyAsync(x => x.DimensionAttributeValueSet == values.RecId && x.DimensionAttributeValue == value.RecId));
        var customer = await db.Set<CustTable>().SingleAsync(x => x.AccountNum == $"FNV2-{Company}-CUSTOMER");
        Assert.Equal(values.RecId, customer.DefaultDimension);

        // Ledger dimension: an active account structure and a combination for main account 681099 under it.
        var structure = await db.Set<DimensionHierarchy>().SingleAsync(x => x.Name == "FNV2");
        Assert.False(structure.IsDeleted);
        Assert.Equal(NoYes.No, structure.IsDraft);
        var account = await db.Set<MainAccount>().SingleAsync(x => x.MainAccountId == "681099");
        var combination = await db.Set<DimensionAttributeValueCombination>()
            .SingleAsync(x => x.DataAreaId == Company && x.MainAccount == account.RecId && x.AccountStructure == structure.RecId);
        Assert.Equal("681099", combination.MainAccountValue);
    }

    [Fact]
    public async Task Scenario04_installments_total_100_percent_and_customer_defaults_resolve()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        await PrepareAsync(db);

        var schedule = await db.Set<PaymSched>().SingleAsync(x => x.Name == "FNV2");
        var lines = await db.Set<PaymSchedLine>().Where(x => x.Name == "FNV2").ToListAsync();
        Assert.Equal(2, schedule.NumOfPayment);
        Assert.Equal(2, lines.Count);
        Assert.Equal(100m, lines.Sum(x => x.Value));

        var customer = await db.Set<CustTable>().SingleAsync(x => x.AccountNum == $"FNV2-{Company}-CUSTOMER");
        var term = await db.Set<PaymTerm>().SingleAsync(x => x.PaymTermId == customer.PaymTermId);
        Assert.Equal(schedule.Name, term.PaymSched);
        Assert.True(await db.Set<DlvMode>().AnyAsync(x => x.Code == customer.DlvModeId));
        Assert.True(await db.Set<DlvTerm>().AnyAsync(x => x.Code == "FNV2"));
        Assert.True(await db.Set<DimensionAttributeValueSet>().AnyAsync(x => x.RecId == customer.DefaultDimension));
    }

    [Fact]
    public async Task Scenario05_tax_journal_and_charge_post_the_documented_amounts_and_rerun_adds_nothing()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var db = database.Context;
        var actor = new TestIdentity("foundation-scenario-test");
        using var provider = new ServiceCollection().AddSingleton<ApplicationDbContext>(db)
            .AddSingleton<ICompanyExecutionContext>(actor).AddSingleton<ICurrentUserService>(actor)
            .AddFoundationTransactionExamples().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var examples = scope.ServiceProvider.GetRequiredService<FoundationTransactionExamples>();
        await examples.RunAsync(new(2026, "foundation-scenario-test"));

        // Journal: expense 350 + recoverable tax 52.50 (15%), paid 402.50.
        // Purchase: 100 x 25 = 2,500 inventory + 50 delivery charge, tax 15% of 2,550 = 382.50, paid 2,932.50.
        var journalTax = Assert.Single(await db.Set<TaxJournalTrans>().ToListAsync());
        Assert.Equal(52.50m, journalTax.TaxAmount);
        Assert.Equal(52.50m + 382.50m, (await db.Set<TaxTrans>().ToListAsync()).Sum(x => x.TaxAmount));
        // The journal line is the vendor side; the taxed expense is its offset, which the tax detail records.
        var expense = await db.Set<DimensionAttributeValueCombination>().SingleAsync(x => x.RecId == journalTax.OperationLedgerDimension);
        Assert.Equal(350m, await Ledger(expense.MainAccountValue));
        Assert.Equal(2500m, await Ledger(GeneralLedgerSeedExample.Inventory));
        Assert.Equal(50m, await Ledger(GeneralLedgerSeedExample.DeliveryExpense));
        Assert.Equal(435m, await Ledger(GeneralLedgerSeedExample.RecoverablePurchaseTax));
        Assert.Equal(-(402.50m + 2932.50m), await Ledger(GeneralLedgerSeedExample.Bank));
        Assert.Equal(-(402.50m + 2932.50m), (await db.Set<BankAccountTrans>().ToListAsync()).Sum(x => x.AmountCur));
        Assert.Equal(0m, await Ledger(GeneralLedgerSeedExample.VendorControl)); // both obligations paid
        var stock = await db.Set<InventSum>().Where(x => x.ItemId == "APV2-STOCK").ToListAsync();
        Assert.Equal((100m, 2500m), (stock.Sum(x => x.PhysicalInvent), stock.Sum(x => x.PostedValue)));
        await LedgerBalances.AssertEveryVoucherBalancesAsync(db, Company);

        // Rerun keeps the tax detail and adds no postings.
        var counts = (await db.Set<GeneralJournalAccountEntry>().CountAsync(), await db.Set<TaxTrans>().CountAsync(), await db.Set<MarkupTrans>().CountAsync());
        db.ChangeTracker.Clear();
        await examples.RunAsync(new(2026, "foundation-scenario-test"));
        Assert.Equal(counts, (await db.Set<GeneralJournalAccountEntry>().CountAsync(), await db.Set<TaxTrans>().CountAsync(), await db.Set<MarkupTrans>().CountAsync()));
        Assert.Equal(journalTax.RecId, (await db.Set<TaxJournalTrans>().SingleAsync()).RecId);

        Task<decimal> Ledger(string mainAccount) => LedgerBalances.OfMainAccountAsync(db, Company, mainAccount);
    }

    private static async Task<IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Reports.FoundationPreparationReport> PrepareAsync(ApplicationDbContext db)
    {
        using var provider = new ServiceCollection().AddSingleton(db).AddFoundationConfigurationAndMasterDataExamples().BuildServiceProvider();
        using var scope = provider.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<FoundationCycleSeeder>().PrepareAsync(new(2026, "foundation-scenario-test"));
    }
}
