import React from 'react';
import {
  Alert,
  Box,
  CircularProgress,
  Grid,
  IconButton,
  InputAdornment,
  List,
  ListItemButton,
  MenuItem,
  Paper,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import MenuIcon from '@mui/icons-material/Menu';
import SearchIcon from '@mui/icons-material/Search';
import { PageContainer } from '@shared/components/page/PageContainer';
import { PageHeader } from '@shared/components/page/PageHeader';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { ActionPaneGroup } from '@shared/components/action-pane/ActionPaneGroup';
import { EnterpriseCrudActions } from '@shared/components/action-pane/EnterpriseCrudActions';
import { EnterpriseCommandUtilities } from '@shared/components/action-pane/EnterpriseCommandUtilities';
import { RightUtilityRail } from '@shared/components/page/RightUtilityRail';
import { RelatedInformationPanel } from '@shared/components/page/RelatedInformationPanel';
import { EnterpriseFilterPanel } from '@shared/components/data-grid/EnterpriseFilterPanel';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { DataGridProps } from '@shared/components/data-grid/types';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { EmptyState } from '@shared/components/feedback/EmptyState';
import { EmptyDataWatermark } from '@shared/components/feedback/EmptyDataWatermark';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { AccessDeniedState } from '@shared/components/feedback/AccessDeniedState';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { usePermission } from '@core/permissions/usePermission';
import { ListDetailsLayout } from './ListDetailsLayout';
import { useListDetailsPage } from './useListDetailsPage';
import { ConfirmationDialog } from '@shared/components/dialogs/ConfirmationDialog';
import type {
  DetailSectionConfig,
  DetailValue,
  EnterpriseListDetailsConfig,
  ListDetailRecord,
  ListDetailsHeaderField,
} from './types';
import { d365 } from './d365Tokens';

interface LegacyListDetailsProps<T extends ListDetailRecord> {
  variant?: 'standard';
  title: string;
  subtitle?: string;
  actionPane?: React.ReactNode;
  dataGridProps: DataGridProps<T>;
  detailsPane?: React.ReactNode;
  loading?: boolean;
  selectedId?: string | null;
  dialogs?: React.ReactNode;
}
interface EnterpriseListDetailsProps<T extends ListDetailRecord> {
  variant: 'enterprise';
  title: string;
  config: EnterpriseListDetailsConfig<T>;
  dialogs?: React.ReactNode;
}
export type ListDetailsPageProps<T extends ListDetailRecord = ListDetailRecord> =
  LegacyListDetailsProps<T> | EnterpriseListDetailsProps<T>;

export function ListDetailsPage<T extends ListDetailRecord = ListDetailRecord>(
  props: ListDetailsPageProps<T>
): React.ReactElement {
  return props.variant === 'enterprise' ? (
    <EnterpriseListDetailsPage title={props.title} config={props.config} dialogs={props.dialogs} />
  ) : (
    <LegacyListDetailsPage {...props} />
  );
}

function EnterpriseListDetailsPage<T extends ListDetailRecord>({
  title,
  config,
  dialogs,
}: Omit<EnterpriseListDetailsProps<T>, 'variant'>): React.ReactElement {
  const { t } = useAppTranslation();
  const [listPaneVisible, setListPaneVisible] = React.useState(true);
  const [deleteConfirmationOpen, setDeleteConfirmationOpen] = React.useState(false);
  const state = useListDetailsPage(config);
  const [emptyPreviewRecord] = React.useState<T>(() => config.createRecord());
  const { hasPermission: canView } = usePermission(config.permissions?.view);
  const { hasPermission: canCreate } = usePermission(config.permissions?.create);
  const { hasPermission: canEdit } = usePermission(config.permissions?.edit);
  const { hasPermission: canDelete } = usePermission(config.permissions?.delete);
  const record = state.draft;
  const displayedRecord = record ?? emptyPreviewRecord;
  const displayedEditing = Boolean(record) && state.editing;
  const labels = {
    view: config.viewLabel ?? t('common.standardView', 'Standard view'),
    filter: config.filterLabel ?? t('actions.filter'),
    information: config.informationLabel ?? t('common.information'),
    yes: config.yesLabel ?? t('common.yes', 'Yes'),
    no: config.noLabel ?? t('common.no', 'No'),
  };
  const crud = {
    editLabel: t('actions.edit'),
    newLabel: t('actions.new'),
    deleteLabel: t('actions.delete'),
    saveLabel: t('actions.save'),
    cancelLabel: t('actions.cancel'),
    ...config.crud,
  };
  const utilities = {
    personalizeLabel: t('utilities.personalize'),
    guideLabel: t('utilities.guide'),
    notificationsLabel: t('common.notifications'),
    refreshLabel: t('actions.refresh'),
    openWindowLabel: t('utilities.openWindow'),
    notificationCount: 0,
    ...config.utilities,
  };
  const sections =
    typeof config.sections === 'function'
      ? config.sections({
        record: displayedRecord,
        editing: displayedEditing,
        onRecordChange: record ? state.changeRecord : () => undefined,
      })
      : config.sections;
  if (!canView) return <AccessDeniedState />;
  const listPane =
    config.presentation?.mode === 'grid' && config.presentation.columns ? (
      <Box sx={{ height: '100%', minHeight: 0, display: 'flex', flexDirection: 'column' }}>
        {state.filterVisible && (
          <Box sx={{ p: 1 }}>
            <TextField
              fullWidth
              size="small"
              placeholder={labels.filter}
              value={state.query}
              disabled={state.editing}
              onChange={(event) => state.setQuery(event.target.value)}
              slotProps={{
                input: {
                  startAdornment: (
                    <InputAdornment position="start">
                      <SearchIcon sx={{ fontSize: 16 }} />
                    </InputAdornment>
                  ),
                },
              }}
              sx={filterSx}
            />
          </Box>
        )}
        {config.presentation.headerContent}
        <Box sx={{ flex: 1, minHeight: 0 }}>
          <DataGrid
            rows={state.visibleRecords}
            columns={config.presentation.columns}
            loading={state.loading}
            height="100%"
            rowHeight={config.presentation.masterRowHeight}
            headerHeight={config.presentation.masterHeaderHeight}
            hideToolbar
            hideFooter
            hideSidebar
            hideFilterRow
            selectionMode="single"
            selectedIds={state.selectedId ? [state.selectedId] : []}
            onRowClick={state.choose}
            storageKey={config.presentation.storageKey}
          />
        </Box>
      </Box>
    ) : (
      <RecordList
        records={state.visibleRecords}
        selectedId={state.selectedId}
        editing={state.editing}
        query={state.query}
        filterVisible={state.filterVisible}
        filterLabel={labels.filter}
        getPrimaryText={config.getPrimaryText}
        getSecondaryText={config.getSecondaryText}
        batchSize={config.presentation?.recordListBatchSize}
        onQueryChange={state.setQuery}
        onSelect={state.choose}
      />
    );
  const fullscreen = config.presentation?.fullscreenCanvas;
  return (
    <PageContainer
      sx={{
        gap: 0,
        minHeight: { xs: 520, lg: 0 },
        height: fullscreen ? 'calc(100vh - 5px)' : { xs: 'auto', lg: '100%' },
        maxHeight: fullscreen ? 'calc(100vh - 5px)' : { lg: '100%' },
        overflow: 'hidden',
        position: fullscreen ? 'fixed' : 'relative',
        inset: fullscreen ? '5px 0 0 5px' : undefined,
        zIndex: fullscreen ? 1300 : undefined,
        pr: { lg: `${d365.utilityRailWidth}px` },
        bgcolor: d365.canvas,
        fontFamily: d365.fontFamily,
        color: d365.text,
        '& .MuiTypography-root, & .MuiButton-root, & .MuiInputBase-root, & input, & textarea': {
          fontFamily: `${d365.fontFamily} !important`,
        },
      }}
    >
      <ActionPane
        variant="flat"
        endActions={
          <EnterpriseCommandUtilities
            disabled={state.editing}
            {...utilities}
            onRefresh={state.refresh}
          />
        }
      >
        <IconButton size="small" sx={{ color: d365.primary, p: '5px' }}>
          <ArrowBackIcon sx={{ fontSize: 17 }} />
        </IconButton>
        <IconButton
          size="small"
          aria-label={t('actions.toggleList', 'Toggle record list')}
          aria-pressed={listPaneVisible}
          onClick={() => setListPaneVisible((visible) => !visible)}
          sx={{
            width: 31,
            height: 31,
            bgcolor: listPaneVisible ? d365.primary : 'transparent',
            color: listPaneVisible ? '#fff' : d365.primary,
            borderRadius: d365.radius,
            transition: 'background-color 140ms ease, color 140ms ease',
            '&:hover': { bgcolor: listPaneVisible ? '#2559d6' : 'action.hover' },
          }}
        >
          <MenuIcon sx={{ fontSize: 17 }} />
        </IconButton>
        <EnterpriseCrudActions
          editing={state.editing}
          {...crud}
          canEdit={Boolean(state.selected) && canEdit && !state.saving}
          canDelete={Boolean(state.selected) && canDelete && !state.saving}
          editPermission={config.permissions?.edit}
          newPermission={config.permissions?.create}
          deletePermission={config.permissions?.delete}
          onEdit={state.startEdit}
          onNew={canCreate ? state.startNew : undefined}
          onDelete={() => setDeleteConfirmationOpen(true)}
          onSave={state.save}
          onCancel={state.cancel}
        />
        <ActionPaneGroup>
          {config.commands?.map((command) => (
            <ActionPaneButton
              key={command.id}
              label={command.label}
              disabled={
                state.editing || command.disabled || (command.requiresSelection && !state.selected)
              }
              onClick={() => command.onClick?.(state.selected)}
            />
          ))}
          <IconButton disabled={state.editing} size="small" sx={{ color: 'primary.main' }}>
            <SearchIcon sx={{ fontSize: 17 }} />
          </IconButton>
        </ActionPaneGroup>
      </ActionPane>
      <Box
        sx={{
          display: 'flex',
          flex: 1,
          height: '100%',
          minHeight: 0,
          gap: '23px',
          ml: '1px',
          overflow: 'hidden',
          position: 'relative',
          alignItems: 'stretch',
        }}
      >
        {state.error ? (
          <ErrorState message={state.error} onRetry={state.refresh} />
        ) : state.loading && !state.records.length ? (
          <LoadingState />
        ) : (
          <ListDetailsLayout
            editing={displayedEditing}
            values={config.getValues(displayedRecord)}
            yesLabel={labels.yes}
            noLabel={labels.no}
            sections={sections as DetailSectionConfig[]}
            onChange={record ? state.changeValue : () => undefined}
            listWidth={config.presentation?.listWidth}
            listMinWidth={config.presentation?.listMinWidth}
            listMaxWidth={config.presentation?.listMaxWidth}
            listResizable={config.presentation?.listResizable}
            listPaneVisible={listPaneVisible}
            onListPaneClose={() => setListPaneVisible(false)}
            listWidthStorageKey={
              config.presentation?.listWidthStorageKey ?? globalThis.location?.pathname
            }
            listPane={listPane}
            header={
              <>
                <RecordHeader
                  title={title}
                  viewLabel={labels.view}
                  yesLabel={labels.yes}
                  noLabel={labels.no}
                  record={displayedRecord}
                  fields={config.headerFields.map((field) =>
                    config.numberSequence && field.id === String(config.numberSequence.field)
                      ? { ...field, disabled: !state.numberSequenceMetadata?.manual }
                      : field
                  )}
                  editing={displayedEditing}
                  maxWidth={config.presentation?.headerMaxWidth}
                  onChange={record ? state.changeHeader : () => undefined}
                />
                {record && Object.keys(state.validationErrors).length > 0 && (
                  <Alert severity="error" sx={{ mb: 1 }}>
                    <Typography sx={{ fontWeight: 600, fontSize: '0.75rem' }}>
                      {config.validationTitle ??
                        t('validation.correctErrors', 'Please correct the validation errors.')}
                    </Typography>
                    {Object.entries(state.validationErrors).map(([field, message]) => (
                      <Typography key={field} sx={{ fontSize: '0.6875rem' }}>
                        {message}
                      </Typography>
                    ))}
                  </Alert>
                )}
              </>
            }
          />
        )}
        {!record && !state.loading && !state.error && (
          <Box sx={{ position: 'absolute', inset: 0, pointerEvents: 'none' }}>
            <EmptyDataWatermark />
          </Box>
        )}
        {state.filterPanelOpen && config.advancedFilter && (
          <Box sx={sidePanelSx}>
            <EnterpriseFilterPanel
              title={config.advancedFilter.title ?? t('filters.title')}
              addLabel={config.advancedFilter.addLabel ?? t('actions.add')}
              fieldOptions={
                config.advancedFilter.fields?.map(({ id, label }) => ({ value: id, label })) ?? [
                  { value: 'default', label: config.advancedFilter.fieldLabel },
                ]
              }
              conditions={state.draftAdvancedFilters}
              operatorOptions={getFilterOperatorOptions(t)}
              applyLabel={config.advancedFilter.applyLabel ?? t('actions.apply')}
              resetLabel={config.advancedFilter.resetLabel ?? t('actions.reset')}
              onConditionsChange={state.setDraftAdvancedFilters}
              onApply={state.applyAdvancedFilter}
              onReset={state.resetAdvancedFilter}
            />
          </Box>
        )}
        {state.informationPanelOpen && config.relatedInformation && (
          <Box sx={sidePanelSx}>
            <RelatedInformationPanel
              title={config.relatedInformation.title ?? t('relatedInformation.title')}
              sections={config.relatedInformation.sections(state.selected)}
            />
          </Box>
        )}
      </Box>
      <RightUtilityRail
        filterLabel={labels.filter}
        informationLabel={labels.information}
        filterActive={config.advancedFilter ? state.filterPanelOpen : state.filterVisible}
        informationActive={state.informationPanelOpen}
        onFilter={state.toggleFilter}
        onInformation={state.toggleInformation}
        showInformation={Boolean(config.relatedInformation) || Boolean(config.showInformation)}
        disabled={state.editing}
      />
      {dialogs}
      <ConfirmationDialog
        open={deleteConfirmationOpen}
        onClose={() => setDeleteConfirmationOpen(false)}
        onConfirm={() => {
          setDeleteConfirmationOpen(false);
          void state.remove();
        }}
        severity="error"
        title={t('dialogs.confirmDeleteTitle', 'Confirm deletion')}
        message={t('dialogs.confirmDeleteOne', 'Delete the selected record?')}
        confirmLabel={t('actions.delete')}
        cancelLabel={t('actions.cancel')}
        loading={state.saving}
      />
    </PageContainer>
  );
}

export function RecordList<T extends ListDetailRecord>({
  records,
  selectedId,
  editing,
  query,
  filterVisible,
  filterLabel,
  getPrimaryText,
  getSecondaryText,
  batchSize = 30,
  onQueryChange,
  onSelect,
}: {
  records: T[];
  selectedId: string | null;
  editing: boolean;
  query: string;
  filterVisible: boolean;
  filterLabel: string;
  getPrimaryText: (record: T) => string;
  getSecondaryText?: (record: T) => string;
  batchSize?: number;
  onQueryChange: (value: string) => void;
  onSelect: (record: T) => void;
}) {
  const safeBatchSize = Math.max(1, Math.floor(batchSize));
  const [visibleCount, setVisibleCount] = React.useState(safeBatchSize);
  const loadMoreRef = React.useRef<HTMLDivElement | null>(null);
  const visibleRecords = records.slice(0, visibleCount);
  const hasMore = visibleCount < records.length;

  React.useEffect(() => setVisibleCount(safeBatchSize), [query, records, safeBatchSize]);

  const loadNextBatch = React.useCallback(() => {
    setVisibleCount((current) => Math.min(records.length, current + safeBatchSize));
  }, [records.length, safeBatchSize]);

  React.useEffect(() => {
    const target = loadMoreRef.current;
    if (!target || !hasMore || typeof IntersectionObserver === 'undefined') return undefined;
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) loadNextBatch();
      },
      { root: target.parentElement, rootMargin: '120px 0px' }
    );
    observer.observe(target);
    return () => observer.disconnect();
  }, [hasMore, loadNextBatch]);

  const handleScroll = (event: React.UIEvent<HTMLDivElement>) => {
    const target = event.currentTarget;
    if (hasMore && target.scrollHeight - target.scrollTop - target.clientHeight <= 120)
      loadNextBatch();
  };

  return (
    <Box sx={{ height: '100%', minHeight: 0, display: 'flex', flexDirection: 'column' }}>
      {filterVisible && (
        <Box sx={{ p: '14px 10px 8px', flexShrink: 0 }}>
          <TextField
            fullWidth
            size="small"
            placeholder={filterLabel}
            value={query}
            disabled={editing}
            onChange={(event) => onQueryChange(event.target.value)}
            slotProps={{
              input: {
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon sx={{ fontSize: 14 }} />
                  </InputAdornment>
                ),
              },
            }}
            sx={filterSx}
          />
        </Box>
      )}
      <Box
        onScroll={handleScroll}
        sx={{ flex: 1, minHeight: 0, overflowY: 'auto', scrollbarWidth: 'thin' }}
      >
        <List disablePadding>
          {visibleRecords.map((record) => (
            <ListItemButton
              key={record.id}
              selected={record.id === selectedId}
              disabled={editing && record.id !== selectedId}
              onClick={() => onSelect(record)}
              sx={{
                display: 'block',
                px: '10px',
                py: '10px',
                minHeight: 67,
                borderBottom: `1px solid ${d365.darkBorder}`,
                borderInlineStart: record.id === selectedId ? 4 : 0,
                borderInlineStartColor: d365.selectedBar,
                '&.Mui-selected, &.Mui-selected:hover': { bgcolor: d365.selectedRow },
              }}
            >
              <Typography
                noWrap
                sx={{
                  fontSize: 15,
                  lineHeight: 1.3,
                  fontWeight: 400,
                  color: record.id === selectedId ? '#1746aa' : d365.text,
                }}
              >
                {getPrimaryText(record)}
              </Typography>
              {getSecondaryText && (
                <Typography
                  sx={{
                    mt: '6px',
                    fontSize: 10,
                    lineHeight: 1.2,
                    color: record.id === selectedId ? '#1746aa' : d365.text,
                  }}
                >
                  {getSecondaryText(record)}
                </Typography>
              )}
            </ListItemButton>
          ))}
        </List>
        {hasMore && (
          <Box
            ref={loadMoreRef}
            role="status"
            aria-label="Loading more records"
            sx={{ height: 42, display: 'flex', alignItems: 'center', justifyContent: 'center' }}
          >
            <CircularProgress size={18} />
          </Box>
        )}
      </Box>
    </Box>
  );
}

function RecordHeader<T>({
  title,
  viewLabel,
  yesLabel,
  noLabel,
  record,
  fields,
  editing,
  maxWidth,
  onChange,
}: {
  title: string;
  viewLabel: string;
  yesLabel: string;
  noLabel: string;
  record: T;
  fields: ListDetailsHeaderField<T>[];
  editing: boolean;
  maxWidth?: number;
  onChange: (id: string, value: DetailValue) => void;
}) {
  return (
    <Box sx={{ px: 0, pt: '3px', pb: '3px', minHeight: 130, boxSizing: 'border-box' }}>
      <Typography sx={{ fontSize: 11, lineHeight: 1.5 }}>{viewLabel}</Typography>
      <Typography
        component="h1"
        sx={{
          mt: '5px',
          mb: '21px',
          fontSize: d365.titleFontSize,
          lineHeight: 1.2,
          fontWeight: 600,
        }}
      >
        {title}
      </Typography>
      <Box
        sx={{
          display: 'grid',
          mt: '5px',
          width: '100%',
          maxWidth,
          gridTemplateColumns: {
            xs: '1fr 1fr',
            lg: fields.some((field) => field.width)
              ? fields
                  .map((field) =>
                    typeof field.width === 'number'
                      ? `${field.width}px`
                      : (field.width ?? 'minmax(100px, 1fr)')
                  )
                  .join(' ')
              : `repeat(${Math.min(7, fields.length)},minmax(140px,1fr))`,
          },
          gap: '10px',
        }}
      >
        {fields.map((field) => {
          const value = field.getValue(record);
          const editable = editing && !field.disabled && field.type !== 'display';
          return (
            <Box key={field.id}>
              <Typography
                noWrap
                title={field.label}
                sx={{ mb: '2px', fontSize: d365.labelFontSize, lineHeight: 1.1, color: d365.text }}
              >
                {field.label}
              </Typography>
              {field.type === 'boolean' ? (
                <Box sx={{ display: 'flex', alignItems: 'center', height: d365.controlHeight }}>
                  <Switch
                    size="small"
                    checked={Boolean(value)}
                    disabled={!editable}
                    onChange={(_, checked) => onChange(field.id, checked)}
                  />
                  <Typography sx={{ fontSize: d365.fontSize }}>
                    {value ? yesLabel : noLabel}
                  </Typography>
                </Box>
              ) : editable && field.type === 'select' ? (
                <TextField
                  select
                  value={value}
                  onChange={(event) => onChange(field.id, event.target.value)}
                  sx={headerEditFieldSx}
                >
                  {(field.options ?? []).map((option) => (
                    <MenuItem key={option.value} value={option.value}>
                      {option.label}
                    </MenuItem>
                  ))}
                </TextField>
              ) : editable ? (
                <TextField
                  type={field.type === 'number' ? 'number' : 'text'}
                  value={value}
                  onChange={(event) =>
                    onChange(
                      field.id,
                      field.type === 'number' ? Number(event.target.value) : event.target.value
                    )
                  }
                  sx={headerEditFieldSx}
                />
              ) : (
                <HeaderViewField
                  value={
                    field.type === 'select'
                      ? (field.options?.find((option) => option.value === String(value))?.label ??
                        value)
                      : value
                  }
                  numeric={field.type === 'number'}
                />
              )}
            </Box>
          );
        })}
      </Box>
    </Box>
  );
}

function LegacyListDetailsPage<T extends ListDetailRecord>({
  title,
  subtitle,
  actionPane,
  dataGridProps,
  detailsPane,
  loading = false,
  selectedId,
  dialogs,
}: LegacyListDetailsProps<T>) {
  const { t } = useAppTranslation();
  return (
    <PageContainer>
      <PageHeader title={title} subtitle={subtitle} />
      {actionPane && <ActionPane>{actionPane}</ActionPane>}
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: selectedId ? 5 : 12, lg: selectedId ? 4 : 12 }}>
          <Box sx={{ height: 600, width: '100%' }}>
            <DataGrid {...dataGridProps} />
          </Box>
        </Grid>
        {selectedId && (
          <Grid size={{ xs: 12, md: 7, lg: 8 }}>
            <Paper
              elevation={0}
              sx={{
                p: 2,
                borderRadius: 1,
                border: (theme) => `1px solid ${theme.palette.divider}`,
                minHeight: 600,
              }}
            >
              {loading ? (
                <LoadingState message={t('messages.loadingDetails')} />
              ) : (
                (detailsPane ?? (
                  <EmptyState
                    title={t('messages.selectRecord')}
                    message={t('messages.selectRecordHelp')}
                  />
                ))
              )}
            </Paper>
          </Grid>
        )}
      </Grid>
      {dialogs}
    </PageContainer>
  );
}

const filterSx = {
  '& .MuiInputBase-root': { height: 34, borderRadius: d365.radius, fontSize: d365.fontSize },
  '& .MuiInputAdornment-root': { mr: 0.25 },
};
function HeaderViewField({
  value,
  numeric = false,
}: {
  value: DetailValue;
  numeric?: boolean;
}): React.ReactElement {
  return (
    <Box
      sx={{
        minHeight: d365.controlHeight,
        display: 'flex',
        alignItems: 'center',
        justifyContent: numeric ? 'flex-end' : 'flex-start',
        borderBottom: `1px solid ${d365.darkBorder}`,
        px: 0.5,
        overflow: 'hidden',
        bgcolor: 'transparent',
      }}
    >
      <Typography noWrap sx={{ fontSize: d365.fontSize, color: '#315efb' }}>
        {String(value ?? '')}
      </Typography>
    </Box>
  );
}

const headerEditFieldSx = {
  width: '100%',
  mt: '5px',
  '& .MuiInputBase-root': {
    height: d365.controlHeight,
    borderRadius: d365.radius,
    fontSize: d365.fontSize,
  },
  '& .MuiInputBase-input': { px: 0.75, py: 0.25 },
};
const sidePanelSx = {
  position: { xs: 'absolute', lg: 'static' },
  insetInlineEnd: 0,
  top: 0,
  bottom: 0,
  zIndex: 4,
  height: '100%',
  minHeight: 0,
  display: 'flex',
};
const getFilterOperatorOptions = (
  t: (key: string, options?: Record<string, unknown>) => string
) => [
    { value: 'contains' as const, label: t('filters.contains') },
    { value: 'equals' as const, label: t('filters.equals') },
    { value: 'startsWith' as const, label: t('filters.startsWith') },
    { value: 'endsWith' as const, label: t('filters.endsWith') },
    { value: 'notEquals' as const, label: t('filters.notEquals') },
    { value: 'doesNotContain' as const, label: t('filters.doesNotContain') },
  ];
