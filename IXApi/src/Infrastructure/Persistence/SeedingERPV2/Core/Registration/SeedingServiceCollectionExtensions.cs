using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.AccountsReceivable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.CashAndBank;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Communication;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.GeneralLedger;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Inventory;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Administration;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Organization;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Tax;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Workflow;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
public static class SeedingServiceCollectionExtensions
{
    public static IServiceCollection AddFoundationPhaseOneSeeding(this IServiceCollection services)
    {
        services.AddAccountsPayablePhaseOneSeeding();
        services.AddFoundationSeeders();
        services.AddFoundationExtendedSeeders();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, FoundationIntegrityValidator>());
        return services;
    }

    /// <summary>
    /// Application settings, number sequences and the recurring jobs. Every other phase allocates its
    /// document numbers from here, so it stays independent of them.
    /// </summary>
    public static IServiceCollection AddAdministrationPhaseOneSeeding(this IServiceCollection services)
    {
        services.TryAddScoped<SeedRunner>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, SeedModelIntegrityValidator>());
        services.AddFoundationSeeders();
        services.AddAdministrationSeeders();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, AdministrationIntegrityValidator>());
        return services;
    }

    /// <summary>
    /// Notification templates. The events that raise a notification live in the other modules, so the
    /// phase seeds only the templates they render from.
    /// </summary>
    public static IServiceCollection AddCommunicationPhaseOneSeeding(this IServiceCollection services)
    {
        services.TryAddScoped<SeedRunner>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, SeedModelIntegrityValidator>());
        services.AddFoundationSeeders();
        services.AddCommunicationSeeders();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, CommunicationIntegrityValidator>());
        return services;
    }

    /// <summary>
    /// The workflow reference catalog. A process definition is assembled from it in the application, so
    /// the phase seeds the catalog and validates the definitions already stored against it.
    /// </summary>
    public static IServiceCollection AddWorkflowPhaseOneSeeding(this IServiceCollection services)
    {
        services.TryAddScoped<SeedRunner>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, SeedModelIntegrityValidator>());
        services.AddFoundationSeeders();
        services.AddWorkflowSeeders();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, WorkflowIntegrityValidator>());
        return services;
    }

    /// <summary>
    /// Document management. It needs only the company itself, so it does not pull in a finance phase.
    /// </summary>
    public static IServiceCollection AddOrganizationPhaseOneSeeding(this IServiceCollection services)
    {
        services.TryAddScoped<SeedRunner>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, SeedModelIntegrityValidator>());
        services.AddFoundationSeeders();
        services.AddOrganizationSeeders();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, OrganizationIntegrityValidator>());
        return services;
    }

    /// <summary>
    /// Every module's configuration and master data in one run: Foundation, General ledger, Tax, Cash
    /// and bank, Inventory, Accounts payable, Accounts receivable, Administration, Communication,
    /// Workflow and Organization. The runner orders all seeders on their declared dependencies and
    /// runs every module's integrity validator over the combined result before it commits, so a
    /// cross-module reference that one phase alone would never check fails the whole run. No
    /// transactions are created; those stay with each module's transaction examples.
    /// </summary>
    public static IServiceCollection AddAllModulesPhaseOneSeeding(this IServiceCollection services)
    {
        services.AddFoundationPhaseOneSeeding();
        services.AddAccountsReceivablePhaseOneSeeding();
        services.AddAdministrationPhaseOneSeeding();
        services.AddCommunicationPhaseOneSeeding();
        services.AddWorkflowPhaseOneSeeding();
        services.AddOrganizationPhaseOneSeeding();
        // Registered only by their own example packages before, so a plain phase-one run never ran them.
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, TaxIntegrityValidator>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, CashAndBankIntegrityValidator>());
        return services;
    }

    public static IServiceCollection AddAccountsPayablePhaseOneSeeding(this IServiceCollection services)
    {
        services.TryAddScoped<SeedRunner>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, SeedModelIntegrityValidator>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, AccountsPayablePhaseOneValidator>());
        return services.AddGeneralLedgerSeeders().AddTaxSeeders().AddInventorySeeders()
            .AddCashAndBankSeeders().AddAccountsPayableSeeders();
    }

    /// <summary>
    /// The sales example. It builds on the purchase phase, because the demo items have to be bought
    /// before they can be sold, and it reuses that ledger, chart, bank, geography and warehouse.
    /// </summary>
    public static IServiceCollection AddAccountsReceivablePhaseOneSeeding(this IServiceCollection services)
    {
        services.AddAccountsPayablePhaseOneSeeding();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISeedValidator, AccountsReceivablePhaseOneValidator>());
        return services.AddAccountsReceivableSeeders();
    }
}
