using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Foundation.Tax;

[ApiController]
[Route("api/v1/tax/reports")]
[DomainPermission("Tax", "Settlement", "View")]
public sealed class TaxReportController(ITaxReportService service) : ControllerBase
{
    /// <summary>Posted sales tax per code and direction, for a date range or a settlement period.</summary>
    [HttpGet("posted-sales-tax")]
    public async Task<IActionResult> PostedSalesTax([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate,
        [FromQuery] string? taxPeriod, [FromQuery] string? taxCode, CancellationToken ct)
    {
        var (result, error) = await service.RunAsync(fromDate, toDate, taxPeriod, taxCode, ct);
        return error is null
            ? Ok(APIResponse<TaxReportDto>.Ok(result!))
            : UnprocessableEntity(APIResponse<object>.Fail(error));
    }
}
