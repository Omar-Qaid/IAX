using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Entities
{
    [Table("SysExceptionLogs")]
    public class SysExceptionLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RecId { get; set; }

        public string Severity { get; set; } = "Error";
        public string? ExceptionType { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? StackTrace { get; set; }
        public string? Source { get; set; }

        // HTTP
        public string? HttpMethod { get; set; }
        public string? Path { get; set; }
        public int? StatusCode { get; set; }
        public string? QueryString { get; set; }
        public string? RequestPath { get; set; }

        // هوية العميل
        public string? UserName { get; set; }
        public string? ClientIpMasked { get; set; }
        public string? UserAgent { get; set; }

        // تتبع
        public string? RequestId { get; set; }
        public string? CorrelationId { get; set; }
        public string? TraceId { get; set; }
        public string? SpanId { get; set; }

        // مسار MVC
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }

        // سياق بيئي
        public string? Tags { get; set; }
        public string? Environment { get; set; }
        public string? AppVersion { get; set; }
        public string? Server { get; set; }

        // مقتطفات آمنة من الطلب
        public string? RequestHeaders { get; set; }
        public string? RequestBodyPreview { get; set; }
        public long? RequestContentLength { get; set; }
        public long? ElapsedMs { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = default!;
    }
}

