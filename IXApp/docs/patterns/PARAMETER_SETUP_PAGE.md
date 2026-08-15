# Setup pattern

`SetupPage` is an implemented, config-driven parameter editor. It accepts navigation items, accordion sections, initial values, localized labels, optional header content, and an async `onSave` callback.

Supported field types are text, number, boolean, and select. State and dirty comparison are local; `useUnsavedChanges` protects browser unload. The page shows a pending-disabled save command and a success snackbar after save. It stacks navigation/content on small screens and uses a split view from `md`.

This pattern does not use React Hook Form or Zod automatically and does not provide field-level error mapping. Use it for bounded parameter sets; choose a module form when validation or custom controls exceed the configuration contract.
