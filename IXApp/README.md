# IXApp - D365 Finance & Operations-Style Enterprise Frontend Architecture

`IXApp` is a modular, high-performance React 18/19 and TypeScript enterprise web application designed following Microsoft Dynamics 365 Finance & Operations (D365 F&O) architecture and design patterns.

---

## 1. Technology Stack

- **Core & Runtime**: React.js, TypeScript (Strict Mode), Vite
- **UI Framework & Design System**: Material UI (MUI v6), MUI X Data Grid
- **Routing**: React Router v7 (Lazy Loaded Modules)
- **State Management**: TanStack React Query (Server State), Zustand (Global Client State)
- **Forms & Validation**: React Hook Form, Zod
- **API & Networking**: Axios with ASP.NET Core REST Web API integration support
- **Localization**: i18next, react-i18next (English LTR & Arabic RTL)
- **Testing & Quality**: Vitest, React Testing Library, Oxlint / ESLint, Prettier

---

## 2. Project Architecture & Folder Structure

The project strictly adheres to a feature-based modular dependency hierarchy:

```text
app -> modules -> patterns -> shared -> core
```

```text
IXApp/
├── public/
│   └── locales/
│       ├── en/translation.json
│       └── ar/translation.json
│
├── src/
│   ├── app/
│   │   ├── layouts/       # AppLayout, AuthLayout, FullScreenLayout
│   │   ├── providers/     # AppProviders, QueryProvider, ThemeProvider, NotificationProvider
│   │   ├── routes/        # AppRoutes, routeConfig, RouteGuard, routePaths
│   │   ├── store/         # useAppStore, useNavigationStore, usePreferenceStore
│   │   └── theme/         # D365 Compact Palette, Typography, Component Overrides
│   │
│   ├── core/
│   │   ├── api/           # Axios Client, Interceptors, ApiError, ApiValidationProblem
│   │   ├── auth/          # AuthProvider, AuthContext, PermissionGuard, useAuth
│   │   ├── localization/  # i18n, Language definitions, LTR/RTL switcher
│   │   ├── permissions/   # Granular permissions & PermissionGuard
│   │   ├── errors/        # ErrorBoundary, ErrorMapper, ErrorMessages
│   │   ├── constants/     # App constants, Date formats, Storage keys
│   │   └── utilities/     # Formatters, Date, Enum, Validation utilities
│   │
│   ├── shared/
│   │   ├── components/
│   │   │   ├── app-shell/   # AppTopBar, AppNavigationDrawer, ModuleNavigation, GlobalSearch
│   │   │   ├── page/        # PageHeader, PageTitle, PageBreadcrumbs, PageContainer
│   │   │   ├── action-pane/ # D365 ActionPane, ActionPaneGroup, ActionPaneButton
│   │   │   ├── fast-tabs/   # Collapsible FastTabs accordions with summary
│   │   │   ├── fields/      # AppTextField, AppNumberField, AppCurrencyField, AppSelectField
│   │   │   ├── forms/       # EntityForm, FormRow, FormColumn, FormValidationSummary
│   │   │   ├── data-grid/   # AppDataGrid, DataGridColumnFactory
│   │   │   ├── dialogs/     # AppDialog, ConfirmationDialog, DeleteConfirmationDialog
│   │   │   └── feedback/    # LoadingState, ErrorState, EmptyState, AccessDeniedState
│   │   └── hooks/           # usePageMode, usePageRefresh, useNotifications
│   │
│   ├── patterns/            # Reusable Page Layout Patterns
│   │   ├── simple-list/     # SimpleListPage
│   │   ├── list-details/    # ListDetailsPage (Split View)
│   │   ├── master-form/     # MasterFormPage (System Settings)
│   │   ├── document/        # DocumentPage (Header-Lines Transactional)
│   │   └── workspace/       # WorkspacePage (Dashboard tiles & worklists)
│   │
│   ├── modules/             # Business Feature Modules
│   │   ├── dashboard/
│   │   ├── accounts-receivable/ (Customers, Customer Groups, Sales Orders)
│   │   ├── foundation/          (Currencies)
│   │   └── system-administration/ (Application Settings)
│   │
│   └── mocks/               # Strongly-typed mock data & services layer
```

---

## 3. Development Commands

### Installation
```bash
npm install
```

### Running Locally
```bash
npm run dev
```

### Building for Production
```bash
npm run build
```

### Running Tests
```bash
npm run test:run
```

### Linting
```bash
npm run lint
```

---

## 4. Key D365 Enterprise Concepts & Patterns

### 1. Simple List Page Pattern (`SimpleListPage`)
Used for setup and reference tables (e.g., `CurrenciesPage`, `CustomerGroupsPage`).
- Header + ActionPane + Filter + DataGrid + Page-level Bulk Save.

### 2. List & Details Page Pattern (`ListDetailsPage`)
Used for master records (e.g., `CustomersPage`).
- Split view layout: left list with quick search and right details pane organized in collapsible FastTabs (General, Financial, Contact).

### 3. Header-Lines Document Pattern (`DocumentPage`)
Used for transactional documents (e.g., `SalesOrderPage`).
- Document Header FastTabs + Editable Line Items Data Grid + Financial Totals Summary + Process Action Commands (`Confirm`, `Post Invoice`, `Cancel`).

### 4. Workspace Page Pattern (`WorkspacePage`)
Used for dashboards and role centers (e.g., `DashboardPage`).
- Summary KPI tiles + Recent order lists + Quick task links.

---

## 5. Connecting to ASP.NET Core REST Web API

The frontend is fully configured to connect to an ASP.NET Core REST API.

1. Update `.env` or `.env.development`:
   ```env
   VITE_API_BASE_URL=https://localhost:7001/api
   VITE_ENABLE_MOCK_API=false
   ```
2. Replace mock service implementations in module `services/` with `apiClient` calls. ASP.NET Core `ValidationProblemDetails` responses (HTTP 400 with `errors` dictionary) are automatically parsed into `ApiError` instances.
