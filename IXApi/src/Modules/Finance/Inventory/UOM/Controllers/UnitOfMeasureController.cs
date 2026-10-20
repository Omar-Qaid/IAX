using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/UnitOfMeasure")]
[DomainPermission("Inventory", "Transactions")]
public sealed class UnitOfMeasureController(IUnitOfMeasureService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(APIResponse<object>.Ok(await service.ListAsync(ct)));
    [HttpPost] public async Task<IActionResult> Create(UnitOfMeasureInputDto input, CancellationToken ct) => Result(await service.CreateAsync(input, ct));
    [HttpPut("{recId:long}")] public async Task<IActionResult> Update(long recId, UnitOfMeasureInputDto input, CancellationToken ct) => Result(await service.UpdateAsync(recId, input, ct));
    [HttpDelete("{recId:long}")] public async Task<IActionResult> Delete(long recId, CancellationToken ct) { var error = await service.DeleteAsync(recId, ct); return error is null ? Ok(APIResponse<bool>.Ok(true)) : Conflict(APIResponse<object>.Fail(error)); }
    [HttpPost("conversions")] public async Task<IActionResult> CreateConversion(UnitOfMeasureConversionInputDto input, CancellationToken ct) => ConversionResult(await service.SaveConversionAsync(null, input, ct));
    [HttpPut("conversions/{recId:long}")] public async Task<IActionResult> UpdateConversion(long recId, UnitOfMeasureConversionInputDto input, CancellationToken ct) => ConversionResult(await service.SaveConversionAsync(recId, input, ct));
    [HttpDelete("conversions/{recId:long}")] public async Task<IActionResult> DeleteConversion(long recId, CancellationToken ct) { var error = await service.DeleteConversionAsync(recId, ct); return error is null ? Ok(APIResponse<bool>.Ok(true)) : NotFound(APIResponse<object>.Fail(error)); }
    private IActionResult Result((UnitOfMeasureDto? Unit, string? Error) result) => result.Unit is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<UnitOfMeasureDto>.Ok(result.Unit));
    private IActionResult ConversionResult((UnitOfMeasureConversionDto? Conversion, string? Error) result) => result.Conversion is null ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!)) : Ok(APIResponse<UnitOfMeasureConversionDto>.Ok(result.Conversion));
}
