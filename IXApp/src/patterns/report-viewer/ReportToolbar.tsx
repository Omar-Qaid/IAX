import React from 'react';
import { Box, Button, Divider, IconButton, Menu, MenuItem, Paper, Stack, TextField, Tooltip, Typography } from '@mui/material';
import ArrowBack from '@mui/icons-material/ArrowBack';
import ArrowDropDown from '@mui/icons-material/ArrowDropDown';
import AutoStoriesOutlined from '@mui/icons-material/AutoStoriesOutlined';
import ChevronLeft from '@mui/icons-material/ChevronLeft';
import ChevronRight from '@mui/icons-material/ChevronRight';
import DownloadOutlined from '@mui/icons-material/DownloadOutlined';
import FullscreenOutlined from '@mui/icons-material/FullscreenOutlined';
import MoreHoriz from '@mui/icons-material/MoreHoriz';
import OpenInNewOutlined from '@mui/icons-material/OpenInNewOutlined';
import PrintOutlined from '@mui/icons-material/PrintOutlined';
import RefreshOutlined from '@mui/icons-material/RefreshOutlined';
import SearchOutlined from '@mui/icons-material/SearchOutlined';
import ViewSidebarOutlined from '@mui/icons-material/ViewSidebarOutlined';
import ZoomIn from '@mui/icons-material/ZoomIn';
import ZoomOut from '@mui/icons-material/ZoomOut';
import type { ReportExportFormat, ReportZoomMode } from './types';

const toolbarButtonSx = { minWidth: 0, px: 1, color: '#172b3a', textTransform: 'none', fontSize: 12, borderRadius: 0.5 } as const;
const zoomModes: ReportZoomMode[] = ['Automatic Zoom', 'Page Width', 'Whole Page', '100%'];

interface ReportToolbarProps {
  compact: boolean;
  currentPage: number;
  totalPages: number;
  thumbnailsOpen: boolean;
  zoom: number;
  zoomMode: ReportZoomMode;
  exportFormats: readonly ReportExportFormat[];
  onClose: () => void;
  onReload?: () => void | Promise<void>;
  onPrint: () => void;
  onExport: (format: ReportExportFormat) => void;
  onFullscreen: () => void;
  onToggleThumbnails: () => void;
  onPageChange: (page: number) => void;
  onZoomChange: (zoom: number) => void;
  onZoomModeChange: (mode: ReportZoomMode) => void;
  onSearch: (query: string) => void;
}

export function ReportToolbar(props: ReportToolbarProps): React.ReactElement {
  const [exportAnchor, setExportAnchor] = React.useState<HTMLElement | null>(null);
  const [overflowAnchor, setOverflowAnchor] = React.useState<HTMLElement | null>(null);
  const [zoomAnchor, setZoomAnchor] = React.useState<HTMLElement | null>(null);
  const [searchOpen, setSearchOpen] = React.useState(false);
  const [search, setSearch] = React.useState('');
  const { compact, currentPage, totalPages, thumbnailsOpen, zoom, zoomMode, exportFormats } = props;

  return <>
    <Paper square elevation={1} className="printout-screen-only" sx={{ zIndex: 3, display: 'flex', alignItems: 'center', gap: 0.25, px: 0.75, borderBottom: '1px solid #d2d6dc', borderRadius: '9px' }}>
      <Tooltip title="Back"><IconButton size="small" onClick={props.onClose}><ArrowBack fontSize="small" /></IconButton></Tooltip>
      <Divider orientation="vertical" flexItem />
      <Button sx={toolbarButtonSx} endIcon={<ArrowDropDown />} onClick={(event) => setExportAnchor(event.currentTarget)}>Export</Button>
      {!compact && props.onReload ? <Button sx={toolbarButtonSx} onClick={() => void props.onReload?.()}>Reload</Button> : null}
      {!compact ? <Button sx={toolbarButtonSx} onClick={(event) => setZoomAnchor(event.currentTarget)}>Options</Button> : null}
      <Box sx={{ flex: 1 }} />
      {!compact ? <Stack direction="row" spacing={0.15}>
        <Tooltip title="Reading mode"><IconButton size="small" color="primary"><AutoStoriesOutlined sx={{ fontSize: 18 }} /></IconButton></Tooltip>
        <Tooltip title="Reload"><IconButton size="small" color="primary" onClick={() => void props.onReload?.()}><RefreshOutlined sx={{ fontSize: 18 }} /></IconButton></Tooltip>
        <Tooltip title="Open presentation"><IconButton size="small" color="primary" onClick={props.onFullscreen}><OpenInNewOutlined sx={{ fontSize: 18 }} /></IconButton></Tooltip>
      </Stack> : <IconButton size="small" onClick={(event) => setOverflowAnchor(event.currentTarget)}><MoreHoriz /></IconButton>}
    </Paper>
    <Paper square elevation={0} className="printout-screen-only" sx={{ zIndex: 2, display: 'grid', gridTemplateColumns: '1fr auto 1fr', alignItems: 'center', px: 0.75, borderBottom: '1px solid #c9ced6', bgcolor: '#f7f7f8' }}>
      <Stack direction="row" spacing={0.25} sx={{ alignItems: 'center', minWidth: 0 }}>
        <Tooltip title="Toggle thumbnails"><IconButton size="small" color={thumbnailsOpen ? 'primary' : 'default'} onClick={props.onToggleThumbnails}><ViewSidebarOutlined sx={{ fontSize: 19 }} /></IconButton></Tooltip>
        <Tooltip title="Search report"><IconButton size="small" onClick={() => setSearchOpen((value) => !value)}><SearchOutlined sx={{ fontSize: 19 }} /></IconButton></Tooltip>
        {searchOpen ? <TextField autoFocus size="small" placeholder="Search" value={search} onChange={(event) => setSearch(event.target.value)} onKeyDown={(event) => { if (event.key === 'Enter') props.onSearch(search); }} sx={{ width: { xs: 78, sm: 140 }, '& .MuiInputBase-root': { height: 27, fontSize: 11.5 } }} /> : null}
        <IconButton size="small" aria-label="Previous page" disabled={currentPage <= 1} onClick={() => props.onPageChange(currentPage - 1)}><ChevronLeft /></IconButton>
        <IconButton size="small" aria-label="Next page" disabled={currentPage >= totalPages} onClick={() => props.onPageChange(currentPage + 1)}><ChevronRight /></IconButton>
        <TextField size="small" value={currentPage} onChange={(event) => props.onPageChange(Number(event.target.value) || 1)} slotProps={{ htmlInput: { 'aria-label': 'Current page', min: 1, max: totalPages } }} sx={{ width: 48, '& .MuiInputBase-root': { height: 27, fontSize: 12 }, '& input': { py: 0, textAlign: 'center' } }} />
        <Typography sx={{ fontSize: 11.5, whiteSpace: 'nowrap' }}>of {totalPages}</Typography>
      </Stack>
      <Stack direction="row" spacing={0.25} sx={{ alignItems: 'center' }}>
        <IconButton size="small" aria-label="Zoom out" onClick={() => props.onZoomChange(zoom - 10)}><ZoomOut /></IconButton>
        <IconButton size="small" aria-label="Zoom in" onClick={() => props.onZoomChange(zoom + 10)}><ZoomIn /></IconButton>
        <Button sx={{ ...toolbarButtonSx, minWidth: { xs: 112, sm: 145 }, bgcolor: '#e1e3e6' }} endIcon={<ArrowDropDown />} onClick={(event) => setZoomAnchor(event.currentTarget)}>{zoomMode}</Button>
      </Stack>
      <Stack direction="row" spacing={0.1} sx={{ justifySelf: 'end' }}>
        {!compact ? <><Tooltip title="Presentation mode"><IconButton size="small" onClick={props.onFullscreen}><FullscreenOutlined fontSize="small" /></IconButton></Tooltip><Tooltip title="Print"><IconButton size="small" onClick={props.onPrint}><PrintOutlined fontSize="small" /></IconButton></Tooltip><Tooltip title="Download"><IconButton size="small" onClick={() => props.onExport('PDF')}><DownloadOutlined fontSize="small" /></IconButton></Tooltip><Tooltip title="Additional actions"><IconButton size="small" onClick={(event) => setOverflowAnchor(event.currentTarget)}><MoreHoriz fontSize="small" /></IconButton></Tooltip></> : null}
      </Stack>
    </Paper>
    <Menu anchorEl={exportAnchor} open={Boolean(exportAnchor)} onClose={() => setExportAnchor(null)}>{exportFormats.map((format) => <MenuItem key={format} onClick={() => { setExportAnchor(null); props.onExport(format); }} sx={{ minWidth: 118, fontSize: 12 }}>{format}</MenuItem>)}</Menu>
    <Menu anchorEl={zoomAnchor} open={Boolean(zoomAnchor)} onClose={() => setZoomAnchor(null)}>{zoomModes.map((mode) => <MenuItem key={mode} selected={zoomMode === mode} onClick={() => { props.onZoomModeChange(mode); setZoomAnchor(null); }}>{mode}</MenuItem>)}</Menu>
    <Menu anchorEl={overflowAnchor} open={Boolean(overflowAnchor)} onClose={() => setOverflowAnchor(null)}>
      {compact && props.onReload ? <MenuItem onClick={() => { void props.onReload?.(); setOverflowAnchor(null); }}>Reload</MenuItem> : null}
      <MenuItem onClick={() => { props.onFullscreen(); setOverflowAnchor(null); }}>Presentation</MenuItem>
      <MenuItem onClick={() => { props.onPrint(); setOverflowAnchor(null); }}>Print</MenuItem>
      <MenuItem onClick={() => { props.onExport('PDF'); setOverflowAnchor(null); }}>Download / Save</MenuItem>
    </Menu>
  </>;
}
