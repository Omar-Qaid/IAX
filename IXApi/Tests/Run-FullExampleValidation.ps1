[CmdletBinding()]
param(
    [string]$SqlServer = '.',
    [switch]$NoBuild,
    [switch]$NoRestore
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$resultsDirectory = Join-Path $repositoryRoot ('TestResults/FullExampleEnabled/' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
$settings = @{
    IXAPI_AP_PHASE_ONE_SQLSERVER = $SqlServer
    IAX_BATCH_SQL_TESTS = '1'
    IAX_SEED_ERP_V2_SQL_TESTS = '1'
}
$previousSettings = @{}
foreach ($key in $settings.Keys) {
    $previousSettings[$key] = [Environment]::GetEnvironmentVariable($key, 'Process')
    [Environment]::SetEnvironmentVariable($key, $settings[$key], 'Process')
}

try {
    # IXApi.Tests compiles every test file; the focused projects (InventoryCycle.Tests and others) are
    # subsets of it for a faster inner loop, so running them here would only repeat the same tests.
    foreach ($project in @('IXApi.Tests')) {
        $arguments = @('test', (Join-Path $PSScriptRoot "$project.csproj"),
            '--logger', "trx;LogFileName=$project.trx", '--results-directory', $resultsDirectory)
        if ($NoBuild) { $arguments += '--no-build' }
        if ($NoRestore) { $arguments += '--no-restore' }
        & dotnet @arguments
        $testExitCode = $LASTEXITCODE
        $resultPath = Join-Path $resultsDirectory "$project.trx"
        if (-not (Test-Path -LiteralPath $resultPath)) { throw "No test result produced for $project (exit $testExitCode)." }
        [xml]$result = Get-Content -LiteralPath $resultPath -Raw
        $cases = @($result.TestRun.Results.UnitTestResult | Where-Object { $null -ne $_ })
        $notPassed = @($cases | Where-Object { $_.outcome -ne 'Passed' })
        if ($testExitCode -ne 0 -or $cases.Count -eq 0 -or $notPassed.Count -gt 0) {
            throw "$project did not pass every test: exit=$testExitCode, cases=$($cases.Count), non-passing=$($notPassed.Count). See $resultPath"
        }
    }
    Write-Output "All tests passed with zero skips. Evidence: $resultsDirectory"
}
finally {
    foreach ($key in $settings.Keys) {
        [Environment]::SetEnvironmentVariable($key, $previousSettings[$key], 'Process')
    }
}
