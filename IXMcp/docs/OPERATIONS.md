# IXMcp operations

Package the exact verified IXApi contract artifact with each IXMcp release and mount it read-only at `/contracts`. Configure `IXMcp__IXApiBaseUrl`, `IXMcp__EnabledReadTools`, and emergency denies through controlled deployment configuration. Secrets and bearer credentials must not appear in image layers or settings files.

Deploy IXApi backward-compatible changes first, verify the routed pool, then deploy the matching contract and IXMcp image. `/health/live` checks the process; `/health/ready` requires a valid catalog with at least one admitted tool and no rejected refresh. Start with one canary instance, test authenticated discovery and all three reads, then expand.

For rollback, add affected tools to `EmergencyDeniedTools` first, restore a compatible catalog/server image, verify readiness and IXApi compatibility, then reconnect clients that cache tool lists. Software rollback does not reverse committed business operations.

Run `scripts/verify.ps1` before packaging. Production acceptance also requires an external secret store, TLS termination, authenticated staging evidence, capacity measurements, alert routing, and a rehearsed canary/rollback procedure.
