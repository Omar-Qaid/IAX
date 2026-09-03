/**
 * Centralized Generic Print Template & Report Engine
 *
 * Public API surface for the shared print engine. Import from this barrel
 * to access types, runtime rendering, designer hook, element preview,
 * and template selection utilities from any module.
 *
 * @example
 * ```ts
 * import {
 *   useReportDesigner,
 *   type PrintTemplateDocument,
 * } from '@shared/components/report-designer';
 * ```
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

// Element preview
export { ReportDesignerElementPreview } from './ReportDesignerElementPreview';

// API
export { reportDesignerApi } from './api/reportDesignerApi';
