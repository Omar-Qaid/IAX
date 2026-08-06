using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace IAX.IXApi.Bootstrap.Extensions
{
    public static class MapsterExtensions
    {
        public static void RegisterMapsterConfigurations(this IServiceCollection services, params Assembly[] assemblies)
        {
            var config = TypeAdapterConfig.GlobalSettings;

            // Note: Mapster 7.4.0 has no global "NullGuard" toggle (NullGuard/NullGuardFlag
            // don't exist in this version). Guard against NullReferenceException from
            // flattening un-loaded EF navigations (e.g. Department.Name -> DepartmentName)
            // per mapping via an IRegister — see OrgEmployeeMapping / OrgShowroomMapping:
            //   .Map(d => d.DepartmentName, s => s.Department != null ? s.Department.Name : null)

            // Suppress the explicit-interface member IEntity.Id that Mapster
            // would otherwise attempt to map (causes ambiguity).
            config.Default.IgnoreMember((member, _) => member.Name.Contains("IEntity.Id"));

            // Ignore RecId when mapping to entities (non-DTO destinations) to prevent EF Core key-modification errors on updates.
            config.Default.IgnoreMember((member, side) => 
                side == MemberSide.Destination && 
                member.Name == "RecId" && 
                member.Info is System.Reflection.MemberInfo memberInfo && 
                memberInfo.DeclaringType != null && 
                !memberInfo.DeclaringType.Name.Contains("Dto"));

            config.Scan(assemblies);
            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();
        }
    }
}
