# D365 Finance & Operations — Purchase Order Full-Cycle ERP Alignment

Functional Blueprint + Technical Design + Standard Tables + Business Examples + Test Scenarios

Objective: Build a basic, fully functional Purchase Order (PO) module in your ERP that follows the standard business logic, inventory behavior, financial posting, document lifecycle, and data relationships of Microsoft Dynamics 365 Finance & Operations (D365 F&O).

The implementation will cover the complete Procure-to-Pay (P2P) process, from master-data configuration through purchase order completion, including returns, corrections, partial deliveries, and invoice matching.

## 1. Scope of implementation

Included — Basic ERP Core

Vendor master

Released products

Purchasing configuration

Purchase prices and discounts

Purchase order creation

Purchase order confirmation

Inventory transactions

Product receipt

Partial receipt and delivery

Vendor invoice posting

Inventory and GL accounting

Purchase returns

Corrections and cancellation

Vendor payment and settlement

Status tracking and validation

End-to-end testing

Excluded — Advanced Modules

Advanced WHS / WMS, mobile scanners, license plates, warehouse work, production, manufacturing, BOMs, master planning, transportation management, advanced quality management, and intercompany automation.

We will use the same standard business concepts and data models as D365 F&O, rather than creating unrelated tables or workflows. For example, purchase headers correspond to `PurchTable`, purchase lines to `PurchLine`, and inventory transactions to `InventTrans`.

If your ERP does not already have a required concept, implement its D365-equivalent structure and behavior before introducing a custom design.

## 2. Complete Purchase Order lifecycle

Phase 1 — Core Setup

Vendor · Product · Site · Warehouse · UOM · Item model group · Posting profiles

Phase 2 — Create Purchase Order

PO Header + Lines + Prices + Taxes + Dimensions

Phase 3 — Confirm Purchase Order

Confirmation journal · No physical receipt

Phase 4 — Receive Goods

Registration (optional) → Product receipt → Physical inventory increase

Phase 5 — Vendor Invoice

Invoice matching → Financial inventory update → Accounts payable

Phase 6 — Payment and Settlement

Vendor payment journal → Settlement → Close liability

Phase 7 — Close and Reconcile

Remaining quantities · Inventory value · Vendor balance · GL reconciliation

Alternative branches: change order, underdelivery, overdelivery, return, product-receipt correction, invoice correction, and cancellation.

Microsoft distinguishes PO approval, confirmation, physical receipt, and financial invoicing as separate lifecycle events. Their status changes and inventory effects must not be collapsed into one operation.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+2



## 3. Phase-by-phase implementation roadmap

| Phase | Business module                       | Completion requirement                                       |
| ----- | ------------------------------------- | ------------------------------------------------------------ |
| 1     | Master data and purchasing parameters | Vendors, products, dimensions, and posting setup work        |
| 2     | Purchase order creation               | Valid header/lines, totals, and inventory order transactions |
| 3     | Confirmation and changes              | Confirmation history and revision behavior work              |
| 4     | Inventory registration                | Optional non-WHS registration works                          |
| 5     | Product receipt                       | Physical inventory increases correctly                       |
| 6     | Vendor invoice                        | Financial inventory and AP update correctly                  |
| 7     | Payments and settlement               | Vendor liability is cleared correctly                        |
| 8     | Purchase returns                      | Negative quantities and correct inventory issue behavior     |
| 9     | Corrections and exceptions            | Partial, excess, shortage, reversal, and cancellation        |
| 10    | Accounting and costing                | Ledger and inventory valuation reconcile                     |
| 11    | Reporting and audit                   | End-to-end document traceability                             |
| 12    | Test execution                        | All required forms and cases pass                            |

# Phase 1 — Configure purchasing master data

Before creating a PO, the ERP must have all required master data.

## 1.1 Required D365 standard forms and tables

| Configuration               | D365 F&O form / menu                                                   | Main tables                                                           |
| --------------------------- | ---------------------------------------------------------------------- | --------------------------------------------------------------------- |
| Vendor master               | Accounts payable > Vendors > All vendors                               | `VendTable`, `DirPartyTable`                                          |
| Products                    | Product information management > Released products                     | `InventTable`, `EcoResProduct`, `EcoResProductTranslation`            |
| Site                        | Inventory management > Setup > Inventory breakdown > Sites             | `InventSite`                                                          |
| Warehouse                   | Inventory management > Setup > Inventory breakdown > Warehouses        | `InventLocation`                                                      |
| Item model group            | Inventory management > Setup > Inventory > Item model groups           | `InventModelGroup`, `InventModelGroupItem`                            |
| Item group                  | Inventory management > Setup > Inventory > Item groups                 | `InventItemGroup`, `InventItemGroupItem`                              |
| Storage dimension group     | Product information management > Setup > Dimension and variant groups  | `InventDimGroup`, item-to-group assignment metadata                   |
| Units of measure            | Organization administration > Setup > Units > Units                    | `UnitOfMeasure`, `UnitOfMeasureConversion`                            |
| Inventory posting           | Inventory management > Setup > Posting > Posting                       | `InventPosting`                                                       |
| Accounts payable parameters | Accounts payable > Setup > Accounts payable parameters                 | `VendParameters`                                                      |
| Procurement parameters      | Procurement and sourcing > Setup > Procurement and sourcing parameters | `PurchParameters`                                                     |
| Purchase pricing            | Procurement and sourcing > Prices and discounts                        | `PriceDiscTable`                                                      |
| Financial dimensions        | General ledger > Chart of accounts > Dimensions                        | `DimensionAttribute`, `DimensionAttributeValueSet` and related tables |

Paths can vary slightly by application version and enabled features. Some setup values also have shared/global and legal-entity-specific records.

## 1.2 Example master data

DEMO COMPANY: USMF — EXAMPLE VALUES

Vendor

V-0001

ABC Trading

Item

ITEM-001

Cotton Uniform

Site

SITE-01

Warehouse

WH-01

Purchase unit

PCS

Unit price

SAR 50.00

Ordered quantity

100 PCS

VAT example

15%

These are illustrative test records, not a claim that these values already exist in USMF.

### Configure item model group

For `ITEM-001`, use the following baseline:

| Setting                   | Value                       | Meaning                                                                 |
| ------------------------- | --------------------------- | ----------------------------------------------------------------------- |
| Stocked product           | Yes                         | Inventory is tracked                                                    |
| Inventory model           | FIFO                        | Example cost valuation method                                           |
| Post physical inventory   | Yes                         | Enables physical inventory posting where applicable                     |
| Post financial inventory  | Yes                         | Financial inventory cost posting                                        |
| Receiving requirements    | Yes                         | Require receipt before invoice                                          |
| Registration requirements | No                          | Skip optional registration for basic workflow                           |
| Include physical value    | According to costing policy | Controls how physical costs participate in certain costing calculations |

Important: Item model group requirements, stock tracking, item costing, and ledger posting setup must be independently configured. The item model group alone does not establish every purchasing posting account.

### Phase 1 required tests

- Create a vendor and verify invoice account, currency, payment terms, tax group, and address.
- Create a stocked released product and assign item group, model group, unit, and storage dimensions.
- Configure site/warehouse and confirm the product can transact in them.
- Configure the relevant inventory posting accounts.
- Test valid and invalid item/vendor selections in PO entry.
- Confirm that missing mandatory setup produces clear validation errors rather than silent defaults.

# Phase 2 — Purchase order creation

## 2.1 Main D365 forms

Navigation: Procurement and sourcing > Purchase orders > All purchase orders > New.

D365 uses two primary tables for the purchase order:

- `PurchTable`: header
- `PurchLine`: line details

Additional tables include `InventTransOrigin`, `InventTrans`, `InventDim`, and pricing/tax/charge-related tables.

## 2.2 Example purchase order

PURCHASE ORDER

# PO-0001001

Open order

Vendor

V-0001 · ABC Trading

Currency

SAR

Site / Warehouse

SITE-01 / WH-01

Requested receipt

28 Oct 2026

| Item     | Qty     | Price | Amount   |
| -------- | ------- | ----- | -------- |
| ITEM-001 | 100 PCS | 50.00 | 5,000.00 |

Subtotal

5,000.00 SAR

VAT 15%

750.00 SAR

Total

## 5,750.00 SAR

## 2.3 Header fields — `PurchTable`

| Field              | Example              | Purpose                                       |
| ------------------ | -------------------- | --------------------------------------------- |
| `PurchId`          | PO-0001001           | Unique purchase order ID                      |
| `OrderAccount`     | V-0001               | Ordering vendor                               |
| `InvoiceAccount`   | V-0001               | Vendor receiving invoice liability            |
| `PurchName`        | ABC Trading          | Order name                                    |
| `PurchStatus`      | Backorder            | Operational document status                   |
| `DocumentState`    | Approved / Confirmed | Approval-confirmation status where applicable |
| `CurrencyCode`     | SAR                  | Currency                                      |
| `PurchPoolId`      | Optional             | Purchase order pool                           |
| `Payment`          | NET30                | Payment terms                                 |
| `DeliveryDate`     | 2026-10-28           | Delivery date                                 |
| `InventLocationId` | WH-01                | Default warehouse                             |
| `DefaultDimension` | RecId                | Financial dimension set                       |

The header also contains other delivery, tax, address, purchasing, and administrative fields. The actual relationship between location, dimensions, and header defaults must follow standard initialization behavior.

## 2.4 Line fields — `PurchLine`

| Field                   | Example             | Purpose                               |
| ----------------------- | ------------------- | ------------------------------------- |
| `PurchId`               | PO-0001001          | Header reference                      |
| `LineNumber`            | 1.00                | Display line number                   |
| `ItemId`                | ITEM-001            | Purchased item                        |
| `PurchQty`              | 100                 | Quantity in purchase unit             |
| `PurchUnit`             | PCS                 | Purchasing unit                       |
| `PurchPrice`            | 50.00               | Purchase price per price unit         |
| `PriceUnit`             | 1                   | Price quantity basis                  |
| `LineAmount`            | 5,000.00            | Net line amount                       |
| `InventDimId`           | Dimension reference | Inventory dimensions                  |
| `InventTransId`         | Generated ID        | Inventory transaction reference       |
| `RemainPurchPhysical`   | 100                 | Physical remainder in purchase unit   |
| `RemainPurchFinancial`  | 100                 | Financial remainder in purchase unit  |
| `RemainInventPhysical`  | 100                 | Physical remainder in inventory unit  |
| `RemainInventFinancial` | 100                 | Financial remainder in inventory unit |
| `DefaultDimension`      | RecId               | Line financial dimensions             |

The remaining quantities use different units. This matters when a vendor sells in cartons but your ERP stocks individual pieces.

## 2.5 Inventory transaction creation

For an approved PO with a stocked item, the standard process creates an inventory receipt transaction.

| Table               | Example state                            |
| ------------------- | ---------------------------------------- |
| `PurchTable`        | One header                               |
| `PurchLine`         | One line with qty 100                    |
| `InventTransOrigin` | Origin associated with PO line           |
| `InventTrans`       | Inbound qty +100, receipt status Ordered |
| `InventDim`         | Site SITE-01, warehouse WH-01            |

Inventory effect at PO creation:

| Quantity measure      | Before | After |
| --------------------- | ------ | ----- |
| Physical on-hand      | 0      | 0     |
| Ordered inbound       | 0      | 100   |
| Received              | 0      | 0     |
| Financially purchased | 0      | 0     |

No physical inventory increase occurs simply because an approved PO was created. With change management enabled, a draft PO is a distinct state and inventory transactions are created when it reaches the appropriate approval state.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+1



### Validation cases

1. Vendor does not exist: reject.
2. Item does not exist or is not released to the company: reject.
3. Required storage dimension missing: reject.
4. Invalid warehouse/site relationship: reject.
5. Zero quantity: prevent unintended stocked-item order creation.
6. Missing purchase price: apply configured warnings or business rules.
7. Vendor blocked for purchasing: enforce the vendor block.
8. Purchase unit differs from inventory unit: apply correct conversion.
9. Item price modified: recalculate net amount, discount, charges, and tax.
10. Multiple PO lines for the same item but different warehouses: keep separate inventory dimensions and quantities.

# Phase 3 — Purchase order confirmation

## 3.1 Business behavior

Navigation: All purchase orders > select PO > Purchase > Confirm.

Confirmation represents the vendor-agreed version of the order.

D365 maintains a history of confirmed documents. If a PO is revised and confirmed again, a new confirmation journal captures that version.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+1



## 3.2 Impacted tables

| Table                          | Expected impact                                              |
| ------------------------------ | ------------------------------------------------------------ |
| `PurchTable`                   | Confirmation/document state changes                          |
| `PurchLine`                    | Current order line data retained                             |
| `PurchPurchOrderJour`          | New confirmation journal header                              |
| `PurchPurchOrderTrans`         | Snapshot of confirmed lines                                  |
| `InventTrans`                  | No physical receipt merely from confirmation                 |
| Accounting distribution tables | May be affected according to configured accounting processes |

### Example

PO-0001001:

- Item: ITEM-001
- Quantity: 100
- Price: SAR 50
- Confirmed amount: SAR 5,000 excluding VAT

After confirmation, inventory still shows 100 ordered and 0 physically received.

Now the vendor negotiates a new unit price of SAR 48.

With the applicable change-management rules, the user revises the PO and reconfirms.

| Version        | Quantity | Unit price | Net amount |
| -------------- | -------- | ---------- | ---------- |
| Confirmation 1 | 100      | 50         | 5,000      |
| Confirmation 2 | 100      | 48         | 4,800      |

The old confirmation remains in history. The active PO uses the revised price.

Your ERP must not overwrite historical confirmations.

# Phase 4 — Optional inventory registration

For a basic ERP, registration is optional unless required by the item model group.

Registration indicates that goods have arrived or been registered, but it is not the same as posting a product receipt.

D365 supports registration without advanced WHS.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+1



Example: Register 30 of the 100 ordered units.

| Receipt transaction status | Quantity |
| -------------------------- | -------- |
| Registered                 | 30       |
| Ordered                    | 70       |
| Physically received        | 0        |

The `InventTrans` records can be split across statuses and dimensions. Do not assume one `InventTrans` row permanently represents the entire PO line.

For your first implementation, use direct product receipt without registration. Add the standard non-WHS registration behavior as a separately tested option.

# Phase 5 — Product receipt (physical inventory update)

This is one of the most important phases.

## 5.1 Form and action

Navigation: Purchase order > Receive > Product receipt.

A product receipt represents physical receipt of purchased goods, whether all or part of the ordered quantity.

The system supports posting a quantity different from the original PO quantity, and each posting creates a receipt journal.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+1



## 5.2 Main affected tables

| Table                                               | Responsibility                                                       |
| --------------------------------------------------- | -------------------------------------------------------------------- |
| `PurchTable`                                        | Aggregate order status                                               |
| `PurchLine`                                         | Update remaining physical quantity                                   |
| `VendPackingSlipJour`                               | Posted product receipt header                                        |
| `VendPackingSlipTrans`                              | Posted product receipt lines                                         |
| `InventTrans`                                       | Physical inventory update                                            |
| `InventTransOrigin`                                 | Links inventory transaction origin                                   |
| `InventSum`                                         | Aggregated on-hand quantities, maintained by the inventory framework |
| `InventDim`                                         | Site, warehouse, batch, and relevant dimensions                      |
| `InventTransPosting`                                | Inventory posting references, where applicable                       |
| `GeneralJournalEntry`, `GeneralJournalAccountEntry` | Posted GL journal data if receipt ledger posting is enabled          |

## 5.3 Interactive quantity example

Change the quantities to see how a basic PO line progresses through partial receipts and invoices.

PO-0001001 — Inventory simulator

Reset

Ordered quantity

100 PCS

Product received

60 PCS

Vendor invoiced

40 PCS

Price / PCS (SAR)

Remain physical

# 40

PCS to receive

Remain financial

# 60

PCS to invoice

Received value

## 3,000.00

SAR, excluding VAT

Invoiced value

## 2,000.00

SAR, excluding VAT

Invoiced

Received, not invoiced

Not received

This simulator assumes a stocked item, 1:1 unit conversion, receipt-before-invoice policy, no overdelivery, no returns, and no previous starting inventory.

## 5.4 Example: Two partial product receipts

Initial order = 100 PCS.

Receipt 1:

- Product receipt ID: `PR-0001`
- Received: 60 PCS
- Remaining physical: 40 PCS

Receipt 2:

- Product receipt ID: `PR-0002`
- Received: 40 PCS
- Remaining physical: 0 PCS

### Inventory transaction effects

| Stage        | InventTrans receipt status | Qty | Physical on-hand |
| ------------ | -------------------------- | --- | ---------------- |
| Create PO    | Ordered                    | 100 | 0                |
| Post PR-0001 | Received                   | 60  | 60               |
| Outstanding  | Ordered                    | 40  | —                |
| Post PR-0002 | Received                   | 40  | 100              |

The system retains receipt journal records for both postings.

### Physical versus financial quantity

After both receipts but before invoicing:

| Field                             | Expected value |
| --------------------------------- | -------------- |
| `PurchLine.PurchQty`              | 100            |
| `PurchLine.RemainPurchPhysical`   | 0              |
| `PurchLine.RemainPurchFinancial`  | 100            |
| `PurchLine.RemainInventPhysical`  | 0              |
| `PurchLine.RemainInventFinancial` | 100            |

These are logical values in the 1:1 unit-conversion example. The purchase order has been physically received, but it has not been financially completed.

### Product receipt reversal

If PR-0001 was incorrectly posted for 60 units but only 50 arrived, D365 supports correcting the receipt to reduce its posted quantity, subject to later transactions and other restrictions.

Do not update `VendPackingSlipTrans.Qty` or `InventTrans.Qty` directly. Use standard correction/reversal behavior, preserving audit history. Microsoft states that a product receipt correction can reduce quantity; increasing receipt quantity requires another receipt posting.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



# Phase 6 — Vendor invoice and financial inventory update

## 6.1 Business process

Navigation: Purchase order > Invoice > Invoice.

The vendor invoice completes the financial purchase update.

The ERP must handle:

- Invoice header, number, date, currency, and vendor.
- One or multiple PO lines.
- Matching against one or multiple product receipts.
- Price differences.
- Quantity differences.
- Purchase taxes and charges.
- Posting inventory cost.
- Creating accounts payable liability.

D365 allows invoice matching against partial receipts and provides different default quantity selections.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+1



## 6.2 Main tables

| Table                                 | Function                                  |
| ------------------------------------- | ----------------------------------------- |
| `VendInvoiceInfoTable`                | Pending vendor invoice header             |
| `VendInvoiceInfoLine`                 | Pending vendor invoice lines              |
| `VendInvoiceJour`                     | Posted vendor invoice header              |
| `VendInvoiceTrans`                    | Posted vendor invoice lines               |
| `VendInvoicePackingSlipQuantityMatch` | Product receipt/invoice quantity matching |
| `PurchLine`                           | Financial remainder updated               |
| `InventTrans`                         | Financial inventory update                |
| `InventTransPosting`                  | Inventory posting references              |
| `VendTrans`                           | Vendor financial transaction              |
| `TaxTrans`                            | Posted tax transactions                   |
| `GeneralJournalEntry`                 | Ledger journal header                     |
| `GeneralJournalAccountEntry`          | Ledger journal entries                    |

Exact matching and posting records depend on the D365 features, costing methods, and configured validation policy.

## 6.3 Example: Invoice first 60 units

PO amount before tax = SAR 5,000.

Product receipt PR-0001 = 60 PCS.

Invoice INV-0001:

| Field         | Value     |
| ------------- | --------- |
| Quantity      | 60        |
| Unit price    | SAR 50    |
| Net amount    | SAR 3,000 |
| VAT 15%       | SAR 450   |
| Total invoice | SAR 3,450 |

After invoice posting, assuming the remaining 40 have not been received:

| Metric               | Value |
| -------------------- | ----- |
| Original order       | 100   |
| Physically received  | 60    |
| Financially invoiced | 60    |
| Remain physical      | 40    |
| Remain financial     | 40    |

The 60 received/invoiced units are financially updated to Purchased, and the remaining 40 stay Ordered.

## 6.4 GL impact — simplified accounting example

For a stocked item, assume:

- Purchase receipt accrual enabled.
- Physical and financial inventory posting enabled.
- Recoverable input VAT.
- No price variance or additional charges.
- Quantity = 60 at SAR 50.

### At physical receipt

Simplified economic accounting effect:

| Account                                    | Debit | Credit |
| ------------------------------------------ | ----- | ------ |
| Inventory / physical inventory recognition | 3,000 | —      |
| Accrued purchase liability                 | —     | 3,000  |

### At vendor invoice

Simplified net effect including reversal of receipt accrual:

| Account                    | Debit | Credit |
| -------------------------- | ----- | ------ |
| Accrued purchase liability | 3,000 | —      |
| Input VAT recoverable      | 450   | —      |
| Accounts payable — vendor  | —     | 3,450  |

The inventory is converted from physical recognition to financial valuation through the configured inventory posting process.

D365 technical distinction: The actual vouchers may contain additional lines for purchase expenditure un-invoiced, purchase expenditure for product, physical cost clearing, stock variation, accrual reversal, and financial inventory receipt. Do not implement the simplified accounting illustration as a hardcoded two-line posting model. Derive vouchers from posting profiles, cost model, and the posting event.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+1



## 6.5 Invoice price difference

Suppose the order was confirmed at SAR 50 per PCS but the vendor invoice is SAR 52 per PCS.

For 60 PCS:

- Original receipt estimate = SAR 3,000.
- Actual invoice amount = SAR 3,120.
- Difference = SAR 120.

Your ERP should determine whether the difference updates inventory cost, creates an adjustment or variance according to the inventory model and accounting setup.

Do not automatically post every difference to a generic expense account. Costing behavior depends on FIFO, weighted average, moving average, standard cost, and related settings.

# Phase 7 — Vendor payment and settlement

Inventory processing is complete when quantities are received and invoiced. Payment is part of the wider procure-to-pay cycle and should remain separate from PO quantity status.

### D365 forms

Accounts payable > Payments > Vendor payment journal.

### Main tables

| Table                        | Purpose                                    |
| ---------------------------- | ------------------------------------------ |
| `LedgerJournalTable`         | Payment journal header                     |
| `LedgerJournalTrans`         | Payment journal lines                      |
| `VendTrans`                  | Posted vendor transactions                 |
| `VendSettlement`             | Settlement relationships                   |
| `BankAccountTrans`           | Bank account transactions, when applicable |
| `GeneralJournalEntry`        | Posted accounting entry                    |
| `GeneralJournalAccountEntry` | Posted ledger lines                        |

### Example

Vendor invoice INV-0001 total = SAR 3,450.

Payment journal:

- Vendor: V-0001
- Payment: SAR 3,450
- Bank: BANK-001
- Settlement against INV-0001

Simplified posting:

| Account          | Debit | Credit |
| ---------------- | ----- | ------ |
| Accounts payable | 3,450 | —      |
| Bank             | —     | 3,450  |

After full settlement, the invoice's open vendor balance is zero.

A fully invoiced purchase order can have an unpaid vendor invoice. Do not use vendor payment status to calculate `PurchLine.RemainPurchFinancial`.

# Phase 8 — Purchase returns

Purchase returns are not simply negative stock adjustments. They must retain the vendor, PO, invoice, inventory, and financial relationships.

D365 handles vendor returns by using a return purchase order with negative quantities and supports copying from an earlier invoice with marking where appropriate.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



## Example

Originally purchased and invoiced:

- ITEM-001: 100 PCS
- Price: SAR 50
- Inventory: 100 PCS

Return 10 damaged PCS.

Create returned purchase order:

- PO: `RET-0001`
- Vendor: V-0001
- Item: ITEM-001
- Quantity: −10
- Unit price: SAR 50

### Inventory effects

| Step                        | Quantity change              | Resulting physical quantity |
| --------------------------- | ---------------------------- | --------------------------- |
| Original purchase           | +100                         | 100                         |
| Create return PO            | 0                            | 100                         |
| Post return product receipt | −10                          | 90                          |
| Post vendor credit invoice  | 0 additional physical change | 90                          |

### Inventory transaction status

A purchase return is an inventory issue, not an inventory receipt.

| Stage                                  | `InventTrans` issue status |
| -------------------------------------- | -------------------------- |
| Return PO created                      | On order                   |
| Return shipment/product receipt posted | Deducted                   |
| Vendor credit invoice posted           | Sold                       |

The final issue status name `Sold` is a technical inventory transaction status used for financially updated issues, including purchase returns. It does not mean that a customer sale occurred.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



Main impacted tables are again `PurchTable`, `PurchLine`, `InventTrans`, `VendPackingSlipJour/Trans`, and `VendInvoiceJour/Trans`, plus relevant vendor and accounting transactions.

Test a return created from the original invoice as well as a valid manual return. Where supported, verify marking/cost linkage to the original purchase.

# Phase 9 — Exceptions and business cases

A D365-aligned ERP must support more than the straightforward happy path.

## 9.1 Full test scenario catalog

Use this checklist as the implementation acceptance matrix. Mark scenarios when both their business behavior and database effects have passed.

## 0 / 56 passed

Core procurement test coverage

Clear tests

Order creation (0/12)Confirmation and changes (0/8)Receiving (0/12)Invoicing and accounting (0/12)Returns, payment, edge cases (0/12)

PO-01

New stocked-item purchase order

PO-02

Multiple items in one PO

PO-03

Same item in multiple lines

PO-04

Different warehouse per line

PO-05

Different site per line

PO-06

Different receipt dates

PO-07

Purchase unit differs from inventory unit

PO-08

Line discount and header discount

PO-09

Taxable and exempt lines

PO-10

Vendor-specific purchase price

PO-11

Multiple currencies and exchange rates

PO-12

Additional allocated charges

&#x20;Copy test checklist and results

For each of these cases, verify the form-level result and all relevant persisted records. A successful notification alone is not sufficient evidence of correct inventory or accounting behavior.

## 9.2 Key exception rules

| Scenario                            | Required behavior                                                       |
| ----------------------------------- | ----------------------------------------------------------------------- |
| Receipt qty less than ordered       | Keep outstanding physical remainder                                     |
| Receipt qty exceeds ordered         | Check overdelivery tolerance and permitted settings                     |
| Vendor invoice differs from receipt | Apply configured invoice matching and discrepancy rules                 |
| Price changes after receipt         | Keep posted receipt history; account for invoice price variance         |
| Product receipt is canceled         | Post reversal history and restore appropriate quantity/status           |
| PO line is canceled                 | Remove outstanding quantity without reversing quantities already posted |
| User tries to receive twice         | Prevent duplicate posting of same transaction request                   |
| Two users receive same remainder    | Revalidate current quantity at commit time                              |
| Wrong vendor account                | Prevent invalid invoice/vendor relationship                             |
| Different UOM on purchase line      | Convert correctly for inventory accounting                              |
| Non-stocked item                    | Process procurement and expense without treating it as stocked on-hand  |
| Return already invoiced goods       | Use negative PO/credit process, preserving source relationship          |

Important: Overdelivery and invoice controls are settings-driven. The application must not assume that every case is always permitted or always blocked.

# Phase 10 — Inventory accounting and financial reconciliation

This phase verifies that procurement transactions produce consistent inventory values and accounting postings.

## 10.1 Financial status versus physical status

| PO event                     | Physical inventory             | Financial inventory           | GL impact                                      |
| ---------------------------- | ------------------------------ | ----------------------------- | ---------------------------------------------- |
| PO creation/approval         | No receipt                     | None                          | Normally none in basic PO scenario             |
| PO confirmation              | No receipt                     | None                          | Normally none in basic PO scenario             |
| Registration only            | Registered status              | None                          | Normally no inventory GL posting               |
| Product receipt              | Increases                      | Not yet finally invoiced      | Possible physical/accrual postings             |
| Vendor invoice               | No additional physical receipt | Updates inventory financially | AP, tax, inventory and clearing postings       |
| Vendor payment               | Unchanged                      | Unchanged                     | AP and bank                                    |
| Purchase return shipment     | Decreases                      | Not yet financially reversed  | Physical/clearing postings according to setup  |
| Vendor return credit invoice | Unchanged physically           | Financial issue finalized     | Vendor credit and inventory/expense adjustment |

### Critical rule

The `InventTrans` receipt statuses typically move through:

Ordered

Registered

Received

Purchased

Registration can be skipped.

These statuses can coexist on separate inventory transaction records for one PO line when only some quantity has been processed.

## 10.2 Inventory posting accounts to configure

At minimum, inspect these D365 purchase-related posting types:

- Purchase, inventory receipt (financial cost)
- Cost of purchased materials received (physical inventory)
- Purchase accrual
- Purchase expenditure, un-invoiced
- Purchase expenditure for product
- Stock variation, where relevant
- Relevant tax accounts
- Vendor summary account from vendor posting profiles

The exact account types and posting sequence differ by item model group, costing method, and configuration. Microsoft's posting-profile documentation includes examples with additional physical and financial clearing entries.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+1



## 10.3 Accounting reconciliation controls

At the end of every test batch, validate:

Quantity control

\\[ \text{Net physical stock movement} = \text{Receipts}-\text{Returns} \\]

Purchase line control (simple normal PO with no canceled remainder):

\\[ \text{Remain physical} = \text{Ordered}-\text{Net received} \\]

\\[ \text{Remain financial} = \text{Ordered}-\text{Net invoiced} \\]

Accounting controls

- Total journal debits must equal total journal credits in accounting currency.
- Physical accruals should reconcile to eligible received-but-not-invoiced quantities and values, subject to posting configuration.
- Financial inventory cost should reconcile to posted inventory transaction amounts, accounting for adjustments and valuation methodology.
- Vendor subledger liabilities should reconcile to their control accounts.
- Input VAT should reconcile to tax transactions and relevant ledger accounts.

Do not assume every remaining-quantity field can be reconstructed from a simplistic sum of original quantity minus journal quantity in all cases. Delivery remainder, underdelivery closure, unit conversion, cancellations, and corrections affect the logic.

# Phase 11 — Technical database architecture

## 11.1 Core D365 table relationships

Purchase order and inventory data model

Conceptual relationship map, not a literal physical foreign-key diagram. Posted receipt/invoice line associations, inventory origins, and journal keys must be implemented with their actual D365 composite keys and source references.

## 11.2 Important relationship rules

| Parent                | Child                  | Relationship                                                   |
| --------------------- | ---------------------- | -------------------------------------------------------------- |
| `VendTable`           | `PurchTable`           | Ordering vendor account and legal entity                       |
| `PurchTable`          | `PurchLine`            | `PurchId` and company scope                                    |
| `PurchLine`           | `InventTransOrigin`    | Inventory transaction ID/origin relationship                   |
| `InventTransOrigin`   | `InventTrans`          | Origin `RecId`                                                 |
| `PurchLine`           | `InventDim`            | `InventDimId`                                                  |
| `PurchTable`          | `PurchPurchOrderJour`  | Purchase order ID and confirmation version                     |
| `VendPackingSlipJour` | `VendPackingSlipTrans` | Posted receipt journal relationship                            |
| `VendInvoiceJour`     | `VendInvoiceTrans`     | Posted invoice journal relationship                            |
| `VendInvoiceTrans`    | `VendTrans`            | Financial voucher/vendor context; not a simple one-column join |
| `InventTrans`         | `InventTransPosting`   | Inventory posting association                                  |
| `VendTrans`           | `VendSettlement`       | Vendor transaction settlement references                       |

Important: Do not join every D365 table solely on `PurchId`, `Voucher`, or `InvoiceId`. Many are nonunique in isolation. Use company, record identifiers, inventory origin references, voucher context, and the documented composite relations.

## 11.3 Technical fields to verify at each posting

| Stage            | Main record                 | Required checks                                            |
| ---------------- | --------------------------- | ---------------------------------------------------------- |
| Create PO        | `PurchTable`                | PurchId, OrderAccount, status, currency                    |
| Add line         | `PurchLine`                 | ItemId, quantity, unit, price, dimensions, remainders      |
| Approve PO       | `InventTrans`               | Ordered inbound quantities                                 |
| Confirm PO       | `PurchPurchOrderJour/Trans` | Confirmation version and immutable snapshot                |
| Register         | `InventTrans`               | Registered quantity if used                                |
| Product receipt  | `VendPackingSlipJour/Trans` | Correct receipt document and posted quantity               |
| Physical update  | `InventTrans`               | Received quantity, physical date/voucher and cost fields   |
| Invoice          | `VendInvoiceJour/Trans`     | Invoice number, quantities, amount, tax                    |
| Financial update | `InventTrans`               | Purchased quantity, financial date/voucher and cost fields |
| Vendor liability | `VendTrans`                 | Financial liability                                        |
| Payment          | `LedgerJournalTrans`        | Payment voucher                                            |
| Settlement       | `VendSettlement`            | Settlement amounts and references                          |

Be careful with database enum fields such as `PurchStatus` and `StatusReceipt`: your implementation should use named enums, not assume that enum integer values are stable or interchangeable between systems.

# Phase 12 — Complete implementation and validation

## 12.1 Recommended form implementation order

Build and test the following forms in sequence.

| Sequence | ERP form                 | Required functionality                      |
| -------- | ------------------------ | ------------------------------------------- |
| 1        | Vendor setup             | Create, edit, block, search vendor          |
| 2        | Released product         | Validate purchasing and inventory setup     |
| 3        | Purchase order list      | Filter/search by vendor, status, date       |
| 4        | Purchase order details   | Header and editable lines                   |
| 5        | Purchase order totals    | Prices, charges, discounts, taxes           |
| 6        | Purchase confirmation    | Confirm/reconfirm with history              |
| 7        | Inventory transactions   | Display ordered, received, purchased status |
| 8        | Product receipt posting  | Full/partial receipt                        |
| 9        | Product receipt journal  | Review/correct/cancel receipts              |
| 10       | Vendor invoice           | Create, match, validate, post               |
| 11       | Vendor invoice journal   | Review vouchers and posted invoices         |
| 12       | Purchase returns         | Negative orders and shipment posting        |
| 13       | Vendor payments          | Journal and settlement                      |
| 14       | Inventory on-hand        | Physical, ordered, available quantities     |
| 15       | Audit and reconciliation | PO-to-invoice-to-ledger trace               |

## 12.2 Mandatory end-to-end acceptance example

Use a single PO to demonstrate the entire lifecycle.

PO-0001001 · ITEM-001 · 100 PCS @ SAR 50 · 15% VAT

| Step | Transaction               | Physical stock | Physical remainder | Financial remainder |
| ---- | ------------------------- | -------------- | ------------------ | ------------------- |
| 1    | Create and approve PO     | 0              | 100                | 100                 |
| 2    | Confirm PO                | 0              | 100                | 100                 |
| 3    | Product receipt 60        | 60             | 40                 | 100                 |
| 4    | Invoice 60                | 60             | 40                 | 40                  |
| 5    | Product receipt 40        | 100            | 0                  | 40                  |
| 6    | Invoice 40                | 100            | 0                  | 0                   |
| 7    | Vendor payment            | 100            | 0                  | 0                   |
| 8    | Return 10 and post credit | 90             | 0 on original PO   | 0 on original PO    |

This example assumes zero starting stock, no other transactions, and 1:1 purchase/inventory units. The return is a separate return PO.

At completion, validate that:

- The original PO is fully invoiced.
- The two product receipts total 100.
- The two positive vendor invoices total SAR 5,750 including VAT.
- The return reduces physical stock by 10.
- The return credit document is SAR 575 including VAT, assuming the original price and tax treatment.
- Inventory documents, vendor subledger, and GL transactions are traceable.
- Original confirmation, receipt, and invoice records remain auditable.

## 13. Exact rules for building an ERP aligned with D365

Use the following as non-negotiable implementation requirements:

1\. Separate document state from inventory transaction state. A confirmed PO is not necessarily physically received, and a received PO is not necessarily invoiced.

2\. Separate physical quantity from financial quantity. Both must be independently traceable.

3\. Support multiple `InventTrans` records per PO line. Partial receipt, invoice progress, and dimension splits must work.

4\. Keep posted document history immutable. Use corrections/reversals rather than modifying historical posted records.

5\. Follow inventory dimensions. At minimum, site and warehouse; add batch or serial only if the business uses them.

6\. Use configurable accounting rules. Do not hardcode inventory, accrual, VAT, or vendor control accounts.

7\. Respect purchase/inventory UOM conversions. Purchase quantity and inventory quantity are not always equal.

8\. Calculate document status from the correct lifecycle state. Never infer it only from payment state or physical on-hand.

9\. Implement concurrent transaction protection. Posting must be atomic and validated against the current remaining quantity.

10\. Make postings idempotent. Retried operations must not duplicate receipts, invoices, inventory movements, or GL entries.

11\. Validate complete document chains. PO → confirmation → product receipt → vendor invoice → inventory transaction → ledger → settlement.

12\. Match standard D365 behavior before extending it. Do not introduce independent purchasing entities when an equivalent D365 concept already exists.

## 14. Recommended first implementation milestone

Start with the smallest complete process that proves the ERP's purchasing architecture.

Milestone 1 — Stocked-item purchase to invoice

Implement one vendor, one stocked product, one site, one warehouse, one PO, partial product receipts, and a matched vendor invoice.

Definition of done

- PO header and lines save and reload correctly.
- Approval creates the expected ordered inventory transactions.
- Confirmation generates the correct versioned journal.
- Receipt of 60/100 updates physical stock and the physical remainder.
- Invoice of 60/100 updates financial inventory and the financial remainder.
- Receipt and invoice of the remaining 40 complete the order.
- Posted vouchers reconcile with inventory and AP.
- Every form supports view, validation, and audit of the corresponding standard records.

Once this milestone passes, add advanced purchase cases one by one: returns, reversals, changes, unit conversions, extra charges, currency differences, and exceptions.

### Official Microsoft references

These are the primary baseline references to verify against the specific D365 application version being aligned:

- [Purchase order creation](https://learn.microsoft.com/en-us/dynamics365/supply-chain/procurement/purchase-order-creation?utm_source=chatgpt.com)
- [Purchase order approval and confirmation](https://learn.microsoft.com/en-us/dynamics365/supply-chain/procurement/purchase-order-approval-confirmation?utm_source=chatgpt.com)
- [Product receipt process](https://learn.microsoft.com/en-us/dynamics365/supply-chain/procurement/product-receipt-against-purchase-orders?utm_source=chatgpt.com)
- [Vendor invoice processing](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/vendor-invoices-overview?utm_source=chatgpt.com)
- [Inventory transaction states and posting profiles](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/inventory-posting-profiles?utm_source=chatgpt.com)

Implementation principle: A phase is complete only when its forms, field defaults, validations, inventory transitions, posting records, document history, accounting entries, and positive/negative test cases work together consistently. This produces a purchasing module aligned with the D365 F&O core rather than merely a purchase order entry screen.