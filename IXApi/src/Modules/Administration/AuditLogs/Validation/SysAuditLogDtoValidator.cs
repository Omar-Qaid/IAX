using FluentValidation;
using IAX.IXApi.Modules.Administration.AuditLogs.Dtos;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Validation
{
    public class SysAuditLogDtoValidator : BaseValidator<SysAuditLogDto>
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
