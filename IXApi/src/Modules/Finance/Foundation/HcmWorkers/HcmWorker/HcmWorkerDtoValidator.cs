using FluentValidation;

namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;

public sealed class HcmWorkerDtoValidator : AbstractValidator<HcmWorkerDto>
{
    public HcmWorkerDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.NameAlias).MaximumLength(60);
        RuleFor(x => x.OccupationId).GreaterThan((short)0);
        RuleFor(x => x.GenderId).GreaterThan((byte)0);
        RuleFor(x => x.NationalityId).GreaterThan((short)0);
        RuleFor(x => x.PersonnelNumber).MaximumLength(25);
    }
}