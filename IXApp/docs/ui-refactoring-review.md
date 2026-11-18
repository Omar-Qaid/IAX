# UI review before refactoring

Reviewed 2026-11-18 against the current checkout. This is the Phase 1 source review, completed before application edits.

## Architecture and current behavior

The application has 90 TSX files in page directories. Several are compatibility wrappers or report-viewer components, so this count is not a route count. Routing is lazy-loaded through [pageRegistry.ts](../src/app/routes/pageRegistry.ts) and [routeConfig.tsx](../src/app/routes/routeConfig.tsx). RouteGuard handles authentication and route permissions. Action permissions are also applied by page configurations and shared command components. Absence of an inline permission constant does not mean a route is public.

Installed versions inspected locally are React 19.2.8, MUI 9.2.0, TypeScript 6.0.3, Vite 8.1.5 and Playwright 1.62.1. [package.json](../package.json) declares compatible ranges. React Query owns server state; Zustand owns company/preferences/UI stores; forms use local state, configured validators or React Hook Form/Zod depending on the feature. Axios infrastructure provides authentication/company context.

The enforced layers are app → modules/patterns/shared/core, modules → their own domain/patterns/shared/core, patterns → patterns/shared/core, shared → shared/core. See [ARCHITECTURE-BOUNDARIES.md](../docs/ARCHITECTURE-BOUNDARIES.md) and [development.md](../docs/development.md). Workflow owns the Process Builder package.

ThemeProvider constructs a theme for mode, language direction, font, density, contrast and primary color. Emotion supplies RTL styling. Component overrides define compact text fields, dialogs, menus and accordions. Shared enterpriseUiTokens and uiDensity overlap in measurements, while several page patterns override these defaults with literal colors and pixel sizes. See [ThemeProvider.tsx](../src/app/providers/ThemeProvider.tsx), [componentOverrides.ts](../src/app/theme/componentOverrides.ts), [enterpriseUiTokens.ts](../src/shared/constants/enterpriseUiTokens.ts).

## Existing reusable UI

- SimpleListPage already supplies translated labels, utilities, selection, filtering, permissions, confirmation, unsaved-edit handling and nested configuration defaults. It supports controlled, remote and static data. Pages still repeat many identical default labels.
- ListDetailsPage and useListDetailsPage own record selection, draft lifecycle, validation presentation and configured CRUD. ListDetailsLayout renders section fields and list/navigation layout. MasterDetailPage already composes this layout. Tree, list-grid and balance patterns build on these foundations.
- FastTab owns a second accordion implementation. MasterDetailSection and ListDetailsLayout duplicate its surface/header/content structure and controlled/uncontrolled expansion logic with differing spacing and literal light colors.
- SetupPage, document drawers, dialog/drawer fast tabs, WorkspacePage and ReportViewer have distinct purposes. Process Builder and report design are specialized workspaces.
- Feature compositions already exist for workflow setup, customer payment setup and price/discount setup. The HCM reference grids have the same four fields, API shape, validation, notification and refresh behavior but repeat their page implementation.

## Prioritized implementation plan

1. Extend the existing FastTab with explicit enterprise appearance, controlled expansion, rich summary and local style slots. Centralize enterprise accordion presentation in theme overrides. Compose ListDetailsLayout and MasterDetailSection from it while preserving field rendering, summary content, legal-entity appearance and expansion semantics.
2. Adopt the existing SimpleListPage defaults where pages repeat the exact same labels. Preserve custom labels, search fields, filters, callbacks, permissions, storage keys and source keys. This lets future pattern changes propagate to callers.
3. Consolidate HcmDepartmentPage, HcmNationalityPage and HcmOccupationPage into a feature-owned HCM reference composition over SimpleListPage. Keep the three route wrappers, API adapters, validation and identities explicit.
4. Correct PageSection style composition so callback/array sx overrides remain local. Validate section isolation, controlled expansion and theme behavior, followed by representative page tests and browser checks.
5. Leave larger business workspace decomposition for a separate scoped pass where required by evidence. In particular, invoice posting, dynamic workflow forms and inventory journals need focused behavioral coverage before structural rewrites.

## Verification limits and observations

This review establishes source wiring and UI ownership. It does not establish live server correctness, database contents, production permissions or visual equivalence on every route. The dashboard uses mock data; ExchangeRatePage owns local rate state; the generic workflow report has local generated results. Those behaviors are recorded without changing API contracts. Some screens have hard-coded English strings. Global cache scoping and business validation are separate from the UI refactor.

Each entry below records the page purpose, pattern/delegates, source imports and local state/validation/permission evidence. Related files are linked to make the wiring reviewable. Runtime verification is reported separately after implementation.

## Page-by-page source summary

### ApplicationSettingsPage

[ApplicationSettingsPage.tsx](../src/modules/administration/pages/ApplicationSettingsPage.tsx) — Global application settings and user preferences. Setup navigation, schema validation, permission-gated global editing.
Pattern setup. Local state and behavior evidence delegated to imported composition.
Permission evidence SETTINGS_UPDATE.
Related implementation [SetupPage.tsx](../src/patterns/setup/SetupPage.tsx), [types.ts](../src/patterns/setup/types.ts), [ErrorState.tsx](../src/shared/components/feedback/ErrorState.tsx), [LoadingState.tsx](../src/shared/components/feedback/LoadingState.tsx), [settingsQueries.ts](../src/modules/administration/queries/settingsQueries.ts).

### BatchJobActivePeriodPage

[BatchJobActivePeriodPage.tsx](../src/modules/administration/pages/BatchJobActivePeriodPage.tsx) — Editable batch scheduling windows with required identifiers and local row validation.
Pattern simple-list. Local state and behavior evidence delegated to imported composition.
Permission evidence BACKGROUND_JOB_CREATE, BACKGROUND_JOB_DELETE, BACKGROUND_JOB_EDIT.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [queryClient.ts](../src/core/api/queryClient.ts), [useNotifications.ts](../src/shared/hooks/useNotifications.ts), [batchJobActivePeriodApi.ts](../src/modules/administration/api/batchJobActivePeriodApi.ts).

### BatchJobHistoryPage

[BatchJobHistoryPage.tsx](../src/modules/administration/pages/BatchJobHistoryPage.tsx) — Read-only execution history delegated to BatchJobHistory, including status, timestamps and output.
Pattern feature composition / custom layout. Local state and behavior evidence delegated to imported composition.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [BatchJobHistory.tsx](../src/modules/administration/components/BatchJobHistory.tsx).

### SysBackgroundJobGroupPage

[SysBackgroundJobGroupPage.tsx](../src/modules/administration/pages/SysBackgroundJobGroupPage.tsx) — Editable batch group reference list with notifications and required group identifiers.
Pattern simple-list. Local state and behavior evidence delegated to imported composition.
Permission evidence BACKGROUND_JOB_CREATE, BACKGROUND_JOB_DELETE, BACKGROUND_JOB_EDIT.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [queryClient.ts](../src/core/api/queryClient.ts), [useNotifications.ts](../src/shared/hooks/useNotifications.ts), [sysBackgroundJobGroupApi.ts](../src/modules/administration/api/sysBackgroundJobGroupApi.ts).

### SysBackgroundJobPage

[SysBackgroundJobPage.tsx](../src/modules/administration/pages/SysBackgroundJobPage.tsx) — Batch definitions, recurrence, payload and task grid. Execution commands and handlers remain feature-owned.
Pattern list-details. Local state and behavior evidence useQuery, useState, validate:.
Permission evidence System.BackgroundJobs.Create, System.BackgroundJobs.Delete, System.BackgroundJobs.Edit, System.BackgroundJobs.Run, System.BackgroundJobs.View.
Related implementation [useGridFilterRow.ts](../src/shared/components/data-grid/hooks/useGridFilterRow.ts), [useBatchJobCommands.tsx](../src/modules/administration/components/useBatchJobCommands.tsx), [BatchJobTasks.tsx](../src/modules/administration/components/BatchJobTasks.tsx), [AppDateTimeField.tsx](../src/shared/components/fields/AppDateTimeField.tsx), [AppLookupField.tsx](../src/shared/components/fields/AppLookupField.tsx), [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [sysBackgroundJobApi.ts](../src/modules/administration/api/sysBackgroundJobApi.ts), [sysBackgroundJobGroupApi.ts](../src/modules/administration/api/sysBackgroundJobGroupApi.ts), [batchJobActivePeriodApi.ts](../src/modules/administration/api/batchJobActivePeriodApi.ts).

### SysNumberSequencePage

[SysNumberSequencePage.tsx](../src/modules/administration/pages/SysNumberSequencePage.tsx) — Number sequence scope, segments, cleanup and performance configuration with required-code validation.
Pattern list-details. Local state and behavior evidence useState, validate:.
Permission evidence System.NumberSequences.Create, System.NumberSequences.Delete, System.NumberSequences.Edit, System.NumberSequences.View.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [sysNumberSequenceApi.ts](../src/modules/administration/api/sysNumberSequenceApi.ts).

### DashboardPage

[DashboardPage.tsx](../src/modules/dashboard/pages/DashboardPage.tsx) — Workspace tiles derived from mock customer, order and currency datasets.
Pattern workspace. Local state and behavior evidence delegated to imported composition.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [WorkspacePage.tsx](../src/patterns/workspace/WorkspacePage.tsx), [WorkspaceSection.tsx](../src/patterns/workspace/WorkspaceSection.tsx), [WorkspaceTile.tsx](../src/patterns/workspace/WorkspaceTile.tsx).

### CustomerListPage

[CustomerListPage.tsx](../src/modules/finance/accounts-receivable/customer/pages/CustomerListPage.tsx) — Customer inquiry, quick creation, record navigation, account actions and contact navigation.
Pattern simple-list. Local state and behavior evidence useQuery, useState.
Permission evidence CONTACT_VIEW, CUSTOMER_CREATE, CUSTOMER_DELETE, CUSTOMER_TRANSACTION_VIEW, CUSTOMER_UPDATE, SALES_ORDER_VIEW.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [StatusBadge.tsx](../src/shared/components/status/StatusBadge.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [CustomerQuickCreate.tsx](../src/modules/finance/accounts-receivable/customer/components/CustomerQuickCreate.tsx), [customerQuickCreateApi.ts](../src/modules/finance/accounts-receivable/customer/api/customerQuickCreateApi.ts), [useNotifications.ts](../src/shared/hooks/useNotifications.ts).

### CustomerOpenTransactionsPage

[CustomerOpenTransactionsPage.tsx](../src/modules/finance/accounts-receivable/customer/pages/CustomerOpenTransactionsPage.tsx) — Read-only open-balance inquiry scoped by company and customer, with reconciliation feedback.
Pattern simple-list. Local state and behavior evidence useQuery, useParams.
Permission evidence CUSTOMER_TRANSACTION_VIEW.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [customerOpenTransactionsApi.ts](../src/modules/finance/accounts-receivable/customer/api/customerOpenTransactionsApi.ts).

### CustTablePage

[CustTablePage.tsx](../src/modules/finance/accounts-receivable/customer/pages/CustTablePage.tsx) — Customer master, defaults, logistics and account actions. Field validation and command permissions are feature-owned.
Pattern list-details. Local state and behavior evidence useQuery, useState, validate:, useParams.
Permission evidence CONTACT_CREATE, CONTACT_VIEW, CUSTOMER_CREATE, CUSTOMER_DELETE, CUSTOMER_TRANSACTION_VIEW, CUSTOMER_UPDATE, CUSTOMER_VIEW, FREE_TEXT_INVOICE_VIEW, PRICE_DISCOUNT_GROUP_VIEW, SALES_ORDER_CREATE, SALES_ORDER_VIEW, TRADE_AGREEMENT_JOURNAL_NAME_VIEW, TRADE_AGREEMENT_JOURNAL_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [customerSellRibbon.ts](../src/modules/finance/accounts-receivable/customer/components/customerSellRibbon.ts), [SalesOrderQuickCreate.tsx](../src/modules/finance/accounts-receivable/sales-order/components/SalesOrderQuickCreate.tsx), [customerRibbon.ts](../src/modules/finance/accounts-receivable/customer/components/customerRibbon.ts), [recordTableIds.ts](../src/shared/components/documents/recordTableIds.ts), [customerQuickCreateApi.ts](../src/modules/finance/accounts-receivable/customer/api/customerQuickCreateApi.ts), [PartyLogisticsPanels.tsx](../src/shared/components/logistics/PartyLogisticsPanels.tsx), [salesOrderLinesApi.ts](../src/modules/finance/accounts-receivable/sales-order/api/salesOrderLinesApi.ts), [accountsReceivableQueryKeys.ts](../src/modules/finance/accounts-receivable/queries/accountsReceivableQueryKeys.ts).

### CustTrans

[CustTrans.tsx](../src/modules/finance/accounts-receivable/customer/pages/CustTrans.tsx) — Read-only customer ledger inquiry, text/open/closed/revaluation filters and amounts. Direct API call in page.
Pattern simple-list. Local state and behavior evidence useQuery, useState, useParams.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [apiClient.ts](../src/core/api/apiClient.ts), [apiResponse.ts](../src/core/api/apiResponse.ts), [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts).

### CustomerGroupListPage

[CustomerGroupListPage.tsx](../src/modules/finance/accounts-receivable/customer-group/pages/CustomerGroupListPage.tsx) — Editable customer groups with posting/payment lookups and cache invalidation after saves.
Pattern simple-list. Local state and behavior evidence useQuery.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [customerGroupApi.ts](../src/modules/finance/accounts-receivable/customer-group/api/customerGroupApi.ts), [customerQuickCreateApi.ts](../src/modules/finance/accounts-receivable/customer/api/customerQuickCreateApi.ts).

### CustFreeInvoiceDetailsPage

[CustFreeInvoiceDetailsPage.tsx](../src/modules/finance/accounts-receivable/free-text-invoice/pages/CustFreeInvoiceDetailsPage.tsx) — Free text invoice document, header and line editing, accounting/tax lookups, totals, posting and validation.
Pattern document, list-details, master-detail. Local state and behavior evidence useQuery, useState, useParams, useUnsavedChanges.
Permission evidence CUSTOMER_VIEW, FREE_TEXT_INVOICE_CREATE, FREE_TEXT_INVOICE_DELETE, FREE_TEXT_INVOICE_EDIT, FREE_TEXT_INVOICE_POST.
Related implementation [index.ts](../src/patterns/master-detail/index.ts), [accountsReceivableQueryKeys.ts](../src/modules/finance/accounts-receivable/queries/accountsReceivableQueryKeys.ts), [ActionPaneBackButton.tsx](../src/shared/components/action-pane/ActionPaneBackButton.tsx), [index.ts](../src/shared/components/documents/index.ts), [types.ts](../src/patterns/list-details/types.ts), [DocumentTotalsDrawer.tsx](../src/patterns/document/DocumentTotalsDrawer.tsx), [EnterpriseCrudActions.tsx](../src/shared/components/action-pane/EnterpriseCrudActions.tsx), [ConfirmationDialog.tsx](../src/shared/components/dialogs/ConfirmationDialog.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [useGridFilterRow.ts](../src/shared/components/data-grid/hooks/useGridFilterRow.ts), [FieldPresentationContext.tsx](../src/shared/components/fields/FieldPresentationContext.tsx), [LookupField.tsx](../src/shared/components/lookups/LookupField.tsx), [DataGridLookupField.tsx](../src/shared/components/lookups/DataGridLookupField.tsx), [types.ts](../src/shared/components/lookups/types.ts), [LoadingState.tsx](../src/shared/components/feedback/LoadingState.tsx), [ErrorState.tsx](../src/shared/components/feedback/ErrorState.tsx), [useUnsavedChanges.ts](../src/shared/hooks/useUnsavedChanges.ts), [useNotifications.ts](../src/shared/hooks/useNotifications.ts), [customerQuickCreateApi.ts](../src/modules/finance/accounts-receivable/customer/api/customerQuickCreateApi.ts), [freeTextInvoiceApi.ts](../src/modules/finance/accounts-receivable/free-text-invoice/api/freeTextInvoiceApi.ts).

### CustFreeInvoiceListPage

[CustFreeInvoiceListPage.tsx](../src/modules/finance/accounts-receivable/free-text-invoice/pages/CustFreeInvoiceListPage.tsx) — Free text invoice list with new/edit/delete/post commands and selected-record information.
Pattern simple-list. Local state and behavior evidence useQuery, useMutation, useState.
Permission evidence CUSTOMER_VIEW, FREE_TEXT_INVOICE_CREATE, FREE_TEXT_INVOICE_DELETE, FREE_TEXT_INVOICE_EDIT, FREE_TEXT_INVOICE_POST.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [ListInfoValue.tsx](../src/patterns/simple-list/ListInfoValue.tsx), [AppDialog.tsx](../src/shared/components/dialogs/AppDialog.tsx), [ConfirmationDialog.tsx](../src/shared/components/dialogs/ConfirmationDialog.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [useNotifications.ts](../src/shared/hooks/useNotifications.ts), [freeTextInvoiceApi.ts](../src/modules/finance/accounts-receivable/free-text-invoice/api/freeTextInvoiceApi.ts), [customerQuickCreateApi.ts](../src/modules/finance/accounts-receivable/customer/api/customerQuickCreateApi.ts), [accountsReceivableQueryKeys.ts](../src/modules/finance/accounts-receivable/queries/accountsReceivableQueryKeys.ts).

### PriceDiscountGroupPage

[PriceDiscountGroupPage.tsx](../src/modules/finance/accounts-receivable/price-discount/pages/PriceDiscountGroupPage.tsx) — Price/discount group setup delegated to PriceDiscSetupListPage.
Pattern feature composition / custom layout. Local state and behavior evidence delegated to imported composition.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [PriceDiscSetupListPage.tsx](../src/modules/finance/accounts-receivable/price-discount/components/PriceDiscSetupListPage.tsx).

### TradeAgreementJournalNamePage

[TradeAgreementJournalNamePage.tsx](../src/modules/finance/accounts-receivable/price-discount/pages/TradeAgreementJournalNamePage.tsx) — Trade agreement journal name setup delegated to PriceDiscSetupListPage.
Pattern feature composition / custom layout. Local state and behavior evidence delegated to imported composition.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [PriceDiscSetupListPage.tsx](../src/modules/finance/accounts-receivable/price-discount/components/PriceDiscSetupListPage.tsx).

### TradeAgreementJournalPage

[TradeAgreementJournalPage.tsx](../src/modules/finance/accounts-receivable/price-discount/pages/TradeAgreementJournalPage.tsx) — Trade agreement journal setup delegated to PriceDiscSetupListPage.
Pattern feature composition / custom layout. Local state and behavior evidence delegated to imported composition.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [PriceDiscSetupListPage.tsx](../src/modules/finance/accounts-receivable/price-discount/components/PriceDiscSetupListPage.tsx).

### CustConfirmJournal

[CustConfirmJournal.tsx](../src/modules/finance/accounts-receivable/sales-order/pages/CustConfirmJournal.tsx) — Sales order confirmations, filtered to the selected order, using the simple-list inquiry.
Pattern simple-list. Local state and behavior evidence useQuery, useState, useParams.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [ErrorState.tsx](../src/shared/components/feedback/ErrorState.tsx), [LoadingState.tsx](../src/shared/components/feedback/LoadingState.tsx), [salesOrderListApi.ts](../src/modules/finance/accounts-receivable/sales-order/api/salesOrderListApi.ts), [salesOrderConfirmationApi.ts](../src/modules/finance/accounts-receivable/sales-order/api/salesOrderConfirmationApi.ts), [accountsReceivableQueryKeys.ts](../src/modules/finance/accounts-receivable/queries/accountsReceivableQueryKeys.ts).

### CustPackingSlipJournal

[CustPackingSlipJournal.tsx](../src/modules/finance/accounts-receivable/sales-order/pages/CustPackingSlipJournal.tsx) — Packing slip headers and selected journal lines. Queries are keyed by company/order.
Pattern simple-list. Local state and behavior evidence useQuery, useState, useParams.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [ErrorState.tsx](../src/shared/components/feedback/ErrorState.tsx), [LoadingState.tsx](../src/shared/components/feedback/LoadingState.tsx), [salesOrderListApi.ts](../src/modules/finance/accounts-receivable/sales-order/api/salesOrderListApi.ts), [salesOrderPackingSlipApi.ts](../src/modules/finance/accounts-receivable/sales-order/api/salesOrderPackingSlipApi.ts), [accountsReceivableQueryKeys.ts](../src/modules/finance/accounts-receivable/queries/accountsReceivableQueryKeys.ts).

### SalesOrderChargesPage

[SalesOrderChargesPage.tsx](../src/modules/finance/accounts-receivable/sales-order/pages/SalesOrderChargesPage.tsx) — Order or line charges delegated to the feature DocumentChargesPage with edit permissions.
Pattern feature composition / custom layout. Local state and behavior evidence useQuery, useParams.
Permission evidence SALES_ORDER_UPDATE.
Related implementation [ErrorState.tsx](../src/shared/components/feedback/ErrorState.tsx), [LoadingState.tsx](../src/shared/components/feedback/LoadingState.tsx), [queryClient.ts](../src/core/api/queryClient.ts), [DocumentChargesPage.tsx](../src/modules/finance/foundation/charges/components/DocumentChargesPage.tsx), [salesOrderListApi.ts](../src/modules/finance/accounts-receivable/sales-order/api/salesOrderListApi.ts), [salesOrderLinesApi.ts](../src/modules/finance/accounts-receivable/sales-order/api/salesOrderLinesApi.ts), [accountsReceivableQueryKeys.ts](../src/modules/finance/accounts-receivable/queries/accountsReceivableQueryKeys.ts).

### SalesOrderDetailsPage

[SalesOrderDetailsPage.tsx](../src/modules/finance/accounts-receivable/sales-order/pages/SalesOrderDetailsPage.tsx) — Sales order header/lines, totals, copy, confirmation and packing-slip commands. useSalesOrderDocument and view builders own business state.
Pattern document, list-details. Local state and behavior evidence delegated to imported composition.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [EnterpriseCrudActions.tsx](../src/shared/components/action-pane/EnterpriseCrudActions.tsx), [ActionPaneGroup.tsx](../src/shared/components/action-pane/ActionPaneGroup.tsx), [ActionPaneRibbonTrigger.tsx](../src/shared/components/action-pane/ActionPaneRibbonTrigger.tsx), [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [ErrorState.tsx](../src/shared/components/feedback/ErrorState.tsx), [LogisticsPostalAddressDrawer.tsx](../src/shared/components/logistics/LogisticsPostalAddressDrawer.tsx), [LoadingState.tsx](../src/shared/components/feedback/LoadingState.tsx), [DocumentTotalsDrawer.tsx](../src/patterns/document/DocumentTotalsDrawer.tsx), [DocumentCopyDrawer.tsx](../src/patterns/document/DocumentCopyDrawer.tsx), [salesOrderCopyApi.ts](../src/modules/finance/accounts-receivable/sales-order/api/salesOrderCopyApi.ts), [SalesOrderConfirmationDialog.tsx](../src/modules/finance/accounts-receivable/sales-order/components/SalesOrderConfirmationDialog.tsx), [SalesOrderPackingSlipPostingDialog.tsx](../src/modules/finance/accounts-receivable/sales-order/components/SalesOrderPackingSlipPostingDialog.tsx), [salesOrderDetailsConfig.ts](../src/modules/finance/accounts-receivable/sales-order/details/salesOrderDetailsConfig.ts), [useSalesOrderDocument.tsx](../src/modules/finance/accounts-receivable/sales-order/details/useSalesOrderDocument.tsx), [salesOrderTotals.ts](../src/modules/finance/accounts-receivable/sales-order/details/salesOrderTotals.ts), [SalesOrderHeaderPanel.tsx](../src/modules/finance/accounts-receivable/sales-order/details/SalesOrderHeaderPanel.tsx), [SalesOrderLineFields.tsx](../src/modules/finance/accounts-receivable/sales-order/details/SalesOrderLineFields.tsx), [SalesOrderLineTabs.tsx](../src/modules/finance/accounts-receivable/sales-order/details/SalesOrderLineTabs.tsx), [SalesOrderHeaderSections.tsx](../src/modules/finance/accounts-receivable/sales-order/details/SalesOrderHeaderSections.tsx).

### SalesOrderListPage

[SalesOrderListPage.tsx](../src/modules/finance/accounts-receivable/sales-order/pages/SalesOrderListPage.tsx) — Sales orders, status, selected-record information and quick creation.
Pattern simple-list. Local state and behavior evidence useQuery, useState.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [ListInfoValue.tsx](../src/patterns/simple-list/ListInfoValue.tsx), [StatusBadge.tsx](../src/shared/components/status/StatusBadge.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [salesOrderListApi.ts](../src/modules/finance/accounts-receivable/sales-order/api/salesOrderListApi.ts), [SalesOrderQuickCreate.tsx](../src/modules/finance/accounts-receivable/sales-order/components/SalesOrderQuickCreate.tsx), [accountsReceivableQueryKeys.ts](../src/modules/finance/accounts-receivable/queries/accountsReceivableQueryKeys.ts).

### CustomerPostingProfilesPage

[CustomerPostingProfilesPage.tsx](../src/modules/finance/accounts-receivable/setup/pages/CustomerPostingProfilesPage.tsx) — Posting profile master and child account setup using list-details-listgrid.
Pattern list-details, list-details-listgrid. Local state and behavior evidence useState, validate:.
Permission evidence CUSTOMER_POSTING_PROFILE_CREATE, CUSTOMER_POSTING_PROFILE_DELETE, CUSTOMER_POSTING_PROFILE_EDIT, CUSTOMER_POSTING_PROFILE_VIEW.
Related implementation [ListDetailsListGridPage.tsx](../src/patterns/list-details-listgrid/ListDetailsListGridPage.tsx), [ListGridField.tsx](../src/patterns/list-details-listgrid/ListGridField.tsx), [types.ts](../src/patterns/list-details/types.ts), [customerPostingProfileApi.ts](../src/modules/finance/accounts-receivable/setup/api/customerPostingProfileApi.ts), [CustomerPostingAccountsPanel.tsx](../src/modules/finance/accounts-receivable/setup/components/CustomerPostingAccountsPanel.tsx).

### CustParametersPage

[CustParametersPage.tsx](../src/modules/finance/accounts-receivable/setup/pages/CustParametersPage.tsx) — Customer parameters grouped in SetupPage sections and switches. Verify persistence independently before changing behavior.
Pattern setup. Local state and behavior evidence delegated to imported composition.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [SetupPage.tsx](../src/patterns/setup/SetupPage.tsx), [types.ts](../src/patterns/setup/types.ts).

### CustPaymModePage

[CustPaymModePage.tsx](../src/modules/finance/accounts-receivable/setup/pages/CustPaymModePage.tsx) — Payment methods delegated to CustomerPaymentSetupPage.
Pattern feature composition / custom layout. Local state and behavior evidence delegated to imported composition.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [CustomerPaymentSetupPage.tsx](../src/modules/finance/accounts-receivable/setup/components/CustomerPaymentSetupPage.tsx).

### CustPaymTermPage

[CustPaymTermPage.tsx](../src/modules/finance/accounts-receivable/setup/pages/CustPaymTermPage.tsx) — Payment terms delegated to CustomerPaymentSetupPage.
Pattern feature composition / custom layout. Local state and behavior evidence delegated to imported composition.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [CustomerPaymentSetupPage.tsx](../src/modules/finance/accounts-receivable/setup/components/CustomerPaymentSetupPage.tsx).

### MarkupTablePage

[MarkupTablePage.tsx](../src/modules/finance/foundation/charges/pages/MarkupTablePage.tsx) — Charges code master with required code/description and tax lookup.
Pattern list-details. Local state and behavior evidence useQuery, validate:.
Permission evidence MARKUP_TABLE_CREATE, MARKUP_TABLE_DELETE, MARKUP_TABLE_EDIT, MARKUP_TABLE_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [markupTableApi.ts](../src/modules/finance/foundation/charges/api/markupTableApi.ts), [taxItemGroupApi.ts](../src/modules/finance/tax/api/taxItemGroupApi.ts).

### AccountContactsPage

[AccountContactsPage.tsx](../src/modules/finance/foundation/contacts/pages/AccountContactsPage.tsx) — Customer/vendor scoped contacts grid and detail routing. Direct ContactPerson API read/delete in page.
Pattern simple-list. Local state and behavior evidence useQuery, useState, useParams.
Permission evidence CONTACT_CREATE, CONTACT_DELETE, CONTACT_EDIT.
Related implementation [apiClient.ts](../src/core/api/apiClient.ts), [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx).

### ContactPersonPage

[ContactPersonPage.tsx](../src/modules/finance/foundation/contacts/pages/ContactPersonPage.tsx) — Contact form using ListDetailsPage, name fields, dates, addresses, communication and account navigation.
Pattern list-details. Local state and behavior evidence useQuery, useMutation, useState, validate:, useParams.
Permission evidence CONTACT_CREATE, CONTACT_DELETE, CONTACT_EDIT, CONTACT_VIEW, CUSTOMER_VIEW.
Related implementation [apiClient.ts](../src/core/api/apiClient.ts), [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [PartyLogisticsPanels.tsx](../src/shared/components/logistics/PartyLogisticsPanels.tsx), [customerQuickCreateApi.ts](../src/modules/finance/accounts-receivable/customer/api/customerQuickCreateApi.ts).

### CurrencyPage

[CurrencyPage.tsx](../src/modules/finance/foundation/currency/pages/CurrencyPage.tsx) — Currency master and formatting/rounding settings with required currency code.
Pattern list-details. Local state and behavior evidence validate:.
Permission evidence CURRENCY_MANAGE, CURRENCY_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [currencyApi.ts](../src/modules/finance/foundation/currency/api/currencyApi.ts).

### ExchangeRatePage

[ExchangeRatePage.tsx](../src/modules/finance/foundation/exchange-rate/pages/ExchangeRatePage.tsx) — Currency pairs and rates in locally controlled state and TabularDetailPanel. Server persistence is not established by this page.
Pattern list-details. Local state and behavior evidence useState, validate:.
Permission evidence CURRENCY_MANAGE, CURRENCY_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [TabularDetailPanel.tsx](../src/patterns/list-details/TabularDetailPanel.tsx), [types.ts](../src/patterns/list-details/types.ts), [types.ts](../src/shared/components/data-grid/types.ts).

### ExchangeRateTypePage

[ExchangeRateTypePage.tsx](../src/modules/finance/foundation/exchange-rate/pages/ExchangeRateTypePage.tsx) — Editable exchange-rate-type reference grid.
Pattern simple-list. Local state and behavior evidence delegated to imported composition.
Permission evidence CURRENCY_MANAGE.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [queryClient.ts](../src/core/api/queryClient.ts), [useNotifications.ts](../src/shared/hooks/useNotifications.ts), [exchangeRateTypeApi.ts](../src/modules/finance/foundation/exchange-rate/api/exchangeRateTypeApi.ts).

### EcoResStorageDimensionGroupPage

[EcoResStorageDimensionGroupPage.tsx](../src/modules/finance/inventory/dimension-groups/pages/EcoResStorageDimensionGroupPage.tsx) — Storage-dimension group master with required group identifier/name.
Pattern list-details. Local state and behavior evidence validate:.
Permission evidence INVENTORY_TRANSACTION_CREATE, INVENTORY_TRANSACTION_DELETE, INVENTORY_TRANSACTION_EDIT, INVENTORY_TRANSACTION_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [DataGrid.tsx](../src/shared/components/data-grid/DataGrid.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [storageDimensionGroupApi.ts](../src/modules/finance/inventory/dimension-groups/api/storageDimensionGroupApi.ts).

### EcoResTrackingDimensionGroupPage

[EcoResTrackingDimensionGroupPage.tsx](../src/modules/finance/inventory/dimension-groups/pages/EcoResTrackingDimensionGroupPage.tsx) — Tracking-dimension master and dimension settings.
Pattern list-details. Local state and behavior evidence validate:.
Permission evidence INVENTORY_TRANSACTION_CREATE, INVENTORY_TRANSACTION_DELETE, INVENTORY_TRANSACTION_EDIT, INVENTORY_TRANSACTION_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [DataGrid.tsx](../src/shared/components/data-grid/DataGrid.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [trackingDimensionGroupApi.ts](../src/modules/finance/inventory/dimension-groups/api/trackingDimensionGroupApi.ts).

### InventOnHandPage

[InventOnHandPage.tsx](../src/modules/finance/inventory/inquiries/pages/InventOnHandPage.tsx) — Server-paged on-hand inquiry with draft filters committed on Search and pagination.
Pattern simple-list. Local state and behavior evidence useQuery, useState.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [inventOnHandApi.ts](../src/modules/finance/inventory/inquiries/api/inventOnHandApi.ts).

### InventTransPage

[InventTransPage.tsx](../src/modules/finance/inventory/inquiries/pages/InventTransPage.tsx) — Inventory transaction inquiry with selectable transaction and dimension details.
Pattern simple-list. Local state and behavior evidence useQuery, useState.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [inventTransApi.ts](../src/modules/finance/inventory/inquiries/api/inventTransApi.ts), [summarizeInventTransRows.ts](../src/modules/finance/inventory/inquiries/components/summarizeInventTransRows.ts).

### InventItemGroupPage

[InventItemGroupPage.tsx](../src/modules/finance/inventory/item-groups/pages/InventItemGroupPage.tsx) — Item group master and posting accounts using list-details-listgrid and ledger lookups.
Pattern list-details, list-details-listgrid. Local state and behavior evidence useQuery, validate:.
Permission evidence INVENTORY_TRANSACTION_CREATE, INVENTORY_TRANSACTION_DELETE, INVENTORY_TRANSACTION_EDIT, INVENTORY_TRANSACTION_VIEW.
Related implementation [ListDetailsListGridPage.tsx](../src/patterns/list-details-listgrid/ListDetailsListGridPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [inventItemGroupApi.ts](../src/modules/finance/inventory/item-groups/api/inventItemGroupApi.ts), [InventItemGroupPostingGrid.tsx](../src/modules/finance/inventory/item-groups/components/InventItemGroupPostingGrid.tsx).

### InventModelGroupPage

[InventModelGroupPage.tsx](../src/modules/finance/inventory/item-groups/pages/InventModelGroupPage.tsx) — Inventory model group policy fields using configured detail sections.
Pattern list-details. Local state and behavior evidence validate:.
Permission evidence INVENTORY_TRANSACTION_CREATE, INVENTORY_TRANSACTION_DELETE, INVENTORY_TRANSACTION_EDIT, INVENTORY_TRANSACTION_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [inventModelGroupApi.ts](../src/modules/finance/inventory/item-groups/api/inventModelGroupApi.ts).

### InventJournalNamePage

[InventJournalNamePage.tsx](../src/modules/finance/inventory/journals/pages/InventJournalNamePage.tsx) — Journal-name configuration with lookups and required identifier/description.
Pattern list-details. Local state and behavior evidence useQuery, validate:.
Permission evidence INVENTORY_TRANSACTION_CREATE, INVENTORY_TRANSACTION_DELETE, INVENTORY_TRANSACTION_EDIT, INVENTORY_TRANSACTION_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [inventJournalNameApi.ts](../src/modules/finance/inventory/journals/api/inventJournalNameApi.ts).

### InventJournalPage

[InventJournalPage.tsx](../src/modules/finance/inventory/journals/pages/InventJournalPage.tsx) — Custom inventory-journal workspace with journal/line dialogs, dependent lookups, validation and posting.
Pattern feature composition / custom layout. Local state and behavior evidence useQuery, useMutation, useState.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [inventJournalApi.ts](../src/modules/finance/inventory/journals/api/inventJournalApi.ts), [inventJournalDraft.ts](../src/modules/finance/inventory/journals/components/inventJournalDraft.ts), [inventLocationApi.ts](../src/modules/finance/inventory/warehouses/api/inventLocationApi.ts), [wmsLocationApi.ts](../src/modules/finance/inventory/warehouses/api/wmsLocationApi.ts).

### EcoResDistinctProductDetailsPage

[EcoResDistinctProductDetailsPage.tsx](../src/modules/finance/inventory/products/pages/EcoResDistinctProductDetailsPage.tsx) — Shared product master details with required product number/name.
Pattern list-details. Local state and behavior evidence validate:, useParams.
Permission evidence INVENTORY_TRANSACTION_CREATE, INVENTORY_TRANSACTION_DELETE, INVENTORY_TRANSACTION_EDIT, INVENTORY_TRANSACTION_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [ecoResProductApi.ts](../src/modules/finance/inventory/products/api/ecoResProductApi.ts).

### EcoResDistinctProductListPage

[EcoResDistinctProductListPage.tsx](../src/modules/finance/inventory/products/pages/EcoResDistinctProductListPage.tsx) — Shared product list and creation/navigation actions.
Pattern simple-list. Local state and behavior evidence useQuery, useState.
Permission evidence INVENTORY_TRANSACTION_CREATE, INVENTORY_TRANSACTION_DELETE, INVENTORY_TRANSACTION_EDIT.
Related implementation [queryClient.ts](../src/core/api/queryClient.ts), [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [ecoResProductApi.ts](../src/modules/finance/inventory/products/api/ecoResProductApi.ts), [EcoResProductCreateDrawer.tsx](../src/modules/finance/inventory/products/components/EcoResProductCreateDrawer.tsx).

### EcoResProductDetailsPage

[EcoResProductDetailsPage.tsx](../src/modules/finance/inventory/products/pages/EcoResProductDetailsPage.tsx) — Released-product details scoped to company with inventory group lookups and barcode action.
Pattern list-details. Local state and behavior evidence useQuery, useState, validate:, useParams.
Permission evidence INVENTORY_TRANSACTION_EDIT, INVENTORY_TRANSACTION_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [releasedProductApi.ts](../src/modules/finance/inventory/products/api/releasedProductApi.ts), [ReleasedProductBarcodesDialog.tsx](../src/modules/finance/inventory/products/components/ReleasedProductBarcodesDialog.tsx).

### EcoResProductListPage

[EcoResProductListPage.tsx](../src/modules/finance/inventory/products/pages/EcoResProductListPage.tsx) — Released products list and navigation.
Pattern simple-list. Local state and behavior evidence useQuery, useState.
Permission evidence INVENTORY_TRANSACTION_CREATE, INVENTORY_TRANSACTION_EDIT.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [queryClient.ts](../src/core/api/queryClient.ts), [types.ts](../src/shared/components/data-grid/types.ts), [releasedProductApi.ts](../src/modules/finance/inventory/products/api/releasedProductApi.ts), [ReleasedProductCreateDrawer.tsx](../src/modules/finance/inventory/products/components/ReleasedProductCreateDrawer.tsx).

### EcoResProductReleasePage

[EcoResProductReleasePage.tsx](../src/modules/finance/inventory/products/pages/EcoResProductReleasePage.tsx) — Custom product release flow with selected products and companies.
Pattern feature composition / custom layout. Local state and behavior evidence useQuery, useState.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [PageContainer.tsx](../src/shared/components/page/PageContainer.tsx), [DataGrid.tsx](../src/shared/components/data-grid/DataGrid.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [ecoResProductApi.ts](../src/modules/finance/inventory/products/api/ecoResProductApi.ts), [releasedProductApi.ts](../src/modules/finance/inventory/products/api/releasedProductApi.ts).

### InventParametersPage

[InventParametersPage.tsx](../src/modules/finance/inventory/setup/pages/InventParametersPage.tsx) — Inventory parameters and number-sequence lookup in SetupPage.
Pattern setup. Local state and behavior evidence useQuery.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [SetupPage.tsx](../src/patterns/setup/SetupPage.tsx), [types.ts](../src/patterns/setup/types.ts), [inventParametersApi.ts](../src/modules/finance/inventory/setup/api/inventParametersApi.ts).

### InventPostingPage

[InventPostingPage.tsx](../src/modules/finance/inventory/setup/pages/InventPostingPage.tsx) — Posting mappings presented by tab, account type and editable grid in TabbedChoiceGridPage.
Pattern tabbed-choice-grid. Local state and behavior evidence useQuery, useState.
Permission evidence INVENTORY_TRANSACTION_CREATE, INVENTORY_TRANSACTION_DELETE, INVENTORY_TRANSACTION_EDIT.
Related implementation [index.ts](../src/patterns/tabbed-choice-grid/index.ts), [types.ts](../src/shared/components/data-grid/types.ts), [inventPostingApi.ts](../src/modules/finance/inventory/setup/api/inventPostingApi.ts).

### UnitOfMeasurePage

[UnitOfMeasurePage.tsx](../src/modules/finance/inventory/units-of-measure/pages/UnitOfMeasurePage.tsx) — Units of measure with required symbol and decimal precision validation.
Pattern list-details. Local state and behavior evidence validate:.
Permission evidence INVENTORY_TRANSACTION_CREATE, INVENTORY_TRANSACTION_DELETE, INVENTORY_TRANSACTION_EDIT, INVENTORY_TRANSACTION_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [unitOfMeasureApi.ts](../src/modules/finance/inventory/units-of-measure/api/unitOfMeasureApi.ts), [UnitConversionEditor.tsx](../src/modules/finance/inventory/units-of-measure/components/UnitConversionEditor.tsx).

### InventLocationPage

[InventLocationPage.tsx](../src/modules/finance/inventory/warehouses/pages/InventLocationPage.tsx) — Warehouse master, site and inventory setup lookups with required identifier/name.
Pattern list-details. Local state and behavior evidence useQuery, validate:.
Permission evidence INVENTORY_TRANSACTION_CREATE, INVENTORY_TRANSACTION_DELETE, INVENTORY_TRANSACTION_EDIT, INVENTORY_TRANSACTION_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [LookupField.tsx](../src/shared/components/lookups/LookupField.tsx), [types.ts](../src/shared/components/lookups/types.ts), [index.ts](../src/shared/components/tree-control/index.ts), [inventLocationApi.ts](../src/modules/finance/inventory/warehouses/api/inventLocationApi.ts).

### InventSitePage

[InventSitePage.tsx](../src/modules/finance/inventory/warehouses/pages/InventSitePage.tsx) — Site master with warehouse hierarchy content.
Pattern list-details. Local state and behavior evidence useQuery, validate:.
Permission evidence INVENTORY_TRANSACTION_CREATE, INVENTORY_TRANSACTION_DELETE, INVENTORY_TRANSACTION_EDIT, INVENTORY_TRANSACTION_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [index.ts](../src/shared/components/tree-control/index.ts), [inventSiteApi.ts](../src/modules/finance/inventory/warehouses/api/inventSiteApi.ts).

### WMSLocationPage

[WMSLocationPage.tsx](../src/modules/finance/inventory/warehouses/pages/WMSLocationPage.tsx) — Warehouse location master with warehouse selection and required location identifier.
Pattern list-details. Local state and behavior evidence useQuery, validate:.
Permission evidence INVENTORY_TRANSACTION_CREATE, INVENTORY_TRANSACTION_DELETE, INVENTORY_TRANSACTION_EDIT, INVENTORY_TRANSACTION_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [inventLocationApi.ts](../src/modules/finance/inventory/warehouses/api/inventLocationApi.ts), [wmsLocationApi.ts](../src/modules/finance/inventory/warehouses/api/wmsLocationApi.ts).

### PartyBalancePage

[PartyBalancePage.tsx](../src/modules/finance/party-balances/pages/PartyBalancePage.tsx) — Shared customer/vendor balance adapter. Currency amounts and related-account permission checks feed OpenBalanceCurrencyPage.
Pattern open-balance-currency. Local state and behavior evidence useQuery, useParams.
Permission evidence CONTACT_VIEW, CUSTOMER_TRANSACTION_VIEW, VENDOR_TRANSACTION_VIEW.
Related implementation [LoadingState.tsx](../src/shared/components/feedback/LoadingState.tsx), [ErrorState.tsx](../src/shared/components/feedback/ErrorState.tsx), [OpenBalanceCurrencyPage.tsx](../src/patterns/open-balance-currency/OpenBalanceCurrencyPage.tsx), [types.ts](../src/patterns/open-balance-currency/types.ts), [partyBalanceApi.ts](../src/modules/finance/party-balances/api/partyBalanceApi.ts).

### TaxAuthorityPage

[TaxAuthorityPage.tsx](../src/modules/finance/tax/pages/TaxAuthorityPage.tsx) — Tax authority address/payment configuration with field validation.
Pattern list-details. Local state and behavior evidence validate:.
Permission evidence TAX_AUTHORITY_CREATE, TAX_AUTHORITY_DELETE, TAX_AUTHORITY_EDIT, TAX_AUTHORITY_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [taxAuthorityApi.ts](../src/modules/finance/tax/api/taxAuthorityApi.ts).

### TaxGroupPage

[TaxGroupPage.tsx](../src/modules/finance/tax/pages/TaxGroupPage.tsx) — Tax group master and tax-code membership grid.
Pattern list-details, list-details-listgrid. Local state and behavior evidence useQuery, validate:.
Permission evidence TAX_GROUP_CREATE, TAX_GROUP_DELETE, TAX_GROUP_EDIT, TAX_GROUP_VIEW.
Related implementation [ListDetailsListGridPage.tsx](../src/patterns/list-details-listgrid/ListDetailsListGridPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [taxGroupApi.ts](../src/modules/finance/tax/api/taxGroupApi.ts), [taxTableApi.ts](../src/modules/finance/tax/api/taxTableApi.ts), [TaxGroupLinesPanel.tsx](../src/modules/finance/tax/components/TaxGroupLinesPanel.tsx).

### TaxItemGroupPage

[TaxItemGroupPage.tsx](../src/modules/finance/tax/pages/TaxItemGroupPage.tsx) — Item tax group master and included tax codes.
Pattern list-details, list-details-listgrid. Local state and behavior evidence useQuery, validate:.
Permission evidence TAX_ITEM_GROUP_CREATE, TAX_ITEM_GROUP_DELETE, TAX_ITEM_GROUP_EDIT, TAX_ITEM_GROUP_VIEW.
Related implementation [ListDetailsListGridPage.tsx](../src/patterns/list-details-listgrid/ListDetailsListGridPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [taxItemGroupApi.ts](../src/modules/finance/tax/api/taxItemGroupApi.ts), [taxTableApi.ts](../src/modules/finance/tax/api/taxTableApi.ts), [TaxItemGroupLinesPanel.tsx](../src/modules/finance/tax/components/TaxItemGroupLinesPanel.tsx).

### TaxPeriodPage

[TaxPeriodPage.tsx](../src/modules/finance/tax/pages/TaxPeriodPage.tsx) — Tax settlement periods and interval lines with authority lookup.
Pattern list-details, list-details-listgrid. Local state and behavior evidence useQuery, validate:.
Permission evidence TAX_PERIOD_CREATE, TAX_PERIOD_DELETE, TAX_PERIOD_EDIT, TAX_PERIOD_VIEW.
Related implementation [ListDetailsListGridPage.tsx](../src/patterns/list-details-listgrid/ListDetailsListGridPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [taxPeriodApi.ts](../src/modules/finance/tax/api/taxPeriodApi.ts), [taxAuthorityApi.ts](../src/modules/finance/tax/api/taxAuthorityApi.ts), [TaxPeriodIntervalsPanel.tsx](../src/modules/finance/tax/components/TaxPeriodIntervalsPanel.tsx).

### TaxTablePage

[TaxTablePage.tsx](../src/modules/finance/tax/pages/TaxTablePage.tsx) — Tax codes, currency/period lookup and configured detail fields.
Pattern list-details. Local state and behavior evidence useQuery, validate:.
Permission evidence TAX_CODE_CREATE, TAX_CODE_DELETE, TAX_CODE_EDIT, TAX_CODE_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [currencyApi.ts](../src/modules/finance/foundation/currency/api/currencyApi.ts), [taxPeriodApi.ts](../src/modules/finance/tax/api/taxPeriodApi.ts), [taxTableApi.ts](../src/modules/finance/tax/api/taxTableApi.ts).

### LoginPage

[LoginPage.tsx](../src/modules/identity/pages/LoginPage.tsx) — Authentication form with local busy/error state and return navigation.
Pattern feature composition / custom layout. Local state and behavior evidence useState.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.

### DocuView

[DocuView.tsx](../src/modules/organization/documents/pages/DocuView.tsx) — Record attachments, upload, notes, file preview and image gallery in list-details.
Pattern list-details. Local state and behavior evidence useQuery, useState, validate:.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [AttachmentGalleryDialog.tsx](../src/shared/components/documents/AttachmentGalleryDialog.tsx), [documentApi.ts](../src/shared/components/documents/documentApi.ts).

### HcmDepartmentPage

[HcmDepartmentPage.tsx](../src/modules/organization/pages/HcmDepartmentPage.tsx) — Department code/name/alias/description reference grid with create/update/delete.
Pattern simple-list. Local state and behavior evidence delegated to imported composition.
Permission evidence ORGANIZATION_STRUCTURE_VIEW.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [queryClient.ts](../src/core/api/queryClient.ts), [useNotifications.ts](../src/shared/hooks/useNotifications.ts), [hcmReferenceApi.ts](../src/modules/organization/api/hcmReferenceApi.ts).

### HcmNationalityPage

[HcmNationalityPage.tsx](../src/modules/organization/pages/HcmNationalityPage.tsx) — Nationality code/name/alias/description reference grid with create/update/delete.
Pattern simple-list. Local state and behavior evidence delegated to imported composition.
Permission evidence ORGANIZATION_STRUCTURE_VIEW.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [queryClient.ts](../src/core/api/queryClient.ts), [useNotifications.ts](../src/shared/hooks/useNotifications.ts), [hcmReferenceApi.ts](../src/modules/organization/api/hcmReferenceApi.ts).

### HcmOccupationPage

[HcmOccupationPage.tsx](../src/modules/organization/pages/HcmOccupationPage.tsx) — Occupation code/name/alias/description reference grid with create/update/delete.
Pattern simple-list. Local state and behavior evidence delegated to imported composition.
Permission evidence ORGANIZATION_STRUCTURE_VIEW.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [queryClient.ts](../src/core/api/queryClient.ts), [useNotifications.ts](../src/shared/hooks/useNotifications.ts), [hcmReferenceApi.ts](../src/modules/organization/api/hcmReferenceApi.ts).

### HcmPositionPage

[HcmPositionPage.tsx](../src/modules/organization/pages/HcmPositionPage.tsx) — Position master with organization unit/role and effective dates.
Pattern list-details. Local state and behavior evidence useQuery, validate:.
Permission evidence Organization.Structure.Create, Organization.Structure.Edit, Organization.Structure.View.
Related implementation [AppLookupField.tsx](../src/shared/components/fields/AppLookupField.tsx), [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [organizationStructureApi.ts](../src/modules/organization/api/organizationStructureApi.ts).

### HcmShowroomPage

[HcmShowroomPage.tsx](../src/modules/organization/pages/HcmShowroomPage.tsx) — Showroom master, assignments and shared party logistics panels.
Pattern list-details. Local state and behavior evidence validate:.
Permission evidence Organization.Showrooms.Create, Organization.Showrooms.Delete, Organization.Showrooms.Edit, Organization.Showrooms.View.
Related implementation [useGridFilterRow.ts](../src/shared/components/data-grid/hooks/useGridFilterRow.ts), [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [PartyLogisticsPanels.tsx](../src/shared/components/logistics/PartyLogisticsPanels.tsx), [HcmShowroomAssignmentsPanel.tsx](../src/modules/organization/components/HcmShowroomAssignmentsPanel.tsx), [hcmShowroomApi.ts](../src/modules/organization/api/hcmShowroomApi.ts).

### HcmWorkerPage

[HcmWorkerPage.tsx](../src/modules/organization/pages/HcmWorkerPage.tsx) — Worker master, reference lookups, assignments, addresses and electronic contact information.
Pattern list-details. Local state and behavior evidence useQuery, validate:.
Permission evidence Organization.Employees.Create, Organization.Employees.Delete, Organization.Employees.Edit, Organization.Employees.View.
Related implementation [useGridFilterRow.ts](../src/shared/components/data-grid/hooks/useGridFilterRow.ts), [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [AppLookupField.tsx](../src/shared/components/fields/AppLookupField.tsx), [hcmWorkerApi.ts](../src/modules/organization/api/hcmWorkerApi.ts), [PartyLogisticsPanels.tsx](../src/shared/components/logistics/PartyLogisticsPanels.tsx), [HcmWorkerAssignmentsPanel.tsx](../src/modules/organization/components/HcmWorkerAssignmentsPanel.tsx), [HcmWorkerAssignmentDiagram.tsx](../src/modules/organization/components/HcmWorkerAssignmentDiagram.tsx).

### LegalEntityPage

[LegalEntityPage.tsx](../src/modules/organization/pages/LegalEntityPage.tsx) — Legal entity details, local address/contact panels, dashboard image and report-logo attachments.
Pattern list-details. Local state and behavior evidence useQuery, useState, validate:.
Permission evidence LEGAL_ENTITY_MANAGE, LEGAL_ENTITY_VIEW.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [TabularDetailPanel.tsx](../src/patterns/list-details/TabularDetailPanel.tsx), [types.ts](../src/patterns/list-details/types.ts), [types.ts](../src/shared/components/data-grid/types.ts), [LogisticsPostalAddressDrawer.tsx](../src/shared/components/logistics/LogisticsPostalAddressDrawer.tsx), [LogisticsElectronicAddressDrawer.tsx](../src/shared/components/logistics/LogisticsElectronicAddressDrawer.tsx), [documentApi.ts](../src/shared/components/documents/documentApi.ts), [legalEntityImageAttachments.ts](../src/modules/organization/api/legalEntityImageAttachments.ts).

### OrganizationHierarchyNodePage

[OrganizationHierarchyNodePage.tsx](../src/modules/organization/pages/OrganizationHierarchyNodePage.tsx) — Effective-dated hierarchy tree, parent/unit selection and node validation.
Pattern list-details, list-details-tree. Local state and behavior evidence useQuery, useState, validate:, useParams.
Permission evidence Organization.Structure.Create, Organization.Structure.Edit, Organization.Structure.View.
Related implementation [index.ts](../src/patterns/list-details-tree/index.ts), [types.ts](../src/patterns/list-details/types.ts), [LookupField.tsx](../src/shared/components/lookups/LookupField.tsx), [organizationStructureApi.ts](../src/modules/organization/api/organizationStructureApi.ts).

### OrganizationHierarchyPage

[OrganizationHierarchyPage.tsx](../src/modules/organization/pages/OrganizationHierarchyPage.tsx) — Hierarchy list with associated tree navigation and as-of context.
Pattern simple-list. Local state and behavior evidence useState.
Permission evidence Organization.Structure.Create, Organization.Structure.Edit.
Related implementation [queryClient.ts](../src/core/api/queryClient.ts), [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [DataGridLookupField.tsx](../src/shared/components/lookups/DataGridLookupField.tsx), [organizationStructureApi.ts](../src/modules/organization/api/organizationStructureApi.ts).

### OrganizationPage

[OrganizationPage.tsx](../src/modules/organization/pages/OrganizationPage.tsx) — Compatibility wrapper for OrganizationUnitsPage.
Pattern feature composition / custom layout. Local state and behavior evidence delegated to imported composition.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.

### OrganizationRolePage

[OrganizationRolePage.tsx](../src/modules/organization/pages/OrganizationRolePage.tsx) — Organization role reference list with required code/name.
Pattern simple-list. Local state and behavior evidence delegated to imported composition.
Permission evidence Organization.Structure.Create, Organization.Structure.Edit.
Related implementation [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [queryClient.ts](../src/core/api/queryClient.ts), [useNotifications.ts](../src/shared/hooks/useNotifications.ts), [organizationStructureApi.ts](../src/modules/organization/api/organizationStructureApi.ts).

### OrganizationUnitsPage

[OrganizationUnitsPage.tsx](../src/modules/organization/pages/OrganizationUnitsPage.tsx) — Organization-unit tree and detail editing with type/as-of filters and length validation.
Pattern list-details, list-details-tree. Local state and behavior evidence useQuery, useState, validate:.
Permission evidence Organization.Structure.Create, Organization.Structure.Edit, Organization.Structure.View.
Related implementation [LookupField.tsx](../src/shared/components/lookups/LookupField.tsx), [index.ts](../src/patterns/list-details-tree/index.ts), [types.ts](../src/patterns/list-details/types.ts), [organizationStructureApi.ts](../src/modules/organization/api/organizationStructureApi.ts).

### ReportingHierarchyPage

[ReportingHierarchyPage.tsx](../src/modules/organization/pages/ReportingHierarchyPage.tsx) — Reporting hierarchy master and reporting-line panel with positions and effective dates.
Pattern list-details. Local state and behavior evidence useQuery, useState, validate:.
Permission evidence Organization.Structure.Create, Organization.Structure.Edit, Organization.Structure.View.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [TabularDetailPanel.tsx](../src/patterns/list-details/TabularDetailPanel.tsx), [types.ts](../src/patterns/list-details/types.ts), [types.ts](../src/shared/components/data-grid/types.ts), [organizationStructureApi.ts](../src/modules/organization/api/organizationStructureApi.ts).

### ProcessBuilderPage

[ProcessBuilderPage.tsx](../src/modules/process-builder/pages/ProcessBuilderPage.tsx) — Specialized workflow builder with navigation, drag/drop workspace, settings, loader, draft and action hooks.
Pattern feature composition / custom layout. Local state and behavior evidence useState, useParams.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [useLogicalDrawerAnchor.ts](../src/shared/hooks/useLogicalDrawerAnchor.ts), [ProcessBuilderNavigationPanel.tsx](../src/modules/process-builder/components/ProcessBuilderNavigationPanel.tsx), [ProcessBuilderSettingsPanel.tsx](../src/modules/process-builder/components/ProcessBuilderSettingsPanel.tsx), [ProcessBuilderWorkspace.tsx](../src/modules/process-builder/components/ProcessBuilderWorkspace.tsx), [useProcessBuilderDraft.ts](../src/modules/process-builder/hooks/useProcessBuilderDraft.ts), [processBuilderTokens.ts](../src/modules/process-builder/components/processBuilderTokens.ts), [useProcessBuilderLoader.ts](../src/modules/process-builder/hooks/useProcessBuilderLoader.ts), [useProcessBuilderActions.ts](../src/modules/process-builder/hooks/useProcessBuilderActions.ts), [processBuilderNavigation.ts](../src/modules/process-builder/hooks/processBuilderNavigation.ts).

### WfActivitiesPage

[WfActivitiesPage.tsx](../src/modules/workflow/pages/WfActivitiesPage.tsx) — Workflow activity definition, process/step/performer lookup and validation.
Pattern list-details. Local state and behavior evidence validate:.
Permission evidence Workflow.Activities.Create, Workflow.Activities.Delete, Workflow.Activities.Edit, Workflow.Activities.View.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [AppLookupGridField.tsx](../src/shared/components/fields/AppLookupGridField.tsx), [wfActivityApi.ts](../src/modules/workflow/api/wfActivityApi.ts), [wfPerformerApi.ts](../src/modules/workflow/api/wfPerformerApi.ts), [wfStepApi.ts](../src/modules/workflow/api/wfStepApi.ts), [workflowSetupApis.ts](../src/modules/workflow/api/workflowSetupApis.ts), [workflowMasterApi.ts](../src/modules/workflow/api/workflowMasterApi.ts).

### WfActivityTypesPage

[WfActivityTypesPage.tsx](../src/modules/workflow/pages/WfActivityTypesPage.tsx) — Activity-type reference setup delegated to WorkflowSetupListPage.
Pattern feature composition / custom layout. Local state and behavior evidence delegated to imported composition.
Permission evidence Workflow.ActivityTypes.Create, Workflow.ActivityTypes.Delete, Workflow.ActivityTypes.Edit.
Related implementation [WorkflowSetupListPage.tsx](../src/modules/workflow/components/WorkflowSetupListPage.tsx), [workflowMasterApi.ts](../src/modules/workflow/api/workflowMasterApi.ts), [workflowSetupApis.ts](../src/modules/workflow/api/workflowSetupApis.ts).

### WFCategoryPage

[WFCategoryPage.tsx](../src/modules/workflow/pages/WFCategoryPage.tsx) — Workflow category setup delegated to WorkflowSetupListPage.
Pattern feature composition / custom layout. Local state and behavior evidence delegated to imported composition.
Permission evidence Workflow.Categories.Create, Workflow.Categories.Delete, Workflow.Categories.Edit.
Related implementation [workflowMasterApi.ts](../src/modules/workflow/api/workflowMasterApi.ts), [wfCategoryApi.ts](../src/modules/workflow/api/wfCategoryApi.ts), [WorkflowSetupListPage.tsx](../src/modules/workflow/components/WorkflowSetupListPage.tsx).

### WfControlsPage

[WfControlsPage.tsx](../src/modules/workflow/pages/WfControlsPage.tsx) — Workflow control-type setup delegated to WorkflowSetupListPage.
Pattern feature composition / custom layout. Local state and behavior evidence delegated to imported composition.
Permission evidence Workflow.Controls.Create, Workflow.Controls.Delete, Workflow.Controls.Edit.
Related implementation [WorkflowSetupListPage.tsx](../src/modules/workflow/components/WorkflowSetupListPage.tsx), [workflowMasterApi.ts](../src/modules/workflow/api/workflowMasterApi.ts), [workflowSetupApis.ts](../src/modules/workflow/api/workflowSetupApis.ts).

### WfDataTypesPage

[WfDataTypesPage.tsx](../src/modules/workflow/pages/WfDataTypesPage.tsx) — Workflow data-type setup delegated to WorkflowSetupListPage.
Pattern feature composition / custom layout. Local state and behavior evidence delegated to imported composition.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [WorkflowSetupListPage.tsx](../src/modules/workflow/components/WorkflowSetupListPage.tsx), [workflowMasterApi.ts](../src/modules/workflow/api/workflowMasterApi.ts), [workflowSetupApis.ts](../src/modules/workflow/api/workflowSetupApis.ts).

### WfGenericReportPage

[WfGenericReportPage.tsx](../src/modules/workflow/pages/WfGenericReportPage.tsx) — Workflow report filters/metadata and report results. Local sample/timer-based results require separate persistence verification.
Pattern simple-list. Local state and behavior evidence useQuery, useState.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [types.ts](../src/shared/components/data-grid/types.ts), [AppLookupGridField.tsx](../src/shared/components/fields/AppLookupGridField.tsx), [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [wfProcessApi.ts](../src/modules/workflow/api/wfProcessApi.ts), [wfRequestControlApi.ts](../src/modules/workflow/api/wfRequestControlApi.ts).

### WfMailPage

[WfMailPage.tsx](../src/modules/workflow/pages/WfMailPage.tsx) — Workflow mailbox with dynamic detail fields, attachments, response actions and report viewers.
Pattern list-details. Local state and behavior evidence useQuery, useMutation, useState.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [documentApi.ts](../src/shared/components/documents/documentApi.ts), [recordTableIds.ts](../src/shared/components/documents/recordTableIds.ts), [wfProcessApi.ts](../src/modules/workflow/api/wfProcessApi.ts), [wfRequestApi.ts](../src/modules/workflow/api/wfRequestApi.ts), [DynamicControlRenderer.tsx](../src/modules/workflow/components/DynamicControlRenderer.tsx), [MailFieldValue.tsx](../src/modules/workflow/components/MailFieldValue.tsx), [reportDesignerApi.ts](../src/modules/workflow/report-designer/api/reportDesignerApi.ts), [index.ts](../src/shared/components/report-designer/index.ts), [index.ts](../src/shared/components/report-viewer/index.ts).

### WfPerformersPage

[WfPerformersPage.tsx](../src/modules/workflow/pages/WfPerformersPage.tsx) — Performer setup, assignment type, users, related field and database query configuration.
Pattern list-details. Local state and behavior evidence useQuery, useState, validate:.
Permission evidence Workflow.Performers.Create, Workflow.Performers.Delete, Workflow.Performers.Edit, Workflow.Performers.View.
Related implementation [useGridFilterRow.ts](../src/shared/components/data-grid/hooks/useGridFilterRow.ts), [AppLookupField.tsx](../src/shared/components/fields/AppLookupField.tsx), [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [workflowMasterApi.ts](../src/modules/workflow/api/workflowMasterApi.ts), [wfPerformerApi.ts](../src/modules/workflow/api/wfPerformerApi.ts), [WfPerformerUsersPanel.tsx](../src/modules/workflow/components/WfPerformerUsersPanel.tsx), [WfPerformerRelatedField.tsx](../src/modules/workflow/components/WfPerformerRelatedField.tsx), [WfPerformerDatabaseQueryFields.tsx](../src/modules/workflow/components/WfPerformerDatabaseQueryFields.tsx).

### WfPrioritiesPage

[WfPrioritiesPage.tsx](../src/modules/workflow/pages/WfPrioritiesPage.tsx) — Priority reference setup delegated to WorkflowSetupListPage.
Pattern feature composition / custom layout. Local state and behavior evidence delegated to imported composition.
Permission evidence Workflow.Priorities.Create, Workflow.Priorities.Delete, Workflow.Priorities.Edit.
Related implementation [WorkflowSetupListPage.tsx](../src/modules/workflow/components/WorkflowSetupListPage.tsx), [workflowMasterApi.ts](../src/modules/workflow/api/workflowMasterApi.ts), [workflowSetupApis.ts](../src/modules/workflow/api/workflowSetupApis.ts).

### WFProcessPage

[WFProcessPage.tsx](../src/modules/workflow/pages/WFProcessPage.tsx) — Workflow process definition with category, priority, behavior and required name.
Pattern list-details. Local state and behavior evidence useQuery, validate:.
Permission evidence Workflow.Processes.Create, Workflow.Processes.Delete, Workflow.Processes.Edit, Workflow.Processes.View.
Related implementation [AppLookupGridField.tsx](../src/shared/components/fields/AppLookupGridField.tsx), [AppLookupField.tsx](../src/shared/components/fields/AppLookupField.tsx), [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [wfProcessApi.ts](../src/modules/workflow/api/wfProcessApi.ts), [wfCategoryApi.ts](../src/modules/workflow/api/wfCategoryApi.ts), [workflowSetupApis.ts](../src/modules/workflow/api/workflowSetupApis.ts).

### WfRequestFromPage

[WfRequestFromPage.tsx](../src/modules/workflow/pages/WfRequestFromPage.tsx) — Process-specific dynamic submission form and published print-template mode with attachments.
Pattern list-details. Local state and behavior evidence useQuery, useState, useParams.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [wfProcessApi.ts](../src/modules/workflow/api/wfProcessApi.ts), [DynamicForm.tsx](../src/modules/workflow/components/DynamicForm.tsx), [RecordAttachmentsButton.tsx](../src/shared/components/documents/RecordAttachmentsButton.tsx), [recordTableIds.ts](../src/shared/components/documents/recordTableIds.ts), [reportDesignerApi.ts](../src/modules/workflow/report-designer/api/reportDesignerApi.ts), [reportCompany.ts](../src/shared/components/report-viewer/reportCompany.ts).

### WfRequestSubmissionPage

[WfRequestSubmissionPage.tsx](../src/modules/workflow/pages/WfRequestSubmissionPage.tsx) — Category/process selection that leads to the dynamic request form.
Pattern list-details. Local state and behavior evidence useQuery.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [wfCategoryApi.ts](../src/modules/workflow/api/wfCategoryApi.ts), [wfProcessApi.ts](../src/modules/workflow/api/wfProcessApi.ts).

### WFStepsPage

[WFStepsPage.tsx](../src/modules/workflow/pages/WFStepsPage.tsx) — Process-scoped step configuration with assignment and behavior sections.
Pattern list-details. Local state and behavior evidence delegated to imported composition.
Permission evidence Workflow.Steps.Create, Workflow.Steps.Delete, Workflow.Steps.Edit, Workflow.Steps.View.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [AppLookupGridField.tsx](../src/shared/components/fields/AppLookupGridField.tsx), [wfProcessApi.ts](../src/modules/workflow/api/wfProcessApi.ts), [wfStepApi.ts](../src/modules/workflow/api/wfStepApi.ts).

### WFVariablesPage

[WFVariablesPage.tsx](../src/modules/workflow/pages/WFVariablesPage.tsx) — Process-scoped workflow variables with data-type lookup and mappings.
Pattern list-details. Local state and behavior evidence useQuery.
Permission evidence Workflow.Variables.Create, Workflow.Variables.Delete, Workflow.Variables.Edit, Workflow.Variables.View.
Related implementation [ListDetailsPage.tsx](../src/patterns/list-details/ListDetailsPage.tsx), [types.ts](../src/patterns/list-details/types.ts), [AppLookupField.tsx](../src/shared/components/fields/AppLookupField.tsx), [AppLookupGridField.tsx](../src/shared/components/fields/AppLookupGridField.tsx), [workflowSetupApis.ts](../src/modules/workflow/api/workflowSetupApis.ts), [wfProcessApi.ts](../src/modules/workflow/api/wfProcessApi.ts), [wfVariableApi.ts](../src/modules/workflow/api/wfVariableApi.ts).

### ReportDesignerListPage

[ReportDesignerListPage.tsx](../src/modules/workflow/report-designer/pages/ReportDesignerListPage.tsx) — Print-template list, process scope, editor dialog, publish/archive/delete and confirmation state.
Pattern simple-list. Local state and behavior evidence useQuery, useState.
Permission evidence WF_PRINT_TEMPLATE_ARCHIVE, WF_PRINT_TEMPLATE_CREATE, WF_PRINT_TEMPLATE_DELETE, WF_PRINT_TEMPLATE_EDIT, WF_PRINT_TEMPLATE_PUBLISH.
Related implementation [queryClient.ts](../src/core/api/queryClient.ts), [ActionPaneButton.tsx](../src/shared/components/action-pane/ActionPaneButton.tsx), [types.ts](../src/shared/components/data-grid/types.ts), [AppLookupGridField.tsx](../src/shared/components/fields/AppLookupGridField.tsx), [AppDialog.tsx](../src/shared/components/dialogs/AppDialog.tsx), [ConfirmationDialog.tsx](../src/shared/components/dialogs/ConfirmationDialog.tsx), [useNotifications.ts](../src/shared/hooks/useNotifications.ts), [useLocalStorage.ts](../src/shared/hooks/useLocalStorage.ts), [SimpleListPage.tsx](../src/patterns/simple-list/SimpleListPage.tsx), [recordTableIds.ts](../src/shared/components/documents/recordTableIds.ts), [wfProcessApi.ts](../src/modules/workflow/api/wfProcessApi.ts), [reportDesignerApi.ts](../src/modules/workflow/report-designer/api/reportDesignerApi.ts), [WorkflowReportDesigner.tsx](../src/modules/workflow/report-designer/components/WorkflowReportDesigner.tsx), [index.ts](../src/shared/components/report-designer/index.ts).

### WorkflowMailReportViewerPage

[WorkflowMailReportViewerPage.tsx](../src/modules/workflow/report-viewer/pages/WorkflowMailReportViewerPage.tsx) — Mail report preview/export with request details, process and company data.
Pattern report-viewer. Local state and behavior evidence useQuery, useState.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [ReportViewer.tsx](../src/patterns/report-viewer/ReportViewer.tsx), [exportReport.ts](../src/patterns/report-viewer/exportReport.ts), [ReportViewerDocument.tsx](../src/shared/components/report-viewer/ReportViewerDocument.tsx), [reportCompany.ts](../src/shared/components/report-viewer/reportCompany.ts), [wfRequestApi.ts](../src/modules/workflow/api/wfRequestApi.ts), [WorkflowMailReportViewer.tsx](../src/modules/workflow/report-viewer/components/WorkflowMailReportViewer.tsx), [useNotifications.ts](../src/shared/hooks/useNotifications.ts), [wfProcessApi.ts](../src/modules/workflow/api/wfProcessApi.ts), [apiError.ts](../src/core/api/apiError.ts).

### WorkflowOfficialFormViewerPage

[WorkflowOfficialFormViewerPage.tsx](../src/modules/workflow/report-viewer/pages/WorkflowOfficialFormViewerPage.tsx) — Published official form preview/export with request/template/company data.
Pattern report-viewer. Local state and behavior evidence useQuery, useState.
Permission evidence inspect route registry and imported feature/shared configurations; no inline permission constant in this file.
Related implementation [ReportViewer.tsx](../src/patterns/report-viewer/ReportViewer.tsx), [exportReport.ts](../src/patterns/report-viewer/exportReport.ts), [reportCompany.ts](../src/shared/components/report-viewer/reportCompany.ts), [useNotifications.ts](../src/shared/hooks/useNotifications.ts), [wfRequestApi.ts](../src/modules/workflow/api/wfRequestApi.ts), [reportDesignerApi.ts](../src/modules/workflow/report-designer/api/reportDesignerApi.ts), [index.ts](../src/shared/components/report-viewer/index.ts).

The refactoring carried out after this review, and its verification, are recorded in
[ui-refactoring-log.md](ui-refactoring-log.md).
