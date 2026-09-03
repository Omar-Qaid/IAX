# Report Designer pattern

`ReportDesigner` is the domain-neutral workspace shell for visual report, template, and process designers across all application modules (Workflow print templates, Finance financial report builders, Organization structure charts, etc.).

It provides a bounded surface supporting both:

1. **Slot-based composition**: Standardized `toolbar`, `sidebar` (component palette), `properties` (inspector), `footer`, and `canvas` slots.
2. **Unconstrained child layout**: Custom grid/flex row layouts managed directly by the feature module.

Feature modules retain full ownership of domain schemas, data bindings, canvas rendering, property inspectors, and persistence.
