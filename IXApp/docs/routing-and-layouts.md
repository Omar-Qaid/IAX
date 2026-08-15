# Routing, layouts, and navigation

## Runtime route composition

`src/app/App.tsx` renders `AppProviders` and `AppRoutes`. `AppRoutes` owns the `BrowserRouter` and passes `appRoutes` to React Router's `useRoutes`.

The route configuration is split by responsibility:

- `routes/routePaths.ts` contains URL constants and URL builders.
- `routes/pageRegistry.ts` is the registry of lazy-loaded business pages and optional permissions.
- `routes/routeConfig.tsx` builds login, protected shell, index, page, access-denied, route-error, and not-found routes.
- `routes/routeMetadata.ts` derives breadcrumbs from registered navigation and supplies special metadata for parameterized routes.
- `configuration/navigation.ts` defines module navigation and filters links that are not present in the page registry.

Add a routable page by adding a path, a lazy page definition, navigation metadata/link where appropriate, translations, permission constants, and route tests. `APP_PAGE_DEFINITIONS` is the source used by routing and navigation validation.

## Guards

`RouteGuard` waits for authentication bootstrap, redirects anonymous users to `/login`, preserves `pathname + search` in `location.state.returnTo`, and renders an app-owned access-denied state when the registered permission fails. `PermissionGuard` in `core/auth` is for hiding or replacing individual UI regions and does not perform navigation.

## Layouts

- `AppLayout` wraps protected pages in `AppShell` and normally renders an `Outlet`.
- `AuthLayout` centers login content in a constrained `Paper`.
- `FullScreenLayout` exists as a full-viewport container but is not currently selected by `routeConfig.tsx`.

## Responsive shell

`AppShell` uses a fixed top bar and a flex main region. At widths below the MUI `md` breakpoint the content takes full width and sidebar interaction becomes mobile-oriented. The preference store supports `vertical`, `horizontal`, and `mini` navigation layouts. The shell also mounts the command palette, notification drawer, and settings drawer once at application scope.

## Navigation consistency

Do not hardcode a route when a `ROUTE_PATHS` constant or builder exists. Navigation links without a registered page are filtered out by `filterModuleNavigation`. Update `routeMetadata.ts` for parameterized pages or breadcrumb sequences that cannot be inferred from module navigation.

See also [App shell](app-shell.md), [Authentication](authentication.md), and [Testing](testing.md).
