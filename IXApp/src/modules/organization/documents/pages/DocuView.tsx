import React from 'react';
import { Alert, Box, Button, CircularProgress, IconButton, Stack, TextField, Tooltip, Typography } from '@mui/material';
import DeleteOutlined from '@mui/icons-material/DeleteOutlined';
import OpenInNewOutlined from '@mui/icons-material/OpenInNewOutlined';
import { useQuery } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailValues, EnterpriseListDetailsConfig, ListDetailRecord } from '@patterns/list-details/types';
import { AttachmentGalleryDialog } from '@shared/components/documents/AttachmentGalleryDialog';
import { documentApi, type DocumentDto } from '@shared/components/documents/documentApi';

interface DocuViewRecord extends Omit<DocumentDto, 'id'>, ListDetailRecord {
  documentId: number;
  pendingFiles: File[];
}

const toRecord = (document: DocumentDto): DocuViewRecord => ({
  ...document,
  id: String(document.id),
  documentId: document.id,
  pendingFiles: [],
});
const toDocument = (record: DocuViewRecord): DocumentDto => {
  const { documentId, pendingFiles: _pendingFiles, id: _id, ...document } = record;
  return { ...document, id: documentId };
};

const formatBytes = (value: number) => value < 1024
  ? `${value} B`
  : value < 1024 ** 2
    ? `${(value / 1024).toFixed(1)} KB`
    : `${(value / 1024 ** 2).toFixed(1)} MB`;

const fieldSx = {
  width: 220,
  '& .MuiInputBase-root': { height: 30, fontSize: 12 },
  '& .MuiInputBase-input': { px: 0.5, py: 0.25 },
};

function AttachmentDetails({ record, editing, onChange }: { record: DocuViewRecord; editing: boolean; onChange: (record: DocuViewRecord) => void }): React.ReactElement {
  const fileKind = record.kind === 'File' || record.kind === 'Image';
  return <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(3, minmax(220px, 270px))' }, gap: 6 }}>
    <Box>
      <Typography sx={{ mb: 1.5, fontSize: 11, fontWeight: 700 }}>FILE INFORMATION</Typography>
      <Typography sx={{ mb: 0.5, fontSize: 12 }}>File name</Typography>
      {editing && fileKind && record.documentId === 0
        ? <Stack spacing={0.75} sx={{ width: 220 }}>
            <Button component="label" variant="outlined" size="small" sx={{ width: 220, justifyContent: 'flex-start', textTransform: 'none' }}>
              {record.pendingFiles.length > 0 ? `${record.pendingFiles.length} file${record.pendingFiles.length === 1 ? '' : 's'} selected` : record.fileName || 'Choose files'}
              <input hidden multiple type="file" onChange={(event) => {
                const files = Array.from(event.target.files ?? []);
                onChange({ ...record, pendingFiles: files, fileName: files.length === 1 ? files[0].name : record.fileName });
                event.target.value = '';
              }} />
            </Button>
            {record.pendingFiles.map((file, index) => <Stack key={`${file.name}-${file.size}-${file.lastModified}`} direction="row" spacing={0.5} sx={{ alignItems: 'center', minWidth: 0 }}>
              <Typography title={file.name} noWrap sx={{ flex: 1, minWidth: 0, fontSize: 11 }}>{file.name}</Typography>
              <Typography sx={{ flexShrink: 0, color: 'text.secondary', fontSize: 10 }}>{formatBytes(file.size)}</Typography>
              <Tooltip title="Remove"><IconButton size="small" aria-label={`Remove ${file.name}`} onClick={() => onChange({ ...record, pendingFiles: record.pendingFiles.filter((_, fileIndex) => fileIndex !== index) })} sx={{ width: 24, height: 24 }}><DeleteOutlined sx={{ fontSize: 15 }} /></IconButton></Tooltip>
            </Stack>)}
          </Stack>
        : <Typography noWrap sx={{ width: 220, minHeight: 28, px: 0.5, borderBottom: '1px solid #605e5c', fontSize: 12 }}>{record.fileName || '—'}</Typography>}
    </Box>
    <Box>
      <Typography sx={{ mb: 1.5, fontSize: 11, fontWeight: 700 }}>FILE DETAILS</Typography>
      <Typography sx={{ mb: 0.5, fontSize: 12 }}>File type</Typography>
      <Typography sx={{ width: 220, minHeight: 28, px: 0.5, borderBottom: '1px solid #605e5c', fontSize: 12 }}>{record.fileType || record.kind}</Typography>
      <Typography sx={{ mt: 1.25, mb: 0.5, fontSize: 12 }}>Original file name</Typography>
      <Typography noWrap sx={{ width: 220, minHeight: 28, px: 0.5, borderBottom: '1px solid #605e5c', fontSize: 12 }}>{record.originalFileName || (record.pendingFiles.length === 1 ? record.pendingFiles[0].name : record.pendingFiles.length > 1 ? `${record.pendingFiles.length} files` : '—')}</Typography>
    </Box>
    <Box>
      <Typography sx={{ mb: 1.5, fontSize: 11, fontWeight: 700 }}>FILE LOCATION</Typography>
      <Typography sx={{ mb: 0.5, fontSize: 12 }}>{record.kind === 'URL' ? 'URL' : 'File location'}</Typography>
      {editing && record.kind === 'URL'
        ? <TextField variant="standard" value={record.url ?? ''} onChange={(event) => onChange({ ...record, url: event.target.value })} sx={fieldSx} />
        : <Typography noWrap sx={{ width: 220, minHeight: 28, px: 0.5, borderBottom: '1px solid #605e5c', fontSize: 12, color: record.kind === 'URL' ? 'primary.main' : 'inherit' }}>{record.kind === 'URL' ? record.url : fileKind ? 'Managed document storage' : '—'}</Typography>}
    </Box>
  </Box>;
}

function LocalFilePreview({ file }: { file: File }): React.ReactElement {
  const [url, setUrl] = React.useState('');
  React.useEffect(() => {
    const next = URL.createObjectURL(file);
    setUrl(next);
    return () => URL.revokeObjectURL(next);
  }, [file]);
  const open = () => {
    const next = URL.createObjectURL(file);
    window.open(next, '_blank', 'noopener,noreferrer');
    window.setTimeout(() => URL.revokeObjectURL(next), 60_000);
  };
  return <Box sx={{ minWidth: 0 }}>
    <Stack direction="row" spacing={0.75} sx={{ mb: 0.5, alignItems: 'center' }}>
      <Typography title={file.name} noWrap sx={{ flex: 1, minWidth: 0, fontSize: 12, fontWeight: 600 }}>{file.name}</Typography>
      <Typography sx={{ color: 'text.secondary', fontSize: 10 }}>{formatBytes(file.size)}</Typography>
      <Tooltip title="Open preview"><IconButton size="small" aria-label={`Preview ${file.name}`} onClick={open}><OpenInNewOutlined sx={{ fontSize: 16 }} /></IconButton></Tooltip>
    </Stack>
    {file.type.startsWith('image/') && url && <Box component="img" src={url} alt={file.name} sx={{ display: 'block', maxWidth: '100%', height: 180, objectFit: 'contain', border: '1px solid', borderColor: 'divider' }} />}
    {file.type === 'application/pdf' && url && <Box component="iframe" title={file.name} src={url} sx={{ width: '100%', height: 220, border: '1px solid', borderColor: 'divider' }} />}
  </Box>;
}

function DocumentPreview({ record, onOpen }: { record: DocuViewRecord; onOpen: () => void }): React.ReactElement {
  const preview = useQuery({
    queryKey: ['document-preview', record.documentId],
    queryFn: ({ signal }) => documentApi.previewBlob(record.documentId, signal),
    enabled: record.documentId > 0 && (record.kind === 'File' || record.kind === 'Image'),
  });
  const [url, setUrl] = React.useState<string | null>(null);
  React.useEffect(() => {
    if (!preview.data) { setUrl(null); return undefined; }
    const next = URL.createObjectURL(preview.data); setUrl(next);
    return () => URL.revokeObjectURL(next);
  }, [preview.data]);
  if (record.documentId === 0 && (record.kind === 'File' || record.kind === 'Image') && record.pendingFiles.length > 0) return <Box sx={{ display: 'grid', gridTemplateColumns: record.pendingFiles.length > 1 ? { xs: '1fr', md: 'repeat(2, minmax(0, 1fr))' } : 'minmax(0, 760px)', gap: 1.25 }}>{record.pendingFiles.map((file) => <LocalFilePreview key={`${file.name}-${file.size}-${file.lastModified}`} file={file} />)}</Box>;
  if (record.kind === 'Note') return <Typography sx={{ whiteSpace: 'pre-wrap', fontSize: 12 }}>{record.notes || 'No note content.'}</Typography>;
  if (record.kind === 'URL') return record.url
    ? <Button component="a" href={record.url} target="_blank" rel="noreferrer">Open URL</Button>
    : <Typography sx={{ color: 'text.secondary', fontSize: 12 }}>No URL is available.</Typography>;
  if (preview.isLoading) return <CircularProgress size={22} />;
  if (!url || preview.isError) return <Typography sx={{ color: 'text.secondary', fontSize: 12 }}>Preview is not available.</Typography>;
  if (record.contentType?.startsWith('image/')) return <Box component="button" type="button" aria-label={`Open ${record.name} gallery`} onClick={onOpen} sx={{ display: 'block', p: 0, border: 0, bgcolor: 'transparent', cursor: 'zoom-in' }}><Box component="img" src={url} alt={record.name} sx={{ display: 'block', maxWidth: '100%', maxHeight: 420, objectFit: 'contain' }} /></Box>;
  if (record.contentType === 'application/pdf') return <Box sx={{ position: 'relative' }}><Box component="iframe" title={record.name} src={url} sx={{ width: '100%', height: 420, border: 0 }} /><Button size="small" startIcon={<OpenInNewOutlined />} onClick={onOpen} sx={{ position: 'absolute', top: 8, right: 8, bgcolor: 'rgba(255,255,255,.94)', boxShadow: 1, '&:hover': { bgcolor: '#fff' } }}>Open gallery</Button></Box>;
  return <Button startIcon={<OpenInNewOutlined />} onClick={onOpen}>Open preview gallery</Button>;
}

export function DocuView(): React.ReactElement {
  const [searchParams] = useSearchParams();
  const refTableId = Number(searchParams.get('refTableId'));
  const refRecId = Number(searchParams.get('refRecId'));
  const types = useQuery({ queryKey: ['document-types'], queryFn: ({ signal }) => documentApi.types(signal) });
  const [galleryRecord, setGalleryRecord] = React.useState<DocuViewRecord | null>(null);
  const typeOptions = React.useMemo(() => (types.data ?? []).map((type) => ({ value: type.typeId, label: type.name })), [types.data]);
  const validReference = Number.isSafeInteger(refTableId) && refTableId > 0 && Number.isSafeInteger(refRecId) && refRecId > 0;

  const config = React.useMemo<EnterpriseListDetailsConfig<DocuViewRecord>>(() => ({
    dataSource: {
      type: 'remote',
      key: `docu-view-${refTableId}-${refRecId}`,
      load: async (signal) => (await documentApi.list(refTableId, refRecId, signal)).items.map(toRecord),
      create: async (record) => {
        if ((record.kind === 'File' || record.kind === 'Image') && record.pendingFiles.length > 0) {
          return Promise.all(record.pendingFiles.map(async (file) => toRecord(await documentApi.create(refTableId, refRecId, {
            typeId: record.typeId,
            name: record.pendingFiles.length === 1 && record.name.trim() ? record.name : file.name,
            notes: record.notes ?? '',
            url: '',
            file,
          }))));
        }
        return toRecord(await documentApi.create(refTableId, refRecId, {
          typeId: record.typeId,
          name: record.name,
          notes: record.notes ?? '',
          url: record.url ?? '',
          file: null,
        }));
      },
      update: async (record) => toRecord(await documentApi.update(record.documentId, {
        fileName: record.fileName ?? undefined,
        name: record.name,
        notes: record.notes ?? '',
        url: record.url ?? '',
        restriction: record.restriction,
      })),
      delete: async (record) => documentApi.remove(record.documentId),
    },
    createRecord: () => {
      const type = types.data?.[0];
      return {
        id: `new-${Date.now()}`, documentId: 0, pendingFiles: [],
        refTableId, refRecId, refCompanyId: null, typeId: type?.typeId ?? 'File', documentTypeName: type?.name ?? 'File',
        typeGroup: type?.typeGroup ?? 0, kind: type?.kind ?? 'File', valueRecId: null, name: '', fileName: null,
        originalFileName: null, fileType: null, contentType: null, fileSize: null, notes: '', url: '', restriction: 0,
        createdBy: null, createdAt: null, modifiedBy: null, modifiedAt: null,
      };
    },
    getPrimaryText: (record) => record.name || record.fileName || 'New attachment',
    getSecondaryText: (record) => `${record.documentTypeName} · Record ${record.refRecId}`,
    matchesSearch: (record, query) => `${record.name} ${record.fileName ?? ''} ${record.documentTypeName}`.toLocaleLowerCase().includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      notes: record.notes ?? '', restriction: record.restriction ?? 0, createdBy: record.createdBy ?? '', createdAt: record.createdAt ? new Date(record.createdAt).toLocaleString() : '',
      typeId: record.typeId, company: record.refCompanyId ?? '',
    }),
    setValues: (record, values) => ({ ...record, notes: String(values.notes ?? ''), restriction: Number(values.restriction ?? 0) }),
    headerFields: [
      { id: 'name', label: 'Description', getValue: (record) => record.name, setValue: (record, value) => ({ ...record, name: String(value) }), width: 'minmax(320px, 520px)' },
      { id: 'typeId', label: 'Type', type: 'select', options: typeOptions, getValue: (record) => record.typeId, setValue: (record, value) => { const selected = types.data?.find((type) => type.typeId === String(value)); const kind = selected?.kind ?? record.kind; return { ...record, typeId: String(value), documentTypeName: selected?.name ?? String(value), typeGroup: selected?.typeGroup ?? record.typeGroup, kind, pendingFiles: kind === 'File' || kind === 'Image' ? record.pendingFiles : [] }; }, width: 180, linkStyle: true },
      { id: 'attached', label: 'Attached', type: 'boolean', disabled: true, getValue: () => true, setValue: (record) => record, width: 115 },
    ],
    sections: ({ record, editing, onRecordChange }) => [
      { id: 'general', title: 'General', groups: [
        { id: 'details', title: 'DETAILS', fields: [{ name: 'notes', label: 'Notes', multiline: true, rows: 5, width: 220 }] },
        { id: 'create', title: 'CREATE', fields: [{ name: 'createdBy', label: 'Created by', type: 'display', width: 220 }, { name: 'createdAt', label: 'Created date and time', type: 'display', width: 220 }] },
        { id: 'restriction', fields: [{ name: 'restriction', label: 'Restriction', type: 'select', width: 165, options: [{ value: '0', label: 'Internal' }, { value: '1', label: 'External' }] }] },
      ] },
      { id: 'attachment', title: 'Attachment', content: <AttachmentDetails record={record} editing={editing} onChange={onRecordChange} /> },
      { id: 'preview', title: 'Preview', defaultExpanded: true, content: <DocumentPreview record={record} onOpen={() => setGalleryRecord(record)} /> },
      { id: 'more-details', title: 'More details', groups: [{ id: 'identification', title: 'IDENTIFICATION', fields: [{ name: 'typeId', label: 'Type', type: 'display', linkStyle: true, width: 165 }, { name: 'company', label: 'Company account', type: 'display', linkStyle: true, width: 165 }] }] },
    ],
    commands: [
      { id: 'open', label: 'Open', requiresSelection: true, onClick: (record) => { if (record) setGalleryRecord(record); } },
      { id: 'history', label: 'View history', disabled: true },
      { id: 'deleted', label: 'Deleted attachments', disabled: true },
      { id: 'created-by', label: 'Created by', disabled: true },
      { id: 'settings', label: 'Settings', disabled: true },
      { id: 'references', label: 'References', disabled: true },
      { id: 'options', label: 'Options', disabled: true },
    ],
    showAttachmentAction: false,
    viewLabel: 'Standard view', filterLabel: 'Filter', yesLabel: 'Yes', noLabel: 'No',
    advancedFilter: {
      fieldLabel: 'Description',
      getValue: (record) => record.name,
      matches: (record, value) => `${record.name} ${record.fileName ?? ''} ${record.documentTypeName}`.toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()),
    },
    validate: (record) => {
      const errors: Record<string, string> = {};
      if (record.kind === 'Note' && (!record.name.trim() || !record.notes?.trim())) errors.notes = 'Subject and note are required.';
      if (record.kind === 'URL' && !/^https?:\/\//i.test(record.url ?? '')) errors.url = 'A valid URL is required.';
      if (record.documentId === 0 && (record.kind === 'File' || record.kind === 'Image') && record.pendingFiles.length === 0) errors.file = 'Choose at least one file.';
      return errors;
    },
  }), [refRecId, refTableId, typeOptions, types.data]);

  if (!validReference) return <Alert severity="error">A valid RefTableId and RefRecId are required to open DocuView.</Alert>;
  return <>
    <ListDetailsPage variant="enterprise" title={`Attachments for record ${refTableId} - ${refRecId}`} config={config} />
    <AttachmentGalleryDialog open={galleryRecord != null} initialDocument={galleryRecord ? toDocument(galleryRecord) : null} refTableId={refTableId} refRecId={refRecId} onClose={() => setGalleryRecord(null)} />
  </>;
}
