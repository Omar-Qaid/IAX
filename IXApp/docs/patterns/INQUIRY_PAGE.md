# Inquiry Page

## Purpose
A read-only analysis page with advanced filter panels and result DataGrids.

## When to use
- Transaction Inquiry, Audit Log Viewer, Balance Inquiry.
- Any page presenting filtered, read-only query results.

## Folder structure
```text
src/patterns/inquiry/
├── InquiryPage.tsx            # Pattern component
├── InquiryFilterPanel.tsx     # Filter sidebar
└── types.ts                   # Pattern type exports
```

## Required components
```text
InquiryPage
├── ActionPane (Export, Print)
├── InquiryFilterPanel
└── AppDataGrid (read-only)
```

## Data flow
```text
Filter changes → URL search params update → TanStack Query refetch → Grid re-renders.
```

## Examples
Voucher Transactions Inquiry.

## Rules
- DataGrid must be read-only.
- Filters should map to URL parameters for bookmarking.
- Support CSV export.

## Description UI
An analytical interface featuring a collapsible or persistent advanced filter sidebar on the left or top. The primary workspace is a read-only DataGrid optimized for data density and sorting/filtering. Actions are limited to exports and print commands.
