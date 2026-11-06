namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Reports;

/// <summary>Persisted configuration/master references, not reconciliation of posted transactions.</summary>
public sealed record AccountsReceivableReconciliationReport(
    ExampleSetupReference Configuration,
    IReadOnlyList<ExampleCustomerReference> Customers,
    IReadOnlyList<ExampleSalesPriceReference> SalesPrices)
{
    public ExampleAdditionalMasterReferences? AdditionalMasters { get; init; }
}

public sealed record ExampleAdditionalMasterReferences(long CompanyRecId, long SalesParametersRecId,
    long CustomerParametersRecId, long SalesAccountStructureRecId, long CustomerPostingProfileRecId,
    long SalesCashDiscountRecId, long OutputTaxGroupRecId);

public sealed record ExampleSetupReference(long LedgerRecId, long ChartOfAccountsRecId,
    long FiscalCalendarRecId, string AccountingCurrency, string SiteId, string WarehouseId,
    string InventDimId, long CompanyBankRecId, string CompanyBankAccountId,
    string InvoiceJournal, string PaymentJournal, string TaxGroup, string ItemTaxGroup);

public sealed record ExampleCustomerReference(long RecId, string AccountNum, long PartyRecId,
    string Name, string Currency, string PaymentTerms, string PaymentMethod,
    long AddressRecId, string CustomerGroup, string TaxGroup);

public sealed record ExampleSalesPriceReference(long RecId, string CustomerAccount,
    string ItemId, string Currency, decimal Amount, decimal PriceUnit, string Unit,
    string InventDimId, DateTime ValidFrom, DateTime ValidTo);
