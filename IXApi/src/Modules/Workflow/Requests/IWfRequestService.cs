using IAX.IXApi.Infrastructure.Persistence.Services;


namespace IAX.IXApi.Modules.Workflow.Requests
{
    public interface IWfRequestService : IBaseService<WfRequest>
    {
        Task<DynamicRequestFormDto?> GetFormDefinitionAsync(long processId, CancellationToken cancellationToken = default);
        Task<SubmitDynamicRequestResultDto> SubmitDynamicAsync(SubmitDynamicRequestDto submission, CancellationToken cancellationToken = default);
    }
}
