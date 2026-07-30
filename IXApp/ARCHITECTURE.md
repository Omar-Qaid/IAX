# IXApp Frontend Architecture

## 1. Document Purpose

This document defines the frontend architecture, dependency rules, project organization, design patterns, implementation standards, and engineering conventions for **IXApp**.

IXApp is a React.js enterprise frontend inspired by Microsoft Dynamics 365 Finance & Operations application concepts. It does not reproduce Microsoft proprietary source code, branding, icons, or exact visual design. Instead, it applies similar enterprise interaction patterns using React, TypeScript, Material UI, and reusable application architecture.

The frontend is designed to connect to an ASP.NET Core REST Web API.

---

## 2. Architecture Goals

The architecture must provide:

- A scalable enterprise frontend foundation.
- Feature-based business modules.
- Reusable page patterns and enterprise controls.
- Clear separation between application, business, presentation, and infrastructure concerns.
- Centralized API, authentication, permissions, localization, theme, errors, and notifications.
- Strong TypeScript contracts.
- Predictable dependency direction.
- Responsive desktop and mobile behavior.
- English and Arabic localization.
- LTR and RTL layouts.
- Light and dark themes.
- Compact D365-style enterprise density.
- Testable services, hooks, components, and page patterns.
- A safe migration path from mock services to the ASP.NET Core REST API.

---

## 3. Technology Stack

### Core Technologies

```text
React.js
TypeScript
Vite
Material UI
MUI X Data Grid
React Router
Axios
TanStack Query
React Hook Form
Zod
Zustand
i18next
ESLint
Prettier
Vitest
React Testing Library
```

### Package Management

Use `npm` unless the project environment explicitly requires another package manager.

### Excluded Technologies

Do not introduce the following without an approved architectural decision:

```text
Next.js
Angular
Vue
Bootstrap
Tailwind CSS
Redux
Multiple UI frameworks
Multiple form libraries
Multiple HTTP clients
```

Redux may only be introduced when Zustand and the current state architecture cannot satisfy a verified requirement.

---

## 4. Architectural Style

IXApp uses a **feature-based layered modular architecture**.

The primary architecture layers are:

```text
app
modules
patterns
shared
core
```

Each layer has a specific responsibility and a controlled dependency direction.

### Layer Responsibilities

| Layer | Responsibility |
|---|---|
| `app` | Application bootstrap, providers, routing, layouts, global configuration, theme composition, and application-level state |
| `modules` | Business features, business pages, domain hooks, validation, services, types, and module route definitions |
| `patterns` | Reusable enterprise page patterns such as simple list, master-detail, document, workspace, inquiry, and process pages |
| `shared` | Reusable UI components, hooks, dialogs, fields, grids, forms, feedback controls, and generic utilities |
| `core` | Infrastructure-neutral foundations such as API client, auth contracts, errors, permissions, localization, common types, and low-level utilities |

---

## 5. Dependency Direction

The required dependency direction is:

```text
app
↓
modules
↓
patterns
↓
shared
↓
core
```

### Permitted Dependencies

```text
app → modules, patterns, shared, core
modules → patterns, shared, core
patterns → shared, core
shared → core
core → external libraries only
```

### Forbidden Dependencies

```text
core → shared
core → patterns
core → modules

shared → patterns
shared → modules

patterns → modules

business module → another business module directly
```

### Cross-Module Communication

A business module must not import another business module directly.

When one module needs data or behavior owned by another module, use one of these approaches:

1. A shared contract in `core` or `shared`.
2. An application-level orchestration service.
3. A backend API endpoint.
4. A shared read-only lookup abstraction.
5. An event or command abstraction owned by the application layer.

---

## 6. Project Structure

```text
IXApp/
├── public/
│   ├── favicon.ico
│   └── locales/
│       ├── en/
│       │   └── translation.json
│       └── ar/
│           └── translation.json
│
├── src/
│   ├── app/
│   │   ├── App.tsx
│   │   ├── main.tsx
│   │   ├── configuration/
│   │   ├── layouts/
│   │   ├── providers/
│   │   ├── routes/
│   │   ├── store/
│   │   └── theme/
│   │
│   ├── core/
│   │   ├── api/
│   │   ├── auth/
│   │   ├── errors/
│   │   ├── localization/
│   │   ├── permissions/
│   │   ├── routing/
│   │   ├── constants/
│   │   ├── types/
│   │   └── utilities/
│   │
│   ├── shared/
│   │   ├── components/
│   │   │   ├── app-shell/
│   │   │   ├── page/
│   │   │   ├── action-pane/
│   │   │   ├── data-grid/
│   │   │   ├── forms/
│   │   │   ├── fields/
│   │   │   ├── fast-tabs/
│   │   │   ├── dialogs/
│   │   │   ├── lookups/
│   │   │   ├── feedback/
│   │   │   ├── status/
│   │   │   └── common/
│   │   ├── hooks/
│   │   ├── services/
│   │   ├── validation/
│   │   ├── constants/
│   │   ├── types/
│   │   └── utilities/
│   │
│   ├── patterns/
│   │   ├── simple-list/
│   │   ├── list-details/
│   │   ├── master-form/
│   │   ├── master-detail/
│   │   ├── document/
│   │   ├── workspace/
│   │   ├── inquiry/
│   │   ├── setup/
│   │   ├── process/
│   │   ├── tree-details/
│   │   └── profile/
│   │
│   ├── modules/
│   │   ├── dashboard/
│   │   ├── accounts-receivable/
│   │   ├── accounts-payable/
│   │   ├── general-ledger/
│   │   ├── inventory-management/
│   │   ├── sales-and-marketing/
│   │   ├── procurement-and-sourcing/
│   │   ├── cash-and-bank-management/
│   │   ├── fixed-assets/
│   │   ├── human-resources/
│   │   ├── foundation/
│   │   └── system-administration/
│   │
│   ├── mocks/
│   ├── assets/
│   └── test/
│
├── .env
├── .env.development
├── .env.production
├── package.json
├── tsconfig.json
├── vite.config.ts
└── README.md
```

Do not create empty folders without an implemented responsibility or a documented placeholder export.

---

## 7. Application Layer

The `app` layer owns application composition.

### Responsibilities

- React application bootstrap.
- Global providers.
- Root layouts.
- Route registration.
- Global configuration.
- Theme creation.
- Application-wide stores.
- Feature flags.
- Environment configuration.
- Error boundaries.
- Authentication startup.
- Query client startup.
- Localization startup.

### Recommended Structure

```text
src/app/
├── App.tsx
├── main.tsx
├── configuration/
│   ├── appConfig.ts
│   ├── environment.ts
│   └── featureFlags.ts
├── layouts/
│   ├── AppLayout.tsx
│   ├── AuthLayout.tsx
│   └── FullScreenLayout.tsx
├── providers/
│   ├── AppProviders.tsx
│   ├── QueryProvider.tsx
│   ├── ThemeProvider.tsx
│   ├── LocalizationProvider.tsx
│   └── NotificationProvider.tsx
├── routes/
│   ├── AppRoutes.tsx
│   ├── routeConfig.tsx
│   ├── RouteGuard.tsx
│   ├── routePaths.ts
│   └── types.ts
├── store/
│   ├── useAppStore.ts
│   ├── useNavigationStore.ts
│   └── usePreferenceStore.ts
└── theme/
    ├── createAppTheme.ts
    ├── componentOverrides.ts
    ├── palette.ts
    ├── shadows.ts
    ├── spacing.ts
    ├── typography.ts
    └── types.ts
```

---

## 8. Core Layer

The `core` layer contains low-level application foundations.

It must remain independent from `shared`, `patterns`, and `modules`.

### Core Responsibilities

- Axios client configuration.
- Request and response interceptors.
- API error models.
- Standard API response contracts.
- Authentication contracts and storage.
- Permission contracts and services.
- Localization bootstrap.
- Route helper functions.
- Common constants.
- Generic application types.
- Pure utility functions.

### API Structure

```text
src/core/api/
├── apiClient.ts
├── apiConfig.ts
├── apiError.ts
├── apiResponse.ts
├── interceptors.ts
└── queryKeys.ts
```

### API Rules

- Visual components must never call Axios directly.
- API endpoints must be centralized.
- Authentication tokens must be added through interceptors.
- Errors must be normalized before reaching pages.
- API services must return typed data.
- Cancellation must be supported where appropriate.
- Backend validation errors must map to form fields when possible.
- Query keys must come from a centralized factory.

---

## 9. Shared Layer

The `shared` layer contains generic reusable functionality.

Shared components must not contain business-module-specific logic.

### Shared Component Categories

```text
app-shell
page
action-pane
data-grid
forms
fields
fast-tabs
dialogs
lookups
feedback
status
common
```

### Shared Design Rules

A shared component must:

- Be generic.
- Have a clear and stable API.
- Accept typed props.
- Avoid domain-specific terminology.
- Avoid direct API calls.
- Avoid importing module code.
- Support accessibility.
- Support localization.
- Support RTL where applicable.
- Use theme tokens instead of hardcoded colors.
- Separate rendering from orchestration when complexity increases.

---

## 10. Business Modules

Each business capability belongs inside `src/modules`.

A module owns its:

- Pages.
- Business components.
- Hooks.
- Services.
- Validation schemas.
- Domain types.
- Constants.
- Route definitions.
- Module-level public exports.

### Example Feature Structure

```text
src/modules/accounts-receivable/customers/
├── components/
│   ├── CustomerForm.tsx
│   ├── CustomerGrid.tsx
│   └── CustomerSummary.tsx
├── hooks/
│   ├── useCustomers.ts
│   └── useCustomerForm.ts
├── pages/
│   ├── CustomersPage.tsx
│   └── CustomerDetailsPage.tsx
├── services/
│   └── customerService.ts
├── validation/
│   └── customerSchema.ts
├── types/
│   └── customer.ts
├── constants/
│   └── customerConstants.ts
└── index.ts
```

### Module Rules

- Modules may use `patterns`, `shared`, and `core`.
- Module pages compose reusable patterns and controls.
- Domain validation remains inside the module.
- Domain services remain inside the module.
- Module-specific types must not leak into shared components.
- A module public API must be exposed through `index.ts`.
- Internal files should not be imported from outside the module through deep paths unless intentionally public.

---

## 11. Application Shell

IXApp uses an enterprise application shell inspired by D365 Finance & Operations.

### Shell Components

```text
AppShell
AppTopBar
AppNavigationDrawer
ModuleNavigation
NavigationGroup
NavigationItem
GlobalSearch
CompanySelector
NotificationMenu
UserMenu
PageBreadcrumbs
MainContent
```

### Desktop Composition

```text
┌──────────────────────────────────────────────────────────────┐
│ IXApp | Module | Search | Company | Notifications | User    │
├───────────────┬──────────────────────────────────────────────┤
│ Navigation    │ Breadcrumbs                                  │
│               ├──────────────────────────────────────────────┤
│               │ Page Header                                  │
│               │ Action Pane                                  │
│               │ Page Content                                 │
│               │                                              │
└───────────────┴──────────────────────────────────────────────┘
```

### Responsive Rules

On smaller screens:

- The navigation becomes a temporary drawer.
- The action pane supports horizontal scrolling or overflow menus.
- Forms become single-column.
- Data grids remain horizontally usable.
- Page padding is reduced.
- Nonessential toolbar content may collapse into menus.
- Company and user controls must remain accessible.

---

## 12. Navigation Architecture

Navigation must be configuration-driven.

### Navigation Model

A navigation item may contain:

```ts
interface NavigationItem {
  id: string;
  label: string;
  path?: string;
  icon?: React.ReactNode;
  children?: NavigationItem[];
  permission?: PermissionRequirement;
  hidden?: boolean;
  disabled?: boolean;
  order?: number;
}
```

### Navigation Rules

- Routes are the source of truth for navigation destinations.
- Permission filtering occurs before rendering.
- Active items derive from the current route.
- Navigation labels use localization keys.
- Module navigation must support groups and expandable sections.
- Recent pages and favorites are user preferences, not route definitions.
- Navigation configuration must not contain visual component state.

---

## 13. Routing

Routing is centralized and module-aware.

### Route Definition

Each route should support:

```ts
interface AppRoute {
  id: string;
  path: string;
  element: React.ReactNode;
  children?: AppRoute[];
  permission?: PermissionRequirement;
  layout?: 'app' | 'auth' | 'fullscreen';
  breadcrumb?: string;
  title?: string;
  hiddenFromNavigation?: boolean;
}
```

### Routing Rules

- Use lazy loading for business modules.
- Route guards enforce authentication and authorization.
- Breadcrumbs derive from route metadata.
- Paths must be centralized.
- Page components must not hardcode navigation URLs.
- Unknown routes render a not-found state.
- Unauthorized routes render an access-denied state.

---

## 14. Standard Page Composition

Every page follows this logical structure:

```text
Page
├── PageHeader
├── ActionPane
├── PageFilter
├── PageContent
├── PageDialogs
└── PageFeedback
```

A page should compose reusable parts instead of owning all behavior in one file.

Avoid placing the following in one oversized page component:

- API calls.
- Form initialization.
- Grid state.
- Mutation logic.
- Dialog state.
- Permission logic.
- Validation mapping.
- Large visual markup.

Move responsibilities into focused hooks, services, components, and page-pattern abstractions.

---

## 15. Reusable Page Patterns

### 15.1 Simple List

Use for setup, reference, and lightweight master data.

```text
SimpleListPage
├── PageHeader
├── ActionPane
├── FilterBar
├── DataGrid
├── SelectionSummary
├── Dialogs
└── PageFeedback
```

Examples:

- Currencies.
- Customer groups.
- Payment terms.
- Units.
- Tax groups.

### 15.2 List and Details

Use for records that require a list and a detail view.

```text
ListDetailsPage
├── PageHeader
├── ActionPane
├── SplitView
│   ├── RecordList
│   └── DetailsPane
├── Dialogs
└── PageFeedback
```

Examples:

- Customers.
- Vendors.
- Products.
- Employees.

### 15.3 Master Form

Use for parameters and application setup.

```text
MasterFormPage
├── PageHeader
├── ActionPane
├── FastTabs
├── FormSections
└── PageFeedback
```

Examples:

- Application settings.
- Module parameters.
- Posting setup.

### 15.4 Master-Detail

Use when a selected master record controls one or more detail collections.

```text
MasterDetailPage
├── MasterGridOrForm
├── DetailToolbar
├── DetailGrid
├── DetailDialogs
└── PageFeedback
```

Examples:

- Customer groups and posting profiles.
- Warehouses and locations.
- Journals and lines.

### 15.5 Header-Lines Document

Use for transactional documents.

```text
DocumentPage
├── DocumentHeader
├── ActionPane
├── StatusBar
├── HeaderFastTabs
├── LinesDataGrid
├── LineDetails
├── TotalsPanel
├── Dialogs
└── PageFeedback
```

Examples:

- Sales orders.
- Purchase orders.
- Journals.
- Transfer orders.

### 15.6 Workspace

Use for dashboards and operational work centers.

```text
WorkspacePage
├── WorkspaceHeader
├── ActionPane
├── SummaryTiles
├── KPISection
├── Charts
├── WorkLists
└── QuickLinks
```

### 15.7 Inquiry

Use for read-only analysis and filtered results.

### 15.8 Setup

Use for hierarchical or grouped configuration.

### 15.9 Process or Wizard

Use for multi-step operations.

### 15.10 Tree and Details

Use for hierarchical structures such as organization units and account structures.

### 15.11 Profile

Use for entity-centered views with summary, details, history, and related information.

---

## 16. Action Pane Architecture

The action pane provides D365-inspired grouped commands.

### Standard Action Groups

```text
New
Maintain
Process
Inquiries
Print
Options
```

### Common Commands

```text
New
Edit
Delete
Save
Cancel
Refresh
Copy
Confirm
Post
Submit
Approve
Reject
Print
Export
Attachments
History
```

### Action Definition

```ts
interface ActionDefinition {
  id: string;
  label: string;
  icon?: React.ReactNode;
  group: string;
  order?: number;
  hidden?: boolean;
  disabled?: boolean;
  loading?: boolean;
  permission?: PermissionRequirement;
  requiresSelection?: boolean;
  allowedPageModes?: PageMode[];
  tooltip?: string;
  keyboardShortcut?: string;
  onClick: () => void | Promise<void>;
}
```

### Action Rules

- Actions are configuration-driven.
- Permission filtering occurs before rendering.
- Actions must reflect the current page mode.
- Save and cancel are page-level commands.
- Loading actions must prevent duplicate execution.
- Destructive actions require confirmation.
- Keyboard shortcuts must not conflict with browser defaults.
- Hidden and disabled are distinct states.
- Overflow behavior is required on small screens.

---

## 17. Page Modes

Standard page modes:

```ts
enum PageMode {
  View = 'view',
  Create = 'create',
  Edit = 'edit'
}
```

### Expected Behavior

| Mode | Form | Grid | Actions |
|---|---|---|---|
| View | Read-only | Selectable | New, Edit, Refresh |
| Create | Editable | Context-dependent | Save, Cancel |
| Edit | Editable | Context-dependent | Save, Cancel, Refresh |

### State Rules

- Unsaved changes must be tracked.
- Navigation away from dirty pages requires confirmation.
- Refresh clears unsaved changes after confirmation.
- Save resets dirty state.
- Cancel restores the last persisted state.
- Double-click may enter edit mode where the pattern permits it.

---

## 18. Data Grid Architecture

`AppDataGrid` is the standard grid abstraction.

### Capabilities

- Typed rows and columns.
- Compact density.
- Sorting.
- Filtering.
- Pagination.
- Search.
- Column visibility.
- Selection.
- Loading state.
- Error state.
- Empty state.
- Server-side or client-side modes.
- Inline editing where explicitly allowed.
- Keyboard navigation.
- Export hooks.
- Persisted user preferences.
- Responsive minimum widths.

### Grid Rules

- Column definitions should be created outside render or memoized.
- Business formatting must be implemented through typed renderers.
- Grid API references must not leak unnecessarily into page code.
- Server-side filters and sorting must map through service adapters.
- Selection must be controlled where page actions depend on it.
- Row identifiers must be stable.
- Do not use array index as a row identifier.
- Page-level Save is preferred over row-level Save/Cancel for editable enterprise pages.
- Refresh must reload persisted data and clear uncommitted edits.

---

## 19. Forms and Fields

React Hook Form is the standard form state library.

Zod is the standard validation library.

### Standard Field Components

```text
AppTextField
AppNumberField
AppCurrencyField
AppDateField
AppDateTimeField
AppBooleanField
AppSelectField
AppEnumField
AppLookupField
AppDisplayField
AppBilingualField
AppGeneratedCodeField
```

### Field API Requirements

Every reusable field should consistently support relevant properties such as:

```ts
name
label
value
defaultValue
required
disabled
readOnly
error
helperText
placeholder
size
fullWidth
onChange
onBlur
validation
permission
```

### Form Rules

- Domain schemas stay inside modules.
- Generic validation helpers stay inside `shared`.
- Form controls must use consistent error rendering.
- Backend validation errors must map to field names.
- Form state must not be duplicated in Zustand.
- Form layout must adapt to RTL and mobile.
- Read-only and disabled states must remain visually distinct.
- Create and edit defaults must be explicit.
- Reset behavior must restore the correct persisted or default values.

---

## 20. FastTabs

FastTabs provide collapsible enterprise form sections.

### Requirements

- Controlled or uncontrolled expansion.
- Optional summary text when collapsed.
- Keyboard accessibility.
- RTL support.
- Compact spacing.
- Error indication for invalid hidden fields.
- Optional lazy mounting for expensive content.
- Persisted expansion preference when useful.

---

## 21. Dialogs

All dialogs use the shared dialog architecture.

### Standard Dialogs

```text
AppDialog
FormDialog
ConfirmationDialog
DeleteConfirmationDialog
LookupDialog
ProcessDialog
```

### Dialog Rules

- Dialog state must be explicit.
- Destructive dialogs require a clear destructive action.
- Form dialogs integrate with form validation.
- Dialog actions must be consistent.
- Escape and backdrop behavior must match operation risk.
- Long-running operations display progress.
- Dialogs must support mobile full-screen behavior where appropriate.
- Dialog components must not perform hidden API calls.

---

## 22. Lookups

Lookups provide reusable entity selection.

### Lookup Components

```text
LookupField
LookupGrid
LookupSearchBar
LookupFilterPanel
LookupValueRenderer
useLookup
```

### Lookup Rules

- Support code and display value.
- Support server-side search.
- Support keyboard selection.
- Support clear behavior.
- Support permission filtering.
- Return a typed selected value.
- Avoid coupling a generic lookup to one domain entity.
- Domain lookup adapters belong inside the owning module.

---

## 23. State Management

Use the correct tool for each state category.

| State Type | Tool |
|---|---|
| Server state | TanStack Query |
| Form state | React Hook Form |
| Global UI preferences | Zustand |
| Local component state | React state |
| URL/filter state | React Router search parameters |
| Validation | Zod |
| Authentication session | Auth provider and approved store abstraction |

### State Rules

- Do not duplicate server data in Zustand.
- Do not store form field values globally.
- Use URL state for shareable filters and tabs where appropriate.
- Keep transient dialog state local unless cross-page orchestration requires otherwise.
- Use selectors with Zustand to minimize rerenders.
- Persist only approved preferences.

---

## 24. API and Query Architecture

### Service Flow

```text
Page
→ domain hook
→ domain service
→ core API client
→ ASP.NET Core REST API
```

### Query Key Factory

Query keys must be centralized and hierarchical.

Example:

```ts
export const queryKeys = {
  all: ['api'] as const,
  entities: {
    all: ['entities'] as const,
    byEntity: (entityName: string) =>
      ['entities', entityName] as const,
    list: (entityName: string, params?: unknown) =>
      ['entities', entityName, 'list', params] as const,
    detail: (entityName: string, id: string | number) =>
      ['entities', entityName, 'detail', id] as const,
  },
};
```

### Mutation Rules

- Invalidate the narrowest valid query scope.
- Do not invalidate the entire application cache after every mutation.
- Use optimistic updates only when rollback behavior is reliable.
- Normalize backend errors.
- Prevent duplicate submissions.
- Surface success and error notifications consistently.

---

## 25. Authentication and Authorization

Authentication and authorization are separate concerns.

### Authentication Responsibilities

- Sign in.
- Sign out.
- Session restoration.
- Token refresh.
- Current-user retrieval.
- Secure token storage strategy.
- Expired-session handling.

### Authorization Responsibilities

- Route access.
- Navigation visibility.
- Action availability.
- Field visibility.
- Read-only enforcement.
- Module and resource permissions.

### Permission Model

```ts
interface PermissionRequirement {
  module: string;
  resource: string;
  action?: string;
}
```

### Security Rules

- Frontend permission checks improve UX but do not replace backend authorization.
- Hidden actions must also be rejected by the API.
- Sensitive data must not be loaded merely because it is hidden visually.
- Permission evaluation must be centralized.

---

## 26. Localization and RTL

IXApp supports English and Arabic.

### Localization Requirements

- All user-facing text uses translation keys.
- Translation resources live in `public/locales`.
- Dates, numbers, and currencies use locale-aware formatting.
- Validation messages are localizable.
- Navigation labels are localized.
- Dialog and notification text is localized.

### Arabic Mode

When Arabic is selected:

- Set `document.dir = 'rtl'`.
- Use Arabic translation resources.
- Reverse layout-sensitive icons where appropriate.
- Preserve numeric alignment.
- Preserve grid readability.
- Keep codes and technical identifiers LTR when necessary.
- Ensure dialogs, menus, navigation, and forms render correctly in RTL.

---

## 27. Theme and Visual Design

IXApp uses an original enterprise identity.

### Design Characteristics

- Compact but readable.
- Neutral page background.
- White or theme surface content areas.
- Clear hierarchy.
- Minimal decoration.
- Dense data grids.
- Compact forms.
- Structured action groups.
- Collapsible FastTabs.
- Strong selected-row state.
- Subtle borders.
- Small border radius.
- Accessible contrast.
- Responsive layout.

### Theme Tokens

All values must come from the Material UI theme:

- Palette.
- Typography.
- Spacing.
- Shape.
- Shadows.
- Breakpoints.
- Component variants.
- Component overrides.
- Density.

Do not hardcode colors inside components except for documented exceptional cases.

### Required Theme Support

```text
Light mode
Dark mode
LTR
RTL
Compact density
English
Arabic
```

### Material UI Overrides

Centralize overrides for:

```text
MuiButton
MuiIconButton
MuiTextField
MuiInputBase
MuiSelect
MuiAutocomplete
MuiDialog
MuiDrawer
MuiAppBar
MuiToolbar
MuiTabs
MuiTab
MuiAccordion
MuiMenu
MuiTooltip
MuiDataGrid
```

---

## 28. Naming Conventions

### PascalCase

Use for:

- React components.
- Component files.
- Interfaces.
- Types.
- Enums.
- Classes.

Examples:

```text
CustomerDetailsPage.tsx
CustomerForm.tsx
CustomerFormProps
PageMode
```

### camelCase

Use for:

- Variables.
- Functions.
- Methods.
- Props.
- Hooks.
- Service instances.

Examples:

```text
selectedCustomer
handleSave
useCustomerForm
loadCustomers
```

### UPPER_SNAKE_CASE

Use for:

- Constants.
- Storage keys.
- Fixed action identifiers.
- Configuration constants.

Examples:

```text
DEFAULT_PAGE_SIZE
CUSTOMER_QUERY_KEY
APP_STORAGE_KEY
```

### Component Export Rule

Every component file must export a component with the same name as the file.

```tsx
// CustomerForm.tsx
export function CustomerForm() {
  return null;
}
```

Reusable components should use named exports.

---

## 29. Path Aliases

Configure TypeScript and Vite aliases:

```text
@app/*
@core/*
@shared/*
@patterns/*
@modules/*
@assets/*
@mocks/*
@test/*
```

Example:

```tsx
import { PageContainer } from '@shared/components/page/PageContainer';
```

Avoid long relative imports.

---

## 30. Error Handling

Errors are normalized through the core error architecture.

### Error Categories

```text
Validation error
Authentication error
Authorization error
Not found
Conflict
Business rule error
Network error
Server error
Unknown error
```

### Error Rules

- API errors are mapped to `AppError`.
- Pages render shared error states.
- Forms render field-level validation where possible.
- Unexpected errors are caught by an error boundary.
- User messages must be understandable and localizable.
- Technical details are logged through an approved logging mechanism.
- Sensitive backend details must not be exposed.

---

## 31. Notifications and Feedback

Use centralized notification handling.

### Feedback States

```text
Loading
Saving
Success
Warning
Error
Empty
No results
Access denied
```

### Rules

- Avoid duplicate notifications.
- Long-running operations show progress.
- Success notifications should identify the completed operation.
- Validation errors should remain close to the affected fields.
- Empty states should offer a relevant next action when allowed.
- Access-denied states must not reveal restricted information.

---

## 32. Testing Strategy

### Unit Tests

Test:

- Utility functions.
- Validation schemas.
- Query-key factories.
- Permission helpers.
- Reducer-like state transitions.
- Service adapters.

### Component Tests

Test:

- Field behavior.
- Dialog behavior.
- Action visibility.
- Page mode transitions.
- Loading, empty, and error states.
- RTL rendering where relevant.

### Integration Tests

Test:

- List loading.
- Create flow.
- Edit flow.
- Delete confirmation.
- Refresh behavior.
- Permission restrictions.
- Query invalidation.
- Form error mapping.

### Test Rules

- Use Vitest.
- Use React Testing Library.
- Prefer behavior-based tests.
- Avoid testing implementation details.
- Mock network boundaries, not internal component behavior.
- Keep shared test utilities in `src/test`.

---

## 33. Performance Standards

- Lazy-load module routes.
- Memoize expensive column and action definitions.
- Use Zustand selectors.
- Avoid unnecessary context providers.
- Avoid recreating large `sx` objects during render.
- Use query caching intentionally.
- Virtualize large data sets through MUI Data Grid.
- Debounce search where appropriate.
- Cancel stale requests.
- Avoid oversized page components.
- Avoid broad cache invalidation.
- Measure before introducing complex optimization.

---

## 34. Accessibility

All reusable components must support:

- Keyboard navigation.
- Visible focus states.
- Accessible labels.
- Correct semantic elements.
- Dialog focus trapping.
- Screen-reader descriptions.
- Sufficient contrast.
- Logical tab order.
- Reduced-motion preferences where applicable.

Icon-only buttons require accessible labels or tooltips.

---

## 35. Responsive Design

### Desktop

- Persistent or collapsible navigation.
- Full action pane.
- Multi-column forms.
- Dense grids.
- Split views.

### Tablet

- Reduced navigation width.
- Collapsible details.
- Action overflow.
- Flexible form columns.

### Mobile

- Temporary drawer.
- Single-column forms.
- Horizontal grid scrolling.
- Full-screen dialogs when appropriate.
- Compact page padding.
- Prioritized actions.

---

## 36. Representative Initial Modules

The first implementation should include:

```text
Dashboard
Accounts Receivable
Foundation
System Administration
```

### Representative Pages

```text
DashboardPage
CustomersPage
CustomerDetailsPage
CustomerGroupsPage
SalesOrdersPage
SalesOrderPage
CurrenciesPage
ApplicationSettingsPage
```

Mock data and mock services may be used until the ASP.NET Core API is connected.

---

## 37. Architecture Guardrails

The following rules are mandatory:

- No direct Axios usage inside visual components.
- No direct module-to-module imports.
- No module business logic inside shared components.
- No hardcoded user-facing text.
- No hardcoded theme colors in components.
- No duplicated form libraries.
- No duplicated HTTP clients.
- No oversized page files.
- No uncontrolled circular dependencies.
- No unstable row identifiers.
- No empty architectural folders without responsibility.
- No broad cache invalidation without justification.
- No default exports for reusable shared components.
- No silent destructive operations.
- No frontend-only authorization assumptions.
- No long relative import chains when aliases are available.

---

## 38. Definition of Done

A feature is complete when:

- It follows the correct layer and folder.
- Dependency direction is valid.
- Routes are registered.
- Permissions are enforced.
- Text is localized.
- RTL is verified where relevant.
- Loading, empty, error, and success states exist.
- Form validation is implemented.
- API interaction is typed.
- Query invalidation is correct.
- Responsive behavior is verified.
- Accessibility requirements are met.
- Tests cover critical behavior.
- `npm run build` succeeds.
- `npm run lint` succeeds.
- `npm run test` succeeds.

---

## 39. Build and Quality Commands

The following commands must succeed:

```bash
npm run dev
npm run build
npm run lint
npm run test
```

---

## 40. Architectural Decision Process

Any change that affects one or more of the following requires an architectural decision record or explicit technical review:

- Layer boundaries.
- Dependency direction.
- State-management libraries.
- Form libraries.
- HTTP clients.
- Routing architecture.
- Authentication strategy.
- Permission model.
- Shared component APIs.
- Theme token structure.
- Localization architecture.
- Cross-module communication.
- Public module contracts.

---

## 41. Summary

IXApp is structured as a modular enterprise React frontend with strict dependency direction, reusable D365-inspired page patterns, centralized infrastructure, strongly typed domain modules, and consistent enterprise UI behavior.

The architecture favors:

```text
configuration over duplication
composition over inheritance
typed contracts over implicit behavior
module ownership over global coupling
shared patterns over repeated page implementations
centralized infrastructure over local reinvention
```

This document is the authoritative frontend architecture reference for IXApp.
