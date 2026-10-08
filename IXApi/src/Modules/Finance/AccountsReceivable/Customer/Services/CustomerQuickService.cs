using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Customer;

public enum CustomerQuickUpdateStatus { CustomerNotFound, PartyNotFound, Updated }
public sealed record CustomerQuickUpdateResult(CustomerQuickUpdateStatus Status, CustTable? Customer = null, DirPartyTable? Party = null);

public sealed class CustomerQuickService : ICustomerQuickService
{
    private readonly ICustomerService _service;
    private readonly IPartyService _partyService;
    private readonly ISysNumberSequenceService _numberSequences;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ILocationService _locations;
    private readonly IPostalAddressService _postalAddresses;
    private readonly IPartyLocationService _partyLocations;

    public CustomerQuickService(ICustomerService service, IPartyService partyService,
        ISysNumberSequenceService numberSequences, IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        ILocationService locations, IPostalAddressService postalAddresses, IPartyLocationService partyLocations)
    {
        _service = service;
        _partyService = partyService;
        _numberSequences = numberSequences;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _locations = locations;
        _postalAddresses = postalAddresses;
        _partyLocations = partyLocations;
    }

        public async Task<(CustTable Customer, DirPartyTable Party)> QuickCreateAsync(
            CustomerQuickCreateDto input,
            CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
            var result = await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    var party = await _partyService.CreatePartyAsync(input.Name.Trim(), "en-us", cancellationToken);
                    party.NameAlias = string.IsNullOrWhiteSpace(input.NameAlias) ? input.Name.Trim() : input.NameAlias.Trim();
                    party.DataAreaId = _currentUser.GetDataAreaId() ?? "dat";
                    await _unitOfWork.CompleteAsync(cancellationToken);

                    var sequence = await _numberSequences.NextAsync("Customer", cancellationToken: cancellationToken);
                    var customerEntity = input.Adapt<CustTable>();
                    customerEntity.AccountNum = sequence.Code;
                    customerEntity.Party = party.RecId;
                    customerEntity.DataAreaId = _currentUser.GetDataAreaId() ?? "dat";
                    var customer = await _service.AddAsync(customerEntity, cancellationToken);

                    if (!string.IsNullOrWhiteSpace(input.Street))
                    {
                        var address = new AddressInfoDto
                        {
                            Description = "Primary address",
                            Primary = true,
                            Street = input.Street.Trim(),
                            CountryRegionId = input.CountryRegionId?.Trim() ?? string.Empty
                        };
                        var location = await _locations.CreateLocationAsync(address.Description, true, cancellationToken);
                        await _postalAddresses.CreatePostalAddressAsync(location.RecId, address, cancellationToken);
                        await _partyLocations.LinkLocationToPartyAsync(party.RecId, location.RecId, true, true, cancellationToken);
                    }

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    return (customer, party);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            });

            return result;
        }

        public async Task<CustomerQuickUpdateResult> QuickUpdateAsync(
            long id,
            CustomerQuickCreateDto input,
            CancellationToken cancellationToken = default)
        {
            var customer = await _service.GetByIdAsync(candidate => candidate.RecId == id, includes: Array.Empty<string>(), cancellationToken: cancellationToken);
            if (customer == null)
                return new(CustomerQuickUpdateStatus.CustomerNotFound);

            var party = await _unitOfWork.Context.Set<DirPartyTable>()
                .FirstOrDefaultAsync(candidate => candidate.RecId == customer.Party, cancellationToken);
            if (party == null)
                return new(CustomerQuickUpdateStatus.PartyNotFound);

            party.Name = input.Name.Trim();
            party.NameAlias = string.IsNullOrWhiteSpace(input.NameAlias) ? input.Name.Trim() : input.NameAlias.Trim();
            new CustomerQuickUpdateSource(input, customer).Adapt(customer);

            await _service.UpdateAsync(customer, cancellationToken);
            return new(CustomerQuickUpdateStatus.Updated, customer, party);
        }

}
