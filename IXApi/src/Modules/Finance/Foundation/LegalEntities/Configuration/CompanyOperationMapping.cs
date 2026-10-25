using Mapster;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;

namespace IAX.IXApi.Modules.Finance.Foundation.LegalEntities;

public sealed record CompanyWriteSource(CompanyInfoDto Dto);
public sealed record CompanyPostalMappingSource(LogisticsPostalAddress Address, DirPartyLocation? PartyLocation, LogisticsLocation? Location, List<string> Roles);
public sealed record CompanyContactMappingSource(LogisticsElectronicAddress Address, DirPartyLocation? PartyLocation, LogisticsLocation? Location, List<string> Roles);

public sealed class CompanyOperationMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CompanyWriteSource, CompanyInfo>().IgnoreNonMapped(true)
            .Map(d => d.DataArea, s => s.Dto.DataArea)
            .Map(d => d.Name, s => s.Dto.Name)
            .Map(d => d.LanguageId, s => s.Dto.LanguageId)
            .Map(d => d.CurrencyCode, s => s.Dto.CurrencyCode)
            .Map(d => d.TaxLicenseNum, s => s.Dto.TaxLicenseNum)
            .Map(d => d.FederalTaxId, s => s.Dto.FederalTaxId)
            .Map(d => d.BankAccount, s => s.Dto.BankAccount)
            .Map(d => d.Calendar, s => s.Dto.Calendar)
            .Map(d => d.TimeZone, s => s.Dto.TimeZone)
            .Map(d => d.Memo, s => s.Dto.Memo)
            .Map(d => d.ArabicName, s => s.Dto.ArabicName)
            .Map(d => d.LocalizedRegion, s => s.Dto.LocalizedRegion);

        config.NewConfig<CompanyPostalMappingSource, AddressInfoDto>().MapWith(s => new AddressInfoDto
                    {
                        Id = s.Address.RecId.ToString(),
                        Location = s.Address.Location,
                        LocationId = (s.Location == null ? string.Empty : s.Location.LocationId ?? string.Empty),
                        Description = (s.Location == null ? string.Empty : s.Location.Description ?? string.Empty),
                        Address = s.Address.Address,
                        Primary = (s.PartyLocation == null ? (NoYes?)null : s.PartyLocation.IsPrimary) == IAX.IXApi.Modules.Finance.Common.NoYes.Yes,
                        Street = s.Address.Street,
                        City = s.Address.City,
                        State = s.Address.State,
                        ZipCode = s.Address.ZipCode,
                        County = s.Address.County,
                        CountryRegionId = s.Address.CountryRegionId,
                        ValidFrom = s.Address.ValidFrom,
                        ValidTo = s.Address.ValidTo,
                        Roles = s.Roles
                    });

        config.NewConfig<CompanyContactMappingSource, ContactInfoDto>().MapWith(s => new ContactInfoDto
                    {
                        Id = s.Address.RecId.ToString(),
                        Location = s.Address.Location,
                        LocationId = (s.Location == null ? string.Empty : s.Location.LocationId ?? string.Empty),
                        Description = s.Address.Description,
                        Type = s.Address.Type.ToString(),
                        Number = s.Address.Locator,
                        Extension = s.Address.LocatorExtension,
                        Primary = s.Address.IsPrimary == IAX.IXApi.Modules.Finance.Common.NoYes.Yes || (s.PartyLocation != null && s.PartyLocation.IsPrimary == IAX.IXApi.Modules.Finance.Common.NoYes.Yes),
                        Roles = s.Roles
                    });
    }
}
