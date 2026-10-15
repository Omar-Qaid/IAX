using System.Globalization;
using IAX.IXApi.Modules.Finance.Entities;
using Mapster;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed record InventPostingMappingSource(InventPosting Posting, string MainAccount);

public sealed class InventPostingMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<InventPostingMappingSource, InventPostingRuleDto>()
            .Map(destination => destination.Id,
                source => source.Posting.RecId.ToString(CultureInfo.InvariantCulture))
            .Map(destination => destination.InventAccountType,
                source => (int)source.Posting.InventAccountType)
            .Map(destination => destination.ItemCode,
                source => (int)source.Posting.ItemCode)
            .Map(destination => destination.ItemRelation,
                source => source.Posting.ItemRelation)
            .Map(destination => destination.CategoryRelation,
                source => source.Posting.CategoryRelation.ToString(CultureInfo.InvariantCulture))
            .Map(destination => destination.CustVendCode,
                source => (int)source.Posting.CustVendCode)
            .Map(destination => destination.CustVendRelation,
                source => source.Posting.CustVendRelation)
            .Map(destination => destination.TaxGroupId,
                source => source.Posting.TaxGroupId)
            .Map(destination => destination.LedgerDimension,
                source => source.Posting.LedgerDimension.ToString(CultureInfo.InvariantCulture))
            .Map(destination => destination.MainAccount,
                source => source.MainAccount);
    }
}
