using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    public class GlobalAddressBookService : IGlobalAddressBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocationService _locationService;
        private readonly IPostalAddressService _postalAddressService;
        private readonly IElectronicAddressService _electronicAddressService;
        private readonly IPartyLocationService _partyLocationService;

        public GlobalAddressBookService(
            IUnitOfWork unitOfWork,
            ILocationService locationService,
            IPostalAddressService postalAddressService,
            IElectronicAddressService electronicAddressService,
            IPartyLocationService partyLocationService)
        {
            _unitOfWork = unitOfWork;
            _locationService = locationService;
            _postalAddressService = postalAddressService;
            _electronicAddressService = electronicAddressService;
            _partyLocationService = partyLocationService;
        }

        public async Task UpdateGlobalAddressBookAsync(long partyRecId, List<AddressInfoDto> addresses, List<ContactInfoDto> contacts, CancellationToken cancellationToken = default)
        {
            if (partyRecId <= 0) return;

            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    var existingLocations = await _partyLocationService.GetPartyLocationsAsync(partyRecId, cancellationToken);
                    long targetLocationId = 0;

                    // 1. Process Addresses first
                    if (addresses != null && addresses.Any())
                    {
                        // Ensure exactly one address in the incoming list is primary
                        var primaryAddresses = addresses.Where(a => a.Primary).ToList();
                        if (primaryAddresses.Count > 1)
                        {
                            // If a new address is added as primary, prioritize it. Otherwise, prioritize the last one marked primary.
                            var chosenPrimary = primaryAddresses.FirstOrDefault(a => string.IsNullOrEmpty(a.Id) || a.Id == "0" || a.Location == 0)
                                                ?? primaryAddresses.Last();
                            foreach (var addr in addresses)
                            {
                                addr.Primary = (addr == chosenPrimary);
                            }
                        }
                        else if (primaryAddresses.Count == 0)
                        {
                            addresses.First().Primary = true;
                        }

                        var existingPostalLocs = existingLocations.Where(x => x.IsPostalAddress == IAX.IXApi.Modules.Finance.Common.NoYes.Yes).ToList();
                        var incomingIds = new HashSet<long>();

                        foreach (var addr in addresses)
                        {
                            if (long.TryParse(addr.Id, out long parsedId) && parsedId > 0 && addr.Location > 0)
                            {
                                await _locationService.UpdateLocationDescriptionAsync(addr.Location, addr.Description, cancellationToken);
                                await _postalAddressService.UpdatePostalAddressAsync(addr.Location, addr, cancellationToken);
                                await _partyLocationService.UpdatePartyLocationPrimaryAsync(partyRecId, addr.Location, true, addr.Primary, cancellationToken);
                                incomingIds.Add(addr.Location);
                                if (addr.Primary || targetLocationId == 0)
                                {
                                    targetLocationId = addr.Location;
                                }
                            }
                            else
                            {
                                var location = await _locationService.CreateLocationAsync(addr.Description, true, cancellationToken);
                                await _postalAddressService.CreatePostalAddressAsync(location.RecId, addr, cancellationToken);
                                await _partyLocationService.LinkLocationToPartyAsync(partyRecId, location.RecId, true, addr.Primary, cancellationToken);
                                incomingIds.Add(location.RecId);
                                if (addr.Primary || targetLocationId == 0)
                                {
                                    targetLocationId = location.RecId;
                                }
                            }
                        }

                        var toDelete = existingPostalLocs.Where(x => !incomingIds.Contains(x.Location)).ToList();
                        foreach (var del in toDelete)
                        {
                            var postal = await _unitOfWork.Context.Set<LogisticsPostalAddress>().FirstOrDefaultAsync(p => p.Location == del.Location, cancellationToken);
                            if (postal != null)
                            {
                                await _postalAddressService.DeletePostalAddressAsync(postal.RecId, cancellationToken);
                            }
                            
                            await _partyLocationService.UnlinkLocationAsync(partyRecId, del.Location, cancellationToken);
                            await _partyLocationService.DeleteOrphanedLocationAsync(del.Location, cancellationToken);
                        }
                    }

                    // 2. If no address is provided, resolve/create a single shared location for contacts
                    if (targetLocationId == 0)
                    {
                        var firstContactLoc = existingLocations.FirstOrDefault(x => x.IsPostalAddress == IAX.IXApi.Modules.Finance.Common.NoYes.No);
                        if (firstContactLoc != null)
                        {
                            targetLocationId = firstContactLoc.Location;
                        }
                        else if (contacts != null && contacts.Any())
                        {
                            var location = await _locationService.CreateLocationAsync("Contacts Location", false, cancellationToken);
                            await _partyLocationService.LinkLocationToPartyAsync(partyRecId, location.RecId, false, true, cancellationToken);
                            targetLocationId = location.RecId;
                        }
                    }

                    // 3. Process Contacts under targetLocationId
                    if (contacts != null && targetLocationId > 0)
                    {
                        var incomingContactIds = new HashSet<long>();

                        foreach (var contact in contacts)
                        {
                            contact.Location = targetLocationId;

                            if (long.TryParse(contact.Id, out long parsedId) && parsedId > 0)
                            {
                                await _electronicAddressService.UpdateElectronicAddressAsync(targetLocationId, contact, cancellationToken);
                                incomingContactIds.Add(parsedId);
                            }
                            else
                            {
                                var electronic = await _electronicAddressService.CreateElectronicAddressAsync(targetLocationId, contact, cancellationToken);
                                incomingContactIds.Add(electronic.RecId);
                            }
                        }

                        var existingContactsForLocation = await _unitOfWork.Context.Set<LogisticsElectronicAddress>()
                            .Where(x => x.Location == targetLocationId)
                            .ToListAsync(cancellationToken);

                        var toDeleteContacts = existingContactsForLocation.Where(x => !incomingContactIds.Contains(x.RecId)).ToList();
                        foreach (var delContact in toDeleteContacts)
                        {
                            await _electronicAddressService.DeleteElectronicAddressAsync(delContact.RecId, cancellationToken);
                        }

                        var remainingContactsCount = await _unitOfWork.Context.Set<LogisticsElectronicAddress>().CountAsync(x => x.Location == targetLocationId, cancellationToken);
                        var postalAddressExists = await _unitOfWork.Context.Set<LogisticsPostalAddress>().AnyAsync(x => x.Location == targetLocationId, cancellationToken);
                        if (remainingContactsCount == 0 && !postalAddressExists)
                        {
                            await _partyLocationService.UnlinkLocationAsync(partyRecId, targetLocationId, cancellationToken);
                            await _partyLocationService.DeleteOrphanedLocationAsync(targetLocationId, cancellationToken);
                        }
                    }

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            });
        }

        public async Task<AddressInfoDto> CreatePartyAddressAsync(long partyId, AddressInfoDto dto, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    var location = await _locationService.CreateLocationAsync(dto.Description, true, cancellationToken);
                    var postal = await _postalAddressService.CreatePostalAddressAsync(location.RecId, dto, cancellationToken);
                    await _partyLocationService.LinkLocationToPartyAsync(partyId, location.RecId, true, dto.Primary, cancellationToken);

                    dto.Id = postal.RecId.ToString();
                    dto.Location = location.RecId;
                    dto.LocationId = location.LocationId;

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    return dto;
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            });
        }

        public async Task<AddressInfoDto> UpdatePartyAddressAsync(long partyId, AddressInfoDto dto, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    await _locationService.UpdateLocationDescriptionAsync(dto.Location, dto.Description, cancellationToken);
                    var postal = await _postalAddressService.UpdatePostalAddressAsync(dto.Location, dto, cancellationToken);
                    await _partyLocationService.UpdatePartyLocationPrimaryAsync(partyId, dto.Location, true, dto.Primary, cancellationToken);

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    return dto;
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            });
        }

        public async Task<bool> DeletePartyAddressAsync(long partyId, long locationId, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    var postalLoc = await _unitOfWork.Context.Set<LogisticsPostalAddress>().FirstOrDefaultAsync(x => x.Location == locationId, cancellationToken);
                    if (postalLoc == null) return false;

                    await _postalAddressService.DeletePostalAddressAsync(postalLoc.RecId, cancellationToken);
                    await _partyLocationService.UnlinkLocationAsync(partyId, locationId, cancellationToken);
                    await _partyLocationService.DeleteOrphanedLocationAsync(locationId, cancellationToken);

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    return true;
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            });
        }

        public async Task<ContactInfoDto> CreatePartyContactAsync(long partyId, ContactInfoDto dto, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    var location = await _locationService.CreateLocationAsync(dto.Description, false, cancellationToken);
                    var contact = await _electronicAddressService.CreateElectronicAddressAsync(location.RecId, dto, cancellationToken);
                    await _partyLocationService.LinkLocationToPartyAsync(partyId, location.RecId, false, dto.Primary, cancellationToken);

                    dto.Id = contact.RecId.ToString();
                    dto.Location = location.RecId;

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    return dto;
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            });
        }

        public async Task<ContactInfoDto> UpdatePartyContactAsync(long partyId, ContactInfoDto dto, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    await _locationService.UpdateLocationDescriptionAsync(dto.Location, dto.Description, cancellationToken);
                    var contact = await _electronicAddressService.UpdateElectronicAddressAsync(dto.Location, dto, cancellationToken);
                    await _partyLocationService.UpdatePartyLocationPrimaryAsync(partyId, dto.Location, false, dto.Primary, cancellationToken);

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    return dto;
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            });
        }

        public async Task<bool> DeletePartyContactAsync(long partyId, long locationId, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.Context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    var contactLoc = await _unitOfWork.Context.Set<LogisticsElectronicAddress>().FirstOrDefaultAsync(x => x.Location == locationId, cancellationToken);
                    if (contactLoc == null) return false;

                    await _electronicAddressService.DeleteElectronicAddressAsync(contactLoc.RecId, cancellationToken);
                    await _partyLocationService.UnlinkLocationAsync(partyId, locationId, cancellationToken);
                    await _partyLocationService.DeleteOrphanedLocationAsync(locationId, cancellationToken);

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    return true;
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            });
        }
    }
}


