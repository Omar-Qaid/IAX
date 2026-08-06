using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Identity.Authentication;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks
{
    /// <summary>
    /// Seeds one SysNumberSequence row for every entity that owns a Code field.
    /// EntityName must match the value passed to ISysNumberSequenceService.NextAsync
    /// (or AutoCodeExtensions.EnsureCodeAsync) from the corresponding service.
    /// </summary>
    public class NumberSequenceSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
        {
            var sysUser = await users.FindByNameAsync("sys");
            var createdBy = sysUser?.Id ?? "sys";

            // (Code, Name, NameAR, EntityName, Prefix, FormatPattern, PaddingLength, ResetCycle)
            var defs = new (string Code, string Name, string NameAR, string EntityName, string Prefix, string FormatPattern, int PaddingLength, SequenceResetCycle Reset)[]
            {
                // ─── ERP / Accounts ──────────────────────────────────────────────
                ("NS-CUS",       "Customer Sequence",         "تسلسل العميل",            "Customer",            "CUS", "{PREFIX}-{SEQ}",       5, SequenceResetCycle.Never),
                ("NS-CUSGRP",    "Customer Group Sequence",   "تسلسل مجموعة العميل",     "CustomerGroup",       "CGRP","{PREFIX}-{SEQ}",       4, SequenceResetCycle.Never),
                ("NS-VEN",       "Vendor Sequence",           "تسلسل المورد",            "Vendor",            "VEN", "{PREFIX}-{SEQ}",       5, SequenceResetCycle.Never),
                ("NS-VENGRP",    "Vendor Group Sequence",     "تسلسل مجموعة المورد",     "VendorGroup",       "VGRP","{PREFIX}-{SEQ}",       4, SequenceResetCycle.Never),
                ("NS-PARTNER",   "Partner Sequence",          "تسلسل الشريك",            "AccPartner",          "PRT", "{PREFIX}-{SEQ}",       5, SequenceResetCycle.Never),
                ("NS-PARTNERGRP","Partner Group Sequence",    "تسلسل مجموعة الشريك",     "AccPartnerGroup",     "PGRP","{PREFIX}-{SEQ}",       4, SequenceResetCycle.Never),
                ("NS-VOUCHER",   "Voucher Sequence",          "تسلسل القيد",             "Voucher",             "VOU", "{PREFIX}-{SEQ}",       6, SequenceResetCycle.Never),

                // ─── ERP / AR ────────────────────────────────────────────────────
                ("NS-SO",        "Sales Order Sequence",      "تسلسل أمر البيع",         "SalesTable",          "SO",  "{PREFIX}-{YYYY}-{SEQ}", 5, SequenceResetCycle.Yearly),
                ("NS-SOL",       "Sale Line Sequence",        "تسلسل بند البيع",         "SalesLine",           "SOL", "{PREFIX}-{SEQ}",       6, SequenceResetCycle.Never),
                ("NS-PS",        "Packing Slip Sequence",     "تسلسل إشعار التعبئة",     "PackingSlip",         "PS",  "{PREFIX}-{YYYY}-{SEQ}", 5, SequenceResetCycle.Yearly),
                ("NS-PICK",      "Picking Route Sequence",    "تسلسل قائمة الانتقاء",    "WMSPickingRoute",     "PICK","{PREFIX}-{YYYY}-{SEQ}", 5, SequenceResetCycle.Yearly),
                ("NS-CNFRM",     "SO Confirm Sequence",       "تسلسل تأكيد أمر البيع",   "CustConfirmJour",     "CONF","{PREFIX}-{YYYY}-{SEQ}", 5, SequenceResetCycle.Yearly),
                ("NS-PSJOURID",  "PS Journal ID Sequence",    "تسلسل قيد إشعار التعبئة", "CustPackingSlipJour", "PSJ", "{PREFIX}-{YYYY}-{SEQ}", 5, SequenceResetCycle.Yearly),
                ("NS-INV",       "Invoice Sequence",          "تسلسل الفاتورة",          "CustInvoiceJour",     "INV", "{PREFIX}-{YYYY}-{SEQ}", 5, SequenceResetCycle.Yearly),
                ("NS-INVT",      "Invoice Trans Sequence",    "تسلسل بند الفاتورة",      "CustInvoiceTrans",    "INVT","{PREFIX}-{SEQ}",       6, SequenceResetCycle.Never),
                ("NS-CTRANS",    "Cust Transaction Sequence", "تسلسل حركة العميل",       "CustTrans",           "CTRX","{PREFIX}-{YYYY}-{SEQ}", 6, SequenceResetCycle.Never),
                ("NS-CSETTLE",   "Cust Settlement Sequence",  "تسلسل تسوية العميل",      "CustSettlement",      "CSET","{PREFIX}-{SEQ}",       6, SequenceResetCycle.Never),
                ("NS-ORG",       "Lot Origin Sequence",       "تسلسل أصل لوت الحركة",   "InventTransOriginId", "ORG",  "{PREFIX}-{SEQ}",       6, SequenceResetCycle.Never),
                ("NS-LJT",       "Ledger Journal Sequence",   "تسلسل دفتر الأستاذ",      "LedgerJournalTable",  "LJT", "{PREFIX}-{YYYY}-{SEQ}", 5, SequenceResetCycle.Yearly),
                ("NS-INVENTJRN", "Inventory Journal Sequence", "تسلسل دفاتر المخزون",     "InventJournalTable",  "IJRN", "{PREFIX}-{YYYY}-{SEQ}", 5, SequenceResetCycle.Yearly),
                ("NS-PO",        "Purchase Order Sequence",   "تسلسل أمر الشراء",        "PurchTable",          "PO",  "{PREFIX}-{YYYY}-{SEQ}", 5, SequenceResetCycle.Yearly),
                ("NS-VTRANS",    "Vend Transaction Sequence", "تسلسل حركة المورد",       "VendTrans",           "VTRX","{PREFIX}-{YYYY}-{SEQ}", 6, SequenceResetCycle.Never),
                ("NS-VINVREG",   "Vend Invoice Register Seq", "تسلسل سجل فاتورة المورد", "VendInvoiceRegister", "VIR", "{PREFIX}-{YYYY}-{SEQ}", 6, SequenceResetCycle.Yearly),
                ("NS-EXCHADJ",   "Exch Adjustment Sequence",  "تسلسل تسوية الصرف",       "ExchAdjustment",      "FXA", "{PREFIX}-{YYYY}-{SEQ}", 5, SequenceResetCycle.Yearly),
                ("NS-VSETTLE",   "Vend Settlement Sequence",  "تسلسل تسوية المورد",      "VendSettlement",      "VSET","{PREFIX}-{SEQ}",       6, SequenceResetCycle.Never),

                // ─── ERP / Inventory ─────────────────────────────────────────────
    
                ("NS-LOT",       "Invent Lot Sequence",       "تسلسل لوت المخزون",       "InventTransId",       "LOT", "{PREFIX}-{SEQ}",       8, SequenceResetCycle.Never),
                ("NS- RP",   " Group Sequence",       "تسلسل مجموعة الصنف",      "Invent roup",        "IGRP","{PREFIX}-{SEQ}",       4, SequenceResetCycle.Never),
                ("NS-UOM",       "UOM Sequence",              "تسلسل وحدة القياس",       "UnitOfMeasure",          "UOM", "{PREFIX}-{SEQ}",       3, SequenceResetCycle.Never),
                ("NS-IUOM",      " UOM Sequence",         "تسلسل وحدة قياس الصنف",   "Invent OM",          "IUOM","{PREFIX}-{SEQ}",       4, SequenceResetCycle.Never),
                ("NS-INVTRX",    "Inv Transaction Sequence",  "تسلسل حركة المخزون",      "InventTrans",            "ITRX","{PREFIX}-{YYYY}{MM}-{SEQ}", 6, SequenceResetCycle.Monthly),

                // ─── Organization ────────────────────────────────────────────────
                ("NS-EMP",       "Employee Sequence",         "تسلسل الموظف",            "OrgEmployee",         "EMP", "{PREFIX}-{SEQ}",       5, SequenceResetCycle.Never),
                ("NS-DEPT",      "Department Sequence",       "تسلسل القسم",             "OrgDepartment",       "DPT", "{PREFIX}-{SEQ}",       4, SequenceResetCycle.Never),
                ("NS-OCC",       "Occupation Sequence",       "تسلسل المهنة",            "OrgOccupation",       "OCC", "{PREFIX}-{SEQ}",       4, SequenceResetCycle.Never),
                ("NS-NAT",       "Nationality Sequence",      "تسلسل الجنسية",           "OrgNationality",      "NAT", "{PREFIX}-{SEQ}",       4, SequenceResetCycle.Never),
                ("NS-GEN",       "Gender Sequence",           "تسلسل الجنس",             "OrgGender",           "GEN", "{PREFIX}-{SEQ}",       2, SequenceResetCycle.Never),
                ("NS-COMP",      "Company Sequence",          "تسلسل الشركة",            "OrgCompany",          "COMP","{PREFIX}-{SEQ}",       3, SequenceResetCycle.Never),
                ("NS-ANN",       "Announcement Sequence",     "تسلسل الإعلان",           "OrgAnnouncement",     "ANN", "{PREFIX}-{YYYY}-{SEQ}", 4, SequenceResetCycle.Yearly),
                ("NS-ATT",       "Attachment Sequence",       "تسلسل المرفق",            "OrgAttachment",       "ATT", "{PREFIX}-{SEQ}",       6, SequenceResetCycle.Never),
                ("NS-JOB",       "Job Sequence",              "تسلسل الوظيفة",           "OrgJob",              "JOB", "{PREFIX}-{SEQ}",       4, SequenceResetCycle.Never),

                // ─── Identity / Users ────────────────────────────────────────────
                ("NS-UGRP",      "User Group Sequence",       "تسلسل مجموعة المستخدم",   "OrgEmployeeGroup",           "UGRP","{PREFIX}-{SEQ}",       4, SequenceResetCycle.Never),
                ("NS-UCAT",      "User Category Sequence",    "تسلسل فئة المستخدم",      "OrgEmployeeCategory",        "UCAT","{PREFIX}-{SEQ}",       4, SequenceResetCycle.Never),

                // ─── Workflow ────────────────────────────────────────────────────
                ("NS-WFCAT",     "Wf Category Sequence",      "تسلسل تصنيف العملية",     "WfCategory",          "WCT", "{PREFIX}-{SEQ}",       4, SequenceResetCycle.Never),
                ("NS-WFPRI",     "Wf Priority Sequence",      "تسلسل أولوية العملية",    "WfPriority",          "WPR", "{PREFIX}-{SEQ}",       3, SequenceResetCycle.Never),
                ("NS-WFPROC",    "Wf Process Sequence",       "تسلسل العملية",           "WfProcess",           "PROC","{PREFIX}-{SEQ}",       5, SequenceResetCycle.Never),
                ("NS-WFSTEP",    "Wf Step Sequence",          "تسلسل خطوة العملية",      "WfStep",              "STEP","{PREFIX}-{SEQ}",       5, SequenceResetCycle.Never),
                ("NS-WFACT",     "Wf Activity Sequence",      "تسلسل نشاط العملية",      "WfActivity",          "ACT", "{PREFIX}-{SEQ}",       5, SequenceResetCycle.Never),
                ("NS-WFATYPE",   "Wf Activity Type Sequence", "تسلسل نوع النشاط",        "WfActivityType",      "ATYP","{PREFIX}-{SEQ}",       3, SequenceResetCycle.Never),
                ("NS-WFACTRL",   "Wf Activity Control Seq.",  "تسلسل عنصر نشاط العملية", "WfActivityControl",   "ACTL","{PREFIX}-{SEQ}",       5, SequenceResetCycle.Never),
                ("NS-WFREQCTL",  "Wf Request Control Seq.",   "تسلسل عنصر الطلب",        "WfRequestControl",    "RCTL","{PREFIX}-{SEQ}",       5, SequenceResetCycle.Never),
                ("NS-WFCTL",     "Wf Control Sequence",       "تسلسل العنصر",            "WfControl",           "CTL", "{PREFIX}-{SEQ}",       3, SequenceResetCycle.Never),
                ("NS-WFOP",      "Wf Operator Sequence",      "تسلسل المعامل",           "WfOperator",          "OP",  "{PREFIX}-{SEQ}",       3, SequenceResetCycle.Never),
                ("NS-WFPERF",    "Wf Performer Sequence",     "تسلسل المنفذ",            "WfPerformer",         "PRF", "{PREFIX}-{SEQ}",       5, SequenceResetCycle.Never),
                ("NS-WFREQ",     "Wf Request Sequence",       "تسلسل الطلب",             "WfRequest",           "REQ", "{PREFIX}-{YYYY}-{SEQ}", 6, SequenceResetCycle.Yearly),
                ("NS-WFREQD",    "Wf Request Detail Seq.",    "تسلسل بند الطلب",         "WfRequestDetail",     "REQD","{PREFIX}-{SEQ}",       6, SequenceResetCycle.Never),
                ("NS-WFTRN",     "Wf Transition Sequence",    "تسلسل الانتقال",          "WfTransition",        "TRN", "{PREFIX}-{SEQ}",       5, SequenceResetCycle.Never),
                ("NS-WFVAR",     "Wf Variable Sequence",      "تسلسل المتغير",           "WfVariable",          "VAR", "{PREFIX}-{SEQ}",       5, SequenceResetCycle.Never),
                ("NS-WFDT",      "Wf DataType Sequence",      "تسلسل نوع البيانات",      "WfDataType",          "DT",  "{PREFIX}-{SEQ}",       3, SequenceResetCycle.Never),

                // ─── System ──────────────────────────────────────────────────────
                ("NS-LOC",       "Logistics Location Sequence", "تسلسل موقع الخدمات اللوجستية", "LogisticsLocation", "LOC", "{PREFIX}-{SEQ}", 6, SequenceResetCycle.Never),
                ("NS-NS",        "Number Sequence Code",      "تسلسل تسلسلات الترقيم",   "SysNumberSequence",   "NS",  "{PREFIX}-{SEQ}",       4, SequenceResetCycle.Never),
            };

            var existingCodes = await db.SysNumberSequences
                .Select(s => s.Code)
                .ToListAsync(ct);
            var existingSet = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

            var toAdd = new List<SysNumberSequence>();
            foreach (var d in defs)
            {
                if (existingSet.Contains(d.Code)) continue;
                toAdd.Add(new SysNumberSequence
                {
                    Code = d.Code,
                    Name = d.Name,
                    NameAR = d.NameAR,
                    EntityName = d.EntityName,
                    Prefix = d.Prefix,
                    FormatPattern = d.FormatPattern,
                    PaddingLength = d.PaddingLength,
                    SmallestValue = 1,
                    LargestValue = 999999999,
                    NextValue = 1,
                    Step = 1,
                    ResetCycle = d.Reset,
                    IsActive = true,
                    CreatedBy = createdBy,
                    OwnerAccountId = createdBy,
                });
            }

            if (toAdd.Count > 0)
            {
                db.SysNumberSequences.AddRange(toAdd);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
