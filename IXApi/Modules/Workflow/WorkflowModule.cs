using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAX.IXApi.Modules.Workflow
{
    public static class WorkflowModule
    {
        public static IServiceCollection AddWorkflowModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<Activities.IWfActivityControlService, Activities.WfActivityControlService>();
            services.AddScoped<Activities.IWfActivityControlsOptionService, Activities.WfActivityControlsOptionService>();
            services.AddScoped<Activities.IWfActivityControlsValidationService, Activities.WfActivityControlsValidationService>();
            services.AddScoped<Activities.IWfActivityMappingVariableService, Activities.WfActivityMappingVariableService>();
            services.AddScoped<Activities.IWfActivityNotificationDispatcher, Activities.WfActivityNotificationDispatcher>();
            services.AddScoped<Activities.IWfActivityService, Activities.WfActivityService>();
            services.AddScoped<Activities.IWfActivityTypeService, Activities.WfActivityTypeService>();
            services.AddScoped<Categories.IWfCategoryService, Categories.WfCategoryService>();
            services.AddScoped<Controls.IWfControlService, Controls.WfControlService>();
            services.AddScoped<DataExchange.IWfExcelImportService, DataExchange.WfExcelImportService>();
            services.AddScoped<Operators.IWfOperatorService, Operators.WfOperatorService>();
            services.AddScoped<Performers.IWfPerformerService, Performers.WfPerformerService>();
            services.AddScoped<Priorities.IWfPriorityService, Priorities.WfPriorityService>();
            services.AddScoped<Processes.IWfProcessService, Processes.WfProcessService>();
            services.AddScoped<Requests.IValidationEngine, Requests.ValidationEngine>();
            services.AddScoped<Requests.IWfRequestControlService, Requests.WfRequestControlService>();
            services.AddScoped<Requests.IWfRequestControlsOptionService, Requests.WfRequestControlsOptionService>();
            services.AddScoped<Requests.IWfRequestControlsValidationService, Requests.WfRequestControlsValidationService>();
            services.AddScoped<Requests.IWfRequestMappingVariableService, Requests.WfRequestMappingVariableService>();
            services.AddScoped<Requests.IWfRequestService, Requests.WfRequestService>();
            services.AddScoped<Steps.IWfStepService, Steps.WfStepService>();
            services.AddScoped<Transitions.IWfTransitionService, Transitions.WfTransitionService>();
            services.AddScoped<Variables.IWfDataTypeService, Variables.WfDataTypeService>();
            services.AddScoped<Variables.IWfVariableService, Variables.WfVariableService>();
            return services;
        }
    }
}
