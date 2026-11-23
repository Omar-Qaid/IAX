using System.Reflection;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Common;
using System.Text.Json;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus;
using IAX.IXApi.Modules.Finance.Inventory.InventoryStatus.Entities;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle;
using IAX.IXApi.Modules.Finance.Inventory.ProductLifecycle.Entities;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Identity;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;
using ReleasedItem = IAX.IXApi.Modules.Finance.Entities.InventTable;
using SharedProduct = IAX.IXApi.Modules.Finance.Inventory.InventTable.EcoResProduct;

namespace IAX.IXApi.Tests;

public sealed partial class InventoryArchitectureTests
{
    [Theory]
    [InlineData((int)LedgerPostingModule.Ledger)]
    [InlineData((int)LedgerPostingModule.Tax)]
    [InlineData((int)LedgerPostingModule.Bank)]
    [InlineData((int)LedgerPostingModule.Customer)]
    [InlineData((int)LedgerPostingModule.Vendor)]
    [InlineData((int)LedgerPostingModule.Inventory)]
    public async Task Undefined_period_module_access_never_authorizes_posting(int module)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var date = new DateTime(2026, 1, 15);
        var invalid = (LedgerPeriodAccessLevel)99;
        db.AddRange(new Ledger { RecId = 1, DataAreaId = "dat", FiscalCalendar = 2, AccountingCurrency = "SAR" },
            new FiscalCalendarPeriod { RecId = 3, DataAreaId = "dat", FiscalCalendar = 2,
                Type = FiscalPeriodType.Operating, StartDate = date.AddDays(-1), EndDate = date.AddDays(1) },
            new LedgerFiscalCalendarPeriod { RecId = 4, DataAreaId = "dat", Ledger = 1,
                FiscalCalendarPeriod = 3, Status = FiscalPeriodStatus.Open },
            new LedgerPeriodModuleAccessControl { RecId = 5, DataAreaId = "dat", LedgerFiscalCalendarPeriod = 4,
                LedgerAccessLevel = invalid, TaxAccessLevel = invalid, BankAccessLevel = invalid,
                CustAccessLevel = invalid, VendAccessLevel = invalid, InventAccessLevel = invalid });
        await db.SaveChangesAsync();

        var error = await Assert.ThrowsAsync<SubledgerPostingException>(() =>
            new SubledgerPostingSupport(db).LoadLedgerContextAsync("dat", date, (LedgerPostingModule)module, "test", default));
        Assert.Contains("invalid", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Shared_posting_refuses_multiple_ledgers_for_the_same_company()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var date = new DateTime(2026, 1, 15);
        db.AddRange(new Ledger { RecId = 1, DataAreaId = "dat", FiscalCalendar = 2, AccountingCurrency = "SAR" },
            new Ledger { RecId = 2, DataAreaId = "dat", FiscalCalendar = 2, AccountingCurrency = "SAR" },
            new FiscalCalendarPeriod { RecId = 3, DataAreaId = "dat", FiscalCalendar = 2,
                Type = FiscalPeriodType.Operating, StartDate = date.AddDays(-1), EndDate = date.AddDays(1) },
            new LedgerFiscalCalendarPeriod { RecId = 4, DataAreaId = "dat", Ledger = 1,
                FiscalCalendarPeriod = 3, Status = FiscalPeriodStatus.Open });
        await db.SaveChangesAsync();

        var error = await Assert.ThrowsAsync<SubledgerPostingException>(() =>
            new SubledgerPostingSupport(db).LoadLedgerContextAsync("dat", date, LedgerPostingModule.Inventory, "test", default));
        Assert.Contains("exactly one ledger", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Shared_posting_refuses_ambiguous_fiscal_period_configuration(bool overlappingPeriods)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var date = new DateTime(2026, 1, 15);
        db.AddRange(new Ledger { RecId = 1, DataAreaId = "dat", FiscalCalendar = 2, AccountingCurrency = "SAR" },
            new FiscalCalendarPeriod { RecId = 3, DataAreaId = "dat", FiscalCalendar = 2,
                Type = FiscalPeriodType.Operating, StartDate = date.AddDays(-1), EndDate = date.AddDays(1) },
            new LedgerFiscalCalendarPeriod { RecId = 4, DataAreaId = "dat", Ledger = 1,
                FiscalCalendarPeriod = 3, Status = FiscalPeriodStatus.Open });
        if (overlappingPeriods)
            db.Add(new FiscalCalendarPeriod { RecId = 5, DataAreaId = "dat", FiscalCalendar = 2,
                Type = FiscalPeriodType.Operating, StartDate = date, EndDate = date });
        else
            db.Add(new LedgerFiscalCalendarPeriod { RecId = 6, DataAreaId = "dat", Ledger = 1,
                FiscalCalendarPeriod = 3, Status = FiscalPeriodStatus.Closed });
        await db.SaveChangesAsync();
        var error = await Assert.ThrowsAsync<SubledgerPostingException>(() =>
            new SubledgerPostingSupport(db).LoadLedgerContextAsync("dat", date, LedgerPostingModule.Inventory, "test", default));
        Assert.Contains("ambiguous", error.Message);
    }

    [Fact]
    public async Task Provisional_inventory_cost_uses_financial_site_pool_across_warehouses()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AddRange(
            new InventSum { RecId = 1, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "A",
                InventSiteId = "SITE", InventLocationId = "WH-A", PostedQty = 2, PostedValue = 20,
                PhysicalInvent = 3, PhysicalValue = 100 },
            new InventSum { RecId = 2, DataAreaId = "dat", ItemId = "ITEM", InventDimId = "B",
                InventSiteId = "SITE", InventLocationId = "WH-B", PostedQty = 3, PostedValue = 60,
                PhysicalInvent = 3 });
        await db.SaveChangesAsync();

        Assert.Equal(16m, await InventoryMovement.SiteRunningAverageCostAsync(db, "dat", "ITEM", "SITE", default));
    }

    [Fact]
    public async Task Inventory_period_access_is_independent_of_ledger_access()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new InventoryContext(new DbContextOptionsBuilder<InventoryContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var date = DateTime.Today;
        db.AddRange(
            new Ledger { RecId = 1, DataAreaId = "dat", FiscalCalendar = 2, AccountingCurrency = "SAR" },
            new FiscalCalendarPeriod { RecId = 3, DataAreaId = "dat", FiscalCalendar = 2,
                StartDate = date.AddDays(-1), EndDate = date.AddDays(1), Type = FiscalPeriodType.Operating },
            new LedgerFiscalCalendarPeriod { RecId = 4, DataAreaId = "dat", Ledger = 1,
                FiscalCalendarPeriod = 3, Status = FiscalPeriodStatus.Open },
            new LedgerPeriodModuleAccessControl { RecId = 5, DataAreaId = "dat",
                LedgerFiscalCalendarPeriod = 4, LedgerAccessLevel = LedgerPeriodAccessLevel.All,
                InventAccessLevel = LedgerPeriodAccessLevel.None });
        await db.SaveChangesAsync();
        var support = new SubledgerPostingSupport(db);

        await support.LoadLedgerContextAsync("dat", date, LedgerPostingModule.Ledger, "test", default);
        var error = await Assert.ThrowsAsync<SubledgerPostingException>(() =>
            support.LoadLedgerContextAsync("dat", date, LedgerPostingModule.Inventory, "test", default));
        Assert.Contains("Inventory module is closed", error.Message);
    }
}
