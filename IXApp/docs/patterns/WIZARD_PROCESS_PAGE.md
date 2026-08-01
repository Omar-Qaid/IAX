# Wizard / Process Page

## 1. Pattern Purpose & When to Use It
- **Purpose**: A multi-step guided workflow for executing multi-stage complex tasks.
- **When to Use**: - Month-End Close Wizard, Data Import/Export Wizard, Initial Setup Wizard.

## 2. UI Structure & Layout
Top step progress bar indicator, middle step content form, bottom action bar (Back, Next, Finish).

## 3. Page Sections & Components
- PageHeader
- ProcessStepIndicator (Steps 1..N)
- Current Step Form Region
- Wizard Action Bar (Previous, Next, Execute, Cancel)

## 4. Folder Structure
```text
src/patterns/process/
├── ProcessPage.tsx
├── ProcessStepIndicator.tsx
└── types.ts
```

## 5. Required Reusable Components
- ProcessStepIndicator
- ActionPane / Button controls

## 6. Data Flow & State Management
- **Data Flow**: Step completion -> validates step state -> advances activeStep index -> final step submits whole execution payload.
- **State Management**: - Multi-step form state accumulator.

## 7. Actions & Commands
- Next Step, Previous Step, Cancel Wizard, Finish/Execute Process

## 8. Validation Rules
- Strict per-step validation before allowing Next navigation.

## 9. Naming Conventions & Best Practices
- **Naming Conventions**: - *WizardPage.tsx or *ProcessPage.tsx
- **Best Practices**: - Preserve state when user goes Back to previous steps.

## 10. Do's and Don'ts Rules
DO: Show clear progress indicator.
DON'T: Allow jumping ahead to unvalidated future steps.

## 11. Implementation Example
```tsx
// MonthEndCloseWizardPage
```
