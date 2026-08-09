$path = "IXApi\src\Modules\Organization"
$files = Get-ChildItem -Path $path -Recurse -Filter *.cs
foreach ($file in $files) {
    if ($file.FullName -like "*\obj\*" -or $file.FullName -like "*\bin\*") { continue }
    $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
    
    # 1. Clean invalid imports
    $newContent = $content -replace "using IAX.IXApi.Modules.Finance.Entities;", ""
    $newContent = $newContent -replace "using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;", ""
    
    # 2. Add Microsoft.Extensions.Logging to controllers
    if ($file.Name -like "*Controller.cs" -and $newContent -notmatch "using Microsoft.Extensions.Logging;") {
        $newContent = "using Microsoft.Extensions.Logging;`r`n" + $newContent
    }
    
    # 3. Add Shared.Domain.Entities if class inherits from Entity/LookupEntity/OrgEntity/MasterEntity
    if ($newContent -match "\b(Entity|LookupEntity|OrgEntity|MasterEntity|BaseController)\b" -and $newContent -notmatch "using IAX.IXApi.Shared.Domain.Entities;") {
        $newContent = "using IAX.IXApi.Shared.Domain.Entities;`r`n" + $newContent
    }
    
    if ($newContent -ne $content) {
        [System.IO.File]::WriteAllText($file.FullName, $newContent, [System.Text.Encoding]::UTF8)
        Write-Output "Applied rules to: $($file.Name)"
    }
}

# 4. Target specific files
# OrgShowroom.cs
$showroomPath = "$path\Showrooms\OrgShowroom.cs"
if (Test-Path $showroomPath) {
    $content = [System.IO.File]::ReadAllText($showroomPath, [System.Text.Encoding]::UTF8)
    $content = $content -replace "using IAX.IXApi.Modules.Organization.Employees.Abstraction;", "using IAX.IXApi.Shared.Domain.Entities;"
    [System.IO.File]::WriteAllText($showroomPath, $content, [System.Text.Encoding]::UTF8)
    Write-Output "Updated OrgShowroom.cs"
}

# HcmWorkerService.cs
$workerServicePath = "$path\Employees\HcmWorkerService.cs"
if (Test-Path $workerServicePath) {
    $content = [System.IO.File]::ReadAllText($workerServicePath, [System.Text.Encoding]::UTF8)
    $content = $content -replace "IAX.IXApi.Infrastructure.Persistence.ApplicationDbContext", "DbContext"
    $content = $content -replace "using IAX.IXApi.Infrastructure.Persistence;", ""
    [System.IO.File]::WriteAllText($workerServicePath, $content, [System.Text.Encoding]::UTF8)
    Write-Output "Updated HcmWorkerService.cs"
}
