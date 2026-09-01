import React from 'react';
import { Alert, Badge, Box, Button, CircularProgress, IconButton, ToggleButton, ToggleButtonGroup, Tooltip, Typography } from '@mui/material';
import AttachFileOutlined from '@mui/icons-material/AttachFileOutlined';
import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { wfProcessApi, type WfProcessRecord } from '../api/wfProcessApi';
import {
  DynamicForm,
  type DynamicFormHandle,
  type DynamicFormStatus,
} from '../components/DynamicForm';
import { RecordAttachmentsButton } from '@shared/components/documents/RecordAttachmentsButton';
import { documentTableIds } from '@shared/components/documents/recordTableIds';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { localizedName } from '@shared/utilities/localizedName';
import { printTemplateApi } from '../print-templates/api/printTemplateApi';
import { fetchPrintoutCompany, toPrintoutCompany } from '@shared/components/printout/reportCompany';
import { useCompanyStore } from '@core/company/useCompanyStore';

type RequestDisplayMode = 'normal' | 'printTemplate';

const emptyProcess = (): WfProcessRecord => ({
  id: 'empty-process',
  recId: 0,
  code: null,
  name: '',
  description: null,
  categoryId: 0,
  score: 0,
  canRepeat: false,
  mandatoryDocs: false,
  priorityId: 0,
  processTypeId: 0,
  sysField: false,
  sortOrder: 0,
  usersProcesses: [],
  isActive: true,
  rowVersion: null,
  recVersion: 1,
  dataAreaId: 'dat',
});

export function RequestFromPage(): React.ReactElement {
  const { t, currentLanguage, isRtl } = useAppTranslation();
  const formRef = React.useRef<DynamicFormHandle>(null);
  const requestFileInputRef = React.useRef<HTMLInputElement>(null);
  const [displayMode, setDisplayMode] = React.useState<RequestDisplayMode>('normal');
  const [requestFiles, setRequestFiles] = React.useState<File[]>([]);
  const [formStatus, setFormStatus] = React.useState<DynamicFormStatus>({
    score: 0,
    saving: false,
    canSubmit: false,
    requestId: null,
  });
  const updateFormStatus = React.useCallback(
    (status: DynamicFormStatus) =>
      setFormStatus((current) =>
        current.score === status.score &&
        current.saving === status.saving &&
        current.canSubmit === status.canSubmit &&
        current.requestId === status.requestId
          ? current
          : status
      ),
    []
  );
  const { categoryId: categoryParam, processId: processParam } = useParams();
  const categoryId = Number(categoryParam);
  const requestedProcessId = Number(processParam);
  const currentCompany = useCompanyStore((state) => state.currentCompany);
  const processes = useQuery({
    queryKey: ['workflow', 'request-from-processes', categoryId, requestedProcessId],
    queryFn: async ({ signal }) =>
      (await wfProcessApi.list(signal))
        .filter((process) => process.categoryId === categoryId)
        .sort((left, right) =>
          left.recId === requestedProcessId
            ? -1
            : right.recId === requestedProcessId
              ? 1
              : left.sortOrder - right.sortOrder
        ),
    enabled:
      Number.isSafeInteger(categoryId) &&
      categoryId > 0 &&
      Number.isSafeInteger(requestedProcessId) &&
      requestedProcessId > 0,
  });
  const records = processes.data ?? [];
  const requestedProcess = records.find((process) => process.recId === requestedProcessId);
  const requestDate = React.useMemo(() => new Date().toISOString().slice(0, 10), []);
  const requestDateLabel = React.useMemo(
    () =>
      new Intl.DateTimeFormat(currentLanguage.code, {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
        timeZone: 'UTC',
      }).format(new Date(`${requestDate}T00:00:00Z`)),
    [currentLanguage.code, requestDate]
  );
  const publishedTemplates = useQuery({
    queryKey: ['workflow', 'request-from-published-templates', requestedProcessId],
    queryFn: ({ signal }) => printTemplateApi.listPublishedByProcess(requestedProcessId, signal),
    enabled: Number.isSafeInteger(requestedProcessId) && requestedProcessId > 0,
  });
  const selectedTemplate = React.useMemo(
    () => publishedTemplates.data?.find((template) => template.isDefault) ?? publishedTemplates.data?.[0],
    [publishedTemplates.data]
  );
  const publishedTemplate = useQuery({
    queryKey: ['workflow', 'request-from-published-template', requestedProcessId, selectedTemplate?.templateId],
    queryFn: ({ signal }) =>
      printTemplateApi.getPublishedForProcess(requestedProcessId, selectedTemplate!.templateId, signal),
    enabled: displayMode === 'printTemplate' && requestedProcessId > 0 && Boolean(selectedTemplate),
  });
  const reportCompany = useQuery({
    queryKey: ['report-company', currentCompany],
    queryFn: ({ signal }) => fetchPrintoutCompany(currentCompany, signal),
    staleTime: 60_000,
    enabled: displayMode === 'printTemplate' && Boolean(currentCompany),
  });
  const company = reportCompany.data ?? toPrintoutCompany(undefined, currentCompany);

  const config: EnterpriseListDetailsConfig<WfProcessRecord> = {
    readOnly: true,
    dataSource: {
      type: 'controlled',
      records,
      onRecordsChange: () => undefined,
      loading: processes.isLoading,
      error: processes.error instanceof Error ? processes.error.message : null,
      refresh: async () => {
        await processes.refetch();
      },
    },
    createRecord: emptyProcess,
    getPrimaryText: (process) =>
      localizedName(process, isRtl) || process.code || t('workflowRequest.unnamedProcess'),
    getSecondaryText: (process) => process.description || process.code || '',
    initialQuery: localizedName(requestedProcess, isRtl) || requestedProcess?.code || '',
    matchesSearch: (process, query) =>
      `${process.code ?? ''} ${process.name ?? ''} ${process.nameAlias ?? ''} ${process.description ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: () => ({}),
    setValues: (process) => process,
    showAttachmentAction: false,
    actionPaneAfterListContent: (
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          px: 0.75,
          marginInlineStart: 0.5,
          borderInline: '1px solid',
          borderColor: 'divider',
        }}
      >
        <ToggleButtonGroup
          exclusive
          size="small"
          value={displayMode}
          onChange={(_, value: RequestDisplayMode | null) => value && setDisplayMode(value)}
          aria-label={t('workflowRequest.displayMode')}
          sx={{ marginInlineEnd: 0.75, height: 30, '& .MuiToggleButton-root': { px: 1.25, py: 0, fontSize: 11, textTransform: 'none' } }}
        >
          <ToggleButton value="normal">{t('workflowRequest.normalDisplay')}</ToggleButton>
          <ToggleButton value="printTemplate">{t('workflowRequest.printTemplateDisplay')}</ToggleButton>
        </ToggleButtonGroup>
        <Button
          size="small"
          variant="contained"
          disabled={!formStatus.canSubmit || formStatus.saving}
          onClick={() => formRef.current?.submit()}
          sx={{ minWidth: 122, height: 30, borderRadius: 0.75, fontSize: 11.5 }}
        >
          {formStatus.saving ? t('workflowRequest.submitting') : t('workflowRequest.submit')}
        </Button>
      </Box>
    ),
    actionPaneEndContent: (
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          marginInlineEnd: 0.5,
          borderInlineStart: '1px solid',
          borderColor: 'divider',
        }}
      >
        <Box sx={{ px: 0.8, py: 0.15, minWidth: 74 }}>
          <Typography sx={{ fontSize: 11.5, lineHeight: 1.2, fontWeight: 750 }}>
            {t('workflowRequest.score', { score: formStatus.score })}
          </Typography>
        </Box>
        {formStatus.requestId ? (
          <RecordAttachmentsButton
            refTableId={documentTableIds.wfRequest}
            refRecId={formStatus.requestId}
          />
        ) : (
          <>
            <Tooltip
              title={
                requestFiles.length
                  ? t('workflowRequest.attachmentsQueued')
                  : t('workflowRequest.attachFiles')
              }
            >
              <span>
                <IconButton
                  size="small"
                  aria-label={t('workflowRequest.requestAttachments')}
                  disabled={formStatus.saving}
                  onClick={() => requestFileInputRef.current?.click()}
                  sx={{ width: 42, height: 31, p: 0, color: 'primary.main', borderRadius: 0 }}
                >
                  <Badge
                    badgeContent={requestFiles.length}
                    color="primary"
                    showZero
                    max={99}
                    overlap="circular"
                    anchorOrigin={{
                      vertical: 'top',
                      horizontal: currentLanguage.dir === 'rtl' ? 'left' : 'right',
                    }}
                    sx={{
                      '& .MuiBadge-badge': {
                        minWidth: 18,
                        height: 18,
                        px: 0.4,
                        fontSize: 10,
                        fontWeight: 600,
                        top: -1,
                        insetInlineEnd: 0,
                      },
                    }}
                  >
                    <AttachFileOutlined sx={{ fontSize: 20 }} />
                  </Badge>
                </IconButton>
              </span>
            </Tooltip>
            <input
              ref={requestFileInputRef}
              hidden
              multiple
              type="file"
              onChange={(event) => {
                setRequestFiles((current) => [...current, ...Array.from(event.target.files ?? [])]);
                event.target.value = '';
              }}
            />
          </>
        )}
      </Box>
    ),
    headerFields: [
      {
        id: 'summary',
        label: '',
        type: 'display',
        disabled: true,
        renderOwnLabel: true,
        getValue: (process) =>
          `${localizedName(process, isRtl)}\u001f${process.code ?? ''}\u001f${requestDateLabel}`,
        setValue: (process) => process,
        render: ({ value }) => {
          const [name, code, date] = String(value ?? '').split('\u001f');
          return (
            <Box
              sx={{
                minWidth: 0,
                display: 'flex',
                alignItems: 'baseline',
                gap: 0.75,
                overflow: 'hidden',
              }}
            >
              <Typography
                component="h1"
                noWrap
                sx={{ flexShrink: 0, fontSize: 14, lineHeight: 1.25, fontWeight: 750 }}
              >
                {name || t('workflowRequest.request')}
              </Typography>
              <Typography
                color="text.secondary"
                noWrap
                sx={{ minWidth: 0, fontSize: 10.5, lineHeight: 1.2 }}
              >
                {[code, date].filter(Boolean).join(' · ')}
              </Typography>
            </Box>
          );
        },
      },
    ],
    sections: ({ record }): DetailSectionConfig[] => [
      {
        id: 'request-form',
        title: t('workflowRequest.requestForm'),
        hideHeader: true,
        defaultExpanded: true,
        content: (
          <Box
            sx={{
              width: { xs: '100%', md: 'calc(100% + 28px)' },
              maxWidth: { xs: '100%', md: 'calc(100% + 28px)' },
              marginInlineEnd: { md: '-28px' },
              minWidth: 0,
              alignSelf: 'stretch',
              boxSizing: 'border-box',
              height: { xs: 'auto', md: 'calc(100dvh - 172px)' },
              minHeight: { md: 420 },
              maxHeight: { xs: 'none', md: 'calc(100dvh - 172px)' },
              border: '1px solid',
              borderColor: '#c9c9c9',
              borderRadius: 1.25,
              bgcolor: '#fff',
              overflow: 'hidden',
              boxShadow: '0 1px 3px rgba(15,23,42,.08)',
            }}
          >
            <Box
              sx={{
                width: '100%',
                maxWidth: '100%',
                minWidth: 0,
                height: '100%',
                boxSizing: 'border-box',
                overflowY: { xs: 'visible', md: 'auto' },
                overflowX: 'hidden',
                p: { xs: 0.5, sm: 0.75 },
                scrollbarGutter: 'stable',
                scrollbarWidth: 'thin',
                scrollbarColor: '#9b9b9b #f1f1f1',
                '&::-webkit-scrollbar': { width: 11 },
                '&::-webkit-scrollbar-track': {
                  bgcolor: '#f1f1f1',
                  borderInlineStart: '1px solid #e1e1e1',
                },
                '&::-webkit-scrollbar-thumb': {
                  bgcolor: '#9b9b9b',
                  borderRadius: 6,
                  border: '3px solid #f1f1f1',
                },
                '&::-webkit-scrollbar-thumb:hover': { bgcolor: '#707070' },
              }}
            >
              <Box sx={{ display: displayMode === 'normal' || Boolean(publishedTemplate.data) ? 'block' : 'none', p: displayMode === 'printTemplate' ? { xs: 0.5, md: 1.5 } : 0, bgcolor: '#fff', overflow: 'auto' }}>
                <DynamicForm
                  ref={formRef}
                  key={record.id}
                  processId={record.recId}
                  requestFiles={requestFiles}
                  showActions={false}
                  onStatusChange={updateFormStatus}
                  displayMode={displayMode}
                  printTemplate={publishedTemplate.data?.document}
                  printCompany={company}
                  requestDate={requestDate}
                />
              </Box>
              {displayMode === 'printTemplate' && !publishedTemplate.data && (
                <Box sx={{ minHeight: 320, p: { xs: 0.5, md: 1.5 }, bgcolor: '#fff', overflow: 'auto' }}>
                  {(publishedTemplates.isLoading || publishedTemplate.isLoading || reportCompany.isLoading) && (
                    <Box sx={{ minHeight: 280, display: 'grid', placeItems: 'center' }}>
                      <CircularProgress aria-label={t('workflowRequest.loadingPrintTemplate')} />
                    </Box>
                  )}
                  {!publishedTemplates.isLoading && !selectedTemplate && (
                    <Alert severity="info">{t('workflowRequest.noPublishedPrintTemplate')}</Alert>
                  )}
                  {(publishedTemplates.isError || publishedTemplate.isError || reportCompany.isError) && (
                    <Alert severity="error">{t('workflowRequest.printTemplateLoadFailed')}</Alert>
                  )}
                </Box>
              )}
            </Box>
          </Box>
        ),
      },
    ],
    presentation: { mode: 'list', compactRecordHeader: true, listInitiallyVisible: false },
    advancedFilter: {
      fieldLabel: t('workflowRequest.process'),
      getValue: (process) => localizedName(process, isRtl),
      matches: (process, value) =>
        localizedName(process, isRtl).toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()),
    },
  };
  return (
    <ListDetailsPage
      variant="enterprise"
      title={t('workflowRequest.requestForm')}
      config={config}
    />
  );
}
