using System.Reflection;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Departments;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.Markup;
using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits;
using IAX.IXApi.Modules.Finance.Foundation.Structure;
using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments;
using IAX.IXApi.Modules.Finance.Foundation.WorkerShowroomAssignments;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Identity;
using IAX.IXApi.Shared.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Mapster;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class SalesOrderConfirmationTests
{
    static SalesOrderConfirmationTests()
    {
        new SalesOrderConfirmationMapping().Register(TypeAdapterConfig.GlobalSettings);
        new SalesOrderListMapping().Register(TypeAdapterConfig.GlobalSettings);
    }

    [Fact]
    public async Task Discount_recalculation_does_not_exceed_value_after_multiline_discount()
    {
        await using var fixture = await Fixture.CreateAsync();
        var line = await fixture.Db.Set<SalesLine>().SingleAsync();
        line.MultiLnDisc = 800m;
        fixture.Db.Set<InventTableModule>().Add(new InventTableModule {
            RecId = 10, DataAreaId = "dat", ItemId = "ITEM-A",
            ModuleType = ModuleInventPurchSales.Sales, LineDisc = "GROUP"
        });
        fixture.Db.Set<PriceDiscTable>().Add(new PriceDiscTable {
            RecId = 10, DataAreaId = "dat", Module = ModuleInventCustVend.Cust,
            Relation = PriceType.LineDiscSales, AccountCode = PriceDiscPartyCodeType.All,
            ItemCode = PriceDiscProductCodeType.GroupId, ItemRelation = "GROUP",
            Currency = "SAR", Percent1 = 50m, UnitAppliesToAll = 1
        });
        await fixture.Db.SaveChangesAsync();

        var result = await new SalesOrderDiscountService(new TestUnitOfWork(fixture.Db), new TestCompany())
            .RecalculateAsync(1, default);

        Assert.Equal(SalesDiscountRecalculationStatus.Recalculated, result.Status);
        Assert.Equal(200m, line.LineDisc);
        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1, new(), default));
    }

    [Fact]
    public async Task Percentage_charge_follows_line_net_and_zero_does_not_fall_back_to_rate()
    {
        await using var fixture = await Fixture.CreateAsync();
        var order = await fixture.Db.Set<SalesTable>().SingleAsync();
        var line = await fixture.Db.Set<SalesLine>().SingleAsync();
        fixture.Db.Set<MarkupTrans>().Add(new MarkupTrans {
            RecId = 1, DataAreaId = "dat", TransRecId = order.RecId, TransTableId = FinanceTableNumbers.SalesTable,
            ModuleType = MarkupModuleType.Customer, MarkupCategory = MarkupCategory.Percent,
            Value = 10m, CalculatedAmount = 90m
        });
        await fixture.Db.SaveChangesAsync();

        line.LineAmount = 500m;
        await fixture.Db.SaveChangesAsync();
        await SalesOrderChargeRefresh.RefreshAsync(fixture.Db, order, default);
        await fixture.Db.SaveChangesAsync();
        Assert.Equal(40m, (await fixture.Db.Set<MarkupTrans>().SingleAsync()).CalculatedAmount);

        line.LineAmount = line.LineDisc;
        await fixture.Db.SaveChangesAsync();
        await SalesOrderChargeRefresh.RefreshAsync(fixture.Db, order, default);
        await fixture.Db.SaveChangesAsync();
        Assert.Equal(0m, (await fixture.Db.Set<MarkupTrans>().SingleAsync()).CalculatedAmount);
        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1, new(), default));
        Assert.Equal(0m, (await fixture.Db.Set<CustConfirmJour>().SingleAsync()).ConfirmAmount);
    }

    [Fact]
    public async Task Recalculated_charge_invalidates_previous_confirmation_only_when_amount_changes()
    {
        await using var fixture = await Fixture.CreateAsync();
        var order = await fixture.Db.Set<SalesTable>().SingleAsync();
        var line = await fixture.Db.Set<SalesLine>().SingleAsync();
        fixture.Db.Set<MarkupTrans>().Add(new MarkupTrans {
            DataAreaId = "dat", TransRecId = order.RecId, TransTableId = FinanceTableNumbers.SalesTable,
            ModuleType = MarkupModuleType.Customer, MarkupCategory = MarkupCategory.Percent,
            Value = 10m, CalculatedAmount = 90m
        });
        await fixture.Db.SaveChangesAsync();

        order.DocumentStatus = DocumentStatus.Confirmation;
        await SalesOrderChargeRefresh.RefreshAsync(fixture.Db, order, default);
        Assert.Equal(DocumentStatus.Confirmation, order.DocumentStatus);

        line.LineAmount = 500m;
        await fixture.Db.SaveChangesAsync();
        await SalesOrderChargeRefresh.RefreshAsync(fixture.Db, order, default);
        Assert.Equal(DocumentStatus.None, order.DocumentStatus);
        Assert.Equal(40m, (await fixture.Db.Set<MarkupTrans>().SingleAsync()).CalculatedAmount);
    }

    [Fact]
    public void Charges_group_does_not_appear_as_an_unrelated_carrier_group()
    {
        var order = new SalesTable { MarkupGroup = "CHARGES" };
        var record = new SalesOrderListMappingSource(order, string.Empty, string.Empty).Adapt<SalesOrderListDto>();
        Assert.Equal("CHARGES", record.ChargesGroup);
        Assert.Equal(string.Empty, record.CarrierGroup);
    }

    [Fact]
    public async Task Sales_unit_conversion_uses_product_rule_and_rejects_missing_unit()
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Db.Set<InventTable>().Add(new InventTable {
            RecId = 10, DataAreaId = "dat", ItemId = "ITEM-A", Product = 42
        });
        fixture.Db.Set<InventTableModule>().Add(new InventTableModule {
            RecId = 10, DataAreaId = "dat", ItemId = "ITEM-A", ModuleType = ModuleInventPurchSales.Inventory,
            UnitId = "ea"
        });
        fixture.Db.Set<UnitOfMeasure>().AddRange(
            new UnitOfMeasure { RecId = 1, Symbol = "ea", DecimalPrecision = 0 },
            new UnitOfMeasure { RecId = 2, Symbol = "box", DecimalPrecision = 0 });
        fixture.Db.Set<UnitOfMeasureConversion>().Add(new UnitOfMeasureConversion {
            RecId = 1, Product = 42, FromUnitOfMeasure = 2, ToUnitOfMeasure = 1, Factor = 12
        });
        await fixture.Db.SaveChangesAsync();
        var service = new SalesUnitConversionService(new TestUnitOfWork(fixture.Db));
        var item = await fixture.Db.Set<InventTable>().SingleAsync();
        Assert.Equal(24m, await service.ToInventoryQuantityAsync(item, "box", 2m, default));
        Assert.Equal(2m, await service.ToInventoryQuantityAsync(item, "ea", 2m, default));
        Assert.Null(await service.ToInventoryQuantityAsync(item, "pallet", 2m, default));
        var inventoryModule = await fixture.Db.Set<InventTableModule>().SingleAsync();
        inventoryModule.UnitId = "box";
        (await fixture.Db.Set<UnitOfMeasure>().SingleAsync(unit => unit.Symbol == "box")).DecimalPrecision = 3;
        await fixture.Db.SaveChangesAsync();
        Assert.Equal(0.167m, await service.ToInventoryQuantityAsync(item, "ea", 2m, default));
    }

    [Fact]
    public async Task Sales_unit_conversion_uses_item_rule_when_product_is_unassigned()
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Db.Set<InventTable>().AddRange(
            new InventTable { RecId = 10, DataAreaId = "dat", ItemId = "WATER", Product = 0 },
            new InventTable { RecId = 11, DataAreaId = "dat", ItemId = "OIL", Product = 0 });
        fixture.Db.Set<InventTableModule>().AddRange(
            new InventTableModule { RecId = 10, DataAreaId = "dat", ItemId = "WATER", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "BTL" },
            new InventTableModule { RecId = 11, DataAreaId = "dat", ItemId = "OIL", ModuleType = ModuleInventPurchSales.Inventory, UnitId = "BTL" });
        fixture.Db.Set<UnitOfMeasure>().AddRange(
            new UnitOfMeasure { RecId = 1, Symbol = "BTL", DecimalPrecision = 0 },
            new UnitOfMeasure { RecId = 2, Symbol = "CTN", DecimalPrecision = 0 });
        fixture.Db.Set<UnitOfMeasureConversion>().AddRange(
            new UnitOfMeasureConversion { RecId = 1, Product = 10, FromUnitOfMeasure = 2, ToUnitOfMeasure = 1, Factor = 24 },
            new UnitOfMeasureConversion { RecId = 2, Product = 11, FromUnitOfMeasure = 2, ToUnitOfMeasure = 1, Factor = 12 });
        await fixture.Db.SaveChangesAsync();

        var service = new SalesUnitConversionService(new TestUnitOfWork(fixture.Db));
        var items = await fixture.Db.Set<InventTable>().ToDictionaryAsync(item => item.ItemId);
        Assert.Equal(48m, await service.ToInventoryQuantityAsync(items["WATER"], "CTN", 2m, default));
        Assert.Equal(24m, await service.ToInventoryQuantityAsync(items["OIL"], "CTN", 2m, default));
    }

    [Fact]
    public async Task Confirmation_and_reconfirmation_keep_separate_snapshots_without_consuming_inventory()
    {
        await using var fixture = await Fixture.CreateAsync();
        var first = await fixture.Controller.Post(1, new(DateTime.UtcNow.Date), default);
        Assert.IsType<OkObjectResult>(first);
        var journal = await fixture.Db.Set<CustConfirmJour>().SingleAsync();
        var firstLine = await fixture.Db.Set<CustConfirmTrans>().SingleAsync();
        Assert.Equal("CONF-000001", journal.ConfirmId);
        Assert.Equal(900m, firstLine.LineAmount);
        Assert.Equal(10m, firstLine.Qty);
        Assert.Equal(900m, journal.ConfirmAmount);
        Assert.Equal(DocumentStatus.Confirmation, (await fixture.Db.Set<SalesTable>().SingleAsync()).DocumentStatus);
        Assert.Equal(10m, (await fixture.Db.Set<SalesLine>().SingleAsync()).RemainSalesPhysical);
        Assert.Equal(-10m, (await fixture.Db.Set<InventTrans>().SingleAsync()).Qty);

        var line = await fixture.Db.Set<SalesLine>().SingleAsync();
        line.SalesPrice = 120m;
        line.LineAmount = 1200m;
        await fixture.Db.SaveChangesAsync();
        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1, new(DateTime.UtcNow.Date), default));
        var journals = await fixture.Db.Set<CustConfirmJour>().OrderBy(x => x.ConfirmId).ToListAsync();
        var snapshots = await fixture.Db.Set<CustConfirmTrans>().OrderBy(x => x.ConfirmId).ToListAsync();
        Assert.Equal(2, journals.Count);
        Assert.Equal(new[] { 900m, 1100m }, snapshots.Select(x => x.LineAmount));
        Assert.Equal(new[] { 900m, 1100m }, journals.Select(x => x.ConfirmAmount));
        Assert.Equal(10m, (await fixture.Db.Set<SalesLine>().SingleAsync()).RemainSalesPhysical);
        Assert.Equal(-10m, (await fixture.Db.Set<InventTrans>().SingleAsync()).Qty);
    }

    [Fact]
    public async Task Retrying_the_same_posting_request_reuses_its_journal()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.IsType<BadRequestObjectResult>(await fixture.Controller.Post(1,
            new(RequestId: new string('x', FieldLengths.ParmId + 1)), default));
        Assert.Empty(await fixture.Db.Set<CustConfirmJour>().ToListAsync());
        var first = Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1,
            new(RequestId: "post-001"), default));
        var firstHeader = Assert.IsType<ConfirmationHeaderDto>(
            Assert.IsType<APIResponse<object>>(first.Value).Data);
        Assert.Equal("post-001", (await fixture.Db.Set<CustConfirmJour>().SingleAsync()).ParmId);

        var line = await fixture.Db.Set<SalesLine>().SingleAsync();
        line.LineAmount = 1200m;
        await fixture.Db.SaveChangesAsync();
        var retry = Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1,
            new(RequestId: "post-001"), default));
        var retryHeader = Assert.IsType<ConfirmationHeaderDto>(
            Assert.IsType<APIResponse<object>>(retry.Value).Data);
        Assert.Equal(firstHeader.Id, retryHeader.Id);
        Assert.Single(await fixture.Db.Set<CustConfirmJour>().ToListAsync());
        Assert.Single(await fixture.Db.Set<CustConfirmTrans>().ToListAsync());
        Assert.Equal(900m, retryHeader.ConfirmAmount);

        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1,
            new(RequestId: "post-002"), default));
        Assert.Equal(2, await fixture.Db.Set<CustConfirmJour>().CountAsync());
        Assert.Equal(1100m, (await fixture.Db.Set<CustConfirmJour>()
            .SingleAsync(x => x.ParmId == "post-002")).ConfirmAmount);
    }

    [Fact]
    public async Task Confirmation_records_taxed_commercial_amount_without_changing_inventory_demand()
    {
        await using var fixture = await Fixture.CreateAsync();
        var line = await fixture.Db.Set<SalesLine>().SingleAsync();
        line.LineDisc = 0;
        line.TaxGroup = "VAT";
        line.TaxItemGroup = "VAT-ITEM";
        fixture.Db.Set<TaxGroupData>().Add(new TaxGroupData {
            RecId = 1, DataAreaId = "dat", TaxGroup = "VAT", TaxCode = "VAT15"
        });
        fixture.Db.Set<TaxOnItem>().Add(new TaxOnItem {
            RecId = 1, DataAreaId = "dat", TaxItemGroup = "VAT-ITEM", TaxCode = "VAT15"
        });
        fixture.Db.Set<TaxData>().Add(new TaxData {
            RecId = 1, DataAreaId = "dat", TaxCode = "VAT15", TaxValue = 15m
        });
        await fixture.Db.SaveChangesAsync();

        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1, new(), default));
        var first = await fixture.Db.Set<CustConfirmJour>().SingleAsync();
        var firstLine = await fixture.Db.Set<CustConfirmTrans>().SingleAsync();
        Assert.Equal(1000m, first.SalesBalance);
        Assert.Equal(150m, first.SumTax);
        Assert.Equal(1150m, first.ConfirmAmount);
        Assert.Equal(150m, firstLine.TaxAmount);
        Assert.Equal(-10m, (await fixture.Db.Set<InventTrans>().SingleAsync()).Qty);
        Assert.Equal(10m, line.RemainSalesPhysical);
        Assert.Equal(10m, line.RemainSalesFinancial);

        line.SalesQty = 12;
        line.QtyOrdered = 12;
        line.LineAmount = 1200m;
        line.RemainSalesPhysical = 12;
        line.RemainSalesFinancial = 12;
        (await fixture.Db.Set<InventTrans>().SingleAsync()).Qty = -12;
        await fixture.Db.SaveChangesAsync();
        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1, new(), default));

        var history = await fixture.Db.Set<CustConfirmJour>().OrderBy(x => x.ConfirmId).ToListAsync();
        Assert.Equal(new[] { 1150m, 1380m }, history.Select(x => x.ConfirmAmount));
        Assert.Equal(new[] { 10m, 12m }, history.Select(x => x.Qty));
        Assert.Equal(-12m, (await fixture.Db.Set<InventTrans>().SingleAsync()).Qty);
        Assert.Equal(12m, line.RemainSalesPhysical);
        Assert.Equal(12m, line.RemainSalesFinancial);
    }

    [Fact]
    public async Task Tax_inclusive_charge_tax_is_allocated_to_charge_not_merchandise()
    {
        await using var fixture = await Fixture.CreateAsync();
        var order = await fixture.Db.Set<SalesTable>().SingleAsync();
        var line = await fixture.Db.Set<SalesLine>().SingleAsync();
        order.InclTax = true;
        line.LineAmount = 115m;
        line.LineDisc = 0m;
        line.TaxGroup = "VAT";
        line.TaxItemGroup = "VAT-ITEM";
        fixture.Db.Set<TaxGroupData>().Add(new TaxGroupData { RecId = 1, DataAreaId = "dat", TaxGroup = "VAT", TaxCode = "VAT15" });
        fixture.Db.Set<TaxOnItem>().Add(new TaxOnItem { RecId = 1, DataAreaId = "dat", TaxItemGroup = "VAT-ITEM", TaxCode = "VAT15" });
        fixture.Db.Set<TaxData>().Add(new TaxData { RecId = 1, DataAreaId = "dat", TaxCode = "VAT15", TaxValue = 15m });
        fixture.Db.Set<MarkupTrans>().Add(new MarkupTrans {
            RecId = 1, DataAreaId = "dat", TransRecId = order.RecId, TransTableId = FinanceTableNumbers.SalesTable,
            ModuleType = MarkupModuleType.Customer, MarkupCategory = MarkupCategory.Fixed,
            Value = 11.5m, CalculatedAmount = 11.5m, TaxGroup = "VAT", TaxItemGroup = "VAT-ITEM"
        });
        await fixture.Db.SaveChangesAsync();

        var totals = await new SalesOrderReadService(new TestUnitOfWork(fixture.Db), new TestCompany()).TotalsAsync(order.RecId);
        Assert.NotNull(totals);
        decimal Total(string name) => (decimal)totals.GetType().GetProperty(name)!.GetValue(totals)!;
        Assert.Equal(100m, Total("subtotal"));
        Assert.Equal(10m, Total("totalCharges"));
        Assert.Equal(16.5m, Total("salesTax"));
        Assert.Equal(126.5m, Total("invoiceAmount"));

        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(order.RecId, new(), default));
        var journal = await fixture.Db.Set<CustConfirmJour>().SingleAsync();
        Assert.Equal(100m, journal.SalesBalance);
        Assert.Equal(10m, journal.SumMarkup);
        Assert.Equal(16.5m, journal.SumTax);
        Assert.Equal(126.5m, journal.ConfirmAmount);
    }

    [Fact]
    public async Task Line_sales_charge_and_charge_record_both_contribute_to_confirmed_total()
    {
        await using var fixture = await Fixture.CreateAsync();
        var order = await fixture.Db.Set<SalesTable>().SingleAsync();
        var line = await fixture.Db.Set<SalesLine>().SingleAsync();
        line.SalesMarkup = 5m;
        fixture.Db.Set<MarkupTrans>().Add(new MarkupTrans {
            RecId = 1, DataAreaId = "dat", TransRecId = line.RecId,
            TransTableId = FinanceTableNumbers.SalesLine, ModuleType = MarkupModuleType.Customer,
            MarkupCategory = MarkupCategory.Fixed, Value = 10m, CalculatedAmount = 10m
        });
        await fixture.Db.SaveChangesAsync();

        var totals = await new SalesOrderReadService(new TestUnitOfWork(fixture.Db), new TestCompany()).TotalsAsync(order.RecId);
        Assert.NotNull(totals);
        Assert.Equal(15m, totals.GetType().GetProperty("totalCharges")!.GetValue(totals));
        Assert.Equal(915m, totals.GetType().GetProperty("invoiceAmount")!.GetValue(totals));

        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(order.RecId, new(), default));
        var journal = await fixture.Db.Set<CustConfirmJour>().SingleAsync();
        var snapshot = await fixture.Db.Set<CustConfirmTrans>().SingleAsync();
        Assert.Equal(15m, journal.SumMarkup);
        Assert.Equal(915m, journal.ConfirmAmount);
        Assert.Equal(15m, snapshot.SalesMarkup);
    }

    [Fact]
    public async Task Two_line_confirmation_keeps_discount_tax_and_journal_lines_together()
    {
        await using var fixture = await Fixture.CreateAsync();
        var firstLine = await fixture.Db.Set<SalesLine>().SingleAsync();
        firstLine.TaxGroup = "VAT";
        firstLine.TaxItemGroup = "VAT-ITEM";
        firstLine.LinePercent = 10m;
        firstLine.InventTransId = "LOT-000101";
        fixture.Db.Set<SalesLine>().Add(new SalesLine {
            RecId = 2, DataAreaId = "dat", SalesId = "SO1", LineNum = 2, ItemId = "ITEM-B",
            SalesQty = 5, QtyOrdered = 5, RemainSalesPhysical = 5, RemainSalesFinancial = 5,
            SalesUnit = "EA", SalesPrice = 200m, PriceUnit = 1m, LineAmount = 1000m,
            InventTransId = "LOT-000102", InventDimId = "DIM-1", SalesStatus = SalesStatus.Backorder,
            CurrencyCode = "SAR", TaxGroup = "VAT", TaxItemGroup = "VAT-ITEM"
        });
        fixture.Db.Set<TaxGroupData>().Add(new TaxGroupData {
            RecId = 1, DataAreaId = "dat", TaxGroup = "VAT", TaxCode = "VAT15"
        });
        fixture.Db.Set<TaxOnItem>().Add(new TaxOnItem {
            RecId = 1, DataAreaId = "dat", TaxItemGroup = "VAT-ITEM", TaxCode = "VAT15"
        });
        fixture.Db.Set<TaxData>().Add(new TaxData {
            RecId = 1, DataAreaId = "dat", TaxCode = "VAT15", TaxValue = 15m
        });
        await fixture.Db.SaveChangesAsync();

        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1, new(), default));
        var journal = await fixture.Db.Set<CustConfirmJour>().SingleAsync();
        var snapshots = (await fixture.Db.Set<CustConfirmTrans>().ToListAsync())
            .OrderBy(x => x.LineNum).ToList();
        Assert.Equal(15m, journal.Qty);
        Assert.Equal(1900m, journal.SalesBalance);
        Assert.Equal(285m, journal.SumTax);
        Assert.Equal(2185m, journal.ConfirmAmount);
        Assert.Equal(new[] { 900m, 1000m }, snapshots.Select(x => x.LineAmount));
        Assert.Equal(new[] { 135m, 150m }, snapshots.Select(x => x.TaxAmount));
        Assert.All(snapshots, line => Assert.Equal(journal.ConfirmId, line.ConfirmId));

        firstLine.SalesPrice = 110m;
        firstLine.LineAmount = 1100m;
        await fixture.Db.SaveChangesAsync();
        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1, new(), default));
        var original = Assert.IsType<OkObjectResult>(await fixture.Controller.Get(1, journal.RecId, default));
        var data = Assert.IsType<APIResponse<object>>(original.Value).Data!;
        var lines = Assert.IsType<List<ConfirmationLineDto>>(data.GetType().GetProperty("lines")!.GetValue(data));
        Assert.Equal(2, lines.Count);
        Assert.Equal(new[] { 900m, 1000m }, lines.Select(x => x.LineAmount));
    }

    [Fact]
    public async Task Reconfirmation_preserves_a_later_document_status()
    {
        await using var fixture = await Fixture.CreateAsync();
        var order = await fixture.Db.Set<SalesTable>().SingleAsync();
        order.DocumentStatus = DocumentStatus.PackingSlip;
        await fixture.Db.SaveChangesAsync();

        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1, new(), default));

        Assert.Equal(DocumentStatus.PackingSlip, order.DocumentStatus);
        Assert.Single(await fixture.Db.Set<CustConfirmJour>().ToListAsync());
        Assert.Single(await fixture.Db.Set<CustConfirmTrans>().ToListAsync());
    }

    [Fact]
    public async Task Posting_disabled_or_mandatory_credit_check_creates_no_history()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.IsType<BadRequestObjectResult>(await fixture.Controller.Post(1, new(Posting: false), default));
        var customer = await fixture.Db.Set<CustTable>().SingleAsync();
        customer.MandatoryCreditLimit = NoYes.Yes;
        await fixture.Db.SaveChangesAsync();
        Assert.IsType<UnprocessableEntityObjectResult>(await fixture.Controller.Post(1, new(), default));
        Assert.Empty(await fixture.Db.Set<CustConfirmJour>().ToListAsync());
        Assert.Empty(await fixture.Db.Set<CustConfirmTrans>().ToListAsync());
        Assert.Equal(DocumentStatus.None, (await fixture.Db.Set<SalesTable>().SingleAsync()).DocumentStatus);
    }

    [Fact]
    public async Task Pro_forma_preview_calculates_document_without_posting()
    {
        await using var fixture = await Fixture.CreateAsync();
        var result = Assert.IsType<OkObjectResult>(await fixture.Controller.Preview(1, default));
        var data = Assert.IsType<APIResponse<object>>(result.Value).Data!;
        var header = Assert.IsType<ConfirmationHeaderDto>(data.GetType().GetProperty("header")!.GetValue(data));
        var lines = Assert.IsType<List<ConfirmationLineDto>>(data.GetType().GetProperty("lines")!.GetValue(data));
        Assert.Equal(900m, header.ConfirmAmount);
        Assert.Single(lines);
        Assert.Empty(await fixture.Db.Set<CustConfirmJour>().ToListAsync());
        Assert.Empty(await fixture.Db.Set<CustConfirmTrans>().ToListAsync());
        Assert.Equal(DocumentStatus.None, (await fixture.Db.Set<SalesTable>().SingleAsync()).DocumentStatus);
        Assert.Equal(-10m, (await fixture.Db.Set<InventTrans>().SingleAsync()).Qty);
    }

    [Fact]
    public async Task Confirmation_rejects_discounts_that_exceed_the_sales_amount()
    {
        await using var fixture = await Fixture.CreateAsync();
        var line = await fixture.Db.Set<SalesLine>().SingleAsync();
        line.MultiLnDisc = line.LineAmount;
        await fixture.Db.SaveChangesAsync();
        Assert.IsType<UnprocessableEntityObjectResult>(await fixture.Controller.Post(1, new(), default));
        Assert.Empty(await fixture.Db.Set<CustConfirmJour>().ToListAsync());

        line.MultiLnDisc = 0;
        (await fixture.Db.Set<SalesTable>().SingleAsync()).DiscPercent = 101;
        await fixture.Db.SaveChangesAsync();
        Assert.IsType<UnprocessableEntityObjectResult>(await fixture.Controller.Post(1, new(), default));
        Assert.Empty(await fixture.Db.Set<CustConfirmTrans>().ToListAsync());
    }

    [Fact]
    public async Task Copy_and_cancellation_reject_orders_outside_selected_company()
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Db.Set<SalesTable>().Add(new SalesTable {
            RecId = 2, DataAreaId = "other", SalesId = "OTHER-SO", SalesStatus = SalesStatus.Backorder
        });
        await fixture.Db.SaveChangesAsync();
        var company = new TestCompany();
        var work = new TestUnitOfWork(fixture.Db);
        var copy = new SalesOrderCopyService(work, null!, null!, new SalesOrderCopyValidator(), company);
        var cancellation = new SalesOrderCancellationService(work, null!, company);

        Assert.Equal(404, (await copy.GetSalesOrdersAsync(2, default)).StatusCode);
        Assert.Equal(SalesOrderCopyStatus.DestinationNotFound,
            (await copy.CopyAsync(2, new SalesOrderCopyRequest("fromAll", ["1"]), default)).Status);
        Assert.Equal(SalesOrderCancelStatus.NotFound, await cancellation.CancelAsync(2, default));
        Assert.Equal(SalesStatus.Backorder, (await fixture.Db.Set<SalesTable>().SingleAsync(x => x.RecId == 2)).SalesStatus);
        var reads = new SalesOrderReadService(work, company);
        Assert.Null(await reads.LinesAsync(2, default));
        Assert.Null(await reads.TotalsAsync(2, default));
        var visibleOrders = (await reads.GetListAsync(default)).ToList();
        Assert.Single(visibleOrders);
        Assert.Equal("SO1", visibleOrders[0].SalesId);
    }

    [Fact]
    public async Task Removing_sales_charge_requires_a_new_confirmation_without_erasing_history()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1, new(), default));
        var charge = new MarkupTrans {
            RecId = 1, DataAreaId = "dat", ModuleType = MarkupModuleType.Customer,
            TransTableId = FinanceTableNumbers.SalesTable, TransRecId = 1
        };
        fixture.Db.Set<MarkupTrans>().Add(charge);
        await fixture.Db.SaveChangesAsync();
        var command = new MarkupTransCommandService(new TestUnitOfWork(fixture.Db), new TestCompany(), null!);

        await command.DeleteAsync(charge, default);

        Assert.Equal(DocumentStatus.None, (await fixture.Db.Set<SalesTable>().SingleAsync()).DocumentStatus);
        Assert.Single(await fixture.Db.Set<CustConfirmJour>().ToListAsync());
        Assert.Single(await fixture.Db.Set<CustConfirmTrans>().ToListAsync());
    }

    [Fact]
    public async Task Processed_sales_line_cannot_remove_its_charge()
    {
        await using var fixture = await Fixture.CreateAsync();
        var line = await fixture.Db.Set<SalesLine>().SingleAsync();
        line.RemainSalesPhysical = 5m;
        fixture.Db.Set<MarkupTrans>().Add(new MarkupTrans {
            RecId = 10, DataAreaId = "dat", ModuleType = MarkupModuleType.Customer,
            TransTableId = FinanceTableNumbers.SalesLine, TransRecId = line.RecId
        });
        await fixture.Db.SaveChangesAsync();
        var service = new MarkupTransService(new TestUnitOfWork(fixture.Db), new TestCompany(),
            null!, null!);

        var result = await service.DeleteAsync(10, default);

        Assert.NotNull(result.Error);
        Assert.Single(await fixture.Db.Set<MarkupTrans>().ToListAsync());
    }

    [Fact]
    public async Task Copy_from_journal_uses_sales_order_as_well_as_confirmation_identity()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1, new(), default));
        var original = await fixture.Db.Set<CustConfirmJour>().SingleAsync();
        fixture.Db.Set<CustConfirmJour>().Add(new CustConfirmJour {
            RecId = 2, DataAreaId = "dat", SalesId = "OTHER-SO", ConfirmId = original.ConfirmId
        });
        fixture.Db.Set<CustConfirmTrans>().Add(new CustConfirmTrans {
            RecId = 2, DataAreaId = "dat", SalesId = "OTHER-SO", ConfirmId = original.ConfirmId,
            LineNum = 1, ItemId = "OTHER-ITEM"
        });
        await fixture.Db.SaveChangesAsync();
        var copy = new SalesOrderCopyService(new TestUnitOfWork(fixture.Db), null!, null!,
            new SalesOrderCopyValidator(), new TestCompany());

        var result = await copy.GetJournalLinesAsync(1, original.RecId, default);

        Assert.Equal(200, result.StatusCode);
        Assert.Single(Assert.IsAssignableFrom<System.Collections.IEnumerable>(result.Data).Cast<object>());
    }

    [Fact]
    public async Task Copy_rejects_source_lines_without_their_order_or_confirmation_header()
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Db.Set<SalesLine>().Add(new SalesLine {
            RecId = 22, DataAreaId = "dat", SalesId = "MISSING-ORDER", ItemId = "ITEM-A",
            SalesQty = 1, SalesUnit = "EA", CurrencyCode = "SAR"
        });
        fixture.Db.Set<CustConfirmTrans>().Add(new CustConfirmTrans {
            RecId = 23, DataAreaId = "dat", SalesId = "MISSING-ORDER", ConfirmId = "MISSING-CONFIRM",
            ItemId = "ITEM-A", Qty = 1, SalesUnit = "EA", CurrencyCode = "SAR"
        });
        await fixture.Db.SaveChangesAsync();
        var copy = new SalesOrderCopyService(new TestUnitOfWork(fixture.Db), null!, null!,
            new SalesOrderCopyValidator(), new TestCompany());

        Assert.Equal(SalesOrderCopyStatus.SourceLinesNotFound,
            (await copy.CopyAsync(1, new SalesOrderCopyRequest("fromAll", ["22"]), default)).Status);
        Assert.Equal(SalesOrderCopyStatus.SourceLinesNotFound,
            (await copy.CopyAsync(1, new SalesOrderCopyRequest("fromJournal", ["23"]), default)).Status);
        Assert.Single(await fixture.Db.Set<SalesLine>().Where(x => x.SalesId == "SO1").ToListAsync());
        Assert.Empty(await fixture.Db.Set<CustConfirmJour>().ToListAsync());
    }

    [Fact]
    public async Task Precise_line_copy_scales_existing_sales_charge()
    {
        await using var fixture = await Fixture.CreateAsync();
        (await fixture.Db.Set<SalesLine>().SingleAsync()).SalesMarkup = 5m;
        fixture.Db.Set<SalesTable>().Add(new SalesTable {
            RecId = 2, DataAreaId = "dat", SalesId = "SO2", CustAccount = "C1",
            CurrencyCode = "SAR", SalesStatus = SalesStatus.Backorder
        });
        fixture.Db.Set<InventTable>().Add(new InventTable {
            RecId = 10, DataAreaId = "dat", ItemId = "ITEM-A"
        });
        fixture.Db.Set<InventTableModule>().Add(new InventTableModule {
            RecId = 10, DataAreaId = "dat", ItemId = "ITEM-A",
            ModuleType = ModuleInventPurchSales.Inventory, UnitId = "EA"
        });
        await fixture.Db.SaveChangesAsync();
        var work = new TestUnitOfWork(fixture.Db);
        var demand = DispatchProxy.Create<IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces.ISalesInventoryDemandService, TestDemandProxy>();
        var copy = new SalesOrderCopyService(work, demand, new SalesUnitConversionService(work),
            new SalesOrderCopyValidator(), new TestCompany());

        var result = await copy.CopyAsync(2, new SalesOrderCopyRequest("fromAll", ["1"], QuantityFactor: 2), default);

        Assert.Equal(SalesOrderCopyStatus.Success, result.Status);
        var copied = await fixture.Db.Set<SalesLine>().SingleAsync(x => x.SalesId == "SO2");
        Assert.Equal(20m, copied.SalesQty);
        Assert.Equal(10m, copied.SalesMarkup);
    }

    [Fact]
    public async Task Confirmation_reads_keep_list_and_detail_contracts()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.IsType<OkObjectResult>(await fixture.Controller.Post(1, new(), default));

        var listed = Assert.IsType<OkObjectResult>(await fixture.Controller.List(1, default));
        var listResponse = Assert.IsType<APIResponse<object>>(listed.Value);
        var row = Assert.Single(Assert.IsType<List<ConfirmationListItemDto>>(listResponse.Data));
        Assert.Equal("1", row.Id);
        Assert.Equal("CONF-000001", row.ConfirmId);
        Assert.Equal("SO1", row.SalesId);

        var detail = Assert.IsType<OkObjectResult>(await fixture.Controller.Get(1, 1, default));
        var detailData = Assert.IsType<APIResponse<object>>(detail.Value).Data!;
        var detailLines = Assert.IsType<List<ConfirmationLineDto>>(
            detailData.GetType().GetProperty("lines")!.GetValue(detailData));
        var detailLine = Assert.Single(detailLines);
        Assert.Equal("WH-1", detailLine.InventLocationId);
        Assert.Equal("BIN-1", detailLine.WmsLocationId);
        Assert.IsType<NotFoundObjectResult>(await fixture.Controller.Get(1, 2, default));
        Assert.IsType<NotFoundObjectResult>(await fixture.Controller.List(2, default));
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        public ConfirmationContext Db { get; private set; } = null!;
        public SalesOrderConfirmationController Controller { get; private set; } = null!;

        public static async Task<Fixture> CreateAsync()
        {
            var fixture = new Fixture();
            await fixture.connection.OpenAsync();
            fixture.Db = new ConfirmationContext(new DbContextOptionsBuilder<ConfirmationContext>()
                .UseSqlite(fixture.connection).Options);
            await fixture.Db.Database.EnsureCreatedAsync();
            fixture.Db.Set<CustTable>().Add(new CustTable { RecId = 1, DataAreaId = "dat", AccountNum = "C1" });
            fixture.Db.Set<SalesTable>().Add(new SalesTable {
                RecId = 1, DataAreaId = "dat", SalesId = "SO1", CustAccount = "C1",
                InvoiceAccount = "C1", CurrencyCode = "SAR", SalesStatus = SalesStatus.Backorder,
                DocumentStatus = DocumentStatus.None, OrderDate = DateTime.UtcNow.Date
            });
            fixture.Db.Set<SalesLine>().Add(new SalesLine {
                RecId = 1, DataAreaId = "dat", SalesId = "SO1", LineNum = 1, ItemId = "ITEM-A",
                SalesQty = 10, QtyOrdered = 10, RemainSalesPhysical = 10, RemainSalesFinancial = 10,
                SalesUnit = "EA", SalesPrice = 100, PriceUnit = 1, LineAmount = 1000,
                InventDimId = "DIM-1",
                LineDisc = 100, SalesStatus = SalesStatus.Backorder, CurrencyCode = "SAR"
            });
            fixture.Db.Set<InventTrans>().Add(new InventTrans { RecId = 1, DataAreaId = "dat", ItemId = "ITEM-A", Qty = -10 });
            fixture.Db.Set<InventDim>().Add(new InventDim {
                RecId = 1, DataAreaId = "dat", InventDimId = "DIM-1",
                InventSiteId = "SITE-1", InventLocationId = "WH-1", WmsLocationId = "BIN-1"
            });
            await fixture.Db.SaveChangesAsync();
            var sequence = DispatchProxy.Create<ISysNumberSequenceService, TestSequenceProxy>();
            var context = new FinanceContextAdapter(fixture.Db);
            var company = new TestCompany();
            fixture.Controller = new SalesOrderConfirmationController(
                new SalesOrderConfirmationService(new TestUnitOfWork(fixture.Db), sequence, company,
                    new SalesOrderConfirmationValidator()));
            return fixture;
        }

        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await connection.DisposeAsync(); }
    }

    private sealed class ConfirmationContext(DbContextOptions<ConfirmationContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            var included = new HashSet<Type>
            {
                typeof(SalesTable), typeof(SalesLine), typeof(CustTable), typeof(CustParameters),
                typeof(DirPartyTable), typeof(LogisticsPostalAddress),
                typeof(CustConfirmJour), typeof(CustConfirmTrans), typeof(InventTrans),
                typeof(InventDim),
                typeof(MarkupTrans), typeof(TaxGroupData), typeof(TaxOnItem),
                typeof(TaxData), typeof(InventTableModule), typeof(InventTable),
                typeof(UnitOfMeasure), typeof(UnitOfMeasureConversion), typeof(PriceDiscTable)
            };
            foreach (var type in typeof(SalesTable).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract
                    && typeof(Entity<long>).IsAssignableFrom(type) && !included.Contains(type)))
                builder.Ignore(type);
            Configure<SalesTable>(builder);
            Configure<SalesLine>(builder);
            Configure<CustTable>(builder);
            Configure<CustParameters>(builder);
            Configure<DirPartyTable>(builder);
            Configure<LogisticsPostalAddress>(builder);
            Configure<CustConfirmJour>(builder);
            Configure<CustConfirmTrans>(builder);
            Configure<InventTrans>(builder);
            Configure<InventDim>(builder);
            Configure<MarkupTrans>(builder);
            Configure<TaxGroupData>(builder);
            Configure<TaxOnItem>(builder);
            Configure<TaxData>(builder);
            Configure<InventTableModule>(builder);
            Configure<InventTable>(builder);
            Configure<UnitOfMeasure>(builder);
            Configure<UnitOfMeasureConversion>(builder);
            Configure<PriceDiscTable>(builder);
        }

        private static void Configure<TEntity>(ModelBuilder builder) where TEntity : Entity<long>
        {
            builder.Entity<TEntity>().HasKey(x => x.RecId);
            builder.Entity<TEntity>().Property(x => x.RecId).ValueGeneratedOnAdd();
            builder.Entity<TEntity>().Ignore(x => x.RowVersion);
            foreach (var property in typeof(TEntity).GetProperties())
            {
                var type = property.PropertyType;
                if (type != typeof(string) && type != typeof(byte[]) && !type.IsValueType)
                    builder.Entity<TEntity>().Ignore(property.Name);
            }
        }
    }

    public class TestSequenceProxy : DispatchProxy
    {
        private int next;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method?.Name == nameof(ISysNumberSequenceService.NextAsync))
                return Task.FromResult(new NextSequenceResultDto {
                    EntityName = "CustConfirmJour", Value = ++next, Code = $"CONF-{next:D6}"
                });
            throw new NotSupportedException(method?.Name);
        }
    }

    public class TestDemandProxy : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Task.CompletedTask;
    }

    private sealed class TestCompany : ICompanyExecutionContext
    {
        public string GetDataAreaId() => "dat";
        public bool IsRequestedCompanyAuthorized() => true;
    }

    private sealed class TestUnitOfWork(DbContext context) : IUnitOfWork
    {
        public DbContext Context => context;
        public Task<int> CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var repository = DispatchProxy.Create<IGenericRepository<T>, ConfirmationRepositoryProxy<T>>();
            ((ConfirmationRepositoryProxy<T>)(object)repository).Context = context;
            return repository;
        }
        public Task BeginTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task CommitTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task RollbackTransactionAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public void Dispose() { }
    }

    public class ConfirmationRepositoryProxy<T> : DispatchProxy where T : class
    {
        public DbContext Context { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            switch (method?.Name)
            {
                case "GetQueryable": return Context.Set<T>().AsQueryable();
                case "AddAsync": Context.Add((T)args![0]!); return Task.FromResult((T)args[0]!);
                case "UpdateAsync": return Task.FromResult((T)args![0]!);
                case "RemoveAsync": Context.Remove((T)args![0]!); return Task.CompletedTask;
                default: throw new NotSupportedException(method?.Name);
            }
        }
    }

    private sealed class FinanceContextAdapter(DbContext context) : IFinanceDataContext
    {
        public DatabaseFacade Database => context.Database;
        public ChangeTracker ChangeTracker => context.ChangeTracker;
        public DbSet<HcmWorker> HcmWorkers => context.Set<HcmWorker>();
        public DbSet<HcmDepartment> HcmDepartments => context.Set<HcmDepartment>();
        public DbSet<HcmShowroom> HcmShowrooms => context.Set<HcmShowroom>();
        public DbSet<OrganizationUnit> OrganizationUnits => context.Set<OrganizationUnit>();
        public DbSet<HcmWorkerOrganizationAssignment> HcmWorkerOrganizationAssignments => context.Set<HcmWorkerOrganizationAssignment>();
        public DbSet<HcmWorkerOrganizationAssignmentV1> HcmWorkerOrganizationAssignmentsV1 => context.Set<HcmWorkerOrganizationAssignmentV1>();
        public DbSet<HcmWorkerShowroomAssignment> HcmWorkerShowroomAssignments => context.Set<HcmWorkerShowroomAssignment>();
        public DbSet<OrganizationRole> OrganizationRoles => context.Set<OrganizationRole>();
        public DbSet<OrganizationHierarchy> OrganizationHierarchies => context.Set<OrganizationHierarchy>();
        public DbSet<OrganizationHierarchyNode> OrganizationHierarchyNodes => context.Set<OrganizationHierarchyNode>();
        public DbSet<HcmPosition> HcmPositions => context.Set<HcmPosition>();
        public DbSet<HcmReportingHierarchy> HcmReportingHierarchies => context.Set<HcmReportingHierarchy>();
        public DbSet<HcmPositionReportingLine> HcmPositionReportingLines => context.Set<HcmPositionReportingLine>();
        public DbSet<TaxData> TaxData => context.Set<TaxData>();
        public DbSet<TaxGroupHeading> TaxGroupHeadings => context.Set<TaxGroupHeading>();
        public DbSet<TaxGroupData> TaxGroupDatas => context.Set<TaxGroupData>();
        public DbSet<TaxOnItem> TaxOnItems => context.Set<TaxOnItem>();
        public DbSet<TEntity> Set<TEntity>() where TEntity : class => context.Set<TEntity>();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
    }
}
