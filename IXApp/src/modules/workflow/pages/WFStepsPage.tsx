import React, { useMemo } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { WORKFLOW_ROUTE_PATHS } from '../routes/workflowRoutePaths';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { AppLookupGridField } from '@shared/components/fields/AppLookupGridField';
import { wfProcessApi, type WfProcessRecord } from '../api/wfProcessApi';
import { wfStepApi, type WfStepRecord } from '../api/wfStepApi';
import { fetchProcessPage, processLookupColumns } from '../lookups/processLookup';

const numberValue = (value: DetailValue): number => Number(value) || 0;
const textValue = (value: string | null): string => value ?? '';

const emptyStep = (processId = 0): WfStepRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  code: null,
  name: '',
  description: null,
  processId,
  sortOrder: 0,
  score: 0,
  autoPassingHrs: 0,
  allMandatory: false,
  sysField: false,
  isActive: true,
  rowVersion: null,
  recVersion: 1,
  dataAreaId: 'dat',
});

export function WFStepsPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const requestedProcessId = Number(searchParams.get('processId'));
  const scopedProcessId =
    Number.isSafeInteger(requestedProcessId) && requestedProcessId > 0 ? requestedProcessId : null;
  const sections = useMemo<DetailSectionConfig[]>(
    () => [
      {
        id: 'configuration',
        title: t('wfStep.sections.configuration'),
        groups: [
          {
            id: 'assignment',
            title: t('wfStep.groups.assignment'),
            fields: [
              {
                name: 'processId',
                label: t('wfStep.fields.process'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <AppLookupGridField<WfProcessRecord>
                    name="processId"
                    label={t('wfStep.fields.process')}
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
              { name: 'sortOrder', label: t('wfStep.fields.sortOrder'), type: 'number' },
            ],
          },
          {
            id: 'behavior',
            title: t('wfStep.groups.behavior'),
            fields: [
              { name: 'score', label: t('wfStep.fields.score'), type: 'number' },
              {
                name: 'autoPassingHrs',
                label: t('wfStep.fields.autoPassingHrs'),
                type: 'number',
              },
              {
                name: 'allMandatory',
                label: t('wfStep.fields.allMandatory'),
                type: 'boolean',
              },
              {
                name: 'sysField',
                label: t('wfStep.fields.systemStep'),
                type: 'boolean',
                disabled: true,
              },
            ],
          },
        ],
      },
    ],
    [scopedProcessId, t]
  );

  const config: EnterpriseListDetailsConfig<WfStepRecord> = {
    recordTableName: 'WfStep',
    dataSource: {
      type: 'remote',
      key: scopedProcessId ? `workflow-steps-process-${scopedProcessId}` : 'workflow-steps',
      load: async (signal) => {
        const steps = await wfStepApi.list(signal);
        return scopedProcessId === null
          ? steps
          : steps.filter((step) => step.processId === scopedProcessId);
      },
      create: wfStepApi.create,
      update: wfStepApi.update,
      delete: wfStepApi.delete,
    },
    createRecord: () => emptyStep(scopedProcessId ?? 0),
    numberSequence: { key: 'WfStep', field: 'code' },
    getPrimaryText: (record) => textValue(record.name) || textValue(record.code),
    getSecondaryText: (record) => record.code || textValue(record.description),
    matchesSearch: (record, query) =>
      `${record.code ?? ''} ${record.name ?? ''} ${record.description ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      processId: record.processId,
      sortOrder: record.sortOrder,
      score: record.score,
      autoPassingHrs: record.autoPassingHrs,
      allMandatory: record.allMandatory,
      sysField: record.sysField,
    }),
    setValues: (record, values) => ({
      ...record,
      processId: numberValue(values.processId),
      sortOrder: numberValue(values.sortOrder),
      score: numberValue(values.score),
      autoPassingHrs: numberValue(values.autoPassingHrs),
      allMandatory: Boolean(values.allMandatory),
      sysField: Boolean(values.sysField),
    }),
    headerFields: [
      {
        id: 'code',
        label: t('wfStep.fields.code'),
        disabled: true,
        getValue: (record) => textValue(record.code),
        setValue: (record, value) => ({ ...record, code: String(value) || null }),
      },
      {
        id: 'name',
        label: t('wfStep.fields.name'),
        getValue: (record) => textValue(record.name),
        setValue: (record, value) => ({ ...record, name: String(value) || null }),
      },
      {
        id: 'description',
        label: t('wfStep.fields.description'),
        getValue: (record) => textValue(record.description),
        setValue: (record, value) => ({ ...record, description: String(value) || null }),
      },
    ],
    sections,
    permissions: {
      view: 'Workflow.Steps.View',
      create: 'Workflow.Steps.Create',
      edit: 'Workflow.Steps.Edit',
      delete: 'Workflow.Steps.Delete',
    },
    advancedFilter: {
      fieldLabel: t('wfStep.fields.name'),
      getValue: (record) => record.name,
      matches: (record, value) =>
        textValue(record.name).toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()),
    },
    commands: [
      {
        id: 'activities',
        label: t('nav.wfActivities'),
        requiresSelection: true,
        onClick: (step) => {
          if (step) navigate(`${WORKFLOW_ROUTE_PATHS.ACTIVITIES}?stepId=${step.recId}`);
        },
      },
      { id: 'options', label: t('customerCommands.options') },
    ],
  };

  return <ListDetailsPage variant="enterprise" title={t('pages.wfSteps.title')} config={config} />;
}
