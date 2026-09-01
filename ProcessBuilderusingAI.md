# Build a Generic Multi-Agent AI Platform in IAX

## Mission and safety boundary

Analyze IAX first and, only after the analysis is reviewed and approved, build a generic multi-agent AI platform for the whole application. It must support unrelated current and future capabilities without depending on Workflow, Process Builder, reports, or any single business module.

`ProcessBuilderAgent` is only the first proposed reference consumer used to prove the extension model. It is not the purpose, name, or architectural center of the AI platform, and its implementation is a separate business workstream after the generic foundation is approved.

Do not invent a parallel workflow engine, generate executable SQL, or write directly to the database. Keep specialized agents and their prompts/tools outside the generic `Infrastructure/AI` platform.

```text
User input/attachments → Agent → WorkflowBlueprint → Server validator
→ User preview/approval → Transactional builder → Existing Wf* entities → Verification
```

The primary product flow is:

```text
User
  ↓
Text / PDF / Image / Existing Form
  ↓
AI Requirement Analyzer
  ↓
ProcessDefinition JSON
  ↓
Validation + Rules Engine
  ↓
Preview in React
  ↓
User Approves
  ↓
ProcessBuilderService
  ↓
WF Tables
```

`ProcessDefinition JSON` is the serialized form of the versioned, strongly typed `WorkflowBlueprintDto`. It is an intermediate proposal, not executable instructions. It must pass schema validation, deterministic workflow rules, authorization, tenant checks, metadata resolution, and user approval before `ProcessBuilderService` can persist anything.

## Mandatory first step

Do not implement immediately. Inspect the repository and confirm every statement below. Report architecture, physical tables, keys, relationships, tenant/audit/delete behavior, number sequences, current builder flow, runtime flow, reusable code, gaps, proposed contracts/services/files, risks, and unresolved decisions. Stop and wait for explicit approval.

Confirmed baseline to re-verify:

- Backend: `IXApi`, ASP.NET Core, EF Core, generic CRUD services/controllers, `DomainPermission`, number sequences, tenant/audit base entities, and `APIResponse<T>`.
- Frontend: `IXApp`, React/TypeScript, MUI, Zustand, i18next, typed APIs, and `IXApp/src/modules/process-builder`.
- An OpenAI `ChatClient` registration helper exists at `IXApi/Bootstrap/Extensions/OpenAiExtensions.cs`; verify actual runtime registration and production readiness.
- Reuse existing document-management and versioned print-template subsystems.

### Repository-aligned dependency boundary

The current project-reference direction is important:

```text
Workflow module → Shared and selected business modules
Infrastructure  → Workflow and the other modules
Web host        → Infrastructure + modules
```

Adopt this target boundary when creating the feature:

```text
src/
├── Infrastructure/
│   └── AI/
│       ├── Abstractions/
│       ├── Agents/
│       ├── Providers/
│       ├── Tools/
│       ├── Documents/
│       ├── Orchestration/
│       ├── Models/
│       ├── Memory/
│       └── Configuration/
│
└── Modules/
    ├── Workflow/
    │   └── ProcessBuilding/
    └── Intelligence/
        └── ProcessBuilder/
            ├── Agents/
            ├── Analysis/
            ├── Generation/
            ├── Automation/
            ├── Prompts/
            ├── Tools/
            └── Endpoints/
```

Responsibilities:

- `Modules/Workflow/ProcessBuilding` owns the canonical definition contracts needed to create a workflow, deterministic workflow validation/compilation, the transactional builder, and verification. It remains AI-independent.
- `Modules/Intelligence/ProcessBuilder` owns AI-assisted business orchestration: the Process Architect agent, requirement analysis, definition generation, clarification/version orchestration, business prompts/tools, and guarded endpoints. It references Workflow contracts.
- `Infrastructure/AI` is the approved home of the complete generic AI platform: abstractions, generic agents/runtime models, provider implementations, generic tools, document extraction, orchestration, generic models, bounded memory, retries, and configuration.
- The web host/composition root registers provider implementations against interfaces used by Intelligence.
- Neither Workflow nor Intelligence may reference Infrastructure or the OpenAI SDK.
- `Infrastructure/AI/Models` contains generic AI request/response/message/attachment/usage models only; canonical `ProcessDefinition`/`WorkflowBlueprintDto` must not live there.
- `Infrastructure/AI/Tools` binds provider tool-calling protocols to controlled application tools; tool authorization and business contracts remain in Intelligence/Workflow.
- `Infrastructure/AI/Documents` extracts technical content; normalized evidence and business interpretation belong to Intelligence.
- Avoid modules named `AI`, `ChatGPT`, or `OpenAI`; provider names belong under `Infrastructure/AI/Providers`.

### Generic AI-layer rule

The complete `Infrastructure/AI` layer must remain domain-agnostic and reusable by any module. It must not reference or contain:

- `Workflow`, `Wf*` entities, `ProcessDefinition`, or `WorkflowBlueprintDto`
- process controls, steps, activities, transitions, performers, or workflow validation rules
- Process Builder prompts or business-specific tool definitions
- direct EF Core workflow persistence

It may expose only generic capabilities such as:

- model/chat/structured-output requests and responses
- provider-neutral messages and content parts
- generic tool schemas, invocation envelopes, and results
- document/text/image extraction contracts and technical results
- provider selection, credentials/options, retries, timeouts, cancellation, rate/cost/usage metadata, and telemetry
- generic safety limits and serialization

Workflow-specific orchestration belongs under `Modules/Intelligence/ProcessBuilder`; canonical definition validation and persistence belong under `Modules/Workflow/ProcessBuilding`.

Provider-neutral interfaces live in the approved `Infrastructure/AI/Abstractions` folder. Intelligence owns only process-specific ports, agents, prompts, tools, and orchestration adapters. OpenAI SDK types must not appear in the generic abstraction contracts or business-layer contracts.

Preferred generic contracts:

```csharp
public interface IAIService
{
    Task<AIResult<TResponse>> GenerateAsync<TResponse>(
        AIRequest request,
        CancellationToken cancellationToken);
}

public interface IAIProvider
{
    Task<AIProviderResult> GenerateAsync(
        AIProviderRequest request,
        CancellationToken cancellationToken);
}

public interface IAIAgent
{
    string Name { get; }
    string Description { get; }
    Task<AgentResult> ExecuteAsync(
        AgentExecutionContext context,
        CancellationToken cancellationToken);
}
```

- Put `IAIService`, `IAIProvider`, `IAIAgent`, `IAIAgentRegistry`, `IAIAgentOrchestrator`, `IAITool`, and `IDocumentExtractor` in `Infrastructure/AI/Abstractions`.
- Put OpenAI/Azure/future provider implementations in Infrastructure.
- `AIRequest` contains only generic instructions, messages, content/attachments, tool descriptors, response-schema metadata, and execution limits—never Process Builder fields.
- `GenerateAsync<TResponse>` must not reference or special-case ProcessDefinition, reports, invoices, contracts, or another domain model.
- A generic agent runner is allowed only when its state/tool/message contracts remain domain-neutral. Workflow prompts, tools, and state remain in Intelligence.

The generic platform must support DI-based agent registration and discovery, execution by stable agent name, reusable tool registration, structured output, attachment preprocessing, conversation context, logging/audit metadata, usage tracking, cancellation, and normalized errors. Avoid hardcoded agent-name switch statements.

Version 1 first delivers and tests the central generic `AIAgentOrchestrator`, registries, providers, tools, documents, and execution pipeline using domain-neutral test agents. `ProcessBuilderAgent` may then be registered as the first business consumer. Future agents such as `DocumentAnalyzerAgent`, `ReportBuilderAgent`, and `WorkflowAssistantAgent` are independent consumers, not changes to AI Core. Agent-to-agent delegation is deferred.

Durable memory requires an approved retention, privacy, encryption, tenant-isolation, and deletion policy. Generic AI memory must not own workflow blueprints, approvals, or business audit records.

Do not create empty backend `Modules/ProcessBuilder` or `Modules/Reports` folders merely to match a conceptual tree. They should exist only if those backend modules have approved ownership and project boundaries. The current Process Builder backend aggregate belongs to Workflow; the existing React Process Builder remains under `IXApp/src/modules/process-builder`.

## Actual workflow model

### Master metadata—resolve, never invent IDs

- `WfCategories`, `WfPriorities`, `WfProcessTypes`
- `WfControls`, `WfDataTypes`, `WfActivityTypes`, `WfOperators`
- Physical table `WfPerformerType`, entity `WfPerformerType`

### Definition/configuration graph

- `WfProcesses`
- `WfUsersProcesses` (availability by employee/department/occupation; not identity users)
- `WfRequestControls`, `WfRequestControlsOptions`, `WfRequestControlsValidations`
- `WfVariables`
- `WfSteps`, `WfActivities`
- `WfActivityControls`, `WfActivityControlsOptions`, `WfActivityControlsValidations`
- `WfRequestMappingVariables`, `WfActivityMappingVariables`
- `WfTransitions`
- `WfPerformers`
- Physical table `WfUsersPerformers`, code entity/DbSet `WfPerformerUsers`
- `WfPrintTemplates`, `WfPrintTemplateVersions`

```text
WfProcess
├── RequestControls ── options/validations ── request-variable mappings ── Variables
├── Variables
├── Steps ── Activities ── Performer
│                        └── ActivityControls ── options/validations/mappings
├── Transitions
├── UsersProcesses
└── PrintTemplates ── Versions
```

`WfTransition` actually stores `ProcessId`, optional `ActivityId`, optional `RequestControlId`, required `VariableId`, `OperatorId`, comparison `Value`, destination `StepId`, and `SortOrder`. Compile logical transitions into this model; do not assume a source-step/destination-step schema.

### Runtime/history—never populate during definition creation

- `WfRequests`, `WfRequestDetails`, `WfAssignments`, `WfActivityDetails`
- `WfProcessData`, `WfProcessVariables`, `WfRequestVariables`
- `WfRequestPrintVersions`

`WfRequestTransition` exists as a class but is not registered/configured. Treat it as unconfirmed/dead code unless analysis proves otherwise.

The generic words **Form** and **Section** from source documents are analysis/layout concepts, not confirmed IAX database entities. Do not invent `Forms` or `Sections` tables. Map grouping and layout only through mechanisms verified in the current project—such as request-control ordering/`ExtendedProperties` and print-template section/row/column elements—or report a capability gap requiring approval.

## Current Process Builder constraints

The current frontend loads an aggregate view but persists through separate CRUD sequences:

- `saveProcessBuilder`
- `saveProcessVariables`
- `saveProcessSteps`
- `saveProcessActivities`
- `saveProcessRequestControls`
- `saveProcessTransitions`

It stores edits in Zustand and browser `localStorage`. It does not include mappings, access rows, performer creation/user mappings, or print-template documents in its main `ProcessBuilderDocument`.

Do not use this multi-request persistence path for AI creation because it is not atomic. Reuse its editor/preview only where contracts are lossless. Build one backend aggregate command and transaction for the AI path.

## Dynamic request rules

Request definitions come from active request controls, control metadata, options, validations, and JSON `ExtendedProperties`. Respect `Name`/`NameAlias`, control type, ordering, score, required/read-only/unique/criteria/default/column span, visibility, option features, and separate validation/option rows.

Submission creates runtime `WfRequest` and `WfRequestDetail` rows. Never confuse these with definition records. Respect current 255-character runtime-value constraints and existing table/file serialization.

## First reference consumer: Process Builder

After the generic platform is working independently, create `ProcessBuilderAgent` as a specialized Intelligence-module consumer which:

1. Accepts authorized Arabic/English input and attachments.
2. Separates request inputs, repeating data, calculations/system data, activity inputs, variables, routing, performers, print-only content, access rules, and unresolved configuration.
3. Reads only authorized tenant-scoped metadata/reference structures.
4. Generates/revises a versioned, strongly typed blueprint using logical keys.
5. Calls deterministic server validation.
6. Shows preview, assumptions, warnings, and missing configuration.
7. Persists only after explicit approval and fresh validation.
8. Verifies the stored graph and never activates silently.

The AI role is best described as an **AI Process Architect / Requirement Analyzer**. The existing WF application remains the process-definition persistence and execution authority.

This is an **AI-assisted process generator**, not a fully autonomous agent. Structured model output, controlled tool calling, deterministic validation, and explicit human approval are mandatory.

The input pipeline is:

```text
Text / TXT / PDF / Image / Screenshot / Existing Form / Policy Document
  ↓
Authorized document extraction
  ↓
Normalized requirement evidence
  ↓
AI requirement analysis
  ↓
ProcessDefinition JSON / WorkflowBlueprintDto
```

The analyzer must interpret meaning rather than convert every sentence into a field. For example, “attach the bank deposit receipt” becomes a required attachment control, not a text box containing the sentence.

## Controlled tools only

Use narrow typed tools such as `GetWorkflowMetadata`, `GetProcessStructure`, `SearchReferenceProcesses`, `GetPerformerCatalog`, `ValidateWorkflowBlueprint`, `PreviewWorkflowBlueprint`, `CreateWorkflowFromBlueprint`, and `VerifyWorkflowDefinition`.

Read-only discovery should include equivalents of:

- get supported control types and their real `WfControls` metadata
- get supported validations and operators
- get the canonical process-definition schema
- find structurally similar authorized processes
- get a sanitized existing process definition

Similarity is advisory. Never copy IDs, tenant-specific assignments, security, branding, or business rules automatically.

Every tool requires typed input/output, tenant scope, permissions, cancellation, safe errors, and audit for mutations. Never expose `ExecuteSql`, generic mutation, raw connection strings, arbitrary URLs, or unrestricted filesystem access.

## Versioned WorkflowBlueprint

Use logical keys for new relationships. Database IDs are allowed only as validated references to existing metadata.

```text
WorkflowBlueprintDto
├── schemaVersion, process, accessRules[]
├── requestControls[] ── options[]/validations[]/visibility
├── variables[], requestVariableMappings[]
├── performers[] ── controlled resolution
├── steps[] ── activities[] ── controls[]/options[]/validations[]
│                           └── activityVariableMappings[]
├── transitions[]
├── printTemplateProposal(s)
├── evidence[] and element traceability
├── assumptions[], warnings[]
└── missingConfiguration[]
```

Logical keys must be unique, stable within a blueprint version, and case-normalized. Mappings reference control keys and variable keys. Activities reference step keys, metadata activity-type codes, and performer keys. The backend resolves codes to tenant-valid IDs.

`ProcessDefinition` is the product-facing name and `WorkflowBlueprintDto` is the strongly typed backend contract. They represent the same canonical intermediate definition; do not maintain two divergent schemas.

### Evidence, confidence, and clarification

Every generated element should support concise traceability without exposing hidden chain-of-thought:

```text
evidence:
  sourceType: text | txt | pdf | image | screenshot | existingProcess
  sourceReference: page/region/paragraph or attachment reference
  sourceText: short relevant excerpt within copyright/privacy limits
  classification: EXPLICIT | INFERRED | REQUIRES_CLARIFICATION
  confidence: 0.0–1.0
  rationale: short user-facing explanation
```

- `EXPLICIT`: directly present in supplied evidence.
- `INFERRED`: a reversible design inference supported by evidence.
- `REQUIRES_CLARIFICATION`: a material business decision that must not be invented.

Important routing, approval, performer, security, financial threshold, and activation decisions marked `REQUIRES_CLARIFICATION` must block creation until resolved. Confidence alone never overrides deterministic validation or user approval.

The system must preserve the distinction between extracted evidence and AI conclusions so users can see why a control, rule, or workflow stage was proposed.

## Controls, variables, and performers

- Reuse `WfControls`; never invent `ControlId`.
- Preserve English `Name` and Arabic `NameAlias`.
- Use the generic table control for repeating rows; never create `Item1`, `Item2`, etc.
- Approver decisions/comments belong to `WfActivityControls`.
- Create variables only for required state/mapping/routing, and require a valid source for transition variables.
- Resolve performers through actual `WfPerformer` fields: `PerformerTypeId`, `RelatedField`, `IsApplicant`, `IsEmployee`, and manager flags 1–4.
- Never invent employee/user IDs; unresolved routing belongs in `missingConfiguration`.
- The performer entity has SQL fields. AI-generated SQL must never enter them; any reuse requires separate security approval.

## Deterministic validator

Validate independently of AI judgment:

- schema/payload/collection/string limits and duplicate logical keys
- bilingual labels/codes and number-sequence/manual-code rules
- tenant-valid category, priority, process type, control, data type, activity type, operator, and performer references
- sort/auto-pass ranges, option/control compatibility, validations, visibility references/cycles
- request/activity mappings and same-process ownership
- step/activity/control ownership
- transition trigger, variable, operator, typed value, destination, reachability, cycles, completion/rejection paths, and ambiguity
- access rules and print documents using `PrintTemplateDocumentValidator`
- soft-deleted/cross-tenant references and conflicts with existing active processes

Return structured `errors`, `warnings`, and `missingConfiguration`; errors block persistence.

## Transactional builder and lifecycle

Create a dedicated `WorkflowBuilderService`; the agent never persists entities.

The builder must be provider-independent and AI-independent. Its public application contract should accept a validated canonical definition plus execution context, not model-specific messages. The same engine should eventually support:

- AI-generated definitions
- the manual Process Builder
- controlled imports
- trusted APIs
- approved templates
- cloning an authorized existing process

Do not force the existing manual UI to migrate in the first release, but design the service so these paths can converge safely later.

Conceptually:

```csharp
Task<ProcessCreationResult> CreateAsync(
    WorkflowBlueprintDto definition,
    ProcessCreationContext context,
    CancellationToken cancellationToken);
```

The service must accept only a server-validated definition/version token at the external API boundary; it must not trust a client claim that validation already occurred.

Build the canonical definition, deterministic validator/compiler, and transactional builder before integrating a model provider. AI is an input adapter that produces a proposed definition; it is not the foundation of process persistence.

Keep process-specific responsibilities explicitly separated:

```text
IProcessGenerationService
  Requirement/evidence → generic IAIService → proposed ProcessDefinition

IProcessBuilderKnowledgeService
  Authorized controls, validations, rules, and reference structures

IProcessDefinitionValidator
  Deterministic validation against actual WF rules

IProcessBuilderService
  Validated definition → transactional WF configuration persistence
```

Generation belongs to `Intelligence/ProcessBuilder`. Knowledge is exposed through authorized Workflow-owned contracts. Validation and persistence belong to `Workflow/ProcessBuilding`.

- Use one EF execution strategy and transaction.
- Resolve metadata/logical keys server-side.
- Create inactive first and persist in EF-confirmed dependency order.
- Never create runtime rows.
- Verify before commit where feasible and after reload.
- Roll back completely on failure.
- Use idempotency and concurrency protection.
- Never overwrite an active process silently.
- Activation is a separate permissioned command after verification and confirmation.

## Sessions, audit, security, and privacy

Maintain blueprint versions through conversation edits. Do not restart analysis unnecessarily. If persistence is approved, record tenant/user, requirement, attachment references, model/prompt versions, blueprints, validation, edits, approval, idempotency, created process, verification, and activation.

Do not add audit/session tables until retention, privacy, tenant isolation, cleanup, and migration are approved.

Use separate analyze/validate/create/activate permissions and recheck permissions on every tool. Treat files and model output as untrusted. Enforce file limits, scanning where available, prompt-injection resistance, strict output schemas, rate/time/token/cost limits, cancellation, secret redaction, and tenant isolation. Never send unrelated employee/company data or secrets to the model.

## Frontend integration

Extend the existing Process Builder and design system. Reuse MUI, i18next, shared feedback, dynamic-form preview, workflow editor, and print-template designer/runtime.

Provide conversation/attachments, versioned blueprint preview, request form, workflow graph, steps/activities/transitions, performers/access, print proposal, validation panels, and explicit revise/validate/create-inactive/verify/activate actions. Localize all visible text for Arabic RTL and English LTR.

The preview must display source evidence, classification, confidence, assumptions, warnings, and missing information. Users must be able to correct the definition before approval and then open the created inactive process in the normal Process Builder.

Use a responsive two-pane experience where space permits:

```text
┌─────────────────────────────┬─────────────────────────────┐
│ AI input and attachments    │ Generated process          │
│ Conversation / Analyze      │ Form, workflow, warnings   │
│ Clarification questions     │ Evidence / Edit / Validate │
└─────────────────────────────┴─────────────────────────────┘
```

On narrow screens, stack the panels without losing the review-before-create sequence.

## Conceptual API surface

Adjust exact routes to existing controller conventions, but keep responsibilities separate:

```text
POST /api/v1/intelligence/process-builder/analyze   → definition + evidence + warnings
POST /api/v1/intelligence/process-builder/validate  → deterministic validation
POST /api/v1/intelligence/process-builder/create    → approved token → inactive ProcessId
POST /api/v1/intelligence/process-builder/verify    → stored graph verification
POST /api/v1/intelligence/process-builder/activate  → separate authorized action
```

Analyze may accept text plus authorized attachment references. Create must not accept an unvalidated arbitrary definition without a server-issued validation/approval token.

## Testing

Add unit, integration, authorization, tenant, rollback, idempotency, validation, malformed-output, prompt-injection, attachment-limit, and end-to-end tests proving:

- invalid blueprints never make partial writes
- runtime tables remain untouched
- SQL cannot be executed
- cross-tenant/soft-deleted IDs are rejected
- logical keys resolve deterministically
- mappings/transitions point to created entities
- retries do not duplicate processes
- activation requires permission and explicit action
- manual Process Builder, request submission, mail, and print remain compatible

## Reference process 590

Use process 590 only if it exists and the user is authorized. Extract structural patterns only. Never copy its IDs, tenant data, users, performer assignments, business decisions, or branding.

## Required output before implementation

Return: current architecture; actual tables/relationships; configuration/runtime classification; current builder atomicity limitations; request-runtime flow; reusable components; corrected blueprint schema; tools/permissions; validator/builder design; frontend integration; file proposal; security/privacy risks; architecture conflicts; missing decisions; and phased plan with acceptance criteria.

Do not create implementation files until this analysis is reviewed and explicitly approved.
