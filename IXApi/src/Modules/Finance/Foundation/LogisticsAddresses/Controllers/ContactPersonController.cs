using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;

[ApiController]
[Route("api/v1/[controller]")]
[DomainPermission("Organization", "ContactPersons")]
public class ContactPersonController : BaseController<ContactPerson, ContactPersonDto>
{
    public ContactPersonController(
        IContactPersonService service,
        ILogger<ContactPersonController>? logger = null)
        : base(service, logger ?? NullLogger<ContactPersonController>.Instance)
    {
    }
}
