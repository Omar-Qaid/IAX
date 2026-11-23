using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.CashAndBank;

public static class CashAndBankTransactionRegistration
{
    public static IServiceCollection AddCashAndBankTransactionExamples(this IServiceCollection services)
    {
        services.AddCashAndBankConfigurationAndMasterDataExamples();
        services.AddAccountsPayableTransactionExamples();
        services.AddAccountsReceivableTransactionExamples();
        services.TryAddScoped<CashAndBankTransactionExamples>();
        return services;
    }
}
