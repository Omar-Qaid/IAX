# IXApp

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
git clone https://github.com/Omar-Qaid/IAX.git
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

Location: `src/app`

Responsibilities:
* Application startup
* Global providers
* Main layouts
* Route registration
* Theme configuration
* Environment configuration
* Global application stores

---

## Modules Layer

Location: `src/modules`

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
* `accounts-receivable`
* `general-ledger`
* `inventory-management`
* `foundation`
* `system-administration`

---

## Patterns Layer

Location: `src/patterns`

Responsibilities:
* Reusable page structures
* Common page controllers
* Standardized page layouts
* Cross-module page behavior

Examples:
* `SimpleListPage`
* `ListDetailsPage`
* `MasterFormPage`
* `DocumentPage`
* `WorkspacePage`

Page patterns must not contain business-module logic.

---

## Shared Layer

Location: `src/shared`

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

The shared layer may depend on the core layer. It must not depend on business modules.

---

## Core Layer

Location: `src/core`

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
app -> modules -> patterns -> shared -> core
```

Allowed dependencies:
* `app -> modules, patterns, shared, core`
* `modules -> patterns, shared, core`
* `patterns -> shared, core`
* `shared -> core`
* `core -> external libraries`

Forbidden dependencies:
* `core -> shared, patterns, modules`
* `shared -> patterns, modules`
* `patterns -> modules`
* Business module -> another business module directly

---

# Current Sample Modules

* **Dashboard**: Operations overview with KPI tiles and quick links.
* **Accounts Receivable**: Customers, Customer Groups, Sales Orders (Header & Lines).
* **Foundation**: Currencies & Exchange rates.
* **System Administration**: Application Parameters & UI Settings.

---

# License

Copyright © IXApp. All rights reserved.
