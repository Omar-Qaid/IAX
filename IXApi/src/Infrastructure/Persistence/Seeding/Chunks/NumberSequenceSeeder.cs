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
    /// NumberSequence must match the value passed to ISysNumberSequenceService.NextAsync
    /// (or AutoCodeExtensions.EnsureCodeAsync) from the corresponding service.
    /// </summary>
    public class NumberSequenceSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
        {
            var sysUser = await users.FindByNameAsync("sys");
            var createdBy = sysUser?.Id ?? "sys";

            // (NumberSequence, Txt, Format, AnnotatedFormat, Cyclic)
            var defs = new (string NumberSequence, string Txt, string Format, string AnnotatedFormat, int Cyclic)[]
            {
                // ─── ERP / Accounts ──────────────────────────────────────────────
                ("Customer",            "Customer Sequence",         "CUS-######", "{PREFIX}-{SEQ}",       0),
                ("CustomerGroup",       "Customer Group Sequence",   "CGRP-######", "{PREFIX}-{SEQ}",       0),
                ("Vendor",              "Vendor Sequence",           "VEN-######", "{PREFIX}-{SEQ}",       0),
                ("VendorGroup",         "Vendor Group Sequence",     "VGRP-######", "{PREFIX}-{SEQ}",       0),
                ("AccPartner",          "Partner Sequence",          "PRT-######", "{PREFIX}-{SEQ}",       0),
                ("AccPartnerGroup",     "Partner Group Sequence",    "PGRP-######", "{PREFIX}-{SEQ}",       0),
                ("Voucher",             "Voucher Sequence",          "VOU-######", "{PREFIX}-{SEQ}",       0),

                // ─── ERP / AR ────────────────────────────────────────────────────
                ("SalesTable",          "Sales Order Sequence",      "SO-######",  "{PREFIX}-{YYYY}-{SEQ}", 1),
                ("SalesLine",           "Sale Line Sequence",        "SOL-######", "{PREFIX}-{SEQ}",       0),
                ("PackingSlip",         "Packing Slip Sequence",     "PS-######",  "{PREFIX}-{YYYY}-{SEQ}", 1),
                ("WMSPickingRoute",     "Picking Route Sequence",    "PICK-######","{PREFIX}-{YYYY}-{SEQ}", 1),
                ("CustConfirmJour",     "SO Confirm Sequence",       "CONF-######","{PREFIX}-{YYYY}-{SEQ}", 1),
                ("CustPackingSlipJour", "PS Journal ID Sequence",    "PSJ-######", "{PREFIX}-{YYYY}-{SEQ}", 1),
                ("CustInvoiceJour",     "Invoice Sequence",          "INV-######", "{PREFIX}-{YYYY}-{SEQ}", 1),
                ("CustInvoiceTrans",    "Invoice Trans Sequence",    "INVT-######","{PREFIX}-{SEQ}",       0),
                ("CustTrans",           "Cust Transaction Sequence", "CTRX-######","{PREFIX}-{YYYY}-{SEQ}", 0),
                ("CustSettlement",      "Cust Settlement Sequence",  "CSET-######","{PREFIX}-{SEQ}",       0),
                ("InventTransOriginId", "Lot Origin Sequence",       "ORG-######", "{PREFIX}-{SEQ}",       0),
                ("LedgerJournalTable",  "Ledger Journal Sequence",   "LJT-######", "{PREFIX}-{YYYY}-{SEQ}", 1),
                ("InventJournalTable",  "Inventory Journal Sequence","IJRN-######", "{PREFIX}-{YYYY}-{SEQ}", 1),
                ("PurchTable",          "Purchase Order Sequence",   "PO-######",  "{PREFIX}-{YYYY}-{SEQ}", 1),
                ("VendTrans",           "Vend Transaction Sequence", "VTRX-######","{PREFIX}-{YYYY}-{SEQ}", 0),
                ("VendInvoiceRegister", "Vend Invoice Register Seq", "VIR-######", "{PREFIX}-{YYYY}-{SEQ}", 1),
                ("ExchAdjustment",      "Exch Adjustment Sequence",  "FXA-######", "{PREFIX}-{YYYY}-{SEQ}", 1),
                ("VendSettlement",      "Vend Settlement Sequence",  "VSET-######","{PREFIX}-{SEQ}",       0),

                // ─── ERP / Inventory ─────────────────────────────────────────────
    
                ("InventTransId",       "Invent Lot Sequence",       "LOT-######", "{PREFIX}-{SEQ}",       0),
                ("InventGroup",         "Group Sequence",            "IGRP-######","{PREFIX}-{SEQ}",       0),
                ("UnitOfMeasure",       "UOM Sequence",              "UOM-######", "{PREFIX}-{SEQ}",       0),
                ("InventUOM",           "UOM Sequence",              "IUOM-######","{PREFIX}-{SEQ}",       0),
                ("InventTrans",         "Inv Transaction Sequence",  "ITRX-######","{PREFIX}-{YYYY}{MM}-{SEQ}", 1),

                // ─── Organization ────────────────────────────────────────────────
                ("OrgEmployee",         "Employee Sequence",         "EMP-######", "{PREFIX}-{SEQ}",       0),
                ("OrgDepartment",       "Department Sequence",       "DPT-######", "{PREFIX}-{SEQ}",       0),
                ("OrgOccupation",       "Occupation Sequence",       "OCC-######", "{PREFIX}-{SEQ}",       0),
                ("OrgNationality",      "Nationality Sequence",      "NAT-######", "{PREFIX}-{SEQ}",       0),
                ("OrgGender",           "Gender Sequence",           "GEN-######", "{PREFIX}-{SEQ}",       0),
                ("OrgCompany",          "Company Sequence",          "COMP-######","{PREFIX}-{SEQ}",       0),
                ("OrgAnnouncement",     "Announcement Sequence",     "ANN-######", "{PREFIX}-{YYYY}-{SEQ}", 1),
                ("OrgAttachment",       "Attachment Sequence",       "ATT-######", "{PREFIX}-{SEQ}",       0),
                ("OrgJob",              "Job Sequence",              "JOB-######", "{PREFIX}-{SEQ}",       0),

                // ─── Identity / Users ────────────────────────────────────────────
                ("OrgEmployeeGroup",    "User Group Sequence",       "UGRP-######","{PREFIX}-{SEQ}",       0),
                ("OrgEmployeeCategory", "User Category Sequence",    "UCAT-######","{PREFIX}-{SEQ}",       0),

                // ─── Workflow ────────────────────────────────────────────────────
                ("WfCategory",          "Wf Category Sequence",      "WCT-######", "{PREFIX}-{SEQ}",       0),
                ("WfPriority",          "Wf Priority Sequence",      "WPR-######", "{PREFIX}-{SEQ}",       0),
                ("WfProcessType",       "Wf Process Type Sequence",  "WPT-######", "{PREFIX}-{SEQ}",       0),
                ("WfProcess",           "Wf Process Sequence",       "PROC-######","{PREFIX}-{SEQ}",       0),
                ("WfStep",              "Wf Step Sequence",          "STEP-######","{PREFIX}-{SEQ}",       0),
                ("WfActivity",          "Wf Activity Sequence",      "ACT-######", "{PREFIX}-{SEQ}",       0),
                ("WfActivityType",      "Wf Activity Type Sequence", "ATYP-######","{PREFIX}-{SEQ}",       0),
                ("WfActivityControl",   "Wf Activity Control Seq.",  "ACTL-######","{PREFIX}-{SEQ}",       0),
                ("WfRequestControl",    "Wf Request Control Seq.",   "RCTL-######","{PREFIX}-{SEQ}",       0),
                ("WfControl",           "Wf Control Sequence",       "CTL-######", "{PREFIX}-{SEQ}",       0),
                ("WfOperator",          "Wf Operator Sequence",      "OP-######",  "{PREFIX}-{SEQ}",       0),
                ("WfPerformer",         "Wf Performer Sequence",     "PRF-######", "{PREFIX}-{SEQ}",       0),
                ("WfPerformerType",     "Wf Performer Type Sequence","PRT-######", "{PREFIX}-{SEQ}",       0),
                ("WfRequest",           "Wf Request Sequence",       "REQ-######", "{PREFIX}-{YYYY}-{SEQ}", 1),
                ("WfRequestDetail",     "Wf Request Detail Seq.",    "REQD-######","{PREFIX}-{SEQ}",       0),
                ("WfTransition",        "Wf Transition Sequence",    "TRN-######", "{PREFIX}-{SEQ}",       0),
                ("WfVariable",          "Wf Variable Sequence",      "VAR-######", "{PREFIX}-{SEQ}",       0),
                ("WfDataType",          "Wf DataType Sequence",      "DT-######",  "{PREFIX}-{SEQ}",       0),

                // ─── System ──────────────────────────────────────────────────────
                ("LogisticsLocation",   "Logistics Location Sequence", "LOC-######", "{PREFIX}-{SEQ}", 0),
                ("SysNumberSequence",   "Number Sequence Code",      "NS-######",  "{PREFIX}-{SEQ}",       0),
            };

            var existingCodes = await db.SysNumberSequences
                .Where(s => s.NumberSequence != null)
                .Select(s => s.NumberSequence!)
                .ToListAsync(ct);
            var existingSet = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

            var toAdd = new List<SysNumberSequence>();
            foreach (var d in defs)
            {
                if (existingSet.Contains(d.NumberSequence)) continue;
                toAdd.Add(new SysNumberSequence
                {
                    NumberSequence = d.NumberSequence,
                    Txt = d.Txt,
                    Format = d.Format,
                    AnnotatedFormat = d.AnnotatedFormat,
                    Lowest = 1,
                    Highest = 999999999,
                    NextRec = 1,
                    Cyclic = d.Cyclic,
                    CreatedBy = createdBy,
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
