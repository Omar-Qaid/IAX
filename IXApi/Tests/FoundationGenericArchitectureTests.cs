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
        Assert.Contains(services, d => d.ServiceType == typeof(IValidator<TaxTableDto>));
    }
}
