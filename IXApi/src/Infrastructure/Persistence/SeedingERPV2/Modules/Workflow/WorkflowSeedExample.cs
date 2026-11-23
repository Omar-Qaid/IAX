namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Workflow;

/// <summary>
/// Stable business references described by the Workflow manifest and checked against persisted
/// seeders. One row per reference table: the normal activity type, the applicant performer and the
/// employee search control are the three a process definition cannot be assembled without.
/// </summary>
public static class WorkflowSeedExample
{
    public static IReadOnlyList<(string Entity, string Field, string Value)> References { get; } =
    [
        ("WfCategory","Code","CAT6"),
        ("WfPriority","Code","MED"),
        ("WfProcessType","Code","APP"),
        ("WfActivityType","Code","NORMAL"),
        ("WfPerformerType","Code","ORGANIZATIONAL"),
        ("WfPerformer","Code","PERF12"),
        ("WfOperator","Code","EQ"),
        ("WfDataType","Code","STR"),
        ("WfControl","Code","search")
    ];
}
