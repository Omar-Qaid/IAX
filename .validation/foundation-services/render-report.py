from pathlib import Path
import json
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[2]
folder = Path(__file__).parent
records = [json.loads(line) for line in (folder / 'results.jsonl').read_text(encoding='utf-8-sig').splitlines() if line.strip()]
latest = {}
for item in records:
    latest[item['provider']] = item['runId']
events = {provider: [r for r in records if r['runId'] == run] for provider, run in latest.items()}
stages = list(dict.fromkeys(r['stage'] for r in records if r['stage'] != 'run'))
names = {
    'Foundation CRUD inventory': 'حصر خدمات CRUD',
    'Currency pair bulk save and effective rate': 'حفظ أزواج العملات وأسعار الصرف والتحويل والتقريب',
    'Payment due dates, schedules and cash discount arithmetic': 'تواريخ الاستحقاق وجداول الدفع وحساب الخصم النقدي',
    'Tax group commands, lines and queries': 'مجموعات الضرائب وبنودها واستعلاماتها',
    'Tax item group commands, lines and queries': 'مجموعات ضرائب الأصناف وبنودها واستعلاماتها',
    'Tax period create, update, query and delete': 'إنشاء وتعديل وقراءة وحذف الفترة الضريبية',
    'Party, location, postal and electronic addresses': 'الأطراف والعناوين وملكية الموقع والتراجع عند فشل العنوان',
    'Legal entity and address book transaction': 'الشركة ودفتر العناوين في معاملة واحدة',
    'Worker, showroom and assignment services': 'العامل والمعرض والتكليفات وحماية المفاتيح والتراجع',
    'Organization positions, transfer history and overlap rejection': 'الوظائف ونقل العامل وحفظ التاريخ ورفض التداخل',
    'Tax codes, rates and save transaction': 'أكواد الضرائب والأسعار ومعاملة الحفظ وحماية المراجع',
    'Account structure service and dimensions': 'بنية الحسابات وتفعيلها وربطها بالأستاذ',
    'Charge commands, calculation, update, deletion and validation': 'إنشاء المصاريف وحسابها وتعديلها وحذفها ورفض المدخلات الخاطئة',
    'Tax and charges financial effects, reporting and settlement': 'الأثر المالي والمخزني للضريبة والمصاريف والتقرير والتسوية',
    'Tracked primary keys respect company isolation': 'عزل الشركات عند قراءة المفاتيح العادية والمركبة المتتبعة',
    'Tracked tax keys cannot read, update or delete another company': 'منع قراءة وتعديل وحذف مفاتيح ضرائب شركة أخرى',
}
def status(provider, stage):
    found = [r for r in events.get(provider, []) if r['stage'] == stage]
    return found[-1]['status'] if found else 'NOT_RUN'

lines = ['# تقرير اختبار خدمات Foundation', '',
    'الفحص مباشر لخدمات الخلفية باستخدام نموذج EF الحقيقي والمفاتيح الخارجية. SQL Server يستخدم قاعدة مؤقتة مستقلة؛ SQLite يستخدم قاعدة ذاكرة مع تكييف أنواع التخزين فقط. لم تُستخدم قاعدة التطبيق ولم تُشغّل عليها migrations أو seeders.', '',
    'الحصر: **60 خدمة ومكوّن منطق** داخل مجلدات Services في Foundation، باستبعاد خرائط التحويل والاستثناءات: 32 خدمة CRUD و28 خدمة أو مكوّنًا متخصصًا. ملف FoundationServiceCoverage.json يحدد المصادر وأدلة الاختبار. المكونات المساعدة تُختبر مباشرة أو من خلال خدمات الأعمال التي تستدعيها؛ لا يمثل ذلك تغطية لكل سطر أو فرع ممكن.', '',
    '## نتيجة كل مرحلة', '']
for number, stage in enumerate(stages, 1):
    title = names.get(stage, stage.replace(': read/create/update/delete', ' — قراءة وإنشاء وتعديل وحذف'))
    sqlite, sql = status('SQLite', stage), status('SQL Server', stage)
    icon = '✅' if sqlite == sql == 'PASSED' else '❌' if 'FAILED' in (sqlite, sql) else '⏳'
    lines.append(f'{number}. {icon} {title} — SQLite: **{sqlite}**؛ SQL Server: **{sql}**.')
lines += ['', '## الأرقام المتوقعة المستقلة', '',
    '- تحويل 805 دولارات: 3,018.75 ريال عند 3.75 و3,059 ريال عند 3.80؛ الفارق 40.25 ريال. تقريب 1.005 ريال إلى 1.01.',
    '- خصم 2% من فاتورة 2,875 ريال = 57.50 ريال؛ نهاية مهلة الخصم مشمولة واليوم التالي غير مشمول.',
    '- استحقاق NET30 لفاتورة 15 أبريل هو 15 مايو؛ جدول الدفع يحفظ قسطين 50% + 50%. هذا لا يثبت توليد فواتير أقساط؛ مسار حساب الاستحقاق يرفض جدول أقساط غير مدعوم.',
    '- فاتورة المصروف: 350 ريال + 52.50 ضريبة. الشراء المخزني والمصاريف: 2,500 مخزون + 50 توصيل + 382.50 ضريبة.',
    '- المجموع: ضريبة مدخلات 435 ريال، حركة بنك ‎-3,335 ريال، مخزون 100 وحدة بقيمة 2,500 ريال. TaxJournalTrans للمصروف 52.50 ريال.',
    '- معاينة تسوية الضريبة لا تكتب قيودًا. الترحيل يصفّر حساب ضريبة المدخلات وينقل 435 ريال إلى حساب التسوية المهيأ، مع توازن كل قيد وبقاء المخزون 100 وحدة وقيمته 2,500 ريال. التسوية الثانية مرفوضة.', '',
    '## سجل الإصلاحات', '',
    '1. ❌ قراءة سجل متتبّع بالمفتاح بعد تبديل الشركة — Failed فعليًا. 🔧 استبدلت قراءة مخزن التتبع باستعلام يطبق المرشحات للمفاتيح العادية والمركبة، وعولجت المسارات المماثلة في مجموعات الضرائب وبنودها. النتيجة النهائية في المرحلتين الأخيرتين أعلاه.',
    '2. ❌ تحديث العامل بطلب يحمل RecId مختلفًا — Failed فعليًا. 🔧 تجاهل المفتاح الوارد في تحويل بيانات العامل والمعرض؛ الاختبار يحافظ على المفتاح والطرف الأصليين. النتيجة النهائية في مرحلة العامل والمعرض أعلاه.',
    '3. وجدت بالمراجعة أن عمليات العناوين لا تتحقق من ارتباط الموقع بالطرف المطلوب. 🔧 أضيف التحقق قبل التعديل والحذف، وإغلاق المعاملة عند غياب السجل. فحوص منع تعديل/حذف عنوان طرف آخر والتراجع عند دولة غير معروفة ضمن مرحلة العناوين. لم يُسجل لهذا العيب فشل تشغيل سابق للإصلاح؛ أضيف له اختبار انحدار.', '',
    '4. أضيف أيضًا فحص الحفظ الجماعي للشركة مع عنوان وهاتف في الموقع نفسه. أُصلح استعلام الاتصالات ليشمل الموقع البريدي، ويحافظ حذف الاتصال على العنوان وبقية الاتصالات. هذا الإصلاح وفحصه أُضيفا بعد بناء المجموعة الشاملة؛ لذلك أُعيد تشغيل اختباري Foundation على المحركين على النسخة الأخيرة بصورة منفصلة.', '',
    'أخطاء تجهيز الاختبار محفوظة في السجل ومنفصلة عن عيوب الخدمات: مراجع إعفاء ووحدة قياس، تسلسلات أرقام، موقع فريد للعنوان، تفعيل العامل والمعرض، مسح التتبع قبل إعادة seed، ومعرف بنية الحسابات. لم تُلغَ قيود FK أو الفهارس الفريدة لجعل الاختبار ينجح.', '',
    '## حدود النطاق وإعداد التشغيل', '',
    '- وفّرت الاختبارات تهيئة إضافية لتسلسلات DirPartyTable وLogisticsLocation وHcmWorker وHcmShowroom وTaxSettlement وحساب تسوية الضريبة. نجاح الخدمات هنا لا يعني أن حزمة Foundation الأصلية وحدها تهيئ هذه الإعدادات التشغيلية الإضافية.',
    '- SQLite لا يدعم ORDER BY للنوع decimal. ترتيب قائمة المصاريف في الخدمة يُختبر على SQL Server؛ على SQLite تُفحص الحركات المحفوظة والحسابات دون تغيير دقة تخزين المال إلى double.',
    '- حذف كود ضريبة له أسعار مرتبطة مرفوض بقيود المرجع؛ لا يدّعي الفحص وجود حذف متسلسل للأسعار. حذف سجل غير مرجعي مغطى في CRUD.',
    '- تسوية الضريبة الأصلية مغطاة؛ التصحيح على فترة مسوّاة مرفوض. بنى الحسابات الأساسية ذات المسار الواحد مغطاة، وليست البنى المتفرعة المتقدمة.',
    '- إنشاء مخطط SQL عبر EnsureCreated يختبر الموديل الحالي، وليس سلسلة migrations التاريخية. لا يشمل هذا التقرير واجهة المستخدم أو صلاحيات HTTP.', '',
    '## أدلة التشغيل', '']
summary = {}
for provider, rows in events.items():
    passed = sum(status(provider, stage) == 'PASSED' for stage in stages)
    lines.append(f'- {provider}: RunId `{latest[provider]}`؛ {passed}/{len(stages)} مرحلة Passed؛ حالة التشغيل `{status(provider,"run")}`.')
ns = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
for name in ['foundation-complete-regression.trx', 'foundation-final-services.trx', 'foundation-inventory-regression.trx']:
    path = folder / name
    if path.exists():
        doc = ET.parse(path)
        counters = doc.find('.//t:Counters', ns).attrib
        summary[name] = counters
        lines.append(f'- `{name}`: {counters.get("passed")} Passed، {counters.get("failed")} Failed، {counters.get("notExecuted")} NotExecuted؛ إجمالي {counters.get("total")}.')
lines += ['- سجل كل المحاولات بما فيها الفشل: `.validation/foundation-services/results.jsonl`.',
    '- الاختبارات: `IXApi/Tests/FoundationServiceStageTests.cs`، مع الاختبارات المتخصصة الموجودة المذكورة في كتالوج التغطية.',
    '- SQL Server مؤقت من النمط `IXApi_FoundationServices_<guid>` ويُحذف في finally؛ فحص الحذف النهائي مسجل بعد التشغيل.', '',
    '## إعادة التشغيل', '', 'نفّذ من جذر IAX. مسار المخرجات المعزول يتجنب تعارض ملفات البناء مع API قيد التشغيل.', '', '```powershell',
    "$env:IXAPI_AP_PHASE_ONE_SQLSERVER = '(localdb)\MSSQLLocalDB'",
    "$env:IXAPI_FOUNDATION_STAGE_REPORT = Join-Path $PWD '.validation/foundation-services/results.jsonl'",
    "$validationOutput = Join-Path $PWD 'IXApi/.foundation-service-test/bin/'",
    'dotnet build IXApi/IXApi.csproj -m:1 "/p:BaseOutputPath=$validationOutput" /p:UseSharedCompilation=false /p:RunAnalyzers=false',
    'dotnet test IXApi/Tests/IXApi.Tests.csproj "/p:BaseOutputPath=$validationOutput" /p:BuildProjectReferences=false --filter FullyQualifiedName~FoundationServiceStageTests', '```', '']
(root / 'IXApi/docs/testing/FoundationServiceStageTestReport.md').write_text('\n'.join(lines), encoding='utf-8')
(folder / 'summary.json').write_text(json.dumps({'latestRuns': latest, 'stageCount': len(stages), 'testRuns': summary}, indent=2) + '\n', encoding='utf-8')
print(json.dumps({'stagesPerProvider': len(stages), 'latestRuns': latest, 'testRuns': summary}))
