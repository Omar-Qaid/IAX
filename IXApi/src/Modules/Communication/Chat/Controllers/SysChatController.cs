using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Chat.Services;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.NumberSequences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Communication.Chat.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SysChatController : BaseController<SysChatMessage, SysChatMessageDto>
    {
        private readonly ISysChatService _chat;
        private readonly ICurrentUserService _currentUser;

        public SysChatController(
            ISysChatService service,
            ILogger<SysChatController> logger,
            ICurrentUserService currentUser)
            : base(service, logger)
        {
            _chat = service;
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
            if (!_chat.CanAccessRoom(userId, roomId))
                return NotFound(APIResponse<bool>.Fail("Chat room not found."));

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
            var userId = _currentUser.GetCurrentUserId();
            if (!_chat.CanAccessRoom(userId, roomId))
                return NotFound(APIResponse<IEnumerable<SysChatMessageDto>>.Fail("Chat room not found."));

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
            if (!_chat.CanAccessRoom(senderId, roomId))
                return NotFound(APIResponse<SysChatMessageDto>.Fail("Chat room not found."));

            var dto = await _chat.SendAsync(roomId, senderId, request.Content, ct);
            return Ok(APIResponse<SysChatMessageDto>.Ok(dto));
        }

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysChatMessageDto>>>> GetAll(CancellationToken cancellationToken = default) =>
            base.GetAll(cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysChatMessageDto>>>> GetPaged(QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
            base.GetPaged(paginationParams, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysChatMessageDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
            base.GetById(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysChatMessageDto>>> Create(SysChatMessageDto dto, CancellationToken cancellationToken = default) =>
            base.Create(dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysChatMessageDto>>> Update(string id, SysChatMessageDto dto, CancellationToken cancellationToken = default) =>
            base.Update(id, dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
            base.Delete(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysChatMessageDto>>>> CreateRange(IEnumerable<SysChatMessageDto> dtos, CancellationToken cancellationToken = default) =>
            base.CreateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysChatMessageDto>>>> UpdateRange(IEnumerable<SysChatMessageDto> dtos, CancellationToken cancellationToken = default) =>
            base.UpdateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> DeleteRange(IEnumerable<string> ids, CancellationToken cancellationToken = default) =>
            base.DeleteRange(ids, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<NumberSequenceMetadataDto>>> GetNumberSequence(CancellationToken cancellationToken = default) =>
            base.GetNumberSequence(cancellationToken);
    }
}
