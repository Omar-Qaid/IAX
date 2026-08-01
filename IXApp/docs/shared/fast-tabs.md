# FastTabs Component Documentation (`src/shared/components/fast-tabs`)

## 1. Purpose and Responsibilities
The `fast-tabs` sub-system implements Microsoft Dynamics 365 Finance & Operations collapsible accordion form sections. FastTabs allow dense master-data forms (such as Customers, Vendors, and Sales Orders) to organize hundreds of fields into collapsible logical sections (`General`, `Addresses`, `Financial`, `Contact`, `Posting`).

FastTabs display a summary snippet when collapsed, highlight invalid hidden form fields (`hasError={true}`), and preserve compact spacing.

---

## 2. Folder Structure
```text
src/shared/components/fast-tabs/
├── FastTabs.tsx               # Main vertical stack container for FastTab accordions
├── FastTab.tsx                # Single collapsible accordion panel with summary & error chip
├── FastTabHeader.tsx          # Custom header toolbar for FastTab action buttons
└── FastTabSummary.tsx         # Collapsed summary text renderer
```

---

## 3. Naming Conventions
- **Components:** `PascalCase.tsx` (e.g., `FastTabs.tsx`, `FastTab.tsx`).
- **Interfaces:** `FastTabsProps`, `FastTabProps`.

---

## 4. Components
- **`FastTabs`:** Stack container maintaining consistent vertical gap between panels.
- **`FastTab`:** Individual accordion section accepting `id`, `title`, `summary`, `defaultExpanded`, `hasError`, and `required` props.
- **Error Badge:** When `hasError={true}` (e.g., hidden form field fails validation), `FastTab` renders an error title color and a red `<Chip label="Error" />`.

---

## 5. Hooks & Integrations
Integrates with React Hook Form `formState.errors`. Parent forms pass `hasError={!!errors.sectionField}` to alert users when a collapsed FastTab contains validation errors.

---

## 6. Services & APIs
Contains zero direct API calls. Emits state changes via local `useState` toggle.

---

## 7. State Management
Expansion state (`expanded`) is managed locally per FastTab, or controlled externally by parent page state.

---

## 8. Design Patterns
- **Compound Component Pattern:** `<FastTabs>` wraps multiple `<FastTab>` panels.
- **Collapsible Summary Pattern:** Shows quick summary text (e.g., `Summary: "USD | Net 30"`) when collapsed.

---

## 9. Architecture & Dependencies
- **Dependencies:** `@mui/material`, `@mui/icons-material/ExpandMore`, `@mui/icons-material/Error`.
- **Forbidden:** No business module imports (`@modules/*`).

---

## 10. Best Practices
- Provide concise `summary` text so users can scan key data without expanding every tab.
- Set `defaultExpanded={true}` on critical sections (`General`).

---

## 11. Do's and Don'ts
- **DO:** Automatically compute `hasError` from React Hook Form errors so users know where validation failed.
- **DON'T:** Use raw MUI `<Accordion>` primitives directly in domain forms; wrap with `<FastTab>`.

---

## 12. Code Example
```tsx
<FastTabs>
  <FastTab
    id="general"
    title="General"
    summary={`${customer.name || ''} (${customer.code || ''})`}
    defaultExpanded={true}
  >
    <FormRow>
      <FormColumn><AppTextField name="code" label="Customer Code" required /></FormColumn>
      <FormColumn><AppTextField name="name" label="Customer Name" required /></FormColumn>
    </FormRow>
  </FastTab>

  <FastTab
    id="financial"
    title="Financial Information"
    summary={`Currency: ${customer.currencyCode} | Terms: ${customer.paymentTerms}`}
    hasError={!!errors.currencyCode || !!errors.paymentTerms}
  >
    <FormRow>
      <FormColumn><AppSelectField name="currencyCode" label="Currency" /></FormColumn>
    </FormRow>
  </FastTab>
</FastTabs>
```

---

## 13. Decision Rules & Checklist
- [ ] Is `FastTabs` used for multi-section form pages?
- [ ] Is `hasError` bound to RHF errors for section validation visibility?
- [ ] Is summary text concise and dynamic?
