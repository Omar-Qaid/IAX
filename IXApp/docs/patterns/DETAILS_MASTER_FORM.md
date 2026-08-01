# Details Master Form

## Purpose
A full-page form wrapped in a container for configuring application settings, parameters, and setup tables. Content is organized with FastTabs accordion sections.

## When to use
- Application Settings, Module Parameters, Posting Setup.
- Any page where the user configures a singleton settings record or parameters.

## Folder structure
```text
src/patterns/master-form/
├── MasterFormPage.tsx         # Pattern component
├── useMasterFormPage.ts       # Pattern state hook
└── types.ts                   # Pattern type exports
```

## Required components
```text
MasterFormPage
├── PageHeader (title, subtitle)
├── ActionPane (Save, Cancel, Refresh)
├── Paper Container
│   └── FastTabs
│       ├── FastTab "General"
│       ├── FastTab "Localization"
│       └── FastTab "API Configuration"
└── PageFeedback
```

## Data flow
```text
Module Page
  → useForm (React Hook Form)
  → useMutation (TanStack Query)
  → MasterFormPage
    → FastTabs → FormRow → FormColumn → AppTextField
```

## Examples
See Application Settings Page.

## Rules
- Wrap the page in `<FormProvider>`.
- Domain Zod validation schemas inside module validation folder.
- Mark unsaved changes via `useUnsavedChanges(form.formState.isDirty)`.

## Description UI
A clean, document-like form view utilizing FastTabs (expandable/collapsible vertical accordions). When opened, each FastTab reveals a grid of input fields (typically 2-3 columns per row). A sticky ActionPane at the top provides global Save/Cancel controls.
