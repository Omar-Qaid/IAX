import React from 'react';
import { Alert, Box, Button, Paper, Stack, TextField, Typography } from '@mui/material';
import AssignmentOutlined from '@mui/icons-material/AssignmentOutlined';
import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailSectionConfig, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { useNotifications } from '@shared/hooks/useNotifications';
import { wfProcessApi, type WfProcessRecord } from '../api/wfProcessApi';
import { wfRequestApi, type WfRequestRecord } from '../api/wfRequestApi';

const emptyProcess = (): WfProcessRecord => ({
  id: 'empty-process', recId: 0, code: null, name: '', description: null, categoryId: 0,
  score: 0, canRepeat: false, mandatoryDocs: false, priorityId: 0, processTypeId: 0,
  sysField: false, sortOrder: 0, usersProcesses: [], isActive: true, rowVersion: null,
  recVersion: 1, dataAreaId: 'dat',
});

const newRequest = (process: WfProcessRecord): WfRequestRecord => ({
  id: `new-${crypto.randomUUID()}`, recId: 0, code: null,
  name: process.name || process.code || 'Workflow request',
  description: process.description ?? null,
  requestDate: new Date().toISOString(), processId: process.recId, employeeId: null,
  requestDetails: null, isFinished: false, finishedDate: null, isStopped: false,
  stoppedDate: null, score: 0, progress: 0, notes: null, attachmentId: null,
  isActive: true, rowVersion: null, recVersion: 1, dataAreaId: process.dataAreaId,
});

function RequestForm({ process }: { process: WfProcessRecord }) {
  const { notifyError, notifySuccess } = useNotifications();
  const [request, setRequest] = React.useState<WfRequestRecord>(() => newRequest(process));
  const [saving, setSaving] = React.useState(false);
  const [error, setError] = React.useState('');

  React.useEffect(() => {
    setRequest(newRequest(process));
    setError('');
  }, [process]);

  const submit = async () => {
    if (!request.name?.trim()) {
      setError('Request name is required.');
      return;
    }
    setSaving(true);
    setError('');
    try {
      const saved = await wfRequestApi.create(request);
      notifySuccess(`Request ${saved.code || saved.name} submitted successfully.`);
      setRequest(newRequest(process));
    } catch (reason) {
      const message = reason instanceof Error ? reason.message : String(reason);
      setError(message);
      notifyError(message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', lg: 'minmax(0, 1fr) 230px' }, gap: 2 }}>
      <Stack spacing={2}>
        <Typography component="h2" sx={{ fontSize: 18, fontWeight: 700 }}>Basic information</Typography>
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 1.25 }}>
          <TextField required size="small" label="Request name" value={request.name ?? ''}
            onChange={(event) => setRequest((current) => ({ ...current, name: event.target.value }))} />
          <TextField size="small" type="date" label="Request date" value={request.requestDate.slice(0, 10)}
            slotProps={{ inputLabel: { shrink: true } }}
            onChange={(event) => setRequest((current) => ({ ...current,
              requestDate: new Date(`${event.target.value}T00:00:00`).toISOString() }))} />
          <TextField size="small" type="number" label="Employee ID" value={request.employeeId ?? ''}
            onChange={(event) => setRequest((current) => ({ ...current,
              employeeId: Number(event.target.value) || null }))} />
          <TextField size="small" type="number" label="Attachment ID" value={request.attachmentId ?? ''}
            onChange={(event) => setRequest((current) => ({ ...current,
              attachmentId: Number(event.target.value) || null }))} />
          <TextField size="small" multiline rows={4} label="Request description"
            value={request.requestDetails ?? ''}
            onChange={(event) => setRequest((current) => ({ ...current,
              requestDetails: event.target.value || null }))} sx={{ gridColumn: '1 / -1' }} />
          <TextField size="small" multiline rows={3} label="Notes" value={request.notes ?? ''}
            onChange={(event) => setRequest((current) => ({ ...current,
              notes: event.target.value || null }))} sx={{ gridColumn: '1 / -1' }} />
        </Box>
        {error && <Alert severity="error">{error}</Alert>}
        <Button variant="contained" disabled={saving} onClick={submit} sx={{ alignSelf: 'flex-end' }}>
          {saving ? 'Submitting…' : 'Submit request'}
        </Button>
      </Stack>

      <Paper variant="outlined" sx={{ p: 2, alignSelf: 'start', textAlign: 'center' }}>
        <AssignmentOutlined sx={{ fontSize: 56, color: 'primary.main', mb: 1 }} />
        <Typography sx={{ fontWeight: 800 }}>Request summary</Typography>
        <Typography sx={{ mt: 1, fontWeight: 700 }}>{process.name || process.code}</Typography>
        <Typography variant="body2" color="text.secondary">Process: {process.code || process.recId}</Typography>
        <Typography variant="body2" color="text.secondary">Status: Draft</Typography>
        <Typography variant="body2" color="text.secondary">
          Date: {new Date(request.requestDate).toLocaleDateString('en')}
        </Typography>
      </Paper>
    </Box>
  );
}

export function RequestFromPage(): React.ReactElement {
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
    headerFields: [
      { id: 'code', label: 'Process code', disabled: true, getValue: (process) => process.code ?? '', setValue: (process) => process },
      { id: 'name', label: 'Process name', disabled: true, getValue: (process) => process.name ?? '', setValue: (process) => process },
      { id: 'requestDate', label: 'Request date', disabled: true, getValue: () => requestDate, setValue: (process) => process },
    ],
    sections: ({ record }): DetailSectionConfig[] => [{ id: 'request-form', title: 'Request Form',
      defaultExpanded: true, content: <RequestForm key={record.id} process={record} /> }],
    advancedFilter: { fieldLabel: 'Process', getValue: (process) => process.name,
      matches: (process, value) => (process.name ?? '').toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()) },
  };
  return <ListDetailsPage variant="enterprise" title="RequestFrom" config={config} />;
}
