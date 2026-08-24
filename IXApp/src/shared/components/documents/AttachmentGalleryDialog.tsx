import React from 'react';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Dialog,
  DialogContent,
  DialogTitle,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  Paper,
  Stack,
  Tab,
  Tabs,
  Tooltip,
  Typography,
} from '@mui/material';
import ChevronLeft from '@mui/icons-material/ChevronLeft';
import ChevronRight from '@mui/icons-material/ChevronRight';
import Close from '@mui/icons-material/Close';
import DescriptionOutlined from '@mui/icons-material/DescriptionOutlined';
import DownloadOutlined from '@mui/icons-material/DownloadOutlined';
import ImageOutlined from '@mui/icons-material/ImageOutlined';
import InsertDriveFileOutlined from '@mui/icons-material/InsertDriveFileOutlined';
import OpenInNewOutlined from '@mui/icons-material/OpenInNewOutlined';
import PictureAsPdfOutlined from '@mui/icons-material/PictureAsPdfOutlined';
import TableChartOutlined from '@mui/icons-material/TableChartOutlined';
import { useQuery } from '@tanstack/react-query';
import { documentApi, type DocumentDto } from './documentApi';

interface AttachmentGalleryDialogProps {
  open: boolean;
  initialDocument: DocumentDto | null;
  refTableId: number;
  refRecId: number;
  onClose: () => void;
}

type Spreadsheet = { name: string; rows: string[][] };

const extensionOf = (item: Pick<DocumentDto, 'fileName' | 'originalFileName' | 'name'>) => {
  const name = item.fileName || item.originalFileName || item.name;
  const dot = name.lastIndexOf('.');
  return dot >= 0 ? name.slice(dot + 1).toLocaleLowerCase() : '';
};

const fileIcon = (item: DocumentDto) => {
  const extension = extensionOf(item);
  if (item.contentType?.startsWith('image/')) return <ImageOutlined sx={{ color: '#18864b' }} />;
  if (item.contentType === 'application/pdf' || extension === 'pdf') return <PictureAsPdfOutlined sx={{ color: '#c62828' }} />;
  if (extension === 'docx' || extension === 'doc') return <DescriptionOutlined sx={{ color: '#2563b9' }} />;
  if (['xlsx', 'xls', 'csv'].includes(extension)) return <TableChartOutlined sx={{ color: '#16834a' }} />;
  return <InsertDriveFileOutlined sx={{ color: 'text.secondary' }} />;
};

const cellText = (value: unknown): string => {
  if (value == null) return '';
  if (value instanceof Date) return value.toLocaleString();
  if (typeof value !== 'object') return String(value);
  const rich = value as { richText?: Array<{ text?: string }>; result?: unknown; text?: string; hyperlink?: string };
  if (rich.richText) return rich.richText.map((part) => part.text ?? '').join('');
  if (rich.result != null) return cellText(rich.result);
  return rich.text ?? rich.hyperlink ?? '';
};

const parseCsvLine = (line: string) => {
  const cells: string[] = [];
  let current = '';
  let quoted = false;
  for (let index = 0; index < line.length; index += 1) {
    const character = line[index];
    if (character === '"' && quoted && line[index + 1] === '"') { current += '"'; index += 1; }
    else if (character === '"') quoted = !quoted;
    else if (character === ',' && !quoted) { cells.push(current); current = ''; }
    else current += character;
  }
  cells.push(current);
  return cells;
};

function SpreadsheetPreview({ sheets }: { sheets: Spreadsheet[] }): React.ReactElement {
  const [sheetIndex, setSheetIndex] = React.useState(0);
  React.useEffect(() => setSheetIndex(0), [sheets]);
  const sheet = sheets[sheetIndex];
  if (!sheet) return <Alert severity="info">The workbook does not contain visible data.</Alert>;
  return <Stack sx={{ width: '100%', height: '100%', minHeight: 0 }}>
    {sheets.length > 1 && <Tabs value={sheetIndex} onChange={(_, value: number) => setSheetIndex(value)} variant="scrollable" scrollButtons="auto" sx={{ minHeight: 34, borderBottom: '1px solid', borderColor: 'divider', '& .MuiTab-root': { minHeight: 34, py: 0, fontSize: 11 } }}>{sheets.map((candidate) => <Tab key={candidate.name} label={candidate.name} />)}</Tabs>}
    <Box sx={{ flex: 1, minHeight: 0, overflow: 'auto', bgcolor: '#fff' }}>
      <Box component="table" sx={{ borderCollapse: 'collapse', minWidth: '100%', width: 'max-content', '& td': { minWidth: 90, maxWidth: 320, px: 0.75, py: 0.4, border: '1px solid #d7dce2', fontSize: 11.5, whiteSpace: 'pre-wrap', overflowWrap: 'anywhere' }, '& tr:first-of-type td': { bgcolor: '#eef3f8', fontWeight: 700, position: 'sticky', top: 0, zIndex: 1 } }}>
        <tbody>{sheet.rows.map((row, rowIndex) => <tr key={rowIndex}>{row.map((cell, columnIndex) => <td key={columnIndex}>{cell}</td>)}</tr>)}</tbody>
      </Box>
      {sheet.rows.length >= 500 && <Alert severity="info">Preview is limited to the first 500 rows.</Alert>}
    </Box>
  </Stack>;
}

function OfficePreview({ blob, item }: { blob: Blob; item: DocumentDto }): React.ReactElement {
  const extension = extensionOf(item);
  const [wordHtml, setWordHtml] = React.useState('');
  const [sheets, setSheets] = React.useState<Spreadsheet[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState('');
  React.useEffect(() => {
    let active = true;
    setLoading(true); setError(''); setWordHtml(''); setSheets([]);
    void (async () => {
      try {
        if (extension === 'docx') {
          const mammoth = await import('mammoth');
          const result = await mammoth.convertToHtml({ arrayBuffer: await blob.arrayBuffer() });
          if (active) setWordHtml(result.value);
        } else if (extension === 'xlsx') {
          const ExcelJS = await import('exceljs');
          const workbook = new ExcelJS.Workbook();
          await workbook.xlsx.load(await blob.arrayBuffer());
          const next: Spreadsheet[] = [];
          workbook.eachSheet((worksheet) => {
            const rows: string[][] = [];
            worksheet.eachRow({ includeEmpty: true }, (row, rowNumber) => {
              if (rowNumber > 500) return;
              const values: string[] = [];
              const maxColumn = Math.min(worksheet.columnCount, 100);
              for (let column = 1; column <= maxColumn; column += 1) values.push(cellText(row.getCell(column).value));
              rows.push(values);
            });
            next.push({ name: worksheet.name, rows });
          });
          if (active) setSheets(next);
        } else if (extension === 'csv') {
          const rows = (await blob.text()).split(/\r?\n/).slice(0, 500).filter((line) => line.length > 0).map(parseCsvLine);
          if (active) setSheets([{ name: 'CSV', rows }]);
        } else throw new Error('This legacy Office format cannot be rendered in the browser.');
      } catch (reason) {
        if (active) setError(reason instanceof Error ? reason.message : 'Unable to render this document.');
      } finally { if (active) setLoading(false); }
    })();
    return () => { active = false; };
  }, [blob, extension]);
  if (loading) return <Box sx={{ height: '100%', display: 'grid', placeItems: 'center' }}><CircularProgress size={28} /></Box>;
  if (error) return <Alert severity="info">{error} Use Open or Download to view the original file.</Alert>;
  if (wordHtml) {
    const source = `<!doctype html><html><head><meta charset="utf-8"><style>body{font:14px/1.55 Segoe UI,Arial,sans-serif;margin:32px;color:#202124}img{max-width:100%;height:auto}table{border-collapse:collapse}td,th{border:1px solid #ccd2d9;padding:5px 8px}</style></head><body>${wordHtml}</body></html>`;
    return <Box component="iframe" sandbox="" title={item.name} srcDoc={source} sx={{ width: '100%', height: '100%', border: 0, bgcolor: '#fff' }} />;
  }
  return <SpreadsheetPreview sheets={sheets} />;
}

function GalleryPreview({ item }: { item: DocumentDto }): React.ReactElement {
  const preview = useQuery({
    queryKey: ['document-gallery-preview', item.id],
    queryFn: ({ signal }) => documentApi.previewBlob(item.id, signal),
    enabled: item.kind === 'File' || item.kind === 'Image',
  });
  const [url, setUrl] = React.useState('');
  React.useEffect(() => {
    if (!preview.data) { setUrl(''); return undefined; }
    const next = URL.createObjectURL(preview.data); setUrl(next);
    return () => URL.revokeObjectURL(next);
  }, [preview.data]);
  if (item.kind === 'Note') return <Typography sx={{ p: 3, whiteSpace: 'pre-wrap' }}>{item.notes || 'No note content.'}</Typography>;
  if (item.kind === 'URL') return item.url ? <Button component="a" href={item.url} target="_blank" rel="noreferrer">Open URL</Button> : <Alert severity="info">No URL is available.</Alert>;
  if (preview.isLoading) return <Box sx={{ height: '100%', display: 'grid', placeItems: 'center' }}><CircularProgress /></Box>;
  if (preview.isError || !preview.data || !url) return <Alert severity="error">Unable to load this attachment preview.</Alert>;
  const extension = extensionOf(item);
  if (item.contentType?.startsWith('image/')) return <Box component="img" src={url} alt={item.name} sx={{ display: 'block', width: '100%', height: '100%', objectFit: 'contain' }} />;
  if (item.contentType === 'application/pdf' || extension === 'pdf') return <Box component="iframe" title={item.name} src={`${url}#toolbar=1&navpanes=0`} sx={{ width: '100%', height: '100%', border: 0 }} />;
  if (['docx', 'doc', 'xlsx', 'xls', 'csv'].includes(extension)) return <OfficePreview blob={preview.data} item={item} />;
  return <Alert severity="info">Preview is not available for this file type. Use Open or Download to view it.</Alert>;
}

export function AttachmentGalleryDialog({ open, initialDocument, refTableId, refRecId, onClose }: AttachmentGalleryDialogProps): React.ReactElement {
  const documents = useQuery({
    queryKey: ['document-gallery', refTableId, refRecId],
    queryFn: ({ signal }) => documentApi.list(refTableId, refRecId, signal),
    enabled: open,
  });
  const items = React.useMemo(() => documents.data?.items ?? (initialDocument ? [initialDocument] : []), [documents.data?.items, initialDocument]);
  const [selectedId, setSelectedId] = React.useState<number | null>(initialDocument?.id ?? null);
  React.useEffect(() => { if (open) setSelectedId(initialDocument?.id ?? null); }, [initialDocument?.id, open]);
  const index = Math.max(0, items.findIndex((item) => item.id === selectedId));
  const current = items[index] ?? initialDocument;
  const move = React.useCallback((offset: number) => {
    if (items.length < 2) return;
    const next = (index + offset + items.length) % items.length;
    setSelectedId(items[next].id);
  }, [index, items]);
  React.useEffect(() => {
    if (!open) return undefined;
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'ArrowLeft') move(-1);
      if (event.key === 'ArrowRight') move(1);
    };
    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, [move, open]);
  return <Dialog open={open} onClose={onClose} fullWidth maxWidth="xl" aria-labelledby="attachment-gallery-title" slotProps={{ paper: { sx: { height: '88vh', maxHeight: 900 } } }}>
    <DialogTitle id="attachment-gallery-title" sx={{ minHeight: 48, py: 0.75, px: 1.25, display: 'flex', alignItems: 'center', gap: 1, borderBottom: '1px solid', borderColor: 'divider' }}>
      <Typography component="span" noWrap sx={{ flex: 1, minWidth: 0, fontSize: 15, fontWeight: 700 }}>{current?.name || current?.fileName || 'Attachment preview'}</Typography>
      {current && <Typography sx={{ color: 'text.secondary', fontSize: 11 }}>{index + 1} / {items.length}</Typography>}
      {current && (current.kind === 'File' || current.kind === 'Image') && <><Tooltip title="Download"><IconButton size="small" aria-label={`Download ${current.name}`} onClick={() => void documentApi.download(current)}><DownloadOutlined fontSize="small" /></IconButton></Tooltip><Tooltip title="Open in new tab"><IconButton size="small" aria-label={`Open ${current.name} in new tab`} onClick={() => void documentApi.preview(current)}><OpenInNewOutlined fontSize="small" /></IconButton></Tooltip></>}
      <Tooltip title="Close"><IconButton size="small" aria-label="Close attachment preview" onClick={onClose}><Close fontSize="small" /></IconButton></Tooltip>
    </DialogTitle>
    <DialogContent sx={{ p: 0, display: 'grid', gridTemplateColumns: { xs: '1fr', md: '230px minmax(0, 1fr)' }, minHeight: 0, bgcolor: '#f5f6f8' }}>
      <Paper square variant="outlined" sx={{ display: { xs: 'none', md: 'block' }, minHeight: 0, overflow: 'auto', borderWidth: 0, borderRightWidth: 1 }}>
        <Typography sx={{ px: 1.25, pt: 1.1, pb: 0.6, color: 'text.secondary', fontSize: 10.5, fontWeight: 700 }}>ATTACHMENTS ({items.length})</Typography>
        <List dense disablePadding>{items.map((item) => <ListItemButton key={item.id} selected={item.id === current?.id} onClick={() => setSelectedId(item.id)} sx={{ mx: 0.5, mb: 0.25, px: 1, borderRadius: 0.75 }}><Box sx={{ width: 32, display: 'grid', placeItems: 'center' }}>{fileIcon(item)}</Box><ListItemText primary={item.name || item.fileName} secondary={item.documentTypeName} slotProps={{ primary: { noWrap: true, sx: { fontSize: 11.5, fontWeight: 600 } }, secondary: { noWrap: true, sx: { fontSize: 10 } } }} /></ListItemButton>)}</List>
      </Paper>
      <Box sx={{ minWidth: 0, minHeight: 0, position: 'relative', display: 'grid', placeItems: 'stretch', p: { xs: 0.75, md: 1.25 } }}>
        {documents.isLoading && !current ? <Box sx={{ display: 'grid', placeItems: 'center' }}><CircularProgress /></Box> : current ? <Paper variant="outlined" sx={{ minWidth: 0, minHeight: 0, overflow: 'hidden', display: 'grid', placeItems: 'stretch', bgcolor: '#fff' }}><GalleryPreview item={current} /></Paper> : <Alert severity="info">No attachments are available.</Alert>}
        {items.length > 1 && <><IconButton aria-label="Previous attachment" onClick={() => move(-1)} sx={{ position: 'absolute', left: 18, top: '50%', transform: 'translateY(-50%)', bgcolor: 'rgba(255,255,255,.9)', boxShadow: 2, '&:hover': { bgcolor: '#fff' } }}><ChevronLeft /></IconButton><IconButton aria-label="Next attachment" onClick={() => move(1)} sx={{ position: 'absolute', right: 18, top: '50%', transform: 'translateY(-50%)', bgcolor: 'rgba(255,255,255,.9)', boxShadow: 2, '&:hover': { bgcolor: '#fff' } }}><ChevronRight /></IconButton></>}
      </Box>
    </DialogContent>
  </Dialog>;
}
