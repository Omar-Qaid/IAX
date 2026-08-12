import React, { useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  DetailValue,
  DetailValues,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { AppLookupGridField } from '@shared/components/fields/AppLookupGridField';
import { wfActivityApi, type WfActivityRecord } from '../api/wfActivityApi';
import { wfPerformerApi } from '../api/wfPerformerApi';
import { wfStepApi, type WfStepRecord } from '../api/wfStepApi';
import { wfActivityTypeApi } from '../api/workflowSetupApis';
import type { WorkflowMasterRecord } from '../api/workflowMasterApi';

const numberValue = (value: DetailValue | undefined): number => Number(value) || 0;
const textValue = (value: string | null): string => value ?? '';
const lookupColumns = [
  { field: 'code', header: 'workflowSetup.fields.code', width: 120 },
  { field: 'name', header: 'workflowSetup.fields.name', flex: 1 },
  { field: 'nameAR', header: 'workflowSetup.fields.nameAR', flex: 1 },
] as const;

const createLookupPage = <T extends { code: string | null; name: string | null; nameAR: string | null }>(
  load: (signal?: AbortSignal) => Promise<T[]>
) =>
  async ({ pageNumber, pageSize, search, signal }: { pageNumber: number; pageSize: number; search: string; signal?: AbortSignal }) => {
    const records = await load(signal);
    const query = search.trim().toLocaleLowerCase();
    const filtered = query
      ? records.filter((record) =>
          `${record.code ?? ''} ${record.name ?? ''} ${record.nameAR ?? ''}`
            .toLocaleLowerCase()
            .includes(query)
        )
      : records;
    const start = (pageNumber - 1) * pageSize;
    return {
      data: filtered.slice(start, start + pageSize),
      pageNumber,
      totalPages: Math.max(1, Math.ceil(filtered.length / pageSize)),
      totalRecords: filtered.length,
    };
  };

const fetchStepPage = createLookupPage(wfStepApi.list);
const fetchActivityTypePage = createLookupPage(wfActivityTypeApi.list);
const fetchPerformerPage = createLookupPage(wfPerformerApi.list);

const emptyActivity = (stepId = 0): WfActivityRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  code: null,
  name: '',
  nameAR: '',
  description: null,
  descriptionAR: null,
  sortOrder: 0,
  activityTypeId: 0,
  stepId,
  performerId: 0,
  score: 0,
  sysNotificationTemplateId: null,
  alertingBySystem: false,
  alertingByEmail: false,
  alertingBySms: false,
  alertingByWhatsApp: false,
  showPreviousSteps: false,
  showPreviousDocs: false,
  mandatoryDocs: false,
  autoPassEnabled: false,
  autoPassingHrs: 0,
  extendedProperties: null,
  isActive: true,
  rowVersion: null,
  recVersion: 1,
  dataAreaId: 'dat',
});

export function WfActivitiesPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const [searchParams] = useSearchParams();
  const requestedStepId = Number(searchParams.get('stepId'));
  const scopedStepId =
    Number.isSafeInteger(requestedStepId) && requestedStepId > 0 ? requestedStepId : null;

  const sections = useMemo<DetailSectionConfig[]>(
    () => [
      {
        id: 'configuration',
        title: t('wfActivity.sections.configuration'),
        groups: [
          {
            id: 'assignment',
            title: t('wfActivity.groups.assignment'),
            fields: [
              {
                name: 'stepId',
                label: t('wfActivity.fields.step'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <AppLookupGridField<WfStepRecord>
                    name="stepId"
                    label={t('wfActivity.fields.step')}
                    value={numberValue(value)}
                    onChange={(stepId) => onChange(Number(stepId) || 0)}
                    disabled={disabled || scopedStepId !== null}
                    columns={[...lookupColumns]}
                    queryKey={['workflow', 'step-lookup']}
                    fetchPage={fetchStepPage}
                    fetchById={async (stepId) =>
                      (await wfStepApi.list()).find((step) => step.recId === Number(stepId)) ?? null
                    }
                    valueField="recId"
                    labelField="name"
                    labelFieldAr="nameAR"
                    pageSize={25}
                  />
                ),
              },
              {
                name: 'activityTypeId',
                label: t('wfActivity.fields.activityType'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <AppLookupGridField<WorkflowMasterRecord>
                    name="activityTypeId"
                    label={t('wfActivity.fields.activityType')}
                    value={numberValue(value)}
                    onChange={(id) => onChange(Number(id) || 0)}
                    disabled={disabled}
                    columns={[...lookupColumns]}
                    queryKey={['workflow', 'activity-type-lookup']}
                    fetchPage={fetchActivityTypePage}
                    fetchById={async (activityTypeId) =>
                      (await wfActivityTypeApi.list()).find(
                        (activityType) => activityType.recId === Number(activityTypeId)
                      ) ?? null
                    }
                    valueField="recId"
                    labelField="name"
                    labelFieldAr="nameAR"
                    pageSize={25}
                  />
                ),
              },
              {
                name: 'performerId',
                label: t('wfActivity.fields.performer'),
                renderOwnLabel: true,
                render: ({ value, disabled, onChange }) => (
                  <AppLookupGridField<WorkflowMasterRecord>
                    name="performerId"
                    label={t('wfActivity.fields.performer')}
                    value={numberValue(value)}
                    onChange={(id) => onChange(Number(id) || 0)}
                    disabled={disabled}
                    columns={[...lookupColumns]}
                    queryKey={['workflow', 'performer-lookup']}
                    fetchPage={fetchPerformerPage}
                    fetchById={async (performerId) =>
                      (await wfPerformerApi.list()).find(
                        (performer) => performer.recId === Number(performerId)
                      ) ?? null
                    }
                    valueField="recId"
                    labelField="name"
                    labelFieldAr="nameAR"
                    pageSize={25}
                  />
                ),
              },
              { name: 'score', label: t('wfActivity.fields.score'), type: 'number' },
            ],
          },
          {
            id: 'notifications',
            title: t('wfActivity.groups.notifications'),
            fields: [
              { name: 'sysNotificationTemplateId', label: t('wfActivity.fields.notificationTemplate'), type: 'number' },
              { name: 'alertingBySystem', label: t('wfActivity.fields.alertingBySystem'), type: 'boolean' },
              { name: 'alertingByEmail', label: t('wfActivity.fields.alertingByEmail'), type: 'boolean' },
              { name: 'alertingBySms', label: t('wfActivity.fields.alertingBySms'), type: 'boolean' },
              { name: 'alertingByWhatsApp', label: t('wfActivity.fields.alertingByWhatsApp'), type: 'boolean' },
            ],
          },
          {
            id: 'behavior',
            title: t('wfActivity.groups.behavior'),
            fields: [
              { name: 'showPreviousSteps', label: t('wfActivity.fields.showPreviousSteps'), type: 'boolean' },
              { name: 'showPreviousDocs', label: t('wfActivity.fields.showPreviousDocs'), type: 'boolean' },
              { name: 'mandatoryDocs', label: t('wfActivity.fields.mandatoryDocs'), type: 'boolean' },
              { name: 'autoPassEnabled', label: t('wfActivity.fields.autoPassEnabled'), type: 'boolean' },
              { name: 'autoPassingHrs', label: t('wfActivity.fields.autoPassingHrs'), type: 'number' },
              { name: 'extendedProperties', label: t('wfActivity.fields.extendedProperties'), multiline: true, rows: 3 },
            ],
          },
        ],
      },
    ],
    [scopedStepId, t]
  );

  const config: EnterpriseListDetailsConfig<WfActivityRecord> = {
    dataSource: {
      type: 'remote',
      key: scopedStepId ? `workflow-activities-step-${scopedStepId}` : 'workflow-activities',
      load: async (signal) => {
        const activities = await wfActivityApi.list(signal);
        return scopedStepId === null
          ? activities
          : activities.filter((activity) => activity.stepId === scopedStepId);
      },
      create: wfActivityApi.create,
      update: wfActivityApi.update,
      delete: wfActivityApi.delete,
    },
    createRecord: () => emptyActivity(scopedStepId ?? 0),
    getPrimaryText: (record) => textValue(record.name) || textValue(record.code),
    getSecondaryText: (record) => record.code || textValue(record.nameAR),
    matchesSearch: (record, query) =>
      `${record.code ?? ''} ${record.name ?? ''} ${record.nameAR ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      stepId: record.stepId,
      activityTypeId: record.activityTypeId,
      performerId: record.performerId,
      score: record.score,
      sysNotificationTemplateId: record.sysNotificationTemplateId ?? 0,
      alertingBySystem: record.alertingBySystem,
      alertingByEmail: record.alertingByEmail,
      alertingBySms: record.alertingBySms,
      alertingByWhatsApp: record.alertingByWhatsApp,
      showPreviousSteps: record.showPreviousSteps,
      showPreviousDocs: record.showPreviousDocs,
      mandatoryDocs: record.mandatoryDocs,
      autoPassEnabled: record.autoPassEnabled,
      autoPassingHrs: record.autoPassingHrs,
      extendedProperties: textValue(record.extendedProperties),
    }),
    setValues: (record, values) => ({
      ...record,
      stepId: numberValue(values.stepId),
      activityTypeId: numberValue(values.activityTypeId),
      performerId: numberValue(values.performerId),
      score: numberValue(values.score),
      sysNotificationTemplateId: numberValue(values.sysNotificationTemplateId) || null,
      alertingBySystem: Boolean(values.alertingBySystem),
      alertingByEmail: Boolean(values.alertingByEmail),
      alertingBySms: Boolean(values.alertingBySms),
      alertingByWhatsApp: Boolean(values.alertingByWhatsApp),
      showPreviousSteps: Boolean(values.showPreviousSteps),
      showPreviousDocs: Boolean(values.showPreviousDocs),
      mandatoryDocs: Boolean(values.mandatoryDocs),
      autoPassEnabled: Boolean(values.autoPassEnabled),
      autoPassingHrs: numberValue(values.autoPassingHrs),
      extendedProperties: String(values.extendedProperties || '') || null,
    }),
    headerFields: [
      { id: 'code', label: t('wfActivity.fields.code'), disabled: true, getValue: (record) => textValue(record.code), setValue: (record, value) => ({ ...record, code: String(value) || null }) },
      { id: 'name', label: t('wfActivity.fields.name'), getValue: (record) => textValue(record.name), setValue: (record, value) => ({ ...record, name: String(value) || null }) },
      { id: 'nameAR', label: t('wfActivity.fields.nameAR'), getValue: (record) => textValue(record.nameAR), setValue: (record, value) => ({ ...record, nameAR: String(value) || null }) },
      { id: 'description', label: t('wfActivity.fields.description'), getValue: (record) => textValue(record.description), setValue: (record, value) => ({ ...record, description: String(value) || null }) },
      { id: 'descriptionAR', label: t('wfActivity.fields.descriptionAR'), getValue: (record) => textValue(record.descriptionAR), setValue: (record, value) => ({ ...record, descriptionAR: String(value) || null }) },
    ],
    sections,
    permissions: {
      view: 'Workflow.Activities.View',
      create: 'Workflow.Activities.Create',
      edit: 'Workflow.Activities.Edit',
      delete: 'Workflow.Activities.Delete',
    },
    validate: (record) => ({
      ...(!record.name?.trim() ? { name: t('validation.required', { field: t('wfActivity.fields.name') }) } : {}),
      ...(!record.nameAR?.trim() ? { nameAR: t('validation.required', { field: t('wfActivity.fields.nameAR') }) } : {}),
      ...(record.activityTypeId <= 0 ? { activityTypeId: t('validation.required', { field: t('wfActivity.fields.activityType') }) } : {}),
      ...(record.stepId <= 0 ? { stepId: t('validation.required', { field: t('wfActivity.fields.step') }) } : {}),
    }),
    advancedFilter: {
      fieldLabel: t('wfActivity.fields.name'),
      getValue: (record) => record.name,
      matches: (record, value) => textValue(record.name).toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()),
    },
    commands: [{ id: 'options', label: t('customerCommands.options') }],
  };

  return <ListDetailsPage variant="enterprise" title={t('pages.wfActivities.title')} config={config} />;
}
