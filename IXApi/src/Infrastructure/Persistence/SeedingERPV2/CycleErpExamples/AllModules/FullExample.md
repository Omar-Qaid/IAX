# الدورة المحاسبية الأساسية في D365 F&O: المشتريات والمبيعات والمخزون والمالية

**النطاق:** Core ERP فقط: GL و AP و AR و Inventory، مع أصناف مخزنية وخدمية. بدون WHS وإنتاج والحالات المتقدمة.

---

## 0. المثال المتكامل

### 0.1 البيانات

| العنصر | القيمة |
|---|---|
| Legal entity | `kcg`، العملة SAR، VAT 15% |
| Site / Warehouses | `S1`؛ `W1` رئيسي، `W2` فرع، `TRN` Transit |
| Vendor | `V-2001`، VendGroup `LOCAL`، Net30، TaxGroup `KSA-DOM-P` |
| Customer | `C-1001`، CustGroup `DOM`، Net30، TaxGroup `KSA-DOM` |
| Stocked item | `STK-001`، FIFO، Stocked = Yes، TaxItemGroup `STD` |
| Purchase service | `SRV-P01` (صيانة)، Stocked = No |
| Sales service | `SRV-S01` (تركيب)، Stocked = No |

### 0.2 دليل الحسابات المستخدم

| Main account | الاسم | النوع |
|---|---|---|
| 110100 | Bank | Balance |
| 120100 | AR summary | Balance |
| 120500 | VAT receivable | Balance |
| 130100 | Inventory | Balance |
| 210100 | AP summary | Balance |
| 210500 | Purchase accrual (اختياري) | Balance |
| 220100 | VAT payable | Balance |
| 220900 | VAT settlement | Balance |
| 310100 | Capital | Equity |
| 410100 | Sales revenue – items | Revenue |
| 410200 | Service revenue | Revenue |
| 420900 | Inventory profit | Revenue |
| 510100 | COGS | Expense |
| 520100 | Maintenance expense | Expense |
| 520200 | Samples expense | Expense |
| 520300 | Inventory loss | Expense |
| 520400 | Rent expense | Expense |

### 0.3 التسلسل الزمني

| # | التاريخ | الحدث | المستند |
|---|---|---|---|
| 1 | 01-Nov | قيد افتتاحي: رأس المال 100,000 في البنك | GJ-0001 |
| 2 | 02-Nov | أمر شراء: 100×STK-001 @60 + 1×SRV-P01 @1,000 | PO-0001 |
| 3 | 03-Nov | استلام منتج | PR-0001 |
| 4 | 05-Nov | فاتورة مورد: 7,000 + 1,050 VAT = **8,050** | VI-0001 |
| 5 | 06–07-Nov | تحويل 30 وحدة من W1 إلى W2 | TO-0001 |
| 6 | 08-Nov | أمر بيع: 10×STK-001 @100 + SRV-S01 @500 من W1 | SO-0001 |
| 7 | 09-Nov | Packing slip | PS-0001 |
| 8 | 10-Nov | فاتورة عميل: 1,500 + 225 = **1,725** | INV-0001 |
| 9 | 12-Nov | Movement journal: صرف وحدتين من W2 كعينات | IMOV-0001 |
| 10 | 15-Nov | جرد W1: الرصيد في النظام 60، المعدود 58 | ICNT-0001 |
| 11 | 20-Nov | دفعة للمورد 8,050 | VPAY-0001 |
| 12 | 25-Nov | تحصيل من العميل 1,725 | CPAY-0001 |
| 13 | 26-Nov | قيد يومية: إيجار 3,000 + 450 VAT من البنك | GJ-0002 |
| 14 | 30-Nov | Inventory close، تسوية VAT، Reconciliation، Period close | — |

---

# الجزء 1: الإعدادات (Configuration) لكل موديول

## 1.1 General Ledger

| الإعداد | الجدول | أهم الحقول |
|---|---|---|
| Chart of accounts | `LedgerChartOfAccounts` | Name |
| Main accounts | `MainAccount` | MainAccountId, Name, **Type** (Balance/P&L/Revenue/Expense/Asset/Liability/Equity), MainAccountCategory, DebitCreditProposal |
| إعداد الحساب لكل شركة | `MainAccountLegalEntity` | DefaultDimension, Suspended |
| Financial dimensions | `DimensionAttribute`, `DimensionAttributeValue` | Name, Values |
| Account structure | `DimensionHierarchy`, `DimensionConstraintNode` | الأبعاد المسموحة لكل حساب |
| Fiscal calendar | `FiscalCalendar`, `FiscalCalendarYear`, `FiscalCalendarPeriod` | StartDate, EndDate, **Type** (Opening/Operating/Closing) |
| Ledger | `Ledger` | Name = DataArea, **AccountingCurrency**, ReportingCurrency, ChartOfAccounts, FiscalCalendar, DefaultExchangeRateType |
| حالة الفترة | `LedgerFiscalCalendarPeriod` | **Status** (Open / On hold / Permanently closed)، Module access |
| Currencies / Rates | `Currency`, `ExchangeRateType`, `ExchangeRateCurrencyPair`, `ExchangeRate` | — |
| System accounts | `LedgerSystemAccounts` | PostingType (Penny difference, Error account…) → LedgerDimension |
| GL parameters | `LedgerParameters` | Batch transfer rules، Voucher checks |
| Journal names | `LedgerJournalName` | JournalName, **JournalType** (Daily / Payment / CustPayment), VoucherSeries, OffsetAccountType, OffsetLedgerDimension |
| Bank accounts | `BankAccountTable` | AccountID, **LedgerDimension** (110100), CurrencyCode |
| Number sequences | `NumberSequenceTable`, `NumberSequenceReference` | Voucher series |
| Tax | `TaxTable`, `TaxData`, `TaxGroupHeading/Data`, `TaxItemGroupHeading`, `TaxOnItem`, `TaxLedgerAccountGroup` (Payable 220100، Receivable 120500، Settlement 220900)، `TaxPeriodHead`, `TaxAuthorityAddress` | — |

## 1.2 Accounts Payable

| الإعداد | الجدول | أهم الحقول |
|---|---|---|
| AP parameters | `VendParameters` | Default posting profile، **Post product receipt in ledger**، Invoice matching، Automatic settlement |
| Posting profile | `VendLedger`, `VendLedgerAccounts` | PostingProfile, AccountCode, Num, **SumLedgerDimension** (210100) |
| Vendor group | `VendGroup` | VendGroup, PaymTermId, TaxGroupId |
| Terms of payment | `PaymTerm` | PaymMethod, NumOfDays |
| Methods of payment | `VendPaymModeTable` | PaymMode, AccountType = Bank, PaymentLedgerDimension |
| Purchase order types / approval | `PurchParameters` | Change management (اختياري) |
| Procurement categories | `EcoResCategory` | لسطور الخدمة بدون صنف (اختياري) |
| Number sequences | — | PurchId, Product receipt, Vendor invoice voucher, Payment voucher |
| Journal names | `LedgerJournalName` | JournalType = **Payment** |

## 1.3 Accounts Receivable

| الإعداد | الجدول | أهم الحقول |
|---|---|---|
| AR parameters | `CustParameters` | Default posting profile، Post packing slip in ledger، **CreditMaxCheck** |
| Posting profile | `CustLedger`, `CustLedgerAccounts` | **SumLedgerDimension** (120100) |
| Customer group | `CustGroup` | PaymTermId, TaxGroupId |
| Methods of payment | `CustPaymModeTable` | PaymMode, PaymentLedgerDimension |
| Journal names | `LedgerJournalName` | JournalType = **CustPayment** |
| Number sequences | — | SalesId, Packing slip, Invoice, FTI, Voucher |

## 1.4 Inventory

| الإعداد | الجدول | أهم الحقول |
|---|---|---|
| Sites / Warehouses | `InventSite`, `InventLocation` | InventLocationId, InventSiteId, **InventLocationType** (Standard / Transit)، **InventLocationIdTransit** (W1 → TRN) |
| Inventory parameters | `InventParameters` | Closing settings, Physical negative |
| Item model group | `InventModelGroup` | **InventModel** (FIFO)، **StockedProduct**، PostOnhandPhysical، PostOnhandFinancial، NegativePhysical/Financial |
| Item group | `InventItemGroup` | ItemGroupId |
| Storage dimension group | `EcoResStorageDimensionGroup` | Site + Warehouse active، Financial inventory |
| Tracking dimension group | `EcoResTrackingDimensionGroup` | Batch/Serial (None هنا) |
| Inventory journal names | `InventJournalName` | JournalNameId, **JournalType** (Movement / LossProfit / Transfer / Count), VoucherSeries |
| Counting groups | `InventCountGroup` | اختياري |
| **Inventory posting** | `InventPosting` | **InventAccountType** + ItemCode/ItemRelation + CustVendCode + **LedgerDimension** |

**ربط Inventory posting في المثال:**

| Posting (InventAccountType) | الحساب |
|---|---|
| Purchase – Inventory receipt (PurchReceipt) | 130100 |
| Purchase – Expenditure for expense (PurchConsump), للخدمة غير المخزنية | 520100 |
| Purchase – Expenditure un-invoiced / Accrual (اختياري) | 130100 / 210500 |
| Sales – Inventory issue (SalesIssue) | 130100 |
| Sales – Consumption / COGS (SalesConsump) | 510100 |
| Sales – Revenue (SalesRevenue)، Item group STK | 410100 |
| Sales – Revenue، Item SRV-S01 | 410200 |
| Inventory – Issue / Receipt (InventIssue / InventReceipt) | 130100 |
| Inventory – Loss (InventLoss) | 520300 |
| Inventory – Profit (InventProfit) | 420900 |

---

# الجزء 2: البيانات الرئيسية (Master Data)

## 2.1 GL

| البيان | الجداول |
|---|---|
| Main accounts (القائمة أعلاه) | `MainAccount`, `MainAccountLegalEntity` |
| Dimension values (Department / CostCenter) | `DimensionAttributeValue`، وتُستخدم كـ DefaultDimension |
| Bank account | `BankAccountTable` |
| Opening balances | `LedgerJournalTrans` (Daily) |

## 2.2 Vendor

| الجدول | الحقول |
|---|---|
| `DirPartyTable` / `DirOrganization` | PartyNumber, Name |
| `VendTable` | AccountNum, Party, **VendGroup**, Currency, **PaymTermId**, PaymMode, **TaxGroup**, InclTax, **Blocked**, InvoiceAccount, DefaultDimension |
| `LogisticsPostalAddress`, `DirPartyLocation` | العناوين |
| `TaxRegistration` | VAT number |
| `VendBankAccount` | حساب بنك المورد |

## 2.3 Customer

| الجدول | الحقول |
|---|---|
| `DirPartyTable` | — |
| `CustTable` | AccountNum, Party, CustGroup, Currency, PaymTermId, PaymMode, TaxGroup, InclTax, **CreditMax**, **Blocked**, DefaultDimension |
| `LogisticsPostalAddress`, `TaxRegistration` | — |

## 2.4 Products / Items

| الجدول | STK-001 | SRV-P01 / SRV-S01 |
|---|---|---|
| `EcoResProduct` | ProductType = Item | ProductType = Service |
| `InventTable` | ItemId, ItemType = Item | ItemType = Service |
| `InventModelGroupItem` | FIFO (Stocked) | NONSTK |
| `InventItemGroupItem` | STK | SRV |
| `EcoResStorageDimensionGroupItem` | SiteWH | — |
| `InventTableModule` (Invent/Purch/Sales) | UnitId, **Price**, **TaxItemGroupId** | نفس الحقول |
| `InventItemPurchSetup` / `InventItemSalesSetup` / `InventItemInventSetup` | Default Site/WH, Stopped | — |
| `PriceDiscTable` | Trade agreements (اختياري) | — |

---

# الجزء 3: دورة المشتريات (Procure-to-Pay)

## P1: إنشاء أمر الشراء PO-0001 (02-Nov)

| الجدول | الحقول المتأثرة |
|---|---|
| `PurchTable` | PurchId, OrderAccount, InvoiceAccount, **PurchStatus = Backorder (1)**, **DocumentState = Draft**, CurrencyCode, Payment, TaxGroup, InclTax, InventSiteId, InventLocationId = W1, DeliveryDate, DefaultDimension, SourceDocumentHeader |
| `PurchLine` | LineNumber, ItemId, PurchQty, QtyOrdered, PurchPrice, LineAmount, TaxGroup, TaxItemGroup, InventDimId, **InventTransId**, **RemainPurchPhysical = 100**, RemainPurchFinancial = 0, PurchStatus, SourceDocumentLine |
| `SourceDocumentHeader` / `SourceDocumentLine` | Source document framework |
| `AccountingDistribution` | التوزيع المحاسبي المتوقع لكل سطر |
| `TaxUncommitted` | الضريبة قبل الترحيل (1,050) |
| `InventTransOrigin` | InventTransId, ReferenceCategory = Purch (3), ReferenceId = PO-0001 |
| `InventTrans` (STK) | Qty = +100, **StatusReceipt = Ordered (5)** |
| `InventSum` | **Ordered +100**، AvailOrdered +100 |

- **الأثر المادي:** لا يوجد. كمية متوقعة فقط.
- **الأثر المالي:** لا يوجد.
- **SRV-P01:** سطر PO فقط، بدون `InventSum`.

## P2: الموافقة والتأكيد (Confirm)

| الجدول | الحقول |
|---|---|
| `PurchTable` | **DocumentState = Approved ثم Confirmed** |
| `VendPurchOrderJour` | PurchOrderDocNum, PurchId, PurchaseOrderDate, Amount |
| `VendPurchOrderTrans` | سطور التأكيد |
| `PurchTableHistory` / `PurchLineHistory` | نسخة الـ versioning |

لا يوجد أثر مادي ولا مالي.

## P3: استلام المنتج PR-0001 (03-Nov): الأثر المادي

| الجدول | الحقول |
|---|---|
| `VendPackingSlipJour` | **PackingSlipId = PR-0001**, PurchId, DeliveryDate, DocumentDate, LedgerVoucher (إذا كان الترحيل للـ ledger مفعّلاً) |
| `VendPackingSlipTrans` | ItemId, Qty, Remain, InventTransId, ValueMST, SourceDocumentLine |
| `PurchLine` | **RemainPurchPhysical 100→0**، **RemainPurchFinancial 0→100**، **PurchStatus = Received (2)** |
| `PurchTable` | PurchStatus = Received |
| `InventTrans` | **StatusReceipt = Received (2)**، DatePhysical، PackingSlipId، **CostAmountPhysical = +6,000** |
| `InventSum` (W1) | Ordered -100، **Received +100**، **PhysicalInvent +100**، AvailPhysical +100، PhysicalValue +6,000 |

- **الأثر المادي:** المخزون في W1 = 100 ومتاح للبيع.
- **الأثر المالي:** لا يوجد افتراضياً. إذا كان `VendParameters` → Post product receipt in ledger = Yes و `PostOnhandPhysical` = Yes، يُنشأ قيد Accrual التالي، ويُعكس تلقائياً عند الفاتورة. الحركة تظهر في `InventTransPosting` (InventTransPostingType = Physical).

| Dr | Cr | المبلغ |
|---|---|---|
| Purchase expenditure un-invoiced | Purchase accrual 210500 | 6,000 (و 1,000 للخدمة) |

## P4: فاتورة المورد VI-0001 (05-Nov): الأثر المالي

**Pending (قبل الترحيل):**

| الجدول | الحقول |
|---|---|
| `VendInvoiceInfoTable` | **Num** (رقم فاتورة المورد), PurchId, InvoiceAccount, DocumentDate, TransDate, ParmId, ApprovalStatus |
| `VendInvoiceInfoSubTable` | ربط الفاتورة بالـ PO |
| `VendInvoiceInfoLine` | ItemId, **ReceiveNow**, PurchPrice, LineAmount, TaxGroup, TaxItemGroup |
| `VendInvoicePackingSlipQuantityMatch` | المطابقة مع PR-0001 |
| `TaxUncommitted` | 1,050 |

**Posted (بعد الترحيل):**

| الجدول | الحقول |
|---|---|
| `VendInvoiceJour` | InvoiceId, PurchId, InvoiceDate, **LedgerVoucher**, **InvoiceAmount = 8,050**, **SumTax = 1,050**, DueDate = 05-Dec, PostingProfile |
| `VendInvoiceTrans` | ItemId, Qty, PurchPrice, LineAmount, TaxAmount, InventTransId |
| `VendInvoicePurchLink` | PurchId ↔ InvoiceId |
| `PurchLine` | RemainPurchFinancial = 0، **PurchStatus = Invoiced (3)** |
| `InventTrans` (STK) | **StatusReceipt = Purchased (1)**، DateFinancial، InvoiceId، Voucher، **CostAmountPosted = +6,000** |
| `InventSum` (W1) | Received -100، **PostedQty +100**، **PostedValue +6,000**، PhysicalValue -6,000 |
| `TaxTrans` | سطر 900 وسطر 150، **TaxDirection = IncomingTax**، المبالغ موجبة |
| `VendTrans` | AccountNum, Voucher, Invoice, **TransType = Purch**, **AmountCur = -8,050**, DueDate, PostingProfile, Closed = empty |
| `VendTransOpen` | RefRecId → VendTrans، AmountCur = -8,050، DueDate |
| `GeneralJournalEntry` + `GeneralJournalAccountEntry` | القيد أدناه |
| `SubledgerVoucherGeneralJournalEntry` | Voucher ↔ GJE |
| `InventTransPosting` | Financial posting للصنف |

**قيد اليومية:**

| الحساب | Dr | Cr |
|---|---|---|
| 130100 Inventory (Purchase receipt) | 6,000 | |
| 520100 Maintenance expense (SRV-P01) | 1,000 | |
| 120500 VAT receivable | 1,050 | |
| 210100 AP summary (VendBalance) | | 8,050 |

## P5: دفعة المورد VPAY-0001 (20-Nov)

| الجدول | الحقول |
|---|---|
| `LedgerJournalTable` | JournalNum, **JournalType = Payment**, Posted |
| `LedgerJournalTrans` | Voucher, TransDate, **AccountType = Vend**, LedgerDimension (V-2001), **AmountCurDebit = 8,050**, OffsetAccountType = Bank, OffsetLedgerDimension, PaymMode, PaymReference, MarkedInvoice |
| `SpecTrans` | ربط الدفعة بـ `VendTransOpen` (Balance01 = 8,050) |

**بعد الترحيل:**

| الجدول | الأثر |
|---|---|
| `VendTrans` (Payment) | **TransType = Payment**، AmountCur = +8,050، Closed |
| `VendTrans` (VI-0001) | **SettleAmountCur = -8,050**، **Closed = 20-Nov** |
| `VendSettlement` | TransRecId, OffsetRecid, SettleAmountCur, SettleAmountMST |
| `VendTransOpen` | يُحذف السجلان |
| `BankAccountTrans` | AmountCur = -8,050، Reconciled = No |

**القيد:** Dr 210100 AP 8,050 / Cr 110100 Bank 8,050.

## ملخص دورة المشتريات

| المرحلة | InventTrans | InventSum | Vendor subledger | GL |
|---|---|---|---|---|
| PO | Ordered (5) | Ordered +100 | — | — |
| Confirm | Ordered | — | — | — |
| Product receipt | **Received (2)** | Received +100، Physical +100 | — | اختياري (Accrual) |
| Invoice | **Purchased (1)** | PostedQty +100، PostedValue +6,000 | VendTrans -8,050 | Inv / Exp / VAT / AP |
| Payment | — | — | Settlement، Closed | AP / Bank |

---

# الجزء 4: دورة المبيعات (Order-to-Cash)

## S1: أمر البيع SO-0001 (08-Nov)

| الجدول | الحقول |
|---|---|
| `SalesTable` | SalesId, CustAccount, **SalesStatus = Backorder (1)**, DocumentStatus = None, Payment, TaxGroup, InventLocationId = W1 |
| `SalesLine` | ItemId, SalesQty, SalesPrice, LineAmount, TaxGroup, TaxItemGroup, **InventTransId**, **RemainSalesPhysical = 10** |
| `InventTransOrigin` | ReferenceCategory = Sales (0) |
| `InventTrans` | Qty = -10، **StatusIssue = OnOrder (6)** |
| `InventSum` | **OnOrder +10** |

**التحققات:** Blocked، Credit limit. لا يوجد أثر مالي.

## S2: التأكيد والحجز

| الخطوة | الجداول | الأثر |
|---|---|---|
| Confirm | `CustConfirmJour` / `CustConfirmTrans` | `SalesTable.DocumentStatus = Confirmation` |
| Reserve | `InventTrans` StatusIssue = **ReservPhysical (4)** | `InventSum.ReservPhysical +10`، AvailPhysical 70→60 |

## S3: Packing slip PS-0001 (09-Nov): الأثر المادي

| الجدول | الحقول |
|---|---|
| `CustPackingSlipJour` / `CustPackingSlipTrans` | PackingSlipId, Qty, InventTransId |
| `SalesLine` | **RemainSalesPhysical 10→0**، **RemainSalesFinancial 0→10**، **SalesStatus = Delivered (2)** |
| `InventTrans` | **StatusIssue = Deducted (2)**، DatePhysical، **CostAmountPhysical = -600** |
| `InventSum` (W1) | ReservPhysical -10، **Deducted +10**، **PhysicalInvent 70→60** |

**الأثر المالي:** لا يوجد افتراضياً. يصبح اختيارياً مع Post packing slip in ledger.

## S4: فاتورة العميل INV-0001 (10-Nov): الأثر المالي

| الجدول | الحقول |
|---|---|
| `CustInvoiceJour` | InvoiceId, **LedgerVoucher**, SalesBalance = 1,500, **SumTax = 225**, **InvoiceAmount = 1,725**, DueDate |
| `CustInvoiceTrans` | ItemId, Qty, LineAmount, TaxAmount, InventTransId |
| `SalesLine` / `SalesTable` | **SalesStatus = Invoiced (3)** |
| `InventTrans` | **StatusIssue = Sold (1)**، DateFinancial، **CostAmountPosted = -600** |
| `InventSum` (W1) | Deducted -10، **PostedQty -10**، **PostedValue -600** |
| `TaxTrans` | -150 و -75، **OutgoingTax** |
| `CustTrans` | **TransType = Sales**، **AmountCur = +1,725**، DueDate |
| `CustTransOpen` | 1,725 |

**قيد اليومية:**

| الحساب | Dr | Cr |
|---|---|---|
| 120100 AR (CustBalance) | 1,725 | |
| 410100 Sales revenue items | | 1,000 |
| 410200 Service revenue | | 500 |
| 220100 VAT payable | | 225 |
| 510100 COGS | 600 | |
| 130100 Inventory (Sales issue) | | 600 |

## S5: تحصيل CPAY-0001 (25-Nov)

| الجدول | الحقول |
|---|---|
| `LedgerJournalTable` | **JournalType = CustPayment** |
| `LedgerJournalTrans` | **AccountType = Cust**، **AmountCurCredit = 1,725**، Offset = Bank |
| `SpecTrans` | الربط بـ `CustTransOpen` |
| `CustTrans` (Payment) | **TransType = Payment**، AmountCur = -1,725 |
| `CustTrans` (INV-0001) | SettleAmountCur = 1,725، **Closed** |
| `CustSettlement` | — |
| `CustTransOpen` | يُحذف |
| `BankAccountTrans` | +1,725 |

**القيد:** Dr 110100 Bank 1,725 / Cr 120100 AR 1,725.

## ملخص دورة المبيعات

| المرحلة | InventTrans | InventSum | Customer subledger | GL |
|---|---|---|---|---|
| SO | OnOrder (6) | OnOrder +10 | — | — |
| Reserve | ReservPhysical (4) | ReservPhysical +10 | — | — |
| Packing slip | **Deducted (2)** | Deducted +10، Physical -10 | — | اختياري |
| Invoice | **Sold (1)** | PostedQty -10، Value -600 | CustTrans +1,725 | AR / Rev / VAT / COGS |
| Payment | — | — | Settlement | Bank / AR |

---

# الجزء 5: دورة المخزون

## I1: تحويل بأمر تحويل TO-0001 (W1 → TRN → W2)

**الإنشاء (06-Nov):**

| الجدول | الحقول |
|---|---|
| `InventTransferTable` | TransferId, **InventLocationIdFrom = W1**, **InventLocationIdTo = W2**, **InventLocationIdTransit = TRN**, **TransferStatus = Created**, ShipDate, ReceiveDate |
| `InventTransferLine` | ItemId, **QtyTransfer = 30**, QtyShipNow, QtyReceiveNow, QtyRemainShip, QtyRemainReceive, **InventTransId** (issue), **InventTransIdReceive**, **InventTransIdTransitFrom**, **InventTransIdTransitTo**, InventDimId |
| `InventTransOrigin` | Transfer order ship / receive / transit |
| `InventTrans` | W1 issue -30 **OnOrder**؛ W2 receipt +30 **Ordered** |

**الشحن Ship (06-Nov):**

| الجدول | الأثر |
|---|---|
| `InventTransferJour` / `InventTransferJourLine` | VoucherId, UpdateType = **Shipment** |
| `InventTrans` W1 | **StatusIssue = Sold (1)**، CostAmountPosted = -1,800 |
| `InventTrans` TRN | **StatusReceipt = Purchased (1)**، +1,800 |
| `InventSum` | W1 PostedQty 100→70؛ TRN +30 |
| `InventTransferTable` | **TransferStatus = Shipped** |

**الاستلام Receive (07-Nov):**

| الجدول | الأثر |
|---|---|
| `InventTransferJour` | UpdateType = **Receive** |
| `InventTrans` TRN | Sold -30 |
| `InventTrans` W2 | **Purchased +30**، +1,800 |
| `InventSum` | TRN = 0؛ W2 = 30 / 1,800 |
| `InventTransferTable` | **TransferStatus = Received** |

**الأثر المالي:** يُنشأ Voucher بنفس حساب 130100 Dr/Cr، فالصافي **0**. يظهر أثر فعلي فقط إذا اختلف حساب المخزون أو البُعد المالي بين المستودعات.

## I2: Transfer journal (تحويل مباشر بدون Transit، بديل لـ TO)

| الجدول | الحقول |
|---|---|
| `InventJournalTable` | JournalId, **JournalType = Transfer**, Posted |
| `InventJournalTrans` | ItemId, Qty, InventDimId (From), **ToInventDimId** (To), CostPrice |
| `InventTrans` | سطر issue **Sold** + سطر receipt **Purchased** |

الأثر المالي: صافيه 0.

## I3: Movement journal IMOV-0001 (12-Nov): صرف عينات

| الجدول | الحقول |
|---|---|
| `InventJournalTable` | **JournalType = Movement** |
| `InventJournalTrans` | ItemId, **Qty = -2**, InventDimId = W2, CostPrice = 60, CostAmount = -120, **LedgerDimension = 520200** (Offset account), TransDate, Voucher |
| `InventTrans` | **StatusIssue = Sold (1)** مباشرة (مادي ومالي معاً)، CostAmountPosted = -120 |
| `InventSum` (W2) | PostedQty 30→28، PostedValue -120 |

**القيد:** Dr 520200 Samples expense 120 / Cr 130100 Inventory 120.

## I4: Counting journal ICNT-0001 (15-Nov): جرد W1

| الجدول | الحقول |
|---|---|
| `InventJournalTable` | **JournalType = Count** |
| `InventJournalTrans` | ItemId, InventDimId = W1, **InventOnHand = 60** (رصيد النظام)، **Counted = 58**، **Qty = -2** (الفرق)، CostPrice = 60 |
| `InventTrans` | **Sold**، -120 |
| `InventSum` (W1) | PostedQty 60→58 |

**القيد:** Dr 520300 Inventory loss 120 / Cr 130100 Inventory 120. إذا جاء الفرق موجباً: Dr 130100 / Cr 420900 Inventory profit.

> **Profit/Loss journal** (JournalType = LossProfit): نفس الأثر، لكن بإدخال الكمية مباشرة بدل الجرد.

## I5: Inventory close (30-Nov)

| الجدول | الحقول |
|---|---|
| `InventClosing` | Voucher, TransDate, AdjustmentType (Closing/Recalculation), Running |
| `InventSettlement` | TransRecId, SettleTransId, QtySettled, **CostAmountAdjustment**, SettleType, Voucher |
| `InventTrans` | **CostAmountAdjustment**، ValueOpen |
| GL | تعديل COGS والمخزون بالفرق |

في المثال: التكلفة ثابتة (60)، فالتعديل = **0**. يظهر الفرق فقط عند تعدد أسعار الشراء.

## ملخص قيود المخزون

| العملية | JournalType | InventTrans | GL |
|---|---|---|---|
| Transfer order | — | Sold / Purchased عبر TRN | 0 (نفس الحساب) |
| Transfer journal | Transfer | Sold / Purchased | 0 |
| Movement | Movement | Sold أو Purchased | Inventory ↔ Offset account |
| Profit/Loss | LossProfit | Sold أو Purchased | Inventory ↔ Loss/Profit |
| Counting | Count | Sold أو Purchased | Inventory ↔ Loss/Profit |
| Close | — | CostAmountAdjustment | COGS ↔ Inventory |

---

# الجزء 6: الدورة المالية (GL)

## F1: القيد الافتتاحي GJ-0001 (01-Nov)

| الجدول | الحقول |
|---|---|
| `LedgerJournalTable` | **JournalType = Daily** |
| `LedgerJournalTrans` | AccountType = **Bank**، AmountCurDebit = 100,000؛ OffsetAccountType = **Ledger** (310100) |
| `BankAccountTrans` | +100,000 |
| GL | Dr 110100 Bank 100,000 / Cr 310100 Capital 100,000 |

## F2: قيد مصروف مع ضريبة GJ-0002 (26-Nov)

| الجدول | الحقول |
|---|---|
| `LedgerJournalTrans` | AccountType = Ledger (520400)، **AmountCurDebit = 3,000**، **TaxGroup = KSA-DOM-P**، **TaxItemGroup = STD**، Offset = Bank |
| `TaxUncommitted` | قبل الترحيل: 450 |
| `TaxTrans` | IncomingTax +450، Source = LedgerJournalTrans |
| `BankAccountTrans` | -3,450 |

**القيد:** Dr 520400 Rent 3,000 / Dr 120500 VAT receivable 450 / Cr 110100 Bank 3,450.

## F3: تسوية الضريبة (Settle and post sales tax) (30-Nov)

| الجدول | الأثر |
|---|---|
| `TaxTrans` | سجلات **TaxOrigin = TaxReporting** |
| `TaxReportVoucher` | Voucher الفترة |

**القيد:**

| الحساب | Dr | Cr |
|---|---|---|
| 220100 VAT payable | 225 | |
| 120500 VAT receivable | | 1,500 |
| 220900 VAT settlement | 1,275 | |

صافي 1,275 مستحق للاسترداد من الهيئة.

## F4: Bank reconciliation (Basic)

`BankAccountTrans.Reconciled = Yes` لكل حركة مطابقة لكشف البنك.

## F5: Reconciliation (Sub-ledger مقابل GL)

| المطابقة | Sub-ledger | GL |
|---|---|---|
| AP | `SUM(VendTrans.AmountMST)` = `SUM(VendTransOpen)` | 210100 |
| AR | `SUM(CustTrans.AmountMST)` | 120100 |
| Inventory | `SUM(InventSum.PostedValue)` (+ PhysicalValue إذا كان مرحّلاً) | 130100 |
| Bank | `SUM(BankAccountTrans.AmountMST)` | 110100 |
| VAT | `TaxTrans` غير المسوّاة | 120500 / 220100 |

## F6: إقفال الفترة

| الخطوة | الجدول | الأثر |
|---|---|---|
| Module access: AP/AR/Inventory = No access | `LedgerFiscalCalendarPeriod` | يمنع الترحيل من الموديولات |
| Status = On hold ثم Permanently closed | `LedgerFiscalCalendarPeriod.Status` | قفل كامل |
| Trial balance | `GeneralJournalAccountEntry` (+ `DimensionFocusBalance` بعد Update balances) | — |

> Year-end close خارج نطاق المثال الشهري.

---

# الجزء 7: النتائج النهائية (30-Nov)

## 7.1 المخزون: STK-001

| Warehouse | Qty | Value |
|---|---|---|
| W1 | 100 - 30 - 10 - 2 = **58** | 3,480 |
| W2 | 30 - 2 = **28** | 1,680 |
| TRN | 0 | 0 |
| **Total** | **86** | **5,160** ✓ = 6,000 - 600 - 120 - 120 |

## 7.2 Trial Balance

| الحساب | Dr | Cr |
|---|---|---|
| 110100 Bank (100,000 - 8,050 + 1,725 - 3,450) | 90,225 | |
| 120100 AR | 0 | |
| 120500 VAT receivable | 0 | |
| 130100 Inventory | 5,160 | |
| 210100 AP | | 0 |
| 220100 VAT payable | | 0 |
| 220900 VAT settlement | 1,275 | |
| 310100 Capital | | 100,000 |
| 410100 Sales revenue items | | 1,000 |
| 410200 Service revenue | | 500 |
| 510100 COGS | 600 | |
| 520100 Maintenance | 1,000 | |
| 520200 Samples | 120 | |
| 520300 Inventory loss | 120 | |
| 520400 Rent | 3,000 | |
| **Total** | **101,500** | **101,500** ✓ |

---

# الجزء 8: استعلامات التحقق (SQL)

```sql
DECLARE @Co NVARCHAR(4)='kcg', @AsOf DATE='2026-11-30';

-- Q1: Trace أي مستند مخزني (PO / SO / TO / Journal)
DECLARE @Ref NVARCHAR(20)='PO-0001';
SELECT ito.REFERENCECATEGORY, ito.REFERENCEID, ito.ITEMID, id.INVENTLOCATIONID,
       it.QTY, it.STATUSRECEIPT, it.STATUSISSUE, it.DATEPHYSICAL, it.DATEFINANCIAL,
       it.PACKINGSLIPID, it.INVOICEID, it.COSTAMOUNTPHYSICAL, it.COSTAMOUNTPOSTED,
       it.COSTAMOUNTADJUSTMENT, it.VOUCHERPHYSICAL, it.VOUCHER
FROM INVENTTRANSORIGIN ito
JOIN INVENTTRANS it ON it.INVENTTRANSORIGIN = ito.RECID AND it.DATAAREAID = ito.DATAAREAID
JOIN INVENTDIM id   ON id.INVENTDIMID = it.INVENTDIMID AND id.DATAAREAID = it.DATAAREAID
WHERE ito.REFERENCEID = @Ref AND ito.DATAAREAID = @Co;

-- Q2: On-hand لكل مستودع
SELECT s.ITEMID, d.INVENTLOCATIONID,
       SUM(s.POSTEDQTY) PostedQty, SUM(s.POSTEDVALUE) PostedValue,
       SUM(s.RECEIVED) Received, SUM(s.DEDUCTED) Deducted,
       SUM(s.ORDERED) Ordered, SUM(s.ONORDER) OnOrder, SUM(s.RESERVPHYSICAL) Reserved,
       SUM(s.PHYSICALINVENT) PhysicalInvent, SUM(s.AVAILPHYSICAL) AvailPhysical
FROM INVENTSUM s
JOIN INVENTDIM d ON d.INVENTDIMID = s.INVENTDIMID AND d.DATAAREAID = s.DATAAREAID
WHERE s.DATAAREAID = @Co AND s.CLOSED = 0
GROUP BY s.ITEMID, d.INVENTLOCATIONID;

-- Q3: GL لأي Voucher
DECLARE @Voucher NVARCHAR(20)='VI-0001-VOUCHER';
SELECT gje.SUBLEDGERVOUCHER, gje.ACCOUNTINGDATE, ma.MAINACCOUNTID, ma.NAME,
       gjae.POSTINGTYPE, gjae.ACCOUNTINGCURRENCYAMOUNT, gjae.ISCREDIT
FROM GENERALJOURNALENTRY gje
JOIN GENERALJOURNALACCOUNTENTRY gjae ON gjae.GENERALJOURNALENTRY = gje.RECID
JOIN MAINACCOUNT ma ON ma.RECID = gjae.MAINACCOUNT
WHERE gje.SUBLEDGERVOUCHER = @Voucher AND gje.SUBLEDGERVOUCHERDATAAREAID = @Co;

-- Q4: Subledger vs GL (AP / AR / Inventory / Bank)
WITH GL AS (
  SELECT ma.MAINACCOUNTID, SUM(gjae.ACCOUNTINGCURRENCYAMOUNT) Bal
  FROM GENERALJOURNALACCOUNTENTRY gjae
  JOIN GENERALJOURNALENTRY gje ON gje.RECID = gjae.GENERALJOURNALENTRY
  JOIN LEDGER l ON l.RECID = gje.LEDGER
  JOIN MAINACCOUNT ma ON ma.RECID = gjae.MAINACCOUNT
  WHERE l.NAME = @Co AND gje.ACCOUNTINGDATE <= @AsOf AND gje.POSTINGLAYER = 0
  GROUP BY ma.MAINACCOUNTID)
SELECT 'AP' Area,  (SELECT SUM(AMOUNTMST) FROM VENDTRANS WHERE DATAAREAID=@Co AND TRANSDATE<=@AsOf) Sub,
                   (SELECT Bal FROM GL WHERE MAINACCOUNTID='210100') GLBal
UNION ALL
SELECT 'AR',       (SELECT SUM(AMOUNTMST) FROM CUSTTRANS WHERE DATAAREAID=@Co AND TRANSDATE<=@AsOf),
                   (SELECT Bal FROM GL WHERE MAINACCOUNTID='120100')
UNION ALL
SELECT 'INV',      (SELECT SUM(POSTEDVALUE) FROM INVENTSUM WHERE DATAAREAID=@Co AND CLOSED=0),
                   (SELECT Bal FROM GL WHERE MAINACCOUNTID='130100')
UNION ALL
SELECT 'BANK',     (SELECT SUM(AMOUNTMST) FROM BANKACCOUNTTRANS WHERE DATAAREAID=@Co AND TRANSDATE<=@AsOf),
                   (SELECT Bal FROM GL WHERE MAINACCOUNTID='110100');

-- Q5: Trial balance
SELECT ma.MAINACCOUNTID, ma.NAME,
       SUM(CASE WHEN gjae.ISCREDIT=0 THEN gjae.ACCOUNTINGCURRENCYAMOUNT ELSE 0 END) Dr,
       SUM(CASE WHEN gjae.ISCREDIT=1 THEN -gjae.ACCOUNTINGCURRENCYAMOUNT ELSE 0 END) Cr,
       SUM(gjae.ACCOUNTINGCURRENCYAMOUNT) Balance
FROM GENERALJOURNALACCOUNTENTRY gjae
JOIN GENERALJOURNALENTRY gje ON gje.RECID = gjae.GENERALJOURNALENTRY
JOIN LEDGER l ON l.RECID = gje.LEDGER
JOIN MAINACCOUNT ma ON ma.RECID = gjae.MAINACCOUNT
WHERE l.NAME=@Co AND gje.ACCOUNTINGDATE <= @AsOf AND gje.POSTINGLAYER = 0
GROUP BY ma.MAINACCOUNTID, ma.NAME
ORDER BY ma.MAINACCOUNTID;
```

---

# الجزء 9: خطة الاختبار، خدمة بخدمة

## 9.1 GL

| ID | الخدمة | المتوقع | التحقق |
|---|---|---|---|
| GL-01 | Main accounts + Types | الإنشاء والتصنيف صحيحان | `MainAccount` |
| GL-02 | Account structure | رفض بُعد غير مسموح | رسالة خطأ |
| GL-03 | Fiscal calendar + periods | 12 فترة + Opening + Closing | `FiscalCalendarPeriod` |
| GL-04 | Number sequences | Voucher متسلسل | `NumberSequenceTable` |
| GL-05 | Journal names | Daily / Payment / CustPayment | `LedgerJournalName` |
| GL-06 | Opening journal | Bank 100,000 | Q3، `BankAccountTrans` |
| GL-07 | GJ with tax | 3,000 + 450 | `TaxUncommitted` ← `TaxTrans` |
| GL-08 | Unbalanced journal | **رفض** | — |
| GL-09 | Posting in closed period | **رفض** | `LedgerFiscalCalendarPeriod` |
| GL-10 | Module access = No | **رفض** ترحيل AP/AR | — |
| GL-11 | Reverse journal | قيد عكسي | `TransactionReversalTrans` |
| GL-12 | Tax settlement | 1,275 للـ settlement | `TaxTrans` (TaxReporting) |
| GL-13 | Bank reconciliation | Reconciled = Yes | `BankAccountTrans` |
| GL-14 | Trial balance | متوازن 101,500 | Q5 |

## 9.2 AP

| ID | الخدمة | المتوقع | التحقق |
|---|---|---|---|
| AP-01 | Posting profile | AP summary = 210100 | `VendLedgerAccounts` |
| AP-02 | Create vendor | الـ defaults تُنسخ من VendGroup | `VendTable` |
| AP-03 | Vendor Blocked = All | **منع** إنشاء PO | `VendTable.Blocked` |
| AP-04 | Create PO | Backorder، Ordered +100 | `PurchTable`, `PurchLine`, `InventTrans` (5) |
| AP-05 | PO tax | 1,050 | `TaxUncommitted` |
| AP-06 | Confirm PO | DocumentState = Confirmed | `VendPurchOrderJour` |
| AP-07 | Product receipt full | Received، On-hand +100 | `VendPackingSlipJour`, `InventTrans` (2) |
| AP-08 | Partial receipt 60 + 40 | Remain صحيح | `PurchLine.RemainPurchPhysical` |
| AP-09 | Receipt with ledger posting | Accrual ثم عكسه عند الفاتورة | `InventTransPosting` (Physical) |
| AP-10 | Cancel product receipt | العودة إلى Ordered | `InventTrans` |
| AP-11 | Pending vendor invoice | ضريبة 1,050 + Matching | `VendInvoiceInfoTable/Line` |
| AP-12 | Post vendor invoice | VendTrans -8,050، Purchased | Q1، Q3، `VendTrans` |
| AP-13 | Non-stock service line | Expense 520100، بدون InventSum | Q2، Q3 |
| AP-14 | Invoice price ≠ PO | تحذير matching أو تكلفة جديدة | `VendInvoiceInfoLine` |
| AP-15 | Duplicate vendor invoice number | **رفض** | `VendParameters` |
| AP-16 | Vendor payment full | Closed | `VendSettlement`, `VendTransOpen` |
| AP-17 | Partial payment | Open متبقٍّ | `VendTransOpen` |
| AP-18 | Settle / unsettle | يعمل | `VendSettlement` |
| AP-19 | Payment reversal | فتح الفاتورة | `VendTrans` |
| AP-20 | AP reconciliation | Sub = GL | Q4 |

## 9.3 AR

| ID | الخدمة | المتوقع | التحقق |
|---|---|---|---|
| AR-01 | Posting profile | 120100 | `CustLedgerAccounts` |
| AR-02 | Create customer | الـ defaults صحيحة | `CustTable` |
| AR-03 | Customer Blocked | **منع** | — |
| AR-04 | Credit limit | تحذير أو منع | `CustParameters.CreditMaxCheck` |
| AR-05 | Create SO | OnOrder | `SalesLine`, `InventTrans` (6) |
| AR-06 | Confirm SO | Confirmation | `CustConfirmJour` |
| AR-07 | Reserve | ReservPhysical | `InventSum` |
| AR-08 | Packing slip | Deducted، -10 | `CustPackingSlipJour`, `InventTrans` (2) |
| AR-09 | Partial delivery | Remain صحيح | `SalesLine` |
| AR-10 | Cancel packing slip | العودة إلى Reserved | `InventTrans` |
| AR-11 | Invoice | CustTrans 1,725، COGS 600 | Q1، Q3 |
| AR-12 | Service line | Revenue 410200 فقط | Q3 |
| AR-13 | Sell > available (Negative = No) | **رفض** | — |
| AR-14 | Free text invoice | AR + Revenue + VAT | `CustInvoiceTable` ← `CustInvoiceJour` |
| AR-15 | Credit note (أساسي) | عكس الإيراد والتكلفة | `CustTrans` سالب |
| AR-16 | Customer payment full | Closed | `CustSettlement` |
| AR-17 | Partial payment | Open متبقٍّ | `CustTransOpen` |
| AR-18 | Aging | Buckets صحيحة | Aging report |
| AR-19 | AR reconciliation | Sub = GL | Q4 |

## 9.4 Inventory

| ID | الخدمة | المتوقع | التحقق |
|---|---|---|---|
| INV-01 | Sites / Warehouses / Transit | ربط W1 بـ TRN | `InventLocation` |
| INV-02 | Model group FIFO / NONSTK | Stocked flag صحيح | `InventModelGroup` |
| INV-03 | Inventory posting | كل نوع له حساب | `InventPosting` |
| INV-04 | Release items | الإعداد مكتمل | `InventTable`, `InventTableModule` |
| INV-05 | Transfer order create | OnOrder / Ordered | `InventTransferTable/Line` |
| INV-06 | Ship TO | W1 -30، TRN +30، Shipped | Q1، Q2 |
| INV-07 | Receive TO | TRN 0، W2 +30، Received | Q1، Q2 |
| INV-08 | Partial ship/receive | Remain صحيح | `InventTransferLine` |
| INV-09 | TO GL effect | صافي 0 | Q3 |
| INV-10 | Transfer journal | Issue + Receipt | `InventJournalTrans.ToInventDimId` |
| INV-11 | Movement journal | Dr 520200 / Cr 130100 | Q3 |
| INV-12 | Profit/Loss (+1) | Dr 130100 / Cr 420900 | Q3 |
| INV-13 | Counting journal | الفرق -2 خسارة | `InventJournalTrans.Counted` |
| INV-14 | Negative inventory block | **رفض** | — |
| INV-15 | On-hand list | 58 / 28 | Q2 |
| INV-16 | Inventory value report | 5,160 | Inventory value report |
| INV-17 | Inventory close | Adjustment = 0 | `InventClosing`, `InventSettlement` |
| INV-18 | Close مع سعرين مختلفين | تعديل COGS | `InventTrans.CostAmountAdjustment` |
| INV-19 | Inventory reconciliation | PostedValue = GL 130100 | Q4 |

## 9.5 End-to-End

| ID | السيناريو | المتوقع |
|---|---|---|
| E2E-01 | تشغيل التسلسل الزمني 1–14 كاملاً | Trial balance = الجدول 7.2 |
| E2E-02 | مطابقة AP / AR / Inv / Bank | جميع الفروقات = 0 (Q4) |
| E2E-03 | Traceability: PO → PR → VI → Payment | كل الـ vouchers مرتبطة |
| E2E-04 | Traceability: SO → PS → INV → Payment | كل الـ vouchers مرتبطة |
| E2E-05 | Period close | لا ترحيل بعد الإقفال |

