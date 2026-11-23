# كتالوج السيناريوهات لكل موديول (Core D365 F&O)

## دليل الاختصارات

| الرمز | المعنى |
|---|---|
| **IT** | `InventTrans` |
| **IS** | `InventSum` |
| StatusReceipt | Ordered (5) → Received (2) → Purchased (1) |
| StatusIssue | OnOrder (6) → ReservPhysical (4) → Deducted (2) → Sold (1) |
| **—** | لا يوجد أثر |
| ⚙️ | سيناريو اختياري، يُنفَّذ حسب حاجة العميل |

---

# 1. Accounts Payable (المشتريات والموردون)

## 1.1 المورد (Master)

| ID | السيناريو | الجداول والحقول | المادي | المالي |
|---|---|---|---|---|
| AP-01 | إنشاء مورد | `DirPartyTable`, `VendTable` (VendGroup, PaymTermId, TaxGroup, Currency), `LogisticsPostalAddress`, `TaxRegistration`, `VendBankAccount` | — | — |
| AP-02 | مورد لمرة واحدة | `VendTable.OneTimeVendor = Yes` | — | — |
| AP-03 | إيقاف مورد (Invoice / Payment / All) | `VendTable.Blocked` | — | يمنع المستند حسب النوع |
| AP-04 | Invoice account مختلف | `VendTable.InvoiceAccount` | — | `VendTrans` تُنشأ على حساب الفوترة |

## 1.2 أوامر الشراء

| ID | السيناريو | الجداول والحقول | المادي | المالي |
|---|---|---|---|---|
| AP-05 | PO صنف مخزني، دورة كاملة | `PurchTable/Line` → `VendPackingSlipJour/Trans` → `VendInvoiceJour/Trans`, `VendTrans/Open`, `TaxTrans` | IT: 5→2→1؛ IS: Ordered→Received→PostedQty | Dr Inventory + Dr VAT / Cr AP |
| AP-06 | PO صنف خدمي (Non-stock) | نفس الجداول، بدون IS | — | Dr Expense (Purch expenditure for expense) + VAT / Cr AP |
| AP-07 | PO بفئة مشتريات (بدون صنف) | `PurchLine.ProcurementCategory`، ItemId فارغ | — | Dr Expense (من Category posting) / Cr AP |
| AP-08 | PO مختلط (مخزني + خدمي) | — | المخزني فقط | Inventory + Expense |
| AP-09 | استلام جزئي (60 ثم 40) | `VendPackingSlipJour` ×2، `PurchLine.RemainPurchPhysical` | IT يُقسَّم: 60 Received و 40 Ordered | — |
| AP-10 | استلام زائد | `PurchLine.OverDeliveryPct` | IT بالكمية الزائدة | — |
| AP-11 | إغلاق المتبقي (Deliver remainder = 0) | `PurchLine.RemainPurchPhysical = 0` | يُحذف IT المتبقي (Ordered) | — |
| AP-12 | عدة استلامات ← فاتورة واحدة | `VendInvoicePackingSlipQuantityMatch` | — | قيد واحد |
| AP-13 | استلام واحد ← عدة فواتير | `PurchLine.RemainPurchFinancial` | IT يُقسَّم إلى Purchased و Received | قيد لكل فاتورة |
| AP-14 | فاتورة بدون استلام مسبق | ReceiveNow في الفاتورة | IT من 5 إلى 1 مباشرة | قيد كامل |
| AP-15 | فرق سعر الفاتورة عن PO | `VendInvoiceInfoLine.PurchPrice` | `CostAmountPosted` بسعر الفاتورة | FIFO: المخزون بسعر الفاتورة. Standard cost: حساب Price variance |
| AP-16 | إلغاء استلام | Cancel على `VendPackingSlipJour` | IT يعود من 2 إلى 5 | عكس الـ Accrual إن كان مرحّلاً |
| AP-17 | تصحيح كمية استلام | Correct ← `VendPackingSlipVersion` | IT يُعدَّل | — |
| AP-18 | إلغاء PO قبل الاستلام | `PurchStatus = Canceled` | يُحذف IT، و IS Ordered ينقص | — |
| AP-19 | خصم سطر / خصم إجمالي | `PurchLine.LineDisc`, `LinePercent`، `PurchTable.DiscPercent`، `VendInvoiceJour.SumLineDisc / EndDisc` | التكلفة صافية بعد الخصم | Inventory صافي |
| AP-20 | مصاريف شحن (Charges) | `MarkupTable`, `MarkupTrans` (MarkupCode, Value, TransTableId) | Charge to item: تُضاف لـ CostAmountPosted | Dr Inventory أو Freight expense / Cr AP |
| AP-21 | مرتجع للمورد | PO بكمية سالبة، `ReturnItemNum`، Return packing slip، Credit note | IT issue: 6→2→1، IS PostedQty ينقص | Dr AP / Cr Inventory / Cr VAT receivable |
| AP-22 | إشعار دائن بدون مرتجع (تعديل سعر) | فاتورة سالبة بدون PO | — | Dr AP / Cr Expense + VAT |
| AP-23 | Acquisition بعملة أجنبية | `PurchTable.CurrencyCode`، `VendTrans.AmountCur` و `AmountMST` و `ExchRate` | التكلفة بالعملة المحلية | بسعر صرف تاريخ الفاتورة |

## 1.3 فواتير بدون PO

| ID | السيناريو | الجداول والحقول | المادي | المالي |
|---|---|---|---|---|
| AP-24 | Invoice journal | `LedgerJournalTable/Trans` (AccountType = Vend، Offset = Ledger، TaxGroup)، `TaxUncommitted` | — | Dr Expense + VAT / Cr AP |
| AP-25 | Pending vendor invoice بفئة مشتريات | `VendInvoiceInfoTable/Line` | — | نفس الأثر |
| AP-26 ⚙️ | Invoice register ثم Invoice approval | قيدان على مرحلتين | — | قيد مؤقت، ثم القيد النهائي |

## 1.4 المدفوعات والتسويات

| ID | السيناريو | الجداول والحقول | المالي |
|---|---|---|---|
| AP-27 | دفعة كاملة | `LedgerJournalTrans` (Payment)، `SpecTrans`، `VendSettlement`، `BankAccountTrans` | Dr AP / Cr Bank |
| AP-28 | دفعة جزئية | `VendTransOpen` يبقى بالمتبقي | نفس القيد |
| AP-29 | دفعة لعدة فواتير | `SpecTrans` متعدد | قيد واحد |
| AP-30 | Payment proposal حسب الاستحقاق | `CustVendPaymProposalLine` ← `LedgerJournalTrans` | نفس القيد |
| AP-31 | دفع بشيك | `BankChequeTable` (ChequeNum, ChequeStatus) | نفس القيد |
| AP-32 | دفعة بدون تسوية (On account) | `VendTrans` بدون SpecTrans | Dr AP، تبقى مفتوحة |
| AP-33 | تسوية إشعار دائن مع فاتورة | `VendSettlement` | لا قيد |
| AP-34 | خصم نقدي | `CashDisc`, `VendTransCashDisc`, `VendSettlement.UtilizedCashDisc` | Dr AP / Cr Bank / Cr Cash discount |
| AP-35 | دفعة مقدمة | `LedgerJournalTrans.Prepayment = Yes` ثم التسوية مع الفاتورة | Dr Vendor prepayment / Cr Bank |
| AP-36 | عكس دفعة أو إلغاء شيك | `TransactionReversalTrans`، `BankChequeTable` = Cancelled | قيد عكسي، والفاتورة تُفتح من جديد |
| AP-37 | دفع بعملة أجنبية | `VendSettlement.ExchAdjustment` | Realized gain/loss |

## 1.5 نهاية الفترة

| ID | السيناريو | الجداول | المالي |
|---|---|---|---|
| AP-38 | Vendor aging | `VendTrans`, `VendTransOpen` | — |
| AP-39 | AP reconciliation | Sub = GL 210100 | — |
| AP-40 | Received not invoiced | `VendPackingSlipTrans`, IT = Received | رصيد Accrual (إذا كان مرحّلاً) |
| AP-41 ⚙️ | إعادة تقييم عملة | `VendTrans` (TransType = ExchAdjustment) | Unrealized gain/loss |

---

# 2. Accounts Receivable (المبيعات والعملاء)

## 2.1 العميل (Master)

| ID | السيناريو | الجداول والحقول | الأثر |
|---|---|---|---|
| AR-01 | إنشاء عميل | `DirPartyTable`, `CustTable`, `LogisticsPostalAddress`, `TaxRegistration` | — |
| AR-02 | إيقاف عميل (Invoice / All) | `CustTable.Blocked` | منع المستندات |
| AR-03 | تجاوز حد الائتمان | `CustTable.CreditMax`، `CustParameters.CreditMaxCheck` | تحذير أو إيقاف |
| AR-04 | Invoice account مختلف | `CustTable.InvoiceAccount` | `CustTrans` على حساب الفوترة |

## 2.2 أوامر البيع

| ID | السيناريو | الجداول والحقول | المادي | المالي |
|---|---|---|---|---|
| AR-05 | عرض سعر ← أمر بيع | `SalesQuotationTable/Line` ← `SalesTable` | IT QuotationIssue (7) ثم OnOrder (6) | — |
| AR-06 | SO صنف مخزني، دورة كاملة | `SalesTable/Line` ← `CustPackingSlipJour` ← `CustInvoiceJour`، `CustTrans`، `TaxTrans` | IT: 6→4→2→1 | Dr AR / Cr Revenue + VAT؛ Dr COGS / Cr Inventory |
| AR-07 | SO صنف خدمي | بدون IS | — | Dr AR / Cr Service revenue + VAT |
| AR-08 | SO مختلط | — | المخزني فقط | Revenue + COGS للمخزني فقط |
| AR-09 | تسليم جزئي | `CustPackingSlipJour` ×n، `SalesLine.RemainSalesPhysical` | IT يُقسَّم | — |
| AR-10 | تسليم زائد أو ناقص، أو إغلاق المتبقي | `SalesLine.OverDeliveryPct / UnderDeliveryPct` | IT يُعدَّل | — |
| AR-11 | عدة Packing slips ← فاتورة واحدة | `CustInvoicePackingSlipQuantityMatch` | — | قيد واحد |
| AR-12 | Summary invoice لعدة أوامر | `SalesParmTable`، `CustInvoiceSalesLink` (عدة SalesId) | — | قيد واحد |
| AR-13 | فاتورة بدون Packing slip | Invoice = All | IT من 4 أو 6 إلى 1 مباشرة | قيد كامل |
| AR-14 | فاتورة جزئية | `SalesLine.RemainSalesFinancial` | IT يُقسَّم | قيد للجزء المفوتر |
| AR-15 | إلغاء Packing slip | Cancel | IT يعود من 2 إلى 4 | عكس القيد إن كان مرحّلاً |
| AR-16 | تصحيح Packing slip | Correct | IT يُعدَّل | — |
| AR-17 | إلغاء SO أو سطر قبل التسليم | `SalesStatus = Canceled` | يُحذف IT | — |
| AR-18 | Trade agreement وخصومات | `PriceDiscTable`، `SalesLine.LineDisc / LinePercent`، `SalesTable.DiscPercent`، `CustInvoiceJour.SumLineDisc / EndDisc` | — | Revenue صافٍ، أو Dr Sales discount إن كان مفعّلاً |
| AR-19 | مصاريف شحن على العميل | `MarkupTrans` (Credit type = Ledger) | — | Dr AR / Cr Freight revenue + VAT |
| AR-20 | Return order مع Credit note | `SalesTable` (SalesType = ReturnItem, ReturnItemNum)، `SalesLine.InventTransIdReturn`، Arrival (`WMSJournalTable`) | IT receipt: 3→2→1، IS يزيد | Dr Revenue + VAT / Cr AR؛ Dr Inventory / Cr COGS |
| AR-21 | مرتجع Credit only (بدون استلام) | Disposition code = Credit only | — | Dr Revenue + VAT / Cr AR |
| AR-22 | إشعار دائن بدون مرتجع | FTI سالبة | — | Dr Revenue + VAT / Cr AR |
| AR-23 | بيع بعملة أجنبية | `CustTrans.AmountCur` و `AmountMST` | — | بسعر صرف تاريخ الفاتورة |

## 2.3 Free Text Invoice

| ID | السيناريو | الجداول | المالي |
|---|---|---|---|
| AR-24 | FTI خدمة | `CustInvoiceTable/Line` ← `CustInvoiceJour/Trans` | Dr AR / Cr Revenue + VAT |
| AR-25 | FTI بعدة أسطر بضرائب مختلفة (15% / 0% / معفى) | `TaxTrans` لكل سطر | حسب الكود |
| AR-26 | تصحيح أو إلغاء FTI (Correct / Credit) | FTI عكسية | عكس |

## 2.4 التحصيل والتسويات

| ID | السيناريو | الجداول | المالي |
|---|---|---|---|
| AR-27 | تحصيل كامل | `LedgerJournalTrans` (CustPayment)، `SpecTrans`، `CustSettlement`، `BankAccountTrans` | Dr Bank / Cr AR |
| AR-28 | تحصيل جزئي | `CustTransOpen` يبقى بالمتبقي | — |
| AR-29 | تحصيل لعدة فواتير | `SpecTrans` متعدد | — |
| AR-30 | تحصيل زائد أو On account | `CustTrans` مفتوحة دائنة | — |
| AR-31 | تسوية Credit note مع فاتورة | `CustSettlement` | لا قيد |
| AR-32 | خصم نقدي | `CustTransCashDisc`, `UtilizedCashDisc` | Dr Cash discount |
| AR-33 | شيك مرتجع أو عكس تحصيل | `TransactionReversalTrans` | عكس، والفاتورة تُفتح |
| AR-34 | دفعة مقدمة من العميل | `LedgerJournalTrans.Prepayment = Yes` | Dr Bank / Cr Customer prepayment |
| AR-35 | رد رصيد دائن للعميل | Journal: Cust Debit / Bank Credit | Dr AR / Cr Bank |
| AR-36 | إعدام دين (Write-off) | Write off ← `LedgerJournalTrans` + Reason | Dr Bad debt / Cr AR |
| AR-37 | تحصيل بعملة أجنبية | `CustSettlement.ExchAdjustment` | Realized gain/loss |

## 2.5 نهاية الفترة

| ID | السيناريو | الجداول |
|---|---|---|
| AR-38 | Aging + Customer statement | `CustTrans`, `CustTransOpen`, `CustAging` |
| AR-39 | AR reconciliation | Sub = GL 120100 |
| AR-40 | Delivered not invoiced | IT = Deducted، IS.Deducted، PhysicalValue |
| AR-41 ⚙️ | إعادة تقييم عملة | `CustTrans` (TransType = ExchAdjustment) |
| AR-42 ⚙️ | Collection letters / Interest | `CustCollectionLetterJour`, `CustInterestJour` |

---

# 3. Inventory (المخزون)

## 3.1 الأرصدة والتحويلات

| ID | السيناريو | الجداول والحقول | المادي | المالي |
|---|---|---|---|---|
| INV-01 | رصيد افتتاحي للمخزون | `InventJournalTable/Trans` (Movement، Offset = Opening account) | IT Purchased (1)، IS PostedQty يزيد | Dr Inventory / Cr Opening balance |
| INV-02 | Transfer order كامل | `InventTransferTable/Line`، `InventTransferJour/JourLine` | W1 Sold ← TRN Purchased/Sold ← W2 Purchased | صافي 0 |
| INV-03 | TO شحن جزئي | `QtyShipNow`, `QtyRemainShip` | جزء في TRN | صافي 0 |
| INV-04 | TO استلام جزئي | `QtyReceiveNow`, `QtyRemainReceive` | الباقي يبقى في TRN | صافي 0 |
| INV-05 | إلغاء TO قبل الشحن | حذف الأسطر | يُحذف IT | — |
| INV-06 | TO بين Sites لكل منها بُعد مالي مختلف | Site = Financial dimension | — | Dr Inventory (Site B) / Cr Inventory (Site A) |
| INV-07 | Transfer journal (نفس الموقع) | `InventJournalTrans.ToInventDimId` | Sold / Purchased | صافي 0 |
| INV-08 | Transfer journal بين Sites | نفس الجداول | — | كما في INV-06 |

## 3.2 التسويات والجرد

| ID | السيناريو | الجداول والحقول | المادي | المالي |
|---|---|---|---|---|
| INV-09 | صرف لمصروف (Movement issue) | JournalType = Movement، Qty < 0، `LedgerDimension` | IT Sold | Dr Expense / Cr Inventory |
| INV-10 | استلام من حساب مالي (Movement receipt) | Qty > 0 | IT Purchased | Dr Inventory / Cr Offset |
| INV-11 | زيادة مخزون (Profit/Loss) | JournalType = LossProfit، Qty > 0 | +Qty | Dr Inventory / Cr Inventory profit |
| INV-12 | تالف أو فاقد (Profit/Loss) | Qty < 0 | -Qty | Dr Inventory loss / Cr Inventory |
| INV-13 | جرد بزيادة | JournalType = Count، `InventOnHand` < `Counted` | +Qty | Dr Inventory / Cr Profit |
| INV-14 | جرد بعجز | `InventOnHand` > `Counted` | -Qty | Dr Loss / Cr Inventory |
| INV-15 | جرد مطابق | Qty = 0 | لا IT | — |
| INV-16 ⚙️ | قفل الأصناف أثناء الجرد | `InventParameters` | منع الحركة | — |
| INV-17 ⚙️ | حجز مخزون يدوي (Blocking) | `InventBlocking` | AvailPhysical ينقص | — |

## 3.3 التكلفة والإقفال

| ID | السيناريو | الجداول والحقول | المالي |
|---|---|---|---|
| INV-18 | مخزون سالب مسموح، ثم استلام | `InventModelGroup.NegativePhysical/Financial` | تكلفة تقديرية، ثم تعديل عند الإقفال |
| INV-19 | ترحيل بتاريخ قبل آخر إقفال | `InventClosing` | **رفض** |
| INV-20 | Inventory close بدون فرق | `InventClosing`, `InventSettlement` | تعديل = 0 |
| INV-21 | Inventory close بفرق FIFO | `InventSettlement.CostAmountAdjustment`، `InventTrans.CostAmountAdjustment` | Dr Inventory / Cr COGS (أو العكس). المثال تحت الجدول |
| INV-22 | Recalculation (بدون إقفال) | `InventClosing` (AdjustmentType = Recalculation) | تعديل بدون قفل الفترة |
| INV-23 | إلغاء الإقفال (Cancel closing) | `InventClosing` | عكس التعديلات |
| INV-24 | فرق سعر فاتورة بعد بيع الصنف | AP-15 + إقفال | الفرق يذهب للـ COGS عند الإقفال |
| INV-25 | صنف غير مخزني | Stocked = No | Expense أو Revenue فقط |
| INV-26 | تقييم الاستلام والتسليم غير المفوتر | IS.PhysicalValue | يظهر في Inventory value report |
| INV-27 ⚙️ | صنف Standard cost | `InventItemPrice`, Costing version | فروقات السعر على حسابات Price variance |

**مثال INV-21:**

| الحركة | التفاصيل | القيمة |
|---|---|---|
| شراء 1 | 100 @ 60 | 6,000 |
| شراء 2 | 100 @ 66 | 6,600 |
| بيع 150 قبل الإقفال | بالمتوسط الجاري 63 | COGS = 9,450 |
| تكلفة FIFO الصحيحة | 100×60 + 50×66 | 9,300 |
| **تعديل الإقفال** | Dr Inventory / Cr COGS | **150** |
| الرصيد المتبقي | 50 @ 66 | 3,300 ✓ |

## 3.4 التقارير

| ID | التقرير | المصدر |
|---|---|---|
| INV-28 | On-hand list | `InventSum` |
| INV-29 | Inventory transactions | `InventTrans`, `InventTransOrigin` |
| INV-30 | Inventory value report | `InventTrans`, `InventSum` مقابل GL 130100 |

---

# 4. General Ledger (المالية)

## 4.1 القيود اليومية

| ID | السيناريو | الجداول والحقول | المالي |
|---|---|---|---|
| GL-01 | أرصدة افتتاحية | `LedgerJournalTrans` (Daily) | Balance sheet فقط |
| GL-02 | قيد بسيط (حساب ← حساب) | `LedgerJournalTrans` (Ledger / Ledger) | — |
| GL-03 | Voucher متعدد الأسطر | عدة أسطر بنفس Voucher | — |
| GL-04 | قيد بأبعاد مالية | `LedgerDimension` (Dept / CostCenter) | الأبعاد تظهر في GJAE |
| GL-05 | مصروف بضريبة مدخلات | `TaxGroup`, `TaxItemGroup`، `TaxTrans` IncomingTax | Dr Expense + VAT / Cr Bank |
| GL-06 | إيراد متنوع بضريبة مخرجات | `TaxTrans` OutgoingTax | Dr Bank / Cr Revenue + VAT |
| GL-07 | قيد على مورد أو عميل | AccountType = Vend أو Cust | يُنشئ `VendTrans` أو `CustTrans` |
| GL-08 | إيداع أو سحب بنكي | AccountType = Bank، `BankAccountTrans` | — |
| GL-09 | تحويل بين حسابين بنكيين | Bank ↔ Bank | — |
| GL-10 | رسوم بنكية | Dr Bank charges / Cr Bank | — |
| GL-11 | مستحق بعكس تلقائي | `LedgerJournalTrans.ReverseEntry`, `ReverseDate` | قيد عكسي في الفترة التالية |
| GL-12 | إطفاء مصروف مقدم | قيد شهري | Dr Expense / Cr Prepaid |
| GL-13 ⚙️ | قيد دوري (Periodic journal) | JournalType = Periodic | — |
| GL-14 | قيد بعملة أجنبية | `AmountCur`, `ExchRate` | — |
| GL-15 | عكس قيد مرحّل | Voucher → Reverse، `TransactionReversalTrans` | قيد عكسي |
| GL-16 ⚙️ | قيد بموافقة (Workflow) | `LedgerJournalTable.WorkflowApprovalStatus` | — |

## 4.2 الضريبة والبنك

| ID | السيناريو | الجداول | المالي |
|---|---|---|---|
| GL-17 | تسوية الضريبة (Settle and post) | `TaxTrans` (TaxOrigin = TaxReporting)، `TaxReportVoucher` | Payable / Receivable إلى حساب Settlement |
| GL-18 | سداد الضريبة للهيئة | `VendTrans` (مورد الهيئة) ثم دفعة | Dr Authority / Cr Bank |
| GL-19 | تسوية بنكية (Basic) | `BankAccountTrans.Reconciled` | — |
| GL-20 ⚙️ | إعادة تقييم حسابات GL بالعملة | Foreign currency revaluation | Unrealized gain/loss |

## 4.3 الإقفال والتقارير

| ID | السيناريو | الجداول | الأثر |
|---|---|---|---|
| GL-21 | Module access (AP/AR/INV = No access) | `LedgerFiscalCalendarPeriod` | منع ترحيل الموديولات |
| GL-22 | فترة On hold ثم Closed | `LedgerFiscalCalendarPeriod.Status` | منع كامل |
| GL-23 | إعادة فتح فترة | Status = Open | — |
| GL-24 | إقفال السنة | Closing period، `GeneralJournalEntry` (Closing layer) | أرصدة P&L تُنقل إلى Retained earnings |
| GL-25 | Trial balance / Voucher transactions | `GeneralJournalAccountEntry`، `DimensionFocusBalance` | — |
| GL-26 | مطابقة Subledgers | AP / AR / INV / Bank / VAT مقابل GL | الفرق = 0 |

---

# 5. السيناريوهات السلبية (Negative) لكل موديول

| ID | الموديول | السيناريو | المتوقع |
|---|---|---|---|
| NEG-01 | AP | PO لمورد Blocked | رفض |
| NEG-02 | AP | رقم فاتورة مورد مكرر | رفض أو تحذير |
| NEG-03 | AP | كمية فاتورة أكبر من المستلَم | تحذير matching |
| NEG-04 | AP | إلغاء استلام بعد الفاتورة | رفض |
| NEG-05 | AR | SO لعميل Blocked | رفض |
| NEG-06 | AR | تجاوز Credit limit | إيقاف |
| NEG-07 | AR | تسليم كمية أكبر من المتاح (Negative = No) | رفض |
| NEG-08 | AR | إلغاء Packing slip بعد الفاتورة | رفض |
| NEG-09 | AR | حذف SO مفوتر | رفض |
| NEG-10 | INV | حركة بدون Site أو Warehouse | رفض (Storage dimension) |
| NEG-11 | INV | Inventory posting ناقص | خطأ "Account not specified" |
| NEG-12 | INV | ترحيل قبل تاريخ آخر إقفال مخزون | رفض |
| NEG-13 | GL | قيد غير متوازن | رفض |
| NEG-14 | GL | ترحيل في فترة مغلقة أو On hold | رفض |
| NEG-15 | GL | حساب Suspended أو بُعد غير مسموح | رفض |
| NEG-16 | GL | Posting profile بدون Summary account | رفض |

---

# 6. ملخص التغطية

| الموديول | عدد السيناريوهات | الأساسي | الاختياري ⚙️ |
|---|---|---|---|
| AP | 41 | 39 | 2 |
| AR | 42 | 40 | 2 |
| Inventory | 30 | 27 | 3 |
| GL | 26 | 23 | 3 |
| Negative | 16 | 16 | — |
| **الإجمالي** | **155** | **145** | **10** |

**ترتيب التنفيذ المقترح:**
1. Setup و Master data لكل موديول.
2. AP-05 إلى AP-14، ثم المدفوعات.
3. AR-06 إلى AR-14، ثم التحصيل.
4. Inventory: التحويلات والتسويات والجرد.
5. التعديلات والمرتجعات (AP-15 إلى AP-22، AR-15 إلى AR-22).
6. GL: القيود اليومية، ثم الضريبة والبنك.
7. Inventory close (INV-20 إلى INV-23).
8. المطابقات (GL-26) وإقفال الفترة (GL-21 و GL-22).
9. السيناريوهات السلبية في آخر كل موديول.

