import React from 'react';
import {
  Box,
  Button,
  Chip,
  FormControlLabel,
  MenuItem,
  Stack,
  Switch,
  TextField,
} from '@mui/material';
import AddOutlined from '@mui/icons-material/AddOutlined';
import EditOutlined from '@mui/icons-material/EditOutlined';
import PublishOutlined from '@mui/icons-material/PublishOutlined';
import ArchiveOutlined from '@mui/icons-material/ArchiveOutlined';
import DeleteOutline from '@mui/icons-material/DeleteOutlined';
import RefreshOutlined from '@mui/icons-material/RefreshOutlined';
import { useQuery } from '@tanstack/react-query';
import { queryClient } from '@core/api/queryClient';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { AppLookupGridField } from '@shared/components/fields/AppLookupGridField';
import { AppDialog } from '@shared/components/dialogs/AppDialog';
import { ConfirmationDialog } from '@shared/components/dialogs/ConfirmationDialog';
import { useNotifications } from '@shared/hooks/useNotifications';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { wfProcessApi, type WfProcessRecord } from '../../api/wfProcessApi';
import { fetchProcessPage, processLookupColumns } from '../../lookups/processLookup';
import { printTemplateApi } from '../api/printTemplateApi';
import { TemplateDesigner } from '../components/TemplateDesigner';
import {
  createEmptyPrintTemplateDocument,
  type PrintTemplateLanguage,
  type PrintTemplateOrientation,
  type PrintTemplateSummary,
  type SavePrintTemplateInput,
} from '../types/printTemplate.types';

type ConfirmAction = 'publish' | 'archive' | 'delete' | null;
type PrintTemplateRow = PrintTemplateSummary & { id: string };

const statusKey = (status: PrintTemplateSummary['status']): string => {
  if (status === 1 || String(status).toLowerCase() === 'published')
    return 'printTemplates.status.published';
  if (status === 2 || String(status).toLowerCase() === 'archived')
    return 'printTemplates.status.archived';
  return 'printTemplates.status.draft';
};

const emptyDraft = (language: PrintTemplateLanguage): SavePrintTemplateInput => ({
  code: '',
  name: '',
  description: null,
  isDefault: false,
  document: createEmptyPrintTemplateDocument(language),
});

export function PrintTemplatesPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const { notifyError, notifySuccess } = useNotifications();
  const processes = useQuery({
    queryKey: ['workflow', 'processes', 'print-templates'],
    queryFn: ({ signal }) => wfProcessApi.list(signal),
  });
  const [processId, setProcessId] = React.useState(0);
  const [selectedId, setSelectedId] = React.useState<number | null>(null);
  const [editorOpen, setEditorOpen] = React.useState(false);
  const [editingId, setEditingId] = React.useState<number | null>(null);
  const [draft, setDraft] = React.useState<SavePrintTemplateInput>(() =>
    emptyDraft(currentLanguage.code === 'ar' ? 'ar' : 'en')
  );
  const [saving, setSaving] = React.useState(false);
  const [confirmAction, setConfirmAction] = React.useState<ConfirmAction>(null);

  React.useEffect(() => {
    if (!processId && processes.data?.length) setProcessId(processes.data[0].recId);
  }, [processId, processes.data]);

  const templatesKey = React.useMemo(
    () => ['workflow', 'print-templates', processId] as const,
    [processId]
  );
  const templates = useQuery({
    queryKey: templatesKey,
    queryFn: ({ signal }) => printTemplateApi.listByProcess(processId, signal),
    enabled: processId > 0,
  });
  const selected = templates.data?.find((item) => item.templateId === selectedId) ?? null;

  React.useEffect(() => {
    if (selectedId && !templates.data?.some((item) => item.templateId === selectedId))
      setSelectedId(null);
  }, [selectedId, templates.data]);

  const refresh = async () => {
    await queryClient.invalidateQueries({ queryKey: templatesKey });
  };

  const openNew = () => {
    setEditingId(null);
    setDraft(emptyDraft(currentLanguage.code === 'ar' ? 'ar' : 'en'));
    setEditorOpen(true);
  };

  const openTemplate = async (templateId: number) => {
    try {
      const template = await printTemplateApi.get(templateId);
      setEditingId(template.templateId);
      setDraft({
        code: template.code,
        name: template.name,
        description: template.description,
        isDefault: template.isDefault,
        document: template.document,
      });
      setEditorOpen(true);
    } catch (error) {
      notifyError(error instanceof Error ? error.message : t('printTemplates.messages.loadFailed'));
    }
  };

  const openEdit = async () => {
    if (!selected) return;
    await openTemplate(selected.templateId);
  };

  const save = async () => {
    if (!processId || !draft.code.trim() || !draft.name.trim()) return;
    setSaving(true);
    try {
      if (editingId) await printTemplateApi.update(editingId, draft);
      else await printTemplateApi.create({ ...draft, processId });
      setEditorOpen(false);
      await refresh();
      notifySuccess(t('printTemplates.messages.saved'));
    } catch (error) {
      notifyError(error instanceof Error ? error.message : t('printTemplates.messages.saveFailed'));
    } finally {
      setSaving(false);
    }
  };

  const runConfirmedAction = async () => {
    if (!selected || !confirmAction) return;
    setSaving(true);
    try {
      if (confirmAction === 'publish') await printTemplateApi.publish(selected.templateId);
      if (confirmAction === 'archive') await printTemplateApi.archive(selected.templateId);
      if (confirmAction === 'delete') await printTemplateApi.deleteDraft(selected.templateId);
      setConfirmAction(null);
      setSelectedId(null);
      await refresh();
      notifySuccess(t(`printTemplates.messages.${confirmAction}Success`));
    } catch (error) {
      notifyError(
        error instanceof Error ? error.message : t('printTemplates.messages.actionFailed')
      );
    } finally {
      setSaving(false);
    }
  };

  const columns = React.useMemo<ColumnDef<PrintTemplateRow>[]>(
    () => [
      { field: 'code', headerName: 'printTemplates.fields.code', width: 150, pinned: 'left' },
      { field: 'name', headerName: 'printTemplates.fields.name', minWidth: 220, flex: 1 },
      {
        field: 'status',
        headerName: 'printTemplates.fields.status',
        width: 120,
        renderCell: ({ row }) => (
          <Chip
            size="small"
            label={t(statusKey(row.status))}
            color={
              row.status === 1 || String(row.status).toLowerCase() === 'published'
                ? 'success'
                : row.status === 2 || String(row.status).toLowerCase() === 'archived'
                  ? 'default'
                  : 'warning'
            }
          />
        ),
      },
      { field: 'latestVersionNo', headerName: 'printTemplates.fields.version', width: 100 },
      {
        field: 'isDefault',
        headerName: 'printTemplates.fields.default',
        width: 100,
        type: 'boolean',
      },
      { field: 'language', headerName: 'printTemplates.fields.language', width: 100 },
      {
        field: 'orientation',
        headerName: 'printTemplates.fields.orientation',
        width: 120,
        renderCell: ({ row }) => t(`printTemplates.orientation.${row.orientation}`),
      },
    ],
    [t]
  );

  const rows = React.useMemo<PrintTemplateRow[]>(
    () =>
      (templates.data ?? []).map((template) => ({ ...template, id: String(template.templateId) })),
    [templates.data]
  );

  const confirmation = confirmAction
    ? {
        title: t(`printTemplates.confirm.${confirmAction}.title`),
        message: t(`printTemplates.confirm.${confirmAction}.message`, {
          name: selected?.name ?? '',
        }),
        confirmLabel: t(`printTemplates.actions.${confirmAction}`),
      }
    : null;

  const actionPane = (
    <>
      <ActionPaneButton
        label={t('actions.new')}
        icon={<AddOutlined />}
        onClick={openNew}
        permission={PERMISSIONS.WF_PRINT_TEMPLATE_CREATE}
        disabled={!processId}
      />
      <ActionPaneButton
        label={t('actions.edit')}
        icon={<EditOutlined />}
        onClick={() => void openEdit()}
        permission={PERMISSIONS.WF_PRINT_TEMPLATE_EDIT}
        disabled={
          !selected || selected.status === 2 || String(selected.status).toLowerCase() === 'archived'
        }
      />
      <ActionPaneButton
        label={t('printTemplates.actions.publish')}
        icon={<PublishOutlined />}
        onClick={() => setConfirmAction('publish')}
        permission={PERMISSIONS.WF_PRINT_TEMPLATE_PUBLISH}
        disabled={!selected?.hasDraft}
      />
      <ActionPaneButton
        label={t('printTemplates.actions.archive')}
        icon={<ArchiveOutlined />}
        onClick={() => setConfirmAction('archive')}
        permission={PERMISSIONS.WF_PRINT_TEMPLATE_ARCHIVE}
        disabled={
          !selected || selected.status === 2 || String(selected.status).toLowerCase() === 'archived'
        }
      />
      <ActionPaneButton
        label={t('actions.delete')}
        icon={<DeleteOutline />}
        onClick={() => setConfirmAction('delete')}
        permission={PERMISSIONS.WF_PRINT_TEMPLATE_DELETE}
        disabled={!selected || selected.currentVersionId != null}
      />
      <ActionPaneButton
        label={t('actions.refresh')}
        icon={<RefreshOutlined />}
        onClick={() => void refresh()}
      />
    </>
  );

  const processFilter = (
    <Box sx={{ width: { xs: 'auto', sm: 450 }, mx: { xs: 1, sm: 2.5 }, py: 0.5 }}>
      <AppLookupGridField<WfProcessRecord>
        name="processId"
        label={t('printTemplates.fields.process')}
        value={processId || null}
        onChange={(value) => {
          setProcessId(Number(value) || 0);
          setSelectedId(null);
        }}
        disabled={processes.isLoading}
        columns={[...processLookupColumns]}
        queryKey={['workflow', 'process-lookup']}
        fetchPage={fetchProcessPage}
        fetchById={async (value) => wfProcessApi.getById(Number(value)).catch(() => null)}
        valueField="recId"
        labelField="name"
        pageSize={25}
      />
    </Box>
  );

  const dialogs = (
    <>
      <AppDialog
        open={editorOpen}
        onClose={() => !saving && setEditorOpen(false)}
        title={t(
          editingId ? 'printTemplates.editor.editTitle' : 'printTemplates.editor.createTitle'
        )}
        maxWidth="xl"
        actions={
          <>
            <Button onClick={() => setEditorOpen(false)} disabled={saving}>
              {t('actions.cancel')}
            </Button>
            <Button
              variant="contained"
              onClick={() => void save()}
              disabled={saving || !draft.code.trim() || !draft.name.trim()}
            >
              {t('actions.save')}
            </Button>
          </>
        }
      >
        <Stack spacing={1.5}>
          <TextField
            required
            size="small"
            label={t('printTemplates.fields.code')}
            value={draft.code}
            onChange={(event) => setDraft((value) => ({ ...value, code: event.target.value }))}
            slotProps={{ htmlInput: { maxLength: 50 } }}
          />
          <TextField
            required
            size="small"
            label={t('printTemplates.fields.name')}
            value={draft.name}
            onChange={(event) => setDraft((value) => ({ ...value, name: event.target.value }))}
            slotProps={{ htmlInput: { maxLength: 200 } }}
          />
          <TextField
            size="small"
            multiline
            minRows={2}
            label={t('printTemplates.fields.description')}
            value={draft.description ?? ''}
            onChange={(event) =>
              setDraft((value) => ({ ...value, description: event.target.value || null }))
            }
          />
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5}>
            <TextField
              select
              fullWidth
              size="small"
              label={t('printTemplates.fields.language')}
              value={draft.document.language}
              onChange={(event) => {
                const language = event.target.value as PrintTemplateLanguage;
                setDraft((value) => ({
                  ...value,
                  document: {
                    ...value.document,
                    language,
                    direction: language === 'ar' ? 'rtl' : 'ltr',
                  },
                }));
              }}
            >
              <MenuItem value="en">{t('printTemplates.languages.english')}</MenuItem>
              <MenuItem value="ar">{t('printTemplates.languages.arabic')}</MenuItem>
            </TextField>
            <TextField
              select
              fullWidth
              size="small"
              label={t('printTemplates.fields.orientation')}
              value={draft.document.page.orientation}
              onChange={(event) => {
                const orientation = event.target.value as PrintTemplateOrientation;
                setDraft((value) => ({
                  ...value,
                  document: { ...value.document, page: { ...value.document.page, orientation } },
                }));
              }}
            >
              <MenuItem value="portrait">{t('printTemplates.orientation.portrait')}</MenuItem>
              <MenuItem value="landscape">{t('printTemplates.orientation.landscape')}</MenuItem>
            </TextField>
          </Stack>
          <FormControlLabel
            control={
              <Switch
                checked={draft.isDefault}
                onChange={(_, checked) => setDraft((value) => ({ ...value, isDefault: checked }))}
              />
            }
            label={t('printTemplates.fields.default')}
          />
          <TemplateDesigner
            document={draft.document}
            onChange={(document) => setDraft((value) => ({ ...value, document }))}
          />
        </Stack>
      </AppDialog>

      {confirmation ? (
        <ConfirmationDialog
          open
          onClose={() => !saving && setConfirmAction(null)}
          onConfirm={() => void runConfirmedAction()}
          title={confirmation.title}
          message={confirmation.message}
          confirmLabel={confirmation.confirmLabel}
          loading={saving}
          severity={confirmAction === 'delete' ? 'error' : 'warning'}
        />
      ) : null}
    </>
  );

  return (
    <SimpleListPage<PrintTemplateRow>
      variant="enterprise"
      title={t('printTemplates.title')}
      subtitle={t('printTemplates.subtitle')}
      contextLabel={t('printTemplates.title')}
      viewLabel={t('printTemplates.subtitle')}
      actionPane={actionPane}
      filterBar={processFilter}
      dataSource={{
        type: 'controlled',
        rows,
        loading: templates.isLoading,
        error:
          templates.error instanceof Error
            ? templates.error.message
            : templates.isError
              ? t('printTemplates.messages.loadFailed')
              : null,
        refresh,
      }}
      columns={columns}
      dataGridProps={{
        getRowId: (row) => row.templateId,
        selectionMode: 'single',
        selectedIds: selectedId ? [selectedId] : [],
        onSelectionChange: (ids) => setSelectedId(ids.length ? Number(ids[0]) : null),
        onRowDoubleClick: (row) => {
          setSelectedId(row.templateId);
          void openTemplate(row.templateId);
        },
        storageKey: 'workflow.print-templates',
        hideAddRowButton: true,
        hideFilterRow: false,
      }}
      dialogs={dialogs}
      contentMinHeight={420}
    />
  );
}
