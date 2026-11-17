# Foundation record previews

`foundationRecordPreviews.ts` registers the D365 hover cards for currencies, sales tax groups, item sales tax groups and tax authorities (code and description) and exports their entity names as `FOUNDATION_RECORD_PREVIEWS`. Each entry declares its `masterRoute`, so fields and lookups pointing at those forms show the card automatically.

Imported once from `app/configuration/recordPreviews.ts`.

[Record preview](../../../../shared/components/record-preview/README.md) · [Foundation](../README.md)
