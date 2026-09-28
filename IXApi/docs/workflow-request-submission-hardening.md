# Request submission fixes

## Contracts and rollout

- Requests without attachments continue to use `POST /api/v1/WfRequest/submit` with JSON.
- Requests with attachments use `POST /api/v1/WfRequest/submit-with-files` with multipart form data: `submission` contains the JSON DTO and repeated `files` parts contain the files. `uploadTargets` must contain one entry per file, in the same order. A null control/option pair means a request-level attachment; a control ID targets its detail; a control and option ID target that option's supporting document.
- The multipart action preserves the document-create permission requirement. Upload metadata is derived from received files. JSON-only metadata cannot satisfy file requirements. Process-level `MandatoryDocuments` requires an actual attachment.
- Deploy the frontend and API together. Apply `20260928111242_ExpandWorkflowRequestAnswers` before using larger answers. The migration changes only `WfRequestDetails.ControlValue` to `nvarchar(max)`; application validation limits each answer to 1,000,000 characters. Its downgrade refuses to truncate existing answers longer than 255 characters.

## Persistence and failure behavior

The submission coordinator owns a serializable transaction encompassing validation, request/details, variables, assignments, notification intents, and document records. It commits only after file storage succeeds. SQL Server also holds a company/process application lock before uniqueness checks until the transaction ends, preventing two service submissions from passing the same uniqueness check concurrently.

On failure the coordinator rolls back database changes, deletes earlier files from that attempt, and detaches newly tracked entities before a transient retry. Cleanup uses a non-cancelled token and attempts all files. Cleanup failures are reported with the original error. Physical storage is not a distributed transaction: process termination or an ambiguous database commit still requires operational reconciliation. The endpoint does not provide a general idempotency key for requests without unique answers.

The browser sends the entire submission in one call. A rejected submission retains entered answers/files for retry. Repeat clicks and submission after success are guarded; changing process clears queued request-level attachments. Print-template mode displays option attachments and dependencies.

## Validation

- Empty reference lists reject supplied employee/showroom values.
- Table column definitions are not treated as selectable answers.
- Unsupported validation types fail closed and cannot be newly configured through the request-validation DTO validator.
- `mask` / `inputMask` use the same tokens in browser and server: `0` or `9` requires one ASCII digit, `A` or `a` one ASCII letter, and `*` one ASCII alphanumeric character. Other characters are literals; backslash escapes a token. Masks match the complete value. Example: `000-AA` accepts `123-AB`.
- Database uniqueness rules remain server-side checks; they are recognized rather than rejected as unsupported local rules.

## Verification scope

Focused tests cover file metadata rejection, upload ownership transport, required print uploads, failed-submit retry, repeat-click suppression, process attachment isolation, unknown rules, masks, empty references, larger-answer persistence, and transactional rollback/file cleanup followed by retry. SQLite verifies database rollback; it does not verify SQL Server application-lock behavior under real concurrency.

Process repeat policy and configured-user eligibility still need business semantics. Scheduled request submission remains an explicit unimplemented extension point; these are outside the seven confirmed defects addressed here.
