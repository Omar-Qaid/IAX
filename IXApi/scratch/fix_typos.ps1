$files = Get-ChildItem -Path "src" -Recurse -Filter "*.cs"
foreach ($file in $files) {
    try {
        $content = [System.IO.File]::ReadAllText($file.FullName)
        if ([string]::IsNullOrWhiteSpace($content)) { continue }

        $updated = $content
        $updated = $updated.Replace("HcmWorkersss", "HcmWorkers")
        $updated = $updated.Replace("HcmWorkerss", "HcmWorkers")

        if ($content -ne $updated) {
            [System.IO.File]::WriteAllText($file.FullName, $updated)
        }
    } catch {
        # ignore
    }
}
