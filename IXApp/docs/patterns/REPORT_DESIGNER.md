# Report Designer

`src/patterns/report-designer/ReportDesigner.tsx` is an implemented, domain-neutral workspace shell for visual report designers. It owns the bounded designer surface, toolbar row, workspace row, overflow containment, and accessible region semantics.

The pattern deliberately does not own a report document schema, component palette, field lookup, property editor, renderer, or persistence. Those remain with the consuming feature. Workflow Print Templates composes the pattern while retaining its existing `PrintTemplateDocument`, request-control bindings, designer hook, runtime renderer, and API contracts.

Use this pattern when a feature needs a visual report/template editing workspace. Use [`report-viewer`](../../src/patterns/report-viewer/README.md) for preview, export, and print behavior.
