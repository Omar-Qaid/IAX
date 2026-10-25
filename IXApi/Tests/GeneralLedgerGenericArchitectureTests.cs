using FluentValidation;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Finance;
using IAX.IXApi.Modules.Finance.GeneralLedger.Bank;
using IAX.IXApi.Modules.Finance.GeneralLedger.Bank.Validation;
using IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar;
using IAX.IXApi.Modules.Finance.GeneralLedger.FiscalCalendar.Validation;
using IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry;
using IAX.IXApi.Modules.Finance.GeneralLedger.JournalEntry.Validation;
using IAX.IXApi.Modules.Finance.GeneralLedger.Ledger;
using IAX.IXApi.Modules.Finance.GeneralLedger.Ledger.Validation;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerAccount;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerAccount.Validation;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable;
using IAX.IXApi.Modules.Finance.GeneralLedger.LedgerJournalTable.Validation;
using IAX.IXApi.Shared.Application.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class GeneralLedgerGenericArchitectureTests
{
    [Fact]
    public void General_ledger_crud_controllers_use_registered_generic_services()
    {
        var services = new ServiceCollection();
        services.AddFinanceModule(new ConfigurationBuilder().Build());
        var controllers = new[]
        {
            typeof(FiscalCalendarController),
            typeof(FiscalCalendarYearController),
            typeof(FiscalCalendarPeriodController),
            typeof(LedgerFiscalCalendarPeriodController),
            typeof(BankAccountTableController),
            typeof(BankGroupController),
            typeof(BankTransTypeController),
            typeof(BankAccountTransController),
            typeof(BankAccountStatementController),
            typeof(LedgerController),
            typeof(LedgerJournalTableController),
            typeof(LedgerJournalTransController),
            typeof(LedgerJournalNameController),
            typeof(MainAccountController),
            typeof(MainAccountCategoryController),
            typeof(LedgerChartOfAccountsController),
            typeof(GeneralJournalEntryController),
            typeof(GeneralJournalAccountEntryController),
            typeof(SubledgerVoucherGeneralJournalEntryController)
        };

        foreach (var controller in controllers)
        {
            Assert.True(controller.BaseType is { IsGenericType: true } &&
                controller.BaseType.GetGenericTypeDefinition() == typeof(BaseController<,>), controller.FullName);

            var entity = controller.BaseType!.GetGenericArguments()[0];
            var serviceContract = typeof(IBaseService<>).MakeGenericType(entity);
            var primaryCtor = controller.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
            var serviceParameter = primaryCtor.GetParameters()[0].ParameterType;
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
            typeof(LedgerFiscalCalendarPeriodDtoValidator),
            typeof(BankAccountTableDtoValidator),
            typeof(BankGroupDtoValidator),
            typeof(BankTransTypeDtoValidator),
            typeof(BankAccountTransDtoValidator),
            typeof(BankAccountStatementDtoValidator),
            typeof(LedgerDtoValidator),
            typeof(LedgerJournalTableDtoValidator),
            typeof(LedgerJournalTransDtoValidator),
            typeof(LedgerJournalNameDtoValidator),
            typeof(MainAccountDtoValidator),
            typeof(MainAccountCategoryDtoValidator),
            typeof(LedgerChartOfAccountsDtoValidator),
            typeof(GeneralJournalEntryDtoValidator),
            typeof(GeneralJournalAccountEntryDtoValidator),
            typeof(SubledgerVoucherGeneralJournalEntryDtoValidator)
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
