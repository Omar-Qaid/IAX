import { beforeEach, describe, expect, it, vi } from 'vitest';
import { loadProcessBuilderDraft } from '@modules/process-builder/hooks/useProcessBuilderDraft';

const mocks = vi.hoisted(() => ({
  processGet: vi.fn(),
  processUpdate: vi.fn(),
  variableList: vi.fn(),
  variableCreate: vi.fn(),
  variableUpdate: vi.fn(),
  variableDelete: vi.fn(),
  stepList: vi.fn(),
  stepCreate: vi.fn(),
  stepUpdate: vi.fn(),
  activityList: vi.fn(),
  activityUpdate: vi.fn(),
  activityCreate: vi.fn(),
  activityDelete: vi.fn(),
  activityControlList: vi.fn(),
  activityControlUpdate: vi.fn(),
  activityControlCreate: vi.fn(),
  activityControlDelete: vi.fn(),
  activityValidationList: vi.fn(),
  activityValidationUpdate: vi.fn(),
  activityValidationCreate: vi.fn(),
  activityValidationDelete: vi.fn(),
  activityOptionList: vi.fn(),
  activityOptionUpdate: vi.fn(),
  activityOptionCreate: vi.fn(),
  activityOptionDelete: vi.fn(),
  activityTypeList: vi.fn(),
  controlTypeList: vi.fn(),
  operatorList: vi.fn(),
  dataTypeList: vi.fn(),
  requestControlList: vi.fn(),
  requestControlUpdate: vi.fn(),
  requestControlCreate: vi.fn(),
  requestControlDelete: vi.fn(),
  requestValidationList: vi.fn(),
  requestValidationUpdate: vi.fn(),
  requestValidationCreate: vi.fn(),
  requestValidationDelete: vi.fn(),
  requestOptionList: vi.fn(),
  requestOptionUpdate: vi.fn(),
  requestOptionCreate: vi.fn(),
  requestOptionDelete: vi.fn(),
  transitionList: vi.fn(),
  transitionUpdate: vi.fn(),
  transitionCreate: vi.fn(),
  transitionDelete: vi.fn(),
  apiGet: vi.fn(),
}));

vi.mock('@modules/workflow/api/wfProcessApi', () => ({
  wfProcessApi: { getById: mocks.processGet, update: mocks.processUpdate },
}));
vi.mock('@modules/workflow/api/wfVariableApi', () => ({
  wfVariableApi: { list: mocks.variableList, delete: mocks.variableDelete, create: mocks.variableCreate, update: mocks.variableUpdate },
}));
vi.mock('@modules/workflow/api/wfStepApi', () => ({
  wfStepApi: { list: mocks.stepList, create: mocks.stepCreate, update: mocks.stepUpdate },
}));
vi.mock('@modules/workflow/api/wfActivityApi', () => ({
  wfActivityApi: {
    list: mocks.activityList,
    update: mocks.activityUpdate,
    create: mocks.activityCreate,
    delete: mocks.activityDelete,
  },
}));
vi.mock('@modules/workflow/api/wfActivityControlApi', () => ({
  wfActivityControlApi: {
    list: mocks.activityControlList,
    update: mocks.activityControlUpdate,
    create: mocks.activityControlCreate,
    delete: mocks.activityControlDelete,
  },
}));
vi.mock('@modules/workflow/api/wfActivityControlValidationApi', () => ({
  wfActivityControlValidationApi: {
    list: mocks.activityValidationList,
    update: mocks.activityValidationUpdate,
    create: mocks.activityValidationCreate,
    delete: mocks.activityValidationDelete,
  },
}));
vi.mock('@modules/workflow/api/wfActivityControlOptionApi', () => ({
  wfActivityControlOptionApi: {
    list: mocks.activityOptionList,
    update: mocks.activityOptionUpdate,
    create: mocks.activityOptionCreate,
    delete: mocks.activityOptionDelete,
  },
}));
vi.mock('@modules/workflow/api/workflowSetupApis', () => ({
  wfActivityTypeApi: { list: mocks.activityTypeList },
  wfControlApi: { list: mocks.controlTypeList },
  wfOperatorApi: { list: mocks.operatorList },
  wfDataTypeApi: { list: mocks.dataTypeList },
}));
vi.mock('@modules/workflow/api/wfRequestControlApi', () => ({
  wfRequestControlApi: {
    list: mocks.requestControlList,
    update: mocks.requestControlUpdate,
    create: mocks.requestControlCreate,
    delete: mocks.requestControlDelete,
  },
}));
vi.mock('@modules/workflow/api/wfRequestControlValidationApi', () => ({
  wfRequestControlValidationApi: {
    list: mocks.requestValidationList,
    update: mocks.requestValidationUpdate,
    create: mocks.requestValidationCreate,
    delete: mocks.requestValidationDelete,
  },
}));
vi.mock('@modules/workflow/api/wfRequestControlOptionApi', () => ({
  wfRequestControlOptionApi: {
    list: mocks.requestOptionList,
    update: mocks.requestOptionUpdate,
    create: mocks.requestOptionCreate,
    delete: mocks.requestOptionDelete,
  },
}));
vi.mock('@modules/workflow/api/wfTransitionApi', () => ({
  wfTransitionApi: {
    list: mocks.transitionList,
    update: mocks.transitionUpdate,
    create: mocks.transitionCreate,
    delete: mocks.transitionDelete,
  },
}));
vi.mock('@core/api/apiClient', () => ({
  apiClient: { get: mocks.apiGet },
}));

import {
  loadProcessBuilder,
  saveProcessActivities,
  saveProcessRequestControls,
  saveProcessTransitions,
  saveProcessVariables,
  saveProcessBuilder,
} from '@modules/process-builder/api/processBuilderApi';

const process = {
  id: '1', recId: 1, code: 'PROC-1', name: 'Process', description: 'Process description', dataAreaId: 'dat',
  categoryId: 1, priorityId: 1, processTypeId: 1, score: 0,
  isRepeatable: false, repeatIntervalHours: 12, mandatoryDocuments: false, isActive: true,
};
const step = {
  id: '10', recId: 10, processId: 1, code: 'STEP-1', name: 'Step 1', sortOrder: 1,
  score: 0, mustCompleteAll: false, isSystemDefined: false, isActive: true,
};
const activity = {
  id: '20', recId: 20, stepId: 10, code: 'ACT-1', name: 'Review', activityTypeId: 1,
  performerId: 2, score: 0, mandatoryDocuments: false, isAutoPassEnabled: false,
  autoPassAfterHours: 0, sortOrder: 10, isActive: true, dataAreaId: 'dat',
};
const activityControl = {
  id: '30', recId: 30, activityId: 20, processId: 1, controlId: 2,
  code: 'FIELD-1', name: 'Decision', nameAlias: 'القرار', description: null, score: 0, sortOrder: 10,
  mandatory: true, uniqueKey: false, usedAsCriteria: false, usedInSearch: false,
  validationRules: null,
  extendedProperties: JSON.stringify({ required: true, visible: true, defaultValue: '' }),
  isActive: true, rowVersion: null, recVersion: 1, dataAreaId: 'dat',
};
const validation = {
  id: '40', recId: 40, activityControlId: 30, validationType: 'required',
  validationExpression: null, operator: null, value: null, maskInput: null,
  errorMessage: 'Decision is required', severity: 'Error', sortOrder: 10,
  isActive: true, rowVersion: null, recVersion: 1, dataAreaId: 'dat',
};
const option = {
  id: '50', recId: 50, activityControlId: 30, value: 'APP', name: 'Approve',
  sortOrder: 10, isActive: true, rowVersion: null, recVersion: 1, dataAreaId: 'dat',
};

beforeEach(() => {
  vi.clearAllMocks();
  mocks.processGet.mockResolvedValue(process);
  mocks.processUpdate.mockImplementation(async (record) => record);
  mocks.variableList.mockResolvedValue([]);
  mocks.dataTypeList.mockResolvedValue([
    { recId: 1, code: 'INT', isActive: true },
    { recId: 2, code: 'STR', isActive: true },
    { recId: 3, code: 'DT', isActive: true },
    { recId: 4, code: 'BOOL', isActive: true },
  ]);
  mocks.stepList.mockResolvedValue([step]);
  mocks.activityList.mockResolvedValue([activity]);
  mocks.activityControlList.mockResolvedValue([activityControl]);
  mocks.activityValidationList.mockResolvedValue([validation]);
  mocks.activityOptionList.mockResolvedValue([option]);
  mocks.activityTypeList.mockResolvedValue([{ recId: 1, code: 'APPROVAL', name: 'Approval' }]);
  mocks.controlTypeList.mockResolvedValue([{ recId: 2, code: 'DROPDOWN', name: 'Dropdown', controlType: 'dropdown-manual' }]);
  mocks.operatorList.mockResolvedValue([]);
  mocks.requestControlList.mockResolvedValue([]);
  mocks.requestValidationList.mockResolvedValue([]);
  mocks.requestOptionList.mockResolvedValue([]);
  mocks.transitionList.mockResolvedValue([]);
  mocks.apiGet.mockResolvedValue({ data: { success: true, data: { mode: 'manual', manual: true, available: true, previewCode: null, message: null } } });
  mocks.activityUpdate.mockImplementation(async (record) => record);
  mocks.activityControlUpdate.mockImplementation(async (record) => record);
  mocks.activityValidationUpdate.mockImplementation(async (record) => record);
  mocks.activityOptionUpdate.mockImplementation(async (record) => record);
  mocks.requestControlUpdate.mockImplementation(async (record) => record);
  mocks.requestValidationUpdate.mockImplementation(async (record) => record);
  mocks.requestOptionUpdate.mockImplementation(async (record) => record);
  mocks.transitionUpdate.mockImplementation(async (record) => record);
});

describe('Process Builder Activity Form backend integration', () => {
  it.each(['full', 'activities'] as const)('round trips and explicitly clears the notification template in %s save', async (mode) => {
    mocks.stepUpdate.mockImplementation(async (record) => record);
    mocks.activityList.mockResolvedValue([{ ...activity, sysNotificationTemplateId: 17 }]);
    const document = await loadProcessBuilder(1);
    const configured = document.steps[0].activities[0];
    expect(configured.sysNotificationTemplateId).toBe(17);
    configured.sysNotificationTemplateId = 23;
    const save = mode === 'full' ? saveProcessBuilder : saveProcessActivities;
    await save(document);
    expect(mocks.activityUpdate).toHaveBeenLastCalledWith(expect.objectContaining({ sysNotificationTemplateId: 23 }));
    configured.sysNotificationTemplateId = null;
    await save(document);
    expect(mocks.activityUpdate).toHaveBeenLastCalledWith(expect.objectContaining({ sysNotificationTemplateId: null }));
  });
  it.each(['request', 'activity'] as const)('loads and saves relative date bounds for %s controls', async (kind) => {
    const dateRule = { ...validation, validationType: 'minDate', value: 'today' };
    if (kind === 'request') {
      mocks.requestControlList.mockResolvedValue([{ ...activityControl, processId: 1 }]);
      mocks.requestValidationList.mockResolvedValue([{ ...dateRule, requestControlId: 30 }]);
    } else mocks.activityValidationList.mockResolvedValue([dateRule]);
    const document = await loadProcessBuilder(1);
    const control = kind === 'request' ? document.requestControls[0] : document.steps[0].activities[0].controls[0];
    expect(control.validations[0]).toMatchObject({ type: 'minDate', value: 'today' });
    control.validations[0] = { ...control.validations[0], type: 'maxDate', value: 'today+1y' };
    if (kind === 'request') await saveProcessRequestControls(document);
    else await saveProcessActivities(document);
    expect(kind === 'request' ? mocks.requestValidationUpdate : mocks.activityValidationUpdate).toHaveBeenCalledWith(expect.objectContaining({
      validationType: 'maxDate', value: 'today+1y',
    }));
  });

  it.each(['full', 'transitions'] as const)('blocks %s saves before writing unsupported compound conditions', async (mode) => {
    const document = await loadProcessBuilder(1);
    document.transitions = [{
      id: 'new-transition', name: 'Approval route', sourceStepId: '', targetStepId: '10',
      variableId: '1', operator: '=', operatorId: '', value: 'yes', sortOrder: 1, active: true,
      triggerSource: 'none', triggerId: '', conditionCombinator: 'OR',
      additionalConditions: [{ variableId: '2', operator: '>', value: '100' }],
    }];
    await expect(mode === 'full' ? saveProcessBuilder(document) : saveProcessTransitions(document)).rejects.toThrow('AND/OR rule groups are draft only');
    expect(mocks.processUpdate).not.toHaveBeenCalled();
    expect(mocks.transitionCreate).not.toHaveBeenCalled();
    expect(mocks.transitionUpdate).not.toHaveBeenCalled();
    expect(mocks.transitionDelete).not.toHaveBeenCalled();
  });

  it.each(['request', 'activity'] as const)('preserves inactive options hidden from the %s editor', async (kind) => {
    const inactive = { ...option, recId: 51, id: '51', name: 'Archived option', value: 'ARCHIVED', isActive: false };
    if (kind === 'request') {
      mocks.requestControlList.mockResolvedValue([{ ...activityControl, processId: 1 }]);
      mocks.requestOptionList.mockResolvedValue([option, inactive].map((item) => ({ ...item, requestControlId: 30 })));
    } else {
      mocks.activityOptionList.mockResolvedValue([option, inactive]);
    }
    const document = await loadProcessBuilder(1);
    if (kind === 'request') await saveProcessRequestControls(document);
    else await saveProcessActivities(document);
    expect(kind === 'request' ? mocks.requestOptionDelete : mocks.activityOptionDelete).not.toHaveBeenCalled();
  });

  it('removes superseded option aliases so disabled features stay disabled on reload', async () => {
    mocks.requestControlList.mockResolvedValue([{ ...activityControl, processId: 1 }]);
    mocks.requestOptionList.mockResolvedValue([{
      ...option, requestControlId: 30, extendedProperties: JSON.stringify({ allowFileUpload: true, sendAlert: true }),
    }]);
    const document = await loadProcessBuilder(1);
    const features = document.requestControls[0].optionFeatureConfigurations![0];
    features.requireFileUpload = false;
    features.sendAlertMessage = false;
    await saveProcessRequestControls(document);
    const saved = JSON.parse(mocks.requestOptionUpdate.mock.calls[0][0].extendedProperties);
    expect(saved).toMatchObject({ requireFileUpload: false, sendAlertMessage: false });
    expect(saved).not.toHaveProperty('allowFileUpload');
    expect(saved).not.toHaveProperty('sendAlert');
  });

  it('remaps generated graph identities into both full-save transition trigger kinds', async () => {
    const document = await loadProcessBuilder(1);
    const sourceControl = document.steps[0].activities[0].controls[0];
    document.steps = [{ ...document.steps[0], id: 'new-step', activities: [{
      ...document.steps[0].activities[0], id: 'new-activity', controls: [],
    }] }];
    document.variables = [{
      id: 'new-variable', code: 'V1', name: 'Answer', description: '', dataType: 'text',
      sortOrder: 1, required: false, active: true, scope: 'process', defaultValue: '',
    }];
    document.requestControls = [{ ...sourceControl, id: 'new-control', options: [], optionIds: [], validations: [] }];
    document.transitions = (['activity', 'requestControl'] as const).map((triggerSource, index) => ({
      id: `new-transition-${index}`, name: 'Next', sourceStepId: 'new-step', targetStepId: 'new-step',
      variableId: 'new-variable', operator: '=', operatorId: '', value: 'yes', sortOrder: index + 1, active: true,
      triggerSource, triggerId: triggerSource === 'activity' ? 'new-activity' : 'new-control',
    }));
    mocks.operatorList.mockResolvedValue([{ recId: 1, code: 'EQ', name: '=', isActive: true }]);
    mocks.variableCreate.mockImplementation(async (record) => {
      const saved = { ...record, recId: 101, id: '101' };
      mocks.variableList.mockResolvedValue([saved]);
      return saved;
    });
    mocks.stepCreate.mockImplementation(async (record) => {
      const saved = { ...record, recId: 102, id: '102' };
      mocks.stepList.mockResolvedValue([saved]);
      return saved;
    });
    mocks.activityCreate.mockImplementation(async (record) => {
      const saved = { ...record, recId: 103, id: '103' };
      mocks.activityList.mockResolvedValue([saved]);
      return saved;
    });
    mocks.requestControlCreate.mockImplementation(async (record) => {
      const saved = { ...record, recId: 104, id: '104' };
      mocks.requestControlList.mockResolvedValue([saved]);
      return saved;
    });
    await saveProcessBuilder(document);
    expect(mocks.activityCreate).toHaveBeenCalledWith(expect.objectContaining({ stepId: 102 }));
    expect(mocks.transitionCreate).toHaveBeenNthCalledWith(1, expect.objectContaining({
      processId: 1, variableId: 101, stepId: 102, activityId: 103, requestControlId: null,
    }));
    expect(mocks.transitionCreate).toHaveBeenNthCalledWith(2, expect.objectContaining({
      processId: 1, variableId: 101, stepId: 102, activityId: null, requestControlId: 104,
    }));
  });

  it('rejects a foreign step during full-save preflight', async () => {
    const document = await loadProcessBuilder(1);
    mocks.stepList.mockResolvedValue([{ ...step, processId: 2 }]);
    await expect(saveProcessBuilder(document)).rejects.toThrow('stale or belongs to another parent');
    expect(mocks.processUpdate).not.toHaveBeenCalled();
    expect(mocks.activityUpdate).not.toHaveBeenCalled();
    expect(mocks.requestControlUpdate).not.toHaveBeenCalled();
    expect(mocks.variableDelete).not.toHaveBeenCalled();
  });

  it('rejects validation identities from another control before form mutations', async () => {
    const document = await loadProcessBuilder(1);
    mocks.requestControlList.mockResolvedValue([{ ...activityControl, processId: 1 }]);
    mocks.requestValidationList.mockResolvedValue([{ ...validation, requestControlId: 99 }]);
    document.requestControls = [document.steps[0].activities[0].controls[0]];
    await expect(saveProcessRequestControls(document)).rejects.toThrow('validations');
    expect(mocks.requestControlUpdate).not.toHaveBeenCalled();
    expect(mocks.requestValidationUpdate).not.toHaveBeenCalled();
    expect(mocks.requestControlDelete).not.toHaveBeenCalled();
  });

  it('rejects activity parents from another process before deleting activities', async () => {
    const document = await loadProcessBuilder(1);
    mocks.stepList.mockResolvedValue([{ ...step, processId: 2 }]);
    await expect(saveProcessActivities(document)).rejects.toThrow('Activity parent steps');
    expect(mocks.activityDelete).not.toHaveBeenCalled();
    expect(mocks.activityUpdate).not.toHaveBeenCalled();
  });

  it('keeps activity aliases and features aligned when blank options are omitted', async () => {
    const document = await loadProcessBuilder(1);
    const control = document.steps[0].activities[0].controls[0];
    control.options = ['', 'Approve'];
    control.optionIds = [null, '50'];
    control.optionAliases = ['Discarded', 'Approval alias'];
    control.optionFeatureConfigurations = [
      { requireFileUpload: false, sendAlertMessage: false, alertMessage: '', performerIds: [], showOtherControls: false, visibleControlIds: [] },
      { requireFileUpload: true, sendAlertMessage: true, alertMessage: 'Approval notice', performerIds: ['2'], showOtherControls: false, visibleControlIds: [] },
    ];
    await saveProcessActivities(document);
    expect(mocks.activityOptionUpdate).toHaveBeenCalledWith(expect.objectContaining({
      recId: 50, value: 'APP', nameAlias: 'Approval alias',
    }));
    const saved = mocks.activityControlUpdate.mock.calls[0][0];
    expect(JSON.parse(saved.extendedProperties).optionFeatureConfigurations).toEqual([
      control.optionFeatureConfigurations[1],
    ]);
  });

  it('preserves unknown request option metadata while updating owned features', async () => {
    mocks.requestControlList.mockResolvedValue([{ ...activityControl, processId: 1 }]);
    mocks.requestOptionList.mockResolvedValue([{
      ...option, requestControlId: 30,
      extendedProperties: JSON.stringify({ integration: { key: 'keep-me' }, requireFileUpload: false }),
    }]);
    const document = await loadProcessBuilder(1);
    document.requestControls[0].optionFeatureConfigurations![0].requireFileUpload = true;
    await saveProcessRequestControls(document);
    const saved = mocks.requestOptionUpdate.mock.calls[0][0];
    expect(JSON.parse(saved.extendedProperties)).toMatchObject({
      integration: { key: 'keep-me' }, requireFileUpload: true,
    });
  });

  it('rejects malformed request option metadata before modifying controls', async () => {
    mocks.requestControlList.mockResolvedValue([{ ...activityControl, processId: 1 }]);
    mocks.requestOptionList.mockResolvedValue([{
      ...option, requestControlId: 30, extendedProperties: '{broken',
    }]);
    const document = await loadProcessBuilder(1);
    await expect(saveProcessRequestControls(document)).rejects.toThrow('option ExtendedProperties');
    expect(mocks.requestControlUpdate).not.toHaveBeenCalled();
    expect(mocks.requestControlDelete).not.toHaveBeenCalled();
    expect(mocks.requestOptionUpdate).not.toHaveBeenCalled();
  });

  it('rejects missing option visibility targets before form writes', async () => {
    mocks.requestControlList.mockResolvedValue([{ ...activityControl, processId: 1 }]);
    mocks.requestOptionList.mockResolvedValue([{ ...option, requestControlId: 30 }]);
    const document = await loadProcessBuilder(1);
    document.requestControls[0].optionFeatureConfigurations = [{
      requireFileUpload: false, sendAlertMessage: false, alertMessage: '', performerIds: [],
      showOtherControls: true, visibleControlIds: ['missing-target'],
    }];
    await expect(saveProcessRequestControls(document)).rejects.toThrow('visibility target');
    expect(mocks.requestControlUpdate).not.toHaveBeenCalled();
    expect(mocks.requestControlDelete).not.toHaveBeenCalled();
    expect(mocks.requestOptionUpdate).not.toHaveBeenCalled();
  });

  it('remaps new option visibility targets to generated IDs', async () => {
    mocks.requestControlList.mockResolvedValue([{ ...activityControl, processId: 1 }]);
    mocks.requestOptionList.mockResolvedValue([{ ...option, requestControlId: 30 }]);
    const document = await loadProcessBuilder(1);
    const source = document.requestControls[0];
    source.optionFeatureConfigurations = [{
      requireFileUpload: false, sendAlertMessage: false, alertMessage: '', performerIds: [],
      showOtherControls: true, visibleControlIds: ['new-target'],
    }];
    document.requestControls.push({ ...source, id: 'new-target', code: 'TARGET', label: 'Target',
      options: [], optionIds: [], optionFeatureConfigurations: [], validations: [] });
    mocks.requestControlCreate.mockImplementation(async (record) => ({ ...record, recId: 99, id: '99' }));
    await saveProcessRequestControls(document);
    expect(JSON.parse(mocks.requestOptionUpdate.mock.calls.at(-1)![0].extendedProperties).visibleControlIds).toEqual(['99']);
  });
  it.each(['999', 'deleted-draft-control'])(
    'rejects missing visibility source %s before request form writes', async (sourceId) => {
      mocks.requestControlList.mockResolvedValue([{ ...activityControl, processId: 1 }]);
      const document = await loadProcessBuilder(1);
      document.requestControls[0].visibilityCondition = { variableId: sourceId, operator: '=', value: 'Yes' };
      await expect(saveProcessRequestControls(document)).rejects.toThrow('visibility source');
      expect(mocks.requestControlDelete).not.toHaveBeenCalled();
      expect(mocks.requestControlCreate).not.toHaveBeenCalled();
      expect(mocks.requestControlUpdate).not.toHaveBeenCalled();
    }
  );

  it('remaps a valid new visibility source to its generated control ID', async () => {
    mocks.requestControlList.mockResolvedValue([{ ...activityControl, processId: 1 }]);
    const document = await loadProcessBuilder(1);
    const target = document.requestControls[0];
    document.requestControls.push({ ...target, id: 'new-source', label: 'Source', code: 'SRC',
      options: [], optionIds: [], validations: [], visibilityCondition: null });
    target.visibilityCondition = { variableId: 'new-source', operator: '=', value: 'Yes' };
    mocks.requestControlCreate.mockImplementation(async (record) => ({ ...record, recId: 99, id: '99' }));
    await saveProcessRequestControls(document);
    const targetWrites = mocks.requestControlUpdate.mock.calls.map(([record]) => record)
      .filter((record) => record.recId === 30);
    expect(JSON.parse(targetWrites.at(-1)!.extendedProperties).visibilityCondition).toEqual({
      sourceControlId: 99, operator: '=', value: 'Yes',
    });
  });

  it.each(['step', 'requestControl', 'activity'] as const)(
    'rejects an unavailable transition %s before transition mutations', async (missing) => {
      mocks.variableList.mockResolvedValue([{ recId: 60, processId: 1, dataTypeId: 2,
        name: 'Choice', sortOrder: 10, isActive: true }]);
      mocks.operatorList.mockResolvedValue([{ recId: 3, code: 'EQ', name: '=' }]);
      const document = await loadProcessBuilder(1);
      document.transitions = [{ id: 'new-transition', name: '', sourceStepId: '',
        targetStepId: missing === 'step' ? '999' : '10', variableId: '60',
        operator: '=', operatorId: '3', value: 'A', sortOrder: 10, active: true,
        triggerSource: missing === 'step' ? 'none' : missing, triggerId: '999' }];
      mocks.transitionList.mockResolvedValue([{ recId: 70, processId: 1 }]);
      await expect(saveProcessTransitions(document)).rejects.toThrow('outside the current process document');
      expect(mocks.transitionDelete).not.toHaveBeenCalled();
      expect(mocks.transitionCreate).not.toHaveBeenCalled();
      expect(mocks.transitionUpdate).not.toHaveBeenCalled();
    }
  );

  it.each(['request', 'activity'] as const)(
    'rejects malformed stored %s metadata before control writes', async (kind) => {
      const stored = { ...activityControl, processId: 1, extendedProperties: '{invalid' };
      if (kind === 'request') mocks.requestControlList.mockResolvedValue([stored]);
      else mocks.activityControlList.mockResolvedValue([stored]);
      const document = await loadProcessBuilder(1);
      await expect(kind === 'request' ? saveProcessRequestControls(document) : saveProcessActivities(document))
        .rejects.toThrow('ExtendedProperties');
      expect(kind === 'request' ? mocks.requestControlUpdate : mocks.activityControlUpdate).not.toHaveBeenCalled();
      expect(kind === 'request' ? mocks.requestControlDelete : mocks.activityControlDelete).not.toHaveBeenCalled();
    }
  );

  it.each(['request', 'activity'] as const)(
    'rejects stale %s option IDs before control mutations', async (kind) => {
      if (kind === 'request') {
        mocks.requestControlList.mockResolvedValue([{ ...activityControl, processId: 1 }]);
        mocks.requestOptionList.mockResolvedValue([{ ...option, requestControlId: 30 }]);
      }
      const document = await loadProcessBuilder(1);
      const control = kind === 'request' ? document.requestControls[0] : document.steps[0].activities[0].controls[0];
      control.optionIds = ['999'];
      const save = kind === 'request' ? saveProcessRequestControls : saveProcessActivities;
      await expect(save(document)).rejects.toThrow('no longer belongs');
      const mutations = kind === 'request'
        ? [mocks.requestControlDelete, mocks.requestControlUpdate, mocks.requestControlCreate,
          mocks.requestValidationDelete, mocks.requestValidationUpdate, mocks.requestOptionDelete, mocks.requestOptionUpdate]
        : [mocks.activityControlDelete, mocks.activityControlUpdate, mocks.activityControlCreate,
          mocks.activityValidationDelete, mocks.activityValidationUpdate, mocks.activityOptionDelete, mocks.activityOptionUpdate];
      mutations.forEach((mutation) => expect(mutation).not.toHaveBeenCalled());
    }
  );

  it.each(['request', 'activity'] as const)(
    'preserves %s option identities through draft recovery and replacement', async (kind) => {
      const first = { ...option, requestControlId: 30, score: 0, extendedProperties: null };
      const second = { ...first, id: '51', recId: 51, name: 'Reject', value: 'REJ', sortOrder: 20 };
      const list = kind === 'request' ? mocks.requestOptionList : mocks.activityOptionList;
      const update = kind === 'request' ? mocks.requestOptionUpdate : mocks.activityOptionUpdate;
      const create = kind === 'request' ? mocks.requestOptionCreate : mocks.activityOptionCreate;
      const remove = kind === 'request' ? mocks.requestOptionDelete : mocks.activityOptionDelete;
      list.mockResolvedValue([first, second]);
      if (kind === 'request') mocks.requestControlList.mockResolvedValue([{ ...activityControl, processId: 1 }]);
      create.mockImplementation(async (record) => ({ ...record, recId: 52, id: '52' }));
      const server = await loadProcessBuilder(1);
      const control = kind === 'request' ? server.requestControls[0] : server.steps[0].activities[0].controls[0];
      control.options = ['Return', 'Decline'];
      control.optionIds = [null, '51'];
      localStorage.setItem('ixapp.process-builder.1', JSON.stringify(server));
      try {
        const recovered = loadProcessBuilderDraft('1', server);
        if (kind === 'request') await saveProcessRequestControls(recovered);
        else await saveProcessActivities(recovered);
        expect(remove).toHaveBeenCalledExactlyOnceWith(expect.objectContaining({ recId: 50 }));
        expect(update).toHaveBeenCalledExactlyOnceWith(expect.objectContaining({ recId: 51, name: 'Decline', value: 'REJ' }));
        expect(create).toHaveBeenCalledExactlyOnceWith(expect.objectContaining({ recId: 0, name: 'Return', value: 'Return' }));
      } finally {
        localStorage.removeItem('ixapp.process-builder.1');
      }
    }
  );

  it('restores activity option features after saving and reopening the process', async () => {
    const features = {
      requireFileUpload: true, sendAlertMessage: true, alertMessage: 'Evidence required',
      performerIds: ['2'], showOtherControls: true, visibleControlIds: ['30'],
    };
    mocks.activityControlList.mockResolvedValue([{
      ...activityControl,
      extendedProperties: JSON.stringify({ optionFeatureConfigurations: [features] }),
    }]);
    const document = await loadProcessBuilder(1);
    expect(document.steps[0].activities[0].controls[0].optionFeatureConfigurations).toEqual([features]);
    await saveProcessActivities(document);
    const saved = mocks.activityControlUpdate.mock.calls.at(-1)![0];
    mocks.activityControlList.mockResolvedValue([saved]);
    const reopened = await loadProcessBuilder(1);
    expect(reopened.steps[0].activities[0].controls[0].optionFeatureConfigurations).toEqual([features]);
  });

  it('normalizes missing or malformed activity option feature arrays', async () => {
    mocks.activityControlList.mockResolvedValue([{
      ...activityControl,
      extendedProperties: JSON.stringify({ optionFeatureConfigurations: { invalid: true } }),
    }]);
    const document = await loadProcessBuilder(1);
    expect(document.steps[0].activities[0].controls[0].optionFeatureConfigurations).toEqual([{
      requireFileUpload: false, sendAlertMessage: false, alertMessage: '', performerIds: [],
      showOtherControls: false, visibleControlIds: [],
    }]);
  });

  it('rejects unsupported variable types before deleting existing variables', async () => {
    const document = await loadProcessBuilder(1);
    mocks.variableList.mockResolvedValue([{ recId: 90, processId: 1, dataTypeId: 2 }]);
    document.variables = [{
      id: 'new-variable', code: '', name: 'Payload', description: '', dataType: 'object',
      sortOrder: 10, required: false, active: true, scope: 'process', defaultValue: '',
    }];
    await expect(saveProcessVariables(document)).rejects.toThrow('supported active');
    expect(mocks.variableDelete).not.toHaveBeenCalled();
  });

  it('recovers server types for legacy drafts while retaining other edits', async () => {
    mocks.variableList.mockResolvedValue([{
      recId: 60, processId: 1, dataTypeId: 1, sortOrder: 10, name: 'Amount', isActive: true,
    }]);
    const server = await loadProcessBuilder(1);
    const draft = structuredClone(server);
    delete draft.dataTypeCatalogVersion;
    draft.variables[0].dataType = 'text';
    draft.variables[0].name = 'Edited amount';
    localStorage.setItem('ixapp.process-builder.1', JSON.stringify(draft));
    try {
      expect(loadProcessBuilderDraft('1', server).variables[0]).toMatchObject({
        dataType: 'number', name: 'Edited amount',
      });
      draft.dataTypeCatalogVersion = 1;
      localStorage.setItem('ixapp.process-builder.1', JSON.stringify(draft));
      expect(loadProcessBuilderDraft('1', server).variables[0].dataType).toBe('text');
    } finally {
      localStorage.removeItem('ixapp.process-builder.1');
    }
  });

  it('maps renamed workflow settings without sending removed fields', async () => {
    mocks.activityList.mockResolvedValue([{ ...activity, isAutoPassEnabled: true, autoPassAfterHours: 24,
      mandatoryDocuments: true, isEmailNotificationEnabled: true, canViewPreviousDocuments: true }]);
    mocks.stepList.mockResolvedValue([{ ...step, mustCompleteAll: true, isSystemDefined: true }]);
    const document = await loadProcessBuilder(1);
    expect(document.repeatIntervalHours).toBe(12);
    expect(mocks.stepList).toHaveBeenCalledWith(undefined, 1);
    expect(mocks.activityList).toHaveBeenCalledWith(undefined, 1);
    expect(mocks.activityOptionList).toHaveBeenCalledWith(undefined, 1);
    expect(document.steps[0]).toMatchObject({ allMandatory: true, systemField: true });
    expect(document.steps[0]).not.toHaveProperty('autoPassingHours');
    expect(document.steps[0].activities[0]).toMatchObject({
      mandatoryDocs: true, autoPassEnabled: true, autoPassingHours: 24,
    });
    document.steps[0].activities[0].autoPassingHours = 48;
    document.steps[0].activities[0].isEmailNotificationEnabled = false;
    document.steps[0].activities[0].isWhatsAppNotificationEnabled = true;
    document.steps[0].activities[0].canViewPreviousSteps = true;
    await saveProcessActivities(document);
    const saved = mocks.activityUpdate.mock.calls[0][0];
    expect(saved).toMatchObject({ mandatoryDocuments: true, isAutoPassEnabled: true,
      autoPassAfterHours: 48, isEmailNotificationEnabled: false, isWhatsAppNotificationEnabled: true,
      canViewPreviousSteps: true, canViewPreviousDocuments: true });
    for (const removed of ['mandatoryDocs', 'autoPassEnabled', 'autoPassingHrs', 'alertingByEmail', 'showPreviousDocs']) {
      expect(saved).not.toHaveProperty(removed);
    }
  });

  it('keeps server notification settings when restoring a draft made before these fields existed', async () => {
    mocks.activityList.mockResolvedValue([{ ...activity, isEmailNotificationEnabled: true,
      canViewPreviousDocuments: true }]);
    const server = await loadProcessBuilder(1);
    const draft = JSON.parse(JSON.stringify(server));
    delete draft.steps[0].activities[0].isEmailNotificationEnabled;
    delete draft.steps[0].activities[0].canViewPreviousDocuments;
    localStorage.setItem('ixapp.process-builder.1', JSON.stringify(draft));
    try {
      expect(loadProcessBuilderDraft('1', server).steps[0].activities[0]).toMatchObject({
        isEmailNotificationEnabled: true, canViewPreviousDocuments: true,
      });
    } finally {
      localStorage.removeItem('ixapp.process-builder.1');
    }
  });

  it('loads and saves activity controls, options, and validations', async () => {
    mocks.activityControlList.mockResolvedValue([{
      ...activityControl,
      validationRules: JSON.stringify({ externalRule: { enabled: true } }),
      extendedProperties: JSON.stringify({ required: true, visible: true, integration: { key: 'keep' } }),
    }]);
    const document = await loadProcessBuilder(1);
    const control = document.steps[0].activities[0].controls[0];

    expect(document).toMatchObject({
      name: 'Process',
      description: 'Process description',
      categoryId: '1',
      priorityId: '1',
      processType: '1',
    });

    expect(control).toMatchObject({
      id: '30',
      label: 'Decision',
      labelAR: 'القرار',
      type: 'dropdown-manual',
      sortOrder: 1,
      required: true,
      options: ['Approve'],
      validations: [{ id: '40', type: 'required', message: 'Decision is required' }],
    });

    control.label = 'Final decision';
    control.labelAR = 'القرار النهائي';
    control.options = ['Approve'];
    await saveProcessActivities(document);

    const activityWrite = mocks.activityControlUpdate.mock.calls.at(-1)![0];
    expect(JSON.parse(activityWrite.extendedProperties)).toMatchObject({ integration: { key: 'keep' } });
    expect(JSON.parse(activityWrite.validationRules)).toMatchObject({ externalRule: { enabled: true } });

    expect(mocks.activityControlUpdate).toHaveBeenCalledWith(expect.objectContaining({
      recId: 30,
      activityId: 20,
      processId: 1,
      name: 'Final decision',
      nameAlias: 'القرار النهائي',
      sortOrder: 1,
      validationRules: expect.any(String),
    }));
    expect(mocks.activityValidationUpdate).toHaveBeenCalledWith(expect.objectContaining({
      recId: 40,
      activityControlId: 30,
      errorMessage: 'Decision is required',
    }));
    expect(mocks.activityOptionUpdate).toHaveBeenCalledWith(expect.objectContaining({
      recId: 50,
      activityControlId: 30,
      name: 'Approve',
      value: 'APP',
    }));
  });

  it('uses the backend NORMAL activity type for a new approval-mode activity', async () => {
    mocks.activityTypeList.mockResolvedValue([
      { recId: 2, code: 'NORMAL', name: 'مرحلة عادية', isActive: true },
      { recId: 1, code: 'PARTIAL', name: 'مرحلة جزئية', isActive: true },
    ]);
    const document = await loadProcessBuilder(1);
    document.steps[0].activities[0].activityTypeId = '';
    document.steps[0].activities[0].type = 'approval';

    await saveProcessActivities(document);

    expect(mocks.activityUpdate).toHaveBeenCalledWith(expect.objectContaining({
      recId: 20,
      activityTypeId: 2,
    }));
  });

  it('loads and saves request controls, selectable options, validations, and transitions', async () => {
    const requestControl = {
      id: '31', recId: 31, processId: 1, controlId: 4, code: 'REQ-1', name: 'Choices', nameAlias: 'الخيارات',
      description: null, mandatory: true, uniqueKey: false, score: 0, usedAsCriteria: true,
      sortOrder: 10, validationRules: JSON.stringify({ externalRule: { enabled: true } }),
      extendedProperties: JSON.stringify({
        integration: { key: 'keep' },
        required: true,
        visible: true,
      }),
      isActive: true, rowVersion: null, recVersion: 1, dataAreaId: 'dat',
    };
    const requestValidation = {
      id: '41', recId: 41, requestControlId: 31, validationType: 'minSelected',
      validationExpression: null, operator: null, value: '1', maskInput: null,
      errorMessage: 'Select at least one', severity: 'Error', sortOrder: 10,
      isActive: true, rowVersion: null, recVersion: 1, dataAreaId: 'dat',
    };
    const requestOption = {
      id: '51', recId: 51, requestControlId: 31, value: 'ONE_CODE', name: 'One', score: 0, sortOrder: 10,
      extendedProperties: JSON.stringify({
          requireFileUpload: true,
          sendAlertMessage: true,
          alertMessage: 'Upload approval evidence.',
          performerIds: ['5', '7'],
          showOtherControls: true,
          visibleControlIds: ['31'],
      }),
      isActive: true, rowVersion: null, recVersion: 1, dataAreaId: 'dat',
    };
    const variable = {
      id: '60', recId: 60, processId: 1, code: 'VAR-1', name: 'Choice', description: null,
      dataTypeId: 2, sortOrder: 10, isActive: true,
    };
    const transition = {
      id: '70', recId: 70, processId: 1, activityId: null, requestControlId: 31,
      variableId: 60, operatorId: 3, value: 'One', stepId: 10, sortOrder: 10,
      isActive: true, rowVersion: null, recVersion: 1, dataAreaId: 'dat',
    };
    mocks.variableList.mockResolvedValue([variable]);
    mocks.requestControlList.mockResolvedValue([requestControl]);
    mocks.requestValidationList.mockResolvedValue([requestValidation]);
    mocks.requestOptionList.mockResolvedValue([requestOption]);
    mocks.transitionList.mockResolvedValue([transition]);
    mocks.controlTypeList.mockResolvedValue([
      { recId: 2, code: 'DROPDOWN-DB', name: 'Database Dropdown', controlType: 'dropdown-db' },
      { recId: 4, code: 'CHECKLIST', name: 'Check Box List', controlType: 'checkboxlist' },
    ]);
    mocks.operatorList.mockResolvedValue([{ recId: 3, code: 'EQ', name: '=' }]);

    const document = await loadProcessBuilder(1);
    const control = document.requestControls[0];
    expect(control).toMatchObject({
      id: '31',
      type: 'checkboxlist',
      sortOrder: 1,
      options: ['One'],
      optionFeatureConfigurations: [{
        requireFileUpload: true,
        sendAlertMessage: true,
        performerIds: ['5', '7'],
        showOtherControls: true,
        visibleControlIds: ['31'],
      }],
      validations: [{ id: '41', type: 'minSelected', value: '1' }],
    });
    expect(document.transitions[0]).toMatchObject({
      triggerSource: 'requestControl',
      triggerId: '31',
      operator: '=',
    });

    control.options = ['One'];
    const result = await saveProcessRequestControls(document);

    for (const [write] of mocks.requestControlUpdate.mock.calls) {
      expect(JSON.parse(write.extendedProperties)).toMatchObject({ integration: { key: 'keep' } });
      expect(JSON.parse(write.validationRules)).toMatchObject({ externalRule: { enabled: true } });
    }

    expect(result.controls[0].labelAR).toBe('الخيارات');
    expect(result.controlIds).toEqual({ '31': '31' });
    expect(mocks.requestControlUpdate).toHaveBeenCalledWith(expect.objectContaining({
      recId: 31, processId: 1, controlId: 4, name: 'Choices', nameAlias: 'الخيارات', sortOrder: 1,
    }));
    expect(mocks.requestControlUpdate).toHaveBeenLastCalledWith(expect.objectContaining({
      extendedProperties: expect.not.stringContaining('optionFeatureConfigurations'),
    }));
    expect(mocks.requestValidationUpdate).toHaveBeenCalledWith(expect.objectContaining({
      recId: 41, requestControlId: 31, validationType: 'minSelected',
    }));
    expect(mocks.requestOptionUpdate).toHaveBeenCalledWith(expect.objectContaining({
      recId: 51, requestControlId: 31, name: 'One',
      value: 'ONE_CODE',
      extendedProperties: expect.stringContaining('requireFileUpload'),
    }));

    await saveProcessTransitions(document);
    expect(mocks.transitionUpdate).toHaveBeenCalledWith(expect.objectContaining({
      recId: 70,
      requestControlId: 31,
      activityId: null,
      variableId: 60,
      operatorId: 3,
      stepId: 10,
    }));
  });
});
