# D365 Finance & Operations — Complete Core Inventory ERP Blueprint

Functional design + technical architecture + database tables + end-to-end business scenarios + implementation and testing roadmap

## 1. Objective and scope

Build a complete core Inventory Management module aligned with Microsoft Dynamics 365 Finance & Operations (D365 F&O), using standard D365 concepts, business rules, table structures, inventory statuses, posting behavior, and transaction lifecycle.

The objective is not merely to create inventory forms. It is to implement an interconnected ERP inventory system where every operation correctly affects stock quantities, inventory transactions, item costs, financial postings, and historical records.

Included in scope:

- Product and released-item creation.
- Inventory configuration, item groups, item model groups, units of measure.
- Sites, warehouses, storage dimensions, and optional batch/serial tracking.
- Inventory posting configuration and item cost management.
- Opening inventory balances.
- Purchase orders, product receipts, and vendor invoices.
- Inventory reservations and physical picking.
- Sales orders, packing slips, and customer invoices.
- Inventory movement, adjustment, transfer, and counting journals.
- Basic transfer orders between warehouses.
- Purchase and sales returns.
- On-hand inventory, inventory transaction history, and inventory costing.
- Inventory reconciliation, inventory recalculation, and period closing.
- Standard forms, validations, security, auditability, and integration testing.

Excluded from scope:

- Advanced Warehouse Management (WHS).
- Warehouse work, mobile scanning, waves, and license plates.
- Production orders, manufacturing, BOM processing, shop-floor operations.
- Transportation management, quality-management workflows, and advanced planning.
- Product variants and complex product configuration in the initial implementation.

Site and warehouse dimensions are still required for basic inventory. They do not mean that advanced WHS is enabled.

## 2. Complete inventory lifecycle

CORE INVENTORY BUSINESS FLOW

1\. Company & inventory configuration

Sites · Warehouses · Dimensions · Item groups · Posting

2\. Product master & released products

Item · UOM · Cost model · Default order settings

3A. Opening stock

Opening adjustment / movement

3B. Purchasing

PO → Product receipt → Invoice

4\. Available inventory

Physical on-hand · Available physical · Financial inventory

Stock transfers

Count & adjust

Reserve & pick

5\. Sales fulfillment

SO → Confirmation → Reservation → Picking → Packing slip → Invoice

6\. Returns, reconciliation & inventory close

Customer / vendor returns · Cost settlement · GL reconciliation

The inventory transaction ledger, `InventTrans`, is the central operational record connecting most stock-changing processes. The source document, inventory origin, dimensions, inventory posting, and cost calculations must remain consistent.

## 3. Implementation phases

Each phase has dependencies, standard D365 forms, impacted tables, example scenarios, and acceptance tests.

| Phase | Functionality                          | Outcome                            |
| ----- | -------------------------------------- | ---------------------------------- |
| 01    | Company and base configuration         | Inventory environment ready        |
| 02    | Inventory dimensions and warehouses    | Sites, warehouses, tracking        |
| 03    | Item groups, model groups, and posting | Inventory accounting configured    |
| 04    | Product and released-item setup        | Items ready for transactions       |
| 05    | Opening inventory                      | Initial stock and value            |
| 06    | Purchasing and receiving               | Stock received and invoiced        |
| 07    | On-hand and reservations               | Stock availability controlled      |
| 08    | Sales and inventory issue              | Stock sold and cost recognized     |
| 09    | Inventory journals                     | Movement, adjustment, counting     |
| 10    | Basic warehouse transfers              | Inventory moved between warehouses |
| 11    | Purchase and sales returns             | Returned stock and costs handled   |
| 12    | Inventory costing and closing          | Inventory valuation reconciled     |
| 13    | Reports, security, and audit           | Traceable and controlled inventory |
| 14    | Integration tests and go-live          | Complete working ERP cycle         |

We'll use a single example item across the phases so that quantity and cost changes can be tracked and reconciled.

## 4. Example test environment

| Configuration            | Example value    |
| ------------------------ | ---------------- |
| Company                  | USMF             |
| Item number              | ITEM-001         |
| Item name                | Cotton Uniform   |
| Product type             | Item             |
| Product subtype          | Product          |
| Inventory unit           | PCS              |
| Purchase unit            | BOX              |
| Sales unit               | PCS              |
| Purchase conversion      | 1 BOX = 10 PCS   |
| Item model group         | FIFO             |
| Item group               | FINISHED         |
| Storage dimension group  | Site + Warehouse |
| Tracking dimension group | None             |
| Site                     | S01              |
| Main warehouse           | WH-01            |
| Secondary warehouse      | WH-02            |
| Sample purchase price    | 50 SAR/PCS       |
| Sample sales price       | 80 SAR/PCS       |
| Initial stock            | 0                |

The example assumes stocked inventory, no advanced WHS, no batch/serial dimensions for the primary item, physical negative inventory disabled, and financial inventory based on FIFO. Accounting examples use simplified amounts, excluding tax, discounts, and additional charges unless explicitly specified.

These align with Microsoft's standard inventory and product configuration concepts.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+2



# Part I — Foundation and configuration

## Phase 01 — Company and inventory base configuration

Goal: Create the minimum company, accounting, and inventory infrastructure before any product or stock transaction is allowed.

### Step 1.1 — Legal entity

Standard navigation:

`Organization administration > Organizations > Legal entities`

| Field               | Example              | Purpose                 |
| ------------------- | -------------------- | ----------------------- |
| Company             | USMF                 | Legal entity identifier |
| Name                | Demo Trading Company | Company name            |
| Accounting currency | SAR                  | Financial reporting     |
| Country/region      | SA                   | Localization            |
| Fiscal calendar     | FY2026               | Accounting periods      |

Impacted standard tables

| Table                  | Purpose                    |
| ---------------------- | -------------------------- |
| `DataArea`             | Legal entity definition    |
| `Ledger`               | Ledger configuration       |
| `LedgerFiscalCalendar` | Ledger-to-calendar setup   |
| `FiscalCalendar`       | Fiscal calendar definition |

Company creation does not create inventory transactions. It only establishes the company context.

### Step 1.2 — Inventory parameters

Navigation:

`Inventory management > Setup > Inventory and warehouse management parameters`

Review the applicable settings for:

- Inventory reservation behavior.
- Physical and financial update posting.
- Ordered-item reservation.
- Number sequences.
- Counting and transfer defaults.
- Whether physical product receipt and packing slip postings create ledger vouchers.

Keep advanced warehouse processing disabled or unconfigured for the basic implementation.

Important: Inventory management parameters and item model groups work together. For example, physical inventory postings to the general ledger depend on multiple settings, not just one checkbox.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+1



### Phase 01 acceptance tests

| Test                                | Expected                        |
| ----------------------------------- | ------------------------------- |
| Select company USMF                 | Correct company context         |
| Open inventory parameters           | Configuration can be maintained |
| Validate accounting currency        | SAR                             |
| Validate fiscal period              | Posting date allowed            |
| Validate inventory number sequences | Document identifiers generated  |
| Switch to another legal entity      | Company-specific data separated |

## Phase 02 — Sites, warehouses, and inventory dimensions

D365 distinguishes three dimension categories:

1. Product dimensions: size, color, style, configuration.
2. Storage dimensions: site, warehouse, location, and other applicable storage attributes.
3. Tracking dimensions: batch and serial numbers.

For this basic ERP, implement site and warehouse as the required stock structure. Tracking dimensions are an optional but standard core extension.

### Step 2.1 — Create sites

Navigation:

`Inventory management > Setup > Inventory breakdown > Sites`

Example:

| Site | Name          |
| ---- | ------------- |
| S01  | Jeddah Main   |
| S02  | Riyadh Branch |

Standard table: `InventSite`

Important fields include `SiteId`, `Name`, and `DataAreaId`.

### Step 2.2 — Create warehouses

Navigation:

`Inventory management > Setup > Inventory breakdown > Warehouses`

Example:

| Warehouse  | Site | Description         |
| ---------- | ---- | ------------------- |
| WH-01      | S01  | Main Warehouse      |
| WH-02      | S01  | Secondary Warehouse |
| WH-03      | S02  | Riyadh Warehouse    |
| WH-TRANSIT | S01  | Transit Warehouse   |

Standard table: `InventLocation`

Important fields:

- `InventLocationId`
- `InventSiteId`
- `Name`
- `DataAreaId`

Example relationship:

```
InventSite
  SiteId = S01
      |
      +-- InventLocation WH-01
      |
      +-- InventLocation WH-02
      |
      +-- InventLocation WH-TRANSIT
```

A warehouse must belong to the correct site. You must not allow an inventory dimension combination that violates that relationship.

### Step 2.3 — Storage dimension group

Navigation:

`Product information management > Setup > Dimension and variant groups > Storage dimension groups`

Create or select:

`BASIC-SW`

Example configuration

| Dimension        | Active | Physical inventory | Financial inventory |
| ---------------- | ------ | ------------------ | ------------------- |
| Site             | Yes    | Yes                | Yes                 |
| Warehouse        | Yes    | Yes                | No                  |
| Location         | No     | No                 | No                  |
| Inventory status | No     | No                 | No                  |
| License plate    | No     | No                 | No                  |

The table is an illustrative configuration, not a universal required setup. Financial inventory by warehouse is a design choice with costing consequences.

Example: ITEM-001 can have 100 units in WH-01 and 50 in WH-02. The physical inventory must be distinguishable between warehouses, even when financial costing is pooled at the site level.

Relevant tables include:

- `InventDim`
- `InventDimParm`
- `EcoResStorageDimensionGroup`
- Standard storage dimension group assignment structures.

`InventDim` holds inventory dimension combinations. It is not a product master or a transaction header.

### Step 2.4 — Tracking dimension group

Navigation:

`Product information management > Setup > Dimension and variant groups > Tracking dimension groups`

Implement two standard configuration options:

| Group  | Batch | Serial | Use                     |
| ------ | ----- | ------ | ----------------------- |
| NONE   | No    | No     | Standard stock          |
| BATCH  | Yes   | No     | Batch-controlled stock  |
| SERIAL | No    | Yes    | Serial-controlled stock |

For the primary example, use `NONE`.

Separate test items should later cover batch and serial validation.

### Phase 02 acceptance tests

Create the three warehouses, validate their site assignments, and confirm that no inventory movement can reference an invalid warehouse.

Test that `InventDim` identifies the correct site/warehouse combination, without assuming a new `InventDim` row must be created on every document.

## Phase 03 — Inventory accounting and control configuration

This phase defines how stock behaves operationally and financially.

### Step 3.1 — Units of measure

Navigation:

`Organization administration > Setup > Units > Units`

Example units:

| Unit  | Symbol | Category |
| ----- | ------ | -------- |
| Piece | PCS    | Quantity |
| Box   | BOX    | Quantity |

Add a conversion:

`1 BOX = 10 PCS`

A purchase for 5 BOX must be converted into 50 PCS in inventory.

Standard table families:

- `UnitOfMeasure`
- `UnitOfMeasureConversion`
- `UnitOfMeasureTranslation`, where applicable

Do not treat the purchasing unit as a separate physical stock ledger. Inventory quantities must be standardized to the item's inventory unit.

### Step 3.2 — Item groups

Navigation:

`Inventory management > Setup > Inventory > Item groups`

Example:

| Group      | Description                                |
| ---------- | ------------------------------------------ |
| FINISHED   | Purchased finished goods                   |
| RAW        | Raw materials, if needed for trading stock |
| SPARES     | Spare parts                                |
| CONSUMABLE | Consumables                                |

Even though the example includes an optional `RAW` classification, no manufacturing processes are part of this implementation.

Standard table: `InventItemGroup`

Assignment table: `InventItemGroupItem`

Item groups help classify items and drive account selection through posting configuration.

### Step 3.3 — Item model groups

Navigation:

`Inventory management > Setup > Inventory > Item model groups`

Example:

| Setting                      | Value    |
| ---------------------------- | -------- |
| Group                        | FIFO     |
| Inventory model              | FIFO     |
| Stocked product              | Enabled  |
| Physical negative inventory  | Disabled |
| Financial negative inventory | Disabled |
| Post physical inventory      | Enabled  |
| Post financial inventory     | Enabled  |

The exact available options and labels depend on application version and item model.

Standard table: `InventModelGroup`

Assignment table: `InventModelGroupItem`

Business rules

- Stocked items participate in on-hand quantity.
- Physical negative inventory disabled means transactions cannot produce prohibited negative physical stock.
- Financial negative inventory settings control relevant financially updated issues.
- FIFO governs costing and settlement behavior, not the warehouse's physical picking sequence.
- Physical posting and financial posting are separate accounting stages for purchase and sales order flows.

### Step 3.4 — Inventory posting profiles

Navigation:

`Inventory management > Setup > Posting > Posting`

Example account design:

| Posting purpose                  | Sample main account |
| -------------------------------- | ------------------- |
| Inventory asset                  | 130100              |
| Product receipt accrual / offset | 210200              |
| Cost of goods sold               | 510100              |
| Inventory loss or adjustment     | 510200              |
| Inventory gain                   | 410200              |
| Inventory movement offset        | 590100              |

Actual posting types are configured on the appropriate posting profile tabs. They must not be reduced to a single universal `InventoryAccount` field.

Relevant standard tables include `InventPosting` and the applicable ledger/account-combination structures.

Accounting example

Receive 100 PCS at SAR 50 each:

\\[ 100 \times 50 = 5{,}000 \text{ SAR} \\]

At financial receipt, the simplified inventory capitalization is:

| Account                            | Debit | Credit |
| ---------------------------------- | ----- | ------ |
| Inventory asset                    | 5,000 | 0      |
| Receipt clearing / relevant offset | 0     | 5,000  |

The complete vendor invoice voucher can also include vendor liability, tax, accrual reversal, charges, and price differences according to configuration.

### Phase 03 acceptance tests

Validate that a stocked item cannot be transacted without the required model group and inventory dimensions. Test positive and negative journal posting account resolution, unit conversions, fiscal periods, and posting errors caused by missing accounts.

Microsoft reference: [Inventory posting profiles](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/inventory-posting-profiles?utm_source=chatgpt.com).

## Phase 04 — Product master and released product

This is the starting point for the operational item lifecycle.

### Step 4.1 — Create a product

Navigation:

`Product information management > Products > All products and product masters`

Select New and enter:

| Field           | Example        |
| --------------- | -------------- |
| Product type    | Item           |
| Product subtype | Product        |
| Product number  | ITEM-001       |
| Product name    | Cotton Uniform |
| Search name     | COTTON UNIFORM |

For the first release, create a distinct product, not a product master with variants.

### Step 4.2 — Release the product to a company

Release the shared product to legal entity `USMF`, or create it directly through the Released products page where supported.

Navigation:

`Product information management > Products > Released products`

Complete the company-specific fields:

| Field                      | Value    |
| -------------------------- | -------- |
| Item number                | ITEM-001 |
| Item group                 | FINISHED |
| Item model group           | FIFO     |
| Storage dimension group    | BASIC-SW |
| Tracking dimension group   | NONE     |
| Inventory unit             | PCS      |
| Purchase unit              | BOX      |
| Sales unit                 | PCS      |
| Default order type         | Purchase |
| Default site               | S01      |
| Default purchase warehouse | WH-01    |
| Default sales warehouse    | WH-01    |

### Standard product tables

| Table                                                 | Role                                                            |
| ----------------------------------------------------- | --------------------------------------------------------------- |
| `EcoResProduct`                                       | Shared product definition                                       |
| `EcoResProductTranslation`                            | Product name and translated descriptions                        |
| `InventTable`                                         | Released item in a legal entity                                 |
| `InventTableModule`                                   | Purchase, sales, and inventory module-specific item values      |
| `InventItemGroupItem`                                 | Item group assignment                                           |
| `InventModelGroupItem`                                | Item model group assignment                                     |
| `InventTable` and related dimension-group assignments | Item inventory dimension setup                                  |
| `InventItemLocation`                                  | Item-specific location/default inventory setup where applicable |
| `EcoResProductCategory`                               | Product category assignment, if used                            |

### Example of records and relationships

EcoResProduct

RecId = 100001 · DisplayProductNumber = ITEM-001

InventTable

ItemId = ITEM-001 · Product = 100001 · DataAreaId = USMF

InventTableModule

Inventory, purchase and sales unit/pricing records

Item setup links

Item group · Model group · Dimension groups

The `RecId` above is an illustrative identifier, not a predetermined D365-generated value.

### Critical validations

1. The product identifier must be unique within its defined scope.
2. An item must not be usable in stock transactions before essential setup is complete.
3. Inventory unit and conversions must be valid.
4. A dimension group should not be changed casually after transactions exist.
5. Shared-product data and legal-entity released-item data must be maintained separately.
6. Product creation must not generate opening stock.

Expected result: ITEM-001 is ready for purchasing, selling, counting, and journal transactions, but its on-hand quantity remains zero.

D365 supports the shared-product/released-product distinction explicitly.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+1



# Part II — Inventory transactions and document lifecycles

## Phase 05 — Opening stock

Use standard inventory journals to establish the initial stock balance and value during migration or go-live.

### Scenario A — Opening balance of 100 PCS

Assume the item has no existing stock.

| Field       | Value     |
| ----------- | --------- |
| Item        | ITEM-001  |
| Site        | S01       |
| Warehouse   | WH-01     |
| Quantity    | +100 PCS  |
| Unit cost   | SAR 50    |
| Total value | SAR 5,000 |

Navigation:

`Inventory management > Journal entries > Items > Movement`

Choose an approved opening-balance journal name with the appropriate ledger offset account. A suitable inventory adjustment journal may be used instead according to the migration and accounting policy.

Steps:

1. Create the journal header.
2. Add ITEM-001 with dimensions S01 / WH-01.
3. Enter +100 PCS.
4. Specify the opening unit cost and required offset account.
5. Validate the journal.
6. Post the journal.
7. Review inventory transactions, on-hand, and vouchers.

### Impacted tables

| Table                        | Operation                       |
| ---------------------------- | ------------------------------- |
| `InventJournalTable`         | Journal header creation         |
| `InventJournalTrans`         | Journal line creation           |
| `InventTransOrigin`          | Inventory transaction origin    |
| `InventTrans`                | Inventory receipt transaction   |
| `InventDim`                  | Dimension combination reference |
| `InventSum`                  | On-hand balance updated         |
| `GeneralJournalEntry`        | Ledger journal entry            |
| `GeneralJournalAccountEntry` | Posted accounting lines         |

### Expected physical and financial outcome

Physical stock

# 100

PCS

Stock value

## 5,000

SAR

Issue reservations

# 0

PCS

For a standard posted positive movement journal, the inventory receipt is physically and financially updated together. Its receipt status is normally `Purchased`.

Negative tests: duplicate opening balances, zero quantity, negative quantity that violates settings, missing offset account, missing cost, incorrect warehouse, and reposting an already posted journal.

## Phase 06 — Purchase order to inventory receipt to vendor invoice

This is the core inbound purchasing cycle.

### Scenario B — Purchase 50 PCS at SAR 60

Start with 100 PCS opening stock at SAR 50.

Create a purchase order:

| Field                | Value       |
| -------------------- | ----------- |
| Vendor               | V-001       |
| Purchase order       | PO-0001     |
| Item                 | ITEM-001    |
| Order quantity       | 5 BOX       |
| Inventory equivalent | 50 PCS      |
| Purchase unit price  | SAR 600/BOX |
| Total cost           | SAR 3,000   |
| Site                 | S01         |
| Warehouse            | WH-01       |

### Step 6.1 — Create purchase order

Navigation:

`Procurement and sourcing > Purchase orders > All purchase orders`

Create PO-0001 and its line.

Affected tables:

- `PurchTable`
- `PurchLine`
- `InventTransOrigin`
- `InventTrans`
- `InventDim`

Expected inventory transaction:

| Field          | Value      |
| -------------- | ---------- |
| Direction      | Receipt    |
| Quantity       | +50 PCS    |
| Receipt status | Ordered    |
| Physical date  | Not posted |
| Financial date | Not posted |

There is no increase in physical on-hand merely because a purchase order is created.

### Step 6.2 — Confirm purchase order

Choose the purchase order confirmation action.

Typical document history:

- `VendPurchOrderJour`
- `VendPurchOrderTrans`

The purchase order line and its inventory reference remain linked.

Inventory impact: No physical inventory receipt or financial capitalization from confirmation alone.

### Step 6.3 — Post product receipt

Choose:

`Receive > Product receipt`

Enter supplier packing slip/reference number and receive 50 PCS.

Affected tables:

| Table                  | Purpose                                 |
| ---------------------- | --------------------------------------- |
| `VendPackingSlipJour`  | Product receipt header                  |
| `VendPackingSlipTrans` | Product receipt line                    |
| `InventTrans`          | Receipt status and physical details     |
| `InventSum`            | Physical stock                          |
| `InventDim`            | Inventory dimension reference           |
| `InventTransPosting`   | Inventory posting detail, as applicable |

Inventory status becomes `Received`.

Physical inventory increases by 50 PCS.

### Step 6.4 — Post vendor invoice

Choose:

`Invoice > Invoice`

Post the vendor invoice for SAR 3,000, excluding tax.

Typical affected tables:

- `VendInvoiceJour`
- `VendInvoiceTrans`
- `VendTrans`
- `InventTrans`
- `InventTransPosting`
- `GeneralJournalEntry`
- `GeneralJournalAccountEntry`

Inventory receipt status becomes `Purchased`.

The inventory cost is now financially updated. Actual ledger vouchers depend on posting profiles and accounting settings.

### Inventory result

Inventory quantity through purchase cycle

ITEM-001, WH-01, quantities in PCS

Physical on-hand

Available physical

04080120160OpeningPO createdPO confirmedReceivedInvoiced

### Purchase edge cases

| Case                                    | Expected behavior                                               |
| --------------------------------------- | --------------------------------------------------------------- |
| PO 50, receive 20                       | Only 20 added physically                                        |
| Receive remaining 30 later              | Cumulative receipt becomes 50                                   |
| Invoice 20 before remaining 30 received | Apply invoice and matching-policy rules; do not invent receipts |
| Invoice price differs from PO           | Financial cost adjusted under configured rules                  |
| Over-receipt                            | Enforce overdelivery tolerance                                  |
| Wrong warehouse                         | Validate correct dimensions and corrective process              |
| Duplicate product receipt reference     | Apply configured duplicate controls                             |
| PO canceled before receipt              | No physical stock                                               |
| PO line reduced after partial receipt   | Enforce remaining and received-quantity restrictions            |
| Item uses batch tracking                | Require/resolve valid batch dimensions                          |

The separation between Ordered, Received, and Purchased is essential to D365 inventory accounting.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+1



## Phase 07 — On-hand inventory and reservations

### Step 7.1 — On-hand inquiry

Navigation:

`Inventory management > Inquiries and reports > On-hand list`

The screen must support filters for:

- Item number.
- Site.
- Warehouse.
- Optional batch or serial dimensions.
- Physical inventory.
- Reserved physical.
- Available physical.
- Ordered receipts.
- Ordered issues.
- Total available.

Standard primary table: `InventSum`, summarized by item and inventory dimensions.

### Important inventory quantity definitions

\\[ \text{Available physical} = \text{Physical inventory} - \text{Physical reserved} \\]

\\[ \text{Total available} = \text{Available physical} + \text{Ordered in total} - \text{On order} \\]

These are D365 on-hand concepts; `Total available` is not synonymous with physical stock currently available for picking.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



### Step 7.2 — Reserve 30 PCS for a sales order

Assume physical stock is 150 PCS and no prior reservations exist.

Before reservation:

| Quantity           | Value |
| ------------------ | ----- |
| Physical on-hand   | 150   |
| Reserved physical  | 0     |
| Available physical | 150   |

After reservation:

| Quantity           | Value |
| ------------------ | ----- |
| Physical on-hand   | 150   |
| Reserved physical  | 30    |
| Available physical | 120   |

Inventory value remains unchanged. Reserving stock does not physically deliver or financially sell the item.

Inventory issue status changes from `On order` to `Reserved physical` for the reserved transaction quantity. In a partial reservation, one source line can be represented by multiple inventory transaction records with different statuses.

Relevant tables:

- `SalesLine`
- `InventTransOrigin`
- `InventTrans`
- `InventSum`
- `InventDim`

### Reservation test cases

- Full reservation.
- Partial reservation.
- Attempt to reserve more than physically available.
- Release reservation.
- Reservation from another warehouse.
- Multiple sales orders competing for the same stock.
- Concurrent reservations by different users.
- Reserved-ordered stock, only when enabled.
- Picking after reservation.
- Cancellation of reserved sales lines.

The system must execute reservations transactionally so that concurrent users cannot allocate the same available quantity beyond the permitted limit.

## Phase 08 — Sales order to invoice

### Scenario C — Sell 30 PCS at SAR 80

We assume 150 PCS are in WH-01, and 30 PCS will be sold.

| Field            | Value       |
| ---------------- | ----------- |
| Sales order      | SO-0001     |
| Customer         | C-001       |
| Item             | ITEM-001    |
| Quantity         | 30 PCS      |
| Sales price      | SAR 80      |
| Revenue          | SAR 2,400   |
| Site / Warehouse | S01 / WH-01 |

### Step 8.1 — Create sales order

Navigation:

`Sales and marketing > Sales orders > All sales orders`

Create the header and line.

Tables:

- `SalesTable`
- `SalesLine`
- `InventTransOrigin`
- `InventTrans`
- `InventDim`

Expected issue status: `On order`.

No physical stock deduction or financial COGS posting occurs merely from sales order creation.

### Step 8.2 — Sales confirmation

Choose:

`Sell > Generate > Confirmation`

Typical document tables:

- `CustConfirmJour`
- `CustConfirmTrans`

Critical business rule: Sales confirmation does not physically issue stock.

It creates a confirmed commercial document and updates relevant order status/history, but stock is not deducted solely by confirming.

### Step 8.3 — Reserve inventory

Reserve 30 PCS.

`InventTrans.StatusIssue = Reserved physical`

Physical inventory remains 150 PCS.

Available physical becomes 120 PCS.

### Step 8.4 — Pick inventory

Use the basic sales-order picking process, not WHS work or mobile picking.

Depending on the configured picking workflow, basic picking list registration uses standard picking list journal records such as:

- `WMSPickingRoute`
- `WMSOrderTrans`

These standard basic picking-list tables should not be confused with advanced warehouse work (`WHSWorkTable`, `WHSWorkLine`).

Expected inventory status: `Picked`.

Stock is physically present but no longer available for normal allocation. Picking is not a financial sale.

### Step 8.5 — Post packing slip

Choose:

`Pick and pack > Generate > Packing slip`

Affected tables:

| Table                  | Purpose                                   |
| ---------------------- | ----------------------------------------- |
| `CustPackingSlipJour`  | Packing slip header                       |
| `CustPackingSlipTrans` | Packing slip lines                        |
| `InventTrans`          | Physical issue                            |
| `InventSum`            | On-hand quantity update                   |
| `InventTransPosting`   | Physical posting records where applicable |

Issue status: `Deducted`.

Physical stock changes:

\\[ 150 - 30 = 120 \text{ PCS} \\]

### Step 8.6 — Post sales invoice

Choose:

`Invoice > Generate > Invoice`

Affected tables:

- `CustInvoiceJour`
- `CustInvoiceTrans`
- `CustTrans`
- `InventTrans`
- `InventTransPosting`
- `GeneralJournalEntry`
- `GeneralJournalAccountEntry`

Issue status: `Sold`.

Revenue excluding tax:

\\[ 30 \times 80 = 2{,}400 \text{ SAR} \\]

With FIFO, assume the first 30 units come from the opening stock layer at SAR 50.

\\[ \text{COGS} = 30 \times 50 = 1{,}500 \\]

\\[ \text{Gross profit} = 2{,}400-1{,}500=900 \\]

This is the expected settled FIFO result under the given assumptions. Initial cost posting may use running-average cost until recalculation or inventory close.

### Step-by-step status visualization

1

Sales order

Status: On order

Physical 150 · Available 150

2

Reserve 30

Status: Reserved physical

Physical 150 · Available 120

3

Pick 30

Status: Picked

Physical 150 · Available 120

4

Packing slip

Status: Deducted

Physical 120 · Available 120

5

Invoice

Status: Sold

Physical 120 · Financially issued

### Accounting impact

At the final financial posting stage, the simplified entries are:

| Account             | Debit | Credit |
| ------------------- | ----- | ------ |
| Customer receivable | 2,400 | —      |
| Sales revenue       | —     | 2,400  |
| Cost of goods sold  | 1,500 | —      |
| Inventory asset     | —     | 1,500  |

Tax and any earlier physical-posting vouchers are omitted here to show the final economic effect.

### Sales edge cases

1. Partial shipment: SO 30, packing slip 10, then 20.
2. Partial invoice: only the eligible shipped quantities.
3. Reserve 20 of 30, with the remainder on order.
4. Cancel order before delivery.
5. Modify sales quantity after confirmation.
6. Change warehouse on an unfulfilled line.
7. Change inventory unit or sales unit and recalculate conversions.
8. Ship from two warehouses by splitting lines/dimensions.
9. Attempt to ship more than allowed.
10. Post a customer credit note and return.
11. Reverse/correct a posted packing slip using supported functionality.
12. Detect duplicate invoice posting.
13. Test with and without physical ledger posting enabled.
14. Verify multiple inventory transactions for one sales line.

Inventory status definitions are documented by Microsoft.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



## Phase 09 — Inventory journals

Inventory journals are needed even when purchasing and sales are already implemented. They handle adjustments, internal transfers, stock counts, and non-order receipts/issues.

Microsoft differentiates movement, inventory adjustment, transfer, counting, and other journal types. For this ERP, use the four basic types below.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



### 9.1 Inventory movement journal

Navigation:

`Inventory management > Journal entries > Items > Movement`

Use when inventory must be added or removed with a specified general ledger offset.

Example — Issue damaged stock

| Field                | Value          |
| -------------------- | -------------- |
| Item                 | ITEM-001       |
| Warehouse            | WH-01          |
| Quantity             | -5 PCS         |
| Reason               | Damaged goods  |
| Offset account       | Inventory loss |
| FIFO cost assumption | 50 SAR/PCS     |

Quantity decreases by 5.

Simplified accounting:

- Debit inventory loss: SAR 250.
- Credit inventory asset: SAR 250.

A positive movement journal can also create stock, subject to valid costing, posting configuration, and authorization.

### 9.2 Inventory adjustment journal

Navigation:

`Inventory management > Journal entries > Items > Inventory adjustment`

Example: inventory is physically found, adding 3 PCS.

| Field           | Value    |
| --------------- | -------- |
| Item            | ITEM-001 |
| Quantity        | +3       |
| Warehouse       | WH-01    |
| Unit cost       | SAR 50   |
| Inventory value | +SAR 150 |

Unlike a movement journal with a manually specified ledger offset, the inventory adjustment journal relies on the configured inventory posting rules for its offset.

### 9.3 Inventory transfer journal

Navigation:

`Inventory management > Journal entries > Items > Transfer`

Example:

`WH-01 → WH-02`, 20 PCS.

Before:

| Warehouse | Qty |
| --------- | --- |
| WH-01     | 100 |
| WH-02     | 0   |
| Total     | 100 |

After:

| Warehouse | Qty |
| --------- | --- |
| WH-01     | 80  |
| WH-02     | 20  |
| Total     | 100 |

The transfer journal produces an issue for the source dimensions and a receipt for the destination dimensions. It does not maintain in-transit stock as a separate operational stage.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



Standard tables:

- `InventJournalTable`
- `InventJournalTrans`
- `InventTransOrigin`
- `InventTrans`
- `InventDim`
- `InventSum`
- Accounting posting tables when a financial voucher is required.

The total company quantity must remain unchanged.

### 9.4 Counting journal

Navigation:

`Inventory management > Journal entries > Item counting > Counting`

Example:

| Field              | Quantity |
| ------------------ | -------- |
| System on-hand     | 80       |
| Physically counted | 77       |
| Difference         | -3       |

Upon posting:

- Physical stock decreases by 3.
- The inventory variance is accounted for using inventory posting rules.
- A posted adjustment transaction is created.
- The count remains traceable to its journal.

Relevant additional setup includes counting groups, counting policies, and journal names.

### Journal comparison

| Journal type         | Quantity effect                             | Main use                        |
| -------------------- | ------------------------------------------- | ------------------------------- |
| Movement             | Increase/decrease                           | Explicit offset account         |
| Inventory adjustment | Increase/decrease                           | Stock quantity/value correction |
| Transfer             | Source decrease, destination increase       | Immediate internal movement     |
| Counting             | Difference between counted and system stock | Physical count reconciliation   |

### Journal technical requirements

Implement posting as a controlled, atomic business operation.

1. Validate journal status and lines.
2. Validate site and warehouse dimensions.
3. Validate units and conversions.
4. Enforce inventory model and negative inventory policies.
5. Validate financial posting profiles.
6. Generate inventory origins and transactions.
7. Update physical/financial inventory correctly.
8. Generate accounting records.
9. Commit only when the entire posting succeeds.
10. Block duplicate posting.

Do not directly update `InventSum` to post a journal. On-hand balances must result from legitimate inventory posting operations.

## Phase 10 — Basic transfer orders

A transfer order differs from an inventory transfer journal.

A transfer journal is immediate. A transfer order supports the document lifecycle of shipping from one warehouse and receiving into another.

### Scenario D — Transfer 40 PCS

| Field                 | Value      |
| --------------------- | ---------- |
| Transfer order        | TO-0001    |
| Item                  | ITEM-001   |
| Source warehouse      | WH-01      |
| Destination warehouse | WH-02      |
| Transit warehouse     | WH-TRANSIT |
| Quantity              | 40 PCS     |

### Step 10.1 — Create transfer order

Navigation:

`Inventory management > Inbound orders > Transfer order`

Standard primary tables:

- `InventTransferTable`
- `InventTransferLine`
- `InventTransOrigin`
- `InventTrans`
- `InventDim`

At order creation, expected transfer issue and receipt transactions are established. Stock is not physically moved merely by creating the transfer order.

### Step 10.2 — Ship transfer

Post the transfer shipment.

Expected effect:

- WH-01 decreases by 40.
- Transit stock increases by 40.
- WH-02 does not yet increase.
- Transfer shipment document is recorded.

### Step 10.3 — Receive transfer

Post receipt at WH-02.

Expected effect:

- Transit stock decreases by 40.
- WH-02 increases by 40.
- Transfer order quantities/statuses are updated.

Microsoft's standard transfer-order posting can involve four inventory transactions after shipment, representing source, transit, and destination movements.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



### Warehouse balance example

40-unit transfer through transit

Physical inventory by warehouse, example

WH-01

Transit

WH-02

0306090120Before shipmentAfter shipmentAfter receipt

### Additional standard tables to inspect

- `InventTransferJour`
- `InventTransferParmTable`
- `InventTransferParmLine`
- `InventTransPosting`
- Relevant standard transfer shipping/receiving document records.

Do not equate transfer order shipment with an ordinary single negative journal line.

### Transfer tests

- Full shipment and receipt.
- Partial shipment.
- Partial receipt.
- Multiple shipment batches.
- Ship quantity exceeds permitted quantity.
- Stock insufficient at source.
- Source and destination identical.
- Different sites.
- Stock left in transit at period end.
- Cancellation before shipment.
- Quantity variance or shortage during receipt.
- Accounting and valuation by dimension group.
- Reconciliation of source + transit + destination.

## Phase 11 — Purchase and sales returns

Returns reverse the direction of the original business flow but still require their own inventory transactions and financial documents.

### 11.1 Customer sales return

Original sale:

- 10 PCS sold.
- Unit sales price: SAR 80.
- Original FIFO unit cost: SAR 50.

Customer returns 2 PCS.

Use a standard sales return/return order workflow with the required return reason and disposition.

Typical lifecycle:

`Return order → Physical receipt → Credit note`

Expected effects when returned goods are accepted into stock:

- Inventory quantity increases by 2.
- A receipt transaction is physically updated.
- Financial posting of the credit note completes the return's financial stage.
- Revenue/receivable and cost effects are adjusted according to return accounting rules.

Relevant tables:

- `SalesTable`
- `SalesLine`
- `ReturnTable`, where applicable
- `InventTrans`
- `CustPackingSlipJour` / `CustPackingSlipTrans`
- `CustInvoiceJour` / `CustInvoiceTrans`
- `InventSum`

Do not treat every customer return as immediately sellable stock. A damaged return can require an appropriate disposition or subsequent adjustment.

### 11.2 Purchase return

Original PO receipt: 50 PCS.

Return 5 PCS to the vendor.

A standard purchase return order uses a negative purchase quantity and a vendor credit-note process.

Physical effect:

- Inventory decreases by 5 when the return is physically posted.
- The inventory issue becomes financially updated through the applicable vendor credit processing.
- Vendor balance and inventory cost are adjusted.

Relevant tables:

- `PurchTable`
- `PurchLine`
- `InventTrans`
- `VendPackingSlipJour` / `VendPackingSlipTrans`
- `VendInvoiceJour` / `VendInvoiceTrans`
- `VendTrans`
- `InventSum`

### Return scenarios to test

| Scenario                             | Validation                                      |
| ------------------------------------ | ----------------------------------------------- |
| Return all sold quantity             | Full reversal flow                              |
| Partial return                       | Only returned quantity changes                  |
| Return damaged goods                 | Stock disposition controlled                    |
| Customer credit without stock return | Financial-only correction where supported       |
| Vendor return before vendor invoice  | Correct matching and issue lifecycle            |
| Return from a different warehouse    | Correct warehouse accounting and controls       |
| Return after inventory close         | Cost correction/settlement rules                |
| Return with batch/serial             | Matching tracking dimensions                    |
| Duplicate return posting             | Prevent incorrect duplicate inventory movements |

# Part III — Costing, valuation, and closing

## Phase 12 — Inventory costing

A core ERP cannot be considered complete without inventory cost management.

For the initial implementation, support one costing model thoroughly before adding alternative costing models. FIFO is a suitable reference implementation.

### 12.1 Inventory valuation scenario

Assume the following transactions:

| Transaction        | Qty  | Unit cost |
| ------------------ | ---- | --------- |
| Purchase receipt A | +100 | 50 SAR    |
| Purchase receipt B | +50  | 60 SAR    |
| Sales issue        | -30  | FIFO      |

Before issue, inventory value:

\\[ (100\times50)+(50\times60)=8{,}000 \\]

Under FIFO settlement, first receipt layer supplies the 30 issued units.

\\[ COGS=30\times50=1{,}500 \\]

Remaining ending inventory:

\\[ 70\times50+50\times60=6{,}500 \\]

Closing quantity

# 120

PCS

Closing value

## 6,500

SAR

Settled COGS

## 1,500

SAR

These results are for the illustrated FIFO layers after appropriate financial updates and settlement.

### 12.2 Physical versus financial costing

D365 separates physical and financial inventory updates. A product receipt may exist before the vendor invoice, and a packing slip may exist before the customer invoice.

The distinction affects:

- `InventTrans.DatePhysical`
- `InventTrans.DateFinancial`
- `InventTrans.CostAmountPhysical`
- `InventTrans.CostAmountPosted`
- Cost adjustments.
- Financial posting and inventory valuation.
- Inventory closing/settlement.

Not every physical inventory update creates a ledger voucher. That depends on posting configuration.

### 12.3 Inventory recalculation

Navigation:

`Inventory management > Periodic tasks > Closing and adjustment`

Inventory recalculation adjusts running costs without completing the final inventory settlements performed by inventory close.

### 12.4 Inventory close

Inventory close settles financially updated issues against receipts according to the inventory model.

Key standard tables include:

- `InventTrans`
- `InventSettlement`
- `InventClosing`
- `InventTransPosting`
- General ledger posting structures.

Microsoft explicitly notes that FIFO and other periodic inventory models can initially post issues at a running-average cost, with inventory close making subsequent settlements and adjustments.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



### 12.5 Closing controls

Before closing a period:

1. Review outstanding physical receipts and unposted vendor invoices.
2. Review delivered but uninvoiced sales.
3. Investigate negative quantities and cost anomalies.
4. Reconcile inventory subledger value with relevant general ledger balances.
5. Review inventory journal postings.
6. Execute recalculation if required.
7. Run inventory close.
8. Review settlements and cost adjustments.
9. Restrict posting to closed periods as appropriate.
10. Test the standard reversal/reopen process under controlled conditions.

Never implement FIFO simply by changing the item's average cost field. FIFO requires a transaction-level costing and settlement model.

# Part IV — Technical architecture and database alignment

## Phase 13 — Standard D365 F&O table architecture

The ERP implementation should follow D365's separation between master data, business documents, inventory transactions, on-hand summaries, and financial postings.

### 13.1 Table relationship diagram

Conceptual dependency view. Actual foreign keys and posting relationships are more extensive; InventDim references dimension combinations, while InventSum represents aggregated on-hand balances.

### 13.2 Master table inventory

| Domain          | Standard table             | Responsibility                    |
| --------------- | -------------------------- | --------------------------------- |
| Products        | `EcoResProduct`            | Product definition                |
| Products        | `EcoResProductTranslation` | Names/descriptions                |
| Released items  | `InventTable`              | Company-specific item             |
| Item properties | `InventTableModule`        | Purchase, sales, inventory values |
| Item groups     | `InventItemGroup`          | Classification                    |
| Item groups     | `InventItemGroupItem`      | Item assignment                   |
| Cost control    | `InventModelGroup`         | Inventory model behavior          |
| Cost control    | `InventModelGroupItem`     | Model group assignment            |
| UOM             | `UnitOfMeasure`            | Units                             |
| UOM             | `UnitOfMeasureConversion`  | Unit conversions                  |
| Site            | `InventSite`               | Site definitions                  |
| Warehouse       | `InventLocation`           | Warehouse definitions             |
| Dimensions      | `InventDim`                | Dimension combinations            |
| Posting         | `InventPosting`            | Inventory account determination   |

### 13.3 Transaction table inventory

| Business event        | Main tables                                                             |
| --------------------- | ----------------------------------------------------------------------- |
| Purchase order        | `PurchTable`, `PurchLine`                                               |
| PO confirmation       | `VendPurchOrderJour`, `VendPurchOrderTrans`                             |
| Product receipt       | `VendPackingSlipJour`, `VendPackingSlipTrans`                           |
| Vendor invoice        | `VendInvoiceJour`, `VendInvoiceTrans`                                   |
| Sales order           | `SalesTable`, `SalesLine`                                               |
| SO confirmation       | `CustConfirmJour`, `CustConfirmTrans`                                   |
| Picking list          | `WMSPickingRoute`, `WMSOrderTrans` in the applicable basic picking flow |
| Packing slip          | `CustPackingSlipJour`, `CustPackingSlipTrans`                           |
| Customer invoice      | `CustInvoiceJour`, `CustInvoiceTrans`                                   |
| Inventory journal     | `InventJournalTable`, `InventJournalTrans`                              |
| Transfer order        | `InventTransferTable`, `InventTransferLine`                             |
| Inventory transaction | `InventTransOrigin`, `InventTrans`                                      |
| On-hand               | `InventSum`, `InventDim`                                                |
| Inventory settlement  | `InventSettlement`, `InventClosing`                                     |
| General ledger        | `GeneralJournalEntry`, `GeneralJournalAccountEntry`                     |

Use standard D365 names and relationships as the reference model. Some table types, fields, and relationships vary by release and enabled features; verify against the target environment's AOT metadata.

### 13.4 Essential InventTrans fields

| Field                  | Purpose                       |
| ---------------------- | ----------------------------- |
| `RecId`                | Transaction record identifier |
| `ItemId`               | Item number                   |
| `InventTransOrigin`    | Origin reference              |
| `InventDimId`          | Inventory dimensions          |
| `Qty`                  | Signed inventory quantity     |
| `StatusReceipt`        | Receipt lifecycle status      |
| `StatusIssue`          | Issue lifecycle status        |
| `DatePhysical`         | Physical update date          |
| `DateFinancial`        | Financial update date         |
| `CostAmountPhysical`   | Physical cost amount          |
| `CostAmountPosted`     | Financially posted cost       |
| `CostAmountAdjustment` | Cost adjustment               |

Critical implementation rule: `InventTrans` should not be manually created as an independent stock record without its source-document and posting lifecycle. The posting logic must maintain the associated origin, on-hand balance, voucher, and cost relationships.

# Part V — Business-rule and test coverage

## Phase 14 — Comprehensive test matrix

A complete ERP must be tested by business case, not only by opening each screen.

### Interactive coverage checklist

Implementation test coverage

0 of 41 tests completed

## 0%

1\. Product

0/5

Create a distinct product

Release product into company

Validate item groups, units, dimensions

Disable or block item transactions

Test duplicate and incomplete items

2\. Purchase

0/6

Create and confirm purchase order

Partial product receipt

Full product receipt

Vendor invoice at same price

Vendor invoice price variance

Overdelivery and cancellation

3\. Availability

0/5

Display site/warehouse on-hand

Partial and full reservation

Unreserve stock

Concurrent reservation conflict

Negative-inventory restriction

4\. Sales

0/6

Create and confirm sales order

Pick reserved stock

Post partial packing slip

Post full packing slip

Post customer invoice

Validate physical vs financial effects

5\. Journals

0/5

Positive and negative movement

Positive and negative adjustment

Transfer journal

Counting journal and variance

Duplicate-posting prevention

6\. Transfer orders

0/5

Create transfer order

Partial shipment

Transit-stock validation

Partial and full receipt

Reconcile all warehouse quantities

7\. Returns

0/4

Customer return and credit

Purchase return and vendor credit

Damaged returned goods

Partial and duplicate returns

8\. Finance & close

0/5

Validate FIFO settlements

Inventory recalculation

Inventory close

Inventory-to-ledger reconciliation

Posting-period restrictions

&#x20;Copy test checklist

## Non-negotiable inventory invariants

These requirements should be verified in every transaction implementation.

| Invariant             | Requirement                                                        |
| --------------------- | ------------------------------------------------------------------ |
| Quantity integrity    | Physical changes reconcile with posted inventory transactions      |
| Dimension integrity   | Stock belongs to valid site/warehouse combinations                 |
| Reservation integrity | Reserved quantity cannot exceed permitted availability             |
| Cost integrity        | Financial cost follows the configured inventory model              |
| Posting integrity     | No partial commit after posting failure                            |
| Traceability          | Every inventory transaction has an appropriate source reference    |
| Idempotency           | Repeating a posting request must not double-post                   |
| Accounting            | Financial inventory movement reconciles with applicable GL posting |
| Security              | Unauthorized users cannot post adjustments or close inventory      |
| Audit                 | Posted transactions and document histories are retained            |
| Company separation    | Transactions respect legal entity boundaries                       |
| Reconciliation        | `InventSum` and `InventTrans` remain consistent                    |

Microsoft specifically identifies direct updates to inventory transaction data as a potential cause of `InventSum` inconsistencies.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



# Part VI — Complete worked example

This scenario connects purchasing, sales, inventory journals, transfer orders, and returns.

Use ITEM-001 with opening stock of zero and assume each cost event is financially completed before final FIFO settlement.

## 15. Full inventory-cycle transaction ledger

| #  | Operation                 | WH-01 | WH-02 | Transit | Total |
| -- | ------------------------- | ----- | ----- | ------- | ----- |
| 0  | Initial state             | 0     | 0     | 0       | 0     |
| 1  | Opening +100              | 100   | 0     | 0       | 100   |
| 2  | Purchase receipt +50      | 150   | 0     | 0       | 150   |
| 3  | Reserve 30                | 150   | 0     | 0       | 150   |
| 4  | Sales packing slip -30    | 120   | 0     | 0       | 120   |
| 5  | Transfer journal 20       | 100   | 20    | 0       | 120   |
| 6  | Damaged stock -5          | 95    | 20    | 0       | 115   |
| 7  | Counting gain +3          | 98    | 20    | 0       | 118   |
| 8  | Transfer order ship 40    | 58    | 20    | 40      | 118   |
| 9  | Transfer order receive 40 | 58    | 60    | 0       | 118   |
| 10 | Customer returns 2        | 60    | 60    | 0       | 120   |
| 11 | Vendor return 5           | 55    | 60    | 0       | 115   |

End-to-end inventory quantity evolution

Company total physical inventory, PCS

04590135180StartPurchaseSaleDamageCountTransfer receiveVendor return

Final quantities:

WH-01

# 55

PCS

WH-02

# 60

PCS

Company total

# 115

PCS

The acceptance criterion is not simply that the final number is 115. The ERP must also prove that every movement is traceable to its document, inventory dimensions, inventory origin, cost transaction, and applicable financial posting.

## 16. Forms that must be fully functional

These are the minimum required functional areas and forms, using standard D365 pages as the reference.

| Menu / form            | Required operations                           |
| ---------------------- | --------------------------------------------- |
| Released products      | Create, edit, view, release                   |
| Product details        | Assign groups, units, dimensions              |
| Item model groups      | Maintain cost/stock behavior                  |
| Item groups            | Maintain item classification                  |
| Sites                  | Create and maintain                           |
| Warehouses             | Create and maintain                           |
| Inventory posting      | Maintain ledger account mappings              |
| On-hand list           | Filter, group, inspect stock                  |
| Inventory transactions | Drill down into receipt/issue                 |
| Movement journals      | Create, validate, post                        |
| Adjustment journals    | Create, validate, post                        |
| Transfer journals      | Create, validate, post                        |
| Counting journals      | Count, adjust, post                           |
| Purchase orders        | Create, confirm, receive, invoice             |
| Sales orders           | Create, confirm, reserve, pick, ship, invoice |
| Transfer orders        | Create, ship, receive                         |
| Purchase returns       | Return stock, credit                          |
| Sales returns          | Receive stock, credit                         |
| Closing and adjustment | Recalculate and close inventory               |

Each form needs proper lookup filtering, field validation, state transitions, posting permissions, and correct table integration.

A working screen with a Save button is not sufficient. It must participate correctly in the standard end-to-end transaction model.

## 17. Suggested implementation order for your ERP project

### 1

Milestone 1

Core master data

Company, sites, warehouses, UOM, dimensions, item groups and item creation

### 2

Milestone 2

Inventory engine

Inventory transaction origin, transaction statuses, on-hand calculation, posting and reservations

### 3

Milestone 3

Inbound and outbound

Purchase receipts/invoices, sales reservations, packing slips and invoices

### 4

Milestone 4

Stock control

Movement, adjustment, transfer, counting and transfer orders

### 5

Milestone 5

Financial integrity

Costing, inventory settlement, returns, reconciliation and closing

### 6

Milestone 6

Production readiness

Security, regression tests, migration, audit trail and go-live checks

## 18. Architecture decisions you should enforce

For your ERP to remain aligned with D365 F&O rather than becoming a simplified inventory application with D365-style names:

- Use the D365 standard logical entities. Do not invent independent equivalents for `InventTrans`, `InventDim`, `InventSum`, `InventTable`, or the purchasing/sales document structures. Physical schema details may be adapted to your platform, but preserve their meaning and relationships.
- Separate documents from inventory postings. Creating a purchase order is not receiving stock. Creating a sales invoice is not equivalent to all previous sales fulfillment steps.
- Preserve physical and financial status. A document can be physically received yet not financially invoiced.
- Treat reservations separately from stock issues. Reserved stock is still physically on hand.
- Support partial fulfillment. One purchase or sales line can produce multiple physical/financial updates and inventory transactions.
- Keep on-hand as a derived, synchronized result. `InventSum`-equivalent information is not a substitute for a transaction ledger.
- Implement accounting and costing as connected subsystems. FIFO requires cost settlement behavior rather than a static cost value.
- Verify against D365 standard metadata. Do not create missing fields or classes merely because an initial simplified design lacks them. First identify the corresponding D365 object.

### Reference documentation

Microsoft's primary documentation relevant to the implementation:

- [Product information management](https://learn.microsoft.com/en-us/dynamics365/supply-chain/pim/product-information?utm_source=chatgpt.com)
- [Inventory journal types and posting](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-journals?utm_source=chatgpt.com)
- [Inventory posting profiles and statuses](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/inventory-posting-profiles?utm_source=chatgpt.com)
- [Inventory on-hand list](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-on-hand-list?utm_source=chatgpt.com)
-