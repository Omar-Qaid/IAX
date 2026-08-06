using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    public class SettingsSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
        {
            var already = await db.SysSettings.AnyAsync(ct);
            if (!already)
            {
                var settings = new SysSettings
                {
                    AppName = "HBMC ERP",
                    DefaultLanguage = "en",
                    TimeZone = "UTC",
                    Currency = "USD",
                    DateFormat = "YYYY-MM-DD",
                    EnableAuditLog = true,
                    MaxUploadSize = 10485760, // 10MB
                    PaginationSize = 10,
                    DecimalPlaces = 2,
                    IsActive = true
                };

                await db.SysSettings.AddAsync(settings, ct);
                await db.SaveChangesAsync(ct);
            }

            // Seed notification templates
            var hasTemplates = await db.Set<SysNotificationTemplate>().AnyAsync(ct);
            if (!hasTemplates)
            {
                var templates = new List<SysNotificationTemplate>
                {
                    new SysNotificationTemplate
                    {
                        Code = "WF_REQUEST_SUBMITTED",
                        Name = "Workflow Request Submitted",
                        NameAR = "تقديم طلب إجراء العمل",
                        Subject = "Workflow Request Submitted: {{RequestNumber}}",
                        SubjectAR = "تم تقديم طلب إجراء العمل: {{RequestNumber}}",
                        Body = "Hello {{UserName}},\n\nYour workflow request for '{{RequestType}}' (Ref: {{RequestNumber}}) has been successfully submitted and is currently pending approval.\n\nStatus: {{RequestStatus}}",
                        BodyAR = "مرحباً {{UserName}}،\n\nتم تقديم طلب إجراء العمل لـ '{{RequestType}}' (المرجع: {{RequestNumber}}) بنجاح وهو قيد الموافقة حالياً.\n\nالحالة: {{RequestStatus}}",
                        Variables = "UserName,RequestNumber,RequestType,RequestStatus",
                        Icon = "send",
                        DefaultPriority = SysNotificationPriority.High,
                        DefaultCategory = "Workflow",
                        DefaultChannel = SysNotificationChannel.InApp,
                        Language = "all",
                        IsDeleted = false
                    },
                    new SysNotificationTemplate
                    {
                        Code = "WF_REQUEST_APPROVED",
                        Name = "Workflow Request Approved",
                        NameAR = "الموافقة على طلب إجراء العمل",
                        Subject = "Workflow Request Approved: {{RequestNumber}}",
                        SubjectAR = "تمت الموافقة على طلب إجراء العمل: {{RequestNumber}}",
                        Body = "Hello {{UserName}},\n\nGreat news! Your workflow request for '{{RequestType}}' (Ref: {{RequestNumber}}) has been approved.\n\nDetails: Approved by {{ApproverName}}.",
                        BodyAR = "مرحباً {{UserName}}،\n\nأخبار رائعة! تمت الموافقة على طلب إجراء العمل لـ '{{RequestType}}' (المرجع: {{RequestNumber}}).\n\nالتفاصيل: تمت الموافقة بواسطة {{ApproverName}}.",
                        Variables = "UserName,RequestNumber,RequestType,ApproverName",
                        Icon = "check_circle",
                        DefaultPriority = SysNotificationPriority.High,
                        DefaultCategory = "Workflow",
                        DefaultChannel = SysNotificationChannel.InApp,
                        Language = "all",
                        IsDeleted = false
                    },
                    new SysNotificationTemplate
                    {
                        Code = "WF_REQUEST_REJECTED",
                        Name = "Workflow Request Rejected",
                        NameAR = "رفض طلب إجراء العمل",
                        Subject = "Workflow Request Rejected: {{RequestNumber}}",
                        SubjectAR = "تم رفض طلب إجراء العمل: {{RequestNumber}}",
                        Body = "Hello {{UserName}},\n\nYour workflow request for '{{RequestType}}' (Ref: {{RequestNumber}}) has been rejected.\n\nReason: {{RejectionReason}}",
                        BodyAR = "مرحباً {{UserName}}،\n\nتم رفض طلب إجراء العمل الخاص بك لـ '{{RequestType}}' (المرجع: {{RequestNumber}}).\n\nالسبب: {{RejectionReason}}",
                        Variables = "UserName,RequestNumber,RequestType,RejectionReason",
                        Icon = "cancel",
                        DefaultPriority = SysNotificationPriority.Critical,
                        DefaultCategory = "Workflow",
                        DefaultChannel = SysNotificationChannel.InApp,
                        Language = "all",
                        IsDeleted = false
                    },
                    new SysNotificationTemplate
                    {
                        Code = "INV_LOW_STOCK",
                        Name = "Inventory Low Stock Alert",
                        NameAR = "تنبيه انخفاض المخزون",
                        Subject = "Low Stock Alert: {{ ame}}",
                        SubjectAR = "تنبيه انخفاض المخزون: {{ ame}}",
                        Body = "Attention Procurement Team,\n\nThe stock level for  '{{ ame}}' (SKU: {{SKU}}) has dropped below the safety threshold. Current stock is {{CurrentStock}} {{UOM}}, while the safety limit is {{SafetyLimit}} {{UOM}}.\n\nPlease arrange for reorder immediately.",
                        BodyAR = "إلى فريق المشتريات،\n\nانخفض مستوى المخزون للصنف '{{ ame}}' (SKU: {{SKU}}) إلى ما دون حد الأمان. المخزون الحالي هو {{CurrentStock}} {{UOM}}، في حين أن حد الأمان هو {{SafetyLimit}} {{UOM}}.\n\nيرجى التنسيق لإعادة الطلب فوراً.",
                        Variables = " ame,SKU,CurrentStock,UOM,SafetyLimit",
                        Icon = "warning",
                        DefaultPriority = SysNotificationPriority.High,
                        DefaultCategory = "Inventory",
                        DefaultChannel = SysNotificationChannel.InApp,
                        Language = "all",
                        IsDeleted = false
                    },
                    new SysNotificationTemplate
                    {
                        Code = "FIN_INVOICE_DUE",
                        Name = "Invoice Payment Due Reminder",
                        NameAR = "تذكير بموعد استحقاق الفاتورة",
                        Subject = "Invoice Due: {{InvoiceNumber}}",
                        SubjectAR = "استحقاق الفاتورة: {{InvoiceNumber}}",
                        Body = "Dear Customer,\n\nThis is a friendly reminder that invoice '{{InvoiceNumber}}' with an outstanding balance of {{Amount}} {{Currency}} is due on {{DueDate}}.\n\nPlease process your payment at your earliest convenience.",
                        BodyAR = "عزيزي العميل،\n\nهذا تذكير ودي بأن الفاتورة '{{InvoiceNumber}}' برصيد مستحق قدره {{Amount}} {{Currency}} يستحق السداد في {{DueDate}}.\n\nيرجى معالجة الدفع في أقرب وقت ممكن.",
                        Variables = "InvoiceNumber,Amount,Currency,DueDate",
                        Icon = "receipt",
                        DefaultPriority = SysNotificationPriority.Medium,
                        DefaultCategory = "Finance",
                        DefaultChannel = SysNotificationChannel.InApp,
                        Language = "all",
                        IsDeleted = false
                    },
                    new SysNotificationTemplate
                    {
                        Code = "HR_LEAVE_APPROVED",
                        Name = "HR Leave Request Approved",
                        NameAR = "الموافقة على طلب الإجازة",
                        Subject = "Leave Request Approved: {{LeaveType}}",
                        SubjectAR = "تمت الموافقة على طلب الإجازة: {{LeaveType}}",
                        Body = "Hello {{UserName}},\n\nYour leave request for '{{LeaveType}}' from {{StartDate}} to {{EndDate}} has been approved by your department manager.",
                        BodyAR = "مرحباً {{UserName}}،\n\nتمت الموافقة على طلب الإجازة الخاص بك لـ '{{LeaveType}}' من {{StartDate}} إلى {{EndDate}} بواسطة مدير قسمك.",
                        Variables = "UserName,LeaveType,StartDate,EndDate",
                        Icon = "event_available",
                        DefaultPriority = SysNotificationPriority.Medium,
                        DefaultCategory = "HR",
                        DefaultChannel = SysNotificationChannel.InApp,
                        Language = "all",
                        IsDeleted = false
                    }
                };

                await db.Set<SysNotificationTemplate>().AddRangeAsync(templates, ct);
                await db.SaveChangesAsync(ct);
            }

            // Seed the recurring workflow auto-pass sweep job (every 15 minutes).
            var hasAutoPassJob = await db.Set<SysBackgroundJob>()
                .AnyAsync(j => j.JobKey == "WfActivityAutoPass", ct);
            if (!hasAutoPassJob)
            {
                await db.Set<SysBackgroundJob>().AddAsync(new SysBackgroundJob
                {
                    Name = "Workflow Activity Auto-Pass Sweep",
                    JobKey = "WfActivityAutoPass",
                    Description = "Auto-finishes workflow assignments whose AutoPassingHrs window has elapsed.",
                    ScheduleType = SysJobScheduleType.Recurring,
                    IntervalSeconds = 900,
                    Status = SysJobStatus.Active,
                    IsEnabled = true,
                    PreventOverlap = true,
                    MaxRetryCount = 1,
                    RetryDelaySeconds = 60,
                    TimeoutSeconds = 300,
                    NextRunAt = DateTime.UtcNow,
                }, ct);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}

