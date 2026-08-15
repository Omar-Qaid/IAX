# API infrastructure

`apiClient.ts` creates the Axios client from `apiConfig.ts`; `interceptors.ts` attaches session credentials and maps response failures. `apiError.ts` and `apiResponse.ts` define transport contracts. `queryClient.ts` configures shared TanStack Query defaults.

Feature endpoints belong in their module API folder. Pages should consume module hooks/services instead of calling this client directly.

[API and state](../../../docs/api-and-state.md) · [Error infrastructure](../errors/README.md)
