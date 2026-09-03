/**
 * Workflow Print Template Types
 *
 * Re-exports all generic print engine types from the centralized shared engine,
 * plus any Workflow-specific type extensions. This barrel ensures backward
 * compatibility — all existing Workflow imports continue to work unchanged.
 */

// Re-export everything from the centralized engine
export type {
  PrintTemplateLanguage,
  PrintTemplateDirection,
  PrintTemplateOrientation,
  PrintTemplateStatus,
  PrintConditionValue,
  PrintTemplateDocument,
  PrintFieldBinding,
  PrintVisibilityCondition,
  PrintValueFormat,
  PrintElementStyle,
  PrintTextElement,
  PrintFieldElement,
  PrintSectionElement,
  PrintRowElement,
  PrintColumnElement,
  PrintDividerElement,
  PrintImageElement,
  PrintTableColumn,
  PrintTableElement,
  PrintWorkflowApprovalElement,
  PrintSignatureElement,
  PrintQrCodeElement,
  PrintBarcodeElement,
  PrintAttachmentElement,
  PrintPageNumberElement,
  PrintDateElement,
  PrintSpacerElement,
  PrintPageBreakElement,
  PrintTemplateElement,
  ReportDesignerSummary,
  PrintTemplate,
  PublishedPrintTemplate,
  SavePrintTemplateInput,
} from '@shared/components/report-designer';

export { createEmptyPrintTemplateDocument } from '@shared/components/report-designer';
