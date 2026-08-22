using IAX.IXApi.Bootstrap.Extensions;
using Microsoft.Extensions.DependencyInjection;
using IAX.IXApi.Infrastructure.Persistence.Services;
using System.Reflection;

namespace IAX.IXApi.Bootstrap
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, Assembly assembly)
        {
            services.AddSignalR();

            // ── Event Bus ────────────────────────────────────────────────────
            // Auto-register every ISysEventHandler<TEvent> against its closed interface so the
            // event bus can fan an event out to all subscribers. Adding a handler needs no DI edit.
            var eventHandlerOpenInterface = typeof(IAX.IXApi.Shared.Domain.Events.ISysEventHandler<>);
            foreach (var type in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
            {
                foreach (var closed in type.GetInterfaces()
                             .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == eventHandlerOpenInterface))
                {
                    services.AddScoped(closed, type);
                }
            }

            services.AddScoped(typeof(IBaseService<>), typeof(GenericService<>));
            
            // Infrastructure Services
            services.AddScoped<Infrastructure.Identity.ICurrentUserService, Infrastructure.Identity.CurrentUserService>();
            services.AddScoped<Infrastructure.Persistence.Seeding.IDatabaseSeederService, Infrastructure.Persistence.Seeding.DatabaseSeederService>();
            services.AddScoped<Infrastructure.Realtime.ISysRealtimeManager, Infrastructure.Realtime.SysRealtimeManager>();

            // Shared Services
            services.AddScoped<Shared.Application.Conversion.IValueConverter, Shared.Application.Conversion.ValueConverterService>();
            services.AddScoped<Shared.Domain.Events.ISysEventBus, Shared.Domain.Events.SysEventBus>();

            services.RegisterValidators(assembly);
            services.RegisterMapsterConfigurations(assembly);
            
            return services;
        }
    }
}
