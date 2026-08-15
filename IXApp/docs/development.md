# Development and coding guidelines

## Local commands

Run commands from `IXApp`:

```bash
npm run dev
npm run typecheck
npm run lint
npm run test:run
npm run test:e2e:list
npm run build
npm run audit:static
```

`npm run verify` runs the repository's configured formatting subset, static audits, lint, typecheck, unit tests, and production build. `format:check:all` checks the full tree.

## TypeScript and imports

The frontend uses strict TypeScript, ES modules, and the aliases `@app`, `@core`, `@shared`, `@patterns`, `@modules`, `@assets`, `@mocks`, and `@test`. Prefer an alias for cross-layer imports and short relative paths within one feature. Respect the enforced dependency matrix in [Architecture boundaries](ARCHITECTURE-BOUNDARIES.md).

Import Material UI icons by their file path:

```ts
import AddIcon from '@mui/icons-material/Add';
```

The architecture audit rejects the `@mui/icons-material` barrel, forbidden layer edges, direct cross-module imports, circular source dependencies, and unresolved internal imports.

## Naming

- Components, pages, providers, layouts, classes, and exported interfaces: `PascalCase`.
- Hooks: `useXxx` in `camelCase.ts` or `camelCase.tsx`.
- API adapters, services, utilities, and query-key modules: `camelCase`.
- Constants: use the local convention; large public maps use uppercase names such as `ROUTE_PATHS` and `PERMISSIONS`.
- Tests: `*.test.ts(x)` under `src/test`; browser tests: `*.pw.ts` under `e2e`.
- Route and query IDs should be stable strings, not display labels.

## Adding a feature

1. Place business ownership under the appropriate `src/modules/<domain>` folder.
2. Reuse a documented pattern or shared primitive.
3. Define DTO-to-view-model conversion at the API/repository boundary.
4. Add route path, registry entry, permission, navigation, breadcrumbs, and translations when routable.
5. Handle loading, error, empty, validation, success, and permission states.
6. Add focused tests and run checks proportional to the change.
7. Update the relevant docs when public behavior changes.

## Review checklist

- No business rules in `core`, `shared`, or page-pattern components.
- No raw Axios instance in a visual component; module APIs use `apiClient`.
- API envelope failures and missing data are handled.
- Server state, form state, global UI state, and local state use the correct owner.
- User-visible text is translatable and directional layout works in RTL.
- Responsive behavior is checked at desktop and mobile widths.
- Stable row/entity identifiers are used.
- Destructive actions require confirmation.
