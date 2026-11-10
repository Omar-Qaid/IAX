# تقرير اختبار خدمات Foundation

في إعادة التشغيل المتسلسلة بعد AR، نجحت اختبارات التهيئة والخدمات الثمانية دون فشل أو تخطٍ. الأدلة في `.validation/erp-confirm-foundation.trx` و`.validation/erp-confirm-foundation.jsonl`.

في إعادة التحقق بعد قبول Accounts Payable وAccounts Receivable، نجحت اختبارات Foundation الثمانية دون فشل أو تخطٍ. غطّت فحوص التهيئة الجداول الـ68 للإعدادات والبيانات الرئيسية، وثبات المفاتيح عند إعادة التشغيل، ورفض تعارض البيانات والمراجع العابرة للشركات مع التراجع عن الكتابات، ومطابقة كتالوج التغطية. غطّت أمثلة الترحيل جداول `TaxTrans` و`TaxJournalTrans` و`MarkupTrans` عبر خدمات الأعمال، مع التحقق من عدم تكرار السجلات عند إعادة التنفيذ. نجحت 48 نقطة تحقق للخدمات على كل من SQLite وSQL Server، واكتمل تنظيف قاعدة SQL Server. الأدلة في `.validation/erp-phase3-foundation.trx` و`.validation/erp-phase3-foundation-stages.jsonl`. لم تكشف إعادة التحقق عن عيب يستدعي تعديل التهيئة.

الفحص مباشر لخدمات الخلفية باستخدام نموذج EF الحقيقي والمفاتيح الخارجية. SQL Server يستخدم قاعدة مؤقتة مستقلة؛ SQLite يستخدم قاعدة ذاكرة مع تكييف أنواع التخزين فقط. لم تُستخدم قاعدة التطبيق ولم تُشغّل عليها migrations أو seeders.

الحصر: **60 خدمة ومكوّن منطق** داخل مجلدات Services في Foundation، باستبعاد خرائط التحويل والاستثناءات: 32 خدمة CRUD و28 خدمة أو مكوّنًا متخصصًا. ملف FoundationServiceCoverage.json يحدد المصادر وأدلة الاختبار. المكونات المساعدة تُختبر مباشرة أو من خلال خدمات الأعمال التي تستدعيها؛ لا يمثل ذلك تغطية لكل سطر أو فرع ممكن.

## نتيجة كل مرحلة

1. ✅ حصر خدمات CRUD — SQLite: **PASSED**؛ SQL Server: **PASSED**.
2. ✅ CashDiscService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
3. ✅ CurrencyService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
4. ✅ ExchangeRateCurrencyPairService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
5. ✅ ExchangeRateService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
6. ✅ ExchangeRateTypeService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
7. ✅ DlvModeService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
8. ✅ DlvTermService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
9. ✅ HcmDepartmentService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
10. ✅ GenderService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
11. ✅ HcmShowroomService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
12. ✅ HcmWorkerService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
13. ✅ CompanyInfoService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
14. ✅ ContactPersonService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
15. ✅ ElectronicAddressService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
16. ✅ LocationService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
17. ✅ PartyLocationService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
18. ✅ PartyService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
19. ✅ PostalAddressService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
20. ✅ MarkupTableService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
21. ✅ HcmNationalityService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
22. ✅ HcmOccupationService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
23. ✅ OrganizationUnitService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
24. ✅ PaymSchedLineService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
25. ✅ PaymSchedService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
26. ✅ PaymTermService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
27. ✅ TaxAuthorityAddressService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
28. ✅ TaxExemptCodeService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
29. ✅ TaxGroupService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
30. ✅ TaxItemGroupService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
31. ✅ TaxLedgerAccountGroupService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
32. ✅ TaxPeriodHeadCrudService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
33. ✅ TaxTableService — قراءة وإنشاء وتعديل وحذف — SQLite: **PASSED**؛ SQL Server: **PASSED**.
34. ✅ حفظ أزواج العملات وأسعار الصرف والتحويل والتقريب — SQLite: **PASSED**؛ SQL Server: **PASSED**.
35. ✅ مجموعات الضرائب وبنودها واستعلاماتها — SQLite: **PASSED**؛ SQL Server: **PASSED**.
36. ✅ مجموعات ضرائب الأصناف وبنودها واستعلاماتها — SQLite: **PASSED**؛ SQL Server: **PASSED**.
37. ✅ إنشاء وتعديل وقراءة وحذف الفترة الضريبية — SQLite: **PASSED**؛ SQL Server: **PASSED**.
38. ✅ الأطراف والعناوين وملكية الموقع والتراجع عند فشل العنوان — SQLite: **PASSED**؛ SQL Server: **PASSED**.
39. ✅ عزل الشركات عند قراءة المفاتيح العادية والمركبة المتتبعة — SQLite: **PASSED**؛ SQL Server: **PASSED**.
40. ✅ تواريخ الاستحقاق وجداول الدفع وحساب الخصم النقدي — SQLite: **PASSED**؛ SQL Server: **PASSED**.
41. ✅ الشركة ودفتر العناوين في معاملة واحدة — SQLite: **PASSED**؛ SQL Server: **PASSED**.
42. ✅ العامل والمعرض والتكليفات وحماية المفاتيح والتراجع — SQLite: **PASSED**؛ SQL Server: **PASSED**.
43. ✅ أكواد الضرائب والأسعار ومعاملة الحفظ وحماية المراجع — SQLite: **PASSED**؛ SQL Server: **PASSED**.
44. ✅ بنية الحسابات وتفعيلها وربطها بالأستاذ — SQLite: **PASSED**؛ SQL Server: **PASSED**.
45. ✅ إنشاء المصاريف وحسابها وتعديلها وحذفها ورفض المدخلات الخاطئة — SQLite: **PASSED**؛ SQL Server: **PASSED**.
46. ✅ الأثر المالي والمخزني للضريبة والمصاريف والتقرير والتسوية — SQLite: **PASSED**؛ SQL Server: **PASSED**.
47. ✅ منع قراءة وتعديل وحذف مفاتيح ضرائب شركة أخرى — SQLite: **PASSED**؛ SQL Server: **PASSED**.
48. ✅ الوظائف ونقل العامل وحفظ التاريخ ورفض التداخل — SQLite: **PASSED**؛ SQL Server: **PASSED**.

## الأرقام المتوقعة المستقلة

- تحويل 805 دولارات: 3,018.75 ريال عند 3.75 و3,059 ريال عند 3.80؛ الفارق 40.25 ريال. تقريب 1.005 ريال إلى 1.01.
- خصم 2% من فاتورة 2,875 ريال = 57.50 ريال؛ نهاية مهلة الخصم مشمولة واليوم التالي غير مشمول.
- استحقاق NET30 لفاتورة 15 أبريل هو 15 مايو؛ جدول الدفع يحفظ قسطين 50% + 50%. هذا لا يثبت توليد فواتير أقساط؛ مسار حساب الاستحقاق يرفض جدول أقساط غير مدعوم.
- فاتورة المصروف: 350 ريال + 52.50 ضريبة. الشراء المخزني والمصاريف: 2,500 مخزون + 50 توصيل + 382.50 ضريبة.
- المجموع: ضريبة مدخلات 435 ريال، حركة بنك ‎-3,335 ريال، مخزون 100 وحدة بقيمة 2,500 ريال. TaxJournalTrans للمصروف 52.50 ريال.
- معاينة تسوية الضريبة لا تكتب قيودًا. الترحيل يصفّر حساب ضريبة المدخلات وينقل 435 ريال إلى حساب التسوية المهيأ، مع توازن كل قيد وبقاء المخزون 100 وحدة وقيمته 2,500 ريال. التسوية الثانية مرفوضة.

## سجل الإصلاحات

1. ❌ قراءة سجل متتبّع بالمفتاح بعد تبديل الشركة — Failed فعليًا. 🔧 استبدلت قراءة مخزن التتبع باستعلام يطبق المرشحات للمفاتيح العادية والمركبة، وعولجت المسارات المماثلة في مجموعات الضرائب وبنودها. النتيجة النهائية في المرحلتين الأخيرتين أعلاه.
2. ❌ تحديث العامل بطلب يحمل RecId مختلفًا — Failed فعليًا. 🔧 تجاهل المفتاح الوارد في تحويل بيانات العامل والمعرض؛ الاختبار يحافظ على المفتاح والطرف الأصليين. النتيجة النهائية في مرحلة العامل والمعرض أعلاه.
3. وجدت بالمراجعة أن عمليات العناوين لا تتحقق من ارتباط الموقع بالطرف المطلوب. 🔧 أضيف التحقق قبل التعديل والحذف، وإغلاق المعاملة عند غياب السجل. فحوص منع تعديل/حذف عنوان طرف آخر والتراجع عند دولة غير معروفة ضمن مرحلة العناوين. لم يُسجل لهذا العيب فشل تشغيل سابق للإصلاح؛ أضيف له اختبار انحدار.

4. أضيف أيضًا فحص الحفظ الجماعي للشركة مع عنوان وهاتف في الموقع نفسه. أُصلح استعلام الاتصالات ليشمل الموقع البريدي، ويحافظ حذف الاتصال على العنوان وبقية الاتصالات. هذا الإصلاح وفحصه أُضيفا بعد بناء المجموعة الشاملة؛ لذلك أُعيد تشغيل اختباري Foundation على المحركين على النسخة الأخيرة بصورة منفصلة.

أخطاء تجهيز الاختبار محفوظة في السجل ومنفصلة عن عيوب الخدمات: مراجع إعفاء ووحدة قياس، تسلسلات أرقام، موقع فريد للعنوان، تفعيل العامل والمعرض، مسح التتبع قبل إعادة seed، ومعرف بنية الحسابات. لم تُلغَ قيود FK أو الفهارس الفريدة لجعل الاختبار ينجح.

## حدود النطاق وإعداد التشغيل

- وفّرت الاختبارات تهيئة إضافية لتسلسلات DirPartyTable وLogisticsLocation وHcmWorker وHcmShowroom وTaxSettlement وحساب تسوية الضريبة. نجاح الخدمات هنا لا يعني أن حزمة Foundation الأصلية وحدها تهيئ هذه الإعدادات التشغيلية الإضافية.
- SQLite لا يدعم ORDER BY للنوع decimal. ترتيب قائمة المصاريف في الخدمة يُختبر على SQL Server؛ على SQLite تُفحص الحركات المحفوظة والحسابات دون تغيير دقة تخزين المال إلى double.
- حذف كود ضريبة له أسعار مرتبطة مرفوض بقيود المرجع؛ لا يدّعي الفحص وجود حذف متسلسل للأسعار. حذف سجل غير مرجعي مغطى في CRUD.
- تسوية الضريبة الأصلية مغطاة؛ التصحيح على فترة مسوّاة مرفوض. بنى الحسابات الأساسية ذات المسار الواحد مغطاة، وليست البنى المتفرعة المتقدمة.
- إنشاء مخطط SQL عبر EnsureCreated يختبر الموديل الحالي، وليس سلسلة migrations التاريخية. لا يشمل هذا التقرير واجهة المستخدم أو صلاحيات HTTP.

## أدلة التشغيل

- SQLite: RunId `bec0b564d3b84d9485e9da4dc1d7a40f`؛ 48/48 مرحلة Passed؛ حالة التشغيل `PASSED`.
- SQL Server: RunId `34523ca45c024774ba88c051805572f3`؛ 48/48 مرحلة Passed؛ حالة التشغيل `PASSED`.
- `foundation-complete-regression.trx`: 855 Passed، 0 Failed، 0 NotExecuted؛ إجمالي 857.
- `foundation-final-services.trx`: 2 Passed، 0 Failed، 0 NotExecuted؛ إجمالي 2.
- `foundation-inventory-regression.trx`: 128 Passed، 0 Failed، 0 NotExecuted؛ إجمالي 128.
- سجل كل المحاولات بما فيها الفشل: `.validation/foundation-services/results.jsonl`.
- الاختبارات: `IXApi/Tests/FoundationServiceStageTests.cs`، مع الاختبارات المتخصصة الموجودة المذكورة في كتالوج التغطية.
- SQL Server مؤقت من النمط `IXApi_FoundationServices_<guid>` ويُحذف في finally؛ فحص الحذف النهائي مسجل بعد التشغيل.

## إعادة التشغيل

نفّذ من جذر IAX. مسار المخرجات المعزول يتجنب تعارض ملفات البناء مع API قيد التشغيل.

```powershell
$env:IXAPI_AP_PHASE_ONE_SQLSERVER = '(localdb)\MSSQLLocalDB'
$env:IXAPI_FOUNDATION_STAGE_REPORT = Join-Path $PWD '.validation/foundation-services/results.jsonl'
$validationOutput = Join-Path $PWD 'IXApi/.foundation-service-test/bin/'
dotnet build IXApi/IXApi.csproj -m:1 "/p:BaseOutputPath=$validationOutput" /p:UseSharedCompilation=false /p:RunAnalyzers=false
dotnet test IXApi/Tests/IXApi.Tests.csproj "/p:BaseOutputPath=$validationOutput" /p:BuildProjectReferences=false --filter FullyQualifiedName~FoundationServiceStageTests
```
