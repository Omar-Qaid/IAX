using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Inventory.InventTransPosting
{
    [ApiController]
    [Route("api/v1/InventTransPosting")]
    [DomainPermission("Inventory", "Transactions")]
    public sealed class InventTransPostingController(IInventTransPostingService service) : ControllerBase
    {
        [HttpGet("by-invent-trans/{inventTransId}")]
        public async Task<IActionResult> ByInventTransId(string inventTransId, CancellationToken ct)
        {
            var (items, error) = await service.ByInventTransIdAsync(inventTransId, ct);
            return items != null ? Ok(items) : UnprocessableEntity(new { error });
        }

        [HttpGet("by-voucher/{voucher}")]
        public async Task<IActionResult> ByVoucher(string voucher, CancellationToken ct)
        {
            var (items, error) = await service.ByVoucherAsync(voucher, ct);
            return items != null ? Ok(items) : UnprocessableEntity(new { error });
        }
    }
}
