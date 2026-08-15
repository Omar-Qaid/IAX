# Application routes

## Responsibilities

- `pageRegistry.ts`: lazy feature-page imports.
- `routeConfig.tsx`: layout and route composition.
- `routePaths.ts`: path constants.
- `routeMetadata.ts`: titles, breadcrumbs, and related metadata.
- `RouteGuard.tsx`: authenticated and permission-protected access.
- `AppAccessDeniedState.tsx`: route-specific denial presentation.
- `AppRoutes.tsx`: router-facing root component.

Add a page import to the registry, reuse a path constant, add metadata, and place it under the correct layout. Do not embed route registration inside a module page.

[Routing and layouts](../../../docs/routing-and-layouts.md) · [Authentication](../../../docs/authentication.md)
