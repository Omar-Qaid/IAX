# Inquiry / Read-only Page

## 1. Pattern Purpose & When to Use It
- **Purpose**: A read-only analytical view with advanced filtering panels and high-density result grids.
- **When to Use**: - Audit Logs, Ledger Transactions, Balance Inquiry, Customer Transaction History.

## 2. UI Structure & Layout
Top filter drawer/panel followed by full-screen read-only AppDataGrid.

## 3. Page Sections & Components
- PageHeader
- ActionPane (Export, Print, Refresh)
- InquiryFilterPanel (Date, Account, Status filters)
- Read-only AppDataGrid

## 4. Folder Structure
```text
src/patterns/inquiry/
├── InquiryPage.tsx
├── InquiryFilterPanel.tsx
└── types.ts
```

## 5. Required Reusable Components
- InquiryFilterPanel
- AppDataGrid (read-only mode)

## 6. Data Flow & State Management
- **Data Flow**: User applies filters -> URL query params updated -> query refetches results.
- **State Management**: - Filter state mapped to URL search params.

## 7. Actions & Commands
- Apply Filter, Reset Filter, Export CSV, Print View

## 8. Validation Rules
- Valid date range comparisons.

## 9. Naming Conventions & Best Practices
- **Naming Conventions**: - *InquiryPage.tsx
- **Best Practices**: - Disable cell editing in inquiry grids.

## 10. Do's and Don'ts Rules
DO: Support URL bookmarking of filter criteria.
DON'T: Allow data mutations on inquiry pages.

## 11. Implementation Example
```tsx
// LedgerTransactionsInquiryPage
```
