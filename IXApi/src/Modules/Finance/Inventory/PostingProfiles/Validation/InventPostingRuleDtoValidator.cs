using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventPostingRuleDtoValidator : BaseValidator<InventPostingRuleDto>
{
    public InventPostingRuleDtoValidator()
    {
        RuleFor(x => x.ItemRelation)
            .MaximumLength(FieldLengths.ItemId).WithMessage($"Item relation cannot exceed {FieldLengths.ItemId} characters.");

        RuleFor(x => x.CustVendRelation)
            .MaximumLength(FieldLengths.CustVendRelation).WithMessage($"Customer/vendor relation cannot exceed {FieldLengths.CustVendRelation} characters.");

        RuleFor(x => x.TaxGroupId)
            .MaximumLength(FieldLengths.TaxGroupId).WithMessage($"Tax group ID cannot exceed {FieldLengths.TaxGroupId} characters.");
    }
}
