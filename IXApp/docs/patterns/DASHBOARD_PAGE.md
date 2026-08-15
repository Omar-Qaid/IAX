# Dashboard page status

There is no `src/patterns/dashboard` implementation. The routed `modules/dashboard/pages/DashboardPage.tsx` composes `WorkspacePage`, `WorkspaceSection`, and `WorkspaceTile` with shared mock datasets.

Use the [Workspace pattern](WORKSPACE_PAGE.md) for current dashboard-style pages. Introduce a separate dashboard pattern only when a reusable chart/filter contract exists in source and has tests.
