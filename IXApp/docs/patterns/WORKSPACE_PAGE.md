# Workspace pattern

The implemented workspace family contains:

- `WorkspacePage`: page container/header and vertical content stack;
- `WorkspaceSection`: titled grouping surface;
- `WorkspaceTile`: KPI/action tile with optional icon, value, change indicator, color, and click behavior.

The pattern does not fetch metrics, impose a chart library, or coordinate filters. The routed dashboard supplies mock-derived values. Module workspaces should own queries and pass rendered sections/tiles into the pattern, while preserving responsive MUI grid composition and accessible click behavior.
