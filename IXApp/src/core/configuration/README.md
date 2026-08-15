# Runtime configuration

`environment.ts` maps `VITE_API_BASE_URL` and `VITE_ENABLE_MOCK_API` into the typed `AppEnvironment`. It is the runtime source of truth for transport and mock-mode selection.

Frontend environment values are public after bundling; never add secrets.

[API and state](../../../docs/api-and-state.md) · [Frontend README](../../../README.md)
