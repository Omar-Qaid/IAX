# C6 read-only pilot checkpoint

Date: 2026-09-17  
Status: MCP protocol and admission implementation verified; authenticated live staging evidence pending.

The official SDK now receives dynamically generated list/call handlers. Deployment configuration can admit only known GET candidates. Discovery filters tools by the current user's IXApi permissions, and call handling repeats the same admission/permission checks before using the validated request token and company. All pilot tools remain disabled in checked-in settings.

Local tests prove protocol schemas, permission-filtered discovery and execution-context propagation. The official MCP client completes a Streamable HTTP handshake against the in-process server, discovers the admitted Workflow tool, invokes it and receives structured content. No staging URL, test credentials or live Workflow request ID were available, so live REST/MCP result parity and denial behavior are not claimed.
