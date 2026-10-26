using System.IO;
using System.Linq;
using System.Reflection;
using IAX.IXApi.Api.Controllers;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Modules.Communication;
using IAX.IXApi.Modules.Communication.Chat.Controllers;
using IAX.IXApi.Modules.Communication.Chat.Dtos;
using IAX.IXApi.Modules.Communication.Chat.Entities;
using IAX.IXApi.Modules.Communication.Chat.Services;
using IAX.IXApi.Modules.Communication.Chat.Validation;
using IAX.IXApi.Modules.Communication.Notifications.Controllers;
using IAX.IXApi.Modules.Communication.Notifications.Dtos;
using IAX.IXApi.Modules.Communication.Notifications.Entities;
using IAX.IXApi.Modules.Communication.Notifications.Interfaces;
using IAX.IXApi.Modules.Communication.Notifications.Services;
using IAX.IXApi.Modules.Communication.Notifications.Validation;
using IAX.IXApi.Shared.Application.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class CommunicationGenericArchitectureTests
{
    [Fact]
    public void Communication_controllers_inherit_base_controller()
    {
        var controllers = new[]
        {
            (Controller: typeof(SysChatController), Entity: typeof(SysChatMessage), Dto: typeof(SysChatMessageDto)),
            (Controller: typeof(SysNotificationController), Entity: typeof(SysNotification), Dto: typeof(SysNotificationDto)),
            (Controller: typeof(SysNotificationPreferenceController), Entity: typeof(SysNotificationPreference), Dto: typeof(SysNotificationPreferenceDto)),
            (Controller: typeof(SysNotificationRecipientController), Entity: typeof(SysNotificationRecipient), Dto: typeof(SysNotificationRecipientDto)),
            (Controller: typeof(SysNotificationTemplateController), Entity: typeof(SysNotificationTemplate), Dto: typeof(SysNotificationTemplateDto))
        };

        foreach (var (controller, entity, dto) in controllers)
        {
            var expectedBase = typeof(BaseController<,>).MakeGenericType(entity, dto);
            Assert.True(expectedBase.IsAssignableFrom(controller),
                $"{controller.Name} does not inherit from BaseController<{entity.Name}, {dto.Name}>");

            var ctor = controller.GetConstructors().Single();
            var serviceParam = ctor.GetParameters()[0].ParameterType;
            Assert.True(typeof(IBaseService<>).MakeGenericType(entity).IsAssignableFrom(serviceParam),
                $"{controller.Name} constructor parameter {serviceParam.Name} does not implement IBaseService<{entity.Name}>");
        }
    }

    [Fact]
    public void Communication_services_implement_generic_interfaces_and_base_service()
    {
        var services = new ServiceCollection();
        services.AddCommunicationModule(new ConfigurationBuilder().Build());

        var serviceDefinitions = new[]
        {
            (Interface: typeof(ISysChatService), Implementation: typeof(SysChatService), Entity: typeof(SysChatMessage)),
            (Interface: typeof(ISysNotificationService), Implementation: typeof(SysNotificationService), Entity: typeof(SysNotification)),
            (Interface: typeof(ISysNotificationPreferenceService), Implementation: typeof(SysNotificationPreferenceService), Entity: typeof(SysNotificationPreference)),
            (Interface: typeof(ISysNotificationRecipientService), Implementation: typeof(SysNotificationRecipientService), Entity: typeof(SysNotificationRecipient)),
            (Interface: typeof(ISysNotificationTemplateService), Implementation: typeof(SysNotificationTemplateService), Entity: typeof(SysNotificationTemplate))
        };

        foreach (var (iface, impl, entity) in serviceDefinitions)
        {
            // Verify interface inherits IBaseService<TEntity>
            Assert.True(typeof(IBaseService<>).MakeGenericType(entity).IsAssignableFrom(iface),
                $"{iface.Name} does not implement IBaseService<{entity.Name}>");

            // Verify implementation inherits BaseService<TEntity>
            Assert.True(typeof(BaseService<>).MakeGenericType(entity).IsAssignableFrom(impl),
                $"{impl.Name} does not inherit from BaseService<{entity.Name}>");

            // Verify registration in DI
            var registration = Assert.Single(services, d => d.ServiceType == iface);
            Assert.Equal(impl, registration.ImplementationType);
        }
    }

    [Fact]
    public void Communication_validators_inherit_base_validator()
    {
        var validators = new[]
        {
            (Validator: typeof(SysChatMessageDtoValidator), Dto: typeof(SysChatMessageDto)),
            (Validator: typeof(SendChatMessageRequestValidator), Dto: typeof(SendChatMessageRequest)),
            (Validator: typeof(SysNotificationDtoValidator), Dto: typeof(SysNotificationDto)),
            (Validator: typeof(CreateSysNotificationDtoValidator), Dto: typeof(CreateSysNotificationDto)),
            (Validator: typeof(SysNotificationPreferenceDtoValidator), Dto: typeof(SysNotificationPreferenceDto)),
            (Validator: typeof(SysNotificationRecipientDtoValidator), Dto: typeof(SysNotificationRecipientDto)),
            (Validator: typeof(SysNotificationTemplateDtoValidator), Dto: typeof(SysNotificationTemplateDto))
        };

        foreach (var (validator, dto) in validators)
        {
            var expectedBase = typeof(BaseValidator<>).MakeGenericType(dto);
            Assert.True(expectedBase.IsAssignableFrom(validator),
                $"{validator.Name} does not inherit from BaseValidator<{dto.Name}>");
        }
    }

    [Fact]
    public void Communication_features_adhere_to_seven_folder_layout()
    {
        var candidatePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src", "Modules", "Communication"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "IXApi", "src", "Modules", "Communication"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../IXApi/src/Modules/Communication"))
        };

        var commPath = candidatePaths.FirstOrDefault(Directory.Exists);
        if (commPath != null)
        {
            var expectedFolders = new[] { "Configuration", "Controllers", "Dtos", "Entities", "Interfaces", "Services", "Validation" };
            var features = new[] { "Chat", "Notifications" };

            foreach (var feature in features)
            {
                var featureDir = Path.Combine(commPath, feature);
                Assert.True(Directory.Exists(featureDir), $"Feature directory '{feature}' does not exist.");

                var subdirs = Directory.GetDirectories(featureDir).Select(Path.GetFileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var expected in expectedFolders)
                {
                    Assert.True(subdirs.Contains(expected), $"Feature '{feature}' is missing required folder '{expected}'.");
                }
            }
        }
    }

    [Fact]
    public void Chat_controller_disables_generic_raw_crud()
    {
        var disabledActions = new[] { "Create", "Update", "Delete", "GetAll", "GetPaged", "GetById", "GetNumberSequence" };
        foreach (var action in disabledActions)
        {
            var method = typeof(SysChatController).GetMethod(
                action,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            Assert.True(method is not null, $"SysChatController.{action} should be declared on the controller.");
            Assert.True(method!.IsDefined(typeof(NonActionAttribute), inherit: false),
                $"SysChatController.{action} should have [NonAction]");
        }
    }

    [Fact]
    public void Notification_controller_disables_unfiltered_lookup()
    {
        var method = Assert.Single(typeof(SysNotificationController).GetMethods(), candidate =>
            candidate.Name == "GetById" && candidate.DeclaringType == typeof(SysNotificationController)
            && candidate.GetBaseDefinition().DeclaringType != typeof(SysNotificationController));
        Assert.True(method.IsDefined(typeof(NonActionAttribute), inherit: false),
            "SysNotificationController.GetById should have [NonAction]");
    }

    [Fact]
    public async Task Notification_template_and_recipient_validators_validate_dtos()
    {
        var templateValidator = new SysNotificationTemplateDtoValidator();
        var recipientValidator = new SysNotificationRecipientDtoValidator();

        var templateResult = await templateValidator.ValidateAsync(new SysNotificationTemplateDto());
        Assert.False(templateResult.IsValid);

        var recipientResult = await recipientValidator.ValidateAsync(new SysNotificationRecipientDto());
        Assert.False(recipientResult.IsValid);
    }
}
