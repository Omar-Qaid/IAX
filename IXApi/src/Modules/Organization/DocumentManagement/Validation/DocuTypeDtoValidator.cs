using FluentValidation;
using IAX.IXApi.Modules.Organization.DocumentManagement.Dtos;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Organization.DocumentManagement.Validation;

public class DocuTypeDtoValidator : BaseValidator<DocuTypeDto>
{
    public DocuTypeDtoValidator()
    {
        RuleFor(x => x.TypeId).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
    }
}
