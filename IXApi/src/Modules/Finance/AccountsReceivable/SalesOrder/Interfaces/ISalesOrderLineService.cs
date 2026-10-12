namespace IAX.IXApi.Modules.Finance.AccountsReceivable;

public interface ISalesOrderLineService
{
    Task<SalesLineWriteResult> AddAsync(long recId, SalesLineInputDto input, CancellationToken cancellationToken);
    Task<SalesLineWriteResult> ChangeAsync(long recId, long lineId, SalesLineInputDto? input, CancellationToken cancellationToken);
}
