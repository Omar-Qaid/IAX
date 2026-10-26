using FluentValidation;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Organization.Announcements;
using IAX.IXApi.Modules.Organization.DocumentManagement.Services;
using IAX.IXApi.Modules.Organization.DocumentManagement.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Modules.Organization
{
    public static class OrganizationModule
    {
        public static IServiceCollection AddOrganizationModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<IBaseService<Announcement>>(sp => sp.GetRequiredService<IAnnouncementService>());
            services.AddScoped<IValidator<AnnouncementDto>, AnnouncementDtoValidator>();

            services.Configure<DocumentStorageOptions>(configuration.GetSection("DocumentStorage"));
            services.AddSingleton<IFileStorageProvider, FileStorageService>();
            services.AddScoped<IDocumentService, DocumentService>();

            return services;
        }
    }
}
