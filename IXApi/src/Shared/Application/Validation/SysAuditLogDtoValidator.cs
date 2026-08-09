using FluentValidation;

namespace IAX.IXApi.Shared.Application.Validation
{
    public class SysAuditLogDtoValidator : AbstractValidator<IAX.IXApi.Shared.Application.Contracts.SysAuditLogDto>
    {
        public SysAuditLogDtoValidator()
        {
            RuleFor(x => x.TableName).NotEmpty().WithMessage("Table Name is required");
            RuleFor(x => x.RecordId).NotEmpty().WithMessage("Record ID is required");
            RuleFor(x => x.ColumnName).NotEmpty().WithMessage("Column Name is required");
            RuleFor(x => x.Operation).NotEmpty().WithMessage("Operation is required")
                .Must(op => op == "Insert" || op == "Update" || op == "Delete")
                .WithMessage("Operation must be Insert, Update, or Delete");
        }
    }
}
