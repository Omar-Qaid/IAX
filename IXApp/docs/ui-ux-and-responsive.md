# UI/UX, accessibility, and responsive design

## Visual system

The UI uses Material UI 9 with a theme created by `app/theme/createAppTheme.ts`. `usePreferenceStore` controls light/dark mode, compact/comfortable density, contrast, color preset, font family, font size, zoom, RTL override, and navigation layout. Prefer theme palette and spacing values; established enterprise pattern tokens such as `d365Tokens`, `uiDensity`, and Process Builder tokens are also valid within their owning systems.

The default experience is deliberately dense: small controls, compact grid rows, restrained borders, and low-elevation surfaces. Reuse `PageContainer`, `PageHeader`, `ActionPane`, `PageContent`, patterns, and shared fields before creating a custom layout.

## Responsive behavior

MUI default breakpoints are used (`sm` 600, `md` 900, `lg` 1200, `xl` 1536).

- The shell switches to mobile behavior below `md`.
- `FormColumn` defaults to 12/6/4/3 columns across `xs`/`sm`/`md`/`lg`.
- `DataGrid` renders `DataGridMobileBody` below `md`; desktop uses the header/body grid.
- `ListDetailsLayout` supports responsive stacking, pane visibility, and optional persisted resizing.
- `SetupPage` stacks navigation and content on small screens and uses a side-by-side layout from `md`.
- Process Builder converts its three-pane layout to stacked regions on compact screens and has dedicated browser coverage.
- Tabs should use `variant="scrollable"` and `scrollButtons="auto"` when labels may overflow.

Avoid fixed viewport assumptions inside module pages. Let the shell own the viewport and make page content use `minHeight: 0` and controlled overflow where necessary.

## Localization and RTL

English and Arabic resources are loaded by `core/localization/i18n.ts`. Language detection checks local storage then the browser. `ThemeProvider` applies language, direction, density, contrast, and zoom to the document. Use `useAppTranslation()` for user-visible strings and logical CSS properties (`marginInlineStart`, `borderInlineEnd`, `insetInlineStart`) for directional layouts.

## Accessibility

- The shell exposes a keyboard-focusable skip link to `#main-content`.
- Use semantic headings and landmarks provided by page components.
- Icon-only controls require translated `aria-label` text.
- Prefer role/name queries in tests; this exercises the accessible contract.
- Dialogs must retain MUI focus handling and a discoverable title.
- Grid and lookup keyboard behavior must remain available alongside pointer interaction.
- Loading, empty, error, and access-denied states must be explicit rather than blank content.

## Interaction rules

- Put page commands in `ActionPane`; use confirmation dialogs for destructive operations.
- Display pending state and prevent duplicate async submissions.
- Use notifications for operation outcomes and inline/form summaries for correctable validation errors.
- Keep stable row IDs and clear selection states.
- Use specific MUI icon imports such as `@mui/icons-material/Add`, never the icon barrel.
