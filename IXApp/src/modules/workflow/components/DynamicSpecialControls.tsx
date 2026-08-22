import React from 'react';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  FormHelperText,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Paper,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import AttachFileOutlined from '@mui/icons-material/AttachFileOutlined';
import Add from '@mui/icons-material/Add';
import Close from '@mui/icons-material/Close';
import AudioFileOutlined from '@mui/icons-material/AudioFileOutlined';
import DescriptionOutlined from '@mui/icons-material/DescriptionOutlined';
import DeleteOutlined from '@mui/icons-material/DeleteOutlined';
import FolderZipOutlined from '@mui/icons-material/FolderZipOutlined';
import ImageOutlined from '@mui/icons-material/ImageOutlined';
import EditOutlined from '@mui/icons-material/EditOutlined';
import InsertDriveFileOutlined from '@mui/icons-material/InsertDriveFileOutlined';
import LocationOnOutlined from '@mui/icons-material/LocationOnOutlined';
import MyLocationOutlined from '@mui/icons-material/MyLocationOutlined';
import Remove from '@mui/icons-material/Remove';
import PictureAsPdfOutlined from '@mui/icons-material/PictureAsPdfOutlined';
import Search from '@mui/icons-material/Search';
import SaveOutlined from '@mui/icons-material/SaveOutlined';
import SlideshowOutlined from '@mui/icons-material/SlideshowOutlined';
import TableChartOutlined from '@mui/icons-material/TableChartOutlined';
import TextSnippetOutlined from '@mui/icons-material/TextSnippetOutlined';
import VideoFileOutlined from '@mui/icons-material/VideoFileOutlined';
import type { RenderableControl, RenderableValidation } from './DynamicControlRenderer';

const normalized = (value: string) => value.replace(/[^a-z0-9]/gi, '').toLocaleLowerCase();
const ruleOperand = (rule: RenderableValidation) => rule.value ?? rule.expression ?? '';
const formatBytes = (bytes: number) => {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 ** 2) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / 1024 ** 2).toFixed(1)} MB`;
};
const sizeLimitBytes = (value: string): number | null => {
  const match = value.trim().match(/^(\d+(?:\.\d+)?)\s*(b|kb|mb|gb)?$/i);
  if (!match) return null;
  const amount = Number(match[1]);
  const unit = (match[2] ?? 'mb').toLocaleLowerCase();
  return amount * ({ b: 1, kb: 1024, mb: 1024 ** 2, gb: 1024 ** 3 }[unit] ?? 1024 ** 2);
};

interface FileMetadata { name: string; size: number; type: string }
const fileTypeTile = (file: FileMetadata): { color: string; label: string; icon: React.ReactElement } => {
  const extension = file.name.split('.').pop()?.toLocaleLowerCase() ?? '';
  if (extension === 'pdf' || file.type === 'application/pdf') return { color: '#e53945', label: 'PDF', icon: <PictureAsPdfOutlined /> };
  if (['zip', 'rar', '7z', 'tar', 'gz'].includes(extension)) return { color: '#ffb938', label: 'ZIP', icon: <FolderZipOutlined /> };
  if (file.type.startsWith('audio/') || ['mp3', 'wav', 'aac', 'm4a', 'flac', 'ogg'].includes(extension)) return { color: '#8b3ff0', label: 'Audio', icon: <AudioFileOutlined /> };
  if (file.type.startsWith('video/') || ['mp4', 'mov', 'avi', 'mkv', 'webm'].includes(extension)) return { color: '#ff4a38', label: 'Video', icon: <VideoFileOutlined /> };
  if (file.type.startsWith('image/') || ['png', 'jpg', 'jpeg', 'gif', 'webp', 'bmp', 'svg'].includes(extension)) return { color: '#17b968', label: 'Image', icon: <ImageOutlined /> };
  if (['doc', 'docx', 'rtf'].includes(extension)) return { color: '#477fea', label: 'Word', icon: <DescriptionOutlined /> };
  if (['xls', 'xlsx', 'csv'].includes(extension)) return { color: '#08a765', label: 'Excel', icon: <TableChartOutlined /> };
  if (['ppt', 'pptx'].includes(extension)) return { color: '#ff7043', label: 'PowerPoint', icon: <SlideshowOutlined /> };
  if (['txt', 'log', 'md'].includes(extension)) return { color: '#434a55', label: 'TXT', icon: <TextSnippetOutlined /> };
  if (extension === 'psd') return { color: '#001e36', label: 'PS', icon: <Typography sx={{ color: '#31a8ff', fontSize: 11, fontWeight: 900 }}>Ps</Typography> };
  if (extension === 'ai') return { color: '#3b1b00', label: 'AI', icon: <Typography sx={{ color: '#ff9a00', fontSize: 11, fontWeight: 900 }}>Ai</Typography> };
  return { color: '#94a3b8', label: extension.toLocaleUpperCase() || 'FILE', icon: <InsertDriveFileOutlined /> };
};
export const readFileMetadata = (value: string): FileMetadata[] => {
  if (!value) return [];
  try {
    const parsed = JSON.parse(value) as unknown;
    return Array.isArray(parsed) ? parsed.flatMap((item) => {
      if (!item || typeof item !== 'object') return [];
      const candidate = item as Partial<FileMetadata> & { n?: string; s?: number; t?: string };
      const name = candidate.name ?? candidate.n; const size = candidate.size ?? candidate.s;
      return typeof name === 'string' && typeof size === 'number'
        ? [{ name, size, type: candidate.type ?? candidate.t ?? '' }]
        : [];
    }) : [];
  } catch { return []; }
};

export function FileDropControl({ control, value, onChange, onFilesChange, error, helperText, preview }: {
  control: RenderableControl;
  value: string;
  onChange: (value: string) => void;
  error?: boolean;
  helperText?: string;
  preview?: boolean;
  onFilesChange?: (files: File[]) => void;
}) {
  const inputRef = React.useRef<HTMLInputElement>(null);
  const previewUrlsRef = React.useRef<string[]>([]);
  const [dragging, setDragging] = React.useState(false);
  const [localError, setLocalError] = React.useState('');
  const [previewUrls, setPreviewUrls] = React.useState<Array<string | null>>([]);
  const [selectedFiles, setSelectedFiles] = React.useState<File[]>([]);
  const validations = control.validations ?? [];
  const metadata = readFileMetadata(value);
  const [previewIndex, setPreviewIndex] = React.useState<number | null>(null);
  const previewFile = previewIndex == null ? null : metadata[previewIndex] ?? null;
  const previewUrl = previewIndex == null ? null : previewUrls[previewIndex] ?? null;
  const maxRule = validations.find((rule) => ['maxfiles', 'maxselected'].includes(normalized(rule.type)));
  const maxFiles = maxRule ? Math.max(1, Number(ruleOperand(maxRule)) || 1) : Number.MAX_SAFE_INTEGER;
  const multiple = !maxRule || maxFiles > 1;
  const extensionRule = validations.find((rule) => ['fileextensions', 'fileextension', 'allowedextensions', 'allowedtypes'].includes(normalized(rule.type)));
  const extensions = ruleOperand(extensionRule ?? { type: '' }).split(',').map((item) => item.trim().toLocaleLowerCase().replace(/^\./, '')).filter(Boolean);
  const sizeRule = validations.find((rule) => ['filesize', 'maxfilesize'].includes(normalized(rule.type)));
  const maxSize = sizeLimitBytes(ruleOperand(sizeRule ?? { type: '' }));
  const accept = extensions.length ? extensions.map((item) => item.includes('/') ? item : `.${item}`).join(',') : undefined;

  const updatePreviewUrls = (files: File[], append: boolean) => {
    if (!append) previewUrlsRef.current.forEach((url) => URL.revokeObjectURL(url));
    const created = files.map((file) => {
      if (!(file.type.startsWith('image/') || file.type === 'application/pdf') || typeof URL.createObjectURL !== 'function') return null;
      return URL.createObjectURL(file);
    });
    const existing = append ? Array.from({ length: metadata.length }, (_, index) => previewUrls[index] ?? null) : [];
    const next = [...existing, ...created];
    previewUrlsRef.current = next.filter((url): url is string => Boolean(url));
    setPreviewUrls(next);
  };
  React.useEffect(() => () => previewUrlsRef.current.forEach((url) => URL.revokeObjectURL(url)), []);
  React.useEffect(() => {
    if (value) return;
    previewUrlsRef.current.forEach((url) => URL.revokeObjectURL(url));
    previewUrlsRef.current = [];
    setPreviewUrls([]);
    setSelectedFiles([]);
  }, [value]);

  const selectFiles = (incoming: File[]) => {
    const files = multiple ? incoming : incoming.slice(0, 1);
    const invalidExtension = files.find((file) => extensions.length > 0 && !extensions.includes(file.name.split('.').pop()?.toLocaleLowerCase() ?? '') && !extensions.includes(file.type.toLocaleLowerCase()));
    if (invalidExtension) {
      setLocalError(extensionRule?.errorMessage || `Allowed file types: ${extensions.join(', ')}.`);
      return;
    }
    const oversized = files.find((file) => maxSize != null && file.size > maxSize);
    if (oversized) {
      setLocalError(sizeRule?.errorMessage || `${oversized.name} exceeds the maximum size of ${formatBytes(maxSize!)}.`);
      return;
    }
    if ((multiple ? metadata.length : 0) + files.length > maxFiles) {
      setLocalError(maxRule?.errorMessage || `You can upload up to ${maxFiles} file${maxFiles === 1 ? '' : 's'}.`);
      return;
    }
    setLocalError('');
    updatePreviewUrls(files, multiple);
    const selected = files.map((file) => ({ name: file.name, size: file.size, type: file.type }));
    const nextMetadata = multiple ? [...metadata, ...selected] : selected;
    const nextFiles = multiple ? [...selectedFiles, ...files] : files;
    setSelectedFiles(nextFiles);
    onFilesChange?.(nextFiles);
    onChange(JSON.stringify(nextMetadata.map((file) => ({ n: file.name, s: file.size, t: file.type }))));
  };
  const remove = (index: number) => {
    const removedUrl = previewUrls[index];
    if (removedUrl) URL.revokeObjectURL(removedUrl);
    const nextPreviews = previewUrls.filter((_, itemIndex) => itemIndex !== index);
    previewUrlsRef.current = nextPreviews.filter((url): url is string => Boolean(url));
    setPreviewUrls(nextPreviews);
    const nextFiles = selectedFiles.filter((_, itemIndex) => itemIndex !== index);
    setSelectedFiles(nextFiles);
    onFilesChange?.(nextFiles);
    setPreviewIndex((current) => current === index ? null : current != null && current > index ? current - 1 : current);
    onChange(JSON.stringify(metadata.filter((_, itemIndex) => itemIndex !== index).map((file) => ({ n: file.name, s: file.size, t: file.type }))));
  };
  const removeAll = () => {
    previewUrlsRef.current.forEach((url) => URL.revokeObjectURL(url));
    previewUrlsRef.current = [];
    setPreviewUrls([]);
    setPreviewIndex(null);
    setLocalError('');
    setSelectedFiles([]);
    onFilesChange?.([]);
    onChange('[]');
  };
  const canEdit = !preview && !control.readOnly;

  return <FormControl error={Boolean(error || localError)} required={control.required} fullWidth sx={{ position: 'relative', display: 'grid', gridTemplateColumns: 'minmax(0, 1fr)', gap: control.hideLabel ? 0 : 0.45 }}>
    {!control.hideLabel && <>
      <Typography component="label" sx={{ minWidth: 0, pr: canEdit ? 7 : 0, fontSize: 12.5, lineHeight: 1.2, fontWeight: 700 }}>{control.label}{control.required ? ' *' : ''}</Typography>
      {canEdit && <Stack direction="row" spacing={0.25} sx={{ position: 'absolute', zIndex: 1, top: -4.5, right: 0 }}>
        <Tooltip title={metadata.length ? 'Upload More Files' : 'Upload Files'} arrow><IconButton size="small" color="primary" aria-label={metadata.length ? 'Upload More Files' : 'Upload Files'} onClick={() => inputRef.current?.click()} sx={{ width: 24, height: 24, borderRadius: 0.75 }}><AttachFileOutlined sx={{ fontSize: 17 }} /></IconButton></Tooltip>
        {metadata.length > 0 && <Tooltip title="Remove All Files" arrow><IconButton size="small" aria-label="Remove All Files" onClick={removeAll} sx={{ width: 24, height: 24, borderRadius: 0.75 }}><DeleteOutlined sx={{ fontSize: 17 }} /></IconButton></Tooltip>}
      </Stack>}
    </>}
    <Box
      role={control.compact ? undefined : 'button'}
      tabIndex={control.compact ? -1 : canEdit ? 0 : -1}
      aria-label={`Upload ${control.label}`}
      onClick={() => !control.compact && canEdit && inputRef.current?.click()}
      onKeyDown={(event) => { if (canEdit && (event.key === 'Enter' || event.key === ' ')) inputRef.current?.click(); }}
      onDragOver={(event) => { event.preventDefault(); if (canEdit) setDragging(true); }}
      onDragLeave={() => setDragging(false)}
      onDrop={(event) => { event.preventDefault(); setDragging(false); if (canEdit) selectFiles(Array.from(event.dataTransfer.files)); }}
      sx={{
        height: metadata.length === 0 ? 30 : undefined, minHeight: metadata.length === 0 ? 30 : control.compact ? 30 : 44, maxHeight: 156, boxSizing: 'border-box', border: '1px dashed', borderStyle: control.compact ? 'solid' : 'dashed', borderColor: error || localError ? 'error.main' : dragging ? 'primary.main' : '#c9c9c9',
        borderRadius: 0.75, bgcolor: dragging ? 'action.hover' : '#fafbfc', display: 'grid', alignItems: 'center',
        cursor: control.compact ? 'default' : canEdit ? 'pointer' : 'default', p: control.compact ? 0.35 : 0.65, overflow: 'hidden', transition: '120ms ease',
        '&:focus-visible': { outline: '2px solid', outlineColor: 'primary.main', outlineOffset: 2 },
      }}
    >
      <Stack spacing={0.45} sx={{ width: '100%', minWidth: 0, minHeight: 0 }}>
        {control.compact && metadata.length === 0 && <Stack direction="row" spacing={0.55} sx={{ height: 24, minHeight: 24, px: 0.4, alignItems: 'center', minWidth: 0 }}>
          <AttachFileOutlined color="primary" sx={{ fontSize: 16, flexShrink: 0 }} />
          <Typography sx={{ flex: 1, minWidth: 0, fontSize: 11.5, lineHeight: 1.2, fontWeight: 650 }}>{control.label}{control.required ? ' *' : ''}</Typography>
          {canEdit && <Button size="small" aria-label={`Attach ${control.label}`} onClick={() => inputRef.current?.click()} sx={{ minWidth: 45, minHeight: 24, py: 0, fontSize: 10.5 }}>Attach</Button>}
        </Stack>}
        {!control.compact && metadata.length === 0 && <Stack direction="row" spacing={0.75} sx={{ height: 24, minHeight: 24, px: 0.5, alignItems: 'center', justifyContent: 'center', minWidth: 0 }}>
          <AttachFileOutlined color="primary" sx={{ fontSize: 19, flexShrink: 0 }} />
          <Box sx={{ minWidth: 0 }}>
            <Typography sx={{ fontSize: 12, lineHeight: 1.25, fontWeight: 650 }}>Drop {multiple ? 'documents' : 'a document'} here or <Box component="span" sx={{ color: 'primary.main' }}>Browse</Box></Typography>
            {(extensions.length > 0 || maxSize != null) && <Typography color="text.secondary" noWrap sx={{ display: 'none', fontSize: 10.5, lineHeight: 1.2 }}>
              {[extensions.length ? extensions.join(', ') : '', maxSize != null ? `Maximum ${formatBytes(maxSize)}` : ''].filter(Boolean).join(' · ')}
            </Typography>}
          </Box>
        </Stack>}
        {control.compact && metadata.length > 0 && <Box data-testid="compact-attachments" sx={{ width: '100%', display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: 0.5, px: 0.25, py: 0.15 }}>
          {metadata.map((file, index) => <Tooltip key={`${file.name}-${index}`} title={`${file.name} · ${formatBytes(file.size)}`} arrow>
            <Chip
              size="small"
              icon={<InsertDriveFileOutlined sx={{ color: `${fileTypeTile(file).color} !important` }} />}
              label={file.name}
              aria-label={`Preview ${file.name}`}
              onClick={() => setPreviewIndex(index)}
              onDelete={canEdit ? () => remove(index) : undefined}
              deleteIcon={canEdit ? <Close aria-label={`Remove ${file.name}`} /> : undefined}
              sx={{ height: 26, maxWidth: 160, borderRadius: 0.75, bgcolor: '#fff', border: '1px solid', borderColor: 'divider', '& .MuiChip-label': { minWidth: 0, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', px: 0.6, fontSize: 10.5 }, '& .MuiChip-icon': { ml: 0.5, fontSize: 15 }, '& .MuiChip-deleteIcon': { mr: 0.35, fontSize: 15 } }}
            />
          </Tooltip>)}
          {canEdit && <Button size="small" aria-label="Add another file" startIcon={<Add sx={{ fontSize: 14 }} />} onClick={() => inputRef.current?.click()} sx={{ minWidth: 48, minHeight: 26, py: 0, px: 0.65, fontSize: 10.5 }}>Add</Button>}
        </Box>}
        {!control.compact && metadata.length > 0 && <Box sx={{ width: '100%', maxHeight: 150, overflowY: 'auto', display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(58px, 1fr))', gap: 0.55, alignItems: 'start' }}>
          {metadata.map((file, index) => {
            const filePreviewUrl = previewUrls[index];
            const image = file.type.startsWith('image/') || /\.(png|jpe?g|gif|webp|bmp|svg)$/i.test(file.name);
            const pdf = file.type === 'application/pdf' || /\.pdf$/i.test(file.name);
            const tile = fileTypeTile(file);
            return <Paper key={`${file.name}-${index}`} aria-label={`${file.name}, ${formatBytes(file.size)}`} elevation={0} sx={{ minWidth: 0, position: 'relative', bgcolor: 'transparent', textAlign: 'center' }}>
              <Box component="button" type="button" aria-label={`Preview ${file.name}`} onClick={(event) => { event.stopPropagation(); setPreviewIndex(index); }} sx={{ width: '100%', minWidth: 0, p: 0.25, border: 0, bgcolor: 'transparent', cursor: 'pointer', borderRadius: 0.6, '&:hover': { bgcolor: 'action.hover' }, '&:focus-visible': { outline: '2px solid', outlineColor: 'primary.main' } }}>
                <Box sx={{ width: 42, height: 42, mx: 'auto', display: 'grid', placeItems: 'center', overflow: 'hidden', borderRadius: 0.6, bgcolor: filePreviewUrl && (image || pdf) ? '#fff' : tile.color, color: '#fff', border: '1px solid', borderColor: filePreviewUrl && (image || pdf) ? 'divider' : tile.color, '& > svg': { fontSize: 25 } }}>
                  {filePreviewUrl && image ? <Box component="img" src={filePreviewUrl} alt="" sx={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                    : filePreviewUrl && pdf ? <Box component="embed" src={`${filePreviewUrl}#page=1&toolbar=0&navpanes=0`} type="application/pdf" aria-label={`Thumbnail of ${file.name}`} sx={{ width: '100%', height: '100%', pointerEvents: 'none' }} />
                      : tile.icon}
                </Box>
                <Typography title={file.name} noWrap sx={{ mt: 0.25, minWidth: 0, fontSize: 9.5, lineHeight: 1.2 }}>{file.name}</Typography>
              </Box>
              {!preview && !control.readOnly && <IconButton size="small" aria-label={`Remove ${file.name}`} onClick={(event) => { event.stopPropagation(); remove(index); }} sx={{ position: 'absolute', top: -3, right: -2, width: 19, height: 19, bgcolor: 'rgba(255,255,255,.94)', boxShadow: 1, '&:hover': { bgcolor: '#fff' } }}><Close sx={{ fontSize: 12 }} /></IconButton>}
            </Paper>;
          })}
        </Box>}
      </Stack>
      <input ref={inputRef} hidden type="file" multiple={multiple} accept={accept} onChange={(event) => {
        selectFiles(Array.from(event.target.files ?? [])); event.target.value = '';
      }} />
    </Box>
    {(localError || helperText) && <FormHelperText>{localError || helperText}</FormHelperText>}
    <Dialog open={previewFile != null} onClose={() => setPreviewIndex(null)} fullWidth maxWidth="md" aria-labelledby="file-preview-title" slotProps={{ paper: { sx: { maxHeight: '88vh' } } }}>
      <DialogTitle id="file-preview-title" sx={{ py: 1, px: 1.5, display: 'flex', alignItems: 'center', gap: 1, borderBottom: '1px solid', borderColor: 'divider' }}>
        <Typography component="span" noWrap sx={{ flex: 1, fontSize: 14, fontWeight: 700 }}>Preview {previewFile?.name}</Typography>
        <Typography color="text.secondary" sx={{ fontSize: 11 }}>{previewFile ? formatBytes(previewFile.size) : ''}</Typography>
        <IconButton size="small" aria-label="Close file preview" onClick={() => setPreviewIndex(null)}><Close fontSize="small" /></IconButton>
      </DialogTitle>
      <DialogContent sx={{ minHeight: { xs: 260, sm: 420 }, p: 1.25, display: 'grid', placeItems: 'center', bgcolor: '#f4f5f7' }}>
        {previewFile && previewUrl && (previewFile.type.startsWith('image/') || /\.(png|jpe?g|gif|webp|bmp|svg)$/i.test(previewFile.name)) && <Box component="img" src={previewUrl} alt={`Preview of ${previewFile.name}`} sx={{ maxWidth: '100%', maxHeight: '72vh', objectFit: 'contain' }} />}
        {previewFile && previewUrl && (previewFile.type === 'application/pdf' || /\.pdf$/i.test(previewFile.name)) && <Box component="embed" src={`${previewUrl}#toolbar=1&navpanes=0`} type="application/pdf" aria-label={`Preview of ${previewFile.name}`} sx={{ width: '100%', height: { xs: 300, sm: 560 }, border: 0 }} />}
        {previewFile && !previewUrl && <Stack spacing={1} sx={{ alignItems: 'center', color: 'text.secondary' }}><Box sx={{ width: 64, height: 64, display: 'grid', placeItems: 'center', borderRadius: 1, bgcolor: fileTypeTile(previewFile).color, color: '#fff', '& > svg': { fontSize: 36 } }}>{fileTypeTile(previewFile).icon}</Box><Typography sx={{ fontSize: 13, fontWeight: 650 }}>{previewFile.name}</Typography><Typography sx={{ fontSize: 11 }}>A visual preview is unavailable for this saved file.</Typography></Stack>}
      </DialogContent>
    </Dialog>
  </FormControl>;
}

type Point = [number, number];
type Stroke = Point[];
const parseSignature = (value: string): Stroke[] => {
  if (!value.startsWith('sig:')) return [];
  return value.slice(4).split('|').filter(Boolean).map((stroke) => stroke.split(';').flatMap((point) => {
    const [x, y] = point.split(',').map(Number); return Number.isFinite(x) && Number.isFinite(y) ? [[x, y] as Point] : [];
  })).filter((stroke) => stroke.length > 0);
};
const serializeSignature = (strokes: Stroke[]) => `sig:${strokes.map((stroke) => stroke.map(([x, y]) => `${Math.round(x)},${Math.round(y)}`).join(';')).join('|')}`;
const drawStrokes = (canvas: HTMLCanvasElement, strokes: Stroke[]) => {
  const context = canvas.getContext('2d'); if (!context) return;
  context.clearRect(0, 0, canvas.width, canvas.height); context.lineWidth = 2.2; context.lineCap = 'round'; context.lineJoin = 'round'; context.strokeStyle = '#111827';
  for (const stroke of strokes) { if (!stroke.length) continue; context.beginPath(); context.moveTo(...stroke[0]); for (const point of stroke.slice(1)) context.lineTo(...point); context.stroke(); }
};
function SignatureCanvas({ strokes, onChange }: { strokes: Stroke[]; onChange: (strokes: Stroke[]) => void }) {
  const canvasRef = React.useRef<HTMLCanvasElement>(null); const drawing = React.useRef(false);
  React.useEffect(() => { if (canvasRef.current) drawStrokes(canvasRef.current, strokes); }, [strokes]);
  const point = (event: React.PointerEvent<HTMLCanvasElement>): Point => { const rect = event.currentTarget.getBoundingClientRect(); return [(event.clientX - rect.left) * 600 / rect.width, (event.clientY - rect.top) * 220 / rect.height]; };
  const pointCount = strokes.reduce((total, stroke) => total + stroke.length, 0);
  return <Box sx={{ height: { xs: 130, sm: 150 }, maxHeight: 170, minHeight: 0, boxSizing: 'border-box', overflow: 'hidden', border: '1px solid', borderColor: 'divider', borderRadius: 0.75, bgcolor: '#f8f8f5', touchAction: 'none', '& canvas': { height: '100%' } }}>
    <canvas ref={canvasRef} width={600} height={220} aria-label="Signature drawing area" style={{ width: '100%', display: 'block', cursor: 'crosshair', touchAction: 'none' }}
      onPointerDown={(event) => { if (pointCount >= 28) return; drawing.current = true; event.currentTarget.setPointerCapture(event.pointerId); onChange([...strokes, [point(event)]]); }}
      onPointerMove={(event) => { if (!drawing.current || pointCount >= 28) return; const nextPoint = point(event); const currentStroke = strokes.at(-1); const previous = currentStroke?.at(-1); if (previous && Math.hypot(previous[0] - nextPoint[0], previous[1] - nextPoint[1]) < 12) return; const next = strokes.map((stroke, index) => index === strokes.length - 1 ? [...stroke, nextPoint] : stroke); onChange(next); }}
      onPointerUp={() => { drawing.current = false; }} onPointerCancel={() => { drawing.current = false; }} />
  </Box>;
}
export function SignatureControl({ control, value, onChange, error, helperText, preview }: { control: RenderableControl; value: string; onChange: (value: string) => void; error?: boolean; helperText?: string; preview?: boolean }) {
  const saved = parseSignature(value);
  const [open, setOpen] = React.useState(false); const [draft, setDraft] = React.useState<Stroke[]>(saved);
  const editorRef = React.useRef<HTMLDivElement>(null);
  const canEdit = !preview && !control.readOnly;
  const openDialog = () => { setDraft(saved); setOpen(true); };
  const saveSignature = () => { if (draft.length > 0) onChange(serializeSignature(draft)); setOpen(false); };
  const removeSignature = () => { onChange(''); setDraft([]); setOpen(false); };
  React.useEffect(() => {
    if (!open) return undefined;
    const saveOnOutsidePointer = (event: PointerEvent) => {
      if (editorRef.current?.contains(event.target as Node)) return;
      if (draft.length > 0) onChange(serializeSignature(draft));
      setOpen(false);
    };
    document.addEventListener('pointerdown', saveOnOutsidePointer, true);
    return () => document.removeEventListener('pointerdown', saveOnOutsidePointer, true);
  }, [draft, onChange, open]);
  return <FormControl error={error} required={control.required} fullWidth sx={{ display: 'grid', gridTemplateColumns: 'minmax(0, 1fr)', gap: 0.45 }}>
    <Typography component="label" sx={{ fontSize: 12.5, lineHeight: 1.2, fontWeight: 700 }}>{control.label}{control.required ? ' *' : ''}</Typography>
    {!open && <Paper aria-label={`${control.label} signature preview`} title={canEdit ? saved.length ? 'Click to edit signature' : 'Click to add signature' : undefined} onClick={canEdit ? openDialog : undefined} variant="outlined" sx={{ height: 30, minHeight: 30, maxHeight: 30, flex: '0 0 30px', position: 'relative', boxSizing: 'border-box', display: 'grid', placeItems: 'center', px: 1, overflow: 'hidden', bgcolor: saved.length ? '#fff' : '#fafbfc', borderStyle: saved.length ? 'solid' : 'dashed', cursor: canEdit ? 'pointer' : 'default', userSelect: canEdit ? 'none' : 'auto', transition: 'border-color 140ms ease, box-shadow 140ms ease', '&:hover': canEdit ? { borderColor: 'primary.main', boxShadow: '0 2px 8px rgba(15,23,42,.09)' } : undefined, '&:hover .saved-signature-actions, &:focus-within .saved-signature-actions': { opacity: 1, pointerEvents: 'auto' } }}>
      {saved.length > 0 ? <Box component="svg" viewBox="0 0 600 220" sx={{ width: '100%', height: 26 }} aria-label="Saved signature">
        {saved.map((stroke, index) => <polyline key={index} points={stroke.map((point) => point.join(',')).join(' ')} fill="none" stroke="#111827" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" />)}
      </Box> : <Stack direction="row" spacing={0.8} sx={{ width: '100%', alignItems: 'center', color: 'text.secondary' }}><EditOutlined color="primary" sx={{ fontSize: 19 }} /><Typography sx={{ flex: 1, fontSize: 12, fontWeight: 650 }}>No signature</Typography>{canEdit && <Button size="small" onClick={(event) => { event.stopPropagation(); openDialog(); }} sx={{ minWidth: 46 }}>Sign</Button>}</Stack>}
      {canEdit && saved.length > 0 && <Box className="saved-signature-actions" role="toolbar" aria-label="Saved signature actions" sx={{ position: 'absolute', zIndex: 3, inset: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 0.5, bgcolor: 'rgba(15,23,42,.56)', opacity: 0, pointerEvents: 'none', transition: 'opacity 150ms ease', '@media (hover: none)': { inset: 'auto 3px 3px auto', bgcolor: 'transparent', opacity: 1, pointerEvents: 'auto' } }}><Tooltip title="Edit Signature" arrow><IconButton aria-label="Edit Signature" onClick={(event) => { event.stopPropagation(); openDialog(); }} sx={{ width: 28, height: 28, bgcolor: '#fff', color: 'primary.main', boxShadow: 2, '&:hover': { bgcolor: '#f4f8fb' } }}><EditOutlined sx={{ fontSize: 16 }} /></IconButton></Tooltip><Tooltip title="Remove Signature" arrow><IconButton aria-label="Remove Signature" onClick={(event) => { event.stopPropagation(); removeSignature(); }} sx={{ width: 28, height: 28, bgcolor: '#fff', color: 'error.main', boxShadow: 2, '&:hover': { bgcolor: '#fff5f5' } }}><DeleteOutlined sx={{ fontSize: 16 }} /></IconButton></Tooltip></Box>}
    </Paper>}
    {helperText && <FormHelperText>{helperText}</FormHelperText>}
    {open && <Box ref={editorRef} sx={{ position: 'relative', '&:hover .signature-actions, &:focus-within .signature-actions': { opacity: 1, transform: 'translateY(0)', pointerEvents: 'auto' } }}>
      <SignatureCanvas strokes={draft} onChange={setDraft} />
      {canEdit && <Paper className="signature-actions" role="toolbar" aria-label="Signature actions" elevation={3} sx={{ position: 'absolute', zIndex: 2, top: 8, right: 8, display: 'flex', alignItems: 'center', gap: 0.25, p: 0.35, border: '1px solid', borderColor: 'divider', borderRadius: 1, bgcolor: 'rgba(255,255,255,.96)', opacity: 0, transform: 'translateY(-4px)', pointerEvents: 'none', transition: 'opacity 140ms ease, transform 140ms ease', '@media (hover: none)': { opacity: 1, transform: 'translateY(0)', pointerEvents: 'auto' } }}>
        <Tooltip title="Clear Signature" arrow><IconButton size="small" aria-label="Clear Signature" onClick={() => setDraft([])} sx={{ width: 30, height: 30, borderRadius: 0.75 }}><DeleteOutlined sx={{ fontSize: 17 }} /></IconButton></Tooltip>
        <Tooltip title="Cancel Signature" arrow><IconButton size="small" aria-label="Cancel Signature" onClick={() => setOpen(false)} sx={{ width: 30, height: 30, borderRadius: 0.75 }}><Close sx={{ fontSize: 17 }} /></IconButton></Tooltip>
        <Tooltip title="Save Signature" arrow><span><IconButton size="small" color="primary" aria-label="Save Signature" disabled={draft.length === 0} onClick={saveSignature} sx={{ width: 30, height: 30, borderRadius: 0.75 }}><SaveOutlined sx={{ fontSize: 17 }} /></IconButton></span></Tooltip>
      </Paper>}
    </Box>}
  </FormControl>;
}

interface LocationValue { address: string; latitude: number; longitude: number }
const readLocation = (value: string): LocationValue | null => { try { const item = JSON.parse(value) as Partial<LocationValue>; return typeof item.latitude === 'number' && typeof item.longitude === 'number' ? { address: item.address ?? '', latitude: item.latitude, longitude: item.longitude } : null; } catch { return null; } };
const locationValue = (item: LocationValue) => JSON.stringify({ address: item.address.slice(0, 130), latitude: Number(item.latitude.toFixed(6)), longitude: Number(item.longitude.toFixed(6)) });
const reverseAddress = async (latitude: number, longitude: number) => {
  const response = await fetch(`https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${latitude}&lon=${longitude}`);
  if (!response.ok) return '';
  const result = await response.json() as { display_name?: string }; return result.display_name ?? '';
};
function LocationThumbnail({ latitude, longitude }: { latitude: number; longitude: number }) {
  const zoom = 13; const scale = 256 * 2 ** zoom;
  const safeLatitude = Math.max(-85.0511, Math.min(85.0511, latitude));
  const safeLongitude = Math.max(-180, Math.min(180, longitude));
  const center = {
    x: (safeLongitude + 180) / 360 * scale,
    y: (1 - Math.log(Math.tan(safeLatitude * Math.PI / 180) + 1 / Math.cos(safeLatitude * Math.PI / 180)) / Math.PI) / 2 * scale,
  };
  const tileX = Math.floor(center.x / 256); const tileY = Math.floor(center.y / 256);
  const tiles = Array.from({ length: 9 }, (_, index) => ({ x: tileX + index % 3 - 1, y: tileY + Math.floor(index / 3) - 1 }));
  return <Box aria-label="Selected location map preview" sx={{ width: 44, height: '100%', minHeight: 0, flexShrink: 0, position: 'relative', overflow: 'hidden', borderRadius: 0.5, bgcolor: '#dbeafe', border: '1px solid', borderColor: 'divider' }}>
    {tiles.map((tile) => <Box component="img" draggable={false} key={`${tile.x}-${tile.y}`} src={`https://tile.openstreetmap.org/${zoom}/${tile.x}/${tile.y}.png`} alt="" sx={{ position: 'absolute', width: 256, height: 256, maxWidth: 'none', left: `calc(50% + ${tile.x * 256 - center.x}px)`, top: `calc(50% + ${tile.y * 256 - center.y}px)` }} />)}
    <LocationOnOutlined color="error" sx={{ position: 'absolute', left: '50%', top: '50%', fontSize: 27, transform: 'translate(-50%, -100%)', filter: 'drop-shadow(0 1px 2px white)', pointerEvents: 'none' }} />
  </Box>;
}
function MapPicker({ latitude, longitude, onSelect }: { latitude: number; longitude: number; onSelect: (latitude: number, longitude: number) => void }) {
  const [zoom, setZoom] = React.useState(13); const scale = 256 * 2 ** zoom;
  const project = (lat: number, lon: number) => { const safeLatitude = Math.max(-85.0511, Math.min(85.0511, lat)); const safeLongitude = Math.max(-180, Math.min(180, lon)); return { x: (safeLongitude + 180) / 360 * scale, y: (1 - Math.log(Math.tan(safeLatitude * Math.PI / 180) + 1 / Math.cos(safeLatitude * Math.PI / 180)) / Math.PI) / 2 * scale }; };
  const unproject = (x: number, y: number) => ({ longitude: x / scale * 360 - 180, latitude: Math.atan(Math.sinh(Math.PI * (1 - 2 * y / scale))) * 180 / Math.PI });
  const center = project(latitude, longitude); const tileX = Math.floor(center.x / 256); const tileY = Math.floor(center.y / 256);
  const tiles = Array.from({ length: 25 }, (_, index) => ({ x: tileX + index % 5 - 2, y: tileY + Math.floor(index / 5) - 2 }));
  return <Box role="application" aria-label="Location map" onClick={(event) => { const rect = event.currentTarget.getBoundingClientRect(); const point = unproject(center.x + event.clientX - rect.left - rect.width / 2, center.y + event.clientY - rect.top - rect.height / 2); onSelect(point.latitude, point.longitude); }}
    sx={{ height: { xs: 190, sm: 220 }, position: 'relative', overflow: 'hidden', bgcolor: '#dbeafe', cursor: 'crosshair', border: '1px solid', borderColor: 'divider', borderRadius: 1.25, boxShadow: 'inset 0 0 0 1px rgba(255,255,255,.35)' }}>
    {tiles.map((tile) => <Box component="img" draggable={false} key={`${tile.x}-${tile.y}`} src={`https://tile.openstreetmap.org/${zoom}/${tile.x}/${tile.y}.png`} alt="" sx={{ position: 'absolute', width: 256, height: 256, maxWidth: 'none', left: `calc(50% + ${tile.x * 256 - center.x}px)`, top: `calc(50% + ${tile.y * 256 - center.y}px)` }} />)}
    <LocationOnOutlined color="error" sx={{ position: 'absolute', left: '50%', top: '50%', fontSize: 42, transform: 'translate(-50%, -100%)', filter: 'drop-shadow(0 1px 2px white)', pointerEvents: 'none' }} />
    <Paper variant="outlined" sx={{ position: 'absolute', top: 8, left: 8, display: 'grid', bgcolor: 'rgba(255,255,255,.94)', overflow: 'hidden' }} onClick={(event) => event.stopPropagation()}>
      <Tooltip title="Zoom in" placement="right"><IconButton size="small" aria-label="Zoom in" disabled={zoom >= 18} onClick={() => setZoom((current) => Math.min(18, current + 1))} sx={{ width: 30, height: 30, borderRadius: 0 }}><Add sx={{ fontSize: 17 }} /></IconButton></Tooltip>
      <Tooltip title="Zoom out" placement="right"><IconButton size="small" aria-label="Zoom out" disabled={zoom <= 3} onClick={() => setZoom((current) => Math.max(3, current - 1))} sx={{ width: 30, height: 30, borderRadius: 0, borderTop: '1px solid', borderColor: 'divider' }}><Remove sx={{ fontSize: 17 }} /></IconButton></Tooltip>
    </Paper>
    <Typography variant="caption" sx={{ position: 'absolute', right: 4, bottom: 2, bgcolor: 'rgba(255,255,255,.8)', px: 0.5 }}>© OpenStreetMap</Typography>
  </Box>;
}
export function LocationControl({ control, value, onChange, error, helperText, preview }: { control: RenderableControl; value: string; onChange: (value: string) => void; error?: boolean; helperText?: string; preview?: boolean }) {
  const saved = readLocation(value); const [open, setOpen] = React.useState(false); const [draft, setDraft] = React.useState<LocationValue>(saved ?? { address: '', latitude: 0, longitude: 0 });
  const canEdit = !preview && !control.readOnly;
  const [query, setQuery] = React.useState(''); const [results, setResults] = React.useState<LocationValue[]>([]); const [busy, setBusy] = React.useState(false); const [resolvingLocation, setResolvingLocation] = React.useState(false); const [dialogError, setDialogError] = React.useState(''); const [locationPermissionBlocked, setLocationPermissionBlocked] = React.useState(false); const [hasSelection, setHasSelection] = React.useState(Boolean(saved));
  const selectCoordinates = async (latitude: number, longitude: number) => { setResolvingLocation(true); setHasSelection(true); setDraft((current) => ({ ...current, latitude, longitude })); try { const address = await reverseAddress(latitude, longitude); setDraft({ address, latitude, longitude }); } catch { /* coordinates remain usable */ } finally { setResolvingLocation(false); } };
  const search = async () => { if (!query.trim()) return; setBusy(true); setDialogError(''); setResults([]); try { const response = await fetch(`https://nominatim.openstreetmap.org/search?format=jsonv2&limit=5&q=${encodeURIComponent(query.trim())}`); if (!response.ok) throw new Error('Location search failed.'); const data = await response.json() as Array<{ display_name: string; lat: string; lon: string }>; const nextResults = data.map((item) => ({ address: item.display_name, latitude: Number(item.lat), longitude: Number(item.lon) })); setResults(nextResults); if (nextResults.length === 0) setDialogError('No locations were found for this address.'); } catch (reason) { setDialogError(reason instanceof Error ? reason.message : 'Location search failed.'); } finally { setBusy(false); } };
  const currentLocation = () => { if (!navigator.geolocation) { setLocationPermissionBlocked(false); setDialogError('Device location is not available.'); return; } setBusy(true); setDialogError(''); setLocationPermissionBlocked(false); navigator.geolocation.getCurrentPosition((position) => { setLocationPermissionBlocked(false); void selectCoordinates(position.coords.latitude, position.coords.longitude).finally(() => setBusy(false)); }, (reason) => { const blocked = reason.code === 1; setLocationPermissionBlocked(blocked); setDialogError(blocked ? 'Location access is blocked for this site. Change the browser permission or select a point manually on the map.' : reason.message || 'Unable to access device location.'); setBusy(false); }, { enableHighAccuracy: true, timeout: 12000 }); };
  const openMap = () => { setDraft(saved ?? { address: '', latitude: 0, longitude: 0 }); setHasSelection(Boolean(saved)); setResolvingLocation(false); setQuery(''); setResults([]); setDialogError(''); setLocationPermissionBlocked(false); setOpen(true); if (!saved) currentLocation(); };
  const removeLocation = () => { onChange(''); setDraft({ address: '', latitude: 0, longitude: 0 }); setHasSelection(false); setOpen(false); };
  const validSelection = hasSelection && Number.isFinite(draft.latitude) && Number.isFinite(draft.longitude) && Math.abs(draft.latitude) <= 85.0511 && Math.abs(draft.longitude) <= 180;
  const dialogTitle = control.label.trim().toLocaleLowerCase() === 'new field' ? 'Select Location' : control.label;
  return <FormControl error={error} required={control.required} fullWidth sx={{ display: 'grid', gridTemplateColumns: 'minmax(0, 1fr)', gap: 0.45, '& > .MuiPaper-root': { height: 30, minHeight: 30, maxHeight: 30, flexBasis: 30 }, '& > .MuiPaper-root .MuiButton-root': { height: 24, minHeight: 24 } }}>
    <Typography component="label" sx={{ fontSize: 12.5, lineHeight: 1.2, fontWeight: 700 }}>{control.label}{control.required ? ' *' : ''}</Typography>
    {saved ? <Paper aria-label={`${control.label} location preview`} onClick={canEdit ? openMap : undefined} variant="outlined" sx={{ height: 40, minHeight: 40, maxHeight: 40, flex: '0 0 40px', position: 'relative', boxSizing: 'border-box', p: 0.25, display: 'flex', alignItems: 'stretch', overflow: 'hidden', bgcolor: '#fff', cursor: canEdit ? 'pointer' : 'default', userSelect: canEdit ? 'none' : 'auto', transition: 'border-color 140ms ease, box-shadow 140ms ease', '&:hover': canEdit ? { borderColor: 'primary.main', boxShadow: '0 2px 8px rgba(15,23,42,.09)' } : undefined, '&:hover .location-actions, &:focus-within .location-actions': { opacity: 1, pointerEvents: 'auto' } }}><Stack direction="row" spacing={0.65} sx={{ width: '100%', minWidth: 0, alignItems: 'stretch' }}><LocationThumbnail latitude={saved.latitude} longitude={saved.longitude} /><Stack spacing={0} sx={{ minWidth: 0, flex: 1, justifyContent: 'center' }}><Typography title={saved.address || 'Selected location'} noWrap sx={{ fontSize: 11.5, lineHeight: 1.2, fontWeight: 700 }}>{saved.address || 'Selected location'}</Typography><Typography component="span" noWrap sx={{ color: 'text.secondary', fontSize: 9, lineHeight: 1.15, fontFamily: 'monospace' }}>{saved.latitude.toFixed(6)}, {saved.longitude.toFixed(6)}</Typography></Stack></Stack>{canEdit && <Box className="location-actions" role="toolbar" aria-label="Location actions" sx={{ position: 'absolute', zIndex: 3, inset: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 0.5, bgcolor: 'rgba(15,23,42,.56)', opacity: 0, pointerEvents: 'none', transition: 'opacity 150ms ease', '@media (hover: none)': { inset: 'auto 3px 3px auto', bgcolor: 'transparent', opacity: 1, pointerEvents: 'auto' } }}><Tooltip title="Edit Location" arrow><IconButton aria-label="Edit Location" onClick={(event) => { event.stopPropagation(); openMap(); }} sx={{ width: 28, height: 28, bgcolor: '#fff', color: 'primary.main', boxShadow: 2, '&:hover': { bgcolor: '#f4f8fb' } }}><EditOutlined sx={{ fontSize: 16 }} /></IconButton></Tooltip><Tooltip title="Remove Location" arrow><IconButton aria-label="Remove Location" onClick={(event) => { event.stopPropagation(); removeLocation(); }} sx={{ width: 28, height: 28, bgcolor: '#fff', color: 'error.main', boxShadow: 2, '&:hover': { bgcolor: '#fff5f5' } }}><DeleteOutlined sx={{ fontSize: 16 }} /></IconButton></Tooltip></Box>}</Paper> : <Paper aria-label={`${control.label} location preview`} title={canEdit ? 'Click to add location' : undefined} onClick={canEdit ? openMap : undefined} variant="outlined" sx={{ height: 40, minHeight: 40, maxHeight: 40, flex: '0 0 40px', boxSizing: 'border-box', px: 1, display: 'flex', alignItems: 'center', color: 'text.secondary', bgcolor: '#fafbfc', borderStyle: 'dashed', cursor: canEdit ? 'pointer' : 'default', userSelect: canEdit ? 'none' : 'auto', transition: 'border-color 140ms ease, background-color 140ms ease', '&:hover': canEdit ? { borderColor: 'primary.main', bgcolor: 'rgba(0, 91, 161, .035)' } : undefined }}><Stack direction="row" spacing={0.75} sx={{ width: '100%', alignItems: 'center' }}><LocationOnOutlined color="primary" sx={{ fontSize: 18 }} /><Typography sx={{ flex: 1, fontSize: 12, fontWeight: 650 }}>No location selected</Typography>{canEdit && <Button size="small" aria-label="Open Map" onClick={(event) => { event.stopPropagation(); openMap(); }} sx={{ minWidth: 52, height: 30, minHeight: 30, py: 0 }}>Choose</Button>}</Stack></Paper>}
    {helperText && <FormHelperText>{helperText}</FormHelperText>}
    <Dialog open={open} onClose={() => setOpen(false)} aria-labelledby="location-dialog-title" fullWidth maxWidth="sm" slotProps={{ paper: { sx: { width: { xs: 'calc(100% - 24px)', sm: 520 }, borderRadius: 2, maxHeight: '88vh', boxShadow: '0 18px 55px rgba(15,23,42,.24)' } } }}>
      <DialogTitle id="location-dialog-title" sx={{ minHeight: 48, py: 1, px: 2, display: 'flex', alignItems: 'center', justifyContent: 'space-between', borderBottom: '1px solid', borderColor: 'divider' }}><Typography component="span" sx={{ fontSize: 16, fontWeight: 700 }}>{dialogTitle}</Typography><Tooltip title="Close"><IconButton size="small" aria-label="Close location dialog" onClick={() => setOpen(false)} sx={{ width: 28, height: 28 }}><Close sx={{ fontSize: 18 }} /></IconButton></Tooltip></DialogTitle>
      <DialogContent sx={{ p: { xs: 1.25, sm: 1.5 }, bgcolor: '#fafbfc' }}><Stack spacing={1.1}>
        <Box sx={{ height: 40, display: 'grid', gridTemplateColumns: 'minmax(0, 1fr) 40px 40px', alignItems: 'stretch', gap: 0.75 }}><TextField fullWidth size="small" placeholder="Search for an address" value={query} onChange={(event) => setQuery(event.target.value)} onKeyDown={(event) => { if (event.key === 'Enter') { event.preventDefault(); void search(); } }} slotProps={{ htmlInput: { 'aria-label': 'Search for an address' } }} sx={{ '& .MuiInputBase-root': { height: 40, boxSizing: 'border-box' } }} /><Tooltip title="Search"><span style={{ display: 'block', width: 40, height: 40 }}><IconButton aria-label="Search" disabled={busy || !query.trim()} onClick={() => void search()} sx={{ width: 40, height: 40, boxSizing: 'border-box', border: '1px solid', borderColor: 'divider', borderRadius: 0.75 }}>{busy ? <CircularProgress size={17} /> : <Search sx={{ fontSize: 19 }} />}</IconButton></span></Tooltip><Tooltip title="Use current location"><span style={{ display: 'block', width: 40, height: 40 }}><IconButton aria-label="Use current location" disabled={busy} onClick={currentLocation} sx={{ width: 40, height: 40, boxSizing: 'border-box', border: '1px solid', borderColor: 'divider', borderRadius: 0.75 }}><MyLocationOutlined sx={{ fontSize: 19 }} /></IconButton></span></Tooltip></Box>
        {dialogError && <Alert severity={locationPermissionBlocked ? 'warning' : 'error'}>{dialogError}</Alert>}
        {results.length > 0 && <Paper variant="outlined"><List dense disablePadding>{results.map((item) => <ListItem key={`${item.latitude}-${item.longitude}`} disablePadding><ListItemButton onClick={() => { setDraft(item); setHasSelection(true); setResults([]); }}><ListItemText primary={item.address} secondary={`${item.latitude.toFixed(6)}, ${item.longitude.toFixed(6)}`} /></ListItemButton></ListItem>)}</List></Paper>}
        <MapPicker latitude={draft.latitude} longitude={draft.longitude} onSelect={(latitude, longitude) => void selectCoordinates(latitude, longitude)} />
        <TextField size="small" label="Selected address" value={hasSelection ? draft.address : ''} placeholder="Select a point on the map" slotProps={{ input: { readOnly: true } }} />
        <Stack direction="row" spacing={1}><TextField fullWidth size="small" label="Latitude" value={hasSelection ? draft.latitude.toFixed(6) : ''} slotProps={{ input: { readOnly: true } }} sx={{ '& .MuiInputBase-input': { color: 'text.secondary', fontSize: 12.5 } }} /><TextField fullWidth size="small" label="Longitude" value={hasSelection ? draft.longitude.toFixed(6) : ''} slotProps={{ input: { readOnly: true } }} sx={{ '& .MuiInputBase-input': { color: 'text.secondary', fontSize: 12.5 } }} /></Stack>
      </Stack></DialogContent>
      <DialogActions sx={{ position: 'sticky', bottom: 0, px: 2, py: 1, borderTop: '1px solid', borderColor: 'divider', bgcolor: 'background.paper' }}><Button size="small" onClick={() => setOpen(false)}>Cancel</Button><Button size="small" variant="contained" disabled={busy || resolvingLocation || !validSelection} onClick={() => { onChange(locationValue(draft)); setOpen(false); }}>{resolvingLocation ? 'Loading Location…' : 'Save Location'}</Button></DialogActions>
    </Dialog>
  </FormControl>;
}
