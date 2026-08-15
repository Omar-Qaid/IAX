# Inquiry pattern status

`src/patterns/inquiry/InquiryPage.tsx` and `types.ts` are currently empty. `InquiryFilterPanel.tsx` exists, but there is no complete reusable inquiry page contract.

Do not import or document `InquiryPage` as implemented. Until the scaffold is completed, compose a read-only module page from `PageContainer`, filters, and `DataGrid`, and keep mutations disabled. A completed pattern should define filter ownership, URL synchronization, loading/error behavior, and tests before this status changes.
