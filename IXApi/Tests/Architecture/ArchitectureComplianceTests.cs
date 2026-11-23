using System;
using System.Linq;
using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace IAX.IXApi.Tests
{
    public class ArchitectureComplianceTests
    {
        private static readonly Assembly[] Assemblies = new[]
        {
            typeof(IAX.IXApi.Api.Middleware.GlobalExceptionHandler).Assembly,
            typeof(IAX.IXApi.Modules.Identity.IdentityModule).Assembly,
            typeof(IAX.IXApi.Modules.Organization.OrganizationModule).Assembly,
            typeof(IAX.IXApi.Modules.Workflow.WorkflowModule).Assembly,
            typeof(IAX.IXApi.Modules.Finance.FinanceModule).Assembly,
            typeof(IAX.IXApi.Modules.Communication.CommunicationModule).Assembly,
            typeof(IAX.IXApi.Modules.Administration.AdministrationModule).Assembly,
            typeof(IAX.IXApi.Shared.Domain.Entities.AuditableEntity).Assembly,
            typeof(IAX.IXApi.Infrastructure.Persistence.ApplicationDbContext).Assembly
        };

        [Fact]
        public void Shared_Namespace_Should_Not_Depend_On_Infrastructure_Or_Api_Or_Bootstrap()
        {
            var forbidden = new[]
            {
                "IAX.IXApi.Infrastructure",
                "IAX.IXApi.Api",
                "IAX.IXApi.Bootstrap"
            };

            var result = Types.InAssemblies(Assemblies)
                .That()
                .ResideInNamespace("IAX.IXApi.Shared")
                .ShouldNot()
                .HaveDependencyOnAny(forbidden)
                .GetResult();

            Assert.True(result.IsSuccessful, "Shared namespace has illegal dependencies on Infrastructure, Api, or Bootstrap.");
        }

        [Fact]
        public void Shared_Namespace_Should_Only_Depend_On_Identity_Module_For_User_Auditing()
        {
            var forbiddenModules = new[]
            {
                "IAX.IXApi.Modules.Organization",
                "IAX.IXApi.Modules.Workflow",
                "IAX.IXApi.Modules.Finance",
                "IAX.IXApi.Modules.Communication",
                "IAX.IXApi.Modules.Administration"
            };

            var result = Types.InAssemblies(Assemblies)
                .That()
                .ResideInNamespace("IAX.IXApi.Shared")
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenModules)
                .GetResult();

            Assert.True(result.IsSuccessful, "Shared namespace depends on modular boundaries other than Identity.");
        }

        [Fact]
        public void Shared_Domain_Should_Not_Depend_On_Infrastructure_Or_Api()
        {
            var forbidden = new[]
            {
                "IAX.IXApi.Infrastructure",
                "IAX.IXApi.Api"
            };

            var result = Types.InAssemblies(Assemblies)
                .That()
                .ResideInNamespace("IAX.IXApi.Shared.Domain")
                .ShouldNot()
                .HaveDependencyOnAny(forbidden)
                .GetResult();

            Assert.True(result.IsSuccessful, "Shared.Domain should not depend on Infrastructure or Api.");
        }

        [Fact]
        public void Modules_Should_Follow_Strict_Modular_Boundaries()
        {
            var modules = new[] { "Identity", "Organization", "Workflow", "Finance", "Communication", "Administration" };

            foreach (var module in modules)
            {
                // Define whitelisted modules that this module is allowed to depend on (for legacy compatibility)
                var whitelistedModules = module switch
                {
                    "Identity" => new[] { "Organization", "Administration" }, // User employee link and audits
                    "Organization" => new[] { "Finance", "Identity", "Administration" },    // Address structures, permissions, and audits
                    "Workflow" => new[] { "Identity", "Organization", "Finance", "Administration", "Communication" }, // Performers, step/variable tracking, number sequences, audits, and notifications
                    "Finance" => new[] { "Identity", "Organization", "Administration" },   // Permissions, legal entities/employees, number sequences, and audits
                    "Communication" => new[] { "Identity", "Administration", "Organization", "Finance" }, // Realtime chat and notification users/permissions/workers
                    "Administration" => new[] { "Identity", "Communication" }, // Permissions and background jobs (e.g. sample notifications cleanup)
                    _ => Array.Empty<string>()
                };

                var forbiddenModules = modules
                    .Where(m => m != module && !whitelistedModules.Contains(m))
                    .Select(m => $"IAX.IXApi.Modules.{m}")
                    .ToArray();

                if (forbiddenModules.Length == 0) continue;

                var result = Types.InAssemblies(Assemblies)
                    .That()
                    .ResideInNamespace($"IAX.IXApi.Modules.{module}")
                    .ShouldNot()
                    .HaveDependencyOnAny(forbiddenModules)
                    .GetResult();

                var failingTypes = result.FailingTypeNames != null 
                    ? string.Join(", ", result.FailingTypeNames) 
                    : "None";

                Assert.True(result.IsSuccessful, $"Module '{module}' violates boundary rules by depending on forbidden modules. Violations in: {failingTypes}");
            }
        }
    }
}
