# Parameter / Setup Page

## 1. Pattern Purpose & When to Use It
- **Purpose**: A categorized setup screen with left tree/tab navigation and right configuration form panels.
- **When to Use**: - System Setup, Module Parameters, Posting Rules Setup.

## 2. UI Structure & Layout
Left sidebar navigation list/tree; Right detail settings form panel.

## 3. Page Sections & Components
- PageHeader
- SetupNavigation (Categories list)
- Active Category Settings Form

## 4. Folder Structure
```text
src/patterns/setup/
├── SetupPage.tsx
├── SetupNavigation.tsx
└── types.ts
```

## 5. Required Reusable Components
- SetupNavigation
- AppTextField
- AppSelectField

## 6. Data Flow & State Management
- **Data Flow**: Category click -> switches visible parameter form group.
- **State Management**: - Parameter object state with page-level save.

## 7. Actions & Commands
- Save Parameters, Restore Defaults

## 8. Validation Rules
- Range checks on numeric parameter fields.

## 9. Naming Conventions & Best Practices
- **Naming Conventions**: - *SetupPage.tsx
- **Best Practices**: - Group parameters logically into clear categories.

## 10. Do's and Don'ts Rules
DO: Provide helpful tooltips for complex setup options.
DON'T: Save individual field changes automatically without user explicit save.

## 11. Implementation Example
```tsx
// SystemSetupPage
```
