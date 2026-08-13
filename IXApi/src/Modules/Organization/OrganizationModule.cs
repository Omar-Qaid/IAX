using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Modules.Organization
{
    public static class OrganizationModule
    {
        public static IServiceCollection AddOrganizationModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<Announcements.IOrgAnnouncementService, Announcements.OrgAnnouncementService>();
            services.AddScoped<Attachments.IOrgAttachmentService, Attachments.OrgAttachmentService>();
            services.AddScoped<Departments.IOrgDepartmentService, Departments.OrgDepartmentService>();
            services.AddScoped<Features.OrgEmployeeCategory.IOrgEmployeeCategoryService, Features.OrgEmployeeCategory.OrgEmployeeCategoryService>();
            services.AddScoped<Features.OrgEmployeeGroup.IOrgEmployeeGroupService, Features.OrgEmployeeGroup.OrgEmployeeGroupService>();
            services.AddScoped<EmployeeManagers.IOrgEmployeeManagerService, EmployeeManagers.OrgEmployeeManagerService>();
            services.AddScoped<Employees.IHcmWorkerService, Employees.HcmWorkerService>();
            services.AddScoped<Genders.IOrgGenderService, Genders.OrgGenderService>();
            services.AddScoped<ManagementLevels.IOrgManagementLevelService, ManagementLevels.OrgManagementLevelService>();
            services.AddScoped<Nationalities.IOrgNationalityService, Nationalities.OrgNationalityService>();
            services.AddScoped<Occupations.IOrgOccupationService, Occupations.OrgOccupationService>();
            services.AddScoped<Showrooms.IOrgShowroomService, Showrooms.OrgShowroomService>();
            return services;
        }
    }
}
