import React from 'react';
import { Badge, Box, Button, IconButton, Tooltip, Typography } from '@mui/material';
import AttachFileOutlined from '@mui/icons-material/AttachFileOutlined';
import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailSectionConfig, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { wfProcessApi, type WfProcessRecord } from '../api/wfProcessApi';
import { DynamicForm, type DynamicFormHandle, type DynamicFormStatus } from '../components/DynamicForm';
import { RecordAttachmentsButton } from '@shared/components/documents/RecordAttachmentsButton';
import { documentTableIds } from '@shared/components/documents/recordTableIds';

const emptyProcess = (): WfProcessRecord => ({
  id: 'empty-process', recId: 0, code: null, name: '', description: null, categoryId: 0,
  score: 0, canRepeat: false, mandatoryDocs: false, priorityId: 0, processTypeId: 0,
  sysField: false, sortOrder: 0, usersProcesses: [], isActive: true, rowVersion: null,
  recVersion: 1, dataAreaId: 'dat',
});

export function RequestFromPage(): React.ReactElement {
  const formRef = React.useRef<DynamicFormHandle>(null);
  const requestFileInputRef = React.useRef<HTMLInputElement>(null);
  const [requestFiles, setRequestFiles] = React.useState<File[]>([]);
  const [formStatus, setFormStatus] = React.useState<DynamicFormStatus>({ score: 0, saving: false, canSubmit: false, requestId: null });
  const updateFormStatus = React.useCallback((status: DynamicFormStatus) => setFormStatus((current) =>
    current.score === status.score && current.saving === status.saving && current.canSubmit === status.canSubmit && current.requestId === status.requestId ? current : status
  ), []);
  const { categoryId: categoryParam, processId: processParam } = useParams();
  const categoryId = Number(categoryParam);
  const requestedProcessId = Number(processParam);
  const processes = useQuery({
    queryKey: ['workflow', 'request-from-processes', categoryId, requestedProcessId],
    queryFn: async ({ signal }) =>
      (await wfProcessApi.list(signal))
        .filter((process) => process.categoryId === categoryId)
        .sort((left, right) =>
          left.recId === requestedProcessId ? -1 : right.recId === requestedProcessId ? 1 : left.sortOrder - right.sortOrder
        ),
    enabled:
      Number.isSafeInteger(categoryId) && categoryId > 0 &&
      Number.isSafeInteger(requestedProcessId) && requestedProcessId > 0,
  });
  const records = processes.data ?? [];
  const requestedProcess = records.find((process) => process.recId === requestedProcessId);
  const requestDate = React.useMemo(() => new Date().toISOString().slice(0, 10), []);
  const requestDateLabel = React.useMemo(() => new Intl.DateTimeFormat('en-GB', { day: '2-digit', month: 'short', year: 'numeric', timeZone: 'UTC' }).format(new Date(`${requestDate}T00:00:00Z`)), [requestDate]);

  const config: EnterpriseListDetailsConfig<WfProcessRecord> = {
    readOnly: true,
    dataSource: { type: 'controlled', records, onRecordsChange: () => undefined,
      loading: processes.isLoading, error: processes.error instanceof Error ? processes.error.message : null,
      refresh: async () => { await processes.refetch(); } },
    createRecord: emptyProcess,
    getPrimaryText: (process) => process.name || process.code || 'Unnamed process',
    getSecondaryText: (process) => process.description || process.code || '',
    initialQuery: requestedProcess?.name || requestedProcess?.code || '',
    matchesSearch: (process, query) => `${process.code ?? ''} ${process.name ?? ''} ${process.description ?? ''}`
      .toLocaleLowerCase().includes(query.toLocaleLowerCase()),
    getValues: () => ({}),
    setValues: (process) => process,
    showAttachmentAction: false,
    actionPaneAfterListContent: <Box sx={{ display: 'flex', alignItems: 'center', px: 0.75, ml: 0.5, borderLeft: '1px solid', borderRight: '1px solid', borderColor: 'divider' }}>
      <Button size="small" variant="contained" disabled={!formStatus.canSubmit || formStatus.saving} onClick={() => formRef.current?.submit()} sx={{ minWidth: 122, height: 30, borderRadius: 0.75, fontSize: 11.5 }}>{formStatus.saving ? 'Submitting…' : 'Submit Request'}</Button>
    </Box>,
    actionPaneEndContent: <Box sx={{ display: 'flex', alignItems: 'center', mr: 0.5, borderLeft: '1px solid', borderColor: 'divider' }}>
      <Box sx={{ px: 0.8, py: 0.15, minWidth: 74 }}><Typography sx={{ fontSize: 11.5, lineHeight: 1.2, fontWeight: 750 }}>Score: {formStatus.score}</Typography></Box>
      {formStatus.requestId ? <RecordAttachmentsButton refTableId={documentTableIds.wfRequest} refRecId={formStatus.requestId} /> : <>
        <Tooltip title={requestFiles.length ? 'Request attachments queued for save' : 'Attach files to this request'}><span><IconButton size="small" aria-label="Request attachments" disabled={formStatus.saving} onClick={() => requestFileInputRef.current?.click()} sx={{ width: 42, height: 31, p: 0, color: 'primary.main', borderRadius: 0 }}><Badge badgeContent={requestFiles.length} color="primary" showZero max={99} overlap="circular" anchorOrigin={{ vertical: 'top', horizontal: 'right' }} sx={{ '& .MuiBadge-badge': { minWidth: 18, height: 18, px: 0.4, fontSize: 10, fontWeight: 600, top: -1, right: 0 } }}><AttachFileOutlined sx={{ fontSize: 20 }} /></Badge></IconButton></span></Tooltip>
        <input ref={requestFileInputRef} hidden multiple type="file" onChange={(event) => { setRequestFiles((current) => [...current, ...Array.from(event.target.files ?? [])]); event.target.value = ''; }} />
      </>}
    </Box>,
    headerFields: [
      { id: 'summary', label: '', type: 'display', disabled: true, renderOwnLabel: true,
        getValue: (process) => `${process.name ?? ''}\u001f${process.code ?? ''}\u001f${requestDateLabel}`,
        setValue: (process) => process,
        render: ({ value }) => { const [name, code, date] = String(value ?? '').split('\u001f'); return <Box sx={{ minWidth: 0, display: 'flex', alignItems: 'baseline', gap: 0.75, overflow: 'hidden' }}><Typography component="h1" noWrap sx={{ flexShrink: 0, fontSize: 14, lineHeight: 1.25, fontWeight: 750 }}>{name || 'Request'}</Typography><Typography color="text.secondary" noWrap sx={{ minWidth: 0, fontSize: 10.5, lineHeight: 1.2 }}>{[code, date].filter(Boolean).join(' · ')}</Typography></Box>; } },
    ],
    sections: ({ record }): DetailSectionConfig[] => [{ id: 'request-form', title: 'Request Form',
      hideHeader: true, defaultExpanded: true, content: <Box sx={{ width: { xs: '100%', md: 'calc(100% + 28px)' }, maxWidth: { xs: '100%', md: 'calc(100% + 28px)' }, mr: { md: '-28px' }, minWidth: 0, alignSelf: 'stretch', boxSizing: 'border-box', height: { xs: 'auto', md: 'calc(100dvh - 172px)' }, minHeight: { md: 420 }, maxHeight: { xs: 'none', md: 'calc(100dvh - 172px)' }, border: '1px solid', borderColor: '#c9c9c9', borderRadius: 1.25, bgcolor: '#fff', overflow: 'hidden', boxShadow: '0 1px 3px rgba(15,23,42,.08)' }}>
        <Box sx={{ width: '100%', maxWidth: '100%', minWidth: 0, height: '100%', boxSizing: 'border-box', overflowY: { xs: 'visible', md: 'auto' }, overflowX: 'hidden', p: { xs: 0.5, sm: 0.75 }, scrollbarGutter: 'stable', scrollbarWidth: 'thin', scrollbarColor: '#9b9b9b #f1f1f1', '&::-webkit-scrollbar': { width: 11 }, '&::-webkit-scrollbar-track': { bgcolor: '#f1f1f1', borderLeft: '1px solid #e1e1e1' }, '&::-webkit-scrollbar-thumb': { bgcolor: '#9b9b9b', borderRadius: 6, border: '3px solid #f1f1f1' }, '&::-webkit-scrollbar-thumb:hover': { bgcolor: '#707070' } }}>
          <DynamicForm ref={formRef} key={record.id} processId={record.recId} requestFiles={requestFiles} showActions={false} onStatusChange={updateFormStatus} />
        </Box>
      </Box> }],
    presentation: { mode: 'list', compactRecordHeader: true, listInitiallyVisible: false },
    advancedFilter: { fieldLabel: 'Process', getValue: (process) => process.name,
      matches: (process, value) => (process.name ?? '').toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()) },
  };
  return <ListDetailsPage variant="enterprise" title="RequestFrom" config={config} />;
}
