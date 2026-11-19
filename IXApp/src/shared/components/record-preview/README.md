# Record preview subsystem

D365-style hover/focus cards for master data in grids and forms. Each entity is defined once and every field, lookup or column that references it shows the same card.

## Structure

| File                       | Responsibility                                                                                              |
| -------------------------- | ----------------------------------------------------------------------------------------------------------- |
| `RecordPreview.tsx`        | Public wrapper: resolves the entity (`entity` or `masterRoute`) and renders the card in a popper            |
| `RecordPreviewCard.tsx`    | Presentational D365 card, also used by grid lookups for their option preview                                |
| `hooks/`                   | `useRecordPreview`: hover/focus timing, keyboard behaviour, TanStack Query load ([README](hooks/README.md)) |
| `recordPreviewRegistry.ts` | `registerRecordPreview` and entity resolution by name or master route                                       |
| `recordPreviewLoaders.ts`  | `pagedRecordLoader` (any BaseController `/paged` endpoint), `listRecordLoader` (cached lists)               |
| `types.ts`, `constants.ts` | Contracts, delays, cache time, query keys                                                                   |
| `index.ts`                 | Export boundary                                                                                             |

Modules register their entities in `previews/<module>RecordPreviews.ts`, imported once from `app/configuration/recordPreviews.ts`. Components accept data and callbacks; loaders and field lists stay in the owning module.

[Complete record preview guide](../../../../docs/shared/record-preview.md) · [Shared components](../README.md)
