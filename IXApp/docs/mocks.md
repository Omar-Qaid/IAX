# Mock data and adapters

Mock mode is explicit and partial. `core/configuration/environment.ts` enables it only when `VITE_ENABLE_MOCK_API=true`; `.env.test` does this for automated tests, while development and production environment files set it to false.

## Shared datasets

`src/mocks/data` currently contains:

- `currencies.ts`
- `customerGroups.ts`
- `customers.ts`
- `salesOrders.ts`

These are typed arrays consumed directly by dashboard and several Accounts Receivable pages. They are not stateful repository implementations and do not automatically simulate HTTP latency or pagination.

## Feature-owned mock adapters

- `modules/administration/adapters/settingsMockRepository.ts` implements the settings repository contract with in-memory mutable records.
- `modules/organization/adapters/legalEntityMockRepository.ts` implements the legal-entity repository contract.
- `core/auth/mockAuthAdapter.ts` supplies the test/development mock identity when mock mode is enabled.
- `shared/services/logisticsAddressMockData.ts` provides compatible geography fixtures behind the shared logistics hooks.

The corresponding services choose an adapter once at module load based on `environment.enableMockApi`. Other module APIs, especially workflow and currency setup APIs, are HTTP-only even when mock mode is enabled unless tests replace them with Vitest mocks.

## Rules

- A mock adapter must implement the same TypeScript contract and return copies when callers could mutate records.
- Keep domain mock adapters with the owning module; keep broadly reused fixture data under `src/mocks` only when architecture permits it.
- Tests should mock network boundaries deliberately; do not assume `VITE_ENABLE_MOCK_API` makes every route offline-capable.
- Never describe mock behavior as production behavior.
