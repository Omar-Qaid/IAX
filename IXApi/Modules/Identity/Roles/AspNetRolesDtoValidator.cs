using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;
using IAX.IXApi.Modules.Identity.Authentication;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Impersonation;

namespace IAX.IXApi.Modules.Identity.Roles
{
    public class AspNetRolesDtoValidator : BaseValidator<AspNetRoleDto>
    {
        public AspNetRolesDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        }
    }
}



