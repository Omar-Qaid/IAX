using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Interfaces;
using IAX.IXApi.Modules.Finance.AccountsReceivable.SalesOrder.Services;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using IAX.IXApi.Modules.Finance.Foundation.Markup;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.CycleErpExamples.AccountsPayable;
public static class AccountsPayableTransactionRegistration
{
    /// <summary>Opt-in transactions. The host supplies ApplicationDbContext and the company/current-user services.</summary>
    public static IServiceCollection AddAccountsPayableTransactionExamples(this IServiceCollection services)
    {
        services.AddAccountsPayableConfigurationAndMasterDataExamples();
        services.TryAddScoped<IUnitOfWork, UnitOfWork>();
        services.TryAddScoped<ISysNumberSequenceService, SysNumberSequenceService>();
        services.TryAddScoped<ISalesInventoryNumberService, SalesInventoryNumberService>();
        services.TryAddScoped<IInventDimensionResolver, InventDimensionResolver>();
        services.TryAddScoped<SalesUnitConversionService>();
        services.TryAddScoped<IPurchInventorySupplyService, PurchInventorySupplyService>();
        services.TryAddScoped<IPurchOrderService, PurchOrderService>();
        services.TryAddScoped<IPurchConfirmationService, PurchConfirmationService>();
        services.TryAddScoped<IProductReceiptService, ProductReceiptService>();
        services.TryAddScoped<IVendorInvoiceService, VendorInvoiceService>();
        services.TryAddScoped<IVendInvoiceInfoService, VendInvoiceInfoService>();
        services.TryAddScoped<IVendPaymentService, VendPaymentService>();
        services.TryAddScoped<IVendSettleOpenTransactionsService, VendSettleOpenTransactionsService>();
        services.TryAddScoped<IVendSettlementReversalService, VendSettlementReversalService>();
        services.TryAddScoped<IVendPaymentReversalService, VendPaymentReversalService>();
        services.TryAddScoped<IPurchReturnService, PurchReturnService>();
        services.TryAddScoped<ILedgerJournalTableService, LedgerJournalTableService>();
        services.TryAddScoped<ILedgerJournalTransService, LedgerJournalTransService>();
        services.TryAddScoped<IMarkupChargeCalculator, MarkupChargeCalculator>();
        services.TryAddScoped<IMarkupTransCommandService, MarkupTransCommandService>();
        services.TryAddScoped<IValidator<MarkupTransDto>, MarkupTransValidator>();
        services.TryAddScoped<IMarkupTransService, MarkupTransService>();
        services.TryAddScoped<AccountsPayableJournalExamples>();
        services.TryAddScoped<AccountsPayableTransactionFlow>();
        services.TryAddScoped<AccountsPayableTransactionExamples>();
        return services;
    }
}
