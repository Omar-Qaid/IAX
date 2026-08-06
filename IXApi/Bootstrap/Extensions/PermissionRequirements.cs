using Microsoft.AspNetCore.Authorization;

namespace IAX.IXApi.Bootstrap.Extensions
{
    public class ReadPermission : IAuthorizationRequirement { }
    public class EditPermission : IAuthorizationRequirement { }
    public class DeletePermission : IAuthorizationRequirement { }
    public class NoImpersonationRequirement : IAuthorizationRequirement { }
}
