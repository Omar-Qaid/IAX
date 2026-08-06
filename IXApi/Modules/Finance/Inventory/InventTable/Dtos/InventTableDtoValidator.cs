using IAX.IXApi.Shared.Application.Validation;
using FluentValidation;

namespace IAX.IXApi.Modules.Finance.Inventory           {
    public class InventTableDtoValidator : BaseValidator<InventTableDto>
    {
        public InventTableDtoValidator()
        {
        }
    }
}

