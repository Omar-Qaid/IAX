using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Shared.Application.Identity;

namespace IAX.IXApi.Tests.Support.Identity;

/// <summary>
/// The acting user and company for a test: always authorized, in company <c>dat</c> unless a test switches
/// <see cref="DataAreaId"/>. The user id is written to <c>CreatedBy</c>/<c>OwnerAccountId</c>, so a test that
/// asserts on audit columns names its own actor.
/// </summary>
public sealed class TestIdentity(string userId = "test", string dataAreaId = TestIdentity.DefaultCompany)
    : ICompanyExecutionContext, ICurrentUserService
{
    public const string DefaultCompany = "dat";

    public string DataAreaId { get; set; } = dataAreaId;

    public string GetDataAreaId() => DataAreaId;
    public bool IsRequestedCompanyAuthorized() => true;
    public string GetCurrentUserId() => userId;
    public string GetOwnerAccountId() => userId;
}
