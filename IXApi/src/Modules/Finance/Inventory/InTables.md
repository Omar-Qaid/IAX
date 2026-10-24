# D365 F&O — Field-by-Field Implementation Dictionary

Specification: Core Inventory ERP, Phase 1\
Alignment: Microsoft Dynamics 365 Finance & Operations\
Scope: Product setup through inventory receipt, issue, transfer, adjustment, costing, and reconciliation.

The field dictionary should be the engineering contract between the database, application forms, business logic, and inventory posting engine.

## 1. Required dictionary structure

Every field will be documented against these implementation properties:

| Property               | Description                                                 |
| ---------------------- | ----------------------------------------------------------- |
| Module / Table / Field | Exact D365 object and field identifier                      |
| Data type              | String, enum, decimal, date, Int64, reference, etc.         |
| Key / relationship     | Primary key, foreign key or business key                    |
| Required               | Always, conditional, or system-generated                    |
| Default / source       | User, master data, number sequence, posting engine          |
| Business purpose       | Why the field exists                                        |
| Validation             | What must be checked before saving or posting               |
| Form / ownership       | Where the field is edited or displayed                      |
| Lifecycle              | Create, confirm, receive, reserve, post, close              |
| Example                | ITEM-001, WH-01, quantity, cost, etc.                       |
| Verification           | Confirmed D365 concept versus AOT field verification needed |

Data types in the functional dictionary are logical types. Exact SQL length, enum backing values, and extended data types (EDTs) must come from the target D365 metadata.

## 2. Field ownership rules

These rules apply throughout the implementation.

| Field category         | Create or update by                  | Main restriction                       |
| ---------------------- | ------------------------------------ | -------------------------------------- |
| Product master         | Authorized item-maintenance user     | Valid product configuration            |
| Released-item settings | Item master maintenance              | Company-specific setup                 |
| Site/warehouse         | Inventory administrator              | Valid storage structure                |
| Purchase/sales line    | Document workflow                    | Respect document status                |
| `InventDimId`          | Standard dimension resolution        | Do not manually invent dimension IDs   |
| `InventTransOrigin`    | Standard source transaction logic    | Maintain source relationship           |
| `InventTrans`          | Inventory transaction/posting engine | Do not independently edit posted stock |
| `InventSum`            | Standard on-hand update logic        | No direct manual adjustment            |
| Financial vouchers     | Posting framework                    | No manual voucher fabrication          |
| Cost settlement        | Inventory costing engine             | Respect the costing model              |

For example, `InventTrans.DatePhysical` belongs to physical inventory posting, while `InventTrans.DateFinancial` belongs to financial update. A user should not be able to change either field directly from a normal inventory transaction inquiry.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



## 3. Implementation dictionary workbook

D365 F&O Core Inventory — Field Implementation Dictionary

Excel workbook · 58 tables · 447 field entries · 4 worksheets

Download the Excel field dictionary

The workbook includes:

1. START HERE — scope, conventions, references, and implementation assumptions.
2. Field Dictionary — 447 field entries covering data types, ownership, required behavior, relationships, validation, sample values, and AOT verification status.
3. Table Inventory — 58 tables grouped by module and implementation sequence.
4. Posting Rules — 15 business operations showing inventory and financial effects.

## 4. Example: Field-level specification

The following illustrates how the dictionary should drive your actual implementation.

### `InventTrans` — Core inventory transaction

| Field                  | Owner              | Required behavior                             |
| ---------------------- | ------------------ | --------------------------------------------- |
| `RecId`                | System             | Unique generated record                       |
| `ItemId`               | Source document    | Must identify a valid released item           |
| `InventTransOrigin`    | Transaction engine | Must reference an existing origin             |
| `InventDimId`          | Dimension service  | Must resolve to valid inventory dimensions    |
| `Qty`                  | Transaction engine | Signed inventory quantity                     |
| `StatusReceipt`        | Posting engine     | Valid receipt status                          |
| `StatusIssue`          | Posting engine     | Valid issue status                            |
| `DatePhysical`         | Physical posting   | Updated on physical receipt/issue             |
| `DateFinancial`        | Financial posting  | Updated on financial receipt/issue            |
| `CostAmountPhysical`   | Costing/posting    | Physical inventory cost                       |
| `CostAmountPosted`     | Financial posting  | Financially posted cost                       |
| `CostAmountAdjustment` | Costing engine     | Cost adjustment from settlement/recalculation |

### Example: Purchase 50 PCS

Purchase order created

Ordered

`PurchLine.PurchQty = 50` (assuming PCS is the purchase unit)

`InventTrans.Qty = +50`

`InventTrans.StatusReceipt = Ordered`

Product receipt posted

Received

Receipt status becomes `Received`, `DatePhysical` is populated, and physical on-hand increases by 50.

Vendor invoice posted

Purchased

Receipt status becomes `Purchased`, `DateFinancial` is populated, and inventory cost is financially updated.

This is the level of behavior that should be implemented and tested for every business-critical field.

## 5. Verification and implementation status

The workbook is a functional implementation baseline, not a certified extraction of every standard Microsoft table column.

Before writing your database schema or application logic, verify the exact field names, EDTs, enums, relationships, and indexes against the D365 F&O version you are aligning with. Rows involving potentially version-dependent fields are explicitly flagged for AOT verification.

Also, do not treat calculated form values as stored database columns unless the standard D365 schema confirms them.

Recommended next engineering deliverable: turn this field dictionary into a form-by-form implementation specification, mapping each field to its D365 form control, lookup, defaulting rule, editable state, validation event, related tables, and test case. That will provide a direct implementation contract for building and testing the complete basic inventory module.

# D365 F&O — Basic Inventory ERP: Tables and Fields

You need the standard D365 Finance & Operations database structure for building a basic inventory ERP, including the fields of each table.

Scope:

- Product master and released products
- Item groups, model groups, units, sites, and warehouses
- Inventory dimensions, on-hand, and inventory transactions
- Inventory journals and reservations
- Purchase orders, receipts, and vendor invoices
- Sales orders, packing slips, and customer invoices
- Inventory transfers, counting, and returns
- Inventory posting, costing, and closing

Excluded: WHS, production, manufacturing, BOM, MRP, and advanced warehouse features.

Important: There are two different requirements:

1. All physical fields: Every standard column defined in a particular D365 F&O version, including technical fields.
2. Basic ERP fields: Only the standard columns required to implement the inventory lifecycle.

For building your ERP, the second is the correct functional scope. However, you should still extract the actual physical fields to avoid inventing columns or using incorrect names.

## 1. Core tables by module

| Module                 | Standard tables                                                                                                                                              |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Products               | `EcoResProduct`, `EcoResProductTranslation`, `InventTable`, `InventTableModule`                                                                              |
| Item groups            | `InventItemGroup`, `InventItemGroupItem`                                                                                                                     |
| Costing setup          | `InventModelGroup`, `InventModelGroupItem`                                                                                                                   |
| Units                  | `UnitOfMeasure`, `UnitOfMeasureConversion`                                                                                                                   |
| Inventory dimensions   | `InventDim`, `EcoResStorageDimensionGroup`, `EcoResTrackingDimensionGroup`                                                                                   |
| Site and warehouse     | `InventSite`, `InventLocation`, `InventItemLocation`                                                                                                         |
| Item order settings    | `InventItemInventSetup`, `InventItemPurchSetup`, `InventItemSalesSetup`                                                                                      |
| Inventory transactions | `InventTransOrigin`, `InventTrans`, `InventSum`, `InventTransPosting`                                                                                        |
| Journals               | `InventJournalName`, `InventJournalTable`, `InventJournalTrans`                                                                                              |
| Purchasing             | `PurchTable`, `PurchLine`, `VendPurchOrderJour`, `VendPurchOrderTrans`, `VendPackingSlipJour`, `VendPackingSlipTrans`, `VendInvoiceJour`, `VendInvoiceTrans` |
| Sales                  | `SalesTable`, `SalesLine`, `CustConfirmJour`, `CustConfirmTrans`, `CustPackingSlipJour`, `CustPackingSlipTrans`, `CustInvoiceJour`, `CustInvoiceTrans`       |
| Transfers              | `InventTransferTable`, `InventTransferLine`, `InventTransferJour`                                                                                            |
| Costing                | `InventPosting`, `InventSettlement`, `InventClosing`                                                                                                         |
| Financial posting      | `GeneralJournalEntry`, `GeneralJournalAccountEntry`                                                                                                          |
| Supporting masters     | `CustTable`, `VendTable`, `CustTrans`, `VendTrans`                                                                                                           |

Additional standard tables may be required for returns, posting parameters, tracking dimensions, and document-to-inventory origin relationships.

## 2. Core fields for each table

The following are the fields to prioritize in your basic ERP schema. They are grouped by table, with no WHS or production-specific processing fields.

### Product and configuration

`{t.name}`}

RecId, DisplayProductNumber, ProductType, SearchName, InstanceRelationType

`{t.name}`}

RecId, Product, LanguageId, Name, Description

`{t.name}`}

RecId, ItemId, Product, NameAlias, PrimaryVendorId, ItemBuyerGroupId, NetWeight, UnitVolume, DataAreaId

`{t.name}`}

RecId, ItemId, ModuleType, UnitId, Price, PriceUnit, PriceDate, LineDisc, MultiLineDisc, EndDisc, DataAreaId

`{t.name}`}

RecId, ItemGroupId, Name, DataAreaId

`{t.name}`}

RecId, ItemId, ItemGroupId, DataAreaId

`{t.name}`}

RecId, ModelGroupId, Name, InventModel, NegativePhysical, NegativeFinancial, PostPhysicalInvent, PostFinancialInvent, DataAreaId

`{t.name}`}

RecId, ItemId, ModelGroupId, DataAreaId

`{t.name}`}

RecId, SiteId, Name, DataAreaId

`{t.name}`}

RecId, InventLocationId, InventSiteId, Name, DataAreaId

`{t.name}`}

RecId, Symbol, UnitOfMeasureClass, DecimalPrecision

`{t.name}`}

RecId, FromUnitOfMeasure, ToUnitOfMeasure, Factor, Numerator, Denominator, Product

`{t.name}`}

RecId, InventDimId, InventSiteId, InventLocationId, InventBatchId, InventSerialId, DataAreaId

### Inventory transactions and journals

`{t.name}`}

RecId, InventTransId, ItemId, ItemInventDimId, ReferenceCategory, ReferenceId, DataAreaId

`{t.name}`}

RecId, ItemId, InventTransOrigin, InventDimId, Qty, StatusReceipt, StatusIssue, DateExpected, DatePhysical, DateFinancial, VoucherPhysical, Voucher, CostAmountPhysical, CostAmountPosted, CostAmountAdjustment, ValueOpen, DateClosed, DataAreaId

`{t.name}`}

RecId, ItemId, InventDimId, PostedQty, Received, Deducted, Registered, Arrived, ReservPhysical, ReservOrdered, OnOrder, Ordered, Picked, DataAreaId

`{t.name}`}

JournalNameId, Name, JournalType, VoucherSeries, DataAreaId

`{t.name}`}

RecId, JournalId, JournalNameId, Description, Posted, DataAreaId

`{t.name}`}

RecId, JournalId, LineNum, TransDate, ItemId, InventDimId, ToInventDimId, Qty, CostPrice, CostAmount, InventTransId, Voucher, DataAreaId

### Purchase and sales cycle

`{t.name}`}

RecId, PurchId, OrderAccount, InvoiceAccount, PurchName, PurchStatus, DocumentStatus, CurrencyCode, DeliveryDate, DefaultDimension, DataAreaId

`{t.name}`}

RecId, PurchId, LineNumber, ItemId, InventTransId, InventDimId, PurchQty, PurchUnit, PurchPrice, PriceUnit, LineAmount, RemainPurchPhysical, RemainPurchFinancial, RemainInventPhysical, RemainInventFinancial, PurchStatus, DataAreaId

`{t.name}`}

PurchId, PurchOrderDocNum, ItemId (line), InventTransId (line), Qty (line), PurchPrice (line), DataAreaId

`{t.name}`}

PurchId, PackingSlipId, DeliveryDate (header), ItemId (line), Qty (line), InventDimId (line), InventTransId (line), DataAreaId

`{t.name}`}

InvoiceId, PurchId, InvoiceDate (header), InvoiceAccount (header), LedgerVoucher (header), ItemId (line), Qty (line), PurchPrice (line), LineAmount (line), DataAreaId

`{t.name}`}

RecId, SalesId, CustAccount, InvoiceAccount, SalesName, SalesStatus, DocumentStatus, CurrencyCode, DeliveryDate, DefaultDimension, DataAreaId

`{t.name}`}

RecId, SalesId, LineNum, ItemId, InventTransId, InventDimId, SalesQty, SalesUnit, SalesPrice, PriceUnit, LineAmount, RemainSalesPhysical, RemainSalesFinancial, RemainInventPhysical, RemainInventFinancial, SalesStatus, DataAreaId

`{t.name}`}

SalesId, ConfirmId, ItemId (line), InventTransId (line), Qty (line), SalesPrice (line), DataAreaId

`{t.name}`}

SalesId, PackingSlipId, DeliveryDate (header), ItemId (line), Qty (line), InventDimId (line), InventTransId (line), DataAreaId

`{t.name}`}

InvoiceId, SalesId, InvoiceDate (header), InvoiceAccount (header), LedgerVoucher (header), ItemId (line), Qty (line), SalesPrice (line), LineAmount (line), DataAreaId

### Transfers, costing and general ledger

`{t.name}`}

RecId, TransferId, InventLocationIdFrom, InventLocationIdTo, TransferStatus, ShipDate, ReceiveDate, DataAreaId

`{t.name}`}

RecId, TransferId, LineNum, ItemId, QtyTransfer, QtyShipped, QtyReceived, QtyRemainShip, QtyRemainReceive, InventDimId, DataAreaId

`{t.name}`}

RecId, TransferId, VoucherId, TransDate, UpdateType, DataAreaId

`{t.name}`}

InventAccountType, ItemCode, ItemRelation, AccountCode, AccountRelation, LedgerDimension, DataAreaId

`{t.name}`}

RecId and inventory transaction references, settlement quantity, settlement value, voucher and date

`{t.name}`}

RecId, AccountingDate, SubledgerVoucher, SubledgerVoucherDataAreaId, Ledger, JournalNumber

`{t.name}`}

RecId, GeneralJournalEntry, LedgerDimension, AccountingCurrencyAmount, TransactionCurrencyAmount, TransactionCurrencyCode, PostingType, Text

These are field checklists for functional implementation. They should not be treated as a fully verified, exact D365 schema export.

## 3. SQL to obtain every actual field of each table

Microsoft identifies Visual Studio Application Explorer as the authoritative location for application table metadata.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn

+1



When you have access to the D365 development database, run this read-only SQL to obtain the actual fields, SQL data types, nullability and primary-key membership.

```

DECLARE @Scope TABLE
(
    ModuleName varchar(50),
    TableName sysname
);

INSERT INTO @Scope VALUES
('Product','EcoResProduct'),
('Product','EcoResProductTranslation'),
('Product','InventTable'),
('Product','InventTableModule'),
('Product','InventItemGroup'),
('Product','InventItemGroupItem'),
('Product','InventModelGroup'),
('Product','InventModelGroupItem'),
('Setup','InventSite'),
('Setup','InventLocation'),
('Setup','InventDim'),
('Setup','EcoResStorageDimensionGroup'),
('Setup','EcoResTrackingDimensionGroup'),
('Setup','UnitOfMeasure'),
('Setup','UnitOfMeasureConversion'),
('Setup','InventItemLocation'),
('Setup','InventItemInventSetup'),
('Setup','InventItemPurchSetup'),
('Setup','InventItemSalesSetup'),
('Inventory','InventTransOrigin'),
('Inventory','InventTrans'),
('Inventory','InventSum'),
('Inventory','InventTransPosting'),
('Inventory','InventJournalName'),
('Inventory','InventJournalTable'),
('Inventory','InventJournalTrans'),
('Purchase','PurchTable'),
('Purchase','PurchLine'),
('Purchase','VendPurchOrderJour'),
('Purchase','VendPurchOrderTrans'),
('Purchase','VendPackingSlipJour'),
('Purchase','VendPackingSlipTrans'),
('Purchase','VendInvoiceJour'),
('Purchase','VendInvoiceTrans'),
('Sales','SalesTable'),
('Sales','SalesLine'),
('Sales','CustConfirmJour'),
('Sales','CustConfirmTrans'),
('Sales','CustPackingSlipJour'),
('Sales','CustPackingSlipTrans'),
('Sales','CustInvoiceJour'),
('Sales','CustInvoiceTrans'),
('Transfer','InventTransferTable'),
('Transfer','InventTransferLine'),
('Transfer','InventTransferJour'),
('Transfer','InventTransferJourLine'),
('Costing','InventPosting'),
('Costing','InventSettlement'),
('Costing','InventClosing'),
('Finance','GeneralJournalEntry'),
('Finance','GeneralJournalAccountEntry'),
('Finance','CustTrans'),
('Finance','VendTrans');

SELECT
    s.ModuleName,
    s.TableName,
    c.column_id AS FieldOrder,
    c.name AS FieldName,
    ty.name AS SQLType,
    c.max_length AS MaxLengthBytes,
    c.precision AS [Precision],
    c.scale AS Scale,
    c.is_nullable AS IsNullable,
    CASE WHEN pk.column_id IS NOT NULL
         THEN 1 ELSE 0 END AS IsPrimaryKey,
    CASE WHEN t.object_id IS NULL
         THEN 'TABLE NOT FOUND'
         ELSE 'FOUND' END AS TableStatus
FROM @Scope s
LEFT JOIN sys.tables t
    ON t.name = s.TableName
LEFT JOIN sys.columns c
    ON c.object_id = t.object_id
LEFT JOIN sys.types ty
    ON ty.user_type_id = c.user_type_id
LEFT JOIN
(
    SELECT ic.object_id, ic.column_id
    FROM sys.indexes i
    JOIN sys.index_columns ic
        ON ic.object_id = i.object_id
       AND ic.index_id = i.index_id
    WHERE i.is_primary_key = 1
) pk
    ON pk.object_id = c.object_id
   AND pk.column_id = c.column_id
ORDER BY
    s.ModuleName,
    s.TableName,
    c.column_id;

```

This produces the exact database column inventory from your particular environment and makes it possible to distinguish missing tables from incorrect field assumptions. It does not include all AOT-only properties or calculated display methods.

## 4. What your ERP must implement first

For a working basic inventory engine, build and test in this sequence:

1. Product and released-item master — `EcoResProduct`, `InventTable`, `InventTableModule`.
2. Inventory setup — item model group, item group, units, site, warehouse and dimensions.
3. Inventory transactions — `InventTransOrigin`, `InventTrans`, `InventDim`, `InventSum`.
4. Opening inventory and journals — `InventJournalTable`, `InventJournalTrans`.
5. Purchase receipt cycle — `PurchLine`, product receipt and vendor invoice tables.
6. Sales issue cycle — `SalesLine`, reservation, packing slip and customer invoice tables.
7. Transfers and inventory accounting — transfer order tables, posting profiles, settlements and ledger integration.

The key distinction: You can use the listed tables and functional fields as the basic ERP design scope. For an exact D365 F&O-aligned database implementation, use the SQL metadata output or AOT export as the definitive field list instead of creating unverified standard fields.