# Core infrastructure

## Overview

Core owns low-level application infrastructure and generic contracts. It must not depend on shared UI, page patterns, or business modules.

| Folder | Responsibility | Documentation |
| --- | --- | --- |
| `api` | Axios client, interceptors, API/error/response contracts, Query client | [README](api/README.md) |
| `auth` | Session bootstrap, tokens, adapters, guards, hooks | [README](auth/README.md) |
| `configuration` | Vite runtime environment mapping | [README](configuration/README.md) |
| `constants` | Generic app/date/storage constants | [README](constants/README.md) |
| `errors` | Error model, mapping, reporting, and boundary | [README](errors/README.md) |
| `localization` | i18next initialization and language contracts | [README](localization/README.md) |
| `permissions` | Permission identifiers, checks, types, and hook | [README](permissions/README.md) |
| `routing` | Framework-level navigation helpers/service | [README](routing/README.md) |
| `types` | Generic entities, pagination, and selection contracts | [README](types/README.md) |
| `utilities` | Pure date, enum, formatting, object, string, and validation helpers | [README](utilities/README.md) |

[Core guide](../../docs/core.md) · [Architecture boundaries](../../docs/ARCHITECTURE-BOUNDARIES.md)
