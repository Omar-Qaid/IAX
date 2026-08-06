using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IAX.IXApi.Shared.Application.Attributes
{
    public abstract class ContactNumberAttribute : ValidationAttribute
    {
        protected abstract int MinLength { get; }

        protected abstract int MaxLength { get; }

        protected abstract string TypeName { get; }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            string contactNumber = value.ToString()!;

            if (contactNumber.Length < MinLength || contactNumber.Length > MaxLength)
            {
                return new ValidationResult(ErrorMessage ?? $"{TypeName} number must be between {MinLength} and {MaxLength} digits.");
            }

            Regex validationExpression = new Regex($@"^\+[1-9]\d{{{MinLength},{MaxLength}}}$");

            bool isValid = validationExpression.IsMatch(contactNumber);

            if (isValid)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? $"Invalid {TypeName} format. It should be in E.164 format (e.g., +1234567890).");
        }
    }
}
