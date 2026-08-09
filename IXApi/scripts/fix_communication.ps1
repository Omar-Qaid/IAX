# 1. Ensure the destination directory exists
$destDir = "IXApi\src\Modules\Communication\Realtime"
if (!(Test-Path $destDir)) {
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
}

# 2. Move the Realtime folder from Infrastructure to Communication
$srcRealtime = "IXApi\src\Infrastructure\Realtime"
if (Test-Path $srcRealtime) {
    # Move files inside the folder
    Get-ChildItem -Path $srcRealtime -Filter *.cs | ForEach-Object {
        $destFile = Join-Path $destDir $_.Name
        Move-Item -Path $_.FullName -Destination $destFile -Force
    }
    # Remove empty srcRealtime directory
    Remove-Item -Recurse -Force $srcRealtime
    Write-Output "Moved Realtime SignalR files to Communication module"
}

# 3. Add using Microsoft.Extensions.Configuration/DependencyInjection to CommunicationModule.cs
$modulePath = "IXApi\src\Modules\Communication\CommunicationModule.cs"
if (Test-Path $modulePath) {
    $content = [System.IO.File]::ReadAllText($modulePath, [System.Text.Encoding]::UTF8)
    if ($content -notmatch "using Microsoft.Extensions.DependencyInjection;") {
        $content = "using Microsoft.Extensions.DependencyInjection;`r`nusing Microsoft.Extensions.Configuration;`r`n" + $content
        [System.IO.File]::WriteAllText($modulePath, $content, [System.Text.Encoding]::UTF8)
        Write-Output "Updated CommunicationModule.cs imports"
    }
}

# 4. Clean up all files in Communication
$path = "IXApi\src\Modules\Communication"
$files = Get-ChildItem -Path $path -Recurse -Filter *.cs
foreach ($file in $files) {
    if ($file.FullName -like "*\obj\*" -or $file.FullName -like "*\bin\*") { continue }
    $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
    
    # Clean invalid imports
    $newContent = $content -replace "using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;", ""
    $newContent = $newContent -replace "using IAX.IXApi.Infrastructure.Persistence;", ""
    
    # Replace ApplicationDbContext with DbContext
    $newContent = $newContent -replace "ApplicationDbContext", "DbContext"
    
    # Add Microsoft.Extensions.Logging to controllers
    if ($file.Name -like "*Controller.cs" -and $newContent -notmatch "using Microsoft.Extensions.Logging;") {
        $newContent = "using Microsoft.Extensions.Logging;`r`n" + $newContent
    }
    
    # Add Shared.Domain.Entities if class inherits from Entity/LookupEntity/MasterEntity
    if ($newContent -match "\b(Entity|LookupEntity|MasterEntity|BaseController|BaseService)\b" -and $newContent -notmatch "using IAX.IXApi.Shared.Domain.Entities;") {
        $newContent = "using IAX.IXApi.Shared.Domain.Entities;`r`n" + $newContent
    }
    
    if ($newContent -ne $content) {
        [System.IO.File]::WriteAllText($file.FullName, $newContent, [System.Text.Encoding]::UTF8)
        Write-Output "Applied rules to: $($file.Name)"
    }
}
