using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Communication.Notifications.Controllers
{
    /// <summary>
    /// REST API controller for the Notification Management Module.
    /// Provides endpoints for the current user's notifications plus admin send capabilities.
    /// Inherits from BaseController&lt;SysNotification, SysNotificationDto&gt; following the generic architecture.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SysNotificationController : BaseController<SysNotification, SysNotificationDto>
    {
        private readonly ISysNotificationService _notificationService;
        private readonly ICurrentUserService _currentUser;

        public SysNotificationController(
            ISysNotificationService service,
            ILogger<SysNotificationController> logger,
            ICurrentUserService currentUser)
            : base(service, logger)
        {
            _notificationService = service;
            _currentUser = currentUser;
        }

        // ── Current User Endpoints ───────────────────────────────────────

        /// <summary>
        /// Gets the current user's notifications (paged).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<APIResponse<IEnumerable<SysNotificationDto>>>> GetMyNotifications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isRead = null,
            [FromQuery] string? category = null,
            [FromQuery] bool isArchived = false,
            CancellationToken ct = default)
        {
            var userId = _currentUser.GetCurrentUserId();
            var (items, totalCount) = await _notificationService.GetUserNotificationsAsync(
                userId, Math.Max(1, pageNumber), Math.Clamp(pageSize, 1, 100), isRead, category, ct, isArchived);

            var response = APIResponse<IEnumerable<SysNotificationDto>>.Ok(items);
            response.Pagination = new PaginationMetadata(Math.Max(1, pageNumber), Math.Clamp(pageSize, 1, 100), totalCount);
            return Ok(response);
        }

        /// <summary>
        /// Gets the current user's unread notification count.
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<APIResponse<int>>> GetUnreadCount(CancellationToken ct = default)
        {
            var userId = _currentUser.GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId, ct);
            return Ok(APIResponse<int>.Ok(count));
        }

        /// <summary>
        /// Marks a single notification as read for the current user.
        /// </summary>
        [HttpPut("{id}/read")]
        public async Task<ActionResult<APIResponse<bool>>> MarkAsRead(long id, CancellationToken ct = default)
        {
            var userId = _currentUser.GetCurrentUserId();
            await _notificationService.MarkAsReadAsync(id, userId, ct);
            return Ok(APIResponse<bool>.Ok(true, "Marked as read"));
        }

        /// <summary>
        /// Marks all notifications as read for the current user.
        /// </summary>
        [HttpPut("read-all")]
        public async Task<ActionResult<APIResponse<bool>>> MarkAllAsRead(CancellationToken ct = default)
        {
            var userId = _currentUser.GetCurrentUserId();
            await _notificationService.MarkAllAsReadAsync(userId, ct);
            return Ok(APIResponse<bool>.Ok(true, "All marked as read"));
        }

        /// <summary>
        /// Archives a notification for the current user.
        /// </summary>
        [HttpPut("{id}/archive")]
        public async Task<ActionResult<APIResponse<bool>>> Archive(long id, CancellationToken ct = default)
        {
            var userId = _currentUser.GetCurrentUserId();
            await _notificationService.ArchiveAsync(id, userId, ct);
            return Ok(APIResponse<bool>.Ok(true, "Archived"));
        }

        /// <summary>
        /// Deletes a notification for the current user.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<APIResponse<bool>>> Delete(long id, CancellationToken ct = default)
        {
            var userId = _currentUser.GetCurrentUserId();
            await _notificationService.DeleteAsync(id, userId, ct);
            return Ok(APIResponse<bool>.Ok(true, "Deleted"));
        }

        // ── Admin / System Send Endpoints ────────────────────────────────

        /// <summary>
        /// Sends a notification (admin/system use).
        /// </summary>
        [HttpPost]
        [DomainPermission("System", "Notifications", "Create")]
        public async Task<ActionResult<APIResponse<SysNotificationDto>>> Send(
            [FromBody] CreateSysNotificationDto dto,
            CancellationToken ct = default)
        {
            var result = await _notificationService.SendAsync(dto, ct);
            return Ok(APIResponse<SysNotificationDto>.Ok(result, "Notification sent"));
        }

        // ── Suppress generic CRUD endpoints that conflict with custom endpoints ────

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysNotificationDto>>>> GetAll(CancellationToken cancellationToken = default) =>
            base.GetAll(cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysNotificationDto>>> Create(SysNotificationDto dto, CancellationToken cancellationToken = default) =>
            base.Create(dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
            base.Delete(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysNotificationDto>>>> GetPaged(QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
            base.GetPaged(paginationParams, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysNotificationDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
            base.GetById(id, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<SysNotificationDto>>> Update(string id, SysNotificationDto dto, CancellationToken cancellationToken = default) =>
            base.Update(id, dto, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysNotificationDto>>>> CreateRange(IEnumerable<SysNotificationDto> dtos, CancellationToken cancellationToken = default) =>
            base.CreateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<SysNotificationDto>>>> UpdateRange(IEnumerable<SysNotificationDto> dtos, CancellationToken cancellationToken = default) =>
            base.UpdateRange(dtos, cancellationToken);

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> DeleteRange(IEnumerable<string> ids, CancellationToken cancellationToken = default) =>
            base.DeleteRange(ids, cancellationToken);
    }
}
