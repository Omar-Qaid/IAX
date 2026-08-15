# Authentication and authorization

IXApp selects one `AuthAdapter` in `core/auth/authService.ts`: the API adapter when mock mode is off and `mockAuthAdapter` when `VITE_ENABLE_MOCK_API=true`.

## API contract

- `POST /v1/Auth/login` accepts username/password and returns an `ApiResponse` containing `accessToken`.
- `GET /v1/Auth/me` returns the current user, roles, and permissions.
- `POST /v1/Auth/refresh-token` returns a replacement access token and requires the current bearer token.
- `POST /v1/Auth/logout` revokes the current token when the backend is reachable.

The auth adapter has its own Axios client to avoid recursive use of the normal token-refresh interceptor.

## Session lifecycle

The access token is held in memory and mirrored to `sessionStorage`. Legacy local-storage auth values are removed. On reload, `AuthProvider` calls `/Auth/me` when a token exists; it does not trust a stored user profile. During the final minute before JWT expiry, `ensureFreshAccessToken` coordinates one in-flight renewal. An already expired token is cleared and cannot be renewed by the current contract.

`401` clears the session and query cache through the auth event flow. `403` is an authorization failure and does not sign the user out. Logout clears local user/query state even if server revocation fails.

## Guards and permissions

`RouteGuard` protects routes, redirects anonymous users to `/login`, and preserves a safe return path. `PermissionGuard` conditionally renders a component subtree. Permission checks grant access to `SystemAdmin`, wildcard `*`, or an exact permission string; otherwise they fail closed.

Navigation and page registry entries should use constants from `core/permissions/permissions.ts`. UI guards improve usability but do not replace backend authorization.

## Mock mode

The mock adapter exposes `MOCK_USER`, accepts any submitted username, and stores a mock token. This behavior is for tests/mock mode only and must not be described as the production login contract.
