using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow;

/// <summary>
/// Workflow-scoped base for name-bearing master tables. Keeping NameAlias here
/// prevents the column from leaking into lookup tables or other modules.
/// </summary>
public abstract class WfMasterEntity<T> : MasterEntity<T>
{
    [MaxLength(255)]
    public string? NameAlias { get; set; }
}
