namespace IAX.IXApi.Modules.Communication.Notifications.Entities
{
    /// <summary>
    /// Notification priority levels.
    /// </summary>
    public enum SysNotificationPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    /// <summary>
    /// Supported notification delivery channels.
    /// New channels can be added here and a corresponding INotificationChannel implementation created.
    /// </summary>
    public enum SysNotificationChannel
    {
        InApp = 0,
        Email = 1,
        SMS = 2,
        Push = 3,
        WhatsApp = 4,
        MicrosoftTeams = 5,
        Slack = 6,
        Webhook = 7
    }

    /// <summary>
    /// Overall notification lifecycle status.
    /// </summary>
    public enum SysNotificationStatus
    {
        Pending = 0,
        Sent = 1,
        Delivered = 2,
        Failed = 3,
        Expired = 4,
        Cancelled = 5
    }

    /// <summary>
    /// Per-recipient delivery status.
    /// </summary>
    public enum SysDeliveryStatus
    {
        Pending = 0,
        Delivered = 1,
        Read = 2,
        Failed = 3
    }
}
