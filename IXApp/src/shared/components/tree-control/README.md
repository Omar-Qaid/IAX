# TreeControl

`TreeControl<T>` renders compact, recursive hierarchy data with per-branch expansion,
Expand/Collapse all, selected-row highlighting, scrolling, hover feedback, and RTL-safe
indentation.

Supply domain data through `nodes` and map it with `TreeControlConfig<T>` functions for
identity, labels, children, and optional branch detection. This keeps domain-specific data
and API behavior outside the shared component.
