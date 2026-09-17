# C9 production-readiness checkpoint

Date: 2026-09-17  
Status: packaging and automated release gates implemented; environment acceptance pending.

IXMcp has a pinned non-root multi-stage container, read-only contract mount convention, health gates, controlled admission/emergency deny settings, browser Origin and Host controls, per-user concurrency limits, a contract verifier, an independent restore/build/test script, and deployment/canary/rollback instructions.

The release script regenerates the OpenAPI, operation inventory, pilot contracts and manifest directly from the current IXApi assembly before compiling IXMcp. This closes the stale-artifact gap when an endpoint or DTO changes.

Production acceptance still requires the chosen platform, secret store, TLS/proxy policy, authenticated staging run, capacity measurements, alert routing and a rehearsed canary/rollback. These are external deployment facts and are not represented as passed locally.
