using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;

public class HcmShowroomDtoValidator : BaseValidator<HcmShowroomDto>
{
    public HcmShowroomDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.PersonnelNumber).MaximumLength(25);
    }
}
