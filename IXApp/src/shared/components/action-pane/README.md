# Action pane

Provides `ActionPane` grouping/menu/divider/button components, action contracts/definitions, and higher-level `EnterpriseCrudActions` and `EnterpriseCommandUtilities`. Actions can be filtered or disabled from permissions and record/page state.

Pages own action handlers and mutations; the pane owns consistent presentation and overflow behavior. The current implementation has an architecture-audit dependency on pattern tokens, recorded as debt.

[Detailed action-pane guide](../../../../docs/shared/action-pane.md) · [Permissions](../../../core/permissions/README.md)
