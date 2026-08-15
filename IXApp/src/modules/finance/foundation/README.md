# Finance foundation

Owns currency and exchange-rate setup.

- `pages/CurrencyPage.tsx`: currency maintenance backed by `api/currencyApi.ts`.
- `pages/ExchangeRateTypePage.tsx`: exchange-rate type maintenance backed by `api/exchangeRateTypeApi.ts`.
- `pages/ExchangeRatePage.tsx`: exchange-rate page using the foundation contracts.
- `queries/currencyQueryKeys.ts`: feature-owned query keys.

Pages use shared list/form infrastructure and TanStack Query. Keep endpoint wrappers in `api`, stable server-state identities in `queries`, and business presentation in `pages`.

[Modules](../../README.md) · [API and state](../../../../docs/api-and-state.md) · [Simple-list pattern](../../../patterns/simple-list/README.md)
