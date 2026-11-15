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
    private readonly IContactPersonService _contacts;

    public ContactPersonController(
        IContactPersonService service,
        ILogger<ContactPersonController>? logger = null)
        : base(service, logger ?? NullLogger<ContactPersonController>.Instance)
    {
        _contacts = service;
    }

    [HttpGet("accounts/{kind}/{account}")]
    public async Task<IActionResult> ListAccount(string kind, string account, CancellationToken ct)
    {
        var rows = await _contacts.ListAccountAsync(kind, account, ct);
        return rows == null ? NotFound() : Ok(rows);
    }

    [HttpPost("accounts/{kind}/{account}")]
    public async Task<IActionResult> CreateAccountContact(string kind, string account, AccountContactInput input, CancellationToken ct)
    {
        var id = await _contacts.SaveAccountAsync(kind, account, null, input, ct);
        return id == null ? NotFound() : Ok(id);
    }

    [HttpPut("accounts/{kind}/{account}/{id:long}")]
    public async Task<IActionResult> EditAccountContact(string kind, string account, long id, AccountContactInput input, CancellationToken ct)
    {
        var saved = await _contacts.SaveAccountAsync(kind, account, id, input, ct);
        return saved == null ? NotFound() : Ok(saved);
    }
}
