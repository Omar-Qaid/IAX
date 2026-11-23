using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Contracts;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using IAX.IXApi.Modules.Administration.Settings;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Modules.Administration;

/// <summary>
/// The application settings row. It is a single installation-wide record, so it is written once and a
/// rerun leaves an administrator's edits exactly as they are. The defaults are the ones the
/// application already shipped with, so the port does not silently change a running installation.
/// </summary>
public sealed class SysSettingsSeeder : IDataSeeder
{
    public string Id => "ERPV2.SysSettingsSeeder";
    public IReadOnlyCollection<string> Dependencies => ["ERPV2.CurrencySeeder"];

    public const string DefaultCurrency = "USD";

    public async Task SeedAsync(SeedContext c, CancellationToken ct)
    {
        if (await c.Db.Set<SysSettings>().IgnoreQueryFilters().AnyAsync(x => !x.IsDeleted, ct)) return;
        c.Db.Set<SysSettings>().Add(new SysSettings
        {
            AppName = "HBMC ERP",
            DefaultLanguage = "en",
            TimeZone = "UTC",
            Currency = DefaultCurrency,
            DateFormat = "YYYY-MM-DD",
            EnableAuditLog = true,
            MaxUploadSize = 10485760,
            PaginationSize = 10,
            DecimalPlaces = 2,
            IsActive = true,
            DataAreaId = c.Company,
            CreatedBy = c.Actor,
            LastModifiedBy = c.Actor,
            OwnerAccountId = c.Actor
        });
        await c.Db.SaveChangesAsync(ct);
    }
}
