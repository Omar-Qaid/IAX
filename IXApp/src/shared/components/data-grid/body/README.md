# DataGrid body

Renders desktop grid rows and cells. `GridRow` composes cells and row actions/context menus; `GridCell` handles cell presentation/edit interaction; `SkeletonRows` covers pending content. `index.ts` is the local export boundary.

This layer receives processed grid state and callbacks from the parent; it does not fetch records.

[DataGrid](../README.md)
