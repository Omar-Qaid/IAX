# Parameter Setup Page

## Purpose
A hierarchical or grouped configuration page with a left-side navigation tree and a right-side configuration form.

## When to use
- System Setup, Module Configuration, Workflow Configuration.

## Folder structure
```text
src/patterns/setup/
├── SetupPage.tsx              # Pattern component
├── SetupNavigation.tsx        # Left tree navigation
└── types.ts                   # Pattern type exports
```

## Required components
```text
SetupPage
├── SplitLayout
│   ├── Left: SetupNavigation (tree)
│   └── Right: Active setup form
└── PageFeedback
```

## Data flow
```text
Tree node selected → Right pane form state updates to match category.
```

## Examples
General Ledger Parameters.

## Rules
- Navigation items are configuration-driven.
- Each category loads its own form dynamically.

## Description UI
A settings-panel style layout. The left side is a fixed-width menu or tree view listing various configuration categories (e.g., General, Number Sequences, Financial). Clicking a category swaps out the right-side content pane to show the corresponding form controls without causing a full page refresh.
