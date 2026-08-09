# 1. Ensure the destination directory exists
$destDir = "IXApi\src\Modules\Organization\Foundation"
if (!(Test-Path $destDir)) {
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
}

# 2. Move the LogisticsAddresses folder
$srcGAB = "IXApi\src\Modules\Finance\Foundation\LogisticsAddresses"
$destGAB = "IXApi\src\Modules\Organization\Foundation\LogisticsAddresses"
if (Test-Path $srcGAB) {
    if (Test-Path $destGAB) {
        Remove-Item -Recurse -Force $destGAB
    }
    Move-Item -Path $srcGAB -Destination $destGAB -Force
    Write-Output "Moved GAB folder to Organization module"
}

# 3. Create NoYes.cs in Shared project
$sharedEnumsDir = "IXApi\src\Shared\Domain\Enums"
if (!(Test-Path $sharedEnumsDir)) {
    New-Item -ItemType Directory -Path $sharedEnumsDir -Force | Out-Null
}
$noyesContent = @"
namespace IAX.IXApi.Modules.Finance.Common
{
    public enum NoYes
    {
        No  = 0,
        Yes = 1
    }
}
"@
[System.IO.File]::WriteAllText("$sharedEnumsDir\NoYes.cs", $noyesContent, [System.Text.Encoding]::UTF8)
Write-Output "Created NoYes.cs in Shared project"

# 4. Create LogisticsLocationRoleType.cs in Shared project
$roleTypeContent = @"
namespace IAX.IXApi.Modules.Finance.Common
{
    public enum LogisticsLocationRoleType
    {
        None = 0,
        Business = 1,
        Delivery = 2,
        Invoice = 3,
        Home = 4,
        RemitTo = 5,
        ThirdParty = 6,
        Other = 7
    }
}
"@
[System.IO.File]::WriteAllText("$sharedEnumsDir\LogisticsLocationRoleType.cs", $roleTypeContent, [System.Text.Encoding]::UTF8)
Write-Output "Created LogisticsLocationRoleType.cs in Shared project"

# 5. Remove NoYes enum from Enumeration.cs in Finance
$enumPath = "IXApi\src\Modules\Finance\Common\Enums\Enumeration.cs"
if (Test-Path $enumPath) {
    $content = [System.IO.File]::ReadAllText($enumPath, [System.Text.Encoding]::UTF8)
    # Match the public enum NoYes block precisely
    $target = "    public enum NoYes`r`n    {`r`n        No  = 0,`r`n        Yes = 1`r`n    }"
    # Also handle LF just in case
    $content = $content -replace "    public enum NoYes\s*\{\s*No\s*=\s*0,\s*Yes\s*=\s*1\s*\}", ""
    $content = $content.Replace("    public enum NoYes`r`n    {`r`n        No  = 0,`r`n        Yes = 1`r`n    }", "")
    $content = $content.Replace("    public enum NoYes\n    {\n        No  = 0,\n        Yes = 1\n    }", "")
    [System.IO.File]::WriteAllText($enumPath, $content, [System.Text.Encoding]::UTF8)
    Write-Output "Removed duplicate NoYes from Finance's Enumeration.cs"
}

# 6. Delete old LogisticsLocationRoleType.cs from Finance
$oldRoleTypePath = "IXApi\src\Modules\Finance\Common\Enums\LogisticsLocationRoleType.cs"
if (Test-Path $oldRoleTypePath) {
    Remove-Item -Force $oldRoleTypePath
    Write-Output "Deleted duplicate LogisticsLocationRoleType.cs from Finance"
}

# 7. Update FinanceModule.cs to remove GAB registrations
$finModulePath = "IXApi\src\Modules\Finance\FinanceModule.cs"
if (Test-Path $finModulePath) {
    $content = [System.IO.File]::ReadAllText($finModulePath, [System.Text.Encoding]::UTF8)
    # Remove lines containing LogisticsAddresses
    $lines = $content -split "`r`n"
    $newLines = @()
    foreach ($line in $lines) {
        if ($line -notlike "*Foundation.LogisticsAddresses*") {
            $newLines += $line
        }
    }
    $newContent = $newLines -join "`r`n"
    [System.IO.File]::WriteAllText($finModulePath, $newContent, [System.Text.Encoding]::UTF8)
    Write-Output "Removed GAB registrations from FinanceModule.cs"
}

# 8. Update OrganizationModule.cs to add GAB registrations
$orgModulePath = "IXApi\src\Modules\Organization\OrganizationModule.cs"
if (Test-Path $orgModulePath) {
    $content = [System.IO.File]::ReadAllText($orgModulePath, [System.Text.Encoding]::UTF8)
    # Find the place to insert registrations (before return services;)
    $insertPos = $content.IndexOf("            return services;")
    if ($insertPos -gt 0) {
        $registrations = @"
            services.AddScoped<Foundation.LogisticsAddresses.IElectronicAddressService, Foundation.LogisticsAddresses.ElectronicAddressService>();
            services.AddScoped<Foundation.LogisticsAddresses.IGlobalAddressBookService, Foundation.LogisticsAddresses.GlobalAddressBookService>();
            services.AddScoped<Foundation.LogisticsAddresses.ILocationService, Foundation.LogisticsAddresses.LocationService>();
            services.AddScoped<Foundation.LogisticsAddresses.IPartyLocationService, Foundation.LogisticsAddresses.PartyLocationService>();
            services.AddScoped<Foundation.LogisticsAddresses.IPartyService, Foundation.LogisticsAddresses.PartyService>();
            services.AddScoped<Foundation.LogisticsAddresses.IPostalAddressService, Foundation.LogisticsAddresses.PostalAddressService>();
"@
        $newContent = $content.Substring(0, $insertPos) + $registrations + "`r`n" + $content.Substring($insertPos)
        [System.IO.File]::WriteAllText($orgModulePath, $newContent, [System.Text.Encoding]::UTF8)
        Write-Output "Added GAB registrations to OrganizationModule.cs"
    }
}
