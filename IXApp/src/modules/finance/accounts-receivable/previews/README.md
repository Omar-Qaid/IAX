# Accounts-receivable record previews

`accountsReceivableRecordPreviews.ts` registers the D365 hover cards for this module's master data (customers, customer groups) and exports their entity names as `ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS`. Each entry declares its `masterRoute`, so form fields and lookups pointing at that master form get the preview automatically; grid columns opt in with `preview: { entity }`.

The file is imported once from `app/configuration/recordPreviews.ts`. Add a new entity here with a loader from the shared subsystem (`pagedRecordLoader` for BaseController endpoints, `listRecordLoader` for cached lists).

[Record preview](../../../../shared/components/record-preview/README.md) · [Accounts receivable](../README.md)
