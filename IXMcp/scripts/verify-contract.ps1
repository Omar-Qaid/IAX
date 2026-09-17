param(
    [Parameter(Mandatory = $true)]
    [string] $ContractDirectory
)

$ErrorActionPreference = 'Stop'
$required = @('manifest.json', 'ixapi.openapi.json', 'operations.json', 'pilot-contracts.json')
foreach ($name in $required) {
    $path = Join-Path $ContractDirectory $name
    if (-not (Test-Path -LiteralPath $path)) { throw "Missing contract artifact: $path" }
}

$manifest = Get-Content -Raw (Join-Path $ContractDirectory 'manifest.json') | ConvertFrom-Json
$operations = Get-Content -Raw (Join-Path $ContractDirectory 'operations.json') | ConvertFrom-Json
$pilots = Get-Content -Raw (Join-Path $ContractDirectory 'pilot-contracts.json') | ConvertFrom-Json
$operationCount = @($operations).Count
$pilotCount = @($pilots).Count
$actualHash = (Get-FileHash -Algorithm SHA256 (Join-Path $ContractDirectory 'ixapi.openapi.json')).Hash.ToLowerInvariant()

if ($manifest.purpose -ne 'discovery-only' -or $manifest.executable -ne $false) { throw 'Manifest safety flags are invalid.' }
if ($manifest.schemaSha256 -ne $actualHash) { throw 'OpenAPI SHA-256 does not match the manifest.' }
if ($manifest.operationCount -ne $operationCount) { throw 'Operation count does not match the manifest.' }
if ($manifest.pilotCandidateCount -ne $pilotCount) { throw 'Pilot count does not match the manifest.' }

$unclassified = @($operations | Where-Object {
    [string]::IsNullOrWhiteSpace($_.status) -or [string]::IsNullOrWhiteSpace($_.reason)
})
if ($unclassified.Count -ne 0) { throw "$($unclassified.Count) operations have no explicit status/reason." }

$operations | Group-Object module, status | Sort-Object Name | ForEach-Object {
    [pscustomobject]@{ Classification = $_.Name; Count = $_.Count }
} | Format-Table -AutoSize

Write-Output "Verified $operationCount explicitly classified operations and $pilotCount pilot candidates."
