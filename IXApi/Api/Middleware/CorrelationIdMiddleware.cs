namespace IAX.IXApi.Api.Middleware
{
    public sealed class CorrelationIdMiddleware : IMiddleware
    {
        private const string HeaderName = "X-Correlation-ID";

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!context.Request.Headers.TryGetValue(HeaderName, out var id) || string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString("n");
                context.Request.Headers[HeaderName] = id;
            }

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = id.ToString();
                return Task.CompletedTask;
            });

            await next(context);
        }
    }
}
