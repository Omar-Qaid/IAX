# WfRequestPrintVersions

Associates a request with a selected print-template version.

- References `WfRequests`, `WfPrintTemplates`, and `WfPrintTemplateVersions`.
- Stores selection timestamp and selecting user.
- Intended to pin historical official output to a stable template version.
- Tracking seed creates or repairs the association for each representative request/template pair.
- Current `GetPublishedForRequestAsync` still resolves the template's current published version rather than consulting this association; full historical pinning is therefore not yet enforced by that endpoint.

## `wf.sql` snapshot

- **Not present**. Historical request-to-template version pinning is a current-project addition.
