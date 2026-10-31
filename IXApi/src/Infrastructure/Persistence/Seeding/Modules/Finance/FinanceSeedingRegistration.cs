using IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Modules.Finance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Registrations
{
    public static class FinanceSeedingRegistration
    {
        public static IServiceCollection AddFinanceSeeding(this IServiceCollection services, IConfiguration? configuration = null)
        {
            // Core Foundation & Reference Seeders
            services.AddScoped<IModuleSeeder, CompanyInfoSeeder>();
            services.AddScoped<IModuleSeeder, LogisticsAddressSeeder>();
            services.AddScoped<IModuleSeeder, FiscalCalendarSeeder>();
            services.AddScoped<IModuleSeeder, FinancialDimensionSeeder>();
            services.AddScoped<IModuleSeeder, CurrencyAndExchangeRateSeeder>();
            services.AddScoped<IModuleSeeder, UnitOfMeasureSeeder>();
            services.AddScoped<IModuleSeeder, InventItemGroupSeeder>();
            services.AddScoped<IModuleSeeder, CustGroupSeeder>();
            services.AddScoped<IModuleSeeder, SalesDeliverySetupSeeder>();

            // Ledger, AR, AP, and Inventory Setup
            services.AddScoped<IModuleSeeder, MainAccountSeeder>();
            services.AddScoped<IModuleSeeder, CustPaymModeSeeder>();
            services.AddScoped<IModuleSeeder, SalesPoolSeeder>();
            services.AddScoped<IModuleSeeder, MarkupTableSeeder>();
            services.AddScoped<IModuleSeeder, PriceDiscSeeder>();
            services.AddScoped<IModuleSeeder, InventorySiteWarehouseSeeder>();
            services.AddScoped<IModuleSeeder, VendSeeder>();
            services.AddScoped<IModuleSeeder, TaxSeeder>();
            services.AddScoped<IModuleSeeder, InventorySeeder>();
            services.AddScoped<IModuleSeeder, CustLedgerSeeder>();
            services.AddScoped<IModuleSeeder, OrganizationStructureSeeder>();

            // Demo Seeders
            services.AddScoped<IModuleSeeder, LedgerExampleSeeder>();
            services.AddScoped<IModuleSeeder, FinanceOperationalExamplesSeeder>();
            services.AddScoped<IModuleSeeder, BusinessSetupExamplesSeeder>();
            services.AddScoped<IModuleSeeder, PurchaseScenarioSeeder>();
            services.AddScoped<IModuleSeeder, PurchasePartialInvoiceSeeder>();
            services.AddScoped<IModuleSeeder, PurchaseVatInvoiceSeeder>();
            services.AddScoped<IModuleSeeder, SalesScenarioSeeder>();
            services.AddScoped<IModuleSeeder, CommercialScenarioSeeder>();
            services.AddScoped<IModuleSeeder, SalesCompletionScenarioSeeder>();
            services.AddScoped<IModuleSeeder, SalesAdvancedScenarioSeeder>();
            services.AddScoped<IModuleSeeder, InventoryScenarioSeeder>();
            services.AddScoped<IModuleSeeder, InventoryCompletionScenarioSeeder>();
            services.AddScoped<IModuleSeeder, InventoryFullCycleSeeder>();
            services.AddScoped<IModuleSeeder, DraftFinanceExamplesSeeder>();
            services.AddScoped<IModuleSeeder, HcmExampleSeeder>();
            services.AddScoped<IModuleSeeder, OrganizationDemoSeeder>();
            services.AddScoped<IModuleSeeder, SampleCustomerVendorSeeder>();
            services.AddScoped<IModuleSeeder, SampleProductSeeder>();

            // Legacy External DB Imports
            services.AddScoped<IModuleSeeder>(sp =>
            {
                var connStr = configuration?.GetConnectionString("SeedDbConnString")
                    ?? sp.GetService<IConfiguration>()?.GetConnectionString("SeedDbConnString");
                return new OthersDBOrganizationEmployeeSeeder(connStr);
            });

            return services;
        }
    }
}
