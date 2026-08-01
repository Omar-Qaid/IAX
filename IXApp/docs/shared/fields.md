# Form Fields Component Documentation (`src/shared/components/fields`)

## 1. Purpose and Responsibilities
The `fields` sub-system implements the unified form input architecture for **IXApp**. Built on top of React Hook Form and Material UI v9, these components handle data binding, error display, label rendering, required indicators (`*`), read-only styling, compact density (`size="small"`), and fallback standalone controlled mode.

---

## 2. Folder Structure
```text
src/shared/components/fields/
├── AppTextField.tsx           # Standard text, multiline & password field
├── AppNumberField.tsx         # Numeric input field with min/max & formatting
├── AppCurrencyField.tsx       # Currency input field with symbol & decimals
├── AppDateField.tsx           # Date picker input field
├── AppDateTimeField.tsx       # Date-time picker input field
├── AppBooleanField.tsx       # Checkbox & switch toggle field
├── AppSelectField.tsx         # Dropdown select field with options array
├── AppEnumField.tsx           # Enum dropdown selector helper
├── AppLookupField.tsx         # Base dialog lookup field
├── AppLookupGridField.tsx     # Alias for D365 multi-column grid lookup field
├── AppDisplayField.tsx        # Read-only text display field
├── AppBilingualField.tsx      # Dual English/Arabic side-by-side text field
├── AppGeneratedCodeField.tsx  # Auto-generated code number sequence field
└── types.ts                   # BaseFieldProps contract interface
```

---

## 3. Naming Conventions
- **Components:** `PascalCase.tsx` prefixed with `App` (e.g., `AppTextField.tsx`, `AppSelectField.tsx`, `AppLookupGridField.tsx`).
- **Base Interface:** `BaseFieldProps<TFieldValues>`.

---

## 4. Key Field Components

### 4.1 Base Contract (`BaseFieldProps`)
All field components implement a standard interface:
```ts
export interface BaseFieldProps<TFieldValues extends FieldValues = FieldValues> {
  name: Path<TFieldValues>;
  label: string;
  control?: Control<TFieldValues>;
  required?: boolean;
  disabled?: boolean;
  readOnly?: boolean;
  hidden?: boolean;
  helperText?: string;
  fullWidth?: boolean;
  size?: 'small' | 'medium';
  placeholder?: string;
  value?: any;
  onChange?: (value: any) => void;
}
```

### 4.2 React Hook Form Context Integration
Every field component automatically attempts to read `useFormContext()`. If a `<FormProvider>` wraps the component, `control` is detected automatically without explicit prop passing. If no context exists, it falls back to standalone controlled mode (`value` & `onChange`).

---

## 5. Hooks & Integrations
Integrates directly with React Hook Form `Controller` and Zod validation error messages (`fieldState.error.message`).

---

## 6. Services & APIs
Contains zero direct API calls. Input values update the active form state.

---

## 7. State Management
Form state is managed by React Hook Form (`useForm`).

---

## 8. Design Patterns
- **Adapter Pattern:** Adapts Material UI inputs (`TextField`, `Select`, `Checkbox`) to React Hook Form's `Controller`.
- **Hybrid Controller Pattern:** Works inside `<FormProvider>` or as a standalone controlled React component.

---

## 9. Architecture & Dependencies
- **Dependencies:** `@mui/material`, `react-hook-form`.
- **Forbidden:** No business module imports (`@modules/*`).

---

## 10. Best Practices
- Always use `size="small"` (default) to preserve enterprise D365 density.
- Mark mandatory fields with `required={true}`.
- Use `AppLookupGridField` for multi-column relational entity selection.

---

## 11. Do's and Don'ts
- **DO:** Put `AppTextField` inside `<FormColumn>` wrappers inside `<FormRow>`.
- **DON'T:** Use raw MUI `<TextField>` in domain forms. Always use `AppTextField`.

---

## 12. Code Example
```tsx
<FormRow>
  <FormColumn>
    <AppTextField name="code" label="Customer Code" required />
  </FormColumn>
  <FormColumn>
    <AppTextField name="name" label="Customer Name" required />
  </FormColumn>
  <FormColumn>
    <AppSelectField
      name="currencyCode"
      label="Currency"
      options={[
        { value: 'USD', label: 'USD - US Dollar' },
        { value: 'EUR', label: 'EUR - Euro' },
      ]}
    />
  </FormColumn>
</FormRow>
```

---

## 13. Decision Rules & Checklist
- [ ] Is `name` aligned with Zod validation schema key?
- [ ] Is `required` specified for mandatory entity properties?
- [ ] Is error text rendering properly from RHF validation?
