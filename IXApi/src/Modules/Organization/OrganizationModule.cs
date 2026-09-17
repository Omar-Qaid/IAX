using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IAX.IXApi.Modules.Organization.DocumentManagement.Services;
using IAX.IXApi.Modules.Organization.DocumentManagement.Storage;

namespace IAX.IXApi.Modules.Organization
{
    public static class OrganizationModule
    {
        public static IServiceCollection AddOrganizationModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<Structure.OrganizationStructureService>();
            services.AddScoped<IAX.IXApi.Shared.Application.Organization.IOrganizationDirectory>(provider => provider.GetRequiredService<Structure.OrganizationStructureService>());
            services.AddScoped<Announcements.IAnnouncementService, Announcements.AnnouncementService>();
            services.Configure<DocumentStorageOptions>(configuration.GetSection("DocumentStorage"));
            services.AddSingleton<IFileStorageProvider, FileStorageService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<Departments.IDepartmentService, Departments.DepartmentService>();
            services.AddScoped<Features.HcmWorkerCategory.IHcmWorkerCategoryService, Features.HcmWorkerCategory.HcmWorkerCategoryService>();
            services.Configure<DocumentStorageOptions>(configuration.GetSection("DocumentStorage"));
            services.AddSingleton<IFileStorageProvider, FileStorageService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<Departments.IDepartmentService, Departments.DepartmentService>();
            services.AddScoped<Features.HcmWorkerCategory.IHcmWorkerCategoryService, Features.HcmWorkerCategory.HcmWorkerCategoryService>();
            services.AddScoped<Features.HcmWorkerGroup.IHcmWorkerGroupService, Features.HcmWorkerGroup.HcmWorkerGroupService>();
            services.AddScoped<HcmWorkerManagers.IHcmWorkerManagerService, HcmWorkerManagers.HcmWorkerManagerService>();
            services.AddScoped<Employees.IHcmWorkerService, Employees.HcmWorkerService>();
            services.AddScoped<Genders.IGenderService, Genders.GenderService>();
            services.AddScoped<ManagementLevels.IHcmWorkerManagementLevelService, ManagementLevels.HcmWorkerManagementLevelService>();
            services.AddScoped<Nationalities.INationalityService, Nationalities.NationalityService>();
            services.AddScoped<Occupations.IOccupationService, Occupations.OccupationService>();
            return services;
        }
    }
}
