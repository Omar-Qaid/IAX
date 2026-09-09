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

        public CustomerController(
            IBaseService<CustTable> service,
            ILogger<CustomerController> logger,
            IPartyService partyService,
            ISysNumberSequenceService numberSequences,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
            : base(service, logger)
        {
            _partyService = partyService;
            _numberSequences = numberSequences;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
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
                        DataAreaId = _currentUser.GetDataAreaId() ?? "dat"
                    }, cancellationToken);

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
            Status = customer.IsActive ? "active" : "inactive",
            CreatedAt = customer.CreatedAt ?? DateTime.UtcNow
        };
    }
}

