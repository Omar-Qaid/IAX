namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerParameters;

public interface ILedgerParametersService
{
    /// <summary>The company's parameters, created with their defaults on first access.</summary>
    Task<(LedgerParametersDto? Result, string? Error)> GetAsync(CancellationToken ct);

    /// <summary>Replaces the company's parameter values.</summary>
    Task<(LedgerParametersDto? Result, string? Error)> UpdateAsync(LedgerParametersDto input, CancellationToken ct);
}
