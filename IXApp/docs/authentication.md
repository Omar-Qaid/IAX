# Authentication and session contract

IXApp has separate authentication adapters for development mock mode and the production IXApi contract.

Local interactive development uses the real IXApi HTTP development endpoint at `http://localhost:33319/api`.
Automated tests use `.env.test` and the explicit mock adapter; changing test mode does not silently change interactive login behavior.

## Verified IXApi endpoints

- `POST /api/v1/Auth/login` accepts `{ username, password }` and returns an API envelope containing `accessToken`.
- `GET /api/v1/Auth/me` returns the current user, roles, and permissions.
- `POST /api/v1/Auth/refresh-token` issues a new access token but requires the existing bearer token to remain valid.
- `POST /api/v1/Auth/logout` revokes the current JWT by its `jti` until expiration.

The backend does not currently issue a refresh token or authentication cookie. IXApp therefore renews a JWT proactively during the final minute before expiry. An expired token cannot be renewed and requires a new login.

## Browser storage and security boundary

The access token is held in memory and mirrored to `sessionStorage` so a same-tab reload can restore the session. Legacy `localStorage` authentication values are removed during migration. `sessionStorage` reduces persistence but remains readable by JavaScript, so preventing script injection remains mandatory. Moving renewal to an HttpOnly, Secure, SameSite cookie requires a separate backend contract change.

User profiles are not trusted from browser storage. On production bootstrap, IXApp calls `/Auth/me` to restore the authenticated user and fresh roles and permissions.

## Request behavior

- Authenticated requests receive the current bearer token.
- Requests include `X-Company` when a company is selected.
- Near-expiry tokens are renewed through one shared in-flight refresh operation.
- `401` clears the local session and query cache, then protected routes redirect to login.
- `403` remains an authorization error and does not destroy an otherwise valid session.
- Logout clears local identity and query state even if server-side revocation cannot be reached.
