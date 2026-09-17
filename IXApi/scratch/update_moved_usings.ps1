$files = Get-ChildItem -Path "src" -Recurse -Filter "*.cs"
foreach ($file in $files) {
    try {
        $content = [System.IO.File]::ReadAllText($file.FullName)
        if ([string]::IsNullOrWhiteSpace($content)) { continue }

        $updated = $content
        $updated = $updated.Replace("HcmWorkerss", "HcmWorkers")
        $updated = $updated.Replace("namespace IAX.IXApi.Modules.Organization.Genders", "namespace IAX.IXApi.Modules.Finance.Foundation.Genders")
        $updated = $updated.Replace("namespace IAX.IXApi.Modules.Organization.HcmWorkers", "namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers")
        $updated = $updated.Replace("namespace IAX.IXApi.Modules.Organization.HcmWorker", "namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers")
        $updated = $updated.Replace("namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorker", "namespace IAX.IXApi.Modules.Finance.Foundation.HcmWorkers")
        $updated = $updated.Replace("namespace IAX.IXApi.Modules.Organization.Nationalities", "namespace IAX.IXApi.Modules.Finance.Foundation.Nationalities")
        $updated = $updated.Replace("namespace IAX.IXApi.Modules.Organization.Occupations", "namespace IAX.IXApi.Modules.Finance.Foundation.Occupations")
        $updated = $updated.Replace("namespace IAX.IXApi.Modules.Organization.OrganizationUnits", "namespace IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits")
        $updated = $updated.Replace("namespace IAX.IXApi.Modules.Organization.Structure", "namespace IAX.IXApi.Modules.Finance.Foundation.Structure")
        $updated = $updated.Replace("namespace IAX.IXApi.Modules.Organization.WorkerOrganizationAssignments", "namespace IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments")

        $updated = $updated.Replace("using IAX.IXApi.Modules.Organization.Genders", "using IAX.IXApi.Modules.Finance.Foundation.Genders")
        $updated = $updated.Replace("using IAX.IXApi.Modules.Organization.HcmWorkers", "using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers")
        $updated = $updated.Replace("using IAX.IXApi.Modules.Organization.HcmWorker;", "using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;")
        $updated = $updated.Replace("using IAX.IXApi.Modules.Finance.Foundation.HcmWorker;", "using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;")
        $updated = $updated.Replace("using IAX.IXApi.Modules.Organization.Nationalities", "using IAX.IXApi.Modules.Finance.Foundation.Nationalities")
        $updated = $updated.Replace("using IAX.IXApi.Modules.Organization.Occupations", "using IAX.IXApi.Modules.Finance.Foundation.Occupations")
        $updated = $updated.Replace("using IAX.IXApi.Modules.Organization.OrganizationUnits", "using IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits")
        $updated = $updated.Replace("using IAX.IXApi.Modules.Organization.Structure", "using IAX.IXApi.Modules.Finance.Foundation.Structure")
        $updated = $updated.Replace("using IAX.IXApi.Modules.Organization.WorkerOrganizationAssignments", "using IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments")
        
        $updated = $updated.Replace("IAX.IXApi.Modules.Organization.OrganizationUnits", "IAX.IXApi.Modules.Finance.Foundation.OrganizationUnits")
        $updated = $updated.Replace("IAX.IXApi.Modules.Organization.WorkerOrganizationAssignments", "IAX.IXApi.Modules.Finance.Foundation.WorkerOrganizationAssignments")
        $updated = $updated.Replace("IAX.IXApi.Modules.Organization.Structure", "IAX.IXApi.Modules.Finance.Foundation.Structure")

        if ($content -ne $updated) {
            [System.IO.File]::WriteAllText($file.FullName, $updated)
        }
    } catch {
        # ignore
    }
}
