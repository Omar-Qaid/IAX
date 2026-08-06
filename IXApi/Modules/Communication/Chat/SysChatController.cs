using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Communication.Chat.Services;
using IAX.IXApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Communication.Chat
{
    /// <summary>
    /// REST API for chat: post a message (persisted + broadcast over SysChatHub) and read
    /// room history. Real-time delivery is identical whether sent here or via the hub.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class SysChatController : ControllerBase
    {
        private readonly ISysChatService _chat;
        private readonly ICurrentUserService _currentUser;

        public SysChatController(ISysChatService chat, ICurrentUserService currentUser)
        {
            _chat = chat;
            _currentUser = currentUser;
        }

        /// <summary>Gets the current user's conversations with last message + unread counts.</summary>
        [HttpGet("conversations")]
        public async Task<ActionResult<APIResponse<IEnumerable<SysChatConversationDto>>>> GetConversations(
            CancellationToken ct = default)
        {
            var userId = _currentUser.GetCurrentUserId();
            var items = await _chat.GetConversationsAsync(userId, ct);
            return Ok(APIResponse<IEnumerable<SysChatConversationDto>>.Ok(items));
        }

        /// <summary>Marks a room as read up to now for the current user.</summary>
        [HttpPost("{roomId}/read")]
        public async Task<ActionResult<APIResponse<bool>>> MarkRead(string roomId, CancellationToken ct = default)
        {
            var userId = _currentUser.GetCurrentUserId();
            await _chat.MarkReadAsync(userId, roomId, ct);
            return Ok(APIResponse<bool>.Ok(true));
        }

        /// <summary>Gets a page of a room's message history (newest first).</summary>
        [HttpGet("{roomId}/history")]
        public async Task<ActionResult<APIResponse<IEnumerable<SysChatMessageDto>>>> GetHistory(
            string roomId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            var (items, total) = await _chat.GetHistoryAsync(roomId, pageNumber, pageSize, ct);
            var response = APIResponse<IEnumerable<SysChatMessageDto>>.Ok(items);
            response.Pagination = new PaginationMetadata(pageNumber, pageSize, total);
            return Ok(response);
        }

        /// <summary>Posts a message to a room as the current user.</summary>
        [HttpPost("{roomId}/messages")]
        public async Task<ActionResult<APIResponse<SysChatMessageDto>>> Send(
            string roomId,
            [FromBody] SendChatMessageRequest request,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(APIResponse<SysChatMessageDto>.Fail("Message content is required."));

            var senderId = _currentUser.GetCurrentUserId();
            var dto = await _chat.SendAsync(roomId, senderId, request.Content, ct);
            return Ok(APIResponse<SysChatMessageDto>.Ok(dto));
        }
    }

    public class SendChatMessageRequest
    {
        public string Content { get; set; } = null!;
    }
}
