using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Administration.NumberSequences
{
    public class SysNumberSequenceDto : EntityDto<long>
    {
        public string NumberSequence { get; set; } = null!;
        public string Txt { get; set; } = null!;
        public DateTime? LatestCleanDateTime { get; set; }
        public int? LatestCleanDateTimeTzId { get; set; }
        public int? Lowest { get; set; }
        public int? Highest { get; set; }
        public int? NextRec { get; set; }
        public int? Blocked { get; set; }
        public string Format { get; set; } = null!;
        public int? Continuous { get; set; }
        public int? Cyclic { get; set; }
        public string AnnotatedFormat { get; set; } = null!;
        public int? CleanAtAccess { get; set; }
        public int? InUse { get; set; }
        public int? NoIncrement { get; set; }
        public long? NumberSequenceScope { get; set; }
        public decimal? CleanInterval { get; set; }
        public int? AllowChangeUp { get; set; }
        public int? AllowChangeDown { get; set; }
        public int? Manual { get; set; }
        public int? FetchAheadQty { get; set; }
        public int? FetchAhead { get; set; }
        public long? ModifiedTransactionId { get; set; }
        public long? Partition { get; set; }
    }
}