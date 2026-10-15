using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using Mapster;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class DimensionGroupMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<EcoResStorageDimensionGroup, StorageDimensionGroupDto>()
            .Map(d => d.Id, s => s.RecId.ToString())
            .Map(d => d.GroupId, s => s.StorageDimensionGroupId);
        config.NewConfig<StorageDimensionGroupInputDto, EcoResStorageDimensionGroup>()
            .Ignore(d => d.RecId)
            .Ignore(d => d.StorageDimensionGroupId)
            .Map(d => d.Name, s => s.Name.Trim());

        config.NewConfig<EcoResTrackingDimensionGroup, TrackingDimensionGroupDto>()
            .Map(d => d.Id, s => s.RecId.ToString())
            .Map(d => d.GroupId, s => s.TrackingDimensionGroupId);
        config.NewConfig<TrackingDimensionGroupInputDto, EcoResTrackingDimensionGroup>()
            .Ignore(d => d.RecId)
            .Ignore(d => d.TrackingDimensionGroupId)
            .Map(d => d.Name, s => s.Name.Trim());
    }
}
