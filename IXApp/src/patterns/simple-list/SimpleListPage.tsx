import React, { useMemo, useState } from 'react';
import { Box, type SxProps, type Theme } from '@mui/material';
import { PageContainer } from '@shared/components/page/PageContainer';
import { PageHeader } from '@shared/components/page/PageHeader';
import { EnterpriseListHeader } from '@shared/components/page/EnterpriseListHeader';
import { RightUtilityRail } from '@shared/components/page/RightUtilityRail';
import { RelatedInformationPanel, type RelatedInformationSection } from '@shared/components/page/RelatedInformationPanel';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import { ActionPaneGroup } from '@shared/components/action-pane/ActionPaneGroup';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { EnterpriseCrudActions } from '@shared/components/action-pane/EnterpriseCrudActions';
import { EnterpriseCommandUtilities } from '@shared/components/action-pane/EnterpriseCommandUtilities';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import { EnterpriseQuickFilter } from '@shared/components/data-grid/EnterpriseQuickFilter';
import { EnterpriseListFilterBar } from '@shared/components/data-grid/EnterpriseListFilterBar';
import { EnterpriseFilterPanel } from '@shared/components/data-grid/EnterpriseFilterPanel';
import type { ColumnDef, DataGridProps } from '@shared/components/data-grid/types';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { SimpleListDataSource } from './types';
import { useSimpleListDataSource } from './useSimpleListDataSource';

export type SimpleListGridProps<T> = Omit<DataGridProps<T>, 'rows' | 'columns'>;

export interface EnterpriseListSearchField<T> {
  field: keyof T & string;
  label: string;
}

export interface EnterpriseListCommand {
  id: string;
  label: string;
  permission?: string;
  disabled?: boolean;
  onClick?: () => void;
}

export interface EnterpriseListCrudConfig<T> {
  editLabel: string;
  newLabel: string;
  deleteLabel: string;
  editPermission?: string;
  newPermission?: string;
  deletePermission?: string;
  onEdit?: (row: T) => void;
  onNew?: () => void;
  onDelete?: (rows: T[]) => void;
}

export interface EnterpriseListAdvancedFilter<T> {
  title: string;
  addLabel: string;
  fieldLabel: string;
  operatorLabel: string;
  applyLabel: string;
  resetLabel: string;
  matches: (row: T, value: string) => boolean;
}

export interface EnterpriseListConfig<T> {
  contextLabel: string;
  viewLabel: string;
  filterLabel: string;
  informationLabel: string;
  searchByLabel?: string;
  searchMode?: 'quick' | 'field';
  searchFields: EnterpriseListSearchField<T>[];
  defaultSearchField?: keyof T & string;
  locale?: string;
  crud: EnterpriseListCrudConfig<T>;
  commands?: EnterpriseListCommand[];
  utilities: {
    personalizeLabel: string;
    guideLabel: string;
    notificationsLabel: string;
    refreshLabel: string;
    openWindowLabel: string;
    notificationCount?: number;
  };
  advancedFilter?: EnterpriseListAdvancedFilter<T>;
  relatedInformation?: {
    title: string;
    sections: (selectedRow: T | null) => RelatedInformationSection[];
  };
  initialSelection?: 'first' | 'none';
  showFilterOnLoad?: boolean;
  advancedFilterOpenOnLoad?: boolean;
  informationOpenOnLoad?: boolean;
  onReset?: () => void;
}

export interface SimpleListPageProps<T extends { id: string } = { id: string }> {
  title: string;
  subtitle?: string;
  variant?: 'standard' | 'enterprise';
  enterpriseConfig?: EnterpriseListConfig<T>;
  /** Low-level slots retained for exceptional layouts and backward compatibility. */
  contextLabel?: string;
  viewLabel?: string;
  onViewClick?: () => void;
  actionPane?: React.ReactNode;
  actionPaneEndActions?: React.ReactNode;
  filterBar?: React.ReactNode;
  sidePanels?: React.ReactNode;
  utilityRail?: React.ReactNode;
  dataSource: SimpleListDataSource<T>;
  columns: ColumnDef<T>[];
  dataGridProps?: SimpleListGridProps<T>;
  loading?: boolean;
  error?: string | null;
  onRetry?: () => void;
  dialogs?: React.ReactNode;
  gridHeight?: number | string;
  contentMinHeight?: number;
  containerSx?: SxProps<Theme>;
  contentSx?: SxProps<Theme>;
}

export function SimpleListPage<T extends { id: string } = { id: string }>(props: SimpleListPageProps<T>): React.ReactElement {
  const {
    title, subtitle, variant = 'standard', enterpriseConfig, onViewClick, dataGridProps = {},
    loading = false, error, onRetry, dialogs, gridHeight, contentMinHeight = 520,
    containerSx, contentSx,
  } = props;
  const { t } = useAppTranslation();
  const sourceState = useSimpleListDataSource(props.dataSource);
  const rows = sourceState.rows;
  const columns = props.columns;
  const getRowId = dataGridProps.getRowId ?? ((row: T) => row.id);
  const initialRow = enterpriseConfig?.initialSelection === 'none' ? null : (rows[0] ?? null);
  const [selectedIds, setSelectedIds] = useState<(string | number)[]>(initialRow ? [getRowId(initialRow)] : []);
  const [selectedRow, setSelectedRow] = useState<T | null>(initialRow);
  const [query, setQuery] = useState('');
  const [searchField, setSearchField] = useState<string>(enterpriseConfig?.defaultSearchField ?? enterpriseConfig?.searchFields[0]?.field ?? '');
  const [quickFilterVisible, setQuickFilterVisible] = useState(enterpriseConfig?.showFilterOnLoad ?? true);
  const [filterPanelOpen, setFilterPanelOpen] = useState(enterpriseConfig?.advancedFilterOpenOnLoad ?? false);
  const [informationPanelOpen, setInformationPanelOpen] = useState(enterpriseConfig?.informationOpenOnLoad ?? false);
  const [draftAdvancedFilter, setDraftAdvancedFilter] = useState('');
  const [advancedFilter, setAdvancedFilter] = useState('');

  const processedRows = useMemo(() => {
    if (!enterpriseConfig) return rows;
    const normalized = query.trim().toLocaleLowerCase(enterpriseConfig.locale);
    return rows.filter((row) => {
      const fields = enterpriseConfig.searchMode === 'field'
        ? enterpriseConfig.searchFields.filter((candidate) => candidate.field === searchField)
        : enterpriseConfig.searchFields;
      const matchesSearch = !normalized || fields.some(({ field }) => String(row[field] ?? '').toLocaleLowerCase(enterpriseConfig.locale).includes(normalized));
      const matchesAdvanced = !advancedFilter || !enterpriseConfig.advancedFilter || enterpriseConfig.advancedFilter.matches(row, advancedFilter);
      return matchesSearch && matchesAdvanced;
    });
  }, [advancedFilter, enterpriseConfig, query, rows, searchField]);

  const resolvedError = error ?? sourceState.error;
  const resolvedLoading = loading || sourceState.loading;
  const retry = onRetry ?? sourceState.refresh;
  const feedback = resolvedError
    ? <ErrorState message={resolvedError} onRetry={retry} />
    : resolvedLoading
      ? <LoadingState message={t('messages.loadingRecords')} />
      : null;

  if (variant === 'enterprise') {
    const config = enterpriseConfig;
    const reset = () => {
      const firstRow = config?.initialSelection === 'none' ? null : (rows[0] ?? null);
      setQuery('');
      setAdvancedFilter('');
      setDraftAdvancedFilter('');
      setQuickFilterVisible(config?.showFilterOnLoad ?? true);
      setFilterPanelOpen(config?.advancedFilterOpenOnLoad ?? false);
      setInformationPanelOpen(config?.informationOpenOnLoad ?? false);
      setSelectedRow(firstRow);
      setSelectedIds(firstRow ? [getRowId(firstRow)] : []);
      sourceState.refresh();
      config?.onReset?.();
    };
    const resolvedGridProps: DataGridProps<T> = config ? {
      ...dataGridProps,
      rows: processedRows,
      columns,
      selectionMode: dataGridProps.selectionMode ?? 'multiple',
      selectedIds,
      onSelectionChange: (ids) => {
        const normalizedIds = ids as (string | number)[];
        setSelectedIds(normalizedIds);
        setSelectedRow(rows.find((row) => getRowId(row) === normalizedIds.at(-1)) ?? null);
        dataGridProps.onSelectionChange?.(ids);
      },
      onRowClick: (row) => {
        setSelectedRow(row);
        dataGridProps.onRowClick?.(row);
      },
      rowHeight: dataGridProps.rowHeight ?? 31,
      headerHeight: dataGridProps.headerHeight ?? 36,
      hideAddRowButton: dataGridProps.hideAddRowButton ?? true,
      hideToolbar: dataGridProps.hideToolbar ?? true,
      hideFilterRow: dataGridProps.hideFilterRow ?? true,
      hideFooter: dataGridProps.hideFooter ?? true,
      showCellBorders: dataGridProps.showCellBorders ?? false,
      showColumnBorders: dataGridProps.showColumnBorders ?? false,
    } : { ...dataGridProps, rows, columns };
    const generatedActionPane = config && <>
      <EnterpriseCrudActions
        editLabel={config.crud.editLabel}
        newLabel={config.crud.newLabel}
        deleteLabel={config.crud.deleteLabel}
        canEdit={selectedIds.length === 1}
        canDelete={selectedIds.length > 0}
        onEdit={selectedRow ? () => config.crud.onEdit?.(selectedRow) : undefined}
        onNew={config.crud.onNew}
        onDelete={() => config.crud.onDelete?.(rows.filter((row) => selectedIds.includes(getRowId(row))))}
        editPermission={config.crud.editPermission}
        newPermission={config.crud.newPermission}
        deletePermission={config.crud.deletePermission}
      />
      {config.commands && <ActionPaneGroup>{config.commands.map((command) => <ActionPaneButton key={command.id} label={command.label} permission={command.permission} disabled={command.disabled} onClick={command.onClick} />)}</ActionPaneGroup>}
    </>;
    const generatedUtilities = config && <EnterpriseCommandUtilities {...config.utilities} onRefresh={reset} />;
    const generatedFilterBar = config && quickFilterVisible && (config.searchMode === 'field'
      ? <EnterpriseListFilterBar filterLabel={config.filterLabel} searchByLabel={config.searchByLabel ?? config.filterLabel} query={query} field={searchField} options={config.searchFields.map(({ field, label }) => ({ value: field, label }))} onQueryChange={setQuery} onFieldChange={setSearchField} />
      : <EnterpriseQuickFilter label={config.filterLabel} value={query} onChange={setQuery} />);
    const generatedSidePanels = config && <>
      {filterPanelOpen && config.advancedFilter && <Box sx={{ position: { xs: 'absolute', lg: 'static' }, insetInlineEnd: { xs: informationPanelOpen ? 253 : 0 }, top: 0, bottom: 0, zIndex: 4, height: '100%', minHeight: 0, display: 'flex' }}><EnterpriseFilterPanel
        title={config.advancedFilter.title} addLabel={config.advancedFilter.addLabel} fieldLabel={config.advancedFilter.fieldLabel} operatorLabel={config.advancedFilter.operatorLabel}
        value={draftAdvancedFilter} applyLabel={config.advancedFilter.applyLabel} resetLabel={config.advancedFilter.resetLabel}
        onValueChange={setDraftAdvancedFilter} onApply={() => setAdvancedFilter(draftAdvancedFilter)}
        onReset={() => { setDraftAdvancedFilter(''); setAdvancedFilter(''); }} onRemove={() => { setDraftAdvancedFilter(''); setAdvancedFilter(''); }}
      /></Box>}
      {informationPanelOpen && config.relatedInformation && <Box sx={{ position: { xs: 'absolute', lg: 'static' }, insetInlineEnd: 0, top: 0, bottom: 0, zIndex: 4, height: '100%', minHeight: 0, display: 'flex' }}><RelatedInformationPanel title={config.relatedInformation.title} sections={config.relatedInformation.sections(selectedRow)} /></Box>}
    </>;
    const generatedUtilityRail = config && <RightUtilityRail
      filterLabel={config.filterLabel} informationLabel={config.informationLabel}
      filterActive={config.advancedFilter ? filterPanelOpen : quickFilterVisible} informationActive={informationPanelOpen}
      onFilter={() => config.advancedFilter ? setFilterPanelOpen((open) => !open) : setQuickFilterVisible((visible) => !visible)}
      onInformation={() => setInformationPanelOpen((open) => !open)} showInformation={Boolean(config.relatedInformation)}
    />;

    return (
      <PageContainer sx={[{ gap: 0.5, minHeight: { xs: contentMinHeight, lg: 0 }, height: { xs: 'auto', lg: '100%' }, maxHeight: { lg: '100%' }, overflow: { lg: 'hidden' }, position: 'relative', pr: { lg: '38px' } }, ...(Array.isArray(containerSx) ? containerSx : [containerSx])]}>
        {(generatedActionPane ?? props.actionPane) && <ActionPane variant="flat" endActions={generatedUtilities ?? props.actionPaneEndActions}>{generatedActionPane ?? props.actionPane}</ActionPane>}
        <Box sx={[{ display: 'flex', flex: 1, height: '100%', minHeight: { xs: contentMinHeight, lg: 0 }, gap: 1, px: { xs: 0, sm: 1 }, pb: 0.5, overflow: 'hidden', position: 'relative', alignItems: 'stretch' }, ...(Array.isArray(contentSx) ? contentSx : [contentSx])]}>
          <Box sx={{ flex: 1, minWidth: 0, display: 'flex', flexDirection: 'column' }}>
            <EnterpriseListHeader contextLabel={config?.contextLabel ?? props.contextLabel ?? title} viewLabel={config?.viewLabel ?? props.viewLabel ?? title} onViewClick={onViewClick} />
            {generatedFilterBar ?? props.filterBar}
            <Box sx={{ flex: 1, minHeight: 0 }}>{feedback ?? <DataGrid {...resolvedGridProps} height={gridHeight ?? resolvedGridProps.height ?? '100%'} />}</Box>
          </Box>
          {generatedSidePanels ?? props.sidePanels}
        </Box>
        {generatedUtilityRail ?? props.utilityRail}
        {dialogs}
      </PageContainer>
    );
  }

  return <PageContainer sx={containerSx}>
    <PageHeader title={title} subtitle={subtitle} />
    {props.actionPane && <ActionPane endActions={props.actionPaneEndActions}>{props.actionPane}</ActionPane>}
    {feedback ?? <Box sx={{ width: '100%', height: gridHeight ?? 600 }}><DataGrid {...dataGridProps} rows={rows} columns={columns} /></Box>}
    {dialogs}
  </PageContainer>;
}
