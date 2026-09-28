using IAX.IXApi.Infrastructure.Persistence.Services;


namespace IAX.IXApi.Modules.Workflow.Requests
{
    public interface IWfRequestService : IBaseService<WfRequest>
    {
        Task<IReadOnlyList<WfRequestDto>> GetRequestListAsync(CancellationToken cancellationToken = default);
        Task<bool> CanAccessRequestAsync(long requestId, CancellationToken cancellationToken = default);
        Task<DynamicRequestFormDto?> GetFormDefinitionAsync(long processId, CancellationToken cancellationToken = default);
        Task<DynamicRequestLookupPageDto?> GetReferenceOptionsAsync(long processId, long requestControlId,
            int pageNumber, int pageSize, string? search, CancellationToken cancellationToken = default);
        IReadOnlyList<DynamicReferenceFilterFieldDto>? GetReferenceFilterFields(string referenceType);
        Task<DynamicReferenceFilterValuePageDto?> GetReferenceFilterValuesAsync(string referenceType,
            string field, int pageNumber, int pageSize, string? search, CancellationToken cancellationToken = default);
        Task<MailRequestDetailsDto?> GetMailDetailsAsync(long requestId, CancellationToken cancellationToken = default);
        Task SaveMailActivityControlsAsync(long requestId, SaveMailActivityControlsDto submission, CancellationToken cancellationToken = default);
        Task<SubmitDynamicRequestResultDto> SubmitDynamicAsync(SubmitDynamicRequestDto submission, CancellationToken cancellationToken = default);
        Task<List<ValidationResult>> ValidateSubmissionAsync(SubmitDynamicRequestDto submission, CancellationToken cancellationToken = default);
    }
}
