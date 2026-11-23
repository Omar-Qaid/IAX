using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Communication;

/// <summary>
/// The notification templates the application raises events against. The template code carries a
/// unique index with no company in it, so a template belongs to the installation: the first company to
/// seed owns the row and a rerun leaves an edited subject or body exactly as it is.
/// </summary>
public sealed class SysNotificationTemplateSeeder : IDataSeeder
{
    public string Id => "ERPV2.SysNotificationTemplateSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CompanyInfoSeeder"];

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        var existing = await c.Db.Set<SysNotificationTemplate>().IgnoreQueryFilters()
            .Select(x => x.Code).ToListAsync(ct);
        var codes = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var template in Templates())
        {
            if (codes.Contains(template.Code)) continue;
            template.DataAreaId = c.Company;
            template.CreatedBy = c.Actor;
            template.LastModifiedBy = c.Actor;
            template.OwnerAccountId = c.Actor;
            c.Db.Set<SysNotificationTemplate>().Add(template);
        }
        await c.Db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Every placeholder a subject or body uses is also declared in <c>Variables</c>, which is what the
    /// notification service fills the template from; the integrity validator holds the two to that.
    /// </summary>
    private static IEnumerable<SysNotificationTemplate> Templates()
    {
        yield return new SysNotificationTemplate
        {
            Code = "WF_REQUEST_SUBMITTED",
            Name = "Workflow Request Submitted",
            Subject = "Workflow Request Submitted: {{RequestNumber}}",
            SubjectAR = "تم تقديم طلب إجراء العمل: {{RequestNumber}}",
            Body = "Hello {{UserName}},\n\nYour workflow request for '{{RequestType}}' (Ref: {{RequestNumber}}) has been successfully submitted and is currently pending approval.\n\nStatus: {{RequestStatus}}",
            BodyAR = "مرحباً {{UserName}}،\n\nتم تقديم طلب إجراء العمل لـ '{{RequestType}}' (المرجع: {{RequestNumber}}) بنجاح وهو قيد الموافقة حالياً.\n\nالحالة: {{RequestStatus}}",
            Variables = "UserName,RequestNumber,RequestType,RequestStatus",
            Icon = "send",
            DefaultPriority = SysNotificationPriority.High,
            DefaultCategory = "Workflow",
            DefaultChannel = SysNotificationChannel.InApp,
            Language = "all"
        };
        yield return new SysNotificationTemplate
        {
            Code = "WF_REQUEST_APPROVED",
            Name = "Workflow Request Approved",
            Subject = "Workflow Request Approved: {{RequestNumber}}",
            SubjectAR = "تمت الموافقة على طلب إجراء العمل: {{RequestNumber}}",
            Body = "Hello {{UserName}},\n\nYour workflow request for '{{RequestType}}' (Ref: {{RequestNumber}}) has been approved.\n\nDetails: Approved by {{ApproverName}}.",
            BodyAR = "مرحباً {{UserName}}،\n\nتمت الموافقة على طلب إجراء العمل لـ '{{RequestType}}' (المرجع: {{RequestNumber}}).\n\nالتفاصيل: تمت الموافقة بواسطة {{ApproverName}}.",
            Variables = "UserName,RequestNumber,RequestType,ApproverName",
            Icon = "check_circle",
            DefaultPriority = SysNotificationPriority.High,
            DefaultCategory = "Workflow",
            DefaultChannel = SysNotificationChannel.InApp,
            Language = "all"
        };
        yield return new SysNotificationTemplate
        {
            Code = "WF_REQUEST_REJECTED",
            Name = "Workflow Request Rejected",
            Subject = "Workflow Request Rejected: {{RequestNumber}}",
            SubjectAR = "تم رفض طلب إجراء العمل: {{RequestNumber}}",
            Body = "Hello {{UserName}},\n\nYour workflow request for '{{RequestType}}' (Ref: {{RequestNumber}}) has been rejected.\n\nReason: {{RejectionReason}}",
            BodyAR = "مرحباً {{UserName}}،\n\nتم رفض طلب إجراء العمل الخاص بك لـ '{{RequestType}}' (المرجع: {{RequestNumber}}).\n\nالسبب: {{RejectionReason}}",
            Variables = "UserName,RequestNumber,RequestType,RejectionReason",
            Icon = "cancel",
            DefaultPriority = SysNotificationPriority.Critical,
            DefaultCategory = "Workflow",
            DefaultChannel = SysNotificationChannel.InApp,
            Language = "all"
        };
        yield return new SysNotificationTemplate
        {
            Code = "INV_LOW_STOCK",
            Name = "Inventory Low Stock Alert",
            Subject = "Low Stock Alert: {{ItemName}}",
            SubjectAR = "تنبيه انخفاض المخزون: {{ItemName}}",
            Body = "Attention Procurement Team,\n\nThe stock level for '{{ItemName}}' (SKU: {{SKU}}) has dropped below the safety threshold. Current stock is {{CurrentStock}} {{UOM}}, while the safety limit is {{SafetyLimit}} {{UOM}}.\n\nPlease arrange for reorder immediately.",
            BodyAR = "إلى فريق المشتريات،\n\nانخفض مستوى المخزون للصنف '{{ItemName}}' (SKU: {{SKU}}) إلى ما دون حد الأمان. المخزون الحالي هو {{CurrentStock}} {{UOM}}، في حين أن حد الأمان هو {{SafetyLimit}} {{UOM}}.\n\nيرجى التنسيق لإعادة الطلب فوراً.",
            Variables = "ItemName,SKU,CurrentStock,UOM,SafetyLimit",
            Icon = "warning",
            DefaultPriority = SysNotificationPriority.High,
            DefaultCategory = "Inventory",
            DefaultChannel = SysNotificationChannel.InApp,
            Language = "all"
        };
        yield return new SysNotificationTemplate
        {
            Code = "FIN_INVOICE_DUE",
            Name = "Invoice Payment Due Reminder",
            Subject = "Invoice Due: {{InvoiceNumber}}",
            SubjectAR = "استحقاق الفاتورة: {{InvoiceNumber}}",
            Body = "Dear Customer,\n\nThis is a reminder that invoice '{{InvoiceNumber}}' with an outstanding balance of {{Amount}} {{Currency}} is due on {{DueDate}}.\n\nPlease process your payment at your earliest convenience.",
            BodyAR = "عزيزي العميل،\n\nهذا تذكير بأن الفاتورة '{{InvoiceNumber}}' برصيد مستحق قدره {{Amount}} {{Currency}} تستحق السداد في {{DueDate}}.\n\nيرجى معالجة الدفع في أقرب وقت ممكن.",
            Variables = "InvoiceNumber,Amount,Currency,DueDate",
            Icon = "receipt",
            DefaultPriority = SysNotificationPriority.Medium,
            DefaultCategory = "Finance",
            DefaultChannel = SysNotificationChannel.InApp,
            Language = "all"
        };
        yield return new SysNotificationTemplate
        {
            Code = "HR_LEAVE_APPROVED",
            Name = "HR Leave Request Approved",
            Subject = "Leave Request Approved: {{LeaveType}}",
            SubjectAR = "تمت الموافقة على طلب الإجازة: {{LeaveType}}",
            Body = "Hello {{UserName}},\n\nYour leave request for '{{LeaveType}}' from {{StartDate}} to {{EndDate}} has been approved by your department manager.",
            BodyAR = "مرحباً {{UserName}}،\n\nتمت الموافقة على طلب الإجازة الخاص بك لـ '{{LeaveType}}' من {{StartDate}} إلى {{EndDate}} بواسطة مدير قسمك.",
            Variables = "UserName,LeaveType,StartDate,EndDate",
            Icon = "event_available",
            DefaultPriority = SysNotificationPriority.Medium,
            DefaultCategory = "HR",
            DefaultChannel = SysNotificationChannel.InApp,
            Language = "all"
        };
    }
}
