import { wfProcessApi, type WfProcessRecord } from '@modules/workflow/api/wfProcessApi';
import { wfStepApi } from '@modules/workflow/api/wfStepApi';
import { wfVariableApi } from '@modules/workflow/api/wfVariableApi';
import { wfActivityApi } from '@modules/workflow/api/wfActivityApi';
import { wfActivityControlApi } from '@modules/workflow/api/wfActivityControlApi';
import { wfActivityControlOptionApi } from '@modules/workflow/api/wfActivityControlOptionApi';
import { wfActivityControlValidationApi } from '@modules/workflow/api/wfActivityControlValidationApi';
import {
  wfActivityTypeApi,
  wfControlApi,
  wfOperatorApi,
} from '@modules/workflow/api/workflowSetupApis';
import {
  wfRequestControlApi,
  type WfRequestControlRecord,
} from '@modules/workflow/api/wfRequestControlApi';
import { wfRequestControlOptionApi } from '@modules/workflow/api/wfRequestControlOptionApi';
import { wfRequestControlValidationApi } from '@modules/workflow/api/wfRequestControlValidationApi';
import { wfTransitionApi } from '@modules/workflow/api/wfTransitionApi';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type {
  BuilderActivity,
  BuilderControl,
  BuilderControlType,
  BuilderDataType,
  BuilderStep,
  BuilderTransition,
  BuilderVariable,
  ProcessBuilderDocument,
} from '../types/processBuilderTypes';
import { resolvedValidationMessage, validationUsesCustomMessage } from '../validationDefaults';

const dataType = (id: number): BuilderDataType =>
  (({ 1: 'text', 2: 'number', 3: 'boolean', 4: 'date', 5: 'object' })[id] as BuilderDataType) ??
  'text';
const dataTypeId = (type: BuilderDataType): number =>
  ({ text: 1, number: 2, boolean: 3, date: 4, object: 1 })[type];
const numericId = (id: string): number | null => (/^\d+$/.test(id) ? Number(id) : null);
const optionControlTypes = new Set<BuilderControlType>([
  'dropdown-manual',
  'checkboxlist',
  'radiobuttonlist',
  'table',
]);
const builderControlType = (value: string): BuilderControlType => {
  const normalized = value.replace(/[^a-z0-9]/gi, '').toLocaleLowerCase();
  const types: BuilderControlType[] = [
    'digits',
    'longtext',
    'date',
    'time',
    'url',
    'checkboxlist',
    'checkbox',
    'radiobuttonlist',
    'table',
    'label',
    'employeesearch',
    'employeeid',
    'file',
    'showroom',
    'signature',
    'location',
    'advertiser',
  ];
  if (
    normalized.includes('dropdown') &&
    (normalized.includes('database') || normalized.includes('db'))
  )
    return 'dropdown-db';
  if (normalized.includes('dropdown')) return 'dropdown-manual';
  return types.find((type) => normalized.includes(type.replace(/[^a-z0-9]/gi, ''))) ?? 'text';
};
const parseObject = (value: string | null): Record<string, unknown> => {
  if (!value) return {};
  try {
    return JSON.parse(value) as Record<string, unknown>;
  } catch {
    return {};
  }
};
const normalizeOptionFeatureConfiguration = (
  value: unknown
): NonNullable<BuilderControl['optionFeatureConfigurations']>[number] => {
  const item = value && typeof value === 'object' ? (value as Record<string, unknown>) : {};
  return {
    requireFileUpload: item.requireFileUpload === true || item.allowFileUpload === true,
    sendAlertMessage: item.sendAlertMessage === true || item.sendAlert === true,
    alertMessage: typeof item.alertMessage === 'string' ? item.alertMessage : '',
    performerIds: Array.isArray(item.performerIds)
      ? item.performerIds.filter((id): id is string => typeof id === 'string')
      : [],
    showOtherControls:
      item.showOtherControls === true ||
      (Array.isArray(item.visibleControlIds) && item.visibleControlIds.length > 0),
    visibleControlIds: Array.isArray(item.visibleControlIds)
      ? item.visibleControlIds.filter((id): id is string => typeof id === 'string')
      : [],
  };
};
const builderValidationType = (value: string): BuilderControl['validations'][number]['type'] => {
  const normalized = value.replace(/[^a-z0-9]/gi, '').toLocaleLowerCase();
  const aliases: Record<string, BuilderControl['validations'][number]['type']> = {
    required: 'required',
    regex: 'regex',
    pattern: 'pattern',
    minlength: 'minLength',
    maxlength: 'maxLength',
    exactlength: 'exactLength',
    length: 'length',
    minvalue: 'minValue',
    maxvalue: 'maxValue',
    range: 'range',
    compare: 'compare',
    comparison: 'comparison',
    expression: 'expression',
    custom: 'custom',
    customexpression: 'custom',
    crossfield: 'crossField',
    mask: 'mask',
    inputmask: 'inputMask',
    startswith: 'startsWith',
    endswith: 'endsWith',
    contains: 'contains',
    email: 'email',
    url: 'url',
    phone: 'phone',
    saudimobile: 'saudiMobile',
    saudinationalid: 'saudiNationalId',
    saudiiban: 'saudiIban',
    taxnumber: 'taxNumber',
    passport: 'passport',
    fileextensions: 'fileExtensions',
    filesize: 'fileSize',
    maxfiles: 'maxFiles',
    minselected: 'minSelected',
    maxselected: 'maxSelected',
  };
  return aliases[normalized] ?? 'custom';
};
const builderActivityType = (value: string): BuilderActivity['type'] => {
  const normalized = value.replace(/[^a-z0-9]/gi, '').toLocaleLowerCase();
  if (normalized.includes('dataentry')) return 'data-entry';
  if (normalized.includes('notification')) return 'notification';
  if (normalized.includes('review')) return 'review';
  if (normalized.includes('api')) return 'api';
  return 'approval';
};
const resolveActivityType = (
  activityTypes: Awaited<ReturnType<typeof wfActivityTypeApi.list>>,
  activity: BuilderActivity
) => {
  const explicitId = Number(activity.activityTypeId);
  const explicit =
    explicitId > 0 ? activityTypes.find((item) => item.recId === explicitId) : undefined;
  if (explicit) return explicit;

  const normalizedType = activity.type.replace(/[^a-z0-9]/gi, '').toLocaleLowerCase();
  const matching = activityTypes.find((item) =>
    `${item.code ?? ''} ${item.name ?? ''}`
      .replace(/[^a-z0-9]/gi, '')
      .toLocaleLowerCase()
      .includes(normalizedType)
  );
  if (matching) return matching;

  // Designer modes such as "approval" describe behavior, while the seeded
  // backend activity classification is NORMAL/PARTIAL. New activities use NORMAL.
  return (
    activityTypes.find((item) => item.code?.trim().toLocaleUpperCase() === 'NORMAL') ??
    activityTypes.find((item) => item.isActive !== false)
  );
};
const builderOperator = (value: string): BuilderTransition['operator'] => {
  const normalized = value.trim().toLocaleLowerCase();
  if (normalized === '<>' || normalized === 'neq') return '!=';
  if (normalized === 'gt') return '>';
  if (normalized === 'lt') return '<';
  if (normalized === 'gte') return '>=';
  if (normalized === 'lte') return '<=';
  if (normalized === 'between') return 'between';
  return ['=', '!=', '>', '<', '>=', '<=', 'contains', 'isEmpty', 'between'].includes(value)
    ? (value as BuilderTransition['operator'])
    : '=';
};

const validateVariables = (variables: BuilderVariable[]) => {
  const names = new Set<string>();
  for (const [index, variable] of variables.entries()) {
    const name = variable.name.trim();
    if (!name) throw new Error(`Variable ${index + 1}: name is required.`);
    const normalizedName = name.toLocaleLowerCase();
    if (names.has(normalizedName)) throw new Error(`Variable name '${name}' is duplicated.`);
    names.add(normalizedName);
    if (!Number.isInteger(variable.sortOrder) || variable.sortOrder < 0 || variable.sortOrder > 255)
      throw new Error(`Variable '${name}': sort order must be a whole number from 0 to 255.`);
  }
};

const validateTransitionValue = (
  transition: BuilderTransition,
  variable: BuilderVariable,
  index: number
) => {
  if (transition.operator === 'isEmpty') return;
  const value = transition.value.trim();
  if (!value) throw new Error(`Transition ${index + 1}: comparison value is required.`);
  if (variable.dataType === 'number' && !Number.isFinite(Number(value)))
    throw new Error(`Transition ${index + 1}: comparison value must be a number.`);
  if (variable.dataType === 'boolean' && value !== 'true' && value !== 'false')
    throw new Error(`Transition ${index + 1}: comparison value must be Yes or No.`);
  if (variable.dataType === 'date') {
    const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
    const date = match
      ? new Date(Date.UTC(Number(match[1]), Number(match[2]) - 1, Number(match[3])))
      : null;
    if (
      !match ||
      !date ||
      date.getUTCFullYear() !== Number(match[1]) ||
      date.getUTCMonth() !== Number(match[2]) - 1 ||
      date.getUTCDate() !== Number(match[3])
    )
      throw new Error(`Transition ${index + 1}: comparison value must be a valid date.`);
  }
  if (variable.dataType === 'object') {
    try {
      const parsed = JSON.parse(value) as unknown;
      if (parsed == null || typeof parsed !== 'object') throw new Error();
    } catch {
      throw new Error(`Transition ${index + 1}: comparison value must be a valid JSON object.`);
    }
  }
};

const toBuilderVariable = (
  variable: Awaited<ReturnType<typeof wfVariableApi.list>>[number]
): BuilderVariable => ({
  id: String(variable.recId),
  code: variable.code ?? '',
  name: variable.name ?? '',
  description: variable.description ?? '',
  dataType: dataType(variable.dataTypeId),
  sortOrder: variable.sortOrder,
  required: false,
  active: variable.isActive,
  scope: 'process',
  defaultValue: '',
});

export interface ProcessCodeMetadata {
  mode: 'automatic' | 'manual';
  manual: boolean;
  available: boolean;
  previewCode: string | null;
  message: string | null;
}

async function getCodeMetadata(entity: string, signal?: AbortSignal): Promise<ProcessCodeMetadata> {
  const response = await apiClient.get<ApiResponse<ProcessCodeMetadata>>(
    `/v1/${entity}/number-sequence`,
    { signal }
  );
  if (!response.data.success || !response.data.data)
    throw new Error(response.data.message || 'Workflow process number sequence is unavailable.');
  if (!response.data.data.available)
    throw new Error(
      response.data.data.message || 'Workflow process number sequence is unavailable.'
    );
  return response.data.data;
}

export const getProcessCodeMetadata = (signal?: AbortSignal) =>
  getCodeMetadata('WfProcess', signal);
export const getVariableCodeMetadata = (signal?: AbortSignal) =>
  getCodeMetadata('WfVariable', signal);
export const getStepCodeMetadata = (signal?: AbortSignal) => getCodeMetadata('WfStep', signal);
export const getActivityCodeMetadata = (signal?: AbortSignal) =>
  getCodeMetadata('WfActivity', signal);
export const getRequestControlCodeMetadata = (signal?: AbortSignal) =>
  getCodeMetadata('WfRequestControl', signal);

export async function loadProcessBuilder(processId: number): Promise<ProcessBuilderDocument> {
  const [
    process,
    variables,
    steps,
    activities,
    activityTypes,
    activityControls,
    activityValidations,
    activityOptions,
    requestControls,
    controlTypes,
    requestValidations,
    requestOptions,
    transitions,
    operators,
  ] = await Promise.all([
    wfProcessApi.getById(processId),
    wfVariableApi.list(),
    wfStepApi.list(),
    wfActivityApi.list(),
    wfActivityTypeApi.list(),
    wfActivityControlApi.list(),
    wfActivityControlValidationApi.list(),
    wfActivityControlOptionApi.list(),
    wfRequestControlApi.list(),
    wfControlApi.list(),
    wfRequestControlValidationApi.list(),
    wfRequestControlOptionApi.list(),
    wfTransitionApi.list(),
    wfOperatorApi.list(),
  ]);
  const processSteps = steps
    .filter((step) => step.processId === processId)
    .sort((a, b) => a.sortOrder - b.sortOrder);
  const builderSteps: BuilderStep[] = processSteps.map((step) => ({
    id: String(step.recId),
    code: step.code ?? '',
    name: step.name ?? '',
    order: step.sortOrder,
    score: step.score,
    autoPassingHours: step.autoPassingHrs,
    allMandatory: step.allMandatory,
    active: step.isActive,
    systemField: step.sysField,
    condition: null,
    activities: activities
      .filter((activity) => activity.stepId === step.recId)
      .sort((a, b) => a.sortOrder - b.sortOrder)
      .map<BuilderActivity>((activity) => ({
        id: String(activity.recId),
        code: activity.code ?? '',
        name: activity.name ?? '',
        type: builderActivityType(
          activityTypes.find((type) => type.recId === activity.activityTypeId)?.name ?? ''
        ),
        activityTypeId: String(activity.activityTypeId || ''),
        performer: String(activity.performerId || ''),
        score: activity.score,
        sortOrder: activity.sortOrder,
        assignmentMode: 'any',
        active: activity.isActive,
        required: true,
        mandatoryDocs: activity.mandatoryDocs,
        autoPassEnabled: activity.autoPassEnabled,
        autoPassingHours: activity.autoPassingHrs,
        controls: activityControls
          .filter((control) => control.activityId === activity.recId)
          .sort((a, b) => a.sortOrder - b.sortOrder)
          .map<BuilderControl>((control, controlIndex) => {
            const properties = parseObject(control.extendedProperties);
            return {
              id: String(control.recId),
              code: control.code ?? '',
              label: control.name ?? '',
              labelAR: typeof properties.labelAR === 'string' ? properties.labelAR : '',
              labelColor:
                typeof properties.labelColor === 'string' ? properties.labelColor : '#7a4b00',
              type: (() => {
                const item = controlTypes.find(
                  (candidate) => candidate.recId === control.controlId
                );
                return builderControlType(
                  `${item?.code ?? ''} ${item?.name ?? ''} ${item?.controlType ?? ''}`
                );
              })(),
              controlId: String(control.controlId || ''),
              sortOrder: controlIndex + 1,
              columnSpan: ([1, 2, 3].includes(Number(properties.columnSpan))
                ? Number(properties.columnSpan)
                : 1) as 1 | 2 | 3,
              score: control.score,
              required: Boolean(properties.required ?? control.mandatory),
              readOnly: Boolean(properties.readOnly),
              visible: properties.visible !== false,
              uniqueKey: Boolean(properties.uniqueKey ?? control.uniqueKey),
              usedAsCriteria: Boolean(properties.usedAsCriteria ?? control.usedAsCriteria),
              defaultValue:
                typeof properties.defaultValue === 'string' ? properties.defaultValue : '',
              options: activityOptions
                .filter((option) => option.activityControlId === control.recId && option.isActive)
                .sort((a, b) => a.sortOrder - b.sortOrder)
                .map((option) => option.name || option.value),
              validations: activityValidations
                .filter((validation) => validation.activityControlId === control.recId)
                .sort((a, b) => a.sortOrder - b.sortOrder)
                .map((validation) => ({
                  id: String(validation.recId),
                  type: builderValidationType(validation.validationType),
                  value: validation.value ?? '',
                  secondaryValue: validation.validationExpression ?? '',
                  operator: validation.operator ?? '',
                  mask: validation.maskInput ?? '',
                  message: validation.errorMessage,
                  severity:
                    validation.severity as BuilderControl['validations'][number]['severity'],
                  sortOrder: validation.sortOrder,
                  active: validation.isActive,
                })),
              visibilityCondition: null,
            };
          }),
        actions: [],
        validations: [],
        condition: null,
        config: { apiMethod: 'GET', apiUrl: '', notifyEmails: '' },
      })),
  }));
  const builderVariables: BuilderVariable[] = variables
    .filter((variable) => variable.processId === processId)
    .sort((a, b) => a.sortOrder - b.sortOrder)
    .map(toBuilderVariable);
  const builderRequestControls: BuilderControl[] = requestControls
    .filter((control) => control.processId === processId)
    .sort((a, b) => a.sortOrder - b.sortOrder)
    .map((control, controlIndex) => {
      const properties = parseObject(control.extendedProperties);
      const controlOptions = requestOptions
        .filter((option) => option.requestControlId === control.recId && option.isActive)
        .sort((a, b) => a.sortOrder - b.sortOrder);
      const optionFeatures = Array.isArray(properties.optionFeatureConfigurations)
        ? properties.optionFeatureConfigurations
        : [];
      const validations = requestValidations
        .filter((validation) => validation.requestControlId === control.recId)
        .sort((a, b) => a.sortOrder - b.sortOrder)
        .map<BuilderControl['validations'][number]>((validation) => ({
          id: String(validation.recId),
          type: builderValidationType(validation.validationType),
          value: validation.value ?? '',
          secondaryValue: validation.validationExpression ?? '',
          operator: validation.operator ?? '',
          mask: validation.maskInput ?? '',
          message: validation.errorMessage,
          severity: validation.severity as BuilderControl['validations'][number]['severity'],
          sortOrder: validation.sortOrder,
          active: validation.isActive,
        }));
      return {
        id: String(control.recId),
        code: control.code ?? '',
        label: control.name ?? '',
        labelAR: typeof properties.labelAR === 'string' ? properties.labelAR : '',
        labelColor: typeof properties.labelColor === 'string' ? properties.labelColor : '#7a4b00',
        type: (() => {
          const item = controlTypes.find((candidate) => candidate.recId === control.controlId);
          return builderControlType(
            `${item?.code ?? ''} ${item?.name ?? ''} ${item?.controlType ?? ''}`
          );
        })(),
        controlId: String(control.controlId || ''),
        sortOrder: controlIndex + 1,
        columnSpan: ([1, 2, 3].includes(Number(properties.columnSpan))
          ? Number(properties.columnSpan)
          : 1) as 1 | 2 | 3,
        score: control.score,
        required: Boolean(properties.required ?? control.mandatory),
        readOnly: Boolean(properties.readOnly),
        visible: properties.visible !== false,
        uniqueKey: Boolean(properties.uniqueKey ?? control.uniqueKey),
        usedAsCriteria: Boolean(properties.usedAsCriteria ?? control.usedAsCriteria),
        defaultValue: typeof properties.defaultValue === 'string' ? properties.defaultValue : '',
        options: controlOptions.map((option) => option.name || option.value),
        optionScores: controlOptions.map((option) => option.score ?? 0),
        optionFeatureConfigurations: controlOptions.map((option, index) =>
          normalizeOptionFeatureConfiguration(
            Object.keys(parseObject(option.extendedProperties)).length > 0
              ? parseObject(option.extendedProperties)
              : optionFeatures[index]
          )
        ),
        validations,
        visibilityCondition: (() => {
          const condition = properties.visibilityCondition;
          if (!condition || typeof condition !== 'object') return null;
          const item = condition as Record<string, unknown>;
          const sourceControlId = Number(item.sourceControlId);
          return sourceControlId > 0
            ? {
                variableId: String(sourceControlId),
                operator: builderOperator(typeof item.operator === 'string' ? item.operator : '='),
                value: typeof item.value === 'string' ? item.value : '',
              }
            : null;
        })(),
      };
    });
  const builderTransitions: BuilderTransition[] = transitions
    .filter((transition) => transition.processId === processId)
    .sort((a, b) => a.sortOrder - b.sortOrder)
    .map((transition) => {
      const triggerActivity =
        transition.activityId == null
          ? null
          : activities.find((activity) => activity.recId === transition.activityId);
      const operator = operators.find((item) => item.recId === transition.operatorId);
      return {
        id: String(transition.recId),
        name: `Transition ${transition.recId}`,
        sourceStepId: triggerActivity ? String(triggerActivity.stepId) : '',
        targetStepId: String(transition.stepId),
        variableId: String(transition.variableId),
        operator: builderOperator(operator?.name ?? operator?.code ?? ''),
        operatorId: String(transition.operatorId),
        value: transition.value,
        sortOrder: transition.sortOrder,
        active: transition.isActive,
        triggerSource:
          transition.activityId != null
            ? 'activity'
            : transition.requestControlId != null
              ? 'requestControl'
              : 'none',
        triggerId: String(transition.activityId ?? transition.requestControlId ?? ''),
      };
    });

  return {
    id: String(process.recId),
    code: process.code ?? '',
    name: process.name ?? '',
    description: process.description ?? '',
    categoryId: String(process.categoryId || ''),
    priorityId: String(process.priorityId || ''),
    processType: String(process.processTypeId || ''),
    score: process.score,
    canRepeat: process.canRepeat,
    mandatoryDocs: process.mandatoryDocs,
    active: process.isActive,
    variables: builderVariables,
    requestControls: builderRequestControls,
    steps: builderSteps,
    transitions: builderTransitions,
  };
}

export async function saveProcessBuilder(
  document: ProcessBuilderDocument
): Promise<ProcessBuilderDocument> {
  const categoryId = Number(document.categoryId);
  const priorityId = Number(document.priorityId);
  const processTypeId = Number(document.processType);
  if (!document.name.trim()) throw new Error('Process name is required.');
  if (categoryId <= 0) throw new Error('Category is required.');
  if (priorityId <= 0) throw new Error('Priority is required.');
  if (processTypeId <= 0) throw new Error('Process type is required.');

  const existing = document.id !== 'new' ? await wfProcessApi.getById(Number(document.id)) : null;
  const record: WfProcessRecord = {
    ...(existing ?? {
      id: `new-${crypto.randomUUID()}`,
      recId: 0,
      code: null,
      usersProcesses: [],
      rowVersion: null,
      recVersion: 1,
      dataAreaId: 'dat',
      sysField: false,
      sortOrder: 0,
    }),
    name: document.name.trim(),
    description: document.description.trim() || null,
    categoryId,
    priorityId,
    processTypeId,
    score: document.score,
    canRepeat: document.canRepeat,
    mandatoryDocs: document.mandatoryDocs,
    isActive: document.active,
  };
  const persisted = existing
    ? await wfProcessApi.update(record)
    : await wfProcessApi.create({ ...record, code: null });
  const [serverSteps, activityTypes, stepCodeMetadata] = await Promise.all([
    wfStepApi.list(),
    wfActivityTypeApi.list(),
    getStepCodeMetadata(),
  ]);
  const variableResult = await saveProcessVariables({ ...document, id: String(persisted.recId) });
  const requestControlResult = await saveProcessRequestControls({
    ...document,
    id: String(persisted.recId),
  });

  const stepIds = new Map<string, number>();
  for (const step of document.steps) {
    const id = numericId(step.id);
    const current = id == null ? null : serverSteps.find((item) => item.recId === id);
    const stepRecord = {
      ...(current ?? {
        id: `new-${crypto.randomUUID()}`,
        recId: 0,
        code: null,
        description: null,
        rowVersion: null,
        recVersion: 1,
        dataAreaId: persisted.dataAreaId,
      }),
      code: current?.code ?? (stepCodeMetadata.manual ? step.code.trim() || null : null),
      name: step.name,
      processId: persisted.recId,
      sortOrder: step.order,
      score: step.score,
      autoPassingHrs: step.autoPassingHours,
      allMandatory: step.allMandatory,
      sysField: step.systemField,
      isActive: step.active,
    };
    const savedStep = current
      ? await wfStepApi.update(stepRecord)
      : await wfStepApi.create(stepRecord);
    stepIds.set(step.id, savedStep.recId);
  }

  const [serverActivities, activityCodeMetadata] = await Promise.all([
    wfActivityApi.list(),
    getActivityCodeMetadata(),
  ]);
  const activityIds = new Map<string, number>();
  for (const step of document.steps) {
    const stepId = stepIds.get(step.id);
    if (!stepId) continue;
    for (const activity of step.activities) {
      const id = numericId(activity.id);
      const current = id == null ? null : serverActivities.find((item) => item.recId === id);
      const activityType = resolveActivityType(activityTypes, activity);
      const performerId = Number(activity.performer);
      if (!activityType) throw new Error(`No backend activity type matches '${activity.type}'.`);
      if (performerId <= 0)
        throw new Error(`Performer is required for activity '${activity.name}'.`);
      const activityRecord = {
        ...(current ?? {
          id: `new-${crypto.randomUUID()}`,
          recId: 0,
          code: null,
          description: null,
          rowVersion: null,
          recVersion: 1,
          dataAreaId: persisted.dataAreaId,
          sysNotificationTemplateId: null,
          alertingBySystem: false,
          alertingByEmail: false,
          alertingBySms: false,
          alertingByWhatsApp: false,
          showPreviousSteps: false,
          showPreviousDocs: false,
          extendedProperties: null,
          sortOrder: 0,
        }),
        code: current?.code ?? (activityCodeMetadata.manual ? activity.code.trim() || null : null),
        name: activity.name,
        stepId,
        activityTypeId: activityType.recId,
        performerId,
        score: activity.score,
        sortOrder: activity.sortOrder,
        mandatoryDocs: activity.mandatoryDocs,
        autoPassEnabled: activity.autoPassEnabled,
        autoPassingHrs: activity.autoPassingHours,
        isActive: activity.active,
      };
      const savedActivity = current
        ? await wfActivityApi.update(activityRecord)
        : await wfActivityApi.create(activityRecord);
      activityIds.set(activity.id, savedActivity.recId);
    }
  }

  await syncActivityControls(
    { ...document, id: String(persisted.recId) },
    persisted.recId,
    activityIds
  );

  const persistedTransitionsDocument: ProcessBuilderDocument = {
    ...document,
    id: String(persisted.recId),
    variables: variableResult.variables,
    requestControls: requestControlResult.controls,
    steps: document.steps.map((step) => ({
      ...step,
      id: String(stepIds.get(step.id) ?? step.id),
      activities: step.activities.map((activity) => ({
        ...activity,
        id: String(activityIds.get(activity.id) ?? activity.id),
      })),
    })),
    transitions: document.transitions.map((transition) => ({
      ...transition,
      sourceStepId: String(stepIds.get(transition.sourceStepId) ?? transition.sourceStepId),
      targetStepId: String(stepIds.get(transition.targetStepId) ?? transition.targetStepId),
      variableId: variableResult.variableIds[transition.variableId] ?? transition.variableId,
      triggerId:
        transition.triggerSource === 'requestControl'
          ? (requestControlResult.controlIds[transition.triggerId] ?? transition.triggerId)
          : transition.triggerSource === 'activity'
            ? String(activityIds.get(transition.triggerId) ?? transition.triggerId)
            : '',
    })),
  };
  await saveProcessTransitions(persistedTransitionsDocument);

  return loadProcessBuilder(persisted.recId);
}

export interface SaveProcessVariablesResult {
  variables: BuilderVariable[];
  variableIds: Record<string, string>;
}

export async function saveProcessVariables(
  document: ProcessBuilderDocument
): Promise<SaveProcessVariablesResult> {
  const processId = Number(document.id);
  if (!Number.isInteger(processId) || processId <= 0)
    throw new Error('Save the process before saving variables.');
  validateVariables(document.variables);

  const [process, allVariables, codeMetadata] = await Promise.all([
    wfProcessApi.getById(processId),
    wfVariableApi.list(),
    getVariableCodeMetadata(),
  ]);
  const serverVariables = allVariables.filter((item) => item.processId === processId);
  const retainedIds = new Set(
    document.variables
      .map((variable) => numericId(variable.id))
      .filter((id): id is number => id != null)
  );

  for (const variable of serverVariables) {
    if (!retainedIds.has(variable.recId)) await wfVariableApi.delete(variable);
  }

  const savedVariableIds = new Map<string, number>();
  for (const variable of [...document.variables].sort((a, b) => a.sortOrder - b.sortOrder)) {
    const id = numericId(variable.id);
    const current = id == null ? null : serverVariables.find((item) => item.recId === id);
    const variableRecord = {
      ...(current ?? {
        id: `new-${crypto.randomUUID()}`,
        recId: 0,
        code: null,
        rowVersion: null,
        recVersion: 1,
        dataAreaId: process.dataAreaId,
        dataType: null,
        process: null,
      }),
      // Automatic sequences allocate the authoritative code during create.
      code: current?.code ?? (codeMetadata.manual ? variable.code.trim() || null : null),
      name: variable.name.trim(),
      description: variable.description.trim() || null,
      processId,
      dataTypeId: dataTypeId(variable.dataType),
      sortOrder: variable.sortOrder,
      isActive: variable.active,
    };
    const saved = current
      ? await wfVariableApi.update(variableRecord)
      : await wfVariableApi.create(variableRecord);
    savedVariableIds.set(variable.id, saved.recId);
  }

  const variables = (await wfVariableApi.list())
    .filter((variable) => variable.processId === processId)
    .sort((a, b) => a.sortOrder - b.sortOrder)
    .map(toBuilderVariable);
  return {
    variables,
    variableIds: Object.fromEntries(
      [...savedVariableIds].map(([localId, persistedId]) => [localId, String(persistedId)])
    ),
  };
}

export interface SaveProcessStepsResult {
  steps: BuilderStep[];
  stepIds: Record<string, string>;
}

export async function saveProcessSteps(
  document: ProcessBuilderDocument
): Promise<SaveProcessStepsResult> {
  const processId = Number(document.id);
  if (!Number.isInteger(processId) || processId <= 0)
    throw new Error('Save the process before saving steps.');

  const names = new Set<string>();
  for (const [index, step] of document.steps.entries()) {
    const name = step.name.trim();
    if (!name) throw new Error(`Step ${index + 1}: name is required.`);
    const normalizedName = name.toLocaleLowerCase();
    if (names.has(normalizedName)) throw new Error(`Step name '${name}' is duplicated.`);
    names.add(normalizedName);
    if (!Number.isInteger(step.order) || step.order < 0 || step.order > 255)
      throw new Error(`Step '${name}': order must be a whole number from 0 to 255.`);
  }

  const [process, allSteps, codeMetadata] = await Promise.all([
    wfProcessApi.getById(processId),
    wfStepApi.list(),
    getStepCodeMetadata(),
  ]);
  const serverSteps = allSteps.filter((step) => step.processId === processId);
  const retainedIds = new Set(
    document.steps.map((step) => numericId(step.id)).filter((id): id is number => id != null)
  );

  for (const step of serverSteps) {
    if (!retainedIds.has(step.recId)) await wfStepApi.delete(step);
  }

  const savedStepIds = new Map<string, number>();
  for (const step of [...document.steps].sort((a, b) => a.order - b.order)) {
    const id = numericId(step.id);
    const current = id == null ? null : serverSteps.find((item) => item.recId === id);
    const record = {
      ...(current ?? {
        id: `new-${crypto.randomUUID()}`,
        recId: 0,
        code: null,
        description: null,
        rowVersion: null,
        recVersion: 1,
        dataAreaId: process.dataAreaId,
      }),
      code: current?.code ?? (codeMetadata.manual ? step.code.trim() || null : null),
      name: step.name.trim(),
      processId,
      sortOrder: step.order,
      score: step.score,
      autoPassingHrs: step.autoPassingHours,
      allMandatory: step.allMandatory,
      sysField: step.systemField,
      isActive: step.active,
    };
    const saved = current ? await wfStepApi.update(record) : await wfStepApi.create(record);
    savedStepIds.set(step.id, saved.recId);
  }

  const reloaded = await loadProcessBuilder(processId);
  return {
    steps: reloaded.steps,
    stepIds: Object.fromEntries(
      [...savedStepIds].map(([localId, persistedId]) => [localId, String(persistedId)])
    ),
  };
}

async function syncActivityControls(
  document: ProcessBuilderDocument,
  processId: number,
  activityIds: Map<string, number>
): Promise<void> {
  const [process, serverControls, controlTypes, serverValidations, serverOptions] =
    await Promise.all([
      wfProcessApi.getById(processId),
      wfActivityControlApi.list(),
      wfControlApi.list(),
      wfActivityControlValidationApi.list(),
      wfActivityControlOptionApi.list(),
    ]);
  const persistedActivityIds = new Set(activityIds.values());
  const processControls = serverControls.filter((control) =>
    persistedActivityIds.has(control.activityId)
  );
  const controls = document.steps.flatMap((step) =>
    step.activities.flatMap((activity) =>
      activity.controls.map((control, controlIndex) => ({
        activity,
        control: { ...control, sortOrder: controlIndex + 1 },
      }))
    )
  );
  const retainedIds = new Set(
    controls.map(({ control }) => numericId(control.id)).filter((id): id is number => id != null)
  );

  const resolved = controls.map(({ activity, control }, index) => {
    const activityId = activityIds.get(activity.id);
    if (!activityId) throw new Error(`Save activity '${activity.name}' before saving its form.`);
    if (!control.label.trim()) throw new Error(`Activity control ${index + 1}: label is required.`);
    if (!Number.isInteger(control.sortOrder) || control.sortOrder < 0 || control.sortOrder > 255)
      throw new Error(`Activity control '${control.label}': sort order must be from 0 to 255.`);
    const controlType =
      controlTypes.find((item) => item.recId === Number(control.controlId)) ??
      controlTypes.find(
        (item) =>
          builderControlType(`${item.controlType ?? ''} ${item.name ?? ''}`) === control.type
      );
    if (!controlType) throw new Error(`No backend WfControl matches '${control.type}'.`);
    if (optionControlTypes.has(control.type)) {
      const values = control.options.map((option) => option.trim()).filter(Boolean);
      if (new Set(values.map((option) => option.toLocaleLowerCase())).size !== values.length)
        throw new Error(`Activity control '${control.label}': option names must be unique.`);
    }
    for (const rule of control.validations) {
      if (!rule.type) throw new Error(`Validation type is required for '${control.label}'.`);
      if (validationUsesCustomMessage(rule.type) && !rule.message.trim())
        throw new Error(`Error message is required for '${control.label}'.`);
    }
    return { activityId, control, controlType };
  });

  for (const control of processControls) {
    if (!retainedIds.has(control.recId)) await wfActivityControlApi.delete(control);
  }

  const savedControlIds = new Map<string, number>();
  for (const { activityId, control, controlType } of resolved) {
    const id = numericId(control.id);
    const current = id == null ? null : processControls.find((item) => item.recId === id);
    const record = {
      ...(current ?? {
        id: `new-${crypto.randomUUID()}`,
        recId: 0,
        code: control.code.trim() || null,
        description: null,
        rowVersion: null,
        recVersion: 1,
        dataAreaId: process.dataAreaId,
        mandatory: false,
        uniqueKey: false,
        usedAsCriteria: false,
        usedInSearch: false,
      }),
      name: control.label.trim(),
      activityId,
      processId,
      controlId: controlType.recId,
      mandatory: control.required,
      uniqueKey: control.uniqueKey,
      score: control.score,
      usedAsCriteria: control.usedAsCriteria,
      usedInSearch: false,
      sortOrder: control.sortOrder,
      validationRules: JSON.stringify({ validations: control.validations }),
      extendedProperties: JSON.stringify({
        labelAR: control.labelAR,
        labelColor: control.labelColor,
        required: control.required,
        readOnly: control.readOnly,
        visible: control.visible,
        uniqueKey: control.uniqueKey,
        usedAsCriteria: control.usedAsCriteria,
        defaultValue: control.defaultValue,
        columnSpan: control.columnSpan ?? 1,
        optionFeatureConfigurations: control.optionFeatureConfigurations ?? [],
      }),
      isActive: control.visible,
    };
    const saved = current
      ? await wfActivityControlApi.update(record)
      : await wfActivityControlApi.create(record);
    savedControlIds.set(control.id, saved.recId);
  }

  const retainedValidationIds = new Set(
    controls
      .flatMap(({ control }) => control.validations)
      .map((rule) => numericId(rule.id))
      .filter((id): id is number => id != null)
  );
  const retainedControlIds = new Set(savedControlIds.values());
  for (const validation of serverValidations) {
    if (
      retainedControlIds.has(validation.activityControlId) &&
      !retainedValidationIds.has(validation.recId)
    )
      await wfActivityControlValidationApi.delete(validation);
  }
  for (const { control } of controls) {
    const activityControlId = savedControlIds.get(control.id);
    if (!activityControlId) continue;
    for (const [ruleIndex, rule] of control.validations.entries()) {
      const id = numericId(rule.id);
      const current = id == null ? null : serverValidations.find((item) => item.recId === id);
      const record = {
        ...(current ?? {
          id: `new-${crypto.randomUUID()}`,
          recId: 0,
          rowVersion: null,
          recVersion: 1,
          dataAreaId: process.dataAreaId,
        }),
        activityControlId,
        validationType: rule.type,
        validationExpression: rule.secondaryValue.trim() || null,
        operator: rule.operator.trim() || null,
        value: rule.value.trim() || null,
        maskInput: rule.mask.trim() || null,
        errorMessage: resolvedValidationMessage(rule),
        severity: rule.severity,
        sortOrder: (ruleIndex + 1) * 10,
        isActive: rule.active,
      };
      if (current) await wfActivityControlValidationApi.update(record);
      else await wfActivityControlValidationApi.create(record);
    }
  }

  const existingOptionsByControl = new Map<number, typeof serverOptions>();
  for (const option of serverOptions) {
    const options = existingOptionsByControl.get(option.activityControlId) ?? [];
    options.push(option);
    existingOptionsByControl.set(option.activityControlId, options);
  }
  for (const { control } of controls) {
    const activityControlId = savedControlIds.get(control.id);
    if (!activityControlId) continue;
    const existing = (existingOptionsByControl.get(activityControlId) ?? []).sort(
      (a, b) => a.sortOrder - b.sortOrder
    );
    const values = optionControlTypes.has(control.type)
      ? control.options.map((option) => option.trim()).filter(Boolean)
      : [];
    for (const stale of existing.slice(values.length))
      await wfActivityControlOptionApi.delete(stale);
    for (const [index, value] of values.entries()) {
      const current = existing[index];
      const record = {
        ...(current ?? {
          id: `new-${crypto.randomUUID()}`,
          recId: 0,
          rowVersion: null,
          recVersion: 1,
          dataAreaId: process.dataAreaId,
        }),
        activityControlId,
        value,
        name: value,
        sortOrder: (index + 1) * 10,
        isActive: true,
      };
      if (current) await wfActivityControlOptionApi.update(record);
      else await wfActivityControlOptionApi.create(record);
    }
  }
}

export interface SaveProcessActivitiesResult {
  document: ProcessBuilderDocument;
  activityIds: Record<string, string>;
}

export async function saveProcessActivities(
  document: ProcessBuilderDocument
): Promise<SaveProcessActivitiesResult> {
  const processId = Number(document.id);
  if (!Number.isInteger(processId) || processId <= 0)
    throw new Error('Save the process before saving activities.');
  const unsavedStep = document.steps.find((step) => numericId(step.id) == null);
  if (unsavedStep) throw new Error(`Save step '${unsavedStep.name}' before adding its activities.`);

  const [process, serverActivities, serverControls, activityTypes, codeMetadata] =
    await Promise.all([
      wfProcessApi.getById(processId),
      wfActivityApi.list(),
      wfActivityControlApi.list(),
      wfActivityTypeApi.list(),
      getActivityCodeMetadata(),
    ]);
  const stepIds = new Set(document.steps.map((step) => Number(step.id)));
  const processActivities = serverActivities.filter((activity) => stepIds.has(activity.stepId));
  const retainedIds = new Set(
    document.steps
      .flatMap((step) => step.activities)
      .map((activity) => numericId(activity.id))
      .filter((id): id is number => id != null)
  );

  for (const activity of document.steps.flatMap((step) => step.activities)) {
    if (!activity.name.trim()) throw new Error('Activity name is required.');
    const activityType = resolveActivityType(activityTypes, activity);
    if (!activityType) throw new Error(`No backend activity type matches '${activity.type}'.`);
    if (Number(activity.performer) <= 0)
      throw new Error(`Performer is required for activity '${activity.name}'.`);
  }

  for (const activity of processActivities) {
    if (!retainedIds.has(activity.recId)) {
      for (const control of serverControls.filter((item) => item.activityId === activity.recId))
        await wfActivityControlApi.delete(control);
      await wfActivityApi.delete(activity);
    }
  }

  const activityIds = new Map<string, number>();
  for (const step of document.steps) {
    for (const activity of step.activities) {
      const activityType = resolveActivityType(activityTypes, activity);
      if (!activityType) continue; // Validated before mutations.
      const performerId = Number(activity.performer);
      const id = numericId(activity.id);
      const current = id == null ? null : processActivities.find((item) => item.recId === id);
      const record = {
        ...(current ?? {
          id: `new-${crypto.randomUUID()}`,
          recId: 0,
          code: null,
          description: null,
          rowVersion: null,
          recVersion: 1,
          dataAreaId: process.dataAreaId,
          sysNotificationTemplateId: null,
          alertingBySystem: false,
          alertingByEmail: false,
          alertingBySms: false,
          alertingByWhatsApp: false,
          showPreviousSteps: false,
          showPreviousDocs: false,
          extendedProperties: null,
          sortOrder: 0,
        }),
        code: current?.code ?? (codeMetadata.manual ? activity.code.trim() || null : null),
        name: activity.name.trim(),
        stepId: Number(step.id),
        activityTypeId: activityType.recId,
        performerId,
        score: activity.score,
        sortOrder: activity.sortOrder,
        mandatoryDocs: activity.mandatoryDocs,
        autoPassEnabled: activity.autoPassEnabled,
        autoPassingHrs: activity.autoPassingHours,
        isActive: activity.active,
      };
      const saved = current
        ? await wfActivityApi.update(record)
        : await wfActivityApi.create(record);
      activityIds.set(activity.id, saved.recId);
    }
  }

  await syncActivityControls(document, processId, activityIds);

  return {
    document: await loadProcessBuilder(processId),
    activityIds: Object.fromEntries(
      [...activityIds].map(([localId, persistedId]) => [localId, String(persistedId)])
    ),
  };
}

export interface SaveProcessRequestControlsResult {
  controls: BuilderControl[];
  controlIds: Record<string, string>;
}

export async function saveProcessRequestControls(
  document: ProcessBuilderDocument
): Promise<SaveProcessRequestControlsResult> {
  const processId = Number(document.id);
  if (!Number.isInteger(processId) || processId <= 0)
    throw new Error('Save the process before saving request controls.');
  const [process, serverControls, controlTypes, codeMetadata, serverValidations, serverOptions] =
    await Promise.all([
      wfProcessApi.getById(processId),
      wfRequestControlApi.list(),
      wfControlApi.list(),
      getRequestControlCodeMetadata(),
      wfRequestControlValidationApi.list(),
      wfRequestControlOptionApi.list(),
    ]);
  const processControls = serverControls.filter((control) => control.processId === processId);
  const requestControls = document.requestControls.map((control, index) => ({
    ...control,
    sortOrder: index + 1,
  }));
  const retainedIds = new Set(
    requestControls.map((control) => numericId(control.id)).filter((id): id is number => id != null)
  );
  const resolved = requestControls.map((control, index) => {
    if (!control.label.trim()) throw new Error(`Request control ${index + 1}: label is required.`);
    if (!Number.isInteger(control.sortOrder) || control.sortOrder < 0 || control.sortOrder > 255)
      throw new Error(`Request control '${control.label}': sort order must be from 0 to 255.`);
    const controlType =
      controlTypes.find((item) => item.recId === Number(control.controlId)) ??
      controlTypes.find(
        (item) =>
          builderControlType(`${item.controlType ?? ''} ${item.name ?? ''}`) === control.type
      );
    if (!controlType) throw new Error(`No backend WfControl matches '${control.type}'.`);
    if (optionControlTypes.has(control.type)) {
      const normalizedOptions = control.options.map((option) => option.trim()).filter(Boolean);
      if (
        new Set(normalizedOptions.map((option) => option.toLocaleLowerCase())).size !==
        normalizedOptions.length
      )
        throw new Error(`Request control '${control.label}': option names must be unique.`);
    }
    for (const rule of control.validations) {
      if (!rule.type) throw new Error(`Validation type is required for '${control.label}'.`);
      if (validationUsesCustomMessage(rule.type) && !rule.message.trim())
        throw new Error(`Error message is required for '${control.label}'.`);
    }
    return { control, controlType };
  });

  for (const control of processControls) {
    if (!retainedIds.has(control.recId)) await wfRequestControlApi.delete(control);
  }
  const savedControlIds = new Map<string, number>();
  const savedControlRecords = new Map<number, WfRequestControlRecord>();
  for (const { control, controlType } of resolved) {
    const id = numericId(control.id);
    const current = id == null ? null : processControls.find((item) => item.recId === id);
    const record = {
      ...(current ?? {
        id: `new-${crypto.randomUUID()}`,
        recId: 0,
        code: null,
        description: null,
        rowVersion: null,
        recVersion: 1,
        dataAreaId: process.dataAreaId,
        mandatory: false,
        uniqueKey: false,
        usedAsCriteria: false,
      }),
      code: current?.code ?? (codeMetadata.manual ? control.code.trim() || null : null),
      name: control.label.trim(),
      processId,
      controlId: controlType.recId,
      score: control.score,
      sortOrder: control.sortOrder,
      mandatory: control.required,
      uniqueKey: control.uniqueKey,
      usedAsCriteria: control.usedAsCriteria,
      validationRules: JSON.stringify({ validations: control.validations }),
      extendedProperties: JSON.stringify({
        labelAR: control.labelAR,
        labelColor: control.labelColor,
        required: control.required,
        readOnly: control.readOnly,
        visible: control.visible,
        uniqueKey: control.uniqueKey,
        usedAsCriteria: control.usedAsCriteria,
        defaultValue: control.defaultValue,
        columnSpan: control.columnSpan ?? 1,
      }),
      isActive: control.visible,
    };
    const saved = current
      ? await wfRequestControlApi.update(record)
      : await wfRequestControlApi.create(record);
    savedControlIds.set(control.id, saved.recId);
    savedControlRecords.set(saved.recId, saved);
  }

  const retainedValidationIds = new Set(
    requestControls
      .flatMap((control) => control.validations)
      .map((rule) => numericId(rule.id))
      .filter((id): id is number => id != null)
  );
  const retainedControlIds = new Set([...savedControlIds.values()]);
  for (const validation of serverValidations) {
    if (
      retainedControlIds.has(validation.requestControlId) &&
      !retainedValidationIds.has(validation.recId)
    )
      await wfRequestControlValidationApi.delete(validation);
  }
  for (const control of requestControls) {
    const requestControlId = savedControlIds.get(control.id);
    if (!requestControlId) continue;
    for (const [ruleIndex, rule] of control.validations.entries()) {
      const id = numericId(rule.id);
      const current = id == null ? null : serverValidations.find((item) => item.recId === id);
      const record = {
        ...(current ?? {
          id: `new-${crypto.randomUUID()}`,
          recId: 0,
          rowVersion: null,
          recVersion: 1,
          dataAreaId: process.dataAreaId,
        }),
        requestControlId,
        validationType: rule.type,
        validationExpression: rule.secondaryValue.trim() || null,
        operator: rule.operator.trim() || null,
        value: rule.value.trim() || null,
        maskInput: rule.mask.trim() || null,
        errorMessage: resolvedValidationMessage(rule),
        severity: rule.severity,
        sortOrder: (ruleIndex + 1) * 10,
        isActive: rule.active,
      };
      if (current) await wfRequestControlValidationApi.update(record);
      else await wfRequestControlValidationApi.create(record);
    }
  }
  const existingOptionsByControl = new Map<number, typeof serverOptions>();
  for (const option of serverOptions) {
    const options = existingOptionsByControl.get(option.requestControlId) ?? [];
    options.push(option);
    existingOptionsByControl.set(option.requestControlId, options);
  }
  for (const control of requestControls) {
    const requestControlId = savedControlIds.get(control.id);
    if (!requestControlId) continue;
    const existing = (existingOptionsByControl.get(requestControlId) ?? []).sort(
      (a, b) => a.sortOrder - b.sortOrder
    );
    const entries = optionControlTypes.has(control.type)
      ? control.options
          .map((option, index) => {
            const features = normalizeOptionFeatureConfiguration(
              control.optionFeatureConfigurations?.[index]
            );
            const resolveControlId = (value: string) => savedControlIds.get(value) ?? Number(value);
            return {
              value: option.trim(),
              score: control.optionScores?.[index] ?? 0,
              extendedProperties: JSON.stringify({
                ...features,
                visibleControlIds: features.visibleControlIds
                  .map(resolveControlId)
                  .filter((value) => value > 0)
                  .map(String),
              }),
            };
          })
          .filter((entry) => Boolean(entry.value))
      : [];
    for (const stale of existing.slice(entries.length))
      await wfRequestControlOptionApi.delete(stale);
    for (const [index, entry] of entries.entries()) {
      const current = existing[index];
      const record = {
        ...(current ?? {
          id: `new-${crypto.randomUUID()}`,
          recId: 0,
          rowVersion: null,
          recVersion: 1,
          dataAreaId: process.dataAreaId,
          extendedProperties: null,
        }),
        requestControlId,
        value: entry.value,
        name: entry.value,
        score: entry.score,
        sortOrder: (index + 1) * 10,
        extendedProperties: entry.extendedProperties,
        isActive: true,
      };
      if (current) await wfRequestControlOptionApi.update(record);
      else await wfRequestControlOptionApi.create(record);
    }
  }
  for (const control of requestControls) {
    const requestControlId = savedControlIds.get(control.id);
    if (!requestControlId) continue;
    const saved = savedControlRecords.get(requestControlId);
    if (!saved) continue;
    const sourceControlId = control.visibilityCondition
      ? (savedControlIds.get(control.visibilityCondition.variableId) ??
        Number(control.visibilityCondition.variableId))
      : null;
    const properties = parseObject(saved.extendedProperties);
    await wfRequestControlApi.update({
      ...saved,
      extendedProperties: JSON.stringify({
        ...properties,
        optionFeatureConfigurations: undefined,
        visibilityCondition:
          sourceControlId && sourceControlId > 0
            ? {
                sourceControlId,
                operator: control.visibilityCondition?.operator ?? '=',
                value: control.visibilityCondition?.value ?? '',
              }
            : null,
      }),
    });
  }
  const reloaded = await loadProcessBuilder(processId);
  return {
    controls: reloaded.requestControls,
    controlIds: Object.fromEntries(
      [...savedControlIds].map(([localId, persistedId]) => [localId, String(persistedId)])
    ),
  };
}

export async function saveProcessTransitions(
  document: ProcessBuilderDocument
): Promise<ProcessBuilderDocument> {
  const processId = Number(document.id);
  if (!Number.isInteger(processId) || processId <= 0)
    throw new Error('Save the process before saving transitions.');
  const [process, serverTransitions, operators] = await Promise.all([
    wfProcessApi.getById(processId),
    wfTransitionApi.list(),
    wfOperatorApi.list(),
  ]);
  const processTransitions = serverTransitions.filter(
    (transition) => transition.processId === processId
  );
  const retainedIds = new Set(
    document.transitions
      .map((transition) => numericId(transition.id))
      .filter((id): id is number => id != null)
  );
  const resolved = document.transitions.map((transition, index) => {
    const variableId = numericId(transition.variableId);
    const stepId = numericId(transition.targetStepId);
    if (!variableId) throw new Error(`Transition ${index + 1}: save and select a variable.`);
    if (!stepId) throw new Error(`Transition ${index + 1}: save and select a target step.`);
    const variable = document.variables.find((item) => item.id === transition.variableId);
    if (!variable) throw new Error(`Transition ${index + 1}: selected variable is unavailable.`);
    validateTransitionValue(transition, variable, index);
    if (
      !Number.isInteger(transition.sortOrder) ||
      transition.sortOrder < 0 ||
      transition.sortOrder > 255
    )
      throw new Error(`Transition ${index + 1}: sort order must be from 0 to 255.`);
    const operator =
      operators.find((item) => item.recId === Number(transition.operatorId)) ??
      operators.find(
        (item) => builderOperator(item.name ?? item.code ?? '') === transition.operator
      );
    if (!operator)
      throw new Error(
        `Transition ${index + 1}: operator '${transition.operator}' is not configured.`
      );
    const triggerId = transition.triggerSource === 'none' ? null : numericId(transition.triggerId);
    if (transition.triggerSource !== 'none' && !triggerId)
      throw new Error(`Transition ${index + 1}: save and select its trigger.`);
    return { transition, variableId, stepId, operatorId: operator.recId, triggerId };
  });

  for (const transition of processTransitions) {
    if (!retainedIds.has(transition.recId)) await wfTransitionApi.delete(transition);
  }
  for (const item of resolved) {
    const id = numericId(item.transition.id);
    const current =
      id == null ? null : processTransitions.find((transition) => transition.recId === id);
    const record = {
      ...(current ?? {
        id: `new-${crypto.randomUUID()}`,
        recId: 0,
        rowVersion: null,
        recVersion: 1,
        dataAreaId: process.dataAreaId,
      }),
      processId,
      activityId: item.transition.triggerSource === 'activity' ? item.triggerId : null,
      requestControlId: item.transition.triggerSource === 'requestControl' ? item.triggerId : null,
      variableId: item.variableId,
      operatorId: item.operatorId,
      value: item.transition.value.trim(),
      stepId: item.stepId,
      sortOrder: item.transition.sortOrder,
      isActive: item.transition.active,
    };
    if (current) await wfTransitionApi.update(record);
    else await wfTransitionApi.create(record);
  }
  return loadProcessBuilder(processId);
}
