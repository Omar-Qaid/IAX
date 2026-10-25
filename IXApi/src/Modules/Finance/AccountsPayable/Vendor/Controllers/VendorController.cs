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
    [DomainPermission("AccountsPayable", "Vendors")]
    public class VendorController : BaseController<VendTable, VendorDto>
    {
        private readonly IValidator<VendorDto> _validator;

        public VendorController(
            IVendorService service,
            ILogger<VendorController> logger,
            IValidator<VendorDto> validator)
            : base(service, logger)
        {
            _validator = validator;
        }

        public override async Task<ActionResult<APIResponse<VendorDto>>> Create(
            [FromBody] VendorDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _validator.ValidateAsync(dto, cancellationToken);
            return result.IsValid ? await base.Create(dto, cancellationToken)
                : BadRequest(APIResponse<VendorDto>.Fail(result.Errors[0].ErrorMessage));
        }

        public override async Task<ActionResult<APIResponse<VendorDto>>> Update(
            string id, [FromBody] VendorDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _validator.ValidateAsync(dto, cancellationToken);
            return result.IsValid ? await base.Update(id, dto, cancellationToken)
                : BadRequest(APIResponse<VendorDto>.Fail(result.Errors[0].ErrorMessage));
        }
    }
}
