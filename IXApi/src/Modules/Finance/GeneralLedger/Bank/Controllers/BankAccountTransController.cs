using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.Bank
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "Bank")]
    public class BankAccountTransController : BaseController<BankAccountTrans, BankAccountTransDto>
    {
        [NonAction]
        public override Task<ActionResult<APIResponse<BankAccountTransDto>>> Create(BankAccountTransDto dto, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Bank movements must be created by posting.");

        [NonAction]
        public override Task<ActionResult<APIResponse<BankAccountTransDto>>> Update(string id, BankAccountTransDto dto, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Posted bank movements cannot be edited.");

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> Delete(string id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Posted bank movements cannot be deleted.");

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<BankAccountTransDto>>>> CreateRange(IEnumerable<BankAccountTransDto> dtos, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Bank movements must be created by posting.");

        [NonAction]
        public override Task<ActionResult<APIResponse<IEnumerable<BankAccountTransDto>>>> UpdateRange(IEnumerable<BankAccountTransDto> dtos, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Posted bank movements cannot be edited.");

        [NonAction]
        public override Task<ActionResult<APIResponse<bool>>> DeleteRange(IEnumerable<string> ids, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Posted bank movements cannot be deleted.");

        public BankAccountTransController(
            IBankAccountTransService service,
            ILogger<BankAccountTransController>? logger = null)
            : base(service, logger ?? NullLogger<BankAccountTransController>.Instance)
        {
        }
    }
}
