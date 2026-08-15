# Application layer (`src/app`)

The application layer composes the running frontend. It owns bootstrap, global providers, layouts, routes, navigation configuration, the shell, theme construction, and application-wide UI stores. Domain behavior belongs in `modules`; route-agnostic UI belongs in `shared` or `patterns`.

## Bootstrap and providers

`main.tsx` mounts `App`; `App.tsx` wraps `AppRoutes` in `AppProviders`. Provider order is significant:

1. `ErrorBoundary`
2. `LocalizationProvider`
3. `QueryProvider`
4. `AuthProvider`
5. `ThemeProvider`
6. `NotificationProvider`

This ensures localization and query infrastructure exist during authentication bootstrap, while the theme can use language direction and persisted preferences.

## Folders

```text
src/app/
  configuration/  layout constants, feature flags, module navigation
  layouts/        AppLayout, AuthLayout, FullScreenLayout
  navigation/     reusable app-owned navigation rendering
  providers/      provider composition and adapters
  routes/         paths, page registry, guards, metadata, route tree
  shell/          top bar, sidebar, module panel, command palette, drawers
  store/          company, navigation, and preference Zustand stores
  theme/          palettes, typography, spacing, shadows, MUI overrides
```

There is no `appConfig.ts`, `useAuthStore.ts`, or `useThemeStore.ts`. Runtime API configuration lives in `core/configuration`; authentication uses `AuthContext`; visual preferences use `usePreferenceStore`.

## Application stores

- `useAppStore` persists the current company.
- `useNavigationStore` controls shell panels and persists favorites, recent pages, and pinned navigation state.
- `usePreferenceStore` persists visual and navigation preferences.

These stores do not own API records or normal form values. See [API and state](api-and-state.md).

## Route ownership

Routes are data-driven through `APP_PAGE_DEFINITIONS`, with lazy module imports and optional permissions. See [Routing and layouts](routing-and-layouts.md).

## Theme and configuration

`createAppTheme` builds an LTR/RTL MUI theme from palette, typography, density, contrast, preset color, and font preferences. `ThemeProvider` also applies document language, direction, density, contrast, and zoom. `configuration/constants.ts` contains layout, color, API endpoint, pagination, and grid-filter constants; `featureFlags.ts` exposes the current static flags.

## Dependency rule

`app` may compose all frontend layers. Lower layers must not import from `app`; the architecture audit enforces this boundary. App-owned route and shell components may import module definitions because they are composition roots.

Related: [Architecture boundaries](ARCHITECTURE-BOUNDARIES.md), [App shell](app-shell.md), and [Development guidelines](development.md).
