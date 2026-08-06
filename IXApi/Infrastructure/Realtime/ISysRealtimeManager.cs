namespace IAX.IXApi.Infrastructure.Realtime
{
    /// <summary>
    /// Generic real-time communication manager.
    /// Any module in the system can inject this to push real-time events to connected clients.
    /// 
    /// Usage examples:
    ///   Notifications:  await _realtime.SendToUserAsync(userId, SysRealtimeMessage.Notification(dto));
    ///   Chat:           await _realtime.SendToGroupAsync("room_123", SysRealtimeMessage.Create(ChatMessage, msg));
    ///   Workflow:       await _realtime.SendToUserAsync(userId, SysRealtimeMessage.WorkflowEvent(data));
    ///   Progress:       await _realtime.SendToUserAsync(userId, SysRealtimeMessage.JobProgress(progress));
    ///   Dashboard:      await _realtime.BroadcastAsync(SysRealtimeMessage.DashboardUpdate(stats));
    ///   System Alert:   await _realtime.BroadcastAsync(SysRealtimeMessage.SystemAlert(alert));
    /// </summary>
    public interface ISysRealtimeManager
    {
        // ── Single Target ────────────────────────────────────────────────

        /// <summary>
        /// Sends a real-time message to a specific user.
        /// </summary>
        Task SendToUserAsync(string userId, SysRealtimeMessage message);

        /// <summary>
        /// Sends a real-time message to a named group.
        /// </summary>
        Task SendToGroupAsync(string groupName, SysRealtimeMessage message);

        // ── Multi-Target ─────────────────────────────────────────────────

        /// <summary>
        /// Sends a real-time message to multiple users.
        /// </summary>
        Task SendToUsersAsync(IEnumerable<string> userIds, SysRealtimeMessage message);

        /// <summary>
        /// Sends a real-time message to multiple groups.
        /// </summary>
        Task SendToGroupsAsync(IEnumerable<string> groupNames, SysRealtimeMessage message);

        // ── Broadcast ────────────────────────────────────────────────────

        /// <summary>
        /// Broadcasts a message to all connected clients.
        /// </summary>
        Task BroadcastAsync(SysRealtimeMessage message);

        // ── Channel-based (generic routing) ──────────────────────────────

        /// <summary>
        /// Sends a message using a channel specification (User, Group, Role, Broadcast).
        /// </summary>
        Task SendAsync(SysRealtimeChannel channel, SysRealtimeMessage message);

        /// <summary>
        /// Sends a message to multiple channels simultaneously.
        /// </summary>
        Task SendAsync(IEnumerable<SysRealtimeChannel> channels, SysRealtimeMessage message);

        // ── Group Management ─────────────────────────────────────────────

        /// <summary>
        /// Adds a user's connection to a named group.
        /// </summary>
        Task AddToGroupAsync(string userId, string groupName);

        /// <summary>
        /// Removes a user's connection from a named group.
        /// </summary>
        Task RemoveFromGroupAsync(string userId, string groupName);
    }
}
