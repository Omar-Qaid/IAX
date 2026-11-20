import React, { useEffect, useMemo, useRef, useState } from 'react';
import { Box, type SxProps, type Theme } from '@mui/material';
import { PageContainer } from '@shared/components/page/PageContainer';
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
import { useLocation, useNavigate } from 'react-router-dom';
import { useGridFilterRow } from '@shared/components/data-grid/hooks/useGridFilterRow';
import { uiDensity } from '@shared/constants/uiDensity';
import { ActionPaneBackButton } from '@shared/components/action-pane/ActionPaneBackButton';
import type { SimpleListDataSource } from './types';
import { useSimpleListDataSource } from './useSimpleListDataSource';
import { ConfirmationDialog } from '@shared/components/dialogs/ConfirmationDialog';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
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
  /** Defaults: Edit / New / Delete. */
  editLabel?: string;
  newLabel?: string;
  deleteLabel?: string;
  editPermission?: string;
  newPermission?: string;
  deletePermission?: string;
  onEdit?: (row: T) => void;
  onNew?: () => void;
  onDelete?: (rows: T[]) => void;
  canDelete?: (rows: T[]) => boolean;
  canEdit?: (row: T) => boolean;
}

export interface EnterpriseListAdvancedFilter<T> {
  /** Labels default to the shared filter pane texts. */
  title?: string;
  addLabel?: string;
  fieldLabel?: string;
  operatorLabel?: string;
  applyLabel?: string;
  resetLabel?: string;
  /** Fallback match for conditions on a field the row does not have (default: `getValue` contains). */
  matches?: (row: T, value: string) => boolean;
  getValue?: (row: T) => unknown;
  fields?: Array<{ field: keyof T & string; label: string }>;
}

/**
 * Page configuration of an enterprise list. Only what is specific to the page
 * is required; labels, utilities, back command, filter texts and locale come
 * from the shared defaults (the reference design: sales order list). Any
 * default can still be overridden per page without touching the pattern.
 */
export interface EnterpriseListConfig<T> {
  readOnly?: boolean;
  /** Breadcrumb above the grid (default: the page title). */
  contextLabel?: string;
  /** Main title for the selected record; the page label stays in the blue caption. */
  getRecordTitle?: (record: T) => string;
  viewLabel?: string;
  filterLabel?: string;
  informationLabel?: string;
  searchByLabel?: string;
  searchMode?: 'quick' | 'field';
  /** Default: every column. */
  searchFields?: EnterpriseListSearchField<T>[];
  defaultSearchField?: keyof T & string;
  /** Default: the current language. */
  locale?: string;
  crud?: EnterpriseListCrudConfig<T>;
  commands?: EnterpriseListCommand[];
  getCommands?: (selectedRow: T | null) => EnterpriseListCommand[];
  attachments?: { refTableId: number; getRefRecId?: (record: T) => number };
  utilities?: Partial<ResolvedListUtilities>;
  advancedFilter?: EnterpriseListAdvancedFilter<T>;
  relatedInformation?: {
    /** Default: "Related information". */
    title?: string;
    sections: (selectedRow: T | null) => RelatedInformationSection[];
  };
  initialSelection?: 'first' | 'none';
  showFilterOnLoad?: boolean;
  advancedFilterOpenOnLoad?: boolean;
  informationOpenOnLoad?: boolean;
  onReset?: () => void;
  /** Default: "Back" to the previous page. */
  backCommand?: { label?: string; onClick?: () => void };
  showSearchCommand?: boolean;
  recordTableName?: string;
  getAuditRecordId?: (record: T) => string | number;
}

interface ResolvedListUtilities {
  personalizeLabel: string;
  guideLabel: string;
  notificationsLabel: string;
  refreshLabel: string;
  openWindowLabel: string;
  notificationCount?: number;
}

/** The configuration after defaults are applied (what the page renders from). */
type ResolvedEnterpriseListConfig<T> = Omit<
  EnterpriseListConfig<T>,
  | 'contextLabel'
  | 'viewLabel'
  | 'filterLabel'
  | 'informationLabel'
  | 'searchFields'
  | 'crud'
  | 'utilities'
  | 'advancedFilter'
  | 'relatedInformation'
> & {
  contextLabel: string;
  viewLabel: string;
  filterLabel: string;
  informationLabel: string;
  searchFields: EnterpriseListSearchField<T>[];
  crud: EnterpriseListCrudConfig<T> & { editLabel: string; newLabel: string; deleteLabel: string };
  utilities: ResolvedListUtilities;
  advancedFilter?: Required<Omit<EnterpriseListAdvancedFilter<T>, 'getValue' | 'fields'>> &
    Pick<EnterpriseListAdvancedFilter<T>, 'getValue' | 'fields'>;
  relatedInformation?: {
    title: string;
    sections: (selectedRow: T | null) => RelatedInformationSection[];
  };
};

export interface SimpleListPageProps<T extends { id: string } = { id: string }> {
  title: string;
  subtitle?: string;
  recordTableName?: string;
  enterpriseConfig?: EnterpriseListConfig<T>;
  /**
   * Overrides for the labels the pattern derives from `enterpriseConfig` and
   * `title`, for the few pages whose header text differs from both.
   */
  contextLabel?: string;
  viewLabel?: string;
  onViewClick?: () => void;
  /** Extra action-pane commands rendered before the generated CRUD commands. */
  actionPane?: React.ReactNode;
  /** A page-specific filter row rendered between the list header and the grid. */
  filterBar?: React.ReactNode;
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
  /** Optional selected-record view replacing the grid, for read-only detail tabs. */
  detailContent?: (selectedRow: T | null) => React.ReactNode;
  /** Document/list-specific content rendered below the grid for the active row. */
  belowGridContent?: React.ReactNode | ((selectedRow: T | null) => React.ReactNode);
}

export function SimpleListPage<T extends { id: string } = { id: string }>(
  props: SimpleListPageProps<T>
): React.ReactElement {
  const {
    title,
    subtitle,
    enterpriseConfig: suppliedConfig,
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
  const { t, currentLanguage } = useAppTranslation();
  const enterpriseConfig = useMemo<ResolvedEnterpriseListConfig<T> | undefined>(() => {
    // Columns carry translation keys; the filter rail renders labels verbatim, so
    // resolve them here. A plain label translates to itself.
    const searchFields = props.columns.map((column) => ({
      field: column.field as keyof T & string,
      label: column.headerName ? t(column.headerName) : '',
    }));
    const defaults: ResolvedEnterpriseListConfig<T> = {
      readOnly: true,
      recordTableName: props.recordTableName,
      contextLabel: props.contextLabel ?? title,
      viewLabel: props.viewLabel ?? t('common.standardView'),
      filterLabel: t('actions.filter'),
      informationLabel: t('common.information'),
      searchFields,
      locale: currentLanguage.code,
      crud: {
        editLabel: t('actions.edit'),
        newLabel: t('actions.new'),
        deleteLabel: t('actions.delete'),
      },
      utilities: {
        personalizeLabel: t('utilities.personalize'),
        guideLabel: t('utilities.guide'),
        notificationsLabel: t('common.notifications'),
        refreshLabel: t('actions.refresh'),
        openWindowLabel: t('utilities.openWindow'),
        notificationCount: 0,
      },
      advancedFilter: {
        title: t('filters.title'),
        addLabel: t('actions.add'),
        fieldLabel: t('filters.field'),
        operatorLabel: t('filters.operator'),
        applyLabel: t('actions.apply'),
        resetLabel: t('actions.reset'),
        matches: () => true,
      },
    };
    if (!suppliedConfig) return { ...defaults, readOnly: true };
    // Deep merge: a page sets only what differs from the shared design.
    const filter = suppliedConfig.advancedFilter;
    const locale = suppliedConfig.locale ?? defaults.locale;
    return {
      ...defaults,
      ...suppliedConfig,
      readOnly: suppliedConfig.readOnly ?? false,
      contextLabel: suppliedConfig.contextLabel ?? defaults.contextLabel,
      viewLabel: suppliedConfig.viewLabel ?? defaults.viewLabel,
      filterLabel: suppliedConfig.filterLabel ?? defaults.filterLabel,
      informationLabel: suppliedConfig.informationLabel ?? defaults.informationLabel,
      searchFields: suppliedConfig.searchFields ?? defaults.searchFields,
      locale,
      crud: { ...defaults.crud, ...stripUndefined(suppliedConfig.crud ?? {}) },
      utilities: { ...defaults.utilities, ...stripUndefined(suppliedConfig.utilities ?? {}) },
      advancedFilter: filter
        ? {
            ...defaults.advancedFilter!,
            ...stripUndefined(filter),
            matches:
              filter.matches ??
              ((row: T, value: string) =>
                String(filter.getValue?.(row) ?? '')
                  .toLocaleLowerCase(locale)
                  .includes(value.trim().toLocaleLowerCase(locale))),
          }
        : defaults.advancedFilter,
      relatedInformation: suppliedConfig.relatedInformation
        ? {
            title: suppliedConfig.relatedInformation.title ?? t('relatedInformation.title'),
            sections: suppliedConfig.relatedInformation.sections,
          }
        : undefined,
      backCommand: suppliedConfig.backCommand,
    };
  }, [
    suppliedConfig,
    props.columns,
    props.recordTableName,
    props.contextLabel,
    props.viewLabel,
    title,
    t,
    currentLanguage.code,
  ]);
  const navigate = useNavigate();
  const { pathname } = useLocation();
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
  const [selectedRowSnapshot, setSelectedRow] = useState<T | null>(initialRow);
  const initialSelectionApplied = useRef(Boolean(initialRow));
  useEffect(() => {
    if (dataGridProps.selectedIds) {
      setSelectedRow(
        rows.find((row) => getRowId(row) === dataGridProps.selectedIds?.at(-1)) ?? null
      );
    }
  }, [dataGridProps.selectedIds, rows, getRowId]);
  const [gridFilterVisible, setGridFilterVisible] = useGridFilterRow(
    dataGridProps.storageKey ?? pathname,
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
          const column = !suppliedConfig?.advancedFilter
            ? columns.find((item) => item.field === condition.field)
            : undefined;
          const value = column?.valueGetter
            ? column.valueGetter({ row })
            : configuredField
              ? row[configuredField.field]
              : enterpriseConfig.advancedFilter?.getValue?.(row);
          return value !== undefined
            ? matchesEnterpriseFilter(value, condition, enterpriseConfig.locale)
            : enterpriseConfig.advancedFilter!.matches(row, condition.value);
        });
      return matchesAdvanced;
    });
  }, [advancedFilters, enterpriseConfig, rows, suppliedConfig, columns]);

  // Commands and record information must use the current visible record, not a stale
  // snapshot retained before filtering, refreshing, posting or deletion.
  const selectedRow =
    selectedRowSnapshot && selectedIds.includes(getRowId(selectedRowSnapshot))
      ? (processedRows.find((row) => getRowId(row) === getRowId(selectedRowSnapshot)) ?? null)
      : null;

  const resolvedError = error ?? sourceState.error;
  const resolvedLoading = loading || sourceState.loading;
  const retry = onRetry ?? sourceState.refresh;
  const feedback = resolvedError ? (
    <ErrorState message={resolvedError} onRetry={retry} />
  ) : resolvedLoading ? (
    <LoadingState message={t('messages.loadingRecords')} />
  ) : null;

  {
    const config = enterpriseConfig;
    const reset = () => {
      const firstRow = config?.initialSelection === 'none' ? null : (rows[0] ?? null);
      setAdvancedFilters([]);
      setDraftAdvancedFilters([
        createEnterpriseFilterCondition(
          config?.advancedFilter?.fields?.[0]?.field ?? config?.searchFields[0]?.field
        ),
      ]);
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
          selectedIds: dataGridProps.selectedIds ?? selectedIds,
          onSelectionChange: (ids) => {
            if (isEditing) return;
            const normalizedIds = ids as (string | number)[];
            setSelectedIds(normalizedIds);
            setSelectedRow(rows.find((row) => getRowId(row) === normalizedIds.at(-1)) ?? null);
            dataGridProps.onSelectionChange?.(ids);
          },
          onRowClick: (row) => {
            if (isEditing) return;
            if ((dataGridProps.selectionMode ?? 'single') === 'single') {
              const ids = [getRowId(row)];
              setSelectedIds(ids);
              dataGridProps.onSelectionChange?.(ids);
            }
            setSelectedRow(row);
            dataGridProps.onRowClick?.(row);
          },
          onEditingChange: (editing) => {
            setIsEditing(editing);
            dataGridProps.onEditingChange?.(editing);
          },
          rowHeight: dataGridProps.rowHeight ?? 31,
          headerHeight: dataGridProps.headerHeight ?? uiDensity.listGridHeaderHeight,
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
        <ActionPaneBackButton
          label={config.backCommand?.label ?? t('actions.back')}
          onClick={config.backCommand?.onClick ?? (() => navigate(-1))}
          disabled={isEditing}
        />
        {props.actionPane}
        {!config.readOnly && (
          <EnterpriseCrudActions
            editLabel={config.crud.editLabel}
            newLabel={config.crud.newLabel}
            deleteLabel={config.crud.deleteLabel}
            canEdit={
              selectedIds.length === 1 &&
              Boolean(selectedRow) &&
              Boolean(config.crud.onEdit || dataGridProps.masterForm) &&
              (selectedRow ? (config.crud.canEdit?.(selectedRow) ?? true) : false)
            }
            canNew={Boolean(config.crud.onNew || dataGridProps.masterForm)}
            canDelete={
              selectedIds.some((id) => processedRows.some((row) => getRowId(row) === id)) &&
              Boolean(config.crud.onDelete) &&
              (config.crud.canDelete?.(
                processedRows.filter((row) => selectedIds.includes(getRowId(row)))
              ) ??
                true)
            }
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
              setPendingDelete(processedRows.filter((row) => selectedIds.includes(getRowId(row))))
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
        {(config.commands || config.getCommands) && (
          <ActionPaneGroup>
            {(config.getCommands?.(selectedRow) ?? config.commands ?? []).map((command) => (
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
        {config.showSearchCommand !== false && (
          <ActionPaneGroup>
            <ActionPaneButton
              label={t('common.search', 'Search')}
              icon={<SearchIcon />}
              disabled={isEditing || dataGridProps.hideFilterRow === true}
              selected={gridFilterVisible && dataGridProps.hideFilterRow !== true}
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
            refTableId={
              config.attachments?.refTableId ?? recordTableId(config.recordTableName ?? title)
            }
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
          <ActionPane variant="flat" endActions={generatedUtilities}>
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
              pageTitle={title}
              contextLabel={
                selectedRow && config?.getRecordTitle
                  ? config.getRecordTitle(selectedRow)
                  : (config?.contextLabel ?? props.contextLabel ?? title)
              }
              viewLabel={config?.viewLabel ?? props.viewLabel ?? title}
              onViewClick={onViewClick}
            />
            {subtitle && <Box sx={{ mx: { xs: 1, sm: 2.5 }, mb: 1 }}>{subtitle}</Box>}
            {props.filterBar ? (
              <Box
                sx={{ pointerEvents: isEditing ? 'none' : 'auto', opacity: isEditing ? 0.6 : 1 }}
              >
                {props.filterBar}
              </Box>
            ) : null}
            <Box sx={{ flex: 1, minHeight: 0, mx: { xs: 1, sm: 2.5 } }}>
              {feedback ??
                (props.detailContent ? (
                  props.detailContent(selectedRow)
                ) : (
                  <DataGrid
                    ref={gridRef}
                    {...resolvedGridProps}
                    height={gridHeight ?? resolvedGridProps.height ?? '100%'}
                  />
                ))}
            </Box>
            {props.belowGridContent && (
              <Box sx={{ mx: { xs: 1, sm: 2.5 } }}>
                {typeof props.belowGridContent === 'function'
                  ? props.belowGridContent(selectedRow)
                  : props.belowGridContent}
              </Box>
            )}
          </Box>
          {generatedSidePanels}
        </Box>
        {generatedUtilityRail}
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

/** Copy of an object without its undefined values (so they do not override defaults). */
function stripUndefined<V extends object>(value: V): Partial<V> {
  return Object.fromEntries(
    Object.entries(value).filter(([, entry]) => entry !== undefined)
  ) as Partial<V>;
}
