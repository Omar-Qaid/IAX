using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace IAX.IXApi.Bootstrap.Extensions
{
    public static class ValidatorRegistrationExtensions
    {
        public static IServiceCollection RegisterValidators(this IServiceCollection services, Assembly assembly)
        {
            services.AddValidatorsFromAssembly(assembly);
            return services;
        }
    }
}
