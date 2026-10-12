namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public interface ISalesOrderQuickCreateService
{
    Task<SalesOrderQuickCreateResult> CreateAsync(SalesOrderQuickCreateDto input, CancellationToken cancellationToken);
}
