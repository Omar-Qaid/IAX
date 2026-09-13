using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process603;

public sealed partial class WfProcess603SeedData
{
    private static async Task SeedActivityControlsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfActivityControls.IgnoreQueryFilters()
            .Where(x => x.ProcessId == 603L)
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfActivityControl>();
        string RequiredRule = "[{\"rule\":\"required\"}]";

        var items = new[]
        {
            new { RecId = 37893L, ActivityId = 6139L, ControlId = (byte)3, Name = "NOTES", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 0.00m },
            new { RecId = 37894L, ActivityId = 6140L, ControlId = (byte)6, Name = "Do the deposits match", NameAlias = "هل الايداعات مطابقة", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item><Item><ar>تمرير لمحاسب دول الخليج</ar><en>Pass to the accountant of the Gulf countries</en><value>تمرير لمحاسب دول الخليج</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 0.00m },
            new { RecId = 37895L, ActivityId = 6140L, ControlId = (byte)3, Name = "NOTES", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 0.00m },
            new { RecId = 37896L, ActivityId = 6141L, ControlId = (byte)6, Name = "Seen", NameAlias = "تم اﻻطلاع", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>تمرير للموارد البشرية</ar><en>Pass to Human Resources</en><value>تمرير للموارد البشرية</value></Item><Item><ar>تحويل المعاملة لمحاسب السعودية</ar><en>Transferring the transaction to a Saudi accountant</en><value>تحويل المعاملة لمحاسب السعودية</value></Item><Item><ar>تحويل المعاملة لمحاسب دول الخليج</ar><en>Transferring the transaction to an accountant in the Gulf countries</en><value>تحويل المعاملة لمحاسب دول الخليج</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 0.00m },
            new { RecId = 37897L, ActivityId = 6141L, ControlId = (byte)3, Name = "NOTES", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 0.00m },
            new { RecId = 37898L, ActivityId = 6142L, ControlId = (byte)6, Name = "Seen", NameAlias = "تم اﻻطلاع", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 0.00m },
            new { RecId = 37899L, ActivityId = 6142L, ControlId = (byte)3, Name = "NOTES", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 0.00m },
            new { RecId = 37901L, ActivityId = 6143L, ControlId = (byte)3, Name = "NOTES", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 0.00m },
            new { RecId = 38110L, ActivityId = 6245L, ControlId = (byte)6, Name = "Do the deposits match", NameAlias = "هل الايداعات مطابقة", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item><Item><ar>اعادة لمحاسب مبيعات المملكة العربية السعودية</ar><en>Return to the sales accountant in the Kingdom of Saudi Arabia</en><value>اعادة لمحاسب مبيعات المملكة العربية السعودية</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 0.00m },
            new { RecId = 38111L, ActivityId = 6245L, ControlId = (byte)3, Name = "NOTES", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 0.00m },
            new { RecId = 38250L, ActivityId = 6143L, ControlId = (byte)6, Name = "Seen", NameAlias = "تم الاطلاع", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 0.00m },
            new { RecId = 39733L, ActivityId = 6754L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 0.00m },
            new { RecId = 39734L, ActivityId = 6754L, ControlId = (byte)6, Name = "Is the covenant compatible", NameAlias = "هل العهد مطابقة", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 0.00m },
            new { RecId = 39890L, ActivityId = 6802L, ControlId = (byte)6, Name = "Seen", NameAlias = "تم الاطلاع", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>No</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 0.00m },
            new { RecId = 39891L, ActivityId = 6802L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 0.00m },
            new { RecId = 39892L, ActivityId = 6139L, ControlId = (byte)6, Name = "The problem has been fixed", NameAlias = "تم إصلاح المشكلة", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item><Item><ar>اعادة لمقدم الطلب </ar><en>Return to the requester </en><value>rs</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 0.00m },
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
                    ProcessId = 603L,
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
