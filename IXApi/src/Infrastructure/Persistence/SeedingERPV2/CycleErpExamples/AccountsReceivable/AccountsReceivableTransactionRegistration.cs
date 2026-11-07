using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;
using IAX.IXApi.Modules.Finance.AccountsReceivable.PackingSlip;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Payment;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Return;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FluentValidation;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsReceivable;

public static class AccountsReceivableTransactionRegistration
{
    /// <summary>
    /// Opt-in transactions. The host supplies ApplicationDbContext and the company/current-user services.
    /// The purchase transaction examples are registered as well, because the demo items have to be bought
    /// and received before the sales cycle has anything to deliver.
    /// </summary>
    public static IServiceCollection AddAccountsReceivableTransactionExamples(this IServiceCollection services)
    {
        services.AddAccountsReceivableConfigurationAndMasterDataExamples();
        services.AddAccountsPayableTransactionExamples();
        services.TryAddScoped<IUnitOfWork, UnitOfWork>();
        services.TryAddScoped<ISysNumberSequenceService, SysNumberSequenceService>();
        services.TryAddScoped<ISalesOrderQuickCreateService, SalesOrderQuickCreateService>();
        services.TryAddScoped<SalesOrderValidationService>();
        services.TryAddScoped<ISalesInventoryDemandService, SalesInventoryDemandService>();
        services.TryAddScoped<IValidator<PostConfirmationRequest>, SalesOrderConfirmationValidator>();
        services.TryAddScoped<ISalesOrderLineService, SalesOrderLineService>();
        services.TryAddScoped<ISalesOrderHeaderService, SalesOrderHeaderService>();
        services.TryAddScoped<ISalesOrderConfirmationService, SalesOrderConfirmationService>();
        services.TryAddScoped<IPackingSlipJournalService, PackingSlipJournalService>();
        services.TryAddScoped<ISalesInvoiceService, SalesInvoiceService>();
        services.TryAddScoped<IFreeTextInvoiceService, FreeTextInvoiceService>();
        services.TryAddScoped<ICustPaymentService, CustPaymentService>();
        services.TryAddScoped<ICustSettleOpenTransactionsService, CustSettleOpenTransactionsService>();
        services.TryAddScoped<ICustSettlementReversalService, CustSettlementReversalService>();
        services.TryAddScoped<ICustPaymentReversalService, CustPaymentReversalService>();
        services.TryAddScoped<ICustReturnService, CustReturnService>();
        services.TryAddScoped<ISalesOrderCancellationService, SalesOrderCancellationService>();
        services.TryAddScoped<AccountsReceivableTransactionFlow>();
        services.TryAddScoped<AccountsReceivableTransactionExamples>();
        return services;
    }
}
