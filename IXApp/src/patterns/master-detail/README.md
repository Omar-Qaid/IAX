# Master-detail pattern

`MasterDetailPage` provides the shared action pane, filterable record navigation, selected-record title and status, and detail content slot. Selection is controlled by the caller, so pages can keep it in route state. Filtering only changes the visible navigation records. Use existing DataGrid and accordion components inside the detail slot for header and line sections. Business data and persistence belong to the consuming page.
