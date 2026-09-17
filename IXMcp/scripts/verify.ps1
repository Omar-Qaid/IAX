$ErrorActionPreference = 'Stop'
$repository = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$dotnetHome = Join-Path $repository '.dotnet'
$packages = Join-Path $dotnetHome 'nuget-packages'
$mcpArtifacts = Join-Path $repository '.artifacts/ixmcp'
$contractArtifacts = Join-Path $repository '.artifacts/mcp-contract'
$contracts = Join-Path $repository '.artifacts/mcp-contract/export'
$nugetConfig = Join-Path $repository 'IXApi/NuGet.Config'
$apiTests = Join-Path $repository 'IXApi/Tests/IXApi.Tests.csproj'
$mcpTests = Join-Path $repository 'IXMcp.Tests/IXMcp.Tests.csproj'

$env:DOTNET_CLI_HOME = $dotnetHome
$env:NUGET_PACKAGES = $packages

dotnet restore $apiTests --configfile $nugetConfig -p:NuGetAudit=false --artifacts-path $contractArtifacts
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet build $apiTests -c Release --no-restore --artifacts-path $contractArtifacts
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet test $apiTests -c Release --no-build --no-restore --artifacts-path $contractArtifacts --filter 'FullyQualifiedName~McpContractDiscoveryTests|FullyQualifiedName~AuthorizationPolicyTests|FullyQualifiedName~CompanyIsolationTests'
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet (Join-Path $contractArtifacts 'bin/IXApi/release/IAX.IXApi.dll') --export-mcp-contract $contracts
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& (Join-Path $PSScriptRoot 'verify-contract.ps1') -ContractDirectory $contracts

dotnet restore $mcpTests --configfile $nugetConfig -p:NuGetAudit=false --artifacts-path $mcpArtifacts
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet build $mcpTests -c Release --no-restore --artifacts-path $mcpArtifacts
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet test $mcpTests -c Release --no-build --no-restore --artifacts-path $mcpArtifacts
exit $LASTEXITCODE
