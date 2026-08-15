# Master form pattern

`src/patterns/master-form/MasterFormPage.tsx` is an implemented presentation shell: page container, header, optional action pane, and bordered content surface. It does not create React Hook Form, query, validation, or dirty state automatically.

`useMasterFormPage` provides local `view`/`edit`/`create` mode and record state. A module may instead use React Hook Form and Zod directly, as administration settings do.

Use this pattern for a singleton form when the thin shell is enough. Use `SetupPage` for config-driven categorized fields and navigation.
