$modulesPath = "c:\Users\Omar.Qaid\Desktop\IAX\IXApi\src\Modules"
$files = [System.IO.Directory]::GetFiles($modulesPath, "*.cs", [System.IO.SearchOption]::AllDirectories)

$results = @()

foreach ($file in $files) {
    if ($file -like "*\obj\*" -or $file -like "*\bin\*") { continue }
    
    $content = [System.IO.File]::ReadAllText($file, [System.Text.Encoding]::UTF8)
    
    # Strip block comments /* ... */
    $clean = [System.Text.RegularExpressions.Regex]::Replace($content, "(?s)/\*.*?\*/", "")
    # Strip line comments // ...
    $clean = [System.Text.RegularExpressions.Regex]::Replace($clean, "//.*", "")
    # Strip string literals "..."
    $clean = [System.Text.RegularExpressions.Regex]::Replace($clean, '"[^"\\]*(?:\\.[^"\\]*)*"', '""')
    
    # Count occurrences of class/interface/struct/record declarations
    # Let's split clean content into lines and track braces
    $lines = $clean -split "`n"
    $braceLevel = 0
    $declarations = 0
    $names = @()
    
    foreach ($line in $lines) {
        $trimmed = $line.Trim()
        if ($trimmed -eq "") { continue }
        
        # Check for type declaration when braceLevel is 0 (file scope namespace) or 1 (normal namespace)
        if ($braceLevel -le 1) {
            if ($trimmed -match '^(public|internal|private|protected)?\s*(static|abstract|sealed|partial)?\s*(class|interface|struct|record)\s+(\w+)') {
                $declarations++
                $names += $Matches[4]
            }
        }
        
        # Track braces
        $opens = ($trimmed.ToCharArray() | Where-Object { $_ -eq '{' }).Count
        $closes = ($trimmed.ToCharArray() | Where-Object { $_ -eq '}' }).Count
        $braceLevel += $opens - $closes
    }
    
    if ($declarations -gt 1) {
        $relPath = $file.Substring("c:\Users\Omar.Qaid\Desktop\IAX\".Length)
        $results += [PSCustomObject]@{
            File = $relPath
            Count = $declarations
            Names = $names -join ", "
        }
    }
}

$results | Format-Table -AutoSize
