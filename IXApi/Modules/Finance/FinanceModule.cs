using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Modules.Finance
{
    public static class FinanceModule
    {
        public static IServiceCollection AddFinanceModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<GeneralLedger.FiscalCalendar.ILedgerFiscalCalendarPeriodService, GeneralLedger.FiscalCalendar.LedgerFiscalCalendarPeriodService>();
            services.AddScoped<GeneralLedger.FiscalCalendar.IFiscalCalendarService, GeneralLedger.FiscalCalendar.FiscalCalendarService>();
            services.AddScoped<GeneralLedger.FiscalCalendar.IFiscalCalendarYearService, GeneralLedger.FiscalCalendar.FiscalCalendarYearService>();
            services.AddScoped<GeneralLedger.FiscalCalendar.IFiscalCalendarPeriodService, GeneralLedger.FiscalCalendar.FiscalCalendarPeriodService>();
            services.AddScoped<AccountsReceivable.PostingProfile.Interfaces.ICustPostingProfileService, AccountsReceivable.PostingProfile.Services.CustPostingProfileService>();

            // Explicit Finance registrations
            services.AddScoped<Shared.Features.ICurrencyService, Shared.Features.CurrencyService>();
            services.AddScoped<Shared.Features.IDlvModeService, Shared.Features.DlvModeService>();
            services.AddScoped<Shared.Features.IDlvTermService, Shared.Features.DlvTermService>();
            services.AddScoped<Foundation.LegalEntities.ICompanyInfoService, Foundation.LegalEntities.CompanyInfoService>();
            services.AddScoped<Foundation.LogisticsAddresses.IElectronicAddressService, Foundation.LogisticsAddresses.ElectronicAddressService>();
            services.AddScoped<Foundation.LogisticsAddresses.IGlobalAddressBookService, Foundation.LogisticsAddresses.GlobalAddressBookService>();
            services.AddScoped<Foundation.LogisticsAddresses.ILocationService, Foundation.LogisticsAddresses.LocationService>();
            services.AddScoped<Foundation.LogisticsAddresses.IPartyLocationService, Foundation.LogisticsAddresses.PartyLocationService>();
            services.AddScoped<Foundation.LogisticsAddresses.IPartyService, Foundation.LogisticsAddresses.PartyService>();
            services.AddScoped<Foundation.LogisticsAddresses.IPostalAddressService, Foundation.LogisticsAddresses.PostalAddressService>();
            services.AddScoped<Shared.Features.IPaymSchedLineService, Shared.Features.PaymSchedLineService>();
            services.AddScoped<Shared.Features.IPaymSchedService, Shared.Features.PaymSchedService>();
            services.AddScoped<Shared.Features.IPaymTermService, Shared.Features.PaymTermService>();
            return services;
        }
    }
}

