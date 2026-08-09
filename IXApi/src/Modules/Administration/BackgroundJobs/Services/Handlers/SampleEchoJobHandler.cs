using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers
{
    public class SampleEchoJobHandler : ISysBackgroundJobHandler
    {
        private readonly ILogger<SampleEchoJobHandler> _logger;

        public SampleEchoJobHandler(ILogger<SampleEchoJobHandler> logger) => _logger = logger;

        public string JobKey => "SampleEcho";

        public sealed class Payload
        {
            public string Label { get; set; } = "tick";
            public int Iterations { get; set; } = 1;
        }

        public async Task ExecuteAsync(SysBackgroundJobContext context, CancellationToken cancellationToken)
        {
            var payload = context.GetPayload<Payload>() ?? new Payload();
            for (var i = 0; i < Math.Max(1, payload.Iterations); i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _logger.LogInformation("[SampleEcho] {Label} ({Attempt}) iteration {I}", payload.Label, context.Attempt, i + 1);
                await Task.Delay(200, cancellationToken);
            }
            context.Output = $"Echoed '{payload.Label}' x{payload.Iterations}.";
        }
    }
}
