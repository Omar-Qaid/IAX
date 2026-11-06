using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Intentionally without rules: no endpoint accepts a <see cref="PurchReturnDto"/>, since the
/// generic create and update actions are disabled. The rules that matter live in
/// <see cref="PurchReturnCreateDtoValidator"/>, <see cref="PurchReturnShipDtoValidator"/> and
/// <see cref="VendCreditNoteDtoValidator"/>, which guard the three real endpoints. Registered only
/// so the generic controller can resolve a validator for its type argument.
/// </summary>
public class PurchReturnDtoValidator : BaseValidator<PurchReturnDto>
{
}
