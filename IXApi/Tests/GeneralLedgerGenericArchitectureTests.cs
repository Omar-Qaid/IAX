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

    [Fact]
    public void General_ledger_features_adhere_to_seven_folder_layout()
    {
        var candidatePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src", "Modules", "Finance", "GeneralLedger"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "IXApi", "src", "Modules", "Finance", "GeneralLedger"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../IXApi/src/Modules/Finance/GeneralLedger"))
        };

        var glPath = candidatePaths.FirstOrDefault(Directory.Exists);
        if (glPath != null)
        {
            var expectedFolders = new[] { "Configuration", "Controllers", "Dtos", "Entities", "Interfaces", "Services", "Validation" };
            var featureDirs = Directory.GetDirectories(glPath);

            Assert.NotEmpty(featureDirs);
            foreach (var dir in featureDirs)
            {
                var subdirs = Directory.GetDirectories(dir).Select(Path.GetFileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var expected in expectedFolders)
                {
                    Assert.True(subdirs.Contains(expected), $"Feature '{Path.GetFileName(dir)}' is missing folder '{expected}'.");
                }
            }
        }
    }
}

