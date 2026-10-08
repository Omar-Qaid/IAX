using Mapster;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;

public sealed record PostalAddressReadSource(LogisticsPostalAddress Address, DirPartyLocation? PartyLocation);
public sealed record ElectronicAddressReadSource(LogisticsElectronicAddress Address, DirPartyLocation? PartyLocation);
public sealed record PostalAddressWriteSource(AddressInfoDto Dto, (string countryId, string stateId, string countyId, string city, long cityId, string zipCode, long zipCodeId, string districtName, long districtId) Geo);
public sealed record ElectronicAddressWriteSource(ContactInfoDto Dto, ElectronicAddressType Type);

public sealed class AddressBookMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PostalAddressReadSource, AddressInfoDto>().MapWith(s => new AddressInfoDto
                {
                    Id = s.Address.RecId.ToString(),
                    Location = s.Address.Location,
                    LocationId = (s.Address.LogisticsLocationTable == null ? string.Empty : s.Address.LogisticsLocationTable.LocationId ?? string.Empty),
                    Description = (s.Address.LogisticsLocationTable == null ? string.Empty : s.Address.LogisticsLocationTable.Description ?? string.Empty),
                    Address = s.Address.Address,
                    Primary = (s.PartyLocation == null ? (NoYes?)null : s.PartyLocation.IsPrimary) == IAX.IXApi.Modules.Finance.Common.NoYes.Yes,
                    Street = s.Address.Street,
                    City = s.Address.City,
                    State = s.Address.State,
                    ZipCode = s.Address.ZipCode,
                    County = s.Address.County,
                    CountryRegionId = s.Address.CountryRegionId,
                    ValidFrom = s.Address.ValidFrom,
                    ValidTo = s.Address.ValidTo
                });

        config.NewConfig<ElectronicAddressReadSource, ContactInfoDto>().MapWith(s => new ContactInfoDto
                {
                    Id = s.Address.RecId.ToString(),
                    Location = s.Address.Location,
                    Description = s.Address.Description,
                    Type = s.Address.Type.ToString(),
                    Number = s.Address.Locator,
                    Extension = s.Address.LocatorExtension,
                    Primary = s.Address.IsPrimary == IAX.IXApi.Modules.Finance.Common.NoYes.Yes || (s.PartyLocation != null && s.PartyLocation.IsPrimary == IAX.IXApi.Modules.Finance.Common.NoYes.Yes)
                });

        config.NewConfig<PostalAddressWriteSource, LogisticsPostalAddress>().IgnoreNonMapped(true)
            .Map(d => d.CountryRegionId, s => s.Geo.countryId)
            .Map(d => d.ZipCode, s => s.Geo.zipCode)
            .Map(d => d.State, s => s.Geo.stateId)
            .Map(d => d.County, s => s.Geo.countyId)
            .Map(d => d.City, s => s.Geo.city)
            .Map(d => d.DistrictName, s => s.Geo.districtName)
            .Map(d => d.CityRecId, s => s.Geo.cityId)
            .Map(d => d.ZipCodeRecId, s => s.Geo.zipCodeId)
            .Map(d => d.District, s => s.Geo.districtId)
            .Map(d => d.Street, s => s.Dto.Street ?? string.Empty)
            .Map(d => d.Address, s => $"{s.Dto.Street}, {s.Geo.city}, {s.Geo.stateId} {s.Geo.zipCode}, {s.Geo.countryId}")
            .Map(d => d.ValidFrom, s => s.Dto.ValidFrom ?? DateTime.MinValue)
            .Map(d => d.ValidTo, s => s.Dto.ValidTo ?? DateTime.MaxValue);

        config.NewConfig<ElectronicAddressWriteSource, LogisticsElectronicAddress>().IgnoreNonMapped(true)
            .Map(d => d.Type, s => s.Type)
            .Map(d => d.Locator, s => s.Dto.Number ?? string.Empty)
            .Map(d => d.LocatorExtension, s => s.Dto.Extension ?? string.Empty)
            .Map(d => d.Description, s => s.Dto.Description ?? string.Empty)
            .Map(d => d.IsPrimary, s => s.Dto.Primary ? IAX.IXApi.Modules.Finance.Common.NoYes.Yes : IAX.IXApi.Modules.Finance.Common.NoYes.No);
    }
}
