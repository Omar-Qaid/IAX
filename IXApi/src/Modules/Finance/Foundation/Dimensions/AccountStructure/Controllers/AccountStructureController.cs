using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Foundation.Dimensions.AccountStructure.Controllers
{
    [ApiController]
    [Route("api/v1/AccountStructure")]
    [DomainPermission("GeneralLedger", "Dimensions")]
    public sealed class AccountStructureController(IAccountStructureService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> List(CancellationToken ct)
        {
            var (items, error) = await service.ListAsync(ct);
            return items != null ? Ok(items) : UnprocessableEntity(new { error });
        }

        [HttpPost]
        [DomainPermission("GeneralLedger", "Dimensions", "Edit")]
        public async Task<IActionResult> Create(AccountStructureCreateDto input, CancellationToken ct)
        {
            var (result, error) = await service.CreateAsync(input, ct);
            return result != null ? Ok(result) : UnprocessableEntity(new { error });
        }

        /// <summary>Activates a draft so it begins constraining what may post.</summary>
        [HttpPost("{name}/activate")]
        [DomainPermission("GeneralLedger", "Dimensions", "Edit")]
        public async Task<IActionResult> Activate(string name, CancellationToken ct)
        {
            var (result, error) = await service.ActivateAsync(name, ct);
            if (result != null) return Ok(result);
            return error == null ? NotFound() : UnprocessableEntity(new { error });
        }

        [HttpPost("{name}/assign-to-ledger")]
        [DomainPermission("GeneralLedger", "Dimensions", "Edit")]
        public async Task<IActionResult> AssignToLedger(string name, CancellationToken ct)
        {
            var (result, error) = await service.AssignToLedgerAsync(name, ct);
            if (result != null) return Ok(result);
            return error == null ? NotFound() : UnprocessableEntity(new { error });
        }
    }
}
