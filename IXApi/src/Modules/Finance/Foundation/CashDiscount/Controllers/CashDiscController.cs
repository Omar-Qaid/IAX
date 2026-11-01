using FluentValidation;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Foundation.CashDiscount;

[ApiController]
[Route("api/v1/[controller]")]
[DomainPermission("AccountsReceivable", "PaymTerm")]
public class CashDiscController : BaseController<CashDisc, CashDiscDto>
{
    private readonly IValidator<CashDiscDto> _validator;

    public CashDiscController(
        ICashDiscService service,
        ILogger<CashDiscController> logger,
        IValidator<CashDiscDto> validator)
        : base(service, logger)
    {
        _validator = validator;
    }

    public override async Task<ActionResult<APIResponse<CashDiscDto>>> Create(
        [FromBody] CashDiscDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _validator.ValidateAsync(dto, cancellationToken);
        return result.IsValid ? await base.Create(dto, cancellationToken)
            : BadRequest(APIResponse<CashDiscDto>.Fail(result.Errors[0].ErrorMessage));
    }

    public override async Task<ActionResult<APIResponse<CashDiscDto>>> Update(
        string id, [FromBody] CashDiscDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _validator.ValidateAsync(dto, cancellationToken);
        return result.IsValid ? await base.Update(id, dto, cancellationToken)
            : BadRequest(APIResponse<CashDiscDto>.Fail(result.Errors[0].ErrorMessage));
    }
}
