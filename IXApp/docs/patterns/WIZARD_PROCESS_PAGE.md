# Process/wizard pattern status

`ProcessNavigation.tsx` and `ProcessStepIndicator.tsx` exist, but `ProcessPage.tsx` and `types.ts` are empty. There is no complete wizard state, validation, persistence, or execution contract.

Do not describe `ProcessPage` as implemented. A future implementation must define step identity, allowed navigation, per-step validation, accumulated state, cancellation, async execution, accessibility, responsive behavior, and tests.
