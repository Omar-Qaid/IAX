using FluentValidation;
using IAX.IXApi.Shared.Application.Validation;

namespace IAX.IXApi.Modules.Communication.Chat.Validation
{
    public class SendChatMessageRequestValidator : BaseValidator<SendChatMessageRequest>
    {
        public SendChatMessageRequestValidator()
        {
            RuleFor(x => x.Content).NotEmpty();
        }
    }
}
