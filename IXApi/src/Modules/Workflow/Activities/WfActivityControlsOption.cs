using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    /// <summary>
    /// A single selectable option for a closed-list Activity control (Drop Down List
    /// "fill manually", CheckBoxList, RadioButtonList). Mirrors the
    /// <see cref="WfActivityControlsValidation"/> pattern (one row per option).
    /// </summary>
    public class WfActivityControlsOption : Entity<long>
    {
        public long ActivityControlId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ActivityControlId))]
        public virtual WfActivityControl ActivityControl { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string Value { get; set; } = null!;   // value submitted when selected
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string Name { get; set; } = null!;   // English display label
        public int SortOrder { get; set; }
    }
}


