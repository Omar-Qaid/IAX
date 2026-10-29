using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerParameters
{
    [ApiController]
    [Route("api/v1/LedgerParameters")]
    [DomainPermission("GeneralLedger", "Setup")]
    public sealed class LedgerParametersController(ILedgerParametersService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var (result, error) = await service.GetAsync(ct);
            return result != null ? Ok(result) : UnprocessableEntity(new { error });
        }

        [HttpPut]
        [DomainPermission("GeneralLedger", "Setup", "Edit")]
        public async Task<IActionResult> Update(LedgerParametersDto input, CancellationToken ct)
        {
            var (result, error) = await service.UpdateAsync(input, ct);
            return result != null ? Ok(result) : UnprocessableEntity(new { error });
        }
    }
}
