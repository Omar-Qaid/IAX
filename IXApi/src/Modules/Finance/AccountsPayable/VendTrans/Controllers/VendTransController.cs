using FluentValidation;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

[ApiController]
[Route("api/v1/VendTrans")]
[DomainPermission("AccountsPayable", "Transactions", "View")]
public sealed class VendTransController : BaseController<VendTrans, VendTransDto>
{
    private readonly IVendTransService _vendTransService;
    private readonly IValidator<VendTransQueryDto> _validator;

    public VendTransController(
        IVendTransService service,
        ILogger<VendTransController> logger,
        IValidator<VendTransQueryDto> validator)
        : base(service, logger)
    {
        _vendTransService = service;
        _validator = validator;
    }

    [HttpGet("{recId:long}")]
    public async Task<ActionResult<APIResponse<VendTransDto>>> Get(long recId, CancellationToken ct)
    {
        var transaction = await _vendTransService.GetAsync(recId, ct);
        return transaction is null
            ? NotFound(APIResponse<VendTransDto>.Fail("Vendor transaction was not found in the selected company."))
            : Ok(APIResponse<VendTransDto>.Ok(transaction));
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<VendTransDto>>> GetById(string id, CancellationToken cancellationToken = default) =>
        base.GetById(id, cancellationToken);

    [HttpGet("paged")]
    public async Task<ActionResult<APIResponse<VendTransPageDto>>> Paged(
        [FromQuery] VendTransQueryDto query, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(query, ct);
        if (!validation.IsValid)
            return BadRequest(APIResponse<VendTransPageDto>.Fail(
                string.Join("; ", validation.Errors.Select(x => x.ErrorMessage))));
        return Ok(APIResponse<VendTransPageDto>.Ok(await _vendTransService.SearchAsync(query, ct)));
    }

    [NonAction]
    public override Task<ActionResult<APIResponse<VendTransDto>>> Create(
        VendTransDto dto,
        CancellationToken cancellationToken = default) =>
        base.Create(dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<VendTransDto>>> Update(
        string id,
        VendTransDto dto,
        CancellationToken cancellationToken = default) =>
        base.Update(id, dto, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<bool>>> Delete(
        string id,
        CancellationToken cancellationToken = default) =>
        base.Delete(id, cancellationToken);

    [NonAction]
    public override Task<ActionResult<APIResponse<IEnumerable<VendTransDto>>>> GetPaged(
        [FromQuery] QueryFilterDto paginationParams, CancellationToken cancellationToken = default) =>
        base.GetPaged(paginationParams, cancellationToken);
}
