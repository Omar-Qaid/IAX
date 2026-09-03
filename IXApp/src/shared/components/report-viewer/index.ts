/**
 * Centralized Generic Print Viewer & Runtime Engine
 *
 * Public API surface for the shared viewer engine. Import from this barrel
 * to access runtime rendering, data formatting, and pagination components.
 */

// Runtime renderer
export {
  ReportTemplateRenderer,
  formatPrintValue,
  requestControlBodyElements,
  requestControlBodyBindings,
  requestControlTemplateElements,
  requestControlTemplateBindings,
} from './ReportTemplateRenderer';

// Runtime data
export type { runtimeReportData } from './runtimeReportData';
export { resolveRuntimeBinding, formatRequestControlValue } from './runtimeReportData';

// Template selection
export {
  selectPublishedTemplates,
  selectDefaultPublishedTemplate,
} from './publishedTemplateSelection';

// Document layout and pagination
export { ReportViewerDocument, type reportCompany, type PrintoutMetadataItem, type PrintoutHeaderConfig, type PrintoutFooterConfig, type PrintoutPageSettings, type ReportViewerDocumentProps, PrintoutHeader, PrintoutFooter } from './ReportViewerDocument';
export { ReportViewerPaginationProvider, useReportViewerPagination } from './ReportViewerPaginationContext';
