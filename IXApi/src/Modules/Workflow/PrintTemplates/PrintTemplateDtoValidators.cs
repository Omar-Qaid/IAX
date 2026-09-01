using FluentValidation;

namespace IAX.IXApi.Modules.Workflow.PrintTemplates;

public sealed class CreatePrintTemplateDtoValidator : AbstractValidator<CreatePrintTemplateDto>
{
    public CreatePrintTemplateDtoValidator()
    {
        RuleFor(item => item.ProcessId).GreaterThan(0);
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
