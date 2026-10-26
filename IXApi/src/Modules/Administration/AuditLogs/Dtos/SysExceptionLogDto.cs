using System;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Dtos
{
    public class SysExceptionLogDto
    {
        public long RecId { get; set; }
        public string Severity { get; set; } = "Error";
        public string? ExceptionType { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? StackTrace { get; set; }
        public string? Source { get; set; }
        public string? HttpMethod { get; set; }
        public string? Path { get; set; }
        public int? StatusCode { get; set; }
        public string? QueryString { get; set; }
        public string? RequestPath { get; set; }
        public string? UserName { get; set; }
        public string? ClientIpMasked { get; set; }
        public string? UserAgent { get; set; }
        public string? RequestId { get; set; }
        public string? CorrelationId { get; set; }
        public string? TraceId { get; set; }
        public string? SpanId { get; set; }
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
    }
}
