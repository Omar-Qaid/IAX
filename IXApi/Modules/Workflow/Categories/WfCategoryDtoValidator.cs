using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;


namespace IAX.IXApi.Modules.Workflow.Categories
{
    public class WfCategoryDtoValidator : BaseValidator<WfCategoryDto>
    {
        public WfCategoryDtoValidator()
        {
             RuleFor(x => x.NameAR).NotEmpty().WithMessage("Arabic Name is required");
             RuleFor(x => x.Name).NotEmpty().WithMessage("English Name is required");
        }
    }
}


