using System.Reflection;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;

namespace IAX.IXApi.Modules.Administration;

public static class AdministrationModule
{
    public static IServiceCollection AddAdministrationModule(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly)
    {
        services.Configure<SysBackgroundJobOptions>(configuration.GetSection("BackgroundJobs"));
        services.AddHostedService<SysBackgroundJobProcessor>();

        foreach (var handlerType in assembly.GetTypes().Where(type =>
                     type is { IsClass: true, IsAbstract: false } &&
                     typeof(ISysBackgroundJobHandler).IsAssignableFrom(type)))
        {
            services.AddScoped(handlerType);
            services.AddScoped(typeof(ISysBackgroundJobHandler), provider => provider.GetRequiredService(handlerType));
        }

        return services;
    }
}
