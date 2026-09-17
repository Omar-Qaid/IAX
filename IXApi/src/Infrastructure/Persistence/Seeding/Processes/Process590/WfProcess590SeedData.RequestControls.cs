using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Requests;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process590;

public sealed partial class WfProcess590SeedData
{
    private static async Task SeedRequestControlsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfRequestControls.IgnoreQueryFilters()
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfRequestControl>();

        var items = new[]
        {
            new { RecId = 20525L, ProcessId = 590L, ControlId = (byte)4, Name = "Date of Violation", NameAlias = "تاريخ المخالفة", SortOrder = (byte)4, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "Date", CanFilter = true, CanGroup = true, CanSort = true },
            new { RecId = 20526L, ProcessId = 590L, ControlId = (byte)13, Name = "The time for the violation begins", NameAlias = "وقت المخالفة يبدا", SortOrder = (byte)5, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "Time", CanFilter = true, CanGroup = true, CanSort = true },
            new { RecId = 20527L, ProcessId = 590L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)8, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "String", CanFilter = true, CanGroup = false, CanSort = true },
            new { RecId = 20528L, ProcessId = 590L, ControlId = (byte)10, Name = "Proof of Violation Attached", NameAlias = "يرجى ارفاق اثبات المخالفة ؟", SortOrder = (byte)8, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "String", CanFilter = false, CanGroup = false, CanSort = false },
            new { RecId = 20530L, ProcessId = 590L, ControlId = (byte)18, Name = "Name of showroom", NameAlias = " اسم الفرع", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 0.00m, ReferenceType = "Branch", FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "String", CanFilter = true, CanGroup = true, CanSort = true },
            new { RecId = 20531L, ProcessId = 590L, ControlId = (byte)6, Name = "Violation Type", NameAlias = "نوع المخالفة", SortOrder = (byte)3, ExtendedProperties = """
<Data><Item><ar>عدم الالتزام بمواعيد الافتتاح والإغلاق لنقاط البيع الفترة الصباحية والمسائية</ar><en>Failure to adhere to the opening and closing times of the points of sale during the morning and evening periods</en><value>عدم الالتزام بمواعيد الافتتاح والإغلاق لنقاط البيع الفترة الصباحية والمسائية</value></Item><Item><ar>عرقلة سير العمل بافتتاح المعرض أو إغلاق المعرض</ar><en>Obstructing the workflow by opening or closing the exhibition</en><value>عرقلة سير العمل بافتتاح المعرض أو إغلاق المعرض</value></Item><Item><ar>عدم الالتزام في الحضور للفرع بعد أداء الصلاة</ar><en>Failure to attend the branch after performing prayers</en><value>عدم الالتزام في الحضور للفرع بعد أداء الصلاة</value></Item><Item><ar>عدم الالتزام في الحضور بعد أخذ فترة الاستراحة Break</ar><en>Failure to attend after taking a break</en><value>عدم الالتزام في الحضور بعد أخذ فترة الاستراحة Break</value></Item><Item><ar>تناول الطعام داخل الفرع أثناء ساعات العمل</ar><en>Eating inside the branch during working hours</en><value>تناول الطعام داخل الفرع أثناء ساعات العمل</value></Item><Item><ar>الشرب داخل الفرع أمام العملاء</ar><en>Drinking inside the branch in front of customers</en><value>الشرب داخل الفرع أمام العملاء</value></Item><Item><ar>النوم أثناء العمل في الفرع أو المستودع</ar><en>Sleeping while working in the branch or warehouse</en><value>النوم أثناء العمل في الفرع أو المستودع</value></Item><Item><ar>التجمع داخل المعرض او امام المعرض </ar><en>Gathering inside the branch or in front of the branch</en><value>التجمع داخل المعرض او امام المعرض </value></Item><Item><ar>عدم الالتزام بتشغيل جميع  إنارات الفرع</ar><en>Failure to operate all branch lights</en><value>عدم الالتزام بتشغيل جميع  إنارات الفرع</value></Item><Item><ar>عدم التواجد في الفرع أثناء ساعات العمل الرسمية</ar><en>Not being present in the branch during official working hours</en><value>عدم التواجد في الفرع أثناء ساعات العمل الرسمية</value></Item><Item><ar>الخروج وترك الفرع مفتوحاً دون سبب رسمي</ar><en>Going out and leaving the branch open without an official reason</en><value>الخروج وترك الفرع مفتوحاً دون سبب رسمي</value></Item><Item><ar>التلاعب بأجهزة البصمة أو أجهزة الكاميرات</ar><en>Tampering with fingerprint devices or camera devices</en><value>التلاعب بأجهزة البصمة أو أجهزة الكاميرات</value></Item><Item><ar>عدم الاهتمام بالادوات والاجهزة والتهاون في العمل الذي قد ينشأ ضرر في صحة الموظفين أو الادوات أو الاجهزة</ar><en>Lack of attention to tools and equipment and negligence in work that may cause harm to the health of employees, tools or equipment.</en><value>عدم الاهتمام بالادوات والاجهزة والتهاون في العمل الذي قد ينشأ ضرر في صحة الموظفين أو الادوات أو الاجهزة</value></Item><Item><ar>عدم الالتزام بإخلاقيات العمل بين زملاء العمل والعملاء</ar><en>Lack of adherence to work ethics among co-workers and clients</en><value>عدم الالتزام بإخلاقيات العمل بين زملاء العمل والعملاء</value></Item><Item><ar>عدم وضع الاغراض الشخصية في مكانها المخصص</ar><en>Do not put personal belongings in the designated place</en><value>عدم وضع الاغراض الشخصية في مكانها المخصص</value></Item><Item><ar>استخدام الهاتف المحمول أو السماعات </ar><en>Use a mobile phone or headphones</en><value>استخدام الهاتف المحمول أو السماعات </value></Item><Item><ar>عدم الالتزام بالزي الرسمي الموحد المعتمد من المنشاة</ar><en>Failure to adhere to the uniform uniform approved by the facility</en><value>عدم الالتزام بالزي الرسمي الموحد المعتمد من المنشاة</value></Item><Item><ar>عدم الالتزام بالنظافة والترتيب بعد الانتهاء من خدمة العميل</ar><en>Failure to adhere to cleanliness and tidiness after completing customer service</en><value>عدم الالتزام بالنظافة والترتيب بعد الانتهاء من خدمة العميل</value></Item><Item><ar>عدم الالتزام بالنظافة وتنظيم الفرع والمستودع</ar><en>Failure to adhere to the cleanliness and organization of the branch and warehouse</en><value>عدم الالتزام بالنظافة وتنظيم الفرع والمستودع</value></Item><Item><ar>عدم الالتزام بإستلام البضاعة مباشرة</ar><en>Failure to commit to receiving the goods directly</en><value>عدم الالتزام بإستلام البضاعة مباشرة</value></Item><Item><ar>عدم الالتزام بالمحافظة على نظافة الادراج الداخلية</ar><en>Failure to maintain the cleanliness of internal drawers</en><value>عدم الالتزام بالمحافظة على نظافة الادراج الداخلية</value></Item><Item><ar>استعمال الدخان والشيشة في المعرض أو امام المعرض أو المستودع</ar><en>Using smoke and shisha in the showroom or in front of the showroom or warehouse</en><value>استعمال الدخان والشيشة في المعرض أو امام المعرض أو المستودع</value></Item><Item><ar>إبراز علامة تجارية مختلفة منافسة أو غير منافسة سواء كان في أكياس  أو منتجات أو كراتين أو بروش</ar><en>Highlighting a different, competing or non-competing brand, whether in bags, products, cartons, or brooches</en><value>إبراز علامة تجارية مختلفة منافسة أو غير منافسة سواء كان في أكياس  أو منتجات أو كراتين أو بروش</value></Item><Item><ar>عدم الالتزام باستلام البضائع خلال مدة الاستلام الرسمية</ar><en>Failure to receive the goods within the official receipt period</en><value>عدم الالتزام باستلام البضائع خلال مدة الاستلام الرسمية</value></Item><Item><ar>اغلاق الفرع أثناء ساعات العمل بدون سبب</ar><en>Closing the branch during working hours for no reason</en><value>اغلاق الفرع أثناء ساعات العمل بدون سبب</value></Item><Item><ar>عدم وقوف واستقبال العملاء من جميع الموظفين أثناء دخول العميل</ar><en>All employees must stand and receive customers while the customer enters</en><value>عدم وقوف واستقبال العملاء من جميع الموظفين أثناء دخول العميل</value></Item><Item><ar>عدم مباشرة العملاء بإحترافية</ar><en>Not dealing with clients professionally</en><value>عدم مباشرة العملاء بإحترافية</value></Item><Item><ar>دمج فترة الاستراحة مع وقت أداء الصلاة</ar><en>Integrate the rest period with prayer time</en><value>دمج فترة الاستراحة مع وقت أداء الصلاة</value></Item><Item><ar>عدم المحافظة على الذوق العام</ar><en>Failure to maintain public taste</en><value>عدم المحافظة على الذوق العام</value></Item><Item><ar>استخدام أجهزة الشركة في اعمال غير رسمية</ar><en>Using company devices for informal work</en><value>استخدام أجهزة الشركة في اعمال غير رسمية</value></Item><Item><ar>عدم الالتزام بتسليم الفاتورة للعميل</ar><en>Failure to deliver the invoice to the customer</en><value>عدم الالتزام بتسليم الفاتورة للعميل</value></Item><Item><ar>عدم الالتزام بالجلوس بشكل لائق</ar><en>Failure to adhere to sitting properly</en><value>عدم الالتزام بالجلوس بشكل لائق</value></Item><Item><ar>استقبال زوار داخل المعرض أو امام المعرض أو خروج معه</ar><en>Receiving visitors inside the exhibition, in front of the exhibition, or going out with it</en><value>استقبال زوار داخل المعرض أو امام المعرض أو خروج معه</value></Item><Item><ar>عدم الالتزام بالتبخير أو التعطير للعملاء المارين امام بوابة الفرع</ar><en>Failure to commit to fumigating or perfuming customers passing in front of the branch gate</en><value>عدم الالتزام بالتبخير أو التعطير للعملاء المارين امام بوابة الفرع</value></Item><Item><ar>الالتزام بعدم أداء الصلاة داخل الفرع</ar><en>Commitment not to perform prayer inside the branch</en><value>الالتزام بعدم أداء الصلاة داخل الفرع</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "String", CanFilter = true, CanGroup = true, CanSort = true },
            new { RecId = 20572L, ProcessId = 590L, ControlId = (byte)17, Name = "List of quality and performance standards at points of sale", NameAlias = "لائحة معايير الجودة الاداء في نقاط البيع", SortOrder = (byte)0, ExtendedProperties = """
<Data><Item><fileId>123519</fileId><ar>الجودة .pdf</ar><en>الجودة .pdf</en><value>Attachments\RequestControl\20572\الجودة .pdf</value><type>pdf</type><length>373997</length></Item></Data>
""", IsActive = true, IsMandatory = false, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "String", CanFilter = false, CanGroup = false, CanSort = false },
            new { RecId = 20595L, ProcessId = 590L, ControlId = (byte)13, Name = "The violation time end", NameAlias = "وقت المخالفة ينتهي", SortOrder = (byte)6, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "Time", CanFilter = true, CanGroup = true, CanSort = true },
            new { RecId = 20724L, ProcessId = 590L, ControlId = (byte)6, Name = "Is the branch inside the Kingdom?", NameAlias = "هل الفرع  داخل المملكة؟", SortOrder = (byte)7, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "String", CanFilter = true, CanGroup = true, CanSort = true },
            new { RecId = 21828L, ProcessId = 590L, ControlId = (byte)6, Name = "Area name", NameAlias = "اسم المنطقة ", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>المنطقة الوسطى </ar><en>Central region</en><value>Central region</value><weight>0</weight></Item><Item><ar>المنطقة الغربية</ar><en>Western region</en><value>Western region</value><weight>0</weight></Item><Item><ar>المنطقة الشرقية </ar><en>Eastern Province</en><value>Eastern Province</value><weight>0</weight></Item><Item><ar>المنطقة الشمالية</ar><en>Northern region</en><value>Northern region</value><weight>0</weight></Item><Item><ar>المنطقة الجنوبية</ar><en>Southern region</en><value>Southern region</value><weight>0</weight></Item><Item><ar>الامارات العربية المتحدة </ar><en>United Arab Emirates</en><value>United Arab Emirates</value><weight>0</weight></Item><Item><ar>عمان </ar><en>Oman</en><value>Oman</value><weight>0</weight></Item><Item><ar>الكويت</ar><en>Kuwait</en><value>Kuwait</value><weight>0</weight></Item><Item><ar>البحرين</ar><en>Bahrain</en><value>Bahrain</value><weight>0</weight></Item><Item><ar>قطر</ar><en>Qatar</en><value>Qatar</value><weight>0</weight></Item><Item><ar>اوربا </ar><en>Europe</en><value>Europe</value><weight>0</weight></Item><Item><ar>امريكا</ar><en>America</en><value>America</value><weight>0</weight></Item><Item><ar>مصر </ar><en>Egypt</en><value>Egypt</value><weight>0</weight></Item><Item><ar>العراق</ar><en>Iraq</en><value>Iraq</value><weight>0</weight></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "String", CanFilter = true, CanGroup = true, CanSort = true },
        };

        foreach (var item in items)
        {
            if (!existingIds.Contains(item.RecId))
            {
                toAdd.Add(new WfRequestControl
                {
                    RecId = item.RecId,
                    Code = "RC" + item.RecId,
                    ProcessId = item.ProcessId,
                    ControlId = item.ControlId,
                    Name = item.Name,
                    NameAlias = item.NameAlias,
                    SortOrder = item.SortOrder,
                    ExtendedProperties = item.ExtendedProperties,
                    ValidationRules = item.IsMandatory ? RequiredRule : null,
                    Score = item.Score,
                    ReferenceType = item.ReferenceType,
                    FieldRole = item.FieldRole,
                    DefaultAggregation = item.DefaultAggregation,
                    DataType = item.DataType,
                    CanFilter = item.CanFilter,
                    CanGroup = item.CanGroup,
                    CanSort = item.CanSort,
                    CreatedBy = owner,
                    OwnerAccountId = owner,
                    IsActive = item.IsActive
                });
            }
        }

        if (toAdd.Any())
        {
            await db.WfRequestControls.AddRangeAsync(toAdd, ct);
            
            await db.Database.OpenConnectionAsync(ct);
            try
            {
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfRequestControls ON", ct);
                await db.SaveChangesAsync(ct);
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfRequestControls OFF", ct);
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        }
    }
}
