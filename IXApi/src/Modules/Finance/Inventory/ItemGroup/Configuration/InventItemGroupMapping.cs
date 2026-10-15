using System.Globalization;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventItemGroupPostingMappingSource(
    string Id,
    int AccountType,
    long LedgerDimension,
    string MainAccount);

public sealed record InventItemGroupMappingSource(
    InventItemGroup Group,
    IReadOnlyList<InventItemGroupPostingDto> Postings);

public sealed class InventItemGroupMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<InventItemGroupPostingMappingSource, InventItemGroupPostingDto>()
            .Map(destination => destination.LedgerDimension,
                source => source.LedgerDimension.ToString(CultureInfo.InvariantCulture));

        config.NewConfig<InventItemGroupMappingSource, InventItemGroupRecordDto>()
            .Map(destination => destination.Id,
                source => source.Group.RecId.ToString(CultureInfo.InvariantCulture))
            .Map(destination => destination.ItemGroupId,
                source => source.Group.ItemGroupId)
            .Map(destination => destination.Name,
                source => source.Group.Name)
            .Map(destination => destination.Postings,
                source => source.Postings);
    }
}
