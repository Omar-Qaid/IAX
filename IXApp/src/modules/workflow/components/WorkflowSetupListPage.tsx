import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { queryClient } from '@core/api/queryClient';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef, FetchRowsParams } from '@shared/components/data-grid/types';
import { useNotifications } from '@shared/hooks/useNotifications';
import { uiDensity } from '@shared/constants/uiDensity';
import type { WorkflowMasterDto, WorkflowMasterRecord } from '../api/workflowMasterApi';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type { NumberSequenceMetadata } from '@patterns/list-details/useListDetailsPage';
import { localizedName } from '@shared/utilities/localizedName';

interface WorkflowSetupApi<TDto extends WorkflowMasterDto> {
  list(signal?: AbortSignal): Promise<WorkflowMasterRecord<TDto>[]>;
  listPage?(
    params: FetchRowsParams
  ): Promise<{ rows: WorkflowMasterRecord<TDto>[]; totalCount: number }>;
  create(record: WorkflowMasterRecord<TDto>): Promise<WorkflowMasterRecord<TDto>>;
  update(record: WorkflowMasterRecord<TDto>): Promise<WorkflowMasterRecord<TDto>>;
  delete(record: WorkflowMasterRecord<TDto>): Promise<void>;
}

const defaultPageRequest = (): Omit<FetchRowsParams, 'signal'> => ({
  sort: [],
  filters: [],
  globalSearch: '',
  page: 0,
  pageSize: 50,
  isFirstPage: true,
});

function useWorkflowSetupPaging<TDto extends WorkflowMasterDto>(
  loadPage: WorkflowSetupApi<TDto>['listPage']
) {
  const [rows, setRows] = useState<WorkflowMasterRecord<TDto>[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(Boolean(loadPage));
  const [error, setError] = useState<string | null>(null);
  const requestIdRef = useRef(0);
  const latestRequestRef = useRef(defaultPageRequest());

  const fetchRows = useCallback(
    async (params: FetchRowsParams) => {
      if (!loadPage) return;
      if (params.isFirstPage) {
        latestRequestRef.current = {
          sort: params.sort,
          filters: params.filters,
          globalSearch: params.globalSearch,
          page: 0,
          pageSize: params.pageSize,
          isFirstPage: true,
          columns: params.columns,
        };
      }
      const requestId = ++requestIdRef.current;
      setLoading(true);
      setError(null);
      try {
        const result = await loadPage(params);
        if (requestId !== requestIdRef.current || params.signal.aborted) return;
        setTotalCount(result.totalCount);
        setRows((current) => {
          if (params.isFirstPage) return result.rows;
          const merged = new Map(current.map((row) => [row.id, row]));
          result.rows.forEach((row) => merged.set(row.id, row));
          return [...merged.values()];
        });
      } catch (loadError) {
        if (params.signal.aborted || requestId !== requestIdRef.current) return;
        setError(loadError instanceof Error ? loadError.message : String(loadError));
      } finally {
        if (requestId === requestIdRef.current) setLoading(false);
      }
    },
    [loadPage]
  );

  const refresh = useCallback(() => {
    if (!loadPage) return Promise.resolve();
    const controller = new AbortController();
    return fetchRows({ ...latestRequestRef.current, signal: controller.signal });
  }, [fetchRows, loadPage]);

  useEffect(() => {
    if (!loadPage) return;
    const controller = new AbortController();
    void fetchRows({ ...defaultPageRequest(), signal: controller.signal });
    return () => controller.abort();
  }, [fetchRows, loadPage]);

  return { rows, totalCount, loading, error, fetchRows, refresh };
}

export interface WorkflowSetupField<TDto extends WorkflowMasterDto> {
  field: keyof WorkflowMasterRecord<TDto> & string;
  labelKey: string;
  width?: number;
  required?: boolean;
  editable?: boolean;
  type?: ColumnDef<WorkflowMasterRecord<TDto>>['type'];
}

interface WorkflowSetupListPageProps<TDto extends WorkflowMasterDto> {
  titleKey: string;
  resourceKey: string;
  api: WorkflowSetupApi<TDto>;
  createRecord: () => WorkflowMasterRecord<TDto>;
  numberSequenceKey: string;
  requiredCoreFields?: Array<'code' | 'name'>;
  permissions?: { create: string; edit: string; delete: string };
  extraFields?: WorkflowSetupField<TDto>[];
}

export function WorkflowSetupListPage<TDto extends WorkflowMasterDto>({
  titleKey,
  resourceKey,
  api,
  createRecord,
  numberSequenceKey,
  requiredCoreFields = [],
  permissions,
  extraFields = [],
}: WorkflowSetupListPageProps<TDto>): React.ReactElement {
  const { t, currentLanguage, isRtl } = useAppTranslation();
  const { notifyError, notifySuccess } = useNotifications();
  const navigate = useNavigate();
  const queryKey = useMemo(() => ['simple-list', resourceKey] as const, [resourceKey]);
  const paging = useWorkflowSetupPaging(api.listPage);
  const sequenceQuery = useQuery({
    queryKey: ['number-sequence', numberSequenceKey],
    queryFn: async ({ signal }) => {
      const response = await apiClient.get<ApiResponse<NumberSequenceMetadata>>(
        `/v1/${numberSequenceKey}/number-sequence`,
        { signal }
      );
      if (!response.data.success || !response.data.data)
        throw new Error(response.data.message || 'Number sequence is unavailable.');
      return response.data.data;
    },
    staleTime: 0,
  });
  const sequence = sequenceQuery.data;
  const columns = useMemo<ColumnDef<WorkflowMasterRecord<TDto>>[]>(
    () => [
      {
        field: 'code',
        headerName: 'workflowSetup.fields.code',
        width: 150,
        pinned: 'left',
        editable: sequence?.manual ?? false,
      },
      ...(isRtl
        ? [
            {
              field: 'nameAlias' as const,
              headerName: 'workflowSetup.fields.nameAlias',
              minWidth: 220,
              flex: 1,
              editable: true,
            },
            {
              field: 'name' as const,
              headerName: 'workflowSetup.fields.name',
              minWidth: 220,
              flex: 1,
              editable: true,
            },
          ]
        : [
            {
              field: 'name' as const,
              headerName: 'workflowSetup.fields.name',
              minWidth: 220,
              flex: 1,
              editable: true,
            },
            {
              field: 'nameAlias' as const,
              headerName: 'workflowSetup.fields.nameAlias',
              minWidth: 220,
              flex: 1,
              editable: true,
            },
          ]),
      ...extraFields.map<ColumnDef<WorkflowMasterRecord<TDto>>>((field) => ({
        field: field.field,
        headerName: field.labelKey,
        width: field.width ?? 150,
        editable: field.editable ?? true,
        type: field.type,
      })),
      {
        field: 'sortOrder',
        headerName: 'workflowSetup.fields.sortOrder',
        width: 110,
        type: 'number',
        editable: true,
      },
    ],
    [extraFields, isRtl, sequence?.manual]
  );
  const refresh = async () => {
    if (api.listPage) await paging.refresh();
    else await queryClient.invalidateQueries({ queryKey });
  };
  const config: EnterpriseListConfig<WorkflowMasterRecord<TDto>> = {
    contextLabel: t(titleKey),
    viewLabel: t('common.standardView'),
    filterLabel: t('actions.filter'),
    informationLabel: t('common.information'),
    searchMode: 'quick',
    locale: currentLanguage.code,
    searchFields: [
      { field: 'code', label: t('workflowSetup.fields.code') },
      { field: 'name', label: t('workflowSetup.fields.name') },
      { field: 'nameAlias', label: t('workflowSetup.fields.nameAlias') },
    ],
    backCommand: { label: t('actions.back'), onClick: () => navigate(-1) },
    showSearchCommand: true,
    recordTableName: numberSequenceKey,
    getAuditRecordId: (record) => record.recId,
    crud: {
      editLabel: t('actions.edit'),
      newLabel: t('actions.new'),
      deleteLabel: t('actions.delete'),
      editPermission: permissions?.edit,
      newPermission: permissions?.create,
      deletePermission: permissions?.delete,
      onDelete: async (rows) => {
        try {
          await Promise.all(rows.map((row) => api.delete(row)));
          await refresh();
          notifySuccess(t('messages.deletedSuccessfully'));
        } catch (error) {
          notifyError(
            error instanceof Error ? error.message : t('errors.deleteFailed')
          );
        }
      },
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
      fieldLabel: t('workflowSetup.fields.name'),
      operatorLabel: t('filters.contains'),
      applyLabel: t('actions.apply'),
      resetLabel: t('actions.reset'),
      getValue: (record) => localizedName(record, isRtl),
      matches: (record, value) =>
        localizedName(record, isRtl)
          .toLocaleLowerCase(currentLanguage.code)
          .includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
  };

  return (
    <SimpleListPage
      variant="enterprise"
      title={t(titleKey)}
      enterpriseConfig={config}
      dataSource={
        api.listPage
          ? {
              type: 'controlled',
              rows: paging.rows,
              loading: paging.loading && paging.rows.length === 0,
              error: paging.error,
              refresh: paging.refresh,
            }
          : { type: 'remote', key: resourceKey, load: (signal) => api.list(signal) }
      }
      columns={columns}
      dataGridProps={{
        serverSide: Boolean(api.listPage),
        pageSize: 50,
        totalRowCount: api.listPage ? paging.totalCount : undefined,
        hasMore: api.listPage ? paging.rows.length < paging.totalCount : undefined,
        loading: api.listPage ? paging.loading : undefined,
        onFetchRows: api.listPage ? paging.fetchRows : undefined,
        storageKey: `workflow.${resourceKey}.reference-view`,
        masterForm: true,
        hideSidebar: false,
        rowHeight: uiDensity.gridRowHeight,
        headerHeight: uiDensity.gridRowHeight,
        onNewRow: () => ({
          ...createRecord(),
          code: sequence?.manual ? '' : (sequence?.previewCode ?? null),
        }),
        onRowSave: async (values, isNew) => {
          const record = values as WorkflowMasterRecord<TDto>;
          if (isNew && !sequence?.available)
            throw new Error(sequence?.message || 'Number sequence is unavailable.');
          if (isNew && sequence?.manual && !String(record.code ?? '').trim())
            throw new Error(t('validation.required', { field: t('workflowSetup.fields.code') }));
          const missingCoreField = requiredCoreFields.find(
            (field) => !String(record[field] ?? '').trim()
          );
          if (missingCoreField)
            throw new Error(
              t('validation.required', { field: t(`workflowSetup.fields.${missingCoreField}`) })
            );
          const missingExtra = extraFields.find(
            (field) => field.required && !String(record[field.field] ?? '').trim()
          );
          if (missingExtra)
            throw new Error(t('validation.required', { field: t(missingExtra.labelKey) }));
          if (isNew || record.recId === 0) {
            await api.create(sequence?.manual ? record : { ...record, code: null });
            await sequenceQuery.refetch();
          } else {
            await api.update(record);
          }
          await refresh();
          notifySuccess(t('messages.savedSuccessfully'));
        },
      }}
    />
  );
}
