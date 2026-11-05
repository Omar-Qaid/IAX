using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;
public static class FoundationExtendedSeedRegistration
{
 public static IServiceCollection AddFoundationExtendedSeeders(this IServiceCollection services)
 {
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, DlvModeSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, DlvTermSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, PaymSchedSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, PaymSchedLineSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, LogisticsLocationRoleSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, DirPartyLocationRoleSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, FoundationPersonSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, OrganizationUnitSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, OrganizationRoleSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, OrganizationHierarchySeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, OrganizationHierarchyNodeSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, HcmPositionSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, HcmReportingHierarchySeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, HcmPositionReportingLineSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, HcmShowroomSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, GenderSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, HcmNationalitySeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, HcmDepartmentSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, HcmOccupationSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, HcmWorkerSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, HcmWorkerOrganizationAssignmentSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, HcmWorkerShowroomAssignmentSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, HcmWorkerOrganizationAssignmentV1Seeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, ContactPersonSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, TaxAuthorityAddressSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, TaxPeriodHeadSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, TaxReportPeriodSeeder>());
  services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSeeder, FoundationDimensionSeeder>());
  return services;
 }
}
