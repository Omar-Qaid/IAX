namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Communication;

/// <summary>Stable business references described by the Communication manifest and checked against persisted seeders.</summary>
public static class CommunicationSeedExample
{
    public static IReadOnlyList<(string Entity, string Field, string Value)> References { get; } =
    [
        ("SysNotificationTemplate","Code","WF_REQUEST_SUBMITTED"),
        ("SysNotificationTemplate","Code","WF_REQUEST_APPROVED"),
        ("SysNotificationTemplate","Code","WF_REQUEST_REJECTED"),
        ("SysNotificationTemplate","Code","INV_LOW_STOCK"),
        ("SysNotificationTemplate","Code","FIN_INVOICE_DUE"),
        ("SysNotificationTemplate","Code","HR_LEAVE_APPROVED")
    ];
}
