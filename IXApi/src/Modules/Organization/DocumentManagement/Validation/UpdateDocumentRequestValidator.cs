using FluentValidation;
using IAX.IXApi.Modules.Organization.DocumentManagement.Dtos;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Organization.DocumentManagement.Validation;

public class UpdateDocumentRequestValidator : BaseValidator<UpdateDocumentRequest>
{
    public UpdateDocumentRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(120);
        RuleFor(x => x.FileName).MaximumLength(518);
    }
}
