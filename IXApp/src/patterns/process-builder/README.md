# Generic Process Builder pattern

Contains a reusable presentation-level `ProcessBuilderPage`, `ProcessBuilderTree`, hook, types, and export boundary. It models generic tree/selection behavior and does not own Workflow backend endpoints.

The business-integrated designer is `src/modules/process-builder`. Keep domain API/state there and use this pattern only when its generic contracts fit.

[Integrated Process Builder](../../modules/process-builder/README.md) · [Process Builder guide](../../../docs/process-builder.md)
