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
import { localizedName } from '@shared/utilities/localizedName';

const numberValue = (value: DetailValue | undefined): number => Number(value) || 0;
const textValue = (value: string | null | undefined): string => value ?? '';
const lookupColumns = [
  { field: 'code', header: 'workflowSetup.fields.code', width: 120 },
  { field: 'name', header: 'workflowSetup.fields.name', flex: 1, showInRtl: false },
  { field: 'nameAlias', header: 'workflowSetup.fields.nameAlias', flex: 1, showInLtr: false },
] as const;

const createLookupPage = <T extends { code: string | null; name: string | null; nameAlias?: string | null; description?: string | null }>(
  load: (signal?: AbortSignal) => Promise<T[]>
) =>
  async ({ pageNumber, pageSize, search, signal }: { pageNumber: number; pageSize: number; search: string; signal?: AbortSignal }) => {
    const records = await load(signal);
    const query = search.trim().toLocaleLowerCase();
    const filtered = query
      ? records.filter((record) =>
          `${record.code ?? ''} ${record.name ?? ''} ${record.nameAlias ?? ''} ${record.description ?? ''}`
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
  nameAlias: null,
  description: null,
  sortOrder: 0,
  activityTypeId: 0,
  stepId,
  performerId: 0,
  score: 0,
  sysNotificationTemplateId: null,
  isSystemNotificationEnabled: false,
  isEmailNotificationEnabled: false,
  isSmsNotificationEnabled: false,
  isWhatsAppNotificationEnabled: false,
  mandatoryDocuments: false,
  canViewPreviousSteps: false,
  canViewPreviousDocuments: false,
  isAutoPassEnabled: false,
  autoPassAfterHours: 0,
  extendedProperties: null,
  isActive: true,
  rowVersion: null,
  recVersion: 1,
  dataAreaId: 'dat',
});

export function WfActivitiesPage(): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
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
                    labelFieldAr="nameAlias"
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
                    labelFieldAr="nameAlias"
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
                    labelFieldAr="nameAlias"
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
              { name: 'isSystemNotificationEnabled', label: t('wfActivity.fields.isSystemNotificationEnabled'), type: 'boolean' },
              { name: 'isEmailNotificationEnabled', label: t('wfActivity.fields.isEmailNotificationEnabled'), type: 'boolean' },
              { name: 'isSmsNotificationEnabled', label: t('wfActivity.fields.isSmsNotificationEnabled'), type: 'boolean' },
              { name: 'isWhatsAppNotificationEnabled', label: t('wfActivity.fields.isWhatsAppNotificationEnabled'), type: 'boolean' },
            ],
          },
          {
            id: 'behavior',
            title: t('wfActivity.groups.behavior'),
            fields: [
              { name: 'mandatoryDocuments', label: t('wfActivity.fields.mandatoryDocuments'), type: 'boolean' },
              { name: 'canViewPreviousDocuments', label: t('wfActivity.fields.canViewPreviousDocuments'), type: 'boolean' },
              { name: 'canViewPreviousSteps', label: t('wfActivity.fields.canViewPreviousSteps'), type: 'boolean' },
              { name: 'isAutoPassEnabled', label: t('wfActivity.fields.isAutoPassEnabled'), type: 'boolean' },
              { name: 'autoPassAfterHours', label: t('wfActivity.fields.autoPassAfterHours'), type: 'number' },
              { name: 'extendedProperties', label: t('wfActivity.fields.extendedProperties'), multiline: true, rows: 3 },
            ],
          },
        ],
      },
    ],
    [scopedStepId, t]
  );

  const config: EnterpriseListDetailsConfig<WfActivityRecord> = {
    recordTableName: 'WfActivity',
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
    numberSequence: { key: 'WfActivity', field: 'code' },
    getPrimaryText: (record) => localizedName(record, isRtl) || textValue(record.code),
    getSecondaryText: (record) => record.code || textValue(record.description),
    matchesSearch: (record, query) =>
      `${record.code ?? ''} ${record.name ?? ''} ${record.nameAlias ?? ''} ${record.description ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      stepId: record.stepId,
      activityTypeId: record.activityTypeId,
      performerId: record.performerId,
      score: record.score,
      sysNotificationTemplateId: record.sysNotificationTemplateId ?? 0,
      isSystemNotificationEnabled: record.isSystemNotificationEnabled,
      isEmailNotificationEnabled: record.isEmailNotificationEnabled,
      isSmsNotificationEnabled: record.isSmsNotificationEnabled,
      isWhatsAppNotificationEnabled: record.isWhatsAppNotificationEnabled,
      canViewPreviousSteps: record.canViewPreviousSteps,
      canViewPreviousDocuments: record.canViewPreviousDocuments,
      mandatoryDocuments: record.mandatoryDocuments,
      isAutoPassEnabled: record.isAutoPassEnabled,
      autoPassAfterHours: record.autoPassAfterHours,
      extendedProperties: textValue(record.extendedProperties),
    }),
    setValues: (record, values) => ({
      ...record,
      stepId: numberValue(values.stepId),
      activityTypeId: numberValue(values.activityTypeId),
      performerId: numberValue(values.performerId),
      score: numberValue(values.score),
      sysNotificationTemplateId: numberValue(values.sysNotificationTemplateId) || null,
      isSystemNotificationEnabled: Boolean(values.isSystemNotificationEnabled),
      isEmailNotificationEnabled: Boolean(values.isEmailNotificationEnabled),
      isSmsNotificationEnabled: Boolean(values.isSmsNotificationEnabled),
      isWhatsAppNotificationEnabled: Boolean(values.isWhatsAppNotificationEnabled),
      canViewPreviousSteps: Boolean(values.canViewPreviousSteps),
      canViewPreviousDocuments: Boolean(values.canViewPreviousDocuments),
      mandatoryDocuments: Boolean(values.mandatoryDocuments),
      isAutoPassEnabled: Boolean(values.isAutoPassEnabled),
      autoPassAfterHours: numberValue(values.autoPassAfterHours),
      extendedProperties: String(values.extendedProperties || '') || null,
    }),
    headerFields: [
      { id: 'code', label: t('wfActivity.fields.code'), disabled: true, getValue: (record) => textValue(record.code), setValue: (record, value) => ({ ...record, code: String(value) || null }) },
      { id: 'name', label: t('wfActivity.fields.name'), getValue: (record) => textValue(record.name), setValue: (record, value) => ({ ...record, name: String(value) || null }) },
      { id: 'nameAlias', label: t('workflowSetup.fields.nameAlias'), getValue: (record) => textValue(record.nameAlias), setValue: (record, value) => ({ ...record, nameAlias: String(value) || null }) },
      { id: 'description', label: t('wfActivity.fields.description'), getValue: (record) => textValue(record.description), setValue: (record, value) => ({ ...record, description: String(value) || null }) },
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
      ...(record.activityTypeId <= 0 ? { activityTypeId: t('validation.required', { field: t('wfActivity.fields.activityType') }) } : {}),
      ...(record.stepId <= 0 ? { stepId: t('validation.required', { field: t('wfActivity.fields.step') }) } : {}),
    }),
    advancedFilter: {
      fieldLabel: t('wfActivity.fields.name'),
      getValue: (record) => localizedName(record, isRtl),
      matches: (record, value) => localizedName(record, isRtl).toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()),
    },
  };

  return <ListDetailsPage variant="enterprise" title={t('pages.wfActivities.title')} config={config} />;
}
