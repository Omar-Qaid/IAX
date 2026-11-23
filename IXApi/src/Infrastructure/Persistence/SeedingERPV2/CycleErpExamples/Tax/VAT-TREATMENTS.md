# VAT treatments: standard, zero-rated, exempt, out of scope

The standard D365 sales tax engine applies the tax codes that two groups share: the party's sales tax group (`TaxGroupData`) and the item's sales tax group (`TaxOnItem`). A zero tax amount alone does not say which treatment applied. Check the code, the direction and the base on the posted `TaxTrans`.

## Seeded setup (`TaxVatCategorySeeder`)

| Treatment | Tax code | Rate (`TaxData`) | `TaxGroupData` row in group `APV2` | Item tax group (`TaxOnItem`) | Posted `TaxTrans` |
|---|---|---:|---|---|---|
| Standard | `APV2-15` | 15 | normal | `APV2`, holding `APV2-15` | Outgoing or Incoming, tax = 15% of the base |
| Zero-rated | `APV2-0` | 0 | normal (in `APV2` and `ARV2`) | `APV2-ZERO`, holding `APV2-0` | Outgoing or Incoming, base recorded, tax 0 |
| Exempt | `APV2-EX` | 0 | `ExemptTax = Yes`, `TaxExemptCode = VATEX-SA-29` | `APV2-EXEMPT`, holding `APV2-EX` | `TaxExemptSales` or `TaxExemptPurchase`, base recorded, tax 0 |
| Out of scope | none | none | none | `APV2-OOS`, holding no code | no row |

All identifiers come from `Modules/Tax/TaxSeedCatalog`, so the codes are defined once and shared:

- the vendor group `APV2` and the customer group `ARV2` both hold `APV2-0` and `APV2-EX`;
- the inventory cycle's item group `IV2-ZERO` holds `APV2-0` instead of defining a second zero-rated code;
- the posting group `APV2` carries the payable account as well as the input account, so a shared code posts on either side.

`TaxIntegrityValidator` also checks references from the other modules:

- every tax group set on `CustTable`, `CustGroup`, `VendTable` and `VendGroup` exists;
- every item tax group set on `InventTableModule` and `MarkupTable` exists;
- every code in a group is defined;
- a code reachable by customers has a payable account, and one reachable by vendors has an input account.

An exempt code applies at rate zero even if its rate row says otherwise. Settlement places an exempt sale on the sales-tax-payable side and an exempt purchase on the input-tax side, both with a zero balance. `TaxIntegrityValidator` rejects:

- an exempt row without an existing exemption reason;
- a zero-rated or exempt code with a non-zero rate;
- any code attached to `APV2-OOS`.

## Acceptance example: mixed sales invoice (prices exclude tax, SAR)

| Line | Qty × price | Discount | Base | Item tax group | Tax |
|---|---:|---:|---:|---|---:|
| Stocked item | 10 × 100 | 100 | 900 | `APV2` | 135 |
| Zero-rated supply | 2 × 200 | 0 | 400 | `APV2-ZERO` | 0 |
| Exempt service | 1 × 300 | 0 | 300 | `APV2-EXEMPT` | 0 |
| **Total** | | | **1,600** | | **135** |

Expected after posting:

- `CustInvoiceJour`: `SalesBalance` 1,600, `SumTax` 135, `InvoiceAmount` 1,735. `CustTrans.AmountCur` is 1,735.
- `TaxTrans` (sales amounts are negative), three rows:
  - `APV2-15`, Outgoing: base −900, tax −135;
  - `APV2-0`, Outgoing: base −400, tax 0;
  - `APV2-EX`, TaxExemptSales: base −300, tax 0.
- Ledger: debit customer 1,735; credit revenue 1,600; credit VAT payable 135. Only the 135 reaches a tax account.
- A line in `APV2-OOS` adds revenue but no `TaxTrans`.

The purchase side mirrors this with positive amounts and `Incoming` / `TaxExemptPurchase` directions. With full recovery, the vendor balance is 1,150 and the input tax 150.

## Out of basic scope

These are not implemented: refused or approximated features would be worse than none.

- partial or non-deductible input tax and reverse charge;
- import VAT;
- manual adjustment of the actual tax amount (`SourceRegulateAmountCur`);
- totals-based rounding;
- ZATCA e-invoice categories and external codes.
