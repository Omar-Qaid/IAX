using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Foundation;

/// <summary>The startup data of the Foundation module, read from <c>Data/baseline-foundation.json</c>.</summary>
public sealed record FoundationBaselineCatalog(
    FoundationBaselineCatalog.CompanyRow[] Companies,
    FoundationBaselineCatalog.LocationRoleRow[] LocationRoles,
    FoundationBaselineCatalog.CountryRow Country,
    FoundationBaselineCatalog.HeadquartersRow[] Headquarters,
    FoundationBaselineCatalog.DimensionRow[] Dimensions,
    FoundationBaselineCatalog.CurrencyRow[] Currencies,
    FoundationBaselineCatalog.RateTypeRow[] ExchangeRateTypes,
    FoundationBaselineCatalog.RateSet ExchangeRates,
    FoundationBaselineCatalog.CalendarRow FiscalCalendar)
{
    public static FoundationBaselineCatalog Load() => BaselineManifest.Read<FoundationBaselineCatalog>("baseline-foundation.json");

    /// <summary>The company every company-scoped baseline row is written to.</summary>
    public const string Company = "dat";

    public sealed record CompanyRow(string DataArea, string PartyNumber, string Name, string LanguageId, string TimeZone);
    public sealed record LocationRoleRow(string Name, bool IsContactInfo);
    public sealed record CountryRow(string CountryRegionId, string IsoCode, string AddrFormat);
    public sealed record HeadquartersRow(string Company, string Region, string RegionName, string County, string City, string CityKey,
        string District, string Zip, string Street, string Building, string Additional, string Phone, string Email);
    public sealed record DimensionRow(string Name, string[] Values);
    public sealed record CurrencyRow(string Code, string? Symbol);
    public sealed record RateTypeRow(string Name, string Description);
    public sealed record RateRow(string From, decimal Rate);
    public sealed record RateSet(string AccountingCurrency, DateTime ValidFrom, DateTime ValidTo, RateRow[] Rates);
    public sealed record CalendarRow(string CalendarId, string Description, int Year);
}
