using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [DomainPermission("AccountsReceivable", "PriceDiscAdmTables")]
    public class PriceDiscAdmTableController : BaseController<PriceDiscAdmTable, PriceDiscAdmTableDto>
    {
        private readonly PriceDisc.PriceDiscJournalPostingService _posting;

        public PriceDiscAdmTableController(IPriceDiscAdmTableService service,
            ILogger<PriceDiscAdmTableController> logger, PriceDisc.PriceDiscJournalPostingService posting)
            : base(service, logger)
        {
            _posting = posting;
        }

        [HttpPost("{recId:long}/validate")]
        public async Task<IActionResult> Validate(long recId, CancellationToken cancellationToken)
        {
            var errors = await _posting.ValidateAsync(recId, cancellationToken);
            if (errors == null) return NotFound(APIResponse<object>.Fail("Trade agreement journal was not found."));
            return errors.Count == 0
                ? Ok(APIResponse<object>.Ok(new { valid = true, errors }))
                : UnprocessableEntity(APIResponse<object>.Ok(new { valid = false, errors }));
        }

        [HttpPost("{recId:long}/post")]
        public async Task<IActionResult> Post(long recId, CancellationToken cancellationToken)
        {
            var result = await _posting.PostAsync(recId, cancellationToken);
            return result.Status switch
            {
                PriceDisc.PriceDiscJournalPostStatus.NotFound => NotFound(APIResponse<object>.Fail("Trade agreement journal was not found.")),
                PriceDisc.PriceDiscJournalPostStatus.AlreadyPosted => UnprocessableEntity(APIResponse<object>.Fail("The trade agreement journal is already posted.")),
                PriceDisc.PriceDiscJournalPostStatus.Invalid => UnprocessableEntity(APIResponse<object>.Ok(new { valid = false, errors = result.Errors! })),
                _ => Ok(APIResponse<object>.Ok(new { posted = true, publishedRules = result.PublishedRules }))
            };
        }
    }
}
