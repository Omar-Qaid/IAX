# Process Builder hooks

`useProcessBuilderDraft.ts` coordinates draft loading, editing integration, and save behavior for the designer.

`useProcessBuilderActions.ts` owns workflow persistence, store updates, notifications, and navigation after saving. Each action uses the shared `useAsyncAction` hook for loading state, error handling, and duplicate submission prevention. Keep workflow mappings here and reusable execution behavior in shared hooks.

Section saves share the typed `useProcessBuilderSectionSave` adapter: `persist` returns the API result, `apply` reconciles it into the store, and `message` selects the existing notification pair. This adapter stays private to the workflow module because its document, store, and message contract are domain-specific. Full-document saving retains its draft-cleanup and navigation flow.

The page defines each workspace's stable ID, label, icon, and content together. Preserve their order because saved navigation uses the numeric tab index. `ProcessBuilderNavigationPanel` owns the tree/palette UI and subscribes only to the navigation state it needs.

`useProcessBuilderLoader.ts` owns document initialization, draft recovery, code metadata, and request cleanup. Its lifecycle follows the builder ID; translation changes only affect error messages. Section metadata settles independently of process loading and preview failures.

[Process Builder module](../README.md)
