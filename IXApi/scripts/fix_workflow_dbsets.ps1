$handlerPath = "IXApi\src\Modules\Workflow\Execution\WfActivityAutoPassJobHandler.cs"
if (Test-Path $handlerPath) {
    $content = [System.IO.File]::ReadAllText($handlerPath, [System.Text.Encoding]::UTF8)
    $content = $content -replace "EF\.Functions\.DateDiffHour\(a\.AssignDate, now\) >= a\.AutoPassingHrs", "a.AssignDate.AddHours(a.AutoPassingHrs) <= now"
    [System.IO.File]::WriteAllText($handlerPath, $content, [System.Text.Encoding]::UTF8)
    Write-Output "Fixed WfActivityAutoPassJobHandler.cs date translation"
}

$enginePath = "IXApi\src\Modules\Workflow\Requests\ValidationEngine.cs"
if (Test-Path $enginePath) {
    $content = [System.IO.File]::ReadAllText($enginePath, [System.Text.Encoding]::UTF8)
    $content = $content.Replace("_context.WfRequestControlsValidations", "_context.Set<WfRequestControlsValidation>()")
    $content = $content.Replace("_context.WfRequestControls", "_context.Set<WfRequestControl>()")
    $content = $content.Replace("_context.WfRequestDetails", "_context.Set<WfRequestDetail>()")
    [System.IO.File]::WriteAllText($enginePath, $content, [System.Text.Encoding]::UTF8)
    Write-Output "Fixed ValidationEngine.cs DbSet calls"
}
