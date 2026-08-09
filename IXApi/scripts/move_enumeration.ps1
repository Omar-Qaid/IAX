$srcFile = "IXApi\src\Modules\Finance\Common\Enums\Enumeration.cs"
$destFile = "IXApi\src\Shared\Domain\Enums\Enumeration.cs"
if (Test-Path $srcFile) {
    # Move the file
    Move-Item -Path $srcFile -Destination $destFile -Force
    Write-Output "Relocated Enumeration.cs to Shared project"
    
    # Remove the temporary NoYes.cs since it is already defined inside Enumeration.cs
    $tempNoYes = "IXApi\src\Shared\Domain\Enums\NoYes.cs"
    if (Test-Path $tempNoYes) {
        Remove-Item -Force $tempNoYes
        Write-Output "Cleaned up temporary NoYes.cs"
    }
} else {
    Write-Output "Source Enumeration.cs not found"
}
