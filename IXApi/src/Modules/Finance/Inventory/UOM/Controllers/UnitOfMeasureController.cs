using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Inventory;

[ApiController]
[Route("api/v1/UnitOfMeasure")]
[DomainPermission("Inventory", "Transactions")]
public sealed class UnitOfMeasureController
    : BaseController<UnitOfMeasure, UnitOfMeasureDto>
{
    private readonly IUnitOfMeasureService _uomService;

    public UnitOfMeasureController(
        IUnitOfMeasureService service,
        ILogger<UnitOfMeasureController> logger)
        : base(service, logger)
    {
        _uomService = service;
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<UnitOfMeasureDto>>>> GetAll(CancellationToken cancellationToken = default) =>
        base.GetAll(cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<UnitOfMeasureDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<UnitOfMeasureDto>>> Create(UnitOfMeasureDto dto, CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<UnitOfMeasureDto>>> Update(string id, UnitOfMeasureDto dto, CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(APIResponse<object>.Ok(await _uomService.ListAsync(ct)));

    [HttpPost]
    public async Task<IActionResult> Create(UnitOfMeasureInputDto input, CancellationToken ct) =>
        Result(await _uomService.CreateAsync(input, ct));

    [HttpPut("{recId:long}")]
    public async Task<IActionResult> Update(long recId, UnitOfMeasureInputDto input, CancellationToken ct) =>
        Result(await _uomService.UpdateAsync(recId, input, ct));

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<APIResponse<bool>>> Delete(long id, CancellationToken cancellationToken = default)
    {
        var error = await _uomService.DeleteAsync(id, cancellationToken);
        return error is null
            ? Ok(APIResponse<bool>.Ok(true))
            : Conflict(APIResponse<bool>.Fail(error));
    }

    [HttpPost("conversions")]
    public async Task<IActionResult> CreateConversion(UnitOfMeasureConversionInputDto input, CancellationToken ct) =>
        ConversionResult(await _uomService.SaveConversionAsync(null, input, ct));

    [HttpPut("conversions/{recId:long}")]
    public async Task<IActionResult> UpdateConversion(long recId, UnitOfMeasureConversionInputDto input, CancellationToken ct) =>
        ConversionResult(await _uomService.SaveConversionAsync(recId, input, ct));

    [HttpDelete("conversions/{recId:long}")]
    public async Task<IActionResult> DeleteConversion(long recId, CancellationToken ct)
    {
        var error = await _uomService.DeleteConversionAsync(recId, ct);
        return error is null
            ? Ok(APIResponse<bool>.Ok(true))
            : NotFound(APIResponse<object>.Fail(error));
    }

    private IActionResult Result((UnitOfMeasureDto? Unit, string? Error) result) =>
        result.Unit is null
            ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<UnitOfMeasureDto>.Ok(result.Unit));

    private IActionResult ConversionResult((UnitOfMeasureConversionDto? Conversion, string? Error) result) =>
        result.Conversion is null
            ? UnprocessableEntity(APIResponse<object>.Fail(result.Error!))
            : Ok(APIResponse<UnitOfMeasureConversionDto>.Ok(result.Conversion));
}
