using IAX.IXApi.Infrastructure.Identity;

namespace IAX.IXApi.Modules.Finance;

internal sealed class NullCurrentUserService : ICurrentUserService
{
    public static readonly NullCurrentUserService Instance = new();

    public string GetCurrentUserId() => string.Empty;
    public string GetOwnerAccountId() => string.Empty;
    public string GetDataAreaId() => string.Empty;
}
