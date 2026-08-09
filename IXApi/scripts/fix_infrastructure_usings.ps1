$files = @(
    "IXApi\src\Infrastructure\Files\ISysFileService.cs",
    "IXApi\src\Infrastructure\Files\SysFileService.cs",
    "IXApi\src\Infrastructure\Identity\CurrentUserService.cs"
)

foreach ($f in $files) {
    if (Test-Path $f) {
        $content = [System.IO.File]::ReadAllText($f, [System.Text.Encoding]::UTF8)
        if ($content -notmatch "using Microsoft.AspNetCore.Http;") {
            $content = "using Microsoft.AspNetCore.Http;`r`n" + $content
            [System.IO.File]::WriteAllText($f, $content, [System.Text.Encoding]::UTF8)
            Write-Output "Added Microsoft.AspNetCore.Http using to $f"
        }
    }
}
