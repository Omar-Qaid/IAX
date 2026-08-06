using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Modules.ERP
{
    public static class ErpModule
    {
        public static IServiceCollection AddErpModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<GeneralLedger.FiscalCalendar.ILedgerFiscalCalendarPeriodService, GeneralLedger.FiscalCalendar.LedgerFiscalCalendarPeriodService>();
            services.AddScoped<GeneralLedger.FiscalCalendar.IFiscalCalendarService, GeneralLedger.FiscalCalendar.FiscalCalendarService>();
            services.AddScoped<GeneralLedger.FiscalCalendar.IFiscalCalendarYearService, GeneralLedger.FiscalCalendar.FiscalCalendarYearService>();
            services.AddScoped<GeneralLedger.FiscalCalendar.IFiscalCalendarPeriodService, GeneralLedger.FiscalCalendar.FiscalCalendarPeriodService>();
            services.AddScoped<AccountsReceivable.ICustPostingProfileService, AccountsReceivable.CustPostingProfileService>();
            return services;
        }
    }
}
