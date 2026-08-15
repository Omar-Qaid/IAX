# Identity module

Identity currently exposes `pages/LoginPage.tsx` through `index.ts`. The page consumes `useAuth` and is hosted by the authentication layout; session, token, and adapter behavior is owned by `core/auth`.

Keep identity pages focused on user interaction and delegate authentication behavior to the auth service/context.

[Authentication](../../../docs/authentication.md) · [Auth infrastructure](../../core/auth/README.md)
