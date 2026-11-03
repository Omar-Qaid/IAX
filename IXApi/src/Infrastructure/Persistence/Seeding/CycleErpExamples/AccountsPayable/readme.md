**Accounts Payable في D365 F&O** يغطي دورة المورد من الإعداد إلى الفاتورة، الدفع، التسوية، والإقفال. ولتغطية الدورة كاملة، نضم مراحل أمر الشراء والاستلام المرتبطة به.

النطاق هنا **Core ERP**: مشتريات مخزنية وخدمية، دون WHS أو إنتاج أو تصنيع.

## 1. Configuration — الإعداد

| الإعداد | ما يجب ضبطه | الجداول الرئيسية |
|---|---|---|
| إعدادات الحسابات الدائنة | الفواتير، التكرار، المطابقة، التسوية، الترقيم | `VendParameters` |
| مجموعات الموردين | تصنيف الموردين والقيم الافتراضية | `VendGroup` |
| ملفات ترحيل الموردين | حساب مراقبة الموردين، وحسابات الدفعات المقدمة | `VendPosting` |
| شروط الدفع | احتساب تاريخ الاستحقاق | `PaymTerm` |
| طرق دفع الموردين | تحويل، شيك، وحساب البنك الافتراضي | `VendPaymModeTable` |
| الخصم النقدي | نسبة الخصم وفترة الاستحقاق | `CashDisc` |
| أسماء اليوميات | يوميات الفواتير والدفع والموافقات | `LedgerJournalName` |
| الضرائب | أكواد ومجموعات الضرائب وحسابات الترحيل | `TaxTable`, `TaxGroupHeading`, `TaxGroupData`, `TaxItemGroupHeading`, `TaxOnItem` |
| العملات وأسعار الصرف | عملة المحاسبة وأسعار صرف المعاملات | `Ledger`, `Currency`, `ExchangeRateType`, `ExchangeRateCurrencyPair`, `ExchangeRate` |
| الحسابات والأبعاد | حسابات الموردين، المصروفات، البنوك، المخزون والأبعاد | `MainAccount`, `DimensionAttributeValueCombination` |
| ترحيل المشتريات والمخزون | استلام غير مفوتر، استحقاق مشتريات، مخزون مفوتر، فروق | `InventPosting`, `InventModelGroup`, `InventParameters` |
| المطابقة والموافقات | مطابقة السعر والكمية والتفاوتات واعتماد الفواتير | `VendParameters` وجداول سياسات المطابقة وWorkflow بحسب الإعداد |

**ملف ترحيل المورد يحدد حساب مراقبة الموردين في الأستاذ العام.** أما حساب المخزون أو المصروف فيتحدد حسب نوع السطر وإعدادات الترحيل والتوزيع المحاسبي. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/vendor-posting-profiles?utm_source=chatgpt.com)

## 2. Master data — البيانات الأساسية

| البيانات | المطلوب | الجداول الرئيسية |
|---|---|---|
| المورد | الحساب، المجموعة، العملة، شروط وطريقة الدفع، الحظر | `VendTable` |
| اسم وهوية المورد | اسم المؤسسة أو الشخص | `DirPartyTable` |
| العناوين والاتصال | عنوان الفاتورة، البريد والهاتف | `DirPartyLocation`, `LogisticsLocation`, `LogisticsPostalAddress`, `LogisticsElectronicAddress` |
| بنك المورد | IBAN، رقم الحساب وبيانات التحويل | `VendBankAccount` |
| بنك الشركة | حساب الدفع والحساب المحاسبي المرتبط | `BankAccountTable` |
| المنتجات المفرج عنها | الصنف، الوحدة، مجموعات المخزون والضرائب | `EcoResProduct`, `InventTable`, `InventTableModule` |
| الموقع والمستودع | أبعاد استلام المخزون | `InventSite`, `InventLocation`, `InventDim` |
| فئات المشتريات | خدمات ومصروفات دون مخزون | `EcoResCategory` |
| الأسعار والخصومات | اتفاقيات أسعار المورد | `PriceDiscTable` |

الإعداد والبيانات الأساسية **لا ينشئان حركة مخزون أو قيدًا محاسبيًا** بمجرد إنشائهما.

## 3. Transactions — الدورة الكاملة

الجداول التالية هي **الجداول الرئيسية المتأثرة**؛ ليست قائمة بجميع الجداول الداخلية. تختلف التفاصيل بحسب مسار إدخال الفاتورة، الإعداد، الإصدار، والتخصيصات.

| المرحلة | الإجراء والنتيجة | الجداول الرئيسية المتأثرة | الأثر الفعلي على المخزون | الأثر المالي |
|---|---|---|---|---|
| 1. إنشاء أمر شراء | إدخال المورد والأصناف أو الخدمات والكميات والأسعار | `PurchTable`, `PurchLine`, `InventTransOrigin`, `InventTrans` للأصناف المخزنية | كمية متوقعة؛ لا يزيد المخزون الفعلي | لا قيد فعلي في الدورة الأساسية |
| 2. تأكيد أمر الشراء | تثبيت وإصدار التأكيد | `PurchTable`, `PurchLine`, `PurchConfirmJour`, `PurchConfirmTrans` | لا استلام | لا مديونية مورد |
| 3. استلام المنتج | استلام كلي أو جزئي | `VendPackingSlipJour`, `VendPackingSlipTrans`, `PurchLine`, `InventTrans` | يزيد المخزون المستلم للأصناف المخزنية | استحقاق واستلام غير مفوتر **إذا كانت إعدادات الترحيل مفعّلة** |
| 4. إدخال فاتورة معلّقة | تسجيل رقم فاتورة المورد وتحديد الأسطر والاستلامات | `VendInvoiceInfoTable`, `VendInvoiceInfoLine`, `VendInvoiceInfoSubLine` | لا استلام إضافي بمجرد الحفظ | لا قيد مرحّل |
| 5. مطابقة واعتماد | فحص السعر والكمية والضرائب والتفاوتات والموافقة | جداول الفاتورة المعلّقة، مستندات الشراء والاستلام، وسجل Workflow | لا تغيير | لا قيد بمجرد المطابقة أو الموافقة |
| 6. ترحيل فاتورة الشراء | تحديث التكلفة المالية وإثبات مديونية المورد | `VendInvoiceJour`, `VendInvoiceTrans`, `VendTrans`, `VendTransOpen`, `TaxTrans`, `InventTrans` | لا زيادة إضافية للكمية المستلمة سابقًا | إثبات المورد والضريبة والتكلفة، وعكس استحقاق الجزء المفوتر |
| 7. فاتورة دون PO | إثبات مصروف مباشر على المورد من يومية الفواتير | `LedgerJournalTable`, `LedgerJournalTrans`, `VendTrans`, `VendTransOpen`, `TaxTrans` | لا حركة مخزون | مدين مصروف/ضريبة، دائن مورد |
| 8. اقتراح الدفع | اختيار الفواتير المستحقة وإنشاء أسطر الدفع | `LedgerJournalTable`, `LedgerJournalTrans` | لا أثر | لا قيد حتى الترحيل |
| 9. توليد ملف/وسيلة الدفع | إعداد ملف البنك أو الشيك وتحديث حالة الدفع | يومية الدفع وجداول وسيلة الدفع | لا أثر | توليد الملف وحده لا يعني ترحيل الدفع |
| 10. ترحيل الدفع | تسجيل خروج النقد وتخفيض صافي مديونية المورد | `LedgerJournalTable`, `LedgerJournalTrans`, `VendTrans`, `VendTransOpen`, `BankAccountTrans` | لا أثر | مدين مورد، دائن بنك، وفق الحساب المقابل |
| 11. التسوية | ربط الدفع بالفاتورة كليًا أو جزئيًا | `VendTrans`, `VendTransOpen`, `VendSettlement` | لا أثر | قد تنشأ قيود خصم نقدي أو فرق صرف أو فروق تسوية |
| 12. المراجعة والإقفال | أعمار الديون، مطابقة المورد مع GL، مراجعة الاستلام غير المفوتر | جداول الموردين والتسويات وGL والاستلام والفواتير | لا تغيير بمجرد التقرير | إعادة تقييم العملة أو التصحيحات قد تنشئ قيودًا |

الاستلام والفاتورة مرحلتان منفصلتان: **الاستلام تحديث فعلي، والفاتورة تحديث مالي**. وترحيل الاستلام إلى الأستاذ العام يعتمد على إعدادات مثل `Post physical inventory` و`Accrue liability on product receipt` وترحيل استلام المنتج في GL. [GitHub](https://github.com/MicrosoftDocs/Dynamics-365-Unified-Operations-Public/blob/main/articles/finance/general-ledger/purchase-order-posting.md?utm_source=chatgpt.com)

**تنبيه تقني:** يومية الفواتير المباشرة ليست نفس مسار فاتورة أمر الشراء؛ لا تفترض أن كل فاتورة مورد لها أسطر في `VendInvoiceTrans`.

### جداول المحاسبة المشتركة

عند الترحيل المحاسبي، تتبع القيود من خلال:

| الغرض | الجداول |
|---|---|
| التوزيع المحاسبي للمستند | `AccountingDistribution` |
| قيد دفتر الأستاذ الفرعي، في المسارات التي تستخدمه | `SubledgerJournalEntry`, `SubledgerJournalAccountEntry` |
| رأس وأسطر القيد في GL | `GeneralJournalEntry`, `GeneralJournalAccountEntry` |
| ربط سند دفتر الأستاذ الفرعي بـGL | `SubledgerVoucherGeneralJournalEntry` |
| الضريبة المرحّلة | `TaxTrans` |

## 4. Physical vs. Financial — مثال كامل

**شراء 100 وحدة × 100 ريال، وضريبة افتراضية 15% قابلة للاسترداد بالكامل:**

- صافي الشراء: **10,000**
- الضريبة: **1,500**
- إجمالي المورد: **11,500**

هذا المثال يوضح **صافي الأثر المحاسبي**؛ السند الفعلي قد يحتوي حسابات وسيطة وأسطرًا إضافية حسب إعدادات المخزون والتكلفة والضرائب.

| المرحلة | المخزون | مدين | دائن | المديونية المفتوحة |
|---|---|---|---|---:|
| أمر الشراء والتأكيد | لم يتم الاستلام | — | — | 0 |
| استلام 100 وحدة | الفعلي يزيد 100 | استلام/مخزون غير مفوتر 10,000 | استحقاق مشتريات 10,000 | 0 في دفتر المورد |
| ترحيل الفاتورة | الكمية نفسها، والتكلفة تصبح مالية | مخزون 10,000 + ضريبة مدخلات 1,500، مع عكس قيد الاستلام | المورد 11,500، مع عكس الحسابات المقابلة للاستلام | 11,500 |
| دفع وتسوية 6,000 | لا تغيير | المورد 6,000 | البنك 6,000 | 5,500 |
| دفع وتسوية 5,500 | لا تغيير | المورد 5,500 | البنك 5,500 | 0 |

**استحقاق الاستلام في GL لا يساوي فاتورة مفتوحة على المورد.** عند الفوترة يُعكس الاستحقاق المرتبط بالكمية المفوترة وتُثبت المديونية على حساب المورد. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/pstg-prfles-ovrvw?utm_source=chatgpt.com)

### مثال الاستلام والفوترة الجزئية

لنفس أمر الشراء، 100 وحدة:

| الحدث | المستلم تراكميًا | المفوتر تراكميًا | النتيجة |
|---|---:|---:|---|
| استلام أول | 60 | 0 | 60 وحدة مستلمة غير مفوترة |
| فوترة أولى لـ40 وحدة | 60 | 40 | 20 وحدة مستلمة غير مفوترة، ومديونية 4,600 |
| استلام المتبقي | 100 | 40 | 60 وحدة مستلمة غير مفوترة |
| فوترة المتبقي | 100 | 100 | اكتمال الكمية والفوترة؛ إجمالي الفواتير 11,500 |

تُراجع الكميات المتبقية في `PurchLine`، والاستلامات في `VendPackingSlipTrans`، والفواتير في `VendInvoiceTrans`، والحركات المخزنية في `InventTrans`.

## 5. الحالات التي يجب تغطيتها

| الحالة | السلوك المطلوب التحقق منه | الجداول/الأثر الرئيسي |
|---|---|---|
| فاتورة خدمات دون مخزون | إثبات المصروف والضريبة والمورد دون كمية مخزنية | يومية الفاتورة أو مستند فاتورة المورد حسب المسار |
| فاتورة تشمل عدة استلامات | ربط الكميات المفوترة بالاستلامات الصحيحة | جداول الفاتورة المعلّقة، الاستلام، والفاتورة المرحّلة |
| اختلاف السعر أو الكمية | تطبيق سياسة المطابقة والتفاوتات والموافقة | مستندات PO والاستلام والفاتورة |
| خصم تجاري أو مصاريف شراء | احتساب الصافي وتوزيع المصاريف على المصروف أو تكلفة الصنف | `MarkupTrans` مع جداول المستند والترحيل |
| دفعة جزئية | إبقاء الرصيد المتبقي مفتوحًا | `VendTransOpen`, `VendSettlement` |
| دفع دون تسوية | بقاء الفاتورة والدفع مفتوحين حتى الربط | `VendTrans`, `VendTransOpen` |
| دفعة مقدمة | إثبات المقدم ثم تطبيقه على الفاتورة النهائية | إعداد المقدم، مستنداته، حركات المورد والتسوية |
| خصم نقدي | إغلاق الفاتورة بالدفع والخصم المستحق | `VendSettlement` وقيود الخصم |
| عملة أجنبية | إثبات الفاتورة والدفع بأسعارهما ومعالجة فرق الصرف | `VendTrans`, `VendSettlement` وGL |
| مرتجع شراء مخزني | إخراج الكمية ثم إثبات إشعار دائن المورد | `PurchTable`, `PurchLine`, `InventTrans`, جداول الاستلام/الفاتورة |
| إشعار دائن مالي فقط | تخفيض المديونية والمصروف/الضريبة دون إخراج مخزون | مستند الإشعار، `VendTrans`, `TaxTrans` وGL |
| إلغاء كمية غير مستلمة | إلغاء المتبقي دون عكس حركات مكتملة | `PurchLine` وحركات المخزون المرتبطة |
| تصحيح استلام | تصحيح الكمية والاستحقاق ضمن القيود المسموحة | جداول الاستلام، `InventTrans` وGL عند التطبيق |
| إلغاء تسوية | إعادة فتح الأرصدة ومعالجة القيود المرتبطة | `VendSettlement`, `VendTransOpen`, `VendTrans` |
| عكس دفعة | عكس الدفع وفق حالته وطريقة الدفع والمسار المدعوم | يوميات الدفع، البنك، المورد وGL |
| فاتورة مكررة أو مورد محظور | تطبيق الفحص والمنع بحسب الإعداد | `VendParameters`, `VendTable` ومستند الفاتورة |

توجد طريقتان مختلفتان للمقدم: **فاتورة دفعة مقدمة مرتبطة بأمر شراء**، و**دفعة مقدمة من يومية الدفع**. يجب اختبار كل مسار مستخدم بشكل مستقل. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/prepayments-invoices-vs-prepayments?utm_source=chatgpt.com)

التسوية أيضًا مرحلة مستقلة عن الدفع؛ يمكن تنفيذها أثناء الترحيل أو لاحقًا، وقد تنتج عنها قيود إضافية بحسب الخصم وفروق العملة والحسابات والأبعاد. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/cash-bank-management/settlement-overview?utm_source=chatgpt.com)

## 6. متى نعتبر دورة AP مكتملة؟

يجب أن تنجح هذه المطابقات بعد تنفيذ الحالات السابقة:

- **المورد:** الفواتير + الإشعارات الدائنة + الدفعات = الرصيد الصحيح.
- **المفتوح:** الأرصدة المتبقية تعكس التسويات الجزئية والكاملة.
- **GL:** مجموع دفتر الموردين يطابق حسابات مراقبة الموردين.
- **الاستلام غير المفوتر:** يبقى فقط للكمية المستلمة التي لم تُفوتر.
- **المخزون:** الاستلام يزيد الكمية، والفاتورة لا تكرر الزيادة، والمرتجع يخفضها.
- **البنك والضريبة:** كل مبلغ مرحّل قابل للتتبع إلى مستنده وسنده.
- **التصحيحات:** تحفظ السجل الأصلي وروابط العكس والتسوية.

**هذه خريطة الدورة القياسية ونطاق التحقق. تأكيد أن نظامك يغطيها بالكامل يحتاج فحص التنفيذ والبيانات ونتائج الترحيل لكل حالة.**




You are a senior ASP.NET Core ERP developer and Microsoft Dynamics 365 Finance & Operations functional and technical consultant.

Implement a complete **Accounts Payable seed-data package** in my existing ERP, covering configuration, master data, transactions, inventory effects, accounting entries, payments, settlement, and corrections.

Use the attached Accounts Payable document as the scope reference. Verify its technical details against official Microsoft documentation and available D365 metadata before relying on table names or behavior.

**Implement the code and run meaningful verification. Continue through all phases without stopping after each phase. Do not deliver only a plan, pseudocode, or isolated snippets.**

## 1. Scope and architecture

Focus on **basic core ERP**:

- Accounts Payable.
- Basic Purchasing and Inventory integration.
- General Ledger, Tax, and Bank integration.
- No WHS, production, manufacturing, or advanced procurement.
- No unrelated modules or unnecessary abstractions.

First inspect the existing entities, configurations, services, posting engines, validators, and seeders. Follow the existing architecture and module ownership.

Map standard D365 concepts to the application’s existing entities. **Do not create duplicate entities or copy D365 table structures merely to imitate D365.**

## 2. Configuration and master data

Seed the following prerequisites, reusing existing records where appropriate:

| Area | Example seed data | D365 reference tables to verify and map |
|---|---|---|
| Company | DEMO company, SAR accounting currency, open fiscal period | `CompanyInfo`, `Ledger`, `FiscalCalendar`, `FiscalCalendarYear`, `FiscalCalendarPeriod` |
| Accounts | Inventory, receipt accrual, AP control, input tax, expense, bank, prepayment, discount, exchange differences | `MainAccount`, `LedgerChartOfAccounts` |
| Vendor setup | LOCAL and FOREIGN groups; standard and prepayment posting profiles | `VendParameters`, `VendGroup`, `VendPosting` |
| Payment setup | NET30 terms, bank transfer method, optional 2% cash discount | `PaymTerm`, `VendPaymModeTable`, `CashDisc` |
| Journals | AP invoice journal and vendor payment journal | `LedgerJournalName` |
| Tax | Illustrative recoverable VAT 15% and zero-tax scenario | `TaxTable`, `TaxGroupHeading`, `TaxGroupData`, `TaxItemGroupHeading`, `TaxOnItem` |
| Vendors | V-LOCAL, V-SERVICE, V-USD | `VendTable`, `DirPartyTable` |
| Vendor details | Fictional addresses, contacts, and bank details | `DirPartyLocation`, `LogisticsPostalAddress`, `LogisticsElectronicAddress`, `VendBankAccount` |
| Company bank | BANK-SAR and optional BANK-USD | `BankAccountTable` |
| Products | ITEM-AP-001 and ITEM-AP-002, stocked purchased items | `EcoResProduct`, `InventTable`, `InventTableModule` |
| Inventory setup | Basic site MAIN, warehouse MAIN, valid model and item groups | `InventSite`, `InventLocation`, `InventModelGroup`, `InventItemGroup`, `InventPosting`, `InventParameters` |
| Services | SERVICE-MAINT procurement category or existing equivalent | `EcoResCategory` |
| Prices | ITEM-AP-001 purchase price SAR 100 | `PriceDiscTable` |
| Exchange rates | USD/SAR invoice rate 3.75 and payment rate 3.80 on separate dates | `Currency`, `ExchangeRateType`, `ExchangeRateCurrencyPair`, `ExchangeRate` |

Use these illustrative main accounts, adapting codes to the existing chart:

| Account | Purpose |
|---|---|
| 110100 | Company bank |
| 120100 | Recoverable input VAT |
| 130100 | Financial inventory |
| 130110 | Physical inventory received, not invoiced |
| 140100 | Vendor advances |
| 210100 | Vendor control |
| 210200 | Purchase receipt accrual |
| 510100 | Maintenance expense |
| 610100 | Realized exchange loss |
| 710100 | Realized exchange gain |
| 710200 | Purchase cash discount |

Include any additional intermediary accounts required by the existing posting engine. Do not collapse actual posting entries merely to match this illustrative chart.

Configuration and master-data creation must not create inventory quantities or financial transactions.

## 3. Required transaction traceability

For every scenario and every step, record:

- Scenario ID.
- Company and vendor.
- Source document and line references.
- Posted document and voucher references.
- Quantity, price, net amount, tax, and total.
- Ordered, received, invoiced, returned, and remaining quantities.
- Inventory physical and financial updates.
- Debit and credit entries.
- Vendor balance and open amounts.
- Payment and settlement references.
- Actual application tables/entities inserted, updated, or read.
- Relevant D365 reference tables.

Distinguish configuration tables read during processing from transaction tables actually changed.

## 4. Scenario AP-01 — Complete purchase-to-payment cycle

Use:

- Vendor: V-LOCAL.
- Item: ITEM-AP-001.
- Quantity: 100.
- Unit price: SAR 100.
- Net: SAR 10,000.
- VAT: SAR 1,500.
- Total: SAR 11,500.
- Opening inventory for this isolated scenario: zero.
- Receipt accrual enabled.
- No physical tax posting, charges, discounts, or price differences.

Execute:

### Step 1 — Create purchase order

Create PO-AP-001 for 100 units.

Expected:

- Ordered: 100.
- Received: 0.
- Invoiced: 0.
- Physical stock: 0.
- Vendor liability: 0.
- No GL posting.

Trace `PurchTable`, `PurchLine`, `InventTransOrigin`, and `InventTrans` or their application equivalents.

### Step 2 — Confirm purchase order

Confirm PO-AP-001.

Expected:

- Confirmation history exists.
- No stock receipt.
- No vendor liability.
- No GL posting in the basic configuration.

Verify the standard purchase confirmation history tables, including `VendPurchOrderJour` and `VendPurchOrderTrans`, against metadata. Do not reuse unverified table names from the reference document.

### Step 3 — Receive 100 units

Post PR-AP-001.

Expected:

- Physical stock: 100.
- Received but not invoiced: 100.
- Financially invoiced quantity: 0.
- No invoice balance in the vendor subledger.

Illustrative net accounting effect:

- Debit physical inventory received: SAR 10,000.
- Credit purchase receipt accrual: SAR 10,000.

Trace `VendPackingSlipJour`, `VendPackingSlipTrans`, `PurchLine`, `InventTrans`, inventory accounting records, and GL records.

Preserve all intermediate posting lines generated by the real engine.

### Step 4 — Create and approve pending invoice

Create INV-AP-001 and match it to PR-AP-001.

Expected:

- Quantity: 100.
- Net: SAR 10,000.
- VAT: SAR 1,500.
- Total: SAR 11,500.
- No additional inventory quantity.
- No posted vendor liability before invoice posting.

Trace `VendInvoiceInfoTable`, `VendInvoiceInfoLine`, `VendInvoiceInfoSubLine`, accounting distributions, and approval records where applicable.

### Step 5 — Post invoice

Post INV-AP-001.

Expected illustrative net entries:

- Debit purchase receipt accrual: SAR 10,000.
- Credit physical inventory received: SAR 10,000.
- Debit financial inventory: SAR 10,000.
- Debit input VAT: SAR 1,500.
- Credit vendor control: SAR 11,500.

Expected balances:

- Physical stock: 100.
- Financially updated quantity: 100.
- Inventory value: SAR 10,000.
- Receipt accrual for this purchase: zero.
- Vendor open invoice: SAR 11,500.

Trace `VendInvoiceJour`, `VendInvoiceTrans`, `VendTrans`, `VendTransOpen`, `TaxTrans`, `InventTrans`, and GL records.

### Step 6 — Post and settle first payment

Pay SAR 6,000 through BANK-SAR and settle it against INV-AP-001.

Entries:

- Debit vendor control: SAR 6,000.
- Credit bank: SAR 6,000.

Expected remaining invoice amount: SAR 5,500.

### Step 7 — Post and settle final payment

Pay and settle SAR 5,500.

Entries:

- Debit vendor control: SAR 5,500.
- Credit bank: SAR 5,500.

Final expected state:

- Invoice fully settled.
- Vendor open amount: zero.
- Total bank outflow: SAR 11,500.
- Inventory quantity: 100.
- Inventory value: SAR 10,000.
- Input VAT: SAR 1,500.
- Receipt accrual: zero.

Trace `LedgerJournalTable`, `LedgerJournalTrans`, `BankAccountTrans`, `VendTrans`, `VendTransOpen`, `VendSettlement`, and GL records.

## 5. Additional complete examples

Keep scenarios independent so their balances can be verified separately.

| ID | Scenario and input | Expected result |
|---|---|---|
| AP-02 | Order 100 × SAR 100; receive 60; invoice 40 | Stock 60; financially updated quantity 40; 20 received but uninvoiced; 40 unreceived; invoice SAR 4,600 |
| AP-02 continuation | Receive remaining 40; invoice remaining 60 | Total stock 100; fully invoiced; additional invoice SAR 6,900; total liability SAR 11,500 |
| AP-03 | Direct maintenance invoice SAR 2,000 + VAT 300 | Debit expense 2,000 and VAT 300; credit vendor 2,300; no inventory movement; then pay and settle |
| AP-04 | Invoice SAR 2,300 and separate payment SAR 2,300 without settlement | Vendor net zero, but both transactions remain open; settle afterward and verify closure |
| AP-05 | Vendor advance SAR 3,000 against final invoice SAR 11,500 | Advance applied once; remaining payable SAR 8,500 under the explicitly selected tax assumptions |
| AP-06 | Return 10 units from an invoiced purchase at SAR 100 + 15% VAT | Stock decreases 10; inventory value decreases 1,000 under this isolated no-variance example; vendor liability decreases 1,150 |
| AP-07 | Service credit note SAR 200 + VAT 30 | Debit vendor 230; credit expense 200 and input VAT 30; no stock movement |
| AP-08 | USD 1,000 service invoice, no tax; invoice rate 3.75, payment rate 3.80 | Invoice liability SAR 3,750; bank outflow SAR 3,800; realized exchange loss SAR 50; fully settled |
| AP-09 | Tax-free service invoice SAR 1,000; eligible 2% cash discount | Bank payment SAR 980; discount SAR 20; invoice fully settled |
| AP-10 | Order 100; receive and invoice 60; cancel the remaining 40 | Final received/invoiced quantity 60; no further receipt/invoice remainder; no reversal of completed 60 |
| AP-11 | Receive 60; correct receipt to 50 before invoicing using supported correction | Stock and accrual reflect 50; original receipt and correction remain traceable |
| AP-12 | Fully settle invoice and payment; undo settlement | Invoice and payment reopen correctly; no additional bank movement merely from undoing settlement |
| AP-13 | Reverse an eligible posted payment through the supported process | Bank and vendor effects reversed; settlement handled correctly; original and reversal linked |

For AP-05, implement the supported prepayment process explicitly. If both PO prepayment invoices and prepayment journal vouchers exist, demonstrate both separately, with their own tax and accounting expectations.

For every row, provide the same step-by-step detail as AP-01: documents, affected tables, quantities, entries, balances, and verification.

## 6. Impacted-table mapping

Verify and map these reference groups to the existing application:

| Process | D365 reference tables |
|---|---|
| Purchase order | `PurchTable`, `PurchLine` |
| Purchase confirmation | `VendPurchOrderJour`, `VendPurchOrderTrans` |
| Product receipt | `VendPackingSlipJour`, `VendPackingSlipTrans` |
| Pending invoice | `VendInvoiceInfoTable`, `VendInvoiceInfoLine`, `VendInvoiceInfoSubLine` |
| Posted PO invoice | `VendInvoiceJour`, `VendInvoiceTrans` |
| Invoice/payment journals | `LedgerJournalTable`, `LedgerJournalTrans` |
| Vendor transactions | `VendTrans`, `VendTransOpen` |
| Settlement | `VendSettlement`; verify settlement-marking tables where relevant |
| Bank transactions | `BankAccountTrans` |
| Inventory transactions | `InventTransOrigin`, `InventTrans`, `InventDim` |
| Charges | `MarkupTrans` |
| Posted tax | `TaxTrans` |
| Source-document accounting | `SourceDocumentHeader`, `SourceDocumentLine`, `AccountingDistribution` where used |
| Subledger accounting | `SubledgerJournalEntry`, `SubledgerJournalAccountEntry` where used |
| GL posting | `GeneralJournalEntry`, `GeneralJournalAccountEntry`, `SubledgerVoucherGeneralJournalEntry` |

Do not claim every table changes in every process. Identify the actual path used.

Verify relationships through metadata and application code. Do not join documents using invoice number alone or assume one inventory transaction per purchase line.

## 7. Implementation rules

- Seed configuration and master data using the established persistence pattern.
- Execute business transactions through actual application services.
- Never directly insert posted inventory, ledger, vendor balance, or settlement records to manufacture results.
- Never manually set a document to Posted or Settled.
- Use deterministic scenario references and dependency ordering.
- Separate required setup from optional demo transactions.
- Restrict demo transaction execution to explicitly enabled environments.
- Preserve tenant/company scope and existing business data.
- Support safe reruns, cancellation, logging, and concurrent-run protection.
- Resume incomplete scenarios from validated checkpoints.
- Never duplicate posted documents on rerun.
- Record successful completion only after the relevant operation commits.

If functionality is missing, implement necessary core behavior within the existing architecture where feasible. Otherwise report the precise blocker and continue independent scenarios. Never mark blocked work as passed.

## 8. Verification and deliverables

Run meaningful integration checks for:

- Correct document statuses and remaining quantities.
- Physical quantity versus financial inventory updates.
- Receipt accrual creation and clearing.
- Balanced accounting entries.
- Tax calculations.
- Vendor open balances and settlement allocations.
- Bank movements.
- Foreign-currency differences.
- Vendor-to-GL reconciliation.
- Safe correction and reversal behavior.
- Rerun idempotency and company isolation.
- Rejected duplicate invoices, blocked vendors, closed periods, invalid dimensions, missing accounts, and matching failures without partial posting.

Deliver:

1. Complete implemented seeders and supporting changes.
2. Exact execution commands.
3. Configuration and master-data records.
4. Every example’s document references.
5. A phase-by-phase impacted-table matrix identifying reads, inserts, and updates.
6. Expected versus actual quantities, accounting entries, and balances.
7. Verification results and rerun results.
8. Remaining blockers, if any.

Continue until all supported phases are implemented and verified. Do not stop after configuration or master data.



You are an expert in ASP.NET Core ERP development, database integrity, and Microsoft Dynamics 365 Finance & Operations core Accounts Payable.

Implement complete, realistic **Accounts Payable seed data** in my existing ERP. Cover configuration, master data, and end-to-end transactions, including their physical inventory and financial accounting effects.

**Do the implementation. Do not stop at a plan or sample snippets. Continue through all phases until the supported scope is complete and verified.**

### 1. Inspect the existing implementation first

- Inspect the repository architecture, entities, relationships, services, posting logic, validators, and existing seeders.
- Map the existing Accounts Payable functionality and its dependencies on Purchasing, Inventory, General Ledger, Tax, and Banking.
- Follow existing module boundaries, naming conventions, dependency injection, and seeding contracts.
- Reuse existing seed data and business services.
- Verify relevant standard D365 F&O behavior using official Microsoft documentation. Do not invent standard behavior, tables, fields, statuses, or enum mappings.
- Map D365 concepts to this ERP’s actual entities. Do not create D365-named tables merely to imitate its database.

Scope: **basic core ERP only**. Exclude WHS, production, manufacturing, advanced procurement, and country-specific features unless already required by the existing implementation.

### 2. Seed configuration

Provide all configuration required to run the examples:

- Legal entity, accounting currency, fiscal calendar, and open accounting periods.
- Chart of accounts and valid financial dimensions.
- Vendor groups and posting profiles.
- Accounts Payable parameters and number sequences.
- Payment terms, payment methods, and cash discounts.
- Invoice and payment journal names.
- Tax codes, tax groups, item tax groups, and tax posting accounts.
- Inventory posting profiles and item model groups.
- Purchase receipt accrual and financial inventory settings.
- Company bank accounts and payment clearing accounts where applicable.
- Invoice matching policies and tolerances supported by the application.
- Currency rates and realized/unrealized exchange difference accounts if supported.

Clearly distinguish required configuration from optional demo configuration.

### 3. Seed realistic master data

Create at least:

- Three vendors: a local inventory supplier, a local service supplier, and a foreign-currency supplier.
- Vendor addresses, contacts, payment defaults, tax details, and bank accounts.
- Two stocked purchased items with valid units, inventory groups, and tax configuration.
- One service or procurement category without inventory quantity tracking.
- One site and warehouse using basic inventory.
- Valid purchase prices and supported discounts.
- Relevant company bank accounts and financial dimensions.

Use fictional names, identifiers, and bank details. Preserve company and tenant boundaries.

### 4. Implement complete transaction scenarios

Create independent, clearly named scenarios. Use a fixed configurable scenario date and open fiscal period.

**Scenario A — Complete inventory purchase**

- Purchase 100 units at SAR 100 each.
- Net amount: SAR 10,000.
- Illustrative recoverable VAT at 15%: SAR 1,500.
- Invoice total: SAR 11,500.
- Confirm the purchase order.
- Receive all 100 units.
- Post the vendor invoice.
- Post and settle a payment of SAR 11,500.
- Expected final state: 100 units received and financially updated; invoice fully settled; vendor open balance zero; receipt accrual cleared for the invoiced quantity.

**Scenario B — Partial receipt and partial invoicing**

- Purchase 100 units at SAR 100 each.
- Receive 60 units.
- Invoice 40 units: SAR 4,000 net + SAR 600 VAT = SAR 4,600.
- At this checkpoint: 20 units received but not invoiced; 40 units not received.
- Receive the remaining 40 units.
- Invoice the remaining 60 units: SAR 6,000 net + SAR 900 VAT = SAR 6,900.
- Final invoice total across both invoices: SAR 11,500.
- Verify remaining receipt and invoice quantities after every step.

**Scenario C — Service invoice without a purchase order**

- Post a service expense of SAR 2,000 plus SAR 300 VAT.
- Vendor liability: SAR 2,300.
- Verify expense and tax postings with no inventory quantity movement.
- Pay and settle the invoice.

**Scenario D — Partial payment**

- Post an invoice for SAR 11,500.
- Pay and settle SAR 6,000.
- Verify the remaining invoice balance is SAR 5,500.
- Pay and settle the remainder.
- Verify the invoice closes without duplicate settlement.

**Scenario E — Payment before settlement**

- Post a vendor invoice and a separate payment.
- Initially leave them unsettled.
- Verify both open transactions remain visible even if the vendor’s net balance is zero.
- Settle them afterward and verify closure.

**Scenario F — Vendor prepayment**

- Use a SAR 3,000 advance.
- Implement the supported prepayment journal and/or purchase-order prepayment invoice process.
- Keep the two processes distinct.
- Apply the advance to the final invoice.
- Verify the prepayment account, vendor balance, remaining payable amount, and any applicable tax treatment.
- Derive expected tax entries from the configured process; do not assume all prepayment methods have identical tax treatment.

**Scenario G — Purchase return and vendor credit note**

- Start from a received and invoiced purchase.
- Return 10 units at SAR 100.
- Post a credit note for SAR 1,000 plus SAR 150 VAT.
- Verify inventory decreases by 10 units and vendor liability decreases by SAR 1,150.
- Settle the credit against an open invoice or process the supported vendor refund path.

**Scenario H — Financial credit note without an inventory return**

- Post an allowance of SAR 200 plus SAR 30 VAT against an expense invoice.
- Verify vendor liability decreases by SAR 230.
- Verify there is no inventory quantity movement.

**Scenario I — Cash discount**

- Configure a supported 2% cash discount.
- Post and pay an eligible invoice within the discount period.
- Calculate expected discount and tax adjustments using the application’s actual configuration.
- Verify payment, discount posting, settlement, and final open balance.

**Scenario J — Foreign-currency invoice and payment**

- Use a USD 1,000 service invoice without tax to isolate exchange effects.
- Invoice rate: SAR 3.75 per USD.
- Payment rate: SAR 3.80 per USD.
- Expected invoice accounting value: SAR 3,750.
- Expected bank payment value: SAR 3,800.
- Expected realized exchange loss on full settlement: SAR 50.
- Verify transaction-currency and accounting-currency balances and postings.

**Scenario K — Corrections and remaining quantities**

- Cancel an unreceived purchase quantity.
- Correct a product receipt through the supported process.
- Reverse a payment through the supported process.
- Undo a settlement and verify reopened balances.
- Preserve original documents and audit links.

**Scenario L — Validation failures**

Verify that these attempts fail without leaving partial documents or postings:

- Duplicate invoice where configured to block duplicates.
- Blocked vendor transaction.
- Missing posting account.
- Closed accounting period.
- Invalid tax or financial dimension configuration.
- Quantity or price mismatch beyond configured tolerance.
- Invalid overpayment or settlement allocation according to configured rules.

Run failure scenarios as isolated verification cases rather than leaving corrupted demo data.

### 5. Use actual business processes

- Seed configuration and master data through the existing supported persistence pattern.
- Create, confirm, receive, invoice, pay, return, reverse, and settle transactions through existing application/domain services.
- **Never directly insert posted ledger entries, inventory balances, vendor balances, open transactions, or settlement records to imitate successful processing.**
- Do not manually assign posted statuses.
- Generate posted results through the same logic used by the application.
- Reuse shared inventory, tax, accounting, and settlement services.
- Do not add fake fallback logic when a required business process is missing.

If a required core capability is missing, implement it within the existing architecture when reasonably within scope. Otherwise identify the precise blocker, mark the scenario incomplete, and continue all independent work. Never claim an unsupported scenario passed.

### 6. Make seeding safe and repeatable

- Separate required/reference seeders from demo transaction seeders.
- Register seeders through dependency injection.
- Use deterministic dependency ordering and the existing stable ID/version convention.
- Keep demo transactions explicitly enabled for development/test environments.
- Preserve existing values, IDs, and company/tenant scope.
- Use stable scenario identifiers and document references to prevent duplicates.
- Make reruns resume safely from validated checkpoints.
- Do not repost an already posted document or recreate a completed payment.
- Use transaction boundaries appropriate to each business operation.
- Record success only after the relevant operation commits.
- Support cancellation, structured logging, timing, and actionable errors.
- Protect against concurrent runs using the project’s established mechanism.
- Do not delete existing business data to make a rerun succeed.

### 7. Verify physical and financial results

For each scenario, verify:

- Document status and links between source and posted documents.
- Ordered, received, invoiced, returned, canceled, and remaining quantities.
- Physical inventory quantity and financial inventory cost.
- Receipt accrual and its reversal for invoiced quantities.
- Vendor transactions, open amounts, payments, and settlements.
- Tax bases and posted tax.
- Bank movements.
- Balanced accounting entries in accounting currency.
- Correct transaction-currency amounts and exchange differences.
- Vendor subledger reconciliation to the relevant GL control accounts.
- Company and tenant isolation.
- Rerunning seeders creates no duplicate documents or postings.

Distinguish **physical inventory updates**, **financial inventory updates**, and **GL postings**. A product receipt can post to GL when configured, and invoicing an already received quantity must not receive it again.

Add meaningful integration tests using the actual posting services. Do not use mocked posting results as proof of accounting correctness.

### 8. Deliver the completed implementation

Provide:

1. Implemented seeders and required supporting changes.
2. Exact commands and configuration needed to run them.
3. A scenario matrix showing completed, verified, and blocked cases.
4. Seeded document references and expected amounts.
5. A mapping of each phase to the actual application tables/entities affected.
6. Physical inventory and financial accounting effects for every scenario.
7. Verification results, including rerun/idempotency checks.
8. Remaining gaps with precise reasons.

Proceed through **inspection → configuration → master data → transactions → posting → settlement → verification → rerun checks** without pausing after each phase. Complete all independent work before reporting the final result.