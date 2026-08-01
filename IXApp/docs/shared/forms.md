# Form Layout Component Documentation (`src/shared/components/forms`)

## 1. Purpose and Responsibilities
The `forms` sub-system provides responsive layout wrappers and error summary controls for form input elements in **IXApp**. It establishes a 12-column responsive layout grid (`FormRow`, `FormColumn`) that automatically adapts multi-column enterprise forms into 1-column layouts on mobile screens and 3/4-column layouts on desktop displays.

---

## 2. Folder Structure
```text
src/shared/components/forms/
├── FormContainer.tsx          # Form card wrapper container
├── FormRow.tsx                # MUI Grid container row wrapper (`FormRow`) & column item wrapper (`FormColumn`)
├── FormSection.tsx            # Form section card wrapper
├── FormActions.tsx            # Form submit / cancel bottom action toolbar
├── FormValidationSummary.tsx  # Top error list banner for failed form validation
└── FormErrorSummary.tsx       # Field error summary accordion control
```

---

## 3. Naming Conventions
- **Components:** `PascalCase.tsx` prefixed with `Form` (e.g., `FormContainer.tsx`, `FormRow.tsx`, `FormColumn.tsx`, `FormValidationSummary.tsx`).

---

## 4. Components
- **`FormRow`:** Grid row container enforcing standard 16px (`spacing={2}`) field gap and bottom margin.
- **`FormColumn`:** Grid column wrapper defaulting to responsive breakpoints: `xs={12}` (1 column mobile), `sm={6}` (2 column tablet), `md={4}` (3 column desktop), `lg={3}` (4 column wide desktop).
- **`FormValidationSummary`:** Alert box displaying list of failed form validation messages at the top of a page.

---

## 5. Hooks & Integrations
Integrates with React Hook Form `useFormContext` to extract `errors`.

---

## 6. Services & APIs
Contains zero direct API calls.

---

## 7. State Management
Stateless layout controls driven by parent form state.

---

## 8. Design Patterns
- **Grid Layout Pattern:** Standardizes 12-column flexbox layouts across all business modules.

---

## 9. Architecture & Dependencies
- **Dependencies:** `@mui/material`.
- **Forbidden:** No business module imports (`@modules/*`).

---

## 10. Best Practices
- Always place input fields inside `<FormColumn>` components inside `<FormRow>` containers.
- Use default breakpoint props (`xs=12`, `sm=6`, `md=4`) to ensure desktop views render compact 3-column rows while mobile screens collapse to single-column rows.

---

## 11. Do's and Don'ts
- **DO:** Group logical input fields inside `<FormRow>` blocks.
- **DON'T:** Use fixed pixel positions or manual float styles for form layout.

---

## 12. Code Example
```tsx
<FormRow>
  <FormColumn xs={12} md={4}>
    <AppTextField name="code" label="Customer Code" required />
  </FormColumn>
  <FormColumn xs={12} md={4}>
    <AppTextField name="name" label="Customer Name" required />
  </FormColumn>
  <FormColumn xs={12} md={4}>
    <AppSelectField name="status" label="Status" />
  </FormColumn>
</FormRow>
```

---

## 13. Decision Rules & Checklist
- [ ] Are form inputs wrapped inside `FormRow` and `FormColumn`?
- [ ] Do breakpoints adapt cleanly between mobile (1 column) and desktop (3 columns)?
