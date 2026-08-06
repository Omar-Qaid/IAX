namespace IAX.IXApi.Infrastructure.Identity
{
    public interface ICurrentUserService
    {
        string GetCurrentUserId();
        string GetOwnerAccountId();
        string GetDataAreaId();
    }
}
