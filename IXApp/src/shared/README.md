# Shared frontend library

Shared owns reusable, route-agnostic UI and supporting code. It may depend on core, but should not import business modules or app composition.

| Folder | Responsibility | Documentation |
| --- | --- | --- |
| `components` | Reusable UI subsystems | [README](components/README.md) |
| `constants` | Shared UI/action/record constants | [README](constants/README.md) |
| `hooks` | Generic React hooks | [README](hooks/README.md) |
| `services` | Browser persistence, notifications, preferences, logistics mock data | [README](services/README.md) |
| `types` | Shared action/form/logistics/navigation/page/record contracts | [README](types/README.md) |
| `utilities` | Action, grid, localization, page, and permission helpers | [README](utilities/README.md) |
| `utils` | Focused structural equality helper | [README](utils/README.md) |
| `validation` | Common Zod schemas and messages/helpers | [README](validation/README.md) |

[Shared catalog](../../docs/shared.md) · [Architecture boundaries](../../docs/ARCHITECTURE-BOUNDARIES.md)
