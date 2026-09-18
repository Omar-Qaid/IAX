namespace IAX.IXApi.Bootstrap.OpenApi;

/// <summary>Versioned admission proposal, not runtime authorization or an executable tool registry.</summary>
public sealed record McpPilotContract(
    int MetadataVersion, string ToolName, int ContractVersion, string Method, string Path,
    bool ExecutionEnabled, string CompanyScope, string[] AllowedQueryParameters,
    int? DefaultPageSize, int? MaximumPageSize, string[] AllowedDataFields,
    string RecordAuthorization, string[] RequiredEvidence)
{
    public static McpPilotContract? ForOperation(string method, string path) => (method, path) switch
    {
        ("GET", "/api/v1/organization-structure/units") => new(1, "organization_units_list", 1,
            method, path, false, "company-required", ["asOf"], null, null,
            ["id", "code", "name", "type"],
            "Organization.Structure.View; company-filtered API query",
            ["adapter-enforces-input-and-output-policy", "authenticated-company-and-record-isolation",
             "real-client-invocation", "data-owner-review"]),
        ("GET", "/api/v1/Customer/paged") => Create("finance_customers_search", path,
            ["recId", "accountNum", "custGroup", "currency", "blocked", "isActive"], true,
            "AccountsReceivable.Customers.View; company-filtered API query"),
        ("GET", "/api/v1/WfRequest/{id}") => Create("workflow_requests_get", path,
            ["recId", "code", "name", "processId", "requestDate", "isFinished", "isStopped", "progress"], false,
            "CanAccessRequestAsync; creator/employee/assignment or broad view; preserve API 404 concealment"),
        _ => null
    };

    private static McpPilotContract Create(string name, string path, string[] fields, bool paged, string authorization) =>
        new(1, name, 1, "GET", path, false, "company-required",
            paged ? ["PageNumber", "PageSize", "SearchTerm"] : [],
            paged ? 25 : null, paged ? 100 : null, fields, authorization,
            ["adapter-enforces-input-and-output-policy", "authenticated-company-and-record-isolation",
             "real-client-invocation", "data-owner-review"]);
}
