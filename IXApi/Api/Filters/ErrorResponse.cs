using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Api.Filters
{
    public class ErrorResponse
    {
        public static IActionResult GenerateErrorResponse(ActionContext context)
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var response = APIResponse<object>.Fail("One or more validation errors occurred.", errors);
            
            return new UnprocessableEntityObjectResult(response);
        }
    }
}
