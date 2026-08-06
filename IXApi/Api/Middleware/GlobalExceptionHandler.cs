using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;

namespace IAX.IXApi.Api.Middleware
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;
        private readonly IServiceProvider _serviceProvider;

        private static readonly string[] SensitiveKeys = { "password", "pass", "pwd", "token", "authorization", "client_secret", "api_key", "secret" };
        private const int MaxPreviewLength = 2048;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _env = env;
            _serviceProvider = serviceProvider;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            
            var (status, title) = Classify(exception);
            var traceId = context.TraceIdentifier;
            var corrId = context.Request.Headers.TryGetValue("X-Correlation-ID", out var cid) ? cid.ToString() : null;

            _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}, CorrelationId: {CorrelationId}, Status: {Status}", 
                traceId, corrId, status);

            // Log to Database
            await PersistExceptionAsync(context, exception, status, sw.ElapsedMilliseconds, corrId);

            ProblemDetails problemDetails;

            if (exception is FluentValidation.ValidationException vex)
            {
                var validationProblem = new ValidationProblemDetails(
                    vex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                )
                {
                    Status = status,
                    Title = "One or more validation errors occurred.",
                    Detail = "Please refer to the errors property for additional details.",
                    Instance = context.Request.Path
                };
                problemDetails = validationProblem;
            }
            else if (exception is Microsoft.EntityFrameworkCore.DbUpdateException dbex)
            {
                var innerMsg = dbex.InnerException?.Message ?? "";
                string detailMessage = "Database error occurred.";
                
                if (innerMsg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) || 
                    innerMsg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
                {
                    status = (int)HttpStatusCode.Conflict;
                    title = "Conflict";
                    detailMessage = "A record with the same unique data already exists.";
                }
                else if (innerMsg.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase) || 
                         innerMsg.Contains("foreign key", StringComparison.OrdinalIgnoreCase))
                {
                    status = (int)HttpStatusCode.Conflict;
                    title = "Conflict";
                    detailMessage = "Cannot perform this action because the record is linked to other data.";
                }
                else if (_env.IsDevelopment())
                {
                    detailMessage = innerMsg;
                }

                problemDetails = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = detailMessage,
                    Instance = context.Request.Path
                };
            }
            else
            {
                problemDetails = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = _env.IsDevelopment() ? exception.ToString() : exception.Message,
                    Instance = context.Request.Path
                };
            }

            problemDetails.Extensions["traceId"] = traceId;
            if (!string.IsNullOrEmpty(corrId))
            {
                problemDetails.Extensions["correlationId"] = corrId;
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = status;

            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static (int Status, string Title) Classify(Exception ex) =>
            ex switch
            {
                FluentValidation.ValidationException => ((int)HttpStatusCode.BadRequest, "Validation Error"),
                UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized"),
                ArgumentException => ((int)HttpStatusCode.BadRequest, "Invalid request"),
                KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Not found"),
                OperationCanceledException => ((int)HttpStatusCode.RequestTimeout, "Request canceled"),
                _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred")
            };

        private async Task PersistExceptionAsync(HttpContext context, Exception exception, int status, long elapsedMs, string? correlationId)
        {
            try
            {
                // We use a new scope to avoid issues with the current request's DbContext state
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var activity = Activity.Current;

                var log = new SysExceptionLog
                {
                    Severity = "Error",
                    ExceptionType = exception.GetType().FullName,
                    ExceptionMessage = Truncate(exception.Message, 1024),
                    StackTrace = Truncate(exception.ToString(), 32000),
                    Source = exception.Source,
                    HttpMethod = context.Request.Method,
                    Path = context.Request.Path,
                    StatusCode = status,
                    QueryString = Truncate(context.Request.QueryString.Value, MaxPreviewLength),
                    UserName = context.User?.Identity?.IsAuthenticated == true ? context.User.Identity!.Name : "Anonymous",
                    ClientIpMasked = MaskIp(context.Connection.RemoteIpAddress?.ToString()),
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    RequestId = context.TraceIdentifier,
                    CorrelationId = correlationId,
                    TraceId = activity?.TraceId.ToString(),
                    SpanId = activity?.SpanId.ToString(),
                    Environment = _env.EnvironmentName,
                    AppVersion = GetAppVersion(),
                    Server = Environment.MachineName,
                    RequestHeaders = Truncate(MaskHeaders(context.Request.Headers), MaxPreviewLength),
                    ElapsedMs = elapsedMs,
                    Tags = "Unhandled,API,ExceptionHandler",
                    ControllerName = context.GetRouteValue("controller")?.ToString(),
                    ActionName = context.GetRouteValue("action")?.ToString()
                };

                db.SysExceptionLogs.Add(log);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist exception log to database via IExceptionHandler.");
            }
        }

        private static string? Truncate(string? s, int max) => string.IsNullOrEmpty(s) ? s : (s.Length > max ? s[..max] : s);

        private static string MaskIp(string? ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return "unknown";
            var parts = ip.Split('.');
            return parts.Length == 4 ? $"{parts[0]}.{parts[1]}.***.***" : ip;
        }

        private static string MaskHeaders(IHeaderDictionary headers)
        {
            var dict = headers.ToDictionary(k => k.Key, v => string.Join(",", v.Value.ToArray()), StringComparer.OrdinalIgnoreCase);
            var keysToMask = new[] { "Authorization", "Cookie", "Set-Cookie", "ApiKey" };
            foreach (var key in dict.Keys.Where(k => keysToMask.Any(m => m.Equals(k, StringComparison.OrdinalIgnoreCase))).ToList())
            {
                dict[key] = "***";
            }
            return JsonSerializer.Serialize(dict);
        }

        private static string? GetAppVersion() => System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
    }
}

