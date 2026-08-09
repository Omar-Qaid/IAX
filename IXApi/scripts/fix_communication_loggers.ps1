# 1. Add Microsoft.Extensions.Logging to any file using ILogger or ILogger<>
$path = "IXApi\src\Modules\Communication"
$files = Get-ChildItem -Path $path -Recurse -Filter *.cs
foreach ($file in $files) {
    if ($file.FullName -like "*\obj\*" -or $file.FullName -like "*\bin\*") { continue }
    $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
    
    if ($content -match "\bILogger\b" -and $content -notmatch "using Microsoft.Extensions.Logging;") {
        $content = "using Microsoft.Extensions.Logging;`r`n" + $content
        [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Output "Added Microsoft.Extensions.Logging to: $($file.Name)"
    }
}

# 2. Fix CleanupExpiredNotificationsJobHandler.cs imports
$handlerPath = "$path\Notifications\BackgroundJobs\CleanupExpiredNotificationsJobHandler.cs"
if (Test-Path $handlerPath) {
    $content = [System.IO.File]::ReadAllText($handlerPath, [System.Text.Encoding]::UTF8)
    $target = "using IAX.IXApi.Modules.Administration.BackgroundJobs;"
    $replacement = "using IAX.IXApi.Modules.Administration.BackgroundJobs;`r`nusing IAX.IXApi.Modules.Administration.BackgroundJobs.Services;`r`nusing IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;"
    $content = $content.Replace($target, $replacement)
    [System.IO.File]::WriteAllText($handlerPath, $content, [System.Text.Encoding]::UTF8)
    Write-Output "Updated CleanupExpiredNotificationsJobHandler.cs imports"
}
