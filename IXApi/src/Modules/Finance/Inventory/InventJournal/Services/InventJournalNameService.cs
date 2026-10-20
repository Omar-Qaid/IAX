using FluentValidation;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Inventory;

public sealed class InventJournalNameService : IInventJournalNameService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanyExecutionContext _company;
    private readonly IValidator<InventJournalNameInputDto> _validator;

    public InventJournalNameService(IUnitOfWork unitOfWork, ICompanyExecutionContext company,
        IValidator<InventJournalNameInputDto> validator)
    {
        _unitOfWork = unitOfWork;
        _company = company;
        _validator = validator;
    }

    public async Task<List<InventJournalNameDto>> ListAsync(CancellationToken ct)
    {
        var companyId = RequireCompany();
        var rows = await _unitOfWork.Context.Set<InventJournalName>().AsNoTracking()
            .Where(x => x.DataAreaId == companyId).OrderBy(x => x.JournalType).ThenBy(x => x.JournalNameId).ToListAsync(ct);
        var ids = rows.Where(x => x.VoucherNumberSequenceTable != 0).Select(x => x.VoucherNumberSequenceTable).Distinct().ToList();
        var codes = await _unitOfWork.Context.Set<SysNumberSequence>().IgnoreQueryFilters().AsNoTracking()
            .Where(x => ids.Contains(x.RecId)).ToDictionaryAsync(x => x.RecId, x => x.NumberSequence, ct);
        return rows.Select(x => Map(x, codes.GetValueOrDefault(x.VoucherNumberSequenceTable, string.Empty))).ToList();
    }

    public async Task<InventJournalNameLookupsDto> LookupsAsync(CancellationToken ct)
    {
        var companyId = RequireCompany();
        var rows = await _unitOfWork.Context.Set<SysNumberSequence>().IgnoreQueryFilters().AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive && x.InUse != 0 && (x.DataAreaId == companyId || x.NumberSequenceScope == null))
            .OrderByDescending(x => x.DataAreaId == companyId).ThenBy(x => x.NumberSequence).ToListAsync(ct);
        return new InventJournalNameLookupsDto
        {
            NumberSequences = rows.Select(x => new InventJournalNumberSequenceLookupDto(x.RecId, x.NumberSequence, x.Txt)).ToList()
        };
    }

    public async Task<(InventJournalNameDto? JournalName, string? Error)> CreateAsync(InventJournalNameInputDto input, CancellationToken ct)
    {
        var error = await ValidateAsync(input, ct);
        if (error != null) return (null, error);
        var companyId = RequireCompany();
        var code = Normalize(input.JournalNameId);
        if (await _unitOfWork.Context.Set<InventJournalName>().AnyAsync(x => x.DataAreaId == companyId && x.JournalNameId == code, ct))
            return (null, $"Journal name '{code}' already exists in the selected company.");
        error = await ValidateNumberSequenceAsync(input.VoucherNumberSequenceTable, companyId, ct);
        if (error != null) return (null, error);
        var entity = new InventJournalName { DataAreaId = companyId, JournalNameId = code };
        Apply(entity, input);
        await _unitOfWork.Repository<InventJournalName>().AddAsync(entity, ct);
        await _unitOfWork.CompleteAsync(ct);
        return (Map(entity, await SequenceCodeAsync(entity.VoucherNumberSequenceTable, ct)), null);
    }

    public async Task<(InventJournalNameDto? JournalName, string? Error)> UpdateAsync(long recId, InventJournalNameInputDto input, CancellationToken ct)
    {
        var error = await ValidateAsync(input, ct);
        if (error != null) return (null, error);
        var companyId = RequireCompany();
        var entity = await _unitOfWork.Context.Set<InventJournalName>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == recId, ct);
        if (entity == null) return (null, "Journal name was not found.");
        if (!string.Equals(entity.JournalNameId, Normalize(input.JournalNameId), StringComparison.OrdinalIgnoreCase))
            return (null, "Journal name ID cannot be changed after creation.");
        if (entity.JournalType != input.JournalType && await IsUsedAsync(companyId, entity.JournalNameId, ct))
            return (null, "Journal type cannot be changed after the journal name has been used.");
        error = await ValidateNumberSequenceAsync(input.VoucherNumberSequenceTable, companyId, ct);
        if (error != null) return (null, error);
        Apply(entity, input);
        await _unitOfWork.CompleteAsync(ct);
        return (Map(entity, await SequenceCodeAsync(entity.VoucherNumberSequenceTable, ct)), null);
    }

    public async Task<InventJournalNameDeleteResult> DeleteAsync(long recId, CancellationToken ct)
    {
        var companyId = RequireCompany();
        var entity = await _unitOfWork.Context.Set<InventJournalName>().FirstOrDefaultAsync(x => x.DataAreaId == companyId && x.RecId == recId, ct);
        if (entity == null) return InventJournalNameDeleteResult.NotFound;
        if (await IsUsedAsync(companyId, entity.JournalNameId, ct)) return InventJournalNameDeleteResult.InUse;
        _unitOfWork.Context.Remove(entity);
        await _unitOfWork.CompleteAsync(ct);
        return InventJournalNameDeleteResult.Deleted;
    }

    private async Task<string?> ValidateAsync(InventJournalNameInputDto input, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(input, ct);
        return result.IsValid ? null : result.Errors[0].ErrorMessage;
    }

    private async Task<string?> ValidateNumberSequenceAsync(long recId, string companyId, CancellationToken ct)
    {
        if (recId == 0) return null;
        var valid = await _unitOfWork.Context.Set<SysNumberSequence>().IgnoreQueryFilters().AsNoTracking()
            .AnyAsync(x => x.RecId == recId && !x.IsDeleted && x.IsActive && x.InUse != 0 &&
                (x.DataAreaId == companyId || x.NumberSequenceScope == null), ct);
        return valid ? null : "The selected voucher number sequence is not available in the selected company.";
    }

    private Task<bool> IsUsedAsync(string companyId, string journalNameId, CancellationToken ct) =>
        _unitOfWork.Context.Set<InventJournalTable>().AnyAsync(x => x.DataAreaId == companyId && x.JournalNameId == journalNameId, ct);

    private async Task<string> SequenceCodeAsync(long recId, CancellationToken ct) => recId == 0 ? string.Empty :
        await _unitOfWork.Context.Set<SysNumberSequence>().IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.RecId == recId).Select(x => x.NumberSequence).FirstOrDefaultAsync(ct) ?? string.Empty;

    private static void Apply(InventJournalName entity, InventJournalNameInputDto input)
    {
        entity.Description = input.Description.Trim();
        entity.JournalType = input.JournalType;
        entity.VoucherNumberSequenceTable = input.VoucherNumberSequenceTable;
        entity.DetailSummary = input.DetailSummary;
        entity.DeletePostedLines = input.DeletePostedLines ? NoYes.Yes : NoYes.No;
        entity.Reservation = input.Reservation;
    }

    private static InventJournalNameDto Map(InventJournalName x, string sequenceCode) => new()
    {
        Id = x.RecId.ToString(), RecId = x.RecId, JournalNameId = x.JournalNameId, Description = x.Description,
        JournalType = x.JournalType, VoucherNumberSequenceTable = x.VoucherNumberSequenceTable,
        VoucherNumberSequenceCode = sequenceCode, DetailSummary = x.DetailSummary,
        DeletePostedLines = x.DeletePostedLines == NoYes.Yes, Reservation = x.Reservation
    };

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private string RequireCompany() => _company.GetDataAreaId() ?? throw new InvalidOperationException("A company must be selected.");
}
