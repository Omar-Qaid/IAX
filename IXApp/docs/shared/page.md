# Page Structural Components Documentation (`src/shared/components/page`)

## 1. Purpose and Responsibilities
The `page` sub-system provides route-agnostic structural containers for assembling pages in **IXApp**. Inspired by Microsoft Dynamics 365 Finance & Operations page headers, it enforces consistent margins, paper background cards, and page section headers across all business modules.

---

## 2. Folder Structure
```text
src/shared/components/page/
├── PageContainer.tsx          # Top-level page column layout wrapper (`PageContainer`, `PageContent`, `PageSection`)
├── PageHeader.tsx             # Standard page title & status header (`PageHeader`, `PageTitle`)
├── PageStatusBar.tsx          # Bottom status bar summary component
└── types.ts                   # Page header & container contracts
```

---

## 3. Naming Conventions
- **Components:** `PascalCase.tsx` prefixed with `Page` (e.g., `PageContainer.tsx`, `PageHeader.tsx`).

---

## 4. Components
- **`PageContainer`:** Flexbox wrapper providing consistent vertical spacing (`gap: 1.5`) between page header, action pane, and page content.
- **`PageHeader`:** Renders page title (`h5`, `fontWeight: 700`), record ID subtitle, status badge, and optional header action buttons.
- **`PageContent`:** Paper card container (`elevation={0}`, border: `1px solid divider`) wrapping the main grid or form content.
- **`PageSection`:** Section wrapper with subtle bottom border divider and primary-colored bold title label.

---

## 5. Hooks & Integrations
Contains no route or application-store integrations. Route-aware breadcrumbs belong to `src/app/navigation`.

---

## 6. Services & APIs
Contains zero direct API calls.

---

## 7. State Management
Stateless layout wrapper controls driven by parent page properties.

---

## 8. Design Patterns
- **Structural Container Pattern:** Standardizes page layout hierarchy (`PageContainer` $\rightarrow$ `PageHeader` $\rightarrow$ `ActionPane` $\rightarrow$ `PageContent`).

---

## 9. Architecture & Dependencies
- **Dependencies:** `@mui/material`, `@core/localization`.
- **Forbidden:** No business module imports (`@modules/*`).

---

## 10. Best Practices
- Every page component should wrap its output inside `<PageContainer>`.
- Use `<PageHeader title="..." />` to maintain consistent typography across the application.

---

## 11. Do's and Don'ts
- **DO:** Use `PageSection` to group related form cards inside master setup pages.
- **DON'T:** Use custom raw `div` tags for page padding; use `PageContainer` and `PageContent`.

---

## 12. Code Example
```tsx
<PageContainer>
  <PageHeader title="Customers" subtitle="Accounts Receivable" />
  <ActionPane>{/* Action buttons */}</ActionPane>
  <PageContent>
    <AppDataGrid rows={customers} columns={columns} />
  </PageContent>
</PageContainer>
```

---

## 13. Decision Rules & Checklist
- [ ] Is `PageContainer` wrapping the root JSX of the page?
- [ ] Is `PageHeader` rendering the localized title?
