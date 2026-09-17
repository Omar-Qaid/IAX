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
            services.AddScoped<Announcements.IAnnouncementService, Announcements.AnnouncementService>();
            services.Configure<DocumentStorageOptions>(configuration.GetSection("DocumentStorage"));
            services.AddSingleton<IFileStorageProvider, FileStorageService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.Configure<DocumentStorageOptions>(configuration.GetSection("DocumentStorage"));
            services.AddSingleton<IFileStorageProvider, FileStorageService>();
            services.AddScoped<IDocumentService, DocumentService>();
                 return services;
        }
    }
}
