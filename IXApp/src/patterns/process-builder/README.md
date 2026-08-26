# Generic Process Builder pattern

Contains a reusable presentation-level `ProcessBuilderPage`, `ProcessBuilderTree`, hook, types, and export boundary. It models generic tree/selection behavior and does not own Workflow backend endpoints.

The Workflow-owned business designer is retained at `src/modules/process-builder`. Keep its domain API/state there and use this generic presentation pattern only when its contracts fit.

[Integrated Process Builder](../../modules/process-builder/README.md) · [Process Builder guide](../../../docs/process-builder.md)
