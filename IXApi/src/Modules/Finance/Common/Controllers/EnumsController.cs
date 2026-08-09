using Microsoft.AspNetCore.Mvc;

namespace IAX.IXApi.Modules.Finance.Common.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class EnumsController : ControllerBase
{
    [HttpGet("{enumName}")]
    public IActionResult GetEnumValues(string enumName)
    {
        var enumType = typeof(DetailSummaryPosting).Assembly.GetTypes()
            .FirstOrDefault(type => type.IsEnum && type.Name.Equals(enumName, StringComparison.OrdinalIgnoreCase));

        return enumType is null
            ? NotFound($"Enum {enumName} not found.")
            : Ok(Enum.GetNames(enumType));
    }
}
