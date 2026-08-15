# Shared hooks

The current domain-neutral hooks are:

- `useDebounce`: delayed value updates, default 300 ms.
- `useDisclosure`: local open/close/toggle controls.
- `useLocalStorage`: typed persisted state.
- `useNotifications`: stable helpers for the global notification queue.
- `usePageMode`: `view`, `create`, `edit`, `copy`, `readonly`, and `process` mode helpers.
- `usePageRefresh`: refresh callback with optional dirty confirmation.
- `usePrevious`: previous render value.
- `useUnsavedChanges`: browser `beforeunload` protection only.
- `useListPage`: legacy generic list loading/save/delete state.
- `useDocumentPage`: legacy document load/process state; callers explicitly call `fetchDocument`.
- `useLookupGridField`/`useGridLookupData`: infinite lookup data through TanStack Query.
- `useLogisticsAddress` and its geography query hooks.

There is also a pattern-owned `patterns/document/useDocumentPage.ts`; import deliberately because it is not the same module as `shared/hooks/useDocumentPage.ts`.

Hooks should remain route- and domain-agnostic. Keep callbacks stable where they are passed to query keys or deeply memoized controls, and do not hide feature-specific API URLs inside shared hooks.
