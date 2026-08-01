# Master Details Grid

## Purpose
A variation of Master Details where BOTH the master and detail sections are DataGrids stacked vertically or side-by-side.

## When to use
- Comparing multiple parent records and their children simultaneously.
- Worklists where the user needs to quickly scan through parent headers and view line items.

## Folder structure
```text
src/patterns/master-detail-grid/
├── MasterDetailGridPage.tsx   # Pattern component
└── types.ts                   # Pattern type exports
```

## Required components
```text
MasterDetailGridPage
├── PageHeader
├── ActionPane
├── Top Grid (Master Records)
└── Bottom Grid (Detail Records)
```

## Data flow
```text
Top Grid row selection → Updates `selectedMasterId` state → Bottom Grid query refetches using `selectedMasterId`.
```

## Examples
Voucher Inquiry where top is the voucher header and bottom is voucher lines.

## Rules
- Always show the currently selected master record context above the detail grid.
- If no master is selected, the detail grid must display an empty state.

## Description UI
Two distinct DataGrid components separated by a horizontal splitter. The top grid acts as the driver. Clicking a row in the top grid highlights it, and the bottom grid instantaneously populates with the child rows associated with that highlighted parent.
