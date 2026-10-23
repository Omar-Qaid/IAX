using System.Globalization;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using InventModelGroupEntity = IAX.IXApi.Modules.Finance.Entities.InventModelGroup;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventModelGroupService : BaseService<InventModelGroupEntity>, IInventModelGroupService
{
    private readonly ICompanyExecutionContext company;
    private DbContext Db => _unitOfWork.Context;

    public InventModelGroupService(IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        ICompanyExecutionContext company) : base(unitOfWork, currentUser) => this.company = company;

    public async Task<List<InventModelGroupRecordDto>?> ListAsync(CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return null;
        return (await Db.Set<InventModelGroupEntity>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId).OrderBy(x => x.ModelGroupId).ToListAsync(ct)).Select(Map).ToList();
    }

    public async Task<(InventModelGroupRecordDto? Group, string? Error)> CreateAsync(InventModelGroupRecordDto input, CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(companyId)) return (null, "A legal entity must be selected.");
        var id = input.ModelGroupId.Trim().ToUpperInvariant();
        var error = Validate(id, input);
        if (error is not null) return (null, error);
        if (await Db.Set<InventModelGroupEntity>().AnyAsync(x => x.DataAreaId == companyId && x.ModelGroupId == id, ct))
            return (null, $"Item model group '{id}' already exists.");
        var row = new InventModelGroupEntity { ModelGroupId = id, DataAreaId = companyId };
        Apply(row, input);
        Db.Add(row);
        await _unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<(InventModelGroupRecordDto? Group, string? Error)> UpdateAsync(long recId, InventModelGroupRecordDto input, CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        var row = await Db.Set<InventModelGroupEntity>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == companyId, ct);
        if (row is null) return (null, "Item model group was not found.");
        var id = input.ModelGroupId.Trim().ToUpperInvariant();
        var error = Validate(id, input);
        if (error is not null) return (null, error);
        if (row.ModelGroupId != id) return (null, "Item model group ID cannot be changed after creation.");
        if (await Db.Set<InventModelGroupItem>().AnyAsync(x => x.ModelGroupDataAreaId == companyId && x.ModelGroupId == id, ct))
            return (null, "A model group assigned to released products cannot be changed through basic setup.");
        Apply(row, input);
        await _unitOfWork.CompleteAsync(ct);
        return (Map(row), null);
    }

    public async Task<(bool Deleted, string? Error)> DeleteAsync(long recId, CancellationToken ct)
    {
        var companyId = company.GetDataAreaId();
        var row = await Db.Set<InventModelGroupEntity>().FirstOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == companyId, ct);
        if (row is null) return (false, "Item model group was not found.");
        if (await Db.Set<InventModelGroupItem>().AnyAsync(x => x.ModelGroupDataAreaId == companyId && x.ModelGroupId == row.ModelGroupId, ct))
            return (false, "The group is assigned to one or more released products.");
        Db.Remove(row);
        await _unitOfWork.CompleteAsync(ct);
        return (true, null);
    }

    private static NoYes YesNo(bool value) => value ? NoYes.Yes : NoYes.No;
    private static bool Bool(NoYes value) => value == NoYes.Yes;
    private static string? Validate(string id, InventModelGroupRecordDto input) =>
        id.Length == 0 ? "Item model group is required." :
        string.IsNullOrWhiteSpace(input.Name) ? "Name is required." :
        input.InventModel != (int)InventModel.FIFO ? "The basic inventory release supports FIFO only." :
        !input.StockedProduct ? "The basic inventory model group must contain stocked products." :
        input.NegativePhysical || input.NegativeFinancial ? "Negative physical and financial inventory are disabled in the basic release." :
        input.InclPhysicalValueInCost ? "Include physical value in cost is disabled in the basic release." :
        !input.PostOnHandFinancial ? "Financial inventory posting is required in the basic release." :
        input.StandardCost || input.MandatoryWmsOrder || input.QuarantineControl || input.PdsCheck ||
        input.MandatoryRegister || input.MandatoryReceive || input.MandatoryPick || input.MandatoryDeduct ||
        input.IncludePhysicalValueForAverage || input.ItemProdReservationActive ||
        input.ItemProdReservation != (int)ItemProdReservation.None || input.ReserveByDate ||
        input.ReserveReversed || input.McrReservation || !string.IsNullOrWhiteSpace(input.PdsDispositionCode) ||
        input.PdsConsReq || input.PdsReqVendBatchDetail || input.PdsSameLot || input.PdsVendorCheckItem ||
        input.PdsPickCriteria != (int)PdsPickCriteria.None ||
        input.PickingListBatchExpirationDateValidationRule != (int)PickingListBatchExpirationDateValidationRule.None
            ? "Advanced reservation, warehouse, production, and batch controls are outside the basic inventory scope." : null;

    private static void Apply(InventModelGroupEntity row, InventModelGroupRecordDto x)
    {
        row.Name = x.Name.Trim(); row.InventModel = (InventModel)x.InventModel; row.StockedProduct = YesNo(x.StockedProduct);
        row.PostOnHandPhysical = YesNo(x.PostOnHandPhysical); row.PostOnHandFinancial = YesNo(x.PostOnHandFinancial);
        row.PostPhysicalRevenue = YesNo(x.PostPhysicalRevenue); row.PostPhysicalPurchase = YesNo(x.PostPhysicalPurchase);
        row.StandardCost = YesNo(x.StandardCost); row.MandatoryRegister = YesNo(x.MandatoryRegister);
        row.MandatoryReceive = YesNo(x.MandatoryReceive); row.MandatoryPick = YesNo(x.MandatoryPick);
        row.MandatoryDeduct = YesNo(x.MandatoryDeduct); row.MandatoryWmsOrder = YesNo(x.MandatoryWmsOrder);
        row.NegativePhysical = YesNo(x.NegativePhysical); row.NegativeFinancial = YesNo(x.NegativeFinancial);
        row.QuarantineControl = YesNo(x.QuarantineControl); row.InclPhysicalValueInCost = YesNo(x.InclPhysicalValueInCost);
        row.InventCostRecalculationIncludePhysicalValueForAverageModel = YesNo(x.IncludePhysicalValueForAverage);
        row.ItemProdReservationActive = YesNo(x.ItemProdReservationActive); row.ItemProdReservation = (ItemProdReservation)x.ItemProdReservation;
        row.ReserveByDate = YesNo(x.ReserveByDate); row.ReserveReversed = YesNo(x.ReserveReversed);
        row.McrReservation = YesNo(x.McrReservation); row.PdsDispositionCode = x.PdsDispositionCode.Trim();
        row.PdsCheck = YesNo(x.PdsCheck); row.PdsConsReq = YesNo(x.PdsConsReq);
        row.PdsReqVendBatchDetail = YesNo(x.PdsReqVendBatchDetail); row.PdsSameLot = YesNo(x.PdsSameLot);
        row.PdsVendorCheckItem = YesNo(x.PdsVendorCheckItem); row.PdsPickCriteria = (PdsPickCriteria)x.PdsPickCriteria;
        row.PickingListBatchExpirationDateValidationRule = (PickingListBatchExpirationDateValidationRule)x.PickingListBatchExpirationDateValidationRule;
    }

    private static InventModelGroupRecordDto Map(InventModelGroupEntity x) => new()
    {
        Id = x.RecId.ToString(CultureInfo.InvariantCulture), ModelGroupId = x.ModelGroupId, Name = x.Name,
        InventModel = (int)x.InventModel, StockedProduct = Bool(x.StockedProduct), PostOnHandPhysical = Bool(x.PostOnHandPhysical),
        PostOnHandFinancial = Bool(x.PostOnHandFinancial), PostPhysicalRevenue = Bool(x.PostPhysicalRevenue),
        PostPhysicalPurchase = Bool(x.PostPhysicalPurchase), StandardCost = Bool(x.StandardCost), MandatoryRegister = Bool(x.MandatoryRegister),
        MandatoryReceive = Bool(x.MandatoryReceive), MandatoryPick = Bool(x.MandatoryPick), MandatoryDeduct = Bool(x.MandatoryDeduct),
        MandatoryWmsOrder = Bool(x.MandatoryWmsOrder), NegativePhysical = Bool(x.NegativePhysical), NegativeFinancial = Bool(x.NegativeFinancial),
        QuarantineControl = Bool(x.QuarantineControl), InclPhysicalValueInCost = Bool(x.InclPhysicalValueInCost),
        IncludePhysicalValueForAverage = Bool(x.InventCostRecalculationIncludePhysicalValueForAverageModel),
        ItemProdReservationActive = Bool(x.ItemProdReservationActive), ItemProdReservation = (int)x.ItemProdReservation,
        ReserveByDate = Bool(x.ReserveByDate), ReserveReversed = Bool(x.ReserveReversed), McrReservation = Bool(x.McrReservation),
        PdsDispositionCode = x.PdsDispositionCode, PdsCheck = Bool(x.PdsCheck), PdsConsReq = Bool(x.PdsConsReq),
        PdsReqVendBatchDetail = Bool(x.PdsReqVendBatchDetail), PdsSameLot = Bool(x.PdsSameLot),
        PdsVendorCheckItem = Bool(x.PdsVendorCheckItem), PdsPickCriteria = (int)x.PdsPickCriteria,
        PickingListBatchExpirationDateValidationRule = (int)x.PickingListBatchExpirationDateValidationRule
    };
}
