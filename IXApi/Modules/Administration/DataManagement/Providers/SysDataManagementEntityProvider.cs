using System.Reflection;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Administration.DataManagement.Services;

namespace IAX.IXApi.Modules.Administration.DataManagement.Providers
{
    [ScopedService]
    public class SysDataManagementEntityProvider : ISysDataManagementEntityProvider
    {
        private readonly Lazy<IReadOnlyDictionary<string, Type>> _allowedEntities;

        public SysDataManagementEntityProvider()
        {
            _allowedEntities = new Lazy<IReadOnlyDictionary<string, Type>>(DiscoverEntities);
        }

        public Type? GetEntityType(string entityName)
        {
            if (_allowedEntities.Value.TryGetValue(entityName, out var type))
            {
                return type;
            }
            return null;
        }

        public IReadOnlyDictionary<string, Type> GetAllowedEntities()
        {
            return _allowedEntities.Value;
        }

        private IReadOnlyDictionary<string, Type> DiscoverEntities()
        {
            var entities = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            
            // Get all assemblies related to the project
            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<DataManagementAttribute>() != null);

                foreach (var type in types)
                {
                    // Use the class name or a custom name if provided in the attribute
                    var attr = type.GetCustomAttribute<DataManagementAttribute>();
                    var name = attr?.DisplayName ?? type.Name;
                    
                    if (!entities.ContainsKey(name))
                    {
                        entities[name] = type;
                    }
                }
            }

            return entities;
        }
    }
}
