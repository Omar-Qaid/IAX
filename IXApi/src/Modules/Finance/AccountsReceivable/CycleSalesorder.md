# D365 Finance & Operations — Complete Sales Order Cycle Blueprint

Core ERP implementation | Sales and marketing | Accounts receivable | Inventory management

This blueprint defines how to build and validate a basic Sales Order module aligned with Microsoft Dynamics 365 Finance & Operations (D365 F&O), using standard business processes, forms, tables, relationships, inventory transactions, and accounting behavior.

The objective is to reproduce D365 F&O's essential sales functionality inside your ERP, rather than inventing a different sales architecture.

## 1. Project scope

Included — Core Sales ERP

Customers, sales parameters, sales orders, lines, prices, discounts, taxes, sales charges, inventory availability, reservations, confirmation, basic picking, packing slips, invoicing, payments, returns, credit notes and cancellation.

Excluded — Advanced functionality

Advanced Warehouse Management (WHS), mobile warehouse devices, wave processing, load planning, production, manufacturing, BOM, MRP, transportation management and advanced intercompany processing.

Basic picking remains included because D365 supports picking without advanced WHS.

## 2. Complete sales order lifecycle

Phase 1 — Master Data & Configuration

Customer · Item · Sales settings · Price · Tax · Posting

Phase 2 — Sales Order Header

Customer · Address · Currency · Delivery · Payment

Phase 3 — Sales Order Lines

Item · Quantity · Unit · Price · Discounts · Tax · Dimensions

Phase 4 — Availability & Reservation

On-hand validation · Reserve · Unreserve

Phase 5 — Sales Confirmation

Validate · Confirm · Confirmation journal

Phase 6 — Basic Picking (Optional)

Picking list · Register picked quantity

Phase 7 — Packing Slip

Physical issue · Inventory deduction · Delivery journal

Phase 8 — Sales Invoice

Financial issue · Revenue · Tax · COGS · Customer balance

Phase 9 — Payment & Settlement

Customer payment · Invoice settlement · Closing balance

Alternative workflows: partial delivery, partial invoicing, backorders, cancellation, returns and credit notes.

## 3. Sales order lifecycle — sample data

Use a single consistent example to validate every phase.

Sales order

SO-000125

Customer

CUST-001

Company

USMF

Currency

SAR

Warehouse

WH-01

Site

SITE-01

| Item     | Quantity | Unit price | Amount    |
| -------- | -------- | ---------- | --------- |
| ITEM-001 | 10       | 100 SAR    | 1,000 SAR |
| ITEM-002 | 5        | 200 SAR    | 1,000 SAR |
| Total    | 15       |            | 2,000 SAR |

Subtotal

## 2,000

VAT (15%)

## 300

Total SAR

## 2,300

Assumes no discounts, no charges, standard taxable items, and SAR-denominated transactions.

Every phase below identifies the ERP form, required behavior, standard D365 tables, important fields, inventory/accounting consequences and test cases.

# Phase 1 — Sales configuration and master data

Before creating sales orders, configure the core master data.

## 1.1 Customer master

D365 menu: Accounts receivable → Customers → All customers

Primary form: `CustTable`

### Main tables and fields

| Table                    | Core fields                                                                                                                                  | Purpose                                  |
| ------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------- |
| `CustTable`              | `AccountNum`, `CustGroup`, `Currency`, `PaymTermId`, `PaymMode`, `TaxGroup`, `InvoiceAccount`, `Party`, `CreditMax`, `Blocked`, `DataAreaId` | Customer account and commercial defaults |
| `DirPartyTable`          | `RecId`, `Name`, `NameAlias`                                                                                                                 | Customer party name                      |
| `DirPartyLocation`       | `Party`, `Location`                                                                                                                          | Party/location association               |
| `LogisticsLocation`      | `RecId`                                                                                                                                      | Address location                         |
| `LogisticsPostalAddress` | `Location`, `Street`, `City`, `ZipCode`, `CountryRegionId`, `ValidFrom`, `ValidTo`                                                           | Postal and delivery addresses            |
| `CustGroup`              | `CustGroup`, `Name`, `DataAreaId`                                                                                                            | Customer group                           |
| `CustParameters`         | Sales/AR parameter fields                                                                                                                    | AR-wide configuration                    |

### Example

```
Customer account: CUST-001
Customer name: ABC Trading
Customer group: DOMESTIC
Currency: SAR
Payment terms: NET30
Tax group: VAT15
Credit limit: 50,000 SAR
Blocked: No
Invoice account: CUST-001
```

### Required ERP behavior

1. Customer account must exist.
2. Customer must not be blocked for the requested operation.
3. Currency, tax group, payment terms and address should default to the sales order.
4. The invoice account can differ from the ordering customer.
5. The system should support a separate invoice address and delivery address.
6. Credit-limit checks should honor the configured policy.

Test: Select `CUST-001` on a new sales order. Verify all defaults are populated correctly.

## 1.2 Released products and inventory setup

D365 menu: Product information management → Products → Released products

| Table                      | Core fields                                                                                                           |
| -------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| `EcoResProduct`            | `RecId`, `DisplayProductNumber`, `ProductType`, `ProductSubType`                                                      |
| `EcoResProductTranslation` | `Product`, `LanguageId`, `Name`, `Description`                                                                        |
| `InventTable`              | `ItemId`, `Product`, `ItemGroupId`, `ModelGroupId`, `DimGroupId`, `ItemType`, `DataAreaId`                            |
| `InventTableModule`        | `ItemId`, `ModuleType`, `UnitId`, `Price`, `PriceUnit`, `TaxItemGroupId`                                              |
| `InventItemGroup`          | `ItemGroupId`, `Name`                                                                                                 |
| `InventModelGroupItem`     | `ItemId`, `ModelGroupId`                                                                                              |
| `InventDim`                | `InventDimId`, `InventSiteId`, `InventLocationId`, `InventBatchId`, `InventSerialId`                                  |
| `InventSite`               | `SiteId`, `Name`                                                                                                      |
| `InventLocation`           | `InventLocationId`, `Name`, `InventSiteId`                                                                            |
| `InventSum`                | `ItemId`, `InventDimId`, `PostedQty`, `Received`, `Deducted`, `ReservPhysical`, `ReservOrdered`, `OnOrder`, `Ordered` |

The item sales unit, sales price and sales tax item group are associated with the sales-module record in `InventTableModule`, where `ModuleType` identifies Sales.

### Item example

```
Item: ITEM-001
Product name: Cotton Shirt
Product type: Item
Item group: FG
Inventory model: FIFO
Sales unit: PCS
Sales price: 100 SAR
Sales tax item group: VAT15
Site: SITE-01
Warehouse: WH-01
Available physical: 100 PCS
```

The item must be released to the legal entity before it can be used on a standard stocked sales line.

## 1.3 Required sales configuration

| Configuration                  | Standard D365 table / area                 | Required function                      |
| ------------------------------ | ------------------------------------------ | -------------------------------------- |
| Sales parameters               | `SalesParameters`                          | Sales processing defaults              |
| Accounts receivable parameters | `CustParameters`                           | Invoice and customer behavior          |
| Number sequences               | `NumberSequenceTable` and references       | Automatic SalesId and documents        |
| Customer groups                | `CustGroup`                                | Account grouping                       |
| Sales units                    | `UnitOfMeasure`, `UnitOfMeasureConversion` | Unit and quantity conversion           |
| Sales pricing                  | `PriceDiscTable`                           | Trade agreements and applicable prices |
| Charges                        | `MarkupTable`, `MarkupTrans`               | Freight and other charges              |
| Sales tax groups               | `TaxGroupData`, related tax setup          | Customer-side tax applicability        |
| Item sales tax groups          | `TaxItemGroup` and related setup           | Item-side tax applicability            |
| Posting profiles               | `InventPosting`, `CustPostingProfile`      | Inventory and AR ledger postings       |
| Payment terms                  | `PaymTerm`                                 | Invoice due date                       |
| Delivery terms                 | `DlvTerm`                                  | Commercial delivery terms              |
| Delivery modes                 | `DlvMode`                                  | Method of delivery                     |

Some configuration relationships require associated setup tables beyond the main tables shown. Treat the table lists as the core implementation inventory, not a complete AOT field dictionary.

### Phase 1 completion criteria

- Customer can be selected.
- Stocked item can be selected.
- Sales UOM and default price populate.
- Sales tax groups are defined.
- Inventory site and warehouse are configured.
- Number sequence generates a SalesId.
- Posting profiles support packing slip and invoice posting.

# Phase 2 — Create sales order header

D365 menu: Sales and marketing → Sales orders → All sales orders → New

Main form: `SalesTable`

Main database table: `SalesTable`

## 2.1 Core SalesTable fields

| Field                   | Purpose                                    | Example           |
| ----------------------- | ------------------------------------------ | ----------------- |
| `SalesId`               | Unique sales order identifier              | SO-000125         |
| `SalesType`             | Sales order type                           | Sales order       |
| `SalesStatus`           | Overall order status                       | Backorder         |
| `DocumentStatus`        | Latest relevant document status            | None              |
| `CustAccount`           | Ordering customer                          | CUST-001          |
| `InvoiceAccount`        | Invoice customer                           | CUST-001          |
| `SalesName`             | Sales order name                           | ABC Trading       |
| `CurrencyCode`          | Sales currency                             | SAR               |
| `CustGroup`             | Customer group                             | DOMESTIC          |
| `Payment`               | Payment term                               | NET30             |
| `PaymMode`              | Payment method                             | BANK              |
| `TaxGroup`              | Sales tax group                            | VAT15             |
| `DlvMode`               | Mode of delivery                           | ROAD              |
| `DlvTerm`               | Terms of delivery                          | DAP               |
| `DeliveryDate`          | Requested delivery date (where applicable) | 2026-10-30        |
| `DeliveryPostalAddress` | Delivery address RecId                     | Address reference |
| `InvoicePostalAddress`  | Invoice address RecId                      | Address reference |
| `InventSiteId`          | Default site where exposed/configured      | SITE-01           |
| `InventLocationId`      | Default warehouse where exposed/configured | WH-01             |
| `DefaultDimension`      | Financial dimension combination            | Dimension set     |
| `PurchOrderFormNum`     | Customer purchase order reference          | PO-CUST-101       |

Important: D365's date, delivery and inventory-dimension defaults are spread across header, line and associated records. Do not assume every visible field is stored in `SalesTable`. Validate the exact standard fields against the target application's AOT.

## 2.2 Header creation logic

```
1. User clicks New Sales Order
2. System generates SalesId
3. User selects customer
4. System loads customer defaults
5. User verifies invoice account
6. System defaults currency and payment terms
7. System defaults tax group and delivery details
8. User selects delivery address
9. User saves order
10. System creates SalesTable record
```

### Affected tables

| Operation                | Table                                      | Expected impact                                   |
| ------------------------ | ------------------------------------------ | ------------------------------------------------- |
| Create sales order       | `SalesTable`                               | Insert one order header                           |
| Generate order ID        | Number sequence framework                  | Consume or reserve next number according to setup |
| Change customer          | `SalesTable`                               | Refresh permitted customer-related defaults       |
| Select address           | `SalesTable`, address reference tables     | Reference valid delivery/invoice address          |
| Add financial dimensions | `SalesTable`, `DimensionAttributeValueSet` | Store default dimension reference                 |

### Business rules

- No sales order without a valid customer.
- No duplicate SalesId within its valid company scope.
- Prevent unauthorized customer or company changes after downstream posting.
- Currency and exchange-rate handling must follow the transaction currency and posting date.
- Changing delivery address must not directly change physical inventory.
- New sales orders do not automatically generate posted revenue, COGS or customer invoices.

# Phase 3 — Add sales order lines

Main line table: `SalesLine`

Supporting tables: `InventTable`, `InventTableModule`, `InventDim`, `InventTransOrigin`, `InventTrans`, and pricing/tax configuration.

## 3.1 Main SalesLine fields

| Field                   | Meaning                                    | Example             |
| ----------------------- | ------------------------------------------ | ------------------- |
| `RecId`                 | Unique internal line identity              | Generated           |
| `SalesId`               | Header relationship                        | SO-000125           |
| `LineNum`               | Order line sequence                        | 1                   |
| `ItemId`                | Released item                              | ITEM-001            |
| `Name`                  | Sales line description                     | Cotton Shirt        |
| `SalesQty`              | Sales quantity                             | 10                  |
| `SalesUnit`             | Sales UOM                                  | PCS                 |
| `QtyOrdered`            | Quantity in inventory unit                 | 10                  |
| `SalesPrice`            | Sales unit price                           | 100                 |
| `PriceUnit`             | Price quantity basis                       | 1                   |
| `LineAmount`            | Net line amount, excluding tax             | 1,000               |
| `LinePercent`           | Line discount percentage                   | 0                   |
| `LineDisc`              | Line discount amount                       | 0                   |
| `MultiLnDisc`           | Multiline discount amount                  | 0                   |
| `MultiLnPercent`        | Multiline discount percentage              | 0                   |
| `CurrencyCode`          | Transaction currency                       | SAR                 |
| `TaxGroup`              | Sales tax group                            | VAT15               |
| `TaxItemGroup`          | Item tax group                             | VAT15               |
| `InventDimId`           | Inventory dimension reference              | Generated           |
| `InventTransId`         | Inventory transaction reference ID         | Generated           |
| `SalesStatus`           | Line processing status                     | Backorder           |
| `RemainSalesPhysical`   | Physical sales quantity remaining          | 10                  |
| `RemainSalesFinancial`  | Financial sales quantity remaining         | 10                  |
| `RemainInventPhysical`  | Physical inventory quantity remaining      | 10                  |
| `RemainInventFinancial` | Financial inventory quantity remaining     | 10                  |
| `DeliverNow`            | Quantity designated for immediate delivery | 0                   |
| `ShippingDateRequested` | Requested shipment date                    | 2026-10-29          |
| `ReceiptDateRequested`  | Requested receipt date                     | 2026-10-30          |
| `DefaultDimension`      | Financial dimensions                       | Dimension reference |

Inventory units and sales units can differ, so the Sales and Invent remaining quantities should not be treated as identical in every scenario.

## 3.2 Sales line insertion sequence

1. Check that `ItemId` exists in `InventTable` for the current legal entity.
2. Read item sales defaults from `InventTableModule`.
3. Set quantity and apply sales-to-inventory unit conversion.
4. Determine sales price using applicable trade agreements and pricing rules.
5. Apply line, multiline and total discounts as applicable.
6. Determine site, warehouse and other active inventory dimensions.
7. Resolve tax groups, requested dates and delivery defaults.
8. Calculate `LineAmount` and persist the `SalesLine`.
9. Create or maintain related inventory issue transaction and origin through standard inventory logic.
10. Recalculate order totals and perform applicable validations.

### Related database changes

| Table               | Expected change                                         |
| ------------------- | ------------------------------------------------------- |
| `SalesLine`         | Insert line                                             |
| `InventDim`         | Reuse or create inventory dimension combination         |
| `InventTransOrigin` | Create/source-link inventory transaction origin         |
| `InventTrans`       | Create initial issue transaction                        |
| `SalesTable`        | Header remains linked; statuses/defaults may be updated |
| `MarkupTrans`       | Insert charges only when applicable                     |
| `PriceDiscTable`    | Read pricing configuration; normally not modified       |

### Inventory effect

For a standard stocked item:

```
Sales order: SO-000125
Line: ITEM-001
Quantity: 10 PCS

InventTrans:
  Direction: Issue
  Quantity: -10
  StatusIssue: On order

Physical stock has not been deducted.
Financial inventory has not been deducted.
```

The initial `On order` issue transaction represents expected outbound inventory.

## 3.3 Price and discount calculation

For the simplest case:

\\[ \text{LineAmount} = \frac{\text{SalesQty}\times\text{SalesPrice}}{\text{PriceUnit}}-\text{Discounts} \\]

Example:

| Component            | Amount    |
| -------------------- | --------- |
| Quantity             | 10        |
| Unit price           | 100 SAR   |
| Gross amount         | 1,000 SAR |
| Line discount 10%    | −100 SAR  |
| Net taxable amount   | 900 SAR   |
| VAT 15%              | 135 SAR   |
| Amount including VAT | 1,035 SAR |

Implement the D365 calculation sequence rather than simply applying a discount to the final invoice amount. Line discounts, multiline discounts, total discounts, charges, tax settings and rounding may affect the result differently.

### Critical line-change scenarios

| Change                | Required system behavior                                          |
| --------------------- | ----------------------------------------------------------------- |
| Change ItemId         | Refresh item-dependent defaults and inventory origin consistently |
| Change quantity       | Update remaining quantities and inventory transaction quantity    |
| Change SalesUnit      | Convert inventory quantity and reevaluate unit-dependent pricing  |
| Change sales price    | Recalculate line net amount                                       |
| Change discount       | Recalculate net amount and taxable basis                          |
| Change tax group      | Recalculate estimated sales tax                                   |
| Change warehouse      | Adjust inventory dimension association and related reservations   |
| Change delivery date  | Revalidate date-dependent rules                                   |
| Delete line           | Remove/cancel valid unposted requirements using standard logic    |
| Change confirmed line | Preserve historical confirmation and apply re-confirmation rules  |

Do not implement line modifications using isolated SQL updates. In D365, updating the sales line involves transaction, pricing and reservation business logic beyond a single `SalesLine` record.

# Phase 4 — Inventory availability and reservation

D365 navigation: Sales order → Select line → Inventory → Reservation

Reservation is part of basic core ERP functionality and must not depend on advanced WHS.

## 4.1 Main tables

| Table               | Key fields                                                                                                            | Responsibility                   |
| ------------------- | --------------------------------------------------------------------------------------------------------------------- | -------------------------------- |
| `InventSum`         | `ItemId`, `InventDimId`, `PostedQty`, `Received`, `Deducted`, `ReservPhysical`, `ReservOrdered`, `OnOrder`, `Ordered` | On-hand and reservation balances |
| `InventDim`         | `InventDimId`, `InventSiteId`, `InventLocationId`, `InventBatchId`, `InventSerialId`                                  | Inventory dimensions             |
| `InventTrans`       | `InventTransOrigin`, `InventDimId`, `Qty`, `StatusIssue`, `DatePhysical`, `DateFinancial`                             | Inventory movement               |
| `InventTransOrigin` | `RecId`, `InventTransId`, `ReferenceCategory`, `ReferenceId`                                                          | Source document relationship     |
| `SalesLine`         | `InventTransId`, `InventDimId`, `SalesQty`, `QtyOrdered`                                                              | Sales demand                     |

## 4.2 Reservation example

Initial state:

```
ITEM-001
Site: SITE-01
Warehouse: WH-01

Physical stock:       100 PCS
Reserved physical:      0 PCS
Available physical:   100 PCS

Sales order quantity:  10 PCS
```

After reserving 10 PCS:

```
Physical stock:       100 PCS
Reserved physical:     10 PCS
Available physical:    90 PCS

InventTrans issue status:
Reserved physical
```

Reservation does not deduct the physical stock.

### Example reservation screen

| Item     | Ordered | Reserved | Remaining to reserve |
| -------- | ------- | -------- | -------------------- |
| ITEM-001 | 10      | 10       | 0                    |
| ITEM-002 | 5       | 3        | 2                    |

The user must be able to reserve or unreserve each line independently.

## 4.3 Required reservation behavior

- Manual physical reservation.
- Automatic reservation when configured.
- Partial reservation.
- Unreserve inventory.
- Prevent reservation of more available physical stock than permitted.
- Respect site, warehouse, batch, serial and other active dimensions.
- Reevaluate reservations when order quantity or inventory dimensions change.
- Prevent double allocation when two orders reserve stock concurrently.
- Handle physical and ordered reservations according to configuration.

Critical: On-hand quantity must be calculated using D365 inventory logic and inventory dimensions. Do not assume `InventSum.PostedQty` alone equals current physical on-hand or available quantity.

# Phase 5 — Sales order confirmation

D365 navigation: Sales and marketing → Sales orders → All sales orders → Open order → Sell → Confirm sales order.

Microsoft's standard confirmation procedure posts a confirmation journal and updates `SalesTable.DocumentStatus` to Confirmation. The order's fulfillment `SalesStatus` does not automatically become a separate Confirmed state.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



## 5.1 Main confirmation tables

| Table              | Key fields                                                                     | Behavior                   |
| ------------------ | ------------------------------------------------------------------------------ | -------------------------- |
| `SalesTable`       | `SalesId`, `SalesStatus`, `DocumentStatus`                                     | Update document status     |
| `SalesLine`        | `SalesId`, `LineNum`, `SalesQty`, `SalesPrice`, `LineAmount`                   | Source for confirmation    |
| `CustConfirmJour`  | `SalesId`, `ConfirmId`, `ConfirmDate`, `InvoiceAccount`, `CurrencyCode`        | Posted confirmation header |
| `CustConfirmTrans` | `ConfirmId`, `SalesId`, `LineNum`, `ItemId`, `Qty`, `SalesPrice`, `LineAmount` | Confirmed line snapshot    |
| `SalesParmTable`   | `ParmId`, `TableRefId`, `SalesId`                                              | Posting parameters         |
| `SalesParmLine`    | `ParmId`, `TableRefId`, `LineNum`                                              | Posting line selection     |

The `SalesParm*` records are part of the posting framework and should not be treated as permanent substitutes for posted journals. Some posting records are temporary or subject to cleanup.

## 5.2 Confirmation process

```
SO-000125
    |
    v
Validate customer
    |
Validate order lines
    |
Check credit limit (according to setup)
    |
Calculate confirmation quantities
    |
Generate confirmation journal
    |
Update SalesTable.DocumentStatus
    |
Print sales confirmation
```

### Confirmation example

Before:

```
SalesTable:
SalesId        = SO-000125
SalesStatus    = Backorder
DocumentStatus = None
```

After:

```
SalesTable:
SalesId        = SO-000125
SalesStatus    = Backorder
DocumentStatus = Confirmation

CustConfirmJour:
ConfirmId = CONF-000125
SalesId   = SO-000125

CustConfirmTrans:
ITEM-001 = 10 PCS
ITEM-002 =  5 PCS
```

Document number `CONF-000125` is illustrative.

## 5.3 Confirmation accounting effect

| Area                 | Effect       |
| -------------------- | ------------ |
| Physical stock       | No deduction |
| Financial inventory  | No deduction |
| Customer balance     | No posting   |
| Revenue              | No posting   |
| Cost of goods sold   | No posting   |
| Confirmation journal | Created      |

Confirmation is a commercial document event, not a physical or financial inventory posting.

## 5.4 Pro forma versus posting

| Action                                  | Create journal?                         | Physical/financial impact? |
| --------------------------------------- | --------------------------------------- | -------------------------- |
| Pro forma confirmation (`Posting = No`) | No posted confirmation journal          | No                         |
| Confirm sales order (`Posting = Yes`)   | Yes                                     | No inventory/GL posting    |
| Reconfirm changed order                 | New confirmation history as appropriate | No inventory/GL posting    |

Test: Confirm the same order after changing an allowed line quantity. Verify that previous confirmation history is retained and the latest confirmation reflects the changed quantity.

# Phase 6 — Basic picking

Picking is optional in a non-WHS sales cycle.

A basic ERP may use a picking list, register quantities picked, and then post the packing slip. It may also ship directly without requiring picking registration. Microsoft explicitly supports shipping sales orders without warehousing processes.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



## 6.1 Core tables

| Table                 | Key fields / concepts                     |
| --------------------- | ----------------------------------------- |
| `WMSPickingRoute`     | Picking route identity and status         |
| `WMSOrderTrans`       | Picking order transactions and quantities |
| `WMSPickingListJour`  | Posted picking list journal header        |
| `WMSPickingListTrans` | Posted picking list lines                 |
| `InventTrans`         | Issue status and picked quantity          |
| `SalesLine`           | Sales quantity and delivery remainder     |

The standard picking framework contains additional relationships and setup. These tables belong to legacy/basic picking functionality, not necessarily advanced warehouse work creation.

## 6.2 Basic example

```
Order line: ITEM-001
Ordered:    10 PCS
Reserved:   10 PCS

Picking list generated: 10 PCS
Registered picked:      8 PCS
Remaining to pick:      2 PCS
```

After successful picking registration, the applicable inventory transaction quantity can reach `Picked`.

Picking does not equal a posted sales packing slip.

## 6.3 Required test cases

- Pick full quantity.
- Pick partial quantity.
- Register picking list.
- Attempt picking more than allowed.
- Pick two lines separately.
- Cancel or reverse picking through supported operations.
- Post packing slip based on picked quantities.
- Skip picking entirely for a permitted direct-shipping flow.

# Phase 7 — Packing slip / physical delivery

D365 navigation: Open sales order → Pick and pack → Post packing slip

This is the principal physical inventory issue stage.

## 7.1 Main packing slip tables

| Table                  | Core fields                                                                                         |
| ---------------------- | --------------------------------------------------------------------------------------------------- |
| `CustPackingSlipJour`  | `PackingSlipId`, `SalesId`, `DeliveryDate`, `OrderAccount`, `InvoiceAccount`, `DataAreaId`          |
| `CustPackingSlipTrans` | `PackingSlipId`, `SalesId`, `LineNum`, `ItemId`, `Qty`, `InventQty`, `InventDimId`, `InventTransId` |
| `SalesLine`            | `SalesQty`, `RemainSalesPhysical`, `RemainSalesFinancial`, `SalesStatus`, `InventTransId`           |
| `SalesTable`           | `SalesStatus`, `DocumentStatus`                                                                     |
| `InventTrans`          | `Qty`, `StatusIssue`, `DatePhysical`, `VoucherPhysical`, `CostAmountPhysical`                       |
| `InventSum`            | Physical inventory quantity components                                                              |
| `InventTransOrigin`    | Inventory transaction source linkage                                                                |

## 7.2 Full packing slip example

Before delivery:

```
Sales order: SO-000125
ITEM-001: 10 PCS
ITEM-002:  5 PCS
```

Post packing slip for all quantities.

Expected results:

| Field / record                 | Before                       | After     |
| ------------------------------ | ---------------------------- | --------- |
| `ITEM-001` physical remaining  | 10                           | 0         |
| `ITEM-002` physical remaining  | 5                            | 0         |
| `ITEM-001` financial remaining | 10                           | 10        |
| `ITEM-002` financial remaining | 5                            | 5         |
| Inventory issue status         | Reserved physical / On order | Deducted  |
| Packing slip journal           | None                         | Created   |
| Order fulfillment status       | Backorder                    | Delivered |

The order `DocumentStatus` is updated to Packing slip.

A packing slip posts a physical issue. Whether associated physical cost also posts to the general ledger depends on inventory parameters, item model group posting settings and inventory posting profiles.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+1



## 7.3 Partial packing slip example

Order:

```
ITEM-001
SalesQty = 10 PCS
```

First delivery = 6 PCS.

Ordered

# 10

Delivered

# 6

Remaining

# 4

60% physically delivered, 40% outstanding.

Expected:

```
SalesLine:
SalesQty               = 10
RemainSalesPhysical    = 4
RemainSalesFinancial   = 10

InventTrans (conceptual split):
Transaction A: -6, StatusIssue = Deducted
Transaction B: -4, StatusIssue = On order / reserved status
```

The exact record split depends on inventory dimensions, reservations and posting history.

## 7.4 Packing slip quantity options

| Quantity option                 | Expected use                                   |
| ------------------------------- | ---------------------------------------------- |
| All                             | Ship remaining eligible quantity               |
| Deliver now                     | Ship quantity specified for immediate delivery |
| Picked                          | Ship picked quantity                           |
| Other supported posting options | Apply according to setup and document type     |

Critical rule: Never reduce `SalesQty` just to record a partial shipment. Maintain the original ordered quantity and record the physical fulfillment separately.

# Phase 8 — Sales invoicing and financial posting

D365 navigation: Open sales order → Invoice → Generate → Invoice

Sales invoices financially update the sales order and its inventory issue transactions.

## 8.1 Core invoice tables

| Table                        | Core fields                                                                                                                 |
| ---------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| `CustInvoiceJour`            | `InvoiceId`, `InvoiceDate`, `SalesId`, `OrderAccount`, `InvoiceAccount`, `CurrencyCode`, `LedgerVoucher`, `InvoiceAmount`   |
| `CustInvoiceTrans`           | `InvoiceId`, `SalesId`, `LineNum`, `ItemId`, `Qty`, `SalesPrice`, `LineAmount`, `TaxGroup`, `TaxItemGroup`, `InventTransId` |
| `SalesLine`                  | `RemainSalesFinancial`, `SalesStatus`, `SalesQty`                                                                           |
| `SalesTable`                 | `SalesStatus`, `DocumentStatus`                                                                                             |
| `InventTrans`                | `StatusIssue`, `DateFinancial`, `Voucher`, `CostAmountPosted`                                                               |
| `CustTrans`                  | `AccountNum`, `Voucher`, `Invoice`, `AmountCur`, `CurrencyCode`, `TransDate`                                                |
| `TaxTrans`                   | `Voucher`, `TransDate`, `TaxCode`, `TaxBaseAmount`, `TaxAmount`                                                             |
| `GeneralJournalEntry`        | `RecId`, `AccountingDate`, `SubledgerVoucher`                                                                               |
| `GeneralJournalAccountEntry` | `GeneralJournalEntry`, `LedgerDimension`, `AccountingCurrencyAmount`                                                        |

## 8.2 Invoice example

Assume both order lines are fully delivered and ready for invoicing.

| Description            | Amount    |
| ---------------------- | --------- |
| ITEM-001: 10 × 100     | 1,000 SAR |
| ITEM-002: 5 × 200      | 1,000 SAR |
| Net sales              | 2,000 SAR |
| VAT 15%                | 300 SAR   |
| Customer invoice total | 2,300 SAR |

### Accounting example

Assume:

- ITEM-001 inventory cost = 60 SAR per unit.
- ITEM-002 inventory cost = 120 SAR per unit.
- Total cost = 1,200 SAR.
- No discounts or freight charges.

| Account             | Debit | Credit |
| ------------------- | ----- | ------ |
| Accounts receivable | 2,300 | —      |
| Sales revenue       | —     | 2,000  |
| Output VAT          | —     | 300    |
| Cost of goods sold  | 1,200 | —      |
| Inventory           | —     | 1,200  |
| Total               | 3,500 | 3,500  |

This is a simplified net accounting illustration. If physical inventory posting was enabled at packing slip, D365 can first post delivered-cost accounts and reverse/reclassify them at invoicing. Avoid generating duplicate COGS or inventory expense when designing the accounting engine.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



## 8.3 Inventory updates after full invoice

```
SalesLine:
RemainSalesPhysical  = 0
RemainSalesFinancial = 0
SalesStatus          = Invoiced

InventTrans:
StatusIssue    = Sold
DatePhysical   = Packing slip date
DateFinancial  = Invoice financial date
```

A posted invoice also creates customer and tax subledger information together with applicable general ledger entries.

## 8.4 Partial invoicing

Example:

```
Ordered quantity:       10
Physically delivered:    6
First invoice quantity:  4
```

Under a packing-slip-based invoice policy:

| Field                         | Result |
| ----------------------------- | ------ |
| Ordered                       | 10     |
| Delivered                     | 6      |
| Invoiced                      | 4      |
| Remaining physical delivery   | 4      |
| Remaining financial invoicing | 6      |
| Delivered but not invoiced    | 2      |

This scenario is especially important because the two remaining fields represent different obligations.

A successful implementation must allow a line to be physically delivered while some or all of its quantity remains financially uninvoiced.

# Phase 9 — Customer payment and settlement

D365 menu: Accounts receivable → Payments → Customer payment journal

Core workflow:

```
Posted Sales Invoice
        |
        v
Open Customer Transaction
        |
        v
Create Customer Payment Journal
        |
        v
Post Payment
        |
        v
Settle Invoice against Payment
        |
        v
Remaining Customer Balance = 0
```

## Core tables

| Table                        | Relevant fields / purpose                                                                 |
| ---------------------------- | ----------------------------------------------------------------------------------------- |
| `LedgerJournalTable`         | `JournalNum`, `JournalName`, `JournalType`, `Posted`                                      |
| `LedgerJournalTrans`         | `JournalNum`, `Voucher`, `AccountType`, `AccountNum`, `AmountCurDebit`, `AmountCurCredit` |
| `CustTrans`                  | Customer debit and credit transactions                                                    |
| `CustTransOpen`              | Open customer transaction amount                                                          |
| `CustSettlement`             | Invoice/payment settlement relationships                                                  |
| `GeneralJournalEntry`        | Posted voucher header                                                                     |
| `GeneralJournalAccountEntry` | Financial posting detail                                                                  |

### Example

Invoice:

```
InvoiceId = INV-000125
Customer  = CUST-001
Amount    = 2,300 SAR
```

Payment received:

```
Payment voucher = PAY-000125
Amount          = 2,300 SAR
Payment method  = BANK
```

Simplified accounting:

| Account             | Debit | Credit |
| ------------------- | ----- | ------ |
| Bank                | 2,300 | —      |
| Accounts receivable | —     | 2,300  |

After full settlement, the customer invoice's remaining open balance should be zero.

Important: An invoice being fully settled does not determine `SalesTable.SalesStatus`; fulfillment/invoicing status and customer payment status are separate concerns.

# Phase 10 — Returns, credit notes and cancellation

## 10.1 Cancel an unfulfilled sales order

Example:

```
Sales Order: SO-000125
Ordered:     10 PCS
Delivered:    0 PCS
Invoiced:     0 PCS
```

Expected behavior:

- Cancel outstanding delivery requirements.
- Release reservations.
- Update sales line fulfillment status.
- Update or cancel the remaining inventory issue quantity through standard inventory logic.
- Preserve historical confirmation journals.
- Do not create sales revenue, VAT, COGS or customer transactions merely because the order was canceled.

D365's sales status is primarily a fulfillment status. A fully canceled, undelivered line can have no remaining delivery quantity and a `Canceled` sales status. Do not introduce a custom mandatory approval/status sequence unless your business requires it.

## 10.2 Cancel the delivery remainder

Suppose the customer ordered 10 units, 6 have shipped and the remaining 4 are no longer required.

| Quantity                   | Before cancellation | After cancellation        |
| -------------------------- | ------------------- | ------------------------- |
| Original ordered           | 10                  | 10 (historical reference) |
| Physically delivered       | 6                   | 6                         |
| Remaining delivery         | 4                   | 0                         |
| Financially invoiced       | 0                   | 0                         |
| Delivered awaiting invoice | 6                   | 6                         |

Use the standard Delivery remainder → Cancel quantity functionality or the corresponding D365 business logic. In D365, quantity fields and inventory transaction quantities may be adjusted; the table above describes the business reconciliation, not a promise that every original line field remains numerically unchanged.

Cancellation must never reverse posted packing slip or invoice history simply by deleting records.

## 10.3 Customer return and credit note

D365 navigation: Sales and marketing → Return orders → All return orders

A standard return order is represented within the sales order framework as a returned order.

### Important tables

| Table                  | Role                                                     |
| ---------------------- | -------------------------------------------------------- |
| `ReturnTable`          | Return order header and return authorization information |
| `ReturnLine`           | Return line details, reason and disposition              |
| `SalesTable`           | Underlying returned-order sales header                   |
| `SalesLine`            | Return sales lines                                       |
| `InventTrans`          | Returned inventory transactions                          |
| `CustPackingSlipJour`  | Return receipt packing slip journal                      |
| `CustPackingSlipTrans` | Returned quantities                                      |
| `CustInvoiceJour`      | Credit note journal                                      |
| `CustInvoiceTrans`     | Negative invoice/credit lines                            |
| `CustTrans`            | Customer credit transaction                              |
| `TaxTrans`             | Tax reversal/adjustment                                  |

### Return example

Original invoice:

```
ITEM-001
Sold quantity = 10
Unit price    = 100 SAR
VAT           = 15%
```

Customer returns 2 units.

| Credit note component | Amount  |
| --------------------- | ------- |
| Return quantity       | 2       |
| Unit sales price      | 100 SAR |
| Net credit            | 200 SAR |
| VAT credit (15%)      | 30 SAR  |
| Total customer credit | 230 SAR |

If the units are returned to inventory and their original cost is 60 SAR per unit, the illustrative inventory value restored is 120 SAR.

### Return processing steps

1. Create return order and identify original invoice where applicable.
2. Add the return lines and quantities.
3. Record the return reason.
4. Assign the appropriate disposition action.
5. Physically receive the returned goods where required.
6. Post the relevant return packing slip.
7. Post credit note.
8. Verify customer balance, tax and inventory effects.

Standard D365 return dispositions include Credit, Credit only, Scrap, Replace and credit, Replace and scrap, and Return to customer. The disposition determines whether inventory is received, scrapped, replaced or not physically returned.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+1



For the first core implementation, prioritize Credit and Credit only; then implement the other standard dispositions if required.

# Phase 11 — Complete business test matrix

This is the critical verification layer. Every scenario must be validated at the form, business-logic, database and accounting levels.

## 11.1 Main sales scenarios

Sales ERP acceptance checklist

0 of 45 scenarios marked complete

0%

AllPendingCompleted

SO-01

Create customer sales order

Header and defaults saved; no posted revenue

SO-02

Single sales line

Valid item, quantity and issue transaction

SO-03

Multiple sales lines

Each line independently maintained

SO-04

Zero / negative order quantity

Validated according to order type and policy

SO-05

Change item

Refresh appropriate item defaults

SO-06

Change sales quantity

Recalculate quantities and inventory issue

SO-07

Change sales unit

Correct unit conversion and price behavior

SO-08

Change sales price

Recalculate line amount

SO-09

Apply line discount

Net amount and tax update

SO-10

Apply multiline/total discount

Correct discount allocation

SO-11

Add freight charge

Charges and taxes calculated

SO-12

Change delivery address

Header/line delivery behavior updated

SO-13

Change site/warehouse

Inventory dimensions and reservation consistent

SO-14

Insufficient inventory

Prevent or allow according to configured policy

SO-15

Reserve full quantity

Inventory reserved without deduction

SO-16

Reserve partial quantity

Outstanding requirement remains

SO-17

Unreserve quantity

Available physical restored

SO-18

Confirm order

Confirmation journal created; no physical posting

SO-19

Pro forma confirmation

No posted confirmation journal

SO-20

Modify and reconfirm

Confirmation history preserved

SO-21

Picking list

Picking records generated where applicable

SO-22

Partial picking

Picked quantity correctly represented

SO-23

Full packing slip

Physical issue and delivery journal

SO-24

Partial packing slip

Remaining physical quantity correct

SO-25

Two packing slips

Two delivery records, correct cumulative quantity

SO-26

Full sales invoice

Financial posting and customer invoice

SO-27

Partial sales invoice

Remaining financial quantity correct

SO-28

Invoice delivered quantity only

No excess quantity invoiced under packing-slip policy

SO-29

Receive customer payment

Customer receipt posted

SO-30

Partially settle invoice

Open amount remains

SO-31

Fully settle invoice

Open balance becomes zero

SO-32

Cancel undelivered order

Reservations released; no revenue posted

SO-33

Cancel partial remainder

Only outstanding requirements canceled

SO-34

Return received goods

Return inventory receipt and credit note

SO-35

Credit only return

Customer credited without normal physical return

SO-36

Multiple companies

Company isolation respected

SO-37

Different currencies

Exchange rates and accounting currency amounts correct

SO-38

Tax exempt line

Tax exemption according to setup

SO-39

Batch / serial item

Required active inventory dimensions validated

SO-40

Blocked customer or item

Processing restricted according to policy

SO-41

Concurrent reservations

No double allocation of physical stock

SO-42

Repeat posting request

No duplicate posted documents

SO-43

Reverse posted document

Use standard corrections/credits, preserve history

SO-44

Sales order reporting

Totals, open commitments and posted values reconcile

SO-45

Posting failure / rollback

No partial or inconsistent committed changes

Reset Copy checklist

The checklist is an implementation acceptance aid; checking a box does not execute a test or change the ERP database.

## 11.2 Inventory status reconciliation

For standard stocked issue transactions:

| Business step            | Typical InventTrans status | Physical effect                          | Financial effect                 |
| ------------------------ | -------------------------- | ---------------------------------------- | -------------------------------- |
| Sales line created       | On order                   | Outstanding demand                       | None                             |
| Stock reserved           | Reserved physical          | Reserved, not deducted                   | None                             |
| Basic picking registered | Picked                     | Picked, not physically issued            | None                             |
| Packing slip posted      | Deducted                   | Physical deduction                       | Possible physical ledger posting |
| Invoice posted           | Sold                       | Already physically issued in normal flow | Financial inventory issue        |

Microsoft documents these issue statuses and their relationship to packing slips and invoicing.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



When only part of a line moves to the next step, expect separate quantities or inventory transactions at different issue statuses.

## 11.3 Sales status versus document status

Keep these concepts separate in the ERP.

| Stage                                           | `SalesStatus` (typical)                       | `DocumentStatus` (typical)  |
| ----------------------------------------------- | --------------------------------------------- | --------------------------- |
| Order created                                   | Backorder                                     | None                        |
| Sales order confirmed                           | Backorder                                     | Confirmation                |
| Partially delivered                             | Backorder                                     | Packing slip                |
| Fully delivered                                 | Delivered                                     | Packing slip                |
| Partially invoiced with outstanding fulfillment | Backorder / Delivered, depending on remainder | Invoice                     |
| Fully invoiced                                  | Invoiced                                      | Invoice                     |
| Undelivered quantity canceled                   | Canceled where applicable                     | Depends on document history |

D365 may display localized descriptions or different order-level results depending on mixed line statuses. The exact enum values and aggregation rules must be matched rather than replaced by a single custom status field.

# Phase 12 — Standard D365 tables and relationships

This is the core database map your ERP should follow.

## 12.1 Table relationship diagram

## 12.2 Table inventory by business function

| Functional area                 | Core tables                                                                     |
| ------------------------------- | ------------------------------------------------------------------------------- |
| Customer                        | `CustTable`, `DirPartyTable`, `DirPartyLocation`, `LogisticsPostalAddress`      |
| Sales header                    | `SalesTable`                                                                    |
| Sales details                   | `SalesLine`                                                                     |
| Product master                  | `EcoResProduct`, `EcoResProductTranslation`, `InventTable`                      |
| Product sales configuration     | `InventTableModule`                                                             |
| Inventory dimensions            | `InventDim`                                                                     |
| Inventory transactions          | `InventTransOrigin`, `InventTrans`                                              |
| On-hand                         | `InventSum`                                                                     |
| Trade agreements                | `PriceDiscTable`                                                                |
| Sales charges                   | `MarkupTable`, `MarkupTrans`                                                    |
| Confirmation                    | `CustConfirmJour`, `CustConfirmTrans`                                           |
| Basic picking                   | `WMSPickingRoute`, `WMSOrderTrans`, `WMSPickingListJour`, `WMSPickingListTrans` |
| Packing slip                    | `CustPackingSlipJour`, `CustPackingSlipTrans`                                   |
| Invoice                         | `CustInvoiceJour`, `CustInvoiceTrans`                                           |
| Customer transactions           | `CustTrans`, `CustTransOpen`, `CustSettlement`                                  |
| Sales tax                       | `TaxTrans`, `TaxTable`, tax group configuration                                 |
| General ledger                  | `GeneralJournalEntry`, `GeneralJournalAccountEntry`                             |
| Payment journal                 | `LedgerJournalTable`, `LedgerJournalTrans`                                      |
| Return order                    | `ReturnTable`, `ReturnLine`, `SalesTable`, `SalesLine`                          |
| Posting framework               | `SalesParmTable`, `SalesParmLine`, `SalesParmUpdate`                            |
| Inventory posting configuration | `InventPosting`                                                                 |
| Customer posting profile        | `CustPostingProfile`                                                            |

These are the primary standard tables to investigate, not an exhaustive list of every D365 table invoked by the posting framework. Several journal relationships also use source-document references, inventory transaction links and company-specific keys.

## 12.3 Essential database relationships

```
-- Sales order header and lines
SELECT
    ST.DATAAREAID,
    ST.SALESID,
    ST.CUSTACCOUNT,
    ST.SALESSTATUS AS HeaderSalesStatus,
    ST.DOCUMENTSTATUS,
    SL.RECID AS SalesLineRecId,
    SL.LINENUM,
    SL.ITEMID,
    SL.SALESQTY,
    SL.SALESUNIT,
    SL.SALESPRICE,
    SL.LINEAMOUNT,
    SL.REMAINSALESPHYSICAL,
    SL.REMAINSALESFINANCIAL,
    SL.INVENTTRANSID,
    SL.INVENTDIMID
FROM SALESTABLE ST
INNER JOIN SALESLINE SL
    ON SL.SALESID = ST.SALESID
   AND SL.DATAAREAID = ST.DATAAREAID
   AND SL.PARTITION = ST.PARTITION
WHERE ST.SALESID = 'SO-000125';
```

This is a read-only SQL Server example for an environment where the D365 physical tables are accessible. The `PARTITION` predicate applies where the table schema exposes that field.

### Trace the sales inventory transactions

```
SELECT
    SL.SALESID,
    SL.LINENUM,
    SL.ITEMID,
    SL.SALESQTY,
    SL.REMAINSALESPHYSICAL,
    SL.REMAINSALESFINANCIAL,
    ITO.RECID AS OriginRecId,
    IT.RECID AS InventTransRecId,
    IT.QTY,
    IT.STATUSISSUE,
    IT.DATEPHYSICAL,
    IT.DATEFINANCIAL,
    IT.VOUCHERPHYSICAL,
    IT.VOUCHER,
    IT.COSTAMOUNTPHYSICAL,
    IT.COSTAMOUNTPOSTED
FROM SALESLINE SL
LEFT JOIN INVENTTRANSORIGIN ITO
    ON ITO.INVENTTRANSID = SL.INVENTTRANSID
   AND ITO.DATAAREAID = SL.DATAAREAID
   AND ITO.PARTITION = SL.PARTITION
LEFT JOIN INVENTTRANS IT
    ON IT.INVENTTRANSORIGIN = ITO.RECID
   AND IT.DATAAREAID = ITO.DATAAREAID
   AND IT.PARTITION = ITO.PARTITION
WHERE SL.SALESID = 'SO-000125'
ORDER BY SL.LINENUM, IT.RECID;
```

Inventory issue status should be interpreted through the D365 enum metadata rather than relying on guessed integer values. This query also makes it possible to identify transaction splits after partial delivery or invoicing.

## 12.4 Implementation rules

Your ERP must maintain these invariants:

1. `SalesTable` has one-to-many relationships with `SalesLine`.
2. Sales lines use valid released products and inventory dimensions.
3. Each stocked sales line's inventory requirements must be traceable through `InventTransOrigin`.
4. Reservations must not be mistaken for physical deliveries.
5. Physical and financial remaining quantities must be maintained independently.
6. Confirmation, packing slip and invoice journals must retain historical posted details.
7. Posted documents must not be silently altered when the current order changes.
8. Customer transactions must reconcile with posted invoices and settlements.
9. Inventory and ledger posting must remain balanced.
10. Posting must be transactional, concurrency-safe and protected against duplicate execution.

# Phase 13 — Recommended build sequence

Build the ERP one phase at a time and do not progress until the previous phase passes its tests.

| Build stage | Delivery                       | Acceptance requirement               |
| ----------- | ------------------------------ | ------------------------------------ |
| 1           | Configuration and master data  | Customer and item ready              |
| 2           | Sales header                   | Create, edit and save                |
| 3           | Sales lines                    | All line calculations work           |
| 4           | Inventory link                 | Issue transactions created correctly |
| 5           | Reservation                    | Reserve/unreserve accurately         |
| 6           | Confirmation                   | Confirmation journal and report      |
| 7           | Basic picking                  | Optional picking process works       |
| 8           | Packing slip                   | Physical stock deduction correct     |
| 9           | Invoice                        | Financial posting reconciles         |
| 10          | Payment and settlement         | Customer balances reconcile          |
| 11          | Returns and cancellation       | Proper reversal and credit behavior  |
| 12          | Reporting and regression tests | Entire cycle reconciles              |

## 13.1 What each implementation phase must deliver

For every stage, use the same development discipline:

- Forms: fields, defaults, lookups, actions, editable rules and validation messages.
- Tables: standard D365-equivalent names, exact relevant fields, keys and relationships.
- Business logic: calculation sequence, status transitions and posting behavior.
- Inventory: physical and financial effects.
- Accounting: debit/credit impact where applicable.
- Test data: at least one normal case, one partial case and one failure case.
- Acceptance: validate database state before and after each operation.

For your own ERP, keep the domain model aligned with standard D365 tables, but use your application's transaction/service layer for writes. If targeting D365 itself, use supported X++ APIs, posting frameworks and extensions instead of direct database modifications.

## 13.2 Full-cycle reconciliation example

Use `ITEM-001`, quantity 10, sales price 100 SAR, cost 60 SAR.

Interactive transaction walkthrough

1 / 7

Select lifecycle stage

Order creation

Physical on-hand

# 100

PCS

Reserved physical

# 0

PCS

Delivered cumulative

# 0

PCS

Invoiced cumulative

# 0

PCS

Remaining physical delivery

10 / 10

Remaining financial invoicing

10 / 10

Inventory transaction status summary

On order

&#x20;PreviousNext&#x20;

Illustrative stocked-item scenario, with one warehouse, no receipts during fulfillment, full reservation before shipping and invoices limited to delivered quantities. Actual InventTrans splits and posting records depend on configuration.

## 14. Reference documentation

The blueprint is based on D365 Finance and Supply Chain Management concepts, not on the separate Dynamics 365 Sales/Dataverse order model.

- [Microsoft — Create sales orders](https://learn.microsoft.com/en-us/dynamics365/supply-chain/sales-marketing/tasks/create-sales-orders?utm_source=chatgpt.com)
- [Microsoft — Confirm sales orders](https://learn.microsoft.com/en-us/dynamics365/supply-chain/sales-marketing/tasks/confirm-sales-orders?utm_source=chatgpt.com)
- [Microsoft — Ship sales orders without warehousing](https://learn.microsoft.com/en-us/dynamics365/supply-chain/sales-marketing/tasks/ship-sales-orders-without-warehousing?utm_source=chatgpt.com)
- [Microsoft — Sales order posting](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/sales-order-posting?utm_source=chatgpt.com)
- [Microsoft — Sales returns](https://learn.microsoft.com/en-us/dynamics365/supply-chain/sales-marketing/sales-returns?utm_source=chatgpt.com)

Final implementation target: A user can create a standard stocked-item sales order, reserve inventory, confirm the order, deliver all or part of it, post the invoice, receive payment, and process cancellation or returns—while every standard sales, inventory and financial transaction remains consistent and auditable.

The next level of specification is the field-by-field implementation dictionary, particularly for `SalesTable`, `SalesLine`, `InventTrans`, `InventSum`, `CustConfirmJour/Trans`, `CustPackingSlipJour/Trans`, and `CustInvoiceJour/Trans`. That should include exact field types, mandatory conditions, lookup sources, change events, defaulting rules and the expected database state after each action.