using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestDetail : Entity<long>
    {
        public long? ProcessId { get; set; }
        public long RequestId { get; set; }
        public byte? ControlId { get; set; }
        public long? ControlDataId { get; set; }
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string ControlLabel { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string ControlLabelAR { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string ControlValue { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string ControlValueAR { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string ControlValueEN { get; set; } = null!;
        public bool UsedAsCriteria { get; set; }
        public byte SortOrder { get; set; }
        public decimal Score { get; set; }
    }
}


