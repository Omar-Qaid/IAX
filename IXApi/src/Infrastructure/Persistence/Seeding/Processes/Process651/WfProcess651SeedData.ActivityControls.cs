using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Activities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Processes.Process651;

public sealed partial class WfProcess651SeedData
{
    private static async Task SeedActivityControlsAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfActivityControls.IgnoreQueryFilters()
            .Where(x => x.ProcessId == 651L)
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfActivityControl>();
        var generatedControls = new List<WfActivityControl>();
        string RequiredRule = "[{\"rule\":\"required\"}]";

        var items = new[]
        {
            new { RecId = 37887L, ActivityId = 6386L, ControlId = (byte)12, Name = "Name of depository employee", NameAlias = "اسم موظف الايداع", SortOrder = (byte)1, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 3.65m },
            new { RecId = 37888L, ActivityId = 6386L, ControlId = (byte)1, Name = "Deposit number", NameAlias = "رقم الايداع", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 3.65m },
            new { RecId = 37889L, ActivityId = 6386L, ControlId = (byte)1, Name = "Amount", NameAlias = "المبلغ", SortOrder = (byte)3, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 3.65m },
            new { RecId = 37914L, ActivityId = 6386L, ControlId = (byte)4, Name = "Deposit date", NameAlias = "تاريخ الايداع", SortOrder = (byte)4, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 3.65m },
            new { RecId = 37915L, ActivityId = 6386L, ControlId = (byte)4, Name = "Filing period from", NameAlias = "فترة الايداع من", SortOrder = (byte)5, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 3.65m },
            new { RecId = 37916L, ActivityId = 6386L, ControlId = (byte)4, Name = "Deposit period to", NameAlias = "فترة الايداع الى", SortOrder = (byte)6, ExtendedProperties = "", IsActive = true, IsMandatory = true, Score = 3.65m },
            new { RecId = 37890L, ActivityId = 6386L, ControlId = (byte)6, Name = "Is the conversion document attached?", NameAlias = "هل تم ارفاق سند التحويل", SortOrder = (byte)7, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 3.65m },
            new { RecId = 37891L, ActivityId = 6386L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)8, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 3.65m },
            new { RecId = 37880L, ActivityId = 6375L, ControlId = (byte)6, Name = "Customer Service Supervisor Approval", NameAlias = "اعتماد مشرف خدمة العملاء", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 3.65m },
            new { RecId = 37881L, ActivityId = 6375L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 3.65m },
            new { RecId = 37882L, ActivityId = 6376L, ControlId = (byte)6, Name = "Approval of Financial Accountant", NameAlias = "اعتماد محاسب المالية", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>لا</ar><en>No</en><value>لا</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 3.65m },
            new { RecId = 37883L, ActivityId = 6376L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 3.65m },
            new { RecId = 37884L, ActivityId = 6377L, ControlId = (byte)6, Name = "E-Commerce Accountant Approval", NameAlias = "اعتماد محاسب المتجر الالكتروني", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>نعم</ar><en>Yes</en><value>نعم</value></Item><Item><ar>اعادة لمقدم الطلب</ar><en>Return to applicant</en><value>اعادة لمقدم الطلب</value></Item><Item><ar>اعادة لمحاسب المالية</ar><en>Return to the Financial Accountant</en><value>اعادة لمحاسب المالية</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 3.65m },
            new { RecId = 37885L, ActivityId = 6377L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 3.65m },
            new { RecId = 38096L, ActivityId = 6664L, ControlId = (byte)6, Name = "Store accountant approval", NameAlias = "اعتماد محاسب المتجر", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>YES</ar><en>YES</en><value>YES</value></Item><Item><ar>NO</ar><en>NO</en><value>NO</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 3.65m },
            new { RecId = 38097L, ActivityId = 6664L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 3.65m },
            new { RecId = 38122L, ActivityId = 6675L, ControlId = (byte)6, Name = "applicant", NameAlias = "مقدم الطلب", SortOrder = (byte)1, ExtendedProperties = """
<Data><Item><ar>YES</ar><en>YES</en><value>YES</value></Item><Item><ar>NO</ar><en>NO</en><value>NO</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 3.65m },
            new { RecId = 38123L, ActivityId = 6675L, ControlId = (byte)3, Name = "Notes", NameAlias = "ملاحظات", SortOrder = (byte)2, ExtendedProperties = "", IsActive = true, IsMandatory = false, Score = 3.65m },
            new { RecId = 38946L, ActivityId = 6386L, ControlId = (byte)6, Name = "Select the type of transaction return", NameAlias = "تحديد نوع إرجاع العملية", SortOrder = (byte)0, ExtendedProperties = """
<Data><Item><ar>تعديل وتحديث كشوفات في المتجر الإلكتروني فقط ( دون عكس عملية الدفع)</ar><en>Modifying and updating statements in the online store only (without reversing the payment process)</en><value>1</value></Item><Item><ar>عكس عملية الدفع من الحساب البنكي أو بطاقة مدى لعميل المتجر (عن طريق المتجر الإلكتروني) وسوف تصل للعميل خلال أسبوعين عمل .</ar><en>Reversing the payment transaction from the bank account or Mada card for the store's customer (via the online store), and it will reach the customer within two business weeks.</en><value>2</value></Item><Item><ar>عكس عملية الدفع من الحساب البنكي لعميل المتجر الإلكتروني (عن طريق المالية ) وسوف تصل للعميل خلال 7 أيام عمل</ar><en>Reversing the payment transaction from the bank account for the online store's customer (via Finance), and it will reach the customer within 7 business days.</en><value>3</value></Item><Item><ar>عكس عملية الدفع بطاقة المدى أوالفيزا أوالماستر لعميل المتجر الإلكتروني (عن طريق المالية ) وسوف تصل للعميل خلال 14 أيام عمل</ar><en>Reversing the payment transaction using a Mada card, Visa, or Mastercard for the online store's customer (via Finance), and it will reach the customer within 14 business days.</en><value>4</value></Item><Item><ar>عكس عملية الدفع بطاقة تابي أوهابي أوتمارا لعميل المتجر الإلكتروني (عن طريق المالية ) وسوف تصل للعميل خلال 14 أيام عمل</ar><en>Reversing the payment transaction using a Tabby, Happy, or Tamara card for the online store's customer (via Finance), and it will reach the customer within 14 business days.</en><value>5</value></Item><Item><ar>عكس عملية الدفع لعميل خارجي خارج السعودية (عن طريق المالية ) وسوف تصل للعميل خلال 14 أيام عمل</ar><en>Reversing the payment transaction for an external customer outside Saudi Arabia (via Finance), and it will reach the customer within 14 business days.</en><value>6</value></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 0m },
            new { RecId = 39891L, ActivityId = 6386L, ControlId = (byte)6, Name = "Country", NameAlias = "الدولة", SortOrder = (byte)7, ExtendedProperties = """
<Data><Item><ar>المملكة العربية السعودية</ar><en>Ksa</en><value>ksa</value><weight>0</weight></Item><Item><ar>الإمارات العربية المتحدة</ar><en>Uae</en><value>uae</value><weight>0</weight></Item><Item><ar>عمان</ar><en>Oman</en><value>om</value><weight>0</weight></Item><Item><ar>قطر</ar><en>Qatar</en><value>qat</value><weight>0</weight></Item><Item><ar>الكويت</ar><en>Kuwait</en><value>kw</value><weight>0</weight></Item><Item><ar>البحرين</ar><en>Bahrain</en><value>bh</value><weight>0</weight></Item><Item><ar>الولايات المتحدة الأمريكية </ar><en>Usa</en><value>usa</value><weight>0</weight></Item><Item><ar>مصر</ar><en>Eygpt</en><value>eg</value><weight>0</weight></Item></Data>
""", IsActive = true, IsMandatory = true, Score = 0m },
        };

        foreach (var item in items)
        {
            // 39891 is already the Notes control in process 603. Country must use
            // its own generated identity, while retaining its legacy code for reruns.
            var generateIdentity = item.RecId == 39891L;
            var code = "ACTCTRL" + item.RecId;
            var exists = generateIdentity
                ? await db.WfActivityControls.IgnoreQueryFilters().AnyAsync(x =>
                    x.ProcessId == 651L && x.ActivityId == item.ActivityId && x.Code == code, ct)
                : existingIds.Contains(item.RecId);
            if (!exists)
            {
                var control = new WfActivityControl
                {
                    RecId = generateIdentity ? 0L : item.RecId,
                    Code = code,
                    ActivityId = item.ActivityId,
                    ProcessId = 651L,
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
                };
                (generateIdentity ? generatedControls : toAdd).Add(control);
            }
        }

        if (toAdd.Any())
        {
            await db.WfActivityControls.AddRangeAsync(toAdd, ct);
            await SaveWithIdentityAsync(db, "WfActivityControls", ct);
        }

        // Generated keys must be saved only after IDENTITY_INSERT has been turned off.
        if (generatedControls.Count > 0)
        {
            await db.WfActivityControls.AddRangeAsync(generatedControls, ct);
            await db.SaveChangesAsync(ct);
        }
    }
}
