using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class ValidateRequestParams
    {
        public long ProcessId { get; set; }
        public long? RequestId { get; set; }
        public List<WfRequestDetail> Details { get; set; } = new();
    }
}