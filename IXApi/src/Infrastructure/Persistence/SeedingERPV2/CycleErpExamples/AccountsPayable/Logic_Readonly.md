**بالنسبة إلى Accounts payable، نحتاج تغطية دورة Purchase to Pay كاملة:** الإعدادات ← بيانات المورد والأصناف ← أمر الشراء ← الاعتماد والتأكيد ← الاستلام ← مطابقة فاتورة المورد ← ترحيل الفاتورة ← الدفع ← التسوية ← المرتجعات والتصحيحات ← المطابقة والإقفال.

بنفس نطاقك: **D365 F&O، شراء مخزني وخدمي، Core ERP، دون WHS أو إنتاج أو تصنيع، وتكلفة المخزون Weighted average.**

سأستخدم مثالًا مترابطًا، ثم أوضح الجداول والحقول، وبعدها اختبارات **خدمةً خدمة**. النتائج المذكورة **متوقعة للاختبار**؛ لا تُعدّ Passed قبل تنفيذها على بيئتك ومراجعة أدلتها.

---

**1. افصل بين حالة أمر الشراء، والمخزون، ومديونية المورد**

| العنصر | ماذا يعني؟ | مثال |
|---|---|---|
| Purchase order status | اكتمال الاستلام والفوترة | Open order / Received / Invoiced |
| Approval status | اعتماد أمر الشراء وإدارته | Draft / In review / Approved / Confirmed، بحسب الإعداد |
| Inventory transaction status | مرحلة حركة المخزون | Ordered / Registered / Received / Purchased |
| Vendor invoice status | هل الفاتورة مسودة، معتمدة، أو مرحلة؟ | Pending / Workflow / Posted |
| Vendor settlement status | هل المبلغ مستحق أو مدفوع جزئيًا أو بالكامل؟ | Open / Partially settled / Closed |

**أمر شراء حالته Invoiced لا يعني أن المورد استلم مستحقاته.** قد تكون البضاعة مستلمة والفاتورة مرحلة بالكامل، لكنها لم تُدفع.

كذلك:

- **Product receipt:** استلام البضاعة أو إثبات تنفيذ الخدمة.
- **Vendor invoice:** إثبات فاتورة المورد والتزام الدفع عليه.
- **Payment:** تسجيل الدفع للمورد.
- **Settlement:** تخصيص الدفع للفواتير.

---

**2. خريطة الدورة الكاملة**

| المرحلة | العملية | الأثر الفعلي على المخزون | أثر المورد | الأثر المالي |
|---|---|---|---|---|
| 0 | تحديد السياسات | لا يوجد | لا يوجد | تحديد المعالجة |
| 1 | Configuration | لا يوجد | لا يوجد | إعداد قواعد الترحيل |
| 2 | Master data | لا يوجد | لا يوجد | إعداد القيم الافتراضية |
| 3 | أرصدة البداية | حسب مستند الافتتاح | التزام افتتاحي عند وجوده | قيد افتتاح |
| 4 | Purchase requisition — اختياري | لا يوجد | لا فاتورة | قد توجد رقابة موازنة عند تفعيلها |
| 5 | RFQ — اختياري | لا يوجد | لا فاتورة | لا قيد شراء اعتيادي |
| 6 | إنشاء Purchase order | كمية متوقع استلامها | لا مديونية فاتورة | لا قيد اعتيادي |
| 7 | اعتماد وتأكيد PO | لا زيادة فعلية | لا مديونية فاتورة | التزامات موازنة عند تفعيلها |
| 8 | Registration — اختياري | تسجيل وصول الكمية | لا فاتورة | لا يعادل ترحيل الاستلام |
| 9 | Product receipt | زيادة مخزون البضاعة | لا فاتورة في المسار الأساسي | مخزون/مصروف مستلم مقابل استحقاق |
| 10 | Pending vendor invoice | لا زيادة جديدة | مسودة غير مرحلة | لا قيد نهائي في المسار المباشر |
| 11 | Invoice matching / Approval | لا يوجد | تحقق واعتماد | لا قيد نهائي بمجرد المطابقة |
| 12 | Invoice posting | تحديث مالي للمخزون | إثبات الالتزام | مخزون/مصروف وضريبة مقابل المورد |
| 13 | Vendor payment | لا يوجد | تخفيض صافي المستحق | مورد مقابل بنك |
| 14 | Settlement | لا يوجد | ربط الدفع بالفاتورة | فروق إضافية عند وجودها |
| 15 | Returns / Credit notes | حسب نوع التصحيح | تخفيض المستحق أو نشوء رصيد لنا | عكس شراء/ضريبة |
| 16 | Revaluation / Reconciliation / Close | تقييم وتسويات تكلفة | تقييم ومطابقة | فروق عملة وتكلفة وإقفال |

**استثناء مهم:** مسار **Invoice register / Invoice pool** قد ينشئ أثرًا ماليًا مبكرًا؛ لا تخلطه مع مجرد حفظ Pending vendor invoice. سنفصله لاحقًا. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/tasks/key-invoice-data-into-ap-system-invoice-pool?utm_source=chatgpt.com)

---

**3. المرحلة 0 — السياسات التي سنبني عليها المثال**

| القرار | اختيار المثال |
|---|---|
| العملة المحاسبية | SAR |
| عملة الشراء والدفع | SAR |
| تكلفة المخزون | Weighted average الدوري |
| الشراء المخزني | Stocked product |
| الخدمة | Service + Not stocked |
| المستودع | مستودع أساسي دون WHS |
| الاستلام الجزئي | مسموح |
| الفوترة الجزئية | مسموحة |
| مطابقة البضاعة | Three-way matching |
| مطابقة الخدمة | حسب السياسة؛ نفترض قبول تنفيذها قبل الفوترة |
| استحقاق الاستلام | مفعّل |
| ضريبة المثال | 15% كافتراض حسابي |
| ضريبة المدخلات | نفترض قابلة للاسترداد بالكامل |
| السعر شامل الضريبة | لا |
| شروط الدفع | NET30 |
| التسوية | يدوية لتوضيح التخصيص |
| الخصومات والمصاريف | لا توجد في المثال الأساسي |
| Budget control / Encumbrance | غير مفعّلة في المثال |

**قابلية استرداد الضريبة مهمة:** الضريبة غير القابلة للاسترداد قد تدخل في تكلفة الأصل أو المصروف حسب الإعداد. المثال يفصلها في حساب ضريبة مدخلات.

---

**4. المرحلة 1 — Configuration**

الجداول التالية هي **الجداول الرئيسية للتتبع**، وليست حصرًا لكل جداول النظام. بعض التفاصيل تتغير حسب الإصدار والتوطين والميزات؛ راجع Metadata في بيئتك عند بناء الاستعلامات أو التكامل.

**4.1 الإعدادات المالية العامة**

| الإعداد | ماذا تفحص؟ | الجداول الأساسية |
|---|---|---|
| Legal entity / Ledger | الشركة والعملة المحاسبية وعملة التقارير | `CompanyInfo`, `Ledger` |
| Chart of accounts | حساب المورد والبنك والمخزون والمصاريف والضريبة | `MainAccount` |
| Fiscal calendar | السنة والفترات | `FiscalCalendar`, `FiscalCalendarYear`, `FiscalCalendarPeriod` |
| Ledger calendar | الفترة متاحة لترحيل AP والمخزون | `LedgerFiscalCalendarPeriod` |
| Account structures | الحسابات والأبعاد الإلزامية | `DimensionHierarchy` والجداول المرتبطة |
| Financial dimensions | Business unit / Department وغيرها | `DimensionAttribute`, `DimensionAttributeValue`, `DimensionAttributeValueSet`, `DimensionAttributeValueCombination` |
| Exchange rates | النوع والزوج والسعر والتاريخ | `Currency`, `ExchangeRateType`, `ExchangeRateCurrencyPair`, `ExchangeRate` |
| Bank accounts | الحساب البنكي وعملته وحسابه المالي | `BankAccountTable` |

**4.2 إعدادات Accounts payable**

| الشاشة/الإعداد | ما تضبطه؟ | الجداول الأساسية |
|---|---|---|
| Accounts payable parameters | ملف الترحيل، التحقق، المطابقة، التسوية، الفواتير | `VendParameters` |
| Vendor groups | تصنيف الموردين والقيم الافتراضية | `VendGroup` |
| Vendor posting profiles | حساب المورد العادي وحساب المقدّمات | `VendLedger`, `VendLedgerAccounts` |
| Terms of payment | NET30 وتاريخ الاستحقاق | `PaymTerm` |
| Payment schedules | أقساط المورد عند استخدامها | `PaymSched`, `PaymSchedLine` |
| Methods of payment | تحويل/شيك، بنك، صيغة الدفع، حساب وسيط | `VendPaymModeTable` |
| Cash discounts | قواعد خصم السداد | `CashDisc` |
| Journal names | Invoice journal / Payment journal وغيرها | `LedgerJournalName` |
| Number sequences | المورد وPO والسندات الداخلية | `NumberSequenceTable`, `NumberSequenceReference` والجداول المساندة |
| Vendor invoice workflow | الاعتماد والصلاحيات | إعدادات Workflow وجداول تاريخ التنفيذ |
| Invoice validation | رقم فاتورة مكرر، تواريخ، إجماليات | إعدادات AP وسياسات التحقق |
| Charges | شحن وتأمين ورسوم ومعالجتها | `MarkupTable`, `MarkupAutoTable`, `MarkupAutoLine` |

**رقم فاتورة المورد الخارجي** مثل `SUP-INV-001` يختلف عن **رقم Voucher الداخلي**. افحص الاثنين.

يحدد Vendor posting profile حساب الالتزام، ويمكن تخصيصه لمورد أو مجموعة أو جميع الموردين، مع ملف مستقل للدفعات المقدمة. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/vendor-posting-profiles?utm_source=chatgpt.com)

**4.3 إعدادات المخزون والمشتريات**

| الإعداد | المطلوب | الجداول الأساسية |
|---|---|---|
| Procurement parameters | التأكيد وإدارة التغييرات وسياسات الشراء | `PurchParameters` |
| Item model group | Weighted average، Stocked، الترحيل الفعلي والمالي، استحقاق الاستلام | `InventModelGroup`, `InventModelGroupItem` |
| Item group | تحديد حسابات الصنف | `InventItemGroup`, `InventItemGroupItem` |
| Inventory posting | حسابات الاستلام والفاتورة والاستحقاق والمصاريف | `InventPosting` |
| Inventory parameters | إعدادات الترحيل المخزني | `InventParameters` |
| Site / Warehouse | S1 / MAIN | `InventSite`, `InventLocation` |
| Units / Conversions | وحدة الشراء ووحدة المخزون | `UnitOfMeasure`, `UnitOfMeasureConversion` |
| Trade agreements | أسعار المورد والخصومات | `PriceDiscTable` |
| Procurement categories | خدمات ومشتريات غير مخزنية حسب الفئة | `EcoResCategory`, `EcoResCategoryHierarchy` والجداول المرتبطة |

أنواع الترحيل التي تراجعها في Purchase order posting تشمل:

- `Cost of purchased materials received`
- `Cost of purchased materials invoiced`
- `Purchase expenditure, un-invoiced`
- `Purchase, accrual`
- `Purchase expenditure for product`
- `Purchase expenditure for expense`
- `Purchase, stock variation` عند استخدام المعالجة ذات الصلة.

**لا تفترض أن Voucher الاستلام يتكون دائمًا من سطرين.** قد توجد سطور وسيطة وتوزيعات بحسب إعدادات الترحيل. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/post-to-charge-account-accounting-principle?utm_source=chatgpt.com)

**4.4 إعدادات المطابقة**

| السياسة | ماذا تقارن؟ |
|---|---|
| Two-way matching | سعر الفاتورة مقابل أمر الشراء |
| Three-way matching | السعر مقابل PO، والكمية مقابل Product receipt |
| Invoice totals matching | إجماليات الفاتورة |
| Charges matching | المصاريف على الأمر والفاتورة |
| Tolerances | مقدار الفرق المقبول |

اضبط أيضًا **ما يحدث عند تجاوز الفرق**: منع الترحيل أو طلب اعتماد أو السماح وفق الصلاحية والإعداد. ظهور Matching discrepancy وحده لا يضمن أن النظام سيمنع الترحيل. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/accounts-payable-invoice-matching?utm_source=chatgpt.com)

---

**5. المرحلة 2 — Master data**

**5.1 المورد**

| الخاصية | القيمة |
|---|---|
| Vendor account | V-1001 |
| الاسم | شركة التوريد |
| Vendor group | LOCAL |
| Currency | SAR |
| Terms of payment | NET30 |
| Method of payment | BANK |
| Posting profile | GEN |
| Invoice account | V-1001 |
| Tax group | DOMESTIC |
| Default dimensions | BU = Trading |
| حساب المورد البنكي | الحساب المعتمد للدفع |

الجداول والحقول:

| الجدول | الحقول المهمة |
|---|---|
| `VendTable` | `AccountNum`, `Party`, `VendGroup`, `Currency`, `PaymTermId`, `PaymMode`, `InvoiceAccount`, `TaxGroup`, `Blocked`, `DefaultDimension` |
| `DirPartyTable` | `RecId`, `Name` |
| `DirPartyLocation` | `Party`, `Location` |
| `LogisticsPostalAddress` | العنوان وفترة صلاحيته |
| `LogisticsElectronicAddress` | البريد والهاتف |
| `VendBankAccount` | حساب المورد وIBAN وبيانات التحويل |

**اختبار مهم:** مورد الطلب قد يختلف عن Invoice account. المديونية والدفع يجب أن يتبعا الحساب الصحيح.

**5.2 البضاعة والخدمة**

| الخاصية | البضاعة | الخدمة |
|---|---|---|
| Item ID | ITEM-100 | SERV-100 |
| الوصف | قطعة مخزنية | خدمة صيانة |
| Product type | Item | Service |
| Stocked product | Yes | No |
| وحدة الشراء | ea | ea |
| الكمية | 10 | 1 |
| سعر الشراء | 100 | 200 |
| المعالجة | مخزون | مصروف صيانة |
| حركة InventTrans | نعم | لا |
| أثر On-hand | نعم | لا |

الجداول:

- `EcoResProduct`, `EcoResProductTranslation`.
- `InventTable`, `InventTableModule`.
- `InventModelGroupItem`, `InventItemGroupItem`.
- جداول وحدات القياس والأبعاد.

**Service لا تعني تلقائيًا Not stocked.** نختار للخدمة مجموعة نموذج صنف غير مخزنية. ويمكن للخدمة غير المخزنية أن تولّد **استحقاق مصروف عند الاستلام** رغم عدم وجود حركة مخزون. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/inventory-costing-faq?utm_source=chatgpt.com)

---

**6. المثال الكامل**

نبدأ بـ:

- رصيد مخزون `ITEM-100` = صفر.
- رصيد المورد = صفر.
- لا توجد حركات أخرى تؤثر على المثال.

أنشئ أمر شراء `PO-1001`:

| السطر | البيان | الكمية | السعر | الصافي | الضريبة | الإجمالي |
|---|---|---:|---:|---:|---:|---:|
| 1 | ITEM-100 | 10 | 100 | 1,000 | 150 | 1,150 |
| 2 | SERV-100 | 1 | 200 | 200 | 30 | 230 |
| **المجموع** | | | | **1,200** | **180** | **1,380** |

سننفذ:

1. استلام وفوترة 6 قطع.
2. استلام 4 قطع وإثبات تنفيذ الخدمة ثم فوترتهما.
3. دفع 700 وتسويته.
4. دفع 680 وتسويته.
5. المطابقة والإقفال.

---

**7. المرحلتان الاختياريتان — Purchase requisition وRFQ**

| العملية | الغرض | الجداول الرئيسية | الأثر |
|---|---|---|---|
| Purchase requisition | طلب احتياج واعتماده | `PurchReqTable`, `PurchReqLine` | لا استلام أو فاتورة بمجرد الطلب |
| Request for quotation | مقارنة عروض الموردين | `PurchRFQCaseTable`, `PurchRFQCaseLine` وجداول عروض الموردين | لا مخزون أو مديونية فاتورة |
| تحويل إلى PO | إنشاء التزام شراء تشغيلي | `PurchTable`, `PurchLine` | بدء دورة أمر الشراء |

**ما تفحصه:** انتقال الصنف والكمية والوحدة والسعر والأبعاد والمورد المختار، وعدم إنشاء أوامر مكررة لنفس الكمية المعتمدة.

عند تفعيل Budget control أو Encumbrance، تختبر أثر الموازنة أيضًا. هذه الإعدادات خارج المثال المحاسبي الأساسي.

---

**8. إنشاء أمر الشراء**

المسار المعتاد:

`Procurement and sourcing > Purchase orders > All purchase orders`

**8.1 رأس الأمر — PurchTable**

| الحقل | القيمة/التحقق |
|---|---|
| `PurchId` | PO-1001 |
| `OrderAccount` | V-1001 |
| `InvoiceAccount` | V-1001 |
| `CurrencyCode` | SAR |
| `Payment` | NET30 |
| `PaymMode` | BANK |
| `PostingProfile` | GEN |
| `TaxGroup` | DOMESTIC |
| `PurchStatus` | Open order |
| `DocumentState` | بحسب دورة الاعتماد |
| `DefaultDimension` | BU = Trading |

**8.2 سطور الأمر — PurchLine**

| الحقل | البضاعة | الخدمة |
|---|---|---|
| `PurchId` | PO-1001 | PO-1001 |
| `ItemId` | ITEM-100 | SERV-100 |
| `PurchQty` | 10 | 1 |
| `PurchUnit` | ea | ea |
| `PurchPrice` | 100 | 200 |
| `PriceUnit` | 1 | 1 |
| `LineAmount` | 1,000 | 200 |
| `TaxItemGroup` | VAT15 | VAT15 |
| `InventDimId` | S1 / MAIN | لا يمثل رصيدًا للخدمة |
| `RemainPurchPhysical` | 10 | 1 قبل قبول الخدمة |
| `RemainPurchFinancial` | 10 | 1 |
| `DefaultDimension` | أبعاد البضاعة | أبعاد مصروف الخدمة |

**النتيجة:**

- ينشأ طلب توريد مخزني للبضاعة.
- المخزون الفعلي يبقى صفرًا.
- لا تنشأ فاتورة في `VendTrans`.
- لا قيد اعتيادي في GL.
- الخدمة غير المخزنية لا تنشئ `InventTrans`.

**اختبار السعر:** افحص `PriceUnit`، لأن سعر 100 لكل 10 وحدات يختلف عن 100 لكل وحدة.

---

**9. اعتماد أمر الشراء وتأكيده**

عند تفعيل Change management:

1. إعداد الأمر.
2. Submit.
3. اعتماد Workflow.
4. Confirm.

الجداول الأساسية:

| الجدول | الغرض |
|---|---|
| `PurchTable`, `PurchLine` | الأمر الحالي وحالته |
| `PurchTableVersion`, `PurchLineVersion` | نسخ الأمر حسب المسار |
| `VendPurchOrderJour` | سجل تأكيد أمر الشراء |
| `VendPurchOrderTrans` | سطور التأكيد |
| جداول Workflow | تاريخ الإرسال والاعتماد والرفض |

**النتيجة المطلوبة:**

- الأمر معتمد ومؤكد.
- يمكن مراجعة نسخة التأكيد.
- لا يوجد استلام مخزني.
- لا توجد مديونية فاتورة.
- تغيير السعر أو الكمية بعد الاعتماد يخضع لمسار التعديل المطلوب.

---

**10. Registration — عند استخدام استلام على خطوتين**

يمكن تسجيل وصول 6 قطع قبل Product receipt.

| العنصر | الأثر |
|---|---|
| `InventTrans` | حالة Registered للكمية |
| `InventDim` | المستودع والدفعة/السيريال عند الحاجة |
| On-hand | تظهر الكمية ضمن كميات التسجيل |
| المورد | لا فاتورة |
| GL | لا يعادل قيد Product receipt |

**لا تجمع Registered ثم Received لنفس الكمية على أنها استلامان.** عند Product receipt تنتقل الكمية إلى المرحلة التالية.

في المثال الأساسي سنستخدم **Product receipt مباشرة** لتبسيط الأرقام.

---

**11. Product receipt الأول — استلام 6 قطع**

رحّل `PR-1001` لكمية 6.

**11.1 الجداول والحقول**

| الجدول | ما تراجعه؟ |
|---|---|
| `VendPackingSlipJour` | `PurchId`, `PackingSlipId`, `DeliveryDate` |
| `VendPackingSlipTrans` | `ItemId`, `Qty = 6`, `InventTransId`, `InventDimId` |
| `PurchLine` | `RemainPurchPhysical = 4`, `RemainPurchFinancial = 10` |
| `InventTrans` | `Qty = +6`, `StatusReceipt = Received`, `DatePhysical`, التكلفة الفعلية |
| `InventSum` | كمية المخزون وتجميعاتها |
| جداول GL | قيد الاستلام حسب الإعداد |

**اسم الجدول VendPackingSlip… رغم أن الشاشة اسمها Product receipt.**

حالة الاستلام الفعلي هي `Received`، وعند فوترة الشراء تصبح `Purchased`. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/physical-financial-updates?utm_source=chatgpt.com)

**11.2 أثر الكميات**

| البيان | النتيجة |
|---|---:|
| المستلم | 6 |
| الموجود فعليًا | 6 |
| المتبقي للاستلام | 4 |
| المفوتر | 0 |
| قيمة الاستلام المقدرة | 600 |

**11.3 الأثر المحاسبي**

اقتصاديًا نريد إثبات:

| المعنى المحاسبي المجمع | مدين | دائن |
|---|---:|---:|
| مخزون مستلم غير مفوتر | 600 | — |
| استحقاق مشتريات / GRNI | — | 600 |

**هذا عرض مجمع لفهم الأثر، وليس افتراضًا لعدد سطور Voucher.** قد تظهر حسابات المخزون والمصروف غير المفوتر والاستحقاق وحسابات وسيطة إضافية.

الفكرة الأساسية: عند الفوترة يُعكس الاستحقاق المرتبط بالكمية، ويُثبت الالتزام على حساب المورد. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/pstg-prfles-ovrvw?utm_source=chatgpt.com)

**في المسار المباشر لا تنشأ هنا فاتورة Vendor مفتوحة بقيمة 690.** الالتزام في GL هو استحقاق استلام، وليس بعدُ فاتورة المورد المسجلة في `VendTrans`.

---

**12. إنشاء Pending vendor invoice للـ6 قطع**

أدخل:

| البيان | القيمة |
|---|---|
| Vendor invoice number | SUP-INV-001 |
| PO | PO-1001 |
| Product receipt | PR-1001 |
| Quantity | 6 |
| Unit price | 100 |
| Net amount | 600 |
| Tax | 90 |
| Total | 690 |

الجداول:

| الجدول | دوره |
|---|---|
| `VendInvoiceInfoTable` | رأس الفاتورة المعلقة |
| `VendInvoiceInfoLine` | السطور |
| `VendInvoiceInfoSubLine` | ربط السطر باستلامات المنتجات |
| `MarkupTrans` | مصاريف الفاتورة عند وجودها |
| `AccountingDistribution` | توزيع المبالغ محاسبيًا حسب المسار |

يربط `VendInvoiceInfoSubLine` سطور الفاتورة بسطور Product receipt؛ وهذا مهم للفوترة الجزئية ومنع إعادة استخدام الكميات. [Microsoft Learn](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountspayable/transactionline/vendinvoiceinfosubline?utm_source=chatgpt.com)

**مجرد حفظ Pending invoice لا يعني أن مديونية المورد رُحّلت.**

---

**13. المطابقة والاعتماد قبل ترحيل الفاتورة**

في المثال:

| المقارنة | PO | Product receipt | Invoice | النتيجة |
|---|---:|---:|---:|---|
| سعر الوحدة | 100 | — | 100 | مطابق |
| كمية الفاتورة | 10 مطلوبة | 6 مستلمة | 6 | مطابق |
| صافي المفوتر | — | — | 600 | صحيح |
| الضريبة | حسب الإعداد | — | 90 | صحيح |
| الإجمالي | — | — | 690 | صحيح |

**اختبارات الفشل الأساسية:**

- فاتورة 7 قطع مقابل استلام 6.
- سعر 110 مقابل سعر أمر 100، خارج التفاوت المسموح.
- رقم فاتورة مكرر.
- مصروف شحن غير مطابق.
- مبلغ ضريبة أو إجمالي غير صحيح.
- فاتورة بلا اعتماد عند إلزام Workflow.

في الفوترة الجزئية يمكن اختيار Product receipt أو كميات محددة منه. لا تفترض أن كل كمية PO أصبحت قابلة للفوترة لمجرد إنشاء الفاتورة. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/vendor-invoices-overview?utm_source=chatgpt.com)

---

**14. ترحيل الفاتورة الأولى — 690**

**14.1 الجداول المتأثرة**

| الجدول | الأثر |
|---|---|
| `VendInvoiceJour` | سجل الفاتورة المرحلة |
| `VendInvoiceTrans` | السطور المرحلة |
| `VendTrans` | التزام على المورد |
| `VendTransOpen` | المبلغ المفتوح |
| `TaxTrans` | ضريبة المدخلات |
| `InventTrans` | تحديث مالي للـ6 قطع |
| `PurchLine` | تحديث المتبقي للفوترة |
| جداول GL | قيد الفاتورة وعكس الاستلام المرتبط |

**14.2 أهم حقول الفاتورة**

| الجدول | الحقول المهمة |
|---|---|
| `VendInvoiceJour` | `InvoiceId`, `InvoiceDate`, `InvoiceAccount`, `CurrencyCode`, `InvoiceAmount`, `LedgerVoucher`, `PurchId` |
| `VendInvoiceTrans` | `ItemId`, `Qty`, `PurchPrice`, `LineAmount`, `InventTransId`, `InventDimId` |
| `VendTrans` | `AccountNum`, `Invoice`, `Voucher`, `TransDate`, `DueDate`, `CurrencyCode`, `AmountCur`, `AmountMST`, `PostingProfile` |

**14.3 انتبه إلى إشارة VendTrans**

في الحالة العادية:

| الحركة | إشارة `VendTrans.AmountCur` |
|---|---:|
| فاتورة شراء تزيد الالتزام | سالبة |
| دفع للمورد يخفض الالتزام | موجبة |
| Credit note تخفض الالتزام | موجبة |

إذن:

`VendTrans.AmountCur = −690`

وذلك **لا يعني أن القيد المحاسبي خطأ**؛ حساب المورد في GL دائن 690.

يحتفظ `VendTransOpen` بالرصيد المفتوح ويرتبط بالحركة الأصلية عبر `RefRecId`. [Microsoft Learn](https://learn.microsoft.com/common-data-model/schema/core/operationscommon/tables/finance/accountspayable/transaction/vendtransopen?utm_source=chatgpt.com)

**14.4 القيد النهائي المجمع**

بعد تصفية أثر الاستلام المرتبط:

| الحساب | مدين | دائن |
|---|---:|---:|
| المخزون | 600 | — |
| ضريبة المدخلات | 90 | — |
| ذمم الموردين | — | 690 |

إذا كان قيد الاستلام قد أُثبت، فالفاتورة تعكسه وتثبت القيد المالي المناسب. **لا تضف 600 إلى المخزون مرتين.**

**14.5 النتيجة بعد الفاتورة**

| المؤشر | القيمة |
|---|---:|
| المخزون الفعلي | 6 |
| الكمية المفوترة | 6 |
| قيمة المخزون المالية | 600 |
| المتبقي للاستلام | 4 |
| المتبقي للفوترة | 4 |
| المستحق للمورد | 690 |

حركة الـ6 تصبح `Purchased`. الفاتورة لا تستلم 6 قطع إضافية.

---

**15. الاستلام الثاني والخدمة والفاتورة الثانية**

نفّذ:

1. Product receipt للـ4 قطع الباقية.
2. إثبات قبول خدمة الصيانة.
3. فاتورة ثانية للبضاعة والخدمة.

**15.1 استلام البضاعة**

| البيان | القيمة |
|---|---:|
| الكمية | 4 |
| التكلفة | 400 |
| إجمالي المخزون بعد الاستلام | 10 |

**15.2 قبول الخدمة**

الخدمة:

- لا تزيد On-hand.
- لا تنشئ `InventTrans`.
- قد تنشئ استحقاق مصروف 200 وفق الإعداد المختار.

الأثر المجمع عند تفعيل استحقاق الخدمة:

| الحساب | مدين | دائن |
|---|---:|---:|
| مصروف صيانة مستحق | 200 | — |
| استحقاق مشتريات | — | 200 |

ثم يُعكس الاستحقاق المرتبط عند الفوترة.

**15.3 الفاتورة الثانية SUP-INV-002**

| البيان | الصافي | الضريبة | الإجمالي |
|---|---:|---:|---:|
| 4 قطع | 400 | 60 | 460 |
| خدمة الصيانة | 200 | 30 | 230 |
| **المجموع** | **600** | **90** | **690** |

القيد النهائي المجمع بعد تصفية الاستحقاق:

| الحساب | مدين | دائن |
|---|---:|---:|
| المخزون | 400 | — |
| مصروف الصيانة | 200 | — |
| ضريبة المدخلات | 90 | — |
| ذمم الموردين | — | 690 |

**النتيجة بعد الفاتورتين**

| المؤشر | النتيجة |
|---|---:|
| المخزون الفعلي | 10 |
| قيمة المخزون | 1,000 |
| مصروف الخدمة | 200 |
| ضريبة المدخلات | 180 |
| إجمالي المستحق للمورد | 1,380 |
| GRNI لهذه الكميات والخدمة | صفر بعد اكتمال تصفيته |
| حالة PO | Invoiced بعد اكتمال جميع السطور |

**شراء البضاعة لا يصبح مصروف تكلفة مبيعات فورًا.** البضاعة تبقى أصل مخزون إلى أن تُصرف بالعملية المناسبة.

---

**16. شراء الخدمات دون أمر شراء**

في AP لا نستخدم اسم **Free text invoice** بنفس مسار AR. المسارات المعتادة تشمل:

| المسار | الاستخدام |
|---|---|
| Vendor invoice journal | فاتورة مالية مباشرة بلا PO |
| Non-PO pending vendor invoice | فاتورة بفئات شراء وتوزيعات واعتماد حسب الإعداد |
| Invoice register ثم Approval/Pool | تسجيل محاسبي مبكر ثم استكمال المعالجة |

تدعم D365 يومية فاتورة المورد لإدخال الفواتير غير المرتبطة بأمر شراء، كما تدعم مسار سجل الفواتير والمجمع. [Microsoft Learn](https://learn.microsoft.com/hi-in/dynamics365/finance/accounts-payable/vendor-invoices-overview?utm_source=chatgpt.com)

**مثال مستقل:** استشارة 300 + ضريبة 45 = 345.

في Invoice journal:

| الجدول | ما تفحصه؟ |
|---|---|
| `LedgerJournalTable` | اليومية وحالة الترحيل |
| `LedgerJournalTrans` | المورد والفاتورة والتاريخ والمبلغ والحساب المقابل والضريبة |
| `VendTrans`, `VendTransOpen` | الالتزام والرصيد المفتوح |
| `TaxTrans` | الضريبة |
| جداول GL | القيد |

القيد:

| الحساب | مدين | دائن |
|---|---:|---:|
| مصروف استشارات | 300 | — |
| ضريبة مدخلات | 45 | — |
| المورد | — | 345 |

**لا يوجد أثر مخزني.**

ولا تفترض أن فاتورة Invoice journal تمر بجميع جداول `VendInvoiceInfo…` أو تفاصيل فواتير PO؛ التتبع يتبع المسار المستخدم.

---

**17. Invoice register / Invoice pool — اختبار مستقل**

هذا المسار ليس مجرد مسودة:

1. إدخال الفاتورة في Invoice register.
2. ترحيل سجل الفاتورة.
3. إثبات استحقاق/حساب مؤقت بحسب الإعداد.
4. استكمال المعالجة من Invoice pool أو Approval journal.
5. نقل التوزيع إلى المصروف أو الشراء الصحيح وعكس المؤقت.

**ما تفحصه:**

- وجود الأثر الأولي بعد ترحيل Register.
- الفاتورة غير متاحة للدفع قبل الموافقة المطلوبة.
- الحساب المؤقت يُصفّى عند الاستكمال.
- لا تتضاعف مديونية المورد.
- يبقى رقم فاتورة المورد قابلًا للتتبع.

توضح Microsoft أن الاستحقاق الناتج من Invoice register يُعكس عند استكمال الترحيل إلى الحساب المناسب. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/tasks/key-invoice-data-into-ap-system-invoice-pool?utm_source=chatgpt.com)

---

**18. الدفع الأول للمورد — 700**

نعود إلى المثال الأساسي: المستحق = 1,380.

المسار:

`Accounts payable > Payments > Vendor payment journal`

**18.1 اليومية**

| الخاصية | القيمة |
|---|---|
| Account type | Vendor |
| Account | V-1001 |
| Debit | 700 |
| Offset account type | Bank |
| Offset account | BANK-SAR |
| Currency | SAR |
| Method of payment | BANK |

الجداول والحقول:

| الجدول | أهم الحقول |
|---|---|
| `LedgerJournalTable` | `JournalNum`, `JournalName`, `Posted` |
| `LedgerJournalTrans` | `AccountType`, `LedgerDimension`, `AmountCurDebit`, `CurrencyCode`, `OffsetAccountType`, `OffsetLedgerDimension`, `PaymMode`, `Voucher`, `TransDate` |

القيد:

| الحساب | مدين | دائن |
|---|---:|---:|
| ذمم الموردين | 700 | — |
| البنك | — | 700 |

بعد الترحيل:

- `VendTrans.AmountCur = +700`.
- حركة بنك في `BankAccountTrans` وفق إعداد طريقة الدفع.
- لا أثر مخزني.

**إن كانت طريقة الدفع تستخدم حساب Bridging، افحص مرحلة الحساب الوسيط وانتقاله للبنك؛ المثال يفترض الدفع المباشر.**

**توليد ملف الدفع ليس مساويًا لترحيل اليومية أو تأكيد البنك.** افحص هذه المراحل منفصلة. يمكن إنشاء دفعات الموردين باستخدام Payment proposal ثم توليد الدفع بحسب الطريقة والبنك. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/cash-bank-management/tasks/vendor-payment-overview?utm_source=chatgpt.com)

---

**19. تسوية الـ700**

خصص الدفع:

| الفاتورة | المستحق قبل التسوية | المخصص | المتبقي |
|---|---:|---:|---:|
| SUP-INV-001 | 690 | 690 | 0 |
| SUP-INV-002 | 690 | 10 | 680 |
| **المجموع** | **1,380** | **700** | **680** |

الجداول:

| الجدول | الأثر |
|---|---|
| `VendSettlement` | تاريخ وروابط ومبالغ التسوية |
| `VendTrans` | تحديث مبالغ التسوية وتاريخ الإغلاق عند اكتماله |
| `VendTransOpen` | إزالة الأرصدة المغلقة وتخفيض الثانية |
| `SpecTrans` | تعليم الحركات أثناء تجهيز التسوية بحسب المسار |

**الإشارات المتوقعة للرصيد المفتوح:**

- الفاتورة الثانية: `−680`.
- الدفع: استُخدم بالكامل.
- الفاتورة الأولى: أُغلقت.

**VendTrans لا يُحذف عند التسوية.** الذي ينتهي هو رصيد الحركة المفتوح.

يمكن التسوية عند ترحيل الدفع أو بعده، وقد تنتج فروق عملة أو خصم نقدي أو معالجة لاختلاف حسابات وأبعاد الذمم. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/cash-bank-management/settlement-overview?utm_source=chatgpt.com)

---

**20. دفع الـ680 المتبقية**

القيد:

| الحساب | مدين | دائن |
|---|---:|---:|
| ذمم الموردين | 680 | — |
| البنك | — | 680 |

سوِّها على الفاتورة الثانية.

**النتيجة النهائية للمثال الأساسي**

| المؤشر | القيمة |
|---|---:|
| إجمالي فواتير المورد | 1,380 |
| إجمالي المدفوعات | 1,380 |
| رصيد المورد | 0 |
| أرصدة المثال في VendTransOpen | لا يوجد |
| المخزون الفعلي | 10 |
| قيمة المخزون | 1,000 |
| مصروف الصيانة | 200 |
| ضريبة المدخلات | 180 |
| انخفاض البنك | 1,380 |
| تاريخ الفواتير والمدفوعات والتسويات | محفوظ |

**إثبات توازن الأثر المجمع:**

`مخزون 1,000 + مصروف 200 + ضريبة 180 = انخفاض بنك 1,380`

---

**21. تتبع المثال كاملًا**

الأرقام أدناه للمستحق للمورد معروضة كمبالغ موجبة لسهولة القراءة.

| الخطوة | المخزون الفعلي | المتبقي لاستلام البضاعة | المتبقي لفوترة البضاعة | المستحق للمورد |
|---|---:|---:|---:|---:|
| البداية | 0 | — | — | 0 |
| إنشاء PO | 0 | 10 | 10 | 0 |
| تأكيد PO | 0 | 10 | 10 | 0 |
| Product receipt لـ6 | 6 | 4 | 10 | 0 |
| Invoice أولى | 6 | 4 | 4 | 690 |
| Product receipt لـ4 | 10 | 0 | 4 | 690 |
| Invoice ثانية تشمل الخدمة | 10 | 0 | 0 | 1,380 |
| دفع 700 | 10 | 0 | 0 | 680 |
| تسوية 700 | 10 | 0 | 0 | 680 |
| دفع وتسوية 680 | 10 | 0 | 0 | 0 |

**الدفع يخفض صافي رصيد المورد. التسوية تخصص الدفع للفواتير، ولا تخفض الرصيد مرة ثانية.**

---

**22. الدفعات المقدمة للمورد**

يوجد مساران يجب عدم خلطهما:

| المسار | الفكرة |
|---|---|
| Prepayment journal voucher | دفع مقدم باستخدام Posting profile مخصص |
| PO prepayment invoice | تعريف مقدمة على PO، فاتورة مقدمة، دفعها، ثم تطبيقها على النهائية |

تدعم D365 المسارين، ويتضمن مسار فاتورة المقدمة ربطها بأمر الشراء ثم دفعها وتطبيقها على فاتورة المورد. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/prepayments-invoices-vs-prepayments?utm_source=chatgpt.com)

**مثال مستقل، دون ضريبة على المقدمة لتبسيط الحساب: 300.**

عند الدفع مقدمًا:

| الحساب | مدين | دائن |
|---|---:|---:|
| دفعات مقدمة للموردين — أصل | 300 | — |
| البنك | — | 300 |

عند تطبيقها على فاتورة نهائية:

| الحساب | مدين | دائن |
|---|---:|---:|
| ذمم الموردين | 300 | — |
| دفعات مقدمة للموردين | — | 300 |

الجداول الأساسية بحسب المسار:

- `PurchTable`, `PurchPrepayTable`.
- جداول فواتير المورد المعلقة والمرحلة.
- `LedgerJournalTable`, `LedgerJournalTrans`.
- `VendTrans`, `VendTransOpen`, `VendSettlement`.
- جداول GL والضريبة.

**اختبارات مهمة:**

- دفع المقدمة لا يثبت مصروف الشراء النهائي مرة أخرى.
- لا تُطبق المقدمة أكثر من قيمتها المدفوعة والمؤهلة.
- التطبيق الجزئي يترك الرصيد الصحيح.
- إلغاء/تصحيح الفاتورة يعالج تطبيق المقدمة بالطريقة المدعومة.
- الضريبة تتبع إعداد المسار.

---

**23. مرتجع مشتريات فعلي إلى المورد**

مثال مستقل بعد الشراء:

- إرجاع قطعتين.
- تكلفة القطعة 100.
- Credit note = 200 + 30 ضريبة = 230.

الدورة:

1. إنشاء أمر مرتجع/سطور شراء سالبة.
2. نسخ المرجع من فاتورة المورد الأصلية.
3. مراجعة Marking والكمية والأبعاد.
4. تأكيد المرتجع.
5. ترحيل خروج البضاعة عبر إجراء Product receipt للكمية السالبة.
6. ترحيل Credit note.
7. تسويتها على فاتورة أو استرداد المبلغ.

تدعم عملية Credit note نسخ فاتورة المورد بإشارة معكوسة، وقد تربط حركة المرتجع بالأصل باستخدام Marking. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/supply-chain/procurement/tasks/create-purchase-return-order?utm_source=chatgpt.com)

الجداول:

- `PurchTable`, `PurchLine`.
- `VendPackingSlipJour`, `VendPackingSlipTrans`.
- `InventTransOrigin`, `InventTrans`, `InventSum`.
- `VendInvoiceJour`, `VendInvoiceTrans`.
- `VendTrans`, `VendTransOpen`, `VendSettlement`.
- `TaxTrans` وجداول GL.

الأثر النهائي المفترض عند تكلفة 100:

| الحساب | مدين | دائن |
|---|---:|---:|
| ذمم الموردين | 230 | — |
| المخزون | — | 200 |
| ضريبة المدخلات | — | 30 |

النتيجة:

- المخزون ينخفض قطعتين.
- قيمة المخزون تنخفض 200.
- الالتزام ينخفض 230.
- إذا كانت الفاتورة مدفوعة، يصبح لنا رصيد على المورد.

**لا تستخدم متوسطًا مفترضًا دون مراجعة مرجع المرتجع وMarking وتعديلات التكلفة.**

---

**24. إشعار دائن دون خروج بضاعة**

مثال: المورد يمنح تخفيضًا تجاريًا 100 + ضريبة 15، والبضاعة تبقى لدينا.

**الأثر المشترك المطلوب:**

| البيان | الأثر |
|---|---|
| مستحق المورد | ينخفض 115 |
| الضريبة | تخفيض 15 حسب المعالجة |
| الكمية الفعلية | لا تتغير |
| الحساب المقابل لـ100 | يتحدد بطبيعة التخفيض |

حدد هل الـ100:

- تخفيض تكلفة مخزون مرتبط بالشراء.
- تخفيض مصروف خدمة.
- حسم شراء مسجل بحساب مستقل وفق السياسة.

**لا تسجله كمرتجع كمية سالب إذا لم تخرج بضاعة.** واختبر وصول تخفيض التكلفة إلى المخزون أو المصروف الصحيح بدل الاكتفاء بإغلاق المورد.

---

**25. اختلاف سعر الفاتورة عن أمر الشراء**

سيناريو مستقل:

- PO: عشر قطع × 100 = 1,000.
- Product receipt: بقيمة مقدرة 1,000.
- Vendor invoice: عشر قطع × 110 = 1,100.
- نفترض اعتماد فرق السعر.

الضريبة = 165، والإجمالي = 1,265.

| المرحلة | القيمة |
|---|---:|
| الاستلام المقدّر | 1,000 |
| التكلفة المالية للشراء | 1,100 |
| فرق التكلفة | 100 |
| التزام المورد | 1,265 |

**في Weighted average لا تفترض أن الـ100 دائمًا تذهب إلى حساب Purchase price variance كما في نماذج تكلفة أخرى.** افحص تحديث تكلفة الاستلام والتوزيع على المخزون والصرف وتعديلات الإقفال.

إذا كانت الكمية كلها باقية دون حركات أخرى، نتوقع أن تعكس قيمة المخزون تكلفة الشراء المالية 1,100، باستبعاد الضريبة القابلة للاسترداد.

---

**26. المصاريف الإضافية — الشحن والتأمين**

مثال مستقل:

- بضاعة: 1,000.
- شحن: 100.

| السياسة | قيمة المخزون | المصروف |
|---|---:|---:|
| رسملة الشحن على البضاعة | 1,100 | 0 |
| تحميله مصروفًا | 1,000 | 100 |

الجداول الرئيسية:

- `MarkupTable`: تعريف كود المصروف وطريقة الترحيل.
- `MarkupTrans`: المصروف المطبق.
- جداول PO والفاتورة.
- `InventTrans` عند تأثيره على التكلفة.
- GL والضريبة.

**ما تفحصه:** التوزيع على السطور، والكمية الجزئية، والمطابقة، والضريبة، والحساب المقابل، وعدم رسملة المصروف مرتين.

---

**27. شراء بعملة أجنبية**

مثال مستقل دون ضريبة:

- فاتورة 1,000 USD.
- سعر الفاتورة = 3.75 SAR.
- الالتزام = 3,750 SAR.
- سعر الدفع = 3.80 SAR.
- قيمة الدفع = 3,800 SAR.

القيد المجمع عند الإغلاق:

| الحساب | مدين | دائن |
|---|---:|---:|
| ذمم الموردين | 3,750 | — |
| خسارة عملة محققة | 50 | — |
| البنك | — | 3,800 |

إذا كانت الفاتورة مفتوحة بنهاية الفترة، تختبر **AP foreign currency revaluation** للفروق غير المحققة، ثم معالجتها عند التسوية.

تراجع:

- أسعار الصرف وتواريخها.
- `VendTrans.AmountCur`, `AmountMST`.
- `VendTransOpen`.
- حقول وتاريخ إعادة التقييم.
- `VendSettlement`.
- حسابات أرباح وخسائر العملة في GL.

**نجاح الاختبار:** تغلق 1,000 USD بالكامل، وتظهر الـ50 في حساب فرق العملة الصحيح.

---

**28. المطابقة والإقفال**

| المطابقة | ماذا تقارن؟ |
|---|---|
| AP to GL | رصيد الموردين مقابل حسابات الذمم |
| GRNI | الاستلامات غير المفوترة مقابل حسابات الاستحقاق |
| Inventory to GL | قيمة المخزون مقابل حساباته، بنطاق وتاريخ موحدين |
| Bank reconciliation | المدفوعات المرحلة مقابل كشف البنك |
| Tax reconciliation | ضريبة الفواتير والإشعارات مقابل حركاتها وحساباتها |
| Vendor statement | كشف المورد مقابل كشفه الخارجي |
| Aging | المستحقات حسب تاريخ الاستحقاق |
| Prepayment reconciliation | المقدمات المدفوعة والمطبقة والمتبقية |

**GRNI لا يساوي دائمًا صفرًا بنهاية الشهر.** يجب أن يساوي الاستحقاق المبرر للاستلامات غير المفوترة، مع تفسير الفروق.

وبالنسبة إلى Weighted average:

- تستخدم المشتريات المالية في حساب المتوسط وفق إعدادات وأبعاد التكلفة.
- يُجرى Inventory close لتسوية التكلفة الدورية.
- قد تتعدل تكلفة الصرف وقيمة المخزون.
- **لا يتغير مبلغ فاتورة المورد لمجرد تغير متوسط تكلفة المخزون.** [learn.microsoft.com](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/weighted-average-physical-value-marking?utm_source=chatgpt.com)

---

**29. ربط الجداول للتتبع والتقارير**

| التتبع | الربط الأساسي |
|---|---|
| المورد إلى الاسم | `VendTable.Party → DirPartyTable.RecId` |
| رأس PO إلى سطوره | `PurchTable.PurchId → PurchLine.PurchId` مع الشركة |
| سطر PO إلى أصل المخزون | `PurchLine.InventTransId → InventTransOrigin.InventTransId` مع الشركة |
| أصل المخزون إلى الحركات | `InventTransOrigin.RecId → InventTrans.InventTransOrigin` |
| الحركة إلى الأبعاد | `InventTrans.InventDimId → InventDim.InventDimId` مع الشركة |
| الرصيد المفتوح إلى حركة المورد | `VendTransOpen.RefRecId → VendTrans.RecId` مع الشركة |
| الفاتورة المعلقة إلى الاستلام | عبر سطورها و`VendInvoiceInfoSubLine` |
| رأس القيد إلى سطوره | `GeneralJournalEntry.RecId → GeneralJournalAccountEntry.GeneralJournalEntry` |
| سطر GL إلى الحساب والأبعاد | `LedgerDimension → DimensionAttributeValueCombination.RecId` |
| سند الوحدة إلى GL | `SubledgerVoucherGeneralJournalEntry` حسب المسار |

**قواعد تمنع مضاعفة مبالغ التقرير:**

- لا تربط كل سطور الفاتورة بكل سطور الاستلام باستخدام `PurchId` فقط.
- لا تفترض أن فاتورة واحدة تخص PO واحدًا أو Warehouse واحدًا.
- لا تربط `VendTrans` بكل سطور GL ثم تجمع مبلغ المورد دون معالجة التكرار.
- استخدم الشركة ومفاتيح المستند والسطور ومراجع المخزون.
- راجع `VendInvoicePurchLink` عند تتبع فواتير ترتبط بعدة أوامر.
- `VendTransOpen` يعكس الرصيد المفتوح الحالي؛ استخراج رصيد تاريخي يحتاج مراعاة تاريخ التسويات.

---

**30. قائمة الاختبارات — خدمةً خدمة**

**هذه أسماء وظيفية للاختبارات والخدمات في مشروعك، وليست أسماء API رسمية في D365.**

ابدأ الحالات بـ`Not run`.

**30.1 الإعداد والبيانات الأساسية**

| ID | الخدمة | الاختبار | شرط النجاح |
|---|---|---|---|
| CFG-01 | الفترة المالية | ترحيل بتاريخ مفتوح ومغلق | السماح/الرفض حسب الإعداد |
| CFG-02 | Vendor posting | مورد/مجموعة/All | حساب الذمم الصحيح |
| CFG-03 | Inventory posting | استلام وفاتورة | حساب صحيح لكل Posting type |
| CFG-04 | Tax calculation | بضاعة وخدمة | الضريبة والحساب صحيحان |
| CFG-05 | Matching setup | فرق سعر وكمية | تطبيق السياسة والتفاوت والاعتماد |
| CFG-06 | Dimensions | بعد إلزامي ناقص | رفض الترحيل أو طلب الاستكمال |
| CFG-07 | Payment setup | طريقة دفع وبنك | الحسابات والصيغة الصحيحة |
| MD-01 | Vendor maintenance | إنشاء المورد | الاسم والعنوان والعملة والشروط صحيحة |
| MD-02 | Vendor bank | إعداد حساب دفع | استخدام الحساب المعتمد |
| MD-03 | Invoice account | حساب فاتورة مختلف | الالتزام على الحساب الصحيح |
| MD-04 | Vendor blocking | مورد محظور | منع العمليات المحددة |
| MD-05 | Stocked product | صنف مخزني | حركة كمية وتكلفة |
| MD-06 | Service product | Not stocked | لا InventTrans ولا On-hand |
| MD-07 | Unit conversion | وحدة شراء مختلفة | تحويل الكميات والأسعار صحيح |

**30.2 أمر الشراء والاستلام**

| ID | الخدمة | الاختبار | شرط النجاح |
|---|---|---|---|
| PO-01 | Create PO | رأس وسطر بضاعة وخدمة | حفظ القيم والإجمالي |
| PO-02 | Pricing | Trade agreement وPrice unit | سعر صحيح |
| PO-03 | Discounts | خصم سطر/كلي | صافي وضريبة صحيحان |
| PO-04 | Charges | شحن على PO | توزيع وترحيل صحيحان |
| PO-05 | Workflow | إرسال واعتماد ورفض | منع تجاوز دورة الاعتماد |
| PO-06 | Confirmation | تأكيد PO | مستند تأكيد دون استلام |
| PO-07 | Change management | تعديل أمر مؤكد | إعادة الاعتماد عند الحاجة |
| PO-08 | Cancel remainder | إلغاء متبقٍ | لا استلام أو فوترة للملغى |
| RCV-01 | Registration | تسجيل وصول | حالة Registered دون تكرار كمية |
| RCV-02 | Product receipt | استلام 6 | Received وOn-hand = 6 |
| RCV-03 | Partial receipt | استلام 4 لاحقًا | الإجمالي 10 والمتبقي صفر |
| RCV-04 | Service receipt | قبول الخدمة | لا مخزون، واستحقاق وفق الإعداد |
| RCV-05 | Overdelivery | زيادة على PO | تطبيق نسبة السماح |
| RCV-06 | Underdelivery | إقفال كمية ناقصة | المتبقي يعالج وفق السياسة |
| RCV-07 | Receipt correction | تصحيح/إلغاء استلام | عكس الكمية والاستحقاق الصحيح |
| RCV-08 | Required dimensions | Batch/Serial عند إلزامهما | منع الاستلام بأبعاد ناقصة |

**30.3 فاتورة المورد والمطابقة**

| ID | الخدمة | الاختبار | شرط النجاح |
|---|---|---|---|
| BILL-01 | Pending invoice | حفظ المسودة | لا ترحيل نهائي بمجرد الحفظ |
| BILL-02 | Receipt matching | ربط 6 مستلمة | الكمية مرتبطة بالاستلام الصحيح |
| BILL-03 | Quantity mismatch | فاتورة 7 مقابل 6 | تطبيق سياسة الفرق |
| BILL-04 | Price mismatch | سعر خارج التفاوت | منع/اعتماد حسب السياسة |
| BILL-05 | Duplicate invoice | نفس رقم المورد | تطبيق إعداد منع التكرار |
| BILL-06 | Tax validation | ضريبة خاطئة | كشف الفرق ومعالجته |
| BILL-07 | Workflow approval | فاتورة بلا اعتماد | عدم الترحيل غير المسموح |
| BILL-08 | First invoice | 6 × 100 + ضريبة | التزام 690 وتكلفة 600 |
| BILL-09 | Second invoice | 4 قطع + خدمة | التزام 690 وإكمال PO |
| BILL-10 | Full stock purchase | استلام وفوترة كاملان | مخزون والتزام صحيحان |
| BILL-11 | Service PO | خدمة فقط | مصروف دون حركة مخزون |
| BILL-12 | Non-PO invoice | Invoice journal/فئة | مصروف وضريبة ومورد |
| BILL-13 | Invoice register | تسجيل ثم استكمال | تصفية المؤقت دون تكرار |
| BILL-14 | Multiple receipts | فاتورة لعدة استلامات | لا كمية مفقودة أو مكررة |
| BILL-15 | Multiple POs | فاتورة لأكثر من PO | الروابط والتوزيعات صحيحة |
| BILL-16 | Price change | استلام 100 وفاتورة 110 | التكلفة والفرق صحيحان |

**30.4 الدفع والتسوية**

| ID | الخدمة | الاختبار | شرط النجاح |
|---|---|---|---|
| PAY-01 | Payment proposal | اختيار حسب الاستحقاق | اقتراح الفواتير المؤهلة فقط |
| PAY-02 | Payment hold | فاتورة محظورة للدفع | استبعادها/رفض الدفع حسب الإعداد |
| PAY-03 | Payment generation | توليد ملف | الطريقة والبنك والمبلغ صحيحة |
| PAY-04 | Payment posting | دفع 700 | مورد مدين وبنك دائن |
| PAY-05 | Unallocated payment | دفع دون تسوية | حركة دفع مفتوحة |
| SET-01 | Partial settlement | تخصيص 700 | الأولى صفر والثانية 680 |
| SET-02 | Final settlement | دفع وتخصيص 680 | إغلاق أرصدة المثال |
| SET-03 | One payment/many invoices | دفع لعدة فواتير | تخصيص صحيح |
| SET-04 | Many payments/one invoice | دفعات متعددة | تخفيض تدريجي صحيح |
| SET-05 | Automatic settlement | تفعيلها باختبار مستقل | تطبيق القواعد المعتمدة |
| PAY-06 | Cash discount | دفع داخل/خارج المدة | خصم وحساب وضريبة صحيحة |
| PAY-07 | Overpayment | دفع زائد | رصيد لنا على المورد |
| PAY-08 | Underpayment | دفع ناقص | بقاء الفرق أو معالجته المعتمدة |
| PRE-01 | Journal prepayment | مقدمة ثم تطبيق | أصل ثم تصفيته |
| PRE-02 | PO prepayment invoice | فاتورة مقدمة ودفع وتطبيق | عدم تكرار الالتزام أو التطبيق |
| SET-06 | Undo settlement | فك التسوية | إعادة فتح الأرصدة دون عكس البنك |
| PAY-09 | Payment reversal | عكس دفع بالطريقة المدعومة | عكس الأثر ومعالجة التسويات |
| PAY-10 | Vendor refund | استرداد رصيد من المورد | بنك مدين وإغلاق الرصيد |

**30.5 المرتجعات والإقفال**

| ID | الخدمة | الاختبار | شرط النجاح |
|---|---|---|---|
| RET-01 | Purchase return | إرجاع قطعتين | كمية −2 وتخفيض مستحق 230 |
| RET-02 | Return costing | ربط بالفاتورة الأصلية | تكلفة وMarking صحيحان |
| RET-03 | Financial credit | حسم دون بضاعة | لا تغيير كمية |
| RET-04 | Credit settlement | تسوية إشعار دائن | الرصيد المفتوح صحيح |
| FX-01 | Foreign invoice | شراء USD | مبلغ العملة والمحاسبي صحيحان |
| FX-02 | AP revaluation | فاتورة أجنبية مفتوحة | فرق غير محقق صحيح |
| FX-03 | FX payment settlement | دفع بسعر مختلف | فرق محقق وإغلاق العملة |
| REC-01 | Vendor aging | حسب Due date | الشرائح والمجاميع صحيحة |
| REC-02 | Vendor statement | كشف قبل وبعد التسوية | الحركات والرصيد صحيحان |
| REC-03 | AP to GL | مطابقة الموردين | لا فرق غير مفسر |
| REC-04 | GRNI | استلامات غير مفوترة | رصيد مبرر بالمستندات |
| REC-05 | Bank reconciliation | مقارنة بكشف البنك | لا دفع مفقود أو مكرر |
| REC-06 | Inventory to GL | تقرير القيمة | تطابق النطاق والحسابات |
| CLOSE-01 | Weighted average close | مشتريات وصرف بأسعار مختلفة | تسويات تكلفة صحيحة |
| CLOSE-02 | Period close | ترحيل بعد الإقفال | رفض غير المسموح |

**30.6 سلامة الخدمات والتكامل**

خصوصًا إذا ستطبق هذه الدورة في ASP.NET Core:

| ID | الاختبار | شرط النجاح |
|---|---|---|
| SYS-01 | إعادة إرسال طلب الاستلام | لا استلام مكرر |
| SYS-02 | إعادة إرسال طلب الفاتورة | لا التزام أو Voucher مكرر |
| SYS-03 | فشل أثناء الترحيل | لا مستند نصف مرحل |
| SYS-04 | مستخدمان يفوتران نفس الاستلام | منع تجاوز الكمية المؤهلة |
| SYS-05 | مستخدمان يدفعان نفس الفاتورة | منع الدفع المكرر |
| SYS-06 | تسوية متزامنة | عدم استخدام الدفع مرتين |
| SYS-07 | عزل الشركات | لا بيانات أو ترحيل لشركة أخرى |
| SYS-08 | صلاحيات الدفع وتعديل البنك | احترام الصلاحيات والاعتماد |
| SYS-09 | التقريب | تطابق السطور والرأس والضريبة والذمم |
| SYS-10 | Audit trail | توثيق الإنشاء والترحيل والتصحيح |

---

**31. طريقة تسجيل نتيجة الاختبار**

| Test ID | الخدمة | المحاولة | الحالة | الدليل |
|---|---|---:|---|---|
| RCV-02 | استلام 6 قطع | 1 | Failed | PO + رسالة الخطأ |
| RCV-02 | إصلاح سبب الفشل | — | Fixed — awaiting retest | وصف الإصلاح |
| RCV-02 | إعادة استلام 6 | 2 | Passed | Product receipt + InventTrans + Voucher |
| BILL-08 | الفاتورة الأولى | 1 | Passed | Invoice + VendTrans + GL |
| SET-01 | تسوية 700 | 1 | Passed | Settlement history + الرصيد المتبقي |

لكل اختبار احتفظ بـ:

- الشركة والمستخدم.
- البيانات والإعدادات السابقة.
- النتيجة المتوقعة والفعلية.
- رقم PO والاستلام والفاتورة وVoucher.
- الكمية والتكلفة والضريبة ورصيد المورد.
- الخطأ والإصلاح ونتيجة إعادة الاختبار.

**Fixed لا تساوي Passed، ونجاح شاشة الترحيل وحده لا يكفي.** نجاح الدورة يتطلب تطابق **المستندات، والمخزون، والمورد، والبنك، والضريبة، وGL** معًا.

**تقصد دورة الحسابات الدائنة Accounts Payable كاملة في D365 F&O، من طلب الشراء إلى تسوية المورد والإقفال، مع التأثير المالي والمخزني والجداول المتأثرة.**

سنعتمد مثالًا واحدًا للدورة الأساسية، ثم نغطي الدفعات المقدمة والمرتجعات والخدمات وفروق العملة والتصحيحات.

**1. مسار الدورة الكامل**

| الترتيب | المرحلة | الغرض |
|---:|---|---|
| 1 | الإعدادات | تحديد الحسابات والضرائب والمطابقة والدفع |
| 2 | البيانات الأساسية | تجهيز المورد والصنف والمستودع |
| 3 | Purchase requisition — اختياري | طلب احتياج داخلي واعتماده |
| 4 | Request for quotation — اختياري | طلب عروض واختيار المورد |
| 5 | Purchase order | إنشاء أمر الشراء |
| 6 | Workflow / Approval | اعتماد الأمر حسب الإعداد |
| 7 | Purchase confirmation | تأكيد الطلب مع المورد |
| 8 | Registration — حسب الإعداد | تسجيل وصول الكمية وأبعادها |
| 9 | Product receipt | إثبات استلام البضاعة أو قبول الخدمة |
| 10 | Pending vendor invoice | إدخال فاتورة المورد |
| 11 | Invoice matching | مطابقة الأمر والاستلام والفاتورة |
| 12 | Invoice posting | إثبات الفاتورة والالتزام النهائي |
| 13 | Payment proposal — اختياري | اقتراح الفواتير المستحقة |
| 14 | Vendor payment | دفع المورد |
| 15 | Settlement | تخصيص الدفع للفواتير |
| 16 | Reconciliation | مطابقة المورد والبنك والمخزون وGL |
| 17 | Inventory / Period close | تسوية التكلفة وإقفال الفترة |

هذا هو مسار الشراء إلى الدفع؛ فاتورة المورد تربط مرحلة الاستلام بإثبات الالتزام المالي. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/vendor-invoices-overview?utm_source=chatgpt.com)

**2. الإعدادات المطلوبة قبل التنفيذ**

| الإعداد | ما يجب تجهيزه؟ | جداول أساسية |
|---|---|---|
| الشركة والفترات | العملة المحاسبية والفترات المفتوحة | `CompanyInfo`, `Ledger`, `FiscalCalendarPeriod` |
| دليل الحسابات | مخزون، موردون، استحقاق شراء، مصروفات، بنك، ضريبة | `MainAccount` |
| مجموعات الموردين | تصنيف الموردين | `VendGroup` |
| Vendor posting profiles | حساب ذمم الموردين وحسابات المسارات المطلوبة | `VendPosting` |
| Accounts payable parameters | منع تكرار الفاتورة والمطابقة والتسوية | `VendParameters` |
| شروط الدفع | تاريخ الاستحقاق | `PaymTerm` |
| الخصم النقدي | نسبة الخصم ومدته وحساباته | `CashDisc` |
| طرق الدفع والبنوك | طريقة الدفع والحساب البنكي | `VendPaymModeTable`, `BankAccountTable` |
| الضرائب | أكواد ومجموعات ضريبة المورد والصنف | `TaxTable`, `TaxGroupHeading`, `TaxItemGroupHeading` |
| نموذج المخزون | Stocked product، وWeighted average، والترحيل | `InventModelGroup` |
| ترحيل المخزون | حسابات الاستلام والفاتورة والمصروف | `InventPosting` |
| الموقع والمستودع | أبعاد تخزين البضاعة | `InventSite`, `InventLocation` |
| الوحدات | وحدة الشراء وتحويلها لوحدة المخزون | `UnitOfMeasure`, `UnitOfMeasureConversion` |
| الأبعاد المالية | Business unit وCost center وغيرها | جداول الأبعاد المالية |
| التسلسل الرقمي | PO واليوميات والمستندات | `NumberSequenceTable` |

**الإعداد الحاسم للاستلام:** إثبات الكمية بالمخزون لا يعني تلقائيًا وجود قيد استلام في GL؛ ترحيله المالي يعتمد على إعدادات ترحيل الاستلام والمخزون الفعلي واستحقاق الالتزام. [GitHub](https://github.com/MicrosoftDocs/Dynamics-365-Unified-Operations-Public/blob/main/articles/finance/general-ledger/purchase-order-posting.md?utm_source=chatgpt.com)

**3. البيانات الأساسية للمثال**

| البيان | القيمة |
|---|---|
| المورد | `V-1001` |
| اسم المورد | شركة التوريدات |
| العملة | SAR |
| شروط الدفع | 30 يومًا |
| الصنف | `ITEM-001` |
| نوع الصنف | مخزني Stocked |
| نموذج التكلفة | Weighted average |
| الموقع | `S1` |
| المستودع | `WH1` |
| الكمية | 10 قطع |
| سعر القطعة | 100 |
| صافي الشراء | 1,000 |
| الضريبة المفترضة للمثال | 15% = 150 |
| إجمالي الفاتورة | 1,150 |

نفترض رصيد بداية صفر، وضريبة قابلة للاسترداد، وعدم وجود خصومات أو شحن أو حركات مخزون أخرى.

**4. إنشاء المورد**

المسار المعتاد:

`Accounts payable > Vendors > All vendors`

أدخل:

- حساب المورد والاسم والمجموعة.
- عملة التعامل وشروط الدفع.
- طريقة الدفع والحساب البنكي عند الحاجة.
- مجموعة الضريبة والأبعاد المالية.
- حساب الفاتورة Invoice account إذا كان مختلفًا.

| الجدول | وظيفته |
|---|---|
| `VendTable` | بيانات المورد المالية والتشغيلية |
| `DirPartyTable` | اسم الطرف وبياناته المشتركة |
| `VendBankAccount` | حسابات المورد البنكية |
| جداول العناوين | العنوان ووسائل الاتصال |

**الأثر:** لا مخزون ولا مديونية ولا قيد GL بمجرد إنشاء المورد.

**5. تجهيز الصنف المخزني**

تأكد من ربط الصنف بـ:

- Item group.
- Item model group بنموذج Weighted average.
- Storage dimension group.
- Tracking dimension group إذا احتجت Batch أوSerial.
- وحدات الشراء والمخزون وتحويلاتها.
- إعدادات أوامر الشراء الافتراضية.

| الجدول | وظيفته |
|---|---|
| `EcoResProduct` | تعريف المنتج المشترك |
| `InventTable` | الصنف المحرر داخل الشركة |
| `InventItemGroupItem` | ربط الصنف بمجموعة الأصناف |
| `InventModelGroupItem` | ربط الصنف بنموذج المخزون |
| `InventTableModule` | إعدادات الشراء والبيع والمخزون للصنف |

**الأثر:** لا كمية ولا قيمة مخزون حتى تنفيذ حركة فعلية.

**6. طلب الشراء وطلب عروض الأسعار — عند استخدامهما**

| العملية | ما يحدث؟ | جداول رئيسية |
|---|---|---|
| Purchase requisition | الموظف يطلب 10 قطع ويُرسل الطلب للاعتماد | `PurchReqTable`, `PurchReqLine` |
| RFQ | إرسال طلب عرض ومقارنة عروض الموردين | `PurchRFQCaseTable`, `PurchRFQCaseLine` |
| اختيار المورد | اعتماد العرض وإنشاء/تحديث أمر الشراء | جداول RFQ وPO |

في المثال الأساسي لا تتغير كمية المخزون أو ذمم الموردين بهذه المراحل. وقد يظهر حجز ميزانية أو التزام إذا كانت Budget control مفعلة.

**7. إنشاء أمر الشراء Purchase order**

المسار:

`Procurement and sourcing > Purchase orders > All purchase orders`

رأس الأمر:

| الحقل | القيمة |
|---|---|
| Purchase order | `PO-0001` |
| Vendor account | `V-1001` |
| Invoice account | `V-1001` |
| Currency | SAR |
| Payment terms | 30 يومًا |

السطر:

| الصنف | الكمية | الوحدة | السعر | الصافي | الضريبة |
|---|---:|---|---:|---:|---:|
| ITEM-001 | 10 | قطعة | 100 | 1,000 | 150 |

الجداول:

| الجدول | الأثر |
|---|---|
| `PurchTable` | رأس أمر الشراء |
| `PurchLine` | سطر الصنف والكمية والسعر والمتبقي |
| `InventDim` | الموقع والمستودع وأبعاد المخزون |
| `InventTransOrigin` | أصل حركة المخزون |
| `InventTrans` | حركة استلام متوقعة للصنف المخزني |

النتيجة المتوقعة:

| المؤشر | النتيجة |
|---|---|
| حالة استلام حركة المخزون | Ordered |
| الكمية الفعلية | 0 |
| الكمية المطلوبة | 10 |
| رصيد المورد | 0 |
| قيد GL عادي للمثال | لا يوجد |

**وجود InventTrans بكمية طلب لا يعني أن البضاعة أصبحت موجودة فعليًا.**

**8. اعتماد أمر الشراء وتأكيده**

إذا كانت Change management مفعلة:

`Draft → In review → Approved → Confirmed`

بعد الاعتماد نفّذ **Purchase confirmation**.

| الجدول | الأثر |
|---|---|
| `PurchTable`, `PurchLine` | حالة الأمر والبيانات الحالية |
| `VendPurchOrderJour` | مستند التأكيد |
| `VendPurchOrderTrans` | سطور مستند التأكيد |

النتيجة:

- يوجد مستند مؤكد للمورد.
- الكمية الفعلية ما زالت صفرًا.
- لا توجد فاتورة مورد أو حركة `VendTrans`.
- لا يوجد قيد شراء اعتيادي في مثالنا.

**حالة الاعتماد/المستند تختلف عن حالة Purchase status الخاصة بالاستلام والفوترة.**

**9. تسجيل وصول البضاعة Registration — إذا كان مطلوبًا**

وصلت أول دفعة: **6 قطع**.

قد تسجل الكمية وBatch/Serial قبل ترحيل الاستلام.

| المؤشر | النتيجة |
|---|---|
| حالة استلام الكمية المسجلة | Registered |
| المستند | تسجيل وصول |
| فاتورة المورد | لم تُرحل |
| ذمم الموردين | لم تُثبت |

قد تظهر الكمية المسجلة ضمن مقاييس المخزون الفعلي؛ لكن إتاحة استخدامها تعتمد على الحالة والأبعاد والإعدادات. عند تنفيذ Product receipt لاحقًا يجب تحديث نفس الحركة دون مضاعفة الكمية.

يمكن فرض Registration أوProduct receipt قبل الخطوة التالية من خلال متطلبات نموذج المخزون. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/inventory-costing-faq?utm_source=chatgpt.com)

**10. ترحيل استلام أول دفعة Product receipt**

من أمر الشراء:

`Receive > Product receipt`

أدخل:

| البيان | القيمة |
|---|---|
| رقم استلام المورد | `RCV-001` |
| الكمية | 6 |
| السعر التقديري | 100 |
| قيمة الاستلام | 600 |

الجداول:

| الجدول | الأثر |
|---|---|
| `VendPackingSlipJour` | رأس مستند الاستلام |
| `VendPackingSlipTrans` | تفاصيل الاستلام |
| `PurchLine` | تخفيض المتبقي للاستلام |
| `InventTrans` | الكمية المستلمة تصبح Received |
| `InventSum` | تحديث أرصدة المخزون |
| جداول GL | عند تفعيل الترحيل المالي للاستلام |

النتيجة:

| المؤشر | القيمة |
|---|---:|
| مخزون مستلم | 6 |
| المتبقي للاستلام | 4 |
| المتبقي للفوترة | 10 |
| ذمم المورد من فاتورة | 0 |

**الأثر المالي المبسط، بافتراض تفعيل استحقاق الاستلام:**

| الحساب | مدين | دائن |
|---|---:|---:|
| مخزون مستلم غير مفوتر/حساب الاستلام | 600 | — |
| استحقاق مشتريات GRNI | — | 600 |

هذه خلاصة الأثر الاقتصادي؛ قد يتضمن Voucher الفعلي حسابات وسيطة وسطورًا إضافية حسب Posting setup.

**11. إدخال فاتورة أولى ومطابقتها**

المورد أرسل فاتورة مقابل **6 قطع**:

| البيان | القيمة |
|---|---:|
| رقم الفاتورة | `INV-001` |
| الصافي | 600 |
| الضريبة | 90 |
| الإجمالي | 690 |

أنشئ Pending vendor invoice واختر الاستلام `RCV-001`.

| الجدول | وظيفته |
|---|---|
| `VendInvoiceInfoTable` | رأس الفاتورة المعلقة |
| `VendInvoiceInfoLine` | سطور الفاتورة |
| `VendInvoiceInfoSubLine` | تفاصيل ربط السطر بالاستلام |

نفّذ المطابقة:

| عنصر المطابقة | المتوقع |
|---|---|
| المورد | مطابق لأمر الشراء |
| السعر | 100 حسب الأمر |
| كمية الفاتورة | 6 |
| كمية الاستلام المرتبطة | 6 |
| الضريبة | 90 |
| رقم الفاتورة | يمر بسياسة منع التكرار |
| الأبعاد والتوزيعات | مكتملة |
| الاعتماد | مكتمل إذا كان مطلوبًا |

المطابقة الثلاثية تقارن أمر الشراء والاستلام والفاتورة، مع تطبيق سياسة الفروق والتفاوت المسموح. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-payable/tasks/key-invoice-data-ap-system-vendor-invoice?utm_source=chatgpt.com)

**حفظ المسودة وحده لا يثبت الفاتورة نهائيًا في ذمم الموردين.**

**12. ترحيل الفاتورة الأولى**

بعد نجاح المطابقة والاعتماد، نفّذ **Post**.

| الجدول | الأثر |
|---|---|
| `VendInvoiceJour` | رأس فاتورة PO المرحلة |
| `VendInvoiceTrans` | سطور الفاتورة المرحلة |
| `VendTrans` | حركة المورد |
| `VendTransOpen` | الرصيد المفتوح |
| `InventTrans` | الكمية المفوترة تصبح Purchased |
| `PurchLine` | تخفيض المتبقي للفوترة |
| `TaxTrans` | حركات الضريبة |
| `GeneralJournalEntry` | رأس القيد |
| `GeneralJournalAccountEntry` | سطور القيد |

**معالجة الاستلام السابق بصورة مبسطة:**

عكس الأثر المؤقت:

| الحساب | مدين | دائن |
|---|---:|---:|
| استحقاق مشتريات GRNI | 600 | — |
| مخزون مستلم غير مفوتر | — | 600 |

إثبات الفاتورة:

| الحساب | مدين | دائن |
|---|---:|---:|
| المخزون المالي | 600 | — |
| ضريبة المدخلات | 90 | — |
| ذمم الموردين | — | 690 |

النتيجة:

| المؤشر | القيمة |
|---|---:|
| الكمية الفعلية | 6 |
| الكمية المحدثة ماليًا | 6 |
| تكلفة هذه الكمية | 600 |
| المستحق للمورد | 690 |
| GRNI للكمية المفوترة | 0 |
| المتبقي للاستلام | 4 |
| المتبقي للفوترة | 4 |

**الفاتورة لا تضيف 6 قطع مرة ثانية؛ تُحدّث تكلفة الكمية المستلمة ماليًا.**

**13. استلام وفوترة بقية الأمر**

استلم 4 قطع بمستند `RCV-002`، ثم رحّل فاتورة `INV-002`:

| البيان | القيمة |
|---|---:|
| الكمية | 4 |
| الصافي | 400 |
| الضريبة | 60 |
| إجمالي الفاتورة الثانية | 460 |

بعد اكتمال الاستلام والفوترة:

| المؤشر | القيمة |
|---|---:|
| إجمالي الكمية الفعلية | 10 |
| إجمالي الكمية المفوترة | 10 |
| قيمة المخزون | 1,000 |
| ضريبة المدخلات | 150 |
| إجمالي المستحق للمورد | 1,150 |
| المتبقي للاستلام | 0 |
| المتبقي للفوترة | 0 |
| GRNI لهذا الأمر | 0 |
| Purchase status | Invoiced |

**شراء البضاعة يثبت أصل مخزون. تكلفة المبيعات تظهر عند بيعها وصرفها وفق دورة البيع.**

**14. اقتراح الدفع Payment proposal**

المسار المعتاد:

`Accounts payable > Payments > Vendor payment journal`

يمكن إنشاء Payment proposal لاختيار الفواتير حسب:

- تاريخ الاستحقاق.
- المورد والعملة.
- طريقة الدفع.
- فرصة الخصم النقدي.
- حظر الدفع والقيود المعتمدة.

راجع الاقتراح قبل الترحيل: الفواتير، والحساب البنكي، والمبلغ، والتاريخ.

**إنشاء اقتراح أو توليد ملف دفع لا يعني ترحيل اليومية أو تأكيد خروج المبلغ من البنك.**

**15. دفع أول مبلغ: 700**

أنشئ سطر دفع:

| الحقل | القيمة |
|---|---|
| Account type | Vendor |
| Account | V-1001 |
| Debit | 700 |
| Offset account type | Bank |
| Currency | SAR |

الجداول:

| الجدول | الأثر |
|---|---|
| `LedgerJournalTable` | رأس يومية الدفع |
| `LedgerJournalTrans` | تفاصيل الدفع |
| `VendTrans` | حركة دفع للمورد |
| `VendTransOpen` | دفع مفتوح إذا لم يُسوَّ |
| `BankAccountTrans` | حركة البنك في مسار الدفع المباشر |
| جداول GL | القيد المالي |

القيد المفترض للدفع المباشر:

| الحساب | مدين | دائن |
|---|---:|---:|
| ذمم الموردين | 700 | — |
| البنك | — | 700 |

النتيجة:

- صافي المستحق للمورد = **450**.
- المخزون = **10 قطع**.
- قيمة المخزون = **1,000**.
- لا يتغير المخزون بسبب الدفع.

إذا كانت طريقة الدفع تستخدم Bridging account، تكون هناك مرحلة حساب وسيط حسب الإعداد.

**16. تسوية الدفع على الفواتير**

خصص مبلغ 700:

| المستند | الرصيد قبل التخصيص | المخصص | المتبقي |
|---|---:|---:|---:|
| INV-001 | 690 | 690 | 0 |
| INV-002 | 460 | 10 | 450 |
| الدفع | 700 | 700 | 0 |

الجداول:

| الجدول | الأثر |
|---|---|
| `VendSettlement` | روابط ومبالغ وتاريخ التسوية |
| `VendTrans` | تحديث بيانات التسوية والإغلاق |
| `VendTransOpen` | إزالة المغلق وتخفيض المفتوح |
| `SpecTrans` | تعليم الحركات أثناء التحضير، حسب المسار |

**الدفع يخفض صافي رصيد المورد، والتسوية تحدد الفواتير التي غطاها الدفع.**

في مثالنا لا يوجد قيد إضافي لمجرد التخصيص؛ لكن التسوية قد تولد قيود خصم نقدي أو فروق عملة أو فروق حسابات وأبعاد. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/cash-bank-management/settlement-overview?utm_source=chatgpt.com)

**17. دفع وتسوية الباقي: 450**

القيد:

| الحساب | مدين | دائن |
|---|---:|---:|
| ذمم الموردين | 450 | — |
| البنك | — | 450 |

سوِّ الدفع على المتبقي من `INV-002`.

النتيجة النهائية للمثال:

| المؤشر | القيمة |
|---|---:|
| المخزون | 10 قطع |
| قيمة المخزون | 1,000 |
| ضريبة المدخلات | 150 |
| إجمالي المدفوعات | 1,150 |
| رصيد المورد | 0 |
| الرصيد المفتوح لمستندات المثال | 0 |
| GRNI لمستندات المثال | 0 |

تبقى الفواتير والمدفوعات في `VendTrans`، ويبقى تاريخ التسويات محفوظًا.

**18. ملخص حركة المثال الأساسي**

الأرقام المعروضة للمورد تمثل صافي المستحق كمبلغ موجب.

| المرحلة | المخزون الفعلي | الكمية المفوترة | المتبقي للاستلام | المستحق للمورد |
|---|---:|---:|---:|---:|
| إنشاء PO | 0 | 0 | 10 | 0 |
| تأكيد PO | 0 | 0 | 10 | 0 |
| استلام 6 | 6 | 0 | 4 | 0 |
| فاتورة 6 | 6 | 6 | 4 | 690 |
| استلام 4 | 10 | 6 | 0 | 690 |
| فاتورة 4 | 10 | 10 | 0 | 1,150 |
| دفع 700 | 10 | 10 | 0 | 450 |
| تخصيص 700 | 10 | 10 | 0 | 450 |
| دفع وتسوية 450 | 10 | 10 | 0 | 0 |

**19. شراء الخدمات**

مثال مستقل: خدمة صيانة **200 + ضريبة 30 = 230**.

يمكن تنفيذها عبر:

| المسار | الاستخدام |
|---|---|
| Service item غير مخزني على PO | طلب خدمة مع أمر شراء |
| Procurement category على PO | شراء خدمة حسب الفئة |
| Non-PO vendor invoice | فاتورة خدمة دون PO |
| Vendor invoice journal | فاتورة مالية مباشرة |

القيد النهائي:

| الحساب | مدين | دائن |
|---|---:|---:|
| مصروف الصيانة | 200 | — |
| ضريبة المدخلات | 30 | — |
| ذمم الموردين | — | 230 |

ثم الدفع والتسوية مثل المثال الأساسي.

- لا توجد كمية On-hand أوحركة تكلفة مخزنية للخدمة غير المخزنية.
- قبول الخدمة قد يثبت استحقاقًا قبل الفاتورة حسب الإعداد.
- عند استخدام Invoice journal، يكون الإدخال عبر `LedgerJournalTable` و`LedgerJournalTrans`، ولا تفترض مروره بجميع جداول فاتورة PO.

وتوفر D365 مسارات متعددة لإدخال فواتير المورد، منها الفواتير المرتبطة بأمر شراء والفواتير المباشرة وسجل الفواتير. [GitHub](https://github.com/MicrosoftDocs/dynamics365-guidance/blob/main/guidance/business-processes/source-to-pay-manage-accounts-payable-overview.md?utm_source=chatgpt.com)

**20. Invoice register ثم Invoice approval**

هذا مسار بديل للفواتير التي تحتاج تسجيلًا أوليًا ثم استكمال التوزيع والاعتماد:

1. إدخال الفاتورة في Invoice register.
2. ترحيلها وفق حساب التسجيل المؤقت.
3. استكمالها في مسار Approval/Pool المناسب.
4. اعتماد الحسابات والتوزيعات النهائية.
5. ترحيل الاستكمال وتصفية الأثر المؤقت.
6. الدفع والتسوية.

اختبر أن التزام المورد لا يتكرر وأن الحساب المؤقت يُصفّى عند استكمال المعالجة.

**21. الدفعة المقدمة للمورد**

مثال مستقل: دفع مقدمة **300** قبل الشراء، دون ضريبة على المقدمة لتبسيط المثال.

يوجد مساران وظيفيان:

| المسار | الفكرة |
|---|---|
| Prepayment journal voucher | دفع مقدم باستخدام إعدادات ترحيل المقدمة |
| PO prepayment invoice | تعريف مقدمة على PO ثم فاتورتها ودفعها وتطبيقها |

الأثر المبسط للدفع المقدم:

| الحساب | مدين | دائن |
|---|---:|---:|
| دفعات مقدمة للموردين | 300 | — |
| البنك | — | 300 |

بعد فاتورة نهائية بقيمة 1,150، طبّق المقدمة:

| الحساب | مدين | دائن |
|---|---:|---:|
| ذمم الموردين | 300 | — |
| دفعات مقدمة للموردين | — | 300 |

المتبقي للدفع = **850**.

اختبر التطبيق الجزئي، ومنع تطبيق المقدمة مرتين، ومعالجة الضريبة والإلغاء حسب المسار المستخدم.

**22. مرتجع مشتريات فعلي**

مثال مستقل: إرجاع قطعتين بتكلفة 100 للقطعة.

| البيان | القيمة |
|---|---:|
| كمية المرتجع | 2 |
| قيمة البضاعة | 200 |
| الضريبة | 30 |
| Credit note | 230 |

الدورة:

1. إنشاء أمر مرتجع/سطور شراء سالبة.
2. الإشارة للفاتورة الأصلية.
3. مراجعة الأبعاد وربط التكلفة وMarking عند استخدامه.
4. تأكيد الأمر.
5. ترحيل خروج الكمية عبر استلام سالب.
6. ترحيل الإشعار الدائن.
7. تسويته على فاتورة مفتوحة أو استرداد المبلغ من المورد.

الأثر النهائي المفترض:

| الحساب | مدين | دائن |
|---|---:|---:|
| ذمم الموردين | 230 | — |
| المخزون | — | 200 |
| ضريبة المدخلات | — | 30 |

الجداول الرئيسية هي جداول PO والاستلام والفاتورة والمخزون والمورد والتسوية نفسها.

**إذا كانت الفاتورة مدفوعة بالكامل، ينشأ رصيد مستحق لنا على المورد.**

**23. إشعار دائن دون إرجاع بضاعة**

مثال: حسم مالي **100 + ضريبة 15**.

| العنصر | الأثر |
|---|---|
| الكمية | لا تتغير |
| مستحق المورد | ينخفض 115 |
| الضريبة | تُعدل حسب المعالجة |
| مقابل الـ100 | تخفيض تكلفة مخزون أو مصروف أو حساب حسم حسب طبيعة العملية |

اختر مسار التصحيح الذي يحقق التوزيع المطلوب. **إدخال كمية مرتجع سالبة سيؤثر في المخزون رغم عدم خروج البضاعة.**

**24. الخصم النقدي والدفع الناقص والزائد**

أمثلة مستقلة:

| الحالة | المثال | النتيجة |
|---|---|---|
| خصم نقدي | فاتورة 1,000، دفع 980، خصم معتمد 20 | إغلاق الفاتورة وإثبات الخصم |
| دفع ناقص | فاتورة 1,000، دفع 980 دون خصم معتمد | بقاء 20 مفتوحة |
| فرق مسموح | فرق 1 ضمن حدود معالجة الفروق | إغلاقه بحساب الفرق حسب الإعداد |
| دفع زائد | فاتورة 1,000، دفع 1,050 | بقاء 50 رصيدًا لنا على المورد |

تُراجع معالجة الضريبة على الخصم حسب إعدادات الضريبة؛ ولا يؤثر الدفع نفسه في كمية المخزون.

**25. اختلاف سعر الفاتورة والمصاريف الإضافية**

مثال فرق السعر:

| البيان | القيمة |
|---|---:|
| استلام 10 × 100 | 1,000 |
| فاتورة 10 × 110 | 1,100 |
| فرق السعر | 100 |

بعد قبول الفرق، تصبح تكلفة الشراء المالية 1,100. إذا بقيت البضاعة كلها دون حركات أخرى، نتوقع قيمة مخزون 1,100، مع مراعاة الإعدادات.

في Weighted average، افحص تحديث تكلفة الاستلام وتعديلات الصرف والإقفال؛ لا تفترض أن الفرق يُحمَّل تلقائيًا لحساب فرق سعر كما في Standard cost.

مثال شحن 100:

| المعالجة | قيمة المخزون | مصروف الشحن |
|---|---:|---:|
| رسملة الشحن على البضاعة | 1,100 | 0 |
| تحميل الشحن مصروفًا | 1,000 | 100 |

جداول المصاريف الأساسية: `MarkupTable` و`MarkupTrans`، مع جداول المستند والمخزون وGL بحسب طريقة الترحيل.

**26. الشراء بعملة أجنبية**

مثال مستقل دون ضريبة:

| البيان | القيمة |
|---|---:|
| الفاتورة | 1,000 USD |
| سعر الفاتورة | 3.75 SAR |
| الالتزام المحاسبي | 3,750 SAR |
| سعر الدفع | 3.80 SAR |
| مبلغ البنك | 3,800 SAR |
| خسارة العملة المحققة | 50 SAR |

القيد المجمع عند الدفع والتسوية:

| الحساب | مدين | دائن |
|---|---:|---:|
| ذمم الموردين | 3,750 | — |
| خسارة عملة محققة | 50 | — |
| البنك | — | 3,800 |

إذا بقيت الفاتورة مفتوحة بنهاية الفترة، نفّذ اختبار إعادة تقييم العملة ثم تحقق من معالجة الفروق عند التسوية.

**27. فك التسوية وعكس الدفع وتصحيح الاستلام**

| العملية | ما الذي يجب أن يحدث؟ |
|---|---|
| Undo settlement | إعادة فتح مبالغ الفاتورة والدفع دون عكس البنك |
| Payment reversal | عكس الأثر المالي للدفع ومعالجة ارتباطاته بالطريقة المدعومة |
| Receipt correction/cancellation | تصحيح الكمية وأثر الاستحقاق ضمن القيود المتاحة |
| Invoice correction | تصحيح الفاتورة بمسار مدعوم مع تعديل الضريبة والمورد والتكلفة |
| Vendor refund | استلام المبلغ من المورد وتسويته على رصيدنا لديه |

لا تستخدم تحديثات SQL مباشرة لتغيير حالات الترحيل أو حذف الحركات.

**28. المطابقات والإقفال**

| المطابقة | النتيجة المطلوبة |
|---|---|
| Vendor statement | الفواتير والمدفوعات والإشعارات والرصيد صحيحة |
| Vendor aging | الأرصدة موزعة حسب تاريخ الاستحقاق المحدد |
| AP to GL | إجمالي ذمم الموردين يطابق حسابات الرقابة |
| GRNI | الرصيد يفسر الاستلامات غير المفوترة |
| Inventory to GL | قيمة المخزون تطابق الحسابات ضمن نطاق وتاريخ موحدين |
| Bank reconciliation | المدفوعات والمبالغ المرتجعة مطابقة لكشف البنك |
| Tax reconciliation | الضريبة مطابقة للفواتير والإشعارات وحساباتها |
| Prepayments | المدفوع والمطبق والمتبقي قابل للتتبع |

ثم:

1. استكمال المستندات والتصحيحات المطلوبة.
2. مراجعة الاستلامات غير المفوترة.
3. تنفيذ إعادة تقييم العملة عند الحاجة.
4. تنفيذ Inventory recalculation/close وفق سياسة الفترة.
5. مراجعة فروق وتعديلات التكلفة.
6. اعتماد المطابقات وإقفال الفترات.

في **Weighted average الدوري**، يسوّي Inventory close تكلفة الصرف وفق النموذج؛ وقد يعدّل قيمة المخزون وتكلفة الصرف، دون تغيير مبلغ فاتورة المورد بسبب هذه التسوية. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/weighted-average-physical-value-marking?utm_source=chatgpt.com)

**29. قائمة اختبار الدورة الأساسية، خطوةً خطوة**

ابدأ جميع النتائج بـ **Not run**، ثم سجل النتيجة الفعلية.

| ID | العملية التي تختبرها | شرط النجاح |
|---|---|---|
| AP-01 | مراجعة الإعدادات | الحسابات والضريبة والفترات صحيحة |
| AP-02 | إنشاء المورد | بيانات وشروط دفع صحيحة |
| AP-03 | تجهيز الصنف | Stocked + Weighted average + أبعاد صحيحة |
| AP-04 | إنشاء PO لعشر قطع | صافي 1,000 وإجمالي 1,150 |
| AP-05 | اعتماد PO | تطبيق دورة الاعتماد |
| AP-06 | تأكيد PO | مستند تأكيد، دون زيادة المخزون |
| AP-07 | تسجيل الوصول عند اشتراطه | Registered وأبعاد مكتملة |
| AP-08 | استلام 6 | المخزون 6 والمتبقي 4 |
| AP-09 | فحص قيد الاستلام | استحقاق 600 حسب الإعداد |
| AP-10 | إدخال فاتورة أولى | مسودة 690 مرتبطة بالاستلام |
| AP-11 | مطابقة الفاتورة | الكمية والسعر والضريبة صحيحة |
| AP-12 | ترحيل الفاتورة الأولى | مورد 690 وتكلفة مالية 600 |
| AP-13 | استلام 4 | المخزون 10 والمتبقي صفر |
| AP-14 | ترحيل الفاتورة الثانية | 460 وإكمال فوترة PO |
| AP-15 | مراجعة إجمالي المورد | 1,150 |
| AP-16 | دفع 700 | مورد مدين وبنك دائن |
| AP-17 | تسوية 700 | الأولى مغلقة والثانية متبقيها 450 |
| AP-18 | دفع وتسوية 450 | أرصدة المثال مغلقة |
| AP-19 | مطابقة المخزون | 10 قطع بقيمة 1,000 |
| AP-20 | مطابقة الضريبة | 150 |
| AP-21 | مطابقة AP مع GL | لا فرق غير مفسر |
| AP-22 | مطابقة البنك | إجمالي انخفاض 1,150 |
| AP-23 | مراجعة GRNI | صفر لمستندات المثال |
| AP-24 | تنفيذ الإقفال والمراجعة | تكلفة وأرصدة صحيحة |

**30. الاختبارات المكملة لإكمال نطاق AP**

| المجموعة | الحالات المطلوبة |
|---|---|
| الشراء | شراء كامل، استلام جزئي، فوترة جزئية، إلغاء المتبقي |
| المطابقة | فرق كمية، فرق سعر، ضريبة خاطئة، فاتورة مكررة |
| الخدمات | خدمة على PO، فئة شراء، فاتورة مباشرة، Register/Approval |
| الدفع | كامل، جزئي، دون تسوية، دفعة لعدة فواتير، دفعات لفاتورة |
| المقدمات | مقدمة وتطبيق كامل وجزئي |
| الفروق | خصم نقدي، دفع زائد، دفع ناقص |
| المرتجعات | مرتجع فعلي، إشعار مالي دون كمية، استرداد من المورد |
| العملة | فاتورة أجنبية، إعادة تقييم، فرق محقق عند التسوية |
| التصحيح | فك تسوية، عكس دفع، تصحيح استلام وفاتورة |
| الإقفال | Aging، AP/GL، GRNI، مخزون/GL، بنك، ضريبة |
| سلامة النظام | منع الطلب المكرر، الترحيل الجزئي، تجاوز الكمية، والتسويات المتزامنة |

**طريقة تسجيل Passed / Failed / Fixed**

| الاختبار | المحاولة | الحالة | الدليل |
|---|---:|---|---|
| Product receipt | 1 | Failed | رسالة الخطأ ورقم PO |
| إصلاح سبب فشل الاستلام | — | Fixed — awaiting retest | وصف الإصلاح |
| Product receipt | 2 | Passed | مستند الاستلام + حركة المخزون + القيد |
| Vendor invoice | 1 | Passed | الفاتورة + حركة المورد + الضريبة + GL |
| Settlement | 1 | Passed | تاريخ التسوية والرصيد المتبقي |

**اكتمال الدورة يعني تطابق المستندات والكميات والتكلفة وذمم المورد والبنك والضريبة وGL، مع نجاح إعادة الاختبار بعد أي إصلاح.**