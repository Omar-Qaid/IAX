namespace IAX.IXMcp.Security;

public sealed record McpSessionContext(
    string UserId,
    string UserName,
    string AccessToken,
    string Company,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public sealed record SessionValidationResult(
    bool IsValid,
    int StatusCode,
    string ErrorCode,
    McpSessionContext? Context)
{
    public static SessionValidationResult Success(McpSessionContext context) =>
        new(true, StatusCodes.Status200OK, string.Empty, context);

    public static SessionValidationResult Failure(int statusCode, string errorCode) =>
        new(false, statusCode, errorCode, null);
}
