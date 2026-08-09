$servicePath = "IXApi\src\Modules\Communication\Notifications\Services\SysNotificationService.cs"
if (Test-Path $servicePath) {
    $content = [System.IO.File]::ReadAllText($servicePath, [System.Text.Encoding]::UTF8)
    
    # 1. Add missing imports if not present
    if ($content -notmatch "using IAX.IXApi.Modules.Organization.EmployeeGroups;") {
        $imports = @"
using IAX.IXApi.Modules.Organization.EmployeeGroups;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
"@
        $content = $imports + "`r`n" + $content
    }

    # 2. Fix SysNotificationAuditLogs.Add
    $content = $content.Replace("_db.SysNotificationAuditLogs.Add", "_db.Set<SysNotificationAuditLog>().Add")

    # 3. Fix ResolveRecipientsAsync
    $oldBlock = @"
            if (dto.RoleNames?.Any() == true)
            {
                var roleUserIds = await _db.UserRoles.AsNoTracking()
                    .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                    .Where(x => dto.RoleNames.Contains(x.Name))
                    .Select(x => x.UserId)
                    .ToListAsync(ct);
                foreach (var id in roleUserIds) userIds.Add(id);
            }

            if (dto.DepartmentIds?.Any() == true)
            {
                var deptUserIds = await _db.HcmWorkers.AsNoTracking()
                    .Where(e => dto.DepartmentIds.Contains((short)e.DepartmentId))
                    .Join(_db.AspNetUser, e => e.RecId, u => u.OrgEntityId, (e, u) => u.Id)
                    .ToListAsync(ct);
                foreach (var id in deptUserIds) userIds.Add(id);
            }

            if (dto.GroupIds?.Any() == true)
            {
                var groupUserIds = await _db.OrgEmployeeGroupDetails.AsNoTracking()
                    .Where(gd => dto.GroupIds.Contains(gd.UserGroupID))
                    .Select(gd => gd.UserID)
                    .ToListAsync(ct);
                foreach (var id in groupUserIds) userIds.Add(id);
            }
"@

    $newBlock = @"
            if (dto.RoleNames?.Any() == true)
            {
                var roleUserIds = await _db.Set<IAX.IXApi.Modules.Identity.Users.AspNetUserRole>().AsNoTracking()
                    .Join(_db.Set<IAX.IXApi.Modules.Identity.Roles.AspNetRole>(), ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                    .Where(x => dto.RoleNames.Contains(x.Name))
                    .Select(x => x.UserId)
                    .ToListAsync(ct);
                foreach (var id in roleUserIds) userIds.Add(id);
            }

            if (dto.DepartmentIds?.Any() == true)
            {
                var deptUserIds = await _db.Set<HcmWorker>().AsNoTracking()
                    .Where(e => dto.DepartmentIds.Contains((short)e.DepartmentId))
                    .Join(_db.Set<IAX.IXApi.Modules.Identity.Users.AspNetUser>(), e => e.RecId, u => u.OrgEntityId, (e, u) => u.Id)
                    .ToListAsync(ct);
                foreach (var id in deptUserIds) userIds.Add(id);
            }

            if (dto.GroupIds?.Any() == true)
            {
                var groupUserIds = await _db.Set<OrgEmployeeGroupDetail>().AsNoTracking()
                    .Where(gd => dto.GroupIds.Contains(gd.UserGroupID))
                    .Select(gd => gd.UserID)
                    .ToListAsync(ct);
                foreach (var id in groupUserIds) userIds.Add(id);
            }
"@

    # Handle both CRLF and LF
    $content = $content.Replace($oldBlock, $newBlock)
    $content = $content.Replace($oldBlock.Replace("`r`n", "`n"), $newBlock.Replace("`r`n", "`n"))
    
    [System.IO.File]::WriteAllText($servicePath, $content, [System.Text.Encoding]::UTF8)
    Write-Output "Applied fixes to SysNotificationService.cs"
}
