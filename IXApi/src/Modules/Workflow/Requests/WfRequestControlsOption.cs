using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    /// <summary>
    /// A single selectable option for a closed-list Request control (Drop Down List
    /// "fill manually", CheckBoxList, RadioButtonList). Mirrors the
    /// <see cref="WfRequestControlsValidation"/> pattern (one row per option).
    /// </summary>
    public class WfRequestControlsOption : Entity<long>
    {
        public long RequestControlId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(RequestControlId))]
        public virtual WfRequestControl RequestControl { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string Value { get; set; } = null!;   // value submitted when selected
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string NameEn { get; set; } = null!;   // English display label
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string NameAr { get; set; } = null!;   // Arabic display label
        public int SortOrder { get; set; }
    }
}


