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
    /// <summary>
    /// Every flow applies a resolved rate as net * rate / 100. A code configured with any other
    /// origin, calculation method, limit or compounding must be refused rather than posted at the
    /// wrong base, which is what happened before these fields were read.
    /// </summary>
    [Theory]
    [InlineData(TaxBaseType.PctPerGross, "has origin PctPerGross")]
    [InlineData(TaxBaseType.PctPerTax, "has origin PctPerTax")]
    [InlineData(TaxBaseType.AmountByUnit, "has origin AmountByUnit")]
    [InlineData(TaxBaseType.PctGrossOnNet, "has origin PctGrossOnNet")]
    [InlineData(TaxBaseType.PctPerMargin, "has origin PctPerMargin")]
    public async Task An_uncalculated_tax_origin_is_refused_instead_of_posting_the_wrong_base(
        TaxBaseType origin, string expected)
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        var (invoices, po, line) = await ReceivedOrderForTaxAsync(db, x => x.TaxBase = origin);

        var result = await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100)]), default);
        Assert.Equal(422, result.StatusCode);
        Assert.Contains(expected, result.Message);
        Assert.Empty(await db.Set<VendInvoiceJour>().ToListAsync());
    }

    [Fact]
    public async Task An_uncalculated_tax_calculation_method_limit_or_compounding_is_refused_by_name()
    {
        foreach (var (configure, expected) in new (Action<TaxTable>, string)[]
        {
            (x => x.TaxCalcMethod = TaxCalcMethod.Total, "is calculated on the Total amount"),
            (x => x.TaxLimitBase = TaxLimitBase.Invoice, "has a Invoice tax limit"),
            (x => x.TaxLimitBase = TaxLimitBase.Line, "has a Line tax limit"),
            (x => x.TaxOnTax = "OTHER", "is compounded on tax code 'OTHER'")
        })
        {
            await using var fixture = await Fixture.CreateAsync();
            var db = fixture.Db;
            var (invoices, po, line) = await ReceivedOrderForTaxAsync(db, configure);

            var result = await invoices.PostAsync(po.RecId,
                new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100)]), default);
            Assert.Equal(422, result.StatusCode);
            Assert.Contains(expected, result.Message);
        }
    }

    [Fact]
    public async Task The_stored_default_tax_configuration_still_posts_the_percentage_of_net()
    {
        await using var fixture = await Fixture.CreateAsync();
        var db = fixture.Db;
        // PctPerNet / Line / None / no compounding is the stored default, so nothing configured
        // today is affected by the refusals above.
        var (invoices, po, line) = await ReceivedOrderForTaxAsync(db, _ => { });

        Assert.Equal(200, (await invoices.PostAsync(po.RecId,
            new VendorInvoicePostDto("INV-1", new DateTime(2026, 10, 29), [new(line.RecId, 100)]), default)).StatusCode);
        // 5000 net at 15% of net.
        Assert.Equal(750m, (await db.Set<TaxTrans>().SingleAsync()).TaxAmount);
    }
}
