$path = "IXApi\src\Modules\Finance"
$files = Get-ChildItem -Path $path -Recurse -Filter *.cs
foreach ($file in $files) {
    if ($file.FullName -like "*\obj\*" -or $file.FullName -like "*\bin\*") { continue }
    $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
    
    # 1. Clean invalid imports
    $newContent = $content -replace "using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;", ""
    $newContent = $newContent -replace "using IAX.IXApi.Infrastructure.Persistence;", ""
    
    # 2. Replace ApplicationDbContext with DbContext
    $newContent = $newContent -replace "ApplicationDbContext", "DbContext"
    
    # 3. Add Microsoft.Extensions.Logging to controllers
    if ($file.Name -like "*Controller.cs" -and $newContent -notmatch "using Microsoft.Extensions.Logging;") {
        $newContent = "using Microsoft.Extensions.Logging;`r`n" + $newContent
    }
    
    # 4. Add Shared.Domain.Entities if class inherits from Entity/LookupEntity/MasterEntity
    if ($newContent -match "\b(Entity|LookupEntity|MasterEntity|BaseController|BaseService)\b" -and $newContent -notmatch "using IAX.IXApi.Shared.Domain.Entities;") {
        $newContent = "using IAX.IXApi.Shared.Domain.Entities;`r`n" + $newContent
    }
    
    if ($newContent -ne $content) {
        [System.IO.File]::WriteAllText($file.FullName, $newContent, [System.Text.Encoding]::UTF8)
        Write-Output "Applied rules to: $($file.Name)"
    }
}
