using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Mapster;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class DimensionGroupMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<EcoResStorageDimensionGroup, StorageDimensionGroupDto>()
            .Map(d => d.Id, s => s.RecId.ToString())
            .Map(d => d.GroupId, s => s.Name)
            .Map(d => d.Name, s => s.Description)
            .Ignore(d => d.UseSite).Ignore(d => d.UseWarehouse).Ignore(d => d.UseLocation);
        config.NewConfig<StorageDimensionGroupInputDto, EcoResStorageDimensionGroup>()
            .Ignore(d => d.RecId)
            .Ignore(d => d.Name)
            .Map(d => d.Description, s => s.Name.Trim());

        config.NewConfig<EcoResTrackingDimensionGroup, TrackingDimensionGroupDto>()
            .Map(d => d.Id, s => s.RecId.ToString())
            .Map(d => d.GroupId, s => s.Name)
            .Map(d => d.Name, s => s.Description)
            .Ignore(d => d.UseBatchNumber).Ignore(d => d.UseSerialNumber);
        config.NewConfig<TrackingDimensionGroupInputDto, EcoResTrackingDimensionGroup>()
            .Ignore(d => d.RecId)
            .Ignore(d => d.Name)
            .Map(d => d.Description, s => s.Name.Trim());
    }
}
