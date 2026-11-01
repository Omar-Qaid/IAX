namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

public interface ITaxSettlementService
{
    /// <summary>
    /// Settles and posts sales tax for one period interval: clears the input and output tax
    /// accounts and moves the net balance to the sales tax settlement account.
    /// </summary>
    Task<(TaxSettlementResultDto? Result, string? Error)> SettleAndPostAsync(
        TaxSettlementInputDto input, CancellationToken ct = default);

    /// <summary>
    /// What a settlement run would post, without posting it. Same selection and the same rules as
    /// <see cref="SettleAndPostAsync"/>, so a refusal is surfaced before anything is written.
    /// </summary>
    Task<(TaxSettlementResultDto? Result, string? Error)> PreviewAsync(
        TaxSettlementInputDto input, CancellationToken ct = default);
}
