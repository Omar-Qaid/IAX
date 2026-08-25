import React from 'react';
import { Alert, Box, Button, CircularProgress, Dialog, Divider, IconButton, Menu, MenuItem, Paper, Stack, TextField, Tooltip, Typography, useMediaQuery, useTheme } from '@mui/material';
import ArrowBack from '@mui/icons-material/ArrowBack';
import ArrowDropDown from '@mui/icons-material/ArrowDropDown';
import AttachFileOutlined from '@mui/icons-material/AttachFileOutlined';
import AutoStoriesOutlined from '@mui/icons-material/AutoStoriesOutlined';
import ChevronLeft from '@mui/icons-material/ChevronLeft';
import ChevronRight from '@mui/icons-material/ChevronRight';
import DownloadOutlined from '@mui/icons-material/DownloadOutlined';
import FullscreenOutlined from '@mui/icons-material/FullscreenOutlined';
import GridViewOutlined from '@mui/icons-material/GridViewOutlined';
import LayersOutlined from '@mui/icons-material/LayersOutlined';
import ListOutlined from '@mui/icons-material/ListOutlined';
import MoreHoriz from '@mui/icons-material/MoreHoriz';
import OpenInNewOutlined from '@mui/icons-material/OpenInNewOutlined';
import PrintOutlined from '@mui/icons-material/PrintOutlined';
import RefreshOutlined from '@mui/icons-material/RefreshOutlined';
import SearchOutlined from '@mui/icons-material/SearchOutlined';
import ViewSidebarOutlined from '@mui/icons-material/ViewSidebarOutlined';
import ZoomIn from '@mui/icons-material/ZoomIn';
import ZoomOut from '@mui/icons-material/ZoomOut';

export const REPORT_EXPORT_FORMATS = ['PDF', 'Excel', 'Word', 'CSV', 'XML', 'MHTML', 'TIFF'] as const;
export type ReportExportFormat = (typeof REPORT_EXPORT_FORMATS)[number];
type ZoomMode = 'Automatic Zoom' | 'Page Width' | 'Whole Page' | '100%';

export interface ReportViewerProps {
  open: boolean;
  title: string;
  variant?: 'embedded' | 'dialog';
  children?: React.ReactNode;
  loading?: boolean;
  error?: string | null;
  emptyMessage?: string;
  exportFormats?: readonly ReportExportFormat[];
  pageHeight?: number;
  onClose: () => void;
  onReload?: () => void | Promise<void>;
  onPrint: () => void;
  onExport: (format: ReportExportFormat) => void;
}

const toolbarButtonSx = { minWidth: 0, px: 1, color: '#172b3a', textTransform: 'none', fontSize: 12, borderRadius: 0.5 } as const;

export function ReportViewer({ open, title, variant = 'embedded', children, loading = false, error = null, emptyMessage = 'No report is available.', exportFormats = REPORT_EXPORT_FORMATS, pageHeight = 1123, onClose, onReload, onPrint, onExport }: ReportViewerProps): React.ReactElement {
  const theme = useTheme();
  const compact = useMediaQuery(theme.breakpoints.down('md'));
  const viewerRef = React.useRef<HTMLDivElement | null>(null);
  const scrollRef = React.useRef<HTMLDivElement | null>(null);
  const reportRef = React.useRef<HTMLDivElement | null>(null);
  const [exportAnchor, setExportAnchor] = React.useState<HTMLElement | null>(null);
  const [overflowAnchor, setOverflowAnchor] = React.useState<HTMLElement | null>(null);
  const [zoomAnchor, setZoomAnchor] = React.useState<HTMLElement | null>(null);
  const [thumbnailsOpen, setThumbnailsOpen] = React.useState(!compact);
  const [searchOpen, setSearchOpen] = React.useState(false);
  const [search, setSearch] = React.useState('');
  const [currentPage, setCurrentPage] = React.useState(1);
  const [totalPages, setTotalPages] = React.useState(1);
  const [zoom, setZoom] = React.useState(100);
  const [zoomMode, setZoomMode] = React.useState<ZoomMode>('Automatic Zoom');

  React.useEffect(() => setThumbnailsOpen(!compact), [compact]);
  React.useEffect(() => {
    const documentElement = reportRef.current?.querySelector<HTMLElement>('.printout-document') ?? reportRef.current?.firstElementChild as HTMLElement | null;
    if (!documentElement || typeof ResizeObserver === 'undefined') return;
    const update = () => setTotalPages(Math.max(1, Math.ceil(documentElement.offsetHeight / pageHeight)));
    update();
    const observer = new ResizeObserver(update);
    observer.observe(documentElement);
    return () => observer.disconnect();
  }, [children, pageHeight]);

  const goToPage = (page: number) => {
    const next = Math.min(totalPages, Math.max(1, page));
    setCurrentPage(next);
    scrollRef.current?.scrollTo({ top: (next - 1) * pageHeight * zoom / 100, behavior: 'smooth' });
  };
  const selectZoomMode = (mode: ZoomMode) => {
    setZoomMode(mode);
    setZoom(mode === '100%' ? 100 : mode === 'Whole Page' ? 72 : mode === 'Page Width' ? 92 : compact ? 68 : 100);
  };
  const findInReport = () => {
    const query = search.trim().toLocaleLowerCase();
    if (!query) return;
    [...(reportRef.current?.querySelectorAll<HTMLElement>('p, td, [data-report-search]') ?? [])].find((element) => element.textContent?.toLocaleLowerCase().includes(query))?.scrollIntoView({ behavior: 'smooth', block: 'center' });
  };
  const exportReport = (format: ReportExportFormat) => { setExportAnchor(null); onExport(format); };
  const fullscreen = () => void viewerRef.current?.requestFullscreen?.();
  const secondary = <><Tooltip title="Presentation mode"><IconButton size="small" onClick={fullscreen}><FullscreenOutlined fontSize="small" /></IconButton></Tooltip><Tooltip title="Print"><IconButton size="small" onClick={onPrint}><PrintOutlined fontSize="small" /></IconButton></Tooltip><Tooltip title="Download"><IconButton size="small" onClick={() => onExport('PDF')}><DownloadOutlined fontSize="small" /></IconButton></Tooltip><Tooltip title="Additional actions"><IconButton size="small" onClick={(event) => setOverflowAnchor(event.currentTarget)}><MoreHoriz fontSize="small" /></IconButton></Tooltip></>;

  const viewer = <Box ref={viewerRef} role="region" aria-label={title} sx={{ height: variant === 'dialog' ? '100dvh' : '100%', minHeight: 0, display: 'grid', gridTemplateRows: '42px 38px minmax(0, 1fr)', overflow: 'hidden', bgcolor: '#e5e7eb' }}>
    <Paper square elevation={1} className="printout-screen-only" sx={{ zIndex: 3, display: 'flex', alignItems: 'center', gap: 0.25, px: 0.75, borderBottom: '1px solid #d2d6dc', borderRadius: '9px' }}><Tooltip title="Back"><IconButton size="small" onClick={onClose}><ArrowBack fontSize="small" /></IconButton></Tooltip><Divider orientation="vertical" flexItem /><Button sx={toolbarButtonSx} endIcon={<ArrowDropDown />} onClick={(event) => setExportAnchor(event.currentTarget)}>Export</Button>{!compact && onReload ? <Button sx={toolbarButtonSx} onClick={() => void onReload()}>Reload</Button> : null}{!compact ? <Button sx={toolbarButtonSx} onClick={(event) => setZoomAnchor(event.currentTarget)}>Options</Button> : null}<Box sx={{ flex: 1 }} />{!compact ? <Stack direction="row" spacing={0.15}><Tooltip title="Reading mode"><IconButton size="small" color="primary"><AutoStoriesOutlined sx={{ fontSize: 18 }} /></IconButton></Tooltip><Tooltip title="Reload"><IconButton size="small" color="primary" onClick={() => void onReload?.()}><RefreshOutlined sx={{ fontSize: 18 }} /></IconButton></Tooltip><Tooltip title="Open presentation"><IconButton size="small" color="primary" onClick={fullscreen}><OpenInNewOutlined sx={{ fontSize: 18 }} /></IconButton></Tooltip></Stack> : <IconButton size="small" onClick={(event) => setOverflowAnchor(event.currentTarget)}><MoreHoriz /></IconButton>}</Paper>
    <Paper square elevation={0} className="printout-screen-only" sx={{ zIndex: 2, display: 'grid', gridTemplateColumns: '1fr auto 1fr', alignItems: 'center', px: 0.75, borderBottom: '1px solid #c9ced6', bgcolor: '#f7f7f8' }}><Stack direction="row" spacing={0.25} sx={{ alignItems: 'center', minWidth: 0 }}><Tooltip title="Toggle thumbnails"><IconButton size="small" color={thumbnailsOpen ? 'primary' : 'default'} onClick={() => setThumbnailsOpen((value) => !value)}><ViewSidebarOutlined sx={{ fontSize: 19 }} /></IconButton></Tooltip><Tooltip title="Search report"><IconButton size="small" onClick={() => setSearchOpen((value) => !value)}><SearchOutlined sx={{ fontSize: 19 }} /></IconButton></Tooltip>{searchOpen ? <TextField autoFocus size="small" placeholder="Search" value={search} onChange={(event) => setSearch(event.target.value)} onKeyDown={(event) => { if (event.key === 'Enter') findInReport(); }} sx={{ width: { xs: 78, sm: 140 }, '& .MuiInputBase-root': { height: 27, fontSize: 11.5 } }} /> : null}<IconButton size="small" disabled={currentPage <= 1} onClick={() => goToPage(currentPage - 1)}><ChevronLeft /></IconButton><IconButton size="small" disabled={currentPage >= totalPages} onClick={() => goToPage(currentPage + 1)}><ChevronRight /></IconButton><TextField size="small" value={currentPage} onChange={(event) => goToPage(Number(event.target.value) || 1)} inputProps={{ 'aria-label': 'Current page', min: 1, max: totalPages }} sx={{ width: 48, '& .MuiInputBase-root': { height: 27, fontSize: 12 }, '& input': { py: 0, textAlign: 'center' } }} /><Typography sx={{ fontSize: 11.5, whiteSpace: 'nowrap' }}>of {totalPages}</Typography></Stack><Stack direction="row" spacing={0.25} sx={{ alignItems: 'center' }}><IconButton size="small" aria-label="Zoom out" onClick={() => { setZoomMode('100%'); setZoom((value) => Math.max(40, value - 10)); }}><ZoomOut /></IconButton><IconButton size="small" aria-label="Zoom in" onClick={() => { setZoomMode('100%'); setZoom((value) => Math.min(180, value + 10)); }}><ZoomIn /></IconButton><Button sx={{ ...toolbarButtonSx, minWidth: { xs: 112, sm: 145 }, bgcolor: '#e1e3e6' }} endIcon={<ArrowDropDown />} onClick={(event) => setZoomAnchor(event.currentTarget)}>{zoomMode}</Button></Stack><Stack direction="row" spacing={0.1} sx={{ justifySelf: 'end' }}>{!compact ? secondary : null}</Stack></Paper>
    <Box sx={{ minHeight: 0, display: 'grid', gridTemplateColumns: thumbnailsOpen ? { xs: '112px minmax(0, 1fr)', md: '188px minmax(0, 1fr)' } : '0 minmax(0, 1fr)', overflow: 'hidden' }}><Paper square className="printout-screen-only" sx={{ minWidth: 0, display: 'grid', gridTemplateRows: '31px minmax(0, 1fr)', overflow: 'hidden', bgcolor: '#d7d8dc', borderRight: '1px solid #adb2ba' }}>{thumbnailsOpen ? <><Stack direction="row" spacing={0.1} sx={{ alignItems: 'center', px: 0.5, bgcolor: '#eeeeef', borderBottom: '1px solid #b8bdc4' }}><IconButton size="small" color="primary"><GridViewOutlined sx={{ fontSize: 17 }} /></IconButton><IconButton size="small"><ListOutlined sx={{ fontSize: 17 }} /></IconButton><IconButton size="small"><AttachFileOutlined sx={{ fontSize: 17 }} /></IconButton><IconButton size="small"><LayersOutlined sx={{ fontSize: 17 }} /></IconButton></Stack><Stack spacing={1.5} sx={{ p: 1.5, overflowY: 'auto' }}>{Array.from({ length: totalPages }, (_, index) => { const page = index + 1; return <Box key={page} onClick={() => goToPage(page)} sx={{ cursor: 'pointer' }}><Box sx={{ width: { xs: 72, md: 108 }, aspectRatio: '210 / 297', mx: 'auto', bgcolor: '#fff', border: '2px solid', borderColor: currentPage === page ? '#315fa8' : '#9ba2ab', boxShadow: currentPage === page ? '0 0 0 2px rgba(49,95,168,.2)' : '0 2px 5px rgba(0,0,0,.18)', p: 0.75 }}><Box sx={{ height: '18%', borderBottom: '1px solid #9aa4ae' }} /><Stack spacing={0.5} sx={{ mt: 0.75 }}>{[70, 92, 82, 95, 76, 88].map((width, line) => <Box key={line} sx={{ width: `${width}%`, height: 2, bgcolor: '#c2c7ce' }} />)}</Stack></Box><Typography sx={{ mt: 0.5, textAlign: 'center', fontSize: 11, fontWeight: currentPage === page ? 700 : 500 }}>{page}</Typography></Box>; })}</Stack></> : null}</Paper><Box ref={scrollRef} sx={{ minWidth: 0, overflow: 'auto', bgcolor: '#e7e8eb', p: { xs: 1, md: 2 } }}>{loading ? <Box sx={{ minHeight: 320, display: 'grid', placeItems: 'center' }}><CircularProgress /></Box> : error ? <Alert severity="error" sx={{ m: 'auto', maxWidth: 520 }}>{error}</Alert> : children ? <Box ref={reportRef} className="printout-preview-scale" sx={{ width: 'max-content', mx: 'auto', transformOrigin: 'top center', zoom: zoom / 100 }}>{children}</Box> : <Alert severity="info" sx={{ m: 'auto', maxWidth: 520 }}>{emptyMessage}</Alert>}</Box></Box>
    <Menu anchorEl={exportAnchor} open={Boolean(exportAnchor)} onClose={() => setExportAnchor(null)}>{exportFormats.map((format) => <MenuItem key={format} onClick={() => exportReport(format)} sx={{ minWidth: 118, fontSize: 12 }}>{format}</MenuItem>)}</Menu><Menu anchorEl={zoomAnchor} open={Boolean(zoomAnchor)} onClose={() => setZoomAnchor(null)}>{(['Automatic Zoom', 'Page Width', 'Whole Page', '100%'] as ZoomMode[]).map((mode) => <MenuItem key={mode} selected={zoomMode === mode} onClick={() => { selectZoomMode(mode); setZoomAnchor(null); }}>{mode}</MenuItem>)}</Menu><Menu anchorEl={overflowAnchor} open={Boolean(overflowAnchor)} onClose={() => setOverflowAnchor(null)}>{compact && onReload ? <MenuItem onClick={() => { void onReload(); setOverflowAnchor(null); }}>Reload</MenuItem> : null}<MenuItem onClick={() => { fullscreen(); setOverflowAnchor(null); }}>Presentation</MenuItem><MenuItem onClick={() => { onPrint(); setOverflowAnchor(null); }}>Print</MenuItem><MenuItem onClick={() => { onExport('PDF'); setOverflowAnchor(null); }}>Download / Save</MenuItem></Menu>
  </Box>;

  if (variant === 'dialog') return <Dialog open={open} onClose={onClose} fullScreen disableRestoreFocus>{viewer}</Dialog>;
  return open ? viewer : <></>;
}
