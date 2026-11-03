using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.AccountsPayable;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.CycleErpExamples.Inventory;

internal static class InventModelGroupDemoData
{
    internal static InventModelGroup PurchaseAccrual() => new()
    {
        DataAreaId = "dat", ModelGroupId = AccountsPayableScenarioDemoData.ModelGroup,
        Name = "ERP AP physical receipt and financial invoice example", InventModel = InventModel.FIFO,
        StockedProduct = NoYes.Yes, PostOnHandPhysical = NoYes.Yes, PostOnHandFinancial = NoYes.Yes,
        PostPhysicalPurchase = NoYes.Yes, MandatoryReceive = NoYes.Yes,
        NegativePhysical = NoYes.No, NegativeFinancial = NoYes.No, CreatedBy = "sys", OwnerAccountId = "sys"
    };
}
