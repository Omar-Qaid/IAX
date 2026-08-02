import React from 'react';
import { Alert, Box, Grid, IconButton, InputAdornment, List, ListItemButton, MenuItem, Paper, TextField, Typography } from '@mui/material';
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
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { AccessDeniedState } from '@shared/components/feedback/AccessDeniedState';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { usePermission } from '@core/permissions/usePermission';
import { ListDetailsLayout } from './ListDetailsLayout';
import { useListDetailsPage } from './useListDetailsPage';
import type { DetailValue, EnterpriseListDetailsConfig, ListDetailRecord, ListDetailsHeaderField } from './types';

interface LegacyListDetailsProps<T extends ListDetailRecord> {
  variant?: 'standard'; title: string; subtitle?: string; actionPane?: React.ReactNode;
  dataGridProps: DataGridProps<T>; detailsPane?: React.ReactNode; loading?: boolean;
  selectedId?: string | null; dialogs?: React.ReactNode;
}
interface EnterpriseListDetailsProps<T extends ListDetailRecord> {
  variant: 'enterprise'; title: string; config: EnterpriseListDetailsConfig<T>; dialogs?: React.ReactNode;
}
export type ListDetailsPageProps<T extends ListDetailRecord = ListDetailRecord> = LegacyListDetailsProps<T> | EnterpriseListDetailsProps<T>;

export function ListDetailsPage<T extends ListDetailRecord = ListDetailRecord>(props: ListDetailsPageProps<T>): React.ReactElement {
  return props.variant === 'enterprise' ? <EnterpriseListDetailsPage title={props.title} config={props.config} dialogs={props.dialogs} /> : <LegacyListDetailsPage {...props} />;
}

function EnterpriseListDetailsPage<T extends ListDetailRecord>({ title, config, dialogs }: Omit<EnterpriseListDetailsProps<T>, 'variant'>): React.ReactElement {
  const state = useListDetailsPage(config);
  const { hasPermission: canView } = usePermission(config.permissions?.view);
  const { hasPermission: canCreate } = usePermission(config.permissions?.create);
  const { hasPermission: canEdit } = usePermission(config.permissions?.edit);
  const { hasPermission: canDelete } = usePermission(config.permissions?.delete);
  const record = state.draft;
  if (!canView) return <AccessDeniedState />;
  const listPane = config.presentation?.mode === 'grid' && config.presentation.columns
    ? <Box sx={{ height: '100%', minHeight: 0, display: 'flex', flexDirection: 'column' }}>{state.filterVisible && <Box sx={{ p: 1 }}><TextField fullWidth size="small" placeholder={config.filterLabel} value={state.query} disabled={state.editing} onChange={(event) => state.setQuery(event.target.value)} slotProps={{ input: { startAdornment: <InputAdornment position="start"><SearchIcon sx={{ fontSize: 16 }} /></InputAdornment> } }} sx={filterSx} /></Box>}<Box sx={{ flex: 1, minHeight: 0 }}><DataGrid rows={state.visibleRecords} columns={config.presentation.columns} loading={state.loading} height="100%" hideToolbar hideFooter hideSidebar hideFilterRow selectionMode="single" selectedIds={state.selectedId ? [state.selectedId] : []} onRowClick={state.choose} storageKey={config.presentation.storageKey} /></Box></Box>
    : <RecordList records={state.visibleRecords} selectedId={state.selectedId} editing={state.editing} query={state.query} filterVisible={state.filterVisible} filterLabel={config.filterLabel} getPrimaryText={config.getPrimaryText} getSecondaryText={config.getSecondaryText} onQueryChange={state.setQuery} onSelect={state.choose} />;
  return <PageContainer sx={{ gap: 0.5, minHeight: { xs: 520, lg: 0 }, height: { xs: 'auto', lg: '100%' }, maxHeight: { lg: '100%' }, overflow: { lg: 'hidden' }, position: 'relative', pr: { lg: '38px' }, bgcolor: '#faf9f8' }}>
    <ActionPane variant="flat" endActions={<EnterpriseCommandUtilities disabled={state.editing} {...config.utilities} onRefresh={state.refresh} />}>
      <IconButton size="small" sx={{ color: 'primary.main' }}><ArrowBackIcon sx={{ fontSize: 18 }} /></IconButton>
      <IconButton size="small" sx={{ bgcolor: 'primary.main', color: 'primary.contrastText', borderRadius: 0.5 }}><MenuIcon sx={{ fontSize: 18 }} /></IconButton>
      <EnterpriseCrudActions editing={state.editing} {...config.crud} canEdit={Boolean(state.selected) && canEdit && !state.saving} canDelete={Boolean(state.selected) && canDelete && !state.saving} editPermission={config.permissions?.edit} newPermission={config.permissions?.create} deletePermission={config.permissions?.delete} onEdit={state.startEdit} onNew={canCreate ? state.startNew : undefined} onDelete={state.remove} onSave={state.save} onCancel={state.cancel} />
      <ActionPaneGroup>{config.commands?.map((command) => <ActionPaneButton key={command.id} label={command.label} disabled={state.editing || command.disabled} onClick={command.onClick} />)}<IconButton disabled={state.editing} size="small" sx={{ color: 'primary.main' }}><SearchIcon sx={{ fontSize: 17 }} /></IconButton></ActionPaneGroup>
    </ActionPane>
    <Box sx={{ display: 'flex', flex: 1, height: '100%', minHeight: 0, gap: 1, overflow: 'hidden', position: 'relative', alignItems: 'stretch' }}>
    {state.error ? <ErrorState message={state.error} onRetry={state.refresh} /> : state.loading && !state.records.length ? <LoadingState /> : record ? <ListDetailsLayout editing={state.editing} values={config.getValues(record)} yesLabel={config.yesLabel} noLabel={config.noLabel} sections={config.sections} onChange={state.changeValue} listWidth={config.presentation?.listWidth}
      listPane={listPane}
      header={<><RecordHeader title={title} viewLabel={config.viewLabel} record={record} fields={config.headerFields} editing={state.editing} onChange={state.changeHeader} />{Object.keys(state.validationErrors).length > 0 && <Alert severity="error" sx={{ mb: 1 }}><Typography sx={{ fontWeight: 600, fontSize: '0.75rem' }}>{config.validationTitle ?? 'Please correct the validation errors.'}</Typography>{Object.entries(state.validationErrors).map(([field, message]) => <Typography key={field} sx={{ fontSize: '0.6875rem' }}>{message}</Typography>)}</Alert>}</>}
    /> : <EmptyState title="No records" />}
    {state.filterPanelOpen && config.advancedFilter && <Box sx={sidePanelSx}><EnterpriseFilterPanel title={config.advancedFilter.title} addLabel={config.advancedFilter.addLabel} fieldLabel={config.advancedFilter.fieldLabel} operatorLabel={config.advancedFilter.operatorLabel} value={state.draftAdvancedFilter} applyLabel={config.advancedFilter.applyLabel} resetLabel={config.advancedFilter.resetLabel} onValueChange={state.setDraftAdvancedFilter} onApply={state.applyAdvancedFilter} onReset={state.resetAdvancedFilter} onRemove={state.resetAdvancedFilter} /></Box>}
    {state.informationPanelOpen && config.relatedInformation && <Box sx={sidePanelSx}><RelatedInformationPanel title={config.relatedInformation.title} sections={config.relatedInformation.sections(state.selected)} /></Box>}
    </Box>
    <RightUtilityRail filterLabel={config.filterLabel} informationLabel={config.informationLabel} filterActive={config.advancedFilter ? state.filterPanelOpen : state.filterVisible} informationActive={state.informationPanelOpen} onFilter={state.toggleFilter} onInformation={state.toggleInformation} showInformation={Boolean(config.relatedInformation) || Boolean(config.showInformation)} disabled={state.editing} />
    {dialogs}
  </PageContainer>;
}

function RecordList<T extends ListDetailRecord>({ records, selectedId, editing, query, filterVisible, filterLabel, getPrimaryText, getSecondaryText, onQueryChange, onSelect }: {
  records: T[]; selectedId: string | null; editing: boolean; query: string; filterVisible: boolean; filterLabel: string;
  getPrimaryText: (record: T) => string; getSecondaryText?: (record: T) => string;
  onQueryChange: (value: string) => void; onSelect: (record: T) => void;
}) {
  return <>{filterVisible && <Box sx={{ p: 1 }}><TextField fullWidth size="small" placeholder={filterLabel} value={query} disabled={editing} onChange={(event) => onQueryChange(event.target.value)} slotProps={{ input: { startAdornment: <InputAdornment position="start"><SearchIcon sx={{ fontSize: 16 }} /></InputAdornment> } }} sx={filterSx} /></Box>}<List disablePadding>{records.map((record) => <ListItemButton key={record.id} selected={record.id === selectedId} disabled={editing && record.id !== selectedId} onClick={() => onSelect(record)} sx={{ display: 'block', px: 1.5, py: 1, borderBottom: 1, borderColor: 'divider', borderInlineStart: record.id === selectedId ? 4 : 0, borderInlineStartColor: 'primary.main', '&.Mui-selected': { bgcolor: '#dce6f9' } }}><Typography sx={{ fontSize: '0.875rem', fontWeight: 600 }}>{getPrimaryText(record)}</Typography>{getSecondaryText && <Typography sx={{ fontSize: '0.6875rem' }}>{getSecondaryText(record)}</Typography>}</ListItemButton>)}</List></>;
}

function RecordHeader<T>({ title, viewLabel, record, fields, editing, onChange }: { title: string; viewLabel: string; record: T; fields: ListDetailsHeaderField<T>[]; editing: boolean; onChange: (id: string, value: DetailValue) => void }) {
  return <Box sx={{ px: 0.25, pb: 1.5 }}><Typography sx={{ fontSize: '0.75rem' }}>{viewLabel}</Typography><Typography component="h1" sx={{ mb: 1.25, fontSize: '1.35rem', fontWeight: 600 }}>{title}</Typography><Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr 1fr', lg: `repeat(${Math.min(7, fields.length)},minmax(100px,1fr))` }, gap: 1.25 }}>{fields.map((field) => {
    const value = field.getValue(record); const disabled = !editing || field.disabled || field.type === 'display';
    return <Box key={field.id}><Typography noWrap title={field.label} sx={{ fontSize: '0.6875rem', color: 'text.secondary' }}>{field.label}</Typography>{field.type === 'select' ? <TextField select value={value} disabled={disabled} onChange={(event) => onChange(field.id, event.target.value)} sx={headerFieldSx}>{(field.options ?? []).map((option) => <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>)}</TextField> : <TextField type={field.type === 'number' ? 'number' : 'text'} value={value} disabled={disabled} onChange={(event) => onChange(field.id, field.type === 'number' ? Number(event.target.value) : event.target.value)} sx={headerFieldSx} />}</Box>;
  })}</Box></Box>;
}

function LegacyListDetailsPage<T extends ListDetailRecord>({ title, subtitle, actionPane, dataGridProps, detailsPane, loading = false, selectedId, dialogs }: LegacyListDetailsProps<T>) {
  const { t } = useAppTranslation();
  return <PageContainer><PageHeader title={title} subtitle={subtitle} />{actionPane && <ActionPane>{actionPane}</ActionPane>}<Grid container spacing={2}><Grid size={{ xs: 12, md: selectedId ? 5 : 12, lg: selectedId ? 4 : 12 }}><Box sx={{ height: 600, width: '100%' }}><DataGrid {...dataGridProps} /></Box></Grid>{selectedId && <Grid size={{ xs: 12, md: 7, lg: 8 }}><Paper elevation={0} sx={{ p: 2, borderRadius: 1, border: (theme) => `1px solid ${theme.palette.divider}`, minHeight: 600 }}>{loading ? <LoadingState message={t('messages.loadingDetails')} /> : detailsPane ?? <EmptyState title={t('messages.selectRecord')} message={t('messages.selectRecordHelp')} />}</Paper></Grid>}</Grid>{dialogs}</PageContainer>;
}

const filterSx = { '& .MuiInputBase-root': { height: 29, borderRadius: 0.5, fontSize: '0.75rem' } };
const headerFieldSx = { width: '100%', '& .MuiInputBase-root': { height: 29, borderRadius: 0, fontSize: '0.75rem' }, '& fieldset': { borderWidth: '0 0 1px !important' }, '& .MuiInputBase-input.Mui-disabled': { WebkitTextFillColor: 'currentColor' } };
const sidePanelSx = { position: { xs: 'absolute', lg: 'static' }, insetInlineEnd: 0, top: 0, bottom: 0, zIndex: 4, height: '100%', minHeight: 0, display: 'flex' };
