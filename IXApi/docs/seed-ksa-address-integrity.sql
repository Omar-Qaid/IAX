-- Complete Saudi company examples: no real premises are asserted by sample building numbers.
SET NOCOUNT ON;
SELECT l.LocationId, p.RECID AS AddressRecId, p.CountryRegionId, p.State AS Region,
       p.County AS Governorate, p.City, p.DistrictName, p.ZipCode, p.StreetNumber AS BuildingNumber,
       p.Street, p.BuildingCompliment, p.Address
FROM LogisticsPostalAddress p JOIN LogisticsLocation l ON l.RECID=p.Location
WHERE l.LocationId IN ('DAT-LOC-1','HBMC-LOC-1');
SELECT 'SaudiExampleAddressIncomplete' AS CheckName,COUNT(*) AS Violations
FROM LogisticsPostalAddress p JOIN LogisticsLocation l ON l.RECID=p.Location
WHERE l.LocationId IN ('DAT-LOC-1','HBMC-LOC-1') AND (p.CountryRegionId<>'SA' OR p.County='' OR p.State='' OR p.City='' OR p.DistrictName=''
OR LEN(p.ZipCode)<>5 OR LEN(p.StreetNumber)<>4 OR p.Street='' OR p.BuildingCompliment NOT LIKE 'Unit %; Additional number [0-9][0-9][0-9][0-9]')
UNION ALL SELECT 'SaudiExampleGovernorateMismatch',COUNT(*) FROM LogisticsPostalAddress p JOIN LogisticsLocation l ON l.RECID=p.Location
JOIN LogisticsAddressCity c ON c.RECID=p.CityRecId WHERE l.LocationId IN ('DAT-LOC-1','HBMC-LOC-1') AND p.County<>c.CountyId
UNION ALL SELECT 'SaudiExamplePostalMasterMismatch',COUNT(*) FROM LogisticsPostalAddress p JOIN LogisticsLocation l ON l.RECID=p.Location
JOIN LogisticsAddressZipCode z ON z.RECID=p.ZipCodeRecId WHERE l.LocationId IN ('DAT-LOC-1','HBMC-LOC-1')
AND (z.CityRecId<>p.CityRecId OR z.District<>p.District OR z.DistrictName<>p.DistrictName OR z.County<>p.County);
