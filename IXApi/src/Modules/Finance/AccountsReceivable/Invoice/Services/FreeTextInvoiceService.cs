using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.AccountsReceivable.Settlement;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Invoice;

public sealed class FreeTextInvoiceService(
    IUnitOfWork unitOfWork,
    ICompanyExecutionContext company,
    ISysNumberSequenceService numbers,
    ICurrentUserService currentUser) : IFreeTextInvoiceService
{
    private DbContext Db => unitOfWork.Context;

    public FreeTextInvoiceService(IUnitOfWork unitOfWork, ICompanyExecutionContext company,
        ISysNumberSequenceService numbers)
        : this(unitOfWork, company, numbers, NullCurrentUserService.Instance)
    {
    }

    public async Task<IReadOnlyList<FreeTextInvoiceDto>> ListAsync(CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        var headers = await Db.Set<CustInvoiceTable>().AsNoTracking()
            .Where(x => x.DataAreaId == area)
            .OrderByDescending(x => x.InvoiceDate)
            .ThenByDescending(x => x.RecId)
            .ToListAsync(ct);
        if (headers.Count == 0) return [];
        var ids = headers.Select(x => x.RecId).ToList();
        var lines = await Db.Set<CustInvoiceLine>().AsNoTracking()
            .Where(x => x.DataAreaId == area && ids.Contains(x.ParentRecId))
            .ToListAsync(ct);
        return headers.Select(header => Map(header, lines.Where(x => x.ParentRecId == header.RecId))).ToList();
    }

    public async Task<FreeTextInvoiceDto?> GetAsync(long recId, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        var header = await Db.Set<CustInvoiceTable>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
        if (header == null) return null;
        var lines = await Db.Set<CustInvoiceLine>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.ParentRecId == recId)
            .ToListAsync(ct);
        return Map(header, lines);
    }

    public Task<FreeTextInvoiceResult> CreateAsync(FreeTextInvoiceSaveDto input, CancellationToken ct) =>
        SaveAsync(null, input, ct);

    public Task<FreeTextInvoiceResult> CreateCorrectionAsync(long originalRecId, FreeTextInvoiceSaveDto input, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => SaveAsync(null, input, ct, originalRecId));

    public Task<FreeTextInvoiceResult> UpdateAsync(long recId, FreeTextInvoiceSaveDto input, CancellationToken ct) =>
        SaveAsync(recId, input, ct);

    private async Task<FreeTextInvoiceResult> SaveAsync(long? recId, FreeTextInvoiceSaveDto input, CancellationToken ct,
        long? originalRecId = null)
    {
        var area = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var support = new SubledgerPostingSupport(Db);
            const int correctionType = (int)InvoiceAssociationType.CorrectedInvoice;
            CustInvoiceTable? original = null;
            CustInvoiceTable header;
            if (recId.HasValue)
            {
                await support.LockAsync($"FreeTextInvoice:{area}:{recId.Value}", ct);
                header = await Db.Set<CustInvoiceTable>()
                    .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId.Value, ct)
                    ?? throw new FreeTextInvoiceInputException("Free text invoice was not found.", 404);
                if (header.Posted == NoYes.Yes)
                    throw new FreeTextInvoiceInputException("A posted free text invoice cannot be changed.");
                originalRecId = await Db.Set<CustRelatedInvoice>().Where(x => x.DataAreaId == area
                    && x.CustInvoice == header.RecId && x.InvoiceAssociationType == correctionType)
                    .Select(x => x.ParentCustInvoice).SingleOrDefaultAsync(ct);
            }
            else
            {
                if (originalRecId.HasValue)
                {
                    await support.LockAsync($"FreeTextInvoice:{area}:{originalRecId.Value}", ct);
                    if (await Db.Set<CustRelatedInvoice>().AnyAsync(x => x.DataAreaId == area
                        && x.ParentCustInvoice == originalRecId && x.InvoiceAssociationType == correctionType, ct))
                        throw new FreeTextInvoiceInputException("A correction already exists for this free text invoice.");
                }
                header = new CustInvoiceTable { DataAreaId = area };
                Db.Set<CustInvoiceTable>().Add(header);
            }

            var customerAccount = input.CustomerAccount.Trim();
            var customer = await Db.Set<CustTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == customerAccount, ct)
                ?? throw new FreeTextInvoiceInputException("The customer was not found in the selected company.");
            if (customer.Blocked is CustVendorBlocked.All or CustVendorBlocked.Invoicing)
                throw new FreeTextInvoiceInputException("The customer is blocked for invoicing.");

            var invoiceAccount = Normalize(input.InvoiceAccount,
                string.IsNullOrWhiteSpace(customer.InvoiceAccount) ? customer.AccountNum : customer.InvoiceAccount);
            var invoiceCustomer = await Db.Set<CustTable>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == invoiceAccount, ct)
                ?? throw new FreeTextInvoiceInputException("The invoice account was not found in the selected company.");
            if (invoiceCustomer.Blocked is CustVendorBlocked.All or CustVendorBlocked.Invoicing)
                throw new FreeTextInvoiceInputException("The invoice account is blocked for invoicing.");

            var date = (input.InvoiceDate ?? DateTime.UtcNow).Date;
            var currency = Normalize(input.CurrencyCode, customer.CurrencyCode);
            if (originalRecId.HasValue)
            {
                original = await Db.Set<CustInvoiceTable>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area
                    && x.RecId == originalRecId && x.Posted == NoYes.Yes, ct)
                    ?? throw new FreeTextInvoiceInputException("The posted original free text invoice was not found.", 404);
                if (customer.AccountNum != original.OrderAccount || invoiceCustomer.AccountNum != original.InvoiceAccount
                    || currency != original.CurrencyCode || date < original.InvoiceDate.Date)
                    throw new FreeTextInvoiceInputException("A correction must keep the original customer, invoice account and currency and cannot be backdated before the original invoice.");
            }
            if (currency.Length == 0 || !await Db.Set<Currency>().AsNoTracking()
                    .AnyAsync(x => x.DataAreaId == area && x.CurrencyCode == currency, ct))
                throw new FreeTextInvoiceInputException("The invoice currency was not found in the selected company.");
            var paymentTerms = Normalize(input.PaymentTerms, customer.PaymTermId);
            var dueDate = date;
            if (paymentTerms.Length > 0)
            {
                var term = await Db.Set<PaymTerm>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DataAreaId == area && x.PaymTermId == paymentTerms, ct)
                    ?? throw new FreeTextInvoiceInputException("The payment terms were not found in the selected company.");
                try { dueDate = Foundation.PaymentTerms.PaymentDueDateCalculator.Calculate(date, term); }
                catch (InvalidOperationException exception) { throw new FreeTextInvoiceInputException(exception.Message); }
            }
            var paymentMethod = Normalize(input.PaymentMethod, customer.PaymModeId);
            if (paymentMethod.Length > 0 && !await Db.Set<CustPaymModeTable>().AsNoTracking()
                    .AnyAsync(x => x.DataAreaId == area && x.PaymMode == paymentMethod, ct))
                throw new FreeTextInvoiceInputException("The payment method was not found in the selected company.");
            var taxGroup = Normalize(input.TaxGroup, customer.TaxGroupId);
            if (taxGroup.Length > 0 && !await Db.Set<TaxGroupHeading>().AsNoTracking()
                    .AnyAsync(x => x.DataAreaId == area && x.TaxGroup == taxGroup, ct))
                throw new FreeTextInvoiceInputException("The sales tax group was not found in the selected company.");
            var postingProfile = input.PostingProfile?.Trim() ?? string.Empty;
            if (postingProfile.Length == 0)
                postingProfile = await Db.Set<CustParameters>().AsNoTracking()
                    .Where(x => x.DataAreaId == area).Select(x => x.CustPostingProfile).FirstOrDefaultAsync(ct)
                    ?? string.Empty;

            header.InvoiceId = string.Empty;
            header.InvoiceDate = date;
            header.DocumentDate = date;
            header.OrderAccount = customer.AccountNum;
            header.InvoiceAccount = invoiceCustomer.AccountNum;
            header.OrderAccountRefRecId = customer.RecId;
            header.CustGroup = invoiceCustomer.CustGroupId;
            header.CurrencyCode = currency;
            header.Payment = paymentTerms;
            header.PaymMode = paymentMethod;
            header.DueDate = dueDate;
            header.PostingProfile = postingProfile;
            header.DefaultDimension = input.DefaultDimension ?? customer.DefaultDimension;
            header.TaxGroup = taxGroup;
            header.Name = await ResolveDescriptionAsync(input.Description, customer, ct);
            header.Posted = NoYes.No;

            await unitOfWork.CompleteAsync(ct);
            if (original != null && !recId.HasValue)
                Db.Add(new CustRelatedInvoice
                {
                    DataAreaId = area, CustInvoice = header.RecId, ParentCustInvoice = original.RecId,
                    OriginalCustInvoice = original.RecId, InvoiceAssociationType = correctionType
                });
            var previous = await Db.Set<CustInvoiceLine>()
                .Where(x => x.DataAreaId == area && x.ParentRecId == header.RecId).ToListAsync(ct);
            if (previous.Count > 0) Db.Set<CustInvoiceLine>().RemoveRange(previous);
            if (input.Lines.Count == 0)
                throw new FreeTextInvoiceInputException("At least one free text invoice line is required.");

            var lineNum = 0m;
            var savedLines = new List<CustInvoiceLine>();
            foreach (var inputLine in input.Lines)
            {
                if (inputLine.Quantity <= 0 || decimal.Round(inputLine.Quantity, 4) != inputLine.Quantity)
                    throw new FreeTextInvoiceInputException("Line quantity must be positive with at most four decimals.");
                if (inputLine.UnitPrice < 0 || decimal.Round(inputLine.UnitPrice, 4) != inputLine.UnitPrice)
                    throw new FreeTextInvoiceInputException("Line unit price cannot be negative and must have at most four decimals.");
                if (string.IsNullOrWhiteSpace(inputLine.Description))
                    throw new FreeTextInvoiceInputException("Line description is required.");
                await support.LoadDimensionAsync(area, inputLine.LedgerDimension, "free text invoice line", ct);
                var lineTaxGroup = Normalize(inputLine.TaxGroup, taxGroup);
                var itemTaxGroup = inputLine.TaxItemGroup?.Trim() ?? string.Empty;
                if (itemTaxGroup.Length > 0 && !await Db.Set<TaxItemGroupHeading>().AsNoTracking()
                        .AnyAsync(x => x.DataAreaId == area && x.TaxItemGroup == itemTaxGroup, ct))
                    throw new FreeTextInvoiceInputException("The item sales tax group was not found in the selected company.");
                if (itemTaxGroup.Length > 0 && lineTaxGroup.Length == 0)
                    throw new FreeTextInvoiceInputException("A sales tax group is required when an item sales tax group is selected.");
                var amount = decimal.Round(inputLine.Quantity * inputLine.UnitPrice, 2, MidpointRounding.AwayFromZero);
                var codes = await support.ApplicableTaxCodesAsync(area, lineTaxGroup, itemTaxGroup, date, ct);
                var tax = decimal.Round(amount * codes.Sum(x => x.Rate) / 100m, 2, MidpointRounding.AwayFromZero);
                var savedLine = new CustInvoiceLine
                {
                    DataAreaId = area,
                    ParentRecId = header.RecId,
                    LineNum = ++lineNum,
                    Description = inputLine.Description.Trim(),
                    Quantity = inputLine.Quantity,
                    UnitPrice = inputLine.UnitPrice,
                    AmountCur = amount,
                    LedgerDimension = inputLine.LedgerDimension,
                    DefaultDimension = inputLine.DefaultDimension ?? header.DefaultDimension,
                    TaxGroup = lineTaxGroup,
                    TaxItemGroup = itemTaxGroup,
                    TaxAmount = tax,
                    TaxAutoGenerated = NoYes.Yes
                };
                Db.Set<CustInvoiceLine>().Add(savedLine);
                savedLines.Add(savedLine);
            }

            await unitOfWork.CompleteAsync(ct);
            var result = Map(header, savedLines);
            await unitOfWork.CommitTransactionAsync(ct);
            return new(recId.HasValue ? 200 : 201, result,
                recId.HasValue ? "Free text invoice updated." : "Free text invoice created.");
        }
        catch (FreeTextInvoiceInputException exception)
        {
            return await FailAsync(exception.StatusCode, exception.Message, ct);
        }
        catch (SubledgerPostingException exception)
        {
            return await FailAsync(422, exception.Message, ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task<FreeTextInvoiceResult> DeleteAsync(long recId, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var support = new SubledgerPostingSupport(Db);
            await support.LockAsync($"FreeTextInvoice:{area}:{recId}", ct);
            var header = await Db.Set<CustInvoiceTable>()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
            if (header == null) return await FailAsync(404, "Free text invoice was not found.", ct);
            if (header.Posted == NoYes.Yes) return await FailAsync(422, "A posted free text invoice cannot be deleted.", ct);
            var lines = await Db.Set<CustInvoiceLine>()
                .Where(x => x.DataAreaId == area && x.ParentRecId == recId).ToListAsync(ct);
            var correctionLinks = await Db.Set<CustRelatedInvoice>().Where(x => x.DataAreaId == area
                && x.CustInvoice == recId && x.InvoiceAssociationType == (int)InvoiceAssociationType.CorrectedInvoice).ToListAsync(ct);
            Db.RemoveRange(correctionLinks);
            Db.Set<CustInvoiceLine>().RemoveRange(lines);
            Db.Set<CustInvoiceTable>().Remove(header);
            await unitOfWork.CompleteAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
            return new(200, null, "Free text invoice deleted.");
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    public Task<FreeTextInvoiceResult> PostAsync(long recId, CancellationToken ct) =>
        Db.Database.CreateExecutionStrategy().ExecuteAsync(() => PostCoreAsync(recId, ct));

    private async Task<FreeTextInvoiceResult> PostCoreAsync(long recId, CancellationToken ct)
    {
        var area = company.GetDataAreaId();
        if (string.IsNullOrWhiteSpace(area)) return new(400, null, "A company must be selected.");
        var support = new SubledgerPostingSupport(Db);
        await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await support.LockAsync($"FreeTextInvoice:{area}:{recId}", ct);
            var header = await Db.Set<CustInvoiceTable>()
                .FirstOrDefaultAsync(x => x.DataAreaId == area && x.RecId == recId, ct);
            if (header == null) return await FailAsync(404, "Free text invoice was not found.", ct);
            var lines = await Db.Set<CustInvoiceLine>()
                .Where(x => x.DataAreaId == area && x.ParentRecId == recId)
                .ToListAsync(ct);
            if (header.Posted == NoYes.Yes)
            {
                var previous = Map(header, lines);
                await unitOfWork.CommitTransactionAsync(ct);
                return new(200, previous, "Free text invoice was already posted.");
            }
            if (lines.Count == 0) return await FailAsync(422, "At least one free text invoice line is required.", ct);
            var correction = await Db.Set<CustRelatedInvoice>().SingleOrDefaultAsync(x => x.DataAreaId == area
                && x.CustInvoice == recId && x.InvoiceAssociationType == (int)InvoiceAssociationType.CorrectedInvoice, ct);
            if (correction != null)
                await CancelOriginalAsync(area, header, correction, support, ct);

            await PostInvoiceAsync(area, header, lines, support, ct);
            var result = Map(header, lines);
            await unitOfWork.CommitTransactionAsync(ct);
            return new(200, result, "Free text invoice posted.");
        }
        catch (SubledgerPostingException exception)
        {
            return await FailAsync(422, exception.Message, ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            Db.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task CancelOriginalAsync(string area, CustInvoiceTable corrected, CustRelatedInvoice correction,
        SubledgerPostingSupport support, CancellationToken ct)
    {
        var account = corrected.InvoiceAccount;
        var date = corrected.InvoiceDate.Date;
        await support.LockCustomerSettlementAsync(area, account, ct);
        var original = await Db.Set<CustInvoiceTable>().AsNoTracking().SingleOrDefaultAsync(x =>
            x.DataAreaId == area && x.RecId == correction.ParentCustInvoice && x.Posted == NoYes.Yes, ct);
        if (original == null || original.InvoiceAccount != account || original.CurrencyCode != corrected.CurrencyCode
            || original.OrderAccount != corrected.OrderAccount || original.InvoiceDate.Date > date)
            throw new SubledgerPostingException("The posted original does not match the correction.");

        var source = await Db.Set<CustInvoiceJour>().AsNoTracking().SingleOrDefaultAsync(x => x.DataAreaId == area
            && x.InvoiceId == original.InvoiceId && x.InvoiceAccount == account
            && x.DocumentStatus == DocumentStatus.FreeTextInvoice, ct);
        if (source == null)
            throw new SubledgerPostingException("The original free text invoice journal was not found.");
        var debit = await Db.Set<CustTrans>().SingleOrDefaultAsync(x => x.DataAreaId == area
            && x.AccountNum == account && x.Invoice == source.InvoiceId && x.Voucher == source.LedgerVoucher, ct);
        var debitOpen = debit == null ? null : await Db.Set<CustTransOpen>().SingleOrDefaultAsync(x =>
            x.DataAreaId == area && x.AccountNum == account && x.RefRecId == debit.RecId, ct);
        if (debit == null || debitOpen == null || debit.AmountCur <= 0m || debitOpen.AmountCur != debit.AmountCur
            || debitOpen.AmountMst != debit.AmountMst || debitOpen.ReportingCurrencyAmount != debit.ReportingCurrencyAmount)
            throw new SubledgerPostingException("The original invoice must be fully open. Reverse its settlements before correcting it.");

        var ledger = await support.LoadLedgerContextAsync(area, date, LedgerPostingModule.Customer,
            currentUser.GetCurrentUserId(), ct);
        var voucher = await Db.Set<SubledgerVoucherGeneralJournalEntry>().AsNoTracking().SingleOrDefaultAsync(x =>
            x.DataAreaId == area && x.VoucherDataAreaId == area && x.Voucher == source.LedgerVoucher
            && x.AccountingDate == source.InvoiceDate, ct);
        var ledgerLines = voucher == null ? [] : await Db.Set<GeneralJournalAccountEntry>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.GeneralJournalEntry == voucher.GeneralJournalEntry).ToListAsync(ct);
        if (ledgerLines.Count == 0 || ledgerLines.Sum(x => x.TransactionCurrencyAmount) != 0m
            || ledgerLines.Sum(x => x.AccountingCurrencyAmount) != 0m || ledgerLines.Sum(x => x.ReportingCurrencyAmount) != 0m)
            throw new SubledgerPostingException("The original free text invoice voucher is missing or unbalanced.");
        var sourceLines = await Db.Set<CustInvoiceTrans>().AsNoTracking()
            .Where(x => x.DataAreaId == area && x.ParentRecId == source.RecId).ToListAsync(ct);
        if (sourceLines.Count == 0)
            throw new SubledgerPostingException("The original posted invoice has no lines to cancel.");
        var tax = await Db.Set<TaxTrans>().AsNoTracking().Where(x => x.DataAreaId == area
            && x.Voucher == source.LedgerVoucher && x.TransDate == source.InvoiceDate).ToListAsync(ct);

        var id = (await numbers.NextAsync("CustInvoiceJour", area, ct)).Code;
        var entry = support.NewEntry(area, id, id, date, ledger);
        var adjusting = new CustInvoiceTable
        {
            DataAreaId = area, InvoiceId = id, InvoiceDate = date, DocumentDate = date, DueDate = date,
            InvoiceAccount = account, OrderAccount = original.OrderAccount, Name = original.Name,
            CustGroup = original.CustGroup, CurrencyCode = original.CurrencyCode, Payment = original.Payment,
            PaymMode = original.PaymMode, DefaultDimension = original.DefaultDimension,
            PostingProfile = original.PostingProfile, TaxGroup = original.TaxGroup, Posted = NoYes.Yes
        };
        var journal = new CustInvoiceJour
        {
            DataAreaId = area, InvoiceId = id, LedgerVoucher = id, InvoiceDate = date, DocumentDate = date, DueDate = date,
            InvoiceAccount = account, OrderAccount = source.OrderAccount, InvoicingName = source.InvoicingName,
            CustGroup = source.CustGroup, CurrencyCode = source.CurrencyCode, Payment = source.Payment,
            DefaultDimension = source.DefaultDimension, PostingProfile = source.PostingProfile, TaxGroup = source.TaxGroup,
            DocumentStatus = DocumentStatus.FreeTextInvoice, PostedState = NoYes.Yes, Updated = NoYes.Yes,
            Qty = -source.Qty, SalesBalance = -source.SalesBalance, SalesBalanceMst = -source.SalesBalanceMst,
            SumTax = -source.SumTax, SumTaxMst = -source.SumTaxMst,
            InvoiceAmount = -source.InvoiceAmount, InvoiceAmountMst = -source.InvoiceAmountMst
        };
        var credit = new CustTrans
        {
            DataAreaId = area, AccountNum = account, OrderAccount = debit.OrderAccount, Invoice = id, Voucher = id,
            DocumentNum = id, TransDate = date, DocumentDate = date, DueDate = date, Txt = debit.Txt,
            CurrencyCode = debit.CurrencyCode, TransType = debit.TransType, AmountCur = -debit.AmountCur,
            AmountMst = -debit.AmountMst, ReportingCurrencyAmount = -debit.ReportingCurrencyAmount,
            PostingProfile = debit.PostingProfile, DefaultDimension = debit.DefaultDimension
        };
        Db.AddRange(entry, adjusting, journal, credit);
        await unitOfWork.CompleteAsync(ct);

        Db.Add(new SubledgerVoucherGeneralJournalEntry
        {
            DataAreaId = area, GeneralJournalEntry = entry.RecId, Voucher = id, VoucherDataAreaId = area, AccountingDate = date
        });
        Db.AddRange(ledgerLines.Select(line => new GeneralJournalAccountEntry
        {
            DataAreaId = area, GeneralJournalEntry = entry.RecId, OriginalAccountEntry = line.RecId,
            MainAccount = line.MainAccount, LedgerDimension = line.LedgerDimension, LedgerAccount = line.LedgerAccount,
            PostingType = line.PostingType, Text = line.Text, TransactionCurrencyCode = line.TransactionCurrencyCode,
            TransactionCurrencyAmount = -line.TransactionCurrencyAmount, AccountingCurrencyAmount = -line.AccountingCurrencyAmount,
            ReportingCurrencyAmount = -line.ReportingCurrencyAmount, Quantity = -line.Quantity,
            IsCorrection = NoYes.Yes, IsCredit = line.IsCredit == NoYes.Yes ? NoYes.No : NoYes.Yes
        }));
        Db.AddRange(tax.Select(row => TaxTransaction(area, date, id, row.TaxCode, row.TaxGroup, row.TaxItemGroup,
            row.CurrencyCode, -row.TaxBaseAmountCur, -row.TaxAmountCur, -row.TaxBaseAmount, -row.TaxAmount,
            -row.TaxBaseAmountRep, -row.TaxAmountRep, row.TaxValue)));
        foreach (var line in sourceLines)
        {
            var adjustingLine = new CustInvoiceLine
            {
                DataAreaId = area, ParentRecId = adjusting.RecId, LineNum = line.LineNum, Description = line.LineHeader,
                Quantity = -line.Qty, UnitPrice = line.SalesPrice, AmountCur = -line.LineAmount, TaxAmount = -line.TaxAmount,
                LedgerDimension = line.LedgerDimension, DefaultDimension = line.DefaultDimension,
                TaxGroup = line.TaxGroup, TaxItemGroup = line.TaxItemGroup
            };
            Db.Add(adjustingLine);
        }
        await unitOfWork.CompleteAsync(ct);
        var adjustingLines = await Db.Set<CustInvoiceLine>().Where(x => x.DataAreaId == area
            && x.ParentRecId == adjusting.RecId).ToDictionaryAsync(x => x.LineNum, ct);
        Db.AddRange(sourceLines.Select(line => new CustInvoiceTrans
        {
            DataAreaId = area, ParentRecId = journal.RecId, CustInvoiceLineIdRef = adjustingLines[line.LineNum].RecId,
            InvoiceId = id, InvoiceDate = date, LineNum = line.LineNum, LineHeader = line.LineHeader,
            Name = line.Name, Qty = -line.Qty, SalesPrice = line.SalesPrice, PriceUnit = line.PriceUnit,
            LineAmount = -line.LineAmount, LineAmountMst = -line.LineAmountMst, TaxAmount = -line.TaxAmount,
            TaxAmountMst = -line.TaxAmountMst, LineAmountTax = -line.LineAmountTax, LineAmountTaxMst = -line.LineAmountTaxMst,
            LedgerDimension = line.LedgerDimension, DefaultDimension = line.DefaultDimension,
            TaxGroup = line.TaxGroup, TaxItemGroup = line.TaxItemGroup, CurrencyCode = line.CurrencyCode
        }));
        Db.Add(new CustRelatedInvoice
        {
            DataAreaId = area, CustInvoice = adjusting.RecId, OriginalCustInvoice = correction.OriginalCustInvoice,
            ParentCustInvoice = original.RecId, InvoiceAssociationType = (int)InvoiceAssociationType.AdjustingInvoice
        });
        var creditOpen = new CustTransOpen
        {
            DataAreaId = area, AccountNum = account, RefRecId = credit.RecId, TransDate = date, DueDate = date,
            AmountCur = credit.AmountCur, AmountMst = credit.AmountMst, ReportingCurrencyAmount = credit.ReportingCurrencyAmount
        };
        Db.Add(creditOpen);
        await unitOfWork.CompleteAsync(ct);
        var settlement = (await numbers.NextAsync("CustSettlement", area, ct)).Code;
        CustSettleOpenTransactionsService.ApplyPair(Db, area, account, date, settlement,
            debit, debitOpen, credit, creditOpen, debit.AmountCur);
    }

    private async Task PostInvoiceAsync(string area, CustInvoiceTable header, List<CustInvoiceLine> lines,
        SubledgerPostingSupport support, CancellationToken ct)
    {
        var customer = await Db.Set<CustTable>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DataAreaId == area && x.AccountNum == header.InvoiceAccount, ct);
        if (customer == null) throw new SubledgerPostingException("The invoice customer was not found.");
        if (customer.Blocked is CustVendorBlocked.All or CustVendorBlocked.Invoicing)
            throw new SubledgerPostingException("The invoice customer is blocked for invoicing.");
        var ledger = await support.LoadLedgerContextAsync(area, header.InvoiceDate.Date,
            LedgerPostingModule.Customer, currentUser.GetCurrentUserId(), ct);
        if (string.IsNullOrWhiteSpace(header.CurrencyCode))
            throw new SubledgerPostingException("The free text invoice has no currency.");
        // Every line of a free text invoice is in the invoice currency - there is no inventory
        // cost on this document - so the whole voucher translates at the invoice date's rate.
        var translation = await SubledgerCurrencyTranslation.LoadAsync(Db, area, ledger.Ledger,
            header.CurrencyCode, header.InvoiceDate.Date, ct);

        var customerAccount = await support.ResolveCustomerSummaryAsync(area, header.PostingProfile, customer, ct);
        var invoiceId = (await numbers.NextAsync("CustInvoiceJour", area, ct)).Code;
        var entry = support.NewEntry(area, invoiceId, invoiceId, header.InvoiceDate.Date, ledger);
        Db.Add(entry);
        var journal = new CustInvoiceJour
        {
            DataAreaId = area,
            InvoiceId = invoiceId,
            InvoiceDate = header.InvoiceDate.Date,
            DocumentDate = header.DocumentDate.Date,
            OrderAccount = header.OrderAccount,
            InvoiceAccount = header.InvoiceAccount,
            CustGroup = header.CustGroup,
            InvoicingName = header.Name,
            CurrencyCode = header.CurrencyCode,
            LedgerVoucher = invoiceId,
            Payment = header.Payment,
            DueDate = header.DueDate,
            DefaultDimension = header.DefaultDimension,
            PostingProfile = header.PostingProfile,
            TaxGroup = header.TaxGroup,
            DocumentStatus = DocumentStatus.FreeTextInvoice,
            PostedState = NoYes.Yes,
            Updated = NoYes.Yes
        };
        Db.Set<CustInvoiceJour>().Add(journal);
        await unitOfWork.CompleteAsync(ct);

        decimal netTotal = 0m;
        decimal taxTotal = 0m;
        decimal signedVoucherTotal = 0m;
        var taxByGroup = new Dictionary<(string Code, string Group, string ItemGroup, decimal Rate), (decimal Base, decimal Tax)>();
        foreach (var line in lines)
        {
            var revenueAccount = await support.LoadDimensionAsync(area, line.LedgerDimension, "free text invoice line", ct);
            var amount = decimal.Round(line.Quantity * line.UnitPrice, 2, MidpointRounding.AwayFromZero);
            var codes = await support.ApplicableTaxCodesAsync(area, line.TaxGroup, line.TaxItemGroup, header.InvoiceDate.Date, ct);
            var lineTax = decimal.Round(amount * codes.Sum(x => x.Rate) / 100m, 2, MidpointRounding.AwayFromZero);
            line.AmountCur = amount;
            line.TaxAmount = lineTax;
            Db.Add(translation.AccountEntry(area, entry.RecId, revenueAccount,
                LedgerPostingType.FreeTextInvoice, -amount, line.Description, 0m));
            signedVoucherTotal -= amount;
            foreach (var (code, codeTax) in SubledgerPostingSupport.AllocateTax(codes, lineTax))
            {
                var key = (code, line.TaxGroup, line.TaxItemGroup, codes.Single(x => x.Code == code).Rate);
                var current = taxByGroup.GetValueOrDefault(key);
                taxByGroup[key] = (current.Base + amount, current.Tax + codeTax);
            }
            Db.Set<CustInvoiceTrans>().Add(new CustInvoiceTrans
            {
                DataAreaId = area,
                ParentRecId = journal.RecId,
                CustInvoiceLineIdRef = line.RecId,
                InvoiceId = invoiceId,
                InvoiceDate = header.InvoiceDate.Date,
                LineNum = line.LineNum,
                LineHeader = line.Description,
                Name = line.Description,
                Qty = line.Quantity,
                SalesPrice = line.UnitPrice,
                PriceUnit = 1m,
                LineAmount = amount,
                LineAmountMst = translation.ToAccounting(amount),
                TaxAmount = lineTax,
                TaxAmountMst = translation.ToAccounting(lineTax),
                LineAmountTax = amount + lineTax,
                LineAmountTaxMst = translation.ToAccounting(amount + lineTax),
                LedgerDimension = line.LedgerDimension,
                DefaultDimension = line.DefaultDimension,
                TaxGroup = line.TaxGroup,
                TaxItemGroup = line.TaxItemGroup,
                CurrencyCode = header.CurrencyCode
            });
            netTotal += amount;
            taxTotal += lineTax;
        }

        foreach (var (key, value) in taxByGroup)
        {
            // A zero-rated code posts no ledger line and resolves no payable account; its base
            // is still recorded so the turnover can be reported under the code.
            if (value.Tax != 0m)
            {
                var payable = await support.ResolveTaxPayableAsync(area, key.Code, ct);
                Db.Add(translation.AccountEntry(area, entry.RecId, payable, LedgerPostingType.Tax,
                    -value.Tax, $"Sales tax {key.Code} on {invoiceId}", 0m));
            }
            Db.Set<TaxTrans>().Add(TaxTransaction(area, header.InvoiceDate.Date, invoiceId, key.Code,
                key.Group, key.ItemGroup, header.CurrencyCode, -value.Base, -value.Tax,
                translation.ToAccounting(-value.Base), translation.ToAccounting(-value.Tax),
                translation.ToReporting(-value.Base), translation.ToReporting(-value.Tax), key.Rate));
            signedVoucherTotal -= value.Tax;
        }

        var total = netTotal + taxTotal;
        // Derived from the lines already translated, so the accounting currency balances even
        // when each line rounded independently.
        var customerEntry = translation.BalancingEntry(area, entry.RecId, customerAccount,
            LedgerPostingType.CustBalance, total, $"Free text invoice {invoiceId}", 0m);
        Db.Add(customerEntry);
        var totalAccounting = customerEntry.AccountingCurrencyAmount;
        var totalReporting = customerEntry.ReportingCurrencyAmount;
        signedVoucherTotal += total;
        if (signedVoucherTotal != 0m)
            throw new SubledgerPostingException("The free text invoice voucher is not balanced.");
        Db.Add(new SubledgerVoucherGeneralJournalEntry
        {
            DataAreaId = area,
            GeneralJournalEntry = entry.RecId,
            Voucher = invoiceId,
            VoucherDataAreaId = area,
            AccountingDate = header.InvoiceDate.Date
        });

        var custTrans = new CustTrans
        {
            DataAreaId = area,
            AccountNum = header.InvoiceAccount,
            OrderAccount = header.OrderAccount,
            TransDate = header.InvoiceDate.Date,
            Voucher = invoiceId,
            Invoice = invoiceId,
            DocumentNum = invoiceId,
            DocumentDate = header.DocumentDate.Date,
            Txt = header.Name,
            TransType = LedgerTransType.Sales,
            CurrencyCode = header.CurrencyCode,
            AmountCur = total,
            AmountMst = totalAccounting,
            ReportingCurrencyAmount = totalReporting,
            DueDate = header.DueDate,
            PaymTermId = header.Payment,
            PaymMode = header.PaymMode,
            PostingProfile = header.PostingProfile,
            DefaultDimension = header.DefaultDimension
        };
        Db.Set<CustTrans>().Add(custTrans);
        await unitOfWork.CompleteAsync(ct);
        Db.Set<CustTransOpen>().Add(new CustTransOpen
        {
            DataAreaId = area,
            AccountNum = header.InvoiceAccount,
            RefRecId = custTrans.RecId,
            TransDate = header.InvoiceDate.Date,
            DueDate = header.DueDate,
            AmountCur = total,
            AmountMst = totalAccounting,
            ReportingCurrencyAmount = totalReporting
        });

        journal.SalesBalance = netTotal;
        journal.SalesBalanceMst = translation.ToAccounting(netTotal);
        journal.SumTax = taxTotal;
        journal.SumTaxMst = translation.ToAccounting(taxTotal);
        journal.InvoiceAmount = total;
        journal.InvoiceAmountMst = totalAccounting;
        journal.Qty = lines.Sum(x => x.Quantity);
        header.InvoiceId = invoiceId;
        header.Posted = NoYes.Yes;
        await unitOfWork.CompleteAsync(ct);
    }

    private async Task<string> ResolveDescriptionAsync(string? description, CustTable customer, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(description)) return description.Trim();
        if (customer.Party == 0) return customer.AccountNum;
        return await Db.Set<DirPartyTable>().AsNoTracking()
            .Where(x => x.RecId == customer.Party).Select(x => x.Name).FirstOrDefaultAsync(ct)
            ?? customer.AccountNum;
    }

    private static TaxTrans TaxTransaction(string area, DateTime date, string voucher, string code,
        string group, string itemGroup, string currency, decimal baseAmount, decimal taxAmount,
        decimal? accountingBase = null, decimal? accountingTax = null,
        decimal? reportingBase = null, decimal? reportingTax = null, decimal rate = 0m) => new()
    {
        DataAreaId = area,
        TransDate = date,
        Voucher = voucher,
        TaxCode = code,
        TaxGroup = group,
        TaxItemGroup = itemGroup,
        // The rate applied, so a zero-rated row is distinguishable from an unrecorded rate.
        TaxValue = rate,
        TaxDirection = TaxDirection.Outgoing,
        SourceCurrencyCode = currency,
        CurrencyCode = currency,
        SourceBaseAmountCur = baseAmount,
        SourceTaxAmountCur = taxAmount,
        TaxBaseAmountCur = baseAmount,
        TaxAmountCur = taxAmount,
        // The unsuffixed and Rep amounts are the accounting and reporting translations, which is
        // what tax settlement sums; they default to the document amounts for a same-currency invoice.
        TaxBaseAmount = accountingBase ?? baseAmount,
        TaxAmount = accountingTax ?? taxAmount,
        TaxBaseAmountRep = reportingBase ?? baseAmount,
        TaxAmountRep = reportingTax ?? taxAmount
    };

    private async Task<FreeTextInvoiceResult> FailAsync(int statusCode, string message, CancellationToken ct)
    {
        await unitOfWork.RollbackTransactionAsync(ct);
        Db.ChangeTracker.Clear();
        return new(statusCode, null, message);
    }

    private static string Normalize(string? value, string? fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback?.Trim() ?? string.Empty : value.Trim();

    private static FreeTextInvoiceDto Map(CustInvoiceTable header, IEnumerable<CustInvoiceLine> sourceLines)
    {
        var lines = sourceLines.OrderBy(x => x.LineNum).Select(x => new FreeTextInvoiceLineDto(
            x.RecId, x.LineNum, x.Description, x.Quantity, x.UnitPrice, x.AmountCur, x.TaxAmount,
            x.AmountCur + x.TaxAmount, x.LedgerDimension, x.DefaultDimension, x.TaxGroup, x.TaxItemGroup)).ToList();
        var net = lines.Sum(x => x.NetAmount);
        var tax = lines.Sum(x => x.TaxAmount);
        return new FreeTextInvoiceDto(header.RecId, header.InvoiceId, header.OrderAccount, header.InvoiceAccount,
            header.Name, header.InvoiceDate, header.DueDate, header.CurrencyCode, header.Payment, header.PaymMode,
            header.PostingProfile, header.DefaultDimension, header.TaxGroup, header.Posted == NoYes.Yes,
            net, tax, net + tax, lines);
    }

    private sealed class FreeTextInvoiceInputException(string message, int statusCode = 422) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
    }
}
