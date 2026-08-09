# Shared Hooks Documentation (`src/shared/hooks`)

## 1. Purpose and Responsibilities

The `hooks` sub-system provides generic, domain-agnostic React hooks for **IXApp**. These hooks manage notification triggering (`useNotifications`), input debouncing (`useDebounce`), lookup infinite queries (`useLookupGridField`), cascading geography data (`useLogisticsAddress`), page mode transitions (`usePageMode`), list state management (`useListPage`), document state management (`useDocumentPage`), and unsaved changes dirty checking (`useUnsavedChanges`).

---

## 2. Folder Structure

```text
src/shared/hooks/
├── useNotifications.ts       # Global toast notification trigger hook
├── useDebounce.ts            # Input debouncing hook for real-time search
├── useLookupGridField.ts     # TanStack React Query infinite pagination query manager for lookups
├── useLogisticsAddress.ts    # Cascading geography queries (Countries, States, Cities, Counties)
├── usePageMode.ts            # PageMode transition state hook ('view' | 'create' | 'edit')
├── useListPage.ts            # State manager for simple list & detail page patterns
├── useDocumentPage.ts        # Header-lines document state & calculation manager
├── useUnsavedChanges.ts      # Tracks dirty form state & prompts navigation warning
├── usePageRefresh.ts         # Centralized page data refetch hook
└── useLocalStorage.ts        # Browser localStorage state persistence hook
```

---

## 3. Naming Conventions

- **Files & Hooks:** `camelCase.ts` starting with `use` (e.g., `useNotifications.ts`, `useLookupGridField.ts`).

---

## 4. Key Shared Hooks

### 4.1 `useNotifications()`

Accesses the global Snackbar notification provider to emit toast notifications:

```ts
const { notifySuccess, notifyError, notifyInfo, notifyWarning } = useNotifications();
notifySuccess('Customer created successfully');
notifyError('Failed to save changes');
```

The hook and its small Zustand notification queue are shared-owned; the app provider
only renders the active notification.

### 4.2 `useDebounce<T>(value: T, delay: number): T`

Debounces rapidly changing values (e.g. search input fields) by `delay` milliseconds to prevent excessive API network calls.

### 4.3 `useLookupGridField()`

TanStack React Query infinite pagination manager for multi-column grid lookups. Merges page results into a single flat row array and exposes `fetchNextPage`, `hasNextPage`, and `isFetchingNextPage`.

### 4.4 `useLogisticsAddress()`

Provides cascading TanStack React Query hooks for logistics address management:

- `useCountryRegions()`: Fetches all country regions.
- `useStates(countryRegionId)`: Fetches states filtered by selected country.
- `useCities(stateId)`: Fetches cities filtered by selected state.
- `useCounties(stateId)`: Fetches counties filtered by selected state.

Runtime mock selection uses the core environment binding and shared-owned compatible
fixtures, so the hook does not depend on application composition or the global mock layer.

---

## 5. Architecture & Layer Rules

- **Allowed:** `@shared/hooks` $\rightarrow$ `@core`.
- **Forbidden:** No business module imports (`@modules/*`).

---

## 6. Best Practices

- Use `useDebounce` with a default delay of `300ms` for all server-side search input fields.
- Wrap dirty form state checks with `useUnsavedChanges` to prevent accidental loss of user edits during page navigation.

---

## 7. Code Example

```tsx
function SearchableList() {
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearch = useDebounce(searchTerm, 300);

  const { data, isLoading } = useQuery({
    queryKey: ['customers', debouncedSearch],
    queryFn: () => customerService.search(debouncedSearch),
  });
}
```
