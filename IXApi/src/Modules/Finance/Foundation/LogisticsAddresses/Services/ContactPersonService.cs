using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Modules.Finance.AccountsReceivable;
using IAX.IXApi.Modules.Finance.Common;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;

public class ContactPersonService : BaseService<ContactPerson>, IContactPersonService
{
    private readonly ISysNumberSequenceService _sequences;

    public ContactPersonService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser, ISysNumberSequenceService sequences)
        : base(unitOfWork, currentUser)
    {
        _sequences = sequences;
    }

    private async Task<long?> OwnerAsync(string kind, string account, CancellationToken ct)
    {
        var db = _unitOfWork.Context;
        var area = _currentUser.GetDataAreaId();
        return kind switch
        {
            "customer" => await db.Set<CustTable>().Where(x => x.DataAreaId == area && x.AccountNum == account).Select(x => (long?)x.Party).SingleOrDefaultAsync(ct),
            "vendor" => await db.Set<VendTable>().Where(x => x.DataAreaId == area && x.AccountNum == account).Select(x => (long?)x.Party).SingleOrDefaultAsync(ct),
            _ => null
        };
    }

    private IQueryable<ContactPerson> Contacts(long owner)
    {
        var area = _currentUser.GetDataAreaId();
        return _unitOfWork.Context.Set<ContactPerson>().Where(x => x.DataAreaId == area && x.ContactForParty == owner);
    }

    public async Task<AccountContactsDto?> ListAccountAsync(string kind, string account, CancellationToken ct)
    {
        var owner = await OwnerAsync(kind, account, ct);
        if (owner == null) return null;
        var now = DateTime.UtcNow;
        var ownerName = await _unitOfWork.Context.Set<DirPartyTable>().Where(p => p.RecId == owner)
            .Select(p => p.Name).SingleOrDefaultAsync(ct);
        var rows = await Contacts(owner.Value).AsNoTracking().OrderBy(x => x.DirPartyTable!.Name).ThenBy(x => x.RecId)
            .Select(x => new AccountContactRow(x.RecId, x.ContactPersonId, x.Party, x.DirPartyTable!.Name,
                x.DirPartyTable.NameAlias, x.DirPartyTable.LanguageId, x.DirPartyTable.AddressBookNames, x.DirPartyTable.Initials,
                x.DirPartyTable.BirthYear, x.DirPartyTable.BirthMonth, x.DirPartyTable.Birthday,
                x.DirPartyTable.AnniversaryYear, x.DirPartyTable.AnniversaryMonth, x.DirPartyTable.AnniversaryDay,
                x.DirPartyTable.LogisticsElectronicAddress_Phone!.Locator,
                x.DirPartyTable.LogisticsElectronicAddress_Phone.LocatorExtension,
                x.Inactive == NoYes.Yes, x.VendorPortalAccessAllowed == NoYes.Yes,
                x.DirPartyTable.PersonNames.Where(n => n.ValidFrom <= now && n.ValidTo > now)
                    .OrderByDescending(n => n.ValidFrom)
                    .Select(n => new ContactNameParts(n.FirstName, n.MiddleName, n.LastNamePrefix, n.LastName)).FirstOrDefault(),
                x.DirPartyTable.Gender, x.DirPartyTable.MaritalStatus)).ToListAsync(ct);
        return new AccountContactsDto(account, ownerName, rows);
    }

    public async Task<long?> SaveAccountAsync(string kind, string account, long? id, AccountContactInput input, CancellationToken ct)
    {
        var owner = await OwnerAsync(kind, account, ct);
        if (owner == null) return null;
        ContactPerson contact;
        if (id.HasValue)
        {
            var existing = await Contacts(owner.Value).Include(x => x.DirPartyTable!).ThenInclude(p => p.PersonNames).SingleOrDefaultAsync(x => x.RecId == id, ct);
            if (existing?.DirPartyTable == null) return null;
            contact = existing;
        }
        else
        {
            var partyNumber = await _sequences.NextAsync("DirPartyTable", cancellationToken: ct);
            var contactNumber = await _sequences.NextAsync("ContactPerson", cancellationToken: ct);
            var area = _currentUser.GetDataAreaId();
            contact = new ContactPerson
            {
                ContactPersonId = contactNumber.Code ?? throw new InvalidOperationException("Contact number was not allocated."),
                ContactForParty = owner.Value, CustAccount = kind == "customer" ? account : null, DataAreaId = area,
                DirPartyTable = new DirPartyTable { PartyNumber = partyNumber.Code ?? throw new InvalidOperationException("Party number was not allocated."), DataAreaId = area, IsActive = NoYes.Yes }
            };
            _unitOfWork.Context.Set<ContactPerson>().Add(contact);
        }
        var party = contact.DirPartyTable!;
        var now = DateTime.UtcNow;
        if (input.NameParts is { } parts)
        {
            var current = party.PersonNames.FirstOrDefault(n => n.ValidFrom <= now && n.ValidTo > now);
            if (current == null || parts != new ContactNameParts(current.FirstName, current.MiddleName, current.LastNamePrefix, current.LastName))
            {
                if (current != null) current.ValidTo = now;
                party.PersonNames.Add(new DirPersonName { ValidFrom = now, ValidTo = DateTime.MaxValue,
                    DataAreaId = party.DataAreaId, FirstName = parts.FirstName.Trim(), MiddleName = parts.MiddleName.Trim(),
                    LastNamePrefix = parts.LastNamePrefix.Trim(), LastName = parts.LastName.Trim() });
            }
        }
        party.Gender = input.Gender;
        party.MaritalStatus = input.MaritalStatus;
        party.Name = input.Name.Trim();
        party.NameAlias = input.NameAlias?.Trim() ?? "";
        party.LanguageId = input.LanguageId.Trim();
        party.AddressBookNames = input.AddressBookNames?.Trim() ?? "";
        party.Initials = input.Initials;
        // A party can have a month and day without a year. Preserve those parts when the
        // full-date control is empty and the user saves an unrelated change.
        if (input.Birthday.HasValue || (party.BirthYear.HasValue && party.BirthMonth.HasValue && party.Birthday.HasValue))
        {
            party.BirthYear = input.Birthday?.Year;
            party.BirthMonth = input.Birthday?.Month;
            party.Birthday = input.Birthday?.Day;
        }
        if (input.Anniversary.HasValue || (party.AnniversaryYear.HasValue && party.AnniversaryMonth.HasValue && party.AnniversaryDay.HasValue))
        {
            party.AnniversaryYear = input.Anniversary?.Year;
            party.AnniversaryMonth = input.Anniversary?.Month;
            party.AnniversaryDay = input.Anniversary?.Day;
        }
        contact.Inactive = input.Inactive ? NoYes.Yes : NoYes.No;
        await _unitOfWork.CompleteAsync(ct);
        return contact.RecId;
    }
}
