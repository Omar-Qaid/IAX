using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Events;
using IAX.IXApi.Modules.Workflow.Handlers;
using IAX.IXApi.Modules.Workflow.Jobs;
using IAX.IXApi.Shared.Domain.Events;
using IAX.IXApi.Shared.Application.Batch;
using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Workflow
{
    public static class WorkflowModule
    {
        public static IServiceCollection AddWorkflowModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddBatchService<WorkflowRequestSubmissionBatchService>(
                WorkflowRequestSubmissionBatchService.ServiceKey, "Workflow process request submission");
            services.AddScoped<Activities.IWfActivityControlService, Activities.WfActivityControlService>();
            services.AddScoped<IBaseService<Activities.WfActivityControl>>(sp => sp.GetRequiredService<Activities.IWfActivityControlService>());
            services.AddScoped<Activities.IWfActivityControlsOptionService, Activities.WfActivityControlsOptionService>();
            services.AddScoped<IBaseService<Activities.WfActivityControlsOption>>(sp => sp.GetRequiredService<Activities.IWfActivityControlsOptionService>());
            services.AddScoped<Activities.IWfActivityControlsValidationService, Activities.WfActivityControlsValidationService>();
            services.AddScoped<IBaseService<Activities.WfActivityControlsValidation>>(sp => sp.GetRequiredService<Activities.IWfActivityControlsValidationService>());
            services.AddScoped<Activities.IWfActivityMappingVariableService, Activities.WfActivityMappingVariableService>();
            services.AddScoped<IBaseService<Activities.WfActivityMappingVariable>>(sp => sp.GetRequiredService<Activities.IWfActivityMappingVariableService>());
            services.AddScoped<Activities.IWfActivityNotificationDispatcher, Activities.WfActivityNotificationDispatcher>();
            services.AddScoped<Activities.IWfActivityService, Activities.WfActivityService>();
            services.AddScoped<IBaseService<Activities.WfActivity>>(sp => sp.GetRequiredService<Activities.IWfActivityService>());
            services.AddScoped<Activities.IWfActivityTypeService, Activities.WfActivityTypeService>();
            services.AddScoped<IBaseService<Activities.WfActivityType>>(sp => sp.GetRequiredService<Activities.IWfActivityTypeService>());
            services.AddScoped<Categories.IWfCategoryService, Categories.WfCategoryService>();
            services.AddScoped<IBaseService<Categories.WfCategory>>(sp => sp.GetRequiredService<Categories.IWfCategoryService>());
            services.AddScoped<Controls.IWfControlService, Controls.WfControlService>();
            services.AddScoped<IBaseService<Controls.WfControl>>(sp => sp.GetRequiredService<Controls.IWfControlService>());
            services.AddScoped<DataExchange.IWfExcelImportService, DataExchange.WfExcelImportService>();
            services.AddScoped<Operators.IWfOperatorService, Operators.WfOperatorService>();
            services.AddScoped<IBaseService<Operators.WfOperator>>(sp => sp.GetRequiredService<Operators.IWfOperatorService>());
            services.AddScoped<Performers.IWfPerformerService, Performers.WfPerformerService>();
            services.AddScoped<IBaseService<Performers.WfPerformer>>(sp => sp.GetRequiredService<Performers.IWfPerformerService>());
            services.AddScoped<Performers.IWfPerformerTypeService, Performers.WfPerformerTypeService>();
            services.AddScoped<IBaseService<Performers.WfPerformerType>>(sp => sp.GetRequiredService<Performers.IWfPerformerTypeService>());
            services.AddScoped<Priorities.IWfPriorityService, Priorities.WfPriorityService>();
            services.AddScoped<IBaseService<Priorities.WfPriority>>(sp => sp.GetRequiredService<Priorities.IWfPriorityService>());
            services.AddScoped<ProcessTypes.IWfProcessTypeService, ProcessTypes.WfProcessTypeService>();
            services.AddScoped<IBaseService<ProcessTypes.WfProcessType>>(sp => sp.GetRequiredService<ProcessTypes.IWfProcessTypeService>());
            services.AddScoped<Processes.IWfProcessService, Processes.WfProcessService>();
            services.AddScoped<IBaseService<Processes.WfProcess>>(sp => sp.GetRequiredService<Processes.IWfProcessService>());
            services.AddScoped<PrintTemplates.IPrintTemplateService, PrintTemplates.PrintTemplateService>();
            services.AddScoped<PrintTemplates.IReportResourceAuthorizer, PrintTemplates.WorkflowReportResourceAuthorizer>();
            services.AddSingleton<PrintTemplates.PrintTemplateDocumentValidator>();
            services.AddScoped<Requests.IValidationEngine, Requests.ValidationEngine>();
            services.AddScoped<Requests.IWfRequestControlService, Requests.WfRequestControlService>();
            services.AddScoped<IBaseService<Requests.WfRequestControl>>(sp => sp.GetRequiredService<Requests.IWfRequestControlService>());
            services.AddScoped<Requests.IWfRequestControlsOptionService, Requests.WfRequestControlsOptionService>();
            services.AddScoped<IBaseService<Requests.WfRequestControlsOption>>(sp => sp.GetRequiredService<Requests.IWfRequestControlsOptionService>());
            services.AddScoped<Requests.IWfRequestControlsValidationService, Requests.WfRequestControlsValidationService>();
            services.AddScoped<IBaseService<Requests.WfRequestControlsValidation>>(sp => sp.GetRequiredService<Requests.IWfRequestControlsValidationService>());
            services.AddScoped<Requests.IWfRequestMappingVariableService, Requests.WfRequestMappingVariableService>();
            services.AddScoped<IBaseService<Requests.WfRequestMappingVariable>>(sp => sp.GetRequiredService<Requests.IWfRequestMappingVariableService>());
            services.AddScoped<Requests.IWfRequestService, Requests.WfRequestService>();
            services.AddScoped<IBaseService<Requests.WfRequest>>(sp => sp.GetRequiredService<Requests.IWfRequestService>());
            services.AddScoped<Steps.IWfStepService, Steps.WfStepService>();
            services.AddScoped<IBaseService<Steps.WfStep>>(sp => sp.GetRequiredService<Steps.IWfStepService>());
            services.AddScoped<Transitions.IWfTransitionService, Transitions.WfTransitionService>();
            services.AddScoped<IBaseService<Transitions.WfTransition>>(sp => sp.GetRequiredService<Transitions.IWfTransitionService>());
            services.AddScoped<Variables.IWfDataTypeService, Variables.WfDataTypeService>();
            services.AddScoped<IBaseService<Variables.WfDataType>>(sp => sp.GetRequiredService<Variables.IWfDataTypeService>());
            services.AddScoped<Variables.IWfVariableService, Variables.WfVariableService>();
            services.AddScoped<IBaseService<Variables.WfVariable>>(sp => sp.GetRequiredService<Variables.IWfVariableService>());

            // Validators
            services.AddScoped<FluentValidation.IValidator<Controls.WfControlDto>, Controls.WfControlDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Categories.WfCategoryDto>, Categories.WfCategoryDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Priorities.WfPriorityDto>, Priorities.WfPriorityDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<ProcessTypes.WfProcessTypeDto>, ProcessTypes.WfProcessTypeDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Processes.WfProcessDto>, Processes.WfProcessDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Steps.WfStepDto>, Steps.WfStepDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Variables.WfVariableDto>, Variables.WfVariableDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Transitions.WfTransitionDto>, Transitions.WfTransitionDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Activities.WfActivityDto>, Activities.WfActivityDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Activities.WfActivityTypeDto>, Activities.WfActivityTypeDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Activities.WfActivityControlsValidationDto>, Activities.WfActivityControlsValidationDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Requests.WfRequestDto>, Requests.WfRequestDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Requests.WfRequestControlDto>, Requests.WfRequestControlDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Requests.WfRequestControlsOptionDto>, Requests.WfRequestControlsOptionDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Requests.WfRequestControlsValidationDto>, Requests.WfRequestControlsValidationDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Performers.WfPerformerDto>, Performers.WfPerformerDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Performers.WfPerformerTypeDto>, Performers.WfPerformerTypeDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<Operators.WfOperatorDto>, Operators.WfOperatorDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<PrintTemplates.CreatePrintTemplateDto>, PrintTemplates.CreatePrintTemplateDtoValidator>();
            services.AddScoped<FluentValidation.IValidator<PrintTemplates.UpdatePrintTemplateDto>, PrintTemplates.UpdatePrintTemplateDtoValidator>();

            services.AddScoped<ISysEventHandler<WfActivityAlertDispatchedEvent>, WfActivityAlertDispatchedEventHandler>();
            services.AddScoped<ISysEventHandler<WfAssignmentAutoPassedEvent>, WfAssignmentAutoPassedNotificationHandler>();
            services.AddBatchService<WorkflowActivityAutoPassBatchService>(
                WorkflowActivityAutoPassBatchService.ServiceKey, "Workflow activity auto-pass");
            return services;
        }
    }
}
