# Testing Strategy & Guidelines (`src/test`)

## 1. Purpose and Responsibilities
The `test` directory contains test utilities, setup mocks, and automated test suites for **IXApp**. 

Testing ensures that components, forms, page patterns, routing, permissions, error mapping, and data grids perform reliably across light/dark themes and LTR/RTL locales.

---

## 2. Technology Stack & Frameworks
- **Test Runner:** Vitest (`vitest`)
- **DOM Environment:** jsdom (`jsdom`)
- **React Testing Utility:** React Testing Library (`@testing-library/react`)
- **DOM Assertions:** `@testing-library/jest-dom`
- **User Interactions:** `@testing-library/user-event`

---

## 3. Folder Structure & Test Organization
```text
src/test/
├── setupTests.ts              # Global setup & ResizeObserver polyfill
├── testUtils.tsx              # Custom renderWithProviders wrapper
├── app/                       # App layer integration tests
│   ├── AppProviders.test.tsx
│   ├── AppShell.test.tsx
│   ├── Routing.test.tsx
│   └── EnterpriseCore.test.tsx
├── core/                      # Core infrastructure unit tests
│   ├── errorMapper.test.ts
│   ├── formatUtils.test.ts
│   ├── localization.test.ts
│   └── Permissions.test.tsx
├── shared/                    # Shared controls component tests
│   ├── DataGrid.test.tsx
│   ├── Dialog.test.tsx
│   ├── FormField.test.tsx
│   ├── GridLookupReference.test.tsx
│   ├── LogisticsAddress.test.tsx
│   ├── Lookup.test.tsx
│   ├── Notification.test.tsx
│   └── PageHeader.test.tsx
├── patterns/                  # Page pattern architecture tests
│   └── PagePatterns.test.tsx
├── modules/                   # Module scaffolding tests
│   └── ModulesScaffolding.test.tsx
└── mocks/                     # Mock service integration tests
    └── MockServices.test.tsx
```

---

## 4. Vitest ESM Icon Import Rule (CRITICAL)
**CRITICAL RULE:** Do NOT import icons from `@mui/icons-material` barrel index (`import { Delete } from '@mui/icons-material'`). Vitest ESM in JSDOM resolves barrel icon imports to `undefined`, causing test suite runtime failures.

**Always use specific path imports:**
```tsx
// CORRECT:
import DeleteIcon from '@mui/icons-material/Delete';
import SearchIcon from '@mui/icons-material/Search';
import CloseIcon from '@mui/icons-material/Close';
```

---

## 5. JSDOM & TanStack Virtualization Quirk
In JSDOM test environments, DOM scroll containers have 0 height by default. `@tanstack/react-virtual` relies on container dimensions and will return an empty virtual items list (`virtualItems = []`).

**Rule:** Grid and list controls (`DataGridBody.tsx`, `LookupGrid.tsx`) must include a fallback to render items directly when `virtualItems.length === 0`:
```tsx
const displayVirtualItems =
  virtualItems.length > 0
    ? virtualItems
    : rows.map((_, index) => ({
        index,
        key: index,
        start: index * rowHeight,
        size: rowHeight,
      }));
```

---

## 6. Test Utility & Provider Wrapper (`testUtils.tsx`)
Always render components inside `renderWithProviders` or wrap with `QueryClientProvider` and `MemoryRouter`:

```tsx
import React from 'react';
import { render } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';

export function renderWithProviders(ui: React.ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>{ui}</MemoryRouter>
    </QueryClientProvider>
  );
}
```

---

## 7. Quality & Execution Commands
Run the following scripts to validate codebase quality:

```bash
# Run unit tests
npm run test:run

# Run TypeScript type check
npm run typecheck

# Run ESLint validation
npm run lint

# Run Vite production build
npm run build
```

---

## 8. Do's and Don'ts
- **DO:** Use accessible queries (`getByRole`, `getByLabelText`, `getByText`) to test user-facing behavior.
- **DO:** Polyfill `ResizeObserver` in `setupTests.ts`.
- **DON'T:** Test private component implementation details or internal state variables.
- **DON'T:** Use barrel icon imports anywhere in `@shared`, `@patterns`, or `@modules`.
