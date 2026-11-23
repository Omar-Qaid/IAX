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
    public async Task A_net_unit_price_within_the_configured_percentage_posts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay,
            MatchingPolicy.None, null, x =>
            {
                x.UseExtendedPriceMatching = NoYes.Yes;
                x.ExtendedPriceTolerancePercentage = 5m;
                x.UseTotalPriceMatching = NoYes.No;
            });

        // A unit price of 51 against 50 is a 2% variance, inside the 5% tolerance.
        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 51)]), default)).StatusCode);
        Assert.Equal(5100m, (await db.Set<VendInvoiceJour>().SingleAsync()).InvoiceAmount);
    }

    [Fact]
    public async Task A_net_unit_price_outside_the_configured_percentage_is_refused_with_the_variance()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay,
            MatchingPolicy.None, null, x =>
            {
                x.UseExtendedPriceMatching = NoYes.Yes;
                x.ExtendedPriceTolerancePercentage = 5m;
                x.UseTotalPriceMatching = NoYes.No;
            });

        // 54 against 50 is 8%, outside 5%.
        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Equal("Invoice net unit price for item ITEM-001 differs from the purchase order by 8%, "
            + "which exceeds the configured net unit price tolerance of 5%.", result.Message);
        Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());
    }

    /// <summary>
    /// The decision this pins: D365 holds both a percentage and an absolute amount, and <b>any</b>
    /// configured tolerance that is exceeded fails the match. A generous percentage therefore does
    /// not let a line through that the amount tolerance refuses, so configuring a second tolerance
    /// cannot widen the effective one.
    /// </summary>
    [Fact]
    public async Task The_net_unit_price_amount_tolerance_is_enforced_alongside_the_percentage()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay,
            MatchingPolicy.None, null, x =>
            {
                x.UseExtendedPriceMatching = NoYes.Yes;
                x.ExtendedPriceTolerancePercentage = 50m;   // 8% would pass this on its own
                x.ExtendedPriceToleranceAmount = 2m;        // but 4 per unit breaks this
                x.UseTotalPriceMatching = NoYes.No;
            });

        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Equal("Invoice net unit price for item ITEM-001 differs from the purchase order by 4, "
            + "which exceeds the configured net unit price tolerance of 2.", result.Message);
        Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());

        // Inside both tolerances the same line posts, so the refusal above is the amount and not
        // the switch simply refusing everything.
        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-2", new DateTime(2026, 10, 29), [new(line.RecId, 100, 51)]), default)).StatusCode);
    }

    [Fact]
    public async Task Net_unit_price_matching_with_no_tolerance_configured_requires_an_exact_unit_price()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay,
            MatchingPolicy.None, null, x =>
            {
                x.UseExtendedPriceMatching = NoYes.Yes;
                x.UseTotalPriceMatching = NoYes.No;
            });

        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 51)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Equal("Invoice net unit price for item ITEM-001 is 51 against the ordered 50, "
            + "and no net unit price tolerance is configured.", result.Message);
        Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());
    }

    [Fact]
    public async Task Without_a_configured_tolerance_an_invoice_price_difference_is_still_refused_by_name()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay, MatchingPolicy.None, null);

        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 51)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Equal("Invoice price for item ITEM-001 is 51 against the ordered 50 under the configured "
            + "matching policy, and no price match tolerance is configured.", result.Message);
        Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());
    }

    [Fact]
    public async Task An_invoice_price_within_the_configured_tolerance_posts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay, MatchingPolicy.None, 5m);

        // 5100 invoiced against 5000 ordered is a 2% variance, inside the 5% tolerance.
        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 51)]), default)).StatusCode);
        Assert.Equal(5100m, (await db.Set<VendInvoiceJour>().SingleAsync()).InvoiceAmount);
        // The invoiced price is what reaches inventory, not the ordered price.
        Assert.Equal(5100m, (await db.Set<InventSum>().SingleAsync()).PostedValue);
    }

    [Fact]
    public async Task An_invoice_price_outside_the_configured_tolerance_is_refused_with_the_variance()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay, MatchingPolicy.None, 5m);

        // 5400 against 5000 is 8%, outside 5%.
        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Equal("Invoice price for item ITEM-001 differs from the purchase order by 8%, "
            + "which exceeds the configured match tolerance of 5%.", result.Message);
        Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());
    }

    [Fact]
    public async Task The_tolerance_is_measured_both_ways_so_a_cheaper_invoice_is_matched_the_same()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay, MatchingPolicy.None, 5m);

        // Under-charging by 2% is a discrepancy of the same size and is accepted alike.
        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 49)]), default)).StatusCode);
        Assert.Equal(4900m, (await db.Set<VendInvoiceJour>().SingleAsync()).InvoiceAmount);
    }

    [Fact]
    public async Task The_vendor_matching_policy_is_honored_where_it_was_previously_stored_and_ignored()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        // Item None, vendor three-way: before this change only the item was read, so the price
        // difference posted unmatched.
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.None, MatchingPolicy.ThreeWay, 1m);

        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Contains("differs from the purchase order by 8%", result.Message);
    }

    [Fact]
    public async Task With_no_matching_policy_on_either_side_the_invoice_price_is_taken_as_billed()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.None, MatchingPolicy.None, null);

        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default)).StatusCode);
        Assert.Equal(5400m, (await db.Set<VendInvoiceJour>().SingleAsync()).InvoiceAmount);
    }

    [Fact]
    public async Task UseInvoiceMatching_No_switches_matching_off_entirely()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay,
            MatchingPolicy.ThreeWay, null, x => x.UseInvoiceMatching = NoYes.No);

        // The master switch wins over both policies.
        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default)).StatusCode);
        Assert.Equal(5400m, (await db.Set<VendInvoiceJour>().SingleAsync()).InvoiceAmount);
    }

    [Fact]
    public async Task UseTotalPriceMatching_No_keeps_matching_on_but_stops_the_price_comparison()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay,
            MatchingPolicy.None, null, x => x.UseTotalPriceMatching = NoYes.No);

        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default)).StatusCode);
        // The quantity side of the three-way policy still applies: more than received is refused.
        Assert.Equal(422, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-2", new DateTime(2026, 10, 29), [new(line.RecId, 1, 54)]), default)).StatusCode);
    }

    [Fact]
    public async Task The_company_matching_policy_applies_when_neither_the_vendor_nor_the_item_sets_one()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.None,
            MatchingPolicy.None, null, x => x.MatchingPolicy = MatchingPolicy.ThreeWay);

        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Contains("under the configured matching policy", result.Message);
    }

    [Fact]
    public async Task Forbidding_override_makes_the_company_policy_the_only_one_that_counts()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        // Company None, item three-way, override forbidden: the item policy is ignored.
        var (invoices, po, line) = await ReceivedOrderForMatchingAsync(db, MatchingPolicy.ThreeWay,
            MatchingPolicy.None, null, x =>
            {
                x.MatchingPolicy = MatchingPolicy.None;
                x.MatchingPolicyAllowOverride = NoYes.No;
            });

        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100, 54)]), default)).StatusCode);
        Assert.Equal(5400m, (await db.Set<VendInvoiceJour>().SingleAsync()).InvoiceAmount);
    }

    [Fact]
    public async Task A_parameters_row_left_at_its_defaults_matches_exactly_as_no_row_does()
    {
        // Guards the deliberate deviation: D365 ships UseInvoiceMatching as No, but defaulting it to
        // Yes here keeps invoices that are matched today matched.
        await using var withRow = await Fixture.CreateAsync();
        var (rowInvoices, rowPo, rowLine) = await ReceivedOrderForMatchingAsync(withRow.Db,
            MatchingPolicy.ThreeWay, MatchingPolicy.None, null, _ => { });
        var withRowResult = await rowInvoices.PostAsync(rowPo.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(rowLine.RecId, 100, 54)]), default);

        await using var withoutRow = await Fixture.CreateAsync();
        var (bareInvoices, barePo, bareLine) = await ReceivedOrderForMatchingAsync(withoutRow.Db,
            MatchingPolicy.ThreeWay, MatchingPolicy.None, null);
        var withoutRowResult = await bareInvoices.PostAsync(barePo.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(bareLine.RecId, 100, 54)]), default);

        Assert.Equal(422, withRowResult.StatusCode);
        Assert.Equal(withoutRowResult.StatusCode, withRowResult.StatusCode);
        Assert.Equal(withoutRowResult.Message, withRowResult.Message);
    }
}
