# IXApp Design Patterns — Single Source of Truth

This document defines every supported **page pattern** in IXApp.
Each pattern is a reusable, enterprise-grade page template inspired by Microsoft Dynamics 365 Finance & Operations form types. Patterns live in `src/patterns/` and contain **zero domain business logic**.

> **Rule:** Domain modules inside `src/modules/` compose these patterns — they never build pages from scratch.

---

## Table of Contents

| #   | Pattern                                           | Documentation                                                 | Source Path                        | Status         |
| --- | ------------------------------------------------- | ------------------------------------------------------------- | ---------------------------------- | -------------- |
| 1   | [Simple List](#1-simple-list)                     | [`SIMPLE_LIST_PAGE.md`](patterns/SIMPLE_LIST_PAGE.md)         | `src/patterns/simple-list/`        | ✅ Implemented |
| 2   | [List & Details](#2-list--details)                | [`LIST_DETAILS_PAGE.md`](patterns/LIST_DETAILS_PAGE.md)       | `src/patterns/list-details/`       | ✅ Implemented |
| 3   | [Master Form](#3-master-form)                     | [`DETAILS_MASTER_FORM.md`](patterns/DETAILS_MASTER_FORM.md)   | `src/patterns/master-form/`        | ✅ Implemented |
| 4   | [Master-Detail](#4-master-detail)                 | [`MASTER_DETAILS_PAGE.md`](patterns/MASTER_DETAILS_PAGE.md)   | `src/patterns/master-detail/`      | 🔲 Scaffolded  |
| 5   | [Header-Lines Document](#5-header-lines-document) | [`HEADER_LINES_PAGE.md`](patterns/HEADER_LINES_PAGE.md)       | `src/patterns/document/`           | ✅ Implemented |
| 6   | [Workspace](#6-workspace)                         | [`WORKSPACE_PAGE.md`](patterns/WORKSPACE_PAGE.md)             | `src/patterns/workspace/`          | ✅ Implemented |
| 7   | [Inquiry](#7-inquiry)                             | [`INQUIRY_PAGE.md`](patterns/INQUIRY_PAGE.md)                 | `src/patterns/inquiry/`            | 🔲 Scaffolded  |
| 8   | [Setup](#8-setup)                                 | [`PARAMETER_SETUP_PAGE.md`](patterns/PARAMETER_SETUP_PAGE.md) | `src/patterns/setup/`              | 🔲 Scaffolded  |
| 9   | [Process / Wizard](#9-process--wizard)            | [`WIZARD_PROCESS_PAGE.md`](patterns/WIZARD_PROCESS_PAGE.md)   | `src/patterns/process/`            | 🔲 Scaffolded  |
| 10  | [Tree & Details](#10-tree--details)               | (See inline)                                                  | `src/patterns/tree-details/`       | 🔲 Scaffolded  |
| 11  | [Profile](#11-profile)                            | (See inline)                                                  | `src/patterns/profile/`            | 🔲 Scaffolded  |
| 12  | [Tabbed Details](#12-tabbed-details)              | (See inline)                                                  | `src/patterns/tabbed-details/`     | 🔲 Scaffolded  |
| 13  | [Lookup Page](#13-lookup-page)                    | (See inline)                                                  | `src/patterns/lookup/`             | 🔲 Scaffolded  |
| 14  | [Master Details Grid](#master-details-grid)       | [`MASTER_DETAILS_GRID.md`](patterns/MASTER_DETAILS_GRID.md)   | `src/patterns/master-detail-grid/` | 🔲 Scaffolded  |
| 15  | [Document Page](#document-page)                   | [`DOCUMENT_PAGE.md`](patterns/DOCUMENT_PAGE.md)               | `src/patterns/document/`           | ✅ Implemented |
| 16  | [Dashboard Page](#dashboard-page)                 | [`DASHBOARD_PAGE.md`](patterns/DASHBOARD_PAGE.md)             | `src/patterns/dashboard/`          | 🔲 Scaffolded  |

---

## Supporting Infrastructure

All patterns share common building blocks from `@shared` and common state hooks:

### Shared Page Components

| Component                                         | Import                                      |
| ------------------------------------------------- | ------------------------------------------- |
| `PageContainer`                                   | `@shared/components/page/PageContainer`     |
| `PageHeader`                                      | `@shared/components/page/PageHeader`        |
| `ActionPane`                                      | `@shared/components/action-pane/ActionPane` |
| `AppDataGrid`                                     | `@shared/components/data-grid/DataGrid`     |
| `FastTabs` / `FastTab`                            | `@shared/components/fast-tabs/FastTabs`     |
| `LoadingState` / `ErrorState` / `EmptyState`      | `@shared/components/feedback/*`             |
| `ConfirmationDialog` / `DeleteConfirmationDialog` | `@shared/components/dialogs/*`              |

### Shared Page State Hooks

| Hook                  | Purpose                                                                                                        |
| --------------------- | -------------------------------------------------------------------------------------------------------------- |
| `usePageMode()`       | Manages `PageMode` transitions (`'view'` \| `'create'` \| `'edit'` \| `'copy'` \| `'readonly'` \| `'process'`) |
| `useListPage()`       | Full lifecycle state for list-based pages (load, save, delete, dirty tracking, selection)                      |
| `useDocumentPage()`   | Full lifecycle state for transactional documents (load by ID, process actions, dirty tracking)                 |
| `useUnsavedChanges()` | Prompts user when navigating away from dirty forms                                                             |
| `usePageRefresh()`    | Handles F5 / Refresh with dirty-state confirmation                                                             |
| `useNotifications()`  | Triggers global toast notifications (`notifySuccess`, `notifyError`)                                           |

### Page Mode Behavior Matrix

| Mode       | Forms                 | DataGrid          | Primary Actions             |
| ---------- | --------------------- | ----------------- | --------------------------- |
| `view`     | Read-only             | Selectable        | New, Edit, Delete, Refresh  |
| `create`   | Editable              | Context-dependent | Save, Cancel                |
| `edit`     | Editable              | Context-dependent | Save, Cancel, Refresh       |
| `copy`     | Editable (pre-filled) | Context-dependent | Save, Cancel                |
| `readonly` | Read-only             | View-only         | Refresh, Print              |
| `process`  | Step-driven           | Disabled          | Next, Back, Execute, Cancel |

---

## 1. Simple List

### Purpose

A single flat DataGrid for managing setup tables, reference data, and lightweight master data entities. Supports page-level Save/Cancel with inline editing (`masterForm` mode) or popup form dialogs.

### When to Use

- Currencies, Units, Tax Groups, Customer Groups, Payment Terms, Number Sequences.
- Any reference table where the primary interaction is a single editable grid.

### Folder Structure

```text
src/patterns/simple-list/
├── SimpleListPage.tsx         # Pattern component
├── useSimpleListPage.ts       # Pattern state hook (placeholder)
└── types.ts                   # Pattern type exports
```

### Required Components

```text
SimpleListPage
├── PageHeader (title, subtitle)
├── ActionPane (New, Save, Cancel, Delete, Refresh)
├── AppDataGrid (masterForm=true for inline editing)
│   ├── DataGridToolbar (search, add-row button)
│   ├── DataGridHeader (sort, filter)
│   └── DataGridBody (virtualized rows with inline edit cells)
├── ErrorState / LoadingState (conditional)
└── Dialogs (DeleteConfirmationDialog)
```

### Data Flow

```text
Module Page
  → useListPage({ loadData, saveData, deleteData })
  → SimpleListPage (pattern template)
    → AppDataGrid (masterForm={true}, onRowSave, onNewRow)
    → ActionPane (Save → handleSave, Delete → handleDelete)
```

### Example

```tsx
// src/modules/finance/accounts-receivable/currencies/pages/CurrenciesPage.tsx
export function CurrenciesPage() {
  const {
    data,
    loading,
    selectedIds,
    setSelectedIds,
    handleSave,
    handleDelete,
    handleRefresh,
    isDirty,
    pageMode,
  } = useListPage({
    loadData: currencyService.getAll,
    saveData: currencyService.saveAll,
    deleteData: currencyService.delete,
  });

  return (
    <SimpleListPage
      title="Currencies"
      subtitle="General Ledger"
      actionPane={
        <CurrencyActions pageMode={pageMode} onSave={handleSave} onDelete={handleDelete} />
      }
      dataGridProps={{
        rows: data,
        columns: currencyColumns,
        masterForm: true,
        onRowSave: handleRowSave,
        selectedIds,
        onSelectionChange: setSelectedIds,
      }}
    />
  );
}
```

### Rules

- Page-level Save/Cancel — NOT row-level save buttons.
- Use `masterForm={true}` on DataGrid for inline editing.
- Always track `isDirty` and confirm unsaved changes on navigation.
- Memoize `columns` array outside of render.
- DataGrid `getRowId` must return a stable unique identifier — never array index.

---

## 2. List & Details

### Purpose

A split-view layout: the left pane shows a searchable record grid, and the right pane shows the selected record's details in FastTabs. The details pane appears only when a record is selected and adjusts responsively.

### When to Use

- Customers, Vendors, Products, Employees, Warehouses.
- Any master entity that requires viewing a summary grid alongside rich multi-tab detail forms.

### Folder Structure

```text
src/patterns/list-details/
├── ListDetailsPage.tsx        # Pattern component
├── ListDetailsLayout.tsx      # Responsive split layout helper
├── useListDetailsPage.ts      # Pattern state hook (placeholder)
└── types.ts                   # Pattern type exports
```

### Required Components

```text
ListDetailsPage
├── PageHeader (title, subtitle)
├── ActionPane (New, Edit, Delete, Refresh)
├── SplitView (Grid responsive container)
│   ├── Left: AppDataGrid (record list, single selection)
│   └── Right: DetailPane (conditional, shown when selectedId exists)
│       ├── FastTabs
│       │   ├── FastTab "General" (form fields)
│       │   ├── FastTab "Addresses" (LogisticsPostalAddressDrawer)
│       │   ├── FastTab "Financial" (currency, payment terms)
│       │   └── FastTab "Contact" (LogisticsElectronicAddressDrawer)
│       └── LoadingState / EmptyState
└── Dialogs (DeleteConfirmationDialog)
```

### Data Flow

```text
Module Page
  → useListPage({ loadData, deleteData })
  → useQuery for selected record details
  → ListDetailsPage (pattern template)
    → DataGrid (onRowClick → setSelectedId)
    → DetailsPane (FastTabs rendered when selectedId exists)
```

### Example

```tsx
export function CustomerListPage() {
  const { data, loading, selectedId, setSelectedIds, handleDelete, handleRefresh } = useListPage({
    loadData: customerService.getPaged,
  });

  const { data: selectedCustomer } = useQuery({
    queryKey: ['customers', selectedId],
    queryFn: () => customerService.getById(selectedId!),
    enabled: !!selectedId,
  });

  return (
    <ListDetailsPage
      title="Customers"
      subtitle="Accounts Receivable"
      dataGridProps={{
        rows: data,
        columns: customerColumns,
        onRowClick: (row) => setSelectedIds([row.id]),
      }}
      selectedId={selectedId}
      detailsPane={selectedCustomer && <CustomerDetails customer={selectedCustomer} />}
    />
  );
}
```

### Rules

- Grid occupies full width (`md={12}`) when no record is selected; splits to `md={5}` grid + `md={7}` details on selection.
- Switching records when the detail form is dirty must prompt the unsaved changes dialog.
- Detail content is conditionally rendered — never loaded when `selectedId` is `null`.
- Use `FastTabs` with `hasError` to highlight validation issues in collapsed tabs.

---

## 3. Master Form

### Purpose

A full-page form wrapped in a Paper container for configuring application settings, module parameters, and posting setup tables. Content is organized with `FastTabs` accordion sections.

### When to Use

- Application Settings, Module Parameters (AR Parameters, AP Parameters), Posting Setup, Tax Configuration.
- Any page where the user configures a singleton settings record or a small group of parameters.

### Folder Structure

```text
src/patterns/master-form/
├── MasterFormPage.tsx         # Pattern component
├── useMasterFormPage.ts       # Pattern state hook (placeholder)
└── types.ts                   # Pattern type exports
```

### Required Components

```text
MasterFormPage
├── PageHeader (title, subtitle)
├── ActionPane (Save, Cancel, Refresh)
├── Paper Container
│   └── FastTabs
│       ├── FastTab "General"
│       ├── FastTab "Localization"
│       ├── FastTab "UI Preferences"
│       └── FastTab "API Configuration"
└── PageFeedback
```

### Data Flow

```text
Module Page
  → useForm (React Hook Form)
  → useMutation (TanStack Query)
  → MasterFormPage (pattern template)
    → FastTabs → FormRow → FormColumn → AppTextField / AppSelectField
    → ActionPane (Save → form.handleSubmit, Cancel → form.reset)
```

### Example

```tsx
export function ApplicationSettingsPage() {
  const form = useForm<AppSettings>({ resolver: zodResolver(appSettingsSchema) });

  return (
    <FormProvider {...form}>
      <MasterFormPage
        title="Application Settings"
        subtitle="System Administration"
        actionPane={<SettingsActions onSave={form.handleSubmit(handleSave)} />}
      >
        <FastTabs>
          <FastTab id="general" title="General" defaultExpanded>
            <FormRow>
              <FormColumn>
                <AppTextField name="companyName" label="Company Name" required />
              </FormColumn>
              <FormColumn>
                <AppSelectField name="defaultCurrency" label="Default Currency" />
              </FormColumn>
            </FormRow>
          </FastTab>
        </FastTabs>
      </MasterFormPage>
    </FormProvider>
  );
}
```

### Rules

- Wrap the page in `<FormProvider>` so all child field components inherit form context.
- Domain Zod validation schemas remain inside `@modules/module-name/validation/`.
- Mark unsaved changes via `useUnsavedChanges(form.formState.isDirty)`.

---

## 4. Master-Detail

### Purpose

A vertically stacked layout where a top section shows a master record (either a form or grid), and a bottom section shows dependent child collections in a detail DataGrid. The detail grid filters based on the selected master record.

### When to Use

- Journals and Journal Lines, Customer Groups and Posting Profiles, Warehouses and Locations, Chart of Accounts and Sub-accounts.
- Any parent → child (1:N) relationship where both levels are visible simultaneously.

### Folder Structure

```text
src/patterns/master-detail/
├── MasterDetailPage.tsx       # Pattern component (scaffolded)
├── MasterDetailLayout.tsx     # Split layout (master top, detail bottom)
├── useMasterDetailPage.ts     # Pattern state hook (scaffolded)
└── types.ts                   # Pattern type exports
```

### Required Components

```text
MasterDetailPage
├── PageHeader
├── ActionPane
├── Master Section (Form or DataGrid for parent record)
├── Detail Toolbar (Add Line, Delete Line)
├── Detail DataGrid (filtered by selected master ID)
└── Dialogs
```

### Data Flow

```text
Master selection changes → Detail query key updates → Detail grid re-fetches
```

### Rules

- Detail DataGrid must invalidate / re-fetch when master selection changes.
- Save operations may need to persist both master and detail changes atomically.
- Detail grid should display `EmptyState` when no master is selected.

---

## 5. Header-Lines Document

### Purpose

A transactional document page with a header form section (order metadata), a lines DataGrid (line items), and a totals summary panel. Supports document lifecycle process actions (Confirm, Post, Cancel, Print).

### When to Use

- Sales Orders, Purchase Orders, Invoices, Transfer Orders, Journals, Quotations.
- Any transactional document that follows the Header + Lines + Totals structure.

### Folder Structure

```text
src/patterns/document/
├── DocumentPage.tsx           # Pattern component
├── DocumentHeader.tsx         # Header form section (placeholder)
├── DocumentLines.tsx          # Lines DataGrid section (placeholder)
├── DocumentTotals.tsx         # Totals summary panel (placeholder)
├── useDocumentPage.ts         # Pattern state hook (placeholder)
└── types.ts                   # Pattern type exports
```

### Required Components

```text
DocumentPage
├── PageHeader (title, subtitle, statusBadge: "Draft" | "Confirmed" | "Posted")
├── ActionPane (Save, Confirm, Post, Cancel, Print)
├── Header Paper
│   └── headerContent (FastTabs with order metadata: Customer, Date, Currency)
├── Lines Paper
│   └── linesContent (AppDataGrid with masterForm={true}: Item, Qty, Price, Discount, Amount)
├── Totals Paper (right-aligned summary: Subtotal, Tax, Discount, Grand Total)
└── Dialogs (ConfirmationDialog for Confirm/Post lifecycle)
```

### Data Flow

```text
Module Page
  → useDocumentPage(orderId, { loadData, saveData })
  → DocumentPage (pattern template)
    → headerContent: FastTabs with order header form
    → linesContent: AppDataGrid (masterForm for line items)
    → totalsContent: Calculated summary fields
    → ActionPane process buttons → executeProcessAction(confirmOrder, 'Confirmed', 'Failed')
```

### Example

```tsx
export function SalesOrderPage() {
  const { id } = useParams();
  const {
    document: order,
    loading,
    fetchDocument,
    executeProcessAction,
    isDirty,
  } = useDocumentPage(id, {
    loadData: salesOrderService.getById,
    saveData: salesOrderService.save,
  });

  return (
    <DocumentPage
      title={`Sales Order ${order?.orderNumber || ''}`}
      subtitle="Accounts Receivable"
      statusBadge={order?.status}
      actionPane={
        <OrderActions status={order?.status} onConfirm={handleConfirm} onPost={handlePost} />
      }
      headerContent={<OrderHeaderForm order={order} />}
      linesContent={<OrderLinesGrid lines={order?.lines || []} />}
      totalsContent={<OrderTotals order={order} />}
    />
  );
}
```

### Rules

- The `statusBadge` prop reflects the document lifecycle state.
- Process actions (Confirm, Post) must call `executeProcessAction()` which handles loading, error, and success notifications.
- Totals panel is right-aligned and spans `md={4}` on desktop.
- Line item DataGrid uses `masterForm={true}` for inline cell editing.

---

## 6. Workspace

### Purpose

An operational dashboard landing page with KPI summary tiles, charts, data grid work lists, and quick-link navigation cards. This is the primary entry point for each business module.

### When to Use

- Module Dashboard (Accounts Receivable Workspace, Inventory Workspace).
- Operational work centers where managers monitor KPIs and access high-priority queues.

### Folder Structure

```text
src/patterns/workspace/
├── WorkspacePage.tsx          # Pattern component
├── WorkspaceTile.tsx          # KPI summary tile card
├── WorkspaceSection.tsx       # Grouping section (placeholder)
└── types.ts                   # Pattern type exports
```

### Required Components

```text
WorkspacePage
├── PageHeader (title, subtitle)
├── Summary Tiles Row (Grid of WorkspaceTile cards)
│   ├── WorkspaceTile "Total Customers" (value, icon, color)
│   ├── WorkspaceTile "Open Orders" (value, icon, color)
│   ├── WorkspaceTile "Monthly Revenue" (value, icon, color)
│   └── WorkspaceTile "Overdue Balance" (value, icon, color="error")
├── Charts Section (optional)
├── Work Lists (AppDataGrid with recent orders)
└── Quick Links (navigation cards)
```

### Data Flow

```text
Module Page
  → Multiple useQuery hooks for each KPI tile
  → WorkspacePage (pattern template)
    → Grid of WorkspaceTile components
    → AppDataGrid for work lists
```

### Example

```tsx
export function DashboardPage() {
  const { data: stats } = useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: dashboardService.getStats,
  });

  return (
    <WorkspacePage title="Dashboard" subtitle="Home">
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <WorkspaceTile
            title="Total Customers"
            value={stats?.totalCustomers || 0}
            color="primary"
            icon={<PeopleIcon />}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <WorkspaceTile
            title="Open Orders"
            value={stats?.openOrders || 0}
            color="info"
            icon={<ShoppingCartIcon />}
          />
        </Grid>
      </Grid>
      <AppDataGrid rows={stats?.recentOrders || []} columns={recentOrderColumns} height={300} />
    </WorkspacePage>
  );
}
```

### Rules

- Tiles use `elevation={0}` with subtle hover lift animations (`translateY(-2px)`).
- Tiles accept `onClick` for drill-down navigation to detail pages.
- Do not overload the workspace — keep KPI tiles to 4–8 maximum.

---

## 7. Inquiry

### Purpose

A read-only analysis page with advanced filter panels and result DataGrids. Users search and filter data but cannot create, edit, or delete records.

### When to Use

- Transaction Inquiry, Voucher Inquiry, Audit Log Viewer, Balance Inquiry.
- Any page that presents filtered, read-only query results.

### Folder Structure

```text
src/patterns/inquiry/
├── InquiryPage.tsx            # Pattern component (scaffolded)
├── InquiryFilterPanel.tsx     # Advanced filter sidebar or collapsible panel
└── types.ts                   # Pattern type exports
```

### Required Components

```text
InquiryPage
├── PageHeader
├── ActionPane (Refresh, Export, Print)
├── InquiryFilterPanel (date range, entity, status filters)
├── AppDataGrid (read-only, no masterForm)
└── PageFeedback
```

### Data Flow

```text
Filter changes → URL search params update → TanStack Query refetch → Grid re-renders
```

### Rules

- DataGrid must be read-only (`masterForm={false}`).
- Filters should map to URL search parameters for shareable / bookmarkable queries.
- Support CSV export via DataGrid sidebar.

---

## 8. Setup

### Purpose

A hierarchical or grouped configuration page with a left-side navigation tree and a right-side configuration form. Similar to a settings panel with categorized subsections.

### When to Use

- System Setup, Module Configuration, Posting Setup by Category, Workflow Configuration.

### Folder Structure

```text
src/patterns/setup/
├── SetupPage.tsx              # Pattern component (scaffolded)
├── SetupNavigation.tsx        # Left tree navigation for setup categories
└── types.ts                   # Pattern type exports
```

### Required Components

```text
SetupPage
├── PageHeader
├── SplitLayout
│   ├── Left: SetupNavigation (category tree)
│   └── Right: Active setup form (MasterFormPage or FastTabs)
└── PageFeedback
```

### Rules

- Navigation items are configuration-driven.
- Active category highlights in the tree.
- Each category loads its own form or DataGrid content dynamically.

---

## 9. Process / Wizard

### Purpose

A multi-step guided process with step indicators, validation per step, and a final execution action. Steps progress linearly or with conditional branching.

### When to Use

- Period Close Wizard, Data Import Wizard, Batch Job Configuration, Year-End Processing.

### Folder Structure

```text
src/patterns/process/
├── ProcessPage.tsx            # Pattern component (scaffolded)
├── ProcessNavigation.tsx      # Step header indicator bar
├── ProcessStepIndicator.tsx   # Individual step circle/label
└── types.ts                   # Pattern type exports
```

### Required Components

```text
ProcessPage
├── PageHeader
├── ProcessStepIndicator (Step 1: Configure → Step 2: Review → Step 3: Execute)
├── Step Content (dynamic form per active step)
├── ActionPane (Back, Next, Execute, Cancel)
└── ProcessFeedback (progress bar, completion summary)
```

### Data Flow

```text
Step 1 form validates → Next → Step 2 → ... → Execute → API mutation → Success/Error
```

### Rules

- Each step must validate independently before allowing `Next`.
- `Back` preserves entered data from previous steps.
- `Execute` triggers the final API operation and shows progress feedback.
- `Cancel` confirms discarding all wizard progress.

---

## 10. Tree & Details

### Purpose

A page with a hierarchical tree navigation on the left and a detail form/grid on the right. The tree represents a parent-child hierarchy.

### When to Use

- Organization Units, Chart of Accounts, Product Categories, Menu Structures.

### Folder Structure

```text
src/patterns/tree-details/
├── TreeDetailsPage.tsx        # Pattern component (scaffolded)
├── TreeNavigation.tsx         # Expandable tree view component
└── types.ts                   # Pattern type exports
```

### Required Components

```text
TreeDetailsPage
├── PageHeader
├── ActionPane
├── SplitLayout
│   ├── Left: TreeNavigation (expandable/collapsible MUI TreeView)
│   └── Right: Detail form or child DataGrid (based on selected tree node)
└── Dialogs
```

### Rules

- Tree supports expand/collapse, drag-and-drop reordering (optional), and context menus.
- Selecting a tree node fetches and displays associated detail data in the right pane.
- Support keyboard navigation (`ArrowUp`, `ArrowDown`, `Enter`, `ArrowLeft` collapse, `ArrowRight` expand).

---

## 11. Profile

### Purpose

An entity-centered card view with a header banner (avatar, name, status), summary metrics, and tabbed detail sections below. Designed for viewing an entity's complete profile at a glance.

### When to Use

- Customer Profile, Vendor Profile, Employee Profile, User Profile.

### Folder Structure

```text
src/patterns/profile/
├── ProfilePage.tsx            # Pattern component (scaffolded)
├── ProfileHeader.tsx          # Banner header with avatar, name, status badge
├── ProfileSummary.tsx         # Summary metrics cards row
└── types.ts                   # Pattern type exports
```

### Required Components

```text
ProfilePage
├── ProfileHeader (avatar, name, code, status badge, quick actions)
├── ProfileSummary (metric cards: Total Orders, Balance, Last Activity)
├── FastTabs
│   ├── FastTab "Details" (entity form fields)
│   ├── FastTab "Transactions" (DataGrid history)
│   ├── FastTab "Addresses" (Postal + Electronic address lists)
│   └── FastTab "Notes" (text notes and attachments)
└── PageFeedback
```

### Rules

- Profile header is always visible (not scrollable).
- Summary metrics use `WorkspaceTile` or similar card components.
- Detail tabs load lazily to improve initial render performance.

---

## 12. Tabbed Details

### Purpose

A full-page tabbed interface where each tab represents a distinct detail section. Unlike FastTabs (which are collapsible accordions), this uses horizontal MUI Tabs for exclusive panel switching.

### When to Use

- Entity detail views where sections are mutually exclusive (only one visible at a time).
- Alternative to FastTabs when vertical screen space is limited.

### Folder Structure

```text
src/patterns/tabbed-details/
├── TabbedDetailsPage.tsx      # Pattern component (scaffolded)
└── types.ts                   # Pattern type exports
```

### Required Components

```text
TabbedDetailsPage
├── PageHeader
├── ActionPane
├── MUI Tabs Bar (General | Financial | Addresses | History)
├── Active Tab Panel Content
└── PageFeedback
```

### Rules

- Only one tab panel is rendered at a time (content switches, not stacks).
- Tab labels must use localization keys.
- Active tab state can optionally be stored in URL search params for shareable links.

---

## 13. Lookup Page

### Purpose

A dedicated full-page lookup selection interface. Used when inline `LookupGridField` popover dropdowns are insufficient and the user needs full search, filter, and multi-select capabilities.

### When to Use

- Complex multi-criteria entity lookups that exceed the popover grid lookup UX.
- Bulk item selection workflows.

### Folder Structure

```text
src/patterns/lookup/
├── LookupPage.tsx             # Pattern component (scaffolded)
└── types.ts                   # Pattern type exports
```

### Required Components

```text
LookupPage
├── PageHeader
├── FilterBar (search, category, status filters)
├── AppDataGrid (multi-select enabled)
├── ActionBar (Select, Cancel)
└── Selected Items Summary
```

### Rules

- Must return selected items to the calling page (via route state, callback, or shared store).
- Selection state persists across filter/search changes within the lookup session.

---

## Global Pattern Rules

These rules apply to **every** pattern across the entire application:

### Architecture Rules

1. Patterns live in `src/patterns/` and import only from `@shared` and `@core`.
2. Patterns must **never** import from `@modules` or `@app`.
3. Domain modules compose patterns — patterns do not contain business logic.
4. Each pattern folder contains a main component, supporting layout components, a state hook, and a types file.

### Page Composition Rules

5. Every page must follow this structural hierarchy:
   ```text
   PageContainer → PageHeader → ActionPane → Content → Dialogs → Feedback
   ```
6. Action definitions must be memoized (`useMemo`) and configuration-driven.
7. Destructive actions (Delete, Cancel unsaved changes) must require explicit user confirmation via `ConfirmationDialog` or `DeleteConfirmationDialog`.

### State Management Rules

8. Server data is always fetched via TanStack Query (`useQuery`, `useMutation`).
9. Form state is always managed by React Hook Form.
10. Dirty state is always tracked via `useUnsavedChanges(isDirty)`.
11. Page mode transitions (`view` → `edit` → `view`) are managed via `usePageMode()`.
12. URL search parameters are used for shareable filter and tab state where appropriate.

### DataGrid Rules

13. Column definitions must be created outside render or memoized with `useMemo`.
14. Row identifiers must be stable and unique — never use array index.
15. Use `masterForm={true}` for inline editing patterns.
16. Page-level Save/Cancel is preferred over row-level Save/Cancel buttons.

### Icon Import Rule (Vitest ESM Safety)

17. All Material UI icons must be imported via specific path imports:
    ```tsx
    // ✅ CORRECT
    import AddIcon from '@mui/icons-material/Add';
    // ❌ INCORRECT — crashes Vitest ESM
    import { Add } from '@mui/icons-material';
    ```

### Responsive Rules

18. Desktop: Multi-column layouts, persistent sidebar, full action pane.
19. Tablet: Reduced column count, collapsible details, action overflow.
20. Mobile: Single-column forms, temporary drawer, horizontal grid scroll, full-screen dialogs.

---

## Pattern Selection Decision Tree

Use this decision tree to determine which pattern to apply:

```text
Is it a dashboard or operational overview?
  → YES → Workspace Pattern (#6)
  → NO ↓

Is it a read-only query or analysis page?
  → YES → Inquiry Pattern (#7)
  → NO ↓

Is it a multi-step process or wizard?
  → YES → Process Pattern (#9)
  → NO ↓

Is it a singleton settings / parameters form?
  → YES → Master Form Pattern (#3)
  → NO ↓

Is it a hierarchical tree structure?
  → YES → Tree & Details Pattern (#10)
  → NO ↓

Is it a transactional document with header + lines?
  → YES → Document Pattern (#5)
  → NO ↓

Does the entity have a parent → child grid relationship?
  → YES → Master-Detail Pattern (#4)
  → NO ↓

Is it a simple flat reference or setup table?
  → YES → Simple List Pattern (#1)
  → NO ↓

Does it need a split grid + detail view with FastTabs?
  → YES → List & Details Pattern (#2)
  → NO ↓

Is it an entity profile/summary page?
  → YES → Profile Pattern (#11)
  → NO ↓

Does it need horizontal tabs (not accordions)?
  → YES → Tabbed Details Pattern (#12)
  → NO ↓

Is it a full-page selection/lookup interface?
  → YES → Lookup Page Pattern (#13)
  → NO → Evaluate whether a new pattern is warranted (requires architectural review).
```
