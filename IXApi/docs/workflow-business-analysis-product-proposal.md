# Business Analysis & Product Evolution Proposal

## Unified Enterprise Workflow & Business Process Management Platform

Date: 2026-09-10  
Status: Business requirements and product proposal for review, not a statement of completed capabilities.

## Scope and Evidence

This document focuses on business, users, rules, operations, governance, reporting, and product evolution. It does not prescribe technical architecture or implementation details.

The current-state assessment draws on the earlier project review. It is not based on interviews with every department or measured production performance.

The following distinctions apply throughout:

- **Observed foundations:** Configurable processes and forms, request persistence, some history and document presentation, and notification and reporting capabilities.
- **Incomplete in the reviewed submission journey:** Automatically routing submitted requests to responsible people and progressing their decisions through to closure.
- **Target requirements:** AND/OR conditions, child requests using different processes, a unified parent-and-child page, and end-to-end process governance.

## 1. Business Vision

Provide one platform through which departments define their services and manage requests from initiation to outcome, with clear responsibility, understandable decisions, and expected completion times.

The intended business value is to:

- Reduce manual follow-up and requests lost between departments.
- Reduce the effort required to introduce a new procedure through understandable configuration.
- Standardize the experience of submitting, tracking, and deciding requests.
- Improve accountability and the quality of information and supporting documents.
- Help management improve procedures using reliable, interpretable measures.

Improvement percentages must not be invented as commitments. Establish baseline completion time, return rates, and overdue rates, then agree measurable pilot targets with each process owner.

## 2. Current Business Concept

An organizational procedure is represented as a reusable process with its own form, stages, activities, responsible parties, rules, and outcomes. Examples include leave, purchasing, license renewal, inspection, and financial requests.

**Strengths:** Flexible forms, organization around process definitions, Arabic and English support, and opportunities to reuse request information in search and reporting.

**Principal limitation:** Defining a procedure and saving a request do not establish a complete business journey. The platform must also deliver work to the appropriate person, apply decisions, handle exceptions, and close the request with a clear outcome.

A process is a reusable definition. A request is one instance of that process. A task is a responsibility within a request. Completing one task does not necessarily complete the request.

## 3. Core Business Capabilities

The platform should support a connected service journey:

1. Discover a service and understand its requirements before submitting.
2. Provide valid information and complete supporting documents.
3. Determine routing and responsibility using approved rules.
4. Perform tasks, record decisions, and track progress.
5. Handle returns, delays, absence, and exceptions.
6. Close the request with an outcome and inform the relevant people.
7. Search, audit, measure, and improve the process.

Product value should be measured by the completeness of this journey, not only by the number of form configuration options.

## 4. Business Process Lifecycle

### Process Definition Lifecycle

Idea → Process analysis → Draft configuration → Scenario testing → Review and approval → Publication → Performance monitoring → Improvement and a new version → Retirement from new submissions when appropriate.

### Request Lifecycle

Draft → Submitted → In progress → Waiting when necessary → Closed with an outcome.

A request may return for clarification or correction. Cancellation and withdrawal require permission, a reason, and an explicit policy governing work already underway.

Distinguish three concepts:

- **Status:** Where is the request in its journey?
- **Outcome:** Approved, rejected, cancelled, or another approved business result.
- **Waiting reason:** A person, document, external party, child request, or scheduled date.

An inspection being completed does not mean it passed. A task being closed does not necessarily mean approval was granted.

## 5. User Roles / Personas

- **Requester:** Understands eligibility, provides information, resolves missing items, and sees required actions and expected outcomes.
- **Task performer or approver:** Receives a prioritized worklist, sufficient information, and clear decision options.
- **Team supervisor:** Monitors workload and delays and reallocates work within their authority.
- **Process owner:** Owns the purpose, rules, outcomes, and performance measures of a process.
- **Process designer:** Translates business requirements into understandable forms, stages, and rules.
- **Change approver:** Reviews proposed changes and authorizes publication independently where required.
- **Operations administrator:** Resolves stalled work without independently changing business decisions.
- **Auditor:** Reviews history, reasons, and delegations without altering them.
- **Management:** Reviews service performance within its authorized scope.

One person may hold multiple roles in a smaller organization. Separation between configuring and approving changes should reflect the sensitivity of the process.

## 6. Workflow Configuration Capabilities

An authorized administrator should be able to define:

- Service name, description, eligibility, and owning department.
- Field types, allowed values, guidance, required fields, and conditional visibility.
- Information used for search, reporting, and routing decisions.
- Stages, activities, ordering, and available decisions.
- Responsibility rules, expected durations, and required documents.
- Routing conditions, messages, and automatic initiation rules.
- When child requests are created and how their outcomes affect the parent.

Preview and scenario testing should be available before publication. Validation messages should use business language, such as “This route has no responsible party” or “This comparison is not valid for the selected field type.”

Shared definitions should be reusable through approved templates. Updating a shared template must not silently change approved processes; the impact should be visible and reviewed.

## 7. Request & Task Management

Requesters need a reference number, understandable status, latest action, current responsibility where disclosure is permitted, expected completion date, and a clear indication of anything required from them.

Performers need a “My Tasks” view distinguishing new, overdue, returned, and waiting work, with a reason for priority. Business priority and request age should remain distinct so that every request does not become “urgent.”

### Parent and Child Requests

A license request may create an inspection request and a payment request, each following a different process. Business configuration determines which information is passed to the child, which outcomes are returned, and whether the parent waits for a particular child, all required children, or continues independently.

The request family should be visible on one page: the parent header, details, and stages, followed by expandable child sections containing each child's information, decisions, documents, and progress. Multiple children may remain expanded for comparison. Every action must clearly identify the request it affects.

Access to a parent does not automatically grant access to its children. A completed parent does not imply completion of children that were explicitly permitted to continue independently.

## 8. Business Rules & Routing

Rules determine the route using information such as amount, branch, request type, and results of earlier activities.

- **All conditions — AND:** Every condition must match.
- **Any condition — OR:** At least one condition must match.
- Nested groups should allow combinations with explicit meaning.

Example: “The amount is above ten thousand, and the department is either Finance or Legal.”

Configuration must define route priority when several rules match and the outcome when none match. These decisions must not be hidden behind an unexplained default.

Routing conditions are different from requiring all approvers to respond, and from waiting for all child requests. These policies should have separate business labels.

A proposed “Why this route?” feature should explain the decision appropriately without exposing confidential information.

## 9. Approvals & Decisions

Available decisions may include approval, rejection, return for correction, request for information, forwarding, or a specialized result such as compliant/noncompliant.

Each decision should define who may take it, whether a reason or document is required, its effect on the request, and whether correction is possible and under whose authority.

Approval patterns include:

- **Sequential:** Each stage waits for the preceding stage.
- **Parallel:** Several parties work at the same time.
- **Conditional:** An additional approver participates when a rule applies.
- **All designated approvers or one authorized responder:** According to the activity's explicit policy.

A voting threshold or required number of approvals can be introduced later when justified. Early rejection, ties, and nonresponse require defined outcomes.

Sensitive processes may prohibit self-approval, provide authorized substitutes during absence, and record who acted on behalf of whom. Forwarding is not approval, and reassignment must preserve the previous responsibility and transfer history.

## 10. Notifications & Escalations

Notify users about meaningful events: a new task, missing information, a decision, an approaching deadline, an overdue task, or request completion.

A message should contain the request reference, an appropriate summary, required action, deadline, and a way to access the work. Language and channel should follow process policy and user preferences where permitted.

Escalation is more than repeating a message. It may notify a supervisor, reallocate a task, or raise the approval level. Delay must not automatically imply approval unless an explicit business policy permits it.

An SLA needs an agreed meaning: elapsed hours or working hours, whether time pauses while waiting for the requester, and how holidays and absence are handled. Show the expected deadline clearly before assessing whether someone missed it.

## 11. Audit & Governance

Every process should have an owner, reviewer, effective date, and approved version. A change proposal should identify its reason, expected impact, affected procedures, and treatment of open requests.

The history should show who created, changed, decided, forwarded, or delegated work; when and why; the resulting outcome; and the approved procedure followed by the request.

Proposed governance rules:

- Do not silently change an open request's route after its process definition changes.
- Preserve the original decision when recording a correction and its reason.
- Require authority and justification for exceptional administrative actions.
- Stopping new submissions must not automatically cancel existing work.
- Define document confidentiality and retention according to organizational policy.
- Periodically review permissions, templates, and unused rules.

These are proposed governance requirements, not a legal compliance assessment.

## 12. Dynamic Reporting & KPIs

Authorized users should be able to configure filters, grouping, ordering, and measures using process fields. Metric definitions, visibility, and export permissions must remain consistent.

Proposed definitions:

- **Submitted requests:** Requests that completed submission during the period, with an explicit child-request counting policy.
- **Open backlog:** Requests not closed at the measurement time, rather than all requests submitted during the period.
- **Completion time:** Time from accepted submission to closure, with excluded waiting time reported separately according to policy.
- **SLA compliance:** The proportion of eligible cases completed by their agreed deadline, with the measurement population defined.
- **Return-for-information rate:** The proportion of requests returned at least once, with return reasons.
- **Overdue tasks:** Open tasks past their agreed deadline; distinct from overdue requests.

Show the median alongside the average where unusually long cases distort results. Do not combine amounts in different currencies without a declared conversion policy. Do not double-count a parent and its children in a single-service measure without explaining the counting basis.

Use performance information to improve processes, accounting for workload and complexity before comparing individuals or departments.

## 13. Automation & Triggers

Requests may start manually, at a renewal or expiry date, after a business event, or following another request's outcome. Automatically initiated requests must still have a business owner, visible initiation reason, and the same applicable eligibility, decision, and document policies.

Each automation should identify its trigger, timing, target population, exceptions, and failure owner. Repeated events or retries must not create duplicate requests for the same business obligation.

Missing required information should lead to an exception or completion-of-information workflow. Starting an attempt alone must not be presented as successful processing.

## 14. Current Gaps

### Findings from the Earlier Review

- The reviewed submission journey does not yet provide complete routing, assignment, decision progression, and closure.
- Compound conditions and child requests remain target capabilities.
- Policies for missing responsibility, returns, waiting, and parallel work need agreement.
- A designer option does not prove that the corresponding behavior is enforced throughout the request journey.
- Input validation and the interpretation of some data types are inconsistent.
- History needs clearer treatment of parallel work, repeated stages, and business outcomes.
- This review provides no measured baseline for completion time, adoption, or service quality.

### Focused Workflow/BPM Capability Comparison

This is a comparison of business capabilities and direction, not a procurement assessment or a claim of equivalence with a particular product.

**A shared process language:** BPMN provides a visual notation for business processes intended to be understandable to business users. The current concept of stages and rules is a suitable starting point; the product should make branching, waiting, and parallel work explicit without requiring every user to learn the full notation. [OMG BPMN overview](https://www.omg.org/bpmn/).

**Different approval policies:** Microsoft documents sequential approvals and approval flows requiring all assigned approvers. The proposed platform should expose these as explicit business policies rather than only listing recipients. [Sequential approvals](https://learn.microsoft.com/en-us/power-automate/set-up-sequential-approvals), [Everyone must approve](https://learn.microsoft.com/en-us/power-automate/all-assigned-must-approve).

**Accessible decision-making:** Microsoft documents responding through email, the approvals center, or its application. The product implication is to reduce the effort required to reach and understand a task. Additional response channels should follow agreed identity and confidentiality policies. [Approval experience](https://learn.microsoft.com/en-us/power-automate/modern-approvals).

**Operational maturity:** Evaluate the proposed platform against clear ownership, waiting reasons, exception handling, controlled changes, and trustworthy reporting. This is a recommended internal assessment framework, not a verified feature list for every BPM product.

## 15. Recommended Improvements

Prioritize completing one service journey before expanding the range of configuration options:

1. Standardize status, outcome, and waiting reason.
2. Provide a service catalog explaining eligibility, documents, and expected duration before submission.
3. Prevent avoidable omissions and allow users to continue incomplete work without starting again.
4. Make rules explainable and testable before publication.
5. Give every task clear ownership, a deadline, and a required decision.
6. Provide an authorized, coherent parent-and-child history.
7. Make exceptions visible and assign responsibility for resolving them.

Every improvement should have a business owner and acceptance criteria. A configuration screen alone does not make a capability complete.

## 16. New Business Features

- **“What do I need to do now?”:** A concise action list across requests and tasks, reducing search and follow-up effort.
- **Submission readiness check:** Explain missing information, documents, and eligibility issues before submission to reduce returns.
- **Route and waiting explanations:** Show why work reached a particular party and what prevents progress, within the viewer's permissions.
- **Exception center:** Bring together unassigned, overdue, or information-blocked requests with an accountable resolution owner.
- **Approved service templates:** Reuse leave, financial approval, or inspection patterns instead of designing every process from scratch.
- **Process change impact review:** Show what will change and who will be affected before publication.
- **Return-reason analysis:** Identify confusing fields and instructions that repeatedly cause incomplete submissions.
- **Historical completion estimates:** A later enhancement offering an estimate rather than a guarantee, once reliable data exists.

AI-based autonomous approval is outside the core scope. Assistance with summarizing requests or suggesting improvements may be evaluated later while keeping decision authority and accountability explicit.

## 17. Core vs Optional vs Future Features

### Core to the Target Product

Process and form definition, eligibility and permissions, submission and tracking, AND/OR rules, assignment and decisions, basic sequential and parallel work, documents, notifications, history, essential reporting, Arabic and English, publication governance, and necessary exception handling.

Different-process child requests and the unified family page are **core requirements of the user's stated scope**, although they may be released after the ordinary request lifecycle is complete. Phased delivery does not remove them from scope.

### Optional According to Department Needs

Advanced approval thresholds, complex delegation, multiple business calendars, additional approval channels, specialized print layouts, and creating child requests for individual request items.

### Future Enhancements

Completion-time prediction, analysis of actual process journeys, improvement recommendations, capacity planning, and collaboration with external organizations if that scope is approved.

This prioritization is a proposal for business-owner agreement. It does not imply that every core capability belongs in the first release.

## 18. Proposed Business Modules

- **Service catalog:** Discover procedures, eligibility, expected duration, and required documents.
- **Process design and governance:** Define, test, review, publish, and version procedures.
- **Request submission and tracking:** Manage drafts, missing information, and parent-and-child requests.
- **Tasks and decisions:** Manage assignment, priority, execution, and forwarding.
- **Service operations:** Monitor deadlines, delays, exceptions, and workload.
- **Documents and correspondence:** Manage evidence, comments, and authorized communications.
- **Audit and oversight:** Review decisions, delegations, and changes.
- **Reporting and improvement:** Measure service performance and investigate delays and returns.
- **Automation and obligations:** Manage business events, renewals, scheduled work, and automatic follow-up.

These are functional areas for organizing the product and ownership, not a requirement to create a separate application for each area.

## 19. Target Enterprise Workflow Model

Organizational service → Approved process definition → Request with information and evidence → Rule-based routing → Tasks assigned to responsible parties → Recorded decisions → Further stages or child requests → Clear outcome → Measurement and improvement.

Example of a complete journey:

1. An employee submits a renewal request after reviewing eligibility and document requirements.
2. The platform confirms readiness and presents the reference and status.
3. Rules determine whether additional approval and an inspection child request are needed.
4. Inspection follows its own process and is visible under the parent request.
5. If the inspector needs information, the requester sees exactly what is required and which request it concerns.
6. The inspection outcome returns and the agreed parent policy determines continuation, return, or reasoned rejection.
7. The authorized decision-maker records the final outcome and relevant documents become available to permitted users.
8. Management measures the journey and reasons for delay without unintentionally counting the child as a separate service request in the same metric.

Success means the requester, supervisor, and auditor can explain the journey, not merely observe changing status labels.

## 20. Recommended Product Roadmap

### Phase 1: Agree Definitions and Scope

Select two or three representative services, consult their owners, and define roles, decisions, deadlines, exceptions, and baseline measures.

**Deliverables:** Agreed requirements and rules, pilot objectives, and an open decision register.

**Exit criterion:** Business owners can explain when a request starts and ends, who decides, and what happens in exception cases.

### Phase 2: Complete the Ordinary Request Journey

Deliver submission, documents, assignment, decisions, returns, closure, notifications, and history with permissions, language support, and a clear user experience.

**Exit criterion:** Representative cases can finish without off-platform follow-up, and failures can be resolved without duplicate requests or loss of history.

### Phase 3: Flexible Routing and Parallel Work

Introduce compound rules, scenario testing, explicit sequential/parallel approval policies, and better waiting explanations.

**Exit criterion:** Multiple matching routes, no matching route, and unavailable approvers have agreed and explainable outcomes.

### Phase 4: Request Families

Introduce children using different processes, explicit inputs and outcomes, waiting and cancellation policies, and the unified family page.

**Exit criterion:** Child completion, rejection, cancellation, and delay produce understandable parent behavior without unauthorized disclosure.

### Phase 5: Expanded Governance, Automation, and Measurement

Expand version and change management, automatic initiation, escalation, trustworthy reporting, and process-owner performance reviews. Basic governance starts in Phase 1 and is not deferred until this phase.

**Exit criterion:** Operational ownership and metric definitions are agreed, and a new procedure can be launched through approved configuration and documented business testing.

### Phase 6: Value-Driven Improvements

Add advanced delegation, prediction, and bottleneck analysis according to adoption evidence and expected benefit. Do not commit to final dates or costs before scope, resources, and dependencies have been assessed.

### Open Business Decisions

- Which services enter the pilot, and who owns their success?
- When may a submitted request be withdrawn or corrected?
- Does rejection close a request permanently or allow a linked resubmission?
- Which waiting periods pause the SLA?
- What happens when the responsible person is unavailable or has a conflict of interest?
- When must a parent wait for a child, and how does cancellation affect the other requests?
- Do open requests continue under their original procedure when a change is published?
- What viewing, export, and retention policies apply to each role?

Resolve these questions with business stakeholders. The platform should not silently choose an answer merely because it is easier to implement.
