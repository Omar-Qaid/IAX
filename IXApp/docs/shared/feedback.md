# Feedback Components Documentation (`src/shared/components/feedback`)

## 1. Purpose and Responsibilities
The `feedback` sub-system provides standardized UI state indicators for **IXApp**. It ensures that async network operations, empty search results, API server errors, and permission restrictions render consistent, user-friendly visual feedback.

---

## 2. Folder Structure
```text
src/shared/components/feedback/
├── LoadingState.tsx           # Circular spinner & loading text indicator
├── EmptyState.tsx             # No data / empty filter result state with optional action
├── ErrorState.tsx             # Error alert card with retry action button
└── AccessDeniedState.tsx      # Restricted resource lock warning state
```

---

## 3. Naming Conventions
- **Components:** `PascalCase.tsx` ending with `State` (e.g., `LoadingState.tsx`, `EmptyState.tsx`, `ErrorState.tsx`, `AccessDeniedState.tsx`).

---

## 4. Components
- **`LoadingState`:** Renders centered `CircularProgress` and optional localized message.
- **`EmptyState`:** Renders empty icon, title, description, and optional primary action button (e.g., `"Create Customer"`).
- **`ErrorState`:** Renders alert icon, localized error message (`AppError`), and optional `"Retry"` button.
- **`AccessDeniedState`:** Renders security lock icon and access restricted message when RBAC permissions block viewing.

---

## 5. Hooks & Integrations
Integrates with `useTranslation` for localized feedback messages.

---

## 6. Services & APIs
Contains zero direct API calls. Re-executes fetch queries via `onRetry` callbacks.

---

## 7. State Management
Stateless presentation components driven by parent query status (`isLoading`, `isError`, `data.length === 0`).

---

## 8. Design Patterns
- **State Indicator Pattern:** Replaces complex conditional JSX with explicit state components (`if (isLoading) return <LoadingState />`).

---

## 9. Architecture & Dependencies
- **Dependencies:** `@mui/material`, `@core/localization`.
- **Forbidden:** No business module imports (`@modules/*`).

---

## 10. Best Practices
- Always provide an `onRetry` callback in `ErrorState` when using TanStack Query (`refetch`).
- Use `EmptyState` when filter search yields zero rows to guide users on next actions.

---

## 11. Do's and Don'ts
- **DO:** Render `LoadingState` during initial page data fetches.
- **DON'T:** Leave blank white screens during async network loading.

---

## 12. Code Example
```tsx
if (isLoading) return <LoadingState message="Fetching customers..." />;
if (isError) return <ErrorState message={error.message} onRetry={refetch} />;
if (data.length === 0) return <EmptyState title="No customers found" actionText="New Customer" onAction={handleCreate} />;
```

---

## 13. Decision Rules & Checklist
- [ ] Are loading, error, and empty states explicitly handled in page components?
- [ ] Does `ErrorState` provide a retry button?
