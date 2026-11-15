# تفاصيل مراحل دورة D365 F&O والجداول والحقول المتأثرة

نكمل على نفس مثال **Core ERP**. اشترينا **100 قطعة بسعر 50 ريالًا** للمستودع `A`، ونقلنا **30** إلى `B`، وبعنا **40** من `A`، ثم أرجعنا **5** للمورد واستلمنا **2** من العميل وسجلنا نقص جرد **قطعة واحدة** في `B`. تشمل الدورة خدمات الشراء والبيع، والفواتير المباشرة، والدفع والتحصيل، والتسوية، وإقفال الشهر.

في كل مرحلة ستجد **ماذا يُنشأ**، و**ماذا يتغير**، و**النتيجة التي تفحصها في الاختبار**. أسماء الحقول أدناه تخص جداول D365 القياسية حيث أمكن التحقق منها. في التقارير والاستعلامات، راجع علاقة الجدول في **AOT** قبل اعتماد الربط، خصوصًا بين رؤوس المستندات وسطورها وبين القسائم والأستاذ العام.

## Phase 0 — إعداد الشركة والترحيل `Configuration`

| الإعداد | الجداول والحقول أو القيم التي تفحصها | أثر الإعداد عند تنفيذ الدورة |
|---|---|---|
| **الشركة وLedger** | `Ledger` للحسابات والتقويم والعملة المحاسبية، و`CompanyInfo` للشركة. في المثال الشركة تعمل بـ`SAR` | يحدد العملة المحاسبية والتقويم ودليل الحسابات الذي تُرحّل إليه القيود |
| **الحسابات الرئيسية** | `MainAccount.MainAccountId`, `Name` ونوع الحساب. عرّف حسابات البنك ورأس المال والمخزون والاستحقاق والذمم والإيرادات والتكلفة والمصروفات | يحدد تصنيف كل حركة في ميزان المراجعة والقوائم |
| **الفترات المالية** | `FiscalCalendar`, `FiscalCalendarYear`, `FiscalCalendarPeriod` لتعريف الفترات. `LedgerFiscalCalendarPeriod.Ledger`, `FiscalCalendarPeriod`, `Status` لضبط حالة الفترة لكل Ledger | يُختبر الترحيل في فترة `Open` ثم منعه بعد تحويلها إلى `On hold` |
| **الأبعاد المالية** | `DimensionAttribute` لتعريف بُعد مثل `BusinessUnit`، و`DimensionAttributeValueSet` لقيم `DefaultDimension`، و`DimensionAttributeValueCombination` للتركيبة المستخدمة في `LedgerDimension` | قارن البُعد الافتراضي على المصدر بالبُعد المرحّل في GL. المرجع الفعلي للقيد هو `GeneralJournalAccountEntry.LedgerDimension` |
| **ترحيل الموردين والعملاء** | `VendPosting`, `CustPosting`، ومعلمات `VendParameters`, `CustParameters`. افحص ملف الترحيل المختار وقاعدة الحساب أو المجموعة أو `All` وحساب الرقابة الناتج | فاتورة المورد تُرحّل إلى حساب رقابة AP، وفاتورة العميل إلى حساب رقابة AR |
| **ترحيل المخزون** | `InventPosting` لتحديد حسابات الشراء والاستلام غير المفوتر والمبيعات والتكلفة والجرد. افحص نوع الترحيل ونطاق الصنف والحساب المحدد لكل حالة | يحدد الحساب الفعلي عند Product receipt وPacking slip والفاتورة والتسوية المخزنية |
| **نموذج تكلفة الصنف** | `InventModelGroup` وارتباط الصنف عبر `InventModelGroupItem`. افحص `Weighted average`، و`Stocked product`، وخيارات ترحيل المخزون الفعلي والمالي والسماح بالسالب | الصنف المخزني ينتج حركات كمية وقيمة. خدمة البيع تُربط بمجموعة غير مخزنية |
| **الدفع واليوميات والترقيم** | `PaymTerm`, `CustPaymModeTable`, `VendPaymModeTable`, `LedgerJournalName`, `NumberSequenceTable`, `BankAccountTable` | شرط `NET30` يحدد الاستحقاق. اسم اليومية وإعدادها يحددان القسيمة وطريقة عمل اليومية وحساب البنك المستخدم |
| **مطابقة الفاتورة** | إعدادات `VendParameters` وسياسات مطابقة فواتير المورد. افحص سياسة المطابقة الثلاثية وحدود الاختلاف والإجراء عند تجاوزه | في اختبار 61 قطعة مقابل 60 مستلمة يظهر فرق المطابقة، ويُمنع الترحيل العادي إذا كانت السياسة تمنع الاختلاف |

**اختبار المرحلة:** استخدم عينة بقيمة صغيرة وتحقق من **الحساب الفعلي في Voucher transactions**. ظهور اسم الحساب في شاشة الإعداد وحده لا يثبت أن قاعدة الصنف أو العميل أو المورد اختارت هذا الحساب. ولظهور قيد Packing slip يجب تفعيل خيارات الترحيل المطلوبة وتعريف حسابات تكلفة البضاعة المسلّمة. [Microsoft Learn](https://learn.microsoft.com/dynamics365/finance/general-ledger/inventory-posting-profiles?utm_source=chatgpt.com)

## Phase 1 — البيانات الأساسية `Master Data`

| السجل | الجداول والحقول التي تُحفظ أو تُراجع | بيانات المثال |
|---|---|---|
| **المورد** | `VendTable.AccountNum`, `VendGroup`, `Party`, `Currency`, `PaymTermId`, `PaymMode`, `DefaultDimension`، و`DirPartyTable.RecId`, `Name` | `V-001`، ريال، `NET30`، دفع بنكي |
| **العميل** | `CustTable.AccountNum`, `CustGroup`, `Party`, `Currency`, `PaymTermId`, `PaymMode`, `DefaultDimension`، و`DirPartyTable.RecId`, `Name` | `C-001`، ريال، `NET30`، تحصيل بنكي |
| **الصنف المخزني** | `EcoResProduct` لتعريف المنتج، و`InventTable.ItemId`, `Product` للمنتج المفرج عنه في الشركة، و`InventItemGroupItem`, `InventModelGroupItem` لربط المجموعات. `InventTableModule.ModuleType`, `UnitId`, `Price` لإعداد المخزون والشراء والبيع بحسب الوحدة | `ITEM-01`، الوحدة `EA`، شراء 50، بيع 100 |
| **خدمة البيع** | `EcoResProduct`, `InventTable`, `InventModelGroupItem`, `InventTableModule`. افحص نوع المنتج ومجموعة النموذج غير المخزني وسعر البيع ووحدته | `SERV-01`، بيع 300 |
| **خدمة الشراء** | `EcoResCategory` لفئة الشراء. عند إنشاء السطر افحص `PurchLine.ProcurementCategory`, `LineAmount`, `DefaultDimension` والتوزيع المحاسبي | صيانة 200 ريال تُثبت كمصروف عند الفاتورة في هذا المثال |
| **الموقع والمستودعات** | `InventSite.SiteId`، و`InventLocation.InventLocationId`, `InventSiteId`، و`InventDim.InventDimId`, `InventSiteId`, `InventLocationId` | الموقع `S1`، المستودعان `A` و`B`، ومستودع العبور `TRANSIT` |

**اختبار المرحلة:** أنشئ سطر شراء وسطر بيع للصنف. افحص الوحدة والسعر والمستودع والأبعاد التي انتقلت إلى السطر، ثم أنشئ سطر خدمة وتحقق من اختيار الفئة أو الصنف الخدمي الصحيح.

## Phase 2 — المشتريات من أمر الشراء إلى الفاتورة `PO → AP`

المستند `PO-001` لشراء **100 × 50 = 5,000 ريال**. الرصيد يبدأ من صفر.

| المرحلة | الجداول والسجلات المتأثرة | الحقول والقيم المتوقعة |
|---|---|---|
| **2.1 إنشاء PO** | يُنشأ رأس في `PurchTable` وسطر في `PurchLine`. للصنف المخزني، يتولد أصل حركة في `InventTransOrigin` وحركة توريد في `InventTrans` | الرأس `PurchId=PO-001`, `OrderAccount=V-001`, `InvoiceAccount=V-001`, `CurrencyCode=SAR`. السطر `ItemId=ITEM-01`, `PurchQty=100`, `PurchPrice=50`, `LineAmount=5000`, `InventDimId` للمستودع `A`, `InventTransId`. الكمية المتبقية فعليًا وماليًا `100 / 100`. حركة التوريد `Ordered` والرصيد الفعلي صفر |
| **2.2 تأكيد PO** | تُحفظ نسخة التأكيد في `VendPurchOrderJour`, `VendPurchOrderTrans` | راجع مرجع `PurchId` ورقم التأكيد وتاريخه والكمية والسعر. الرصيد الفعلي صفر وذمة المورد صفر |
| **2.3 استلام أول 60** | يُنشأ `VendPackingSlipJour` ورقم استلام `PR-001`، وسطر `VendPackingSlipTrans`. تتحدث `PurchLine` و`InventTrans` ويتغير On-hand | في الاستلام افحص `PackingSlipId`, `DeliveryDate`, `Qty=60`, `InventDimId` ومرجع سطر المصدر. في المخزون افحص `Qty=+60`, `StatusReceipt=Received`, `DatePhysical`, `VoucherPhysical` عند الترحيل إلى GL. يبقى 40 `Ordered`. المتبقي فعليًا وماليًا `40 / 100`. قيد الاستحقاق الصافي 3,000 وذمة المورد صفر |
| **2.4 تجهيز ومطابقة فاتورة 60** | في مسار Pending vendor invoice افحص `VendInvoiceInfoTable`, `VendInvoiceInfoLine` والكمية المحددة من الاستلام | الفاتورة `VI-001`، الكمية 60، سعر الوحدة 50، المبلغ 3,000. افحص مطابقة الكمية مع `PR-001` والسعر مع PO. هذه نقطة اختبار قبل الترحيل |
| **2.5 ترحيل فاتورة 60** | يُنشأ `VendInvoiceJour`, `VendInvoiceTrans`, `VendTrans`, `VendTransOpen`. تُحدّث حركة المخزون والمتبقي المالي. يتاح تتبع ربط سطر الفاتورة بالاستلام عبر `VendInvoicePackingSlipQuantityMatch` عند استخدام مطابقة الاستلام | رأس الفاتورة `InvoiceId`, `InvoiceDate`, `InvoiceAccount`, `LedgerVoucher`. السطر `Qty=60`, `PurchPrice=50`, `LineAmount=3000`. المخزون `StatusReceipt=Purchased`, `DateFinancial`, `Voucher`, `CostAmountPosted` بقيمة شراء 3,000. المتبقي `40 / 40`، والفعلي `A=60`. يُعكس الاستحقاق المؤقت وتظهر ذمة المورد 3,000 |
| **2.6 استلام آخر 40** | يُنشأ استلام ثانٍ، وتتحدث حركة الـ40 والمتبقي الفعلي | `PR-002`, `Qty=40`, `StatusReceipt=Received`. `A=100`. المتبقي `0 / 40`. يظهر استحقاق 2,000 لهذه الدفعة |
| **2.7 ترحيل الفاتورة الثانية** | تُنشأ فاتورة `VI-002` وحركة مورد ورصيد مفتوح، وتُحدّث حركة الـ40 ماليًا | `Qty=40`, `LineAmount=2000`, `StatusReceipt=Purchased`. المتبقي `0 / 0`. المخزون يبقى `A=100` ويصبح شراء الـ100 كله مفوترًا، واستحقاق هذا الأمر صفر |
| **2.8 أمر شراء خدمة** | `PurchTable`, `PurchLine`، وسجل استلام أو قبول الخدمة حسب مسار العمل، ثم `VendInvoiceJour`, `VendInvoiceTrans`, `VendTrans`, `VendTransOpen` عند الفاتورة. افحص `AccountingDistribution` عند تتبع حساب المصروف | `ProcurementCategory` للصيانة، المبلغ 200، وحساب المصروف. قبول الخدمة لا يضيف مخزونًا. في إعداد المثال الفاتورة هي التي تُنشئ مدين مصروف صيانة 200 ودائن مورد 200 |
| **2.9 فاتورة مورد مباشرة** | عبر **Vendor invoice journal** افحص `LedgerJournalTable`, `LedgerJournalTrans`, `VendTrans`, `VendTransOpen` والقيد | `JournalNum`, `Voucher`, `Invoice`, `TransDate`, `AccountType`, `LedgerDimension`, `OffsetLedgerDimension`. مدين مصروف إداري 80، دائن مورد 80. نقطة التتبع تبدأ من اليومية والقسيمة |

الحقول الحاسمة في سطر الشراء هي `RemainPurchPhysical` و`RemainPurchFinancial`. بعد أول استلام تصبح **40 / 100**، وبعد فاتورته تصبح **40 / 40**. وفي `InventTrans` قد تجد الـ60 والـ40 كسجلين لنفس أصل الحركة بسبب التنفيذ الجزئي. [Microsoft Learn](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/procurementandsourcing/worksheetline/purchline?utm_source=chatgpt.com)

## Phase 3 — التحويل بين المستودعات `Transfer Order`

نفّذ التحويل بعد فوترة كل الشراء وقبل البيع. `TR-001` ينقل **30 قطعة من `A` إلى `B`**.

| المرحلة | الجداول والحقول | نتيجة الاختبار |
|---|---|---|
| **3.1 إنشاء التحويل** | `InventTransferTable.TransferId`, `InventLocationIdFrom`, `InventLocationIdTo`, `TransferStatus`. في `InventTransferLine` افحص `ItemId`, `InventDimId`, `QtyTransfer`, `QtyShipped`, `QtyReceived`, `QtyRemainShip`, `QtyRemainReceive` | الكمية المطلوبة 30، المشحون والمستلم صفر. الرصيد `A=100`, `B=0`, `TRANSIT=0` |
| **3.2 ترحيل Ship** | يُنشأ سجل تنفيذ في `InventTransferJour`, `InventTransferJourLine`. تتحدث `InventTransferLine.QtyShipped` و`QtyRemainShip` وحركات `InventTrans` حسب الأبعاد | المشحون 30، والمستلم صفر. الرصيد `A=70`, `B=0`, `TRANSIT=30` |
| **3.3 ترحيل Receive** | يُنشأ سجل تنفيذ للاستلام. تتحدث `QtyReceived`, `QtyRemainReceive`, `TransferStatus` وحركات المخزون في الوجهة والعبور | المستلم 30، والباقي للاستلام صفر. الرصيد `A=70`, `B=30`, `TRANSIT=0` |
| **3.4 يومية تحويل مستقلة** | `InventJournalTable`, `InventJournalTrans`. افحص `JournalId`, `ItemId`, `InventDimId`, `ToInventDimId`, `Qty` وحركتي الصرف والإضافة | نقل مباشر بين البعدين في عملية ترحيل واحدة. لا توجد مرحلة عبور في اليومية |

في إعداد المثال، التحويل داخل نفس الشركة والموقع ونطاق التكلفة. إجمالي المخزون يبقى **100 قطعة وقيمته 5,000**. راجع القسيمة حسب الأبعاد المالية للمخزون وإعدادات الترحيل. وجود أثر داخلي في GL يعتمد على تلك الإعدادات. [Microsoft Learn](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/inventory/worksheetline/inventtransferline?utm_source=chatgpt.com)

## Phase 4 — المبيعات من أمر البيع إلى الفاتورة `SO → AR`

المستند `SO-001` للصنف **40 × 100**، ومعه خدمة **300**. رصيد `A` قبل البيع **70**.

| المرحلة | الجداول والسجلات المتأثرة | الحقول والقيم المتوقعة |
|---|---|---|
| **4.1 إنشاء SO** | يُنشأ `SalesTable` وسطران في `SalesLine`. يتولد أصل حركة طلب صرف للصنف المخزني | `SalesId=SO-001`, `CustAccount=C-001`, `InvoiceAccount=C-001`, `CurrencyCode=SAR`. سطر الصنف `SalesQty=40`, `SalesPrice=100`, `LineAmount=4000`, `InventDimId` للمستودع `A`, `InventTransId`. سطر الخدمة `SalesQty=1`, `LineAmount=300`. متبقي الصنف فعليًا وماليًا `40 / 40` |
| **4.2 التأكيد** | تُنشأ `CustConfirmJour`, `CustConfirmTrans` | راجع `SalesId`, `ConfirmId` والسطور المؤكدة. `A=70` والعميل بلا ذمة من هذا الأمر |
| **4.3 حجز 40** | تتحدث `InventTrans.StatusIssue` إلى `ReservPhysical` للكمية المحجوزة فعليًا. افحص شاشة On-hand بحسب `InventDimId` | إذا لم توجد حجوزات أخرى على `A`، يبقى Physical inventory **70** ويصبح Available physical **30**. إلغاء الحجز يعيد المتاح إلى 70 |
| **4.4 تسليم 25** | يُنشأ `CustPackingSlipJour`, `CustPackingSlipTrans`. تتحدث `SalesLine`, `InventTrans` والرصيد الفعلي | افحص `PackingSlipId`, `DeliveryDate`, `Qty=25`, `InventDimId`. في حركة الصنف `Qty=-25`, `StatusIssue=Deducted`, `DatePhysical`, `VoucherPhysical` عند الترحيل. متبقي الصنف `15 / 40`. الرصيد `A=45`، وتكلفة التسليم المؤقتة 1,250 في إعداد المثال |
| **4.5 فاتورة أول 25** | تُنشأ `CustInvoiceJour`, `CustInvoiceTrans`, `CustTrans`, `CustTransOpen`. تتحول حركة الـ25 إلى تحديث مالي | `SI-001`, `Qty=25`, `LineAmount=2500`, `LedgerVoucher`. الحركة `StatusIssue=Sold`, `DateFinancial`, `Voucher` وتكلفة صرف 1,250. المتبقي `15 / 15`. `A=45`. تظهر ذمة عميل 2,500، وإيراد 2,500، وتكلفة بيع نهائية 1,250 |
| **4.6 تسليم آخر 15 ثم الفاتورة** | Packing slip ثانٍ، ثم فاتورة `SI-002` وسطران في `CustInvoiceTrans` وحركة عميل واحدة للفاتورة في المثال | بعد التسليم `A=30` والمتبقي `0 / 15`. بعد الفاتورة `0 / 0`. فاتورة الصنف 1,500 والخدمة 300، والإجمالي 1,800. تكلفة الصنف 750، والخدمة لا تضيف تكلفة صرف مخزني |
| **4.7 Free text invoice** | قبل الترحيل `CustInvoiceTable`, `CustInvoiceLine`. بعده افحص `CustInvoiceJour`, `CustInvoiceTrans`, `CustTrans`, `CustTransOpen` | راجع `InvoiceAccount` ووصف السطر وحساب الإيراد `LedgerDimension` ومبلغ 100. مدين عميل 100 ودائن إيراد خدمة 100 |

في سطر البيع `RemainSalesPhysical` يتغير عند التسليم، و`RemainSalesFinancial` يتغير عند الفوترة. ولتتبع التكلفة، خذها من حركة المخزون والقسيمة. `CustInvoiceTrans.LineAmount` هنا يوضح **قيمة البيع**. [Microsoft Learn](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/salesandmarketing/worksheetline/salesline?utm_source=chatgpt.com)

## Phase 5 — المرتجعات وCredit only

| العملية | الجداول والحقول المتأثرة | الأثر المتوقع |
|---|---|---|
| **5.1 مرتجع مورد 5 قطع** | أمر مرتجع في `PurchTable`, `PurchLine`. افحص `PurchType`, `PurchQty=-5`, `InventDimId` والمرجع إلى `VI-002`. عند الإرجاع الفعلي افحص `VendPackingSlipJour/Trans` وحركة صرف في `InventTrans`. عند الإشعار الدائن افحص `VendInvoiceJour/Trans`, `VendTrans`, `VendTransOpen` | `A: 30 → 25`. حركة الكمية في المخزون `-5`. بعد الإشعار الدائن ينخفض رصيد المورد 250 وتنخفض قيمة المخزون 250 |
| **5.2 مرتجع عميل قطعتين للمخزون** | `SalesTable`, `SalesLine` لأمر المرتجع. افحص `SalesQty=-2`, `InventTransIdReturn` عند ربطه بحركة البيع الأصلية، وReturn cost price وإجراء التصرف. ثم افحص `CustPackingSlipJour/Trans`, `InventTrans`, `CustInvoiceJour/Trans`, `CustTrans` | سطر البيع المرتجع سالب، وحركة المخزون **موجبة +2**. `A: 25 → 27`. استلام `Received` ثم تحديث مالي `Purchased`. إشعار العميل 200، وترجع تكلفة 100 إلى المخزون مقابل تخفيض تكلفة المبيعات |
| **5.3 Credit only بقيمة 50** | مستند المرتجع وإجراء `Credit only` ثم `CustInvoiceJour/Trans`, `CustTrans`, `CustTransOpen` والقسيمة | ينخفض صافي الإيراد والذمة 50. `A=27` وتبقى تكلفة المبيعات كما هي في هذه الخطوة |

**اختبار الإشارة:** افحص كمية المستند وكمية `InventTrans` كل واحدة في سياقها. في مرتجع العميل `SalesQty=-2` تمثل عكس البيع، بينما `InventTrans.Qty=+2` تمثل دخول قطعتين فعليًا. [Microsoft Learn](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/salesandmarketing/worksheetline/salesline?utm_source=chatgpt.com)

## Phase 6 — دفع المورد وتحصيل العميل والتسوية

### الجداول والحقول المشتركة

| الطبقة | الحقول التي تفحصها |
|---|---|
| **سطر اليومية** | `LedgerJournalTable.JournalNum`, `JournalName`, `Posted`، و`LedgerJournalTrans.Voucher`, `TransDate`, `AccountType`, `LedgerDimension`, `OffsetAccountType`, `OffsetLedgerDimension`, `AmountCurDebit`, `AmountCurCredit`, `CurrencyCode` |
| **حركة المورد أو العميل** | `VendTrans` أو `CustTrans`. راجع `RecId`, `AccountNum`, `Voucher`, `Invoice`, `TransDate`, `DueDate`, `AmountCur`, `AmountMST`, `SettleAmountCur`, `Closed` حسب الحركة |
| **الرصيد المفتوح الحالي** | `VendTransOpen` أو `CustTransOpen`. راجع `RefRecId`, `AccountNum`, `AmountCur`, `AmountMST`, `DueDate`. اربط `RefRecId` مع `RecId` في جدول الحركة الأصلي |
| **سجل التسوية** | `VendSettlement` أو `CustSettlement`. راجع `TransRecId`, `OffsetRecId`, `SettleAmountCur`, `SettleAmountMST`, `TransDate`. عند تتبع فاتورة ودفعة، راجع طرفَي العلاقة لتجنب جمع نفس التسوية مرتين |
| **البنك** | `BankAccountTrans`. راجع `AccountId`, `Voucher`, `TransDate`, `AmountCur` ومطابقة حركة البنك مع القسيمة |

### ما الذي يتغير في كل خطوة؟

| الخطوة | عند الترحيل | بعد التسوية |
|---|---|---|
| **6.1 دفع مورد 3,000 مع تعليم `VI-001`** | تُنشأ حركة دفع في `VendTrans` وحركة نقص في البنك | يُسجل الربط في `VendSettlement`. تغلق `VI-001` وتختفي من الرصيد المفتوح |
| **6.2 دفع مورد 1,000 دون تعليم** | تظهر حركة دفع مستقلة ومفتوحة في `VendTransOpen` إلى جانب الفواتير المفتوحة | سوِّ 1,000 وإشعار 250 مع `VI-002`. يبقى **750** من فاتورة الـ2,000. مع 200 و80 يصبح إجمالي المورد **1,030** |
| **6.3 تحصيل 2,000 من العميل** | تُنشأ حركة تحصيل في `CustTrans` وزيادة 2,000 في البنك | تسوية جزئية مع `SI-001`. قيمة الفاتورة الأصلية 2,500، والمفتوح **500** |
| **6.4 تحصيل 1,000 دون تعليم ثم تسويته** | تظهر حركة تحصيل مفتوحة إلى حين اختيار الفواتير | خصّص 500 لـ`SI-001` و500 لـ`SI-002`. بعد تسوية إشعارَي 200 و50 مع `SI-002` يبقى عليها **1,050**، ومع Free text invoice بـ100 يصبح إجمالي العميل **1,150** |
| **6.5 فك التسوية أو عكس الدفع، اختبار مستقل** | فك التسوية يغيّر سجل التسوية والرصيد المفتوح. عكس الدفع أو التحصيل يعالج كذلك حركة الطرف والبنك وفق مسار العكس المدعوم | افحص `TransOpen` و`Settlement` و`Voucher transactions` قبل كل عملية وبعدها. في مثال التسوية بنفس العملة ودون خصم أو فرق، تعليم الفاتورة بعد ترحيل الدفعة لا يعني حركة بنكية ثانية |

الفرق في اختبار `SI-001` واضح: `CustTrans.AmountCur` للحركة الأصلية يمثل **2,500**، و`CustTransOpen.AmountCur` بعد التحصيل الجزئي يمثل **500**. ولتقرير أعمار بتاريخ سابق، استخدم تقرير Aging بتاريخ القطع المطلوب أو أعد بناء الأرصدة بحسب تاريخ الحركات والتسويات. `TransOpen` يعكس المفتوح الحالي. [Microsoft Learn](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountsreceivable/transaction/custtrans?utm_source=chatgpt.com)

## Phase 7 — الجرد والتسويات وإقفال المخزون

| المرحلة | الجداول والحقول المتأثرة | نتيجة الاختبار |
|---|---|---|
| **7.1 إنشاء Counting journal** | يُنشأ `InventJournalTable` و`InventJournalTrans`. افحص `JournalId`, `JournalNameId`, `JournalType`, `Posted`، وعلى السطر `ItemId`, `InventDimId`, `InventOnHand`, `Counted`, `Qty` | المستودع `B`، رصيد النظام 30، المعدود 29، فرق السطر `-1`. عند الحفظ الرصيد الفعلي ما زال 30 |
| **7.2 ترحيل الجرد** | يتغير `Posted`. تُرحّل حركة `InventTrans` ويُحدَّث On-hand وتظهر القسيمة | `Qty=-1`، وتكلفة 50. `B=29`. مدين خسائر جرد 50 ودائن مخزون 50 |
| **7.3 تسوية مخزون موجبة أو سالبة** | `InventJournalTable`, `InventJournalTrans`, `InventTrans` والقيد. افحص `Qty`, `CostPrice`, `InventDimId`, `DatePhysical`, `DateFinancial`, `CostAmountPosted` | في اختبار مستقل جرّب `+2` و`-1`. تحقق من اتجاه الكمية والقيمة والحساب المقابل في كل حالة |
| **7.4 Inventory close** | افحص `InventSettlement.TransRecId`, `TransDate`, `Voucher`, `CostAmountAdjustment`، واربط مرجع الحركة مع `InventTrans.RecId`. على الحركة راجع `CostAmountPosted` و`CostAmountAdjustment` | في مثال السعر الثابت، المتوقع **56 قطعة بقيمة 2,800** دون فرق تكلفة. افحص سجل تنفيذ الإقفال والتسويات حتى إذا كان فرق القيمة صفرًا |
| **7.5 اختبار Weighted average بسعرين** | على صنف مستقل افحص حركة الصرف في `InventTrans` قبل الإقفال وبعده ثم سطر التسوية وقسيمته | فوّتِر 10 بسعر 40، ثم بع وفوّتِر 5، ثم فوّتِر 10 بسعر 60 في نفس الفترة ونطاق التكلفة ودون Marking. تكلفة الصرف تبدأ **200** وبعد الإقفال **250**. كمية المتبقي **15** وقيمته **750** |

إقفال المخزون يطبق سياسة التكلفة ويسوي حركات الصرف مع الاستلامات المؤهلة. في اختبار السعرين افحص **تعديل تكلفة حركة الصرف الأصلية**. الكمية الفعلية تبقى 15 بعد الإقفال. [Microsoft Learn](https://learn.microsoft.com/en-us/dynamics365/supply-chain/inventory/inventory-journals?utm_source=chatgpt.com)

## Phase 8 — الأستاذ العام والمطابقات وإقفال الشهر

| المرحلة | الجداول والحقول المتأثرة | نتيجة الاختبار |
|---|---|---|
| **8.1 رصيد البنك الافتتاحي** | `LedgerJournalTable`, `LedgerJournalTrans`، ثم `GeneralJournalEntry`, `GeneralJournalAccountEntry`. استخدم نوع حساب **Bank** لحساب البنك المطلوب تتبعه في `BankAccountTrans` | مدين البنك 10,000 ودائن رأس المال 10,000 |
| **8.2 مصروف البنك** | اليومية والقيد و`BankAccountTrans`. افحص الحساب المقابل والقسيمة والمبلغ والتاريخ | مدين مصروف بنكي 20، دائن البنك 20 |
| **8.3 مراجعة القسيمة** | `SubledgerVoucherGeneralJournalEntry.Voucher`, `VoucherDataAreaId`, `AccountingDate`, `GeneralJournalEntry`. في الرأس `GeneralJournalEntry.RecId`, `Ledger`, `AccountingDate`. في السطر `GeneralJournalAccountEntry.GeneralJournalEntry`, `LedgerDimension`, `PostingType`, `AccountingCurrencyAmount`, `TransactionCurrencyAmount`, `IsCredit` | تتوازن القسيمة. تحقق من الحساب الرئيسي والبُعد ونوع الترحيل. إذا كان نقل قيد الـSubledger مؤجلًا، افحص اكتمال النقل قبل المطابقة |
| **8.4 مطابقة الذمم والبنك والمخزون** | `VendTrans/Open`, `CustTrans/Open`, `BankAccountTrans`, `InventTrans`, `InventSettlement` مقابل `GeneralJournalAccountEntry` وتقارير الوحدات | الموردون **1,030 دائن**، العملاء **1,150 مدين**، البنك **8,980 مدين**، المخزون **2,800 مدين**. أرصدة العبور والحسابات الانتقالية للمستندات المكتملة صفر |
| **8.5 ميزان المراجعة وإيقاف الفترة** | ميزان المراجعة والقوائم من قيود GL. بعد المراجعة تتغير `LedgerFiscalCalendarPeriod.Status` للفترة إلى `On hold` | صافي الإيراد **4,150**، تكلفة المبيعات **1,900**، خسائر الجرد **50**، ومصروفات أخرى **300**، فيكون صافي الربح **1,900**. اختبر رفض الترحيل في الفترة الموقوفة |

في اختبار الضريبة المستقل، راجع `TaxGroup` و`TaxItemGroup` على المصدر، ثم `TaxTrans` والقسيمة. عند أساس **100** ومعدل اختبار **15%**، تكون الضريبة **15** وإجمالي ذمة الفاتورة **115**. تبقى هذه الحالة خارج أرصدة المسار الرئيسي. [Microsoft Learn](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/ledger/transaction/subledgervouchergeneraljournalentry?utm_source=chatgpt.com)

## مفاتيح الربط التي تستخدمها في التحقق

| من | إلى | مفتاح التتبع |
|---|---|---|
| سطر PO أو SO المخزني | أصل حركة المخزون | من `PurchLine.InventTransId` أو `SalesLine.InventTransId` إلى `InventTransOrigin.InventTransId` مع الشركة ومرجع المصدر عند الحاجة |
| أصل حركة المخزون | الحركات الجزئية | `InventTransOrigin.RecId = InventTrans.InventTransOrigin`. العلاقة قد تعطي عدة حركات لكل سطر |
| حركة المخزون | الموقع والمستودع | `InventTrans.InventDimId = InventDim.InventDimId` في نفس الشركة. اقرأ `InventSiteId` و`InventLocationId` |
| حركة AP أو AR | الرصيد المفتوح | `VendTrans.RecId = VendTransOpen.RefRecId` أو `CustTrans.RecId = CustTransOpen.RefRecId` |
| حركة AP أو AR | سجل التسوية | افحص `TransRecId` و`OffsetRecId` في جدول التسوية لتحديد الفاتورة والطرف المقابل |
| قسيمة المصدر | قيد الأستاذ | ابدأ بـ`Voucher` أو `LedgerVoucher` أو `VoucherPhysical` بحسب المرحلة، واستخدم الشركة والتاريخ مع علاقة `SubledgerVoucherGeneralJournalEntry` عندما تنطبق |
| رأس قيد الأستاذ | حسابات القيد | `GeneralJournalEntry.RecId = GeneralJournalAccountEntry.GeneralJournalEntry`. حلل `LedgerDimension` من خلال `DimensionAttributeValueCombination` للوصول إلى الحساب والأبعاد |

**أول نقطة تحقق عملية** هي `PO-001` بعد ترحيل `VI-001` وقبل الاستلام الثاني. يجب أن تجد **60 `Purchased` و40 `Ordered`**، و`RemainPurchPhysical=40` و`RemainPurchFinancial=40`، ورصيدًا فعليًا **60**، وفاتورة مورد مفتوحة **3,000**. هذه النقطة تختبر في لقطة واحدة ربط أمر الشراء بالاستلام والفاتورة والمخزون والذمم والقسيمة. الأرقام هنا **نتائج قبول متوقعة** للتنفيذ في بيئة الاختبار. [Microsoft Learn](https://learn.microsoft.com/common-data-model/schema/core/operationscommon/tables/supplychain/inventory/transaction/inventtransorigin?utm_source=chatgpt.com)