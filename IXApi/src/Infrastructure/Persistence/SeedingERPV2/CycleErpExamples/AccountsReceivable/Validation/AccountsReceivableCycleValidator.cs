using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Reports;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Validation;

/// <summary>Validate and capture master references inside the seed transaction; no posted-balance checks.</summary>
public sealed class AccountsReceivableCycleValidator : ISeedValidator
{
    private readonly AccountsReceivableExampleCatalog catalog = new();
    private AccountsReceivableReconciliationReport? prepared;

    internal IReadOnlyList<ConfigurationPreparationStep> PreparationSteps => catalog.Preparation.Steps;

    internal void ValidateDefinition()
    {
        prepared = null;
        catalog.Validate();
    }

    internal AccountsReceivableReconciliationReport TakePreparedReport()
    {
        var result = prepared ?? throw new InvalidOperationException("AR configuration/master data has not been validated.");
        prepared = null;
        return result;
    }

    public async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        catalog.Validate();
        prepared = null;
        static void Check(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException("AR cycle master-data manifest: " + message);
        }
        var ledger = await c.RequireAsync<Ledger>(x => x.Name == "APV2", ct);
        var bank = await c.RequireAsync<BankAccountTable>(x => x.AccountId == "APV2-BANK", ct);
        var invoiceJournal = await c.RequireAsync<LedgerJournalName>(x => x.JournalName == "ARV2-INV", ct);
        var paymentJournal = await c.RequireAsync<LedgerJournalName>(x => x.JournalName == "ARV2-PAY", ct);
        var dim = await c.RequireAsync<InventDim>(x => x.InventDimId == "APV2-DIM", ct);

        var customerReferences = new List<ExampleCustomerReference>();
        foreach (var definition in catalog.Customers)
        {
            var account = ExampleIdentifiers.Customer(c.Company, definition.Suffix);
            var customer = await c.RequireAsync<CustTable>(x => x.AccountNum == account, ct);
            var party = await c.RequireAsync<DirPartyTable>(x => x.RecId == customer.Party, ct);
            var address = await c.RequireAsync<LogisticsPostalAddress>(x => x.Location == party.PrimaryAddressLocation, ct);
            Check(party.Name == definition.Name && customer.CurrencyCode == definition.Currency
                && customer.PaymTermId == definition.PaymentTerms && customer.PaymModeId == definition.PaymentMethod
                && customer.TaxGroupId == definition.TaxGroup && customer.CustGroupId == definition.CustomerGroup,
                "persisted customer differs from customers.json.");
            customerReferences.Add(new(customer.RecId, customer.AccountNum, party.RecId, party.Name, customer.CurrencyCode,
                customer.PaymTermId, customer.PaymModeId, address.RecId, customer.CustGroupId, customer.TaxGroupId));
        }

        var priceReferences = new List<ExampleSalesPriceReference>();
        var from = new DateTime(c.FiscalYear, 1, 1);
        var to = new DateTime(c.FiscalYear, 12, 31);
        foreach (var definition in catalog.Prices)
        {
            var account = ExampleIdentifiers.Customer(c.Company, definition.CustomerSuffix);
            var price = await c.RequireAsync<PriceDiscTable>(x => x.AccountRelation == account && x.ItemRelation == definition.ItemId
                && x.Module == ModuleInventCustVend.Cust && x.Relation == PriceType.PriceSales && x.FromDate == from, ct);
            Check(price.Currency == definition.Currency && price.Amount == definition.Amount && price.PriceUnit == definition.PriceUnit
                && price.UnitId == definition.Unit && price.InventDimId == definition.InventDimId && price.ToDate == to && price.QuantityAmountFrom == 1m,
                "persisted sales quotation differs from sales-prices.json.");
            priceReferences.Add(new(price.RecId, price.AccountRelation, price.ItemRelation, price.Currency, price.Amount,
                price.PriceUnit, price.UnitId, price.InventDimId, price.FromDate, price.ToDate));
        }

        prepared = new(new(ledger.RecId, ledger.ChartOfAccounts, ledger.FiscalCalendar, ledger.AccountingCurrency,
            dim.InventSiteId, dim.InventLocationId, dim.InventDimId, bank.RecId, bank.AccountId,
            invoiceJournal.JournalName, paymentJournal.JournalName, "ARV2", "APV2"),
            customerReferences.AsReadOnly(), priceReferences.AsReadOnly())
        {
            AdditionalMasters = new(
                (await c.RequireAsync<CompanyInfo>(x => x.DataArea == c.Company, ct)).RecId,
                (await c.RequireAsync<SalesParameters>(x => x.DataAreaId == c.Company, ct)).RecId,
                (await c.RequireAsync<CustParameters>(x => x.Key == 0, ct)).RecId,
                (await c.RequireAsync<DimensionHierarchy>(x => x.Name == "ARV2", ct)).RecId,
                (await c.RequireAsync<CustLedgerAccounts>(x => x.PostingProfile == "ARV2"
                    && x.AccountCode == AccountCode.Group && x.Num == "ARV2", ct)).RecId,
                (await c.RequireAsync<CashDisc>(x => x.CashDiscCode == "ARV2-2", ct)).RecId,
                (await c.RequireAsync<TaxLedgerAccountGroup>(x => x.TaxAccountGroup == "ARV2", ct)).RecId)
        };
    }
}
