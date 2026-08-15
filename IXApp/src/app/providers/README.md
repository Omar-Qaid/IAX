# Application providers

`AppProviders.tsx` composes the runtime providers in this order: error boundary, localization, TanStack Query, authentication, theme, and notifications. The folder also defines the localization, query, theme, and notification provider components; authentication is implemented in `core/auth`.

Preserve ordering when a provider consumes an earlier context. New global providers require application-wide ownership; feature providers should remain with their feature.

[Application bootstrap](../../../docs/app.md) · [API and state](../../../docs/api-and-state.md)
