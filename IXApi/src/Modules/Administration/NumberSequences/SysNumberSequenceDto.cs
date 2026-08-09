using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    public class SysNumberSequenceDto : EntityDto<int>
    {
        public string EntityName { get; set; } = null!;
        public string? Prefix { get; set; }
        public string? Suffix { get; set; }
        public long SmallestValue { get; set; } = 1;
        public long LargestValue { get; set; } = 999999999;
        public long NextValue { get; set; } = 1;
        public int Step { get; set; } = 1;
        public string FormatPattern { get; set; } = "{PREFIX}-{SEQ}";
        public int PaddingLength { get; set; } = 5;
        public SequenceResetCycle ResetCycle { get; set; }
        public DateTime? LastResetAt { get; set; }
        public string? TenantId { get; set; }
    }
}