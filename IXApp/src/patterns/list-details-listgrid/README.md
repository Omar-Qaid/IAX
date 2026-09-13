# list-details-listgrid

Composes the shared ListDetailsPage master list/header/sections with a TabularDetailPanel child grid and selected-row editor. Defaults: master list 264 px; child grid 334 px with 31 px rows. Child details stack below the grid on smaller screens; the master list starts closed on mobile.

Domain code owns row selection, remote data, validation and persistence. Use `interactionLocked` while a child editor is dirty or saving, and disable child mutations while editing the master. Register drafts with `useUnsavedChanges` to protect route navigation, unload and company switching. Master CRUD synchronizes the shared query cache; child CRUD owns its own cache.

Customer posting profiles is the first consumer, using CustLedger and CustLedgerAccounts. Its Account/Group number reuses LookupField: Table loads CustTable customers, Group loads CustGroup groups, and All clears/disables the lookup. Reference validation runs again before save. API failures keep the draft open.
