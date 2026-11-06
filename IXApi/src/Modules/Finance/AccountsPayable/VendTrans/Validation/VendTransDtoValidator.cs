using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

/// <summary>
/// Intentionally without rules. <see cref="VendTransDto"/> is a read shape: the vendor transaction
/// controller exposes only reads - by record, paged, and the open balance - and disables create,
/// update and delete, because a vendor transaction is written by invoice, payment and settlement
/// posting, never by a client. A response shape has no input to validate. The query input is
/// validated by <see cref="VendTransQueryDtoValidator"/>.
/// </summary>
public class VendTransDtoValidator : BaseValidator<VendTransDto>
{
}
