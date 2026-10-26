using FluentValidation;
using IAX.IXApi.Modules.Administration.AuditLogs.Dtos;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Validation
{
    public class SysExceptionLogDtoValidator : BaseValidator<SysExceptionLogDto>
    {
        public SysExceptionLogDtoValidator()
        {
            RuleFor(x => x.Severity).NotEmpty();
        }
    }
}
