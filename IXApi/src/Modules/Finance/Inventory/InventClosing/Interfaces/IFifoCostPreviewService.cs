namespace IAX.IXApi.Modules.Finance.Inventory;

public interface IFifoCostPreviewService
{
    Task<FifoCostPreviewDto> PreviewAsync(DateTime cutoff, CancellationToken ct = default);
}
