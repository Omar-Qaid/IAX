using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Communication.Chat.Validation
{
    public class SysChatMessageDtoValidator : BaseValidator<SysChatMessageDto>
    {
        public SysChatMessageDtoValidator()
        {
            RuleFor(x => x.RoomId).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Content).NotEmpty();
        }
    }
}
