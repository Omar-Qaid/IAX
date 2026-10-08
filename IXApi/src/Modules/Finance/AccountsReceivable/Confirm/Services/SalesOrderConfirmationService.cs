using System.Data;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Shared.Application.Identity;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Confirm;

public sealed class SalesOrderConfirmationService(
    IUnitOfWork unitOfWork, ISysNumberSequenceService numberSequences,
    ICompanyExecutionContext company, IValidator<PostConfirmationRequest> validator) : ISalesOrderConfirmationService
{
    private DbContext db => unitOfWork.Context;
    private const int SalesTableDocumentId = 2002;
    private const int SalesLineDocumentId = 2003;

    public async Task<ConfirmationResult> ListAsync(long recId, CancellationToken ct)
    {
        var order = await FindOrderAsync(recId, ct);
        if (order == null) return new(404, null, "Sales order was not found in the selected company.");
        var rows = await db.Set<CustConfirmJour>().AsNoTracking()
            .Where(x => x.DataAreaId == order.DataAreaId && x.SalesId == order.SalesId)
            .OrderByDescending(x => x.ConfirmDate).ThenByDescending(x => x.RecId)
            .ToListAsync(ct);
        return new(200, rows.Adapt<List<ConfirmationListItemDto>>());
    }

    public async Task<ConfirmationResult> GetAsync(long recId, long confirmationRecId, CancellationToken ct)
    {
        var order = await FindOrderAsync(recId, ct);
        if (order == null) return new(404, null, "Sales order was not found in the selected company.");
        var journal = await db.Set<CustConfirmJour>().AsNoTracking().SingleOrDefaultAsync(
            x => x.RecId == confirmationRecId && x.DataAreaId == order.DataAreaId && x.SalesId == order.SalesId, ct);
        if (journal == null) return new(404, null, "Confirmation was not found.");
        var lines = await db.Set<CustConfirmTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == journal.DataAreaId && x.ConfirmId == journal.ConfirmId
                && x.SalesId == journal.SalesId).ToListAsync(ct);
        return new(200, new { header = Present(journal), lines = lines.OrderBy(x => x.LineNum).Select(Present) });
    }

    public async Task<ConfirmationResult> PostAsync(long recId, PostConfirmationRequest? request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request ?? new PostConfirmationRequest(), ct);
        if (!validation.IsValid)
            return new(400, null, validation.Errors[0].ErrorMessage);
        var confirmationDate = request?.ConfirmationDate?.Date ?? DateTime.UtcNow.Date;
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var order = await FindOrderAsync(recId, ct);
            if (order == null) return new ConfirmationResult(404, null, "Sales order was not found in the selected company.");
            if (order.SalesStatus != SalesStatus.Backorder)
                return new ConfirmationResult(422, null, "Only an open sales order can be confirmed.");
            if (string.IsNullOrWhiteSpace(order.CustAccount) || string.IsNullOrWhiteSpace(order.CurrencyCode))
                return new ConfirmationResult(422, null, "Customer and currency are required for confirmation.");
            var customer = await db.Set<CustTable>().AsNoTracking().FirstOrDefaultAsync(
                x => x.DataAreaId == order.DataAreaId && x.AccountNum == order.CustAccount, ct);
            if (customer == null) return new ConfirmationResult(422, null, "The sales order customer was not found.");
            if (customer.Blocked == CustVendorBlocked.All)
                return new ConfirmationResult(422, null, "The customer is blocked for sales order confirmation.");
            var invoiceAccount = string.IsNullOrWhiteSpace(order.InvoiceAccount) ? order.CustAccount : order.InvoiceAccount;
            var invoiceCustomer = invoiceAccount == customer.AccountNum ? customer : await db.Set<CustTable>()
                .AsNoTracking().FirstOrDefaultAsync(x => x.DataAreaId == order.DataAreaId && x.AccountNum == invoiceAccount, ct);
            if (invoiceCustomer == null) return new ConfirmationResult(422, null, "The invoice customer was not found.");
            if (invoiceCustomer.Blocked == CustVendorBlocked.All)
                return new ConfirmationResult(422, null, "The invoice customer is blocked for sales order confirmation.");
            var parameters = await db.Set<CustParameters>().AsNoTracking()
                .Where(x => x.DataAreaId == order.DataAreaId).OrderBy(x => x.Key).FirstOrDefaultAsync(ct);
            if (customer.MandatoryCreditLimit == NoYes.Yes || invoiceCustomer.MandatoryCreditLimit == NoYes.Yes
                || parameters?.CreditLimitCheck > 0)
                return new ConfirmationResult(422, null,
                    "Credit limit validation is required, but a confirmation credit calculation is not configured.");
            var lines = await db.Set<SalesLine>().AsNoTracking()
                .Where(x => x.DataAreaId == order.DataAreaId && x.SalesId == order.SalesId).ToListAsync(ct);
            lines = lines.OrderBy(x => x.LineNum).ToList();
            if (lines.Count == 0 || lines.Any(x => x.SalesStatus != SalesStatus.Backorder
                || x.SalesQty <= 0 || x.PriceUnit <= 0 || x.LineAmount < 0))
                return new ConfirmationResult(422, null,
                    "Confirmation requires open lines with valid quantity, price unit, and amount.");
            var calculated = await CalculateAsync(order, lines, ct);
            var confirmId = await NextAvailableConfirmIdAsync(order.DataAreaId, ct);
            var journal = CreateJournal(order, lines, calculated, confirmId, confirmationDate, invoiceAccount);
            db.Set<CustConfirmJour>().Add(journal);
            foreach (var line in lines)
                db.Set<CustConfirmTrans>().Add(CreateLine(order, line, calculated, confirmId, confirmationDate));
            order.DocumentStatus = DocumentStatus.Confirmation;
            await unitOfWork.CompleteAsync(ct);
            await transaction.CommitAsync(ct);
            return new ConfirmationResult(200, Present(journal), "Sales order confirmed.");
        });
    }

    private Task<SalesTable?> FindOrderAsync(long recId, CancellationToken ct) => db.Set<SalesTable>()
        .SingleOrDefaultAsync(x => x.RecId == recId && x.DataAreaId == company.GetDataAreaId(), ct);

    private async Task<string> NextAvailableConfirmIdAsync(string area, CancellationToken ct)
    {
        for (var attempt = 0; attempt < 10_000; attempt++)
        {
            var code = (await numberSequences.NextAsync("CustConfirmJour", area, ct)).Code;
            if (!await db.Set<CustConfirmJour>().IgnoreQueryFilters().AsNoTracking()
                .AnyAsync(x => x.ConfirmId == code, ct)) return code;
        }
        throw new InvalidOperationException("The confirmation number sequence could not produce an unused ConfirmId.");
    }

    private sealed record Calculation(decimal Gross, decimal LineDiscount, decimal MultiLineDiscount,
        decimal OrderDiscount, decimal Subtotal, decimal Charges, decimal Tax, decimal Total,
        Dictionary<long, decimal> LineTax, Dictionary<long, decimal> LineOrderDiscount,
        Dictionary<long, decimal> LineCharges);

    private async Task<Calculation> CalculateAsync(SalesTable order, List<SalesLine> lines, CancellationToken ct)
    {
        var lineIds = lines.Select(x => x.RecId).ToList();
        var charges = await db.Set<MarkupTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == order.DataAreaId && x.IsDeleted != NoYes.Yes
                && (x.ModuleType == MarkupModuleType.Customer || x.ModuleType == MarkupModuleType.Sales)
                && ((x.TransRecId == order.RecId && (x.TransTableId == SalesTableDocumentId || x.TransTableId == 0))
                    || (lineIds.Contains(x.TransRecId) && x.TransTableId == SalesLineDocumentId)))
            .ToListAsync(ct);
        var groupIds = lines.Select(x => x.TaxGroup).Concat(charges.Select(x => x.TaxGroup)).Where(x => x != "").Distinct().ToList();
        var itemGroupIds = lines.Select(x => x.TaxItemGroup).Concat(charges.Select(x => x.TaxItemGroup)).Where(x => x != "").Distinct().ToList();
        var groupRows = await db.Set<TaxGroupData>().AsNoTracking()
            .Where(x => x.DataAreaId == order.DataAreaId && groupIds.Contains(x.TaxGroup)).ToListAsync(ct);
        var itemRows = await db.Set<TaxOnItem>().AsNoTracking()
            .Where(x => x.DataAreaId == order.DataAreaId && itemGroupIds.Contains(x.TaxItemGroup)).ToListAsync(ct);
        var codes = groupRows.Select(x => x.TaxCode).Intersect(itemRows.Select(x => x.TaxCode)).Distinct().ToList();
        var rates = await db.Set<TaxData>().AsNoTracking()
            .Where(x => x.DataAreaId == order.DataAreaId && codes.Contains(x.TaxCode)).ToListAsync(ct);
        var modules = await db.Set<InventTableModule>().AsNoTracking()
            .Where(x => x.DataAreaId == order.DataAreaId && x.ModuleType == ModuleInventPurchSales.Sales
                && lines.Select(line => line.ItemId).Contains(x.ItemId)).ToListAsync(ct);
        var eligible = lines.Where(x => modules.Any(m => m.ItemId == x.ItemId && m.EndDisc == NoYes.Yes))
            .Select(x => x.RecId).ToHashSet();
        var eligibleNet = lines.Where(x => eligible.Contains(x.RecId))
            .Sum(x => Math.Max(0m, x.LineAmount - x.LineDisc - x.MultiLnDisc));
        var orderDiscount = eligibleNet * order.DiscPercent / 100m;
        decimal tax = 0, chargeTotal = 0;
        var lineTax = new Dictionary<long, decimal>();
        var lineDiscount = new Dictionary<long, decimal>();
        var lineCharges = new Dictionary<long, decimal>();
        decimal Rate(string group, string itemGroup)
        {
            var applicable = groupRows.Where(x => x.TaxGroup == group && x.ExemptTax != NoYes.Yes)
                .Select(x => x.TaxCode)
                .Intersect(itemRows.Where(x => x.TaxItemGroup == itemGroup).Select(x => x.TaxCode));
            return applicable.Distinct().Sum(code => rates.Where(x => x.TaxCode == code
                    && (x.TaxFromDate == default || x.TaxFromDate.Date <= order.OrderDate.Date)
                    && (x.TaxToDate == default || x.TaxToDate.Date >= order.OrderDate.Date))
                .OrderByDescending(x => x.TaxFromDate).FirstOrDefault()?.TaxValue ?? 0m);
        }
        decimal Tax(decimal amount, decimal rate) => order.InclTax && rate > 0
            ? amount * rate / (100m + rate) : amount * rate / 100m;
        foreach (var line in lines)
        {
            var net = Math.Max(0m, line.LineAmount - line.LineDisc - line.MultiLnDisc);
            var allocated = eligible.Contains(line.RecId) && eligibleNet > 0 ? orderDiscount * net / eligibleNet : 0m;
            lineDiscount[line.RecId] = allocated;
            var amount = Tax(Math.Max(0m, net - allocated), Rate(line.TaxGroup, line.TaxItemGroup));
            lineTax[line.RecId] = amount;
            tax += amount;
        }
        foreach (var charge in charges)
        {
            var amount = charge.CalculatedAmount != 0m ? charge.CalculatedAmount : charge.Value;
            chargeTotal += amount;
            tax += Tax(amount, Rate(charge.TaxGroup, charge.TaxItemGroup));
            if (charge.TransTableId == SalesLineDocumentId)
                lineCharges[charge.TransRecId] = lineCharges.GetValueOrDefault(charge.TransRecId) + amount;
        }
        var gross = lines.Sum(x => x.LineAmount);
        var lineDisc = lines.Sum(x => x.LineDisc);
        var multiDisc = lines.Sum(x => x.MultiLnDisc);
        var subtotal = gross - lineDisc - multiDisc - orderDiscount - (order.InclTax ? tax : 0m);
        return new Calculation(gross, lineDisc, multiDisc, orderDiscount, subtotal,
            chargeTotal, tax, subtotal + chargeTotal + tax, lineTax, lineDiscount, lineCharges);
    }

    private static CustConfirmJour CreateJournal(SalesTable order, List<SalesLine> lines,
        Calculation totals, string confirmId, DateTime date, string invoiceAccount) => new()
    {
        DataAreaId = order.DataAreaId, ConfirmId = confirmId, ConfirmDocNum = confirmId,
        ConfirmDate = date, SalesId = order.SalesId, LanguageId = order.LanguageId,
        OrderAccount = order.CustAccount, InvoiceAccount = invoiceAccount, CustGroup = order.CustGroup,
        DeliveryPostalAddress = order.DeliveryPostalAddress, DeliveryName = order.DeliveryName,
        DlvMode = order.DlvMode, DlvTerm = order.DlvTerm, CurrencyCode = order.CurrencyCode,
        Payment = order.PaymTerm, DefaultDimension = order.DefaultDimension,
        ConfirmAmount = totals.Total, SalesBalance = totals.Subtotal,
        SumLineDisc = totals.LineDiscount + totals.MultiLineDiscount, EndDisc = totals.OrderDiscount,
        SumMarkup = totals.Charges, SumTax = totals.Tax, Qty = lines.Sum(x => x.SalesQty),
        CostValue = lines.Sum(x => x.CostPrice * x.SalesQty), InclTax = order.InclTax ? 1 : 0,
        CashDiscPercent = order.CashDiscPercent, WorkerSalesTaker = order.WorkerSalesTaker
    };

    private static CustConfirmTrans CreateLine(SalesTable order, SalesLine line, Calculation totals,
        string confirmId, DateTime date) => new()
    {
        DataAreaId = order.DataAreaId, ConfirmId = confirmId, ConfirmDate = date,
        SalesId = order.SalesId, OrigSalesId = order.SalesId, LineNum = line.LineNum,
        Name = line.Name, ItemId = line.ItemId, SalesUnit = line.SalesUnit,
        PriceUnit = line.PriceUnit, SalesPrice = line.SalesPrice,
        SalesMarkup = line.SalesMarkup + totals.LineCharges.GetValueOrDefault(line.RecId),
        StockedProduct = line.StockedProduct, InventDimId = line.InventDimId,
        InventTransId = line.InventTransId, DlvDate = line.ReceiptDateRequested,
        DlvTerm = line.DlvTerm, Qty = line.SalesQty, InventQty = line.QtyOrdered,
        LineAmount = Math.Max(0m, line.LineAmount - line.LineDisc - line.MultiLnDisc
            - totals.LineOrderDiscount.GetValueOrDefault(line.RecId)),
        LineDisc = line.LineDisc, LinePercent = line.LinePercent,
        DiscAmount = line.LineDisc + line.MultiLnDisc + totals.LineOrderDiscount.GetValueOrDefault(line.RecId),
        DiscPercent = line.LinePercent + line.MultiLnPercent,
        MultiLnDisc = line.MultiLnDisc, MultiLnPercent = line.MultiLnPercent,
        TaxGroup = line.TaxGroup, TaxItemGroup = line.TaxItemGroup,
        TaxAmount = totals.LineTax.GetValueOrDefault(line.RecId), CurrencyCode = line.CurrencyCode,
        DefaultDimension = line.DefaultDimension, SalesCategory = line.SalesCategory,
        SalesGroup = line.SalesGroup
    };

    private static ConfirmationHeaderDto Present(CustConfirmJour journal) => journal.Adapt<ConfirmationHeaderDto>();

    private static ConfirmationLineDto Present(CustConfirmTrans line) => line.Adapt<ConfirmationLineDto>();
}
