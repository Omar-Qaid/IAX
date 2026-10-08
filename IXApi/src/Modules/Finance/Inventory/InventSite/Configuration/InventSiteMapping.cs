using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventSiteMappingSource(InventSite Site, List<InventLocation> Locations);

public sealed class InventSiteMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SiteInputDto, InventSite>()
            .IgnoreNonMapped(true)
            .Map(d => d.Name, s => s.Name.Trim())
            .Map(d => d.DefaultInventStatusID, s => s.DefaultInventStatusId.Trim())
            .Map(d => d.TimeZone, s => (Timezone)s.TimeZone)
            .Map(d => d.IsReceivingWarehouseOverrideAllowed,
                s => s.IsReceivingWarehouseOverrideAllowed ? NoYes.Yes : NoYes.No)
            .Map(d => d.DefaultDimension, s => s.DefaultDimension);

        config.NewConfig<InventSiteMappingSource, InventSiteDto>()
            .Map(d => d.Id, s => s.Site.RecId.ToString())
            .Map(d => d.RecId, s => s.Site.RecId)
            .Map(d => d.SiteId, s => s.Site.SiteId)
            .Map(d => d.Name, s => s.Site.Name)
            .Map(d => d.DefaultInventStatusId, s => s.Site.DefaultInventStatusID)
            .Map(d => d.TimeZone, s => (int)s.Site.TimeZone)
            .Map(d => d.IsReceivingWarehouseOverrideAllowed,
                s => s.Site.IsReceivingWarehouseOverrideAllowed == NoYes.Yes)
            .Map(d => d.DefaultDimension, s => s.Site.DefaultDimension)
            .Map(d => d.Warehouses, s => s.Locations);

        config.NewConfig<InventLocation, InventSiteWarehouseDto>()
            .Map(d => d.Id, s => s.RecId.ToString());
    }
}
