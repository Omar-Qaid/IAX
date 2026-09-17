namespace IAX.IXMcp.Security;

public interface IMcpSessionValidator
{
    Task<SessionValidationResult> ValidateAsync(
        string accessToken,
        string company,
        CancellationToken cancellationToken = default);
}
