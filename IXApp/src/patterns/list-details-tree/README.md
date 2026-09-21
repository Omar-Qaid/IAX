# List Details Tree pattern

`ListDetailsTreePage` composes the established enterprise `ListDetailsPage` controller and detail surface with a searchable hierarchical navigation pane. Use it when parent/child records select an adjacent editor, as in category trees, organization trees, and menu structures.

The feature supplies the normal list-details data source, CRUD, fields, sections, permissions, and validation configuration plus a `tree` contract containing `getParentId` and accessibility labels. Search results retain their ancestor path, selection is keyboard-accessible through native tree semantics, cycles are guarded during rendering, and the existing responsive drawer and resizable navigation behavior are preserved.
