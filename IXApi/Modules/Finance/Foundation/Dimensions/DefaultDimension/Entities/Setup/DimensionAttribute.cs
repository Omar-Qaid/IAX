using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Finance.Shared.Features;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Entities
{
    [Table("DimensionAttribute")]
    public class DimensionAttribute : Entity<long>
    {
        //----------------------------------------- Core Information
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.Name)]
        public string Name { get; set; } = string.Empty;
        public Guid HashKey { get; set; }

        // Enum Properties
        public DimensionAttributeType Type { get; set; }

        // ==========================================================
        // Backing Entity Configuration
        // ==========================================================
        // Basic Properties
        public int BackingEntityTableId { get; set; }
        [Required]
        [StringLength(FieldLengths.BackingEntityTableName)]
        public string BackingEntityTableName { get; set; } = string.Empty;
        public int BackingEntityKeyFieldId { get; set; }
        [Required]
        [StringLength(FieldLengths.BackingEntityKeyFieldName)]
        public string BackingEntityKeyFieldName { get; set; } = string.Empty;
        public int BackingEntityValueFieldId { get; set; }
        [Required]
        [StringLength(FieldLengths.BackingEntityValueFieldName)]
        public string BackingEntityValueFieldName { get; set; } = string.Empty;

        // Enum Properties
        public DimensionBackingEntityType BackingEntityType { get; set; }
        public DimensionBackingEntityPerCompanyType BackingEntityPerCompanyType { get; set; }

        // ==========================================================
        // Views & Columns Mapping
        // ==========================================================
        // Basic Properties
        [Required]
        [StringLength(FieldLengths.ViewName)]
        public string ViewName { get; set; } = string.Empty;
        [Required]
        [StringLength(FieldLengths.DimensionKeyColumnName)]
        public string DimensionKeyColumnName { get; set; } = string.Empty;
        [Required]
        [StringLength(FieldLengths.DimensionValueColumnName)]
        public string DimensionValueColumnName { get; set; } = string.Empty;
        [Required]
        [StringLength(FieldLengths.ReportColumnName)]
        public string ReportColumnName { get; set; } = string.Empty;

        // ==========================================================
        // Translation Table Details
        // ==========================================================
        // Basic Properties
        public int TranslationTableId { get; set; }
        [Required]
        [StringLength(FieldLengths.TranslationTableName)]
        public string TranslationTableName { get; set; } = string.Empty;
        public int TranslationKeyFieldId { get; set; }
        [Required]
        [StringLength(FieldLengths.TranslationKeyFieldName)]
        public string TranslationKeyFieldName { get; set; } = string.Empty;
        public int TranslationNameFieldId { get; set; }
        [Required]
        [StringLength(FieldLengths.TranslationNameFieldName)]
        public string TranslationNameFieldName { get; set; } = string.Empty;
        public int TranslationLanguageIdFieldId { get; set; }
        [Required]
        [StringLength(FieldLengths.TranslationLanguageIdFieldName)]
        public string TranslationLanguageIdFieldName { get; set; } = string.Empty;

        // ==========================================================
        // Translation View Details
        // ==========================================================
        // Basic Properties
        public int TranslationViewId { get; set; }
        [Required]
        [StringLength(FieldLengths.TranslationViewName)]
        public string TranslationViewName { get; set; } = string.Empty;
        public int TranslationViewKeyFieldId { get; set; }
        [Required]
        [StringLength(FieldLengths.TranslationViewKeyFieldName)]
        public string TranslationViewKeyFieldName { get; set; } = string.Empty;
        public int TranslationViewValueFieldId { get; set; }
        [Required]
        [StringLength(FieldLengths.TranslationViewValueFieldName)]
        public string TranslationViewValueFieldName { get; set; } = string.Empty;
        public int TranslationViewNameFieldId { get; set; }
        [Required]
        [StringLength(FieldLengths.TranslationViewNameFieldName)]
        public string TranslationViewNameFieldName { get; set; } = string.Empty;
        public int TranslationViewLanguageIdFieldId { get; set; }
        [Required]
        [StringLength(FieldLengths.TranslationViewLanguageIdFieldName)]
        public string TranslationViewLanguageIdFieldName { get; set; } = string.Empty;
        public int TranslationViewSystemLanguageIdFieldId { get; set; }
        [Required]
        [StringLength(FieldLengths.TranslationViewSystemLanguageIdFieldName)]
        public string TranslationViewSystemLanguageIdFieldName { get; set; } = string.Empty;
        public int TranslationViewTranslatedNameFieldId { get; set; }
        [Required]
        [StringLength(FieldLengths.TranslationViewTranslatedNameFieldName)]
        public string TranslationViewTranslatedNameFieldName { get; set; } = string.Empty;

        // ==========================================================
        // Behaviors & System Policies
        // ==========================================================
        // Enum Properties
        public NoYes CopyValuesOnCreate { get; set; }
        public NoYes GiveDerivedDimensionsPrecedence { get; set; }
        public NoYes KeyAttribute { get; set; }
        public NoYes NameAttribute { get; set; }
        public NoYes ValueAttribute { get; set; }
        public NoYes UseTranslationNameMethod { get; set; }
    }
}
