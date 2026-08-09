using IAX.IXApi.Shared.Application.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Api.Middleware;

public sealed class CompanySelectionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICompanyExecutionContext companyContext)
    {
        if (!companyContext.IsRequestedCompanyAuthorized())
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Company access denied",
                Detail = "The requested company is not authorized for the current user."
            }, context.RequestAborted);
            return;
        }

        await next(context);
    }
}
