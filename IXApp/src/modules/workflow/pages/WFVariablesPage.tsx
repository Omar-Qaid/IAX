import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { AppLookupField } from '@shared/components/fields/AppLookupField';
import { AppLookupGridField } from '@shared/components/fields/AppLookupGridField';
import { wfDataTypeApi } from '../api/workflowSetupApis';
import { wfProcessApi, type WfProcessRecord } from '../api/wfProcessApi';
import { wfVariableApi, type WfVariableRecord } from '../api/wfVariableApi';
import { fetchProcessPage, processLookupColumns } from '../lookups/processLookup';

const emptyVariable = (processId = 0): WfVariableRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  code: null,
  name: '',
  nameAlias: null,
  description: null,
  dataTypeId: 0,
  processId,
  sortOrder: 0,
  dataType: null,
  process: null,
  isActive: true,
  rowVersion: null,
  recVersion: 1,
  dataAreaId: 'dat',
});

const numberValue = (value: DetailValue): number => Number(value) || 0;
const textValue = (value: string | null | undefined): string => value ?? '';

export function WFVariablesPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const [searchParams] = useSearchParams();
  const requestedProcessId = Number(searchParams.get('processId'));
  const scopedProcessId =
    Number.isSafeInteger(requestedProcessId) && requestedProcessId > 0 ? requestedProcessId : null;
  const dataTypesQuery = useQuery({
    queryKey: ['workflow', 'data-type-lookup'],
    queryFn: ({ signal }) => wfDataTypeApi.list(signal),
  });
  const dataTypeOptions = useMemo(
    () =>
      (dataTypesQuery.data ?? []).map((dataType) => ({
        id: dataType.recId,
        code: dataType.code ?? '',
        name: dataType.name ?? '',
      })),
    [dataTypesQuery.data]
  );
  const sections = useMemo<DetailSectionConfig[]>(
    () => [
      {
        id: 'configuration',
        title: t('wfVariable.sections.configuration'),
        groups: [
          {
            id: 'assignment',
            title: t('wfVariable.groups.assignment'),
            fields: [
              {
                name: 'processId',
                label: t('wfVariable.fields.process'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <AppLookupGridField<WfProcessRecord>
                    name="processId"
                    label={t('wfVariable.fields.process')}
                    value={numberValue(value ?? 0)}
                    onChange={(processId) => onChange(Number(processId) || 0)}
                    disabled={disabled || scopedProcessId !== null}
                    columns={[...processLookupColumns]}
                    queryKey={['workflow', 'process-lookup']}
                    fetchPage={fetchProcessPage}
                    fetchById={async (processId) =>
                      wfProcessApi.getById(Number(processId)).catch(() => null)
                    }
                    valueField="recId"
                    labelField="name"
                    pageSize={25}
                  />
                ),
              },
              {
                name: 'dataTypeId',
                label: t('wfVariable.fields.dataType'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <AppLookupField
                    name="dataTypeId"
                    label={t('wfVariable.fields.dataType')}
                    value={numberValue(value ?? 0)}
                    onChange={(dataTypeId) => onChange(Number(dataTypeId) || 0)}
                    options={dataTypeOptions}
                    disabled={disabled || dataTypesQuery.isLoading}
                    displayMode="select"
                  />
                ),
              },
              {
                name: 'sortOrder',
                label: t('wfVariable.fields.sortOrder'),
                type: 'number',
              },
            ],
          },
        ],
      },
    ],
    [dataTypeOptions, dataTypesQuery.isLoading, scopedProcessId, t]
  );

  const config: EnterpriseListDetailsConfig<WfVariableRecord> = {
    recordTableName: 'WfVariable',
    dataSource: {
      type: 'remote',
      key: scopedProcessId ? `workflow-variables-process-${scopedProcessId}` : 'workflow-variables',
      load: async (signal) => {
        const variables = await wfVariableApi.list(signal);
        return scopedProcessId === null
          ? variables
          : variables.filter((variable) => variable.processId === scopedProcessId);
      },
      create: wfVariableApi.create,
      update: wfVariableApi.update,
      delete: wfVariableApi.delete,
    },
    createRecord: () => emptyVariable(scopedProcessId ?? 0),
    numberSequence: { key: 'WfVariable', field: 'code' },
    getPrimaryText: (record) => textValue(record.name) || textValue(record.code),
    getSecondaryText: (record) => record.code || textValue(record.description),
    matchesSearch: (record, query) =>
      `${record.code ?? ''} ${record.name ?? ''} ${record.description ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      processId: record.processId,
      dataTypeId: record.dataTypeId,
      sortOrder: record.sortOrder,
    }),
    setValues: (record, values) => ({
      ...record,
      processId: numberValue(values.processId),
      dataTypeId: numberValue(values.dataTypeId),
      sortOrder: numberValue(values.sortOrder),
    }),
    headerFields: [
      {
        id: 'code',
        label: t('wfVariable.fields.code'),
        disabled: true,
        getValue: (record) => textValue(record.code),
        setValue: (record, value) => ({ ...record, code: String(value) || null }),
      },
      {
        id: 'name',
        label: t('wfVariable.fields.name'),
        getValue: (record) => textValue(record.name),
        setValue: (record, value) => ({ ...record, name: String(value) || null }),
      },
      {
        id: 'nameAlias',
        label: t('workflowSetup.fields.nameAlias'),
        getValue: (record) => textValue(record.nameAlias),
        setValue: (record, value) => ({ ...record, nameAlias: String(value) || null }),
      },
      {
        id: 'description',
        label: t('wfVariable.fields.description'),
        getValue: (record) => textValue(record.description),
        setValue: (record, value) => ({ ...record, description: String(value) || null }),
      },
    ],
    sections,
    permissions: {
      view: 'Workflow.Variables.View',
      create: 'Workflow.Variables.Create',
      edit: 'Workflow.Variables.Edit',
      delete: 'Workflow.Variables.Delete',
    },
    advancedFilter: {
      fieldLabel: t('wfVariable.fields.name'),
      getValue: (record) => record.name,
      matches: (record, value) =>
        textValue(record.name).toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()),
    },
  };

  return (
    <ListDetailsPage variant="enterprise" title={t('pages.wfVariables.title')} config={config} />
  );
}
