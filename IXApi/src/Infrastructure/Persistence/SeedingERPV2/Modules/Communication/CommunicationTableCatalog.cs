namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Communication;

/// <summary>The tables this module owns, as covered by its registered seeders.</summary>
public static class CommunicationTableCatalog
{
    public static IReadOnlySet<string> MasterTypes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "SysNotificationTemplate",
    };

    /// <summary>
    /// A notification and its recipients are written by the notification service when an event
    /// raises one, and delivered by the queued worker. Nothing here seeds them.
    /// </summary>
    public static IReadOnlySet<string> TransactionTypes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "SysNotification",
        "SysNotificationRecipient",
    };
}
