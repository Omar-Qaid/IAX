using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    public class NextSequenceResultDto
    {
        public string EntityName { get; set; } = null!;
        public long Value { get; set; }
        public string Code { get; set; } = null!;
    }
}