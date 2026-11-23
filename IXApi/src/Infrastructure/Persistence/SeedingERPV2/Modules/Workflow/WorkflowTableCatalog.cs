namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Workflow;

/// <summary>The tables this module owns, as covered by its registered seeders.</summary>
public static class WorkflowTableCatalog
{
    /// <summary>
    /// The reference catalog a process definition is assembled from. The definitions themselves — the
    /// processes, their activities, forms and transitions — are configured in the application, not
    /// seeded here, and the integrity validator checks the ones already stored against this catalog.
    /// </summary>
    public static IReadOnlySet<string> MasterTypes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "WfActivityType",
        "WfCategory",
        "WfControl",
        "WfDataType",
        "WfOperator",
        "WfPerformer",
        "WfPerformerType",
        "WfPriority",
        "WfProcessType",
    };

    /// <summary>
    /// A request and everything it records as it moves are written by the workflow services as a
    /// person raises and approves it. Nothing here seeds them.
    /// </summary>
    public static IReadOnlySet<string> TransactionTypes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "WfRequest",
        "WfRequestDetail",
    };
}
