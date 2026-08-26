import React from 'react';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  LinearProgress,
  MenuItem,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import AddOutlined from '@mui/icons-material/AddOutlined';
import Close from '@mui/icons-material/Close';
import DeleteOutlined from '@mui/icons-material/DeleteOutlined';
import DownloadOutlined from '@mui/icons-material/DownloadOutlined';
import EditOutlined from '@mui/icons-material/EditOutlined';
import OpenInNewOutlined from '@mui/icons-material/OpenInNewOutlined';
import VisibilityOutlined from '@mui/icons-material/VisibilityOutlined';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNotifications } from '@shared/hooks/useNotifications';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { documentApi, type CreateDocumentInput, type DocumentDto } from './documentApi';

export interface FileManagerProps {
  open: boolean;
  refTableId: number;
  refRecId: number;
  onClose: () => void;
}
const emptyDraft = (): CreateDocumentInput => ({
  typeId: 'File',
  name: '',
  notes: '',
  url: '',
  file: null,
});
const formatBytes = (value: number | null) =>
  value == null
    ? ''
    : value < 1024
      ? `${value} B`
      : value < 1024 ** 2
        ? `${(value / 1024).toFixed(1)} KB`
        : `${(value / 1024 ** 2).toFixed(1)} MB`;

export function FileManager({
  open,
  refTableId,
  refRecId,
  onClose,
}: FileManagerProps): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const queryClient = useQueryClient();
  const { notifySuccess, notifyError } = useNotifications();
  const [creating, setCreating] = React.useState(false);
  const [draft, setDraft] = React.useState(emptyDraft);
  const [editing, setEditing] = React.useState<DocumentDto | null>(null);
  const [note, setNote] = React.useState<DocumentDto | null>(null);
  const [progress, setProgress] = React.useState(0);
  const queryKey = ['documents', refTableId, refRecId];
  const documents = useQuery({
    queryKey,
    queryFn: ({ signal }) => documentApi.list(refTableId, refRecId, signal),
    enabled: open && refTableId > 0 && refRecId > 0,
  });
  const types = useQuery({
    queryKey: ['document-types'],
    queryFn: ({ signal }) => documentApi.types(signal),
    enabled: open,
  });
  const selectedType = types.data?.find((item) => item.typeId === draft.typeId) ?? types.data?.[0];
  React.useEffect(() => {
    if (selectedType && !types.data?.some((item) => item.typeId === draft.typeId))
      setDraft((value) => ({ ...value, typeId: selectedType.typeId }));
  }, [draft.typeId, selectedType, types.data]);
  const refresh = () => queryClient.invalidateQueries({ queryKey });
  const create = useMutation({
    mutationFn: () => documentApi.create(refTableId, refRecId, draft, setProgress),
    onSuccess: () => {
      notifySuccess(t('documents.messages.attached'));
      setCreating(false);
      setDraft(emptyDraft());
      setProgress(0);
      void refresh();
    },
    onError: (reason) =>
      notifyError(reason instanceof Error ? reason.message : t('documents.messages.attachFailed')),
  });
  const update = useMutation({
    mutationFn: (item: DocumentDto) =>
      documentApi.update(item.id, {
        fileName: item.fileName ?? undefined,
        name: item.name,
        notes: item.notes ?? '',
        url: item.url ?? '',
        restriction: item.restriction,
      }),
    onSuccess: () => {
      notifySuccess(t('documents.messages.updated'));
      setEditing(null);
      void refresh();
    },
    onError: (reason) =>
      notifyError(reason instanceof Error ? reason.message : t('documents.messages.updateFailed')),
  });
  const remove = useMutation({
    mutationFn: (item: DocumentDto) => documentApi.remove(item.id),
    onSuccess: () => {
      notifySuccess(t('documents.messages.deleted'));
      void refresh();
    },
    onError: (reason) =>
      notifyError(reason instanceof Error ? reason.message : t('documents.messages.deleteFailed')),
  });
  const openItem = async (item: DocumentDto) => {
    if (item.kind === 'Note') {
      setNote(item);
      return;
    }
    try {
      await documentApi.preview(item);
    } catch (reason) {
      notifyError(reason instanceof Error ? reason.message : t('documents.messages.openFailed'));
    }
  };
  const canSave =
    selectedType?.kind === 'Note'
      ? Boolean(draft.name.trim() && draft.notes.trim())
      : selectedType?.kind === 'URL'
        ? /^https?:\/\//i.test(draft.url)
        : Boolean(draft.file);

  return (
    <>
      <Dialog
        open={open}
        onClose={onClose}
        fullWidth
        maxWidth="lg"
        aria-labelledby="file-manager-title"
      >
        <DialogTitle
          id="file-manager-title"
          sx={{
            py: 1,
            px: 1.5,
            display: 'flex',
            alignItems: 'center',
            borderBottom: '1px solid',
            borderColor: 'divider',
          }}
        >
          <Typography component="span" sx={{ flex: 1, fontSize: 16, fontWeight: 700 }}>
            {t('documents.attachments')}
          </Typography>
          <Button
            size="small"
            startIcon={<AddOutlined />}
            onClick={() => {
              setDraft(emptyDraft());
              setCreating(true);
            }}
          >
            {t('actions.new')}
          </Button>
          <IconButton size="small" aria-label={t('documents.closeAttachments')} onClick={onClose}>
            <Close />
          </IconButton>
        </DialogTitle>
        <DialogContent sx={{ p: 1.25, minHeight: 320 }}>
          {documents.isLoading && (
            <Box sx={{ py: 8, display: 'grid', placeItems: 'center' }}>
              <CircularProgress />
            </Box>
          )}
          {documents.isError && (
            <Alert severity="error">{t('documents.messages.loadFailed')}</Alert>
          )}
          {!documents.isLoading && !documents.data?.items.length && (
            <Paper variant="outlined" sx={{ py: 7, textAlign: 'center', color: 'text.secondary' }}>
              {t('documents.noDocuments')}
            </Paper>
          )}
          {!!documents.data?.items.length && (
            <Table size="small" aria-label={t('documents.recordAttachments')}>
              <TableHead>
                <TableRow>
                  <TableCell>{t('documents.fields.type')}</TableCell>
                  <TableCell>{t('documents.fields.name')}</TableCell>
                  <TableCell>{t('documents.fields.descriptionNotes')}</TableCell>
                  <TableCell>{t('documents.fields.size')}</TableCell>
                  <TableCell>{t('documents.fields.createdBy')}</TableCell>
                  <TableCell>{t('documents.fields.createdDate')}</TableCell>
                  <TableCell align="right">{t('documents.fields.actions')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {documents.data.items.map((item) => (
                  <TableRow hover key={item.id}>
                    <TableCell>{item.documentTypeName}</TableCell>
                    <TableCell>{item.name}</TableCell>
                    <TableCell sx={{ maxWidth: 280 }}>
                      <Typography noWrap sx={{ fontSize: 12 }}>
                        {item.notes || item.url || '—'}
                      </Typography>
                    </TableCell>
                    <TableCell>{formatBytes(item.fileSize)}</TableCell>
                    <TableCell>{item.createdBy || '—'}</TableCell>
                    <TableCell>
                      {item.createdAt
                        ? new Date(item.createdAt).toLocaleString(currentLanguage.code)
                        : '—'}
                    </TableCell>
                    <TableCell align="right" sx={{ whiteSpace: 'nowrap' }}>
                      <Tooltip title={t('actions.open')}>
                        <IconButton
                          size="small"
                          aria-label={t('documents.openNamed', { name: item.name })}
                          onClick={() => void openItem(item)}
                        >
                          <OpenInNewOutlined fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      {(item.kind === 'File' || item.kind === 'Image') && (
                        <>
                          <Tooltip title={t('documents.preview')}>
                            <IconButton
                              size="small"
                              aria-label={t('documents.previewNamed', { name: item.name })}
                              onClick={() => void documentApi.preview(item)}
                            >
                              <VisibilityOutlined fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title={t('actions.download')}>
                            <IconButton
                              size="small"
                              aria-label={t('documents.downloadNamed', { name: item.name })}
                              onClick={() => void documentApi.download(item)}
                            >
                              <DownloadOutlined fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </>
                      )}
                      <Tooltip title={t('documents.editRename')}>
                        <IconButton
                          size="small"
                          aria-label={t('documents.editNamed', { name: item.name })}
                          onClick={() => setEditing({ ...item })}
                        >
                          <EditOutlined fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title={t('actions.delete')}>
                        <IconButton
                          size="small"
                          color="error"
                          aria-label={t('documents.deleteNamed', { name: item.name })}
                          onClick={() => {
                            if (window.confirm(t('documents.confirmDelete', { name: item.name })))
                              remove.mutate(item);
                          }}
                        >
                          <DeleteOutlined fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>{t('actions.close')}</Button>
        </DialogActions>
      </Dialog>
      <Dialog
        open={creating}
        onClose={() => !create.isPending && setCreating(false)}
        fullWidth
        maxWidth="sm"
        aria-labelledby="new-document-title"
      >
        <DialogTitle id="new-document-title">{t('documents.newAttachment')}</DialogTitle>
        <DialogContent>
          <Stack spacing={1.5} sx={{ pt: 0.5 }}>
            <TextField
              select
              size="small"
              label={t('documents.fields.documentType')}
              value={selectedType?.typeId ?? draft.typeId}
              onChange={(event) => setDraft({ ...emptyDraft(), typeId: event.target.value })}
            >
              {(types.data ?? []).map((item) => (
                <MenuItem key={item.typeId} value={item.typeId}>
                  {item.name}
                </MenuItem>
              ))}
            </TextField>
            {(selectedType?.kind === 'File' || selectedType?.kind === 'Image') && (
              <Button component="label" variant="outlined">
                {draft.file?.name ?? t('documents.chooseFile')}
                <input
                  hidden
                  type="file"
                  accept={selectedType.allowedExtensions
                    .map((value) => (value.startsWith('.') ? value : `.${value}`))
                    .join(',')}
                  onChange={(event) =>
                    setDraft((value) => ({ ...value, file: event.target.files?.[0] ?? null }))
                  }
                />
              </Button>
            )}
            {(selectedType?.kind === 'Note' || selectedType?.kind === 'URL') && (
              <TextField
                size="small"
                label={t(
                  selectedType.kind === 'Note'
                    ? 'documents.fields.subject'
                    : 'documents.fields.name'
                )}
                value={draft.name}
                onChange={(event) => setDraft((value) => ({ ...value, name: event.target.value }))}
              />
            )}
            {selectedType?.kind === 'URL' && (
              <TextField
                size="small"
                label={t('documents.fields.url')}
                value={draft.url}
                onChange={(event) => setDraft((value) => ({ ...value, url: event.target.value }))}
              />
            )}
            <TextField
              size="small"
              multiline
              minRows={selectedType?.kind === 'Note' ? 4 : 2}
              label={t(
                selectedType?.kind === 'Note'
                  ? 'documents.fields.note'
                  : 'documents.fields.description'
              )}
              value={draft.notes}
              onChange={(event) => setDraft((value) => ({ ...value, notes: event.target.value }))}
            />
            {create.isPending && (
              <LinearProgress
                variant={progress ? 'determinate' : 'indeterminate'}
                value={progress}
              />
            )}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button disabled={create.isPending} onClick={() => setCreating(false)}>
            {t('actions.cancel')}
          </Button>
          <Button
            variant="contained"
            disabled={!canSave || create.isPending}
            onClick={() => create.mutate()}
          >
            {t('actions.save')}
          </Button>
        </DialogActions>
      </Dialog>
      <Dialog open={editing != null} onClose={() => setEditing(null)} fullWidth maxWidth="sm">
        <DialogTitle>{t('documents.editAttachment')}</DialogTitle>
        <DialogContent>
          {editing && (
            <Stack spacing={1.5} sx={{ pt: 0.5 }}>
              <TextField
                size="small"
                label={t('documents.fields.name')}
                value={editing.name}
                onChange={(event) =>
                  setEditing({
                    ...editing,
                    name: event.target.value,
                    fileName:
                      editing.kind === 'File' || editing.kind === 'Image'
                        ? event.target.value
                        : editing.fileName,
                  })
                }
              />
              {editing.kind === 'URL' && (
                <TextField
                  size="small"
                  label={t('documents.fields.url')}
                  value={editing.url ?? ''}
                  onChange={(event) => setEditing({ ...editing, url: event.target.value })}
                />
              )}
              <TextField
                size="small"
                multiline
                minRows={3}
                label={t(
                  editing.kind === 'Note' ? 'documents.fields.note' : 'documents.fields.description'
                )}
                value={editing.notes ?? ''}
                onChange={(event) => setEditing({ ...editing, notes: event.target.value })}
              />
            </Stack>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditing(null)}>{t('actions.cancel')}</Button>
          <Button
            variant="contained"
            disabled={!editing || update.isPending}
            onClick={() => editing && update.mutate(editing)}
          >
            {t('actions.save')}
          </Button>
        </DialogActions>
      </Dialog>
      <Dialog open={note != null} onClose={() => setNote(null)} fullWidth maxWidth="sm">
        <DialogTitle>{note?.name}</DialogTitle>
        <DialogContent>
          <Typography sx={{ whiteSpace: 'pre-wrap' }}>{note?.notes}</Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setNote(null)}>{t('actions.close')}</Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
