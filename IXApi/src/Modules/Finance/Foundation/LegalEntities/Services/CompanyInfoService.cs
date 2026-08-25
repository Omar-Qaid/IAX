using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Finance.Persistence;
using Microsoft.EntityFrameworkCore;

using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;

namespace IAX.IXApi.Modules.Finance.Foundation.LegalEntities
{
    public class CompanyInfoService : BaseService<CompanyInfo>, ICompanyInfoService
    {
        private readonly IFinanceDataContext _dbContext;
        private readonly ISysNumberSequenceService _numberSequenceService;
        private readonly IPartyService _partyService;
        private readonly ILocationService _locationService;
        private readonly IGlobalAddressBookService _globalAddressBookService;
        private readonly IPartyLocationService _partyLocationService;

        public CompanyInfoService(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUser, 
            IFinanceDataContext dbContext, 
            ISysNumberSequenceService numberSequenceService, 
            IPartyService partyService,
            ILocationService locationService,
            IGlobalAddressBookService globalAddressBookService,
            IPartyLocationService partyLocationService) : base(unitOfWork, currentUser)
        {
            _dbContext = dbContext;
            _numberSequenceService = numberSequenceService;
            _partyService = partyService;
            _locationService = locationService;
            _globalAddressBookService = globalAddressBookService;
            _partyLocationService = partyLocationService;
        }

        protected override IQueryable<CompanyInfo> ApplyDomainFilters(IQueryable<CompanyInfo> query)
        {
            // Ignore the global DataAreaId filter since CompanyInfo represents the legal entities themselves
            // Users need to be able to see all companies to select/switch between them.
            // We manually apply the IsDeleted filter since IgnoreQueryFilters drops all global filters.
            return query.IgnoreQueryFilters().Where(c => !c.IsDeleted);
        }

        public override async Task<CompanyInfo?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            if (id == null || !long.TryParse(id.ToString(), out var longId)) return null;
            return await ApplyDomainFilters(_repository.GetQueryable()).FirstOrDefaultAsync(c => c.RecId == longId, cancellationToken);
        }

        public override async Task<CompanyInfo> AddAsync(CompanyInfo entity, CancellationToken cancellationToken = default)
        {
            if (entity.Party <= 0)
            {
                var nsResult = await _numberSequenceService.NextAsync("DirPartyTable", cancellationToken: cancellationToken);
                string partyNumber = nsResult.Code ?? Guid.NewGuid().ToString("N").Substring(0, 20);

                var party = new DirPartyTable
                {
                    Name = string.IsNullOrEmpty(entity.Name) ? "New Company" : entity.Name,
                    NameAlias = string.IsNullOrEmpty(entity.Name) ? "New Company" : entity.Name,
                    PartyNumber = partyNumber,
                    LanguageId = string.IsNullOrEmpty(entity.LanguageId) ? "en-us" : entity.LanguageId,
                    AddressBookNames = string.Empty
                };
                _dbContext.Set<DirPartyTable>().Add(party);
                await _dbContext.SaveChangesAsync(cancellationToken);
                entity.Party = party.RecId;
            }

            return await base.AddAsync(entity, cancellationToken);
        }

        public async Task<CompanyInfo> CreateCompanyWithAddressBookAsync(CompanyInfoDto dto, CancellationToken cancellationToken)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // 1. Save DirPartyTable
                    var nsResult = await _numberSequenceService.NextAsync("DirPartyTable", cancellationToken: cancellationToken);
                    string partyNumber = nsResult.Code ?? Guid.NewGuid().ToString("N").Substring(0, 20);

                    var party = new DirPartyTable
                    {
                        Name = string.IsNullOrEmpty(dto.Name) ? "New Company" : dto.Name,
                        NameAlias = string.IsNullOrEmpty(dto.Name) ? "New Company" : dto.Name,
                        PartyNumber = partyNumber,
                        LanguageId = string.IsNullOrEmpty(dto.LanguageId) ? "en-us" : dto.LanguageId,
                        AddressBookNames = string.Empty,
                        CreatedBy = _currentUser.GetCurrentUserId() ?? "sys",
                        OwnerAccountId = _currentUser.GetOwnerAccountId() ?? "sys",
                        IsActive = IAX.IXApi.Modules.Finance.Common.NoYes.Yes
                    };
                    _dbContext.Set<DirPartyTable>().Add(party);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    // 2. Save CompanyInfo
                    var company = new CompanyInfo();
                    ApplyCompanyFields(dto, company);
                    company.Party = party.RecId;
                    company.CreatedBy = _currentUser.GetCurrentUserId() ?? "sys";
                    company.OwnerAccountId = _currentUser.GetOwnerAccountId() ?? "sys";
                    company.IsActive = true;
                    _dbContext.Set<CompanyInfo>().Add(company);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    // 3. Save Addresses and Contacts using the global orchestrator
                    if (dto.Addresses != null || dto.Contacts != null)
                    {
                        await _globalAddressBookService.UpdateGlobalAddressBookAsync(party.RecId, dto.Addresses ?? new(), dto.Contacts ?? new(), cancellationToken);
                    }

                    await transaction.CommitAsync(cancellationToken);
                    return company;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        public async Task<CompanyInfo> UpdateCompanyWithAddressBookAsync(string id, CompanyInfoDto dto, CancellationToken cancellationToken)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    if (!long.TryParse(id, out long parsedId)) throw new Exception("Invalid Company ID.");

                    var company = await ApplyDomainFilters(_repository.GetQueryable()).FirstOrDefaultAsync(c => c.RecId == parsedId, cancellationToken);
                    if (company == null) throw new Exception("Company not found.");

                    // 1. Update CompanyInfo and DirPartyTable Name
                    var originalRecId = company.RecId;
                    var originalParty = company.Party;
                    
                    ApplyCompanyFields(dto, company);
                    
                    company.RecId = originalRecId;
                    company.Party = originalParty;
                    
                    _dbContext.Set<CompanyInfo>().Update(company);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    var party = await _dbContext.Set<DirPartyTable>().FirstOrDefaultAsync(p => p.RecId == company.Party, cancellationToken);
                    if (party != null)
                    {
                        party.Name = dto.Name;
                        party.NameAlias = dto.Name;
                        _dbContext.Set<DirPartyTable>().Update(party);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }

                    // 2. Delegate address and contact updates to global orchestrator
                    if (dto.Addresses != null || dto.Contacts != null)
                    {
                        await _globalAddressBookService.UpdateGlobalAddressBookAsync(company.Party, dto.Addresses ?? new(), dto.Contacts ?? new(), cancellationToken);
                    }

                    await transaction.CommitAsync(cancellationToken);
                    return company;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        // UpdateGlobalAddressBookAsync has been moved to GlobalAddressBookService

        private static void ApplyCompanyFields(CompanyInfoDto dto, CompanyInfo company)
        {
            company.DataArea = dto.DataArea;
            company.Name = dto.Name;
            company.LanguageId = dto.LanguageId;
            company.CurrencyCode = dto.CurrencyCode;
            company.TaxLicenseNum = dto.TaxLicenseNum;
            company.FederalTaxId = dto.FederalTaxId;
            company.BankAccount = dto.BankAccount;
            company.Calendar = dto.Calendar;
            company.TimeZone = dto.TimeZone;
            company.Memo = dto.Memo;
            company.ArabicName = dto.ArabicName;
            company.LocalizedRegion = dto.LocalizedRegion;

            // Logo and ReportLogo are legacy byte[] columns. New images are stored
            // through Document Management, so normal legal-entity saves preserve them.
        }

        public async Task PopulateGlobalAddressBookAsync(IEnumerable<CompanyInfoDto> dtos, CancellationToken cancellationToken)
        {
            if (dtos == null || !dtos.Any()) return;

            var partyIds = dtos.Select(x => x.Party).Distinct().ToList();

            var partyLocations = await _dbContext.Set<DirPartyLocation>()
                .Where(x => partyIds.Contains(x.Party))
                .ToListAsync(cancellationToken);

            var locationIds = partyLocations.Select(x => x.Location).Distinct().ToList();

            var postalAddresses = await _dbContext.Set<LogisticsPostalAddress>()
                .Where(x => locationIds.Contains(x.Location))
                .ToListAsync(cancellationToken);

            var electronicAddresses = await _dbContext.Set<LogisticsElectronicAddress>()
                .Where(x => locationIds.Contains(x.Location))
                .ToListAsync(cancellationToken);

            var logisticsLocations = await _dbContext.Set<LogisticsLocation>()
                .Where(x => locationIds.Contains(x.RecId))
                .ToListAsync(cancellationToken);

            var partyLocationIds = partyLocations.Select(x => x.RecId).Distinct().ToList();

            var dirPartyLocationRoles = await _dbContext.Set<DirPartyLocationRole>()
                .Where(x => partyLocationIds.Contains(x.PartyLocation))
                .ToListAsync(cancellationToken);

            var roleIds = dirPartyLocationRoles.Select(x => x.LocationRole).Distinct().ToList();
            
            var logisticsLocationRoles = await _dbContext.Set<LogisticsLocationRole>()
                .Where(x => roleIds.Contains(x.RecId))
                .ToListAsync(cancellationToken);

            foreach (var dto in dtos)
            {
                var locationsForParty = partyLocations.Where(x => x.Party == dto.Party).ToList();
                var locIdsForParty = locationsForParty.Select(x => x.Location).ToList();

                var pAddrs = postalAddresses.Where(x => locIdsForParty.Contains(x.Location)).ToList();
                dto.Addresses = pAddrs.Select(p => {
                    var pLoc = locationsForParty.FirstOrDefault(l => l.Location == p.Location);
                    var loc = logisticsLocations.FirstOrDefault(l => l.RecId == p.Location);
                    return new AddressInfoDto
                    {
                        Id = p.RecId.ToString(),
                        Location = p.Location,
                        LocationId = loc?.LocationId ?? string.Empty,
                        Description = loc?.Description ?? string.Empty,
                        Address = p.Address,
                        Primary = pLoc?.IsPrimary == IAX.IXApi.Modules.Finance.Common.NoYes.Yes,
                        Street = p.Street,
                        City = p.City,
                        State = p.State,
                        ZipCode = p.ZipCode,
                        County = p.County,
                        CountryRegionId = p.CountryRegionId,
                        ValidFrom = p.ValidFrom,
                        ValidTo = p.ValidTo,
                        Roles = pLoc != null 
                            ? dirPartyLocationRoles.Where(r => r.PartyLocation == pLoc.RecId)
                                .Select(r => logisticsLocationRoles.FirstOrDefault(lr => lr.RecId == r.LocationRole)?.Name ?? "")
                                .Where(n => !string.IsNullOrEmpty(n))
                                .ToList()
                            : new List<string>()
                    };
                }).ToList();

                var eAddrs = electronicAddresses.Where(x => locIdsForParty.Contains(x.Location)).ToList();
                dto.Contacts = eAddrs.Select(e => {
                    var pLoc = locationsForParty.FirstOrDefault(l => l.Location == e.Location);
                    var loc = logisticsLocations.FirstOrDefault(l => l.RecId == e.Location);
                    return new ContactInfoDto
                    {
                        Id = e.RecId.ToString(),
                        Location = e.Location,
                        LocationId = loc?.LocationId ?? string.Empty,
                        Description = e.Description,
                        Type = e.Type.ToString(),
                        Number = e.Locator,
                        Extension = e.LocatorExtension,
                        Primary = e.IsPrimary == IAX.IXApi.Modules.Finance.Common.NoYes.Yes || (pLoc != null && pLoc.IsPrimary == IAX.IXApi.Modules.Finance.Common.NoYes.Yes),
                        Roles = pLoc != null 
                            ? dirPartyLocationRoles.Where(r => r.PartyLocation == pLoc.RecId)
                                .Select(r => logisticsLocationRoles.FirstOrDefault(lr => lr.RecId == r.LocationRole)?.Name ?? "")
                                .Where(n => !string.IsNullOrEmpty(n))
                                .ToList()
                            : new List<string>()
                    };
                }).ToList();
            }
        }
    }
}


