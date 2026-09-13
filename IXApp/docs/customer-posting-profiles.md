# Customer posting profiles

Route: `/accounts-receivable/customer-posting-profiles`, Accounts receivable → Setup.

The `list-details-listgrid` pattern reuses ListDetailsPage, TabularDetailPanel, the enterprise DataGrid, shared lookup and confirmation controls. Profile and child account saves are separate API operations. PostingProfile keys are immutable after creation because account rules reference that alternate key.

| Account code | Account/Group number                       | Saved value            |
| ------------ | ------------------------------------------ | ---------------------- |
| Table (0)    | Customer lookup from `/v1/CustTable/list`  | Customer accountNumber |
| Group (1)    | Customer-group lookup from `/v1/CustGroup` | Group custGroupId      |
| All (2)      | Empty, disabled lookup                     | Empty string           |

Switching account code clears the previous reference. Table/Group saves require a current matching reference; lookup failures and missing/deleted references prevent submission. This is frontend validation, not a replacement for backend authorization or server-side integrity checks. Existing lookup endpoint permissions remain in force.

The resolver already prioritizes Table, then Group, then All. Master saves/deletes update the React Query cache. The shared unsaved-draft registry protects browser unloads, React Router navigation (including history navigation), and company changes. Rejecting a company change leaves storage and query invalidation unchanged. Approved discard proceeds without changing saved data.

The current checkout was missing the previously created posting-profile files; the page, pattern and targeted tests were restored while applying these fixes. Other page routes remain unchanged. AppRoutes uses React Router's data router so its supported navigation blocker can protect all registered editors.

Close remains blank/unavailable: neither persisted entity defines it. Ledger/interest fields retain their numeric identifiers. Additional account DTO fields and rowVersion are preserved. No backend schema or authorization changes are included.

## Verification

- Production TypeScript/Vite build passed; the existing large-chunk advisory remains.
- Four Playwright tests passed: Table/Group/All lookup and payload behavior, missing/deleted-reference rejection, profile cache after SPA navigation, and discard-confirmation behavior.
- Twenty focused unit/regression tests passed, covering company-switch cancellation/confirmation, multiple dirty editors, list-details persistence, navigation configuration, routing, shell and breadcrumbs.
- Targeted ESLint and diff whitespace checks passed. The customer lookup screenshot was visually inspected.
- Browser API responses were intercepted for these tests; authenticated live database persistence was not exercised.
