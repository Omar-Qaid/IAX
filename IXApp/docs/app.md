# Application Layer Documentation (`src/app`)

## 1. Purpose and Responsibilities
The `app` layer is the top-level orchestration layer of **IXApp**. It is responsible for application bootstrapping, global provider composition, layout framing, routing registration, global Zustand stores, theme creation, environment configuration, and feature flags.

The `app` layer does **not** contain business domain rules or generic UI field components. It imports modules, page patterns, shared controls, and core utilities to assemble the running enterprise application.

---

## 2. Folder Structure
```text
src/app/
├── App.tsx                    # Root Application component
├── main.tsx                   # Vite DOM entry point
├── configuration/             # App-wide static configuration
│   ├── appConfig.ts           # App metadata, defaults, timeouts
│   ├── environment.ts         # Environment variable bindings
│   └── featureFlags.ts        # Dynamic feature toggles
├── layouts/                   # Top-level page layout frames
│   ├── AppLayout.tsx          # Main enterprise shell layout frame
│   ├── AuthLayout.tsx         # Centered layout for login/auth
│   └── FullScreenLayout.tsx   # Full-screen mode layout
├── providers/                 # Global React Context providers
│   ├── AppProviders.tsx       # Composite provider wrapper
│   ├── QueryProvider.tsx      # TanStack Query client setup
│   ├── ThemeProvider.tsx      # MUI theme & Emotion cache provider
│   ├── LocalizationProvider.tsx # i18next & LTR/RTL provider
│   └── NotificationProvider.tsx # Global Toast/Snackbar provider
├── routes/                    # Centralized route definitions
│   ├── AppRoutes.tsx          # Main router component
│   ├── routeConfig.tsx        # Route metadata and lazy loaders
│   ├── RouteGuard.tsx         # Auth & permission route protection
│   ├── routePaths.ts          # Centralized URL string constants
│   └── types.ts               # Routing type contracts
├── store/                     # Global Zustand application stores
│   ├── useAuthStore.ts        # User session & token state
│   ├── useNavigationStore.ts  # Sidebar, drawer & menu state
│   └── useThemeStore.ts       # Theme mode (light/dark) & density state
└── theme/                     # Enterprise Material UI visual system
    ├── createAppTheme.ts      # Theme factory for LTR/RTL & dark mode
    ├── componentOverrides.ts  # Centralized Material UI overrides
    ├── palette.ts             # Palette colors & HSL tailored tokens
    ├── shadows.ts             # Subtle enterprise shadow definitions
    ├── spacing.ts             # Compact D365 density spacing
    └── typography.ts          # Inter/Roboto typography system
```

---

## 3. File Naming Conventions
- **React Components & Layouts:** `PascalCase.tsx` (e.g., `AppLayout.tsx`, `AppProviders.tsx`)
- **Stores & Hooks:** `camelCase.ts` starting with `use` (e.g., `useNavigationStore.ts`, `useThemeStore.ts`)
- **Configuration & Utilities:** `camelCase.ts` (e.g., `appConfig.ts`, `routePaths.ts`)

---

## 4. Components in App Layer
- **`App.tsx`:** The root React element. Encloses all application routes inside `AppProviders`.
- **`AppLayout.tsx`:** Renders the enterprise shell (`AppShell`) containing `AppTopBar`, `AppSidebar`, breadcrumbs, and `Outlet` for active routes.
- **`AuthLayout.tsx`:** Renders a clean, centered container for authentication screens.
- **`FullScreenLayout.tsx`:** Renders standalone views without shell navigation bars.

---

## 5. Hooks
- **`useNavigationStore`:** Accesses sidebar collapsed state, module selection, and mobile drawer state.
- **`useThemeStore`:** Accesses current theme mode (`light` | `dark`) and document direction (`ltr` | `rtl`).
- **`useAuthStore`:** Accesses user session, auth token, and logged-in user profile.

---

## 6. Services & APIs
The `app` layer does **not** make direct Axios calls. It initializes the `QueryClient` inside `QueryProvider` and provides the environment variables (`environment.ts`) to the `core/apiClient`.

---

## 7. State Management
- **Zustand Stores (`@app/store/*`):** Used **only** for client-side UI preferences:
  - Navigation drawer open/collapsed state.
  - Active theme mode and language direction.
  - Active company / legal entity context.
- **Rules:** Never store API server data or form values inside Zustand. Use TanStack Query for server state and React Hook Form for form state.

---

## 8. Design Patterns
- **Provider Pattern (`AppProviders.tsx`):** Composes multiple contexts (`QueryClientProvider`, `MUI ThemeProvider`, `I18nextProvider`) into a clean hierarchy.
- **Layout Route Pattern (`AppLayout.tsx`):** Renders persistent navigation frames around nested child routes using `react-router-dom` `<Outlet />`.
- **Higher-Order Route Guard (`RouteGuard.tsx`):** Protects routes based on authentication and RBAC permissions before rendering target page elements.

---

## 9. Architecture & Dependencies
- **Dependencies Allowed:** `@app` $\rightarrow$ `@modules`, `@patterns`, `@shared`, `@core`.
- **Forbidden Dependencies:** Low-level layers (`@core`, `@shared`) must **never** import from `@app`.

---

## 10. Data Flow
1. `main.tsx` mounts `App.tsx`.
2. `App.tsx` wraps the tree in `AppProviders.tsx`.
3. `AppRoutes.tsx` reads `routeConfig.tsx` and applies `RouteGuard`.
4. Matching layout (`AppLayout.tsx`) mounts the `AppShell` and renders the active module page inside `<Outlet />`.

---

## 11. Best Practices & Reusability Rules
- **Lazy Loading:** Always lazy load domain module pages (`const CustomersPage = lazy(() => import('@modules/accounts-receivable/...'))`).
- **Centralized Route Paths:** Never hardcode route strings like `"/customers"` in components. Always reference `routePaths.accountsReceivable.customers`.

---

## 12. Generic Implementation Guidelines
- When adding a new global provider, wrap it inside `AppProviders.tsx` in correct dependency sequence.
- When creating a layout, render children via `<Outlet />` or `children` prop cleanly.

---

## 13. Do's and Don'ts
- **DO:** Keep `App.tsx` clean and readable.
- **DO:** Use `appConfig.ts` for global defaults.
- **DON'T:** Fetch API data directly in layout or provider components.
- **DON'T:** Put business entity schemas in the `app` layer.

---

## 14. Common Mistakes
- **Mistake:** Importing a page from `@modules` synchronously without `React.lazy()`.
- **Correction:** Use `React.lazy()` inside `routeConfig.tsx` to enable Vite code-splitting.

---

## 15. Examples

### Adding a New Route Path in `routePaths.ts`
```ts
export const routePaths = {
  dashboard: '/dashboard',
  accountsReceivable: {
    root: '/accounts-receivable',
    customers: '/accounts-receivable/customers',
    customerDetails: '/accounts-receivable/customers/:id',
  },
} as const;
```

---

## 16. Decision Rules & Checklist
- [ ] Is the route path added to `routePaths.ts`?
- [ ] Is the route component lazy loaded in `routeConfig.tsx`?
- [ ] Are permissions specified if the route requires authorization?
- [ ] Does `npm run build` compile without chunk errors?

---

## 17. Extension Guidelines
To add a new application layout:
1. Create `src/app/layouts/NewLayout.tsx`.
2. Wrap child content with `<Outlet />`.
3. Register the layout option in `src/app/routes/types.ts`.

---

## 18. Performance Considerations
- All module pages are split into separate JavaScript chunks via `React.lazy()`.
- Theme objects are memoized via `createAppTheme(mode, direction)` to prevent DOM style recalculation thrashing.
