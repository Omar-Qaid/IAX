import React from 'react';
import { Box, Button, Chip, Stack, TextField } from '@mui/material';
import AddOutlined from '@mui/icons-material/AddOutlined';
import EditOutlined from '@mui/icons-material/EditOutlined';
import PublishOutlined from '@mui/icons-material/PublishOutlined';
import ArchiveOutlined from '@mui/icons-material/ArchiveOutlined';
import DeleteOutline from '@mui/icons-material/DeleteOutlined';
import SearchOutlined from '@mui/icons-material/SearchOutlined';
import { useQuery } from '@tanstack/react-query';
import { queryClient } from '@core/api/queryClient';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { EnterpriseCommandUtilities } from '@shared/components/action-pane/EnterpriseCommandUtilities';
import { OptionsMenu } from '@shared/components/action-pane/OptionsMenu';
import { RecordAttachmentsButton, recordTableId } from '@shared/components/documents';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { AppLookupGridField } from '@shared/components/fields/AppLookupGridField';
import { RightUtilityRail } from '@shared/components/page/RightUtilityRail';
import { AppDialog } from '@shared/components/dialogs/AppDialog';
import { ConfirmationDialog } from '@shared/components/dialogs/ConfirmationDialog';
import { useNotifications } from '@shared/hooks/useNotifications';
import { useLocalStorage } from '@shared/hooks/useLocalStorage';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { wfProcessApi, type WfProcessRecord } from '../../api/wfProcessApi';
import { fetchProcessPage, processLookupColumns } from '../../lookups/processLookup';
import { printTemplateApi } from '../api/printTemplateApi';
import { TemplateDesigner } from '../components/TemplateDesigner';
import {
  createEmptyPrintTemplateDocument,
  type PrintTemplateLanguage,
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
  const [processId, setProcessId] = useLocalStorage<number>(
    'workflow.print-templates.process-id',
    0
  );
  const [selectedId, setSelectedId] = React.useState<number | null>(null);
  const [editorOpen, setEditorOpen] = React.useState(false);
  const [editingId, setEditingId] = React.useState<number | null>(null);
  const [draft, setDraft] = React.useState<SavePrintTemplateInput>(() =>
    emptyDraft(currentLanguage.code === 'ar' ? 'ar' : 'en')
  );
  const [saving, setSaving] = React.useState(false);
  const [confirmAction, setConfirmAction] = React.useState<ConfirmAction>(null);
  const [filterRowVisible, setFilterRowVisible] = React.useState(false);

  React.useEffect(() => {
    if (
      processId > 0 &&
      processes.data &&
      !processes.data.some((process) => process.recId === processId)
    ) {
      setProcessId(0);
      setSelectedId(null);
    }
  }, [processId, processes.data, setProcessId]);

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
        label={t('common.search', 'Search')}
        icon={<SearchOutlined />}
        onClick={() => setFilterRowVisible((visible) => !visible)}
      />
      <OptionsMenu
        record={selected}
        tableName="WfPrintTemplates"
        getRecordId={(record) => record.templateId}
        title={t('printTemplates.title')}
      />
    </>
  );

  const actionPaneEndActions = (
    <EnterpriseCommandUtilities
      personalizeLabel={t('utilities.personalize')}
      guideLabel={t('utilities.guide')}
      notificationsLabel={t('common.notifications')}
      refreshLabel={t('actions.refresh')}
      openWindowLabel={t('utilities.openWindow')}
      attachmentAction={
        <RecordAttachmentsButton
          refTableId={recordTableId('WfPrintTemplates')}
          refRecId={selected?.templateId ?? null}
        />
      }
      onRefresh={() => void refresh()}
      showPersonalize={false}
      showGuide={false}
      showNotifications={false}
    />
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
          <TemplateDesigner
            processId={processId}
            document={draft.document}
            onChange={(document) => setDraft((value) => ({ ...value, document }))}
            isDefault={draft.isDefault}
            onDefaultChange={(isDefault) => setDraft((value) => ({ ...value, isDefault }))}
          />
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
      actionPaneEndActions={actionPaneEndActions}
      utilityRail={
        <RightUtilityRail
          filterLabel={t('actions.filter')}
          informationLabel={t('common.information')}
          filterActive={filterRowVisible}
          onFilter={() => setFilterRowVisible((visible) => !visible)}
          showInformation={false}
        />
      }
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
        hideFilterRow: !filterRowVisible,
      }}
      dialogs={dialogs}
      contentMinHeight={420}
    />
  );
}
