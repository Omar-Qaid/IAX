using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Tax.Validation
{
    public class TaxDataDtoValidator : BaseValidator<TaxDataDto>
    {
        public TaxDataDtoValidator()
        {
        }
    }
}
