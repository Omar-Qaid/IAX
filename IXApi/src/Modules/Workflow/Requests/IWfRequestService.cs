using IAX.IXApi.Infrastructure.Persistence.Services;


namespace IAX.IXApi.Modules.Workflow.Requests
{
    public interface IWfRequestService : IBaseService<WfRequest>
    {
        Task<IReadOnlyList<WfRequestDto>> GetRequestListAsync(CancellationToken cancellationToken = default);
        Task<DynamicRequestFormDto?> GetFormDefinitionAsync(long processId, CancellationToken cancellationToken = default);
        Task<MailRequestDetailsDto?> GetMailDetailsAsync(long requestId, CancellationToken cancellationToken = default);
        Task<SubmitDynamicRequestResultDto> SubmitDynamicAsync(SubmitDynamicRequestDto submission, CancellationToken cancellationToken = default);
    }
}
