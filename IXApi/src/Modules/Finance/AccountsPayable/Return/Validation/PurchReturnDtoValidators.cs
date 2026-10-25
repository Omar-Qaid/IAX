using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

public sealed class PurchReturnCreateDtoValidator : BaseValidator<PurchReturnCreateDto>
{
    public PurchReturnCreateDtoValidator() => RuleFor(x => x.Lines)
        .Must(x => x is { Count: > 0 }).WithMessage("At least one return line is required.");
}

public sealed class PurchReturnShipDtoValidator : BaseValidator<PurchReturnShipDto>
{
    public PurchReturnShipDtoValidator()
    {
        RuleFor(x => x.PackingSlipId).Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("A return shipment number is required.");
        RuleFor(x => x.Lines).Must(x => x is { Count: > 0 })
            .WithMessage("At least one line is required.");
    }
}

public sealed class VendCreditNoteDtoValidator : BaseValidator<VendCreditNoteDto>
{
    public VendCreditNoteDtoValidator() => RuleFor(x => x.InvoiceId)
        .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("A vendor credit note number is required.");
}
