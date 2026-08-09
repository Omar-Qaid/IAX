$groupPath = "IXApi\src\Modules\Finance\Foundation\Tax\Controllers\TaxGroupController.cs"
if (Test-Path $groupPath) {
    $content = [System.IO.File]::ReadAllText($groupPath, [System.Text.Encoding]::UTF8)
    $content = $content -replace "\b_db\.TaxGroupHeadings\b", "_db.Set<TaxGroupHeading>()"
    $content = $content -replace "\b_db\.TaxGroupDatas\b", "_db.Set<TaxGroupData>()"
    $content = $content -replace "\b_db\.TaxData\b", "_db.Set<TaxData>()"
    [System.IO.File]::WriteAllText($groupPath, $content, [System.Text.Encoding]::UTF8)
    Write-Output "Fixed TaxGroupController.cs DbSet calls"
}

$itemGroupPath = "IXApi\src\Modules\Finance\Foundation\Tax\Controllers\TaxItemGroupController.cs"
if (Test-Path $itemGroupPath) {
    $content = [System.IO.File]::ReadAllText($itemGroupPath, [System.Text.Encoding]::UTF8)
    $content = $content -replace "\b_db\.TaxOnItems\b", "_db.Set<TaxOnItem>()"
    $content = $content -replace "\b_db\.TaxData\b", "_db.Set<TaxData>()"
    [System.IO.File]::WriteAllText($itemGroupPath, $content, [System.Text.Encoding]::UTF8)
    Write-Output "Fixed TaxItemGroupController.cs DbSet calls"
}
