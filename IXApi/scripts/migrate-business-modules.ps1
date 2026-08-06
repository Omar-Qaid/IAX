param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($RepositoryRoot)
$domainsRoot = [IO.Path]::GetFullPath((Join-Path $root 'Domains'))
$modulesRoot = [IO.Path]::GetFullPath((Join-Path $root 'Modules'))

if (-not $domainsRoot.StartsWith($root, [StringComparison]::OrdinalIgnoreCase) -or
    -not $modulesRoot.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Resolved migration paths must remain inside the repository root.'
}

if (-not (Test-Path -LiteralPath $domainsRoot)) {
    throw "Domains directory not found: $domainsRoot"
}

New-Item -ItemType Directory -Path $modulesRoot -Force | Out-Null

$moduleNames = @('Identity', 'Organization', 'Workflow', 'ERP')
foreach ($moduleName in $moduleNames) {
    $source = Join-Path $domainsRoot $moduleName
    $destination = Join-Path $modulesRoot $moduleName

    if (Test-Path -LiteralPath $destination) {
        if (Test-Path -LiteralPath $source) {
            throw "Both source and destination exist for $moduleName. Refusing a partial migration."
        }
        continue
    }

    if (-not (Test-Path -LiteralPath $source)) {
        throw "Module source not found: $source"
    }

    Move-Item -LiteralPath $source -Destination $destination
}

$namespaceReplacements = [ordered]@{
    'HCMAPIs.Domains.Identity'     = 'IAX.IXApi.Modules.Identity'
    'HCMAPIs.Domains.Organization' = 'IAX.IXApi.Modules.Organization'
    'HCMAPIs.Domains.Workflow'     = 'IAX.IXApi.Modules.Workflow'
    'HCMAPIs.Domains.ERP'          = 'IAX.IXApi.Modules.ERP'
}

$utf8 = [Text.UTF8Encoding]::new($false)
Get-ChildItem -LiteralPath $root -Recurse -File -Filter '*.cs' |
    Where-Object { $_.FullName -notmatch '\\(bin|obj|\.dotnet)\\' } |
    ForEach-Object {
        $path = $_.FullName
        $content = [IO.File]::ReadAllText($path)
        $updated = $content
        foreach ($replacement in $namespaceReplacements.GetEnumerator()) {
            $updated = $updated.Replace($replacement.Key, $replacement.Value)
        }
        if ($updated -ne $content) {
            [IO.File]::WriteAllText($path, $updated, $utf8)
        }
    }

Write-Output 'Business modules moved and namespaces updated.'
