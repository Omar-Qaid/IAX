$erpPath = "c:\Users\Omar Almojahid\Desktop\AXERP\HCMAPIs\Domains\ERP"
$fieldLengthsPath = "$erpPath\Common\FieldLengths.cs"

$pattern = "FieldLengths\.([A-Za-z0-9_]+)"
$files = Get-ChildItem -Path $erpPath -Filter *.cs -Recurse | Where-Object { $_.FullName -notmatch "FieldLengths\.cs" }

$usedFields = @()
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $matches = [regex]::Matches($content, $pattern)
    foreach ($match in $matches) {
        $usedFields += $match.Groups[1].Value
    }
}
$usedFields = $usedFields | Select-Object -Unique

$existingFields = @()
$fieldContent = Get-Content $fieldLengthsPath
foreach ($line in $fieldContent) {
    if ($line -match "public const int ([A-Za-z0-9_]+)") {
        $existingFields += $matches[1]
    }
}

$missing = $usedFields | Where-Object { $existingFields -notcontains $_ }

if ($missing.Count -gt 0) {
    $newLines = @()
    foreach ($m in $missing) {
        $newLines += "        public const int $m = 50;"
    }
    
    $fileContentStr = Get-Content $fieldLengthsPath -Raw
    $lastBraceIndex = $fileContentStr.LastIndexOf("}")
    if ($lastBraceIndex -ge 0) {
        $secondLastBraceIndex = $fileContentStr.LastIndexOf("}", $lastBraceIndex - 1)
        if ($secondLastBraceIndex -ge 0) {
            $insertString = "`r`n" + ($newLines -join "`r`n") + "`r`n"
            $fileContentStr = $fileContentStr.Insert($secondLastBraceIndex, $insertString)
            Set-Content -Path $fieldLengthsPath -Value $fileContentStr
            Write-Output "Added $($missing.Count) missing fields:"
            $missing
        }
    }
} else {
    Write-Output "No missing fields found."
}
