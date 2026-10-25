using FluentValidation;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance;
using IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar;
using IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar.Validation;
using IAX.IXApi.Shared.Application.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class GeneralLedgerGenericArchitectureTests
{
    [Fact]
    public void Fiscal_calendar_crud_controllers_use_registered_generic_services()
    {
        var services = new ServiceCollection();
        services.AddFinanceModule(new ConfigurationBuilder().Build());
        var controllers = new[]
        {
            typeof(FiscalCalendarController),
            typeof(FiscalCalendarYearController),
            typeof(FiscalCalendarPeriodController),
            typeof(LedgerFiscalCalendarPeriodController)
        };

        foreach (var controller in controllers)
        {
            Assert.True(controller.BaseType is { IsGenericType: true } &&
                controller.BaseType.GetGenericTypeDefinition() == typeof(BaseController<,>), controller.FullName);

            var entity = controller.BaseType!.GetGenericArguments()[0];
            var serviceContract = typeof(IBaseService<>).MakeGenericType(entity);
            var serviceParameter = controller.GetConstructors().Single().GetParameters()[0].ParameterType;
            Assert.True(serviceContract.IsAssignableFrom(serviceParameter), controller.FullName);

            var registration = Assert.Single(services, descriptor => descriptor.ServiceType == serviceParameter);
            Assert.NotNull(registration.ImplementationType);
            Assert.True(serviceParameter.IsAssignableFrom(registration.ImplementationType), controller.FullName);
            Assert.True(typeof(BaseService<>).MakeGenericType(entity)
                .IsAssignableFrom(registration.ImplementationType), controller.FullName);
        }
    }

    [Fact]
    public void General_ledger_validators_use_existing_base_validator()
    {
        var validators = new[]
        {
            typeof(FiscalCalendarDtoValidator),
            typeof(FiscalCalendarYearDtoValidator),
            typeof(FiscalCalendarPeriodDtoValidator),
            typeof(LedgerFiscalCalendarPeriodDtoValidator)
        };

        foreach (var validator in validators)
        {
            Assert.Equal(typeof(BaseValidator<>), validator.BaseType?.GetGenericTypeDefinition());
        }
    }
}
