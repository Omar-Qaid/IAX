أريد اختبار دورة **الحسابات المدينة Accounts Receivable** وفق منطق D365 F&O، بدءًا من **إنشاء أمر البيع، مرورًا بجميع العمليات، حتى التحصيل والتسوية النهائية**، مع التحقق من الأثر المالي والمخزني لكل خطوة.

يُنفذ الاختبار بالتسلسل التالي:

1. إنشاء أمر البيع `Sales order`.
2. تأكيد أمر البيع `Confirmation`.
3. حجز الكميات `Reservation`.
4. تجهيز والتقاط الأصناف `Picking`، حسب مسار العمل.
5. ترحيل سند التسليم `Packing slip`.
6. ترحيل فاتورة البيع `Sales invoice`.
7. تسجيل وترحيل تحصيل العميل `Customer payment`.
8. تسوية التحصيل مع الفاتورة `Settlement`.
9. التحقق النهائي من رصيد العميل والمبالغ المفتوحة والبنك والقيود وكميات المخزون وتكلفته.

**لكل خطوة، حدّد:**

- العملية والخدمة المسؤولة عنها `Service / API`.
- بيانات الاختبار والنتيجة المتوقعة والفعلية.
- المستندات والجداول المتأثرة.
- **الأثر المالي:** القيد المدين والدائن والمبالغ، وتأثير العملية على العميل والبنك والإيراد والضريبة وتكلفة المبيعات.
- **الأثر المخزني:** الكمية الفعلية والمحجوزة والمتاحة، وحالة حركة المخزون، والتحديث الفعلي والمالي للتكلفة.
- حالة الاختبار: `Passed / Failed / Blocked / Not tested`.

إذا لم يكن للخطوة أثر مالي أو مخزني، اذكر ذلك بوضوح مع السبب.

**سجّل التنفيذ بهذا الشكل:**

1. إنشاء أمر البيع — `Passed`.
2. تأكيد أمر البيع — `Passed`.
3. حجز الكمية — `Passed`.
4. تجهيز الأصناف — `Passed`.
5. ترحيل سند التسليم — `Failed`.
   - سبب الفشل: …
   - الإصلاح المنفذ: … — `Fixed`.
   - إعادة اختبار ترحيل سند التسليم — `Passed`.
6. ترحيل فاتورة البيع — …
7. ترحيل التحصيل — …
8. التسوية — …
9. المطابقة النهائية — …

هذه الحالات مثال على التنسيق فقط؛ لا تُسجّل `Passed` دون تنفيذ الاختبار والتحقق من نتائجه.

اختبر المسار أولًا **بتسليم وفوترة وتحصيل كاملين**، ثم كرّره **بتسليم وفوترة وتحصيل جزئيين**، مع متابعة الكميات والمبالغ المتبقية بعد كل عملية.

لا تعتبر الدورة مكتملة إلا بعد مطابقة المستندات وحركات المخزون والقيود المحاسبية، والتأكد من صحة المبالغ المفتوحة وإقفال الفاتورة بعد تسويتها بالكامل، وعدم وجود أي تكرار ناتج عن إعادة المحاولة.


نعم، التركيز سيكون على **الحسابات المدينة Accounts Receivable في D365 F&O**، من الإعداد حتى التحصيل والإقفال، مع الجداول الأساسية والأثر الفعلي والمالي لكل مرحلة.

هذه خريطة للدورة القياسية في النظام؛ أما تأكيد إعدادات بيئتكم ونتائج الترحيل الفعلية فيحتاج اختبارها داخل النظام.

**أولًا: حدود الدورة والفرق بين Physical وFinancial**

دورة العميل تتكامل مع المبيعات والمخزون والبنوك والأستاذ العام:

| المسار | الدورة |
|---|---|
| بيع أصناف مخزنية | عميل ← أمر بيع ← حجز وتجهيز ← تسليم ← فاتورة ← تحصيل ← تسوية |
| بيع خدمات بأمر بيع | عميل ← أمر بيع خدمة ← فاتورة ← تحصيل ← تسوية |
| فاتورة مباشرة | عميل ← Free text invoice ← تحصيل ← تسوية |
| دفعة مقدمة | عميل ← تحصيل مقدم ← فاتورة لاحقًا ← تطبيق وتسوية الدفعة |
| مرتجع | مرجع الفاتورة الأصلية ← أمر مرتجع ← استلام عند الحاجة ← إشعار دائن ← تسوية أو رد مبلغ |

الحسابات المدينة تدير **دين العميل وتحصيله**، بينما الأثر المخزني يأتي من مستندات المبيعات المتكاملة معها.

| المصطلح | المعنى |
|---|---|
| Physical inventory update | خروج أو دخول فعلي للصنف، مثل ترحيل التسليم أو استلام المرتجع |
| Financial inventory update | تحديث تكلفة الحركة ماليًا عند الفوترة |
| AR financial posting | تسجيل مديونية العميل أو التحصيل أو التسوية |
| GL posting | إنشاء القيود في الأستاذ العام |

**يمكن أن ينشأ قيد محاسبي عند التسليم، رغم أن الحركة ما زالت Physical في المخزون.** كما أن فاتورة الخدمة والتحصيل يؤثران ماليًا دون حركة مخزون. حالات المخزون المعتادة للبيع هي `Deducted` عند التسليم و`Sold` عند الفوترة. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/physical-financial-updates?utm_source=chatgpt.com)

**المرحلة 0 — تحديد السياسة المحاسبية ونطاق العمل**

قبل إدخال الإعدادات، حدّد:

- البيع الآجل والنقدي والعملات المستخدمة.
- العملاء المحليين والخارجيين والشركات الشقيقة.
- الأصناف المخزنية والخدمات والفواتير المباشرة.
- حدود الائتمان وإيقاف العملاء.
- شروط السداد والأقساط والخصم النقدي.
- الضرائب والرسوم والدفعات المقدمة.
- حسابات العملاء والإيرادات والمخزون والتكلفة والبنوك.
- مسؤوليات إنشاء العميل والفوترة والتحصيل واعتماد التسويات.

| أساس النظام | الجداول الرئيسية |
|---|---|
| الشركة والدفتر | `CompanyInfo`, `Ledger` |
| دليل الحسابات | `MainAccount` |
| الفترات المالية | `FiscalCalendar`, `FiscalCalendarYear`, `FiscalCalendarPeriod` |
| الأبعاد المالية | `DimensionAttribute`, `DimensionAttributeValue`, `DimensionAttributeValueSet`, `DimensionAttributeValueCombination` |

**الأثر:** تعريف قواعد العمل فقط؛ لا مديونية ولا حركة مخزون.

**المرحلة 1 — Configuration: إعداد الحسابات المدينة**

تشمل الإعدادات القياسية مجموعات العملاء، الترحيل، السداد، اليوميات، العملات، الضرائب، الائتمان والتحصيل. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/accounts-receivables-set-up-overview?utm_source=chatgpt.com)

| الإعداد / الشاشة | ما يتم تحديده | الجداول الرئيسية |
|---|---|---|
| Accounts receivable parameters | الترحيل الافتراضي، التحديثات، التسويات والترقيم | `CustParameters` |
| Number sequences | أرقام العملاء والفواتير والقسائم والمستندات | `NumberSequenceTable`, `NumberSequenceReference` |
| Customer groups | تصنيف العملاء والقيم الافتراضية | `CustGroup` |
| Customer posting profiles | حساب مراقبة العملاء حسب العميل أو المجموعة | `CustLedger`, `CustLedgerAccounts` |
| Terms of payment | حساب تاريخ الاستحقاق | `PaymTerm` |
| Payment schedules | توزيع الاستحقاق على أقساط | `PaymSched`, `PaymSchedLine` |
| Methods of payment — customers | تحويل، نقد، شيك، حساب مقابل أو وسيط | `CustPaymModeTable` |
| Cash discounts | الخصم مقابل السداد المبكر | `CashDisc` |
| Journal names | أنواع يوميات التحصيل والفواتير والقيود | `LedgerJournalName` |
| Currencies and exchange rates | العملات وأسعار الصرف | `Currency`, `ExchangeRateType`, `ExchangeRateCurrencyPair`, `ExchangeRate` |
| Sales tax | الأكواد والمجموعات وحسابات الضريبة | `TaxTable`, `TaxGroupHeading`, `TaxGroupData`, `TaxItemGroupHeading`, `TaxOnItem` |
| Charges | رسوم النقل والخدمات وغيرها | `MarkupTable`, `MarkupAutoTable`, `MarkupAutoLine` |
| Delivery setup | طريقة وشروط التسليم | `DlvMode`, `DlvTerm` |

**قاعدة الترحيل الأساسية:**

- حساب مراقبة العملاء يأتي من **Customer posting profile**.
- إيراد أمر البيع وتكلفته يأتيان من إعدادات ترحيل المبيعات والمخزون.
- إيراد الفاتورة المباشرة يأتي عادة من الحساب المحدد على سطرها.
- البنك أو الحساب الوسيط يأتي من إعدادات الدفع واليومية.

أولوية البحث في Customer posting profile هي: **عميل محدد `Table` ثم مجموعة `Group` ثم الجميع `All`**. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/customer-posting-profiles?utm_source=chatgpt.com)

**المرحلة 2 — إعداد تكامل المبيعات والمخزون**

هذه المرحلة مطلوبة للبيع المخزني، ولا تكفي إعدادات AR وحدها.

| الإعداد | الغرض | الجداول الرئيسية |
|---|---|---|
| Released products | تعريف الصنف ووحداته وخصائصه | `EcoResProduct`, `InventTable`, `InventTableModule` |
| Item groups | تصنيف الأصناف وربط الترحيل | `InventItemGroup`, `InventItemGroupItem` |
| Item model groups | السياسة المخزنية والتكلفة والترحيل | `InventModelGroup`, `InventModelGroupItem` |
| Inventory posting | المخزون، التكلفة، الإيراد، وحسابات التسليم | `InventPosting` |
| Inventory parameters | ضوابط التحديث والترحيل | `InventParameters` |
| Sites and warehouses | موقع ومستودع الصرف | `InventSite`, `InventLocation` |
| Inventory dimensions | أبعاد الحركة | `InventDim` |
| Prices and discounts | أسعار واتفاقيات البيع | `PriceDiscTable` |

لترحيل تكلفة التسليم إلى الأستاذ العام، تُراجع إعدادات `Post packing slip in ledger` و`Post physical inventory` وحسابات تكلفة الوحدات المسلّمة وتكلفة البضاعة المسلّمة. توجد معالجة خاصة للأصناف ذات التكلفة المعيارية. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/sales-order-posting?utm_source=chatgpt.com)

**المرحلة 3 — Master data: إنشاء العميل**

المسار المعتاد: **Accounts receivable > Customers > All customers**.

| البيانات | مثال | الجداول |
|---|---|---|
| حساب العميل ومجموعته | `C-0001` / محلي | `CustTable`, `CustGroup` |
| الاسم والهوية | شركة النور التجارية | `DirPartyTable` |
| العناوين | عنوان الفوترة والتسليم | `DirPartyLocation`, `LogisticsLocation`, `LogisticsPostalAddress` |
| وسائل التواصل | هاتف وبريد | `LogisticsElectronicAddress` |
| جهات الاتصال | مسؤول المشتريات والمحاسب | `ContactPerson` مع جداول الأطراف |
| حساب العميل البنكي | IBAN | `CustBankAccount` |
| العملة وشروط السداد | SAR / 30 يومًا | قيم مرجعية على `CustTable` |
| الضرائب | مجموعة ضريبة العميل ورقمه الضريبي | `CustTable` والجداول الضريبية |
| الائتمان | حد ائتمان وحالة إيقاف | `CustTable` وإعدادات Credit management عند استخدامها |
| الأبعاد | فرع، قسم، وحدة أعمال | `DefaultDimension` وجداول الأبعاد |

انتبه إلى الفرق بين:

- **Order account:** العميل الذي يطلب البضاعة.
- **Invoice account:** العميل الذي تتحمل ذمته قيمة الفاتورة.

قد يكونان حسابين مختلفين.

**الأثر:** إنشاء العميل لا يُنشئ رصيدًا ماليًا.

**المرحلة 4 — الأرصدة الافتتاحية**

عند الانتقال من نظام سابق، يُفضّل تحميل الفواتير المفتوحة بتفاصيلها: رقم الفاتورة، التاريخ، الاستحقاق، العملة والمتبقي.

| العملية | الجداول المتأثرة |
|---|---|
| تجهيز يومية الأرصدة | `LedgerJournalTable`, `LedgerJournalTrans` |
| ترحيل حركات العملاء | `CustTrans` |
| المبالغ المفتوحة | `CustTransOpen` |
| القيود المحاسبية | `GeneralJournalEntry`, `GeneralJournalAccountEntry` |

القيد المبسط:

| مدين | دائن |
|---|---|
| العملاء | حساب مقابل للأرصدة الافتتاحية |

**لا تسجل إيراد المبيعات مرة أخرى** إذا كانت هذه الفواتير تخص فترات سابقة تم ترحيل نتائجها ضمن الأرصدة الافتتاحية.

**المرحلة 5 — عرض السعر وأمر البيع**

| الخطوة | الجداول الرئيسية | الأثر |
|---|---|---|
| عرض السعر — اختياري | `SalesQuotationTable`, `SalesQuotationLine` | لا مديونية ولا صرف |
| إنشاء أمر البيع | `SalesTable`, `SalesLine` | طلب بيع والتزام تشغيلي |
| حركة المخزون للصنف المخزني | `InventTransOrigin`, `InventTrans` | كمية مطلوبة للصرف |
| تأكيد أمر البيع | `CustConfirmJour`, `CustConfirmTrans` | حفظ مستند التأكيد |

مثال: بيع **10 وحدات × 100 ريال**.

عند إنشاء الأمر وتأكيده:

- لا تُسجل فاتورة على العميل.
- لا يتحقق إيراد البيع في الدورة المعتادة.
- لا تنخفض الكمية الفعلية بسبب التأكيد وحده.
- قد يتغير التعرض الائتماني حسب إعدادات فحص الائتمان.

**المرحلة 6 — الحجز والتجهيز**

| العملية | الجداول الرئيسية | الأثر الفعلي والمالي |
|---|---|---|
| حجز الكمية | `InventTrans`, `InventSum`, `InventDim` | يقل المتاح للحجز؛ لا يوجد تسليم بعد |
| قائمة التجهيز في المخزون الأساسي | `WMSPickingRoute`, `WMSOrderTrans` بحسب المسار | تجهيز وتحديث حالة الالتقاط |
| تحديث الحجز أو الالتقاط | `InventTrans` | حالات مثل `ReservPhysical` و`Picked` |

**الحجز لا يعني خروج البضاعة، والالتقاط لا يعني تسجيل مديونية العميل.** أما WHS المتقدم فيضيف جداول وإجراءات أخرى.

**المرحلة 7 — التسليم: Packing slip**

عند ترحيل التسليم للصنف المخزني:

| الجانب | النتيجة |
|---|---|
| المستند | إنشاء سند تسليم |
| الكمية | تنخفض الكمية الفعلية المسلّمة |
| حالة الصرف | `StatusIssue = Deducted` |
| العميل | لا تنشأ فاتورة مدينة من التسليم وحده |
| المحاسبة | قد تنشأ قيود تكلفة فعلية وفق الإعداد |

**الجداول المتأثرة:**

`CustPackingSlipJour`, `CustPackingSlipTrans`, `SalesLine`, `InventTrans`, `InventSum`، وجداول الأستاذ العام عند تفعيل الترحيل.

إذا كانت تكلفة الوحدات العشر **600 ريال**، فالقيد التوضيحي للتسليم:

| مدين | دائن | المبلغ |
|---|---|---:|
| تكلفة بضاعة مسلّمة غير مفوترة | حساب تكلفة الوحدات المسلّمة | 600 |

هذه حسابات انتقالية حسب تصميم الترحيل. توجد أيضًا إعدادات اختيارية لترحيل الإيراد والضريبة المؤجلين عند التسليم. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/sales-order-posting?utm_source=chatgpt.com)

**المرحلة 8 — فاتورة أمر البيع: Sales invoice**

| الجانب | النتيجة |
|---|---|
| العميل | تسجيل المديونية وتاريخ الاستحقاق |
| الإيراد | تسجيل المبيعات والخصومات والرسوم |
| الضريبة | تسجيل الضريبة حسب إعداداتها |
| المخزون | تحديث مالي للكمية المفوترة؛ الحالة `Sold` |
| تكلفة المبيعات | تسجيل التكلفة المالية وعكس قيود التسليم المرتبطة |

**الجداول الرئيسية:**

| الوظيفة | الجداول |
|---|---|
| الفاتورة المرحلة | `CustInvoiceJour`, `CustInvoiceTrans` |
| حركة العميل | `CustTrans` |
| المبلغ المفتوح | `CustTransOpen` |
| أمر البيع | `SalesTable`, `SalesLine` |
| المخزون والتكلفة | `InventTrans`, `InventSum` |
| الضرائب والرسوم | `TaxTrans`, `MarkupTrans` |
| الأستاذ العام | `GeneralJournalEntry`, `GeneralJournalAccountEntry` |

**مثال توضيحي:** صافي البيع 1,000، وضريبة افتراضية 15% بقيمة 150، والتكلفة 600.

| مدين | دائن | المبلغ |
|---|---|---:|
| العملاء | — | 1,150 |
| — | إيرادات المبيعات | 1,000 |
| — | ضريبة المخرجات | 150 |
| تكلفة المبيعات | المخزون | 600 |

تُعكس قيود تكلفة التسليم السابقة أيضًا. **الفاتورة لا تصرف الوحدات العشر فعليًا مرة ثانية إذا سبق تسليمها.**

**المرحلة 9 — الفاتورة المباشرة: Free text invoice**

تُستخدم مثلًا لفوترة إيجار أو استشارة أو رسوم لا تحتاج دورة بيع وصرف صنف.

| المرحلة | الجداول |
|---|---|
| إدخال رأس الفاتورة | `CustInvoiceTable` |
| إدخال السطور | `CustInvoiceLine` |
| الفاتورة المرحلة | `CustInvoiceJour`, `CustInvoiceTrans` |
| حركة العميل والمفتوح | `CustTrans`, `CustTransOpen` |
| الضريبة والقيد | `TaxTrans`, `GeneralJournalEntry`, `GeneralJournalAccountEntry` |

اسم جدول سطور الإدخال هو **`CustInvoiceLine`**؛ ويختلف عن `CustInvoiceTrans` الخاص بسطور الفاتورة المرحلة. [Microsoft Learn](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountsreceivable/worksheetline/custinvoiceline?utm_source=chatgpt.com)

القيد المعتاد:

| مدين | دائن |
|---|---|
| العملاء بقيمة الإجمالي | الإيراد والضريبة |

يُحدد حساب الإيراد والأبعاد على السطر. **لا توجد حركة `InventTrans` أو تكلفة مخزون بسبب Free text invoice نفسها.** [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/create-free-text-invoice-new?utm_source=chatgpt.com)

**المرحلة 10 — التحصيل: Customer payment**

المسار المعتاد: **Accounts receivable > Payments > Customer payment journal**.

| الخطوة | الجداول الرئيسية |
|---|---|
| إدخال اليومية | `LedgerJournalTable`, `LedgerJournalTrans` |
| ترحيل التحصيل على العميل | `CustTrans` |
| إبقاء دفعة غير مسواة | `CustTransOpen` |
| حركة البنك عند الترحيل إلى حساب بنكي | `BankAccountTrans` |
| قيد الأستاذ العام | `GeneralJournalEntry`, `GeneralJournalAccountEntry` |

القيد المبسط لتحصيل **700 ريال**:

| مدين | دائن | المبلغ |
|---|---|---:|
| البنك | العملاء | 700 |

يمكن أن يمر التحصيل بحساب وسيط بدل البنك مباشرة، حسب طريقة الدفع. **لا يتولد إيراد جديد من تحصيل فاتورة سبق ترحيلها.** [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/accts-recvble-posting?utm_source=chatgpt.com)

**المرحلة 11 — التسوية: Settlement**

التسوية تربط الدفعة بالفاتورة أو تربط الإشعار الدائن بالفاتورة.

| الجدول | دوره |
|---|---|
| `CustTrans` | أصل حركات الفواتير والدفعات والتعديلات |
| `CustTransOpen` | المبالغ التي ما زالت مفتوحة |
| `CustSettlement` | سجل مبالغ وروابط التسوية |
| `SpecTrans` | تعليم الحركات المختارة للتسوية أثناء المعالجة |

هذه الأدوار الأساسية لحركات العملاء والمفتوح والتسويات موثقة لدى Microsoft. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/finance-insights/tables-detail?utm_source=chatgpt.com)

في مثالنا:

| البيان | المبلغ |
|---|---:|
| الفاتورة | 1,150 |
| التحصيل الذي تمت تسويته | 700 |
| المتبقي على الفاتورة | 450 |

- التسوية الجزئية تُبقي المتبقي مفتوحًا.
- التسوية الكاملة تُغلق المبلغ المفتوح مع الاحتفاظ بتاريخ الحركة.
- قد تُنشئ التسوية قيود خصم نقدي، فروق عملة، فروق صغيرة أو موازنة حسابات وأبعاد.
- يمكن ترحيل الدفعة أولًا وتسويتها لاحقًا. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/cash-bank-management/settlement-overview?utm_source=chatgpt.com)

**رصيد العميل صفر لا يضمن أن جميع حركاته مسوّاة:** قد تبقى فاتورة ودفعة متساويتان مفتوحتين إذا لم تتم التسوية.

**المرحلة 12 — الدفعات المقدمة والخصومات والفروقات**

| السيناريو | الأثر المحاسبي المعتاد | الجداول الرئيسية |
|---|---|---|
| دفعة مقدمة | مدين البنك، دائن حساب دفعات العملاء حسب Posting profile | `LedgerJournalTrans`, `CustTrans`, `CustTransOpen` |
| تطبيق المقدّم على الفاتورة | تسوية وإعادة تصنيف عند الحاجة | `CustSettlement`, `CustTrans` وجداول GL |
| خصم سداد مبكر | تخفيض المديونية مع تسجيل الخصم | `CustSettlement`, `CustTransCashDisc`, جداول GL |
| زيادة تحصيل | يبقى رصيد دائن مفتوح إلى أن يطبق أو يرد | `CustTrans`, `CustTransOpen` |
| نقص تحصيل | يبقى المتبقي أو يعالج وفق سياسة الفروق | `CustTransOpen`, `CustSettlement` |
| رد مبلغ للعميل | تخفيض البنك وتسوية الرصيد الدائن | يوميات الدفع، `CustTrans`, `BankAccountTrans` |

تُستخدم الدفعة المقدمة عندما يصل التحصيل قبل وجود فاتورة لتسويته عليها، ويمكن تخصيص Posting profile لها. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/customer-prepayments?utm_source=chatgpt.com)

**المرحلة 13 — المرتجعات والإشعارات الدائنة**

| السيناريو | الأثر الفعلي | الأثر المالي |
|---|---|---|
| مرتجع يعاد للمخزون | استلام الكمية حسب المعالجة | تخفيض المديونية وعكس الإيراد والضريبة والتكلفة ذات الصلة |
| Credit only | لا استلام صنف | تخفيض المديونية دون إعادة كمية |
| مرتجع تالف مع Scrap | يعتمد على قرار معالجة المرتجع | إشعار دائن ومعالجة تكلفة التلف |
| إشعار دائن لخدمة | لا مخزون | عكس الإيراد والضريبة حسب الحالة |

**الجداول:** `SalesTable`, `SalesLine`, `CustPackingSlipJour`, `CustPackingSlipTrans`, `CustInvoiceJour`, `CustInvoiceTrans`, `InventTrans`, `InventSum`, `CustTrans`, `CustTransOpen`, `CustSettlement`, `TaxTrans` وجداول GL.

قرار `Disposition code` يحدد التصرف في البضاعة؛ وخيار `Credit only` يسمح بتخفيض مديونية العميل دون اشتراط رجوعها فعليًا. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/supply-chain/sales-marketing/sales-returns?utm_source=chatgpt.com)

**المرحلة 14 — الائتمان والتحصيل والديون المتأخرة**

| العملية | الأثر | الجداول الأساسية ذات الصلة |
|---|---|---|
| فحص حد الائتمان | السماح أو إيقاف المعالجة حسب السياسة | `CustTable`, `SalesTable` وجداول Credit management |
| Aging | تحليل المفتوح حسب العمر والاستحقاق | `CustTrans`, `CustTransOpen` |
| Collection letters | متابعة المتأخرات؛ وقد توجد رسوم | `CustCollectionLetterJour`, `CustCollectionLetterTrans` |
| Interest notes | احتساب وترحيل فوائد عند استخدامها | `CustInterestJour`, `CustInterestTrans` |
| نزاع العميل | متابعة حالة التحصيل | `CustDisputeHistory` وحركات العميل |
| Write-off | إقفال المبلغ مقابل مصروف أو حساب معتمد | يوميات القيود، `CustTrans`, `CustSettlement`, جداول GL |

**احتساب Aging وحده لا ينشئ قيدًا.** وكذلك متابعة التحصيل لا تعني شطب الدين؛ الشطب عملية مالية مستقلة.

**المرحلة 15 — العملات والإقفال والمطابقة**

| العملية | المطلوب | الأثر |
|---|---|---|
| إعادة تقييم العملات | تقييم المفتوح الأجنبي بسعر الإقفال | فروق عملة غير محققة |
| التسوية بعملة أجنبية | احتساب الفرق عند السداد | فروق عملة محققة حسب الحالة |
| مطابقة AR مع GL | مقارنة دفتر العملاء بحسابات المراقبة | كشف فروق الترحيل |
| مراجعة التسليم غير المفوتر | حصر ما خرج ولم يفوتر | تفسير الحسابات الانتقالية |
| مراجعة الدفعات غير المسواة | تطبيقها على الفواتير الصحيحة | إكمال تسوية المفتوح |
| إقفال المخزون | تسويات تكلفة حسب نموذج الصنف | قد يعدّل تكلفة المبيعات |
| إقفال الفترة | إنهاء الترحيلات والمراجعات | ضبط تاريخ المعالجة |

إعادة تقييم AR تحدّث قيمة المبالغ المفتوحة بالعملة المحاسبية وتسجل فروقًا غير محققة؛ لا تغير أصل قيمة الفاتورة بعملتها الأجنبية. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/cash-bank-management/foreign-currency-revaluation-accounts-payable-accounts-receivable?utm_source=chatgpt.com)

**خريطة الأثر الفعلي والمالي للدورة**

| العملية | كمية المخزون الفعلية | تحديث تكلفة المخزون ماليًا | رصيد العميل | البنك |
|---|---|---|---|---|
| إنشاء العميل | لا | لا | لا | لا |
| عرض السعر وأمر البيع | لا | لا | لا | لا |
| الحجز | لا؛ يتغير المتاح | لا | لا | لا |
| التسليم | تنخفض | تحديث فعلي، وقد يرحل إلى GL | لا عادةً | لا |
| فاتورة البيع المخزني | لا صرف ثانٍ للمسلّم | نعم | يزيد | لا |
| Free text invoice | لا | لا | يزيد | لا |
| التحصيل | لا | لا | ينخفض صافي الرصيد | يزيد أو يمر بوسيط |
| التسوية | لا | لا | تخصيص وإقفال المفتوح | لا تحصيل جديد |
| استلام مرتجع | تزيد إذا أعيد للمخزون | تحديث فعلي | لا من الاستلام وحده | لا |
| إشعار دائن | حسب نوع المرتجع | حسب نوع المرتجع | ينخفض | لا |
| إعادة تقييم العملة | لا | لا | تتغير القيمة المحاسبية | لا |

**التتبع الفني الذي تحتاجه عند التحليل**

| المطلوب تتبعه | نقطة البداية والمسار |
|---|---|
| أمر البيع | `SalesTable` ← `SalesLine` |
| الصرف المخزني | `SalesLine.InventTransId` ← `InventTransOrigin.InventTransId` ← `InventTrans.InventTransOrigin` |
| مستندات التسليم | `CustPackingSlipJour` ← `CustPackingSlipTrans` |
| الفاتورة المرحلة | `CustInvoiceJour` ← `CustInvoiceTrans` |
| الفاتورة المباشرة قبل الترحيل | `CustInvoiceTable` ← `CustInvoiceLine` |
| المفتوح | `CustTransOpen.RefRecId` ← `CustTrans.RecId` مع مراعاة الشركة |
| التسويات | `CustSettlement` ومراجع الحركة المقابلة والشركة |
| القيد المحاسبي | `SubledgerVoucherGeneralJournalEntry` ← `GeneralJournalEntry` ← `GeneralJournalAccountEntry` |

لا تعتمد على `InvoiceId` أو `Voucher` منفردًا للربط؛ راعِ الشركة والتاريخ ومفاتيح العلاقات القياسية. والجداول المذكورة هي **الجداول الأساسية للدورة**؛ قد تضيف الميزات والتوطين والامتدادات جداول أخرى، ولا تُكتب كل هذه الجداول في كل عملية.

**ولاعتبار الدورة مكتملة عمليًا، اختبر على الأقل:**

1. بيع مخزني كامل، ثم بيع بتسليم وفوترة جزئيين.
2. Free text invoice وفاتورة خدمة بأمر بيع.
3. تحصيل كامل وجزئي ودفعة غير مسواة.
4. دفعة مقدمة وتطبيقها على فاتورة.
5. خصم نقدي وزيادة ونقص تحصيل.
6. مرتجع للمخزون و`Credit only`.
7. فاتورة أجنبية وإعادة تقييم وتسوية.
8. عكس تحصيل وفك تسوية بالطريقة المدعومة.
9. Aging ومطابقة العملاء مع GL.
10. مطابقة الكميات والتكلفة والمستندات قبل إقفال الفترة.


نعم، التركيز سيكون على **الحسابات المدينة Accounts Receivable في D365 F&O**، من الإعداد حتى التحصيل والإقفال، مع الجداول الأساسية والأثر الفعلي والمالي لكل مرحلة.

هذه خريطة للدورة القياسية في النظام؛ أما تأكيد إعدادات بيئتكم ونتائج الترحيل الفعلية فيحتاج اختبارها داخل النظام.

**أولًا: حدود الدورة والفرق بين Physical وFinancial**

دورة العميل تتكامل مع المبيعات والمخزون والبنوك والأستاذ العام:

| المسار | الدورة |
|---|---|
| بيع أصناف مخزنية | عميل ← أمر بيع ← حجز وتجهيز ← تسليم ← فاتورة ← تحصيل ← تسوية |
| بيع خدمات بأمر بيع | عميل ← أمر بيع خدمة ← فاتورة ← تحصيل ← تسوية |
| فاتورة مباشرة | عميل ← Free text invoice ← تحصيل ← تسوية |
| دفعة مقدمة | عميل ← تحصيل مقدم ← فاتورة لاحقًا ← تطبيق وتسوية الدفعة |
| مرتجع | مرجع الفاتورة الأصلية ← أمر مرتجع ← استلام عند الحاجة ← إشعار دائن ← تسوية أو رد مبلغ |

الحسابات المدينة تدير **دين العميل وتحصيله**، بينما الأثر المخزني يأتي من مستندات المبيعات المتكاملة معها.

| المصطلح | المعنى |
|---|---|
| Physical inventory update | خروج أو دخول فعلي للصنف، مثل ترحيل التسليم أو استلام المرتجع |
| Financial inventory update | تحديث تكلفة الحركة ماليًا عند الفوترة |
| AR financial posting | تسجيل مديونية العميل أو التحصيل أو التسوية |
| GL posting | إنشاء القيود في الأستاذ العام |

**يمكن أن ينشأ قيد محاسبي عند التسليم، رغم أن الحركة ما زالت Physical في المخزون.** كما أن فاتورة الخدمة والتحصيل يؤثران ماليًا دون حركة مخزون. حالات المخزون المعتادة للبيع هي `Deducted` عند التسليم و`Sold` عند الفوترة. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/supply-chain/cost-management/physical-financial-updates?utm_source=chatgpt.com)

**المرحلة 0 — تحديد السياسة المحاسبية ونطاق العمل**

قبل إدخال الإعدادات، حدّد:

- البيع الآجل والنقدي والعملات المستخدمة.
- العملاء المحليين والخارجيين والشركات الشقيقة.
- الأصناف المخزنية والخدمات والفواتير المباشرة.
- حدود الائتمان وإيقاف العملاء.
- شروط السداد والأقساط والخصم النقدي.
- الضرائب والرسوم والدفعات المقدمة.
- حسابات العملاء والإيرادات والمخزون والتكلفة والبنوك.
- مسؤوليات إنشاء العميل والفوترة والتحصيل واعتماد التسويات.

| أساس النظام | الجداول الرئيسية |
|---|---|
| الشركة والدفتر | `CompanyInfo`, `Ledger` |
| دليل الحسابات | `MainAccount` |
| الفترات المالية | `FiscalCalendar`, `FiscalCalendarYear`, `FiscalCalendarPeriod` |
| الأبعاد المالية | `DimensionAttribute`, `DimensionAttributeValue`, `DimensionAttributeValueSet`, `DimensionAttributeValueCombination` |

**الأثر:** تعريف قواعد العمل فقط؛ لا مديونية ولا حركة مخزون.

**المرحلة 1 — Configuration: إعداد الحسابات المدينة**

تشمل الإعدادات القياسية مجموعات العملاء، الترحيل، السداد، اليوميات، العملات، الضرائب، الائتمان والتحصيل. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/accounts-receivables-set-up-overview?utm_source=chatgpt.com)

| الإعداد / الشاشة | ما يتم تحديده | الجداول الرئيسية |
|---|---|---|
| Accounts receivable parameters | الترحيل الافتراضي، التحديثات، التسويات والترقيم | `CustParameters` |
| Number sequences | أرقام العملاء والفواتير والقسائم والمستندات | `NumberSequenceTable`, `NumberSequenceReference` |
| Customer groups | تصنيف العملاء والقيم الافتراضية | `CustGroup` |
| Customer posting profiles | حساب مراقبة العملاء حسب العميل أو المجموعة | `CustLedger`, `CustLedgerAccounts` |
| Terms of payment | حساب تاريخ الاستحقاق | `PaymTerm` |
| Payment schedules | توزيع الاستحقاق على أقساط | `PaymSched`, `PaymSchedLine` |
| Methods of payment — customers | تحويل، نقد، شيك، حساب مقابل أو وسيط | `CustPaymModeTable` |
| Cash discounts | الخصم مقابل السداد المبكر | `CashDisc` |
| Journal names | أنواع يوميات التحصيل والفواتير والقيود | `LedgerJournalName` |
| Currencies and exchange rates | العملات وأسعار الصرف | `Currency`, `ExchangeRateType`, `ExchangeRateCurrencyPair`, `ExchangeRate` |
| Sales tax | الأكواد والمجموعات وحسابات الضريبة | `TaxTable`, `TaxGroupHeading`, `TaxGroupData`, `TaxItemGroupHeading`, `TaxOnItem` |
| Charges | رسوم النقل والخدمات وغيرها | `MarkupTable`, `MarkupAutoTable`, `MarkupAutoLine` |
| Delivery setup | طريقة وشروط التسليم | `DlvMode`, `DlvTerm` |

**قاعدة الترحيل الأساسية:**

- حساب مراقبة العملاء يأتي من **Customer posting profile**.
- إيراد أمر البيع وتكلفته يأتيان من إعدادات ترحيل المبيعات والمخزون.
- إيراد الفاتورة المباشرة يأتي عادة من الحساب المحدد على سطرها.
- البنك أو الحساب الوسيط يأتي من إعدادات الدفع واليومية.

أولوية البحث في Customer posting profile هي: **عميل محدد `Table` ثم مجموعة `Group` ثم الجميع `All`**. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/customer-posting-profiles?utm_source=chatgpt.com)

**المرحلة 2 — إعداد تكامل المبيعات والمخزون**

هذه المرحلة مطلوبة للبيع المخزني، ولا تكفي إعدادات AR وحدها.

| الإعداد | الغرض | الجداول الرئيسية |
|---|---|---|
| Released products | تعريف الصنف ووحداته وخصائصه | `EcoResProduct`, `InventTable`, `InventTableModule` |
| Item groups | تصنيف الأصناف وربط الترحيل | `InventItemGroup`, `InventItemGroupItem` |
| Item model groups | السياسة المخزنية والتكلفة والترحيل | `InventModelGroup`, `InventModelGroupItem` |
| Inventory posting | المخزون، التكلفة، الإيراد، وحسابات التسليم | `InventPosting` |
| Inventory parameters | ضوابط التحديث والترحيل | `InventParameters` |
| Sites and warehouses | موقع ومستودع الصرف | `InventSite`, `InventLocation` |
| Inventory dimensions | أبعاد الحركة | `InventDim` |
| Prices and discounts | أسعار واتفاقيات البيع | `PriceDiscTable` |

لترحيل تكلفة التسليم إلى الأستاذ العام، تُراجع إعدادات `Post packing slip in ledger` و`Post physical inventory` وحسابات تكلفة الوحدات المسلّمة وتكلفة البضاعة المسلّمة. توجد معالجة خاصة للأصناف ذات التكلفة المعيارية. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/sales-order-posting?utm_source=chatgpt.com)

**المرحلة 3 — Master data: إنشاء العميل**

المسار المعتاد: **Accounts receivable > Customers > All customers**.

| البيانات | مثال | الجداول |
|---|---|---|
| حساب العميل ومجموعته | `C-0001` / محلي | `CustTable`, `CustGroup` |
| الاسم والهوية | شركة النور التجارية | `DirPartyTable` |
| العناوين | عنوان الفوترة والتسليم | `DirPartyLocation`, `LogisticsLocation`, `LogisticsPostalAddress` |
| وسائل التواصل | هاتف وبريد | `LogisticsElectronicAddress` |
| جهات الاتصال | مسؤول المشتريات والمحاسب | `ContactPerson` مع جداول الأطراف |
| حساب العميل البنكي | IBAN | `CustBankAccount` |
| العملة وشروط السداد | SAR / 30 يومًا | قيم مرجعية على `CustTable` |
| الضرائب | مجموعة ضريبة العميل ورقمه الضريبي | `CustTable` والجداول الضريبية |
| الائتمان | حد ائتمان وحالة إيقاف | `CustTable` وإعدادات Credit management عند استخدامها |
| الأبعاد | فرع، قسم، وحدة أعمال | `DefaultDimension` وجداول الأبعاد |

انتبه إلى الفرق بين:

- **Order account:** العميل الذي يطلب البضاعة.
- **Invoice account:** العميل الذي تتحمل ذمته قيمة الفاتورة.

قد يكونان حسابين مختلفين.

**الأثر:** إنشاء العميل لا يُنشئ رصيدًا ماليًا.

**المرحلة 4 — الأرصدة الافتتاحية**

عند الانتقال من نظام سابق، يُفضّل تحميل الفواتير المفتوحة بتفاصيلها: رقم الفاتورة، التاريخ، الاستحقاق، العملة والمتبقي.

| العملية | الجداول المتأثرة |
|---|---|
| تجهيز يومية الأرصدة | `LedgerJournalTable`, `LedgerJournalTrans` |
| ترحيل حركات العملاء | `CustTrans` |
| المبالغ المفتوحة | `CustTransOpen` |
| القيود المحاسبية | `GeneralJournalEntry`, `GeneralJournalAccountEntry` |

القيد المبسط:

| مدين | دائن |
|---|---|
| العملاء | حساب مقابل للأرصدة الافتتاحية |

**لا تسجل إيراد المبيعات مرة أخرى** إذا كانت هذه الفواتير تخص فترات سابقة تم ترحيل نتائجها ضمن الأرصدة الافتتاحية.

**المرحلة 5 — عرض السعر وأمر البيع**

| الخطوة | الجداول الرئيسية | الأثر |
|---|---|---|
| عرض السعر — اختياري | `SalesQuotationTable`, `SalesQuotationLine` | لا مديونية ولا صرف |
| إنشاء أمر البيع | `SalesTable`, `SalesLine` | طلب بيع والتزام تشغيلي |
| حركة المخزون للصنف المخزني | `InventTransOrigin`, `InventTrans` | كمية مطلوبة للصرف |
| تأكيد أمر البيع | `CustConfirmJour`, `CustConfirmTrans` | حفظ مستند التأكيد |

مثال: بيع **10 وحدات × 100 ريال**.

عند إنشاء الأمر وتأكيده:

- لا تُسجل فاتورة على العميل.
- لا يتحقق إيراد البيع في الدورة المعتادة.
- لا تنخفض الكمية الفعلية بسبب التأكيد وحده.
- قد يتغير التعرض الائتماني حسب إعدادات فحص الائتمان.

**المرحلة 6 — الحجز والتجهيز**

| العملية | الجداول الرئيسية | الأثر الفعلي والمالي |
|---|---|---|
| حجز الكمية | `InventTrans`, `InventSum`, `InventDim` | يقل المتاح للحجز؛ لا يوجد تسليم بعد |
| قائمة التجهيز في المخزون الأساسي | `WMSPickingRoute`, `WMSOrderTrans` بحسب المسار | تجهيز وتحديث حالة الالتقاط |
| تحديث الحجز أو الالتقاط | `InventTrans` | حالات مثل `ReservPhysical` و`Picked` |

**الحجز لا يعني خروج البضاعة، والالتقاط لا يعني تسجيل مديونية العميل.** أما WHS المتقدم فيضيف جداول وإجراءات أخرى.

**المرحلة 7 — التسليم: Packing slip**

عند ترحيل التسليم للصنف المخزني:

| الجانب | النتيجة |
|---|---|
| المستند | إنشاء سند تسليم |
| الكمية | تنخفض الكمية الفعلية المسلّمة |
| حالة الصرف | `StatusIssue = Deducted` |
| العميل | لا تنشأ فاتورة مدينة من التسليم وحده |
| المحاسبة | قد تنشأ قيود تكلفة فعلية وفق الإعداد |

**الجداول المتأثرة:**

`CustPackingSlipJour`, `CustPackingSlipTrans`, `SalesLine`, `InventTrans`, `InventSum`، وجداول الأستاذ العام عند تفعيل الترحيل.

إذا كانت تكلفة الوحدات العشر **600 ريال**، فالقيد التوضيحي للتسليم:

| مدين | دائن | المبلغ |
|---|---|---:|
| تكلفة بضاعة مسلّمة غير مفوترة | حساب تكلفة الوحدات المسلّمة | 600 |

هذه حسابات انتقالية حسب تصميم الترحيل. توجد أيضًا إعدادات اختيارية لترحيل الإيراد والضريبة المؤجلين عند التسليم. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/sales-order-posting?utm_source=chatgpt.com)

**المرحلة 8 — فاتورة أمر البيع: Sales invoice**

| الجانب | النتيجة |
|---|---|
| العميل | تسجيل المديونية وتاريخ الاستحقاق |
| الإيراد | تسجيل المبيعات والخصومات والرسوم |
| الضريبة | تسجيل الضريبة حسب إعداداتها |
| المخزون | تحديث مالي للكمية المفوترة؛ الحالة `Sold` |
| تكلفة المبيعات | تسجيل التكلفة المالية وعكس قيود التسليم المرتبطة |

**الجداول الرئيسية:**

| الوظيفة | الجداول |
|---|---|
| الفاتورة المرحلة | `CustInvoiceJour`, `CustInvoiceTrans` |
| حركة العميل | `CustTrans` |
| المبلغ المفتوح | `CustTransOpen` |
| أمر البيع | `SalesTable`, `SalesLine` |
| المخزون والتكلفة | `InventTrans`, `InventSum` |
| الضرائب والرسوم | `TaxTrans`, `MarkupTrans` |
| الأستاذ العام | `GeneralJournalEntry`, `GeneralJournalAccountEntry` |

**مثال توضيحي:** صافي البيع 1,000، وضريبة افتراضية 15% بقيمة 150، والتكلفة 600.

| مدين | دائن | المبلغ |
|---|---|---:|
| العملاء | — | 1,150 |
| — | إيرادات المبيعات | 1,000 |
| — | ضريبة المخرجات | 150 |
| تكلفة المبيعات | المخزون | 600 |

تُعكس قيود تكلفة التسليم السابقة أيضًا. **الفاتورة لا تصرف الوحدات العشر فعليًا مرة ثانية إذا سبق تسليمها.**

**المرحلة 9 — الفاتورة المباشرة: Free text invoice**

تُستخدم مثلًا لفوترة إيجار أو استشارة أو رسوم لا تحتاج دورة بيع وصرف صنف.

| المرحلة | الجداول |
|---|---|
| إدخال رأس الفاتورة | `CustInvoiceTable` |
| إدخال السطور | `CustInvoiceLine` |
| الفاتورة المرحلة | `CustInvoiceJour`, `CustInvoiceTrans` |
| حركة العميل والمفتوح | `CustTrans`, `CustTransOpen` |
| الضريبة والقيد | `TaxTrans`, `GeneralJournalEntry`, `GeneralJournalAccountEntry` |

اسم جدول سطور الإدخال هو **`CustInvoiceLine`**؛ ويختلف عن `CustInvoiceTrans` الخاص بسطور الفاتورة المرحلة. [Microsoft Learn](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountsreceivable/worksheetline/custinvoiceline?utm_source=chatgpt.com)

القيد المعتاد:

| مدين | دائن |
|---|---|
| العملاء بقيمة الإجمالي | الإيراد والضريبة |

يُحدد حساب الإيراد والأبعاد على السطر. **لا توجد حركة `InventTrans` أو تكلفة مخزون بسبب Free text invoice نفسها.** [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/create-free-text-invoice-new?utm_source=chatgpt.com)

**المرحلة 10 — التحصيل: Customer payment**

المسار المعتاد: **Accounts receivable > Payments > Customer payment journal**.

| الخطوة | الجداول الرئيسية |
|---|---|
| إدخال اليومية | `LedgerJournalTable`, `LedgerJournalTrans` |
| ترحيل التحصيل على العميل | `CustTrans` |
| إبقاء دفعة غير مسواة | `CustTransOpen` |
| حركة البنك عند الترحيل إلى حساب بنكي | `BankAccountTrans` |
| قيد الأستاذ العام | `GeneralJournalEntry`, `GeneralJournalAccountEntry` |

القيد المبسط لتحصيل **700 ريال**:

| مدين | دائن | المبلغ |
|---|---|---:|
| البنك | العملاء | 700 |

يمكن أن يمر التحصيل بحساب وسيط بدل البنك مباشرة، حسب طريقة الدفع. **لا يتولد إيراد جديد من تحصيل فاتورة سبق ترحيلها.** [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/general-ledger/accts-recvble-posting?utm_source=chatgpt.com)

**المرحلة 11 — التسوية: Settlement**

التسوية تربط الدفعة بالفاتورة أو تربط الإشعار الدائن بالفاتورة.

| الجدول | دوره |
|---|---|
| `CustTrans` | أصل حركات الفواتير والدفعات والتعديلات |
| `CustTransOpen` | المبالغ التي ما زالت مفتوحة |
| `CustSettlement` | سجل مبالغ وروابط التسوية |
| `SpecTrans` | تعليم الحركات المختارة للتسوية أثناء المعالجة |

هذه الأدوار الأساسية لحركات العملاء والمفتوح والتسويات موثقة لدى Microsoft. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/finance-insights/tables-detail?utm_source=chatgpt.com)

في مثالنا:

| البيان | المبلغ |
|---|---:|
| الفاتورة | 1,150 |
| التحصيل الذي تمت تسويته | 700 |
| المتبقي على الفاتورة | 450 |

- التسوية الجزئية تُبقي المتبقي مفتوحًا.
- التسوية الكاملة تُغلق المبلغ المفتوح مع الاحتفاظ بتاريخ الحركة.
- قد تُنشئ التسوية قيود خصم نقدي، فروق عملة، فروق صغيرة أو موازنة حسابات وأبعاد.
- يمكن ترحيل الدفعة أولًا وتسويتها لاحقًا. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/cash-bank-management/settlement-overview?utm_source=chatgpt.com)

**رصيد العميل صفر لا يضمن أن جميع حركاته مسوّاة:** قد تبقى فاتورة ودفعة متساويتان مفتوحتين إذا لم تتم التسوية.

**المرحلة 12 — الدفعات المقدمة والخصومات والفروقات**

| السيناريو | الأثر المحاسبي المعتاد | الجداول الرئيسية |
|---|---|---|
| دفعة مقدمة | مدين البنك، دائن حساب دفعات العملاء حسب Posting profile | `LedgerJournalTrans`, `CustTrans`, `CustTransOpen` |
| تطبيق المقدّم على الفاتورة | تسوية وإعادة تصنيف عند الحاجة | `CustSettlement`, `CustTrans` وجداول GL |
| خصم سداد مبكر | تخفيض المديونية مع تسجيل الخصم | `CustSettlement`, `CustTransCashDisc`, جداول GL |
| زيادة تحصيل | يبقى رصيد دائن مفتوح إلى أن يطبق أو يرد | `CustTrans`, `CustTransOpen` |
| نقص تحصيل | يبقى المتبقي أو يعالج وفق سياسة الفروق | `CustTransOpen`, `CustSettlement` |
| رد مبلغ للعميل | تخفيض البنك وتسوية الرصيد الدائن | يوميات الدفع، `CustTrans`, `BankAccountTrans` |

تُستخدم الدفعة المقدمة عندما يصل التحصيل قبل وجود فاتورة لتسويته عليها، ويمكن تخصيص Posting profile لها. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/accounts-receivable/customer-prepayments?utm_source=chatgpt.com)

**المرحلة 13 — المرتجعات والإشعارات الدائنة**

| السيناريو | الأثر الفعلي | الأثر المالي |
|---|---|---|
| مرتجع يعاد للمخزون | استلام الكمية حسب المعالجة | تخفيض المديونية وعكس الإيراد والضريبة والتكلفة ذات الصلة |
| Credit only | لا استلام صنف | تخفيض المديونية دون إعادة كمية |
| مرتجع تالف مع Scrap | يعتمد على قرار معالجة المرتجع | إشعار دائن ومعالجة تكلفة التلف |
| إشعار دائن لخدمة | لا مخزون | عكس الإيراد والضريبة حسب الحالة |

**الجداول:** `SalesTable`, `SalesLine`, `CustPackingSlipJour`, `CustPackingSlipTrans`, `CustInvoiceJour`, `CustInvoiceTrans`, `InventTrans`, `InventSum`, `CustTrans`, `CustTransOpen`, `CustSettlement`, `TaxTrans` وجداول GL.

قرار `Disposition code` يحدد التصرف في البضاعة؛ وخيار `Credit only` يسمح بتخفيض مديونية العميل دون اشتراط رجوعها فعليًا. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/supply-chain/sales-marketing/sales-returns?utm_source=chatgpt.com)

**المرحلة 14 — الائتمان والتحصيل والديون المتأخرة**

| العملية | الأثر | الجداول الأساسية ذات الصلة |
|---|---|---|
| فحص حد الائتمان | السماح أو إيقاف المعالجة حسب السياسة | `CustTable`, `SalesTable` وجداول Credit management |
| Aging | تحليل المفتوح حسب العمر والاستحقاق | `CustTrans`, `CustTransOpen` |
| Collection letters | متابعة المتأخرات؛ وقد توجد رسوم | `CustCollectionLetterJour`, `CustCollectionLetterTrans` |
| Interest notes | احتساب وترحيل فوائد عند استخدامها | `CustInterestJour`, `CustInterestTrans` |
| نزاع العميل | متابعة حالة التحصيل | `CustDisputeHistory` وحركات العميل |
| Write-off | إقفال المبلغ مقابل مصروف أو حساب معتمد | يوميات القيود، `CustTrans`, `CustSettlement`, جداول GL |

**احتساب Aging وحده لا ينشئ قيدًا.** وكذلك متابعة التحصيل لا تعني شطب الدين؛ الشطب عملية مالية مستقلة.

**المرحلة 15 — العملات والإقفال والمطابقة**

| العملية | المطلوب | الأثر |
|---|---|---|
| إعادة تقييم العملات | تقييم المفتوح الأجنبي بسعر الإقفال | فروق عملة غير محققة |
| التسوية بعملة أجنبية | احتساب الفرق عند السداد | فروق عملة محققة حسب الحالة |
| مطابقة AR مع GL | مقارنة دفتر العملاء بحسابات المراقبة | كشف فروق الترحيل |
| مراجعة التسليم غير المفوتر | حصر ما خرج ولم يفوتر | تفسير الحسابات الانتقالية |
| مراجعة الدفعات غير المسواة | تطبيقها على الفواتير الصحيحة | إكمال تسوية المفتوح |
| إقفال المخزون | تسويات تكلفة حسب نموذج الصنف | قد يعدّل تكلفة المبيعات |
| إقفال الفترة | إنهاء الترحيلات والمراجعات | ضبط تاريخ المعالجة |

إعادة تقييم AR تحدّث قيمة المبالغ المفتوحة بالعملة المحاسبية وتسجل فروقًا غير محققة؛ لا تغير أصل قيمة الفاتورة بعملتها الأجنبية. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/finance/cash-bank-management/foreign-currency-revaluation-accounts-payable-accounts-receivable?utm_source=chatgpt.com)

**خريطة الأثر الفعلي والمالي للدورة**

| العملية | كمية المخزون الفعلية | تحديث تكلفة المخزون ماليًا | رصيد العميل | البنك |
|---|---|---|---|---|
| إنشاء العميل | لا | لا | لا | لا |
| عرض السعر وأمر البيع | لا | لا | لا | لا |
| الحجز | لا؛ يتغير المتاح | لا | لا | لا |
| التسليم | تنخفض | تحديث فعلي، وقد يرحل إلى GL | لا عادةً | لا |
| فاتورة البيع المخزني | لا صرف ثانٍ للمسلّم | نعم | يزيد | لا |
| Free text invoice | لا | لا | يزيد | لا |
| التحصيل | لا | لا | ينخفض صافي الرصيد | يزيد أو يمر بوسيط |
| التسوية | لا | لا | تخصيص وإقفال المفتوح | لا تحصيل جديد |
| استلام مرتجع | تزيد إذا أعيد للمخزون | تحديث فعلي | لا من الاستلام وحده | لا |
| إشعار دائن | حسب نوع المرتجع | حسب نوع المرتجع | ينخفض | لا |
| إعادة تقييم العملة | لا | لا | تتغير القيمة المحاسبية | لا |

**التتبع الفني الذي تحتاجه عند التحليل**

| المطلوب تتبعه | نقطة البداية والمسار |
|---|---|
| أمر البيع | `SalesTable` ← `SalesLine` |
| الصرف المخزني | `SalesLine.InventTransId` ← `InventTransOrigin.InventTransId` ← `InventTrans.InventTransOrigin` |
| مستندات التسليم | `CustPackingSlipJour` ← `CustPackingSlipTrans` |
| الفاتورة المرحلة | `CustInvoiceJour` ← `CustInvoiceTrans` |
| الفاتورة المباشرة قبل الترحيل | `CustInvoiceTable` ← `CustInvoiceLine` |
| المفتوح | `CustTransOpen.RefRecId` ← `CustTrans.RecId` مع مراعاة الشركة |
| التسويات | `CustSettlement` ومراجع الحركة المقابلة والشركة |
| القيد المحاسبي | `SubledgerVoucherGeneralJournalEntry` ← `GeneralJournalEntry` ← `GeneralJournalAccountEntry` |

لا تعتمد على `InvoiceId` أو `Voucher` منفردًا للربط؛ راعِ الشركة والتاريخ ومفاتيح العلاقات القياسية. والجداول المذكورة هي **الجداول الأساسية للدورة**؛ قد تضيف الميزات والتوطين والامتدادات جداول أخرى، ولا تُكتب كل هذه الجداول في كل عملية.

**ولاعتبار الدورة مكتملة عمليًا، اختبر على الأقل:**

1. بيع مخزني كامل، ثم بيع بتسليم وفوترة جزئيين.
2. Free text invoice وفاتورة خدمة بأمر بيع.
3. تحصيل كامل وجزئي ودفعة غير مسواة.
4. دفعة مقدمة وتطبيقها على فاتورة.
5. خصم نقدي وزيادة ونقص تحصيل.
6. مرتجع للمخزون و`Credit only`.
7. فاتورة أجنبية وإعادة تقييم وتسوية.
8. عكس تحصيل وفك تسوية بالطريقة المدعومة.
9. Aging ومطابقة العملاء مع GL.
10. مطابقة الكميات والتكلفة والمستندات قبل إقفال الفترة.