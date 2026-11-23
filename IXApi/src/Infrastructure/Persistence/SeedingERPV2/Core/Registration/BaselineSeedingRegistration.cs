using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Administration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Communication;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Organization;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;

/// <summary>
/// The baseline profile: the data every installation starts with, run by <see cref="BaselineSeedRunner"/> at
/// startup. Seeders are registered only as <see cref="IBaselineSeeder"/>, so the example profile, which resolves
/// <see cref="IDataSeeder"/>, never runs them.
/// </summary>
public static class BaselineSeedingRegistration
{
    public const string Settings = "Administration.Settings";
    public const string NumberSequences = "Administration.NumberSequences";

    public static IServiceCollection AddBaselineSeeding(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SeedingOptions>(options =>
        {
            configuration.GetSection("DatabaseInitialization").Bind(options);
            configuration.GetSection(SeedingOptions.SectionName).Bind(options);
        });
        services.TryAddScoped<BaselineSeedRunner>();
        services.TryAddScoped<LegacyImportSource>();

        // Administration and Organization: the installation-wide V2 seeders, reused as they are.
        Reuse<SysSettingsSeeder>(services, Settings, BaselineCurrencySeeder.SeederId);
        Reuse<SysNumberSequenceSeeder>(services, NumberSequences, BaselineCompanySeeder.SeederId);
        Reuse<SysBackgroundJobSeeder>(services, "Administration.BackgroundJobs", Settings);
        Reuse<SysNotificationTemplateSeeder>(services, "Administration.NotificationTemplates", BaselineCompanySeeder.SeederId);
        Reuse<DocuTypeSeeder>(services, "Organization.DocumentManagement", BaselineCompanySeeder.SeederId);

        // Foundation and General ledger.
        Add<BaselineCompanySeeder>(services);
        Add<BaselineCurrencySeeder>(services);
        Add<BaselineFiscalCalendarSeeder>(services);
        Add<BaselineFinancialDimensionSeeder>(services);
        Add<BaselineLogisticsSeeder>(services);
        Add<BaselineOrganizationStructureSeeder>(services);
        Add<LegacyEmployeeImportSeeder>(services);
        Add<BaselineLedgerSeeder>(services);
        Add<BaselineMainAccountSeeder>(services);
        Add<BaselineAccountStructureSeeder>(services);

        // Tax, Inventory and Accounts receivable.
        Add<BaselineTaxSeeder>(services);
        Add<BaselineUnitOfMeasureSeeder>(services);
        Add<BaselineItemGroupSeeder>(services);
        Add<BaselineSiteWarehouseSeeder>(services);
        Add<BaselineInventorySetupSeeder>(services);
        Add<BaselineReleasedProductSeeder>(services);
        Add<BaselineCustGroupSeeder>(services);
        Add<BaselineCustPostingProfileSeeder>(services);
        Add<BaselineCustPaymModeSeeder>(services);
        Add<BaselineSalesPoolSeeder>(services);
        Add<BaselinePriceDiscSeeder>(services);
        Add<BaselineDeliverySetupSeeder>(services);
        Add<BaselineChargeSeeder>(services);
        return services;
    }

    private static void Add<TSeeder>(IServiceCollection services) where TSeeder : class, IBaselineSeeder =>
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IBaselineSeeder, TSeeder>());

    private static void Reuse<TSeeder>(IServiceCollection services, string id, params string[] dependencies)
        where TSeeder : class, IDataSeeder
    {
        services.TryAddScoped<TSeeder>();
        services.AddScoped<IBaselineSeeder>(sp => new BaselineSeederAdapter<TSeeder>(sp.GetRequiredService<TSeeder>(), id, dependencies));
    }
}
