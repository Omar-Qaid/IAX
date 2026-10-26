// Compatibility forwarding for callers using IAX.IXApi.Modules.Administration.AuditLogs
global using SysAuditLog = IAX.IXApi.Modules.Administration.AuditLogs.Entities.SysAuditLog;
global using SysExceptionLog = IAX.IXApi.Modules.Administration.AuditLogs.Entities.SysExceptionLog;
global using SysAuditLogDto = IAX.IXApi.Modules.Administration.AuditLogs.Dtos.SysAuditLogDto;
global using SysExceptionLogDto = IAX.IXApi.Modules.Administration.AuditLogs.Dtos.SysExceptionLogDto;
global using ISysAuditLogService = IAX.IXApi.Modules.Administration.AuditLogs.Interfaces.ISysAuditLogService;
global using ISysAuditService = IAX.IXApi.Modules.Administration.AuditLogs.Interfaces.ISysAuditService;
global using ISysExceptionLogService = IAX.IXApi.Modules.Administration.AuditLogs.Interfaces.ISysExceptionLogService;
global using SysAuditLogService = IAX.IXApi.Modules.Administration.AuditLogs.Services.SysAuditLogService;
global using SysAuditService = IAX.IXApi.Modules.Administration.AuditLogs.Services.SysAuditService;
global using SysExceptionLogService = IAX.IXApi.Modules.Administration.AuditLogs.Services.SysExceptionLogService;
global using SysAuditLogController = IAX.IXApi.Modules.Administration.AuditLogs.Controllers.SysAuditLogController;
global using SysExceptionLogController = IAX.IXApi.Modules.Administration.AuditLogs.Controllers.SysExceptionLogController;
global using SysAuditLogDtoValidator = IAX.IXApi.Modules.Administration.AuditLogs.Validation.SysAuditLogDtoValidator;
global using SysExceptionLogDtoValidator = IAX.IXApi.Modules.Administration.AuditLogs.Validation.SysExceptionLogDtoValidator;
