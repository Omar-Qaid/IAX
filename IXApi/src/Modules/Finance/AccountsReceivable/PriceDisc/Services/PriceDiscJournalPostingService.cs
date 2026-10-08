using IAX.IXApi.Modules.Finance.Persistence;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.PriceDisc;

public enum PriceDiscJournalPostStatus { NotFound, AlreadyPosted, Invalid, Posted }
public sealed record PriceDiscJournalPostResult(PriceDiscJournalPostStatus Status, List<string>? Errors = null, int PublishedRules = 0);

public sealed class PriceDiscJournalPostingService
{
    private readonly IFinanceDataContext _db;
    private readonly ICompanyExecutionContext _company;

    public PriceDiscJournalPostingService(IFinanceDataContext db, ICompanyExecutionContext company)
    {
        _db = db;
        _company = company;
    }

        public async Task<List<string>?> ValidateAsync(long recId, CancellationToken cancellationToken)
        {
            var header = await FindHeaderAsync(recId, cancellationToken);
            if (header == null) return null;
            var errors = await ValidateJournalAsync(header, cancellationToken);
            return errors;
        }

        public async Task<PriceDiscJournalPostResult> PostAsync(long recId, CancellationToken cancellationToken)
        {
            return await _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable, cancellationToken);
                var header = await FindHeaderAsync(recId, cancellationToken);
                if (header == null) return new PriceDiscJournalPostResult(PriceDiscJournalPostStatus.NotFound);
                if (header.Posted == NoYes.Yes)
                    return new PriceDiscJournalPostResult(PriceDiscJournalPostStatus.AlreadyPosted);

                var errors = await ValidateJournalAsync(header, cancellationToken);
                if (errors.Count > 0)
                    return new PriceDiscJournalPostResult(PriceDiscJournalPostStatus.Invalid, errors);

                var lines = await _db.Set<PriceDiscAdmTrans>()
                    .Where(line => line.DataAreaId == header.DataAreaId && line.JournalNum == header.JournalNum)
                    .OrderBy(line => line.LineNum)
                    .ToListAsync(cancellationToken);
                foreach (var line in lines)
                {
                    var relation = EffectiveRelation(header, line);
                    if (line.MustBeDeleted != 0)
                    {
                        var existing = line.PriceDiscTableRef == 0 ? null : await _db.Set<PriceDiscTable>()
                            .FirstOrDefaultAsync(row => row.DataAreaId == header.DataAreaId
                                && row.RecId == line.PriceDiscTableRef, cancellationToken);
                        if (existing != null) _db.Set<PriceDiscTable>().Remove(existing);
                        continue;
                    }

                    var posted = line.PriceDiscTableRef == 0 ? null : await _db.Set<PriceDiscTable>()
                        .FirstOrDefaultAsync(row => row.DataAreaId == header.DataAreaId
                            && row.RecId == line.PriceDiscTableRef, cancellationToken);
                    var isNewRule = posted == null;
                    if (posted == null)
                    {
                        posted = new PriceDiscTable { DataAreaId = header.DataAreaId };
                        _db.Set<PriceDiscTable>().Add(posted);
                    }
                    CopyToPublishedRule(line, posted, relation);
                    if (isNewRule)
                        await _db.SaveChangesAsync(cancellationToken);
                    line.PriceDiscTableRef = posted.RecId;
                    line.DifferentFromPosted = 0;
                }

                header.Posted = NoYes.Yes;
                header.PostedDate = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return new PriceDiscJournalPostResult(PriceDiscJournalPostStatus.Posted, PublishedRules: lines.Count(line => line.MustBeDeleted == 0));
            });
        }

        private async Task<PriceDiscAdmTable?> FindHeaderAsync(long recId, CancellationToken cancellationToken)
        {
            var dataAreaId = _company.GetDataAreaId();
            return await _db.Set<PriceDiscAdmTable>().FirstOrDefaultAsync(header =>
                header.RecId == recId && header.DataAreaId == dataAreaId, cancellationToken);
        }

        private async Task<List<string>> ValidateJournalAsync(PriceDiscAdmTable header, CancellationToken cancellationToken)
        {
            var errors = new List<string>();
            var lines = await _db.Set<PriceDiscAdmTrans>().AsNoTracking()
                .Where(line => line.DataAreaId == header.DataAreaId && line.JournalNum == header.JournalNum)
                .OrderBy(line => line.LineNum)
                .ToListAsync(cancellationToken);
            if (lines.Count == 0) errors.Add("The journal must contain at least one line.");
            if (!string.IsNullOrWhiteSpace(header.JournalName)
                && !await _db.Set<PriceDiscAdmName>().AnyAsync(name => name.DataAreaId == header.DataAreaId
                    && name.JournalName == header.JournalName, cancellationToken))
                errors.Add($"Journal name '{header.JournalName}' was not found.");

            foreach (var line in lines)
            {
                var relation = EffectiveRelation(header, line);
                if (relation is not (PriceType.PriceSales or PriceType.LineDiscSales
                    or PriceType.MultilineDiscSales or PriceType.EndDiscSales))
                    errors.Add($"Line {line.LineNum}: only sales price and sales discount relations can be posted here.");
                if (line.FromDate != default && line.ToDate != default && line.FromDate.Date > line.ToDate.Date)
                    errors.Add($"Line {line.LineNum}: the start date is after the end date.");
                if (line.QuantityAmountFrom < 0 || line.QuantityAmountTo < 0
                    || (line.QuantityAmountTo > 0 && line.QuantityAmountFrom > line.QuantityAmountTo))
                    errors.Add($"Line {line.LineNum}: the quantity or amount range is invalid.");
                if (line.Module != ModuleInventCustVend.Cust)
                    errors.Add($"Line {line.LineNum}: sales agreements must use the customer module.");
                if (relation == PriceType.PriceSales && line.Amount <= 0)
                    errors.Add($"Line {line.LineNum}: a sales price must be greater than zero.");
                if (relation != PriceType.PriceSales && (line.Amount < 0 || line.Percent1 < 0 || line.Percent1 > 100))
                    errors.Add($"Line {line.LineNum}: discount amount and percentage are invalid.");
                if (line.AccountCode == PriceDiscPartyCodeType.Table
                    && !await _db.Set<CustTable>().AnyAsync(customer => customer.DataAreaId == header.DataAreaId
                        && customer.AccountNum == line.AccountRelation, cancellationToken))
                    errors.Add($"Line {line.LineNum}: customer '{line.AccountRelation}' was not found.");
                if (line.AccountCode == PriceDiscPartyCodeType.GroupId
                    && !await _db.Set<PriceDiscGroup>().AnyAsync(group => group.DataAreaId == header.DataAreaId
                        && group.Module == ModuleInventCustVend.Cust && group.GroupId == line.AccountRelation,
                        cancellationToken))
                    errors.Add($"Line {line.LineNum}: customer discount group '{line.AccountRelation}' was not found.");
                if (line.ItemCode == PriceDiscProductCodeType.Table
                    && !await _db.Set<InventTable>().AnyAsync(item => item.DataAreaId == header.DataAreaId
                        && item.ItemId == line.ItemRelation, cancellationToken))
                    errors.Add($"Line {line.LineNum}: item '{line.ItemRelation}' was not found.");
                if (line.ItemCode == PriceDiscProductCodeType.GroupId
                    && !await _db.Set<PriceDiscGroup>().AnyAsync(group => group.DataAreaId == header.DataAreaId
                        && group.Module == ModuleInventCustVend.Invent && group.GroupId == line.ItemRelation,
                        cancellationToken))
                    errors.Add($"Line {line.LineNum}: item discount group '{line.ItemRelation}' was not found.");
                if (line.AccountCode == PriceDiscPartyCodeType.All && line.AccountRelation != string.Empty)
                    errors.Add($"Line {line.LineNum}: clear AccountRelation for an all-customer rule.");
                if (line.ItemCode == PriceDiscProductCodeType.All && line.ItemRelation != string.Empty)
                    errors.Add($"Line {line.LineNum}: clear ItemRelation for an all-item rule.");
            }
            return errors;
        }

        private static PriceType EffectiveRelation(PriceDiscAdmTable header, PriceDiscAdmTrans line) =>
            line.Relation == PriceType.PriceSales && header.DefaultRelation != PriceType.PriceSales
                ? header.DefaultRelation : line.Relation;

        private static void CopyToPublishedRule(PriceDiscAdmTrans source, PriceDiscTable target, PriceType relation)
        {
            target.ItemCode = source.ItemCode;
            target.ItemRelation = source.ItemRelation;
            target.AccountCode = source.AccountCode;
            target.AccountRelation = source.AccountRelation;
            target.Relation = relation;
            target.Module = source.Module;
            target.Amount = source.Amount;
            target.Markup = source.Markup;
            target.Percent1 = source.Percent1;
            target.Percent2 = source.Percent2;
            target.PriceUnit = source.PriceUnit;
            target.QuantityAmountFrom = source.QuantityAmountFrom;
            target.QuantityAmountTo = source.QuantityAmountTo;
            target.FromDate = source.FromDate;
            target.ToDate = source.ToDate;
            target.CalendarDays = source.CalendarDays;
            target.DeliveryTime = source.DeliveryTime;
            target.DisregardLeadTime = source.DisregardLeadTime;
            target.Currency = source.Currency;
            target.UnitId = source.UnitId;
            target.InventDimId = source.InventDimId;
            target.Agreement = source.Agreement;
            target.PriceGroup = source.PriceGroup;
            target.PdsCalculationId = source.PdsCalculationId;
            target.AllocateMarkup = source.AllocateMarkup;
            target.GenericCurrency = source.GenericCurrency;
            target.SearchAgain = source.SearchAgain;
            target.PriceApplyAdjustment = source.PriceApplyAdjustment;
            target.UnitAppliesToAll = source.UnitAppliesToAll;
            target.IsGupTradeAgreement = source.IsGupTradeAgreement;
            target.MaximumRetailPrice_In = source.MaximumRetailPrice_In;
            target.SubBillFlatTierPrice = source.SubBillFlatTierPrice;
            target.AgreementHeaderExt_Ru = source.AgreementHeaderExt_Ru;
            target.OriginalPriceDiscAdmTransRecId = source.RecId;
            target.PricingRuleHeader = source.PricingRuleHeader;
            target.PricingRuleLine = source.PricingRuleLine;
            target.PriceComponentCombination = source.PriceComponentCombination;
            target.Partition = source.Partition;
            target.DataAreaId = source.DataAreaId;
        }
}
