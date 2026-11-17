# Record preview hooks

`useRecordPreview(entity, key)` owns the hover/focus behaviour of one previewed value: open delay, keeping the card open while the pointer is on it, closing on leave, typing or Esc, and loading the record through TanStack Query only once the card opens. It returns props for the trigger element and the card, plus the resolved card rows. `index.ts` is the local export boundary.

[Record preview](../README.md)
