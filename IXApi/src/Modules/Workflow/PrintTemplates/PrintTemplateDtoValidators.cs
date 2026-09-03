using FluentValidation;

namespace IAX.IXApi.Modules.Workflow.PrintTemplates;

public sealed class CreatePrintTemplateDtoValidator : AbstractValidator<CreatePrintTemplateDto>
{
    public CreatePrintTemplateDtoValidator()
    {
        RuleFor(item => item)
            .Must(item => item.RefRecId > 0 || item.ProcessId > 0)
            .WithMessage("RefRecId is required.");
        RuleFor(item => item.RefTableId).GreaterThan(0).When(item => item.RefRecId > 0);
        RuleFor(item => item.Code).NotEmpty().MaximumLength(50).Matches("^[A-Za-z0-9_-]+$");
        RuleFor(item => item.Name).NotEmpty().MaximumLength(200);
        RuleFor(item => item.NameAlias).MaximumLength(255);
        RuleFor(item => item.Description).MaximumLength(1000);
        RuleFor(item => item.Document).NotNull();
    }
}

public sealed class UpdatePrintTemplateDtoValidator : AbstractValidator<UpdatePrintTemplateDto>
{
    public UpdatePrintTemplateDtoValidator()
    {
        RuleFor(item => item.Code).NotEmpty().MaximumLength(50).Matches("^[A-Za-z0-9_-]+$");
        RuleFor(item => item.Name).NotEmpty().MaximumLength(200);
        RuleFor(item => item.NameAlias).MaximumLength(255);
        RuleFor(item => item.Description).MaximumLength(1000);
        RuleFor(item => item.Document).NotNull();
    }
}
