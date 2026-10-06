using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Modules.Administration.NumberSequences;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Infrastructure.Identity;

namespace IAX.IXApi.Modules.Finance.AccountsReceivable
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [Route("api/v1/CustTable")]
    [Route("api/CustTable")]
    [DomainPermission("AccountsReceivable", "Customers")]
    public class CustomerController : BaseController<CustTable, CustomerDto>
    {
        private readonly IPartyService _partyService;
        private readonly ISysNumberSequenceService _numberSequences;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly ILocationService _locations;
        private readonly IPostalAddressService _postalAddresses;
        private readonly IPartyLocationService _partyLocations;

        public CustomerController(
            IBaseService<CustTable> service,
            ILogger<CustomerController> logger,
            IPartyService partyService,
            ISysNumberSequenceService numberSequences,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            ILocationService locations,
            IPostalAddressService postalAddresses,
            IPartyLocationService partyLocations)
            : base(service, logger)
        {
            _partyService = partyService;
            _numberSequences = numberSequences;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _locations = locations;
            _postalAddresses = postalAddresses;
            _partyLocations = partyLocations;
        }

        [HttpGet("country-regions")]
        public async Task<IActionResult> GetCountryRegions(CancellationToken cancellationToken = default)
        {
            var countries = await _unitOfWork.Context.Set<LogisticsAddressCountryRegion>()
                .AsNoTracking()
                .OrderBy(country => country.CountryRegionId)
                .Select(country => new { country.CountryRegionId, country.IsoCode })
                .ToListAsync(cancellationToken);
            return Ok(APIResponse<object>.Ok(countries));
        }

        [HttpGet("list")]
        public async Task<ActionResult<APIResponse<IEnumerable<CustomerListDto>>>> GetCustomerList(
            CancellationToken cancellationToken = default)
        {
            var customers = (await _service.GetAllAsync(cancellationToken: cancellationToken)).ToList();
            var partyIds = customers.Select(customer => customer.Party).Where(id => id != 0).Distinct().ToList();
            var parties = await _unitOfWork.Context.Set<DirPartyTable>()
                .AsNoTracking()
                .Where(party => partyIds.Contains(party.RecId))
                .ToDictionaryAsync(party => party.RecId, cancellationToken);

            var result = customers.Select(customer => MapCustomer(customer, parties.GetValueOrDefault(customer.Party))).ToList();
            return Ok(APIResponse<IEnumerable<CustomerListDto>>.Ok(result));
        }

        [HttpGet("{accountNumber}/sales-order-defaults")]
        public async Task<ActionResult<APIResponse<CustomerSalesOrderDefaultsDto>>> GetSalesOrderDefaults(
            string accountNumber, CancellationToken cancellationToken = default)
        {
            var area = _currentUser.GetDataAreaId() ?? "dat";
            var partyId = await _unitOfWork.Context.Set<CustTable>().AsNoTracking()
                .Where(customer => customer.AccountNum == accountNumber && customer.DataAreaId == area)
                .Select(customer => customer.Party)
                .FirstOrDefaultAsync(cancellationToken);
            if (partyId == 0)
                return NotFound(APIResponse<CustomerSalesOrderDefaultsDto>.Fail("Customer account was not found."));

            var addresses = await (
                from link in _unitOfWork.Context.Set<DirPartyLocation>().AsNoTracking()
                join address in _unitOfWork.Context.Set<LogisticsPostalAddress>().AsNoTracking()
                    on link.Location equals address.Location
                where link.Party == partyId && link.IsPostalAddress == IAX.IXApi.Modules.Finance.Common.NoYes.Yes
                    && address.DataAreaId == area
                    && address.ValidFrom <= DateTime.UtcNow && address.ValidTo >= DateTime.UtcNow
                orderby link.IsPrimary descending, address.RecId
                select new { address.RecId, address.Address, Primary = link.IsPrimary == IAX.IXApi.Modules.Finance.Common.NoYes.Yes }
            ).ToListAsync(cancellationToken);

            var contacts = await (
                from link in _unitOfWork.Context.Set<DirPartyLocation>().AsNoTracking()
                join contact in _unitOfWork.Context.Set<LogisticsElectronicAddress>().AsNoTracking()
                    on link.Location equals contact.Location
                where link.Party == partyId && link.IsPostalAddress == IAX.IXApi.Modules.Finance.Common.NoYes.No
                    && (contact.Type == IAX.IXApi.Modules.Finance.Common.ElectronicAddressType.Email
                        || contact.Type == IAX.IXApi.Modules.Finance.Common.ElectronicAddressType.Phone)
                orderby link.IsPrimary descending, contact.IsPrimary descending, contact.RecId
                select new { contact.Type, contact.Locator, Primary = link.IsPrimary == IAX.IXApi.Modules.Finance.Common.NoYes.Yes
                    || contact.IsPrimary == IAX.IXApi.Modules.Finance.Common.NoYes.Yes }
            ).ToListAsync(cancellationToken);

            return Ok(APIResponse<CustomerSalesOrderDefaultsDto>.Ok(new CustomerSalesOrderDefaultsDto
            {
                Address = addresses.FirstOrDefault()?.Address ?? string.Empty,
                Addresses = addresses.Select(address => new CustomerSalesOrderAddressDto
                {
                    Id = address.RecId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    Address = address.Address,
                    Primary = address.Primary
                }).ToList(),
                Contacts = contacts.Select(contact => new CustomerSalesOrderContactDto
                {
                    Type = contact.Type.ToString(),
                    Number = contact.Locator,
                    Primary = contact.Primary
                }).ToList()
            }));
        }

        [HttpPost("quick-create")]
        public async Task<ActionResult<APIResponse<CustomerListDto>>> QuickCreate(
            [FromBody] CustomerQuickCreateDto input,
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
                    var customer = await _service.AddAsync(new CustTable
                    {
                        AccountNum = sequence.Code,
                        Party = party.RecId,
                        CustGroupId = input.CustGroupId.Trim(),
                        CurrencyCode = input.CurrencyCode.Trim(),
                        CustCategory = input.CustCategory?.Trim() ?? string.Empty,
                        PaymTermId = input.PaymTermId?.Trim() ?? string.Empty,
                        PaymModeId = input.PaymModeId?.Trim() ?? string.Empty,
                        DlvModeId = input.DlvModeId?.Trim() ?? string.Empty,
                        TaxGroupId = input.TaxGroupId?.Trim() ?? string.Empty,
                        VatNum = input.VatNum?.Trim() ?? string.Empty,
                        CountryRegionId = input.CountryRegionId?.Trim() ?? string.Empty,
                        Memo = input.Memo?.Trim(),
                        InvoiceAccount = input.InvoiceAccount?.Trim() ?? string.Empty,
                        InventSiteId = input.InventSiteId?.Trim() ?? string.Empty,
                        InventLocationId = input.InventLocationId?.Trim() ?? string.Empty,
                        SalesPoolId = input.SalesPoolId?.Trim() ?? string.Empty,
                        CashDiscBaseDays = input.CashDiscBaseDays ?? 0,
                        UseCashDisc = input.UseCashDisc ?? 0,
                        InclTax = input.InclTax ?? 0,
                        BlockFloorLimitUseInChannel = input.BlockFloorLimitUseInChannel ?? 0,
                        PrepaymentValue = input.PrepaymentValue ?? 0,
                        PrePayType = input.PrePayType ?? 0,
                        DataAreaId = _currentUser.GetDataAreaId() ?? "dat"
                    }, cancellationToken);

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
                    return MapCustomer(customer, party);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            });

            return Ok(APIResponse<CustomerListDto>.Ok(result, "Created successfully"));
        }

        [HttpPut("quick-update/{id:long}")]
        public async Task<ActionResult<APIResponse<CustomerListDto>>> QuickUpdate(
            long id,
            [FromBody] CustomerQuickCreateDto input,
            CancellationToken cancellationToken = default)
        {
            var customer = await _service.GetByIdAsync(candidate => candidate.RecId == id, includes: Array.Empty<string>(), cancellationToken: cancellationToken);
            if (customer == null)
                return NotFound(APIResponse<CustomerListDto>.Fail("Customer not found"));

            var party = await _unitOfWork.Context.Set<DirPartyTable>()
                .FirstOrDefaultAsync(candidate => candidate.RecId == customer.Party, cancellationToken);
            if (party == null)
                return NotFound(APIResponse<CustomerListDto>.Fail("Customer party not found"));

            party.Name = input.Name.Trim();
            party.NameAlias = string.IsNullOrWhiteSpace(input.NameAlias) ? input.Name.Trim() : input.NameAlias.Trim();
            customer.CustGroupId = input.CustGroupId.Trim();
            customer.CurrencyCode = input.CurrencyCode.Trim();
            customer.CustCategory = input.CustCategory?.Trim() ?? string.Empty;
            customer.PaymTermId = input.PaymTermId?.Trim() ?? string.Empty;
            customer.PaymModeId = input.PaymModeId?.Trim() ?? string.Empty;
            customer.DlvModeId = input.DlvModeId?.Trim() ?? string.Empty;
            customer.TaxGroupId = input.TaxGroupId?.Trim() ?? string.Empty;
            customer.VatNum = input.VatNum?.Trim() ?? string.Empty;
            customer.CountryRegionId = input.CountryRegionId?.Trim() ?? string.Empty;
            customer.Memo = input.Memo?.Trim();
            if (input.InvoiceAccount != null) customer.InvoiceAccount = input.InvoiceAccount.Trim();
            if (input.InventSiteId != null) customer.InventSiteId = input.InventSiteId.Trim();
            if (input.InventLocationId != null) customer.InventLocationId = input.InventLocationId.Trim();
            if (input.SalesPoolId != null) customer.SalesPoolId = input.SalesPoolId.Trim();
            if (input.CashDiscBaseDays.HasValue) customer.CashDiscBaseDays = input.CashDiscBaseDays.Value;
            if (input.UseCashDisc.HasValue) customer.UseCashDisc = input.UseCashDisc.Value;
            if (input.InclTax.HasValue) customer.InclTax = input.InclTax.Value;
            if (input.BlockFloorLimitUseInChannel.HasValue) customer.BlockFloorLimitUseInChannel = input.BlockFloorLimitUseInChannel.Value;
            if (input.PrepaymentValue.HasValue) customer.PrepaymentValue = input.PrepaymentValue.Value;
            if (input.PrePayType.HasValue) customer.PrePayType = input.PrePayType.Value;

            await _service.UpdateAsync(customer, cancellationToken);
            return Ok(APIResponse<CustomerListDto>.Ok(MapCustomer(customer, party), "Updated successfully"));
        }

        private static CustomerListDto MapCustomer(CustTable customer, DirPartyTable? party) => new()
        {
            RecId = customer.RecId,
            Party = customer.Party,
            AccountNumber = customer.AccountNum,
            Name = party?.Name ?? customer.AccountNum,
            NameAr = party?.NameAlias,
            CustomerGroupId = customer.CustGroupId,
            CurrencyCode = customer.CurrencyCode,
            CustCategory = customer.CustCategory,
            PaymTermId = customer.PaymTermId,
            PaymModeId = customer.PaymModeId,
            DlvModeId = customer.DlvModeId,
            TaxGroupId = customer.TaxGroupId,
            VatNum = customer.VatNum,
            CountryRegionId = customer.CountryRegionId,
            Memo = customer.Memo,
            InvoiceAccount = customer.InvoiceAccount,
            InventSiteId = customer.InventSiteId,
            InventLocationId = customer.InventLocationId,
            SalesPoolId = customer.SalesPoolId,
            CashDiscBaseDays = customer.CashDiscBaseDays,
            UseCashDisc = customer.UseCashDisc,
            InclTax = customer.InclTax,
            BlockFloorLimitUseInChannel = customer.BlockFloorLimitUseInChannel,
            PrepaymentValue = customer.PrepaymentValue,
            PrePayType = customer.PrePayType,
            Status = customer.IsActive ? "active" : "inactive",
            CreatedAt = customer.CreatedAt ?? DateTime.UtcNow
        };

    }
}

