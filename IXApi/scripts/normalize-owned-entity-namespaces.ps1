param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($RepositoryRoot)
$utf8 = [Text.UTF8Encoding]::new($false)
$sharedImports = @'
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Features.Employee.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
'@.TrimEnd()

Get-ChildItem -LiteralPath $root -Recurse -File -Filter '*.cs' |
    Where-Object { $_.FullName -notmatch '\\(bin|obj|\.dotnet)\\' } |
    ForEach-Object {
        $path = $_.FullName
        $content = [IO.File]::ReadAllText($path)
        $updated = $content

        # Replace the former catch-all import with explicit ownership namespaces.
        $updated = $updated.Replace('using HCMAPIs.Domains.Global.Models;', $sharedImports)

        # Fully-qualified legacy references.
        $updated = $updated.Replace('HCMAPIs.Domains.Global.Models.HcmWorker', 'IAX.IXApi.Modules.Organization.Features.Employee.Entities.HcmWorker')
        $updated = $updated.Replace('HCMAPIs.Domains.Global.Models.SysAuditLog', 'IAX.IXApi.Modules.Administration.AuditLogs.Entities.SysAuditLog')
        $updated = $updated.Replace('HCMAPIs.Domains.Global.Models.SysExceptionLog', 'IAX.IXApi.Modules.Administration.AuditLogs.Entities.SysExceptionLog')
        $updated = $updated.Replace('HCMAPIs.Domains.Global.Models.SysDataSeedLog', 'IAX.IXApi.Infrastructure.Persistence.Seeding.Entities.SysDataSeedLog')
        $updated = $updated.Replace('HCMAPIs.Domains.Global.Models.SysImportResult', 'IAX.IXApi.Modules.Administration.DataManagement.Contracts.SysImportResult')
        $updated = $updated.Replace('HCMAPIs.Domains.Global.Models.FiscalCalendarPeriod', 'IAX.IXApi.Modules.ERP.Entities.FiscalCalendarPeriod')
        $updated = $updated.Replace('HCMAPIs.Domains.Global.Models.FiscalCalendarYear', 'IAX.IXApi.Modules.ERP.Entities.FiscalCalendarYear')
        $updated = $updated.Replace('HCMAPIs.Domains.Global.Models.FiscalCalendar', 'IAX.IXApi.Modules.ERP.Entities.FiscalCalendar')
        $updated = $updated.Replace('HCMAPIs.Domains.Global.Models.InventTable', 'IAX.IXApi.Modules.ERP.Entities.InventTable')
        $updated = $updated.Replace('HCMAPIs.Domains.Global.Models.MainAccount', 'IAX.IXApi.Modules.ERP.Entities.MainAccount')
        $updated = $updated.Replace('Models.Abstraction.ISpecification', 'IAX.IXApi.Shared.Domain.Entities.ISpecification')
        $updated = $updated.Replace('Models.BackgroundJob.SysJobStatus', 'IAX.IXApi.Modules.Administration.BackgroundJobs.Entities.SysJobStatus')
        $updated = $updated.Replace('DTOs.SysAuditLogDto', 'IAX.IXApi.Shared.Application.Contracts.SysAuditLogDto')

        # Assign broad legacy entity namespaces to their actual owner.
        if ($path -like '*\Modules\ERP\*') {
            $updated = $updated.Replace('namespace HCMAPIs.Domains.Global.Models', 'namespace IAX.IXApi.Modules.ERP.Entities')
        }
        elseif ($path -like '*\Shared\Domain\Entities\*') {
            $updated = $updated.Replace('namespace HCMAPIs.Domains.Global.Models', 'namespace IAX.IXApi.Shared.Domain.Entities')
        }
        elseif ($path -like '*\Modules\Communication\Notifications\Entities\*') {
            $updated = $updated.Replace('namespace HCMAPIs.Domains.Global.Models', 'namespace IAX.IXApi.Modules.Communication.Notifications.Entities')
        }

        $updated = $updated.Replace('HCMAPIs.Domains.Global.Services.Utilize', 'IAX.IXApi.Shared.Application.Conversion')
        $updated = $updated.Replace('HCMAPIs.Domains.Global;', 'IAX.IXApi.Shared.Application.Querying;')
        $updated = $updated.Replace('namespace HCMAPIs.Domains.Global', 'namespace IAX.IXApi.Shared.Application.Querying')
        $updated = $updated.Replace('HCMAPIs.Domains.Global.DataGrid', 'IAX.IXApi.Shared.Application.Querying.DataGrid')

        if ($updated -ne $content) { [IO.File]::WriteAllText($path, $updated, $utf8) }
    }

Write-Output 'Entity and shared namespaces normalized.'
