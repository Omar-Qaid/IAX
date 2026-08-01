# Document Page

## Purpose
A generic, unified wrapper for Header/Lines and other transactional entities that require lifecycle management and status badges.

## When to use
- When dealing with complex business documents that undergo state transitions (Draft → Approved → Posted).

## Folder structure
```text
src/patterns/document/
├── DocumentPage.tsx           # Main wrapper component
```

## Required components
```text
DocumentPage
├── Status Badge
├── Document ActionPane
└── Body Regions
```

## Data flow
```text
Similar to Header Lines, relies on `useDocumentPage` hook for dirty tracking and API calls.
```

## Examples
Generic Journal Entry.

## Rules
- Always use `DocumentPage` for entities with a workflow status.

## Description UI
Visually similar to Header Lines but can be adapted for non-line based documents. The distinguishing feature is the prominent Status Badge in the PageHeader and the workflow-oriented ActionPane (Submit, Approve, Reject buttons) that clearly indicate the document is in a process pipeline.
