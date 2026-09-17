# External acceptance gates

Local implementation is fail-closed until the following evidence is supplied and recorded.

## C5 remote identity

- OIDC issuer and MCP audience selected.
- IXMcp validates issuer, audience, expiry and scopes.
- Standards-based IXApi delegation selected; no shared signing secret or blind cross-audience passthrough.
- Logout, revocation, disablement, expiry and permission/company reduction tested with two users and two companies.

## C6 staging pilot

- Staging IXApi address and matching contract release supplied.
- Test users and records supplied for Department, Customer and one accessible/inaccessible Workflow request.
- Direct REST and MCP structured results match after documented projection.
- 401, 403, 404 concealment, permission change and company denial match IXApi.

## C8 write

- Business operation and approver selected.
- Backend supports durable idempotency, concurrency and outcome lookup.
- Shared durable approval/idempotency store selected.
- Replay, changed payload, crash, timeout and commit/response-loss scenarios pass before admission.

## C9 deployment

- Platform, TLS/proxy trust, secret store, telemetry exporter and alert owners selected.
- Container build and vulnerability scan pass in CI.
- Capacity limits measured in staging.
- Canary and rollback are rehearsed with the routed IXApi pool.

## C10 expansion

- Data owners review each requested profile and output classification.
- Every newly admitted operation completes the applicable read or write gates.
- Excluded operations retain a concrete reason; the verifier rejects missing classifications.
