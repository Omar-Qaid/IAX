# 1. Define Paths
$baseDir = "IXApi\src\Modules\Finance\Foundation\Currency"
$interfacesDir = "$baseDir\Interfaces"
$servicesDir = "$baseDir\Services"
$controllersDir = "$baseDir\Controllers"
$configDir = "$baseDir\Configuration"
$validationDir = "$baseDir\Validation"

# Delete old files
Remove-Item -Force "$interfacesDir\ICurrencyServices.cs" -ErrorAction Ignore
Remove-Item -Force "$servicesDir\CurrencyServices.cs" -ErrorAction Ignore
Remove-Item -Force "$controllersDir\CurrencyControllers.cs" -ErrorAction Ignore
Remove-Item -Force "$configDir\CurrencyConfiguration.cs" -ErrorAction Ignore
Remove-Item -Force "$validationDir\CurrencyValidators.cs" -ErrorAction Ignore

# 2. Write Interfaces
$iCurrency = @"
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public interface ICurrencyService : IBaseService<Currency>
    {
    }
}
"@
[System.IO.File]::WriteAllText("$interfacesDir\ICurrencyService.cs", $iCurrency, [System.Text.Encoding]::UTF8)

$iExchangeRate = @"
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public interface IExchangeRateService : IBaseService<ExchangeRate>
    {
    }
}
"@
[System.IO.File]::WriteAllText("$interfacesDir\IExchangeRateService.cs", $iExchangeRate, [System.Text.Encoding]::UTF8)

$iExchangeRatePair = @"
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public interface IExchangeRateCurrencyPairService : IBaseService<ExchangeRateCurrencyPair>
    {
        Task<BulkExchangeRatePairDto> BulkSaveAsync(BulkExchangeRatePairDto dto);
    }
}
"@
[System.IO.File]::WriteAllText("$interfacesDir\IExchangeRateCurrencyPairService.cs", $iExchangeRatePair, [System.Text.Encoding]::UTF8)

$iExchangeRateType = @"
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public interface IExchangeRateTypeService : IBaseService<ExchangeRateType>
    {
    }
}
"@
[System.IO.File]::WriteAllText("$interfacesDir\IExchangeRateTypeService.cs", $iExchangeRateType, [System.Text.Encoding]::UTF8)


# 3. Write Services
$sCurrency = @"
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class CurrencyService : BaseService<Currency>, ICurrencyService
    {
        public CurrencyService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }
    }
}
"@
[System.IO.File]::WriteAllText("$servicesDir\CurrencyService.cs", $sCurrency, [System.Text.Encoding]::UTF8)

$sExchangeRate = @"
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class ExchangeRateService : BaseService<ExchangeRate>, IExchangeRateService
    {
        public ExchangeRateService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }
    }
}
"@
[System.IO.File]::WriteAllText("$servicesDir\ExchangeRateService.cs", $sExchangeRate, [System.Text.Encoding]::UTF8)

$sExchangeRatePair = @"
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Finance.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
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
            
            await _unitOfWork.CompleteAsync();
            dto.RecId = pair.RecId;

            var existingRates = await ratesRepo.GetQueryable()
                .Where(r => r.ExchangeRateCurrencyPair == pair.RecId)
                .ToListAsync();

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
}
"@
[System.IO.File]::WriteAllText("$servicesDir\ExchangeRateCurrencyPairService.cs", $sExchangeRatePair, [System.Text.Encoding]::UTF8)

$sExchangeRateType = @"
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Identity;
using IAX.IXApi.Modules.Finance.Entities;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class ExchangeRateTypeService : BaseService<ExchangeRateType>, IExchangeRateTypeService
    {
        public ExchangeRateTypeService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
            : base(unitOfWork, currentUser)
        {
        }
    }
}
"@
[System.IO.File]::WriteAllText("$servicesDir\ExchangeRateTypeService.cs", $sExchangeRateType, [System.Text.Encoding]::UTF8)


# 4. Write Controllers
$cCurrency = @"
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "Currencies")]
    public class CurrencyController : BaseController<Currency, CurrencyDto>
    {
        public CurrencyController(ICurrencyService service, ILogger<CurrencyController> logger)
            : base(service, logger)
        {
        }
    }
}
"@
[System.IO.File]::WriteAllText("$controllersDir\CurrencyController.cs", $cCurrency, [System.Text.Encoding]::UTF8)

$cExchangeRate = @"
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "ExchangeRates")]
    public class ExchangeRateController : BaseController<ExchangeRate, ExchangeRateDto>
    {
        public ExchangeRateController(IExchangeRateService service, ILogger<ExchangeRateController> logger)
            : base(service, logger)
        {
        }
    }
}
"@
[System.IO.File]::WriteAllText("$controllersDir\ExchangeRateController.cs", $cExchangeRate, [System.Text.Encoding]::UTF8)

$cExchangeRatePair = @"
using System.Threading.Tasks;
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "ExchangeRateCurrencyPairs")]
    public class ExchangeRateCurrencyPairController : BaseController<ExchangeRateCurrencyPair, ExchangeRateCurrencyPairDto>
    {
        private readonly IExchangeRateCurrencyPairService _pairService;
        public ExchangeRateCurrencyPairController(IExchangeRateCurrencyPairService service, ILogger<ExchangeRateCurrencyPairController> logger)
            : base(service, logger)
        {
            _pairService = service;
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkSave([FromBody] BulkExchangeRatePairDto dto)
        {
            var result = await _pairService.BulkSaveAsync(dto);
            return Ok(APIResponse<BulkExchangeRatePairDto>.Ok(result));
        }
    }
}
"@
[System.IO.File]::WriteAllText("$controllersDir\ExchangeRateCurrencyPairController.cs", $cExchangeRatePair, [System.Text.Encoding]::UTF8)

$cExchangeRateType = @"
using IAX.IXApi.Modules.Identity.Permissions;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [DomainPermission("GeneralLedger", "ExchangeRateTypes")]
    public class ExchangeRateTypeController : BaseController<ExchangeRateType, ExchangeRateTypeDto>
    {
        public ExchangeRateTypeController(IExchangeRateTypeService service, ILogger<ExchangeRateTypeController> logger)
            : base(service, logger)
        {
        }
    }
}
"@
[System.IO.File]::WriteAllText("$controllersDir\ExchangeRateTypeController.cs", $cExchangeRateType, [System.Text.Encoding]::UTF8)


# 5. Write Configurations
$cfgCurrency = @"
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
    {
        public void Configure(EntityTypeBuilder<Currency> builder)
        {
            builder.ToTable("Currency");

            builder.HasKey(x => x.RecId);
            builder.HasIndex(x => new { x.CurrencyCode, x.DataAreaId }).IsUnique();

            builder.HasAlternateKey(x => x.CurrencyCode);

            builder.Property(x => x.CurrencyCode)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.Symbol)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(4)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => x.CurrencyCode).IsUnique();
            builder.HasIndex(x => x.DataAreaId);
        }
    }
}
"@
[System.IO.File]::WriteAllText("$configDir\CurrencyConfiguration.cs", $cfgCurrency, [System.Text.Encoding]::UTF8)

$cfgExchangeRate = @"
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
    {
        public void Configure(EntityTypeBuilder<ExchangeRate> builder)
        {
            builder.ToTable("ExchangeRate");

            builder.HasKey(x => x.RecId);

            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();

            builder.HasOne(x => x.ExchangeRateCurrencyPairTable)
                .WithMany(x => x.ExchangeRates)
                .HasForeignKey(x => x.ExchangeRateCurrencyPair)
                .HasPrincipalKey(x => x.RecId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
"@
[System.IO.File]::WriteAllText("$configDir\ExchangeRateConfiguration.cs", $cfgExchangeRate, [System.Text.Encoding]::UTF8)

$cfgExchangeRatePair = @"
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class ExchangeRateCurrencyPairConfiguration : IEntityTypeConfiguration<ExchangeRateCurrencyPair>
    {
        public void Configure(EntityTypeBuilder<ExchangeRateCurrencyPair> builder)
        {
            builder.ToTable("ExchangeRateCurrencyPair");

            builder.Property(x => x.DataAreaId)
                .HasMaxLength(4)
                .HasDefaultValue("dat")
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.FromCurrencyCode,
                x.ToCurrencyCode,
                x.ExchangeRateType,
                x.DataAreaId
            }).IsUnique();

            // From Currency
            builder.HasOne(x => x.FromCurrency)
                .WithMany(x => x.FromExchangeRateCurrencyPairs)
                .HasForeignKey(x => x.FromCurrencyCode)
                .HasPrincipalKey(x => x.CurrencyCode)
                .OnDelete(DeleteBehavior.Restrict);

            // To Currency
            builder.HasOne(x => x.ToCurrency)
                .WithMany(x => x.ToExchangeRateCurrencyPairs)
                .HasForeignKey(x => x.ToCurrencyCode)
                .HasPrincipalKey(x => x.CurrencyCode)
                .OnDelete(DeleteBehavior.Restrict);

            // Exchange Rate Type
            builder.HasOne(x => x.ExchangeRateTypeTable)
                .WithMany(x => x.ExchangeRateCurrencyPairs)
                .HasForeignKey(x => x.ExchangeRateType)
                .HasPrincipalKey(x => x.RecId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
"@
[System.IO.File]::WriteAllText("$configDir\ExchangeRateCurrencyPairConfiguration.cs", $cfgExchangeRatePair, [System.Text.Encoding]::UTF8)

$cfgExchangeRateType = @"
using IAX.IXApi.Modules.Finance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class ExchangeRateTypeConfiguration : IEntityTypeConfiguration<ExchangeRateType>
    {
        public void Configure(EntityTypeBuilder<ExchangeRateType> builder)
        {
            builder.ToTable("ExchangeRateType");
            builder.Property(x => x.DataAreaId).HasMaxLength(4).HasDefaultValue("dat").IsRequired();

            builder.HasKey(x => x.RecId);

            builder.HasAlternateKey(x => x.Name);

            builder.HasIndex(x => new { x.Name, x.DataAreaId }).IsUnique();
        }
    }
}
"@
[System.IO.File]::WriteAllText("$configDir\ExchangeRateTypeConfiguration.cs", $cfgExchangeRateType, [System.Text.Encoding]::UTF8)


# 6. Write Validators
$vCurrency = @"
using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class CurrencyDtoValidator : AbstractValidator<CurrencyDto>
    {
        public CurrencyDtoValidator()
        {
            RuleFor(x => x.CurrencyCode).NotEmpty().MaximumLength(FieldLengths.CurrencyCode);
            RuleFor(x => x.CurrencyCodeIso).NotEmpty().MaximumLength(FieldLengths.CurrencyCodeIso);
            RuleFor(x => x.Txt).MaximumLength(FieldLengths.Txt);
            RuleFor(x => x.Symbol).MaximumLength(FieldLengths.Symbol);
        }
    }
}
"@
[System.IO.File]::WriteAllText("$validationDir\CurrencyDtoValidator.cs", $vCurrency, [System.Text.Encoding]::UTF8)

$vExchangeRatePair = @"
using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class ExchangeRateCurrencyPairDtoValidator : AbstractValidator<ExchangeRateCurrencyPairDto>
    {
        public ExchangeRateCurrencyPairDtoValidator()
        {
            RuleFor(x => x.FromCurrencyCode).NotEmpty().MaximumLength(FieldLengths.FromCurrencyCode);
            RuleFor(x => x.ToCurrencyCode).NotEmpty().MaximumLength(FieldLengths.ToCurrencyCode);
        }
    }
}
"@
[System.IO.File]::WriteAllText("$validationDir\ExchangeRateCurrencyPairDtoValidator.cs", $vExchangeRatePair, [System.Text.Encoding]::UTF8)

$vExchangeRateType = @"
using FluentValidation;
using IAX.IXApi.Modules.Finance.Common;

namespace IAX.IXApi.Modules.Finance.Shared.Features
{
    public class ExchangeRateTypeDtoValidator : AbstractValidator<ExchangeRateTypeDto>
    {
        public ExchangeRateTypeDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(FieldLengths.Name);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(FieldLengths.Description);
        }
    }
}
"@
[System.IO.File]::WriteAllText("$validationDir\ExchangeRateTypeDtoValidator.cs", $vExchangeRateType, [System.Text.Encoding]::UTF8)


# 7. Restore and Update FinanceModule.cs
$finModule = @"
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Modules.Finance
{
    public static class FinanceModule
    {
        public static IServiceCollection AddFinanceModule(this IServiceCollection services, IConfiguration configuration)
        {
            // Fiscal Calendar services
            services.AddScoped<GeneralLedger.FiscalCalendar.ILedgerFiscalCalendarPeriodService, GeneralLedger.FiscalCalendar.LedgerFiscalCalendarPeriodService>();
            services.AddScoped<GeneralLedger.FiscalCalendar.IFiscalCalendarService, GeneralLedger.FiscalCalendar.FiscalCalendarService>();
            services.AddScoped<GeneralLedger.FiscalCalendar.IFiscalCalendarYearService, GeneralLedger.FiscalCalendar.FiscalCalendarYearService>();
            services.AddScoped<GeneralLedger.FiscalCalendar.IFiscalCalendarPeriodService, GeneralLedger.FiscalCalendar.FiscalCalendarPeriodService>();
            services.AddScoped<AccountsReceivable.PostingProfile.Interfaces.ICustPostingProfileService, AccountsReceivable.PostingProfile.Services.CustPostingProfileService>();

            // Explicit Finance registrations
            services.AddScoped<Shared.Features.ICurrencyService, Shared.Features.CurrencyService>();
            services.AddScoped<Shared.Features.IExchangeRateService, Shared.Features.ExchangeRateService>();
            services.AddScoped<Shared.Features.IExchangeRateCurrencyPairService, Shared.Features.ExchangeRateCurrencyPairService>();
            services.AddScoped<Shared.Features.IExchangeRateTypeService, Shared.Features.ExchangeRateTypeService>();
            
            services.AddScoped<Shared.Features.IDlvModeService, Shared.Features.DlvModeService>();
            services.AddScoped<Shared.Features.IDlvTermService, Shared.Features.DlvTermService>();
            services.AddScoped<Foundation.LegalEntities.ICompanyInfoService, Foundation.LegalEntities.CompanyInfoService>();
            
            services.AddScoped<Shared.Features.IPaymSchedLineService, Shared.Features.PaymSchedLineService>();
            services.AddScoped<Shared.Features.IPaymSchedService, Shared.Features.PaymSchedService>();
            services.AddScoped<Shared.Features.IPaymTermService, Shared.Features.PaymTermService>();
            services.AddScoped<Shared.Features.ITaxTableService, Shared.Features.TaxTableService>();
            
            return services;
        }
    }
}
"@
[System.IO.File]::WriteAllText("IXApi\src\Modules\Finance\FinanceModule.cs", $finModule, [System.Text.Encoding]::UTF8)

Write-Output "Successfully split Currency module files and updated FinanceModule.cs registrations!"
