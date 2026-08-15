# Dialogs

`src/shared/components/dialogs` contains reusable modal and drawer interactions:

- `AppDialog`: base title/content/actions shell.
- `ConfirmationDialog`: generic explicit confirmation.
- `DeleteConfirmationDialog`: destructive confirmation variant.
- `FormDialog`: submit/cancel form shell with pending state.
- `ProcessDialog`: execute/cancel shell with progress and error display.
- `AppHistoryDrawer`: history side drawer.
- `LookupDialog`: shared dialog-style lookup wrapper retained alongside the richer `shared/components/lookups/LookupDialog`.
- `useDialog`: local open/data/loading/error state helper.

The caller owns domain mutations and closes the dialog after the operation outcome is known. Disable submission during pending work, keep a programmatic title, and provide accessible labels for close buttons. Use the lookup subsystem for selectable grid data rather than building a business-specific dialog in `shared`.
