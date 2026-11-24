using IAX.IXApi.Api.Controllers;
using FluentValidation;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable;

[ApiController]
[Route("api/v1/[controller]")]
[Route("api/[controller]")]
[DomainPermission("AccountsPayable", "PostingProfiles")]
public class VendLedgerController : BaseController<VendLedger, VendLedgerDto>
{
    private readonly IValidator<VendLedgerDto> _validator;

    public VendLedgerController(
        IVendLedgerService service,
        ILogger<VendLedgerController> logger,
        IValidator<VendLedgerDto> validator)
        : base(service, logger)
    {
        _validator = validator;
    }

    public override async Task<ActionResult<APIResponse<VendLedgerDto>>> Create(
        [FromBody] VendLedgerDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _validator.ValidateAsync(dto, cancellationToken);
        return result.IsValid ? await base.Create(dto, cancellationToken)
            : BadRequest(APIResponse<VendLedgerDto>.Fail(result.Errors[0].ErrorMessage));
    }

    public override async Task<ActionResult<APIResponse<VendLedgerDto>>> Update(
        string id, [FromBody] VendLedgerDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _validator.ValidateAsync(dto, cancellationToken);
        return result.IsValid ? await base.Update(id, dto, cancellationToken)
            : BadRequest(APIResponse<VendLedgerDto>.Fail(result.Errors[0].ErrorMessage));
    }
}
