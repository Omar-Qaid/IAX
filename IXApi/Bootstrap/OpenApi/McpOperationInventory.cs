using System.Security.Cryptography;
using System.Text;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace IAX.IXApi.Bootstrap.OpenApi;

public static class McpOperationInventory
{
    public static McpOperationDescription Describe(ApiDescription description)
    {
        var action = description.ActionDescriptor as ControllerActionDescriptor
            ?? throw new InvalidOperationException("Expected an MVC controller action in the discovery document.");
        var method = description.HttpMethod?.ToUpperInvariant()
            ?? throw new InvalidOperationException($"Missing HTTP method for {action.DisplayName}.");
        var path = "/" + description.RelativePath?.Split('?')[0].TrimStart('/');
        var assemblyName = action.ControllerTypeInfo.Assembly.GetName().Name!;
        var module = assemblyName.StartsWith("IAX.IXApi.Modules.", StringComparison.Ordinal)
            ? assemblyName["IAX.IXApi.Modules.".Length..] : "Host";
        var anonymous = action.EndpointMetadata.OfType<IAllowAnonymous>().Any();
        var permissions = action.FilterDescriptors.Select(filter => filter.Filter)
            .OfType<DomainPermissionAttribute>().Select(permission => permission.GetRequiredPermission(method))
            .Distinct().Order(StringComparer.Ordinal).ToArray();
        var policies = action.EndpointMetadata.OfType<IAuthorizeData>()
            .Select(data => data.Policy).Where(policy => !string.IsNullOrEmpty(policy))
            .Cast<string>().Distinct().Order(StringComparer.Ordinal).ToArray();
        var roles = action.EndpointMetadata.OfType<IAuthorizeData>()
            .Select(data => data.Roles).Where(role => !string.IsNullOrEmpty(role))
            .Cast<string>().Distinct().Order(StringComparer.Ordinal).ToArray();
        var pilot = McpPilotContract.ForOperation(method, path);
        var pilotName = pilot?.ToolName;
        var status = pilotName != null && !anonymous ? "pilot-candidate" : "excluded";
        var reason = status == "pilot-candidate"
            ? "Proposed metadata contract; requires adapter enforcement, data-owner review and authenticated staging validation."
            : anonymous ? "Anonymous/authentication operations are outside the pilot."
            : method != "GET" ? "Not admitted: writes and behavioral read-only POSTs need individual review."
            : "Not selected for the initial read-only pilot; authorization, scope and data policy require review.";
        // Stable discovery IDs, not published tool names: aliases have distinct IDs.
        var routeHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(method + " " + path)))
            .ToLowerInvariant();
        var operationId = $"{module}_{action.ControllerName}_{action.ActionName}_{method}_{routeHash[..16]}";
        var sameActionGroup = $"{action.ControllerTypeInfo.FullName}:{action.MethodInfo}:{method}";
        return new McpOperationDescription(operationId, module, action.ControllerName, action.ActionName,
            method, path, sameActionGroup, pilotName, status, reason, anonymous, permissions, policies, roles,
            pilotName == "workflow_requests_get" ? "CanAccessRequestAsync; creator/employee/assignment or broad view"
                : "Requires review; permission metadata alone does not describe record authorization",
            pilotName != null ? "company-required" : "unclassified",
            description.ParameterDescriptions.Select(parameter => new McpParameterDescription(
                parameter.Name, parameter.Source?.Id ?? "unknown", parameter.IsRequired,
                parameter.Type?.FullName ?? "unknown")).ToArray(),
            description.SupportedResponseTypes.Select(response => new McpResponseDescription(
                response.StatusCode, response.Type?.FullName,
                response.ApiResponseFormats.Select(format => format.MediaType).ToArray())).ToArray(), pilot);
    }
}

public sealed record McpOperationDescription(
    string OperationId, string Module, string Controller, string Action, string Method, string Path,
    string SameActionGroup, string? ProposedToolName, string Status, string Reason, bool AllowsAnonymous,
    string[] DomainPermissions, string[] AuthorizationPolicies, string[] RoleRequirements,
    string RecordAuthorization, string CompanyScope, McpParameterDescription[] Parameters,
    McpResponseDescription[] Responses, McpPilotContract? PilotContract);
public sealed record McpParameterDescription(string Name, string Source, bool Required, string Type);
public sealed record McpResponseDescription(int Status, string? Type, string[] MediaTypes);
