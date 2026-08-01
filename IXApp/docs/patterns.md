# Page Patterns Layer Documentation (`src/patterns`)

## 1. Purpose and Responsibilities
The `patterns` layer defines reusable enterprise page architectures for **IXApp**. Inspired by Microsoft Dynamics 365 Finance & Operations page forms, these templates provide standardized layouts for common business scenarios (simple reference lists, split master-details, tabbed setup forms, operational workspaces, and header-lines transactional documents).

Page patterns contain **no domain business rules**. They structure slots for header titles, action panes, filter bars, data grids, FastTabs, detail panels, and status bars.

---

## 2. Folder Structure
```text
src/patterns/
├── simple-list/               # Reference & setup list pattern
│   ├── SimpleListPage.tsx      # Standard single-grid setup page
│   └── useSimpleListPage.ts   # State management hook for simple lists
├── list-details/              # Split-view master-detail pattern
│   ├── ListDetailsPage.tsx    # Split layout container (Grid + FastTabs)
│   ├── ListDetailsLayout.tsx  # Responsive flex layout
│   └── useListDetailsPage.ts  # Master record selection & dirty tracking hook
├── master-form/               # Form setup & parameters pattern
│   ├── MasterFormPage.tsx     # Tabbed parameter setup page
│   └── useMasterFormPage.ts   # Form submission & dirty state hook
├── workspace/                 # Dashboard & operational work center
│   ├── WorkspacePage.tsx      # Dashboard container
│   ├── WorkspaceTile.tsx      # KPI summary tile card
│   └── WorkspaceSection.tsx   # Workspace grouping section
└── document/                  # Transactional document pattern
    ├── DocumentPage.tsx       # Header-lines document container
    ├── DocumentHeader.tsx     # Document header form section
    ├── DocumentLines.tsx      # Line item DataGrid section
    ├── DocumentTotals.tsx     # Summary totals calculation panel
    └── useDocumentPage.ts     # Document state & lines editing hook
```

---

## 3. File Naming Conventions
- **Components:** `PascalCase.tsx` (e.g., `SimpleListPage.tsx`, `ListDetailsPage.tsx`, `WorkspaceTile.tsx`).
- **Pattern Hooks:** `camelCase.ts` starting with `use` (e.g., `useSimpleListPage.ts`, `useListDetailsPage.ts`, `useDocumentPage.ts`).

---

## 4. Pattern Architecture Specifications

### 4.1 Simple List (`@patterns/simple-list`)
- **Use Case:** Setup & reference tables (Currencies, Customer Groups, Payment Terms, Units).
- **Structure:**
  ```text
  SimpleListPage
  ├── PageHeader
  ├── ActionPane (New, Edit, Delete, Save, Cancel, Refresh)
  ├── DataGrid (Virtualized with inline editing & temporary rows)
  └── SelectionSummary / PageFeedback
  ```
- **Lifecycle Rules:** Page-level Save and Cancel. Edits remain in temporary state until user clicks Save.

### 4.2 List and Details (`@patterns/list-details`)
- **Use Case:** Core master entities requiring grid selection alongside detailed form views (Customers, Vendors, Items).
- **Structure:**
  ```text
  ListDetailsPage
  ├── PageHeader
  ├── ActionPane
  └── SplitView Layout
      ├── Left: Record Grid (List of customers with quick search)
      └── Right: Details Panel (FastTabs: General, Addresses, Financial, Contact)
  ```
- **Dirty State Rule:** Switching selected row when right details form is dirty prompts unsaved changes warning dialog.

### 4.3 Master Form (`@patterns/master-form`)
- **Use Case:** Application setup, module parameters, and posting configurations.
- **Structure:**
  ```text
  MasterFormPage
  ├── PageHeader
  ├── ActionPane (Save, Cancel, Refresh)
  └── FastTabs Container (General, Localization, UI Preferences, API Config)
  ```

### 4.4 Workspace (`@patterns/workspace`)
- **Use Case:** Operational dashboards, KPI cards, quick links, and active work queues.
- **Structure:**
  ```text
  WorkspacePage
  ├── WorkspaceHeader (Title, company selector)
  ├── SummaryTiles (KPI Tiles: Open Orders, Total Customers, Overdue Balance)
  ├── Charts Section
  └── WorkLists (Recent Orders DataGrid, Quick Links)
  ```

### 4.5 Header-Lines Document (`@patterns/document`)
- **Use Case:** Complex transactional documents (Sales Orders, Purchase Orders, Invoices).
- **Structure:**
  ```text
  DocumentPage
  ├── DocumentHeader (Order #, Customer, Date, Status, Currency)
  ├── ActionPane (Save, Confirm, Post, Cancel, Print)
  ├── LinesDataGrid (Line #, Item, Qty, Unit Price, Discount, Total)
  └── DocumentTotals (Subtotal, Tax Total, Discount Total, Grand Total)
  ```

---

## 5. Page Modes & State Rules
Standard page modes supported across patterns:
```ts
export type PageMode = 'view' | 'create' | 'edit' | 'copy' | 'readonly' | 'process';
```

| Mode | Form Behavior | DataGrid Behavior | Primary Actions |
|---|---|---|---|
| `view` | Read-Only | Selectable | New, Edit, Refresh |
| `create` | Editable | Context-dependent | Save, Cancel |
| `edit` | Editable | Context-dependent | Save, Cancel, Refresh |

---

## 6. Design Patterns
- **Template Method Pattern:** Patterns enforce standard slot placement (`PageHeader` $\rightarrow$ `ActionPane` $\rightarrow$ `Content` $\rightarrow$ `Feedback`), while domain modules inject specific columns, fields, and actions.
- **Unsaved Changes Guard:** Pattern hooks track `isDirty` state and intercept route changes or record selections when uncommitted edits exist.

---

## 7. Dependencies & Layer Rules
- **Allowed:** `@patterns` $\rightarrow$ `@shared`, `@core`.
- **Forbidden:** `@patterns` must **never** import from `@modules` or `@app`.

---

## 8. Best Practices & Guidelines
- Domain pages inside `@modules` should compose a pattern from `@patterns` rather than constructing custom layouts from scratch.
- Ensure all action buttons in the `ActionPane` handle loading states (`loading={isSaving}`) to prevent duplicate form submissions.

---

## 9. Do's and Don'ts
- **DO:** Use `SimpleListPage` for setup tables requiring fast grid edits.
- **DO:** Use `ListDetailsPage` when managing master entity details.
- **DON'T:** Put business API URLs or Axios calls inside `@patterns`.
- **DON'T:** Hardcode module titles inside pattern templates.

---

## 10. Examples

### Implementing a Module Page using `ListDetailsPage`
```tsx
export function CustomersPage() {
  const { customers, selectedId, setSelectedId, saveCustomer } = useCustomers();

  return (
    <ListDetailsPage
      title="Customers"
      actions={customerActions}
      records={customers}
      selectedId={selectedId}
      onSelectRecord={setSelectedId}
      renderDetails={(customer) => (
        <CustomerDetailsForm customer={customer} onSave={saveCustomer} />
      )}
    />
  );
}
```

---

## 11. Performance Considerations
- Detail panes in `ListDetailsPage` are conditionally rendered based on active selection to prevent unnecessary form re-renders.
- `DocumentLines` DataGrid uses row key tracking to preserve cursor focus during line item calculations.
