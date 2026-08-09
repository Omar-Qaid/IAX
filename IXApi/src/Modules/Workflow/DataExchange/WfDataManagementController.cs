using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Services;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Modules.Workflow.Activities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IAX.IXApi.Modules.Workflow.DataExchange
{
    [Authorize]
    [EnableRateLimiting("tight")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class WfDataManagementController : ControllerBase
    {
        private readonly ISysDataManagementService _dataManagementService;

        public WfDataManagementController(ISysDataManagementService dataManagementService)
        {
            _dataManagementService = dataManagementService;
        }

        [HttpPost("processes")]
        public async Task<ActionResult<APIResponse<SysImportResult>>> ImportProcesses(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(APIResponse<SysImportResult>.Fail("No file uploaded."));

            using var stream = file.OpenReadStream();
            var result = await _dataManagementService.ImportAsync<WfProcess>(stream);
            return Ok(APIResponse<SysImportResult>.Ok(result, "Processes imported successfully"));
        }

        [HttpPost("activities")]
        public async Task<ActionResult<APIResponse<SysImportResult>>> ImportActivities(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(APIResponse<SysImportResult>.Fail("No file uploaded."));

            using var stream = file.OpenReadStream();
            var result = await _dataManagementService.ImportAsync<WfActivity>(stream);
            return Ok(APIResponse<SysImportResult>.Ok(result, "Activities imported successfully"));
        }

        [HttpGet("processes/template")]
        public async Task<IActionResult> GetProcessTemplate()
        {
            var fileContent = await _dataManagementService.GenerateTemplateAsync<WfProcess>();
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProcessTemplate.xlsx");
        }

        [HttpGet("activities/template")]
        public async Task<IActionResult> GetActivityTemplate()
        {
            var fileContent = await _dataManagementService.GenerateTemplateAsync<WfActivity>();
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ActivityTemplate.xlsx");
        }
        
        [HttpGet("processes/export")]
        public async Task<IActionResult> ExportProcesses()
        {
            var fileContent = await _dataManagementService.ExportAsync<WfProcess>();
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Processes.xlsx");
        }

        [HttpGet("activities/export")]
        public async Task<IActionResult> ExportActivities()
        {
            var fileContent = await _dataManagementService.ExportAsync<WfActivity>();
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Activities.xlsx");
        }
    }
}

