# Finance foundation

Shared finance setup used by every other finance module. One folder per feature,
each with its own `api/`, `pages/` and `components/`:

| Folder           | Feature                                                                                                     |
| ---------------- | ----------------------------------------------------------------------------------------------------------- |
| `currency/`      | Currencies (`Currency`).                                                                                    |
| `exchange-rate/` | Exchange rate types and exchange rates.                                                                     |
| `charges/`       | Charges codes (`MarkupTable`) and document charges (`MarkupTrans`) shown on documents such as sales orders. |
| `tax/`           | Sales tax authorities, settlement periods, sales tax codes, sales tax groups and item sales tax groups.     |
| `contacts/`      | Contact persons and account contacts (shared by customers and vendors).                                     |

Shared by the module: `queries/` (query keys), `previews/` (D365 record previews),
`routes/` (route paths). A new feature gets its own folder with the same layout.

[Modules](../../README.md) · [API and state](../../../../docs/api-and-state.md) · [Simple-list pattern](../../../patterns/simple-list/README.md)
