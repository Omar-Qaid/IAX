using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Communication.Notifications.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Communication.Notifications
{
    /// <summary>
    /// REST API controller for the Notification Management Module.
    /// Provides endpoints for the current user's notifications plus admin send capabilities.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class SysNotificationController : ControllerBase
    {
        private readonly ISysNotificationService _notificationService;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<SysNotificationController> _logger;

        public SysNotificationController(
            ISysNotificationService notificationService,
            ICurrentUserService currentUser,
            ILogger<SysNotificationController> logger)
        {
            _notificationService = notificationService;
            _currentUser = currentUser;
            _logger = logger;
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
            CancellationToken ct = default)
        {
            var userId = _currentUser.GetCurrentUserId();
            var (items, totalCount) = await _notificationService.GetUserNotificationsAsync(
                userId, pageNumber, pageSize, isRead, category, ct);

            var response = APIResponse<IEnumerable<SysNotificationDto>>.Ok(items);
            response.Pagination = new PaginationMetadata(pageNumber, pageSize, totalCount);
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
        public async Task<ActionResult<APIResponse<SysNotificationDto>>> Send(
            [FromBody] CreateSysNotificationDto dto,
            CancellationToken ct = default)
        {
            var result = await _notificationService.SendAsync(dto, ct);
            return Ok(APIResponse<SysNotificationDto>.Ok(result, "Notification sent"));
        }
    }
}
