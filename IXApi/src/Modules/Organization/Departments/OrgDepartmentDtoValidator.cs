using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Organization.Departments
{
    public class OrgDepartmentDtoValidator : BaseValidator<OrgDepartmentDto>
    {
        public OrgDepartmentDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
        }
    }
}
