import type React from 'react';

export const REPORT_EXPORT_FORMATS = [
  'PDF',
  'Excel',
  'Word',
  'CSV',
  'XML',
  'MHTML',
  'TIFF',
] as const;

export type ReportExportFormat = (typeof REPORT_EXPORT_FORMATS)[number];
export type ReportZoomMode = 'Automatic Zoom' | 'Page Width' | 'Whole Page' | '100%';
export type ReportDirection = 'ltr' | 'rtl' | 'auto';

export interface ReportViewerOptions {
  pageHeight?: number;
  /** Ignores small screen/print layout differences when deriving local page count. */
  pageOverflowTolerance?: number;
  initialZoomMode?: ReportZoomMode;
  initialThumbnailsOpen?: boolean;
  minZoom?: number;
  maxZoom?: number;
  zoomStep?: number;
  direction?: ReportDirection;
}

/**
 * Controlled pagination keeps report data fetching outside the viewer. A report can
 * load only the requested server page and pass its rendered page back as children.
 */
export interface ReportViewerPagination {
  currentPage: number;
  totalPages: number;
  loading?: boolean;
  onPageChange: (page: number) => void | Promise<void>;
  onPrefetchPage?: (page: number) => void | Promise<void>;
  renderThumbnail?: (page: number, selected: boolean) => React.ReactNode;
}

export interface ReportPageRequest {
  pageNumber: number;
  pageSize: number;
  signal?: AbortSignal;
}

export interface ReportPageResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

/** Data-source contract for report implementations backed by server-side pages. */
export interface ReportPagedDataSource<T> {
  pageSize: number;
  loadPage: (request: ReportPageRequest) => Promise<ReportPageResult<T>>;
}

export interface ReportViewerProps {
  open: boolean;
  title: string;
  variant?: 'embedded' | 'dialog';
  children?: React.ReactNode;
  loading?: boolean;
  error?: string | null;
  emptyMessage?: string;
  exportFormats?: readonly ReportExportFormat[];
  /** @deprecated Prefer viewerOptions.pageHeight. */
  pageHeight?: number;
  viewerOptions?: ReportViewerOptions;
  pagination?: ReportViewerPagination;
  onClose: () => void;
  onReload?: () => void | Promise<void>;
  onPrint: () => void;
  onExport: (format: ReportExportFormat) => void;
}
