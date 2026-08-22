import React from 'react';
import { Alert, Box, Button, CircularProgress, TextField, Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailValues, EnterpriseListDetailsConfig, ListDetailRecord } from '@patterns/list-details/types';
import { documentApi, type DocumentDto } from '@shared/components/documents/documentApi';

interface DocuViewRecord extends Omit<DocumentDto, 'id'>, ListDetailRecord {
  documentId: number;
  pendingFile: File | null;
}

const toRecord = (document: DocumentDto): DocuViewRecord => ({
  ...document,
  id: String(document.id),
  documentId: document.id,
  pendingFile: null,
});

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
      {editing && fileKind
        ? <Button component="label" variant="outlined" size="small" sx={{ width: 220, justifyContent: 'flex-start', textTransform: 'none' }}>{record.pendingFile?.name || record.fileName || 'Choose file'}<input hidden type="file" onChange={(event) => onChange({ ...record, pendingFile: event.target.files?.[0] ?? null, fileName: event.target.files?.[0]?.name ?? record.fileName })} /></Button>
        : <Typography noWrap sx={{ width: 220, minHeight: 28, px: 0.5, borderBottom: '1px solid #605e5c', fontSize: 12 }}>{record.fileName || '—'}</Typography>}
    </Box>
    <Box>
      <Typography sx={{ mb: 1.5, fontSize: 11, fontWeight: 700 }}>FILE DETAILS</Typography>
      <Typography sx={{ mb: 0.5, fontSize: 12 }}>File type</Typography>
      <Typography sx={{ width: 220, minHeight: 28, px: 0.5, borderBottom: '1px solid #605e5c', fontSize: 12 }}>{record.fileType || record.kind}</Typography>
      <Typography sx={{ mt: 1.25, mb: 0.5, fontSize: 12 }}>Original file name</Typography>
      <Typography noWrap sx={{ width: 220, minHeight: 28, px: 0.5, borderBottom: '1px solid #605e5c', fontSize: 12 }}>{record.originalFileName || record.pendingFile?.name || '—'}</Typography>
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

function DocumentPreview({ record }: { record: DocuViewRecord }): React.ReactElement {
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
  if (record.kind === 'Note') return <Typography sx={{ whiteSpace: 'pre-wrap', fontSize: 12 }}>{record.notes || 'No note content.'}</Typography>;
  if (record.kind === 'URL') return record.url
    ? <Button component="a" href={record.url} target="_blank" rel="noreferrer">Open URL</Button>
    : <Typography sx={{ color: 'text.secondary', fontSize: 12 }}>No URL is available.</Typography>;
  if (preview.isLoading) return <CircularProgress size={22} />;
  if (!url || preview.isError) return <Typography sx={{ color: 'text.secondary', fontSize: 12 }}>Preview is not available.</Typography>;
  if (record.contentType?.startsWith('image/')) return <Box component="img" src={url} alt={record.name} sx={{ maxWidth: '100%', maxHeight: 420, objectFit: 'contain' }} />;
  if (record.contentType === 'application/pdf') return <Box component="iframe" title={record.name} src={url} sx={{ width: '100%', height: 420, border: 0 }} />;
  return <Button onClick={() => void documentApi.preview({ ...record, id: record.documentId })}>Open preview</Button>;
}

export function DocuView(): React.ReactElement {
  const [searchParams] = useSearchParams();
  const refTableId = Number(searchParams.get('refTableId'));
  const refRecId = Number(searchParams.get('refRecId'));
  const types = useQuery({ queryKey: ['document-types'], queryFn: ({ signal }) => documentApi.types(signal) });
  const typeOptions = React.useMemo(() => (types.data ?? []).map((type) => ({ value: type.typeId, label: type.name })), [types.data]);
  const validReference = Number.isSafeInteger(refTableId) && refTableId > 0 && Number.isSafeInteger(refRecId) && refRecId > 0;

  const config = React.useMemo<EnterpriseListDetailsConfig<DocuViewRecord>>(() => ({
    dataSource: {
      type: 'remote',
      key: `docu-view-${refTableId}-${refRecId}`,
      load: async (signal) => (await documentApi.list(refTableId, refRecId, signal)).items.map(toRecord),
      create: async (record) => toRecord(await documentApi.create(refTableId, refRecId, {
        typeId: record.typeId,
        name: record.name,
        notes: record.notes ?? '',
        url: record.url ?? '',
        file: record.pendingFile,
      })),
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
        id: `new-${Date.now()}`, documentId: 0, pendingFile: null,
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
      { id: 'name', label: 'Description', getValue: (record) => record.name, setValue: (record, value) => ({ ...record, name: String(value) }), width: 220 },
      { id: 'typeId', label: 'Type', type: 'select', options: typeOptions, getValue: (record) => record.typeId, setValue: (record, value) => { const selected = types.data?.find((type) => type.typeId === String(value)); return { ...record, typeId: String(value), documentTypeName: selected?.name ?? String(value), typeGroup: selected?.typeGroup ?? record.typeGroup, kind: selected?.kind ?? record.kind }; }, width: 165, linkStyle: true },
      { id: 'attached', label: 'Attached', type: 'boolean', disabled: true, getValue: () => true, setValue: (record) => record, width: 115 },
    ],
    sections: ({ record, editing, onRecordChange }) => [
      { id: 'general', title: 'General', visualVariant: 'legalEntity', minHeight: 245, groups: [
        { id: 'details', title: 'DETAILS', fields: [{ name: 'notes', label: 'Notes', multiline: true, rows: 5, width: 220 }] },
        { id: 'create', title: 'CREATE', fields: [{ name: 'createdBy', label: 'Created by', type: 'display', width: 220 }, { name: 'createdAt', label: 'Created date and time', type: 'display', width: 220 }] },
        { id: 'restriction', fields: [{ name: 'restriction', label: 'Restriction', type: 'select', width: 165, options: [{ value: '0', label: 'Internal' }, { value: '1', label: 'External' }] }] },
      ] },
      { id: 'attachment', title: 'Attachment', visualVariant: 'legalEntity', minHeight: 225, content: <AttachmentDetails record={record} editing={editing} onChange={onRecordChange} /> },
      { id: 'preview', title: 'Preview', visualVariant: 'legalEntity', defaultExpanded: true, content: <DocumentPreview record={record} /> },
      { id: 'more-details', title: 'More details', visualVariant: 'legalEntity', groups: [{ id: 'identification', title: 'IDENTIFICATION', fields: [{ name: 'typeId', label: 'Type', type: 'display', linkStyle: true, width: 165 }, { name: 'company', label: 'Company account', type: 'display', linkStyle: true, width: 165 }] }] },
    ],
    commands: [
      { id: 'open', label: 'Open', requiresSelection: true, onClick: (record) => { if (record) void documentApi.preview({ ...record, id: record.documentId }); } },
      { id: 'history', label: 'View history', disabled: true },
      { id: 'deleted', label: 'Deleted attachments', disabled: true },
      { id: 'created-by', label: 'Created by', disabled: true },
      { id: 'settings', label: 'Settings', disabled: true },
      { id: 'references', label: 'References', disabled: true },
      { id: 'options', label: 'Options', disabled: true },
    ],
    showAttachmentAction: false,
    viewLabel: 'Standard view', filterLabel: 'Filter', yesLabel: 'Yes', noLabel: 'No',
    presentation: { mode: 'list', listWidth: 282, listMinWidth: 220, listMaxWidth: 420, fullscreenCanvas: true, compactRecordHeader: true },
    validate: (record) => {
      const errors: Record<string, string> = {};
      if (record.kind === 'Note' && (!record.name.trim() || !record.notes?.trim())) errors.notes = 'Subject and note are required.';
      if (record.kind === 'URL' && !/^https?:\/\//i.test(record.url ?? '')) errors.url = 'A valid URL is required.';
      if (record.documentId === 0 && (record.kind === 'File' || record.kind === 'Image') && !record.pendingFile) errors.file = 'Choose a file.';
      return errors;
    },
  }), [refRecId, refTableId, typeOptions, types.data]);

  if (!validReference) return <Alert severity="error">A valid RefTableId and RefRecId are required to open DocuView.</Alert>;
  return <ListDetailsPage variant="enterprise" title={`Attachments for record ${refTableId} - ${refRecId}`} config={config} />;
}
