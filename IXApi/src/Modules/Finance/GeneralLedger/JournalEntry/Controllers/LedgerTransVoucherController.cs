using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry
{
    /// <summary>Voucher transactions inquiry over posted accounting. Read only.</summary>
    [ApiController]
    [Route("api/v1/LedgerTransVoucher")]
    [DomainPermission("GeneralLedger", "JournalEntries")]
    public sealed class LedgerTransVoucherController(ILedgerTransVoucherService service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<APIResponse<IReadOnlyList<LedgerTransVoucherRow>>>> Get(
            [FromQuery] string? voucher, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
            CancellationToken ct = default)
        {
            var (rows, error) = await service.GetAsync(voucher, fromDate, toDate, ct);
            return rows != null
                ? Ok(APIResponse<IReadOnlyList<LedgerTransVoucherRow>>.Ok(rows))
                : UnprocessableEntity(APIResponse<IReadOnlyList<LedgerTransVoucherRow>>.Fail(error!));
        }
    }
}
