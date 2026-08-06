using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Administration.NumberSequences;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    public class PartyService : BaseService<DirPartyTable>, IPartyService
    {
        private readonly ISysNumberSequenceService _numberSequenceService;

        public PartyService(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUser, 
            ISysNumberSequenceService numberSequenceService) 
            : base(unitOfWork, currentUser)
        {
            _numberSequenceService = numberSequenceService;
        }

        public async Task<DirPartyTable> CreatePartyAsync(string name, string languageId, CancellationToken cancellationToken = default)
        {
            var nsResult = await _numberSequenceService.NextAsync("DirPartyTable", cancellationToken: cancellationToken);
            string partyNumber = nsResult.Code ?? Guid.NewGuid().ToString("N").Substring(0, 20);

            var party = new DirPartyTable
            {
                Name = string.IsNullOrEmpty(name) ? "New Party" : name,
                NameAlias = string.IsNullOrEmpty(name) ? "New Party" : name,
                PartyNumber = partyNumber,
                LanguageId = string.IsNullOrEmpty(languageId) ? "en-us" : languageId,
                AddressBookNames = string.Empty,
                CreatedBy = _currentUser.GetCurrentUserId() ?? "sys",
                OwnerAccountId = _currentUser.GetOwnerAccountId() ?? "sys",
                IsActive = IAX.IXApi.Modules.Finance.Common.NoYes.Yes
            };

            _unitOfWork.Context.Set<DirPartyTable>().Add(party);
            await _unitOfWork.Context.SaveChangesAsync(cancellationToken);

            return party;
        }

        public async Task<DirPartyTable> UpdatePartyNameAsync(long partyRecId, string name, CancellationToken cancellationToken = default)
        {
            var party = await _unitOfWork.Context.Set<DirPartyTable>().FirstOrDefaultAsync(p => p.RecId == partyRecId, cancellationToken);
            if (party == null)
            {
                throw new ArgumentException($"DirPartyTable not found for RecId {partyRecId}");
            }
            party.Name = name;
            party.NameAlias = name;
            _unitOfWork.Context.Set<DirPartyTable>().Update(party);
            await _unitOfWork.Context.SaveChangesAsync(cancellationToken);
            
            return party;
        }
    }
}


