using FluentValidation;
using IAX.IXApi.Modules.Administration.DataManagement.Dtos;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Administration.DataManagement.Validation
{
    public class SysExportRequestValidator : BaseValidator<SysExportRequest>
    {
        public SysExportRequestValidator()
        {
            RuleFor(x => x.MaxRows).InclusiveBetween(1, 1_000_000).WithMessage("MaxRows must be between 1 and 1,000,000.");
        }
    }
}
