-- Read-only semantic checks beyond declared foreign keys. Shared logistics rows
-- may legitimately be referenced by parties in another company.
SET NOCOUNT ON;
SELECT 'PostalDistrictCityMismatch' AS CheckName, COUNT(*) AS Violations
FROM LogisticsPostalAddress p JOIN LogisticsAddressDistrict d ON d.RECID=p.District WHERE p.CityRecId<>d.City
UNION ALL SELECT 'PostalDistrictNameMismatch', COUNT(*) FROM LogisticsPostalAddress p
JOIN LogisticsAddressDistrict d ON d.RECID=p.District WHERE p.DistrictName<>d.Name
UNION ALL SELECT 'PostalCityGeographyMismatch', COUNT(*) FROM LogisticsPostalAddress p
JOIN LogisticsAddressCity c ON c.RECID=p.CityRecId WHERE p.City<>c.Name OR p.CountryRegionId<>c.CountryRegionId OR p.State<>c.StateId
UNION ALL SELECT 'PostalZipGeographyMismatch', COUNT(*) FROM LogisticsPostalAddress p
JOIN LogisticsAddressZipCode z ON z.RECID=p.ZipCodeRecId WHERE p.ZipCode<>z.ZipCode OR p.CountryRegionId<>z.CountryRegionId OR p.State<>z.State OR p.City<>z.City
UNION ALL SELECT 'HierarchyParentMembershipMismatch', COUNT(*) FROM OrganizationHierarchyNodes n
JOIN OrganizationHierarchyNodes p ON p.RECID=n.ParentNodeId WHERE n.HierarchyId<>p.HierarchyId OR n.DataAreaId<>p.DataAreaId
UNION ALL SELECT 'WorkerAssignmentPositionUnitMismatch', COUNT(*) FROM HcmWorkerOrganizationAssignments a
JOIN HcmPositions p ON p.RECID=a.PositionId WHERE a.OrganizationUnitId<>p.OrganizationUnitId OR a.DataAreaId<>p.DataAreaId
UNION ALL SELECT 'WorkerAssignmentPositionRoleMismatch', COUNT(*) FROM HcmWorkerOrganizationAssignments a
JOIN HcmPositions p ON p.RECID=a.PositionId WHERE a.OrganizationRoleId IS NOT NULL AND p.RoleId IS NOT NULL AND a.OrganizationRoleId<>p.RoleId
UNION ALL SELECT 'WorkerAssignmentInvalidDates', COUNT(*) FROM HcmWorkerOrganizationAssignments WHERE ValidTo<ValidFrom
UNION ALL SELECT 'WorkerAssignmentV1InvalidDates', COUNT(*) FROM HcmWorkerOrganizationAssignmentsV1 WHERE ValidTo<ValidFrom
UNION ALL SELECT 'PositionReportingSelfReference', COUNT(*) FROM HcmPositionReportingLines WHERE ManagerPositionId=SubordinatePositionId
UNION ALL SELECT 'WorkerManagerSelfReference', COUNT(*) FROM HcmWorkerOrganizationAssignmentsV1 WHERE HcmWorkerId=HcmManagerWorkerId
UNION ALL SELECT 'RequestAssignmentUnitMismatch', COUNT(*) FROM WfRequests r
JOIN HcmWorkerOrganizationAssignments a ON a.RECID=r.HcmWorkerAssignmentId WHERE r.OrganizationUnitId IS NOT NULL AND r.OrganizationUnitId<>a.OrganizationUnitId
UNION ALL SELECT 'RequestVariableMappingProcessMismatch', COUNT(*) FROM WfRequestMappingVariables m
JOIN WfRequestControls c ON c.RequestControlId=m.RequestControlId JOIN WfVariables v ON v.RECID=m.VariableId WHERE c.ProcessId<>v.ProcessId
UNION ALL SELECT 'BatchWindowInvalidRange', COUNT(*) FROM BatchJobActivePeriod WHERE FromTimeUtc<0 OR FromTimeUtc>=86400 OR ToTimeUtc<0 OR ToTimeUtc>=86400;

