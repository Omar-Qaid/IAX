/**
 * Generic report-designer contracts and state helpers.
 *
 * Rendering compositions belong to patterns, while API access and business
 * mappings remain in their owning module.
 */

// Types
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
} from './types';
export { createEmptyPrintTemplateDocument } from './types';

// Designer hook
export type { TemplateRegion, DesignerComponentType } from './useReportDesigner';
export { useReportDesigner, createDesignerElement } from './useReportDesigner';
