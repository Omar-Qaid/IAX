using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IAX.IXApi.Modules.Administration.AuditLogs.Services
{
    public sealed class PendingAudit
    {
        public SysAuditLog Log { get; set; } = default!;
        public EntityEntry Entry { get; set; } = default!;
        public string? PkNames { get; set; }
    }
}
