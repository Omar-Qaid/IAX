# Frontend documentation audit

This file records the documentation review completed against the current frontend. It is not a backlog or an alternative architecture specification.

## Findings corrected

- Removed references to nonexistent app stores/configuration files and mock repository folders.
- Replaced machine-specific `file:///` links with repository-relative links.
- Corrected the stack to React 19, TypeScript 6, Vite 8, Material UI 9, custom `DataGrid`, TanStack Query 5, Zustand 5, React Hook Form, Zod, i18next, Vitest, and Playwright.
- Distinguished API-backed, mock-backed, and local-data module pages.
- Reclassified pattern folders by their actual implementation state; several previously documented production patterns are empty or partial scaffolds.
- Documented the page registry, route metadata, permission guards, provider order, browser/session storage, query defaults, error path, responsive grid, and Process Builder integration.
- Removed examples that referenced nonexistent service methods, props, files, or route conventions.

## Current risks to keep visible

- Several Accounts Receivable and dashboard pages intentionally use static/mock or local data; documentation must not present them as API-backed.
- `useUnsavedChanges` protects browser unload, while in-app record-switch protection is pattern-specific rather than universal.
- Not every shared field obtains React Hook Form context in the same way; consult the field guide and public types.
- Empty pattern files must not be treated as available templates.
- The error reporter is console-only until a telemetry adapter is configured.

## Folder-by-folder coverage

The follow-up source inventory reviewed all current directories under `src`. Every one of the 114 source directories now contains a `README.md`, including:

- architectural roots and app/core subsystems;
- each business module and its API, adapter, component, page, query, service, state, type, validation, lookup, and route partitions where present;
- every page-pattern folder, with implementation maturity stated explicitly;
- every shared component subsystem, including the DataGrid body/header/hooks/sidebar internals;
- shared hooks, services, types, constants, utilities, and validation;
- test ownership folders.

Parent READMEs link to their children, [`src/README.md`](../src/README.md) maps the complete source tree, and the main [documentation index](README.md) links architecture guides to source-level documentation. The folder audit found no source directory without an in-place README and no non-Markdown change.

The maintained documentation entry point is [README.md](README.md).
