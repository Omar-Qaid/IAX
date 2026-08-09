param (
    [string]$filePath = "IXApi\src\Modules\Communication\Chat\Services\ISysChatService.cs"
)

$content = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)

# Strip block comments /* ... */ for brace parsing
$clean = [System.Text.RegularExpressions.Regex]::Replace($content, "(?s)/\*.*?\*/", "")

$originalLines = $content -split "`r?\n"
$cleanedLines = $clean -split "`r?\n"

$usings = @()
$namespace = ""
$typeBlocks = @()
$currentBlock = $null
$braceLevel = 0
$hasFileScopedNamespace = $false

for ($i = 0; $i -lt $originalLines.Count; $i++) {
    $origLine = $originalLines[$i]
    $cleanLine = $cleanedLines[$i]
    $trimmedClean = $cleanLine.Trim()
    
    # Strip line comments and strings from clean line for brace parsing
    $cleanLineBrace = $cleanLine -replace '//.*', ''
    $cleanLineBrace = [System.Text.RegularExpressions.Regex]::Replace($cleanLineBrace, '"[^"\\]*(?:\\.[^"\\]*)*"', '""')
    
    $trimmedBrace = $cleanLineBrace.Trim()
    
    # Capture usings at root level (braceLevel = 0)
    if ($trimmedClean -match '^using\s+[^;]+;' -and $braceLevel -eq 0) {
        $usings += $origLine
        continue
    }
    
    # Capture namespace
    if ($trimmedClean -match '^namespace\s+([\w\.]+)') {
        $namespace = $Matches[1]
        if ($trimmedClean -like "*;") {
            $hasFileScopedNamespace = $true
        }
        continue
    }
    
    # Check if we are at namespace level
    $isNamespaceScope = $false
    if ($hasFileScopedNamespace) {
        if ($braceLevel -eq 0) { $isNamespaceScope = $true }
    } else {
        if ($braceLevel -eq 1) { $isNamespaceScope = $true }
    }
    
    if ($isNamespaceScope -and ($trimmedClean -match '^(public|internal|private|protected)?\s*(static|abstract|sealed|partial)?\s*(class|interface|struct|record)\s+(\w+)')) {
        $typeName = $Matches[4]
        $currentBlock = [PSCustomObject]@{
            Name = $typeName
            Lines = [System.Collections.Generic.List[string]]::new()
        }
        $typeBlocks += $currentBlock
    }
    
    if ($currentBlock -ne $null) {
        $currentBlock.Lines.Add($origLine)
    }
    
    # Track braces
    $opens = ($trimmedBrace.ToCharArray() | Where-Object { $_ -eq '{' }).Count
    $closes = ($trimmedBrace.ToCharArray() | Where-Object { $_ -eq '}' }).Count
    $braceLevel += $opens - $closes
    
    $targetBraceLevel = 1
    if ($hasFileScopedNamespace) { $targetBraceLevel = 0 }

    if ($currentBlock -ne $null -and $braceLevel -eq $targetBraceLevel -and $closes -gt 0) {
        $currentBlock = $null
    }
}

if ($typeBlocks.Count -gt 1) {
    $dir = [System.IO.Path]::GetDirectoryName($filePath)
    foreach ($block in $typeBlocks) {
        $newFileName = Join-Path $dir "$($block.Name).cs"
        $newContent = @()
        foreach ($u in $usings) { $newContent += $u }
        if ($usings.Count -gt 0) { $newContent += "" }
        
        if ($hasFileScopedNamespace) {
            $newContent += "namespace $namespace;"
            $newContent += ""
        } else {
            $newContent += "namespace $namespace"
            $newContent += "{"
        }
        
        foreach ($l in $block.Lines) {
            $newContent += $l
        }
        
        if (-not $hasFileScopedNamespace) {
            $newContent += "}"
        }
        
        [System.IO.File]::WriteAllText($newFileName, ($newContent -join "`r`n"), [System.Text.Encoding]::UTF8)
        Write-Output "Created: $newFileName"
    }
    # Delete original file
    Remove-Item -Force $filePath
    Write-Output "Split $filePath successfully into $($typeBlocks.Count) files."
} else {
    Write-Output "File did not contain multiple classes."
}
