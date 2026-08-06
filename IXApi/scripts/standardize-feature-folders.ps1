param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($RepositoryRoot)
$modulesRoot = [IO.Path]::GetFullPath((Join-Path $root 'Modules'))
if (-not $modulesRoot.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Modules path must remain inside the repository.'
}

$renames = [ordered]@{
    'Models' = 'Entities'
    'DTOs' = 'Dtos'
    'Configurations' = 'Configuration'
    'Validators' = 'Validation'
}

foreach ($rename in $renames.GetEnumerator()) {
    Get-ChildItem -LiteralPath $modulesRoot -Recurse -Directory |
        Where-Object Name -CEQ $rename.Key |
        Sort-Object { $_.FullName.Length } -Descending |
        ForEach-Object {
            $source = $_.FullName
            $temporary = Join-Path $_.Parent.FullName (".__rename__" + [Guid]::NewGuid().ToString('N'))
            $destination = Join-Path $_.Parent.FullName $rename.Value
            if (-not $source.StartsWith($root, [StringComparison]::OrdinalIgnoreCase) -or
                -not $destination.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) {
                throw "Rename outside repository: $source"
            }
            Move-Item -LiteralPath $source -Destination $temporary
            Move-Item -LiteralPath $temporary -Destination $destination
        }
}

$utf8 = [Text.UTF8Encoding]::new($false)
Get-ChildItem -LiteralPath $root -Recurse -File -Filter '*.cs' |
    Where-Object { $_.FullName -notmatch '\\(bin|obj|\.dotnet)\\' } |
    ForEach-Object {
        $content = [IO.File]::ReadAllText($_.FullName)
        $updated = $content.Replace('.DTOs', '.Dtos')
        $updated = $updated.Replace('.Configurations', '.Configuration')
        $updated = $updated.Replace('.Validators', '.Validation')
        if ($updated -ne $content) { [IO.File]::WriteAllText($_.FullName, $updated, $utf8) }
    }

Write-Output 'Feature folders standardized.'
