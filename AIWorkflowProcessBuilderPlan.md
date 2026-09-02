# Generic Multi-Agent AI Platform — Review and Implementation Plan

## Scope

This repository-grounded plan prepares a reusable AI platform for the entire IAX application; it does not implement it. Process Builder is documented as the first reference business consumer, not as the platform's architectural purpose.

## Platform objective

Deliver a domain-neutral AI core supporting providers, registered agents, orchestration, safe tools, structured output, documents/attachments, bounded context, telemetry, auditing metadata, authorization, and future extensibility. The core must run and be tested without referencing Workflow or loading a Process Builder agent.

## First reference-consumer flow: Process Builder

```text
User
  ↓
Text / PDF / Image / Existing Form
  ↓
AI Requirement Analyzer
  ↓
ProcessDefinition JSON (serialized WorkflowBlueprintDto)
  ↓
Validation + Rules Engine
  ↓
Preview in React
  ↓
User Approves
  ↓
ProcessBuilderService (single controlled transaction)
  ↓
Existing WF configuration tables
```

The JSON is never executed directly and never contains SQL. The validator and rules engine resolve logical keys and approved metadata codes; the service alone controls EF Core persistence.

## Confirmed findings

- Workflow uses ASP.NET Core/EF Core, generic CRUD services/controllers, `DomainPermission`, tenant/audit base entities, and number sequences.
- The React/MUI Process Builder uses Zustand plus `localStorage` drafts.
- An OpenAI `ChatClient` helper exists, but no active agent/orchestration implementation was found; runtime registration must be confirmed.
- Print templates already have draft/publish/version behavior and a document validator.
- The definition graph centers on processes, access rows, request controls, variables, steps, activities, activity controls, mappings, transitions, performers, and print templates.
- Runtime rows (`WfRequests`, details, assignments, activity/process data and values) must not be created by the builder.

```text
WfProcesses
├── WfUsersProcesses
├── WfRequestControls ── options / validations / request-variable mappings
├── WfVariables
├── WfSteps ── WfActivities ── WfPerformers ── WfUsersPerformers
│                           └── WfActivityControls ── options / validations / mappings
├── WfTransitions
└── WfPrintTemplates ── WfPrintTemplateVersions
```

## Important architecture conflicts

1. Current Process Builder persistence is frontend orchestration across multiple CRUD calls, not one atomic backend save.
2. The builder document omits mappings, process access, performer creation/mapping, and print templates.
3. Physical `WfUsersPerformers` is represented by `WfPerformerUsers` in code.
4. `WfUsersProcesses` targets employee/department/occupation, not identity users.
5. `WfTransition` uses optional activity/request-control triggers plus variable/operator/value and destination step; a generic graph must compile to this shape.
6. `WfProcessVariables` and `WfRequestVariables` are runtime request values, not definition variables.
7. `WfRequestTransition` is an unregistered class and must not be assumed to be a real table.
8. Performer SQL fields must never accept AI-generated SQL.
9. Existing delete behaviors are mixed between cascade and restrict; persistence/update logic must follow EF configuration.
10. `WfRequestVariable` key configuration and other legacy inconsistencies require characterization tests before reliance.
11. Source documents may contain forms and sections, but no separate Workflow form/section entities were confirmed. Treat them as canonical layout concepts and compile them only to verified request-control properties or print-template layout; otherwise report a gap instead of adding tables implicitly.
12. The approved source layout places generic contracts under `Infrastructure/AI/Abstractions`, while the current project direction is `Infrastructure → Modules`. A module-owned business agent cannot reference the existing Infrastructure project without reversing/cycling dependencies. Resolve this before implementation through an approved dependency-safe contracts project or repository-consistent composition approach; do not silently add `Modules → Infrastructure`.

## Recommended components

### Backend feature and dependency boundary

The verified solution currently uses `Infrastructure → modules`. Introduce Intelligence as an orchestration module without reversing that dependency:

```text
IXApi/src/Modules/Workflow/ProcessBuilding/
  canonical definition, rules, compiler, transactional builder, verifier

IXApi/src/Modules/Intelligence/
  agents, analysis, generation, automation, guarded endpoints

IXApi/src/Infrastructure/AI/
  generic abstractions, agents, providers, tools, documents, orchestration, models, memory, configuration

IXApi/Bootstrap or composition root
  registrations and options binding
```

Under `Workflow/ProcessBuilding`:

- versioned canonical definition DTOs and validation-result contracts
- `IWorkflowBlueprintValidator` for deterministic validation
- `IWorkflowBlueprintCompiler` for logical-key/code resolution
- `IWorkflowBuilderService` for one EF transaction
- `IWorkflowVerificationService` for stored-graph verification

Under `Modules/Intelligence`:

- `ProcessBuilderAgent` as a specialized consumer of the generic agent runtime
- `IProcessGenerationService` for converting analyzed requirements into a versioned Workflow blueprint
- `IProcessBuilderKnowledgeService` for authorized, tenant-filtered Workflow metadata and reference knowledge
- requirement analysis and evidence classification
- process-definition generation and revision
- `IWorkflowMetadataService`/`IWorkflowReferenceService` orchestration or ports
- clarification, blueprint-version, and approval-token workflows
- permissioned/rate-limited analyze, preview, and orchestration endpoints

Workflow owns `IProcessDefinitionValidator` (implemented by the deterministic blueprint validator) and `IProcessBuilderService` (the transactional creation boundary). These are core application capabilities, not AI adapters. Keep provider/model dependencies out of them so the same validated definition can later be created from AI, manual UI, import, trusted API, approved template, or authorized clone. Existing `IWorkflow*` names may be retained if they are the repository convention; the responsibility boundary is mandatory, not the exact class name.

Under `Infrastructure/AI`, place the approved generic platform, including its provider-neutral `Abstractions`. `Models` is limited to generic AI transport/execution contracts, `Tools` to generic tool registration/execution, and `Documents` to extraction contracts and implementations. Provider-specific folders such as `Providers/OpenAI` belong here. Process-specific agents, prompts, schemas, and tools remain in Intelligence/Workflow.

### Mandatory generic AI boundary

`Infrastructure/AI` must be completely generic. No file in that layer may reference Workflow namespaces, `Wf*` entities, `ProcessDefinition`, Process Builder prompts, workflow tools, or EF workflow persistence.

Generic contracts live in the approved `Infrastructure/AI/Abstractions` folder. They must remain provider-neutral and domain-neutral. The conceptual API is:

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

`TResponse` is a schema owned by the calling feature. The generic AI layer may serialize and validate structured output against that schema, but it must not know what a workflow process is.

Allowed generic contracts include:

- `IAIService`, `IAIProvider`, or an equivalent provider-neutral structured-output client
- `AIRequest`, `AIResult<TResponse>`, provider request/result envelopes, and generic error/usage metadata
- generic messages, content parts, tool descriptors, tool-call envelopes, and usage results
- `IDocumentContentExtractor` and technical extracted-content/page/region models
- `IAIAgentRegistry`, `IAIAgentOrchestrator`, `IAITool`, `IAIToolRegistry`, and `IDocumentExtractor`
- `IAIMemory` for bounded provider/domain-neutral context, subject to the approved memory policy
- provider configuration, resilience, timeouts, cancellation, telemetry, token/cost accounting, and generic safety limits

Business adapters live at:

```text
Modules/Intelligence/ProcessBuilder/
  Agents/
  Analysis/
  Generation/
  Automation/
  Prompts/
  Tools/
  Endpoints/
```

That adapter translates generic AI output into `WorkflowBlueprintDto` and invokes Workflow-owned validation/building contracts. Prompts named for Process Builder and tools such as workflow metadata lookup must never be placed in Infrastructure.

The platform uses DI-backed agent/tool registries and one central orchestrator. It supports authorization-filtered discovery, stable agent names, generic attachments, bounded conversation context, structured output, logging, usage, cancellation, and normalized failures without hardcoded agent switches. The generic foundation is validated first with domain-neutral test agents; `ProcessBuilderAgent` is then registered as a separate reference consumer. Collaboration/delegation and additional business agents are deferred.

Every agent has an `AgentDescriptor`: unique stable ID, localized name/description, accepted inputs, output schema/renderer key, allowed tools, optional provider/model policy, and authorization requirements. Duplicate IDs and invalid tool mappings fail during startup validation.

Agent and tool responsibilities are separate: agents reason and orchestrate, while tools expose narrow authorized application capabilities. No agent receives a generic database, SQL, command, filesystem, or unrestricted HTTP tool.

Use normalized safe error codes for provider/model availability, invalid structured output, missing/unauthorized agents or tools, tool failure, attachment limits/extraction, validation, rate limiting, timeout, and cancellation. Central options govern provider/model selection, timeouts, tool-call limits, attachment policy, and structured-output bounds; secrets use the established application secret mechanism.

Conversation history and long-term memory are separate replaceable stores. Version 1 may use transient conversation context. Any durable store requires approved retention, encryption, deletion, tenant isolation, and sensitive-content logging rules and must not persist provider-native objects as its primary model.

Production controls also include strict untrusted-content/prompt-injection boundaries; model/tool/context/token budgets and loop detection; provider-neutral model routing; quotas/cost accounting; transient-only retries and circuit breaking where justified; risk-classified tools with confirmation/idempotency metadata; future-compatible streaming events; agent/prompt/tool versioning; regression evaluations; data classification/redaction/provider eligibility; attachment signature/scanning/path safety; end-to-end correlation without chain-of-thought; tenant-safe caching; and feature flags for providers, agents, and tools.

The responsibility split is explicit:

- `IProcessGenerationService` — Intelligence; calls the generic AI service and produces/revises the canonical blueprint.
- `IProcessBuilderKnowledgeService` — exposes only authorized Workflow metadata/reference knowledge required for generation; never returns unrestricted tables or SQL.
- `IProcessDefinitionValidator` — Workflow; deterministic validation and rules, with no model calls.
- `IProcessBuilderService` — Workflow; compiles and atomically persists an approved valid definition, with no model calls.

Do not add empty `Modules/ProcessBuilder` or `Modules/Reports` projects just to mirror a proposed tree. Workflow currently owns process entities/lifecycle. Intelligence may be a new project referencing Workflow; Infrastructure may then reference Intelligence for implementations, preserving `Infrastructure → Intelligence → Workflow`.

Proposed project references:

```text
IAX.IXApi.Modules.Intelligence
  → IAX.IXApi.Shared
  → IAX.IXApi.Modules.Workflow
  → only other approved module contracts required for authorization/documents

IAX.IXApi.Infrastructure
  → owns the approved generic AI abstractions and implementations
  → IAX.IXApi.Modules.Intelligence only where existing composition conventions require it
  → existing module references

IAX.IXApi web host
  → Intelligence + Infrastructure for composition
```

Forbidden references:

```text
Workflow     ✕ Infrastructure
Intelligence ✕ Infrastructure
Workflow     ✕ Intelligence
```

This keeps the process engine reusable without AI and prevents provider code from leaking into business modules.

### Frontend integration

Extend `IXApp/src/modules/process-builder` with an `ai` feature containing typed APIs, blueprint-version state, chat/attachment UI, validation panels, and preview adapters. Do not persist AI output through existing frontend multi-save functions.

Use a responsive split workspace: input/chat/attachments and clarification on one side; generated definition, evidence, form/workflow preview, warnings, edit, and validation on the other. Stack it for smaller screens.

## Implementation phases

### Phase 0 — decisions and threat model

Decide the contracts-project dependency resolution, model provider/deployment, data residency/classification, retention, quotas/cost limits, attachment types/sizes/pages/signatures/scanning, session storage, feature-flag ownership, and—only for the later Process Builder workstream—performer policy, activation semantics, update policy, and reference visibility.

Acceptance: approved architecture decision record and threat model.

### Phase 1 — generic AI foundation

- Create the approved `Infrastructure/AI` folders and domain-neutral abstractions.
- Implement DI-backed agent/tool registries, central orchestrator, execution pipeline, normalized results, cancellation, errors, usage, and telemetry.
- Prove registration and execution with domain-neutral test agents; do not reference Workflow or Process Builder.
- Add architecture tests preventing domain types and provider SDK types from leaking across the generic contracts.
- Add startup validation for duplicate agents/tools, invalid descriptors, and missing tool bindings.
- Add execution policies for budgets/loop protection, model capabilities/routing, tool risks, confirmation, versioning, correlation, feature flags, and authorization.

Acceptance: a test agent can be added through registration without modifying AI Core, and the generic platform builds/tests without Workflow dependencies.

### Phase 2 — providers, documents, and bounded context

- Confirm/replace OpenAI bootstrap using typed options, health checks, secure secrets, cancellation, and timeouts.
- Add provider selection and provider-neutral structured-output handling.
- Add normalized platform errors and centralized bounded configuration without hardcoded secrets.
- Add quotas/usage, data classification/redaction/provider eligibility, transient-only resilience, and attachment signature/path/scanning safeguards.
- Add bounded PDF/image/text extraction and safe attachment preprocessing with source/page/region metadata.
- Use transient bounded conversation context; defer durable memory until its governance is approved.

Acceptance: provider substitution works behind `IAIProvider`; unsupported, oversized, malicious, or unauthorized files fail before model invocation.

### Phase 3 — Process Builder characterization and read model

- Capture current graph loading, tenant filtering, number sequences, print versions, and request submission in integration tests.
- Build an authorized read-only process-structure DTO including controls/options/validations, variables, mappings, steps, activities, performers, transitions, access, and print summaries.
- Add stable-code metadata catalog queries.
- Inspect process 590 only if it exists and is authorized.

Acceptance: complete definition reads contain no runtime records or cross-tenant data.

### Phase 4 — Process Builder blueprint contracts

- Define versioned `WorkflowBlueprintDto` and strict JSON schema using logical keys.
- Add size/depth/count/string limits.
- Define assumptions, warnings, errors, and missing-configuration contracts.
- Define one canonical schema: product-facing `ProcessDefinition`, implemented as the versioned `WorkflowBlueprintDto` contract.
- Add per-element evidence, source reference, short rationale, confidence, and `EXPLICIT` / `INFERRED` / `REQUIRES_CLARIFICATION` classification.
- Add Arabic/English round-trip fixtures.

Acceptance: new nodes link without predicted database identities.

### Phase 5 — Process Builder validator and compiler

- Validate metadata, codes, controls, options, validation rules, mappings, performers, access, and print documents.
- Validate graph reachability, cycles, completion/rejection routes, and transition compatibility.
- Compile logical keys and metadata codes into a deterministic persistence plan without writes.

Acceptance: invalid plans return structured errors and make zero writes.

### Phase 6 — Process Builder transactional builder

- Implement one backend aggregate command using EF execution strategy and transaction.
- Create inactive, resolve generated IDs, persist definition rows only, and verify.
- Add idempotency, concurrency protection, full rollback, and separate activation.
- Keep the builder independent of AI/provider contracts and cover direct service use with deterministic fixtures.

Acceptance: injected failure at every stage leaves no partial graph; retries do not duplicate.

### Phase 7 — Process Builder reference agent

- Expose only metadata/reference/validate/preview/create/verify tools.
- Require strict structured output and minimize context.
- Preserve blueprint versions across user edits and require approved-version tokens for create.
- Keep autonomy deliberately limited: the AI proposes and revises; application rules validate; the user approves; the service creates.
- Add architecture tests that fail if `Infrastructure/AI` references Workflow/Process Builder types or if business modules expose OpenAI/provider SDK types.

Acceptance: no SQL/generic mutation tool exists; malformed output and unauthorized calls are rejected.

### Phase 8 — Process Builder evidence ingestion

- Reuse authorized document storage and references.
- Consume the generic document-extraction contracts; keep Process Builder interpretation inside Intelligence.
- Keep extracted evidence distinct from AI conclusions and include source references where possible.
- Normalize TXT, PDF, image, screenshot, existing-form, and policy/procedure evidence into a bounded extraction contract.
- Retain page/region/paragraph references and only short relevant excerpts; do not store chain-of-thought.

Acceptance: unsupported, oversized, malicious, or unauthorized files fail before model invocation.

### Phase 9 — generic AI workspace and Process Builder view

- Add an authorization-filtered generic AI workspace driven by registered-agent metadata.
- Register the Process Builder result view as a specialized module-owned renderer.
- Add AI conversation and attachments to the existing builder where appropriate.
- Show blueprint versions, request form, graph, performers, access, print proposal, assumptions, warnings, and missing configuration.
- Show evidence, confidence, and explicit/inferred/clarification state for proposed elements.
- Block creation while material approval, performer, routing, threshold, security, or activation decisions require clarification.
- Support edit/regenerate/validate/create inactive/verify/separate activate actions.
- Add full Arabic RTL and English LTR localization.

Acceptance: both languages complete the guarded lifecycle without breaking manual builder use.

### Phase 10 — hardening and rollout

- Test permissions, tenant isolation, rollback, idempotency, prompt injection, file limits, cost/rate/time limits, cancellation, and observability.
- Regression-test manual Process Builder, request submission, mail, and printing.
- Release behind a feature flag with restricted create/activate permission.
- Add prompt/tool-injection suites, execution-loop tests, model-routing/quotas, safe retry/idempotency tests, agent/prompt/tool version regression evaluations, and streaming-contract compatibility tests.

Acceptance: security review and regression suite pass; rollback and monitoring are documented.

### Later convergence — shared creation engine

After the AI path is stable, evaluate migrating manual Process Builder creation, imports, templates, and cloning to the same backend builder contract. This is a later refactor, not a prerequisite for the guarded first release.

## Initial file proposal

```text
IXApi/src/Infrastructure/AI/
  Abstractions/
    IAIService.cs
    IAIProvider.cs
    IAIAgent.cs
    IAIAgentRegistry.cs
    IAIAgentOrchestrator.cs
    IAITool.cs
    IAIToolRegistry.cs
    IAIMemory.cs
    IAIExecutionPolicy.cs
    IAIModelResolver.cs
  Agents/
    AgentRegistry.cs
    AgentDescriptor.cs
    AgentContext.cs
    AgentRequest.cs
    AgentResult.cs
    AgentExecutionContext.cs
  Providers/
    OpenAI/
    AzureOpenAI/
    FutureProviders/
  Tools/
    ToolRegistry.cs
    ToolDescriptor.cs
    ToolExecutionContext.cs
    ToolResult.cs
    ToolExecutionService.cs
  Documents/
    IDocumentExtractor.cs
    DocumentExtractorFactory.cs
    PdfExtractor.cs
    ImageExtractor.cs
    TextExtractor.cs
  Orchestration/
    AIAgentOrchestrator.cs
    AgentSelector.cs
    AgentExecutionPipeline.cs
    ExecutionBudget.cs
    AIExecutionEvent.cs
  Models/
    AIRequest.cs
    AIResponse.cs
    AIMessage.cs
    AIAttachment.cs
    AIUsage.cs
    StructuredAIResponse.cs
  Memory/
    ConversationMemory.cs
    AgentMemory.cs
  Configuration/
    AIOptions.cs
    ProviderOptions.cs
  Security/
    AIDataClassificationPolicy.cs
    UntrustedContentPolicy.cs
  Evaluation/
    AIEvaluationCase.cs
    AIEvaluationRunner.cs
  Telemetry/
    AIExecutionTelemetry.cs

IXApp/src/modules/ai/
  api/ components/ hooks/ store/ types/

# Separate later reference-consumer workstream
IXApi/src/Modules/Workflow/ProcessBuilding/
  Contracts/ Validation/ Building/

IXApi/src/Modules/Intelligence/ProcessBuilder/
  Agents/ Analysis/ Generation/ Knowledge/ Automation/ Prompts/ Tools/ Endpoints/

IXApp/src/modules/process-builder/ai/
  specialized Process Builder result renderer/integration
```

Likely later modifications: `WorkflowModule.cs`, model-provider bootstrap/options, permission registration/seeding, Process Builder page/workspace, and Arabic/English translation resources.

## Principal risks

- partial persistence if current frontend CRUD sequence is reused
- cross-tenant metadata/reference leakage
- hallucinated IDs or unsupported metadata
- AI-generated SQL entering performer SQL fields
- duplicate creation on retries
- incorrect transition compilation
- untrusted document and prompt injection
- silent activation or active-process overwrite
- browser-only drafts being mistaken for durable audit history
- schema inconsistencies being treated as intended behavior

## Definition of done

### Generic AI Core V1

The generic platform works with a neutral test agent and no Workflow dependency. At least one replaceable provider, typed structured output, authorized agent discovery, safe read-only tools, validated TXT/PDF/image paths, cancellation, budgets/loop protection, normalized errors, correlation/usage telemetry, feature flags, Arabic/English generic React workspace, mock-provider tests, prompt/tool-injection tests, tenant/security tests, and architecture-boundary tests pass. No API key or secret exists in source control.

### Later Process Builder reference consumer

After separate approval, an authorized user can provide Arabic/English requirements and approved attachments, review a versioned blueprint, receive deterministic validation, atomically create one inactive definition, verify it, and explicitly activate it—with no arbitrary SQL, runtime-row creation, cross-tenant access, partial persistence, or regressions. This is not part of AI Core completion.
