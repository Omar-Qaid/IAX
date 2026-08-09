using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    public class NextSequenceRequestDto
    {
        public string EntityName { get; set; } = null!;
        public string? TenantId { get; set; }
    }
}