# Shared services

- `notificationStore.ts`: shared notification queue.
- `logisticsAddressMockData.ts`: current in-memory logistics records.

These services are client-side/shared infrastructure, not business API repositories.

A `notificationService` imperative wrapper, a `preferenceService` and a `storageService` browser
storage wrapper are **not** provided: those files existed as empty placeholders and were removed.
Use `notificationStore` directly, and `core` for storage keys and access.

[API and state](../../../docs/api-and-state.md) · [Shared hooks](../hooks/README.md)
