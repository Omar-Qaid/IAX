using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    public class ResetSequenceRequestDto
    {
        public long? NextValue { get; set; }
    }
}