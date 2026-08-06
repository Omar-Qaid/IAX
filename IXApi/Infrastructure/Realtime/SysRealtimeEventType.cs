namespace IAX.IXApi.Infrastructure.Realtime
{
    /// <summary>
    /// All real-time event types supported by the system.
    /// Add new event types here as new modules are integrated.
    /// </summary>
    public enum SysRealtimeEventType
    {
        // ── Notifications ────────────────────────────────────────────────
        Notification = 0,
        NotificationRead = 1,
        UnreadCountUpdate = 2,

        // ── Chat ─────────────────────────────────────────────────────────
        ChatMessage = 10,
        ChatTyping = 11,
        ChatRead = 12,

        // ── Workflow ─────────────────────────────────────────────────────
        WorkflowEvent = 20,
        WorkflowStepChanged = 21,
        WorkflowApproval = 22,
        WorkflowRejection = 23,

        // ── Background Jobs & Progress ───────────────────────────────────
        JobStarted = 29,
        JobProgress = 30,
        JobCompleted = 31,
        JobFailed = 32,

        // ── Dashboard & Live Data ────────────────────────────────────────
        DashboardUpdate = 40,
        LiveDataRefresh = 41,

        // ── System ───────────────────────────────────────────────────────
        SystemAlert = 50,
        MaintenanceNotice = 51,
        ForceLogout = 52,

        // ── Presence ─────────────────────────────────────────────────────
        UserOnline = 60,
        UserOffline = 61,
        UserAway = 62
    }
}
