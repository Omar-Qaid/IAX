using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Finance.Foundation.Departments
{
    public class HcmDepartmentDtoValidator : BaseValidator<HcmDepartmentDto>
    {
        public HcmDepartmentDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        }
    }
}
