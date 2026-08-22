import { beforeEach, describe, expect, it, vi } from 'vitest';

const mocks = vi.hoisted(() => ({
  processGet: vi.fn(),
  variableList: vi.fn(),
  stepList: vi.fn(),
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
  wfProcessApi: { getById: mocks.processGet },
}));
vi.mock('@modules/workflow/api/wfVariableApi', () => ({
  wfVariableApi: { list: mocks.variableList },
}));
vi.mock('@modules/workflow/api/wfStepApi', () => ({
  wfStepApi: { list: mocks.stepList },
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
} from '@modules/process-builder/api/processBuilderApi';

const process = {
  id: '1', recId: 1, code: 'PROC-1', name: 'Process', description: 'Process description', dataAreaId: 'dat',
  categoryId: 1, priorityId: 1, processTypeId: 1, score: 0,
  canRepeat: false, mandatoryDocs: false, isActive: true,
};
const step = {
  id: '10', recId: 10, processId: 1, code: 'STEP-1', name: 'Step 1', sortOrder: 1,
  score: 0, autoPassingHrs: 0, allMandatory: false, sysField: false, isActive: true,
};
const activity = {
  id: '20', recId: 20, stepId: 10, code: 'ACT-1', name: 'Review', activityTypeId: 1,
  performerId: 2, score: 0, mandatoryDocs: false, autoPassEnabled: false,
  autoPassingHrs: 0, sortOrder: 10, isActive: true, dataAreaId: 'dat',
};
const activityControl = {
  id: '30', recId: 30, activityId: 20, processId: 1, controlId: 2,
  code: 'FIELD-1', name: 'Decision', description: null, score: 0, sortOrder: 10,
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
  id: '50', recId: 50, activityControlId: 30, value: 'Approve', name: 'Approve',
  sortOrder: 10, isActive: true, rowVersion: null, recVersion: 1, dataAreaId: 'dat',
};

beforeEach(() => {
  vi.clearAllMocks();
  mocks.processGet.mockResolvedValue(process);
  mocks.variableList.mockResolvedValue([]);
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
  it('loads and saves activity controls, options, and validations', async () => {
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
      type: 'dropdown-manual',
      sortOrder: 1,
      required: true,
      options: ['Approve'],
      validations: [{ id: '40', type: 'required', message: 'Decision is required' }],
    });

    control.label = 'Final decision';
    control.options = ['Approve'];
    await saveProcessActivities(document);

    expect(mocks.activityControlUpdate).toHaveBeenCalledWith(expect.objectContaining({
      recId: 30,
      activityId: 20,
      processId: 1,
      name: 'Final decision',
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
      id: '31', recId: 31, processId: 1, controlId: 4, code: 'REQ-1', name: 'Choices',
      description: null, mandatory: true, uniqueKey: false, score: 0, usedAsCriteria: true,
      sortOrder: 10, validationRules: null,
      extendedProperties: JSON.stringify({
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
      id: '51', recId: 51, requestControlId: 31, value: 'One', name: 'One', score: 0, sortOrder: 10,
      extendedProperties: JSON.stringify({
          requireFileUpload: true,
          sendAlertMessage: true,
          alertMessage: 'Upload approval evidence.',
          performerIds: ['5', '7'],
          showOtherControls: true,
          visibleControlIds: ['33'],
      }),
      isActive: true, rowVersion: null, recVersion: 1, dataAreaId: 'dat',
    };
    const variable = {
      id: '60', recId: 60, processId: 1, code: 'VAR-1', name: 'Choice', description: null,
      dataTypeId: 1, sortOrder: 10, isActive: true,
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
        visibleControlIds: ['33'],
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
    expect(result.controlIds).toEqual({ '31': '31' });
    expect(mocks.requestControlUpdate).toHaveBeenCalledWith(expect.objectContaining({
      recId: 31, processId: 1, controlId: 4, name: 'Choices', sortOrder: 1,
    }));
    expect(mocks.requestControlUpdate).toHaveBeenLastCalledWith(expect.objectContaining({
      extendedProperties: expect.not.stringContaining('optionFeatureConfigurations'),
    }));
    expect(mocks.requestValidationUpdate).toHaveBeenCalledWith(expect.objectContaining({
      recId: 41, requestControlId: 31, validationType: 'minSelected',
    }));
    expect(mocks.requestOptionUpdate).toHaveBeenCalledWith(expect.objectContaining({
      recId: 51, requestControlId: 31, name: 'One',
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
