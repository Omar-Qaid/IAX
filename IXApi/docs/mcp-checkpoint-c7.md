# C7 automatic refresh checkpoint

Date: 2026-09-17  
Status: local lifecycle implementation and tests complete; target-client notification evidence pending.

Startup blocks on the first valid catalog. A configurable poll recompiles the complete artifact set and replaces the immutable snapshot atomically. Invalid refresh retains the previous snapshot, records a degraded diagnostic and prevents readiness. `EmergencyDeniedTools` removes known tools on the next load independently of artifact admission. Tests cover atomic replacement, emergency retirement, invalid-refresh fallback and recovery.

Clients that cache tool lists must reconnect or refresh. Notification behavior for the final IXApp MCP client remains a C6/C9 integration acceptance item.
