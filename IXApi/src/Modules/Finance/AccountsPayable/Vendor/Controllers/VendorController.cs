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
    [DomainPermission("AccountsPayable", "Vendors")]
    public class VendorController : BaseController<VendTable, VendorDto>
    {
        private readonly IValidator<VendorDto> _validator;

        public VendorController(IVendorService service, ILogger<VendorController> logger,
            IValidator<VendorDto>? validator = null)
            : base(service, logger)
        {
            _validator = validator ?? new VendorDtoValidator();
        }

        public override async Task<ActionResult<APIResponse<VendorDto>>> Create(
            [FromBody] VendorDto dto, CancellationToken cancellationToken = default)
        {
            var validation = await _validator.ValidateAsync(dto, cancellationToken);
            return validation.IsValid
                ? await base.Create(dto, cancellationToken)
                : BadRequest(APIResponse<VendorDto>.Fail(validation.Errors[0].ErrorMessage));
        }

        public override async Task<ActionResult<APIResponse<VendorDto>>> Update(
            string id, [FromBody] VendorDto dto, CancellationToken cancellationToken = default)
        {
            var validation = await _validator.ValidateAsync(dto, cancellationToken);
            return validation.IsValid
                ? await base.Update(id, dto, cancellationToken)
                : BadRequest(APIResponse<VendorDto>.Fail(validation.Errors[0].ErrorMessage));
        }
    }
}
