using FluentValidation;
using IAX.IXApi.Modules.Organization.DocumentManagement.Dtos;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Organization.DocumentManagement.Validation;

public class CreateDocumentFormValidator : BaseValidator<CreateDocumentForm>
{
    public CreateDocumentFormValidator()
    {
        RuleFor(x => x.TypeId).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Name).MaximumLength(120);
    }
}
