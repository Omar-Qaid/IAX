# Details / Master Form

## 1. Pattern Purpose & When to Use It
- **Purpose**: A full-page form organized into collapsible FastTabs for viewing and configuring singleton parameters, settings, and complex single-entity master records.
- **When to Use**: - Application Settings, Accounts Receivable Parameters, Accounts Payable Parameters, Tax Configuration.
- Any settings or parameters page where options are grouped logically into categories.

## 2. UI Structure & Layout
Single-column full-width container wrapped in a Paper surface. Organized vertically with a sticky ActionPane at the top followed by stacked FastTabs accordions.

## 3. Page Sections & Components
- PageHeader (Title, Subtitle)
- ActionPane (Save, Cancel, Edit, Refresh)
- Paper Container
  └── FastTabs Accordion Stack
      ├── FastTab "General"
      ├── FastTab "Posting Parameters"
      ├── FastTab "Number Sequences"
      └── FastTab "Default Values"

## 4. Folder Structure
```text
src/patterns/master-form/
├── MasterFormPage.tsx        # Pattern container
├── useMasterFormPage.ts      # Pattern hook
└── types.ts                  # Types
```

## 5. Required Reusable Components
- @shared/components/page/PageContainer
- @shared/components/page/PageHeader
- @shared/components/action-pane/ActionPane
- @shared/components/fast-tabs/FastTabs
- @shared/components/fields/AppTextField
- @shared/components/fields/AppSelectField

## 6. Data Flow & State Management
- **Data Flow**: 1. React Hook Form FormProvider wraps the MasterFormPage.
2. Server settings loaded via TanStack Query.
3. Form reset() populated with server response.
4. Field changes set form state to dirty.
5. Global Save triggers form.handleSubmit() -> API mutation.
- **State Management**: - Form state strictly controlled via React Hook Form.
- Unsaved changes guard via useUnsavedChanges(form.formState.isDirty).
- FastTabs expand/collapse state stored locally.

## 7. Actions & Commands
- Save (validates and submits form)
- Cancel (resets form to last saved state)
- Edit (switches page mode from view to edit)

## 8. Validation Rules
- Schema validation using Zod resolver passed to React Hook Form.
- FastTab header displays red error chip if child fields have validation errors.

## 9. Naming Conventions & Best Practices
- **Naming Conventions**: - Schema: camelCase ending with Schema (arParametersSchema)
- Page: PascalCase ending with Page (ARParametersPage.tsx)
- **Best Practices**: - Always wrap field components inside FormRow and FormColumn for consistent 12-column alignment.
- Use defaultExpanded on primary FastTabs.

## 10. Do's and Don'ts Rules
DO:
- Highlight FastTab errors with error chips.
- Organize fields logically by business frequency.

DON'T:
- Put data grids with hundreds of items inside a Master Form FastTab without virtualization.

## 11. Implementation Example
```tsx
export function ARParametersPage() {
  const form = useForm({ resolver: zodResolver(arParamsSchema) });
  return (
    <FormProvider {...form}>
      <MasterFormPage title="Accounts Receivable Parameters" subtitle="Setup">
        <FastTabs>
          <FastTab id="general" title="General" defaultExpanded>
            <FormRow>
              <FormColumn><AppTextField name="defaultCreditLimit" label="Default Credit Limit" /></FormColumn>
            </FormRow>
          </FastTab>
        </FastTabs>
      </MasterFormPage>
    </FormProvider>
  );
}
```
