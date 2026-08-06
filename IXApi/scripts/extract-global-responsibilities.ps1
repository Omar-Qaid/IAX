param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($RepositoryRoot)
$globalRoot = [IO.Path]::GetFullPath((Join-Path $root 'Domains\Global'))

if (-not $globalRoot.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Resolved Global path must remain inside the repository root.'
}
if (-not (Test-Path -LiteralPath $globalRoot)) {
    throw "Global source directory not found: $globalRoot"
}

function Move-Tree([string]$RelativeSource, [string]$RelativeDestination) {
    $source = [IO.Path]::GetFullPath((Join-Path $root $RelativeSource))
    $destination = [IO.Path]::GetFullPath((Join-Path $root $RelativeDestination))
    if (-not $source.StartsWith($root, [StringComparison]::OrdinalIgnoreCase) -or
        -not $destination.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Move must remain inside repository: $RelativeSource -> $RelativeDestination"
    }
    if (-not (Test-Path -LiteralPath $source)) { return }
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
    Get-ChildItem -LiteralPath $source -Recurse -File | ForEach-Object {
        $relative = $_.FullName.Substring($source.Length).TrimStart('\')
        $target = Join-Path $destination $relative
        $targetDirectory = Split-Path -Parent $target
        New-Item -ItemType Directory -Path $targetDirectory -Force | Out-Null
        if (Test-Path -LiteralPath $target) { throw "Move conflict: $target" }
        Move-Item -LiteralPath $_.FullName -Destination $target
    }
}

function Move-One([string]$RelativeSource, [string]$RelativeDestination) {
    $source = [IO.Path]::GetFullPath((Join-Path $root $RelativeSource))
    $destination = [IO.Path]::GetFullPath((Join-Path $root $RelativeDestination))
    if (-not $source.StartsWith($root, [StringComparison]::OrdinalIgnoreCase) -or
        -not $destination.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Move must remain inside repository: $RelativeSource -> $RelativeDestination"
    }
    if (-not (Test-Path -LiteralPath $source)) { return }
    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
    if (Test-Path -LiteralPath $destination) { throw "Move conflict: $destination" }
    Move-Item -LiteralPath $source -Destination $destination
}

# API host concerns
Move-Tree 'Domains\Global\Middleware' 'Api\Middleware'
Move-Tree 'Domains\Global\ActionFilters' 'Api\Filters'
Move-Tree 'Domains\Global\Controllers' 'Api\Controllers'

# Shared kernel and application contracts
Move-Tree 'Domains\Global\DTOs' 'Shared\Application\Contracts'
Move-Tree 'Domains\Global\Attributes' 'Shared\Application\Attributes'
Move-Tree 'Domains\Global\Validators' 'Shared\Application\Validation'
Move-Tree 'Domains\Global\ExpressionBuilder' 'Shared\Application\Querying'
Move-Tree 'Domains\Global\Events' 'Shared\Domain\Events'
Move-Tree 'Domains\Global\Models\Abstraction' 'Shared\Domain\Entities'
Move-Tree 'Domains\Global\Services\ValueConverter' 'Shared\Application\Conversion'

# Infrastructure
Move-Tree 'Domains\Global\Contexts' 'Infrastructure\Persistence'
Move-Tree 'Domains\Global\Repositories' 'Infrastructure\Persistence\Repositories'
Move-Tree 'Domains\Global\Services\BaseService' 'Infrastructure\Persistence\Services'
Move-Tree 'Domains\Global\Services\CacheService' 'Infrastructure\Caching'
Move-Tree 'Domains\Global\Services\CurrentUserService' 'Infrastructure\Identity'
Move-Tree 'Domains\Global\Services\DatabaseSeederService' 'Infrastructure\Persistence\Seeding'
Move-Tree 'Domains\Global\Services\FileService' 'Infrastructure\Files'
Move-Tree 'Domains\Global\Realtime' 'Infrastructure\Realtime'
Move-Tree 'Domains\Global\Extensions' 'Bootstrap\Extensions'
Move-One 'Domains\Global\Configurations\BaseConfiguration.cs' 'Infrastructure\Persistence\Configuration\BaseConfiguration.cs'
Move-One 'Domains\Global\Models\SysDataSeedLog.cs' 'Infrastructure\Persistence\Seeding\Entities\SysDataSeedLog.cs'

# Communication module
Move-Tree 'Domains\Global\Features\Notification' 'Modules\Communication\Notifications'
Move-Tree 'Domains\Global\Services\NotificationService' 'Modules\Communication\Notifications\Services'
Move-Tree 'Domains\Global\Models\Notification' 'Modules\Communication\Notifications\Entities'
Move-One 'Domains\Global\Configurations\SysNotificationConfiguration.cs' 'Modules\Communication\Notifications\Configuration\SysNotificationConfiguration.cs'
Move-Tree 'Domains\Global\Features\Chat' 'Modules\Communication\Chat'
Move-Tree 'Domains\Global\Services\ChatService' 'Modules\Communication\Chat\Services'
Move-Tree 'Domains\Global\Models\Chat' 'Modules\Communication\Chat\Entities'

# Administration module
Move-Tree 'Domains\Global\Features\AuditLog' 'Modules\Administration\AuditLogs'
Move-Tree 'Domains\Global\Services\AuditService' 'Modules\Administration\AuditLogs\Services'
Move-One 'Domains\Global\Models\SysAuditLog.cs' 'Modules\Administration\AuditLogs\Entities\SysAuditLog.cs'
Move-One 'Domains\Global\Models\SysExceptionLog.cs' 'Modules\Administration\AuditLogs\Entities\SysExceptionLog.cs'
Move-Tree 'Domains\Global\Features\BackgroundJob' 'Modules\Administration\BackgroundJobs'
Move-Tree 'Domains\Global\Services\BackgroundJobs' 'Modules\Administration\BackgroundJobs\Services'
Move-Tree 'Domains\Global\Models\BackgroundJob' 'Modules\Administration\BackgroundJobs\Entities'
Move-One 'Domains\Global\Configurations\SysBackgroundJobConfiguration.cs' 'Modules\Administration\BackgroundJobs\Configuration\SysBackgroundJobConfiguration.cs'
Move-Tree 'Domains\Global\Features\DataManagement' 'Modules\Administration\DataManagement'
Move-Tree 'Domains\Global\Services\DataManagement' 'Modules\Administration\DataManagement\Services'
Move-Tree 'Domains\Global\Services\Providers' 'Modules\Administration\DataManagement\Providers'
Move-One 'Domains\Global\Models\SysImportResult.cs' 'Modules\Administration\DataManagement\Contracts\SysImportResult.cs'
Move-Tree 'Domains\Global\Features\NumberSequence' 'Modules\Administration\NumberSequences'
Move-Tree 'Domains\Global\Features\Settings' 'Modules\Administration\Settings'

# Misplaced Organization entity and composition root
Move-One 'Domains\Global\Models\HcmWorker.cs' 'Modules\Organization\Features\Employee\Entities\HcmWorker.cs'
Move-One 'Domains\Global\GlobalDependencyInjection.cs' 'Bootstrap\GlobalDependencyInjection.cs'

$replacements = [ordered]@{
    'HCMAPIs.Domains.Global.Services.NotificationService' = 'IAX.IXApi.Modules.Communication.Notifications.Services'
    'HCMAPIs.Domains.Global.Models.Notification' = 'IAX.IXApi.Modules.Communication.Notifications.Entities'
    'HCMAPIs.Domains.Global.Features.Notification' = 'IAX.IXApi.Modules.Communication.Notifications'
    'HCMAPIs.Domains.Global.Services.ChatService' = 'IAX.IXApi.Modules.Communication.Chat.Services'
    'HCMAPIs.Domains.Global.Models.Chat' = 'IAX.IXApi.Modules.Communication.Chat.Entities'
    'HCMAPIs.Domains.Global.Features.Chat' = 'IAX.IXApi.Modules.Communication.Chat'
    'HCMAPIs.Domains.Global.Services.BackgroundJobs' = 'IAX.IXApi.Modules.Administration.BackgroundJobs.Services'
    'HCMAPIs.Domains.Global.Models.BackgroundJob' = 'IAX.IXApi.Modules.Administration.BackgroundJobs.Entities'
    'HCMAPIs.Domains.Global.Features.BackgroundJob' = 'IAX.IXApi.Modules.Administration.BackgroundJobs'
    'HCMAPIs.Domains.Global.Services.AuditService' = 'IAX.IXApi.Modules.Administration.AuditLogs.Services'
    'HCMAPIs.Domains.Global.Features.AuditLog' = 'IAX.IXApi.Modules.Administration.AuditLogs'
    'HCMAPIs.Domains.Global.Services.DataManagement' = 'IAX.IXApi.Modules.Administration.DataManagement.Services'
    'HCMAPIs.Domains.Global.Services.Providers' = 'IAX.IXApi.Modules.Administration.DataManagement.Providers'
    'HCMAPIs.Domains.Global.Features.DataManagement' = 'IAX.IXApi.Modules.Administration.DataManagement'
    'HCMAPIs.Domains.Global.Features.NumberSequence' = 'IAX.IXApi.Modules.Administration.NumberSequences'
    'HCMAPIs.Domains.Global.Features.Settings' = 'IAX.IXApi.Modules.Administration.Settings'
    'HCMAPIs.Domains.Global.Services.DatabaseSeeder' = 'IAX.IXApi.Infrastructure.Persistence.Seeding'
    'HCMAPIs.Domains.Global.Services.FileService' = 'IAX.IXApi.Infrastructure.Files'
    'HCMAPIs.Domains.Global.Services.CacheService' = 'IAX.IXApi.Infrastructure.Caching'
    'HCMAPIs.Domains.Global.Services.CurrentUserService' = 'IAX.IXApi.Infrastructure.Identity'
    'HCMAPIs.Domains.Global.Services.BaseService' = 'IAX.IXApi.Infrastructure.Persistence.Services'
    'HCMAPIs.Domains.Global.Repositories' = 'IAX.IXApi.Infrastructure.Persistence.Repositories'
    'HCMAPIs.Domains.Global.Contexts' = 'IAX.IXApi.Infrastructure.Persistence'
    'HCMAPIs.Domains.Global.Realtime' = 'IAX.IXApi.Infrastructure.Realtime'
    'HCMAPIs.Domains.Global.DTOs' = 'IAX.IXApi.Shared.Application.Contracts'
    'HCMAPIs.Domains.Global.Attributes' = 'IAX.IXApi.Shared.Application.Attributes'
    'HCMAPIs.Domains.Global.Validators' = 'IAX.IXApi.Shared.Application.Validation'
    'HCMAPIs.Domains.Global.ExpressionBuilder' = 'IAX.IXApi.Shared.Application.Querying'
    'HCMAPIs.Domains.Global.Events' = 'IAX.IXApi.Shared.Domain.Events'
    'HCMAPIs.Domains.Global.Models.Abstraction' = 'IAX.IXApi.Shared.Domain.Entities'
    'HCMAPIs.Domains.Global.Services.ValueConverter' = 'IAX.IXApi.Shared.Application.Conversion'
    'HCMAPIs.Domains.Global.Middleware' = 'IAX.IXApi.Api.Middleware'
    'HCMAPIs.Domains.Global.ActionFilters' = 'IAX.IXApi.Api.Filters'
    'HCMAPIs.Domains.Global.Controllers' = 'IAX.IXApi.Api.Controllers'
    'HCMAPIs.Domains.Global.Extensions' = 'IAX.IXApi.Bootstrap.Extensions'
}

$utf8 = [Text.UTF8Encoding]::new($false)
Get-ChildItem -LiteralPath $root -Recurse -File -Filter '*.cs' |
    Where-Object { $_.FullName -notmatch '\\(bin|obj|\.dotnet)\\' } |
    ForEach-Object {
        $path = $_.FullName
        $content = [IO.File]::ReadAllText($path)
        $updated = $content
        foreach ($replacement in $replacements.GetEnumerator()) {
            $updated = $updated.Replace($replacement.Key, $replacement.Value)
        }

        # Namespace ownership for files that formerly shared broad Global namespaces.
        if ($path -like '*\Modules\Organization\Features\Employee\Entities\HcmWorker.cs') {
            $updated = $updated.Replace('namespace HCMAPIs.Domains.Global.Models', 'namespace IAX.IXApi.Modules.Organization.Features.Employee.Entities')
        }
        elseif ($path -like '*\Modules\Administration\AuditLogs\Entities\*') {
            $updated = $updated.Replace('namespace HCMAPIs.Domains.Global.Models', 'namespace IAX.IXApi.Modules.Administration.AuditLogs.Entities')
        }
        elseif ($path -like '*\Infrastructure\Persistence\Seeding\Entities\*') {
            $updated = $updated.Replace('namespace HCMAPIs.Domains.Global.Models', 'namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Entities')
        }
        elseif ($path -like '*\Modules\Administration\DataManagement\Contracts\*') {
            $updated = $updated.Replace('namespace HCMAPIs.Domains.Global.Models', 'namespace IAX.IXApi.Modules.Administration.DataManagement.Contracts')
        }
        elseif ($path -like '*\Infrastructure\Persistence\Configuration\*') {
            $updated = $updated.Replace('namespace HCMAPIs.Domains.Global.Configurations', 'namespace IAX.IXApi.Infrastructure.Persistence.Configuration')
        }
        elseif ($path -like '*\Modules\Communication\Notifications\Configuration\*') {
            $updated = $updated.Replace('namespace HCMAPIs.Domains.Global.Configurations', 'namespace IAX.IXApi.Modules.Communication.Notifications.Configuration')
        }
        elseif ($path -like '*\Modules\Administration\BackgroundJobs\Configuration\*') {
            $updated = $updated.Replace('namespace HCMAPIs.Domains.Global.Configurations', 'namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Configuration')
        }
        elseif ($path -like '*\Bootstrap\GlobalDependencyInjection.cs') {
            $updated = $updated.Replace('namespace HCMAPIs.Domains.Global', 'namespace IAX.IXApi.Bootstrap')
        }

        if ($updated -ne $content) { [IO.File]::WriteAllText($path, $updated, $utf8) }
    }

# Remove only empty directories under the verified former Global root.
Get-ChildItem -LiteralPath $globalRoot -Recurse -Directory |
    Sort-Object FullName -Descending |
    Where-Object { -not (Get-ChildItem -LiteralPath $_.FullName -Force) } |
    Remove-Item
if (-not (Get-ChildItem -LiteralPath $globalRoot -Force)) { Remove-Item -LiteralPath $globalRoot }
if ((Test-Path -LiteralPath (Join-Path $root 'Domains')) -and
    -not (Get-ChildItem -LiteralPath (Join-Path $root 'Domains') -Force)) {
    Remove-Item -LiteralPath (Join-Path $root 'Domains')
}

Write-Output 'Global responsibilities extracted into Api, Shared, Infrastructure, and modules.'
