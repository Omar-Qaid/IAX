using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Foundation.LegalEntities
{
    public class CompanyInfoDtoValidator : BaseValidator<CompanyInfoDto>
    {
        public CompanyInfoDtoValidator()
        {
            RuleFor(x => x.DataArea).NotEmpty().MaximumLength(4);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        }
    }
}
