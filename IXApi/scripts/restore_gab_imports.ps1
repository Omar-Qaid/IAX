$path = "IXApi\src\Modules\Organization\Foundation\LogisticsAddresses"
$files = Get-ChildItem -Path $path -Recurse -Filter *.cs
foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
    
    # Check if the file contains references to GAB entities and lacks the using statement
    if ($content -notmatch "using IAX.IXApi.Modules.Finance.Entities;") {
        # Insert using statement after the namespace or at the top
        $content = "using IAX.IXApi.Modules.Finance.Entities;`r`n" + $content
        [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Output "Restored using statement in: $($file.Name)"
    }
}
