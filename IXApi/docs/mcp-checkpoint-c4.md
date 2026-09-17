# C4 secure HTTP executor checkpoint

Date: 2026-09-17  
Status: implemented and verified internally; real tools and MCP invocation remain disabled.

## Delivered

- Resolves tools from one immutable catalog snapshot and rejects unknown or disabled names before HTTP dispatch.
- Validates arguments against the compiled restricted input schema.
- Binds only compiled scalar path and query parameters using invariant formatting and URI escaping.
- Uses one configured IXApi origin and an approved `/api/` route prefix.
- Disables automatic redirects and rejects traversal, absolute, protocol-relative, and backslash routes.
- Accepts bearer credential, company and correlation values only through an internal trusted execution context.
- Rejects CR/LF header injection and never accepts headers, destinations or credentials as tool arguments.
- Applies configurable call timeout, input-byte, decompressed response-byte and JSON-depth limits.
- Propagates caller cancellation and a correlation identifier.
- Normalizes 204, REST failures, malformed JSON and `APIResponse.Success=false` into stable structured outcomes.
- Projects successful data through each tool's output allowlist and pagination through `pageNumber`, `pageSize`, `totalRecords`, and `totalPages` only.

## Verification

The Release build succeeds with zero warnings and zero errors. Twenty-seven tests pass. Fake HTTP tests assert exact route, query, authorization scheme, company and correlation headers; removal of unapproved response fields; unknown/disabled tool rejection; malformed argument rejection; route and header-injection protection; business and HTTP error mapping; 204 behavior; response bounds; redirect handling; and caller cancellation.

## Evidence boundary

The three C1 candidates still have `executionEnabled=false`, so C4 cannot call them. The executor is not exposed through the MCP SDK. Tests use synthetic admitted descriptors and fake HTTP handlers; no live IXApi request or business database access occurred. C5 must establish authenticated caller identity, downstream delegation and validated company context before any real tool can be admitted. Request bodies and write operations remain unsupported until writable-field, approval and idempotency policies exist.
