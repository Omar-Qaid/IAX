namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public interface IVendTransReadService
{
    Task<VendTransDto?> GetAsync(long recId, CancellationToken ct);
    Task<VendTransPageDto> SearchAsync(VendTransQueryDto query, CancellationToken ct);
}
