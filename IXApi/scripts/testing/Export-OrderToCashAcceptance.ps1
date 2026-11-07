param(
    [Parameter(Mandatory = $true)][string]$AttemptsPath,
    [Parameter(Mandatory = $true)][string]$OutputPath,
    [string[]]$RegressionTrxPaths = @()
)
$ErrorActionPreference = 'Stop'
$otcRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$otcPlan = Get-Content -LiteralPath (Join-Path $otcRoot 'docs/testing/OrderToCashAcceptancePlan.json') -Raw | ConvertFrom-Json
$otcGaps = Get-Content -LiteralPath (Join-Path $otcRoot 'docs/testing/OrderToCashOpenGaps.json') -Raw | ConvertFrom-Json
$otcBindings = Get-Content -LiteralPath (Join-Path $otcRoot 'docs/testing/OrderToCashRegressionBindings.json') -Raw | ConvertFrom-Json
$otcRegressions = foreach ($otcTrxPath in $RegressionTrxPaths) {
    [xml]$otcTrx = Get-Content -LiteralPath $otcTrxPath -Raw
    foreach ($otcResult in $otcTrx.TestRun.Results.UnitTestResult) {
        [pscustomobject]@{ testName = $otcResult.testName; outcome = $otcResult.outcome; source = $otcTrxPath }
    }
}
$otcEvents = @(Get-Content -LiteralPath $AttemptsPath | ForEach-Object { $_ | ConvertFrom-Json })
$otcKnown = @($otcPlan.id)
foreach ($otcEvent in $otcEvents) {
    if ($otcEvent.id -notin $otcKnown -and $otcEvent.id -notin @('RUN', 'PREPARATION')) {
        throw "Unknown acceptance test ID: $($otcEvent.id)"
    }
}
$otcCases = foreach ($otcCase in $otcPlan) {
    $otcHistory = @($otcEvents | Where-Object { $_.id -eq $otcCase.id -and $_.status -ne 'Not run' })
    $otcLatest = $otcHistory | Select-Object -Last 1
    $otcGap = $otcGaps | Where-Object id -eq $otcCase.id
    $otcAcceptedRegression = $false
    $otcSupporting = foreach ($otcBinding in ($otcBindings | Where-Object id -eq $otcCase.id)) {
        $otcBindingPassed = $true
        foreach ($otcPattern in $otcBinding.patterns) {
            $otcMatches = @($otcRegressions | Where-Object testName -like $otcPattern)
            if ($otcMatches.Count -eq 0 -or @($otcMatches | Where-Object outcome -ne 'Passed').Count -gt 0) { $otcBindingPassed = $false }
            [ordered]@{ pattern = $otcPattern; scope = $otcBinding.scope; results = $otcMatches }
        }
        if ($otcBinding.satisfiesAcceptanceCondition -and $otcBindingPassed) { $otcAcceptedRegression = $true }
    }
    [ordered]@{
        id = $otcCase.id
        service = $otcCase.service
        expected = $otcCase.expected
        status = if ($otcLatest) { $otcLatest.status } elseif ($otcGap) { $otcGap.status } elseif ($otcAcceptedRegression) { 'Passed' } else { 'Not run' }
        verification = if ($otcLatest) { 'Executed acceptance/cycle stage' } elseif ($otcAcceptedRegression) { 'Existing service regression; fixture scope recorded below' } elseif ($otcGap) { 'Source review; not a successful business-cycle test' } else { 'No acceptance evidence' }
        runId = $otcLatest.runId
        evidence = $otcLatest.evidence
        gap = $otcGap
        supportingRegressionEvidence = @($otcSupporting)
        attempts = $otcHistory
    }
}
$otcSummary = [ordered]@{}
foreach ($otcGroup in ($otcCases | Group-Object { $_.status })) { $otcSummary[$otcGroup.Name] = $otcGroup.Count }
[ordered]@{
    generatedAtUtc = [DateTime]::UtcNow
    planned = $otcPlan.Count
    summary = $otcSummary
    completeAcceptancePassed = @($otcCases | Where-Object { $_.status -ne 'Passed' }).Count -eq 0
    cases = @($otcCases)
} | ConvertTo-Json -Depth 40 | Set-Content -LiteralPath $OutputPath -Encoding utf8
