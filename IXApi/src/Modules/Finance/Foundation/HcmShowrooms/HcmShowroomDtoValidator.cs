using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;

public class HcmShowroomDtoValidator : BaseValidator<HcmShowroomDto>
{
    public HcmShowroomDtoValidator()
    {
        RuleFor(x => x.Party).GreaterThan(0).WithMessage("Party is required.");
    }
}
