# D365 F&O — Purchase Order Field-by-Field Implementation Dictionary

Basic ERP Core | Functional Specification | Database Design | Business Rules | Validation | Posting Logic | Testing

This specification defines how to implement each core purchase-order field in an ERP aligned with Microsoft Dynamics 365 Finance & Operations.

It is designed for developers, database engineers, functional consultants, and QA engineers.

## 1. Implementation scope and field dictionary conventions

Included: Vendor, released product, purchase order, confirmation, product receipt, inventory transactions, pending and posted invoices, returns, taxes, inventory posting, and basic vendor accounting.

Excluded: Advanced warehouse management, production, manufacturing, quality orders, purchase requisitions, intercompany purchasing, and advanced planning.

Every field in the implementation dictionary should have these properties:

| Property      | Definition                                          |
| ------------- | --------------------------------------------------- |
| Table / field | Standard D365 table and field reference             |
| Data type     | Logical data type or D365 EDT                       |
| Required      | Whether the business process requires a value       |
| Source        | User, master data, system, or posting engine        |
| Editable      | When the field can be changed                       |
| Defaulting    | How its initial value is determined                 |
| Validation    | Rules enforced before saving or posting             |
| Update event  | The business event that changes the field           |
| Dependency    | Related master data, fields, and tables             |
| Test          | Expected output for a positive or negative scenario |

### Field control codes

| Code | Meaning                             |
| ---- | ----------------------------------- |
| U    | User input                          |
| D    | Defaulted from existing data        |
| S    | System generated                    |
| C    | Calculated                          |
| P    | Posting engine maintained           |
| R    | Read-only to the user               |
| M    | Mandatory in the defined scenario   |
| O    | Optional or configuration-dependent |

Data types below are logical implementation types, not asserted SQL column types or exact EDT lengths. `Enum` must map to the relevant D365 enum, and `RefRecId` represents a record reference.

Some D365 values appear in a form even though they are sourced from a related table or a calculated display method. Do not automatically add them as physical columns to your ERP.

# 2. Core master-data field dictionary

## 2.1 `VendTable` — Vendor master

Purpose: Stores vendor-specific information used to initialize purchase orders.

Primary business key: `DataAreaId + AccountNum`

| Field              | Type          | Control | Implementation rule                                                                                      |
| ------------------ | ------------- | ------- | -------------------------------------------------------------------------------------------------------- |
| `AccountNum`       | String        | S/M     | Unique vendor number within company; generated from configured number sequence or entered when permitted |
| `Party`            | RefRecId      | S/M     | Links vendor to a valid party record in `DirPartyTable`                                                  |
| `VendGroup`        | String        | U/M     | Validate selected vendor group exists                                                                    |
| `Currency`         | Currency code | D/U     | Default currency for new POs; changing PO currency must trigger applicable price recalculation           |
| `PaymTermId`       | String        | D/U     | Default payment terms copied to the PO                                                                   |
| `PaymMode`         | String        | D/U     | Default payment method                                                                                   |
| `InvoiceAccount`   | String        | D/U/O   | Use separate invoice vendor if configured; otherwise use ordering vendor                                 |
| `TaxGroup`         | String        | D/U/O   | Default sales-tax group for purchases                                                                    |
| `Blocked`          | Enum          | U/M     | Restrict purchasing according to vendor block type                                                       |
| `DefaultDimension` | RefRecId      | D/U/O   | Default vendor financial dimensions                                                                      |
| `DataAreaId`       | String        | S/R     | Current legal entity                                                                                     |
| `RecId`            | Int64         | S/R     | Unique technical record identifier                                                                       |

### Business logic

When creating a purchase order, selecting `OrderAccount` must initialize supported vendor defaults:

- Invoice account
- Currency
- Payment terms
- Delivery terms and mode
- Tax group
- Address
- Relevant financial dimensions

The user may change permitted order-level defaults, but such changes must not silently modify the vendor master. D365 performs vendor-based PO defaulting as part of purchase order creation.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



Validation example: Vendor V-0001 is blocked for all transactions. Creating a new PO for this vendor must fail under the applicable block rule.

## 2.2 `DirPartyTable` — Vendor name and global identity

| Field                  | Type   | Control | Implementation rule              |
| ---------------------- | ------ | ------- | -------------------------------- |
| `RecId`                | Int64  | S/R     | Primary party reference          |
| `PartyNumber`          | String | S/M     | Global party number              |
| `Name`                 | String | U/M     | Organization/person display name |
| `NameAlias`            | String | U/O     | Search alias                     |
| `InstanceRelationType` | Int64  | S/R     | Identifies party subtype         |

`VendTable.Party` references `DirPartyTable.RecId`.

Do not add a redundant authoritative vendor name column merely because the vendor form displays the name.

## 2.3 `InventTable` — Released item

Key: `DataAreaId + ItemId`

| Field              | Type     | Control | Implementation rule                                    |
| ------------------ | -------- | ------- | ------------------------------------------------------ |
| `ItemId`           | String   | S/U/M   | Unique released item number in company                 |
| `Product`          | RefRecId | S/M     | Reference shared product in `EcoResProduct`            |
| `ItemGroupId`      | String   | U/M\*   | Item-group assignment; validate company-specific setup |
| `NameAlias`        | String   | U/O     | Item search name                                       |
| `DefaultDimension` | RefRecId | D/U/O   | Default financial dimensions                           |
| `DataAreaId`       | String   | S/R     | Company                                                |
| `RecId`            | Int64    | S/R     | Technical record ID                                    |

`*` Required for the proposed basic inventory/accounting configuration, although exact D365 validation depends on the setup.

### PO item selection validations

1. Item exists and is released to the current company.
2. Item is allowed for purchase.
3. Required model-group and dimension-group setup exists.
4. Selected purchase unit is valid.
5. Required site/warehouse values are provided.
6. Product lifecycle state permits purchasing, if lifecycle enforcement is configured.&#x20;

   [image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

   Microsoft Learn



## 2.4 `InventTableModule` — Item purchasing configuration

Use the row where `ModuleType` represents purchasing.

| Field        | Type    | Control | Implementation rule                            |
| ------------ | ------- | ------- | ---------------------------------------------- |
| `ItemId`     | String  | S/M     | Item reference                                 |
| `ModuleType` | Enum    | S/M     | Purchase module                                |
| `UnitId`     | String  | U/M     | Default purchase unit                          |
| `Price`      | Decimal | U/O     | Base purchase price                            |
| `PriceUnit`  | Decimal | U/M     | Quantity basis for the price; must be positive |
| `PriceDate`  | Date    | U/O     | Base price reference date                      |
| `Markup`     | Decimal | U/O     | Configured item-module markup, if used         |
| `DataAreaId` | String  | S/R     | Company                                        |
| `RecId`      | Int64   | S/R     | Technical ID                                   |

### Unit price example

Item ITEM-001:

- Purchase unit = CTN
- Inventory unit = PCS
- Conversion = 1 CTN = 12 PCS
- Purchase price = SAR 240 per CTN
- Purchase quantity = 5 CTN

Therefore:

\\[ \text{Inventory quantity}=5\times12=60\text{ PCS} \\]

\\[ \text{Purchase line amount}=5\times240=1{,}200 \\]

The inventory transaction must use 60 PCS, while the PO line retains 5 CTN.

# 3. Purchase order header — `PurchTable`

This is the central field dictionary for the purchase order header.

## 3.1 Identity, vendor, and document state

| Field            | Type   | Control | Default / edit rule           | Validation                                 |
| ---------------- | ------ | ------- | ----------------------------- | ------------------------------------------ |
| `PurchId`        | String | S/M/R   | Generate on creation          | Unique by company                          |
| `OrderAccount`   | String | U/M     | Vendor selected by user       | Must exist and permit purchases            |
| `InvoiceAccount` | String | D/U/M   | From vendor master            | Valid invoice vendor                       |
| `PurchName`      | String | D/U     | From party name               | Nonempty as configured                     |
| `PurchStatus`    | Enum   | S/R     | Based on PO lifecycle         | Never manually write status                |
| `DocumentState`  | Enum   | S/R     | Workflow/confirmation process | Valid transition only                      |
| `PurchaseType`   | Enum   | D/U     | Standard purchase order       | Allow return type/process where applicable |
| `CurrencyCode`   | String | D/U/M   | Vendor default                | Valid active currency                      |
| `PurchPoolId`    | String | U/O     | Optional PO grouping          | Must exist when entered                    |
| `DataAreaId`     | String | S/R     | Current company               | Cannot be changed                          |
| `RecId`          | Int64  | S/R     | Database/framework            | Unique                                     |

### Field event: `OrderAccount` modified

Required execution sequence:

1. Validate vendor.
2. Retrieve company-specific vendor record.
3. Initialize invoice vendor.
4. Initialize currency and payment terms.
5. Initialize tax/delivery defaults.
6. Apply supported financial-dimension defaults.
7. Refresh dependent PO fields.
8. Recalculate affected totals where necessary.

Changing the vendor after lines exist must follow the relevant D365 rules and should require validation/reinitialization of prices, taxes, and dependent information rather than retaining incompatible values.

## 3.2 Delivery and payment fields

| Field                   | Type     | Control | Default / validation                             |
| ----------------------- | -------- | ------- | ------------------------------------------------ |
| `DeliveryDate`          | Date     | D/U     | User/default receipt date; line dates may differ |
| `DlvMode`               | String   | D/U/O   | Vendor delivery mode                             |
| `DlvTerm`               | String   | D/U/O   | Vendor delivery terms                            |
| `DeliveryName`          | String   | D/U     | Delivery destination name                        |
| `DeliveryPostalAddress` | RefRecId | D/U     | Valid postal address reference                   |
| `Payment`               | String   | D/U     | Payment terms                                    |
| `PaymMode`              | String   | D/U/O   | Payment method                                   |
| `TaxGroup`              | String   | D/U/O   | Purchase tax group                               |
| `DefaultDimension`      | RefRecId | D/U/O   | Header financial dimensions                      |

Design caution: Site and warehouse defaults visible on the PO form must be mapped to their actual header/default-dimension source in the target D365 version. Do not presume every displayed site/warehouse value is persisted as an independent `PurchTable` column.

## 3.3 Audit fields

| Field              | Type         | Control | Rule                                   |
| ------------------ | ------------ | ------- | -------------------------------------- |
| `CreatedDateTime`  | UTC datetime | S/R     | Set at insert                          |
| `CreatedBy`        | String       | S/R     | Actual creating user                   |
| `ModifiedDateTime` | UTC datetime | S/R     | Updated by framework                   |
| `ModifiedBy`       | String       | S/R     | Actual modifying user                  |
| `RecVersion`       | Integer      | S/R     | Optimistic concurrency/version control |

`RecVersion` must not be manually maintained as a normal business field.

# 4. Purchase order line — `PurchLine`

This table has the highest concentration of purchasing business rules.

## 4.1 Line identity and inventory dimensions

| Field              | Type     | Control            | Default / behavior                 | Validation                       |
| ------------------ | -------- | ------------------ | ---------------------------------- | -------------------------------- |
| `RecId`            | Int64    | S/R                | Unique line identifier             | Immutable                        |
| `PurchId`          | String   | S/M                | Parent PO                          | Valid parent in same company     |
| `LineNumber`       | Decimal  | S/R                | Generated line sequence            | Unique in PO context             |
| `ItemId`           | String   | U/M for item lines | User item selection                | Released and purchasable         |
| `Name`             | String   | D/U                | Item name                          | Valid description                |
| `InventDimId`      | String   | S/M                | Generated from selected dimensions | Required active dimensions valid |
| `InventTransId`    | String   | S/R                | Generated inventory reference      | Unique source relationship       |
| `DefaultDimension` | RefRecId | D/U                | Financial dimension defaults       | Must satisfy account structures  |
| `DataAreaId`       | String   | S/R                | Company                            | Read-only                        |

### Item selection event

When `ItemId` changes:

1. Verify item exists.
2. Check purchasing eligibility.
3. Retrieve purchase-unit and price defaults.
4. Retrieve item-level tax defaults.
5. Apply item model group and dimension rules.
6. Set or validate site/warehouse.
7. Create/update dependent inventory origin information at the appropriate PO lifecycle event.
8. Recalculate line pricing and amount.

D365 line defaults can come from the header, product configuration, or trade agreements, so do not make the item master the only pricing/defaulting source.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



## 4.2 Quantity fields

| Field                   | Type    | Control | Update rule                                                |
| ----------------------- | ------- | ------- | ---------------------------------------------------------- |
| `PurchQty`              | Decimal | U/M     | Ordered quantity in purchase unit                          |
| `PurchUnit`             | String  | D/U/M   | Unit used for PO quantities                                |
| `RemainPurchPhysical`   | Decimal | P/R     | Quantity remaining to physically receive, in purchase unit |
| `RemainPurchFinancial`  | Decimal | P/R     | Quantity remaining to invoice, in purchase unit            |
| `RemainInventPhysical`  | Decimal | P/R     | Outstanding physical quantity in inventory unit            |
| `RemainInventFinancial` | Decimal | P/R     | Outstanding financial quantity in inventory unit           |
| `InventNow`             | Decimal | D/P     | Inventory quantity selected for applicable processing      |
| `PurchReceivedNow`      | Decimal | D/P     | Receipt processing quantity where used                     |

### Essential quantity behavior

For a normal stocked PO with a one-to-one unit conversion:

Example: PO ordered 100 PCS

| Event      | Remain physical | Remain financial |
| ---------- | --------------- | ---------------- |
| Create     | 100             | 100              |
| Receive 60 | 40              | 100              |
| Invoice 60 | 40              | 40               |
| Receive 40 | 0               | 40               |
| Invoice 40 | 0               | 0                |

Posting rule: Only the standard posting/correction process should update remaining physical and financial quantities.

Do not recalculate and overwrite these fields from the original order quantity every time a form opens. Cancellations, overdeliveries, corrections, returns, and conversions require transaction-aware logic.

## 4.3 Price, discount, and amount fields

| Field          | Type                  | Control | Calculation / validation                                                   |
| -------------- | --------------------- | ------- | -------------------------------------------------------------------------- |
| `PurchPrice`   | Decimal               | D/U     | Default from applicable purchase price rules; user editable when permitted |
| `PriceUnit`    | Decimal               | D/U     | Positive price quantity basis                                              |
| `LineDisc`     | Decimal               | D/U     | Line discount amount under D365 semantics                                  |
| `LinePercent`  | Decimal               | D/U     | Line discount percentage                                                   |
| `LineAmount`   | Decimal               | C       | Calculated net amount using D365 discount and price-unit rules             |
| `CurrencyCode` | Logical line currency | D/R     | Inherits order currency in basic PO scenario; verify actual stored source  |

The final row is intentionally described as a logical value: do not create a `PurchLine.CurrencyCode` physical field without validating the target schema.

### Basic calculation

If no discounts or extra charges are applied:

\\[ \text{Line Amount} = \text{PurchQty}\times\frac{\text{PurchPrice}}{\text{PriceUnit}} \\]

Example:

- Quantity = 100
- Price = 250
- Price unit = 10

Line amount = SAR 2,500.

With discounts, use the D365 discount sequence and appropriate currency rounding. Don't implement `LineDisc` as a simple per-unit discount without validating the standard semantics.

## 4.4 Tax and delivery fields

| Field          | Type   | Control | Rule                                      |
| -------------- | ------ | ------- | ----------------------------------------- |
| `TaxGroup`     | String | D/U/O   | Default sales tax group                   |
| `TaxItemGroup` | String | D/U/O   | Item sales tax group                      |
| `DeliveryDate` | Date   | D/U     | Required under configured delivery policy |
| `PurchStatus`  | Enum   | S/R     | Reflect line fulfillment lifecycle        |

Changes to price, quantity, discounts, tax group, or item tax group must invalidate stale totals and trigger the applicable recalculation.

# 5. Inventory transaction implementation dictionary

## 5.1 `InventTransOrigin`

| Field               | Type   | Control | Implementation rule                                   |
| ------------------- | ------ | ------- | ----------------------------------------------------- |
| `RecId`             | Int64  | S/R     | Inventory origin primary key                          |
| `InventTransId`     | String | S/R     | Unique inventory source identifier within its context |
| `ItemId`            | String | S/R     | Item originating the transaction                      |
| `ReferenceCategory` | Enum   | S/R     | Purchase order source type                            |
| `ReferenceId`       | String | S/R     | Source document reference                             |
| `DataAreaId`        | String | S/R     | Company                                               |

Lifecycle: The origin is established when the purchase line enters the inventory transaction workflow. The posting framework uses it to identify affected inventory transactions.

## 5.2 `InventTrans`

| Field                  | Type     | Control | Initial value           | Update event                         |
| ---------------------- | -------- | ------- | ----------------------- | ------------------------------------ |
| `RecId`                | Int64    | S/R     | Generated               | Never manually changed               |
| `InventTransOrigin`    | RefRecId | S/R     | PO line origin          | Used throughout lifecycle            |
| `ItemId`               | String   | S/R     | From line               | Controlled by inventory framework    |
| `InventDimId`          | String   | S/R     | From line               | Dimension split/change               |
| `Qty`                  | Decimal  | P/R     | +100 for normal PO      | Split/remaining transaction handling |
| `StatusReceipt`        | Enum     | P/R     | Ordered                 | Registration, receipt, invoice       |
| `StatusIssue`          | Enum     | P/R     | None for normal receipt | Return-order issue processing        |
| `DatePhysical`         | Date     | P/R     | Not physically posted   | Product receipt                      |
| `DateFinancial`        | Date     | P/R     | Not financially posted  | Vendor invoice                       |
| `VoucherPhysical`      | String   | P/R     | Blank                   | Physical posting                     |
| `Voucher`              | String   | P/R     | Blank                   | Financial posting                    |
| `CostAmountPhysical`   | Decimal  | P/R     | 0                       | Physical receipt value               |
| `CostAmountPosted`     | Decimal  | P/R     | 0                       | Financial cost posting               |
| `CostAmountAdjustment` | Decimal  | P/R     | 0                       | Inventory cost adjustments           |
| `DataAreaId`           | String   | S/R     | Current company         | Immutable                            |

### Required transaction split logic

Order 100 PCS → receive 60 PCS:

| Inventory record | Qty | Status   |
| ---------------- | --- | -------- |
| Transaction A    | +60 | Received |
| Transaction B    | +40 | Ordered  |

Then invoice 40 of the received 60:

| Inventory record | Qty | Status    |
| ---------------- | --- | --------- |
| Transaction A1   | +40 | Purchased |
| Transaction A2   | +20 | Received  |
| Transaction B    | +40 | Ordered   |

These are illustrative resulting status partitions. Actual record identity/splits are managed by the inventory framework.

D365 inventory receipt statuses include Ordered, Registered, Received, and Purchased, with distinct physical and financial meaning.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



## 5.3 `InventDim`

| Field              | Type   | Control | Rule                                            |
| ------------------ | ------ | ------- | ----------------------------------------------- |
| `InventDimId`      | String | S/R     | Unique inventory dimension combination ID       |
| `InventSiteId`     | String | U/M     | Valid site                                      |
| `InventLocationId` | String | U/M     | Warehouse belongs to selected site              |
| `InventBatchId`    | String | U/O     | Required only when relevant tracking is enabled |
| `InventSerialId`   | String | U/O     | Required only when relevant tracking is enabled |
| `DataAreaId`       | String | S/R     | Company                                         |
| `RecId`            | Int64  | S/R     | Record reference                                |

Rule: Inventory dimensions represent a reusable combination. Do not automatically generate a separate duplicate dimension record for every PO line.

## 5.4 `InventSum`

| Field            | Type    | Control | Rule                                            |
| ---------------- | ------- | ------- | ----------------------------------------------- |
| `ItemId`         | String  | P/R     | Item                                            |
| `InventDimId`    | String  | P/R     | Inventory dimension combination                 |
| `PostedQty`      | Decimal | P/R     | Financially updated stock component             |
| `Received`       | Decimal | P/R     | Physically received but not financially updated |
| `Deducted`       | Decimal | P/R     | Physically issued but not financially updated   |
| `Registered`     | Decimal | P/R     | Registered quantity component                   |
| `Ordered`        | Decimal | P/R     | Ordered receipt quantity                        |
| `OnOrder`        | Decimal | P/R     | Ordered issue quantity                          |
| `ReservPhysical` | Decimal | P/R     | Physical reservations                           |
| `ReservOrdered`  | Decimal | P/R     | Ordered reservations                            |
| `PostedValue`    | Decimal | P/R     | Financially posted stock value                  |
| `PhysicalValue`  | Decimal | P/R     | Physically posted value                         |
| `DataAreaId`     | String  | S/R     | Company                                         |

These aggregate fields are maintained by inventory processing. They are not user-entered PO fields.

# 6. Purchase order confirmation dictionary

## 6.1 `VendPurchOrderJour` — Posted confirmation header

| Field                 | Type     | Control | Rule                            |
| --------------------- | -------- | ------- | ------------------------------- |
| `RecId`               | Int64    | S/R     | Generated confirmation record   |
| `PurchId`             | String   | P/R     | Original purchase order         |
| `PurchaseOrderId`     | String   | P/R     | Confirmation document reference |
| `PurchOrderDate`      | Date     | P/R     | Confirmation document date      |
| `OrderAccount`        | String   | P/R     | Confirmed vendor                |
| `Amount`              | Decimal  | P/R     | Amount at confirmation          |
| `AmountPurchaseOrder` | Decimal  | P/R     | Purchase order amount           |
| `SumTax`              | Decimal  | P/R     | Tax total at confirmation       |
| `SumMarkup`           | Decimal  | P/R     | Charges total                   |
| `SumLineDisc`         | Decimal  | P/R     | Line discount total             |
| `Qty`                 | Decimal  | P/R     | Confirmed quantity total        |
| `PurchTableVersion`   | RefRecId | P/R     | Confirmation version reference  |
| `DataAreaId`          | String   | S/R     | Company                         |

## 6.2 `VendPurchOrderTrans` — Posted confirmation line

| Field           | Type    | Control | Rule                                   |
| --------------- | ------- | ------- | -------------------------------------- |
| `RecId`         | Int64   | S/R     | Confirmation line record               |
| `PurchId`       | String  | P/R     | Original PO                            |
| `ItemId`        | String  | P/R     | Confirmed item                         |
| `InventTransId` | String  | P/R     | Source inventory transaction reference |
| `Qty`           | Decimal | P/R     | Confirmed quantity                     |
| `PurchUnit`     | String  | P/R     | Purchase unit snapshot                 |
| `PurchPrice`    | Decimal | P/R     | Price snapshot                         |
| `PriceUnit`     | Decimal | P/R     | Price unit snapshot                    |
| `LineAmount`    | Decimal | P/R     | Confirmed net line amount              |
| `InventDimId`   | String  | P/R     | Confirmed dimensions                   |
| `DataAreaId`    | String  | S/R     | Company                                |

Confirmation rule: Posted journals are historical snapshots. Reconformation must create the appropriate new journal/version rather than overwriting the old one.

# 7. Product receipt implementation dictionary

## 7.1 Posting input form

User provides the product receipt reference, posting date, and receipt quantities.

| Input                | Source               | Validation                                            |
| -------------------- | -------------------- | ----------------------------------------------------- |
| PO number            | Selected document    | Existing eligible PO                                  |
| Product receipt ID   | User                 | Nonempty valid receipt reference                      |
| Posting/receipt date | User/default         | Valid open period and document date                   |
| Receipt quantity     | User/default         | Within configured tolerance and permissible remainder |
| Site/warehouse       | PO line / dimension  | Valid dimensions                                      |
| Receipt lines        | Purchase order lines | Eligible to receive                                   |

Microsoft's standard product receipt process requires a product receipt identifier and supports manually changing receipt quantities.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+1



## 7.2 `VendPackingSlipJour`

| Field                   | Type     | Control | Posting rule                        |
| ----------------------- | -------- | ------- | ----------------------------------- |
| `RecId`                 | Int64    | P/R     | Generate receipt header             |
| `PurchId`               | String   | P/R     | Source PO reference                 |
| `PackingSlipId`         | String   | U→P     | Persist provided product receipt ID |
| `DeliveryDate`          | Date     | D→P     | Physical receipt date               |
| `DocumentDate`          | Date     | D→P     | Supplier document date              |
| `OrderAccount`          | String   | P/R     | Source order vendor                 |
| `InvoiceAccount`        | String   | P/R     | Source invoice vendor               |
| `DeliveryName`          | String   | P/R     | Delivery destination                |
| `DeliveryPostalAddress` | RefRecId | P/R     | Delivery address                    |
| `DefaultDimension`      | RefRecId | P/R     | Posting dimensions                  |
| `DataAreaId`            | String   | S/R     | Company                             |

## 7.3 `VendPackingSlipTrans`

| Field                 | Type     | Control | Posting rule                           |
| --------------------- | -------- | ------- | -------------------------------------- |
| `RecId`               | Int64    | P/R     | Generate receipt line                  |
| `VendPackingSlipJour` | RefRecId | P/R     | Link receipt header                    |
| `PackingSlipId`       | String   | P/R     | Receipt reference                      |
| `OrigPurchId`         | String   | P/R     | Source purchase order                  |
| `ItemId`              | String   | P/R     | Source item                            |
| `InventTransId`       | String   | P/R     | Inventory origin transaction reference |
| `InventDimId`         | String   | P/R     | Received inventory dimensions          |
| `Qty`                 | Decimal  | P/R     | Receipt quantity in purchase unit      |
| `InventQty`           | Decimal  | P/R     | Quantity in inventory unit             |
| `PurchUnit`           | String   | P/R     | Purchase unit                          |
| `PriceUnit`           | Decimal  | P/R     | Price unit                             |
| `DeliveryDate`        | Date     | P/R     | Receipt date                           |
| `ValueMST`            | Decimal  | P/R     | Receipt value in accounting currency   |
| `Remain`              | Decimal  | P/R     | Posting-related remaining quantity     |
| `RemainInvent`        | Decimal  | P/R     | Inventory-unit remaining quantity      |
| `DefaultDimension`    | RefRecId | P/R     | Posting dimensions                     |
| `DataAreaId`          | String   | S/R     | Company                                |

The published Microsoft field metadata for `VendPackingSlipTrans` includes the quantity, inventory, document, and source fields above.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



### Product receipt posting transaction

For receipt of 60 PCS:

1. Validate order and remaining quantity.
2. Validate open posting period and required inventory dimensions.
3. Create the receipt posting context.
4. Create `VendPackingSlipJour`.
5. Create appropriate `VendPackingSlipTrans` records.
6. Update `InventTrans` physical status, dates, vouchers, and costs.
7. Update remaining physical quantities.
8. Update inventory on-hand through inventory framework.
9. Post configured physical/accrual ledger transactions.
10. Commit the posting atomically.

A failure in a required posting operation must not leave a half-posted receipt.

### Correction rule

Correcting a posted receipt may reduce the received quantity; increasing requires a new receipt. Canceling a posted receipt creates reversal transactions rather than directly deleting the original posting history.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



# 8. Vendor invoice implementation dictionary

## 8.1 `VendInvoiceInfoTable` — Pending invoice header

| Field            | Type    | Control | Rule                         |
| ---------------- | ------- | ------- | ---------------------------- |
| `RecId`          | Int64   | S/R     | Pending invoice record       |
| `ParmId`         | String  | S/R     | Invoice processing reference |
| `TableRefId`     | Integer | S/R     | Posting/header reference     |
| `Num`            | String  | U/M     | Vendor invoice number        |
| `InvoiceAccount` | String  | D/U     | Invoice vendor               |
| `OrderAccount`   | String  | D/R     | Purchasing vendor            |
| `DocumentDate`   | Date    | U/M     | Vendor document date         |
| `TransDate`      | Date    | D/U     | Accounting transaction date  |
| `CurrencyCode`   | String  | D/U     | Purchase invoice currency    |
| `Description`    | String  | U/O     | Invoice description          |
| `DataAreaId`     | String  | S/R     | Company                      |

### Invoice number validation

Check for duplicate vendor invoice numbers according to the configured duplicate-invoice policy. Do not impose global uniqueness of invoice numbers across all vendors and legal entities unless that is the intended policy.

## 8.2 `VendInvoiceInfoLine` — Pending invoice line

| Field           | Type    | Control | Rule                                 |
| --------------- | ------- | ------- | ------------------------------------ |
| `RecId`         | Int64   | S/R     | Pending line ID                      |
| `ParmId`        | String  | S/R     | Invoice processing group             |
| `TableRefId`    | Integer | S/R     | Header/processing reference          |
| `OrigPurchId`   | String  | D/R     | Source PO                            |
| `LineNum`       | Decimal | S/R     | Invoice line sequence                |
| `ItemId`        | String  | D/R     | Item                                 |
| `InventTransId` | String  | D/R     | Inventory reference                  |
| `InventDimId`   | String  | D/R     | Inventory dimensions                 |
| `PurchUnit`     | String  | D/R     | Purchase unit                        |
| `ReceiveNow`    | Decimal | D/U     | Selected invoice-processing quantity |
| `PurchPrice`    | Decimal | D/U     | Invoice purchase price               |
| `LineAmount`    | Decimal | C/R     | Invoice line net amount              |
| `DataAreaId`    | String  | S/R     | Company                              |

The pending invoice model must preserve the association between the invoice line and the source PO/receipt lines when receipt matching is used.

## 8.3 `VendInvoiceJour` — Posted invoice header

| Field              | Type     | Control | Posting rule                                         |
| ------------------ | -------- | ------- | ---------------------------------------------------- |
| `RecId`            | Int64    | P/R     | Posted invoice record                                |
| `InvoiceId`        | String   | P/R     | Vendor invoice number                                |
| `PurchId`          | String   | P/R     | Related PO reference                                 |
| `InvoiceAccount`   | String   | P/R     | Invoice vendor                                       |
| `OrderAccount`     | String   | P/R     | Ordering vendor                                      |
| `InvoiceDate`      | Date     | P/R     | Invoice/accounting date according to posting context |
| `DocumentDate`     | Date     | P/R     | Original invoice document date                       |
| `DueDate`          | Date     | C/P     | Due date from terms and applicable rules             |
| `CurrencyCode`     | String   | P/R     | Currency                                             |
| `InvoiceAmount`    | Decimal  | P/R     | Posted invoice total                                 |
| `InvoiceAmountMST` | Decimal  | P/R     | Total in accounting currency                         |
| `LedgerVoucher`    | String   | P/R     | GL voucher                                           |
| `ExchRate`         | Decimal  | P/R     | Applied exchange rate                                |
| `Payment`          | String   | P/R     | Payment terms                                        |
| `PostingProfile`   | String   | P/R     | Vendor posting profile                               |
| `DefaultDimension` | RefRecId | P/R     | Financial dimensions                                 |
| `DataAreaId`       | String   | S/R     | Company                                              |

## 8.4 `VendInvoiceTrans` — Posted invoice line

| Field              | Type     | Control | Rule                       |
| ------------------ | -------- | ------- | -------------------------- |
| `RecId`            | Int64    | P/R     | Posted invoice line        |
| `InvoiceId`        | String   | P/R     | Vendor invoice             |
| `PurchId`          | String   | P/R     | Original purchase order    |
| `ItemId`           | String   | P/R     | Item                       |
| `InventTransId`    | String   | P/R     | Inventory origin reference |
| `InventDimId`      | String   | P/R     | Dimensions                 |
| `Qty`              | Decimal  | P/R     | Invoiced purchase quantity |
| `PurchUnit`        | String   | P/R     | Purchase unit              |
| `PurchPrice`       | Decimal  | P/R     | Actual invoice price       |
| `PriceUnit`        | Decimal  | P/R     | Price quantity basis       |
| `LineAmount`       | Decimal  | P/R     | Posted net amount          |
| `LineDisc`         | Decimal  | P/R     | Discount amount            |
| `LinePercent`      | Decimal  | P/R     | Discount percentage        |
| `TaxGroup`         | String   | P/R     | Tax group                  |
| `TaxItemGroup`     | String   | P/R     | Item tax group             |
| `DefaultDimension` | RefRecId | P/R     | Financial dimensions       |
| `DataAreaId`       | String   | S/R     | Company                    |

### Invoice posting rule

For a stocked item subject to receipt-before-invoice:

- Validate receipt matching.
- Validate invoice quantity and unit conversion.
- Validate purchase price, charges, taxes, and matching policy.
- Create posted invoice records.
- Financially update eligible inventory transactions.
- Update remaining financial quantities.
- Post vendor liability and applicable tax/ledger transactions.
- Retain the invoice-to-receipt association.

# 9. Accounting field dictionary

## 9.1 `VendTrans`

| Field             | Type    | Control | Purpose                            |
| ----------------- | ------- | ------- | ---------------------------------- |
| `RecId`           | Int64   | P/R     | Vendor transaction reference       |
| `AccountNum`      | String  | P/R     | Vendor account                     |
| `Invoice`         | String  | P/R     | Invoice reference                  |
| `Voucher`         | String  | P/R     | Accounting voucher                 |
| `TransDate`       | Date    | P/R     | Posting date                       |
| `DueDate`         | Date    | P/R     | Payment due date                   |
| `CurrencyCode`    | String  | P/R     | Currency                           |
| `AmountCur`       | Decimal | P/R     | Original signed transaction amount |
| `SettleAmountCur` | Decimal | P/R     | Settlement-related amount          |
| `LastSettleDate`  | Date    | P/R     | Last settlement date               |
| `TransType`       | Enum    | P/R     | Transaction type                   |
| `DataAreaId`      | String  | S/R     | Company                            |

## 9.2 `TaxTrans`

| Field           | Type    | Control | Purpose                             |
| --------------- | ------- | ------- | ----------------------------------- |
| `RecId`         | Int64   | P/R     | Tax transaction ID                  |
| `Voucher`       | String  | P/R     | Posting voucher                     |
| `TransDate`     | Date    | P/R     | Tax transaction date                |
| `TaxCode`       | String  | P/R     | Tax code                            |
| `TaxBaseAmount` | Decimal | P/R     | Taxable base in accounting currency |
| `TaxAmount`     | Decimal | P/R     | Tax amount                          |
| `SourceTableId` | Integer | P/R     | Source table ID                     |
| `SourceRecId`   | Int64   | P/R     | Source record ID                    |
| `DataAreaId`    | String  | S/R     | Company                             |

Tax currency/source amount variants require additional tax fields and can differ from the simplified accounting-currency columns above.

## 9.3 `GeneralJournalEntry`

| Field                        | Type     | Control | Purpose               |
| ---------------------------- | -------- | ------- | --------------------- |
| `RecId`                      | Int64    | P/R     | Posted journal header |
| `AccountingDate`             | Date     | P/R     | Accounting date       |
| `SubledgerVoucher`           | String   | P/R     | Subledger voucher     |
| `SubledgerVoucherDataAreaId` | String   | P/R     | Source company        |
| `JournalNumber`              | String   | P/R     | Journal reference     |
| `Ledger`                     | RefRecId | P/R     | Ledger                |
| `JournalCategory`            | Enum     | P/R     | Journal category      |

## 9.4 `GeneralJournalAccountEntry`

| Field                       | Type     | Control | Purpose                     |
| --------------------------- | -------- | ------- | --------------------------- |
| `RecId`                     | Int64    | P/R     | Posted GL line              |
| `GeneralJournalEntry`       | RefRecId | P/R     | GL header                   |
| `LedgerDimension`           | RefRecId | P/R     | Main account and dimensions |
| `AccountingCurrencyAmount`  | Decimal  | P/R     | Signed accounting amount    |
| `TransactionCurrencyAmount` | Decimal  | P/R     | Transaction-currency amount |
| `TransactionCurrencyCode`   | String   | P/R     | Currency                    |
| `PostingType`               | Enum     | P/R     | Posting classification      |
| `Text`                      | String   | P/R     | Description                 |

### Accounting validation

Every posted voucher must satisfy:

\\[ \sum \text{AccountingCurrencyAmount}=0 \\]

Subject to normal rounding/balancing rules, account structures, posting restrictions, and the posting framework.

# 10. Field event dependency dictionary

This is critical for implementing functioning ERP forms.

| User action           | Direct fields                            | Required downstream effect                                                                         |
| --------------------- | ---------------------------------------- | -------------------------------------------------------------------------------------------------- |
| Select vendor         | `PurchTable.OrderAccount`                | Initialize currency, invoice account, payment, tax, and delivery defaults                          |
| Select item           | `PurchLine.ItemId`                       | Initialize item name, purchase unit, price, item tax group and dimension requirements              |
| Change quantity       | `PurchLine.PurchQty`                     | Recalculate line amount and applicable discounts/taxes; maintain inventory transaction consistency |
| Change purchase unit  | `PurchLine.PurchUnit`                    | Validate conversion and recalculate inventory quantity                                             |
| Change price          | `PurchLine.PurchPrice`                   | Recalculate amount, tax, and totals                                                                |
| Change price unit     | `PurchLine.PriceUnit`                    | Recalculate amount                                                                                 |
| Change discount       | `LineDisc`, `LinePercent`                | Recalculate net line amount and dependent totals                                                   |
| Change tax group      | `TaxGroup`                               | Recalculate applicable tax                                                                         |
| Change site/warehouse | `InventDimId`                            | Validate and resolve dimension combination; update eligible open inventory transactions            |
| Confirm PO            | `DocumentState` and confirmation context | Create immutable confirmation records                                                              |
| Post product receipt  | Receipt quantity and journal fields      | Create receipt journal, update physical inventory and remaining quantity                           |
| Post invoice          | Invoice quantity, price, tax, charges    | Create invoice journal, AP, tax, inventory financial updates and GL postings                       |
| Post return           | Negative purchase quantity               | Update inventory issue transactions and return posting documents                                   |
| Settle payment        | Vendor settlement references             | Update financial settlement, not PO inventory quantities                                           |

# 11. Unified implementation example

Use the following sample data throughout development and QA:

PO-0001001 — ITEM-001

Company

USMF

Vendor

V-0001

Site / Warehouse

SITE-01 / WH-01

Unit

PCS

Ordered

100 PCS

Price

50 SAR

Net amount\
5,000 SAR

VAT (15%)\
750 SAR

Total\
5,750 SAR

## 11.1 Expected values by document stage

| Stage               | `PurchQty` | Remain physical | Remain financial | Inventory status           |
| ------------------- | ---------- | --------------- | ---------------- | -------------------------- |
| PO created/approved | 100        | 100             | 100              | Ordered 100                |
| PO confirmed        | 100        | 100             | 100              | Ordered 100                |
| Product receipt 60  | 100        | 40              | 100              | Received 60 / Ordered 40   |
| Invoice 60          | 100        | 40              | 40               | Purchased 60 / Ordered 40  |
| Product receipt 40  | 100        | 0               | 40               | Purchased 60 / Received 40 |
| Invoice 40          | 100        | 0               | 0                | Purchased 100              |

The test passes only when PO lines, inventory transaction statuses, receipt journals, invoice journals, and ledger/subledger records agree.

# 12. Field validation acceptance checklist

Implementation acceptance

0/15 passed

Vendor fields

Vendor accounts, invoice account and vendor defaults initialize correctly

Product fields

Only valid released, purchasable products can be ordered

Header fields

PO ID, vendor, dates, terms and currency persist correctly

Line fields

Unit, price, discount, tax and line amount calculate correctly

Inventory dimensions

Site and warehouse validate, with correct InventDim references

Inventory origin

Approved PO creates valid inventory origin and ordered quantity

Confirmation fields

Versioned confirmation documents preserve historical values

Receipt fields

Physical receipt updates journal records and physical remainder

Inventory statuses

Ordered, Received and Purchased quantities remain consistent

Invoice fields

Pending and posted invoice amounts, quantities and references match

Vendor accounting

Vendor transaction amount and settlement reconcile

Tax fields

Taxable base and tax postings reconcile with invoice

GL posting

Every journal balances and uses the configured posting accounts

Return fields

Negative PO updates issue-side inventory correctly

Audit controls

Posting documents and transaction identifiers remain traceable

&#x20;Copy acceptance checklist

## 13. Additional field-level specifications required before production

This dictionary establishes the core implementation contract. A complete development specification must also define:

- Exact AOT field metadata and EDTs for the target D365 version.
- Tables and fields for number sequences, vendor posting profiles, item model and storage dimension group assignments.
- Purchase trade agreement precedence, pricing and discount rules.
- Header/line charge allocation and currency rounding.
- Invoice-to-product-receipt matching relationships.
- Purchase posting parameter tables and transaction boundaries.
- Correction/reversal identifiers and audit relationships.
- Financial dimension defaulting and merging precedence.
- Inventory model-specific cost valuation and adjustment behavior.
- Country/localization rules applicable to the legal entity.

These should be implemented from standard D365 metadata and behavior, not inferred from form labels. In particular, an ERP field dictionary must separately document which values are physically stored and which are calculated or presented from related tables.

Recommended build sequence: Implement and validate `VendTable` → `InventTable`/`InventTableModule` → `PurchTable` → `PurchLine` → `InventTransOrigin`/`InventTrans` → confirmation → receipt → invoice → accounting. Complete each field's defaults, editability, validation, event behavior, and database tests before proceeding to the next posting phase.


# D365 F&O — Basic Purchase Order Tables & Fields

Scope: Complete basic Purchase Order lifecycle, including master data, PO creation, confirmation, inventory receiving, vendor invoicing, purchase returns, and basic accounting.

Excludes WHS, WMS, production, manufacturing, procurement requisitions, and other advanced functionality.

The following is a core-field inventory, not a dump of every physical field in Microsoft's application database. It identifies the standard tables and important fields needed for D365 alignment. Actual metadata, optional fields, and country-specific extensions depend on your D365 version.

## 1. Master list of tables

| #  | D365 table                   | Purpose                            | Priority                      |
| -- | ---------------------------- | ---------------------------------- | ----------------------------- |
| 1  | `VendTable`                  | Vendor master                      | Required                      |
| 2  | `DirPartyTable`              | Vendor organization/name           | Required                      |
| 3  | `InventTable`                | Released item                      | Required                      |
| 4  | `EcoResProduct`              | Shared product                     | Required                      |
| 5  | `EcoResProductTranslation`   | Product name/description           | Required                      |
| 6  | `InventItemGroup`            | Item group                         | Required                      |
| 7  | `InventModelGroup`           | Inventory model group              | Required                      |
| 8  | `InventModelGroupItem`       | Item-to-model-group assignment     | Required                      |
| 9  | `InventDim`                  | Inventory dimensions               | Required                      |
| 10 | `InventSite`                 | Site                               | Required                      |
| 11 | `InventLocation`             | Warehouse                          | Required                      |
| 12 | `UnitOfMeasure`              | Units of measure                   | Required                      |
| 13 | `InventTableModule`          | Purchase unit and base item prices | Required                      |
| 14 | `PurchTable`                 | PO header                          | Required                      |
| 15 | `PurchLine`                  | PO lines                           | Required                      |
| 16 | `InventTransOrigin`          | Inventory transaction origin       | Required                      |
| 17 | `InventTrans`                | Inventory movements and statuses   | Required                      |
| 18 | `InventSum`                  | On-hand aggregation                | Required                      |
| 19 | `VendPurchOrderJour`         | Posted PO confirmation header      | Required                      |
| 20 | `VendPurchOrderTrans`        | Posted PO confirmation lines       | Required                      |
| 21 | `VendPackingSlipJour`        | Product receipt header             | Required                      |
| 22 | `VendPackingSlipTrans`       | Product receipt lines              | Required                      |
| 23 | `VendInvoiceInfoTable`       | Pending vendor invoice header      | Required for pending invoices |
| 24 | `VendInvoiceInfoLine`        | Pending vendor invoice lines       | Required for pending invoices |
| 25 | `VendInvoiceJour`            | Posted vendor invoice header       | Required                      |
| 26 | `VendInvoiceTrans`           | Posted vendor invoice lines        | Required                      |
| 27 | `VendTrans`                  | Vendor accounting transactions     | Required                      |
| 28 | `VendSettlement`             | Vendor invoice/payment settlement  | Payment cycle                 |
| 29 | `TaxTrans`                   | Posted taxes                       | Tax-enabled purchasing        |
| 30 | `InventPosting`              | Inventory posting setup            | Required for GL integration   |
| 31 | `InventTransPosting`         | Inventory posting references       | Accounting integration        |
| 32 | `GeneralJournalEntry`        | Posted GL journal header           | Accounting integration        |
| 33 | `GeneralJournalAccountEntry` | Posted GL lines                    | Accounting integration        |
| 34 | `LedgerJournalTable`         | Payment journal header             | Payment cycle                 |
| 35 | `LedgerJournalTrans`         | Payment journal lines              | Payment cycle                 |

Supporting tables for dimensions, discounts, posting, charges, currency, and unit conversion are included below.

Correction to the earlier blueprint: The standard PO confirmation journal tables are `VendPurchOrderJour` and `VendPurchOrderTrans`, not `PurchPurchOrderJour` and `PurchPurchOrderTrans`.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://purchaseorders.io\&sz=32)

purchaseorders.io

+1



# 2. Vendor master

## Table: `VendTable`

D365 form: Accounts payable > Vendors > All vendors

| Field              | Description                     | Example     |
| ------------------ | ------------------------------- | ----------- |
| `AccountNum`       | Vendor account ID               | V-0001      |
| `Party`            | Reference to global party       | Party RecId |
| `VendGroup`        | Vendor group                    | LOCAL       |
| `Currency`         | Default currency                | SAR         |
| `PaymTermId`       | Payment terms                   | NET30       |
| `PaymMode`         | Payment method                  | BANK        |
| `InvoiceAccount`   | Invoice vendor, if different    | V-0001      |
| `TaxGroup`         | Sales tax group                 | VAT15       |
| `Tax1099Fields`    | Country-specific tax settings   | Optional    |
| `Blocked`          | Vendor transaction block        | No          |
| `DefaultDimension` | Financial dimension combination | RecId       |
| `DataAreaId`       | Company                         | USMF        |
| `RecId`            | Internal record ID              | Generated   |

The vendor's display name comes from party/address-book data rather than relying on a simple `VendTable.Name` field.

## Table: `DirPartyTable`

| Field         | Description            |
| ------------- | ---------------------- |
| `RecId`       | Party identifier       |
| `Name`        | Vendor/company name    |
| `NameAlias`   | Search name            |
| `PartyNumber` | Party number           |
| `Type`        | Person or organization |

Relationship: `VendTable.Party = DirPartyTable.RecId`.

# 3. Product master

## Table: `EcoResProduct`

| Field                  | Description                   |
| ---------------------- | ----------------------------- |
| `RecId`                | Shared product identifier     |
| `DisplayProductNumber` | Product number                |
| `ProductType`          | Item or service               |
| `ProductSubType`       | Product or product master     |
| `SearchName`           | Product search name           |
| `InstanceRelationType` | Product subtype discriminator |

## Table: `EcoResProductTranslation`

| Field         | Description           |
| ------------- | --------------------- |
| `RecId`       | Record identifier     |
| `Product`     | `EcoResProduct.RecId` |
| `LanguageId`  | Language              |
| `Name`        | Product name          |
| `Description` | Product description   |

## Table: `InventTable`

D365 form: Product information management > Released products

| Field              | Description               | Example   |
| ------------------ | ------------------------- | --------- |
| `ItemId`           | Released item number      | ITEM-001  |
| `Product`          | `EcoResProduct` reference | RecId     |
| `ItemType`         | Item type                 | Item      |
| `ItemGroupId`      | Inventory item group      | FINISHED  |
| `NameAlias`        | Search name               | Uniform   |
| `DefaultDimension` | Financial dimensions      | RecId     |
| `DataAreaId`       | Company                   | USMF      |
| `RecId`            | Internal record ID        | Generated |

Item model group and storage dimension group assignments are handled through their relevant setup/assignment structures. Avoid assuming every assignment is a direct field on `InventTable`.

## Table: `InventTableModule`

One item can have separate module records for Inventory, Purchase, and Sales.

| Field        | Description                 |
| ------------ | --------------------------- |
| `ItemId`     | Item number                 |
| `ModuleType` | Inventory/Purchase/Sales    |
| `UnitId`     | Default unit for the module |
| `Price`      | Base module price           |
| `PriceUnit`  | Price quantity unit         |
| `PriceDate`  | Price date                  |
| `Markup`     | Markup amount               |
| `DataAreaId` | Company                     |
| `RecId`      | Record ID                   |

For purchase orders, read the Purchase module record, not the Sales module record.

# 4. Inventory configuration

## Table: `InventItemGroup`

| Field         | Description           |
| ------------- | --------------------- |
| `ItemGroupId` | Item group identifier |
| `Name`        | Item group name       |
| `DataAreaId`  | Company               |
| `RecId`       | Record identifier     |

## Table: `InventModelGroup`

| Field                      | Description                                               |
| -------------------------- | --------------------------------------------------------- |
| `ModelGroupId`             | Inventory model group                                     |
| `Name`                     | Model group description                                   |
| `InventModel`              | Inventory valuation model                                 |
| `StockedProduct`           | Stocked-product behavior                                  |
| `PostPhysicalInvent`       | Post physical inventory                                   |
| `PostFinancialInvent`      | Post financial inventory                                  |
| `RegistrationRequirements` | Registration required                                     |
| `ReceivingRequirements`    | Receipt required before invoice                           |
| `IncludePhysicalValue`     | Include physical value in applicable costing calculations |
| `DataAreaId`               | Company                                                   |

These are the important logical settings; verify the exact underlying field names and enum representation against your D365 metadata.

## Table: `InventModelGroupItem`

| Field          | Description          |
| -------------- | -------------------- |
| `ItemId`       | Released item        |
| `ModelGroupId` | Assigned model group |
| `DataAreaId`   | Company              |
| `RecId`        | Record ID            |

## Table: `InventSite`

| Field        | Description |
| ------------ | ----------- |
| `SiteId`     | Site ID     |
| `Name`       | Site name   |
| `DataAreaId` | Company     |
| `RecId`      | Record ID   |

## Table: `InventLocation`

| Field              | Description    |
| ------------------ | -------------- |
| `InventLocationId` | Warehouse ID   |
| `Name`             | Warehouse name |
| `InventSiteId`     | Parent site    |
| `DataAreaId`       | Company        |
| `RecId`            | Record ID      |

## Table: `InventDim`

| Field              | Description                                  |
| ------------------ | -------------------------------------------- |
| `InventDimId`      | Dimension combination ID                     |
| `InventSiteId`     | Site                                         |
| `InventLocationId` | Warehouse                                    |
| `InventBatchId`    | Batch, if used                               |
| `InventSerialId`   | Serial, if used                              |
| `WMSLocationId`    | Location, excluded from basic implementation |
| `LicensePlateId`   | License plate, excluded                      |
| `DataAreaId`       | Company                                      |
| `RecId`            | Record ID                                    |

For the basic model, site and warehouse are sufficient unless the product requires batch or serial tracking.

# 5. Purchase order header

## Table: `PurchTable`

D365 form: Procurement and sourcing > Purchase orders > All purchase orders

| Field                   | Description                                           | Example        |
| ----------------------- | ----------------------------------------------------- | -------------- |
| `PurchId`               | Purchase order number                                 | PO-0001001     |
| `OrderAccount`          | Ordering vendor                                       | V-0001         |
| `InvoiceAccount`        | Invoicing vendor                                      | V-0001         |
| `PurchName`             | PO name                                               | ABC Trading    |
| `PurchStatus`           | Purchase status                                       | Backorder      |
| `DocumentState`         | Workflow/confirmation state                           | Confirmed      |
| `PurchaseType`          | Purchase order type                                   | Purchase order |
| `CurrencyCode`          | PO currency                                           | SAR            |
| `DeliveryDate`          | Delivery date                                         | 2026-10-28     |
| `Payment`               | Payment terms                                         | NET30          |
| `PaymMode`              | Payment method                                        | BANK           |
| `DlvMode`               | Delivery mode                                         | STANDARD       |
| `DlvTerm`               | Delivery terms                                        | Optional       |
| `PurchPoolId`           | Purchase pool                                         | Optional       |
| `InventSiteId`          | Default site, where supported by header configuration | SITE-01        |
| `InventLocationId`      | Default warehouse                                     | WH-01          |
| `DeliveryName`          | Delivery address name                                 | Main warehouse |
| `DeliveryPostalAddress` | Delivery address reference                            | RecId          |
| `TaxGroup`              | Tax group                                             | VAT15          |
| `DefaultDimension`      | Financial dimensions                                  | RecId          |
| `DataAreaId`            | Company                                               | USMF           |
| `RecId`                 | Primary internal ID                                   | Generated      |
| `CreatedDateTime`       | Created datetime                                      | System         |
| `CreatedBy`             | Created by                                            | User           |
| `ModifiedDateTime`      | Last update                                           | System         |

Additional purchase header fields exist, but these are the core fields for a basic order.

Do not persist a custom `TotalAmount` as the authoritative equivalent of D365 order totals. Standard totals are calculated using lines, discounts, charges, taxes, and currency rounding.

# 6. Purchase order lines

## Table: `PurchLine`

This is the primary table for PO inventory behavior.

| Field                   | Description                                          | Example        |
| ----------------------- | ---------------------------------------------------- | -------------- |
| `RecId`                 | Unique line ID                                       | Generated      |
| `PurchId`               | PO number                                            | PO-0001001     |
| `LineNumber`            | PO line number                                       | 1.00           |
| `ItemId`                | Item number                                          | ITEM-001       |
| `Name`                  | Line description                                     | Cotton uniform |
| `PurchStatus`           | Line purchase status                                 | Backorder      |
| `PurchQty`              | Ordered quantity, purchase unit                      | 100            |
| `PurchUnit`             | Purchase unit                                        | PCS            |
| `PurchPrice`            | Purchase price                                       | 50.00          |
| `PriceUnit`             | Price quantity basis                                 | 1              |
| `LineAmount`            | Net line amount                                      | 5,000          |
| `LineDisc`              | Line discount amount                                 | 0              |
| `LinePercent`           | Discount percentage                                  | 0              |
| `InventDimId`           | Inventory dimension combination                      | DIM-001        |
| `InventTransId`         | Inventory transaction origin identifier              | Generated      |
| `RemainPurchPhysical`   | Quantity not physically received, purchase unit      | 100            |
| `RemainPurchFinancial`  | Quantity not financially invoiced, purchase unit     | 100            |
| `RemainInventPhysical`  | Remaining physical qty, inventory unit               | 100            |
| `RemainInventFinancial` | Remaining financial qty, inventory unit              | 100            |
| `InventNow`             | Quantity selected for inventory posting, where used  | 0              |
| `PurchReceivedNow`      | Quantity selected for receipt processing, where used | 0              |
| `DeliveryDate`          | Expected delivery date                               | 2026-10-28     |
| `TaxGroup`              | Sales tax group                                      | VAT15          |
| `TaxItemGroup`          | Item sales tax group                                 | STANDARD       |
| `DefaultDimension`      | Financial dimensions                                 | RecId          |
| `DataAreaId`            | Company                                              | USMF           |
| `CreatedDateTime`       | Created date                                         | System         |

Core relationships:

`PurchLine.PurchId` → `PurchTable.PurchId`

`PurchLine.InventDimId` → `InventDim.InventDimId`

`PurchLine.InventTransId` → `InventTransOrigin.InventTransId`

The relationship is company-aware; use standard relations rather than joining by field value alone.

# 7. Inventory transaction tables

## Table: `InventTransOrigin`

Links the purchase line to its inventory movements.

| Field               | Description                         |
| ------------------- | ----------------------------------- |
| `RecId`             | Inventory transaction origin RecId  |
| `InventTransId`     | Inventory transaction reference ID  |
| `ItemId`            | Item                                |
| `ReferenceCategory` | Source type, such as purchase order |
| `ReferenceId`       | Source document ID                  |
| `DataAreaId`        | Company                             |

Example: PO-0001001, line 1, inventory origin 5637000011.

## Table: `InventTrans`

The most important physical/financial inventory transaction table.

| Field                  | Description                            | Example               |
| ---------------------- | -------------------------------------- | --------------------- |
| `RecId`                | Inventory transaction record           | Generated             |
| `InventTransOrigin`    | Origin reference                       | 5637000011            |
| `ItemId`               | Item                                   | ITEM-001              |
| `InventDimId`          | Inventory dimensions                   | DIM-001               |
| `Qty`                  | Inventory quantity, signed             | +100                  |
| `StatusReceipt`        | Receipt status                         | Ordered               |
| `StatusIssue`          | Issue status                           | None                  |
| `DatePhysical`         | Physical posting date                  | Blank initially       |
| `DateFinancial`        | Financial posting date                 | Blank initially       |
| `VoucherPhysical`      | Physical posting voucher               | After receipt         |
| `Voucher`              | Financial voucher                      | After invoice         |
| `CostAmountPhysical`   | Physical cost amount                   | 3,000 for 60 received |
| `CostAmountPosted`     | Financial posted cost                  | After invoice         |
| `CostAmountAdjustment` | Subsequent cost adjustment             | 0 initially           |
| `CurrencyCode`         | Transaction currency, where applicable | SAR                   |
| `DataAreaId`           | Company                                | USMF                  |

Not every example value is contained in one row. Partial receipts and financial updates may produce separate inventory transaction records.

### Status examples

| Event                   | StatusReceipt | Quantity |
| ----------------------- | ------------- | -------- |
| Create approved PO      | Ordered       | +100     |
| Register 30             | Registered    | +30      |
| Post product receipt 60 | Received      | +60      |
| Post vendor invoice 60  | Purchased     | +60      |

These are alternative/split states, not four cumulative independent receipts.

## Table: `InventSum`

On-hand quantity summary.

| Field            | Description                                  |
| ---------------- | -------------------------------------------- |
| `ItemId`         | Item                                         |
| `InventDimId`    | Inventory dimension                          |
| `PostedQty`      | Financially posted quantity                  |
| `Received`       | Physically received, not financially updated |
| `Deducted`       | Physically deducted, not financially updated |
| `Registered`     | Registered quantity                          |
| `ReservPhysical` | Physical reservation                         |
| `ReservOrdered`  | Ordered reservation                          |
| `Ordered`        | Inbound ordered quantity                     |
| `OnOrder`        | Outbound ordered quantity                    |
| `PostedValue`    | Financially posted inventory value           |
| `PhysicalValue`  | Physical inventory value                     |
| `DataAreaId`     | Company                                      |
| `RecId`          | Record identifier                            |

Actual displayed physical on-hand and available physical inventory are calculated from the relevant quantities and dimensions. Don't store them as extra independent balances without a controlled calculation strategy.

# 8. Purchase confirmation tables

## Table: `VendPurchOrderJour`

Posted purchase order confirmation header.

| Field                 | Description                     |
| --------------------- | ------------------------------- |
| `RecId`               | Confirmation journal identifier |
| `PurchId`             | Purchase order                  |
| `PurchaseOrderId`     | Confirmation document ID        |
| `PurchOrderDate`      | Confirmation document date      |
| `OrderAccount`        | Vendor                          |
| `Amount`              | Confirmation amount             |
| `AmountPurchaseOrder` | Purchase order amount           |
| `SumTax`              | Tax total                       |
| `SumMarkup`           | Charges total                   |
| `SumLineDisc`         | Line discounts                  |
| `Qty`                 | Confirmed quantity total        |
| `ParmId`              | Posting parameter reference     |
| `PurchTableVersion`   | Confirmed PO version reference  |
| `DataAreaId`          | Company                         |

These are documented standard fields.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



## Table: `VendPurchOrderTrans`

Posted confirmation lines — important core fields.

| Field           | Description                     |
| --------------- | ------------------------------- |
| `RecId`         | Confirmation line identifier    |
| `PurchId`       | Source PO                       |
| `ItemId`        | Item                            |
| `InventTransId` | Inventory transaction reference |
| `Qty`           | Confirmed quantity              |
| `PurchUnit`     | Purchase unit                   |
| `PurchPrice`    | Confirmed price                 |
| `PriceUnit`     | Price unit basis                |
| `LineAmount`    | Confirmed line amount           |
| `InventDimId`   | Inventory dimensions            |
| `DataAreaId`    | Company                         |

Keep the confirmation line's version/journal association when implementing the relation. Confirm exact field metadata from the target environment.

# 9. Product receipt tables

## Table: `VendPackingSlipJour`

Posted product receipt header.

| Field                   | Description                   | Example        |
| ----------------------- | ----------------------------- | -------------- |
| `RecId`                 | Receipt journal record        | Generated      |
| `PurchId`               | Purchase order                | PO-0001001     |
| `PackingSlipId`         | Vendor product receipt number | PR-0001        |
| `DeliveryDate`          | Physical delivery date        | 2026-10-28     |
| `DocumentDate`          | Document date                 | 2026-10-28     |
| `OrderAccount`          | Ordering vendor               | V-0001         |
| `InvoiceAccount`        | Invoice vendor                | V-0001         |
| `DeliveryName`          | Delivery name                 | Main warehouse |
| `DeliveryPostalAddress` | Address reference             | RecId          |
| `DefaultDimension`      | Financial dimensions          | RecId          |
| `DataAreaId`            | Company                       | USMF           |

Receipt accounting references and additional delivery information depend on the posting configuration.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



## Table: `VendPackingSlipTrans`

Posted receipt detail lines.

| Field                 | Description                          | Example              |
| --------------------- | ------------------------------------ | -------------------- |
| `RecId`               | Receipt line identifier              | Generated            |
| `PackingSlipId`       | Product receipt number               | PR-0001              |
| `OrigPurchId`         | Original purchase order              | PO-0001001           |
| `ItemId`              | Item                                 | ITEM-001             |
| `InventTransId`       | Inventory origin reference           | Generated            |
| `InventDimId`         | Inventory dimensions                 | DIM-001              |
| `Qty`                 | Physically received quantity         | 60                   |
| `PurchUnit`           | Purchase unit                        | PCS                  |
| `PriceUnit`           | Price quantity basis                 | 1                    |
| `DeliveryDate`        | Delivery date                        | 2026-10-28           |
| `ValueMST`            | Receipt value in accounting currency | 3,000                |
| `Remain`              | Remaining quantity information       | Depending on receipt |
| `RemainInvent`        | Remaining inventory-unit quantity    | Depending on receipt |
| `DefaultDimension`    | Financial dimensions                 | RecId                |
| `VendPackingSlipJour` | Receipt journal reference            | RecId                |
| `DataAreaId`          | Company                              | USMF                 |

Microsoft's CDM field reference also identifies `PurchaseLineLineNumber`, `SourceDocumentLine`, and other receipt-line references.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



## Table: `VendPackingSlipPurchLink`

Supporting relation between product receipt documents and purchase orders.

| Field                 | Description          |
| --------------------- | -------------------- |
| `RecId`               | Record ID            |
| `VendPackingSlipJour` | Receipt header RecId |
| `OrigPurchId`         | Original PO          |
| `PackingSlipId`       | Receipt ID           |
| `DeliveryDate`        | Receipt date         |
| `OrderAccount`        | Ordering vendor      |
| `InvoiceAccount`      | Invoice vendor       |
| `DataAreaId`          | Company              |

Useful when a product receipt document has multiple linked purchase orders.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



# 10. Pending vendor invoice tables

## Table: `VendInvoiceInfoTable`

| Field            | Description                    |
| ---------------- | ------------------------------ |
| `RecId`          | Pending invoice record ID      |
| `ParmId`         | Parameter/processing reference |
| `TableRefId`     | Invoice processing reference   |
| `Num`            | Vendor invoice number          |
| `InvoiceAccount` | Invoice vendor                 |
| `OrderAccount`   | Ordering vendor                |
| `DocumentDate`   | Vendor document date           |
| `TransDate`      | Transaction/accounting date    |
| `CurrencyCode`   | Currency                       |
| `Description`    | Invoice description            |
| `DataAreaId`     | Company                        |

These are representative core fields; the pending-invoice framework has additional header, subtable, and matching relationships.

## Table: `VendInvoiceInfoLine`

| Field           | Description                  |
| --------------- | ---------------------------- |
| `RecId`         | Pending line ID              |
| `ParmId`        | Parameter reference          |
| `TableRefId`    | Header/processing reference  |
| `OrigPurchId`   | Purchase order               |
| `LineNum`       | Invoice line sequence        |
| `ItemId`        | Item                         |
| `InventTransId` | Inventory reference          |
| `InventDimId`   | Inventory dimensions         |
| `PurchUnit`     | Purchasing unit              |
| `ReceiveNow`    | Quantity selected to invoice |
| `PurchPrice`    | Invoice unit price           |
| `LineAmount`    | Invoice line amount          |
| `DataAreaId`    | Company                      |

The distinction between vendor invoice quantity, purchase quantity, receipt matching quantity, and inventory quantity must be retained.

# 11. Posted vendor invoice tables

## Table: `VendInvoiceJour`

| Field               | Description                                 | Example                 |
| ------------------- | ------------------------------------------- | ----------------------- |
| `RecId`             | Posted invoice record                       | Generated               |
| `InvoiceId`         | Vendor invoice number                       | INV-0001                |
| `PurchId`           | Purchase order                              | PO-0001001              |
| `InvoiceAccount`    | Invoice vendor                              | V-0001                  |
| `OrderAccount`      | Ordering vendor                             | V-0001                  |
| `InvoiceDate`       | Invoice date                                | 2026-10-29              |
| `DocumentDate`      | Original document date                      | 2026-10-29              |
| `DueDate`           | Payment due date                            | 2026-11-28              |
| `CurrencyCode`      | Currency                                    | SAR                     |
| `InvoiceAmount`     | Invoice amount, including tax as applicable | 3,450                   |
| `InvoiceAmountMST`  | Accounting currency amount                  | 3,450                   |
| `LedgerVoucher`     | Financial ledger voucher                    | Generated               |
| `CostLedgerVoucher` | Inventory cost voucher reference            | Generated if applicable |
| `ExchRate`          | Currency exchange rate                      | Applicable rate         |
| `Payment`           | Terms of payment                            | NET30                   |
| `PostingProfile`    | AP posting profile                          | GEN                     |
| `DefaultDimension`  | Financial dimensions                        | RecId                   |
| `DataAreaId`        | Company                                     | USMF                    |

Confirmed in the Microsoft vendor invoice journal schema.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



## Table: `VendInvoiceTrans`

| Field              | Description          |
| ------------------ | -------------------- |
| `RecId`            | Invoice line record  |
| `InvoiceId`        | Vendor invoice ID    |
| `PurchId`          | Purchase order       |
| `ItemId`           | Item                 |
| `InventTransId`    | Inventory reference  |
| `InventDimId`      | Inventory dimensions |
| `Qty`              | Invoiced quantity    |
| `PurchUnit`        | Purchasing unit      |
| `PurchPrice`       | Unit price           |
| `PriceUnit`        | Price unit basis     |
| `LineAmount`       | Invoice line amount  |
| `LineDisc`         | Line discount        |
| `LinePercent`      | Discount percent     |
| `TaxGroup`         | Tax group            |
| `TaxItemGroup`     | Item tax group       |
| `DefaultDimension` | Financial dimensions |
| `DataAreaId`       | Company              |

Example:

- `InvoiceId = INV-0001`
- `ItemId = ITEM-001`
- `Qty = 60`
- `PurchPrice = 50`
- `LineAmount = 3000`

# 12. Vendor financial transactions

## Table: `VendTrans`

| Field             | Description                 |
| ----------------- | --------------------------- |
| `RecId`           | Vendor transaction ID       |
| `AccountNum`      | Vendor account              |
| `Invoice`         | Invoice reference           |
| `Voucher`         | Financial voucher           |
| `TransDate`       | Transaction date            |
| `DueDate`         | Due date                    |
| `CurrencyCode`    | Currency                    |
| `AmountCur`       | Original transaction amount |
| `SettleAmountCur` | Settled amount              |
| `LastSettleDate`  | Last settlement date        |
| `TransType`       | Transaction type            |
| `Txt`             | Description                 |
| `DataAreaId`      | Company                     |

## Table: `VendSettlement`

| Field             | Description                           |
| ----------------- | ------------------------------------- |
| `RecId`           | Settlement record ID                  |
| `TransRecId`      | Vendor transaction reference          |
| `OffsetRecId`     | Offset transaction reference          |
| `AccountNum`      | Vendor account                        |
| `SettleAmountCur` | Settled amount                        |
| `SettleAmountMST` | Settled amount in accounting currency |
| `TransDate`       | Settlement date                       |
| `DataAreaId`      | Company                               |

# 13. Inventory posting and general ledger

## Table: `InventPosting`

| Field               | Description                                   |
| ------------------- | --------------------------------------------- |
| `RecId`             | Posting setup record                          |
| `InventAccountType` | Inventory posting type                        |
| `ItemCode`          | Item selection scope                          |
| `ItemRelation`      | Item/item group relation                      |
| `AccountCode`       | Account selection scope                       |
| `AccountRelation`   | Vendor/vendor group relation where applicable |
| `LedgerDimension`   | Posting ledger account                        |
| `DataAreaId`        | Company                                       |

## Table: `InventTransPosting`

| Field                    | Description           |
| ------------------------ | --------------------- |
| `RecId`                  | Posting reference     |
| `InventTransOrigin`      | Inventory origin      |
| `Voucher`                | Financial voucher     |
| `TransDate`              | Posting date          |
| `InventTransPostingType` | Posting type          |
| `LedgerDimension`        | Account combination   |
| `CostAmount`             | Posted inventory cost |
| `DataAreaId`             | Company               |

Posting-field availability and the way accounts are represented should be checked against the target version.

## Table: `GeneralJournalEntry`

| Field                        | Description           |
| ---------------------------- | --------------------- |
| `RecId`                      | Journal header record |
| `AccountingDate`             | Accounting date       |
| `SubledgerVoucher`           | Source voucher        |
| `SubledgerVoucherDataAreaId` | Source company        |
| `JournalNumber`              | Journal number        |
| `Ledger`                     | Ledger reference      |
| `JournalCategory`            | Journal category      |

## Table: `GeneralJournalAccountEntry`

| Field                       | Description                           |
| --------------------------- | ------------------------------------- |
| `RecId`                     | Entry record                          |
| `GeneralJournalEntry`       | Journal header                        |
| `LedgerDimension`           | Main account and financial dimensions |
| `AccountingCurrencyAmount`  | Signed accounting currency amount     |
| `TransactionCurrencyAmount` | Transaction currency amount           |
| `TransactionCurrencyCode`   | Currency                              |
| `PostingType`               | Posting type                          |
| `Text`                      | Description                           |

Accounting entries use signed amounts rather than independent debit/credit columns in this table.

# 14. Vendor payment journals

## Table: `LedgerJournalTable`

| Field         | Description    |
| ------------- | -------------- |
| `RecId`       | Journal record |
| `JournalNum`  | Journal number |
| `JournalName` | Journal name   |
| `Name`        | Description    |
| `JournalType` | Journal type   |
| `Posted`      | Posted flag    |
| `DataAreaId`  | Company        |

## Table: `LedgerJournalTrans`

| Field                   | Description            |
| ----------------------- | ---------------------- |
| `RecId`                 | Journal transaction ID |
| `JournalNum`            | Journal ID             |
| `Voucher`               | Voucher number         |
| `TransDate`             | Transaction date       |
| `AccountType`           | Account type           |
| `LedgerDimension`       | Main journal account   |
| `OffsetAccountType`     | Offset account type    |
| `OffsetLedgerDimension` | Offset account         |
| `CurrencyCode`          | Currency               |
| `AmountCurDebit`        | Debit                  |
| `AmountCurCredit`       | Credit                 |
| `PaymMode`              | Payment method         |
| `Txt`                   | Description            |
| `DataAreaId`            | Company                |

# 15. Additional basic supporting tables

These tables are necessary when the corresponding standard feature is enabled. They are not separate custom purchasing modules.

| Table                                 | Core fields                                                                                                   | Use                                          |
| ------------------------------------- | ------------------------------------------------------------------------------------------------------------- | -------------------------------------------- |
| `PurchParameters`                     | `DataAreaId`, `RecId` and purchasing parameter fields                                                         | PO defaults and controls                     |
| `VendParameters`                      | `DataAreaId`, `RecId` and AP parameter fields                                                                 | Invoice and vendor processing                |
| `VendGroup`                           | `VendGroup`, `Name`, `DataAreaId`                                                                             | Vendor grouping                              |
| `VendPostingProfile`                  | Posting profile identifiers and account setup fields                                                          | Vendor posting setup; verify actual metadata |
| `InventItemGroupItem`                 | `ItemId`, `ItemGroupId`, `DataAreaId`                                                                         | Item group assignment                        |
| `InventDimGroup`                      | `InventDimGroupId`, `Name`                                                                                    | Inventory dimension group                    |
| `UnitOfMeasure`                       | `RecId`, `Symbol`, `UnitOfMeasureClass`                                                                       | Unit definitions                             |
| `UnitOfMeasureConversion`             | `FromUnitOfMeasure`, `ToUnitOfMeasure`, `Factor`, `Numerator`, `Denominator`, `Product`                       | UOM conversion                               |
| `PriceDiscTable`                      | `Relation`, `AccountCode`, `AccountRelation`, `ItemCode`, `ItemRelation`, `Amount`, `Currency`, date fields   | Purchase trade agreements                    |
| `MarkupTable`                         | `MarkupCode`, `Txt`                                                                                           | Purchase charge codes                        |
| `MarkupTrans`                         | `TransTableId`, `TransRecId`, `MarkupCode`, `Value`, `CurrencyCode`                                           | Applied purchase charges                     |
| `TaxGroupHeading`                     | `TaxGroup`, `DataAreaId`                                                                                      | Sales tax group                              |
| `TaxItemGroupHeading`                 | `TaxItemGroup`, `DataAreaId`                                                                                  | Item tax group                               |
| `TaxTrans`                            | `Voucher`, `TransDate`, `TaxCode`, `TaxBaseAmount`, `TaxAmount`, `SourceTableId`, `SourceRecId`, `DataAreaId` | Posted purchase tax                          |
| `Currency`                            | `CurrencyCode` and currency definition fields                                                                 | Transaction currency                         |
| `DimensionAttributeValueSet`          | `RecId`                                                                                                       | Financial dimension set                      |
| `LogisticsPostalAddress`              | `RecId`, `Location`, `Street`, `City`, `ZipCode`, `CountryRegionId`                                           | Delivery addresses                           |
| `PurchParmTable`                      | `ParmId`, `TableRefId`, `PurchId`, `DataAreaId`                                                               | PO posting preparation                       |
| `PurchParmLine`                       | `ParmId`, `TableRefId`, `OrigPurchId`, `ItemId`, `InventTransId`, `ReceiveNow`                                | PO posting line preparation                  |
| `PurchTableVersion`                   | `RecId`, `PurchId`, version-related metadata                                                                  | Change management, if enabled                |
| `PurchLineHistory`                    | Line/version references                                                                                       | Purchase line change history, if enabled     |
| `VendInvoicePackingSlipQuantityMatch` | Invoice line and receipt line references, matched quantity                                                    | Receipt-to-invoice matching                  |

Some parameter and configuration tables contain many version-specific fields. The table names and identifying fields above should be treated as the implementation starting point, not an assertion that the few fields listed represent each table's complete schema.

# 16. Required relationships — Basic Purchase Order ERP

| From                         | To                    | Key relationship                       |
| ---------------------------- | --------------------- | -------------------------------------- |
| `VendTable`                  | `DirPartyTable`       | `Party → RecId`                        |
| `InventTable`                | `EcoResProduct`       | `Product → RecId`                      |
| `InventTableModule`          | `InventTable`         | `ItemId`, company                      |
| `PurchTable`                 | `VendTable`           | `OrderAccount → AccountNum`, company   |
| `PurchLine`                  | `PurchTable`          | `PurchId`, company                     |
| `PurchLine`                  | `InventTable`         | `ItemId`, company                      |
| `PurchLine`                  | `InventDim`           | `InventDimId`, company                 |
| `PurchLine`                  | `InventTransOrigin`   | `InventTransId`, company               |
| `InventTrans`                | `InventTransOrigin`   | `InventTransOrigin → RecId`            |
| `InventSum`                  | `InventDim`           | `InventDimId`, company                 |
| `VendPurchOrderJour`         | `PurchTable`          | `PurchId`, company and version context |
| `VendPackingSlipTrans`       | `VendPackingSlipJour` | Receipt journal reference              |
| `VendInvoiceTrans`           | `VendInvoiceJour`     | Posted invoice journal relation        |
| `VendInvoiceJour`            | `VendTrans`           | Financial voucher/vendor context       |
| `VendTrans`                  | `VendSettlement`      | Vendor transaction `RecId` references  |
| `GeneralJournalAccountEntry` | `GeneralJournalEntry` | `GeneralJournalEntry → RecId`          |

Use exact D365 metadata relations for posting documents. A simple match on `PurchId`, `InvoiceId`, or `Voucher` is not always sufficient to identify the correct record.

# 17. What to implement first

For a basic ERP, prioritize these table groups in this order:

1

Master data

`VendTable`, `InventTable`, `InventTableModule`, `InventDim`, `InventSite`, `InventLocation`

2

Purchase order

`PurchTable`, `PurchLine`, `InventTransOrigin`, `InventTrans`

3

Confirmation

`VendPurchOrderJour`, `VendPurchOrderTrans`

4

Product receipt

`VendPackingSlipJour`, `VendPackingSlipTrans`, `InventSum`

5

Vendor invoice

`VendInvoiceInfoTable`, `VendInvoiceInfoLine`, `VendInvoiceJour`, `VendInvoiceTrans`

6

Accounting

`VendTrans`, `TaxTrans`, `InventPosting`, `GeneralJournalEntry`, `GeneralJournalAccountEntry`

## How to obtain the exact complete field metadata

Because D365 F&O tables have additional fields and extensions that vary by version, use Visual Studio Application Explorer or a read-only X++ metadata inspection to export the definitive field list from your target D365 environment.

For a development/test database, the following SQL returns the physical columns, SQL types, nullability, and field order for the main PO tables:

```
SELECT
    t.name AS TableName,
    c.column_id AS FieldOrder,
    c.name AS FieldName,
    ty.name AS SqlDataType,
    c.max_length AS MaxLength,
    c.precision AS NumericPrecision,
    c.scale AS NumericScale,
    c.is_nullable AS IsNullable
FROM sys.tables t
JOIN sys.columns c
    ON c.object_id = t.object_id
JOIN sys.types ty
    ON ty.user_type_id = c.user_type_id
WHERE t.name IN
(
    'PURCHTABLE',
    'PURCHLINE',
    'INVENTTRANSORIGIN',
    'INVENTTRANS',
    'INVENTSUM',
    'VENDPURCHORDERJOUR',
    'VENDPURCHORDERTRANS',
    'VENDPACKINGSLIPJOUR',
    'VENDPACKINGSLIPTRANS',
    'VENDINVOICEINFOTABLE',
    'VENDINVOICEINFOLINE',
    'VENDINVOICEJOUR',
    'VENDINVOICETRANS',
    'VENDTRANS',
    'VENDSETTLEMENT'
)
ORDER BY
    t.name,
    c.column_id;
```

This is a read-only metadata query. It shows physical SQL columns, not necessarily every AOT field property, extension relationship, display method, or business validation.

For implementation, maintain three distinctions for every field: persisted D365 field, derived/display-only value, and configurable business rule. That prevents the ERP from introducing duplicate balances, invalid relationships, or incorrect posting behavior.