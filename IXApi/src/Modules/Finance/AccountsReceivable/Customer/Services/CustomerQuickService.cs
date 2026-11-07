using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;
using Microsoft.EntityFrameworkCore;
using Mapster;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable.Customer;

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
                        var link = await _partyLocations.LinkLocationToPartyAsync(party.RecId, location.RecId, true, true, cancellationToken);
                        link.IsLocationOwner = NoYes.Yes;
                        await _unitOfWork.CompleteAsync(cancellationToken);
                    }

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    return (customer, party);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                    _unitOfWork.Context.ChangeTracker.Clear();
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
            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    var customer = await _service.GetByIdAsync(candidate => candidate.RecId == id,
                        includes: Array.Empty<string>(), asNoTracking: false, cancellationToken: cancellationToken);
                    var party = customer == null ? null : await _unitOfWork.Context.Set<DirPartyTable>()
                        .FirstOrDefaultAsync(candidate => candidate.RecId == customer.Party, cancellationToken);
                    if (customer == null || party == null)
                    {
                        await _unitOfWork.CommitTransactionAsync(cancellationToken);
                        return new CustomerQuickUpdateResult(customer == null
                            ? CustomerQuickUpdateStatus.CustomerNotFound : CustomerQuickUpdateStatus.PartyNotFound);
                    }

                    party.Name = input.Name.Trim();
                    party.NameAlias = string.IsNullOrWhiteSpace(input.NameAlias) ? input.Name.Trim() : input.NameAlias.Trim();
                    new CustomerQuickUpdateSource(input, customer).Adapt(customer);
                    if (!string.IsNullOrWhiteSpace(input.Street))
                        await UpdatePrimaryAddressAsync(party.RecId, input, cancellationToken);

                    await _service.UpdateAsync(customer, cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    return new CustomerQuickUpdateResult(CustomerQuickUpdateStatus.Updated, customer, party);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                    _unitOfWork.Context.ChangeTracker.Clear();
                    throw;
                }
            });
        }

        private async Task UpdatePrimaryAddressAsync(long partyId, CustomerQuickCreateDto input, CancellationToken cancellationToken)
        {
            var link = await _unitOfWork.Context.Set<DirPartyLocation>().SingleOrDefaultAsync(x =>
                x.Party == partyId && x.IsPostalAddress == NoYes.Yes && x.IsPrimary == NoYes.Yes, cancellationToken);
            if (link == null)
            {
                var address = new AddressInfoDto { Description = "Primary address", Primary = true,
                    Street = input.Street!.Trim(), CountryRegionId = input.CountryRegionId?.Trim() ?? string.Empty };
                var location = await _locations.CreateLocationAsync(address.Description, true, cancellationToken);
                await _postalAddresses.CreatePostalAddressAsync(location.RecId, address, cancellationToken);
                link = await _partyLocations.LinkLocationToPartyAsync(partyId, location.RecId, true, true, cancellationToken);
                link.IsLocationOwner = NoYes.Yes;
                return;
            }
            if (link.IsLocationOwner != NoYes.Yes)
                throw new ArgumentException($"Party {partyId} is not the location owner of primary address {link.Location}.");

            var now = DateTime.UtcNow;
            var postal = await _unitOfWork.Context.Set<LogisticsPostalAddress>().SingleOrDefaultAsync(x =>
                x.Location == link.Location && x.ValidFrom <= now && x.ValidTo >= now, cancellationToken)
                ?? throw new ArgumentException($"Primary location {link.Location} has no current postal address; maintain its effective dates in the address book.");
            await _postalAddresses.UpdatePostalAddressAsync(link.Location, new AddressInfoDto
            {
                Id = postal.RecId.ToString(), Location = link.Location, Primary = true, Street = input.Street!.Trim(),
                CountryRegionId = input.CountryRegionId?.Trim() ?? postal.CountryRegionId,
                State = postal.State, County = postal.County, City = postal.City, ZipCode = postal.ZipCode,
                DistrictName = postal.DistrictName, ValidFrom = postal.ValidFrom, ValidTo = postal.ValidTo
            }, cancellationToken);
        }

}
