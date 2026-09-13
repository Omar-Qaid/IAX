using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process590;

public sealed partial class WfProcess590SeedData
{
    private static async Task SeedActivityControlsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfActivityControls.IgnoreQueryFilters()
            .Where(x => x.ProcessId == 590L)
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfActivityControl>();
        string RequiredRule = "[{\"rule\":\"required\"}]";

        var items = new[]
        {
            new { RecId = 37642L, ActivityId = 6037L, ControlId = (byte)12, Name = "The violating employee", NameAlias = "اسم الموظف المخالف", SortOrder = (byte)1, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 3.93m },
            new { RecId = 37643L, ActivityId = 6037L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 3.93m },
            new { RecId = 37644L, ActivityId = 6038L, ControlId = (byte)6, Name = "Do you have an objection to the violation", NameAlias = "هل لديك اعتراض على المخالفة", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 3.93m },
            new { RecId = 37645L, ActivityId = 6038L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 3.93m },
            new { RecId = 37648L, ActivityId = 6036L, ControlId = (byte)6, Name = "Approval", NameAlias = "الموافقة", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item><Item><ar>اعادة لمقدم الطلب </ar><en>Return to the applicant </en><value>12</value></Item></Data>
""", IsActive = true, IsMandatory = false, Score = 3.93m },
            new { RecId = 37649L, ActivityId = 6040L, ControlId = (byte)6, Name = "Approval", NameAlias = "اعتماد المخالفة على الموظف", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item><Item><ar>اعادة لمدير المعرض</ar><en>Return to the branch manager</en><value>اعادة لمدير المعرض</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 3.93m },
            new { RecId = 37650L, ActivityId = 6040L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 3.93m },
            new { RecId = 37651L, ActivityId = 6041L, ControlId = (byte)6, Name = "The violation was recorded in the system", NameAlias = "تم إدراج المخالفة على النظام", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 3.93m },
            new { RecId = 37652L, ActivityId = 6041L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)3, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 3.93m },
            new { RecId = 37653L, ActivityId = 6042L, ControlId = (byte)6, Name = "Seen", NameAlias = "تم اﻻطلاع", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>ﻻ</ar><en>No</en><value>لا</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 3.93m },
            new { RecId = 37654L, ActivityId = 6042L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 3.93m },
            new { RecId = 37655L, ActivityId = 6036L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 3.93m },
            new { RecId = 37667L, ActivityId = 6046L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 3.93m },
            new { RecId = 37710L, ActivityId = 6039L, ControlId = (byte)12, Name = "Name of the showroom manager", NameAlias = "اسم مدير المعرض", SortOrder = (byte)1, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 3.93m },
            new { RecId = 37711L, ActivityId = 6039L, ControlId = (byte)3, Name = "NOTES", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 3.93m },
            new { RecId = 37978L, ActivityId = 6046L, ControlId = (byte)6, Name = "The violation was charged to the employee", NameAlias = "تم ادراج المخالفة بالنظام", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item><Item><ar>إعادة إلى موظف الموارد في المملكة العربية السعودية</ar><en>Return the request to the hr officer </en><value>إعادة إلى موظف الموارد في المملكة العربية السعودية</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 3.93m },
            new { RecId = 39599L, ActivityId = 6260L, ControlId = (byte)6, Name = "Seen", NameAlias = "تم الاطلاع", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item><Item><ar>إعادة إلى موظف الموارد في المملكة العربية السعودية</ar><en>Return to the HR employee in the Kingdom of Saudi Arabia</en><value>إعادة إلى موظف الموارد في المملكة العربية السعودية</value></Item><Item><ar>اعادة لموظف الموارد البشرية دول الخليج</en><value>اعادة لموظف الموارد البشرية دول الخليج</value></Item><Item><ar>إعادة لمدير المعرض (خارج السعودية)</ar><en>Return to showroom manager (outside Saudi Arabia)</en><value>إعادة لمدير المعرض (خارج السعودية)</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 3.93m },
            new { RecId = 39600L, ActivityId = 6260L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 3.93m },
            new { RecId = 38563L, ActivityId = 6417L, ControlId = (byte)6, Name = "Seen", NameAlias = "تم الاطلاع", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 3.93m },
            new { RecId = 38564L, ActivityId = 6417L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 3.93m },
            new { RecId = 39532L, ActivityId = 6037L, ControlId = (byte)2, Name = "Employee's job number", NameAlias = "الرقم الوظيفي للموظف ", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 3.93m },
            new { RecId = 39536L, ActivityId = 6041L, ControlId = (byte)6, Name = "Type of action taken", NameAlias = "نوع الاجراء المتخذ", SortOrder = (byte)2, ExtendedProperties = """
<Data><Item><ar>انذار كتابي</ar><en>Written warning</en><value>Written warning</value></Item><Item><ar>خصم 5% من اليوم</ar><en>5% discount from today</en><value>5% discount from today</value></Item><Item><ar>خصم 10% من اليوم</ar><en>10% discount from today</en><value>10% discount from today</value></Item><Item><ar>خصم 15% من اليوم</ar><en>15% discount from today</en><value>15% discount from today</value></Item><Item><ar>خصم 25% من اليوم</ar><en>25% discount from today</en><value>25% discount from today</value></Item><Item><ar>خصم 50% من اليوم</ar><en>50% discount from today</en><value>50% discount from today</value></Item><Item><ar>خصم 75% من اليوم</ar><en>75% discount from today</en><value>75% discount from today</value></Item><Item><ar>خصم يوم</ar><en>Day discount</en><value>Day discount</value></Item><Item><ar>خصم يومين</ar><en>Two days discount</en><value>Two days discount</value></Item><Item><ar>خصم 3 ايام</ar><en>3 days discount</en><value>3 days discount</value></Item><Item><ar>خصم 5 ايام</ar><en>5 days discount</en><value>5 days discount</value></Item><Item><ar>فصل مع مكافاة</ar><en>Dismissal with reward</en><value>Dismissal with reward</value></Item><Item><ar>لفت نظر</ar><en>Draw attention</en><value>Draw attention</value></Item><Item><ar>تجاوز</ar><en>Allowed</en><value>Allowed</value></Item><Item><ar>تحويل الى الدائرة القانونية</ar><en>Transfer to the legal department</en><value>Transfer to the legal department</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 0m },
            new { RecId = 39658L, ActivityId = 6046L, ControlId = (byte)6, Name = "Type of action taken", NameAlias = "نوع الاجراء المتخذ", SortOrder = (byte)0, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>YES</en><value>YES</value></Item><Item><ar>لا</ar><en>NO</en><value>NO</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 0m },
        };

        foreach (var item in items)
        {
            if (!existingIds.Contains(item.RecId))
            {
                toAdd.Add(new WfActivityControl
                {
                    RecId = item.RecId,
                    Code = "ACTCTRL" + item.RecId,
                    ActivityId = item.ActivityId,
                    ProcessId = 590L,
                    ControlId = item.ControlId,
                    Name = item.Name,
                    NameAlias = item.NameAlias,
                    SortOrder = item.SortOrder,
                    ExtendedProperties = item.ExtendedProperties,
                    ValidationRules = item.IsMandatory ? RequiredRule : null,
                    Score = item.Score,
                    CreatedBy = owner,
                    OwnerAccountId = owner,
                    IsActive = item.IsActive
                });
            }
        }

        if (toAdd.Any())
        {
            await db.WfActivityControls.AddRangeAsync(toAdd, ct);
            
            await db.Database.OpenConnectionAsync(ct);
            try
            {
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfActivityControls ON", ct);
                await db.SaveChangesAsync(ct);
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfActivityControls OFF", ct);
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        }
    }
}
