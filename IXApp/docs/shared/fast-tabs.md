# FastTabs

FastTabs are vertical collapsible form sections:

- `FastTabs` supplies the stack container.
- `FastTab` wraps one MUI accordion and accepts title, summary, actions, expansion defaults/control, disabled, and error-related presentation through its actual props.
- `FastTabHeader` and `FastTabSummary` are small presentation helpers for custom compositions.

Use FastTabs when multiple sections should remain vertically discoverable. Use `TabbedDetailsPage` when exactly one horizontal panel should be shown. Expansion is local unless the caller passes controlled values. FastTabs do not inspect React Hook Form automatically; the caller derives and supplies any validation/error indicator supported by the component contract.

Keep summaries short, default-expand the most important section, and do not place an unbounded non-virtualized list inside an accordion.
