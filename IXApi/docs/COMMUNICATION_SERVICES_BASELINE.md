# Communication Services Compatibility Baseline

This document freezes the externally observable Communication behavior before
shared contracts are extracted. Refactoring must preserve existing API payloads,
database behavior, realtime event names, routes, permissions, and delivery rules.

## Capability ownership

| Capability | Current public contract | Current implementation owner |
| --- | --- | --- |
| Notification commands and inbox | `ISysNotificationService` | Communication/Notifications |
| Delivery channel strategy | `ISysNotificationChannelSender` | Communication/Notifications/Channels |
| Notification preferences | Controller and `ICommunicationDataContext` | Communication/Notifications |
| Notification templates | Generic service/controller | Communication/Notifications |
| Scheduled notifications | Hosted service | Communication/Notifications |
| Chat persistence and delivery | `ISysChatService` | Communication/Chat |
| Realtime delivery | `ISysRealtimeManager` | Shared contract, Communication implementation |
| SignalR hubs | `SysRealtimeHub`, `SysChatHub` | Communication/Realtime |

## API and realtime compatibility

- `api/v1/SysNotification` keeps inbox queries, unread count, read/archive/delete
  mutations, and notification creation.
- `api/v1/SysNotificationTemplate` keeps inherited CRUD routes and the existing
  `System.NotificationTemplate` permission family.
- Existing notification preference and chat action templates must be inventoried
  against generated endpoint metadata before route normalization. Their
  controllers currently do not declare class-level route attributes.
- `/hubs/realtime` and `/hubs/chat` remain authenticated.
- Existing client method names such as `ReceiveMessage` and existing realtime
  event-type values remain stable.

## Persistence and delivery compatibility

The refactor must not rename Communication tables, keys, indexes, recipient
status values, notification channels, preference defaults, chat room identifiers,
or delivery audit behavior. In-app, email, SMS, push, WhatsApp, Teams, Slack, and
webhook channel registrations remain available.

## Current coupling to remove incrementally

1. Workflow references Communication entities, DTOs, enums, templates, and its
   concrete notification service contract.
2. Administration uses Communication realtime contracts from its background-job
   processor.
3. `ICommunicationDataContext` exposes Identity and Organization entity types for
   recipient resolution.
4. Scheduled notifications run through a dedicated hosted polling service while
   Administration owns a separate generic job engine.
5. Preference persistence is implemented directly in a controller rather than a
   reusable application service.

## Phase 2 extraction boundaries

Introduce narrow shared contracts for notification commands, realtime publishing,
chat commands, recipient resolution, channel delivery, and preference access.
Keep current `ISys*` interfaces and controllers as compatibility adapters until
all consumers are migrated and endpoint metadata confirms route compatibility.

Identity and Organization recipient lookup should move behind provider contracts
implemented by their owning modules. Communication must not expose those modules'
EF entities through its reusable application contracts.

## Required validation after each phase

1. Independent Shared, Administration, Communication, and affected-module builds.
2. Full Release solution build and test suite.
3. API route, authorization, permission, DTO, hub, and event compatibility tests.
4. EF Core pending-model-change check.
5. Startup, `/health`, SignalR negotiation, and notification delivery smoke tests
   when database configuration is available.
