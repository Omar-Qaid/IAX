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

            // ── Background Job Management ────────────────────────────────────
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
            
            services.RegisterServicesWithAttributes(assembly);
            services.RegisterValidators(assembly);
            services.RegisterMapsterConfigurations(assembly);
            
            return services;
        }
    }
}
