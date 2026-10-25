using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FluentValidation;
using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("AccountsPayable", "VendorGroups")]
    public class VendorGroupController : BaseController<VendGroup, VendorGroupDto>
    {
        private readonly IValidator<VendorGroupDto> _validator;

        public VendorGroupController(IVendorGroupService service, ILogger<VendorGroupController> logger,
            IValidator<VendorGroupDto>? validator = null)
            : base(service, logger)
        {
            _validator = validator ?? new VendorGroupDtoValidator();
        }

        public override async Task<ActionResult<APIResponse<VendorGroupDto>>> Create(
            [FromBody] VendorGroupDto dto, CancellationToken cancellationToken = default)
        {
            var validation = await _validator.ValidateAsync(dto, cancellationToken);
            return validation.IsValid
                ? await base.Create(dto, cancellationToken)
                : BadRequest(APIResponse<VendorGroupDto>.Fail(validation.Errors[0].ErrorMessage));
        }

        public override async Task<ActionResult<APIResponse<VendorGroupDto>>> Update(
            string id, [FromBody] VendorGroupDto dto, CancellationToken cancellationToken = default)
        {
            var validation = await _validator.ValidateAsync(dto, cancellationToken);
            return validation.IsValid
                ? await base.Update(id, dto, cancellationToken)
                : BadRequest(APIResponse<VendorGroupDto>.Fail(validation.Errors[0].ErrorMessage));
        }
    }
}
