namespace IAX.IXApi.Infrastructure.Realtime
{
    /// <summary>
    /// Defines how a real-time message is targeted / routed.
    /// </summary>
    public enum SysRealtimeChannelType
    {
        /// <summary>
        /// Send to a specific user (routed to "user_{userId}" group).
        /// </summary>
        User = 0,

        /// <summary>
        /// Send to a named group (e.g. department, team, custom room).
        /// </summary>
        Group = 1,

        /// <summary>
        /// Send to all users in a specific role.
        /// </summary>
        Role = 2,

        /// <summary>
        /// Send to all connected clients (global broadcast).
        /// </summary>
        Broadcast = 3,

        /// <summary>
        /// Send to all users except the sender.
        /// </summary>
        Others = 4
    }

    /// <summary>
    /// Specifies the target destination for a real-time message.
    /// </summary>
    public class SysRealtimeChannel
    {
        /// <summary>
        /// How to route this message.
        /// </summary>
        public SysRealtimeChannelType Type { get; set; }

        /// <summary>
        /// Target identifier:
        ///   - User: userId string
        ///   - Group: group name string
        ///   - Role: role name string
        ///   - Broadcast: not used (can be null)
        /// </summary>
        public string? Target { get; set; }

        // ── Factory Methods ──────────────────────────────────────────────

        public static SysRealtimeChannel ToUser(string userId) =>
            new() { Type = SysRealtimeChannelType.User, Target = userId };

        public static SysRealtimeChannel ToGroup(string groupName) =>
            new() { Type = SysRealtimeChannelType.Group, Target = groupName };

        public static SysRealtimeChannel ToRole(string roleName) =>
            new() { Type = SysRealtimeChannelType.Role, Target = roleName };

        public static SysRealtimeChannel ToAll() =>
            new() { Type = SysRealtimeChannelType.Broadcast };

        public static SysRealtimeChannel ToOthers() =>
            new() { Type = SysRealtimeChannelType.Others };
    }
}
