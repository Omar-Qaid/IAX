using IAX.IXApi.Shared.Application.Batch;

namespace IAX.IXApi.Modules.Workflow.Jobs;

/// <summary>Extension point for submitting process requests from the generic batch worker.</summary>
public sealed class WorkflowRequestSubmissionBatchService : BatchService
{
    public const string ServiceKey = "WorkflowRequestSubmission";

    public override Task<BatchExecutionResult> ProcessAsync(BatchExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // TODO: Add your process-request submission logic here.
        // 1. Read your process ID and input values from context.ParametersJson.
        // 2. Validate the execution account and establish its authorized company scope.
        // 3. Call the normal request submission application service; do not insert
        //    request/assignment rows directly or bypass form validation and routing.
        // 4. Prevent duplicate submissions when a schedule runs again or is retried.
        // 5. Return the actual processed/success/failed counts.
        // Add required dependencies through constructor injection.

        // Fail explicitly until implemented; never report an unsubmitted request as successful.
        throw new NotImplementedException(
            "Add request submission logic to WorkflowRequestSubmissionBatchService.ProcessAsync before enabling this job.");
    }
}
