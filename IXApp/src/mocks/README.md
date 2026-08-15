# Mock data

The mocks root currently owns shared datasets only. Feature mock adapters live with `administration`, `organization`, and core authentication; logistics mock records live in a shared service.

| Folder | Content | Documentation |
| --- | --- | --- |
| `data` | Currency, customer group, customer, and sales-order records | [README](data/README.md) |

Mock mode is selected through `VITE_ENABLE_MOCK_API`, but only features with an adapter/dataset can use it.

[Mock integration](../../docs/mocks.md) · [API and state](../../docs/api-and-state.md)
