using IAX.IXApi.Infrastructure.Persistence.Seeding.Core;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.AccountsPayable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Inventory.DimensionGroups.Entities;
using IAX.IXApi.Modules.Administration.NumberSequences;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Services;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

internal static class EcoResTrackingDimensionGroupItemDemoData
{
    internal static EcoResTrackingDimensionGroupItem Create(long trackingId, string code) =>
        new EcoResTrackingDimensionGroupItem { ItemId = code, ItemDataAreaId = "dat", TrackingDimensionGroup = trackingId, DataAreaId = "dat" };
}
