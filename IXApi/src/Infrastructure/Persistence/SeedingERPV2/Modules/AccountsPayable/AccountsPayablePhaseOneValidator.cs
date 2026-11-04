using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Currency;
using IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;
using IAX.IXApi.Modules.Finance.Inventory.InventTable;
using IAX.IXApi.Modules.Finance.Inventory.ProductCategories.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;

/// <summary>Validate the example's configured references before committing any seed data.</summary>
public sealed class AccountsPayablePhaseOneValidator : ISeedValidator
{
    public async Task ValidateAsync(SeedContext c, CancellationToken ct)
    {
        static void Check(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException("AP phase-one integrity: " + message);
        }
        await AdditionalMasterDataIntegrity.ValidateAsync(c, ct);
        var date = new DateTime(c.FiscalYear, 4, 15);
        var ledger = await c.RequireAsync<Ledger>(x => x.Name == "APV2", ct);
        var company = await c.RequireAsync<CompanyInfo>(x => x.DataArea == c.Company, ct);
        Check(ledger.PrimaryForLegalEntity == company.RecId, "ledger/legal entity reference differs.");
        await c.RequireAsync<LedgerChartOfAccounts>(x => x.RecId == ledger.ChartOfAccounts, ct);
        await c.RequireAsync<Currency>(x => x.CurrencyCode == ledger.AccountingCurrency, ct);
        var period = await c.RequireAsync<FiscalCalendarPeriod>(x => x.FiscalCalendar == ledger.FiscalCalendar
            && x.Type == FiscalPeriodType.Operating && x.StartDate <= date && x.EndDate >= date, ct);
        var ledgerPeriod = await c.RequireAsync<LedgerFiscalCalendarPeriod>(x => x.Ledger == ledger.RecId && x.FiscalCalendarPeriod == period.RecId, ct);
        var ledgerYear = await c.RequireAsync<LedgerFiscalCalendarYear>(x => x.Ledger == ledger.RecId && x.FiscalCalendarYear == period.FiscalCalendarYear, ct);
        Check(ledgerPeriod.Status == FiscalPeriodStatus.Open && ledgerYear.Status == FiscalPeriodStatus.Open, "example period/year is closed.");

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
        var bank = await c.RequireAsync<BankAccountTable>(x => x.AccountId == "APV2-BANK", ct);
        Check(bank.BankAccountStatus == BankAccountStatus.Active && bank.ActiveFrom <= date && bank.ActiveTo >= date,
            "company bank is unavailable on the example date.");
        Check((await Dimension(bank.LedgerDimension)).Type == MainAccountType.Asset, "company bank account must reference the example bank asset.");
        var mode = await c.RequireAsync<VendPaymModeTable>(x => x.PaymMode == "APV2", ct);
        Check(mode.AccountType == LedgerJournalACType.Bank && mode.PaymentLedgerDimension == bank.LedgerDimension,
            "payment method/bank ledger reference differs.");
        var paymentJournal = await c.RequireAsync<LedgerJournalName>(x => x.JournalName == mode.PaymJournalNameId, ct);
        Check(paymentJournal.JournalType == LedgerJournalType.VendDisbursement && paymentJournal.JournalName == bank.VendPaymentJournalName,
            "payment journal is not the configured vendor payment journal.");
        foreach (var code in new[] { "APV2-INV", paymentJournal.JournalName })
        {
            var name = await c.RequireAsync<LedgerJournalName>(x => x.JournalName == code, ct);
            var sequence = await c.RequireAsync<SysNumberSequence>(x => x.RecId == name.NumberSequenceTable, ct);
            Check(sequence.InUse == 1 && sequence.Blocked == 0 && sequence.Manual == 0 && sequence.NoIncrement == 0,
                "journal voucher sequence cannot allocate automatically.");
            Check(SysNumberSequenceService.FormatCode(sequence, 1) != SysNumberSequenceService.FormatCode(sequence, 2),
                "voucher format does not include its counter.");
        }
        var rates = await new EffectiveExchangeRateLookup(c.Db).FindAsync(c.Company, ledger.DefaultExchangeRateType, "USD", ledger.AccountingCurrency, date, ct);
        Check(rates.Error == null && rates.Rate != null, rates.Error ?? "No effective USD exchange rate.");
        var parameters = await c.RequireAsync<VendParameters>(x => x.Key == 0, ct);
        var discount = await c.RequireAsync<CashDisc>(x => x.CashDiscCode == "APV2-2", ct);
        await Dimension(discount.CashDiscLedgerDimension);
        foreach (var suffix in new[] { "LOCAL", "USD" })
        {
            var account = $"APV2-{c.Company}-{suffix}";
            var vendor = await c.RequireAsync<VendTable>(x => x.AccountNum == account, ct);
            var group = await c.RequireAsync<VendGroup>(x => x.VendGroupCode == vendor.VendGroup, ct);
            var term = await c.RequireAsync<PaymTerm>(x => x.PaymTermId == vendor.PaymTermId, ct);
            Check(term.PaymTermId == group.PaymTermId && term.NumOfDays == 30 && term.PaymMethod == PaymMethod.Net,
                "vendor/group payment terms differ from the NET30 quotation.");
            await c.RequireAsync<Currency>(x => x.CurrencyCode == vendor.Currency, ct);
            var profile = await c.RequireAsync<VendLedgerAccounts>(x => x.PostingProfile == parameters.VendPostingProfile
                && x.AccountCode == AccountCode.Group && x.Num == group.VendGroupCode, ct);
            Check((await Dimension(profile.SummaryLedgerDimension)).Type == MainAccountType.Liability, "vendor control account is not a liability.");
            Check((await Dimension(profile.ClearingLedgerDimension)).Type == MainAccountType.Asset, "supplier advance account is not an asset.");
            var party = await c.RequireAsync<DirPartyTable>(x => x.RecId == vendor.Party, ct);
            Check(party.Name == VendorSeedExample.Name(suffix), "vendor party identity differs from the example supplier.");
            var location = await c.RequireAsync<LogisticsLocation>(x => x.RecId == party.PrimaryAddressLocation, ct);
            await c.RequireAsync<DirPartyLocation>(x => x.Party == party.RecId && x.Location == location.RecId && x.IsPrimary == NoYes.Yes, ct);
            var address = await c.RequireAsync<LogisticsPostalAddress>(x => x.Location == location.RecId && x.ValidFrom <= date && x.ValidTo >= date, ct);
            var zip = await c.RequireAsync<LogisticsAddressZipCode>(x => x.RecId == address.ZipCodeRecId, ct);
            Check(zip.ZipCode == address.ZipCode && zip.CityRecId == address.CityRecId && zip.District == address.District
                && zip.CountryRegionId == address.CountryRegionId && zip.State == address.State && zip.County == address.County,
                "postal address/geography references differ.");
            var email = await c.RequireAsync<LogisticsElectronicAddress>(x => x.RecId == party.PrimaryContactEmail, ct);
            Check(email.Location == location.RecId && email.Type == ElectronicAddressType.Email && email.Locator == VendorSeedExample.Email(suffix),
                "vendor primary email is linked to a different location or supplier.");
            var vendorBank = await c.RequireAsync<VendBankAccount>(x => x.VendAccount == vendor.AccountNum && x.AccountID == vendor.BankAccount, ct);
            Check(vendorBank.CurrencyCode == vendor.Currency && vendorBank.AccountNum == VendorSeedExample.BankNumber(suffix),
                "vendor bank/currency reference differs.");
        }

        var warehouse = await c.RequireAsync<InventLocation>(x => x.InventLocationId == "APV2-WH", ct);
        await c.RequireAsync<InventSite>(x => x.SiteId == warehouse.InventSiteId, ct);
        var dimension = await c.RequireAsync<InventDim>(x => x.InventDimId == "APV2-DIM", ct);
        Check(dimension.InventLocationId == warehouse.InventLocationId && dimension.InventSiteId == warehouse.InventSiteId,
            "inventory dimension warehouse/site differs.");
        var inventory = await c.RequireAsync<InventParameters>(x => x.DataAreaId == c.Company, ct);
        Check(inventory.MultiSiteDefaultInventLocationId == warehouse.InventLocationId && inventory.MultiSiteDefaultInventSiteId == warehouse.InventSiteId,
            "inventory default warehouse/site differs.");
        foreach (var itemId in new[] { InventorySeedExample.StockItem, InventorySeedExample.ServiceItem })
        {
            var item = await c.RequireAsync<InventTable>(x => x.ItemId == itemId, ct);
            var product = await c.RequireAsync<EcoResProduct>(x => x.RecId == item.Product, ct);
            var suffix = itemId == InventorySeedExample.StockItem ? "STOCK" : "SVC";
            Check(product.ProductType == item.ItemType && product.DisplayProductNumber == $"APV2-{c.Company}-{suffix}", "released item/product identity or type differs.");
            await c.RequireAsync<VendTable>(x => x.AccountNum == item.PrimaryVendorID, ct);
            var assignment = await c.RequireAsync<InventModelGroupItem>(x => x.ItemId == itemId && x.ItemDataAreaId == c.Company, ct);
            Check(assignment.ModelGroupDataAreaId == c.Company, "item model group belongs to another company.");
            var model = await c.RequireAsync<InventModelGroup>(x => x.ModelGroupId == assignment.ModelGroupId, ct);
            Check((model.StockedProduct == NoYes.Yes) == (item.ItemType == ItemType.Item), "stocked/service model group differs.");
            var itemGroup = await c.RequireAsync<InventItemGroupItem>(x => x.ItemId == itemId && x.ItemDataAreaId == c.Company, ct);
            Check(itemGroup.ItemGroupDataAreaId == c.Company, "item group belongs to another company.");
            await c.RequireAsync<InventItemGroup>(x => x.ItemGroupId == itemGroup.ItemGroupId, ct);
            foreach (var module in new[] { ModuleInventPurchSales.Inventory, ModuleInventPurchSales.Purchase, ModuleInventPurchSales.Sales })
            {
                var setup = await c.RequireAsync<InventTableModule>(x => x.ItemId == itemId && x.ModuleType == module, ct);
                await c.RequireAsync<UnitOfMeasure>(x => x.Symbol == setup.UnitId, ct);
                await c.RequireAsync<TaxItemGroupHeading>(x => x.TaxItemGroup == setup.TaxItemGroupId, ct);
                Check(setup.PriceUnit > 0 && setup.Price == InventorySeedExample.PurchasePrice(itemId), "item module price/unit differs from the example quotation.");
            }
        }
        var hierarchyName = $"APV2-{c.Company}";
        var hierarchy = await c.RequireAsync<EcoResCategoryHierarchy>(x => x.Name == hierarchyName, ct);
        var category = await c.RequireAsync<EcoResCategory>(x => x.Name == hierarchyName && x.CategoryHierarchy == hierarchy.RecId, ct);
        var service = await c.RequireAsync<InventTable>(x => x.ItemId == InventorySeedExample.ServiceItem, ct);
        var categoryLink = await c.RequireAsync<EcoResProductCategory>(x => x.Product == service.Product && x.Category == category.RecId, ct);
        Check(categoryLink.CategoryHierarchy == hierarchy.RecId, "service product category belongs to another hierarchy.");
        foreach (var type in new[] { InventAccountType.PurchPackingSlip, InventAccountType.PurchPackingSlipOffsetAccount,
            InventAccountType.PurchReceipt, InventAccountType.PurchConsump, InventAccountType.PurchDisc })
        {
            var posting = await c.RequireAsync<InventPosting>(x => x.InventAccountType == type && x.ItemCode == TableGroupAll.Group
                && x.ItemRelation == "APV2" && x.CustVendCode == TableGroupAll.All, ct);
            await Dimension(posting.LedgerDimension);
        }

        foreach (var (suffix, itemId, currency, amount) in new[] {
            ("LOCAL", InventorySeedExample.StockItem, "SAR", InventorySeedExample.PaperPriceSar),
            ("LOCAL", InventorySeedExample.ServiceItem, "SAR", InventorySeedExample.MaintenancePriceSar),
            ("USD", InventorySeedExample.StockItem, "USD", InventorySeedExample.PaperPriceUsd) })
        {
            var vendorId = $"APV2-{c.Company}-{suffix}";
            var vendor = await c.RequireAsync<VendTable>(x => x.AccountNum == vendorId, ct);
            var purchase = await c.RequireAsync<InventTableModule>(x => x.ItemId == itemId && x.ModuleType == ModuleInventPurchSales.Purchase, ct);
            var price = await c.RequireAsync<PriceDiscTable>(x => x.AccountRelation == vendorId && x.ItemRelation == itemId
                && x.Module == ModuleInventCustVend.Vend && x.Relation == PriceType.PricePurch && x.FromDate <= date && x.ToDate >= date, ct);
            Check(price.Currency == vendor.Currency && price.Currency == currency && price.UnitId == purchase.UnitId
                && price.InventDimId == dimension.InventDimId && price.Amount == amount && price.PriceUnit == 1m, "vendor price currency/unit/amount differs from the quotation.");
            await c.RequireAsync<TaxGroupHeading>(x => x.TaxGroup == vendor.TaxGroup, ct);
            var membership = await c.RequireAsync<TaxGroupData>(x => x.TaxGroup == vendor.TaxGroup && x.TaxCode == "APV2-15", ct);
            await c.RequireAsync<TaxOnItem>(x => x.TaxItemGroup == purchase.TaxItemGroupId && x.TaxCode == membership.TaxCode, ct);
            var code = await c.RequireAsync<TaxTable>(x => x.TaxCode == membership.TaxCode, ct);
            var taxRate = await c.RequireAsync<TaxData>(x => x.TaxCode == code.TaxCode && x.TaxFromDate <= date && x.TaxToDate >= date, ct);
            Check(membership.ExemptTax == NoYes.No && taxRate.TaxValue == 15m && taxRate.VatExemptPct == 0m,
                "configured example tax intersection/rate differs from 15 percent fully recoverable input tax.");
            var taxPosting = await c.RequireAsync<TaxLedgerAccountGroup>(x => x.TaxAccountGroup == code.TaxAccountGroup, ct);
            Check(taxPosting.TaxIncomingLedgerDimension is > 0, "input tax ledger reference is missing.");
            await Dimension(taxPosting.TaxIncomingLedgerDimension!.Value);
        }
    }
}
