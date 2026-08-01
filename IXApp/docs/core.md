# Core Layer Documentation (`src/core`)

## 1. Purpose and Responsibilities
The `core` layer contains infrastructure-neutral, low-level application foundations for **IXApp**. It is completely decoupled from UI components, page layouts, and business domain modules.

The `core` layer provides:
- Centralized HTTP Axios client and interceptors (`@core/api`).
- Normalized API error models and problem details mapping (`@core/errors`).
- Authentication contracts, session management, and RBAC permission evaluation (`@core/auth`, `@core/permissions`).
- Bilingual localization engine configuration (`@core/localization`).
- Standard TypeScript contracts, pagination types, and pure helper utilities (`@core/types`, `@core/utilities`).

---

## 2. Folder Structure
```text
src/core/
├── api/                       # HTTP API client infrastructure
│   ├── apiClient.ts           # Centralized Axios instance
│   ├── apiConfig.ts           # Base URL, timeout & header defaults
│   ├── apiError.ts            # HttpError & ApiValidationProblem contracts
│   ├── apiResponse.ts         # Generic ApiResponse<T> wrapper
│   └── interceptors.ts        # Request token & response error interceptors
├── auth/                      # Authentication infrastructure
│   ├── AuthContext.tsx        # Auth state context
│   ├── AuthProvider.tsx       # Auth context provider implementation
│   ├── PermissionGuard.tsx    # Conditional RBAC UI rendering wrapper
│   ├── useAuth.ts             # Auth session access hook
│   └── usePermissions.ts      # Permission check helper hook
├── errors/                    # Error handling system
│   ├── AppError.ts            # Normalized application error class
│   ├── ErrorBoundary.tsx      # React error boundary component
│   ├── errorMapper.ts         # Maps Axios/HTTP errors to AppError
│   └── errorMessages.ts       # Centralized error text constants
├── localization/              # i18n setup & utilities
│   ├── i18n.ts                # i18next initialization
│   ├── languages.ts           # Supported languages (EN, AR)
│   └── useAppTranslation.ts   # Typed translation hook wrapper
├── permissions/               # RBAC permission evaluation engine
│   ├── permissionService.ts   # In-memory permission matrix evaluator
│   ├── permissions.ts         # Permission constants & enum keys
│   └── usePermission.ts       # Single-permission verification hook
├── types/                     # Shared generic TypeScript types
│   ├── common.ts              # PagedResult, SortDirection, EntityId
│   └── index.ts               # Type exports
└── utilities/                 # Pure non-UI helper functions
    ├── dateUtils.ts           # Date formatting & parsing
    ├── enumUtils.ts           # Enum key/value extraction
    ├── formatUtils.ts         # Currency & number formatting
    ├── objectUtils.ts         # Object deep merge & diffing
    └── stringUtils.ts         # String trimming & slugification
```

---

## 3. File Naming Conventions
- **Files:** `camelCase.ts` or `PascalCase.tsx` for React context/guard components.
- **Classes & Interfaces:** `PascalCase` (e.g., `AppError`, `ApiValidationProblem`, `PagedResult<T>`).
- **Hooks:** `camelCase.ts` starting with `use` (e.g., `useAuth.ts`, `usePermissions.ts`).

---

## 4. Components & Contracts in Core Layer
- **`apiClient`:** Pre-configured Axios instance using `VITE_API_BASE_URL`. Includes request cancellation and authorization header injection.
- **`AppError`:** Unified error class containing `category`, `message`, `statusCode`, `details`, and `fieldErrors`.
- **`PermissionGuard`:** Component wrapper that conditionally renders `children` if the current user has the required permission:
  ```tsx
  <PermissionGuard module="AccountsReceivable" resource="Customers" action="Create">
    <Button>New Customer</Button>
  </PermissionGuard>
  ```

---

## 5. Hooks
- **`useAuth()`:** Accesses current authenticated user, login, logout, and token functions.
- **`usePermissions()`:** Evaluates `hasPermission(module, resource, action)`, `canView(module, resource)`, and `isAdmin`.
- **`usePermission(permissionCode)`:** Evaluates a single permission requirement string.

---

## 6. Services & APIs
- Core services (like `permissionService`) are pure, in-memory singletons.
- Visual components **must never** import Axios directly; they consume services or hooks that use `apiClient`.

---

## 7. State Management
- Authentication token is stored securely in memory / local storage and exposed via `AuthContext`.
- Core layer contains **no Zustand stores** and **no UI state**.

---

## 8. Design Patterns
- **Singleton Pattern (`apiClient`, `permissionService`):** Single shared instances configured at startup.
- **Adapter Pattern (`errorMapper.ts`):** Translates raw HTTP 400/401/403/404/500 backend responses into standardized `AppError` objects.
- **Guard Pattern (`PermissionGuard.tsx`):** Structural component guarding UI elements based on security rights.

---

## 9. Architecture & Dependencies
- **Dependencies Allowed:** External libraries only (`axios`, `i18next`, `react`).
- **Forbidden Dependencies:** Core must **never** import from `@shared`, `@patterns`, `@modules`, or `@app`.

---

## 10. Data Flow
1. API request is initiated via a service function calling `apiClient.get/post`.
2. `interceptors.ts` injects Bearer Token and Correlation ID.
3. Response is returned, or intercepted by `errorMapper.ts` if an HTTP error occurs.
4. `errorMapper.ts` creates a standardized `AppError` and bubbles it up to the calling hook.

---

## 11. Best Practices & Reusability Rules
- **No Direct DOM or MUI Imports in Core:** Core utilities (`dateUtils`, `formatUtils`, `enumUtils`) must remain pure TypeScript without React/MUI DOM dependencies.
- **Strict Typing:** Always return generic types (`PagedResult<T>`, `ApiResponse<T>`).

---

## 12. Do's and Don'ts
- **DO:** Centralize all API client configuration in `@core/api/apiClient.ts`.
- **DO:** Standardize error mapping using `errorMapper`.
- **DON'T:** Import React UI components or MUI theme styles inside core utilities.
- **DON'T:** Bypass `apiClient` to make raw `fetch()` or `axios.create()` calls elsewhere.

---

## 13. Common Mistakes
- **Mistake:** Throwing raw Axios errors in services.
- **Correction:** Pass errors through `errorMapper(error)` to ensure field errors and validation problems are structured cleanly.

---

## 14. Examples

### Standardized `PagedResult<T>` Interface (`src/core/types/common.ts`)
```ts
export interface PagedResult<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalRecords: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
```

### Standard Error Mapping (`src/core/errors/errorMapper.ts`)
```ts
export function mapApiError(error: unknown): AppError {
  if (isAxiosError(error) && error.response?.data) {
    const data = error.response.data as ApiValidationProblem;
    return new AppError({
      message: data.detail || data.title || 'An API error occurred',
      statusCode: error.response.status,
      fieldErrors: data.errors,
    });
  }
  return new AppError({ message: 'Network or server error' });
}
```

---

## 15. Decision Rules & Checklist
- [ ] Is the utility function pure and free of UI/MUI dependencies?
- [ ] Does the API function use `apiClient`?
- [ ] Are response contracts generic (`<T>`)?
- [ ] Are all error cases mapped to `AppError`?

---

## 16. Performance Considerations
- Interceptors use lightweight memory operations.
- Permission matrix evaluation is performed in $\mathcal{O}(1)$ time using indexed maps.
