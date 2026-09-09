# Workflow source inventory

Companion to [the architecture analysis](workflow-architecture-analysis.md). This is a source/declaration inventory, not a claim of behavioral test coverage. Responsibilities of the runtime and exceptional classes are explained in that report. Repeated configuration families follow the existing entity / DTO / validator / service / controller / EF mapping pattern.

Workflow C# files: 209

## Activities

### [Activities/IWfActivityControlService.cs](../src/Modules/Workflow/Activities/IWfActivityControlService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public interface IWfActivityControlService : IBaseService<WfActivityControl>`

### [Activities/IWfActivityControlsOptionService.cs](../src/Modules/Workflow/Activities/IWfActivityControlsOptionService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public interface IWfActivityControlsOptionService : IBaseService<WfActivityControlsOption>`

### [Activities/IWfActivityControlsValidationService.cs](../src/Modules/Workflow/Activities/IWfActivityControlsValidationService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public interface IWfActivityControlsValidationService : IBaseService<WfActivityControlsValidation>`

### [Activities/IWfActivityMappingVariableService.cs](../src/Modules/Workflow/Activities/IWfActivityMappingVariableService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public interface IWfActivityMappingVariableService : IBaseService<WfActivityMappingVariable>`

### [Activities/IWfActivityNotificationDispatcher.cs](../src/Modules/Workflow/Activities/IWfActivityNotificationDispatcher.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public interface IWfActivityNotificationDispatcher`

### [Activities/IWfActivityService.cs](../src/Modules/Workflow/Activities/IWfActivityService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public interface IWfActivityService : IBaseService<WfActivity>`

### [Activities/IWfActivityTypeService.cs](../src/Modules/Workflow/Activities/IWfActivityTypeService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public interface IWfActivityTypeService : IBaseService<WfActivityType>`

### [Activities/WfActivity.cs](../src/Modules/Workflow/Activities/WfActivity.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivity : WfMasterEntity<long>`

### [Activities/WfActivityAlertDispatchedEvent.cs](../src/Modules/Workflow/Activities/WfActivityAlertDispatchedEvent.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public sealed class WfActivityAlertDispatchedEvent : ISysEvent`

### [Activities/WfActivityAlertDispatchedEventHandler.cs](../src/Modules/Workflow/Activities/WfActivityAlertDispatchedEventHandler.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityAlertDispatchedEventHandler : ISysEventHandler<WfActivityAlertDispatchedEvent>`

### [Activities/WfActivityConfiguration.cs](../src/Modules/Workflow/Activities/WfActivityConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityConfiguration : IEntityTypeConfiguration<WfActivity>`

### [Activities/WfActivityControl.cs](../src/Modules/Workflow/Activities/WfActivityControl.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControl : WfMasterEntity<long>`

### [Activities/WfActivityControlConfiguration.cs](../src/Modules/Workflow/Activities/WfActivityControlConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControlConfiguration : IEntityTypeConfiguration<WfActivityControl>`

### [Activities/WfActivityControlController.cs](../src/Modules/Workflow/Activities/WfActivityControlController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControlController : BaseController<WfActivityControl, WfActivityControlDto>`

### [Activities/WfActivityControlDto.cs](../src/Modules/Workflow/Activities/WfActivityControlDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControlDto : WfMasterEntityDto<long>`

### [Activities/WfActivityController.cs](../src/Modules/Workflow/Activities/WfActivityController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityController : BaseController<WfActivity, WfActivityDto>`

### [Activities/WfActivityControlService.cs](../src/Modules/Workflow/Activities/WfActivityControlService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControlService : BaseService<WfActivityControl>, IWfActivityControlService`

### [Activities/WfActivityControlsOption.cs](../src/Modules/Workflow/Activities/WfActivityControlsOption.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControlsOption : Entity<long>`

### [Activities/WfActivityControlsOptionConfiguration.cs](../src/Modules/Workflow/Activities/WfActivityControlsOptionConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControlsOptionConfiguration : IEntityTypeConfiguration<WfActivityControlsOption>`

### [Activities/WfActivityControlsOptionController.cs](../src/Modules/Workflow/Activities/WfActivityControlsOptionController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControlsOptionController : BaseController<WfActivityControlsOption, WfActivityControlsOptionDto>`

### [Activities/WfActivityControlsOptionDto.cs](../src/Modules/Workflow/Activities/WfActivityControlsOptionDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControlsOptionDto : EntityDto<long>`

### [Activities/WfActivityControlsOptionService.cs](../src/Modules/Workflow/Activities/WfActivityControlsOptionService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControlsOptionService : BaseService<WfActivityControlsOption>, IWfActivityControlsOptionService`

### [Activities/WfActivityControlsValidation.cs](../src/Modules/Workflow/Activities/WfActivityControlsValidation.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControlsValidation : LookupEntity<long>`

### [Activities/WfActivityControlsValidationConfiguration.cs](../src/Modules/Workflow/Activities/WfActivityControlsValidationConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControlsValidationConfiguration : IEntityTypeConfiguration<WfActivityControlsValidation>`

### [Activities/WfActivityControlsValidationController.cs](../src/Modules/Workflow/Activities/WfActivityControlsValidationController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControlsValidationController : BaseController<WfActivityControlsValidation, WfActivityControlsValidationDto>`

### [Activities/WfActivityControlsValidationDto.cs](../src/Modules/Workflow/Activities/WfActivityControlsValidationDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControlsValidationDto : EntityDto<long>`

### [Activities/WfActivityControlsValidationDtoValidator.cs](../src/Modules/Workflow/Activities/WfActivityControlsValidationDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControlsValidationDtoValidator : BaseValidator<WfActivityControlsValidationDto>`

### [Activities/WfActivityControlsValidationService.cs](../src/Modules/Workflow/Activities/WfActivityControlsValidationService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityControlsValidationService : BaseService<WfActivityControlsValidation>, IWfActivityControlsValidationService`

### [Activities/WfActivityDetail.cs](../src/Modules/Workflow/Activities/WfActivityDetail.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityDetail : Entity<long>`

### [Activities/WfActivityDetailConfiguration.cs](../src/Modules/Workflow/Activities/WfActivityDetailConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public sealed class WfActivityDetailConfiguration : IEntityTypeConfiguration<WfActivityDetail>`

### [Activities/WfActivityDto.cs](../src/Modules/Workflow/Activities/WfActivityDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityDto : WfMasterEntityDto<long>`

### [Activities/WfActivityDtoValidator.cs](../src/Modules/Workflow/Activities/WfActivityDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityDtoValidator : BaseValidator<WfActivityDto>`

### [Activities/WfActivityMapping.cs](../src/Modules/Workflow/Activities/WfActivityMapping.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityMapping : IRegister`

### [Activities/WfActivityMappingVariable.cs](../src/Modules/Workflow/Activities/WfActivityMappingVariable.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityMappingVariable : Entity<long>`

### [Activities/WfActivityMappingVariableConfiguration.cs](../src/Modules/Workflow/Activities/WfActivityMappingVariableConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityMappingVariableConfiguration : IEntityTypeConfiguration<WfActivityMappingVariable>`

### [Activities/WfActivityMappingVariableController.cs](../src/Modules/Workflow/Activities/WfActivityMappingVariableController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityMappingVariableController : BaseController<WfActivityMappingVariable, WfActivityMappingVariableDto>`

### [Activities/WfActivityMappingVariableDto.cs](../src/Modules/Workflow/Activities/WfActivityMappingVariableDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityMappingVariableDto : BaseEntityDto<long>`

### [Activities/WfActivityMappingVariableService.cs](../src/Modules/Workflow/Activities/WfActivityMappingVariableService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityMappingVariableService : BaseService<WfActivityMappingVariable>, IWfActivityMappingVariableService`

### [Activities/WfActivityNotificationDispatcher.cs](../src/Modules/Workflow/Activities/WfActivityNotificationDispatcher.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityNotificationDispatcher : IWfActivityNotificationDispatcher`

### [Activities/WfActivityService.cs](../src/Modules/Workflow/Activities/WfActivityService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityService : BaseService<WfActivity>, IWfActivityService`

### [Activities/WfActivityType.cs](../src/Modules/Workflow/Activities/WfActivityType.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityType : WfMasterEntity<byte>`

### [Activities/WfActivityTypeConfiguration.cs](../src/Modules/Workflow/Activities/WfActivityTypeConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityTypeConfiguration : IEntityTypeConfiguration<WfActivityType>`

### [Activities/WfActivityTypeController.cs](../src/Modules/Workflow/Activities/WfActivityTypeController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityTypeController : BaseController<WfActivityType, WfActivityTypeDto>`

### [Activities/WfActivityTypeDto.cs](../src/Modules/Workflow/Activities/WfActivityTypeDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityTypeDto : WfMasterEntityDto<byte>`

### [Activities/WfActivityTypeDtoValidator.cs](../src/Modules/Workflow/Activities/WfActivityTypeDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityTypeDtoValidator : BaseValidator<WfActivityTypeDto>`

### [Activities/WfActivityTypeService.cs](../src/Modules/Workflow/Activities/WfActivityTypeService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Activities`.

- `public class WfActivityTypeService : BaseService<WfActivityType>, IWfActivityTypeService`

## Categories

### [Categories/IWfCategoryService.cs](../src/Modules/Workflow/Categories/IWfCategoryService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Categories`.

- `public interface IWfCategoryService : IBaseService<WfCategory>`

### [Categories/WfCategory.cs](../src/Modules/Workflow/Categories/WfCategory.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Categories`.

- `public class WfCategory : WfMasterEntity<short>`

### [Categories/WfCategoryConfiguration.cs](../src/Modules/Workflow/Categories/WfCategoryConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Categories`.

- `public class WfCategoryConfiguration : IEntityTypeConfiguration<WfCategory>`

### [Categories/WfCategoryController.cs](../src/Modules/Workflow/Categories/WfCategoryController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Categories`.

- `public class WfCategoryController : BaseController<WfCategory, WfCategoryDto>`

### [Categories/WfCategoryDto.cs](../src/Modules/Workflow/Categories/WfCategoryDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Categories`.

- `public class WfCategoryDto : WfMasterEntityDto<short>`

### [Categories/WfCategoryDtoValidator.cs](../src/Modules/Workflow/Categories/WfCategoryDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Categories`.

- `public class WfCategoryDtoValidator : BaseValidator<WfCategoryDto>`

### [Categories/WfCategoryService.cs](../src/Modules/Workflow/Categories/WfCategoryService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Categories`.

- `public class WfCategoryService : BaseService<WfCategory>, IWfCategoryService`

## Controls

### [Controls/IWfControlService.cs](../src/Modules/Workflow/Controls/IWfControlService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Controls`.

- `public interface IWfControlService : IBaseService<WfControl>`

### [Controls/WfControl.cs](../src/Modules/Workflow/Controls/WfControl.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Controls`.

- `public class WfControl : WfMasterEntity<byte>`

### [Controls/WfControlConfiguration.cs](../src/Modules/Workflow/Controls/WfControlConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Controls`.

- `public class WfControlConfiguration : IEntityTypeConfiguration<WfControl>`

### [Controls/WfControlController.cs](../src/Modules/Workflow/Controls/WfControlController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Controls`.

- `public class WfControlController : BaseController<WfControl, WfControlDto>`

### [Controls/WfControlDto.cs](../src/Modules/Workflow/Controls/WfControlDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Controls`.

- `public class WfControlDto : WfMasterEntityDto<byte>`

### [Controls/WfControlDtoValidator.cs](../src/Modules/Workflow/Controls/WfControlDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Controls`.

- `public class WfControlDtoValidator : BaseValidator<WfControlDto>`

### [Controls/WfControlService.cs](../src/Modules/Workflow/Controls/WfControlService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Controls`.

- `public class WfControlService : BaseService<WfControl>, IWfControlService`

## DataExchange

### [DataExchange/IWfExcelImportService.cs](../src/Modules/Workflow/DataExchange/IWfExcelImportService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.DataExchange`.

- `public interface IWfExcelImportService`

### [DataExchange/WfDataManagementController.cs](../src/Modules/Workflow/DataExchange/WfDataManagementController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.DataExchange`.

- `public class WfDataManagementController : ControllerBase`

### [DataExchange/WfExcelImportService.cs](../src/Modules/Workflow/DataExchange/WfExcelImportService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.DataExchange`.

- `public class WfExcelImportService : IWfExcelImportService`

## Execution

### [Execution/WfActivityAutoPassJobHandler.cs](../src/Modules/Workflow/Execution/WfActivityAutoPassJobHandler.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Execution`.

- `public class WfActivityAutoPassJobHandler : ISysBackgroundJobHandler`

### [Execution/WfAssignment.cs](../src/Modules/Workflow/Execution/WfAssignment.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Execution`.

- `public class WfAssignment : Entity<long>`

### [Execution/WfAssignmentAutoPassedEvent.cs](../src/Modules/Workflow/Execution/WfAssignmentAutoPassedEvent.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Execution`.

- `public sealed class WfAssignmentAutoPassedEvent : ISysEvent`

### [Execution/WfAssignmentAutoPassedNotificationHandler.cs](../src/Modules/Workflow/Execution/WfAssignmentAutoPassedNotificationHandler.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Execution`.

- `public class WfAssignmentAutoPassedNotificationHandler : ISysEventHandler<WfAssignmentAutoPassedEvent>`

### [Execution/WfAssignmentConfiguration.cs](../src/Modules/Workflow/Execution/WfAssignmentConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Execution`.

- `public class WfAssignmentConfiguration : IEntityTypeConfiguration<WfAssignment>`

### [Execution/WfTransfer.cs](../src/Modules/Workflow/Execution/WfTransfer.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Execution`.

- `public class WfTransfer : Entity<long>`

### [Execution/WfTransferDetails.cs](../src/Modules/Workflow/Execution/WfTransferDetails.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Execution`.

- `public class WfTransferDetails : Entity<long>`

## Module root

### [GlobalUsings.cs](../src/Modules/Workflow/GlobalUsings.cs)

## Operators

### [Operators/IWfOperatorService.cs](../src/Modules/Workflow/Operators/IWfOperatorService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Operators`.

- `public interface IWfOperatorService : IBaseService<WfOperator>`

### [Operators/WfOperator.cs](../src/Modules/Workflow/Operators/WfOperator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Operators`.

- `public class WfOperator : WfMasterEntity<byte>`

### [Operators/WfOperatorConfiguration.cs](../src/Modules/Workflow/Operators/WfOperatorConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Operators`.

- `public class WfOperatorConfiguration : IEntityTypeConfiguration<WfOperator>`

### [Operators/WfOperatorController.cs](../src/Modules/Workflow/Operators/WfOperatorController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Operators`.

- `public class WfOperatorController : BaseController<WfOperator, WfOperatorDto>`

### [Operators/WfOperatorDto.cs](../src/Modules/Workflow/Operators/WfOperatorDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Operators`.

- `public class WfOperatorDto : WfMasterEntityDto<byte>`

### [Operators/WfOperatorDtoValidator.cs](../src/Modules/Workflow/Operators/WfOperatorDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Operators`.

- `public class WfOperatorDtoValidator : BaseValidator<WfOperatorDto>`

### [Operators/WfOperatorService.cs](../src/Modules/Workflow/Operators/WfOperatorService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Operators`.

- `public class WfOperatorService : BaseService<WfOperator>, IWfOperatorService`

## Performers

### [Performers/IWfPerformerService.cs](../src/Modules/Workflow/Performers/IWfPerformerService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public interface IWfPerformerService : IBaseService<WfPerformer>`

### [Performers/IWfPerformerTypeService.cs](../src/Modules/Workflow/Performers/IWfPerformerTypeService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public interface IWfPerformerTypeService : IBaseService<WfPerformerType>`

### [Performers/WfPerformer.cs](../src/Modules/Workflow/Performers/WfPerformer.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public class WfPerformer: LookupEntity<long>`

### [Performers/WfPerformerConfiguration.cs](../src/Modules/Workflow/Performers/WfPerformerConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public class WfPerformerConfiguration : IEntityTypeConfiguration<WfPerformer>`

### [Performers/WfPerformerController.cs](../src/Modules/Workflow/Performers/WfPerformerController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public class WfPerformerController : BaseController<WfPerformer, WfPerformerDto>`

### [Performers/WfPerformerDto.cs](../src/Modules/Workflow/Performers/WfPerformerDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public class WfPerformerDto : MasterEntityDto<long>`

### [Performers/WfPerformerDtoValidator.cs](../src/Modules/Workflow/Performers/WfPerformerDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public class WfPerformerDtoValidator : BaseValidator<WfPerformerDto>`

### [Performers/WfPerformerService.cs](../src/Modules/Workflow/Performers/WfPerformerService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public class WfPerformerService : BaseService<WfPerformer>, IWfPerformerService`

### [Performers/WfPerformerType.cs](../src/Modules/Workflow/Performers/WfPerformerType.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public class WfPerformerType : LookupEntity<short>`

### [Performers/WfPerformerTypeConfiguration.cs](../src/Modules/Workflow/Performers/WfPerformerTypeConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public class WfPerformerTypeConfiguration : IEntityTypeConfiguration<WfPerformerType>`

### [Performers/WfPerformerTypeController.cs](../src/Modules/Workflow/Performers/WfPerformerTypeController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public class WfPerformerTypeController : BaseController<WfPerformerType, WfPerformerTypeDto>`

### [Performers/WfPerformerTypeDto.cs](../src/Modules/Workflow/Performers/WfPerformerTypeDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public class WfPerformerTypeDto : EntityDto<short>`

### [Performers/WfPerformerTypeDtoValidator.cs](../src/Modules/Workflow/Performers/WfPerformerTypeDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public class WfPerformerTypeDtoValidator : BaseValidator<WfPerformerTypeDto>`

### [Performers/WfPerformerTypeService.cs](../src/Modules/Workflow/Performers/WfPerformerTypeService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public class WfPerformerTypeService : BaseService<WfPerformerType>, IWfPerformerTypeService`

### [Performers/WfPerformerUsers.cs](../src/Modules/Workflow/Performers/WfPerformerUsers.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public class WfPerformerUsers : Entity<long>`

### [Performers/WfPerformerUsersConfiguration.cs](../src/Modules/Workflow/Performers/WfPerformerUsersConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Performers`.

- `public class WfPerformerUsersConfiguration : IEntityTypeConfiguration<WfPerformerUsers>`

## Persistence

### [Persistence/IWorkflowDataContext.cs](../src/Modules/Workflow/Persistence/IWorkflowDataContext.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Persistence`.

- `public interface IWorkflowDataContext`

## PrintTemplates

### [PrintTemplates/IPrintTemplateService.cs](../src/Modules/Workflow/PrintTemplates/IPrintTemplateService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.PrintTemplates`.

- `public interface IPrintTemplateService`

### [PrintTemplates/IReportResourceAuthorizer.cs](../src/Modules/Workflow/PrintTemplates/IReportResourceAuthorizer.cs)

Namespace: `IAX.IXApi.Modules.Workflow.PrintTemplates`.

- `public interface IReportResourceAuthorizer`

### [PrintTemplates/PrintTemplateController.cs](../src/Modules/Workflow/PrintTemplates/PrintTemplateController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.PrintTemplates`.

- `public sealed class PrintTemplateController : ControllerBase`

### [PrintTemplates/PrintTemplateDocument.cs](../src/Modules/Workflow/PrintTemplates/PrintTemplateDocument.cs)

Namespace: `IAX.IXApi.Modules.Workflow.PrintTemplates`.

- `public sealed class PrintTemplateDocument`

- `public sealed class PrintTemplatePage`

- `public sealed class PrintTemplateMargins`

- `public abstract class PrintTemplateElement`

- `public sealed class PrintTextElement : PrintTemplateElement`

- `public sealed class PrintFieldElement : PrintTemplateElement`

- `public sealed class PrintSectionElement : PrintTemplateElement`

- `public sealed class PrintRowElement : PrintTemplateElement`

- `public sealed class PrintColumnElement : PrintTemplateElement`

- `public sealed class PrintDividerElement : PrintTemplateElement;`

- `public sealed class PrintImageElement : PrintTemplateElement`

- `public sealed class PrintTableElement : PrintTemplateElement`

- `public sealed class PrintTableColumn`

- `public sealed class PrintWorkflowApprovalElement : PrintTemplateElement`

- `public sealed class PrintSignatureElement : PrintTemplateElement`

- `public sealed class PrintQrCodeElement : PrintTemplateElement`

- `public sealed class PrintBarcodeElement : PrintTemplateElement`

- `public sealed class PrintAttachmentElement : PrintTemplateElement`

- `public sealed class PrintPageNumberElement : PrintTemplateElement;`

- `public sealed class PrintDateElement : PrintTemplateElement;`

- `public sealed class PrintSpacerElement : PrintTemplateElement`

- `public sealed class PrintPageBreakElement : PrintTemplateElement;`

- `public sealed class PrintFieldBinding`

- `public sealed class PrintVisibilityCondition`

- `public sealed class PrintValueFormat`

- `public sealed class PrintElementStyle`

### [PrintTemplates/PrintTemplateDocumentValidator.cs](../src/Modules/Workflow/PrintTemplates/PrintTemplateDocumentValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.PrintTemplates`.

- `public sealed class PrintTemplateDocumentValidator`

### [PrintTemplates/PrintTemplateDtos.cs](../src/Modules/Workflow/PrintTemplates/PrintTemplateDtos.cs)

Namespace: `IAX.IXApi.Modules.Workflow.PrintTemplates`.

- `public class PrintTemplateSummaryDto`

- `public sealed class PrintTemplateDto : PrintTemplateSummaryDto`

- `public sealed class PublishedPrintTemplateDto : PrintTemplateSummaryDto`

- `public sealed class PrintTemplateVersionDto`

- `public sealed class CreatePrintTemplateDto`

- `public sealed class UpdatePrintTemplateDto`

- `public sealed class PublishPrintTemplateDto`

- `public sealed class PrintTemplateValidationResultDto`

- `public sealed class PrintTemplateValidationException(IEnumerable<string> errors)`

### [PrintTemplates/PrintTemplateDtoValidators.cs](../src/Modules/Workflow/PrintTemplates/PrintTemplateDtoValidators.cs)

Namespace: `IAX.IXApi.Modules.Workflow.PrintTemplates`.

- `public sealed class CreatePrintTemplateDtoValidator : AbstractValidator<CreatePrintTemplateDto>`

- `public sealed class UpdatePrintTemplateDtoValidator : AbstractValidator<UpdatePrintTemplateDto>`

### [PrintTemplates/PrintTemplateService.cs](../src/Modules/Workflow/PrintTemplates/PrintTemplateService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.PrintTemplates`.

- `public sealed class PrintTemplateService : IPrintTemplateService`

### [PrintTemplates/WorkflowReportResourceAuthorizer.cs](../src/Modules/Workflow/PrintTemplates/WorkflowReportResourceAuthorizer.cs)

Namespace: `IAX.IXApi.Modules.Workflow.PrintTemplates`.

- `public sealed class WorkflowReportResourceAuthorizer(`

## Priorities

### [Priorities/IWfPriorityService.cs](../src/Modules/Workflow/Priorities/IWfPriorityService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Priorities`.

- `public interface IWfPriorityService : IBaseService<WfPriority>`

### [Priorities/WfPriority.cs](../src/Modules/Workflow/Priorities/WfPriority.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Priorities`.

- `public class WfPriority : WfMasterEntity<byte>`

### [Priorities/WfPriorityConfiguration.cs](../src/Modules/Workflow/Priorities/WfPriorityConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Priorities`.

- `public class WfPriorityConfiguration : IEntityTypeConfiguration<WfPriority>`

### [Priorities/WfPriorityController.cs](../src/Modules/Workflow/Priorities/WfPriorityController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Priorities`.

- `public class WfPriorityController : BaseController<WfPriority, WfPriorityDto>`

### [Priorities/WfPriorityDto.cs](../src/Modules/Workflow/Priorities/WfPriorityDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Priorities`.

- `public class WfPriorityDto : WfMasterEntityDto<byte>`

### [Priorities/WfPriorityDtoValidator.cs](../src/Modules/Workflow/Priorities/WfPriorityDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Priorities`.

- `public class WfPriorityDtoValidator : BaseValidator<WfPriorityDto>`

### [Priorities/WfPriorityService.cs](../src/Modules/Workflow/Priorities/WfPriorityService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Priorities`.

- `public class WfPriorityService : BaseService<WfPriority>, IWfPriorityService`

## Processes

### [Processes/IWfProcessService.cs](../src/Modules/Workflow/Processes/IWfProcessService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public interface IWfProcessService : IBaseService<WfProcess>`

### [Processes/WfProcess.cs](../src/Modules/Workflow/Processes/WfProcess.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public partial class WfProcess : WfMasterEntity<long>`

### [Processes/WfProcessConfiguration.cs](../src/Modules/Workflow/Processes/WfProcessConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public class WfProcessConfiguration : IEntityTypeConfiguration<WfProcess>`

### [Processes/WfProcessController.cs](../src/Modules/Workflow/Processes/WfProcessController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public class WfProcessController : BaseController<WfProcess, WfProcessDto>`

### [Processes/WfProcessData.cs](../src/Modules/Workflow/Processes/WfProcessData.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public class WfProcessData:Entity<long>`

### [Processes/WfProcessDataConfiguration.cs](../src/Modules/Workflow/Processes/WfProcessDataConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public class WfProcessDataConfiguration : IEntityTypeConfiguration<WfProcessData>`

### [Processes/WfProcessDto.cs](../src/Modules/Workflow/Processes/WfProcessDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public class WfProcessDto : WfMasterEntityDto<long>`

### [Processes/WfProcessDtoValidator.cs](../src/Modules/Workflow/Processes/WfProcessDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public class WfProcessDtoValidator : BaseValidator<WfProcessDto>`

### [Processes/WfProcessMapping.cs](../src/Modules/Workflow/Processes/WfProcessMapping.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public class WfProcessMapping : IRegister`

### [Processes/WfProcessPermissions.cs](../src/Modules/Workflow/Processes/WfProcessPermissions.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public class WfProcessPermissions : Entity<long>`

### [Processes/WfProcessService.cs](../src/Modules/Workflow/Processes/WfProcessService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public class WfProcessService : BaseService<WfProcess>, IWfProcessService`

### [Processes/WfProcessVariable.cs](../src/Modules/Workflow/Processes/WfProcessVariable.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public class WfProcessVariable: Entity<long>`

### [Processes/WfProcessVariableConfiguration.cs](../src/Modules/Workflow/Processes/WfProcessVariableConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public class WfProcessVariableConfiguration : IEntityTypeConfiguration<WfProcessVariable>`

### [Processes/WfUsersProcess.cs](../src/Modules/Workflow/Processes/WfUsersProcess.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public class WfUsersProcess : Entity<long>`

### [Processes/WfUsersProcessConfiguration.cs](../src/Modules/Workflow/Processes/WfUsersProcessConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public class WfUsersProcessConfiguration : IEntityTypeConfiguration<WfUsersProcess>`

### [Processes/WfUsersProcessDto.cs](../src/Modules/Workflow/Processes/WfUsersProcessDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Processes`.

- `public class WfUsersProcessDto : BaseEntityDto<long>`

## ProcessTypes

### [ProcessTypes/IWfProcessTypeService.cs](../src/Modules/Workflow/ProcessTypes/IWfProcessTypeService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.ProcessTypes`.

- `public interface IWfProcessTypeService : IBaseService<WfProcessType>`

### [ProcessTypes/WfProcessType.cs](../src/Modules/Workflow/ProcessTypes/WfProcessType.cs)

Namespace: `IAX.IXApi.Modules.Workflow.ProcessTypes`.

- `public class WfProcessType : WfMasterEntity<byte>`

### [ProcessTypes/WfProcessTypeConfiguration.cs](../src/Modules/Workflow/ProcessTypes/WfProcessTypeConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.ProcessTypes`.

- `public class WfProcessTypeConfiguration : IEntityTypeConfiguration<WfProcessType>`

### [ProcessTypes/WfProcessTypeController.cs](../src/Modules/Workflow/ProcessTypes/WfProcessTypeController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.ProcessTypes`.

- `public class WfProcessTypeController : BaseController<WfProcessType, WfProcessTypeDto>`

### [ProcessTypes/WfProcessTypeDto.cs](../src/Modules/Workflow/ProcessTypes/WfProcessTypeDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.ProcessTypes`.

- `public class WfProcessTypeDto : WfMasterEntityDto<byte>`

### [ProcessTypes/WfProcessTypeDtoValidator.cs](../src/Modules/Workflow/ProcessTypes/WfProcessTypeDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.ProcessTypes`.

- `public class WfProcessTypeDtoValidator : BaseValidator<WfProcessTypeDto>`

### [ProcessTypes/WfProcessTypeService.cs](../src/Modules/Workflow/ProcessTypes/WfProcessTypeService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.ProcessTypes`.

- `public class WfProcessTypeService : BaseService<WfProcessType>, IWfProcessTypeService`

## Requests

### [Requests/DynamicRequestFormDtos.cs](../src/Modules/Workflow/Requests/DynamicRequestFormDtos.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public sealed class DynamicRequestFormDto`

- `public sealed class DynamicRequestControlDto`

- `public sealed class DynamicRequestOptionDto`

- `public sealed class DynamicRequestOptionFeatureDto`

- `public sealed class DynamicRequestValidationDto`

- `public sealed class DynamicRequestConditionDto`

- `public sealed class SubmitDynamicRequestDto`

- `public sealed class DynamicRequestValueDto`

- `public sealed class DynamicRequestOptionFeatureValueDto`

- `public sealed class SubmitDynamicRequestResultDto`

- `public sealed class DynamicRequestAttachmentOwnerDto`

- `public sealed class DynamicRequestValidationException(List<ValidationResult> errors)`

### [Requests/IValidationEngine.cs](../src/Modules/Workflow/Requests/IValidationEngine.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public interface IValidationEngine`

### [Requests/IWfRequestControlService.cs](../src/Modules/Workflow/Requests/IWfRequestControlService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public interface IWfRequestControlService : IBaseService<WfRequestControl>`

### [Requests/IWfRequestControlsOptionService.cs](../src/Modules/Workflow/Requests/IWfRequestControlsOptionService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public interface IWfRequestControlsOptionService : IBaseService<WfRequestControlsOption>`

### [Requests/IWfRequestControlsValidationService.cs](../src/Modules/Workflow/Requests/IWfRequestControlsValidationService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public interface IWfRequestControlsValidationService : IBaseService<WfRequestControlsValidation>`

### [Requests/IWfRequestMappingVariableService.cs](../src/Modules/Workflow/Requests/IWfRequestMappingVariableService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public interface IWfRequestMappingVariableService : IBaseService<WfRequestMappingVariable>`

### [Requests/IWfRequestService.cs](../src/Modules/Workflow/Requests/IWfRequestService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public interface IWfRequestService : IBaseService<WfRequest>`

### [Requests/MailRequestDetailsDtos.cs](../src/Modules/Workflow/Requests/MailRequestDetailsDtos.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public sealed class MailRequestDetailsDto`

- `public sealed class MailRequestFieldDto`

- `public sealed class MailTrackingEntryDto`

### [Requests/ValidateRequestParams.cs](../src/Modules/Workflow/Requests/ValidateRequestParams.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class ValidateRequestParams`

### [Requests/ValidationEngine.cs](../src/Modules/Workflow/Requests/ValidationEngine.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class ValidationEngine : IValidationEngine`

### [Requests/ValidationResult.cs](../src/Modules/Workflow/Requests/ValidationResult.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class ValidationResult`

### [Requests/WfRequest.cs](../src/Modules/Workflow/Requests/WfRequest.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequest : LookupEntity<long>`

### [Requests/WfRequestConfiguration.cs](../src/Modules/Workflow/Requests/WfRequestConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestConfiguration : IEntityTypeConfiguration<WfRequest>`

### [Requests/WfRequestControl.cs](../src/Modules/Workflow/Requests/WfRequestControl.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControl: WfMasterEntity<long>`

### [Requests/WfRequestControlConfiguration.cs](../src/Modules/Workflow/Requests/WfRequestControlConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlConfiguration : IEntityTypeConfiguration<WfRequestControl>`

### [Requests/WfRequestControlController.cs](../src/Modules/Workflow/Requests/WfRequestControlController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlController : BaseController<WfRequestControl, WfRequestControlDto>`

### [Requests/WfRequestControlDto.cs](../src/Modules/Workflow/Requests/WfRequestControlDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlDto : WfMasterEntityDto<long>`

### [Requests/WfRequestControlDtoValidator.cs](../src/Modules/Workflow/Requests/WfRequestControlDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlDtoValidator : BaseValidator<WfRequestControlDto>`

- `internal static class ReportingMetadata`

### [Requests/WfRequestController.cs](../src/Modules/Workflow/Requests/WfRequestController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestController : BaseController<WfRequest, WfRequestDto>`

### [Requests/WfRequestControlService.cs](../src/Modules/Workflow/Requests/WfRequestControlService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlService : BaseService<WfRequestControl>, IWfRequestControlService`

### [Requests/WfRequestControlsOption.cs](../src/Modules/Workflow/Requests/WfRequestControlsOption.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlsOption : Entity<long>`

### [Requests/WfRequestControlsOptionConfiguration.cs](../src/Modules/Workflow/Requests/WfRequestControlsOptionConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlsOptionConfiguration : IEntityTypeConfiguration<WfRequestControlsOption>`

### [Requests/WfRequestControlsOptionController.cs](../src/Modules/Workflow/Requests/WfRequestControlsOptionController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlsOptionController : BaseController<WfRequestControlsOption, WfRequestControlsOptionDto>`

### [Requests/WfRequestControlsOptionDto.cs](../src/Modules/Workflow/Requests/WfRequestControlsOptionDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlsOptionDto : EntityDto<long>`

### [Requests/WfRequestControlsOptionService.cs](../src/Modules/Workflow/Requests/WfRequestControlsOptionService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlsOptionService : BaseService<WfRequestControlsOption>, IWfRequestControlsOptionService`

### [Requests/WfRequestControlsValidation.cs](../src/Modules/Workflow/Requests/WfRequestControlsValidation.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlsValidation : Entity<long>`

### [Requests/WfRequestControlsValidationConfiguration.cs](../src/Modules/Workflow/Requests/WfRequestControlsValidationConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlsValidationConfiguration : IEntityTypeConfiguration<WfRequestControlsValidation>`

### [Requests/WfRequestControlsValidationController.cs](../src/Modules/Workflow/Requests/WfRequestControlsValidationController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlsValidationController : BaseController<WfRequestControlsValidation, WfRequestControlsValidationDto>`

### [Requests/WfRequestControlsValidationDto.cs](../src/Modules/Workflow/Requests/WfRequestControlsValidationDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlsValidationDto : EntityDto<long>`

### [Requests/WfRequestControlsValidationDtoValidator.cs](../src/Modules/Workflow/Requests/WfRequestControlsValidationDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlsValidationDtoValidator : BaseValidator<WfRequestControlsValidationDto>`

### [Requests/WfRequestControlsValidationService.cs](../src/Modules/Workflow/Requests/WfRequestControlsValidationService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestControlsValidationService : BaseService<WfRequestControlsValidation>, IWfRequestControlsValidationService`

### [Requests/WfRequestDetail.cs](../src/Modules/Workflow/Requests/WfRequestDetail.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestDetail : Entity<long>`

### [Requests/WfRequestDto.cs](../src/Modules/Workflow/Requests/WfRequestDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestDto : MasterEntityDto<long>`

### [Requests/WfRequestDtoValidator.cs](../src/Modules/Workflow/Requests/WfRequestDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestDtoValidator : BaseValidator<WfRequestDto>`

### [Requests/WfRequestMappingVariable.cs](../src/Modules/Workflow/Requests/WfRequestMappingVariable.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestMappingVariable : Entity<long>`

### [Requests/WfRequestMappingVariableConfiguration.cs](../src/Modules/Workflow/Requests/WfRequestMappingVariableConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestMappingVariableConfiguration : IEntityTypeConfiguration<WfRequestMappingVariable>`

### [Requests/WfRequestMappingVariableController.cs](../src/Modules/Workflow/Requests/WfRequestMappingVariableController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestMappingVariableController : BaseController<WfRequestMappingVariable, WfRequestMappingVariableDto>`

### [Requests/WfRequestMappingVariableDto.cs](../src/Modules/Workflow/Requests/WfRequestMappingVariableDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestMappingVariableDto : BaseEntityDto<long>`

### [Requests/WfRequestMappingVariableService.cs](../src/Modules/Workflow/Requests/WfRequestMappingVariableService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestMappingVariableService : BaseService<WfRequestMappingVariable>, IWfRequestMappingVariableService`

### [Requests/WfRequestService.Authorization.cs](../src/Modules/Workflow/Requests/WfRequestService.Authorization.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public partial class WfRequestService`

### [Requests/WfRequestService.cs](../src/Modules/Workflow/Requests/WfRequestService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public partial class WfRequestService : BaseService<WfRequest>, IWfRequestService`

### [Requests/WfRequestTransition.cs](../src/Modules/Workflow/Requests/WfRequestTransition.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestTransition : Entity<long>`

### [Requests/WfRequestVariable.cs](../src/Modules/Workflow/Requests/WfRequestVariable.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestVariable:Entity<long>`

### [Requests/WfRequestVariableConfiguration.cs](../src/Modules/Workflow/Requests/WfRequestVariableConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Requests`.

- `public class WfRequestVariableConfiguration : IEntityTypeConfiguration<WfRequestVariable>`

## Steps

### [Steps/IWfStepService.cs](../src/Modules/Workflow/Steps/IWfStepService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Steps`.

- `public interface IWfStepService : IBaseService<WfStep>`

### [Steps/WfStep.cs](../src/Modules/Workflow/Steps/WfStep.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Steps`.

- `public class WfStep : WfMasterEntity<long>`

### [Steps/WfStepConfiguration.cs](../src/Modules/Workflow/Steps/WfStepConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Steps`.

- `public class WfStepConfiguration : IEntityTypeConfiguration<WfStep>`

### [Steps/WfStepController.cs](../src/Modules/Workflow/Steps/WfStepController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Steps`.

- `public class WfStepController : BaseController<WfStep, WfStepDto>`

### [Steps/WfStepDto.cs](../src/Modules/Workflow/Steps/WfStepDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Steps`.

- `public class WfStepDto : WfMasterEntityDto<long>`

### [Steps/WfStepDtoValidator.cs](../src/Modules/Workflow/Steps/WfStepDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Steps`.

- `public class WfStepDtoValidator : BaseValidator<WfStepDto>`

### [Steps/WfStepService.cs](../src/Modules/Workflow/Steps/WfStepService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Steps`.

- `public class WfStepService : BaseService<WfStep>, IWfStepService`

## Transitions

### [Transitions/IWfTransitionService.cs](../src/Modules/Workflow/Transitions/IWfTransitionService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Transitions`.

- `public interface IWfTransitionService : IBaseService<WfTransition>`

### [Transitions/WfTransition.cs](../src/Modules/Workflow/Transitions/WfTransition.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Transitions`.

- `public class WfTransition : Entity<long>`

### [Transitions/WfTransitionConfiguration.cs](../src/Modules/Workflow/Transitions/WfTransitionConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Transitions`.

- `public class WfTransitionConfiguration : IEntityTypeConfiguration<WfTransition>`

### [Transitions/WfTransitionController.cs](../src/Modules/Workflow/Transitions/WfTransitionController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Transitions`.

- `public class WfTransitionController : BaseController<WfTransition, WfTransitionDto>`

### [Transitions/WfTransitionDto.cs](../src/Modules/Workflow/Transitions/WfTransitionDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Transitions`.

- `public class WfTransitionDto : EntityDto<long>`

### [Transitions/WfTransitionDtoValidator.cs](../src/Modules/Workflow/Transitions/WfTransitionDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Transitions`.

- `public class WfTransitionDtoValidator : BaseValidator<WfTransitionDto>`

### [Transitions/WfTransitionService.cs](../src/Modules/Workflow/Transitions/WfTransitionService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Transitions`.

- `public class WfTransitionService : BaseService<WfTransition>, IWfTransitionService`

### [Transitions/WfTransitionTrigger.cs](../src/Modules/Workflow/Transitions/WfTransitionTrigger.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Transitions`.

- `public class WfTransitionTrigger : Entity<long>`

## Variables

### [Variables/IWfDataTypeService.cs](../src/Modules/Workflow/Variables/IWfDataTypeService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Variables`.

- `public interface IWfDataTypeService : IBaseService<WfDataType>`

### [Variables/IWfVariableService.cs](../src/Modules/Workflow/Variables/IWfVariableService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Variables`.

- `public interface IWfVariableService : IBaseService<WfVariable>`

### [Variables/WfDataType.cs](../src/Modules/Workflow/Variables/WfDataType.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Variables`.

- `public class WfDataType : WfMasterEntity<byte>`

### [Variables/WfDataTypeConfiguration.cs](../src/Modules/Workflow/Variables/WfDataTypeConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Variables`.

- `public class WfDataTypeConfiguration : IEntityTypeConfiguration<WfDataType>`

### [Variables/WfDataTypeController.cs](../src/Modules/Workflow/Variables/WfDataTypeController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Variables`.

- `public class WfDataTypeController : BaseController<WfDataType, WfDataTypeDto>`

### [Variables/WfDataTypeDto.cs](../src/Modules/Workflow/Variables/WfDataTypeDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Variables`.

- `public class WfDataTypeDto : WfMasterEntityDto<byte>`

### [Variables/WfDataTypeService.cs](../src/Modules/Workflow/Variables/WfDataTypeService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Variables`.

- `public class WfDataTypeService : BaseService<WfDataType>, IWfDataTypeService`

### [Variables/WfVariable.cs](../src/Modules/Workflow/Variables/WfVariable.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Variables`.

- `public class WfVariable : WfMasterEntity<long>`

### [Variables/WfVariableConfiguration.cs](../src/Modules/Workflow/Variables/WfVariableConfiguration.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Variables`.

- `public class WfVariableConfiguration : IEntityTypeConfiguration<WfVariable>`

### [Variables/WfVariableController.cs](../src/Modules/Workflow/Variables/WfVariableController.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Variables`.

- `public class WfVariableController : BaseController<WfVariable, WfVariableDto>`

### [Variables/WfVariableDto.cs](../src/Modules/Workflow/Variables/WfVariableDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Variables`.

- `public class WfVariableDto : WfMasterEntityDto<long>`

### [Variables/WfVariableDtoValidator.cs](../src/Modules/Workflow/Variables/WfVariableDtoValidator.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Variables`.

- `public class WfVariableDtoValidator : BaseValidator<WfVariableDto>`

### [Variables/WfVariableService.cs](../src/Modules/Workflow/Variables/WfVariableService.cs)

Namespace: `IAX.IXApi.Modules.Workflow.Variables`.

- `public class WfVariableService : BaseService<WfVariable>, IWfVariableService`

## Module root

### [WfMasterEntity.cs](../src/Modules/Workflow/WfMasterEntity.cs)

Namespace: `IAX.IXApi.Modules.Workflow`.

- `public abstract class WfMasterEntity<T> : MasterEntity<T>`

### [WfMasterEntityDto.cs](../src/Modules/Workflow/WfMasterEntityDto.cs)

Namespace: `IAX.IXApi.Modules.Workflow`.

- `public class WfMasterEntityDto<T> : MasterEntityDto<T>`

### [WorkflowModule.cs](../src/Modules/Workflow/WorkflowModule.cs)

Namespace: `IAX.IXApi.Modules.Workflow`.

- `public static class WorkflowModule`
