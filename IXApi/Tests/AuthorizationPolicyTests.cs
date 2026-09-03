using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IAX.IXApi.Bootstrap.Extensions;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;
using IAX.IXApi.Modules.Administration.BackgroundJobs;
using IAX.IXApi.Modules.Administration.AuditLogs;
using IAX.IXApi.Modules.Administration.DataManagement;
using IAX.IXApi.Modules.Administration.Settings;
using IAX.IXApi.Modules.Communication.Chat;
using IAX.IXApi.Modules.Communication.Chat.Services;
using IAX.IXApi.Modules.Communication.Notifications;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.DataExchange;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Variables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class AuthorizationPolicyTests
{
    [Fact]
    public void Seeded_permission_resources_are_well_formed_and_cover_known_contracts()
    {
        var field = typeof(IdentitySeeder).GetField(
            "PermissionDefs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var definitions = Assert.IsType<(string Module, string Resource)[]>(field?.GetValue(null));

        Assert.All(definitions, definition =>
        {
            Assert.Equal(definition.Module.Trim(), definition.Module);
            Assert.Equal(definition.Resource.Trim(), definition.Resource);
            Assert.False(string.IsNullOrWhiteSpace(definition.Module));
            Assert.False(string.IsNullOrWhiteSpace(definition.Resource));
        });
        Assert.Equal(definitions.Length, definitions.Distinct().Count());

        var catalog = definitions.ToHashSet();
        Assert.Contains(("Organization", "LegalEntities"), catalog);
        Assert.Contains(("Organization", "Showrooms"), catalog);
        Assert.Contains(("System", "NotificationTemplate"), catalog);
        Assert.Contains(("System", "Settings"), catalog);
        Assert.Contains(("System", "DataManagement"), catalog);
        Assert.Contains(("System", "BackgroundJobs"), catalog);
        Assert.Contains(("Workflow", "DataTypes"), catalog);
        Assert.Contains(("GeneralLedger", "FiscalCalendars"), catalog);
        Assert.Contains(("AccountsReceivable", "PostingProfiles"), catalog);
        Assert.Contains(("Inventory", "Items"), catalog);
        Assert.Contains(("Inventory", "ItemGroups"), catalog);

        var additionalField = typeof(IdentitySeeder).GetField(
            "AdditionalPermissions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var additional = Assert.IsType<(string Module, string Resource, string Action)[]>(
            additionalField?.GetValue(null));
        Assert.Contains(("AccountsReceivable", "SalesOrders", "Confirm"), additional);
        Assert.Contains(("AccountsReceivable", "SalesOrders", "Post"), additional);
        Assert.Contains(("System", "DataManagement", "Import"), additional);
        Assert.Contains(("System", "DataManagement", "Export"), additional);
    }

    [Fact]
    public void Administration_permissions_preserve_self_service_and_protect_privileged_operations()
    {
        AssertPermission(typeof(SysSettingsController), nameof(SysSettingsController.GetGlobalSettings), "System", "Settings", "View");
        AssertPermission(typeof(SysSettingsController), nameof(SysSettingsController.UpdateGlobalSettings), "System", "Settings", "Edit");
        AssertNoDomainPermission(typeof(SysSettingsController), nameof(SysSettingsController.GetUserSettings));
        AssertNoDomainPermission(typeof(SysSettingsController), nameof(SysSettingsController.UpdateUserSettings));

        AssertPermission(typeof(SysDataManagementController), nameof(SysDataManagementController.Import), "System", "DataManagement", "Import");
        AssertPermission(typeof(SysDataManagementController), nameof(SysDataManagementController.Export), "System", "DataManagement", "Export");
        AssertPermission(typeof(SysBackgroundJobController), nameof(SysBackgroundJobController.GetJobs), "System", "BackgroundJobs", "View");
        AssertPermission(typeof(SysBackgroundJobController), nameof(SysBackgroundJobController.Create), "System", "BackgroundJobs", "Create");
        AssertPermission(typeof(SysBackgroundJobController), nameof(SysBackgroundJobController.Trigger), "System", "BackgroundJobs", "Run");
        AssertPermission(typeof(SysBackgroundJobController), nameof(SysBackgroundJobController.Cancel), "System", "BackgroundJobs", "Cancel");
    }

    [Fact]
    public void Workflow_configuration_controllers_require_the_expected_permissions()
    {
        AssertPermission(typeof(WfDataTypeController), null, "Workflow", "DataTypes", null);
        AssertPermission(typeof(WfRequestMappingVariableController), null, "Workflow", "RequestControls", null);
        AssertPermission(typeof(WfRequestControlsValidationController), null, "Workflow", "RequestControls", null);
        AssertPermission(typeof(WfRequestControlsOptionController), null, "Workflow", "RequestControls", null);
        AssertPermission(typeof(WfActivityMappingVariableController), null, "Workflow", "ActivityControls", null);
        AssertPermission(typeof(WfActivityControlsValidationController), null, "Workflow", "ActivityControls", null);
        AssertPermission(typeof(WfActivityControlsOptionController), null, "Workflow", "ActivityControls", null);
        AssertPermission(typeof(WfDataManagementController), nameof(WfDataManagementController.ImportProcesses), "Workflow", "DataManagement", "Import");
        AssertPermission(typeof(WfDataManagementController), nameof(WfDataManagementController.ExportProcesses), "Workflow", "DataManagement", "Export");
    }

    [Fact]
    public void Finance_business_controllers_require_domain_permissions()
    {
        var financeAssembly = typeof(IAX.IXApi.Modules.Finance.Shared.Features.CurrencyController).Assembly;
        var intentionallySharedReferenceControllers = new HashSet<Type>
        {
            typeof(IAX.IXApi.Modules.Finance.Common.Controllers.EnumsController),
            typeof(IAX.IXApi.Modules.Finance.Common.Controllers.ErpEnumsController),
        };
        var unprotected = financeAssembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(Microsoft.AspNetCore.Mvc.ControllerBase).IsAssignableFrom(type))
            .Where(type => !intentionallySharedReferenceControllers.Contains(type))
            .Where(type => type.CustomAttributes.All(attribute => attribute.AttributeType != typeof(DomainPermissionAttribute)))
            .Select(type => type.FullName)
            .ToList();

        Assert.Empty(unprotected);
        AssertPermission(typeof(IAX.IXApi.Modules.Finance.AccountsReceivable.CustomerController), null, "AccountsReceivable", "Customers", null);
        AssertPermission(typeof(IAX.IXApi.Modules.Finance.AccountsPayable.VendorController), null, "AccountsPayable", "Vendors", null);
        AssertPermission(typeof(IAX.IXApi.Modules.Finance.Shared.Features.CurrencyController), null, "GeneralLedger", "Currencies", null);
        AssertPermission(typeof(IAX.IXApi.Modules.Finance.Shared.Features.TaxTableController), null, "Tax", "TaxCodes", null);
        AssertPermission(typeof(IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar.FiscalCalendarController), null, "GeneralLedger", "FiscalCalendars", null);
    }

    [Fact]
    public void Workflow_request_controller_exposes_only_access_checked_transaction_routes()
    {
        foreach (var methodName in new[]
        {
            nameof(WfRequestController.Create),
            nameof(WfRequestController.GetPaged),
            nameof(WfRequestController.CreateRange),
            nameof(WfRequestController.UpdateRange),
            nameof(WfRequestController.DeleteRange),
        })
        {
            var method = typeof(WfRequestController).GetMethods()
                .Single(candidate => candidate.Name == methodName && candidate.DeclaringType == typeof(WfRequestController));
            Assert.NotNull(method.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.NonActionAttribute), false).SingleOrDefault());
        }

        Assert.Equal(typeof(WfRequestController), typeof(WfRequestController).GetMethod(nameof(WfRequestController.GetById))?.DeclaringType);
        Assert.Equal(typeof(WfRequestController), typeof(WfRequestController).GetMethod(nameof(WfRequestController.Update))?.DeclaringType);
        Assert.Equal(typeof(WfRequestController), typeof(WfRequestController).GetMethod(nameof(WfRequestController.Delete))?.DeclaringType);
        Assert.NotNull(typeof(IWfRequestService).GetMethod(nameof(IWfRequestService.CanAccessRequestAsync)));
    }

    [Fact]
    public void Communication_endpoints_preserve_self_service_and_protect_privileged_send()
    {
        AssertPermission(typeof(SysNotificationController), nameof(SysNotificationController.Send), "System", "Notifications", "Create");
        AssertNoDomainPermission(typeof(SysNotificationController), nameof(SysNotificationController.GetMyNotifications));
        AssertNoDomainPermission(typeof(SysNotificationPreferenceController), nameof(SysNotificationPreferenceController.GetMyPreferences));
        AssertNoDomainPermission(typeof(SysNotificationPreferenceController), nameof(SysNotificationPreferenceController.SavePreferences));

        Assert.NotEmpty(typeof(SysNotificationPreferenceController).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true));
        Assert.NotEmpty(typeof(SysChatController).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true));
        Assert.NotEmpty(typeof(SysNotificationPreferenceController).GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.RouteAttribute), true));
        Assert.NotEmpty(typeof(SysChatController).GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.RouteAttribute), true));
    }

    [Fact]
    public void Chat_room_access_requires_exact_participation()
    {
        var service = new SysChatService(null!, null!);

        Assert.True(service.CanAccessRoom("user-1", "general"));
        Assert.True(service.CanAccessRoom("user-1", "dm:user-1:user-2"));
        Assert.True(service.CanAccessRoom("user-2", "dm:user-1:user-2"));
        Assert.False(service.CanAccessRoom("user", "dm:user-1:user-2"));
        Assert.False(service.CanAccessRoom("user-3", "dm:user-1:user-2"));
        Assert.False(service.CanAccessRoom("user-1", "private-room"));
        Assert.False(service.CanAccessRoom("", "general"));
    }

    [Fact]
    public void Generic_bulk_routes_are_not_public_and_audit_log_is_read_only()
    {
        var baseController = typeof(IAX.IXApi.Api.Controllers.BaseController<,>);
        foreach (var methodName in new[] { "CreateRange", "UpdateRange", "DeleteRange" })
        {
            var method = baseController.GetMethods().Single(candidate => candidate.Name == methodName);
            Assert.NotNull(method.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.NonActionAttribute), false).SingleOrDefault());
            Assert.DoesNotContain(method.CustomAttributes, attribute =>
                attribute.AttributeType == typeof(Microsoft.AspNetCore.Mvc.HttpPostAttribute)
                || attribute.AttributeType == typeof(Microsoft.AspNetCore.Mvc.HttpPutAttribute)
                || attribute.AttributeType == typeof(Microsoft.AspNetCore.Mvc.HttpDeleteAttribute));
        }

        foreach (var methodName in new[]
        {
            nameof(SysAuditLogController.Create),
            nameof(SysAuditLogController.Update),
            nameof(SysAuditLogController.Delete),
        })
        {
            var method = typeof(SysAuditLogController).GetMethods()
                .Single(candidate => candidate.Name == methodName && candidate.DeclaringType == typeof(SysAuditLogController));
            Assert.NotNull(method.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.NonActionAttribute), false).SingleOrDefault());
        }
    }

    [Fact]
    public void Fallback_policy_requires_authenticated_users()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCustomAuthorization();
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        Assert.NotNull(options.FallbackPolicy);
        Assert.Contains(options.FallbackPolicy!.Requirements,
            requirement => requirement is DenyAnonymousAuthorizationRequirement);
    }

    [Fact]
    public async Task Permission_handler_uses_current_database_permissions()
    {
        var service = new StubPermissionService(["Finance.Currency.View"]);
        var handler = new PermissionAuthorizationHandler(service);
        var requirement = new PermissionRequirement("Finance.Currency.View");
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(JwtRegisteredClaimNames.Sub, "user-1")], "Test"));
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.Equal("user-1", service.LastRequestedUserId);
    }

    [Fact]
    public async Task Permission_handler_denies_permission_not_returned_by_database()
    {
        var handler = new PermissionAuthorizationHandler(new StubPermissionService([]));
        var requirement = new PermissionRequirement("Finance.Currency.Edit");
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(JwtRegisteredClaimNames.Sub, "user-1")], "Test"));
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private sealed class StubPermissionService(IReadOnlyCollection<string> permissions)
        : IAppPermissionService
    {
        public string? LastRequestedUserId { get; private set; }

        public Task<List<string>> GetPermissionKeysByUserAsync(
            string userId,
            CancellationToken ct = default)
        {
            LastRequestedUserId = userId;
            return Task.FromResult(permissions.ToList());
        }

        public Task<List<AppPermission>> GetAllAsync(CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<List<AppPermission>> GetByRoleAsync(
            string roleId,
            CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task AssignToRoleAsync(
            string roleId,
            IEnumerable<int> permissionIds,
            CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task RemoveFromRoleAsync(
            string roleId,
            IEnumerable<int> permissionIds,
            CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task SetRolePermissionsAsync(
            string roleId,
            IEnumerable<int> permissionIds,
            CancellationToken ct = default) =>
            throw new NotSupportedException();
    }

    private static void AssertPermission(Type controllerType, string? methodName, string module, string resource, string? action)
    {
        var target = methodName is null
            ? (System.Reflection.MemberInfo)controllerType
            : controllerType.GetMethods().Single(method => method.Name == methodName);
        var data = target.CustomAttributes.Single(attribute => attribute.AttributeType == typeof(DomainPermissionAttribute));
        var arguments = data.ConstructorArguments.Select(argument => argument.Value as string).ToArray();

        Assert.Equal(module, arguments[0]);
        Assert.Equal(resource, arguments[1]);
        Assert.Equal(action, arguments.Length > 2 ? arguments[2] : null);
    }

    private static void AssertNoDomainPermission(Type controllerType, string methodName)
    {
        var method = controllerType.GetMethods().Single(candidate => candidate.Name == methodName);
        Assert.DoesNotContain(method.CustomAttributes, attribute => attribute.AttributeType == typeof(DomainPermissionAttribute));
    }
}
