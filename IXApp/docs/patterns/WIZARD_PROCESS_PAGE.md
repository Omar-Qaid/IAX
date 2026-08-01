# Wizard Process Page

## Purpose
A multi-step guided process with step indicators, validation per step, and a final execution action.

## When to use
- Period Close Wizard, Data Import Wizard, Year-End Processing.

## Folder structure
```text
src/patterns/process/
├── ProcessPage.tsx            # Pattern component
├── ProcessStepIndicator.tsx   # Step circle/label
└── types.ts                   # Pattern type exports
```

## Required components
```text
ProcessPage
├── ProcessStepIndicator (Step 1 → Step 2 → Step 3)
├── Step Content (dynamic form)
└── ActionPane (Back, Next, Execute)
```

## Data flow
```text
Step validates → Next → Next → Execute → API mutation.
```

## Examples
Data Import Wizard.

## Rules
- Each step must validate independently.
- `Back` preserves entered data.
- `Execute` triggers final API operation.

## Description UI
A focused, linear workflow layout. The top of the page displays a prominent step indicator (e.g., circles connected by lines: [1] Upload -> [2] Map -> [3] Import). The center contains the form for the current step. The ActionPane provides standard Next/Back/Cancel navigation controls to guide the user through the process.
