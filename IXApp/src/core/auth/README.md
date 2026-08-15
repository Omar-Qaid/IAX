# Authentication and authorization

Implements API/mock authentication adapters, session storage, JWT helpers, refresh scheduling, auth events, `AuthProvider`/`AuthContext`, `useAuth`, `usePermissions`, and `PermissionGuard`. Bootstrap restores a token and requests `/Auth/me`; the API interceptor handles unauthorized responses.

Route access is composed by `app/routes/RouteGuard.tsx`. Do not store credentials in a Zustand app store.

[Authentication guide](../../../docs/authentication.md) · [Permissions](../permissions/README.md)
