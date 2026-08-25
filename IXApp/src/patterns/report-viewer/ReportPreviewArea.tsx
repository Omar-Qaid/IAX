import React from 'react';
import { Alert, Box, CircularProgress } from '@mui/material';
import type { ReportDirection, ReportZoomMode } from './types';

interface ReportPreviewAreaProps {
  children?: React.ReactNode;
  loading: boolean;
  error: string | null;
  emptyMessage: string;
  zoom: number;
  zoomMode: ReportZoomMode;
  direction: ReportDirection;
  reportRef: React.RefObject<HTMLDivElement | null>;
  scrollRef: React.RefObject<HTMLDivElement | null>;
  onCalculatedZoom: (zoom: number) => void;
  onScroll?: React.UIEventHandler<HTMLDivElement>;
}

export function ReportPreviewArea({ children, loading, error, emptyMessage, zoom, zoomMode, direction, reportRef, scrollRef, onCalculatedZoom, onScroll }: ReportPreviewAreaProps): React.ReactElement {
  React.useEffect(() => {
    if (zoomMode === '100%' || typeof ResizeObserver === 'undefined') return;
    const viewport = scrollRef.current;
    const documentElement = reportRef.current?.querySelector<HTMLElement>('.printout-document') ?? reportRef.current?.firstElementChild as HTMLElement | null;
    if (!viewport || !documentElement) return;
    const update = () => {
      const widthZoom = ((viewport.clientWidth - 40) / Math.max(1, documentElement.offsetWidth)) * 100;
      const heightZoom = ((viewport.clientHeight - 40) / Math.max(1, documentElement.offsetHeight)) * 100;
      const next = zoomMode === 'Whole Page' ? Math.min(widthZoom, heightZoom) : widthZoom;
      onCalculatedZoom(Math.max(40, Math.min(100, Math.round(next))));
    };
    update();
    const observer = new ResizeObserver(update);
    observer.observe(viewport);
    observer.observe(documentElement);
    return () => observer.disconnect();
  }, [children, onCalculatedZoom, reportRef, scrollRef, zoomMode]);

  return <Box ref={scrollRef} dir={direction} onScroll={onScroll} sx={{ minWidth: 0, overflow: 'auto', bgcolor: '#e7e8eb', p: { xs: 1, md: 2 } }}>
    {loading ? <Box sx={{ minHeight: 320, display: 'grid', placeItems: 'center' }}><CircularProgress /></Box>
      : error ? <Alert severity="error" sx={{ m: 'auto', maxWidth: 520 }}>{error}</Alert>
        : children ? <Box ref={reportRef} className="printout-preview-scale" sx={{ width: 'max-content', mx: 'auto', transformOrigin: 'top center', zoom: zoom / 100 }}>{children}</Box>
          : <Alert severity="info" sx={{ m: 'auto', maxWidth: 520 }}>{emptyMessage}</Alert>}
  </Box>;
}
