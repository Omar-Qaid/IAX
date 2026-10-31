namespace IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure;

public interface IAccountStructureService
{
    /// <summary>
    /// Creates an account structure as a draft, with its segments and their allowed values. A draft
    /// constrains nothing until it is activated.
    /// </summary>
    Task<(AccountStructureDto? Result, string? Error)> CreateAsync(AccountStructureCreateDto input, CancellationToken ct);

    /// <summary>
    /// Activates a draft: builds its constraint tree, and refuses when the structure's main account
    /// values are already claimed by another active structure on the ledger, because the main
    /// account value is what selects the structure.
    /// </summary>
    Task<(AccountStructureDto? Result, string? Error)> ActivateAsync(string name, CancellationToken ct);

    /// <summary>Assigns an activated structure to the company's ledger.</summary>
    Task<(AccountStructureDto? Result, string? Error)> AssignToLedgerAsync(string name, CancellationToken ct);

    Task<(IReadOnlyList<AccountStructureDto>? Items, string? Error)> ListAsync(CancellationToken ct);
}
