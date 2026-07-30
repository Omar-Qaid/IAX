# IXApp 
https://github.com/Omar-Qaid/IAX
IXApp is a modular enterprise frontend application built with React, TypeScript, Material UI, and Vite.

The project architecture is inspired by Microsoft Dynamics 365 Finance & Operations user-interface patterns, including:

* Enterprise application shell
* Module-based navigation
* Action panes
* FastTabs
* List pages
* List-and-details pages
* Header-and-lines document pages
* Setup and parameter pages
* Workspaces and dashboards
* Reusable forms
* Reusable fields
* Reusable data grids
* Permission-based UI controls
* English and Arabic localization
* LTR and RTL layouts

IXApp is designed to connect to an ASP.NET Core REST Web API.

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Technology Stack](#technology-stack)
3. [Features](#features)
4. [Prerequisites](#prerequisites)
5. [Installation](#installation)
6. [Environment Configuration](#environment-configuration)
7. [Available Commands](#available-commands)
8. [Project Structure](#project-structure)
9. [Architecture Layers](#architecture-layers)
10. [Dependency Rules](#dependency-rules)
11. [Application Shell](#application-shell)
12. [Page Patterns](#page-patterns)
13. [Shared Components](#shared-components)
14. [Routing](#routing)
15. [Navigation](#navigation)
16. [API Integration](#api-integration)
17. [Mock API](#mock-api)
18. [Server State](#server-state)
19. [Global State](#global-state)
20. [Forms and Validation](#forms-and-validation)
21. [Data Grid](#data-grid)
22. [Action Pane](#action-pane)
23. [FastTabs](#fasttabs)
24. [Dialogs](#dialogs)
25. [Lookups](#lookups)
26. [Permissions](#permissions)
27. [Localization](#localization)
28. [Theme](#theme)
29. [Error Handling](#error-handling)
30. [Notifications](#notifications)
31. [Testing](#testing)
32. [Naming Conventions](#naming-conventions)
33. [Adding a New Module](#adding-a-new-module)
34. [Adding a Simple List Page](#adding-a-simple-list-page)
35. [Adding a List-and-Details Page](#adding-a-list-and-details-page)
36. [Adding a Document Page](#adding-a-document-page)
37. [Adding a Service](#adding-a-service)
38. [Adding a Route](#adding-a-route)
39. [Adding a Navigation Item](#adding-a-navigation-item)
40. [Adding a New Action](#adding-a-new-action)
41. [Adding a Shared Field](#adding-a-shared-field)
42. [Build and Deployment](#build-and-deployment)
43. [Development Guidelines](#development-guidelines)
44. [Current Sample Modules](#current-sample-modules)
45. [Known Limitations](#known-limitations)

---

# Project Overview

IXApp provides a reusable frontend foundation for enterprise business applications.

The project focuses on:

* Standardized page structures
* Predictable user interactions
* Strong TypeScript typing
* Reusable enterprise components
* Modular business features
* Centralized API handling
* Centralized permissions
* Centralized error handling
* Centralized notifications
* Scalable routing
* Responsive layouts
* Accessible Material UI components
* ASP.NET Core Web API integration

The project does not copy Microsoft Dynamics 365 source code or proprietary components.

Instead, it implements similar architectural concepts using open React and Material UI components.

---

# Technology Stack

IXApp uses the following technologies:

| Technology            | Responsibility                          |
| --------------------- | --------------------------------------- |
| React                 | User-interface development              |
| TypeScript            | Static typing                           |
| Vite                  | Development server and production build |
| Material UI           | UI component framework                  |
| MUI X Data Grid       | Enterprise-style tabular data           |
| React Router          | Routing and navigation                  |
| Axios                 | HTTP communication                      |
| TanStack Query        | Server-state management                 |
| React Hook Form       | Form-state management                   |
| Zod                   | Schema validation                       |
| Zustand               | Global client-state management          |
| i18next               | Localization                            |
| Vitest                | Unit testing                            |
| React Testing Library | Component testing                       |
| ESLint                | Code analysis                           |
| Prettier              | Code formatting                         |

---

# Features

IXApp includes or is designed to support:

* Responsive enterprise application shell
* Top navigation bar
* Collapsible side navigation
* Module navigation
* Global search placeholder
* Company selector
* Notification center
* User menu
* Breadcrumbs
* Route guards
* Permission guards
* D365-style action pane
* FastTabs
* Reusable forms
* Reusable form fields
* Reusable data grids
* Page-level Save, Cancel, and Refresh
* List pages
* List-and-details pages
* Master forms
* Master-detail pages
* Header-and-lines document pages
* Workspace pages
* Inquiry pages
* Setup pages
* Process and wizard pages
* Tree-and-details pages
* Profile pages
* English and Arabic localization
* LTR and RTL direction
* Light and dark themes
* Centralized API error handling
* ASP.NET Core validation-problem mapping
* Mock API mode
* Typed routing
* Typed permissions
* Typed page modes
* Typed record states
* Lazy-loaded business modules

---

# Prerequisites

Install the following tools before running the project:

* Node.js 20 or later
* npm 10 or later
* Git

Verify the installed versions:

```bash
node --version
npm --version
git --version
```

---

# Installation

Clone the repository:

```bash
git clone <repository-url>
```

Open the project folder:

```bash
cd IXApp
```

Install dependencies:

```bash
npm install
```

Create the environment files if they do not already exist:

```text
.env
.env.development
.env.production
```

Start the development server:

```bash
npm run dev
```

The application will normally be available at:

```text
http://localhost:5173
```

---

# Environment Configuration

IXApp uses Vite environment variables.

All frontend environment variables must begin with:

```text
VITE_
```

Example `.env.development`:

```env
VITE_APP_NAME=IXApp
VITE_API_BASE_URL=https://localhost:7001/api
VITE_ENABLE_MOCK_API=true
VITE_DEFAULT_LANGUAGE=en
VITE_DEFAULT_THEME=light
VITE_REQUEST_TIMEOUT=30000
```

Example `.env.production`:

```env
VITE_APP_NAME=IXApp
VITE_API_BASE_URL=https://api.example.com/api
VITE_ENABLE_MOCK_API=false
VITE_DEFAULT_LANGUAGE=en
VITE_DEFAULT_THEME=light
VITE_REQUEST_TIMEOUT=30000
```

## Supported Variables

| Variable                | Description                          |
| ----------------------- | ------------------------------------ |
| `VITE_APP_NAME`         | Application display name             |
| `VITE_API_BASE_URL`     | ASP.NET Core Web API base URL        |
| `VITE_ENABLE_MOCK_API`  | Enables or disables mock services    |
| `VITE_DEFAULT_LANGUAGE` | Default application language         |
| `VITE_DEFAULT_THEME`    | Default light or dark theme          |
| `VITE_REQUEST_TIMEOUT`  | HTTP request timeout in milliseconds |

Do not store secrets in frontend environment files.

All values included in the frontend build can be inspected by users.

---

# Available Commands

Start the development server:

```bash
npm run dev
```

Create a production build:

```bash
npm run build
```

Preview the production build locally:

```bash
npm run preview
```

Run ESLint:

```bash
npm run lint
```

Automatically fix supported lint errors:

```bash
npm run lint:fix
```

Format all supported files:

```bash
npm run format
```

Validate formatting:

```bash
npm run format:check
```

Run tests in watch mode:

```bash
npm run test
```

Run all tests once:

```bash
npm run test:run
```

Generate a test coverage report:

```bash
npm run test:coverage
```

---

# Project Structure

```text
IXApp/
├── public/
│   └── locales/
│       ├── en/
│       └── ar/
│
├── src/
│   ├── app/
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
│   │   ├── constants/
│   │   ├── errors/
│   │   ├── localization/
│   │   ├── permissions/
│   │   ├── routing/
│   │   ├── types/
│   │   └── utilities/
│   │
│   ├── shared/
│   │   ├── components/
│   │   │   ├── action-pane/
│   │   │   ├── app-shell/
│   │   │   ├── common/
│   │   │   ├── data-grid/
│   │   │   ├── dialogs/
│   │   │   ├── fast-tabs/
│   │   │   ├── feedback/
│   │   │   ├── fields/
│   │   │   ├── forms/
│   │   │   ├── lookups/
│   │   │   ├── page/
│   │   │   └── status/
│   │   ├── constants/
│   │   ├── hooks/
│   │   ├── services/
│   │   ├── types/
│   │   ├── utilities/
│   │   └── validation/
│   │
│   ├── patterns/
│   │   ├── document/
│   │   ├── inquiry/
│   │   ├── list-details/
│   │   ├── master-detail/
│   │   ├── master-form/
│   │   ├── process/
│   │   ├── profile/
│   │   ├── setup/
│   │   ├── simple-list/
│   │   ├── tree-details/
│   │   └── workspace/
│   │
│   ├── modules/
│   │   ├── accounts-receivable/
│   │   ├── dashboard/
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

---

# Architecture Layers

IXApp is divided into five primary architectural layers.

## App Layer

Location:

```text
src/app
```

Responsibilities:

* Application startup
* Global providers
* Main layouts
* Route registration
* Theme configuration
* Environment configuration
* Global application stores

The app layer composes the complete application.

---

## Modules Layer

Location:

```text
src/modules
```

Responsibilities:

* Business features
* Feature pages
* Module-specific forms
* Module-specific grids
* Feature hooks
* Feature services
* Feature validation
* Feature models
* Feature route definitions

Examples:

```text
accounts-receivable
general-ledger
inventory-management
foundation
system-administration
```

---

## Patterns Layer

Location:

```text
src/patterns
```

Responsibilities:

* Reusable page structures
* Common page controllers
* Standardized page layouts
* Cross-module page behavior

Examples:

```text
SimpleListPage
ListDetailsPage
MasterFormPage
DocumentPage
WorkspacePage
```

Page patterns must not contain business-module logic.

---

## Shared Layer

Location:

```text
src/shared
```

Responsibilities:

* Reusable UI components
* Reusable hooks
* Reusable forms
* Reusable fields
* Data-grid infrastructure
* Dialog infrastructure
* Lookup infrastructure
* Shared feedback states
* Shared validation
* Shared utilities

The shared layer may depend on the core layer.

It must not depend on business modules.

---

## Core Layer

Location:

```text
src/core
```

Responsibilities:

* API client
* Authentication
* Authorization contracts
* Error infrastructure
* Localization infrastructure
* Common types
* Framework-independent utilities
* Routing helpers
* Application constants

The core layer must remain independent of shared components, patterns, and modules.

---

# Dependency Rules

The intended dependency direction is:

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

Allowed dependencies:

```text
app → modules, patterns, shared, core
modules → patterns, shared, core
patterns → shared, core
shared → core
core → external libraries
```

Forbidden dependencies:

```text
core → shared
core → patterns
core → modules
shared → patterns
shared → modules
patterns → modules
```

Business modules should not import implementation details from other business modules.

Shared contracts or application-level orchestration should be used when modules need to interact.

---

# Application Shell

The application shell is located under:

```text
src/shared/components/app-shell
```

Main components:

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
```

The application shell provides:

* Top application bar
* Module selection
* Company selection
* Global search
* Notifications
* User options
* Side navigation
* Breadcrumbs
* Main page-content area

The shell is responsive.

On smaller screens, the permanent side navigation becomes a temporary drawer.

---

# Page Patterns

IXApp standardizes page construction through reusable page patterns.

## Simple List

Use for setup and reference-data pages.

Examples:

* Currencies
* Customer groups
* Tax codes
* Payment terms

Structure:

```text
SimpleListPage
├── PageHeader
├── ActionPane
├── FilterBar
├── DataGrid
├── SelectionSummary
├── Dialogs
└── Feedback
```

---

## List and Details

Use for pages with a record list and a details panel.

Examples:

* Customers
* Vendors
* Products
* Employees

Structure:

```text
ListDetailsPage
├── PageHeader
├── ActionPane
├── RecordList
├── DetailsPanel
├── Dialogs
└── Feedback
```

---

## Master Form

Use for setup, parameter, and configuration pages.

Examples:

* Application settings
* Accounts-receivable parameters
* General-ledger parameters

Structure:

```text
MasterFormPage
├── PageHeader
├── ActionPane
├── FastTabs
├── FormSections
└── Feedback
```

---

## Master Detail

Use when one master record controls related child records.

Examples:

* Customer and addresses
* Posting profile and ledger accounts
* Product and units

Structure:

```text
MasterDetailPage
├── PageHeader
├── ActionPane
├── MasterSection
├── DetailTabs
├── DetailGrid
└── Feedback
```

---

## Document

Use for transactional header-and-lines pages.

Examples:

* Sales order
* Purchase order
* Invoice
* Journal
* Quotation

Structure:

```text
DocumentPage
├── DocumentHeader
├── ActionPane
├── StatusBar
├── HeaderFastTabs
├── LinesGrid
├── LineDetails
├── Totals
├── Dialogs
└── Feedback
```

---

## Workspace

Use for dashboards and role-based overview pages.

Examples:

* Main dashboard
* Accounts-receivable workspace
* Inventory workspace

Structure:

```text
WorkspacePage
├── WorkspaceHeader
├── SummaryTiles
├── KPIs
├── Charts
├── WorkLists
└── QuickLinks
```

---

## Inquiry

Use for read-only analysis.

Examples:

* Customer transactions
* Inventory transactions
* Ledger inquiry

---

## Process

Use for multi-step operations.

Examples:

* Posting wizard
* Period-close process
* Data-import wizard

---

## Tree and Details

Use for hierarchical entities.

Examples:

* Organization hierarchy
* Chart of accounts
* Product categories

---

## Profile

Use for record summary and related activities.

Examples:

* Customer profile
* Vendor profile
* Employee profile

---

# Shared Components

Shared components are located under:

```text
src/shared/components
```

Main component groups:

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

Shared components must:

* Be independent of business modules
* Have focused responsibilities
* Use typed props
* Support accessibility
* Support theme configuration
* Support LTR and RTL where applicable
* Avoid direct API calls

---

# Routing

Routing is handled through React Router.

Main route files:

```text
src/app/routes/AppRoutes.tsx
src/app/routes/routeConfig.tsx
src/app/routes/routePaths.ts
src/app/routes/RouteGuard.tsx
```

Example route paths:

```text
/dashboard
/accounts-receivable/customers
/accounts-receivable/customers/:customerId
/accounts-receivable/customer-groups
/accounts-receivable/sales-orders
/accounts-receivable/sales-orders/:salesOrderId
/foundation/currencies
/system-administration/settings
/access-denied
/not-found
```

Routes should be lazy loaded.

Each route may include metadata:

```ts
export interface AppRoute {
  path: string;
  title: string;
  element: React.ReactNode;
  permission?: string;
  module?: string;
  breadcrumb?: string;
  navigation?: boolean;
}
```

---

# Navigation

The primary navigation structure is module based.

Example:

```text
Dashboard

Accounts Receivable
├── Customers
│   ├── All customers
│   └── Customer groups
├── Orders
│   ├── All sales orders
│   └── Create sales order
├── Inquiries
└── Setup

Foundation
├── Currencies
└── General setup

System Administration
├── Application settings
├── Users
└── Security
```

Navigation items should be generated from typed configuration.

Permission checks should control menu visibility.

---

# API Integration

The API client is located under:

```text
src/core/api
```

Main files:

```text
apiClient.ts
apiConfig.ts
apiError.ts
apiResponse.ts
interceptors.ts
queryKeys.ts
```

The API client is configured using:

```env
VITE_API_BASE_URL=https://localhost:7001/api
```

Example Axios client:

```ts
import axios from 'axios';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: Number(import.meta.env.VITE_REQUEST_TIMEOUT ?? 30000),
  headers: {
    'Content-Type': 'application/json',
  },
});
```

Direct Axios calls inside React visual components are not allowed.

HTTP operations must be placed in module services.

Example:

```text
src/modules/accounts-receivable/customers/services/customerService.ts
```

---

# ASP.NET Core Validation Errors

IXApp supports ASP.NET Core validation-problem responses.

```ts
export interface ApiValidationProblem {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
  traceId?: string;
}
```

Validation errors should be mapped to:

* Form-level messages
* Field-level messages
* Grid-row errors
* Global notifications

Sensitive server information must not be displayed to users.

---

# Mock API

Mock mode is controlled through:

```env
VITE_ENABLE_MOCK_API=true
```

When mock mode is enabled, module services use local mock data.

When mock mode is disabled, the same service contracts use the real ASP.NET Core API.

Example service contract:

```ts
export interface CustomerService {
  getCustomers(
    parameters: CustomerQueryParameters
  ): Promise<PagedResult<Customer>>;

  getCustomer(id: string): Promise<Customer>;

  createCustomer(
    request: CreateCustomerRequest
  ): Promise<Customer>;

  updateCustomer(
    id: string,
    request: UpdateCustomerRequest
  ): Promise<Customer>;

  deleteCustomer(id: string): Promise<void>;
}
```

Mock services should simulate:

* Network delay
* Successful responses
* Validation errors
* Not-found errors
* Create operations
* Update operations
* Delete operations

---

# Server State

TanStack Query is used for server state.

Use it for:

* Fetching
* Caching
* Query invalidation
* Mutations
* Retry
* Request cancellation
* Background refresh
* Loading state
* Error state

Example query keys:

```ts
export const customerQueryKeys = {
  all: ['customers'] as const,

  lists: () =>
    [...customerQueryKeys.all, 'list'] as const,

  list: (parameters: CustomerQueryParameters) =>
    [...customerQueryKeys.lists(), parameters] as const,

  details: () =>
    [...customerQueryKeys.all, 'detail'] as const,

  detail: (id: string) =>
    [...customerQueryKeys.details(), id] as const,
};
```

Do not copy server-query data into Zustand without a documented reason.

---

# Global State

Zustand is used only for global client state.

Suitable global state includes:

* Navigation drawer state
* Selected company
* Selected language
* Theme mode
* User preferences
* Global UI settings

Do not use Zustand for:

* Form values
* Dialog-local state
* Grid-local state
* Server-query results
* Page-specific temporary values

---

# Forms and Validation

IXApp uses:

```text
React Hook Form
Zod
Material UI
```

Form components are located under:

```text
src/shared/components/forms
```

Validation helpers are located under:

```text
src/shared/validation
```

A standard entity form should support:

* View mode
* Create mode
* Edit mode
* Read-only mode
* Dirty-state tracking
* Reset
* Save
* Cancel
* Field validation
* Form validation
* Server validation
* Conditional visibility
* Conditional read-only fields
* Permission-based visibility
* Responsive layout
* FastTabs

---

# Page Modes

IXApp uses a single typed page mode.

```ts
export type PageMode =
  | 'view'
  | 'create'
  | 'edit'
  | 'copy'
  | 'readonly'
  | 'process';
```

Do not create multiple conflicting flags such as:

```ts
isEditing
isCreating
isReadOnly
```

when one page-mode value can represent the state.

Page mode controls:

* Field editability
* Available actions
* Grid editability
* Validation
* Save behavior
* Cancel behavior
* Navigation protection

---

# Record States

Editable entities and grid rows use the following record states:

```ts
export type RecordState =
  | 'unchanged'
  | 'added'
  | 'modified'
  | 'deleted'
  | 'saving'
  | 'saved'
  | 'error';
```

These states are used for:

* Forms
* Grid rows
* Document lines
* Batch operations

---

# Data Grid

The standard grid component is:

```text
AppDataGrid
```

Location:

```text
src/shared/components/data-grid
```

Supported capabilities include:

* Typed rows
* Typed columns
* Compact density
* Pagination
* Sorting
* Filtering
* Search
* Column visibility
* Column resizing
* Column ordering
* Row selection
* Multiple selection
* Double-click editing
* Temporary rows
* Page-level Save
* Loading states
* Error states
* Empty states
* Enum formatting
* Date formatting
* Currency formatting
* Boolean formatting
* Status indicators
* Export
* Responsive behavior

Use reusable column factories:

```text
createTextColumn
createNumberColumn
createCurrencyColumn
createDateColumn
createBooleanColumn
createEnumColumn
createStatusColumn
createLookupColumn
createActionColumn
```

---

# Grid Editing

Editable grids follow these rules:

* Double-click enters edit mode.
* New records are created as temporary client-side rows.
* Modified rows use `RecordState`.
* Unsaved deleted rows are removed locally.
* Persisted deleted rows are marked for deletion.
* Save persists all valid page changes.
* Cancel discards unsaved changes.
* Refresh reloads data.
* Validation errors identify affected rows and fields.

Do not add Save and Cancel buttons to every row unless the page has a specific business requirement.

---

# Action Pane

The action pane is located under:

```text
src/shared/components/action-pane
```

Main components:

```text
ActionPane
ActionPaneGroup
ActionPaneButton
ActionPaneMenu
ActionPaneDivider
```

Typical action groups:

```text
New
Maintain
Process
Inquiries
Print
Options
```

Example:

```tsx
<ActionPane>
  <ActionPaneGroup label="Maintain">
    <ActionPaneButton
      actionId="edit"
      label="Edit"
      icon={<EditOutlinedIcon />}
      onClick={handleEdit}
      disabled={!selectedCustomer}
    />

    <ActionPaneButton
      actionId="refresh"
      label="Refresh"
      icon={<RefreshOutlinedIcon />}
      onClick={handleRefresh}
    />
  </ActionPaneGroup>
</ActionPane>
```

Each action may define:

```ts
export interface PageAction {
  id: string;
  label: string;
  group?: string;
  order?: number;
  hidden?: boolean;
  disabled?: boolean;
  loading?: boolean;
  permission?: string;
  requiresSelection?: boolean;
  allowedPageModes?: PageMode[];
  tooltip?: string;
  keyboardShortcut?: string;
  onClick: () => void | Promise<void>;
}
```

Shared action-pane components must not contain business-specific action behavior.

---

# FastTabs

FastTabs are collapsible enterprise form sections.

Location:

```text
src/shared/components/fast-tabs
```

Example:

```tsx
<FastTab
  id="general"
  title="General"
  summary={customer.name}
  hasError={hasGeneralErrors}
  defaultExpanded
>
  <CustomerGeneralFields />
</FastTab>
```

FastTabs support:

* Expand and collapse
* Default expanded state
* Summary text
* Error indicator
* Required-field indicator
* Lazy content
* Keyboard navigation
* Permission visibility
* Remembered state

---

# Dialogs

Shared dialogs are located under:

```text
src/shared/components/dialogs
```

Available dialog types:

```text
AppDialog
FormDialog
ConfirmationDialog
DeleteConfirmationDialog
LookupDialog
ProcessDialog
```

All dialogs should use consistent:

* Title structure
* Content spacing
* Action buttons
* Validation
* Loading behavior
* Closing behavior
* Escape-key handling
* Focus management
* Unsaved-change protection
* Accessibility

---

# Lookups

Reusable lookup controls are located under:

```text
src/shared/components/lookups
```

Lookup capabilities:

* Stored value
* Display value
* Search
* Filters
* Server loading
* Grid results
* Single selection
* Multiple selection
* Keyboard navigation
* Clear selection
* Loading state
* Error state

Example lookup scenarios:

* Customer group
* Customer
* Currency
* Item
* Warehouse
* Payment terms

---

# Permissions

Permission infrastructure is located under:

```text
src/core/permissions
```

Main utilities:

```text
PermissionGuard
RouteGuard
usePermission
permissionService
```

Example permissions:

```text
dashboard.view

customer.view
customer.create
customer.update
customer.delete

customerGroup.view
customerGroup.manage

salesOrder.view
salesOrder.create
salesOrder.update
salesOrder.confirm
salesOrder.post

currency.view
currency.manage

settings.view
settings.update
```

Example:

```tsx
<PermissionGuard permission="customer.update">
  <ActionPaneButton
    actionId="edit"
    label="Edit"
    onClick={handleEdit}
  />
</PermissionGuard>
```

Frontend permissions are for user experience only.

ASP.NET Core must continue enforcing authorization on the server.

---

# Localization

Localization is implemented using i18next.

Translation files:

```text
public/locales/en/translation.json
public/locales/ar/translation.json
```

Supported languages:

```text
English
Arabic
```

When Arabic is selected:

* The document direction changes to RTL.
* Material UI uses RTL direction.
* Navigation and layout alignment change.
* Translations are loaded from the Arabic resource file.
* Direction-sensitive icons should be mirrored where appropriate.

Do not concatenate translated sentence fragments.

Use complete translation keys.

Example:

```tsx
const { t } = useAppTranslation();

<PageTitle>{t('customers.title')}</PageTitle>
```

---

# Theme

Theme configuration is located under:

```text
src/app/theme
```

Main theme files:

```text
createAppTheme.ts
componentOverrides.ts
palette.ts
typography.ts
spacing.ts
shadows.ts
types.ts
```

The theme supports:

* Light mode
* Dark mode
* LTR
* RTL
* Compact enterprise density
* Centralized Material UI overrides

Do not hardcode colors in page components.

Use theme values:

```tsx
sx={{
  color: 'text.primary',
  backgroundColor: 'background.paper',
  borderColor: 'divider',
}}
```

Material UI overrides should be centralized for:

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

# Error Handling

Error infrastructure is located under:

```text
src/core/errors
```

Main files:

```text
AppError.ts
ErrorBoundary.tsx
errorMapper.ts
errorMessages.ts
```

The application should distinguish:

* Network errors
* Validation errors
* Authentication errors
* Authorization errors
* Not-found errors
* Conflict errors
* Unexpected server errors

Error messages should be useful but must not expose sensitive server details.

Where available, show:

* User-friendly message
* Validation details
* HTTP status
* Trace ID
* Retry action

---

# Notifications

IXApp uses one global notification system.

Supported notification types:

```text
Success
Information
Warning
Error
Persistent
Actionable
```

Example:

```ts
notify.success('Customer saved successfully.');

notify.error('Unable to save the customer.');

notify.warning('You have unsaved changes.');
```

Do not create unrelated Snackbar implementations inside business pages.

---

# Unsaved Changes

The shared unsaved-changes system warns users before:

* Leaving a dirty route
* Refreshing a dirty page
* Changing the selected master record
* Closing a dirty dialog
* Cancelling edits
* Closing the browser tab where supported

The warning must not appear when the page is not dirty.

---

# Refresh Behavior

The standard Refresh action should:

* Reload server data
* Clear stale errors
* Reset edit mode
* Clear temporary rows after confirmation
* Clear dirty state after confirmation
* Avoid duplicate requests
* Preserve filters when appropriate
* Preserve selection only when the record still exists

Use the shared:

```text
usePageRefresh
```

hook where applicable.

---

# Save Behavior

The standard page-level Save action should:

* Validate forms
* Validate edited rows
* Prevent duplicate submissions
* Create added records
* Update modified records
* Delete records marked for deletion
* Map backend validation errors
* Refresh saved data
* Reset dirty state
* Preserve current page context
* Show a success notification

Failed operations must not be silently ignored.

---

# Testing

Testing tools:

```text
Vitest
React Testing Library
User Event
```

Test files may be placed beside their components or under:

```text
src/test
```

Recommended tests:

* Application-shell rendering
* Navigation rendering
* Route guards
* Permission guards
* Action pane
* FastTabs
* Confirmation dialogs
* Form validation
* Data-grid behavior
* Save behavior
* Refresh behavior
* Unsaved-change warnings
* Sales-order calculations

Run tests:

```bash
npm run test
```

Run tests once:

```bash
npm run test:run
```

Generate coverage:

```bash
npm run test:coverage
```

Tests should verify user-visible behavior rather than internal implementation details.

---

# Naming Conventions

## PascalCase

Use for:

* React components
* Component files
* Interfaces
* Types
* Enums
* Classes

Examples:

```text
CustomerForm.tsx
CustomerDetailsPage.tsx
CustomerFormProps
PageMode
```

## camelCase

Use for:

* Variables
* Functions
* Hooks
* Methods
* Props
* Service instances

Examples:

```text
selectedCustomer
handleSave
useCustomerForm
customerService
```

## UPPER_SNAKE_CASE

Use for:

* Constants
* Storage keys
* Fixed identifiers
* Configuration constants

Examples:

```text
DEFAULT_PAGE_SIZE
CUSTOMER_QUERY_KEY
APP_STORAGE_KEY
```

The exported React component name must match the file name.

Example:

```text
CustomerForm.tsx
```

```tsx
export function CustomerForm() {
  return null;
}
```

Reusable components should use named exports.

---

# Path Aliases

Configured aliases:

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

Avoid deeply nested relative imports.

---

# Adding a New Module

Example module:

```text
src/modules/inventory-management
```

Recommended structure:

```text
inventory-management/
├── items/
│   ├── components/
│   ├── hooks/
│   ├── pages/
│   ├── services/
│   ├── validation/
│   ├── types/
│   ├── constants/
│   └── index.ts
├── routes/
│   └── inventoryManagementRoutes.tsx
└── index.ts
```

Steps:

1. Create the module folder.
2. Create one folder per business feature.
3. Define feature types.
4. Define service contracts.
5. Create feature hooks.
6. Create feature components.
7. Create feature pages.
8. Add module routes.
9. Add navigation configuration.
10. Add permissions.
11. Add translations.
12. Add tests.

Modules should not export internal implementation files unnecessarily.

---

# Adding a Simple List Page

Example page:

```text
PaymentTermsPage
```

Recommended feature structure:

```text
payment-terms/
├── components/
│   └── PaymentTermsGrid.tsx
├── hooks/
│   └── usePaymentTerms.ts
├── pages/
│   └── PaymentTermsPage.tsx
├── services/
│   └── paymentTermsService.ts
├── validation/
│   └── paymentTermSchema.ts
├── types/
│   └── paymentTerm.ts
└── index.ts
```

Example page composition:

```tsx
export function PaymentTermsPage() {
  return (
    <SimpleListPage
      header={
        <PageHeader
          title="Payment terms"
          subtitle="Manage customer payment terms."
        />
      }
      actionPane={<PaymentTermsActionPane />}
      content={<PaymentTermsGrid />}
    />
  );
}
```

The page should not directly call Axios.

Data access belongs in the feature service and TanStack Query hooks.

---

# Adding a List-and-Details Page

Example:

```text
VendorsPage
```

Recommended page composition:

```tsx
export function VendorsPage() {
  return (
    <ListDetailsPage
      header={<PageHeader title="Vendors" />}
      actionPane={<VendorActionPane />}
      list={<VendorGrid />}
      details={<VendorDetails />}
    />
  );
}
```

The page controller should manage:

* Selected record
* Page mode
* Dirty state
* Save
* Cancel
* Refresh
* Permission-dependent actions

Large business forms should be split into focused section components.

---

# Adding a Document Page

Example:

```text
PurchaseOrderPage
```

Recommended feature structure:

```text
purchase-orders/
├── components/
│   ├── PurchaseOrderHeader.tsx
│   ├── PurchaseOrderLines.tsx
│   ├── PurchaseOrderTotals.tsx
│   └── PurchaseOrderActionPane.tsx
├── hooks/
│   └── usePurchaseOrder.ts
├── pages/
│   ├── PurchaseOrdersPage.tsx
│   └── PurchaseOrderPage.tsx
├── services/
│   └── purchaseOrderService.ts
├── validation/
│   └── purchaseOrderSchema.ts
├── types/
│   └── purchaseOrder.ts
└── index.ts
```

Example:

```tsx
export function PurchaseOrderPage() {
  return (
    <DocumentPage
      header={<PurchaseOrderHeader />}
      actionPane={<PurchaseOrderActionPane />}
      lines={<PurchaseOrderLines />}
      totals={<PurchaseOrderTotals />}
    />
  );
}
```

The document controller should manage:

* Header state
* Line state
* Document status
* Page mode
* Dirty state
* Calculated totals
* Validation
* Save
* Refresh
* Process actions

---

# Adding a Service

Create a service under the relevant feature.

Example:

```text
src/modules/accounts-receivable/customers/services/customerService.ts
```

```ts
import { apiClient } from '@core/api/apiClient';

import type {
  CreateCustomerRequest,
  Customer,
  CustomerQueryParameters,
  UpdateCustomerRequest,
} from '../types/customer';

import type { PagedResult } from '@core/types/pagination';

export const customerService = {
  async getCustomers(
    parameters: CustomerQueryParameters
  ): Promise<PagedResult<Customer>> {
    const response = await apiClient.get<PagedResult<Customer>>(
      '/customers',
      { params: parameters }
    );

    return response.data;
  },

  async getCustomer(id: string): Promise<Customer> {
    const response = await apiClient.get<Customer>(
      `/customers/${id}`
    );

    return response.data;
  },

  async createCustomer(
    request: CreateCustomerRequest
  ): Promise<Customer> {
    const response = await apiClient.post<Customer>(
      '/customers',
      request
    );

    return response.data;
  },

  async updateCustomer(
    id: string,
    request: UpdateCustomerRequest
  ): Promise<Customer> {
    const response = await apiClient.put<Customer>(
      `/customers/${id}`,
      request
    );

    return response.data;
  },

  async deleteCustomer(id: string): Promise<void> {
    await apiClient.delete(`/customers/${id}`);
  },
};
```

Do not place UI logic in service files.

---

# Adding a Route

Create or update a module route configuration.

Example:

```tsx
import { lazy } from 'react';

const CustomersPage = lazy(async () => {
  const module = await import('../customers/pages/CustomersPage');

  return {
    default: module.CustomersPage,
  };
});

export const accountsReceivableRoutes = [
  {
    path: '/accounts-receivable/customers',
    element: <CustomersPage />,
    title: 'Customers',
    permission: 'customer.view',
    module: 'accountsReceivable',
    navigation: true,
  },
];
```

Register the module route collection in the main application route configuration.

---

# Adding a Navigation Item

Navigation should be defined through typed configuration.

Example:

```ts
export const accountsReceivableNavigation = {
  id: 'accountsReceivable',
  label: 'Accounts Receivable',
  groups: [
    {
      id: 'customers',
      label: 'Customers',
      items: [
        {
          id: 'allCustomers',
          label: 'All customers',
          path: '/accounts-receivable/customers',
          permission: 'customer.view',
        },
      ],
    },
  ],
};
```

Do not duplicate route paths as raw strings throughout the application.

Use centralized route-path constants where practical.

---

# Adding a New Action

Define the action in the page or feature controller.

Example:

```ts
const editAction: PageAction = {
  id: 'edit',
  label: 'Edit',
  group: 'maintain',
  order: 10,
  permission: 'customer.update',
  requiresSelection: true,
  allowedPageModes: ['view'],
  onClick: handleEdit,
};
```

Render the action through the shared ActionPane.

Do not put customer-specific behavior inside `ActionPaneButton`.

---

# Adding a Shared Field

Create a shared field only when its behavior is reusable across multiple modules.

Example:

```text
src/shared/components/fields/AppPercentageField.tsx
```

The field should:

* Integrate with React Hook Form
* Support read-only mode
* Support errors
* Support helper text
* Use theme spacing
* Support accessibility
* Avoid module-specific validation

Module-specific fields should remain inside the relevant module.

---

# Build and Deployment

Create a production build:

```bash
npm run build
```

The output is generated under:

```text
dist/
```

Preview the build locally:

```bash
npm run preview
```

For deployment:

1. Set the production API URL.
2. Disable mock API mode.
3. Run tests.
4. Run linting.
5. Create the production build.
6. Deploy the `dist` folder to the target web server.
7. Configure SPA route fallback to `index.html`.

Example production variables:

```env
VITE_API_BASE_URL=https://api.example.com/api
VITE_ENABLE_MOCK_API=false
```

---

# Development Guidelines

All contributors should follow these rules:

* Preserve the architectural dependency direction.
* Keep components focused.
* Avoid oversized page components.
* Use typed interfaces.
* Avoid `any`.
* Use `unknown` with type guards when needed.
* Place server calls in services.
* Use TanStack Query for server state.
* Use Zustand only for global client state.
* Use React Hook Form and Zod for forms.
* Use shared feedback components.
* Use one global notification system.
* Use shared dialogs.
* Use page-level Save and Refresh behavior.
* Use permissions consistently.
* Add translations for user-visible text.
* Use theme tokens instead of hardcoded colors.
* Add tests for important behavior.
* Remove unused files and imports.
* Do not add competing libraries without architectural approval.

---

# Current Sample Modules

## Dashboard

Page:

```text
DashboardPage
```

Pattern:

```text
Workspace
```

Sample content:

* Total customers
* Open sales orders
* Monthly sales
* Overdue balances
* Recent customers
* Recent sales orders
* Quick links
* Chart placeholder

---

## Accounts Receivable

Features:

```text
Customers
Customer groups
Sales orders
```

Pages:

```text
CustomersPage
CustomerDetailsPage
CustomerGroupsPage
SalesOrdersPage
SalesOrderPage
```

Patterns:

```text
List and Details
Simple List
Document
```

---

## Foundation

Features:

```text
Currencies
```

Page:

```text
CurrenciesPage
```

Pattern:

```text
Simple List
```

---

## System Administration

Features:

```text
Application settings
```

Page:

```text
ApplicationSettingsPage
```

Pattern:

```text
Master Form
```

---

# Known Limitations

The initial project foundation may include the following limitations:

* Authentication may be implemented as a placeholder.
* Permission data may use mock user permissions.
* Company selection may use local sample data.
* Global search may initially be a UI placeholder.
* Notifications may initially use local application events.
* Attachments may not be connected to a backend endpoint.
* Document process actions may initially use mock behavior.
* Saved grid views may initially use local storage.
* Advanced MUI X paid features are not used without a license.
* Backend API payloads must be aligned when the ASP.NET Core integration is connected.

These limitations should be replaced incrementally as backend capabilities become available.

---

# License

Add the appropriate project license here.

Example:

```text
Copyright © IXApp.
All rights reserved.
```

---

# Support

For project issues, feature requests, or architecture questions, use the project repository issue tracker or contact the project development team.
