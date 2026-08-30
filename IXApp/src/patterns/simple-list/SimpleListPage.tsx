import React, { useEffect, useMemo, useRef, useState } from 'react';
import { Box, type SxProps, type Theme } from '@mui/material';
import { PageContainer } from '@shared/components/page/PageContainer';
import { PageHeader } from '@shared/components/page/PageHeader';
import { EnterpriseListHeader } from '@shared/components/page/EnterpriseListHeader';
import {
  RIGHT_UTILITY_RAIL_WIDTH,
  RightUtilityRail,
} from '@shared/components/page/RightUtilityRail';
import {
  RelatedInformationPanel,
  type RelatedInformationSection,
} from '@shared/components/page/RelatedInformationPanel';
import { ActionPane } from '@shared/components/action-pane/ActionPane';
import { ActionPaneGroup } from '@shared/components/action-pane/ActionPaneGroup';
import { ActionPaneButton } from '@shared/components/action-pane/ActionPaneButton';
import { OptionsMenu } from '@shared/components/action-pane/OptionsMenu';
import { EnterpriseCrudActions } from '@shared/components/action-pane/EnterpriseCrudActions';
import { EnterpriseCommandUtilities } from '@shared/components/action-pane/EnterpriseCommandUtilities';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import {
  createEnterpriseFilterCondition,
  EnterpriseFilterPanel,
  matchesEnterpriseFilter,
  type EnterpriseFilterCondition,
} from '@shared/components/data-grid/EnterpriseFilterPanel';
import type { ColumnDef, DataGridHandle, DataGridProps } from '@shared/components/data-grid/types';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { SimpleListDataSource } from './types';
import { useSimpleListDataSource } from './useSimpleListDataSource';
import { ConfirmationDialog } from '@shared/components/dialogs/ConfirmationDialog';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import SearchIcon from '@mui/icons-material/Search';
import { RecordAttachmentsButton, recordTableId } from '@shared/components/documents';

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
  getValue?: (row: T) => unknown;
  fields?: Array<{ field: keyof T & string; label: string }>;
}

export interface EnterpriseListConfig<T> {
  readOnly?: boolean;
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
  attachments?: { refTableId: number; getRefRecId?: (record: T) => number };
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
  backCommand?: { label: string; onClick: () => void };
  showSearchCommand?: boolean;
  recordTableName?: string;
  getAuditRecordId?: (record: T) => string | number;
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

export function SimpleListPage<T extends { id: string } = { id: string }>(
  props: SimpleListPageProps<T>
): React.ReactElement {
  const {
    title,
    subtitle,
    variant = 'standard',
    enterpriseConfig,
    onViewClick,
    dataGridProps = {},
    loading = false,
    error,
    onRetry,
    dialogs,
    gridHeight,
    contentMinHeight = 520,
    containerSx,
    contentSx,
  } = props;
  const { t } = useAppTranslation();
  const gridRef = useRef<DataGridHandle>(null);
  const sourceState = useSimpleListDataSource(props.dataSource);
  const rows = sourceState.rows;
  const columns = props.columns;
  const getRowId = React.useMemo(
    () => dataGridProps.getRowId ?? ((row: T) => row.id),
    [dataGridProps.getRowId]
  );
  const initialRow = enterpriseConfig?.initialSelection === 'none' ? null : (rows[0] ?? null);
  const [selectedIds, setSelectedIds] = useState<(string | number)[]>(
    initialRow ? [getRowId(initialRow)] : []
  );
  const [selectedRow, setSelectedRow] = useState<T | null>(initialRow);
  const initialSelectionApplied = useRef(Boolean(initialRow));
  const [gridFilterVisible, setGridFilterVisible] = useState(
    enterpriseConfig?.showFilterOnLoad ?? false
  );
  const [filterPanelOpen, setFilterPanelOpen] = useState(
    enterpriseConfig?.advancedFilterOpenOnLoad ?? false
  );
  const [informationPanelOpen, setInformationPanelOpen] = useState(
    enterpriseConfig?.informationOpenOnLoad ?? false
  );
  const defaultAdvancedField =
    enterpriseConfig?.advancedFilter?.fields?.[0]?.field ??
    enterpriseConfig?.searchFields[0]?.field ??
    '';
  const [draftAdvancedFilters, setDraftAdvancedFilters] = useState<EnterpriseFilterCondition[]>([
    createEnterpriseFilterCondition(defaultAdvancedField),
  ]);
  const [advancedFilters, setAdvancedFilters] = useState<EnterpriseFilterCondition[]>([]);
  const [isEditing, setIsEditing] = useState(false);
  const [pendingDelete, setPendingDelete] = useState<T[]>([]);
  useUnsavedChanges(isEditing, t('messages.unsavedChanges', 'You have unsaved changes.'));

  useEffect(() => {
    if (
      initialSelectionApplied.current ||
      enterpriseConfig?.initialSelection === 'none' ||
      rows.length === 0
    )
      return;
    const firstRow = rows[0];
    setSelectedRow(firstRow);
    setSelectedIds([getRowId(firstRow)]);
    initialSelectionApplied.current = true;
  }, [enterpriseConfig?.initialSelection, getRowId, rows]);

  const processedRows = useMemo(() => {
    if (!enterpriseConfig) return rows;
    return rows.filter((row) => {
      const matchesAdvanced =
        !enterpriseConfig.advancedFilter ||
        advancedFilters.every((condition) => {
          const configuredField = [
            ...(enterpriseConfig.advancedFilter?.fields ?? []),
            ...enterpriseConfig.searchFields,
          ].find((field) => field.field === condition.field);
          const value = configuredField
            ? row[configuredField.field]
            : enterpriseConfig.advancedFilter?.getValue?.(row);
          return value !== undefined
            ? matchesEnterpriseFilter(value, condition, enterpriseConfig.locale)
            : enterpriseConfig.advancedFilter!.matches(row, condition.value);
        });
      return matchesAdvanced;
    });
  }, [advancedFilters, enterpriseConfig, rows]);

  const resolvedError = error ?? sourceState.error;
  const resolvedLoading = loading || sourceState.loading;
  const retry = onRetry ?? sourceState.refresh;
  const feedback = resolvedError ? (
    <ErrorState message={resolvedError} onRetry={retry} />
  ) : resolvedLoading ? (
    <LoadingState message={t('messages.loadingRecords')} />
  ) : null;

  const standardSelectedRecId = selectedRow ? Number(getRowId(selectedRow)) : null;
  const standardAttachmentRecId =
    standardSelectedRecId != null &&
    Number.isSafeInteger(standardSelectedRecId) &&
    standardSelectedRecId > 0
      ? standardSelectedRecId
      : null;

  if (variant === 'enterprise') {
    const config = enterpriseConfig;
    const reset = () => {
      const firstRow = config?.initialSelection === 'none' ? null : (rows[0] ?? null);
      setAdvancedFilters([]);
      setDraftAdvancedFilters([
        createEnterpriseFilterCondition(
          config?.advancedFilter?.fields?.[0]?.field ?? config?.searchFields[0]?.field
        ),
      ]);
      setGridFilterVisible(config?.showFilterOnLoad ?? false);
      setFilterPanelOpen(config?.advancedFilterOpenOnLoad ?? false);
      setInformationPanelOpen(config?.informationOpenOnLoad ?? false);
      setSelectedRow(firstRow);
      setSelectedIds(firstRow ? [getRowId(firstRow)] : []);
      sourceState.refresh();
      config?.onReset?.();
    };
    const resolvedGridProps: DataGridProps<T> = config
      ? {
          ...dataGridProps,
          rows: processedRows,
          columns,
          selectionMode: dataGridProps.selectionMode ?? 'single',
          selectedIds,
          onSelectionChange: (ids) => {
            if (isEditing) return;
            const normalizedIds = ids as (string | number)[];
            setSelectedIds(normalizedIds);
            setSelectedRow(rows.find((row) => getRowId(row) === normalizedIds.at(-1)) ?? null);
            dataGridProps.onSelectionChange?.(ids);
          },
          onRowClick: (row) => {
            if (isEditing) return;
            setSelectedRow(row);
            dataGridProps.onRowClick?.(row);
          },
          onEditingChange: (editing) => {
            setIsEditing(editing);
            dataGridProps.onEditingChange?.(editing);
          },
          rowHeight: dataGridProps.rowHeight ?? 31,
          headerHeight: dataGridProps.headerHeight ?? 36,
          hideAddRowButton: dataGridProps.hideAddRowButton ?? true,
          hideToolbar: dataGridProps.hideToolbar ?? false,
          hideFilterRow: dataGridProps.hideFilterRow === true || !gridFilterVisible,
          hideFooter: dataGridProps.hideFooter ?? false,
          hideSidebar: dataGridProps.hideSidebar ?? false,
          showCellBorders: dataGridProps.showCellBorders ?? true,
          showColumnBorders: dataGridProps.showColumnBorders ?? true,
          hideInlineEditActions: dataGridProps.hideInlineEditActions ?? true,
        }
      : { ...dataGridProps, rows, columns };
    const generatedActionPane = config && (
      <>
        {config.backCommand && (
          <ActionPaneGroup>
            <ActionPaneButton
              label={config.backCommand.label}
              icon={
                <ArrowBackIcon
                  sx={{ transform: (theme) => (theme.direction === 'rtl' ? 'scaleX(-1)' : 'none') }}
                />
              }
              onClick={config.backCommand.onClick}
            />
          </ActionPaneGroup>
        )}
        {!config.readOnly && (
          <EnterpriseCrudActions
            editLabel={config.crud.editLabel}
            newLabel={config.crud.newLabel}
            deleteLabel={config.crud.deleteLabel}
            canEdit={
              selectedIds.length === 1 && Boolean(config.crud.onEdit || dataGridProps.masterForm)
            }
            canDelete={selectedIds.length > 0 && Boolean(config.crud.onDelete)}
            onEdit={
              selectedRow
                ? () =>
                    config.crud.onEdit
                      ? config.crud.onEdit(selectedRow)
                      : gridRef.current?.startEditRow(getRowId(selectedRow))
                : undefined
            }
            onNew={
              config.crud.onNew
                ? config.crud.onNew
                : dataGridProps.masterForm
                  ? () => gridRef.current?.startAddRow()
                  : undefined
            }
            onDelete={() =>
              setPendingDelete(rows.filter((row) => selectedIds.includes(getRowId(row))))
            }
            editPermission={config.crud.editPermission}
            newPermission={config.crud.newPermission}
            deletePermission={config.crud.deletePermission}
            editing={isEditing}
            saveLabel={t('actions.save')}
            cancelLabel={t('actions.cancel')}
            onSave={() => gridRef.current?.saveEdit()}
            onCancel={() => gridRef.current?.cancelEdit()}
          />
        )}
        {config.commands && (
          <ActionPaneGroup>
            {config.commands.map((command) => (
              <ActionPaneButton
                key={command.id}
                label={command.label}
                permission={command.permission}
                disabled={isEditing || command.disabled}
                onClick={command.onClick}
              />
            ))}
          </ActionPaneGroup>
        )}
        {config.showSearchCommand && (
          <ActionPaneGroup>
            <ActionPaneButton
              label={t('common.search', 'Search')}
              icon={<SearchIcon />}
              disabled={isEditing}
              onClick={() => setGridFilterVisible((visible) => !visible)}
            />
            <OptionsMenu
              record={selectedRow}
              tableName={config.recordTableName ?? title}
              getRecordId={config.getAuditRecordId ?? getRowId}
              title={config.contextLabel}
              disabled={isEditing}
            />
          </ActionPaneGroup>
        )}
      </>
    );
    const selectedAttachmentRecId = selectedRow
      ? (config?.attachments?.getRefRecId?.(selectedRow) ?? Number(getRowId(selectedRow)))
      : null;
    const attachmentRecId =
      selectedAttachmentRecId != null &&
      Number.isSafeInteger(selectedAttachmentRecId) &&
      selectedAttachmentRecId > 0
        ? selectedAttachmentRecId
        : null;
    const generatedUtilities = config && (
      <EnterpriseCommandUtilities
        {...config.utilities}
        attachmentAction={
          <RecordAttachmentsButton
            refTableId={config.attachments?.refTableId ?? recordTableId(title)}
            refRecId={attachmentRecId}
            disabled={isEditing}
          />
        }
        onRefresh={reset}
        disabled={isEditing}
        showPersonalize={false}
        showGuide={false}
        showNotifications={false}
      />
    );
    const generatedSidePanels = config && (
      <>
        {filterPanelOpen && config.advancedFilter && (
          <Box
            sx={{
              position: { xs: 'absolute', lg: 'static' },
              insetInlineEnd: { xs: informationPanelOpen ? 253 : 0 },
              top: 0,
              bottom: 0,
              zIndex: 4,
              height: '100%',
              minHeight: 0,
              display: 'flex',
            }}
          >
            <EnterpriseFilterPanel
              title={config.advancedFilter.title}
              addLabel={config.advancedFilter.addLabel}
              fieldOptions={(config.advancedFilter.fields ?? config.searchFields).map(
                ({ field, label }) => ({ value: field, label })
              )}
              conditions={draftAdvancedFilters}
              operatorOptions={getFilterOperatorOptions(t)}
              applyLabel={config.advancedFilter.applyLabel}
              resetLabel={config.advancedFilter.resetLabel}
              onConditionsChange={setDraftAdvancedFilters}
              onApply={() =>
                setAdvancedFilters(
                  draftAdvancedFilters.filter((condition) => condition.value.trim())
                )
              }
              onReset={() => {
                setDraftAdvancedFilters([
                  createEnterpriseFilterCondition(
                    config.advancedFilter?.fields?.[0]?.field ?? config.searchFields[0]?.field
                  ),
                ]);
                setAdvancedFilters([]);
              }}
            />
          </Box>
        )}
        {informationPanelOpen && config.relatedInformation && (
          <Box
            sx={{
              position: { xs: 'absolute', lg: 'static' },
              insetInlineEnd: 0,
              top: 0,
              bottom: 0,
              zIndex: 4,
              height: '100%',
              minHeight: 0,
              display: 'flex',
            }}
          >
            <RelatedInformationPanel
              title={config.relatedInformation.title}
              sections={config.relatedInformation.sections(selectedRow)}
            />
          </Box>
        )}
      </>
    );
    const generatedUtilityRail = config && (
      <RightUtilityRail
        filterLabel={config.filterLabel}
        informationLabel={config.informationLabel}
        filterActive={config.advancedFilter ? filterPanelOpen : gridFilterVisible}
        informationActive={informationPanelOpen}
        onFilter={() =>
          config.advancedFilter
            ? setFilterPanelOpen((open) => !open)
            : setGridFilterVisible((visible) => !visible)
        }
        onInformation={() => setInformationPanelOpen((open) => !open)}
        showInformation={Boolean(config.relatedInformation)}
        disabled={isEditing}
      />
    );

    return (
      <PageContainer
        sx={[
          {
            gap: 0.5,
            minHeight: { xs: contentMinHeight, lg: 0 },
            height: { xs: 'auto', lg: '100%' },
            maxHeight: { lg: '100%' },
            overflow: { lg: 'hidden' },
            position: 'relative',
            paddingInlineEnd: { lg: `${RIGHT_UTILITY_RAIL_WIDTH}px` },
          },
          ...(Array.isArray(containerSx) ? containerSx : [containerSx]),
        ]}
      >
        {(generatedActionPane ?? props.actionPane) && (
          <ActionPane variant="flat" endActions={generatedUtilities ?? props.actionPaneEndActions}>
            {generatedActionPane ?? props.actionPane}
          </ActionPane>
        )}
        <Box
          sx={[
            {
              display: 'flex',
              flex: 1,
              height: '100%',
              minHeight: { xs: contentMinHeight, lg: 0 },
              gap: 1,
              px: 0,
              pb: 0.5,
              overflow: 'hidden',
              position: 'relative',
              alignItems: 'stretch',
            },
            ...(Array.isArray(contentSx) ? contentSx : [contentSx]),
          ]}
        >
          <Box sx={{ flex: 1, minWidth: 0, display: 'flex', flexDirection: 'column' }}>
            <EnterpriseListHeader
              contextLabel={config?.contextLabel ?? props.contextLabel ?? title}
              viewLabel={config?.viewLabel ?? props.viewLabel ?? title}
              onViewClick={onViewClick}
            />
            {props.filterBar ? (
              <Box
                sx={{ pointerEvents: isEditing ? 'none' : 'auto', opacity: isEditing ? 0.6 : 1 }}
              >
                {props.filterBar}
              </Box>
            ) : null}
            <Box sx={{ flex: 1, minHeight: 0, mx: { xs: 1, sm: 2.5 } }}>
              {feedback ?? (
                <DataGrid
                  ref={gridRef}
                  {...resolvedGridProps}
                  height={gridHeight ?? resolvedGridProps.height ?? '100%'}
                />
              )}
            </Box>
          </Box>
          {generatedSidePanels ?? props.sidePanels}
        </Box>
        {generatedUtilityRail ?? props.utilityRail}
        {dialogs}
        <ConfirmationDialog
          open={pendingDelete.length > 0}
          onClose={() => setPendingDelete([])}
          onConfirm={() => {
            config?.crud.onDelete?.(pendingDelete);
            setPendingDelete([]);
          }}
          severity="error"
          title={t('dialogs.confirmDeleteTitle')}
          message={t('dialogs.confirmDeleteMessage', { count: pendingDelete.length })}
          confirmLabel={t('actions.delete')}
          cancelLabel={t('actions.cancel')}
        />
      </PageContainer>
    );
  }

  return (
    <PageContainer sx={containerSx}>
      <PageHeader title={title} subtitle={subtitle} />
      <ActionPane
        endActions={
          <>
            {props.actionPaneEndActions}
            <RecordAttachmentsButton
              refTableId={recordTableId(title)}
              refRecId={standardAttachmentRecId}
            />
          </>
        }
      >
        {props.actionPane}
      </ActionPane>
      {feedback ?? (
        <Box sx={{ width: '100%', height: gridHeight ?? 600 }}>
          <DataGrid {...dataGridProps} rows={rows} columns={columns} />
        </Box>
      )}
      {dialogs}
    </PageContainer>
  );
}

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
