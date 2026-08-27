export type PrintTemplateLanguage = 'en' | 'ar';
export type PrintTemplateDirection = 'ltr' | 'rtl';
export type PrintTemplateOrientation = 'portrait' | 'landscape';
export type PrintTemplateStatus = 0 | 1 | 2 | 'draft' | 'published' | 'archived';
export type PrintConditionValue = string | number | boolean | null | readonly (string | number | boolean)[];

export interface PrintTemplateDocument {
  schemaVersion: 1;
  language: PrintTemplateLanguage;
  direction: PrintTemplateDirection;
  page: {
    size: 'A4' | 'Letter';
    orientation: PrintTemplateOrientation;
    margins: { top: number; right: number; bottom: number; left: number };
  };
  header: PrintTemplateElement[];
  sections: PrintTemplateElement[];
  footer: PrintTemplateElement[];
  missingFieldBehavior: 'empty' | 'na' | 'placeholder';
}

export interface PrintFieldBinding {
  sourceType: 'system' | 'company' | 'requestControl' | 'workflow' | 'attachment' | 'repeating';
  source?: string | null;
  requestControlId?: number | null;
  controlId?: number | null;
  stepId?: number | null;
}

export interface PrintVisibilityCondition {
  field: PrintFieldBinding;
  operator: '=' | '!=' | '>' | '>=' | '<' | '<=' | 'contains' | 'notContains' | 'isEmpty' | 'isNotEmpty' | 'in' | 'notIn';
  value?: PrintConditionValue;
}

export interface PrintValueFormat {
  type: 'text' | 'date' | 'dateTime' | 'number' | 'currency' | 'percentage' | 'boolean';
  pattern?: string | null;
  currency?: string | null;
  trueText?: string | null;
  falseText?: string | null;
}

export interface PrintElementStyle {
  width?: number | null;
  fontSize?: number | null;
  fontWeight?: number | null;
  alignment?: 'start' | 'center' | 'end' | null;
  color?: string | null;
  backgroundColor?: string | null;
  keepTogether?: boolean;
}

interface PrintElementBase {
  id: string;
  visibleWhen?: PrintVisibilityCondition | null;
  style?: PrintElementStyle | null;
}

export interface PrintTextElement extends PrintElementBase { type: 'text'; value: string }
export interface PrintFieldElement extends PrintElementBase { type: 'field'; label: string; binding: PrintFieldBinding; format?: PrintValueFormat | null; fallback?: string | null }
export interface PrintSectionElement extends PrintElementBase { type: 'section'; title?: string | null; columns: number; elements: PrintTemplateElement[] }
export interface PrintRowElement extends PrintElementBase { type: 'row'; elements: PrintTemplateElement[] }
export interface PrintColumnElement extends PrintElementBase { type: 'column'; span: number; elements: PrintTemplateElement[] }
export interface PrintDividerElement extends PrintElementBase { type: 'divider' }
export interface PrintImageElement extends PrintElementBase { type: 'image'; sourceType: string; binding?: PrintFieldBinding | null; source?: string | null; altText?: string | null }
export interface PrintTableColumn { id: string; label: string; field: string; format?: PrintValueFormat | null; width?: number | null }
export interface PrintTableElement extends PrintElementBase { type: 'table'; dataSource: PrintFieldBinding; columns: PrintTableColumn[]; repeatHeader: boolean }
export interface PrintWorkflowApprovalElement extends PrintElementBase { type: 'workflowApproval'; stepId: number; showName: boolean; showJobTitle: boolean; showStatus: boolean; showDate: boolean; showComment: boolean; showSignature: boolean }
export interface PrintSignatureElement extends PrintElementBase { type: 'signature'; binding: PrintFieldBinding; label?: string | null }
export interface PrintQrCodeElement extends PrintElementBase { type: 'qrCode'; binding: PrintFieldBinding }
export interface PrintBarcodeElement extends PrintElementBase { type: 'barcode'; binding: PrintFieldBinding; format: string }
export interface PrintAttachmentElement extends PrintElementBase { type: 'attachment'; binding?: PrintFieldBinding | null; imagesOnly: boolean }
export interface PrintPageNumberElement extends PrintElementBase { type: 'pageNumber' }
export interface PrintDateElement extends PrintElementBase { type: 'printDate' }
export interface PrintSpacerElement extends PrintElementBase { type: 'spacer'; height: number }
export interface PrintPageBreakElement extends PrintElementBase { type: 'pageBreak' }

export type PrintTemplateElement =
  | PrintTextElement | PrintFieldElement | PrintSectionElement | PrintRowElement | PrintColumnElement
  | PrintDividerElement | PrintImageElement | PrintTableElement | PrintWorkflowApprovalElement
  | PrintSignatureElement | PrintQrCodeElement | PrintBarcodeElement | PrintAttachmentElement
  | PrintPageNumberElement | PrintDateElement | PrintSpacerElement | PrintPageBreakElement;

export interface PrintTemplateSummary {
  templateId: number;
  processId: number;
  processName: string;
  code: string;
  name: string;
  description: string | null;
  pageSize: string;
  orientation: PrintTemplateOrientation;
  language: PrintTemplateLanguage;
  isDefault: boolean;
  status: PrintTemplateStatus;
  currentVersionId: number | null;
  currentVersionNo: number | null;
  latestVersionNo: number;
  hasDraft: boolean;
  isActive: boolean;
  lastModifiedAt: string | null;
}

export interface PrintTemplate extends PrintTemplateSummary {
  editableVersionId: number;
  editableVersionNo: number;
  editableVersionPublished: boolean;
  document: PrintTemplateDocument;
  versions: Array<{ templateVersionId: number; versionNo: number; isPublished: boolean; publishedBy: string | null; publishedAt: string | null; createdAt: string | null }>;
}

export interface SavePrintTemplateInput {
  processId?: number;
  code: string;
  name: string;
  description: string | null;
  isDefault: boolean;
  document: PrintTemplateDocument;
}

export const createEmptyPrintTemplateDocument = (language: PrintTemplateLanguage): PrintTemplateDocument => ({
  schemaVersion: 1,
  language,
  direction: language === 'ar' ? 'rtl' : 'ltr',
  page: { size: 'A4', orientation: 'portrait', margins: { top: 15, right: 15, bottom: 15, left: 15 } },
  header: [],
  sections: [],
  footer: [],
  missingFieldBehavior: 'empty',
});
