import React from 'react';
import { Box, Dialog, useMediaQuery, useTheme } from '@mui/material';
import { ReportPreviewArea } from './ReportPreviewArea';
import { ReportThumbnailPanel } from './ReportThumbnailPanel';
import { ReportToolbar } from './ReportToolbar';
import { PrintoutPaginationProvider } from '@shared/components/printout/PrintoutPaginationContext';
import { REPORT_EXPORT_FORMATS, type ReportExportFormat, type ReportViewerProps, type ReportZoomMode } from './types';

export { REPORT_EXPORT_FORMATS } from './types';
export type { ReportDirection, ReportExportFormat, ReportPageRequest, ReportPageResult, ReportPagedDataSource, ReportViewerOptions, ReportViewerPagination, ReportViewerProps, ReportZoomMode } from './types';

export const calculateReportPageCount = (documentHeight: number, pageHeight: number, overflowTolerance = 0.12): number => {
  const safePageHeight = Math.max(1, pageHeight);
  const safeTolerance = Math.max(0, Math.min(0.49, overflowTolerance));
  return Math.max(1, Math.ceil(documentHeight / safePageHeight - safeTolerance));
};

export function ReportViewer({ open, title, variant = 'embedded', children, loading = false, error = null, emptyMessage = 'No report is available.', exportFormats = REPORT_EXPORT_FORMATS, pageHeight: legacyPageHeight, viewerOptions, pagination, onClose, onReload, onPrint, onExport }: ReportViewerProps): React.ReactElement {
  const theme = useTheme();
  const compact = useMediaQuery(theme.breakpoints.down('md'));
  const viewerRef = React.useRef<HTMLDivElement | null>(null);
  const scrollRef = React.useRef<HTMLDivElement | null>(null);
  const reportRef = React.useRef<HTMLDivElement | null>(null);
  const pageHeight = viewerOptions?.pageHeight ?? legacyPageHeight ?? 1123;
  const pageOverflowTolerance = viewerOptions?.pageOverflowTolerance ?? 0.12;
  const minZoom = viewerOptions?.minZoom ?? 40;
  const maxZoom = viewerOptions?.maxZoom ?? 180;
  const zoomStep = viewerOptions?.zoomStep ?? 10;
  const [thumbnailsOpen, setThumbnailsOpen] = React.useState(viewerOptions?.initialThumbnailsOpen ?? !compact);
  const [localPage, setLocalPage] = React.useState(1);
  const [localTotalPages, setLocalTotalPages] = React.useState(1);
  const [zoom, setZoom] = React.useState(100);
  const [zoomMode, setZoomMode] = React.useState<ReportZoomMode>(viewerOptions?.initialZoomMode ?? 'Automatic Zoom');
  const totalPages = Math.max(1, pagination?.totalPages ?? localTotalPages);
  const currentPage = Math.min(totalPages, Math.max(1, pagination?.currentPage ?? localPage));
  const prefetchPage = pagination?.onPrefetchPage;
  const pagedCurrentPage = pagination?.currentPage;
  const pagedTotalPages = pagination?.totalPages;

  React.useEffect(() => {
    if (viewerOptions?.initialThumbnailsOpen === undefined) setThumbnailsOpen(!compact);
  }, [compact, viewerOptions?.initialThumbnailsOpen]);

  React.useEffect(() => {
    if (pagination) return;
    const documentElement = reportRef.current?.querySelector<HTMLElement>('.printout-document') ?? reportRef.current?.firstElementChild as HTMLElement | null;
    if (!documentElement || typeof ResizeObserver === 'undefined') return;
    const update = () => setLocalTotalPages(calculateReportPageCount(documentElement.offsetHeight, pageHeight, pageOverflowTolerance));
    update();
    const observer = new ResizeObserver(update);
    observer.observe(documentElement);
    return () => observer.disconnect();
  }, [children, pageHeight, pageOverflowTolerance, pagination]);

  React.useEffect(() => {
    if (!prefetchPage || pagedCurrentPage === undefined || pagedTotalPages === undefined) return;
    const next = pagedCurrentPage + 1;
    if (next <= pagedTotalPages) void prefetchPage(next);
  }, [pagedCurrentPage, pagedTotalPages, prefetchPage]);

  const goToPage = React.useCallback((page: number) => {
    const next = Math.min(totalPages, Math.max(1, page));
    if (pagination) {
      void pagination.onPageChange(next);
      return;
    }
    setLocalPage(next);
    scrollRef.current?.scrollTo({ top: ((next - 1) * pageHeight * zoom) / 100, behavior: 'smooth' });
  }, [pageHeight, pagination, totalPages, zoom]);

  const changeZoom = React.useCallback((next: number) => {
    setZoomMode('100%');
    setZoom(Math.max(minZoom, Math.min(maxZoom, next)));
  }, [maxZoom, minZoom]);
  const changeZoomMode = React.useCallback((mode: ReportZoomMode) => {
    setZoomMode(mode);
    if (mode === '100%') setZoom(100);
  }, []);
  const calculateZoom = React.useCallback((next: number) => setZoom(Math.max(minZoom, Math.min(maxZoom, next))), [maxZoom, minZoom]);
  const findInReport = React.useCallback((queryValue: string) => {
    const query = queryValue.trim().toLocaleLowerCase();
    if (!query) return;
    [...(reportRef.current?.querySelectorAll<HTMLElement>('p, td, [data-report-search]') ?? [])].find((element) => element.textContent?.toLocaleLowerCase().includes(query))?.scrollIntoView({ behavior: 'smooth', block: 'center' });
  }, []);
  const fullscreen = React.useCallback(() => void viewerRef.current?.requestFullscreen?.(), []);
  const trackVisiblePage = React.useCallback<React.UIEventHandler<HTMLDivElement>>((event) => {
    if (pagination) return;
    const scaledPageHeight = pageHeight * zoom / 100;
    const next = Math.min(totalPages, Math.max(1, Math.floor(event.currentTarget.scrollTop / Math.max(1, scaledPageHeight)) + 1));
    setLocalPage((current) => current === next ? current : next);
  }, [pageHeight, pagination, totalPages, zoom]);

  const viewer = <Box ref={viewerRef} role="region" aria-label={title} dir={viewerOptions?.direction ?? 'ltr'} sx={{ height: variant === 'dialog' ? '100dvh' : '100%', minHeight: 0, display: 'grid', gridTemplateRows: '42px 38px minmax(0, 1fr)', overflow: 'hidden', bgcolor: '#e5e7eb' }}>
    <ReportToolbar compact={compact} currentPage={currentPage} totalPages={totalPages} thumbnailsOpen={thumbnailsOpen} zoom={zoom} zoomMode={zoomMode} exportFormats={exportFormats} onClose={onClose} onReload={onReload} onPrint={onPrint} onExport={(format: ReportExportFormat) => onExport(format)} onFullscreen={fullscreen} onToggleThumbnails={() => setThumbnailsOpen((value) => !value)} onPageChange={goToPage} onZoomChange={(requestedZoom) => changeZoom(zoom + Math.sign(requestedZoom - zoom) * zoomStep)} onZoomModeChange={changeZoomMode} onSearch={findInReport} />
    <Box sx={{ minHeight: 0, display: 'grid', gridTemplateColumns: thumbnailsOpen ? { xs: '112px minmax(0, 1fr)', md: '188px minmax(0, 1fr)' } : '0 minmax(0, 1fr)', overflow: 'hidden' }}>
      <ReportThumbnailPanel open={thumbnailsOpen} currentPage={currentPage} totalPages={totalPages} onPageChange={goToPage} renderThumbnail={pagination?.renderThumbnail} />
      <PrintoutPaginationProvider value={{ currentPage, totalPages }}>
        <ReportPreviewArea loading={loading || pagination?.loading === true} error={error} emptyMessage={emptyMessage} zoom={zoom} zoomMode={zoomMode} direction={viewerOptions?.direction ?? 'ltr'} reportRef={reportRef} scrollRef={scrollRef} onCalculatedZoom={calculateZoom} onScroll={trackVisiblePage}>{children}</ReportPreviewArea>
      </PrintoutPaginationProvider>
    </Box>
  </Box>;

  if (variant === 'dialog') return <Dialog open={open} onClose={onClose} fullScreen disableRestoreFocus>{viewer}</Dialog>;
  return open ? viewer : <></>;
}
