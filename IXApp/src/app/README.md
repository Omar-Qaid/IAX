# Application composition

## Overview

The app layer owns browser startup and application-wide composition. `main.tsx` mounts `App.tsx`; `AppProviders` installs global contexts; `AppRoutes` selects layouts and lazy feature pages.

| Folder | Responsibility | Documentation |
| --- | --- | --- |
| `configuration` | App constants, feature flags, and navigation definitions | [README](configuration/README.md) |
| `layouts` | Authenticated, authentication, and full-screen route shells | [README](layouts/README.md) |
| `navigation` | Navigation primitives and breadcrumb rendering | [README](navigation/README.md) |
| `providers` | Global provider composition | [README](providers/README.md) |
| `routes` | Route paths, metadata, lazy registry, guards, and route tree | [README](routes/README.md) |
| `shell` | Top bar, sidebar, drawers, menus, search, and command palette | [README](shell/README.md) |
| `store` | Zustand stores for company, navigation, and preferences | [README](store/README.md) |
| `theme` | Theme construction and Material UI overrides | [README](theme/README.md) |

`App.css` and `index.css` provide app/global CSS. Business pages do not belong here.

## Related documentation

- [Application architecture](../../docs/app.md)
- [Routing and layouts](../../docs/routing-and-layouts.md)
- [Application shell](../../docs/app-shell.md)
