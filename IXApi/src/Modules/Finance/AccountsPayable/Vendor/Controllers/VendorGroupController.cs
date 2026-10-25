using IAX.IXApi.Api.Controllers;
using FluentValidation;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("AccountsPayable", "VendorGroups")]
    public class VendorGroupController : BaseController<VendGroup, VendorGroupDto>
    {
        private readonly IValidator<VendorGroupDto> _validator;

        public VendorGroupController(
            IVendorGroupService service,
            ILogger<VendorGroupController> logger,
            IValidator<VendorGroupDto> validator)
            : base(service, logger)
        {
            _validator = validator;
        }

        public override async Task<ActionResult<APIResponse<VendorGroupDto>>> Create(
            [FromBody] VendorGroupDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _validator.ValidateAsync(dto, cancellationToken);
            return result.IsValid ? await base.Create(dto, cancellationToken)
                : BadRequest(APIResponse<VendorGroupDto>.Fail(result.Errors[0].ErrorMessage));
        }

        public override async Task<ActionResult<APIResponse<VendorGroupDto>>> Update(
            string id, [FromBody] VendorGroupDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _validator.ValidateAsync(dto, cancellationToken);
            return result.IsValid ? await base.Update(id, dto, cancellationToken)
                : BadRequest(APIResponse<VendorGroupDto>.Fail(result.Errors[0].ErrorMessage));
        }
    }
}
