namespace IAX.IXApi.Infrastructure.Realtime
{
    /// <summary>
    /// Generic envelope for all real-time messages sent through the system.
    /// Every real-time event (notification, chat, workflow, etc.) is wrapped in this message.
    /// </summary>
    public class SysRealtimeMessage
    {
        /// <summary>
        /// Unique message identifier.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The type of event this message represents.
        /// </summary>
        public SysRealtimeEventType EventType { get; set; }

        /// <summary>
        /// Human-readable event name (used as the SignalR method name on the client).
        /// Defaults to the EventType name if not specified.
        /// </summary>
        public string EventName { get; set; } = null!;

        /// <summary>
        /// The payload data. Can be any serializable object.
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// ID of the user who originated this event. Null for system-generated events.
        /// </summary>
        public string? SenderId { get; set; }

        /// <summary>
        /// ISO 8601 timestamp of when the message was created.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional correlation ID for tracking related events across the system.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Optional metadata for extensibility (e.g. module name, entity info).
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }

        // ── Factory Methods ──────────────────────────────────────────────

        /// <summary>
        /// Creates a new real-time message with the specified event type and payload.
        /// </summary>
        public static SysRealtimeMessage Create(SysRealtimeEventType eventType, object? data = null, string? senderId = null)
        {
            return new SysRealtimeMessage
            {
                EventType = eventType,
                EventName = eventType.ToString(),
                Data = data,
                SenderId = senderId
            };
        }

        /// <summary>
        /// Creates a notification event message.
        /// </summary>
        public static SysRealtimeMessage Notification(object data, string? senderId = null) =>
            Create(SysRealtimeEventType.Notification, data, senderId);

        /// <summary>
        /// Creates an unread count update message.
        /// </summary>
        public static SysRealtimeMessage UnreadCount(int count) =>
            Create(SysRealtimeEventType.UnreadCountUpdate, count);

        /// <summary>
        /// Creates a workflow event message.
        /// </summary>
        public static SysRealtimeMessage WorkflowEvent(object data, string? senderId = null) =>
            Create(SysRealtimeEventType.WorkflowEvent, data, senderId);

        /// <summary>
        /// Creates a job progress message.
        /// </summary>
        public static SysRealtimeMessage JobProgress(object data) =>
            Create(SysRealtimeEventType.JobProgress, data);

        /// <summary>
        /// Creates a system alert message.
        /// </summary>
        public static SysRealtimeMessage SystemAlert(object data) =>
            Create(SysRealtimeEventType.SystemAlert, data);

        /// <summary>
        /// Creates a dashboard update message.
        /// </summary>
        public static SysRealtimeMessage DashboardUpdate(object data) =>
            Create(SysRealtimeEventType.DashboardUpdate, data);
    }
}
