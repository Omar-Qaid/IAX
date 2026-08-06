using IAX.IXApi.Shared.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace IAX.IXApi.Bootstrap.Extensions
{
    public static class AttributeRegistrationExtensions
    {
        public static IServiceCollection RegisterServicesWithAttributes(this IServiceCollection services, Assembly assembly)
        {
            var typesWithAttributes = assembly.GetTypes()
                .Where(t => t.GetCustomAttributes<ScopedServiceAttribute>().Any() ||
                            t.GetCustomAttributes<TransientServiceAttribute>().Any() ||
                            t.GetCustomAttributes<SingletonServiceAttribute>().Any());

            foreach (var type in typesWithAttributes)
            {
                var interfaces = type.GetInterfaces();
                var primaryInterface = interfaces.FirstOrDefault(i => i.Name == $"I{type.Name}") ?? interfaces.FirstOrDefault();

                if (type.GetCustomAttributes<ScopedServiceAttribute>().Any())
                {
                    if (primaryInterface != null) services.AddScoped(primaryInterface, type);
                    else services.AddScoped(type);
                }
                else if (type.GetCustomAttributes<TransientServiceAttribute>().Any())
                {
                    if (primaryInterface != null) services.AddTransient(primaryInterface, type);
                    else services.AddTransient(type);
                }
                else if (type.GetCustomAttributes<SingletonServiceAttribute>().Any())
                {
                    if (primaryInterface != null) services.AddSingleton(primaryInterface, type);
                    else services.AddSingleton(type);
                }
            }

            return services;
        }
    }
}
