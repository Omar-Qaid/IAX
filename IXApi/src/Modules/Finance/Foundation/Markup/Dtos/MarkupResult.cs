namespace IAX.IXApi.Modules.Finance.Foundation.Markup;

public sealed record MarkupServiceError(string Message, bool NotFound = false);
public sealed record MarkupResult<T>(T? Data, MarkupServiceError? Error = null);
