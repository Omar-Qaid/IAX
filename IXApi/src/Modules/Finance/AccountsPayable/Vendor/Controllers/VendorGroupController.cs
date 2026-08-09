using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.AccountsPayable
{
    public class VendorGroupController : BaseController<VendGroup, VendorGroupDto>
    {
        public VendorGroupController(IBaseService<VendGroup> service, ILogger<VendorGroupController> logger)
            : base(service, logger)
        {
        }
    }
}
