# UI refactoring log

What was changed after the Phase 1 source review in [ui-refactoring-review.md](ui-refactoring-review.md), in the order it landed, with the
verification run for each stage. Browser checks referred to below are [e2e/ui-patterns.pw.ts](../e2e/ui-patterns.pw.ts)
(4 customer-section cases and 1 HCM case, each run by the desktop and mobile Playwright
projects).

## Implemented UI refactor

- Extended the existing FastTab with controlled expansion, generated accessible IDs, rich summaries, enterprise/legal-entity appearances, and local root/content style overrides. ListDetailsLayout and MasterDetailSection now compose it. Their callers inherit the same section structure without changing their field configurations or CRUD contracts.
- Centralized section colors, typography, borders and spacing in the MUI theme. Enterprise surfaces use the active palette. Slot-specific rules avoid styling nested sections; content variants allow local padding to override shared defaults.
- Migrated HcmDepartmentPage, HcmNationalityPage and HcmOccupationPage to the organization-owned HcmReferencePage. Their endpoints, validation, permission constants, query keys and saved-view keys remain distinct and unchanged.
- Adopted existing SimpleListPage label defaults in InventOnHandPage, InventTransPage and WorkflowSetupListPage. The workflow composition serves categories, activity types, priorities, data types and controls. Explicit read-only behavior and custom labels remain in place.
- Fixed PageSection composition of object, callback and array sx values. A page's overrides remain local to that instance.

The section change is inherited by existing list/detail and master/detail consumers, including customer, vendor, contact, legal-entity, sales-order and free-text-invoice details. Their business field rendering was retained. No backend code, routes, API adapters, dependencies, validation rules or permission constants were changed.

### Remaining UI work

The review covers source code for every page file; runtime checks sample the changed patterns. Larger invoice, workflow and report workspaces retain their specialized compositions. Literal field colors and some hard-coded labels remain outside the shared section changes. Live-backend correctness and production permission combinations are unverified. The build's large-chunk warning is a separate performance follow-up.

### Verification results

- Final `tsc -b` and Vite production build passed. One combined build invocation exited with a native Windows process error without diagnostics; the isolated compiler and bundler rerun both exited successfully. The large-chunk warning remains.

- Full ESLint run passed. A follow-up lint of changed components and tests also passed.
- Architecture and UTF-8 audits passed; no new layer or icon-import violations.
- 21 tests passed across FastTabs, PageSection, MasterDetailPage, ListDetailsTreePage, ListDetailsPersistence, ListDetailsLocalizedName and HcmReferencePages. Coverage includes controlled expansion, independent state, accessible IDs, nested style isolation, local sx overrides, theme colors, detail persistence and HCM data loading.
- Existing customer and free-text-invoice browser flows passed with intercepted APIs. The customer test initially hit its five-second loading assertion, then passed independently.
- Eight new customer-section browser cases passed: English/Arabic × light/dark × desktop/mobile. Four dark cases were rerun successfully with explicit field-label color checks after screenshot review exposed fixed dark labels.
- Both HCM browser cases passed (desktop/mobile), each visiting department, nationality and occupation lists and verifying their own API adapter and shared New/Edit commands.
- Browser checks use mock API responses; they do not verify a live database or production permissions. The full application unit suite and all 90 pages were not exhaustively exercised.

## Phase 2 continuation (theme centralization and pattern defaults)

Implemented after the entries above, in the same checkout.

### Enterprise surfaces moved onto the theme

`enterpriseUiTokens` defined `canvas`, `surface`, `text`, `mutedText`, `border`, `darkBorder`,
`primary`, `selectedRow`, `selectedBar` and `disabledFill` as literal light-mode hex values, and 42
call sites across `patterns/` and `shared/` consumed them. Dark mode is a user preference and the
primary color is a user preset, so those surfaces ignored both.

[enterpriseSurfaces.ts](../src/shared/constants/enterpriseSurfaces.ts) now derives
`theme.palette.enterprise`
(`canvas`, `surface`, `strongBorder`, `selectedRow`, `selectedBar`, `accent`, `accentStrong`) from
the resolved palette, including the high-contrast preference, and
[createAppTheme.ts](../src/app/theme/createAppTheme.ts) attaches it before component overrides.
Components read it with `enterpriseSurfaces(theme)`, which falls back to the same definition when a
component renders under a plain Material UI theme, because 51 existing test files render shared
components without the application providers. All
42 call sites were migrated to palette values (`text.primary`, `text.secondary`, `divider`,
`primary.contrastText`, `enterprise.*`). Light-mode appearance is preserved; the selected-record
background is now derived from the primary color instead of a fixed blue tint. No color key remains
in `enterpriseUiTokens`.

While migrating, three logical-border properties (`borderInlineStart`, `borderInlineStartColor`,
`borderInlineEnd`) were first given palette-path strings. Those are not Material UI system
properties, so the paths would not have resolved. They were corrected to theme callbacks and to a
`useTheme` value, and [EnterpriseSurfaces.test.tsx](../src/test/shared/EnterpriseSurfaces.test.tsx)
now asserts the rendered CSS for that case.

### Measurement tokens disambiguated

`enterpriseUiTokens` and `uiDensity` both defined `controlHeight`, `gridRowHeight` and
`gridHeaderHeight` with different values for different controls. The enterprise entries were renamed
to `fieldControlHeight`, `detailGridRowHeight` and `detailGridHeaderHeight`, and `toolbarHeight` now
references `uiDensity`. Values are unchanged, so no rendered measurement moved.

### List pages inherit SimpleListPage defaults

Fourteen pages repeated configuration the pattern already resolves: `filterLabel`,
`informationLabel`, `viewLabel: t('common.standardView')`, `locale`, the CRUD label trio, the
`utilities` block, the advanced-filter `addLabel`/`applyLabel`/`resetLabel`/`title`,
`showSearchCommand: true`, the default back command, and `matches` implementations identical to the
pattern fallback. Only page-specific configuration remains; pattern changes now propagate to these
pages. Pages affected: BatchJobActivePeriod, SysBackgroundJobGroup, CustomerList,
CustomerOpenTransactions, CustomerGroupList, PriceDiscSetupList, DocumentCharges, ExchangeRateType,
EcoResDistinctProductList, EcoResProductList, DocuView, OrganizationHierarchy, OrganizationRole,
WfGenericReport.

### Verification of this continuation

- `tsc --noEmit` passed after each step.
- `eslint src` passed with no errors or warnings; six unused declarations exposed by the removals
  were deleted.
- `audit:architecture` and `audit:encoding` passed.
- New tests: [EnterprisePalette.test.ts](../src/test/app/EnterprisePalette.test.ts) (surface keys,
  mode resolution, preset propagation, no colors left in the measurement tokens) and
  [EnterpriseSurfaces.test.tsx](../src/test/shared/EnterpriseSurfaces.test.tsx) (rendered rail
  colors and logical border).
- `npm run format:check` fails, as it did before this work: its file list names two paths that no
  longer exist and twenty unrelated files are unformatted. No file changed here is among them.
- Not verified: dark-mode and color-preset appearance on every one of the 44 list-details pages was
  not inspected visually, and no browser run was repeated for this continuation.

## Setup and quick-create section continuation — 2026-11-19

Before editing, reviewed SetupPage, SetupNavigation, FastTabsDialog and FastTabsDrawer and their consumers. SetupPage owns local draft/saved values, save errors, dirty-state protection and anchor navigation. FastTabsDialog owns initialization/reset, field validation, dependent field changes and save/save-and-open actions; FastTabsDrawer delegates the same form and handles direction-aware placement. Both setup and dialog implementations duplicated accordion headers and body styles already provided by FastTab.

Migrated SetupPage and FastTabsDialog to the existing enterprise FastTab appearance. Field grids, content-padding overrides, default expansion, validation, permission-driven canSave, save callbacks and unsaved-change handling are retained. Setup anchors now use the shared component's root ID. Setup canvas and navigation backgrounds resolve through the active theme. No feature services or backend code changed.

Affected consumers include application settings, inventory/customer parameters, payment setup, customer/sales-order quick create, and product/released-product create drawers. The existing route and feature wrappers require no changes.

Verification for the setup/quick-create continuation:

- Seven focused unit tests passed, including new setup navigation/save and dialog validation/submission regressions, shared FastTab isolation tests and inventory parameter validation.
- Six browser cases passed: customer creation/editing, payment terms and payment methods on desktop and mobile, using intercepted APIs.
- Changed-file ESLint, architecture and encoding audits passed. Vite production bundling passed with the existing large-chunk warning.
- Full visual inspection of all settings and quick-create consumers, and live-backend verification, remain outside this pass.

- Final `tsc -b` passed after correcting test typings and adding a missing `Theme` annotation to the existing TabularDetailPanel style callback. The first compiler process exited with a native Windows error; a subsequent explicit application check supplied the diagnostics. The two new regression tests and lint passed again after those type-only corrections.

### Palette paths routed through the resolver

The enterprise surfaces were initially consumed two ways: imperative reads through
`enterpriseSurfaces(theme)` and palette-path strings (`bgcolor: 'enterprise.canvas'`) on MUI
system props. The string form resolves only when the active theme carries the `enterprise`
palette, so a shared component rendered under a plain Material UI theme — which is how most of
the unit suite renders them — left those surfaces unpainted instead of falling back.

All 23 remaining path strings in `patterns/`, `shared/` and `modules/` now use the resolver as an
`sx` value callback, and sibling palette paths inside the same expression (`text.primary`,
`action.hover`, `primary.contrastText`) were made explicit at the same time. Zero
`'enterprise.*'` strings remain. `src/test/shared/EnterpriseSurfaces.test.tsx` renders
`RightUtilityRail` under a bare `createTheme()` and asserts the fallback paints, so the class of
bug fails loudly.

Verified: `tsc --noEmit` (0), `eslint src` (0 errors, 0 warnings), `audit:architecture` passed,
`vitest run` 659 passed / 2 failed — the two failures (`FreeTextInvoiceList`,
`DataGridRegression`) were already reproduced on a clean checkout and are unrelated to this work.

## Document drawer section continuation — 2026-11-19

Reviewed DocumentTotalsDrawer, DocumentCopyDrawer, AppActionDrawer and their sales-order/free-text-invoice consumers before editing. Totals are supplied by the feature; the drawer's selection control only owns local presentation state. Copy owns source and line selection, clears lines on source changes, and passes selected IDs plus fixed copy options to the feature callback. AppActionDrawer owns placement, scrolling and busy-state close controls.

Both document drawers now compose the existing enterprise FastTab. Calculation-basis and primary source sections remain initially expanded; optional copy-source sections remain collapsed. Retained totals columns, numeric emphasis, grid configuration, field values, content-padding overrides, callbacks, copy options and busy guards. Removed the duplicate totals accordion styles. No calculations, API calls or feature wrappers changed.

Added regression tests for selection persistence across collapse, source-change selection reset, exact copy options, disabled busy controls, initial expansion and caller-supplied total values. The copy tests isolate drawer state using a grid test double; they do not claim to exercise grid internals.

Verification for document drawers: six focused unit tests passed; the existing free-text-invoice browser flow passed on desktop and mobile, including opening totals and confirming the displayed amount. Changed-file ESLint, architecture and encoding audits, and Vite production bundling passed. The existing large-chunk warning remains. Copy-state behavior was tested through the drawer contract; a live-backend copy workflow was not exercised.

### Final regression fixes

- Invoice list and totals amounts now use the selected application language instead of the host locale; financial values and API contracts are unchanged.
- Added an Arabic totals regression and corrected the persisted-grid storage spy to target the actual storage prototype.
- Corrected the formatting-check paths after feature-folder moves and retained the workspace-wide frontend formatting changes.
- The two previously failing regressions passed their focused rerun (9 tests). Full verification is recorded below after completion.

