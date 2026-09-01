import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { WORKFLOW_ROUTE_PATHS } from '../routes/workflowRoutePaths';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { AppLookupGridField } from '@shared/components/fields/AppLookupGridField';
import { AppLookupField } from '@shared/components/fields/AppLookupField';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { wfProcessApi, type WfProcessRecord } from '../api/wfProcessApi';
import { wfCategoryApi, type WfCategoryRecord } from '../api/wfCategoryApi';
import { wfPriorityApi, wfProcessTypeApi } from '../api/workflowSetupApis';

const categoryLookupColumns = [
  { field: 'code', header: 'wfCategory.fields.code', width: 110 },
  { field: 'name', header: 'wfCategory.fields.name', flex: 1 },
] as const;

const fetchCategoryPage = async ({
  pageNumber,
  pageSize,
  search,
  signal,
}: {
  pageNumber: number;
  pageSize: number;
  search: string;
  signal?: AbortSignal;
}) => {
  const categories = await wfCategoryApi.list(signal);
  const normalizedSearch = search.trim().toLocaleLowerCase();
  const filtered = normalizedSearch
    ? categories.filter((category) =>
        `${category.code ?? ''} ${category.name ?? ''}`
          .toLocaleLowerCase()
          .includes(normalizedSearch)
      )
    : categories;
  const start = (pageNumber - 1) * pageSize;
  return {
    data: filtered.slice(start, start + pageSize),
    pageNumber,
    totalPages: Math.max(1, Math.ceil(filtered.length / pageSize)),
    totalRecords: filtered.length,
  };
};

const emptyProcess = (): WfProcessRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  code: null,
  name: '',
  nameAlias: null,
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

const numberValue = (value: DetailValue): number => Number(value) || 0;
const textValue = (value: string | null | undefined): string => value ?? '';

export function WFProcessPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const navigate = useNavigate();
  const prioritiesQuery = useQuery({
    queryKey: ['workflow', 'priority-lookup'],
    queryFn: ({ signal }) => wfPriorityApi.list(signal),
  });
  const processTypesQuery = useQuery({
    queryKey: ['workflow', 'process-type-lookup'],
    queryFn: ({ signal }) => wfProcessTypeApi.list(signal),
  });
  const priorityOptions = useMemo(
    () =>
      (prioritiesQuery.data ?? []).map((priority) => ({
        id: priority.recId,
        code: priority.code ?? '',
        name: priority.name ?? '',
        description: priority.description ?? undefined,
      })),
    [prioritiesQuery.data]
  );
  const processTypeOptions = useMemo(
    () =>
      (processTypesQuery.data ?? []).map((processType) => ({
        id: processType.recId,
        code: processType.code ?? '',
        name: processType.name ?? '',
        description: processType.description ?? undefined,
      })),
    [processTypesQuery.data]
  );
  const sections = useMemo<DetailSectionConfig[]>(
    () => [
      {
        id: 'configuration',
        title: t('wfProcess.sections.configuration'),
        groups: [
          {
            id: 'classification',
            title: t('wfProcess.groups.classification'),
            fields: [
              {
                name: 'categoryId',
                label: t('wfProcess.fields.category'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <AppLookupGridField<WfCategoryRecord>
                    name="categoryId"
                    label={t('wfProcess.fields.category')}
                    value={numberValue(value ?? 0)}
                    onChange={(categoryId) => onChange(Number(categoryId) || 0)}
                    disabled={disabled}
                    required
                    columns={[...categoryLookupColumns]}
                    queryKey={['workflow', 'category-lookup']}
                    fetchPage={fetchCategoryPage}
                    fetchById={async (categoryId) =>
                      wfCategoryApi.getById(Number(categoryId)).catch(() => null)
                    }
                    valueField="recId"
                    labelField="name"
                    pageSize={25}
                  />
                ),
              },
              {
                name: 'priorityId',
                label: t('wfProcess.fields.priority'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <AppLookupField
                    name="priorityId"
                    label={t('wfProcess.fields.priority')}
                    value={numberValue(value ?? 0)}
                    onChange={(priorityId) => onChange(Number(priorityId) || 0)}
                    options={priorityOptions}
                    required
                    disabled={disabled || prioritiesQuery.isLoading}
                  />
                ),
              },
              {
                name: 'processTypeId',
                label: t('wfProcess.fields.processType'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <AppLookupField
                    name="processTypeId"
                    label={t('wfProcess.fields.processType')}
                    value={numberValue(value ?? 0)}
                    onChange={(processTypeId) => onChange(Number(processTypeId) || 0)}
                    options={processTypeOptions}
                    required
                    disabled={disabled || processTypesQuery.isLoading}
                    displayMode="select"
                  />
                ),
              },
            ],
          },
          {
            id: 'behavior',
            title: t('wfProcess.groups.behavior'),
            fields: [
              { name: 'score', label: t('wfProcess.fields.score'), type: 'number' },
              { name: 'sortOrder', label: t('wfProcess.fields.sortOrder'), type: 'number' },
              { name: 'canRepeat', label: t('wfProcess.fields.canRepeat'), type: 'boolean' },
              {
                name: 'mandatoryDocs',
                label: t('wfProcess.fields.mandatoryDocs'),
                type: 'boolean',
              },
              {
                name: 'sysField',
                label: t('wfProcess.fields.systemProcess'),
                type: 'boolean',
                disabled: true,
              },
            ],
          },
        ],
      },
    ],
    [
      prioritiesQuery.isLoading,
      priorityOptions,
      processTypeOptions,
      processTypesQuery.isLoading,
      t,
    ]
  );

  const config: EnterpriseListDetailsConfig<WfProcessRecord> = {
    recordTableName: 'WfProcess',
    dataSource: {
      type: 'remote',
      key: 'workflow-processes',
      load: (signal) => wfProcessApi.list(signal),
      create: wfProcessApi.create,
      update: wfProcessApi.update,
      delete: wfProcessApi.delete,
    },
    createRecord: emptyProcess,
    numberSequence: { key: 'WfProcess', field: 'code' },
    getPrimaryText: (record) => textValue(record.name),
    getSecondaryText: (record) => record.code ?? '',
    matchesSearch: (record, query) =>
      `${record.code ?? ''} ${record.name ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      categoryId: record.categoryId,
      priorityId: record.priorityId,
      processTypeId: record.processTypeId,
      score: record.score,
      sortOrder: record.sortOrder,
      canRepeat: record.canRepeat,
      mandatoryDocs: record.mandatoryDocs,
      sysField: record.sysField,
    }),
    setValues: (record, values) => ({
      ...record,
      categoryId: numberValue(values.categoryId),
      priorityId: numberValue(values.priorityId),
      processTypeId: numberValue(values.processTypeId),
      score: numberValue(values.score),
      sortOrder: numberValue(values.sortOrder),
      canRepeat: Boolean(values.canRepeat),
      mandatoryDocs: Boolean(values.mandatoryDocs),
      sysField: Boolean(values.sysField),
    }),
    headerFields: [
      {
        id: 'code',
        label: t('wfProcess.fields.code'),
        width: 180,
        disabled: true,
        getValue: (record) => textValue(record.code),
        setValue: (record, value) => ({ ...record, code: String(value) || null }),
      },
      {
        id: 'name',
        label: t('wfProcess.fields.name'),
        width: 'minmax(320px, 520px)',
        getValue: (record) => textValue(record.name),
        setValue: (record, value) => ({ ...record, name: String(value) }),
      },
      {
        id: 'nameAlias',
        label: t('workflowSetup.fields.nameAlias'),
        width: 'minmax(320px, 520px)',
        getValue: (record) => textValue(record.nameAlias),
        setValue: (record, value) => ({ ...record, nameAlias: String(value) || null }),
      },
    ],
    sections,
    permissions: {
      view: 'Workflow.Processes.View',
      create: 'Workflow.Processes.Create',
      edit: 'Workflow.Processes.Edit',
      delete: 'Workflow.Processes.Delete',
    },
    validate: (record) => ({
      ...(!record.name?.trim()
        ? { name: t('validation.required', { field: t('wfProcess.fields.name') }) }
        : {}),
      ...(record.categoryId <= 0
        ? { categoryId: t('validation.required', { field: t('wfProcess.fields.category') }) }
        : {}),
      ...(record.priorityId <= 0
        ? { priorityId: t('validation.required', { field: t('wfProcess.fields.priority') }) }
        : {}),
      ...(record.processTypeId <= 0
        ? {
            processTypeId: t('validation.required', {
              field: t('wfProcess.fields.processType'),
            }),
          }
        : {}),
    }),
    advancedFilter: {
      fieldLabel: t('wfProcess.fields.name'),
      getValue: (record) => record.name,
      matches: (record, value) =>
        textValue(record.name).toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()),
    },
    commands: [
      {
        id: 'builder',
        label: t('wfProcess.commands.processBuilder'),
        requiresSelection: true,
        onClick: (process) => {
          if (process) navigate(WORKFLOW_ROUTE_PATHS.processBuilder(process.recId));
        },
      },
      {
        id: 'variables',
        label: t('wfProcess.commands.variables'),
        requiresSelection: true,
        onClick: (process) => {
          if (process) navigate(`${WORKFLOW_ROUTE_PATHS.VARIABLES}?processId=${process.recId}`);
        },
      },
      {
        id: 'steps',
        label: t('wfProcess.commands.steps'),
        requiresSelection: true,
        onClick: (process) => {
          if (process) navigate(`${WORKFLOW_ROUTE_PATHS.STEPS}?processId=${process.recId}`);
        },
      },
      { id: 'options', label: t('customerCommands.options') },
    ],
  };

  return (
    <ListDetailsPage variant="enterprise" title={t('pages.wfProcesses.title')} config={config} />
  );
}
