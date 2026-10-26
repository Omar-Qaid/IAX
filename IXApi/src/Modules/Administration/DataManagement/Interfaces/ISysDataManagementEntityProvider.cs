using System;
using System.Collections.Generic;

namespace IAX.IXApi.Modules.Administration.DataManagement.Interfaces
{
    public interface ISysDataManagementEntityProvider
    {
        Type? GetEntityType(string entityName);
        IReadOnlyDictionary<string, Type> GetAllowedEntities();
    }
}
