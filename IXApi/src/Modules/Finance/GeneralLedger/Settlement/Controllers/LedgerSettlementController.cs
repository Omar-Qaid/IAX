using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Settlement
{
    [ApiController]
    [Route("api/v1/LedgerSettlement")]
    [DomainPermission("GeneralLedger", "Journals")]
    public sealed class LedgerSettlementController(ILedgerSettlementService service) : ControllerBase
    {
        [HttpPost]
        [DomainPermission("GeneralLedger", "Journals", "Edit")]
        public async Task<IActionResult> Settle(LedgerSettlementRequestDto input, CancellationToken ct)
        {
            var (result, error) = await service.SettleAsync(input, ct);
            return result != null ? Ok(result) : UnprocessableEntity(new { error });
        }

        [HttpPost("{settleId}/reverse")]
        [DomainPermission("GeneralLedger", "Journals", "Edit")]
        public async Task<IActionResult> Reverse(string settleId, CancellationToken ct)
        {
            var (result, error) = await service.ReverseAsync(settleId, ct);
            if (result != null) return Ok(result);
            return error == null ? NotFound() : UnprocessableEntity(new { error });
        }

        [HttpGet("open-items/{mainAccountId}")]
        public async Task<IActionResult> OpenItems(string mainAccountId, CancellationToken ct)
        {
            var (items, error) = await service.OpenItemsAsync(mainAccountId, ct);
            return items != null
                ? Ok(new { items, scope = "Posted entries not covered by a standing settlement; balances are unaffected by settlement." })
                : UnprocessableEntity(new { error });
        }
    }
}
