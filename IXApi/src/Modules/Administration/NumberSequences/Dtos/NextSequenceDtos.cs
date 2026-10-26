namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    public class NextSequenceRequestDto
    {
        public string EntityName { get; set; } = null!;
    }

    public class NextSequenceResultDto
    {
        public string EntityName { get; set; } = null!;
        public long Value { get; set; }
        public string Code { get; set; } = null!;
    }

    public class ResetSequenceRequestDto
    {
        public long? NextValue { get; set; }
    }
}
