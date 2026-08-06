param([string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot))

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($RepositoryRoot)
$modules = [IO.Path]::GetFullPath((Join-Path $root 'Modules'))
if (-not $modules.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Modules path must stay inside the repository.'
}

function Move-Tree([string]$From, [string]$To) {
    $source = [IO.Path]::GetFullPath((Join-Path $root $From))
    $targetRoot = [IO.Path]::GetFullPath((Join-Path $root $To))
    if (-not $source.StartsWith($root, [StringComparison]::OrdinalIgnoreCase) -or
        -not $targetRoot.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Move outside repository: $From -> $To"
    }
    if (-not (Test-Path -LiteralPath $source)) { return }
    New-Item -ItemType Directory -Path $targetRoot -Force | Out-Null
    Get-ChildItem -LiteralPath $source -Recurse -File | ForEach-Object {
        $relative = $_.FullName.Substring($source.Length).TrimStart('\')
        $target = Join-Path $targetRoot $relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
        if (Test-Path -LiteralPath $target) { throw "Move conflict: $target" }
        Move-Item -LiteralPath $_.FullName -Destination $target
    }
}

$moves = [ordered]@{
    'Modules\Identity\Features\Auth' = 'Modules\Identity\Authentication'
    'Modules\Identity\Features\User' = 'Modules\Identity\Users'
    'Modules\Identity\Features\Role' = 'Modules\Identity\Roles'
    'Modules\Identity\Features\Permission' = 'Modules\Identity\Permissions'
    'Modules\Identity\Features\Impersonation' = 'Modules\Identity\Impersonation'

    'Modules\Organization\Features\Company' = 'Modules\Organization\Companies'
    'Modules\Organization\Features\Department' = 'Modules\Organization\Departments'
    'Modules\Organization\Features\EmployeeCategory' = 'Modules\Organization\EmployeeCategories'
    'Modules\Organization\Features\EmployeeGroup' = 'Modules\Organization\EmployeeGroups'
    'Modules\Organization\Features\EmployeeManager' = 'Modules\Organization\EmployeeManagers'
    'Modules\Organization\Features\ManagementLevel' = 'Modules\Organization\ManagementLevels'
    'Modules\Organization\Features\Nationality' = 'Modules\Organization\Nationalities'
    'Modules\Organization\Features\Occupation' = 'Modules\Organization\Occupations'
    'Modules\Organization\Features\Gender' = 'Modules\Organization\Genders'
    'Modules\Organization\Features\Showroom' = 'Modules\Organization\Showrooms'
    'Modules\Organization\Features\Announcement' = 'Modules\Organization\Announcements'
    'Modules\Organization\Features\Attachment' = 'Modules\Organization\Attachments'
    'Modules\Organization\Features\Employee' = 'Modules\Organization\Employees'

    'Modules\Workflow\Features\Activity' = 'Modules\Workflow\Activities'
    'Modules\Workflow\Features\Category' = 'Modules\Workflow\Categories'
    'Modules\Workflow\Features\Control' = 'Modules\Workflow\Controls'
    'Modules\Workflow\Features\DataManagement' = 'Modules\Workflow\DataExchange'
    'Modules\Workflow\Features\Execution' = 'Modules\Workflow\Execution'
    'Modules\Workflow\Features\Operator' = 'Modules\Workflow\Operators'
    'Modules\Workflow\Features\Performer' = 'Modules\Workflow\Performers'
    'Modules\Workflow\Features\Priority' = 'Modules\Workflow\Priorities'
    'Modules\Workflow\Features\Process' = 'Modules\Workflow\Processes'
    'Modules\Workflow\Features\Request' = 'Modules\Workflow\Requests'
    'Modules\Workflow\Features\Step' = 'Modules\Workflow\Steps'
    'Modules\Workflow\Features\Transition' = 'Modules\Workflow\Transitions'
    'Modules\Workflow\Features\Variable' = 'Modules\Workflow\Variables'

    'Modules\ERP\Foundation\Features\Currency' = 'Modules\ERP\Foundation\Currency'
    'Modules\ERP\Foundation\Features\DefaultDimension' = 'Modules\ERP\Foundation\Dimensions\DefaultDimension'
    'Modules\ERP\Foundation\Features\LedgerDimension' = 'Modules\ERP\Foundation\Dimensions\LedgerDimension'
    'Modules\ERP\Foundation\Features\LegalEntity' = 'Modules\ERP\Foundation\LegalEntities'
    'Modules\ERP\Foundation\Features\LogisticsAddress' = 'Modules\ERP\Foundation\LogisticsAddresses'
    'Modules\ERP\Foundation\Features\PaymentTerm' = 'Modules\ERP\Foundation\PaymentTerms'
    'Modules\ERP\Foundation\Features\PaymentSchedule' = 'Modules\ERP\Foundation\PaymentSchedules'
    'Modules\ERP\Foundation\Features\DeliveryMode' = 'Modules\ERP\Foundation\DeliveryModes'
    'Modules\ERP\Foundation\Features\DeliveryTerm' = 'Modules\ERP\Foundation\DeliveryTerms'
    'Modules\ERP\Foundation\Features\Markup' = 'Modules\ERP\Foundation\Markup'
    'Modules\ERP\Foundation\Features\Tax' = 'Modules\ERP\Foundation\Tax'

    'Modules\ERP\AccountsReceivable\Features' = 'Modules\ERP\AccountsReceivable'
    'Modules\ERP\AccountsPayable\Features' = 'Modules\ERP\AccountsPayable'
    'Modules\ERP\GeneralLedger\Features' = 'Modules\ERP\GeneralLedger'
    'Modules\ERP\InventoryManagement\Features' = 'Modules\ERP\Inventory'
}

foreach ($move in $moves.GetEnumerator()) { Move-Tree $move.Key $move.Value }

$replacements = [ordered]@{
    'IAX.IXApi.Modules.Identity.Features.Auth' = 'IAX.IXApi.Modules.Identity.Authentication'
    'IAX.IXApi.Modules.Identity.Features.User' = 'IAX.IXApi.Modules.Identity.Users'
    'IAX.IXApi.Modules.Identity.Features.Role' = 'IAX.IXApi.Modules.Identity.Roles'
    'IAX.IXApi.Modules.Identity.Features.Permission' = 'IAX.IXApi.Modules.Identity.Permissions'
    'IAX.IXApi.Modules.Identity.Features.Impersonation' = 'IAX.IXApi.Modules.Identity.Impersonation'

    'IAX.IXApi.Modules.Organization.Features.EmployeeCategory' = 'IAX.IXApi.Modules.Organization.EmployeeCategories'
    'IAX.IXApi.Modules.Organization.Features.EmployeeGroup' = 'IAX.IXApi.Modules.Organization.EmployeeGroups'
    'IAX.IXApi.Modules.Organization.Features.EmployeeManager' = 'IAX.IXApi.Modules.Organization.EmployeeManagers'
    'IAX.IXApi.Modules.Organization.Features.ManagementLevel' = 'IAX.IXApi.Modules.Organization.ManagementLevels'
    'IAX.IXApi.Modules.Organization.Features.Company' = 'IAX.IXApi.Modules.Organization.Companies'
    'IAX.IXApi.Modules.Organization.Features.Department' = 'IAX.IXApi.Modules.Organization.Departments'
    'IAX.IXApi.Modules.Organization.Features.Nationality' = 'IAX.IXApi.Modules.Organization.Nationalities'
    'IAX.IXApi.Modules.Organization.Features.Occupation' = 'IAX.IXApi.Modules.Organization.Occupations'
    'IAX.IXApi.Modules.Organization.Features.Gender' = 'IAX.IXApi.Modules.Organization.Genders'
    'IAX.IXApi.Modules.Organization.Features.Showroom' = 'IAX.IXApi.Modules.Organization.Showrooms'
    'IAX.IXApi.Modules.Organization.Features.Announcement' = 'IAX.IXApi.Modules.Organization.Announcements'
    'IAX.IXApi.Modules.Organization.Features.Attachment' = 'IAX.IXApi.Modules.Organization.Attachments'
    'IAX.IXApi.Modules.Organization.Features.Employee' = 'IAX.IXApi.Modules.Organization.Employees'

    'IAX.IXApi.Modules.Workflow.Features.Activity' = 'IAX.IXApi.Modules.Workflow.Activities'
    'IAX.IXApi.Modules.Workflow.Features.Category' = 'IAX.IXApi.Modules.Workflow.Categories'
    'IAX.IXApi.Modules.Workflow.Features.Control' = 'IAX.IXApi.Modules.Workflow.Controls'
    'IAX.IXApi.Modules.Workflow.Features.DataManagement' = 'IAX.IXApi.Modules.Workflow.DataExchange'
    'IAX.IXApi.Modules.Workflow.Features.Execution' = 'IAX.IXApi.Modules.Workflow.Execution'
    'IAX.IXApi.Modules.Workflow.Features.Operator' = 'IAX.IXApi.Modules.Workflow.Operators'
    'IAX.IXApi.Modules.Workflow.Features.Performer' = 'IAX.IXApi.Modules.Workflow.Performers'
    'IAX.IXApi.Modules.Workflow.Features.Priority' = 'IAX.IXApi.Modules.Workflow.Priorities'
    'IAX.IXApi.Modules.Workflow.Features.Process' = 'IAX.IXApi.Modules.Workflow.Processes'
    'IAX.IXApi.Modules.Workflow.Features.Request' = 'IAX.IXApi.Modules.Workflow.Requests'
    'IAX.IXApi.Modules.Workflow.Features.Step' = 'IAX.IXApi.Modules.Workflow.Steps'
    'IAX.IXApi.Modules.Workflow.Features.Transition' = 'IAX.IXApi.Modules.Workflow.Transitions'
    'IAX.IXApi.Modules.Workflow.Features.Variable' = 'IAX.IXApi.Modules.Workflow.Variables'

    'IAX.IXApi.Modules.ERP.Foundation.Features.DefaultDimension' = 'IAX.IXApi.Modules.ERP.Foundation.Dimensions.DefaultDimension'
    'IAX.IXApi.Modules.ERP.Foundation.Features.LedgerDimension' = 'IAX.IXApi.Modules.ERP.Foundation.Dimensions.LedgerDimension'
    'IAX.IXApi.Modules.ERP.Foundation.Features.LegalEntity' = 'IAX.IXApi.Modules.ERP.Foundation.LegalEntities'
    'IAX.IXApi.Modules.ERP.Foundation.Features.LogisticsAddress' = 'IAX.IXApi.Modules.ERP.Foundation.LogisticsAddresses'
    'IAX.IXApi.Modules.ERP.Foundation.Features.PaymentTerm' = 'IAX.IXApi.Modules.ERP.Foundation.PaymentTerms'
    'IAX.IXApi.Modules.ERP.Foundation.Features.PaymentSchedule' = 'IAX.IXApi.Modules.ERP.Foundation.PaymentSchedules'
    'IAX.IXApi.Modules.ERP.Foundation.Features.DeliveryMode' = 'IAX.IXApi.Modules.ERP.Foundation.DeliveryModes'
    'IAX.IXApi.Modules.ERP.Foundation.Features.DeliveryTerm' = 'IAX.IXApi.Modules.ERP.Foundation.DeliveryTerms'
    'IAX.IXApi.Modules.ERP.Foundation.Features.Currency' = 'IAX.IXApi.Modules.ERP.Foundation.Currency'
    'IAX.IXApi.Modules.ERP.Foundation.Features.Markup' = 'IAX.IXApi.Modules.ERP.Foundation.Markup'
    'IAX.IXApi.Modules.ERP.Foundation.Features.Tax' = 'IAX.IXApi.Modules.ERP.Foundation.Tax'
    'IAX.IXApi.Modules.ERP.AccountsReceivable.Features' = 'IAX.IXApi.Modules.ERP.AccountsReceivable'
    'IAX.IXApi.Modules.ERP.AccountsPayable.Features' = 'IAX.IXApi.Modules.ERP.AccountsPayable'
    'IAX.IXApi.Modules.ERP.GeneralLedger.Features' = 'IAX.IXApi.Modules.ERP.GeneralLedger'
    'IAX.IXApi.Modules.ERP.InventoryManagement.Features' = 'IAX.IXApi.Modules.ERP.Inventory'
    'IAX.IXApi.Modules.ERP.InventoryManagement' = 'IAX.IXApi.Modules.ERP.Inventory'
}

$utf8 = [Text.UTF8Encoding]::new($false)
Get-ChildItem -LiteralPath $root -Recurse -File -Filter '*.cs' |
    Where-Object { $_.FullName -notmatch '\\(bin|obj|\.dotnet)\\' } |
    ForEach-Object {
        $content = [IO.File]::ReadAllText($_.FullName)
        $updated = $content
        foreach ($replacement in $replacements.GetEnumerator()) {
            $updated = $updated.Replace($replacement.Key, $replacement.Value)
        }
        if ($updated -ne $content) { [IO.File]::WriteAllText($_.FullName, $updated, $utf8) }
    }

# Remove only empty legacy wrapper directories.
Get-ChildItem -LiteralPath $modules -Recurse -Directory |
    Sort-Object FullName -Descending |
    Where-Object { -not (Get-ChildItem -LiteralPath $_.FullName -Force) } |
    Remove-Item

Write-Output 'Module tree conformed to the requested structure.'
