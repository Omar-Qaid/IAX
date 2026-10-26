using FluentValidation;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Organization.Announcements;
using IAX.IXApi.Modules.Organization.DocumentManagement.Dtos;
using IAX.IXApi.Modules.Organization.DocumentManagement.Entities;
using IAX.IXApi.Modules.Organization.DocumentManagement.Interfaces;
using IAX.IXApi.Modules.Organization.DocumentManagement.Services;
using IAX.IXApi.Modules.Organization.DocumentManagement.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Modules.Organization
{
    public static class OrganizationModule
    {
        public static IServiceCollection AddOrganizationModule(this IServiceCollection services, IConfiguration configuration)
        {
            // Announcements
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<IBaseService<Announcement>>(sp => sp.GetRequiredService<IAnnouncementService>());
            services.AddScoped<IValidator<AnnouncementDto>, AnnouncementDtoValidator>();

            // Document Management Infrastructure & Services
            services.Configure<DocumentStorageOptions>(configuration.GetSection("DocumentStorage"));
            services.AddSingleton<IFileStorageProvider, FileStorageService>();
            services.AddScoped<IDocumentService, DocumentService>();

            // Entity Services
            services.AddScoped<IDocuTypeService, DocuTypeService>();
            services.AddScoped<IBaseService<DocuType>>(sp => sp.GetRequiredService<IDocuTypeService>());
            services.AddScoped<IDocuRefService, DocuRefService>();
            services.AddScoped<IBaseService<DocuRef>>(sp => sp.GetRequiredService<IDocuRefService>());

            // Validators
            services.AddScoped<IValidator<DocuTypeDto>, DocuTypeDtoValidator>();
            services.AddScoped<IValidator<DocumentDto>, DocumentDtoValidator>();
            services.AddScoped<IValidator<CreateDocumentForm>, CreateDocumentFormValidator>();
            services.AddScoped<IValidator<UpdateDocumentRequest>, UpdateDocumentRequestValidator>();

            return services;
        }
    }
}
