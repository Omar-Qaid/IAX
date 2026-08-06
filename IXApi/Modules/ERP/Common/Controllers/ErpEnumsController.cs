using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace IAX.IXApi.Modules.ERP.Common.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ErpEnumsController : ControllerBase
    {
        [HttpGet("{enumName}")]
        public IActionResult GetEnumValues(string enumName)
        {
            // Find the enum type in the ERP.Common assembly
            var enumType = typeof(IAX.IXApi.Modules.ERP.Common.DetailSummaryPosting).Assembly.GetTypes()
                .FirstOrDefault(t => t.IsEnum && t.Name.Equals(enumName, StringComparison.OrdinalIgnoreCase));

            if (enumType == null)
            {
                return NotFound($"Enum {enumName} not found.");
            }

            var values = Enum.GetNames(enumType).ToList();
            return Ok(values);
        }
    }
}
