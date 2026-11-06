using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Currency;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;

/// <summary>
/// Validate the sales example's configured references before committing any seed data. Whatever the
/// purchase example owns is validated by its own phase-one validator; this one covers the receivable
/// setup, the customers and the sales quotations.
/// </summary>
public sealed class AccountsReceivablePhaseOneValidator : ISeedValidator
{
    public async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        static void Check(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException("AR phase-one integrity: " + message);
        }
        var date = new DateTime(c.FiscalYear, 4, 15);
        var ledger = await c.RequireAsync<Ledger>(x => x.Name == "APV2", ct);
        var structures = new AccountStructureValidator(c.Db);
        async Task<MainAccount> Dimension(long recId)
        {
            var combination = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.RecId == recId, ct);
            var account = await c.RequireAsync<MainAccount>(x => x.RecId == combination.MainAccount, ct);
            Check(account.LedgerChartOfAccounts == ledger.ChartOfAccounts && combination.MainAccountValue == account.MainAccountId,
                "posting account is outside the selected chart or its combination is inconsistent.");
            var error = await structures.ValidateAsync(c.Company, combination, ct);
            Check(error == null, error ?? "Invalid account structure.");
            return account;
        }

        // The sales structure is a second structure on the same ledger; its ranges must stay disjoint
        // from the purchase one, which is what lets both be assigned at once.
        var structure = await c.RequireAsync<DimensionHierarchy>(x => x.Name == "ARV2", ct);
        Check(structure.IsDraft == NoYes.No, "the sales account structure is still a draft.");
        var assigned = await c.Rows<LedgerStructure>().AnyAsync(x => x.Ledger == ledger.RecId && x.DimensionHierarchy == structure.RecId, ct);
        Check(assigned, "the sales account structure is not assigned to the ledger.");
        foreach (var code in GeneralLedgerSeedExample.AccountsReceivableAccounts)
        {
            var combination = await c.RequireAsync<DimensionAttributeValueCombination>(x => x.MainAccountValue == code, ct);
            Check(combination.AccountStructure == structure.RecId, $"main account {code} does not belong to the sales account structure.");
            await Dimension(combination.RecId);
        }

        var bank = await c.RequireAsync<BankAccountTable>(x => x.AccountId == "APV2-BANK", ct);
        Check(bank.BankAccountStatus == BankAccountStatus.Active && bank.ActiveFrom <= date && bank.ActiveTo >= date,
            "company bank is unavailable on the example date.");
        var mode = await c.RequireAsync<CustPaymModeTable>(x => x.PaymMode == "ARV2", ct);
        Check(mode.AccountType == LedgerJournalACType.Bank && mode.PaymentLedgerDimension == bank.LedgerDimension,
            "collection method/bank ledger reference differs.");
        var paymentJournal = await c.RequireAsync<LedgerJournalName>(x => x.JournalName == mode.PaymJournalNameId, ct);
        Check(paymentJournal.JournalType == LedgerJournalType.CustPayment, "the collection journal is not a customer payment journal.");
        foreach (var code in new[] { "ARV2-INV", paymentJournal.JournalName })
        {
            var name = await c.RequireAsync<LedgerJournalName>(x => x.JournalName == code, ct);
            var sequence = await c.RequireAsync<SysNumberSequence>(x => x.RecId == name.NumberSequenceTable, ct);
            Check(sequence.InUse == 1 && sequence.Blocked == 0 && sequence.Manual == 0 && sequence.NoIncrement == 0,
                "journal voucher sequence cannot allocate automatically.");
            Check(SysNumberSequenceService.FormatCode(sequence, 1) != SysNumberSequenceService.FormatCode(sequence, 2),
                "voucher format does not include its counter.");
        }
        foreach (var key in new[] { "SalesTable", "CustConfirmJour", "CustPackingSlipJour", "CustInvoiceTable", "CustInvoiceJour" })
        {
            var sequence = await c.RequireAsync<SysNumberSequence>(x => x.NumberSequence == key, ct);
            Check(sequence.InUse == 1 && sequence.Blocked == 0 && (sequence.NextRec ?? 0) >= 1 && sequence.NextRec <= sequence.Highest,
                $"the {key} sequence cannot allocate the next document number.");
        }

        var parameters = await c.RequireAsync<CustParameters>(x => x.Key == 0, ct);
        var sales = await c.RequireAsync<SalesParameters>(x => x.DataAreaId == c.Company, ct);
        Check(sales.SalesType == SalesType.Sales, "the default sales order type differs.");
        var discount = await c.RequireAsync<CashDisc>(x => x.CashDiscCode == "ARV2-2", ct);
        Check((await Dimension(discount.CashDiscLedgerDimension)).Type == MainAccountType.Expense,
            "the sales cash discount account is not an expense.");
        var profileHeader = await c.RequireAsync<CustLedger>(x => x.PostingProfile == parameters.CustPostingProfile, ct);

        foreach (var suffix in new[] { "LOCAL", "USD" })
        {
            var account = $"ARV2-{c.Company}-{suffix}";
            var customer = await c.RequireAsync<CustTable>(x => x.AccountNum == account, ct);
            var group = await c.RequireAsync<CustGroup>(x => x.CustGroupId == customer.CustGroupId, ct);
            var term = await c.RequireAsync<PaymTerm>(x => x.PaymTermId == customer.PaymTermId, ct);
            Check(term.PaymTermId == group.PaymTermId && term.NumOfDays == 30 && term.PaymMethod == PaymMethod.Net,
                "customer/group payment terms differ from the NET30 quotation.");
            await c.RequireAsync<Currency>(x => x.CurrencyCode == customer.CurrencyCode, ct);
            Check(customer.Blocked == CustVendorBlocked.No, "the example customer is blocked.");
            var profile = await c.RequireAsync<CustLedgerAccounts>(x => x.PostingProfile == profileHeader.PostingProfile
                && x.AccountCode == AccountCode.Group && x.Num == group.CustGroupId, ct);
            Check((await Dimension(profile.SummaryLedgerDimension)).Type == MainAccountType.Asset, "customer control account is not an asset.");
            Check((await Dimension(profile.ClearingLedgerDimension)).Type == MainAccountType.Liability, "customer prepayment account is not a liability.");
            var party = await c.RequireAsync<DirPartyTable>(x => x.RecId == customer.Party, ct);
            Check(party.Name == CustomerSeedExample.Name(suffix), "customer party identity differs from the example customer.");
            var location = await c.RequireAsync<LogisticsLocation>(x => x.RecId == party.PrimaryAddressLocation, ct);
            await c.RequireAsync<DirPartyLocation>(x => x.Party == party.RecId && x.Location == location.RecId && x.IsPrimary == NoYes.Yes, ct);
            var address = await c.RequireAsync<LogisticsPostalAddress>(x => x.Location == location.RecId && x.ValidFrom <= date && x.ValidTo >= date, ct);
            var zip = await c.RequireAsync<LogisticsAddressZipCode>(x => x.RecId == address.ZipCodeRecId, ct);
            Check(zip.ZipCode == address.ZipCode && zip.CityRecId == address.CityRecId && zip.District == address.District
                && zip.CountryRegionId == address.CountryRegionId && zip.State == address.State && zip.County == address.County,
                "postal address/geography references differ.");
            var email = await c.RequireAsync<LogisticsElectronicAddress>(x => x.RecId == party.PrimaryContactEmail, ct);
            Check(email.Location == location.RecId && email.Type == ElectronicAddressType.Email && email.Locator == CustomerSeedExample.Email(suffix),
                "customer primary email is linked to a different location or customer.");
            Check(customer.InventSiteId == "APV2-SITE" && customer.InventLocationId == "APV2-WH", "customer shipping site/warehouse differs.");
        }

        var rates = await new EffectiveExchangeRateLookup(c.Db).FindAsync(c.Company, ledger.DefaultExchangeRateType, "USD", ledger.AccountingCurrency, date, ct);
        Check(rates.Error == null && rates.Rate != null, rates.Error ?? "No effective USD exchange rate.");

        // Sales posting for the shared item group: delivery, invoice issue, cost and revenue.
        foreach (var type in new[] { InventAccountType.SalesPackingSlip, InventAccountType.SalesPackingSlipOffsetAccount,
            InventAccountType.SalesIssue, InventAccountType.SalesConsump, InventAccountType.SalesRevenue, InventAccountType.SalesDisc })
        {
            var posting = await c.RequireAsync<InventPosting>(x => x.InventAccountType == type && x.ItemCode == TableGroupAll.Group
                && x.ItemRelation == "APV2" && x.CustVendCode == TableGroupAll.All, ct);
            await Dimension(posting.LedgerDimension);
        }
        var revenue = await c.RequireAsync<InventPosting>(x => x.InventAccountType == InventAccountType.SalesRevenue
            && x.ItemCode == TableGroupAll.Group && x.ItemRelation == "APV2" && x.CustVendCode == TableGroupAll.All, ct);
        Check((await Dimension(revenue.LedgerDimension)).Type == MainAccountType.Revenue, "the sales revenue account is not a revenue account.");

        foreach (var (suffix, itemId, currency, amount) in new[] {
            ("LOCAL", InventorySeedExample.StockItem, "SAR", CustomerSeedExample.PaperPriceSar),
            ("LOCAL", InventorySeedExample.ServiceItem, "SAR", CustomerSeedExample.MaintenancePriceSar),
            ("USD", InventorySeedExample.StockItem, "USD", CustomerSeedExample.PaperPriceUsd) })
        {
            var customerId = $"ARV2-{c.Company}-{suffix}";
            var customer = await c.RequireAsync<CustTable>(x => x.AccountNum == customerId, ct);
            var salesSetup = await c.RequireAsync<InventTableModule>(x => x.ItemId == itemId && x.ModuleType == ModuleInventPurchSales.Sales, ct);
            var price = await c.RequireAsync<PriceDiscTable>(x => x.AccountRelation == customerId && x.ItemRelation == itemId
                && x.Module == ModuleInventCustVend.Cust && x.Relation == PriceType.PriceSales && x.FromDate <= date && x.ToDate >= date, ct);
            Check(price.Currency == customer.CurrencyCode && price.Currency == currency && price.UnitId == salesSetup.UnitId
                && price.InventDimId == "APV2-DIM" && price.Amount == amount && price.PriceUnit == 1m,
                "customer price currency/unit/amount differs from the sales quotation.");
            // A sale must not be quoted below what the same item was bought for in the same currency.
            if (currency == "SAR")
                Check(price.Amount > InventorySeedExample.PurchasePrice(itemId), "the sales quotation is not above its purchase cost.");
            await c.RequireAsync<TaxGroupHeading>(x => x.TaxGroup == customer.TaxGroupId, ct);
            var membership = await c.RequireAsync<TaxGroupData>(x => x.TaxGroup == customer.TaxGroupId && x.TaxCode == "ARV2-15", ct);
            await c.RequireAsync<TaxOnItem>(x => x.TaxItemGroup == salesSetup.TaxItemGroupId && x.TaxCode == membership.TaxCode, ct);
            var code = await c.RequireAsync<TaxTable>(x => x.TaxCode == membership.TaxCode, ct);
            var taxRate = await c.RequireAsync<TaxData>(x => x.TaxCode == code.TaxCode && x.TaxFromDate <= date && x.TaxToDate >= date, ct);
            Check(membership.ExemptTax == NoYes.No && taxRate.TaxValue == 15m && taxRate.VatExemptPct == 0m,
                "configured example tax intersection/rate differs from 15 percent output tax.");
            var taxPosting = await c.RequireAsync<TaxLedgerAccountGroup>(x => x.TaxAccountGroup == code.TaxAccountGroup, ct);
            Check(taxPosting.TaxOutgoingLedgerDimension is > 0, "output tax ledger reference is missing.");
            Check((await Dimension(taxPosting.TaxOutgoingLedgerDimension!.Value)).Type == MainAccountType.Liability,
                "the output tax account is not a liability.");
        }
    }
}
