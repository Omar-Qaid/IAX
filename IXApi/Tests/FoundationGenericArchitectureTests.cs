using FluentValidation;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance;
using IAX.IXApi.Modules.Finance.Foundation.Currency;
using IAX.IXApi.Modules.Finance.Foundation.DeliveryModes;
using IAX.IXApi.Modules.Finance.Foundation.DeliveryTerms;
using IAX.IXApi.Modules.Finance.Foundation.Departments;
using IAX.IXApi.Modules.Finance.Foundation.Genders;
using IAX.IXApi.Modules.Finance.Foundation.HcmShowrooms;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.LegalEntities;
using IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses;
using IAX.IXApi.Modules.Finance.Foundation.Markup;
using IAX.IXApi.Modules.Finance.Foundation.Nationalities;
using IAX.IXApi.Modules.Finance.Foundation.Occupations;
using IAX.IXApi.Modules.Finance.Foundation.PaymentSchedules;
using IAX.IXApi.Modules.Finance.Foundation.PaymentTerms;
using IAX.IXApi.Modules.Finance.Foundation.Tax;
using IAX.IXApi.Modules.Finance.Foundation.Tax.Controllers;
using IAX.IXApi.Modules.Finance.Foundation.Tax.Validation;
using IAX.IXApi.Shared.Application.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class FoundationGenericArchitectureTests
{
    [Fact]
    public void Foundation_crud_controllers_inherit_base_controller()
    {
        var controllers = new[]
        {
            typeof(CurrencyController),
            typeof(ExchangeRateController),
            typeof(ExchangeRateCurrencyPairController),
            typeof(ExchangeRateTypeController),
            typeof(DlvModeController),
            typeof(DlvTermController),
            typeof(HcmDepartmentController),
            typeof(LegacyDepartmentController),
            typeof(GenderController),
            typeof(HcmShowroomController),
            typeof(HcmNationalityController),
            typeof(LegacyNationalityController),
            typeof(HcmOccupationController),
            typeof(LegacyOccupationController),
            typeof(HcmWorkerController),
            typeof(CompanyInfoController),
            typeof(MarkupTableController),
            typeof(PaymSchedController),
            typeof(PaymSchedLineController),
            typeof(PaymTermController),
            typeof(TaxAuthorityAddressController),
            typeof(TaxExemptCodeController),
            typeof(TaxGroupController),
            typeof(TaxItemGroupController),
            typeof(TaxLedgerAccountGroupController),
            typeof(TaxPeriodHeadController),
            typeof(TaxTableController)
        };

        foreach (var controller in controllers)
        {
            Assert.True(controller.BaseType is { IsGenericType: true } &&
                controller.BaseType.GetGenericTypeDefinition() == typeof(BaseController<,>), controller.FullName);
        }
    }

    [Fact]
    public void Foundation_validators_inherit_base_validator()
    {
        var validators = new[]
        {
            typeof(CurrencyDtoValidator),
            typeof(ExchangeRateTypeDtoValidator),
            typeof(ExchangeRateCurrencyPairDtoValidator),
            typeof(ExchangeRateDtoValidator),
            typeof(BulkExchangeRatePairDtoValidator),
            typeof(DlvModeDtoValidator),
            typeof(DlvTermDtoValidator),
            typeof(CompanyInfoDtoValidator),
            typeof(AddressInfoDtoValidator),
            typeof(ContactInfoDtoValidator),
            typeof(ContactPersonDtoValidator),
            typeof(MarkupTableDtoValidator),
            typeof(MarkupTransValidator),
            typeof(PaymSchedDtoValidator),
            typeof(PaymSchedLineDtoValidator),
            typeof(PaymTermDtoValidator),
            typeof(GenderDtoValidator),
            typeof(HcmNationalityDtoValidator),
            typeof(HcmOccupationDtoValidator),
            typeof(HcmDepartmentDtoValidator),
            typeof(HcmShowroomDtoValidator),
            typeof(HcmWorkerDtoValidator),
            typeof(TaxTableDtoValidator),
            typeof(TaxAuthorityAddressDtoValidator),
            typeof(TaxExemptCodeDtoValidator),
            typeof(TaxGroupDtoValidator),
            typeof(TaxItemGroupDtoValidator),
            typeof(TaxLedgerAccountGroupDtoValidator),
            typeof(TaxPeriodHeadDtoValidator),
            typeof(TaxDataDtoValidator),
            typeof(TaxGroupDataDtoValidator),
            typeof(TaxOnItemDtoValidator)
        };

        foreach (var validator in validators)
        {
            Assert.Equal(typeof(BaseValidator<>), validator.BaseType?.GetGenericTypeDefinition());
        }
    }

    [Fact]
    public void Foundation_services_and_validators_are_registered_in_di()
    {
        var services = new ServiceCollection();
        services.AddFinanceModule(new ConfigurationBuilder().Build());

        // Assert validator registrations exist
        Assert.Contains(services, d => d.ServiceType == typeof(IValidator<CurrencyDto>));
        Assert.Contains(services, d => d.ServiceType == typeof(IValidator<CompanyInfoDto>));
        Assert.Contains(services, d => d.ServiceType == typeof(IValidator<HcmDepartmentDto>));
        Assert.Contains(services, d => d.ServiceType == typeof(IValidator<GenderDto>));
        Assert.Contains(services, d => d.ServiceType == typeof(IValidator<HcmShowroomDto>));
        Assert.Contains(services, d => d.ServiceType == typeof(IValidator<HcmNationalityDto>));
        Assert.Contains(services, d => d.ServiceType == typeof(IValidator<HcmOccupationDto>));
        Assert.Contains(services, d => d.ServiceType == typeof(IValidator<HcmWorkerDto>));
        Assert.Contains(services, d => d.ServiceType == typeof(IValidator<MarkupTableDto>));
        Assert.Contains(services, d => d.ServiceType == typeof(IValidator<MarkupTransDto>));
        Assert.Contains(services, d => d.ServiceType == typeof(IValidator<TaxTableDto>));
    }

    [Fact]
    public void Foundation_features_adhere_to_seven_folder_layout()
    {
        var candidatePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src", "Modules", "Finance", "Foundation"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "IXApi", "src", "Modules", "Finance", "Foundation"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../IXApi/src/Modules/Finance/Foundation"))
        };

        var foundationPath = candidatePaths.FirstOrDefault(Directory.Exists);
        if (foundationPath != null)
        {
            var expectedFolders = new[] { "Configuration", "Controllers", "Dtos", "Entities", "Interfaces", "Services", "Validation" };

            // Check top-level features (excluding container folders Dimensions and HcmWorkers)
            var topLevelFeatures = Directory.GetDirectories(foundationPath)
                .Where(d => !Path.GetFileName(d).Equals("Dimensions", StringComparison.OrdinalIgnoreCase)
                         && !Path.GetFileName(d).Equals("HcmWorkers", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Check nested features in Dimensions and HcmWorkers
            var nestedFeatures = new List<string>();
            var dimensionsDir = Path.Combine(foundationPath, "Dimensions");
            if (Directory.Exists(dimensionsDir))
                nestedFeatures.AddRange(Directory.GetDirectories(dimensionsDir));

            var hcmWorkersDir = Path.Combine(foundationPath, "HcmWorkers");
            if (Directory.Exists(hcmWorkersDir))
                nestedFeatures.AddRange(Directory.GetDirectories(hcmWorkersDir));

            var allFeatures = topLevelFeatures.Concat(nestedFeatures).ToList();
            Assert.NotEmpty(allFeatures);

            foreach (var dir in allFeatures)
            {
                var subdirs = Directory.GetDirectories(dir).Select(Path.GetFileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var expected in expectedFolders)
                {
                    Assert.True(subdirs.Contains(expected), $"Feature '{Path.GetFileName(dir)}' in '{dir}' is missing folder '{expected}'.");
                }
            }
        }
    }
}

