using System.Text.Json;
using System.Text.Json.Serialization;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Reports;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable.Data;

internal sealed record CustomerDefinition(string Suffix, string Name, string Currency,
    string PaymentTerms, string PaymentMethod, string TaxGroup, string CustomerGroup);
internal sealed record SalesPriceDefinition(string CustomerSuffix, string ItemId, string Currency,
    decimal Amount, decimal PriceUnit, string Unit, string InventDimId);
internal sealed record PreparationDefinition(string Scope, bool TransactionScenariosEnabled,
    ConfigurationPreparationStep[] Steps);

/// <summary>Read-only descriptions of module-owned demo records; these files never insert rows.</summary>
internal sealed class AccountsReceivableExampleCatalog
{
    public CustomerDefinition[] Customers { get; } = Read<CustomerDefinition[]>("customers.json");
    public SalesPriceDefinition[] Prices { get; } = Read<SalesPriceDefinition[]>("sales-prices.json");
    public PreparationDefinition Preparation { get; } = Read<PreparationDefinition>("scenarios.json");

    public void Validate()
    {
        static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException("AR master-data manifest: " + message);
        }
        Require(Preparation.Scope == ExampleIdentifiers.Scope && !Preparation.TransactionScenariosEnabled,
            "only configuration and master data are supported.");
        Require(Preparation.Steps is { Length: > 0 } && Preparation.Steps.Select(x => x.Id).Distinct().Count() == Preparation.Steps.Length
            && Preparation.Steps.Select(x => x.Order).SequenceEqual(Enumerable.Range(1, Preparation.Steps.Length))
            && Preparation.Steps.All(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Description)), "preparation steps are invalid.");
        Require(Customers.Length == 2 && Customers.Select(x => x.Suffix).Order().SequenceEqual(new[] { "LOCAL", "USD" }), "customer keys differ.");
        foreach (var customer in Customers)
            Require(customer.Name == CustomerSeedExample.Name(customer.Suffix)
                && customer.Currency == (customer.Suffix == "LOCAL" ? "SAR" : "USD")
                && customer.PaymentTerms == "APV2-N30" && customer.PaymentMethod == "ARV2"
                && customer.TaxGroup == "ARV2" && customer.CustomerGroup == "ARV2",
                "customer definition differs from its owning seeder.");
        var expected = new[] { ("LOCAL", ExampleIdentifiers.StockItem, "SAR", CustomerSeedExample.PaperPriceSar),
            ("LOCAL", ExampleIdentifiers.ServiceItem, "SAR", CustomerSeedExample.MaintenancePriceSar),
            ("USD", ExampleIdentifiers.StockItem, "USD", CustomerSeedExample.PaperPriceUsd) };
        Require(Prices.Length == expected.Length, "sales quotation count differs.");
        foreach (var (customer, item, currency, amount) in expected)
            Require(Prices.Count(x => x.CustomerSuffix == customer && x.ItemId == item && x.Currency == currency && x.Amount == amount
                && x.PriceUnit == 1m && x.Unit == "EA" && x.InventDimId == "APV2-DIM") == 1, "sales quotation differs from its owning seeder.");
    }

    private static T Read<T>(string file)
    {
        using var stream = typeof(AccountsReceivableExampleCatalog).Assembly.GetManifestResourceStream("SeedingERPV2.AccountsReceivable." + file)
            ?? throw new InvalidOperationException("Missing embedded AR master-data manifest: " + file);
        return JsonSerializer.Deserialize<T>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        }) ?? throw new InvalidOperationException("Empty AR master-data manifest: " + file);
    }
}
