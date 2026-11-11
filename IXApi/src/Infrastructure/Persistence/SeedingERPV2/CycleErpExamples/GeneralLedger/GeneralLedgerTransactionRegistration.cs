using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Registration;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.GeneralLedger;

public static class GeneralLedgerTransactionRegistration
{
    public static IServiceCollection AddGeneralLedgerTransactionExamples(this IServiceCollection services)
    {
        services.AddGeneralLedgerConfigurationAndMasterDataExamples();
        services.TryAddScoped<IUnitOfWork, UnitOfWork>();
        services.TryAddScoped<ISysNumberSequenceService, SysNumberSequenceService>();
        services.TryAddScoped<ILedgerJournalTableService, LedgerJournalTableService>();
        services.TryAddScoped<ILedgerJournalTransService, LedgerJournalTransService>();
        services.TryAddScoped<ILedgerJournalReversalService, LedgerJournalReversalService>();
        services.TryAddScoped<GeneralLedgerTransactionExamples>();
        services.TryAddScoped<GeneralLedgerTransactionFlow>();
        return services;
    }
}
