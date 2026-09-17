# C5 identity and company-context checkpoint

Date: 2026-09-17  
Status: local IXApp boundary implemented; remote OAuth/delegation acceptance pending.

IXMcp requires bearer authentication and `X-Company` on every `/mcp` request. It validates both through IXApi `/api/v1/Auth/me`, retains the returned user/roles/permissions only for the request, and passes the same validated token/company to execution. Missing, expired, revoked and company-denied sessions fail before MCP handling. Tests cover two concurrent users and companies without shared mutable context.

This local design is for the assistant inside IXApp and does not claim a separate MCP OAuth audience. Remote production still requires a selected OIDC provider, MCP audience validation, standards-based downstream delegation and lifecycle evidence.
