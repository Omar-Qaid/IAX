using System.Reflection;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip.Dtos;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Return;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed partial class InventoryCycleTests
{
    [Fact]
    public async Task The_pricing_date_follows_PriceDateType_so_a_delivery_dated_agreement_can_be_selected()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        // Valid only around the 2026-10-28 delivery date, never "today".
        var delivery = PurchaseAgreement(1, 33m);
        delivery.FromDate = new DateTime(2026, 10, 20);
        delivery.ToDate = new DateTime(2026, 11, 1);
        db.Set<PriceDiscTable>().Add(delivery);
        db.Set<PurchParameters>().Add(new PurchParameters
        {
            RecId = 7, DataAreaId = "dat", PriceDateType = PriceDateType.DeliveryDate
        });
        db.Set<InventTableModule>().Local.Single(x => x.ModuleType == ModuleInventPurchSales.Purchase).Price = 45m;
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 0, 1, null, null, new DateTime(2026, 10, 28), null,
                UsePriceAgreement: true), default)).StatusCode);
        Assert.Equal(33m, (await db.Set<PurchLine>().SingleAsync()).PurchPrice);
    }

    [Fact]
    public async Task With_PriceDateType_Today_an_agreement_that_expired_before_today_is_not_selected()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        var expiredBeforeToday = PurchaseAgreement(1, 33m);
        expiredBeforeToday.FromDate = new DateTime(2026, 10, 20);
        expiredBeforeToday.ToDate = new DateTime(2026, 11, 1);
        db.Set<PriceDiscTable>().Add(expiredBeforeToday);
        // Today is the default, and no parameters row means the default.
        db.Set<InventTableModule>().Local.Single(x => x.ModuleType == ModuleInventPurchSales.Purchase).Price = 45m;
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 0, 1, null, null, new DateTime(2026, 10, 28), null,
                UsePriceAgreement: true), default)).StatusCode);
        // Falls back to the item's purchase price rather than using the delivery-dated agreement.
        Assert.Equal(45m, (await db.Set<PurchLine>().SingleAsync()).PurchPrice);
    }

    [Fact]
    public async Task A_purchase_line_takes_its_price_verbatim_unless_it_opts_into_an_agreement()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        db.Set<PriceDiscTable>().Add(PurchaseAgreement(1, 40m));
        await db.SaveChangesAsync();

        // The flag is opt-in, so every existing caller keeps sending its own price.
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null), default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal(50m, line.PurchPrice);
        Assert.Equal(5000m, line.LineAmount);
    }

    [Fact]
    public async Task A_purchase_line_resolves_the_vendor_item_agreement_and_prices_the_line_from_it()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        db.Set<PriceDiscTable>().Add(PurchaseAgreement(1, 40m));
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        // The sent price of 50 is discarded for the agreed 40.
        Assert.Equal(40m, line.PurchPrice);
        Assert.Equal(4000m, line.LineAmount);
    }

    [Fact]
    public async Task The_most_specific_purchase_agreement_wins_over_the_all_vendor_rule()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        db.Set<PriceDiscTable>().AddRange(
            PurchaseAgreement(1, 48m, PriceDiscPartyCodeType.All, "", PriceDiscProductCodeType.All, ""),
            PurchaseAgreement(2, 44m, itemCode: PriceDiscProductCodeType.All, itemRelation: ""),
            PurchaseAgreement(3, 40m));
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 0, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        Assert.Equal(40m, (await db.Set<PurchLine>().SingleAsync()).PurchPrice);
    }

    [Fact]
    public async Task A_purchase_agreement_quantity_break_applies_only_from_its_own_quantity()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        db.Set<PriceDiscTable>().AddRange(
            PurchaseAgreement(1, 40m),
            PurchaseAgreement(2, 35m, quantityFrom: 100m));
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 99, "PCS", 0, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 0, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        // SQLite cannot ORDER BY a decimal column, so LineNumber is ordered client-side.
        var lines = (await db.Set<PurchLine>().ToListAsync()).OrderBy(x => x.LineNumber).ToList();
        Assert.Equal(40m, lines[0].PurchPrice);
        Assert.Equal(35m, lines[1].PurchPrice);
    }

    [Fact]
    public async Task An_expired_or_foreign_currency_purchase_agreement_is_not_applied()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        var expired = PurchaseAgreement(1, 40m);
        expired.ToDate = new DateTime(2026, 1, 31);
        var foreign = PurchaseAgreement(2, 30m);
        foreign.Currency = "USD";
        db.Set<PriceDiscTable>().AddRange(expired, foreign);
        db.Set<InventTableModule>().Local.Single(x => x.ModuleType == ModuleInventPurchSales.Purchase).Price = 45m;
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        // No agreement applies, so the item's own purchase price is used, as on the sales side.
        Assert.Equal(45m, (await db.Set<PurchLine>().SingleAsync()).PurchPrice);
    }

    [Fact]
    public async Task A_dimension_specific_purchase_agreement_is_applied_after_the_dimension_resolves()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        // The supply service assigns DIM-000001; a dimension-specific rule cannot match before that.
        db.Set<PriceDiscTable>().AddRange(
            PurchaseAgreement(1, 40m),
            PurchaseAgreement(2, 36m, inventDimId: "DIM-000001"));
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 0, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        var line = await db.Set<PurchLine>().SingleAsync();
        Assert.Equal("DIM-000001", line.InventDimId);
        Assert.Equal(36m, line.PurchPrice);
        Assert.Equal(3600m, line.LineAmount);
    }

    [Fact]
    public async Task A_purchase_agreement_for_another_vendor_or_the_sales_axis_is_not_applied()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        var otherVendor = PurchaseAgreement(1, 20m, accountRelation: "V-0002");
        var salesAxis = PurchaseAgreement(2, 21m);
        salesAxis.Module = ModuleInventCustVend.Cust;
        salesAxis.Relation = PriceType.PriceSales;
        db.Set<PriceDiscTable>().AddRange(otherVendor, salesAxis);
        db.Set<InventTableModule>().Local.Single(x => x.ModuleType == ModuleInventPurchSales.Purchase).Price = 45m;
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 50, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        Assert.Equal(45m, (await db.Set<PurchLine>().SingleAsync()).PurchPrice);
    }

    [Fact]
    public async Task A_vendor_price_group_agreement_applies_to_a_vendor_in_that_group()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (orders, po) = await OpenPurchaseOrderAsync(db);
        db.Set<VendTable>().Local.Single().PriceGroup = "WHOLESALE";
        db.Set<PriceDiscTable>().AddRange(
            PurchaseAgreement(1, 48m, PriceDiscPartyCodeType.All, "", PriceDiscProductCodeType.All, ""),
            PurchaseAgreement(2, 42m, PriceDiscPartyCodeType.GroupId, "WHOLESALE"));
        await db.SaveChangesAsync();

        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 0, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        // The group rule is more specific than the all-vendor rule, as on the sales side.
        Assert.Equal(42m, (await db.Set<PurchLine>().SingleAsync()).PurchPrice);
    }

    [Fact]
    public async Task A_purchase_agreement_journal_publishes_a_vendor_rule_the_purchase_line_then_resolves()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (journal, header) = await PurchaseAgreementJournalAsync(db, _ => { });

        var posted = await journal.PostAsync(header.RecId, default);
        Assert.Equal(PriceDiscJournalPostStatus.Posted, posted.Status);
        var rule = await db.Set<PriceDiscTable>().SingleAsync();
        Assert.Equal(ModuleInventCustVend.Vend, rule.Module);
        Assert.Equal(PriceType.PricePurch, rule.Relation);
        Assert.Equal(40m, rule.Amount);

        // The published rule is what the purchase line resolves, closing the loop.
        db.ChangeTracker.Clear();
        var (orders, _, _, _) = PurchaseServices(db);
        Assert.Equal(200, (await orders.CreateAsync(new PurchOrderCreateDto("V-0001", null, "SAR",
            new DateTime(2026, 10, 28), "S01", "WH-01", null, new DateTime(2026, 10, 20)), default)).StatusCode);
        var po = await db.Set<PurchTable>().SingleAsync();
        Assert.Equal(200, (await orders.AddLineAsync(po.RecId,
            new PurchLineInputDto("ITEM-001", 100, "PCS", 0, 1, null, null, null, null, UsePriceAgreement: true),
            default)).StatusCode);
        Assert.Equal(40m, (await db.Set<PurchLine>().SingleAsync()).PurchPrice);
    }

    [Fact]
    public async Task A_purchase_agreement_journal_refuses_the_customer_module_and_an_unknown_vendor()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (journal, header) = await PurchaseAgreementJournalAsync(db, line => line.Module = ModuleInventCustVend.Cust);
        var wrongModule = await journal.PostAsync(header.RecId, default);
        Assert.Equal(PriceDiscJournalPostStatus.Invalid, wrongModule.Status);
        Assert.Contains("Line 1: purchase agreements must use the vendor module.", wrongModule.Errors!);
        Assert.Empty(await db.Set<PriceDiscTable>().ToListAsync());

        await using var second = await Fixture.CreateAsync();
        var (other, otherHeader) = await PurchaseAgreementJournalAsync(second.Db, line => line.AccountRelation = "V-0404");
        var unknown = await other.PostAsync(otherHeader.RecId, default);
        Assert.Equal(PriceDiscJournalPostStatus.Invalid, unknown.Status);
        Assert.Contains("Line 1: vendor 'V-0404' was not found.", unknown.Errors!);
    }

    [Fact]
    public async Task A_sales_agreement_journal_still_refuses_the_vendor_module()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (journal, header) = await PurchaseAgreementJournalAsync(db, _ => { });
        // Flip the journal to the sales axis while its line stays on the vendor module.
        (await db.Set<PriceDiscAdmTable>().SingleAsync()).DefaultRelation = PriceType.LineDiscSales;
        var trans = await db.Set<PriceDiscAdmTrans>().SingleAsync();
        trans.Relation = PriceType.PriceSales;
        trans.Amount = 5m;
        await db.SaveChangesAsync();

        var result = await journal.PostAsync(header.RecId, default);
        Assert.Equal(PriceDiscJournalPostStatus.Invalid, result.Status);
        Assert.Contains("Line 1: sales agreements must use the customer module.", result.Errors!);
    }
}
