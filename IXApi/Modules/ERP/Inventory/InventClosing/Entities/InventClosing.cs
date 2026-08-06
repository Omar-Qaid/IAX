using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.ERP.Shared.Features;
using IAX.IXApi.Modules.ERP.Common;

namespace IAX.IXApi.Modules.ERP.Entities
{
    [Table("InventClosing")]
    public class InventClosing : Entity<long>
    {
        //----------------------------------------- Core Identity & Tracking
        // Basic Properties
        public DateTime TransDate { get; set; }

        [Required]
        [StringLength(FieldLengths.Voucher)]
        public string Voucher { get; set; } = string.Empty;

        [Required]
        [StringLength(FieldLengths.PeriodCode)]
        public string PeriodCode { get; set; } = string.Empty;

        public int RunNum { get; set; }
        public int NextRunNum { get; set; }

        public string? Notes { get; set; } // nvarchar(MAX) supporting Nullable = YES

        // ==========================================================
        // Closing & Recalculation Parameters
        // ==========================================================
        // Enum Properties
        public InventAdjustmentSpec AdjustmentSpec { get; set; }
        public InventAdjustmentType AdjustmentType { get; set; }
        public InventCostStatus InventCostStatus { get; set; }
        public NoYes Cancellation { get; set; }
        public NoYes CancelClosing { get; set; } // Mapping context for CancelRecalculation
        public NoYes RunRecalculation { get; set; }

        // ==========================================================
        // Settlement Execution & Performance Tuning
        // ==========================================================
        // Basic Properties
        public DateTime Executed { get; set; }
        public int BomLevel { get; set; }
        public int NumOfIteration { get; set; }
        public int MaxIterations { get; set; }
        public decimal MinTransferValue { get; set; }
        public int HelpersCreated { get; set; }
        public long CancelClosingRefRecId { get; set; }

        // Enum Properties
        public NoYes Start_ { get; set; }
        public NoYes End_ { get; set; }
        public NoYes Active { get; set; }

        // ==========================================================
        // Ledger & Sub-Ledger Posting Controls
        // ==========================================================
        // Basic Properties
        public long LedgerPostingBatch { get; set; }

        // Enum Properties
        public NoYes Ledger { get; set; }
        public NoYes LedgerCorrection { get; set; }
        public NoYes ItmAdjustment { get; set; }
        public NoYes ProdJournal { get; set; }

        // ==========================================================
        // Execution Diagnostics & Run Controls
        // ==========================================================
        // Enum Properties
        public NoYes StopOnError { get; set; }
        public NoYes StopRunning { get; set; }
        public NoYes ShouldSummarizeInfolog { get; set; }
    }
}