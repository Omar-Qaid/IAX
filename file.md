# IAX Project-Wide Code Review

**Review date:** 2026-08-30
**Scope:** `IXApi`, `IXApp`, repository configuration, automated tests, architecture checks, and dependency advisories.
**Review mode:** The original review was read-only; remediation changes are tracked below.

## Remediation progress

**Started:** 2026-08-30

The findings below preserve the original reviewed baseline. The current working tree now includes the first containment changes:

| Item                                    | Working-tree status                                                                       | Remaining operational action                                                         |
| --------------------------------------- | ----------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| Database password in `appsettings.json` | Removed                                                                                   | Rotate the real database credential and replace the `sa` application login           |
| Static JWT signing secret               | Removed; known placeholder values are rejected at startup                                 | Rotate the secret in every environment that may have used it                         |
| Predictable seeded administrators       | Removed; seeding now creates roles but no users                                           | Rotate/delete any previously seeded accounts and provision an administrator securely |
| Three-character password policy         | Replaced with a 12-character mixed-character policy                                       | Communicate/enforce password reset requirements for existing accounts                |
| Employee snapshot                       | Replaced with four invented records required by synthetic workflow data                   | Remove the original snapshot from Git history after coordination                     |
| Workflow master snapshot                | Replaced with one compact synthetic workflow                                              | Remove the original snapshot from Git history after coordination                     |
| Regression protection                   | Added focused tests for committed secrets, password policy, and small synthetic snapshots | Add CI enforcement in a later phase                                                  |

The external rotations and Git-history rewrite are intentionally not automated by this code change. They require environment ownership, backup, communication, and a coordinated force-push plan.

Focused containment validation passes: three security-configuration tests and two synthetic-snapshot deserialization/size tests. The complete backend run still has only the previously documented number-sequence formatting failure; all other tests in that run passed.

## Executive summary

The repository has a solid modular direction, explicit frontend dependency-boundary checks, tenant query filters, authentication fallback protection, and meaningful test suites. The current state is not ready for a production deployment, however, because several security and correctness issues are confirmed:

1. A SQL Server `sa` password and a usable JWT signing secret are committed to source control.
2. A tracked seed-data file contains 2,261 employee records with identifiable names and personnel information.
3. Password policy permits three-character passwords, and database seeding defines two administrator accounts with password `123`.
4. Workflow request endpoints require authentication but do not enforce workflow permissions, assignment, ownership, or requester access. Any authenticated user in a company can potentially enumerate and inspect all visible workflow requests in that company.
5. Both backend and frontend test suites currently fail.

These should be addressed before feature work or deployment hardening.

## Review coverage

The repository contains approximately:

- 959 C# files / 92,298 lines
- 520 TypeScript/TSX files / 47,671 lines
- 52 backend tests discovered
- 190 frontend tests discovered

The review covered:

- Application bootstrap and middleware ordering
- Authentication, authorization, JWT handling, password policy, and tenant selection
- Shared repository/service/controller infrastructure
- Workflow request and Mail page data flow
- Seed data and configuration handling
- Frontend architecture boundaries, API interception, auth storage, localization, and tests
- Backend and frontend builds/tests
- Production npm dependency advisories
- Repository automation and documentation consistency

This was a repository-wide static and automated review, not a formal penetration test, production infrastructure audit, or manual browser acceptance test of every page.

## Findings by severity

### CRITICAL-01 — Production-capable secrets are committed

**Evidence**

- `IXApi/appsettings.json:16` contains `User Id=sa;Password=123` in the database connection string.
- `IXApi/appsettings.json:39` contains a static JWT signing secret.
- `IXApi/appsettings.json:61` repeats the database password in a comment.
- Both `IXApi/appsettings.json` and `IXApi/appsettings.Development.json` are tracked by Git.
- `IXApi/README.md:9` incorrectly states that secrets are absent from committed configuration.

**Impact**

- Anyone with repository access may connect to a database if that credential is reused and the server is reachable.
- If a deployment uses the checked-in JWT secret, an attacker can mint valid tokens, including tokens with an `Admin` role, and bypass the permission system.
- Removing the strings in a later commit does not remove them from Git history.

**Required remediation**

1. Rotate the SQL Server password immediately; avoid the `sa` login for application access.
2. Rotate the JWT signing secret in every environment that may have used this value.
3. Replace committed values with empty/sentinel values and load real values from environment variables, a secret manager, or .NET user-secrets for local development.
4. Remove the secrets from Git history if the repository has been shared.
5. Add automated secret scanning in CI and a pre-commit hook.
6. Make startup reject known placeholder/default JWT secrets, not merely secrets shorter than 32 characters.

### CRITICAL-02 — Realistic employee PII is committed as seed data

**Evidence**

- `IXApi/src/Infrastructure/Persistence/Seeding/Data/LegacyOrganizationEmployeeData.json:1` is a tracked 602 KB, single-line JSON resource.
- It contains 2,261 employee entries, along with 26 departments, 106 occupations, 34 nationalities, and other organization data.
- The entries include identifiable Arabic and English names, employee codes, organization relationships, and audit identifiers.
- `LegacyOrganizationEmployeeSeeder.cs` embeds and imports this resource.

**Impact**

- This creates a privacy, repository-distribution, backup, and compliance risk.
- Git history and clones preserve the records even after a normal deletion commit.
- Developers and CI systems receive personal data without needing it to compile or test the application.

**Required remediation**

1. Confirm the provenance and authorization for this dataset with the data owner.
2. Remove it from the repository and Git history.
3. Replace it with a small, synthetic dataset containing invented names and identifiers.
4. Keep any authorized migration dataset in encrypted, access-controlled storage outside source control.
5. Audit repository access and forks if these are real employee records.

### HIGH-01 — Password policy is critically weak and default administrators use `123`

**Evidence**

- `IXApi/Bootstrap/Extensions/ServiceCollectionExtensions.cs:127-132` sets a minimum length of 3 and disables uppercase, lowercase, digit, and non-alphanumeric requirements.
- `IXApi/src/Modules/Identity/Authentication/RegisterDtoValidator.cs:24` only requires six characters, creating inconsistent validation between the API DTO and Identity.
- `IXApi/src/Infrastructure/Persistence/Seeding/Chunks/IdentitySeeder.cs:128-129` defines `sys` and `omar` administrators with password `123`.
- `IXApi/README.md:37` incorrectly claims strong password requirements are enabled.

**Impact**

- Accounts are susceptible to guessing and password spraying.
- Enabling database initialization in a non-production environment creates predictable administrator credentials.
- Non-production environments frequently contain production-like data or network access and should not be treated as harmless.

**Required remediation**

1. Use a minimum length of at least 12 characters and enable appropriate complexity or passphrase controls.
2. Align FluentValidation rules with Identity rules so the client receives accurate validation errors.
3. Do not seed fixed passwords. Generate a one-time secret, require an environment-provided bootstrap password, or use an explicit administrator provisioning command.
4. Force credential rotation for any database where these accounts already exist.
5. Add tests that assert the effective Identity options.

### HIGH-02 — Workflow requests lack resource-level authorization

**Evidence**

- `IXApi/src/Modules/Workflow/Requests/WfRequestController.cs:13` has no `DomainPermission` attribute.
- The inherited base controller has `[Authorize]`, so endpoints are authenticated, but no workflow resource/action permission is evaluated.
- `WfRequestController.cs:23-29` returns the result of `GetRequestListAsync` to any authenticated caller.
- `WfRequestService.cs:39-41` starts from all tenant-visible requests via `GetAllAsync`.
- `WfRequestController.cs:39-46` exposes mail details by request ID without verifying that the caller is the requester, an assigned performer, an authorized observer, or an administrator.
- Inherited create, update, delete, paged, and lookup actions are also reachable under authentication unless separately constrained.

**Impact**

- A normal authenticated user may enumerate workflow requests and view request fields, history, employee names, notes, and attachment metadata for other users in the same company.
- Depending on inherited routes and service behavior, users may also mutate or delete workflow records without the intended workflow permission.
- Tenant filtering limits cross-company access but does not provide row-level authorization within a company.

**Required remediation**

1. Add an explicit workflow request permission at controller/action level.
2. Apply row-level rules to list and details queries: requester, current/past assignee, delegated user, authorized workflow administrator, or another explicitly defined participant.
3. Do not expose generic inherited CRUD endpoints for workflow aggregate roots unless every operation is intentionally supported. Override/disable unsafe actions.
4. Apply the same row-level check before returning attachments or printouts.
5. Add integration tests for unrelated user, requester, assignee, manager, admin, and cross-company access.

### HIGH-03 — Backend number-sequence formatting is broken

**Evidence**

- Backend test `AdministrationCharacterizationTests.Number_sequence_format_prefers_annotated_format_and_replaces_date_tokens` fails at `IXApi/Tests/AdministrationCharacterizationTests.cs:62`.
- Expected: `DOC-20260830-00042`
- Actual: `DOC-20260830-42`
- `IXApi/src/Modules/Administration/NumberSequences/SysNumberSequenceService.cs:278-299` selects `AnnotatedFormat` for output but derives padding only by counting `#` characters in `Format`. With `Format = "IGNORED-{SEQ}"`, padding falls back to one character.

**Impact**

- Generated business document identifiers do not follow configured formatting.
- Integrations, sorting, reconciliation, and uniqueness assumptions can be affected.

**Required remediation**

Define padding as explicit metadata or parse it from the selected format consistently. Add cases for annotated format, legacy hash format, no padding, values wider than the configured width, and date boundaries.

### MEDIUM-01 — Frontend test suite is not green

**Evidence**

`npm run test:run` result:

- 56 test files passed; 3 failed.
- 189 tests passed; 1 failed.
- `IXApp/src/test/modules/ProcessBuilderPage.test.tsx:569` expects the “Process structure” heading to disappear after closing, but it remains mounted.
- `OfficialFormRuntime.test.tsx` and `WorkflowOfficialFormPage.test.tsx` fail before collecting tests because Vite cannot resolve `qrcode.react` imported at `RuntimePrintTemplate.tsx:4`.
- `package.json` and `package-lock.json` declare `qrcode.react`, but `npm ls qrcode.react --depth=0` reports it missing from the current installation.

**Impact**

- A responsive Process Builder interaction is either regressed or its contract/test is stale.
- Official-form runtime behavior is untested in the current environment.
- The local dependency tree is not synchronized with the lock file.

**Required remediation**

1. Run a clean `npm ci` and rerun the tests to separate installation drift from a repository defect.
2. Decide whether closing the responsive drawer must unmount/hide the heading or whether the assertion should use accessibility visibility rather than DOM absence.
3. Ensure CI always installs from `package-lock.json` and runs the complete test suite.

### MEDIUM-02 — Frontend verification fails before reaching most checks

**Evidence**

- `npm run verify` stops at `format:check`.
- Prettier reports 21 files with style violations, including `package.json`, audit scripts, architecture documentation, shared lookup/page components, `SalesOrderPage.tsx`, tests, and both translation files.
- Architecture audit, UTF-8 audit, lint, and type-check pass when run separately.

**Impact**

- The documented validation command is red.
- Because scripts use `&&`, formatting prevents lint, type-check, tests, and build from running in that command.

**Required remediation**

Format the listed files, then rerun `npm run verify`. In CI, consider running independent checks as separate jobs so all failures are visible in one run.

### MEDIUM-03 — Mail “current stage” is fabricated from list position

**Evidence**

- `IXApp/src/modules/workflow/pages/MailPage.tsx:815` assigns `stepNumber: index + 1` after sorting requests by request date.
- `MailPage.tsx:715` displays this value as the current workflow stage.
- Filtering among All, Inbox, Sent, and Important changes list membership and therefore can change the displayed “stage” for the same request.

**Impact**

- Users receive incorrect workflow state information.
- The value is not stable and is unrelated to workflow steps or assignment history.

**Required remediation**

Return the actual current step number/name from the API or derive it from the request’s assignment history. Do not infer business state from UI list order.

### MEDIUM-04 — Mail loads all requests and processes, then filters client-side

**Evidence**

- `MailPage.tsx:782-789` requests the complete workflow request and process collections.
- `MailPage.tsx:796-817` sorts, enriches, and filters them in memory.
- `WfRequestService.GetRequestListAsync` materializes all tenant-visible requests and performs additional worker and party lookups.

**Impact**

- Initial load time and memory use grow with the full workflow history.
- The API returns more sensitive data than the current folder/view needs.
- Client folder definitions are status approximations, not mailbox semantics tied to the current user.

**Required remediation**

Create a paged, server-filtered mail endpoint with explicit folder semantics, search, sort, projection, and row-level authorization. Return process/requester display fields in the projection to avoid full process downloads and secondary detail queries.

### MEDIUM-05 — Unique-value validation fails open on database errors

**Evidence**

- `IXApi/src/Modules/Workflow/Requests/ValidationEngine.cs:234-242` executes the uniqueness count query inside a broad `catch` and returns `false` for any error.
- `false` is indistinguishable from “no duplicate exists.”

**Impact**

- Schema, connection, timeout, mapping, or permission errors silently disable a configured uniqueness rule.
- Invalid duplicate data can be accepted, while operations leave no direct diagnostic from this code path.

**Required remediation**

Log the exception with rule/table/column context and fail the validation operation safely. Use a database unique constraint as the final concurrency-safe enforcement mechanism; application validation alone is race-prone.

### MEDIUM-06 — Frontend production dependency advisory

**Evidence**

- `npm audit --omit=dev --audit-level=moderate` reports two moderate vulnerabilities.
- Path: `exceljs -> uuid < 11.1.1`.
- Advisory: `GHSA-w5hq-g745-h8pq`, missing buffer bounds checking for UUID v3/v5/v6 when a buffer is supplied.
- npm only proposes a forced change to `exceljs@3.4.0`, which is a breaking downgrade and should not be applied automatically.

**Impact**

The direct exploitability appears limited by how `exceljs` uses `uuid`, but this is a production dependency and must be tracked rather than ignored.

**Required remediation**

Review the dependency call path, monitor `exceljs` for a release with a fixed transitive dependency, consider an audited override only if compatibility is proven, and document risk acceptance until upgraded.

### LOW-01 — Unreachable code remains in the backend build

**Evidence**

- Release build emits `CS0162` at `IXApi/src/Infrastructure/Persistence/Seeding/Chunks/WorkflowSeeder.cs:254`.
- `WorkflowSeeder.cs:252` wraps a large request-seeding branch in `if (false)`.

**Impact**

- Dead code obscures the actual seed behavior and creates warning noise that can hide new warnings.

**Required remediation**

Delete the obsolete branch or gate optional sample data behind a named configuration option with tests.

### LOW-02 — Documentation contradicts runtime behavior

**Evidence**

- `IXApi/README.md:9` says committed configuration has no secrets, but secrets are present.
- `IXApi/README.md:18` says database initialization runs by default, while `appsettings.json` sets `DatabaseInitialization:Enabled` to `false` and `Program.cs` defaults it to `false`.
- `IXApi/README.md:37` says strong passwords are required, while the effective minimum is three characters with all complexity flags disabled.

**Impact**

Operators and reviewers can make incorrect security and deployment assumptions.

**Required remediation**

Treat documentation assertions as testable contracts. Update the README after correcting the implementation, and add configuration tests for important defaults.

### LOW-03 — No repository CI workflow is present

**Evidence**

- `.github/workflows` exists but contains no tracked workflow files.
- The repository has useful validation scripts, but nothing in the repository enforces them for pushes or pull requests.

**Impact**

- Formatting, tests, secret exposure, dependency advisories, and architecture regressions can reach the main branch unnoticed.

**Required remediation**

Add CI jobs for frontend install/verify, backend restore/build/test, secret scanning, dependency review, and artifact/report retention. Use pinned runtime versions and lock-file installs.

## Validation results

| Check                                | Result       | Notes                                                                                         |
| ------------------------------------ | ------------ | --------------------------------------------------------------------------------------------- |
| Git worktree before report           | Pass         | Clean; only this report is intentionally added afterward                                      |
| Frontend architecture audit          | Pass         | 0 known layer edges; 0 icon-barrel debt                                                       |
| Frontend encoding audit              | Pass         | UTF-8 mojibake check passed                                                                   |
| Frontend ESLint                      | Pass         | No lint errors                                                                                |
| Frontend TypeScript                  | Pass         | `tsc --noEmit` succeeded                                                                      |
| Frontend formatting                  | Fail         | 21 files reported by Prettier                                                                 |
| Frontend tests                       | Fail         | 3 files failed; 1 assertion failed; 2 suites could not resolve a missing installed dependency |
| Frontend full verify                 | Fail         | Stops at formatting                                                                           |
| Frontend production dependency audit | Fail         | 2 moderate advisories through `exceljs -> uuid`                                               |
| Backend build/test                   | Fail         | Build succeeds with one unreachable-code warning; 1 of 52 tests fails                         |
| Backend dependency advisory audit    | Inconclusive | NuGet advisory service could not be resolved after retry                                      |

## Positive observations

- Frontend dependency-boundary and encoding audits are present and passing.
- ESLint and TypeScript checks pass independently.
- The backend establishes an authenticated fallback policy, validates JWT issuer/audience/signature/lifetime, uses zero clock skew, and checks token revocation.
- JWT access tokens are kept in memory/session storage rather than persistent local storage; legacy local-storage tokens are removed.
- Company selection is checked by middleware, and EF Core applies company/soft-delete query filters.
- CORS uses an explicit origin list rather than a wildcard and validates that the list is non-empty.
- Raw SQL used by workflow uniqueness validation derives table and column identifiers from EF metadata and parameterizes the value, reducing injection risk.
- XML parsing prohibits DTD processing and disables the resolver.
- Regex validation uses a timeout.
- Database initialization is blocked in Production when explicitly enabled.

These controls are valuable, but they do not compensate for the committed signing secret, weak credentials, PII, or missing row-level workflow authorization.

## Recommended remediation order

### Phase 0 — Immediate containment

1. Rotate the database and JWT secrets.
2. Remove/rotate seeded administrator credentials.
3. Restrict repository access while the employee dataset is assessed.
4. Remove secrets and PII from the working tree and history using an approved history-rewrite process.

### Phase 1 — Authorization and identity

1. Implement workflow request permission and row-level participant checks.
2. Disable unsafe inherited CRUD actions for workflow requests.
3. Strengthen and unify password policy.
4. Add authorization integration tests covering normal and adversarial access paths.

### Phase 2 — Restore a green baseline

1. Reinstall frontend dependencies from the lock file.
2. Fix the number-sequence formatting defect.
3. Resolve the Process Builder drawer behavior/test.
4. Remove dead seeding code.
5. Format the 21 reported files.
6. Rerun frontend `verify` and backend tests until both are green.

### Phase 3 — Scalability and resilience

1. Replace Mail’s full collection load with a paged, user-scoped endpoint.
2. Return authoritative current workflow step data.
3. Make uniqueness validation fail safely and back it with database constraints.
4. Resolve or explicitly accept the `exceljs/uuid` advisory.

### Phase 4 — Prevent regression

1. Add CI workflows.
2. Add secret and PII scanning.
3. Add dependency advisory checks.
4. Enforce clean builds, tests, formatting, and architecture audits on pull requests.
5. Keep security-related README claims synchronized with tested configuration.

## Suggested acceptance criteria

The project should not be considered release-ready until all of the following are true:

- No real credentials or personal datasets are present anywhere in reachable Git history.
- All potentially exposed secrets have been rotated.
- No predictable administrator credentials are created by seeding.
- Workflow requests and documents have tested row-level authorization.
- Frontend `npm ci && npm run verify` passes in a clean environment.
- Backend restore/build/test passes without warnings or failures.
- Dependency advisory checks complete and all unresolved production advisories have an owner and documented decision.
- CI enforces the above checks on every pull request.
