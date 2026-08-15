# Action pane

`src/shared/components/action-pane` provides the compact command surface used by enterprise pages.

## Components

- `ActionPane`: root toolbar with `default` or `flat` variants and an `endActions` slot.
- `ActionPaneGroup`: optional labeled group.
- `ActionPaneButton`: icon/label action with disabled, loading, and permission support through `ActionDefinition`.
- `ActionPaneMenu`: menu composed from action definitions.
- `ActionPaneDivider`: vertical divider.
- `EnterpriseCrudActions`: edit/new/delete or save/cancel command set.
- `EnterpriseCommandUtilities`: personalize, guide, notifications, refresh, and open-window affordances.
- `actionDefinitions.ts`: typed helpers for defining action arrays.

Actions emit callbacks; they do not perform API work. Pass pending state from the owning page and use permission strings on commands that require authorization. Group business lifecycle commands in the page, not inside the shared component.

Use path imports for icons and provide accessible labels for icon-only actions. On narrow screens, preserve horizontal access or move secondary commands into a menu rather than removing primary actions.
