using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Modules.ERP
{
    public static class ErpModule
    {
        public static IServiceCollection AddErpModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<GeneralLedger.Features.FiscalCalendar.ILedgerFiscalCalendarPeriodService, GeneralLedger.Features.FiscalCalendar.LedgerFiscalCalendarPeriodService>();
            services.AddScoped<GeneralLedger.Features.FiscalCalendar.IFiscalCalendarService, GeneralLedger.Features.FiscalCalendar.FiscalCalendarService>();
            services.AddScoped<GeneralLedger.Features.FiscalCalendar.IFiscalCalendarYearService, GeneralLedger.Features.FiscalCalendar.FiscalCalendarYearService>();
            services.AddScoped<GeneralLedger.Features.FiscalCalendar.IFiscalCalendarPeriodService, GeneralLedger.Features.FiscalCalendar.FiscalCalendarPeriodService>();
            services.AddScoped<AccountsReceivable.Features.ICustPostingProfileService, AccountsReceivable.Features.CustPostingProfileService>();
            return services;
        }
    }
}
