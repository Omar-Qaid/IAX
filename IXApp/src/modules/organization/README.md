# Organization module

Implements legal-entity maintenance. `LegalEntityPage.tsx` consumes `legalEntityService`, which selects `legalEntityApiRepository` or `legalEntityMockRepository`; `legalEntityTypes.ts` defines the feature contracts.

Data flow: route → `LegalEntityPage` → service → HTTP/mock repository. The page uses shared form/page/lookup infrastructure. Extend all repository implementations when changing the service contract.

[Modules](../README.md) · [Mocks](../../../docs/mocks.md) · [API and state](../../../docs/api-and-state.md)
