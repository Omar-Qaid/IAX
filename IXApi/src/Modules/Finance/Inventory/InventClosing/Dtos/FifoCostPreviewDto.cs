namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record FifoCostPreviewDto(DateTime Cutoff, bool CanExecute, IReadOnlyList<string> Problems,
    IReadOnlyList<FifoCostIssueDto> Issues);
