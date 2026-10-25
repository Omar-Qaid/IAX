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
public class VendLedgerAccountsController : BaseController<VendLedgerAccounts, VendLedgerAccountsDto>
{
    private readonly IValidator<VendLedgerAccountsDto> _validator;

    public VendLedgerAccountsController(
        IVendLedgerAccountsService service,
        ILogger<VendLedgerAccountsController> logger,
        IValidator<VendLedgerAccountsDto> validator)
        : base(service, logger)
    {
        _validator = validator;
    }

    public override async Task<ActionResult<APIResponse<VendLedgerAccountsDto>>> Create(
        [FromBody] VendLedgerAccountsDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _validator.ValidateAsync(dto, cancellationToken);
        return result.IsValid ? await base.Create(dto, cancellationToken)
            : BadRequest(APIResponse<VendLedgerAccountsDto>.Fail(result.Errors[0].ErrorMessage));
    }

    public override async Task<ActionResult<APIResponse<VendLedgerAccountsDto>>> Update(
        string id, [FromBody] VendLedgerAccountsDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _validator.ValidateAsync(dto, cancellationToken);
        return result.IsValid ? await base.Update(id, dto, cancellationToken)
            : BadRequest(APIResponse<VendLedgerAccountsDto>.Fail(result.Errors[0].ErrorMessage));
    }
}
