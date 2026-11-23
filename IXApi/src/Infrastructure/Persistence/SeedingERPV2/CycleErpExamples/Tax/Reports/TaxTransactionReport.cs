using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Reports;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax.Reports;

/// <summary>
/// Both directions of the tax cycle, each produced by the module that owns the document: input tax
/// from the vendor journal examples, output tax from the free text invoice service.
/// </summary>
public sealed record TaxTransactionReport(string Company, int FiscalYear,
    TaxPreparationReport Preparation,
    FoundationTransactionReport InputTax,
    AccountsReceivableTransactionReport OutputTax)
{
    /// <summary>The zero-rated (03) and exempt (04) free text invoices.</summary>
    public IReadOnlyList<string> VatTreatmentInvoices { get; init; } = [];
    public string Scope => "TaxTransactions";
    public bool TransactionsExecuted => true;
}
