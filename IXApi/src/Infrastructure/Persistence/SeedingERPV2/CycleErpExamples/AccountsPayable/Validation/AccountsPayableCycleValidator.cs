using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Validation;

/// <summary>Validate and capture master references inside the seed transaction; no posted-balance checks.</summary>
public sealed class AccountsPayableCycleValidator : ISeedValidator
{
    private readonly AccountsPayableExampleCatalog catalog = new();
    private AccountsPayableReconciliationReport? prepared;

    internal IReadOnlyList<ConfigurationPreparationStep> PreparationSteps => catalog.Preparation.Steps;

    internal void ValidateDefinition()
    {
        prepared = null;
        catalog.Validate();
    }

    internal AccountsPayableReconciliationReport TakePreparedReport()
    {
        var result = prepared ?? throw new InvalidOperationException("AP configuration/master data has not been validated.");
        prepared = null;
        return result;
    }

    public async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        catalog.Validate();
        prepared = null;
        static void Check(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException("AP cycle master-data manifest: " + message);
        }
        var ledger = await c.RequireAsync<Ledger>(x => x.Name == ExampleIdentifiers.Setup, ct);
        var bank = await c.RequireAsync<BankAccountTable>(x => x.AccountId == "APV2-BANK", ct);
        var invoiceJournal = await c.RequireAsync<LedgerJournalName>(x => x.JournalName == "APV2-INV", ct);
        var paymentJournal = await c.RequireAsync<LedgerJournalName>(x => x.JournalName == "APV2-PAY", ct);
        var dim = await c.RequireAsync<InventDim>(x => x.InventDimId == "APV2-DIM", ct);
        var vendorReferences = new List<ExampleVendorReference>();
        foreach (var definition in catalog.Vendors)
        {
            var account = ExampleIdentifiers.Vendor(c.Company, definition.Suffix);
            var vendor = await c.RequireAsync<VendTable>(x => x.AccountNum == account, ct);
            var party = await c.RequireAsync<DirPartyTable>(x => x.RecId == vendor.Party, ct);
            var address = await c.RequireAsync<LogisticsPostalAddress>(x => x.Location == party.PrimaryAddressLocation, ct);
            var vendorBank = await c.RequireAsync<VendBankAccount>(x => x.VendAccount == account && x.AccountID == vendor.BankAccount, ct);
            Check(party.Name == definition.Name && vendor.Currency == definition.Currency && vendor.PaymTermId == definition.PaymentTerms
                && vendor.PaymMode == definition.PaymentMethod && vendor.TaxGroup == definition.TaxGroup && vendor.BankAccount == definition.BankAccount,
                "persisted supplier differs from vendors.json.");
            vendorReferences.Add(new(vendor.RecId, vendor.AccountNum, party.RecId, party.Name, vendor.Currency,
                vendor.PaymTermId, vendor.PaymMode, address.RecId, vendorBank.RecId, vendor.BankAccount));
        }
        var productReferences = new List<ExampleProductReference>();
        foreach (var definition in catalog.Products)
        {
            var item = await c.RequireAsync<InventTable>(x => x.ItemId == definition.ItemId, ct);
            var module = await c.RequireAsync<InventTableModule>(x => x.ItemId == item.ItemId && x.ModuleType == ModuleInventPurchSales.Purchase, ct);
            var model = await c.RequireAsync<InventModelGroupItem>(x => x.ItemId == item.ItemId && x.ItemDataAreaId == c.Company, ct);
            var group = await c.RequireAsync<InventItemGroupItem>(x => x.ItemId == item.ItemId && x.ItemDataAreaId == c.Company, ct);
            Check(item.Name == definition.Name && item.ItemType.ToString() == definition.ProductType && module.UnitId == definition.Unit
                && model.ModelGroupId == definition.ModelGroup && group.ItemGroupId == definition.ItemGroup && module.TaxItemGroupId == definition.ItemTaxGroup,
                "persisted item differs from products.json.");
            productReferences.Add(new(item.RecId, item.ItemId, item.Product, item.Name, item.ItemType.ToString(), module.UnitId,
                model.ModelGroupId, group.ItemGroupId, module.TaxItemGroupId, item.PrimaryVendorID));
        }
        var priceReferences = new List<ExamplePurchasePriceReference>();
        var from = new DateTime(c.FiscalYear, 1, 1);
        var to = new DateTime(c.FiscalYear, 12, 31);
        foreach (var definition in catalog.Prices)
        {
            var account = ExampleIdentifiers.Vendor(c.Company, definition.VendorSuffix);
            var price = await c.RequireAsync<PriceDiscTable>(x => x.AccountRelation == account && x.ItemRelation == definition.ItemId
                && x.Module == ModuleInventCustVend.Vend && x.Relation == PriceType.PricePurch && x.FromDate == from, ct);
            Check(price.Currency == definition.Currency && price.Amount == definition.Amount && price.PriceUnit == definition.PriceUnit
                && price.UnitId == definition.Unit && price.InventDimId == definition.InventDimId && price.ToDate == to && price.QuantityAmountFrom == 1m,
                "persisted purchase quote differs from purchase-prices.json.");
            priceReferences.Add(new(price.RecId, price.AccountRelation, price.ItemRelation, price.Currency, price.Amount,
                price.PriceUnit, price.UnitId, price.InventDimId, price.FromDate, price.ToDate));
        }
        prepared = new(new(ledger.RecId, ledger.ChartOfAccounts, ledger.FiscalCalendar, ledger.AccountingCurrency,
            dim.InventSiteId, dim.InventLocationId, dim.InventDimId, bank.RecId, bank.AccountId,
            invoiceJournal.JournalName, paymentJournal.JournalName, "APV2", "APV2"),
            vendorReferences.AsReadOnly(), productReferences.AsReadOnly(), priceReferences.AsReadOnly());
    }
}
