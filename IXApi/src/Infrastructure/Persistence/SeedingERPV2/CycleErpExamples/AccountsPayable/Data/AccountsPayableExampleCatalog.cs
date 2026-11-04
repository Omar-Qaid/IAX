using System.Text.Json;
using System.Text.Json.Serialization;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable.Data;

internal sealed record VendorDefinition(string Suffix, string Name, string Currency,
    string PaymentTerms, string PaymentMethod, string TaxGroup, string BankAccount);
internal sealed record ProductDefinition(string Suffix, string ItemId, string Name,
    string ProductType, string Unit, string ModelGroup, string ItemGroup, string ItemTaxGroup);
internal sealed record PurchasePriceDefinition(string VendorSuffix, string ItemId, string Currency,
    decimal Amount, decimal PriceUnit, string Unit, string InventDimId);
internal sealed record PreparationDefinition(string Scope, bool TransactionScenariosEnabled,
    ConfigurationPreparationStep[] Steps);

/// <summary>Read-only descriptions of module-owned demo records; these files never insert rows.</summary>
internal sealed class AccountsPayableExampleCatalog
{
    public VendorDefinition[] Vendors { get; } = Read<VendorDefinition[]>("vendors.json");
    public ProductDefinition[] Products { get; } = Read<ProductDefinition[]>("products.json");
    public PurchasePriceDefinition[] Prices { get; } = Read<PurchasePriceDefinition[]>("purchase-prices.json");
    public PreparationDefinition Preparation { get; } = Read<PreparationDefinition>("scenarios.json");

    public void Validate()
    {
        static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException("AP master-data manifest: " + message);
        }
        Require(Preparation.Scope == ExampleIdentifiers.Scope && !Preparation.TransactionScenariosEnabled,
            "only configuration and master data are supported.");
        Require(Preparation.Steps is { Length: > 0 } && Preparation.Steps.Select(x => x.Id).Distinct().Count() == Preparation.Steps.Length
            && Preparation.Steps.Select(x => x.Order).SequenceEqual(Enumerable.Range(1, Preparation.Steps.Length))
            && Preparation.Steps.All(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Description)), "preparation steps are invalid.");
        Require(Vendors.Length == 2 && Vendors.Select(x => x.Suffix).Order().SequenceEqual(new[] { "LOCAL", "USD" }), "supplier keys differ.");
        foreach (var vendor in Vendors)
            Require(vendor.Name == VendorSeedExample.Name(vendor.Suffix) && vendor.Currency == (vendor.Suffix == "LOCAL" ? "SAR" : "USD")
                && vendor.PaymentTerms == "APV2-N30" && vendor.PaymentMethod == "APV2" && vendor.TaxGroup == "APV2" && vendor.BankAccount == "APV2", "supplier definition differs from its owning seeder.");
        Require(Products.Length == 2 && Products.Select(x => x.ItemId).Order().SequenceEqual(new[] { ExampleIdentifiers.ServiceItem, ExampleIdentifiers.StockItem }.Order()), "item keys differ.");
        foreach (var product in Products)
            Require(product.Suffix is "STOCK" or "SVC" && product.ItemId == "APV2-" + product.Suffix && product.Name == InventorySeedExample.Name(product.Suffix)
                && product.ProductType == (product.Suffix == "STOCK" ? "Item" : "Service") && product.Unit == "EA" && product.ItemTaxGroup == "APV2" && product.ItemGroup == "APV2"
                && product.ModelGroup == (product.Suffix == "STOCK" ? "APV2" : "APV2-SVC"), "product definition differs from its owning seeder.");
        var expected = new[] { ("LOCAL", ExampleIdentifiers.StockItem, "SAR", InventorySeedExample.PaperPriceSar),
            ("LOCAL", ExampleIdentifiers.ServiceItem, "SAR", InventorySeedExample.MaintenancePriceSar),
            ("USD", ExampleIdentifiers.StockItem, "USD", InventorySeedExample.PaperPriceUsd) };
        Require(Prices.Length == expected.Length, "purchase quote count differs.");
        foreach (var (vendor, item, currency, amount) in expected)
            Require(Prices.Count(x => x.VendorSuffix == vendor && x.ItemId == item && x.Currency == currency && x.Amount == amount
                && x.PriceUnit == 1m && x.Unit == "EA" && x.InventDimId == "APV2-DIM") == 1, "purchase quote differs from its owning seeder.");
    }

    private static T Read<T>(string file)
    {
        using var stream = typeof(AccountsPayableExampleCatalog).Assembly.GetManifestResourceStream("SeedingERPV2.AccountsPayable." + file)
            ?? throw new InvalidOperationException("Missing embedded AP master-data manifest: " + file);
        return JsonSerializer.Deserialize<T>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        }) ?? throw new InvalidOperationException("Empty AP master-data manifest: " + file);
    }
}
