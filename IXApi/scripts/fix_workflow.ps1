# Clean up all files in Workflow module
$path = "IXApi\src\Modules\Workflow"
$files = Get-ChildItem -Path $path -Recurse -Filter *.cs
foreach ($file in $files) {
    if ($file.FullName -like "*\obj\*" -or $file.FullName -like "*\bin\*") { continue }
    $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
    
    # Clean invalid imports
    $newContent = $content -replace "using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;", ""
    $newContent = $newContent -replace "using IAX.IXApi.Infrastructure.Persistence;", ""
    
    # Replace ApplicationDbContext with DbContext
    $newContent = $newContent -replace "ApplicationDbContext", "DbContext"
    
    # Add Microsoft.Extensions.Logging if missing
    if ($newContent -match "\b(ILogger|ILogger<)\b" -and $newContent -notmatch "using Microsoft.Extensions.Logging;") {
        $newContent = "using Microsoft.Extensions.Logging;`r`n" + $newContent
    }
    
    # Add Shared.Domain.Entities if missing
    if ($newContent -match "\b(Entity|LookupEntity|MasterEntity|BaseController|BaseService)\b" -and $newContent -notmatch "using IAX.IXApi.Shared.Domain.Entities;") {
        $newContent = "using IAX.IXApi.Shared.Domain.Entities;`r`n" + $newContent
    }
    
    if ($newContent -ne $content) {
        [System.IO.File]::WriteAllText($file.FullName, $newContent, [System.Text.Encoding]::UTF8)
        Write-Output "Applied rules to: $($file.Name)"
    }
}
