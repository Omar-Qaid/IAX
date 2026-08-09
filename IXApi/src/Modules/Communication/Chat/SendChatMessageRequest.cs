using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Communication.Chat.Services;
using IAX.IXApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Communication.Chat
{
    public class SendChatMessageRequest
    {
        public string Content { get; set; } = null!;
    }
}