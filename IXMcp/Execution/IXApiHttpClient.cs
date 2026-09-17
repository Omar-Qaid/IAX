namespace IAX.IXMcp.Execution;

public sealed class IXApiHttpClient(HttpClient client)
{
    public Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken) =>
        client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
}
