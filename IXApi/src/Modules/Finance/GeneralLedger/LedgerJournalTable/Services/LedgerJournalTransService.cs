using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Identity;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable
{
    public class LedgerJournalTransService : BaseService<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans>, ILedgerJournalTransService
    {
        protected override async Task OnBeforeAddAsync(IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans entity, CancellationToken ct)
        {
            var area = string.IsNullOrWhiteSpace(entity.DataAreaId) ? _currentUser.GetDataAreaId() : entity.DataAreaId;
            var header = await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.DataAreaId == area && x.JournalNum == entity.JournalNum, ct)
                ?? throw new InvalidOperationException("Journal header was not found.");
            if (header.Posted == NoYes.Yes) throw new InvalidOperationException("Lines cannot be added to a posted journal.");
            if (header.SystemBlocked == NoYes.Yes) throw new InvalidOperationException("The journal is system blocked.");
            entity.DataAreaId = area!;
            entity.Company = string.IsNullOrWhiteSpace(entity.Company) ? area! : entity.Company;
            entity.OffsetCompany = string.IsNullOrWhiteSpace(entity.OffsetCompany) ? area! : entity.OffsetCompany;
            entity.CurrencyCode = string.IsNullOrWhiteSpace(entity.CurrencyCode) ? header.CurrencyCode : entity.CurrencyCode;
            if (header.FixedOffsetAccount == NoYes.Yes || entity.OffsetLedgerDimension == 0)
            {
                entity.OffsetAccountType = header.OffsetAccountType;
                entity.OffsetLedgerDimension = header.OffsetLedgerDimension;
            }
            if (entity.DocumentDate == default) entity.DocumentDate = entity.TransDate;
            entity.NoEdit = NoYes.No;
            // Transfer, approval and payment status are set by the system, never by the client.
            entity.Transferred = NoYes.No;
            entity.TransferredBy = string.Empty;
            entity.TransferredTo = string.Empty;
            entity.TransferredOn = default;
            entity.Approved = NoYes.No;
            entity.Approver = 0;
            entity.PaymentStatus = default;
            if (entity.LineNum == 0)
                entity.LineNum = (await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans>()
                    .Where(x => x.DataAreaId == area && x.JournalNum == entity.JournalNum)
                    .Select(x => x.LineNum).ToListAsync(ct)).DefaultIfEmpty(0m).Max() + 1;
        }

        public override async Task<IEnumerable<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans>> AddRangeAsync(
            IEnumerable<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans> entities, CancellationToken ct = default)
        {
            var rows = entities.ToList();
            // BaseService runs every add hook before tracking any row. Reserve distinct line
            // numbers for the whole batch, including explicit numbers supplied by the caller.
            foreach (var group in rows.GroupBy(x => new
                { Area = string.IsNullOrWhiteSpace(x.DataAreaId) ? _currentUser.GetDataAreaId() : x.DataAreaId, x.JournalNum }))
            {
                var existing = await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans>()
                    .Where(x => x.DataAreaId == group.Key.Area && x.JournalNum == group.Key.JournalNum)
                    .Select(x => x.LineNum).ToListAsync(ct);
                var next = existing.Concat(group.Select(x => x.LineNum)).DefaultIfEmpty(0m).Max();
                foreach (var row in group.Where(x => x.LineNum == 0)) row.LineNum = ++next;
            }
            return await base.AddRangeAsync(rows, ct);
        }

        public override async Task<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans> UpdateAsync(IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans entity, CancellationToken ct = default)
        {
            await PrepareUpdateAsync(entity, ct);
            return await base.UpdateAsync(entity, ct);
        }

        private async Task PrepareUpdateAsync(IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans entity, CancellationToken ct)
        {
            var current = await EnsureEditableAsync(entity.RecId, ct);
            entity.DataAreaId = current.DataAreaId;
            entity.JournalNum = current.JournalNum;
            entity.Company = current.Company;
            entity.NoEdit = current.NoEdit;
            // Shown on the line tabs but not edited through this API: customer/vendor payment and
            // general journals share this DTO, and a client that does not send these fields must
            // not reset them.
            entity.Qty = current.Qty;
            entity.ReleaseDate = current.ReleaseDate;
            entity.Transfer = current.Transfer;
            entity.Transferred = current.Transferred;
            entity.TransferredBy = current.TransferredBy;
            entity.TransferredOn = current.TransferredOn;
            entity.TransferredTo = current.TransferredTo;
            entity.TaxWithholdGroup = current.TaxWithholdGroup;
            entity.TaxDirectionControl = current.TaxDirectionControl;
            entity.CashDiscAmount = current.CashDiscAmount;
            entity.DateCashDisc = current.DateCashDisc;
            entity.VatNumJournal = current.VatNumJournal;
            entity.VendorVatDate = current.VendorVatDate;
            entity.Approved = current.Approved;
            entity.Approver = current.Approver;
            entity.PaymSpec = current.PaymSpec;
            entity.BankTransType = current.BankTransType;
            entity.BankDepositVoucher = current.BankDepositVoucher;
            entity.BankDepositNum = current.BankDepositNum;
            entity.ForeignCompany = current.ForeignCompany;
            entity.ForeignVoucher = current.ForeignVoucher;
            entity.ReasonRefRecId = current.ReasonRefRecId;
            entity.PaymentStatus = current.PaymentStatus;
        }

        public override async Task<IEnumerable<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans>> UpdateRangeAsync(
            IEnumerable<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans> entities, CancellationToken ct = default)
        {
            var rows = entities.ToList();
            foreach (var row in rows) await PrepareUpdateAsync(row, ct);
            return await base.UpdateRangeAsync(rows, ct);
        }

        public override async Task RemoveRangeAsync(
            IEnumerable<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans> entities, CancellationToken ct = default)
        {
            var rows = entities.ToList();
            foreach (var row in rows) await EnsureEditableAsync(row.RecId, ct);
            await base.RemoveRangeAsync(rows, ct);
        }

        public override async Task RemoveAsync(IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans entity, CancellationToken ct = default)
        {
            await EnsureEditableAsync(entity.RecId, ct);
            await base.RemoveAsync(entity, ct);
        }

        private async Task<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans> EnsureEditableAsync(long recId, CancellationToken ct)
        {
            var row = await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTrans>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.RecId == recId, ct)
                ?? throw new InvalidOperationException("Journal line was not found.");
            var header = await _unitOfWork.Context.Set<IAX.IXApi.Modules.Finance.Entities.LedgerJournalTable>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.DataAreaId == row.DataAreaId && x.JournalNum == row.JournalNum, ct)
                ?? throw new InvalidOperationException("Journal header was not found.");
            if (row.NoEdit == NoYes.Yes || header.Posted == NoYes.Yes) throw new InvalidOperationException("Posted journal lines cannot be changed or deleted.");
            if (header.SystemBlocked == NoYes.Yes) throw new InvalidOperationException("The journal is system blocked.");
            return row;
        }

        public LedgerJournalTransService(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : base(unitOfWork, currentUser)
        {
        }

        public LedgerJournalTransService(IUnitOfWork unitOfWork) : this(unitOfWork, NullCurrentUserService.Instance)
        {
        }
    }
}
