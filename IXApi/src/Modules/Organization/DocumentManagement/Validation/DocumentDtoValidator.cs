using FluentValidation;
using IAX.IXApi.Modules.Organization.DocumentManagement.Dtos;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Organization.DocumentManagement.Validation;

public class DocumentDtoValidator : BaseValidator<DocumentDto>
{
    public DocumentDtoValidator()
    {
        RuleFor(x => x.RefTableId).GreaterThan(0);
        RuleFor(x => x.RefRecId).GreaterThan(0);
        RuleFor(x => x.TypeId).NotEmpty().MaximumLength(40);
    }
}
