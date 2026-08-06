namespace IAX.IXApi.Shared.Domain.Entities
{
    /// <summary>
    /// Marker for entities that must NOT be written to the audit trail (<c>SysAuditLogs</c>).
    /// Use for high-frequency / low-value tables such as chat messages and read-states,
    /// where per-column audit rows would be noise and a performance drain.
    /// The <c>AuditInterceptor</c> skips any entity implementing this interface.
    /// </summary>
    public interface IAuditExempt
    {
    }
}
