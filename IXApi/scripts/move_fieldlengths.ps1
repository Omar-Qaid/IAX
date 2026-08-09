# 1. Ensure the destination directory exists
$destDir = "IXApi\src\Shared\Domain\Constants"
if (!(Test-Path $destDir)) {
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
}

# 2. Copy the file to Shared
$srcFile = "IXApi\src\Modules\Finance\Common\Constants\FieldLengths.cs"
$destFile = "IXApi\src\Shared\Domain\Constants\FieldLengths.cs"
if (Test-Path $srcFile) {
    Copy-Item -Path $srcFile -Destination $destFile -Force
    Write-Output "Copied FieldLengths.cs to Shared project"
    
    # 3. Delete the original file in Finance
    Remove-Item -Force $srcFile
    Write-Output "Deleted original FieldLengths.cs from Finance module"
}
