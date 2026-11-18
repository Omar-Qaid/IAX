# Inventory

Inventory setup, products and transactions (D365 Inventory management / Product
information management). One folder per feature, each with its own `api/`,
`pages/` and `components/`:

| Folder | Feature |
| --- | --- |
| `products/` | Products, product variants (distinct products) and released products, product release. |
| `dimension-groups/` | Storage and tracking dimension groups. |
| `item-groups/` | Item groups (with posting) and item model groups. |
| `warehouses/` | Sites, warehouses and warehouse locations. |
| `units-of-measure/` | Units and unit conversions. |
| `journals/` | Inventory journal names and inventory journals. |
| `inquiries/` | On-hand inventory and inventory transactions. |
| `setup/` | Inventory parameters and inventory posting. |
| `docs/` | Implementation plan, checklist and phase status notes. |

Shared by the module: `previews/` (D365 record previews) and `routes/`.

[Modules](../../README.md)
