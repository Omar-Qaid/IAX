namespace IAX.IXApi.Modules.Administration.DataManagement.Services
{
    public interface ISysDataManagementEntityProvider
    {
        Type? GetEntityType(string entityName);
        IReadOnlyDictionary<string, Type> GetAllowedEntities();
    }
}
