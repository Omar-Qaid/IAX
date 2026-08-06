using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class CurrencyService : BaseService<Currency>, ICurrencyService
    {
        public CurrencyService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }
    }
    public class ExchangeRateService : BaseService<ExchangeRate>, IExchangeRateService
    {
        public ExchangeRateService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }
    }
    public class ExchangeRateCurrencyPairService : BaseService<ExchangeRateCurrencyPair>, IExchangeRateCurrencyPairService
    {
        public ExchangeRateCurrencyPairService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }

        public async Task<BulkExchangeRatePairDto> BulkSaveAsync(BulkExchangeRatePairDto dto)
        {
            var pairRepo = _unitOfWork.Repository<ExchangeRateCurrencyPair>();
            var ratesRepo = _unitOfWork.Repository<ExchangeRate>();

            ExchangeRateCurrencyPair pair;
            if (dto.RecId > 0)
            {
                pair = await pairRepo.GetByIdAsync(dto.RecId);
                if (pair == null) throw new Exception("Pair not found");
                pair.FromCurrencyCode = dto.FromCurrencyCode;
                pair.ToCurrencyCode = dto.ToCurrencyCode;
                pair.ExchangeRateType = dto.ExchangeRateType;
                pair.ExchangeRateDisplayFactor = dto.ExchangeRateDisplayFactor;
                await pairRepo.UpdateAsync(pair);
            }
            else
            {
                pair = new ExchangeRateCurrencyPair
                {
                    FromCurrencyCode = dto.FromCurrencyCode,
                    ToCurrencyCode = dto.ToCurrencyCode,
                    ExchangeRateType = dto.ExchangeRateType,
                    ExchangeRateDisplayFactor = dto.ExchangeRateDisplayFactor
                };
                pair = await pairRepo.AddAsync(pair);
            }
            
            // Need to save before updating rates if it's new
            await _unitOfWork.CompleteAsync();
            dto.RecId = pair.RecId;

            var existingRates = await ratesRepo.GetQueryable()
                .Where(r => r.ExchangeRateCurrencyPair == pair.RecId)
                .ToListAsync();

            // Find rates to delete (exist in DB but not in DTO)
            var dtoRateIds = dto.ExchangeRates.Where(r => r.RecId > 0).Select(r => r.RecId).ToList();
            var ratesToDelete = existingRates.Where(r => !dtoRateIds.Contains(r.RecId)).ToList();
            if (ratesToDelete.Any())
            {
                await ratesRepo.RemoveRangeAsync(ratesToDelete);
            }

            foreach (var rateDto in dto.ExchangeRates)
            {
                if (rateDto.RecId > 0)
                {
                    var existing = existingRates.FirstOrDefault(r => r.RecId == rateDto.RecId);
                    if (existing != null)
                    {
                        existing.ValidFrom = rateDto.ValidFrom;
                        existing.ValidTo = rateDto.ValidTo;
                        existing.ExchangeRateValue = rateDto.ExchangeRateValue;
                        await ratesRepo.UpdateAsync(existing);
                    }
                }
                else
                {
                    var newRate = new ExchangeRate
                    {
                        ExchangeRateCurrencyPair = pair.RecId,
                        ValidFrom = rateDto.ValidFrom,
                        ValidTo = rateDto.ValidTo,
                        ExchangeRateValue = rateDto.ExchangeRateValue
                    };
                    await ratesRepo.AddAsync(newRate);
                }
            }

            await _unitOfWork.CompleteAsync();
            return dto;
        }
    }
    public class ExchangeRateTypeService : BaseService<ExchangeRateType>, IExchangeRateTypeService
    {
        public ExchangeRateTypeService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }
    }
}


