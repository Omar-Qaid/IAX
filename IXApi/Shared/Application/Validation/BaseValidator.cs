using FluentValidation;

namespace IAX.IXApi.Shared.Application.Validation;

public abstract class BaseValidator<T> : AbstractValidator<T>
{
    protected BaseValidator()
    {
        // Global validation rules can be added here
    }
}
