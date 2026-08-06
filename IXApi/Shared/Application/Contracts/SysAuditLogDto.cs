using System;

namespace IAX.IXApi.Shared.Application.Contracts
{
    public class SysAuditLogDto
    {
        public long RecId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string RecordId { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string Operation { get; set; } = string.Empty;
        public string? ChangedBy { get; set; }
        public DateTime ChangedOn { get; set; }
    }
}

