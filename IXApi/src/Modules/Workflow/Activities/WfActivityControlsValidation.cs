using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityControlsValidation : LookupEntity<long>
    {
        public long ActivityControlId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ActivityControlId))]
        public virtual WfActivityControl ActivityControl { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string ValidationType { get; set; } = null!; // Required, MinLength, MaxLength, CustomExpression, etc.
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string? ValidationExpression { get; set; } // Formula expression e.g. (Amount > 1000)
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string? Operator { get; set; } // Comparison operator for CustomExpression
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string? Value { get; set; } // Comparison value 1
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string? MaskInput { get; set; } //  MaskInput 
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string ErrorMessageAr { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string ErrorMessageEn { get; set; } = null!;
        [System.ComponentModel.DataAnnotations.StringLength(50)]
        public string Severity { get; set; } = null!; // Error, Warning, Information
        public int SortOrder { get; set; }
    }
}


