# Communication backend architecture refactor

Scope: `IXApi/src/Modules/Communication`. The copied `GeneralLedger/` labels in the request were treated as examples; the Communication tree is the target.

## Shared architecture and baseline

Reviewed Shared `BaseController`, `IBaseService`, `BaseService`, `IUnitOfWork`, `UnitOfWork`, generic repositories, `APIResponse`, Mapster discovery, validation registration, authorization, company context, exception handling, logging, DI, and paging. Baseline Communication build passed with 0 warnings and 0 errors. The pre-existing AccountsReceivable working-tree edits were left untouched.

## Folder results

- **Chat:** Reviewed controller, request/DTOs, service contract and implementation, entities, and EF configuration. Changed `SysChatService` to use the Shared unit of work and Mapster for entity-to-message DTO mapping. Room access, SQL MERGE, save-before-broadcast order, and hub behavior remain in place. Folder build passed.
- **Notifications/Configuration and Entities:** Reviewed all mappings and entities. Added Mapster configuration for notification and recipient-state DTO mapping; EF mappings and entities remain unchanged.
- **Notifications/DTOs and interfaces:** Reviewed contracts. Added a dedicated preference service interface for the existing two-endpoint workflow; generic CRUD remains on the existing template controller.
- **Notifications/Services and Channels:** Reviewed every sender and service. Changed notification persistence to `IUnitOfWork.Context`, moved preferences query/save from the controller into a service, and replaced notification/entity DTO mapping with Mapster. Existing channel transports and message ordering remain in place.
- **Notifications/Jobs:** Reviewed batch wrappers, scheduled processor, and cleanup handler. Each existing discovery, claim, delivery, and retry scope now resolves its own scoped unit of work. Claim token checks, retry scheduling, and SaveChanges positions remain unchanged.
- **Realtime:** Reviewed hub base, chat hub, realtime hub, and manager. Retained their distinct SignalR contracts; generic REST controller/service bases do not apply.
- **Persistence and module root:** Reviewed `ICommunicationDataContext`, global usings, project references, module DI, and README. Kept the legacy context contract for compatibility with the existing Infrastructure registration. Added preference DI and Communication Mapster assembly discovery in `Program.cs`.

## Complete C# file and declaration inventory

| File | Declarations | Result |
|---|---|---|
| `Chat/Entities/SysChatMessage.cs` | SysChatMessage | Retained: Entity and schema shape preserved |
| `Chat/Entities/SysChatMessageConfiguration.cs` | SysChatMessageConfiguration | Retained: Entity and schema shape preserved |
| `Chat/Entities/SysChatReadState.cs` | SysChatReadState | Retained: Entity and schema shape preserved |
| `Chat/Entities/SysChatReadStateConfiguration.cs` | SysChatReadStateConfiguration | Retained: Entity and schema shape preserved |
| `Chat/SendChatMessageRequest.cs` | SendChatMessageRequest | Retained: API contract retained |
| `Chat/Services/ISysChatService.cs` | ISysChatService | Retained: Dedicated service contract retained |
| `Chat/Services/SysChatConversationDto.cs` | SysChatConversationDto | Retained: API contract retained |
| `Chat/Services/SysChatMessageDto.cs` | SysChatMessageDto | Retained: API contract retained |
| `Chat/Services/SysChatService.cs` | SysChatService | Refactored |
| `Chat/SysChatController.cs` | SysChatController | Retained: HTTP boundary retained |
| `CommunicationModule.cs` | CommunicationModule | Refactored |
| `GlobalUsings.cs` | Global usings | Retained: Shared imports retained |
| `Notifications/Configuration/SysNotificationConfiguration.cs` | SysNotificationConfiguration | Retained: EF mapping preserved |
| `Notifications/Configuration/SysNotificationMapping.cs` | SysNotificationMapping | Created |
| `Notifications/Configuration/SysNotificationRecipientConfiguration.cs` | SysNotificationRecipientConfiguration | Retained: EF mapping preserved |
| `Notifications/Configuration/SysNotificationTemplateConfiguration.cs` | SysNotificationTemplateConfiguration | Retained: EF mapping preserved |
| `Notifications/CreateSysNotificationDto.cs` | CreateSysNotificationDto | Retained: API contract retained |
| `Notifications/Entities/SysNotification.cs` | SysNotification | Retained: Entity and schema shape preserved |
| `Notifications/Entities/SysNotificationAuditLog.cs` | SysNotificationAuditLog | Retained: Entity and schema shape preserved |
| `Notifications/Entities/SysNotificationEnums.cs` | SysNotificationPriority, SysNotificationChannel, SysNotificationStatus, SysDeliveryStatus | Retained: Entity and schema shape preserved |
| `Notifications/Entities/SysNotificationPreference.cs` | SysNotificationPreference | Retained: Entity and schema shape preserved |
| `Notifications/Entities/SysNotificationRecipient.cs` | SysNotificationRecipient | Retained: Entity and schema shape preserved |
| `Notifications/Entities/SysNotificationTemplate.cs` | SysNotificationTemplate | Retained: Entity and schema shape preserved |
| `Notifications/Entities/SysScheduledNotification.cs` | SysScheduledNotification, SysScheduledJobType, SysScheduledJobStatus | Retained: Entity and schema shape preserved |
| `Notifications/Jobs/CleanupExpiredNotificationsJobHandler.cs` | CleanupExpiredNotificationsJobHandler | Refactored |
| `Notifications/Jobs/CommunicationBatchServices.cs` | EmailDeliveryBatchService, SmsDeliveryBatchService, PushNotificationBatchService, NotificationDeliveryBatchService | Retained: Batch delegation retained |
| `Notifications/Jobs/WorkflowNotificationBatchServices.cs` | WorkflowNotificationBatchService, WorkflowReminderBatchService, WorkflowEscalationBatchService, WorkflowCleanupBatchService | Refactored |
| `Notifications/Services/Channels/ISysNotificationChannelSender.cs` | ISysNotificationChannelSender | Retained: Channel strategy retained |
| `Notifications/Services/Channels/SysEmailNotificationChannelSender.cs` | SysEmailNotificationChannelSender | Retained: Channel strategy retained |
| `Notifications/Services/Channels/SysInAppNotificationChannelSender.cs` | SysInAppNotificationChannelSender | Refactored |
| `Notifications/Services/Channels/SysNotificationChannelResult.cs` | SysNotificationChannelResult | Retained: Channel strategy retained |
| `Notifications/Services/Channels/SysPushNotificationChannelSender.cs` | SysPushNotificationChannelSender | Retained: Channel strategy retained |
| `Notifications/Services/Channels/SysSlackNotificationChannelSender.cs` | SysSlackNotificationChannelSender | Retained: Channel strategy retained |
| `Notifications/Services/Channels/SysSmsNotificationChannelSender.cs` | SysSmsNotificationChannelSender | Retained: Channel strategy retained |
| `Notifications/Services/Channels/SysTeamsNotificationChannelSender.cs` | SysTeamsNotificationChannelSender | Retained: Channel strategy retained |
| `Notifications/Services/Channels/SysWebhookNotificationChannelSender.cs` | SysWebhookNotificationChannelSender | Retained: Channel strategy retained |
| `Notifications/Services/Channels/SysWhatsAppNotificationChannelSender.cs` | SysWhatsAppNotificationChannelSender | Retained: Channel strategy retained |
| `Notifications/Services/ISysNotificationPreferenceService.cs` | ISysNotificationPreferenceService | Created |
| `Notifications/Services/ISysNotificationService.cs` | ISysNotificationService | Retained: Dedicated service contract retained |
| `Notifications/Services/ScheduledNotificationBatchProcessor.cs` | ScheduledNotificationBatchProcessor | Refactored |
| `Notifications/Services/ScheduledNotificationClaims.cs` | ScheduledNotificationClaims | Retained: Claim predicate retained |
| `Notifications/Services/SysNotificationPreferenceService.cs` | SysNotificationPreferenceService | Created |
| `Notifications/Services/SysNotificationService.cs` | SysNotificationService | Refactored |
| `Notifications/SysNotificationController.cs` | SysNotificationController | Retained: HTTP boundary retained |
| `Notifications/SysNotificationDto.cs` | SysNotificationDto | Retained: API contract retained |
| `Notifications/SysNotificationPreferenceController.cs` | SysNotificationPreferenceController | Refactored |
| `Notifications/SysNotificationPreferenceDto.cs` | SysNotificationPreferenceDto | Retained: API contract retained |
| `Notifications/SysNotificationRecipientDto.cs` | SysNotificationRecipientDto | Retained: API contract retained |
| `Notifications/SysNotificationTemplateController.cs` | SysNotificationTemplateController | Retained: HTTP boundary retained |
| `Notifications/SysNotificationTemplateDto.cs` | SysNotificationTemplateDto | Retained: API contract retained |
| `Persistence/ICommunicationDataContext.cs` | ICommunicationDataContext | Retained: Infrastructure compatibility retained |
| `Realtime/SysChatHub.cs` | SysChatHub | Retained: SignalR contract retained |
| `Realtime/SysHubBase.cs` | SysHubBase | Retained: SignalR contract retained |
| `Realtime/SysRealtimeHub.cs` | SysRealtimeHub | Retained: SignalR contract retained |
| `Realtime/SysRealtimeManager.cs` | SysRealtimeManager | Retained: SignalR contract retained |

## Requested final checklist

1. **Folders analyzed:** Chat and nested Entities/Services; Notifications and nested Configuration/Entities/Jobs/Services/Channels; Realtime; Persistence; module root.
2. **Every class analyzed:** The full 55-file C# inventory is above; files with no architecture gap were retained.
3. **Classes modified:** 8 existing Communication files, listed as Refactored above. `Program.cs` and one existing Communication test were also updated for registration and verification.
4. **Classes created:** `SysNotificationMapping`, `ISysNotificationPreferenceService`, `SysNotificationPreferenceService`; one mapping test file was added.
5. **Classes removed:** None.
6. **Controller changes:** `SysNotificationPreferenceController` now delegates to a service. Other controllers remain thin; `SysNotificationTemplateController` already uses `BaseController`/`IBaseService`.
7. **Service changes:** Chat, notification, scheduled notification, and cleanup persistence now use the Shared unit of work. Preference query/save logic is in a service.
8. **IBaseService usage:** The template CRUD controller retains `IBaseService<SysNotificationTemplate>`. Chat, notification delivery, and preferences use dedicated interfaces because generic CRUD would change their API or workflow.
9. **IUnitOfWork usage:** Persistence paths use the scoped `IUnitOfWork.Context`. Direct `SaveChangesAsync` calls remain at their original positions to preserve delivery and retry semantics; the context is the same scoped `ApplicationDbContext`.
10. **Mapster configuration:** Added `SysNotificationMapping`, registered the Communication assembly for startup scanning, and mapped chat/notification/preference entity DTOs through Mapster. Constructed command DTOs and aggregate conversation DTOs remain explicit because they are not direct entity mappings.
11. **Validation changes:** No new validator was needed. Existing simple HTTP checks and response codes remain.
12. **Configuration changes:** Added Mapster configuration only; no EF configuration was changed.
13. **DI changes:** Registered `ISysNotificationPreferenceService`; Shared `IUnitOfWork` registration already exists.
14. **Architecture issues fixed:** Removed direct persistence from the preference controller, replaced manual entity-to-DTO mapping, and routed Communication persistence through the Shared unit of work.
15. **Build/compile result:** Communication baseline and post-change builds passed. A full IXApi build passed with 0 errors and 173 warnings; the subsequent Communication build passed with 0 errors and 0 warnings after correcting the new Mapster nullability warning. `IXApi.Tests` passed 232 tests with 2 skipped.
16. **API behavior verification:** Changed preference controller retained its route, `[Authorize]`, GET/PUT methods, status codes, and response envelopes. New mapping test checks notification ID, recipient flags, and omitted recipients. No authenticated live API test was run.
17. **Database/schema verification:** No entity, EF configuration, or migration files were modified. No live database comparison was run.
18. **Remaining issues:** The legacy `ICommunicationDataContext` interface and Infrastructure registration remain for compatibility. Background delivery and database behavior have compile/test evidence only; live delivery providers and database flows were not exercised.

## Out-of-scope working tree

Existing AccountsReceivable edits in the working tree were not modified by this task.
