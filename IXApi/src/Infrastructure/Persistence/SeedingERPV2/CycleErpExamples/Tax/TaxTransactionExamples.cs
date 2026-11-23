using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Foundation.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax.Data;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.Tax;

/// <summary>
/// Exercise both tax directions through the services that own the documents. Input tax comes from the
/// vendor journal examples and their accounting associations; output tax comes from the free text
/// invoice, which posts through its own service and moves no inventory. Scenarios 03 and 04 post the
/// zero-rated and exempt treatments the same way, through the item sales tax groups that hold those codes.
/// Nothing is inserted here.
/// </summary>
public sealed class TaxTransactionExamples(
    ApplicationDbContext db,
    TaxCycleSeeder preparation,
    FoundationTransactionExamples inputTax,
    AccountsReceivableTransactionExamples outputTax,
    IFreeTextInvoiceService freeText)
{
    /// <summary>The output-tax scenario: a free text invoice whose single line carries the demo tax group.</summary>
    private static readonly string[] OutputTaxScenarioIds = ["04"];

    /// <summary>Net amount of each VAT-treatment invoice (2 x 200, as in VAT-TREATMENTS.md).</summary>
    public const decimal TreatmentNet = 400m;

    public async Task<TaxTransactionReport> RunAsync(TaxExampleOptions options, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var prepared = await preparation.PrepareAsync(options, ct);
        // Each step runs its own seed runner, which refuses a scope that still tracks entities.
        db.ChangeTracker.Clear();
        var incoming = await inputTax.RunAsync(new FoundationExampleOptions(options.FiscalYear, options.Actor), ct);
        db.ChangeTracker.Clear();
        var outgoing = await outputTax.RunAsync(new AccountsReceivableTransactionOptions(options.FiscalYear, options.Actor, OutputTaxScenarioIds), ct);
        db.ChangeTracker.Clear();
        var zeroRated = await TreatmentInvoiceAsync(options.FiscalYear, "03", TaxSeedCatalog.ZeroItemGroup, ct);
        var exempt = await TreatmentInvoiceAsync(options.FiscalYear, "04", TaxSeedCatalog.ExemptItemGroup, ct);
        return new(prepared.Company, options.FiscalYear, prepared, incoming, outgoing) { VatTreatmentInvoices = [zeroRated, exempt] };
    }

    /// <summary>
    /// A free text invoice to the demo customer (sales tax group ARV2) whose line's item sales tax group holds only
    /// the treatment's code, so the posted TaxTrans shows that treatment. Resolved by name on replay.
    /// </summary>
    private async Task<string> TreatmentInvoiceAsync(int year, string id, string itemTaxGroup, CancellationToken ct)
    {
        var area = db.GetDataAreaId();
        var name = $"TXV2-{year}-{id}";
        var account = $"ARV2-{area}-LOCAL";
        var draft = await db.Set<CustInvoiceTable>().SingleOrDefaultAsync(x => x.DataAreaId == area && x.InvoiceAccount == account && x.Name == name, ct);
        if (draft?.Posted == NoYes.Yes) return draft.InvoiceId;
        long recId;
        if (draft == null)
        {
            var revenue = await db.Set<DimensionAttributeValueCombination>()
                .SingleAsync(x => x.DataAreaId == area && x.MainAccountValue == GeneralLedgerSeedExample.SalesRevenue, ct);
            var created = await freeText.CreateAsync(new FreeTextInvoiceSaveDto
            {
                CustomerAccount = account, InvoiceDate = new DateTime(year, 4, 20), CurrencyCode = "SAR", TaxGroup = "ARV2", Description = name,
                Lines = [new FreeTextInvoiceLineInputDto { Description = $"VAT treatment {itemTaxGroup} (demo)", Quantity = 2m,
                    UnitPrice = TreatmentNet / 2m, LedgerDimension = revenue.RecId, TaxGroup = "ARV2", TaxItemGroup = itemTaxGroup }]
            }, ct);
            if (created.StatusCode is < 200 or > 299) throw new InvalidOperationException($"Tax {id} free text invoice: {created.Message}");
            recId = created.Invoice!.RecId;
        }
        else recId = draft.RecId;
        var posted = await freeText.PostAsync(recId, ct);
        if (posted.StatusCode is < 200 or > 299) throw new InvalidOperationException($"Tax {id} free text invoice posting: {posted.Message}");
        return posted.Invoice!.InvoiceId;
    }
}
