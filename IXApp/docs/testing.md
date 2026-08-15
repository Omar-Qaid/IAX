# Testing strategy (`src/test` and `e2e`)

## Tooling

Vitest 4 runs in jsdom with React Testing Library, jest-dom, and user-event. `vite.config.ts` loads `src/test/setupTests.ts`, which initializes localization and supplies a `ResizeObserver` fallback. `src/test/testUtils.tsx` exports a custom render wrapped in `MemoryRouter` and `AppProviders`.

The repository currently organizes tests by `app`, `core`, `shared`, `patterns`, `modules`, and `mocks`. Process Builder has UI/store and API-integration suites. Playwright runs desktop Chromium and Pixel 7 projects against Vite and stores traces/screenshots on failure.

## Commands

```bash
npm run test:run
npm run test:coverage
npm run test:e2e
npm run test:e2e:list
npm run typecheck
npm run lint
npm run audit:static
npm run build
```

Coverage uses V8 with text, HTML, and JSON-summary reporters. Current configured minimums are 20% statements/functions/lines and 15% branches; they are a floor, not a target for new code.

## Testing conventions

- Render through `@test/testUtils` unless a test intentionally needs a custom provider/router.
- Reset module stores, browser storage, mocks, and query state in test setup when state can leak.
- Prefer accessible role/name queries and user-level interactions.
- Mock API adapters at their module boundary and assert DTO/envelope integration for complex saves.
- Test loading, error, empty, permission, validation, success, and responsive branches that the feature exposes.
- Use stable deterministic fixtures; avoid array-index identity assertions.

## Known environment considerations

TanStack Virtual sees zero-sized containers in jsdom. Grid and lookup implementations include a direct-row fallback when no virtual items are measured; preserve it. Import MUI icons through specific paths because the architecture gate forbids the icon barrel. Browser-only behavior such as responsive shell layout and drag interactions may require Playwright coverage in addition to jsdom tests.
