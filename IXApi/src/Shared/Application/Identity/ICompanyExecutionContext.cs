namespace IAX.IXApi.Shared.Application.Identity;

public interface ICompanyExecutionContext
{
    string GetDataAreaId();
    bool IsRequestedCompanyAuthorized();
}
