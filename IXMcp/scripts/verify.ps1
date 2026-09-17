$ErrorActionPreference = 'Stop'
$repository = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$dotnetHome = Join-Path $repository '.dotnet'
$packages = Join-Path $dotnetHome 'nuget-packages'
$artifacts = Join-Path $repository '.artifacts/ixmcp'
$contracts = Join-Path $repository '.artifacts/mcp-contract/export'

$env:DOTNET_CLI_HOME = $dotnetHome
$env:NUGET_PACKAGES = $packages

& (Join-Path $PSScriptRoot 'verify-contract.ps1') -ContractDirectory $contracts
dotnet build (Join-Path $repository 'IXMcp.Tests/IXMcp.Tests.csproj') -c Release --no-restore --artifacts-path $artifacts
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet test (Join-Path $repository 'IXMcp.Tests/IXMcp.Tests.csproj') -c Release --no-build --no-restore --artifacts-path $artifacts
exit $LASTEXITCODE
