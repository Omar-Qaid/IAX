# D365 F&O — Sales Order Core ERP: Level 2 Technical Specification

Field-by-field implementation, table relationships, validations, defaulting logic, posting behavior and database verification.

This is the next implementation level for building your ERP in alignment with standard Dynamics 365 Finance & Operations.

Scope: Standard Sales Order → Lines → Inventory → Reservation → Confirmation → Basic Picking → Packing Slip → Invoice → Payment → Return.

Excluded: WHS, manufacturing, production, MRP, advanced logistics and custom entities.

## 1. How to read the field dictionary

| Notation   | Meaning                                                |
| ---------- | ------------------------------------------------------ |
| Key        | Identifier or relationship field                       |
| Required   | Needed for the basic process                           |
| Default    | Populated from customer, product or parameters         |
| Calculated | Maintained by standard business logic                  |
| System     | Maintained during creation or posting                  |
| Optional   | Implement when that basic business scenario is enabled |

Types are shown at the logical/X++ type level (`String`, `Enum`, `Real`, `Date`, `Int64/RecId`), not as guessed SQL Server column definitions. Exact EDTs, string lengths, enum values, mandatory metadata and physical SQL types must be read from the target D365 F&O AOT version.

The field dictionary below deliberately lists the core implementation fields, not country-specific, localization-specific, obsolete or advanced-feature fields.

# 2. Sales order header — `SalesTable`

Purpose: Store the commercial header of a sales order.

Form: All sales orders → New / General / Header details.

## 2.1 Essential fields

| Field                   | Logical type       | Required / source     | Function                       |
| ----------------------- | ------------------ | --------------------- | ------------------------------ |
| `RecId`                 | Int64              | System                | Internal record identity       |
| `DataAreaId`            | String             | System                | Legal entity                   |
| `SalesId`               | String             | Key, number sequence  | Sales order number             |
| `SalesType`             | Enum               | Default               | Sales order type               |
| `SalesStatus`           | Enum               | System                | Order fulfillment status       |
| `DocumentStatus`        | Enum               | System                | Latest document status         |
| `CustAccount`           | String             | Required, `CustTable` | Ordering customer              |
| `InvoiceAccount`        | String             | Default, `CustTable`  | Invoice customer               |
| `SalesName`             | String             | Default               | Sales order description        |
| `CustGroup`             | String             | Default               | Customer group                 |
| `CurrencyCode`          | String             | Default               | Sales currency                 |
| `Payment`               | String             | Default               | Payment terms                  |
| `PaymMode`              | String             | Default               | Payment method                 |
| `CashDisc`              | String             | Optional/default      | Cash discount code             |
| `TaxGroup`              | String             | Default               | Sales tax group                |
| `DlvMode`               | String             | Default               | Delivery mode                  |
| `DlvTerm`               | String             | Default               | Delivery terms                 |
| `DeliveryDate`          | Date               | Optional/default      | Delivery date                  |
| `DeliveryName`          | String             | Default               | Delivery recipient             |
| `DeliveryPostalAddress` | Int64              | Default               | Delivery address reference     |
| `InvoicePostalAddress`  | Int64              | Default               | Invoice address reference      |
| `PurchOrderFormNum`     | String             | Optional              | Customer purchase order number |
| `CustomerRef`           | String             | Optional              | Customer reference             |
| `DefaultDimension`      | Int64              | Default               | Financial dimension set        |
| `LanguageId`            | String             | Default               | Document language              |
| `InclTax`               | Enum/Boolean-style | Default               | Prices including sales tax     |
| `ReceiptDateRequested`  | Date               | Optional              | Requested receipt date         |
| `ShippingDateRequested` | Date               | Optional              | Requested shipping date        |

Do not add an invented `TotalAmount`, `VATAmount`, `WarehouseId` or `IsConfirmed` column to `SalesTable` simply because the form displays those concepts. Order totals are generally calculated through the relevant totals framework, and inventory dimensions are handled through their standard references.

## 2.2 Header defaulting sequence

When user selects `CustAccount = CUST-001`:

| Target field            | Source                                      | Expected value  |
| ----------------------- | ------------------------------------------- | --------------- |
| `InvoiceAccount`        | `CustTable.InvoiceAccount` / customer logic | CUST-001        |
| `SalesName`             | Customer party name                         | ABC Trading     |
| `CustGroup`             | `CustTable.CustGroup`                       | DOMESTIC        |
| `CurrencyCode`          | `CustTable.Currency`                        | SAR             |
| `Payment`               | `CustTable.PaymTermId`                      | NET30           |
| `PaymMode`              | `CustTable.PaymMode`                        | BANK            |
| `TaxGroup`              | `CustTable.TaxGroup`                        | VAT15           |
| `DeliveryPostalAddress` | Customer address selection                  | Address RecId   |
| `DefaultDimension`      | Customer default dimensions                 | Dimension RecId |

This is the defaulting intent. Standard initialization also honors sales parameters, explicit order input and other configured precedence rules.

### Header event specification

| User action             | Validation                               | Downstream consequence                                   |
| ----------------------- | ---------------------------------------- | -------------------------------------------------------- |
| New order               | Valid number sequence                    | Insert `SalesTable`                                      |
| Select customer         | Customer exists, allowed for sales       | Load defaults                                            |
| Change customer         | Allowed by order state                   | Reevaluate dependent defaults                            |
| Change currency         | Valid currency and permitted order state | Reevaluate prices, exchange rates and amounts            |
| Change payment terms    | Valid payment term                       | Update header and eligible lines/documents as applicable |
| Change tax group        | Valid tax group                          | Optionally propagate to eligible lines and recalculate   |
| Change delivery address | Valid address                            | Apply header-to-line update policy                       |
| Change delivery date    | Date policy                              | Recalculate eligible line dates                          |
| Confirm order           | Posting validation                       | Write confirmation journal                               |

D365 has a header-to-line update mechanism; changing a header value does not universally mean all existing sales lines are overwritten.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://kdynamics.blogspot.com\&sz=32)

kdynamics.blogspot.com



# 3. Sales order line — `SalesLine`

Purpose: Store each ordered product, quantity, price, discount, tax and delivery requirement.

## 3.1 Identity and inventory references

| Field              | Type   | Classification | Description                             |
| ------------------ | ------ | -------------- | --------------------------------------- |
| `RecId`            | Int64  | Key/System     | Unique sales line                       |
| `DataAreaId`       | String | System         | Legal entity                            |
| `SalesId`          | String | Key            | Parent sales order                      |
| `LineNum`          | Real   | System         | Line sequence                           |
| `ItemId`           | String | Required       | Released product                        |
| `Name`             | String | Default        | Line description                        |
| `InventDimId`      | String | System/Default | Inventory dimension combination         |
| `InventTransId`    | String | System         | Inventory transaction origin identifier |
| `SalesStatus`      | Enum   | System         | Line processing status                  |
| `DefaultDimension` | Int64  | Default        | Financial dimensions                    |

## 3.2 Quantity and UOM fields

| Field                   | Type   | Classification | Description                                |
| ----------------------- | ------ | -------------- | ------------------------------------------ |
| `SalesQty`              | Real   | Required       | Ordered quantity in sales unit             |
| `SalesUnit`             | String | Default        | Sales UOM                                  |
| `QtyOrdered`            | Real   | Calculated     | Ordered quantity in inventory unit         |
| `RemainSalesPhysical`   | Real   | System         | Physical delivery remainder in sales UOM   |
| `RemainSalesFinancial`  | Real   | System         | Financial invoicing remainder in sales UOM |
| `RemainInventPhysical`  | Real   | System         | Physical remainder in inventory UOM        |
| `RemainInventFinancial` | Real   | System         | Financial remainder in inventory UOM       |
| `DeliverNow`            | Real   | Optional       | Quantity designated for delivery now       |

These six quantity measures must not be replaced by one generic `RemainingQty` column.

## 3.3 Price, discount and tax fields

| Field            | Type   | Classification      | Description                                                      |
| ---------------- | ------ | ------------------- | ---------------------------------------------------------------- |
| `SalesPrice`     | Real   | Default/editable    | Sales price                                                      |
| `PriceUnit`      | Real   | Default             | Price basis quantity                                             |
| `CurrencyCode`   | String | Default             | Sales currency                                                   |
| `LineAmount`     | Real   | Calculated          | Net sales amount excluding tax in a normal tax-exclusive example |
| `LineDisc`       | Real   | Calculated/editable | Line discount amount                                             |
| `LinePercent`    | Real   | Calculated/editable | Line discount percentage                                         |
| `MultiLnDisc`    | Real   | Calculated          | Multiline discount                                               |
| `MultiLnPercent` | Real   | Calculated          | Multiline discount percentage                                    |
| `TaxGroup`       | String | Default             | Sales tax group                                                  |
| `TaxItemGroup`   | String | Default             | Item sales tax group                                             |

## 3.4 Delivery fields

| Field                   | Type   | Classification | Description             |
| ----------------------- | ------ | -------------- | ----------------------- |
| `ShippingDateRequested` | Date   | Default        | Requested shipping date |
| `ReceiptDateRequested`  | Date   | Default        | Requested receipt date  |
| `ShippingDateConfirmed` | Date   | Optional       | Confirmed shipping date |
| `ReceiptDateConfirmed`  | Date   | Optional       | Confirmed receipt date  |
| `DlvMode`               | String | Default        | Delivery method         |
| `DlvTerm`               | String | Default        | Delivery term           |
| `DeliveryPostalAddress` | Int64  | Default        | Line delivery address   |

### Example line state

```
SalesId                 SO-000125
LineNum                 1
ItemId                  ITEM-001
SalesQty                10
SalesUnit               PCS
QtyOrdered              10
SalesPrice              100
PriceUnit               1
LineDisc                0
LinePercent             0
LineAmount              1000
TaxGroup                VAT15
TaxItemGroup            VAT15
RemainSalesPhysical     10
RemainSalesFinancial    10
RemainInventPhysical    10
RemainInventFinancial   10
SalesStatus             Backorder
```

## 3.5 Line event logic

| Field changed            | Required business action                                       | Main affected records                                   |
| ------------------------ | -------------------------------------------------------------- | ------------------------------------------------------- |
| `ItemId`                 | Load item defaults, validate inventory identity                | `SalesLine`, `InventDim`, inventory origin/transactions |
| `SalesQty`               | Convert quantity, adjust requirements and remaining quantities | `SalesLine`, `InventTrans`                              |
| `SalesUnit`              | Apply unit conversion, reevaluate pricing                      | `SalesLine`, related inventory transactions             |
| `SalesPrice`             | Recalculate net line amount                                    | `SalesLine`                                             |
| `PriceUnit`              | Recalculate price basis                                        | `SalesLine`                                             |
| `LineDisc`               | Recalculate discount and tax basis                             | `SalesLine`                                             |
| `LinePercent`            | Recalculate line amount                                        | `SalesLine`                                             |
| `TaxGroup`               | Recalculate estimated tax                                      | `SalesLine`, tax calculation                            |
| `TaxItemGroup`           | Recalculate estimated tax                                      | `SalesLine`, tax calculation                            |
| `InventDimId`            | Validate dimensions and adjust reservations/transactions       | `SalesLine`, `InventTrans`, `InventSum`                 |
| Requested delivery dates | Recalculate eligible delivery dates                            | `SalesLine`                                             |

Use the actual D365 pricing, reservation and posting logic as the functional reference. A field's `modifiedField()` behavior by itself is not necessarily sufficient to complete a standard business operation.

# 4. Customer and product supporting tables

These are required to make SalesTable and SalesLine usable.

## 4.1 `CustTable` — Customer master

| Field              | Type   | Role                         |
| ------------------ | ------ | ---------------------------- |
| `RecId`            | Int64  | Internal key                 |
| `AccountNum`       | String | Customer account             |
| `Party`            | Int64  | Party reference              |
| `CustGroup`        | String | Customer group               |
| `Currency`         | String | Default currency             |
| `InvoiceAccount`   | String | Invoice customer             |
| `PaymTermId`       | String | Payment terms                |
| `PaymMode`         | String | Payment method               |
| `CashDisc`         | String | Cash discount                |
| `TaxGroup`         | String | Sales tax group              |
| `CreditMax`        | Real   | Credit limit                 |
| `Blocked`          | Enum   | Customer blocking policy     |
| `DefaultDimension` | Int64  | Default financial dimensions |
| `DataAreaId`       | String | Company                      |

Validation: Before creating or posting, check that the customer exists, has the appropriate status and complies with applicable credit-control configuration.

## 4.2 `DirPartyTable` — Customer party

| Field                  | Type   | Role                   |
| ---------------------- | ------ | ---------------------- |
| `RecId`                | Int64  | Party key              |
| `Name`                 | String | Customer name          |
| `NameAlias`            | String | Search name            |
| `PartyNumber`          | String | Party number           |
| `InstanceRelationType` | Int64  | Party subtype metadata |

Relationship:

`CustTable.Party → DirPartyTable.RecId`

## 4.3 Customer address tables

| Table                    | Field                  | Role               |
| ------------------------ | ---------------------- | ------------------ |
| `DirPartyLocation`       | `Party`                | Party reference    |
| `DirPartyLocation`       | `Location`             | Location reference |
| `LogisticsLocation`      | `RecId`                | Location key       |
| `LogisticsPostalAddress` | `RecId`                | Postal address key |
| `LogisticsPostalAddress` | `Location`             | Location reference |
| `LogisticsPostalAddress` | `Street`               | Street             |
| `LogisticsPostalAddress` | `City`                 | City               |
| `LogisticsPostalAddress` | `ZipCode`              | Postal code        |
| `LogisticsPostalAddress` | `CountryRegionId`      | Country            |
| `LogisticsPostalAddress` | `ValidFrom`, `ValidTo` | Address validity   |

Addresses are effective-dated. Do not use an arbitrary postal address record without validating the appropriate location, role and effective date.

## 4.4 `InventTable` — Released product

| Field          | Type   | Role                                                       |
| -------------- | ------ | ---------------------------------------------------------- |
| `RecId`        | Int64  | Internal key                                               |
| `ItemId`       | String | Item number                                                |
| `Product`      | Int64  | Shared product reference                                   |
| `ItemGroupId`  | String | Item group                                                 |
| `ModelGroupId` | String | Inventory model group reference, where used in the version |
| `DimGroupId`   | String | Inventory dimension group reference, where used            |
| `DataAreaId`   | String | Company                                                    |

The product's tracking, storage, costing and dimension-group assignments must be resolved using the corresponding version-specific standard tables. Some group assignments are no longer represented by a single direct field on `InventTable`.

## 4.5 `InventTableModule` — Sales unit and price

| Field            | Type   | Role                     |
| ---------------- | ------ | ------------------------ |
| `ItemId`         | String | Item                     |
| `ModuleType`     | Enum   | Sales/Purchase/Inventory |
| `UnitId`         | String | Default unit             |
| `Price`          | Real   | Default price            |
| `PriceUnit`      | Real   | Price quantity basis     |
| `TaxItemGroupId` | String | Item tax group           |
| `DataAreaId`     | String | Company                  |

For a sales order, read the record with the Sales module type.

## 4.6 Other minimum product records

| Table                      | Essential fields                                                 | Usage                            |
| -------------------------- | ---------------------------------------------------------------- | -------------------------------- |
| `EcoResProduct`            | `RecId`, `DisplayProductNumber`, `ProductType`, `ProductSubType` | Shared product                   |
| `EcoResProductTranslation` | `Product`, `LanguageId`, `Name`, `Description`                   | Product description              |
| `InventItemGroup`          | `ItemGroupId`, `Name`                                            | Inventory posting grouping       |
| `InventModelGroupItem`     | `ItemId`, `ModelGroupId`, `DataAreaId`                           | Inventory model group assignment |
| `InventSite`               | `SiteId`, `Name`                                                 | Site                             |
| `InventLocation`           | `InventLocationId`, `InventSiteId`, `Name`                       | Warehouse                        |
| `UnitOfMeasure`            | `RecId`, `Symbol`                                                | Unit identity                    |
| `UnitOfMeasureConversion`  | `FromUnitOfMeasure`, `ToUnitOfMeasure`, `Factor`                 | Unit conversion                  |

# 5. Inventory transaction tables

## 5.1 `InventDim`

| Field              | Type   | Usage                             |
| ------------------ | ------ | --------------------------------- |
| `InventDimId`      | String | Dimension combination identifier  |
| `InventSiteId`     | String | Site                              |
| `InventLocationId` | String | Warehouse                         |
| `InventBatchId`    | String | Batch, if active                  |
| `InventSerialId`   | String | Serial number, if active          |
| `WMSLocationId`    | String | Warehouse location, if applicable |

In your basic ERP, begin with site and warehouse. Add batch and serial only where the stocked item requires them. Do not build advanced warehouse work around `WMSLocationId`.

## 5.2 `InventTransOrigin`

| Field               | Type   | Usage                         |
| ------------------- | ------ | ----------------------------- |
| `RecId`             | Int64  | Origin key                    |
| `InventTransId`     | String | Origin transaction identifier |
| `ReferenceCategory` | Enum   | Source category               |
| `ReferenceId`       | String | Related source document       |
| `ItemId`            | String | Item reference                |
| `DataAreaId`        | String | Company                       |

Relationship: `SalesLine.InventTransId` connects to the appropriate inventory origin, which connects to the actual `InventTrans` rows.

## 5.3 `InventTrans`

| Field                  | Type   | Usage                     |
| ---------------------- | ------ | ------------------------- |
| `RecId`                | Int64  | Transaction key           |
| `InventTransOrigin`    | Int64  | Origin reference          |
| `InventDimId`          | String | Inventory dimension       |
| `Qty`                  | Real   | Signed inventory quantity |
| `StatusIssue`          | Enum   | Issue transaction status  |
| `StatusReceipt`        | Enum   | Receipt status            |
| `DatePhysical`         | Date   | Physical posting date     |
| `DateFinancial`        | Date   | Financial posting date    |
| `VoucherPhysical`      | String | Physical voucher          |
| `Voucher`              | String | Financial voucher         |
| `CostAmountPhysical`   | Real   | Physical cost             |
| `CostAmountPosted`     | Real   | Financial posted cost     |
| `CostAmountAdjustment` | Real   | Cost adjustment           |
| `DateExpected`         | Date   | Expected inventory date   |
| `DataAreaId`           | String | Company                   |

### Example record progression

| Stage                        | `Qty` | `StatusIssue`                |
| ---------------------------- | ----- | ---------------------------- |
| New sales order              | −10   | On order                     |
| Reserved                     | −10   | Reserved physical            |
| Packing slip for 6           | −6    | Deducted                     |
| Remaining demand             | −4    | Reserved physical / On order |
| Invoice for 4 of delivered 6 | −4    | Sold                         |
| Delivered, not invoiced      | −2    | Deducted                     |
| Remaining to deliver         | −4    | Reserved physical / On order |

One original inventory requirement can split into multiple `InventTrans` rows. Your implementation must not assume a permanent one-to-one relationship between `SalesLine` and `InventTrans`.

## 5.4 `InventSum`

| Field            | Type   | Usage                          |
| ---------------- | ------ | ------------------------------ |
| `RecId`          | Int64  | Balance record                 |
| `ItemId`         | String | Item                           |
| `InventDimId`    | String | Dimension combination          |
| `PostedQty`      | Real   | Posted quantity component      |
| `Received`       | Real   | Physically received component  |
| `Deducted`       | Real   | Physically deducted component  |
| `ReservPhysical` | Real   | Physical reservation component |
| `ReservOrdered`  | Real   | Ordered reservation component  |
| `OnOrder`        | Real   | Outstanding issue component    |
| `Ordered`        | Real   | Ordered receipt component      |
| `DataAreaId`     | String | Company                        |

Do not independently edit `InventSum` quantities from the sales screen. They are inventory balances maintained through inventory processing.

# 6. Confirmation tables

## 6.1 `CustConfirmJour`

| Field                   | Type   | Usage                       |
| ----------------------- | ------ | --------------------------- |
| `RecId`                 | Int64  | Journal identity            |
| `SalesId`               | String | Sales order                 |
| `ConfirmId`             | String | Confirmation number         |
| `ConfirmDate`           | Date   | Confirmation date           |
| `OrderAccount`          | String | Ordering customer           |
| `InvoiceAccount`        | String | Invoice customer            |
| `CurrencyCode`          | String | Currency                    |
| `SalesBalance`          | Real   | Sales balance amount        |
| `ConfirmAmount`         | Real   | Confirmation amount         |
| `SumTax`                | Real   | Tax total                   |
| `SumMarkup`             | Real   | Charges total               |
| `DeliveryPostalAddress` | Int64  | Delivery address            |
| `Payment`               | String | Payment terms               |
| `ParmId`                | String | Posting parameter reference |
| `DataAreaId`            | String | Company                     |

Microsoft publishes these attributes in its Common Data Model documentation.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



## 6.2 `CustConfirmTrans`

Core fields to map:

| Field           | Type   | Usage                      |
| --------------- | ------ | -------------------------- |
| `RecId`         | Int64  | Journal line identity      |
| `ConfirmId`     | String | Confirmation document      |
| `SalesId`       | String | Sales order                |
| `LineNum`       | Real   | Original line number       |
| `ItemId`        | String | Item                       |
| `Qty`           | Real   | Confirmed quantity         |
| `SalesPrice`    | Real   | Confirmed sales price      |
| `LineAmount`    | Real   | Confirmed line amount      |
| `InventDimId`   | String | Inventory dimensions       |
| `InventTransId` | String | Inventory origin reference |
| `DataAreaId`    | String | Company                    |

The posted confirmation must preserve historical pricing and quantity even when subsequent allowed order changes occur.

# 7. Packing slip tables

## 7.1 `CustPackingSlipJour`

| Field                   | Type   | Usage                  |
| ----------------------- | ------ | ---------------------- |
| `RecId`                 | Int64  | Journal identity       |
| `PackingSlipId`         | String | Packing slip number    |
| `SalesId`               | String | Sales order            |
| `DeliveryDate`          | Date   | Physical delivery date |
| `OrderAccount`          | String | Ordering customer      |
| `InvoiceAccount`        | String | Invoice customer       |
| `CurrencyCode`          | String | Currency               |
| `DeliveryName`          | String | Delivery name          |
| `DeliveryPostalAddress` | Int64  | Delivery address       |
| `DlvMode`               | String | Delivery method        |
| `DlvTerm`               | String | Delivery terms         |
| `DataAreaId`            | String | Company                |

## 7.2 `CustPackingSlipTrans`

| Field           | Type   | Usage                                |
| --------------- | ------ | ------------------------------------ |
| `RecId`         | Int64  | Journal line identity                |
| `PackingSlipId` | String | Packing slip number                  |
| `SalesId`       | String | Sales order                          |
| `LineNum`       | Real   | Sales line                           |
| `ItemId`        | String | Product                              |
| `Qty`           | Real   | Delivered quantity in sales unit     |
| `InventQty`     | Real   | Delivered quantity in inventory unit |
| `InventDimId`   | String | Dimensions                           |
| `InventTransId` | String | Inventory origin reference           |
| `DataAreaId`    | String | Company                              |

### Posting effect

```
Packing slip PS-000125
ITEM-001 quantity 6 PCS

CustPackingSlipJour   Insert journal
CustPackingSlipTrans  Insert delivered line
SalesLine             Update physical remainder
InventTrans           Issue quantity → Deducted
InventSum             Update physical on-hand
SalesTable            Update document/fulfillment state
```

All related operations must succeed or roll back together.

# 8. Invoice tables

## 8.1 `CustInvoiceJour`

| Field            | Type   | Usage                    |
| ---------------- | ------ | ------------------------ |
| `RecId`          | Int64  | Invoice journal identity |
| `InvoiceId`      | String | Invoice number           |
| `SalesId`        | String | Source sales order       |
| `InvoiceDate`    | Date   | Invoice date             |
| `OrderAccount`   | String | Ordering customer        |
| `InvoiceAccount` | String | Invoice customer         |
| `CurrencyCode`   | String | Currency                 |
| `InvoiceAmount`  | Real   | Invoice total            |
| `SumTax`         | Real   | Tax amount               |
| `SumMarkup`      | Real   | Charges amount           |
| `LedgerVoucher`  | String | Posting voucher          |
| `DueDate`        | Date   | Invoice due date         |
| `DataAreaId`     | String | Company                  |

## 8.2 `CustInvoiceTrans`

| Field           | Type   | Usage                 |
| --------------- | ------ | --------------------- |
| `RecId`         | Int64  | Invoice line identity |
| `InvoiceId`     | String | Invoice number        |
| `SalesId`       | String | Source sales order    |
| `LineNum`       | Real   | Sales line            |
| `ItemId`        | String | Product               |
| `Qty`           | Real   | Invoiced quantity     |
| `SalesPrice`    | Real   | Unit price            |
| `LineAmount`    | Real   | Line amount           |
| `TaxGroup`      | String | Customer tax group    |
| `TaxItemGroup`  | String | Item tax group        |
| `InventDimId`   | String | Inventory dimensions  |
| `InventTransId` | String | Inventory origin      |
| `DataAreaId`    | String | Company               |

## 8.3 Invoice posting relationships

Invoice posting updates more than the invoice tables:

```
CustInvoiceJour
       |
       +-- CustInvoiceTrans
       |
       +-- CustTrans
       |
       +-- TaxTrans
       |
       +-- GeneralJournalEntry
              |
              +-- GeneralJournalAccountEntry

SalesLine
       |
       +-- InventTransOrigin
               |
               +-- InventTrans (Sold)
```

These are logical processing relationships, not all direct SQL foreign keys.

# 9. Pricing, discounts, taxes and charges

## 9.1 `PriceDiscTable` — Trade agreements

| Field                | Type   | Role                       |
| -------------------- | ------ | -------------------------- |
| `RecId`              | Int64  | Record identity            |
| `Relation`           | Enum   | Price or discount relation |
| `AccountCode`        | Enum   | Customer scope             |
| `AccountRelation`    | String | Customer/customer group    |
| `ItemCode`           | Enum   | Item scope                 |
| `ItemRelation`       | String | Item/item group            |
| `Amount`             | Real   | Price or discount amount   |
| `Currency`           | String | Agreement currency         |
| `UnitId`             | String | Applicable unit            |
| `FromDate`           | Date   | Effective from             |
| `ToDate`             | Date   | Effective through          |
| `QuantityAmountFrom` | Real   | Minimum quantity           |
| `QuantityAmountTo`   | Real   | Maximum quantity           |
| `DataAreaId`         | String | Company                    |

Business rule: Select the applicable trade agreement according to customer, item, unit, currency, quantity, dates and the standard price-search hierarchy.

For example:

```
Item: ITEM-001

Base sales price:       100 SAR
Customer agreement:     90 SAR
Ordered quantity:       10 PCS

Applicable unit price:  90 SAR
Line amount:           900 SAR
```

The system must preserve manually entered prices when standard price-override rules require it.

## 9.2 `MarkupTable` / `MarkupTrans` — Sales charges

| Table         | Core fields                                                                       | Function                      |
| ------------- | --------------------------------------------------------------------------------- | ----------------------------- |
| `MarkupTable` | `MarkupCode`, `Txt`, `ModuleType`                                                 | Charge setup                  |
| `MarkupTrans` | `MarkupCode`, `TransTableId`, `TransRecId`, `Value`, `CurrencyCode`, `ModuleType` | Charge assigned to order/line |

`MarkupTrans` can reference different transaction records. Validate its module and source identifiers rather than joining on a single sales-specific foreign key.

## 9.3 Sales tax

| Table             | Core fields / concepts                                          | Function               |
| ----------------- | --------------------------------------------------------------- | ---------------------- |
| `TaxGroupHeading` | `TaxGroup`, description                                         | Sales tax group header |
| `TaxGroupData`    | `TaxGroup`, `TaxCode`                                           | Tax code membership    |
| `TaxItemGroup`    | `TaxItemGroup`, description                                     | Item tax group         |
| `TaxOnItem`       | `TaxItemGroup`, `TaxCode`                                       | Item tax applicability |
| `TaxTable`        | `TaxCode`                                                       | Tax code setup         |
| `TaxData`         | Tax code, effective rates and limits                            | Tax rate configuration |
| `TaxTrans`        | `Voucher`, `TransDate`, `TaxCode`, `TaxBaseAmount`, `TaxAmount` | Posted tax transaction |

A normal tax code is applicable when the customer-side sales tax group and item-side sales tax group identify a matching valid tax code, subject to tax setup.

Example:

```
Line amount: 900 SAR
Tax group: VAT15
Item tax group: VAT15

Tax rate: 15%
Calculated VAT: 135 SAR
Gross line amount: 1,035 SAR
```

Do not hardcode VAT 15% into the sales-line calculation. Use configurable tax data and the applicable date, tax basis and rounding settings.

# 10. Customer payment and accounting tables

## 10.1 `CustTrans`

| Field          | Type   | Purpose                       |
| -------------- | ------ | ----------------------------- |
| `RecId`        | Int64  | Customer transaction identity |
| `AccountNum`   | String | Customer                      |
| `Voucher`      | String | Voucher                       |
| `Invoice`      | String | Invoice reference             |
| `TransDate`    | Date   | Transaction date              |
| `AmountCur`    | Real   | Transaction currency amount   |
| `CurrencyCode` | String | Currency                      |
| `AmountMST`    | Real   | Accounting currency amount    |
| `DueDate`      | Date   | Due date                      |
| `Closed`       | Date   | Closed date, when applicable  |
| `DataAreaId`   | String | Company                       |

## 10.2 `CustTransOpen`

| Field        | Type   | Purpose                 |
| ------------ | ------ | ----------------------- |
| `RecId`      | Int64  | Open transaction record |
| `RefRecId`   | Int64  | `CustTrans` reference   |
| `AmountCur`  | Real   | Open amount             |
| `DataAreaId` | String | Company                 |

## 10.3 `CustSettlement`

| Field               | Type   | Purpose             |
| ------------------- | ------ | ------------------- |
| `RecId`             | Int64  | Settlement identity |
| `TransRecId`        | Int64  | Settled transaction |
| `OffsetRecid`       | Int64  | Offset transaction  |
| `SettleAmountCur`   | Real   | Settlement amount   |
| `SettlementVoucher` | String | Settlement voucher  |
| `TransDate`         | Date   | Settlement date     |
| `DataAreaId`        | String | Company             |

## 10.4 Payment and ledger tables

| Table                        | Core fields                                                                                                            |
| ---------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| `LedgerJournalTable`         | `JournalNum`, `JournalName`, `JournalType`, `Posted`, `DataAreaId`                                                     |
| `LedgerJournalTrans`         | `JournalNum`, `Voucher`, `TransDate`, `AccountType`, `AccountNum`, `AmountCurDebit`, `AmountCurCredit`, `CurrencyCode` |
| `GeneralJournalEntry`        | `RecId`, `AccountingDate`, `SubledgerVoucher`, `Ledger`                                                                |
| `GeneralJournalAccountEntry` | `GeneralJournalEntry`, `LedgerDimension`, `AccountingCurrencyAmount`, `TransactionCurrencyAmount`                      |
| `InventPosting`              | Inventory posting relationship and account configuration                                                               |
| `CustPostingProfile`         | Customer posting profile configuration                                                                                 |

Payment and settlement are distinct posting operations. Settlement reduces the invoice's open receivable but does not change the invoiced sales quantity.

# 11. Return order tables

For the basic implementation, support physical return and credit-only return.

| Table                           | Core fields / concepts                                                | Responsibility                   |
| ------------------------------- | --------------------------------------------------------------------- | -------------------------------- |
| `ReturnTable`                   | Return identifier, customer, sales reference, status                  | Return authorization/header      |
| `ReturnLine`                    | Return identifier, item/line references, return quantity, disposition | Return details                   |
| `SalesTable`                    | `SalesId`, `SalesType`, `CustAccount`                                 | Underlying returned-order record |
| `SalesLine`                     | `ItemId`, `SalesQty`, `InventTransId`, `InventDimId`                  | Return order line                |
| `InventTrans`                   | `Qty`, `StatusReceipt`, `DatePhysical`, `DateFinancial`               | Inventory receipt                |
| `CustPackingSlipJour` / `Trans` | Receipt document references and quantities                            | Physical return receipt          |
| `CustInvoiceJour` / `Trans`     | Credit note values                                                    | Customer credit                  |

For `ReturnTable` and `ReturnLine`, treat the field descriptions as functional mapping requirements until the exact AOT fields are inspected. Return dispositions and return-to-sales relationships must not be reduced to a custom negative-quantity sales transaction.

# 12. Standard posting framework

These are important even in a basic ERP because they explain how D365 separates document preparation from final posting.

| Table             | Core fields / concepts                          | Responsibility            |
| ----------------- | ----------------------------------------------- | ------------------------- |
| `SalesParmUpdate` | `ParmId`, posting-related settings              | Posting operation         |
| `SalesParmTable`  | `ParmId`, `TableRefId`, `SalesId`               | Header posting parameters |
| `SalesParmLine`   | `ParmId`, `TableRefId`, `LineNum`, `DeliverNow` | Line/quantity selection   |

The standard framework prepares the relevant header and lines before confirmation, packing slip or invoice posting.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://community.dynamics.com\&sz=32)

community.dynamics.com



Do not treat `SalesParmTable` or `SalesParmLine` as the final accounting or delivery history. The posted journal tables are the historical records.

# 13. Detailed business validation matrix

| Event                   | Required validation                   | Expected outcome                      |
| ----------------------- | ------------------------------------- | ------------------------------------- |
| Create header           | Customer, company, number sequence    | `SalesTable` inserted                 |
| Add line                | Released item and valid unit          | `SalesLine` and inventory requirement |
| Modify quantity         | Posted/delivered/reserved constraints | Inventory requirement adjusted        |
| Modify unit             | Conversion exists                     | Inventory quantity recalculated       |
| Modify price            | Editing allowed                       | Line amount recalculated              |
| Modify dimension        | Dimensions valid and permitted        | Correct inventory association         |
| Reserve                 | Sufficient eligible stock             | `InventTrans` reservation state       |
| Confirm                 | Order can be confirmed                | Confirmation journal                  |
| Pick                    | Eligible quantity                     | Picking records                       |
| Post packing slip       | Eligible delivery quantity            | Physical issue journal                |
| Post invoice            | Eligible invoice quantity             | Invoice, tax, ledger, AR              |
| Receive payment         | Valid journal and account             | Payment transaction                   |
| Settle payment          | Valid open transactions               | Open balance reduced                  |
| Cancel remainder        | Only cancellable quantity             | Remaining demand canceled             |
| Return goods            | Valid return/disposition              | Physical receipt and/or credit        |
| Post duplicate document | Posting identity already used         | Duplicate prevented                   |
| Posting error           | Any validation/ledger failure         | Atomic rollback                       |

# 14. Database state verification

Example: `SO-000125`, quantity 10.

| Stage          | SalesQty | Remain physical | Remain financial | Delivery journal | Invoice journal  |
| -------------- | -------- | --------------- | ---------------- | ---------------- | ---------------- |
| Created        | 10       | 10              | 10               | None             | None             |
| Reserved       | 10       | 10              | 10               | None             | None             |
| Confirmed      | 10       | 10              | 10               | None             | None             |
| Packing slip 6 | 10       | 4               | 10               | PS-001           | None             |
| Invoice 4      | 10       | 4               | 6                | PS-001           | INV-001          |
| Packing slip 4 | 10       | 0               | 6                | PS-001, PS-002   | INV-001          |
| Invoice 6      | 10       | 0               | 0                | PS-001, PS-002   | INV-001, INV-002 |

The table is a valid business reconciliation for a simple standard stocked-item scenario. Exact posted journal IDs and inventory transaction splits will be determined by your D365 setup.

# 15. Extract the actual fields and SQL types from D365

Because you requested D365 alignment, the safest way to produce a complete and version-correct physical field dictionary is to inspect the target application's schema.

This read-only SQL Server query returns actual physical columns, types, nullability and identity information for your selected core tables.

```

SELECT
    t.name AS TableName,
    c.column_id AS ColumnOrder,
    c.name AS FieldName,
    ty.name AS SqlDataType,
    c.max_length AS MaxLengthBytes,
    c.precision AS NumericPrecision,
    c.scale AS NumericScale,
    c.is_nullable AS IsNullable,
    c.is_identity AS IsIdentity
FROM sys.tables t
INNER JOIN sys.columns c
    ON c.object_id = t.object_id
INNER JOIN sys.types ty
    ON ty.user_type_id = c.user_type_id
WHERE t.name IN
(
    'SALESTABLE',
    'SALESLINE',
    'CUSTTABLE',
    'INVENTTABLE',
    'INVENTTABLEMODULE',
    'INVENTDIM',
    'INVENTTRANS',
    'INVENTTRANSORIGIN',
    'INVENTSUM',
    'CUSTCONFIRMJOUR',
    'CUSTCONFIRMTRANS',
    'CUSTPACKINGSLIPJOUR',
    'CUSTPACKINGSLIPTRANS',
    'CUSTINVOICEJOUR',
    'CUSTINVOICETRANS',
    'CUSTTRANS',
    'CUSTTRANSOPEN',
    'CUSTSETTLEMENT',
    'PRICEDISCTABLE',
    'MARKUPTRANS',
    'TAXTRANS',
    'RETURNTable',
    'RETURNLINE',
    'SALESPARMTABLE',
    'SALESPARMLINE'
)
ORDER BY
    t.name,
    c.column_id;

```

The query returns the database's physical column definitions, not full AOT metadata such as EDTs, enums, field labels, relations, `modifiedField()` logic or form-level mandatory settings. Those must be inspected in Visual Studio/AOT, and fields can differ by application version.

For reference, Microsoft also publishes the [SalesLine Common Data Model field definitions](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/salesandmarketing/worksheetline/salesline?utm_source=chatgpt.com).

## 16. Implementation priority

For your basic ERP, the recommended priority is:

Priority 1 — Essential transaction engine

Core

`SalesTable`, `SalesLine`, `InventTable`, `InventDim`, `InventTransOrigin`, `InventTrans`, `InventSum`

Priority 2 — Standard document posting

`CustConfirmJour`, `CustConfirmTrans`, `CustPackingSlipJour`, `CustPackingSlipTrans`, `CustInvoiceJour`, `CustInvoiceTrans`

Priority 3 — Prices, tax and accounting

`PriceDiscTable`, `MarkupTrans`, tax tables, `CustTrans`, `CustTransOpen`, `CustSettlement`, general ledger tables

Priority 4 — Supporting workflows

Sales posting parameters, basic picking, returns and standard configuration tables

Implementation principle: Reuse the standard D365 F&O table names and business relationships. Do not create additional custom entities simply to manage order stages, quantities, statuses or totals when those concepts are already represented in the core sales and inventory data model.

This specification covers the primary fields needed for the basic cycle. It is not yet a certified exhaustive AOT dictionary: exact metadata, version-specific fields and database constraints must be verified against the D365 environment you are aligning to.