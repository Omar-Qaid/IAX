using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Requests;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process603;

public sealed partial class WfProcess603SeedData
{
    private static async Task SeedRequestControlsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfRequestControls.IgnoreQueryFilters()
            .Where(x => x.ProcessId == 603L)
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfRequestControl>();
        string RequiredRule = "[{\"rule\":\"required\"}]";

        var items = new[]
        {
            new { RecId = 20627L, ProcessId = 603L, ControlId = (byte)12, Name = "Name of the depository employee", NameAlias = "اسم موظف الايداع", SortOrder = (byte)1, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 0.00m, ReferenceType = "Employee", FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "String", CanFilter = true, CanGroup = true, CanSort = true },
            new { RecId = 20628L, ProcessId = 603L, ControlId = (byte)1, Name = "رقم الايداع", NameAlias = "رقم الايداع", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Measure", DefaultAggregation = "SUM", DataType = "Integer", CanFilter = true, CanGroup = false, CanSort = true },
            new { RecId = 20629L, ProcessId = 603L, ControlId = (byte)1, Name = "Amount", NameAlias = "المبلغ", SortOrder = (byte)3, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Measure", DefaultAggregation = "SUM", DataType = "Integer", CanFilter = true, CanGroup = false, CanSort = true },
            new { RecId = 20633L, ProcessId = 603L, ControlId = (byte)3, Name = "NOTES", NameAlias = "ملاحظات", SortOrder = (byte)10, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "String", CanFilter = true, CanGroup = false, CanSort = true },
            new { RecId = 20634L, ProcessId = 603L, ControlId = (byte)6, Name = "Has the transfer document been attached?", NameAlias = "هل تم ارفاق سند التحويل", SortOrder = (byte)7, ExtendedProperties = "<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value><weight>0</weight></Item><Item><ar>لا</ar><en>No</en><value>لا</value><weight>0</weight></Item></Data>", IsActive = true, IsMandatory = true, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "String", CanFilter = true, CanGroup = true, CanSort = true },
            new { RecId = 20654L, ProcessId = 603L, ControlId = (byte)4, Name = "Filing date", NameAlias = "تاريخ الايداع", SortOrder = (byte)4, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "Date", CanFilter = true, CanGroup = true, CanSort = true },
            new { RecId = 20655L, ProcessId = 603L, ControlId = (byte)4, Name = "Deposit period from", NameAlias = "فترة الايداع من", SortOrder = (byte)5, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "Date", CanFilter = true, CanGroup = true, CanSort = true },
            new { RecId = 20656L, ProcessId = 603L, ControlId = (byte)4, Name = "Deposit period to", NameAlias = "فترة الايداع الى", SortOrder = (byte)6, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "Date", CanFilter = true, CanGroup = true, CanSort = true },
            new { RecId = 21962L, ProcessId = 603L, ControlId = (byte)6, Name = "Country", NameAlias = "الدولة", SortOrder = (byte)8, ExtendedProperties = "<Data><Item><ar>المملكة العربية السعودية</ar><en>Ksa</en><value>ksa</value><weight>0</weight></Item><Item><ar>الإمارات العربية المتحدة</ar><en>Uae</en><value>uae</value><weight>0</weight></Item><Item><ar>عمان</ar><en>Oman</en><value>om</value><weight>0</weight></Item><Item><ar>قطر</ar><en>Qatar</en><value>qat</value><weight>0</weight></Item><Item><ar>الكويت</ar><en>Kuwait</en><value>kw</value><weight>0</weight></Item><Item><ar>البحرين</ar><en>Bahrain</en><value>bh</value><weight>0</weight></Item><Item><ar>الولايات المتحدة الأمريكية </ar><en>Usa</en><value>usa</value><weight>0</weight></Item><Item><ar>مصر</ar><en>Eygpt</en><value>eg</value><weight>0</weight></Item></Data>", IsActive = true, IsMandatory = true, Score = 0.00m, ReferenceType = (string?)null, FieldRole = "Dimension", DefaultAggregation = "NONE", DataType = "String", CanFilter = true, CanGroup = true, CanSort = true },
        };

        foreach (var item in items)
        {
            if (item.RecId == 0) continue;

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
