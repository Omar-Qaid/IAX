namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;

/// <summary>Persisted configuration/master references, not reconciliation of posted transactions.</summary>
public sealed record AccountsPayableReconciliationReport(
    ExampleSetupReference Configuration,
    IReadOnlyList<ExampleVendorReference> Vendors,
    IReadOnlyList<ExampleProductReference> Products,
    IReadOnlyList<ExamplePurchasePriceReference> PurchasePrices)
{
    public ExampleAdditionalMasterReferences? AdditionalMasters { get; init; }
}
public sealed record ExampleAdditionalMasterReferences(long CompanyRecId, long PurchaseParametersRecId,
    long StorageDimensionGroupRecId, long TrackingDimensionGroupRecId,
    long PurchaseDefaultsRecId, long InventoryDefaultsRecId, long ChargeCodeRecId);

public sealed record ExampleSetupReference(long LedgerRecId, long ChartOfAccountsRecId,
    long FiscalCalendarRecId, string AccountingCurrency, string SiteId, string WarehouseId,
    string InventDimId, long CompanyBankRecId, string CompanyBankAccountId,
    string InvoiceJournal, string PaymentJournal, string TaxGroup, string ItemTaxGroup);

public sealed record ExampleVendorReference(long RecId, string AccountNum, long PartyRecId,
    string Name, string Currency, string PaymentTerms, string PaymentMethod,
    long AddressRecId, long BankRecId, string DefaultBankAccount);

public sealed record ExampleProductReference(long RecId, string ItemId, long ProductRecId,
    string Name, string ProductType, string Unit, string ModelGroup, string ItemGroup,
    string ItemTaxGroup, string PrimaryVendor);

public sealed record ExamplePurchasePriceReference(long RecId, string VendorAccount,
    string ItemId, string Currency, decimal Amount, decimal PriceUnit, string Unit,
    string InventDimId, DateTime ValidFrom, DateTime ValidTo);
