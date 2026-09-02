import { create } from 'zustand';
import type {
  BuilderActivity,
  BuilderActivityAction,
  BuilderControl,
  BuilderNode,
  BuilderStep,
  BuilderTransition,
  BuilderVariable,
  ProcessBuilderDocument,
} from '../types/processBuilderTypes';
import i18n from '@core/localization/i18n';

const id = () => crypto.randomUUID();
const sequenceControls = (controls: BuilderControl[]): BuilderControl[] =>
  controls.map((control, index) => ({ ...control, sortOrder: index + 1 }));
export const createProcessBuilderDocument = (builderId = 'new'): ProcessBuilderDocument => ({
  id: builderId,
  code: builderId === 'new' ? '' : `PB-${builderId}`,
  name: '',
  description: '',
  categoryId: '',
  priorityId: '',
  processType: '',
  score: 0,
  canRepeat: false,
  mandatoryDocs: false,
  active: true,
  variables: [],
  requestControls: [],
  steps: [],
  transitions: [],
});

const selectionForTab = (
  document: ProcessBuilderDocument,
  selected: BuilderNode,
  selectedStepId: string | null,
  tab: number
): BuilderNode => {
  if (tab === 0) return { kind: 'process' };

  if (tab === 1) {
    const variable =
      selected.kind === 'variable'
        ? document.variables.find((item) => item.id === selected.id)
        : undefined;
    const selectedVariable = variable ?? document.variables[0];
    return selectedVariable
      ? { kind: 'variable', id: selectedVariable.id }
      : { kind: 'workspace', tab };
  }

  if (tab === 2) {
    const control =
      selected.kind === 'requestControl'
        ? document.requestControls.find((item) => item.id === selected.id)
        : undefined;
    const requestControl = control ?? document.requestControls[0];
    return requestControl
      ? { kind: 'requestControl', id: requestControl.id }
      : { kind: 'workspace', tab };
  }

  if (tab === 3) {
    const activeStepId =
      selected.kind === 'step'
        ? selected.id
        : selected.kind === 'activity' || selected.kind === 'control'
          ? selected.stepId
          : undefined;
    const step =
      document.steps.find((item) => item.id === activeStepId) ??
      document.steps.find((item) => item.id === selectedStepId) ??
      document.steps[0];
    return step ? { kind: 'step', id: step.id } : { kind: 'workspace', tab };
  }

  if (tab === 4) {
    const selectedActivity =
      selected.kind === 'activity'
        ? document.steps
            .find((step) => step.id === selected.stepId)
            ?.activities.find((activity) => activity.id === selected.id)
        : selected.kind === 'control'
          ? document.steps
              .find((step) => step.id === selected.stepId)
              ?.activities.find((activity) => activity.id === selected.activityId)
          : selected.kind === 'step'
            ? document.steps.find((step) => step.id === selected.id)?.activities[0]
            : undefined;
    const rememberedActivity = document.steps.find((step) => step.id === selectedStepId)
      ?.activities[0];
    const activity =
      selectedActivity ??
      rememberedActivity ??
      document.steps.flatMap((step) => step.activities)[0];
    if (!activity) return { kind: 'workspace', tab };
    const step = document.steps.find((item) =>
      item.activities.some((item) => item.id === activity.id)
    );
    return step
      ? { kind: 'activity', stepId: step.id, id: activity.id }
      : { kind: 'workspace', tab };
  }

  if (tab === 5) {
    const selectedActivity =
      selected.kind === 'activity'
        ? document.steps
            .find((step) => step.id === selected.stepId)
            ?.activities.find((activity) => activity.id === selected.id)
        : selected.kind === 'control'
          ? document.steps
              .find((step) => step.id === selected.stepId)
              ?.activities.find((activity) => activity.id === selected.activityId)
          : undefined;
    const rememberedActivity = document.steps.find((step) => step.id === selectedStepId)
      ?.activities[0];
    const activity =
      selectedActivity ??
      rememberedActivity ??
      document.steps.flatMap((step) => step.activities)[0];
    if (!activity) return { kind: 'workspace', tab };
    const activityStep = document.steps.find((step) =>
      step.activities.some((item) => item.id === activity.id)
    );
    if (!activityStep) return { kind: 'workspace', tab };
    const selectedControl =
      selected.kind === 'control' && selected.activityId === activity.id
        ? activity.controls.find((control) => control.id === selected.id)
        : activity.controls[0];
    const control = selectedControl;
    if (!control) return { kind: 'activity', stepId: activityStep.id, id: activity.id };
    for (const step of document.steps) {
      const activity = step.activities.find((item) =>
        item.controls.some((item) => item.id === control.id)
      );
      if (activity)
        return { kind: 'control', stepId: step.id, activityId: activity.id, id: control.id };
    }
    return { kind: 'workspace', tab };
  }

  if (tab === 6) {
    const transition =
      selected.kind === 'transition'
        ? document.transitions.find((item) => item.id === selected.id)
        : undefined;
    const selectedTransition = transition ?? document.transitions[0];
    return selectedTransition
      ? { kind: 'transition', id: selectedTransition.id }
      : { kind: 'workspace', tab };
  }

  return { kind: 'workspace', tab };
};

export interface ProcessBuilderNavigationState {
  selected: BuilderNode;
  selectedStepId: string | null;
  leftTab: number;
  centerTab: number;
}

const selectionExists = (
  document: ProcessBuilderDocument,
  selected: BuilderNode | null | undefined
): boolean => {
  if (!selected) return false;
  if (selected.kind === 'process') return true;
  if (selected.kind === 'workspace')
    return Number.isInteger(selected.tab) && selected.tab >= 0 && selected.tab <= 7;
  if (selected.kind === 'variable')
    return document.variables.some((item) => item.id === selected.id);
  if (selected.kind === 'requestControl')
    return document.requestControls.some((item) => item.id === selected.id);
  if (selected.kind === 'step') return document.steps.some((item) => item.id === selected.id);
  if (selected.kind === 'activity')
    return (
      document.steps
        .find((step) => step.id === selected.stepId)
        ?.activities.some((item) => item.id === selected.id) ?? false
    );
  if (selected.kind === 'control')
    return (
      document.steps
        .find((step) => step.id === selected.stepId)
        ?.activities.find((activity) => activity.id === selected.activityId)
        ?.controls.some((item) => item.id === selected.id) ?? false
    );
  if (selected.kind === 'transition')
    return document.transitions.some((item) => item.id === selected.id);
  return false;
};

interface State {
  document: ProcessBuilderDocument;
  selected: BuilderNode;
  controlSettingsPane: 'configure' | 'options' | 'validation' | 'transitions';
  selectedStepId: string | null;
  leftTab: number;
  centerTab: number;
  dirty: boolean;
  markDraftSaved: () => void;
  initialize: (document: ProcessBuilderDocument) => void;
  applyPersistedDocument: (document: ProcessBuilderDocument) => void;
  restoreNavigation: (navigation: ProcessBuilderNavigationState) => void;
  select: (node: BuilderNode) => void;
  openControlSettings: (
    node: Extract<BuilderNode, { kind: 'requestControl' | 'control' }>,
    pane: 'configure' | 'options' | 'validation' | 'transitions'
  ) => void;
  setLeftTab: (value: number) => void;
  setCenterTab: (value: number) => void;
  updateProcess: (values: Partial<ProcessBuilderDocument>) => void;
  setGeneratedCode: (code: string) => void;
  setPersistedVariables: (
    variables: BuilderVariable[],
    variableIds: Record<string, string>
  ) => void;
  setPersistedSteps: (steps: BuilderStep[], stepIds: Record<string, string>) => void;
  setPersistedActivities: (
    document: ProcessBuilderDocument,
    activityIds: Record<string, string>
  ) => void;
  setPersistedTransitions: (transitions: BuilderTransition[]) => void;
  setPersistedRequestControls: (
    controls: BuilderControl[],
    controlIds: Record<string, string>
  ) => void;
  addVariable: () => void;
  updateVariable: (id: string, values: Partial<BuilderVariable>) => void;
  removeVariable: (id: string) => void;
  reorderVariables: (activeId: string, overId: string) => void;
  addRequestControl: (type?: BuilderControl['type']) => void;
  updateRequestControl: (id: string, values: Partial<BuilderControl>) => void;
  removeRequestControl: (id: string) => void;
  reorderRequestControls: (activeId: string, overId: string) => void;
  reorderRequestControlOptions: (controlId: string, fromIndex: number, toIndex: number) => void;
  reorderActivityControlOptions: (
    stepId: string,
    activityId: string,
    controlId: string,
    fromIndex: number,
    toIndex: number
  ) => void;
  addStep: () => void;
  updateStep: (id: string, values: Partial<BuilderStep>) => void;
  removeStep: (id: string) => void;
  addActivity: (stepId: string, type?: BuilderActivity['type']) => void;
  updateActivity: (stepId: string, id: string, values: Partial<BuilderActivity>) => void;
  removeActivity: (stepId: string, id: string) => void;
  addActivityControl: (stepId: string, activityId: string, type?: BuilderControl['type']) => void;
  updateActivityControl: (
    stepId: string,
    activityId: string,
    id: string,
    values: Partial<BuilderControl>
  ) => void;
  removeActivityControl: (stepId: string, activityId: string, id: string) => void;
  addActivityAction: (
    stepId: string,
    activityId: string,
    type?: BuilderActivityAction['type']
  ) => void;
  updateActivityAction: (
    stepId: string,
    activityId: string,
    id: string,
    values: Partial<BuilderActivityAction>
  ) => void;
  removeActivityAction: (stepId: string, activityId: string, id: string) => void;
  moveStep: (id: string, direction: -1 | 1) => void;
  reorderSteps: (activeId: string, overId: string) => void;
  reorderActivities: (stepId: string, activeId: string, overId: string) => void;
  reorderControls: (stepId: string, activityId: string, activeId: string, overId: string) => void;
  addTransition: (trigger?: {
    triggerSource: 'requestControl' | 'activity';
    triggerId: string;
  }) => void;
  updateTransition: (id: string, values: Partial<BuilderTransition>) => void;
  removeTransition: (id: string) => void;
}

export const useProcessBuilderStore = create<State>((set) => {
  const change = (mutate: (document: ProcessBuilderDocument) => ProcessBuilderDocument) =>
    set((state) => ({ document: mutate(state.document), dirty: true }));
  return {
    document: createProcessBuilderDocument(),
    selected: { kind: 'process' },
    controlSettingsPane: 'configure',
    selectedStepId: null,
    leftTab: 0,
    centerTab: 0,
    dirty: false,
    initialize: (document) =>
      set({
        document,
        selected: { kind: 'process' },
        controlSettingsPane: 'configure',
        selectedStepId: document.steps[0]?.id ?? null,
        leftTab: 0,
        centerTab: 0,
        dirty: false,
      }),
    applyPersistedDocument: (document) =>
      set((state) => {
        const selectedStepId =
          document.steps.find((step) => step.id === state.selectedStepId)?.id ??
          document.steps[state.document.steps.findIndex((step) => step.id === state.selectedStepId)]
            ?.id ??
          document.steps[0]?.id ??
          null;
        return {
          document,
          selected: selectionForTab(document, state.selected, selectedStepId, state.centerTab),
          selectedStepId,
          leftTab: state.leftTab,
          centerTab: state.centerTab,
          dirty: false,
        };
      }),
    restoreNavigation: (navigation) =>
      set((state) => {
        const requestedSelection = navigation.selected as BuilderNode | undefined;
        const centerTab =
          Number.isInteger(navigation.centerTab) &&
          navigation.centerTab >= 0 &&
          navigation.centerTab <= 7
            ? navigation.centerTab
            : 0;
        const leftTab = navigation.leftTab === 1 ? 1 : 0;
        const selectedStepId =
          navigation.selectedStepId != null &&
          state.document.steps.some((step) => step.id === navigation.selectedStepId)
            ? navigation.selectedStepId
            : requestedSelection?.kind === 'step'
              ? requestedSelection.id
              : requestedSelection?.kind === 'activity' || requestedSelection?.kind === 'control'
                ? requestedSelection.stepId
                : (state.document.steps[0]?.id ?? null);
        const selected = selectionExists(state.document, requestedSelection)
          ? requestedSelection!
          : selectionForTab(
              state.document,
              { kind: 'workspace', tab: centerTab },
              selectedStepId,
              centerTab
            );
        return { centerTab, leftTab, selectedStepId, selected, controlSettingsPane: 'configure' };
      }),
    // Browser draft persistence is not a database save; keep server changes dirty.
    markDraftSaved: () => undefined,
    select: (selected) =>
      set((state) => ({
        selected,
        controlSettingsPane: 'configure',
        selectedStepId:
          selected.kind === 'step'
            ? selected.id
            : selected.kind === 'activity' || selected.kind === 'control'
              ? selected.stepId
              : state.selectedStepId,
      })),
    openControlSettings: (selected, controlSettingsPane) =>
      set((state) => ({
        selected,
        controlSettingsPane,
        selectedStepId: selected.kind === 'control' ? selected.stepId : state.selectedStepId,
      })),
    setLeftTab: (leftTab) => set({ leftTab }),
    setCenterTab: (centerTab) =>
      set((state) => ({
        centerTab,
        selected: selectionForTab(state.document, state.selected, state.selectedStepId, centerTab),
        controlSettingsPane: 'configure',
      })),
    updateProcess: (values) => change((document) => ({ ...document, ...values })),
    setGeneratedCode: (code) => set((state) => ({ document: { ...state.document, code } })),
    setPersistedVariables: (variables, variableIds) =>
      set((state) => ({
        document: {
          ...state.document,
          variables,
          transitions: state.document.transitions.map((transition) =>
            variableIds[transition.variableId]
              ? { ...transition, variableId: variableIds[transition.variableId] }
              : transition
          ),
        },
        selected:
          state.selected.kind === 'variable' && variableIds[state.selected.id]
            ? { kind: 'variable', id: variableIds[state.selected.id] }
            : state.selected,
      })),
    setPersistedSteps: (persistedSteps, stepIds) =>
      set((state) => {
        const steps = state.document.steps.map((step) => {
          const persistedId = stepIds[step.id] ?? step.id;
          const persisted = persistedSteps.find((item) => item.id === persistedId);
          return persisted
            ? { ...persisted, activities: step.activities }
            : { ...step, id: persistedId };
        });
        const selectedStepId =
          state.selectedStepId == null
            ? null
            : (stepIds[state.selectedStepId] ?? state.selectedStepId);
        const remapStepId = (stepId: string) => stepIds[stepId] ?? stepId;
        return {
          document: {
            ...state.document,
            steps,
            transitions: state.document.transitions.map((transition) => ({
              ...transition,
              sourceStepId: remapStepId(transition.sourceStepId),
              targetStepId: remapStepId(transition.targetStepId),
            })),
          },
          selectedStepId,
          selected:
            state.selected.kind === 'step'
              ? { kind: 'step', id: remapStepId(state.selected.id) }
              : state.selected.kind === 'activity' || state.selected.kind === 'control'
                ? { ...state.selected, stepId: remapStepId(state.selected.stepId) }
                : state.selected,
        };
      }),
    setPersistedActivities: (persisted, activityIds) =>
      set((state) => {
        const steps = state.document.steps.map((step) => {
          const persistedStep = persisted.steps.find((item) => item.id === step.id);
          return persistedStep ? { ...step, activities: persistedStep.activities } : step;
        });
        return {
          document: {
            ...state.document,
            steps,
            transitions: state.document.transitions.map((transition) =>
              transition.triggerSource === 'activity' && activityIds[transition.triggerId]
                ? { ...transition, triggerId: activityIds[transition.triggerId] }
                : transition
            ),
          },
          selected:
            state.selected.kind === 'activity' && activityIds[state.selected.id]
              ? { ...state.selected, id: activityIds[state.selected.id] }
              : state.selected.kind === 'control'
                ? selectionForTab(
                    { ...state.document, steps },
                    state.selected,
                    state.selectedStepId,
                    state.centerTab
                  )
                : state.selected,
        };
      }),
    setPersistedTransitions: (transitions) =>
      set((state) => {
        const selected = state.selected;
        return {
          document: { ...state.document, transitions },
          selected:
            selected.kind === 'transition'
              ? transitions.some((transition) => transition.id === selected.id)
                ? selected
                : transitions[0]
                  ? { kind: 'transition', id: transitions[0].id }
                  : { kind: 'workspace', tab: 6 }
              : selected,
        };
      }),
    setPersistedRequestControls: (requestControls, controlIds) =>
      set((state) => ({
        document: {
          ...state.document,
          requestControls: sequenceControls(requestControls),
          transitions: state.document.transitions.map((transition) =>
            transition.triggerSource === 'requestControl' && controlIds[transition.triggerId]
              ? { ...transition, triggerId: controlIds[transition.triggerId] }
              : transition
          ),
        },
        selected:
          state.selected.kind === 'requestControl' && controlIds[state.selected.id]
            ? { kind: 'requestControl', id: controlIds[state.selected.id] }
            : state.selected,
        dirty: state.document.transitions.some(
          (transition) =>
            transition.triggerSource === 'requestControl' &&
            Boolean(controlIds[transition.triggerId])
        ),
      })),
    addVariable: () =>
      change((d) => ({
        ...d,
        variables: [
          ...d.variables,
          {
            id: id(),
            code: '',
            name: i18n.t('wfProcessBuilder.defaults.newVariable'),
            description: '',
            dataType: 'text',
            sortOrder: (d.variables.length + 1) * 10,
            required: false,
            active: true,
            scope: 'process',
            defaultValue: '',
          },
        ],
      })),
    updateVariable: (key, values) =>
      change((d) => ({
        ...d,
        variables: d.variables.map((x) => (x.id === key ? { ...x, ...values } : x)),
      })),
    removeVariable: (key) =>
      set((state) => {
        const document = {
          ...state.document,
          variables: state.document.variables.filter((item) => item.id !== key),
          transitions: state.document.transitions.map((transition) =>
            transition.variableId === key ? { ...transition, variableId: '' } : transition
          ),
        };
        return {
          document,
          selected: selectionForTab(
            document,
            state.selected,
            state.selectedStepId,
            state.centerTab
          ),
          dirty: true,
        };
      }),
    reorderVariables: (activeId, overId) =>
      change((d) => {
        const variables = [...d.variables];
        const from = variables.findIndex((x) => x.id === activeId);
        const to = variables.findIndex((x) => x.id === overId);
        if (from < 0 || to < 0 || from === to) return d;
        const [moved] = variables.splice(from, 1);
        variables.splice(to, 0, moved);
        return {
          ...d,
          variables: variables.map((variable, index) => ({
            ...variable,
            sortOrder: (index + 1) * 10,
          })),
        };
      }),
    addRequestControl: (type = 'text') =>
      change((d) => ({
        ...d,
        requestControls: sequenceControls([
          ...d.requestControls,
          {
            id: id(),
            code: '',
            label: i18n.t('wfProcessBuilder.defaults.newField', { lng: 'en' }),
            labelAR: i18n.t('wfProcessBuilder.defaults.newField', { lng: 'ar' }),
            labelColor: '#7a4b00',
            type,
            controlId: '',
            sortOrder: d.requestControls.length + 1,
            score: 0,
            required: false,
            readOnly: false,
            visible: true,
            uniqueKey: false,
            usedAsCriteria: false,
            canFilter: true,
            canGroup: true,
            canSort: true,
            referenceType: null,
            fieldRole: 'Dimension',
            dataType: 'String',
            defaultAggregation: 'NONE',
            defaultValue: '',
            options: [],
            optionScores: [],
            optionFeatureConfigurations: [],
            validations: [],
            visibilityCondition: null,
          },
        ]),
      })),
    updateRequestControl: (key, values) =>
      change((d) => ({
        ...d,
        requestControls: d.requestControls.map((x) => (x.id === key ? { ...x, ...values } : x)),
      })),
    removeRequestControl: (key) =>
      set((state) => {
        const document = {
          ...state.document,
          requestControls: sequenceControls(
            state.document.requestControls.filter((item) => item.id !== key)
          ),
          transitions: state.document.transitions.map((transition) =>
            transition.triggerSource === 'requestControl' && transition.triggerId === key
              ? { ...transition, triggerSource: 'none' as const, triggerId: '' }
              : transition
          ),
        };
        return {
          document,
          selected: selectionForTab(
            document,
            state.selected,
            state.selectedStepId,
            state.centerTab
          ),
          dirty: true,
        };
      }),
    reorderRequestControls: (activeId, overId) =>
      change((d) => {
        const controls = [...d.requestControls];
        const from = controls.findIndex((x) => x.id === activeId);
        const to = controls.findIndex((x) => x.id === overId);
        if (from < 0 || to < 0 || from === to) return d;
        const [moved] = controls.splice(from, 1);
        controls.splice(to, 0, moved);
        return { ...d, requestControls: sequenceControls(controls) };
      }),
    reorderRequestControlOptions: (controlId, fromIndex, toIndex) =>
      change((d) => ({
        ...d,
        requestControls: d.requestControls.map((control) => {
          if (
            control.id !== controlId ||
            fromIndex === toIndex ||
            fromIndex < 0 ||
            toIndex < 0 ||
            fromIndex >= control.options.length ||
            toIndex >= control.options.length
          )
            return control;
          const options = [...control.options];
          const optionAliases = [...(control.optionAliases ?? control.options.map(() => ''))];
          const optionScores = [...(control.optionScores ?? control.options.map(() => 0))];
          const optionFeatureConfigurations = [
            ...(control.optionFeatureConfigurations ??
              control.options.map(() => ({
                requireFileUpload: false,
                sendAlertMessage: false,
                alertMessage: '',
                performerIds: [],
                showOtherControls: false,
                visibleControlIds: [],
              }))),
          ];
          const [moved] = options.splice(fromIndex, 1);
          const [movedAlias] = optionAliases.splice(fromIndex, 1);
          const [movedScore] = optionScores.splice(fromIndex, 1);
          const [movedFeatures] = optionFeatureConfigurations.splice(fromIndex, 1);
          options.splice(toIndex, 0, moved);
          optionAliases.splice(toIndex, 0, movedAlias);
          optionScores.splice(toIndex, 0, movedScore);
          optionFeatureConfigurations.splice(toIndex, 0, movedFeatures);
          return { ...control, options, optionAliases, optionScores, optionFeatureConfigurations };
        }),
      })),
    reorderActivityControlOptions: (stepId, activityId, controlId, fromIndex, toIndex) =>
      change((d) => ({
        ...d,
        steps: d.steps.map((step) =>
          step.id !== stepId
            ? step
            : {
                ...step,
                activities: step.activities.map((activity) =>
                  activity.id !== activityId
                    ? activity
                    : {
                        ...activity,
                        controls: activity.controls.map((control) => {
                          if (
                            control.id !== controlId ||
                            fromIndex === toIndex ||
                            fromIndex < 0 ||
                            toIndex < 0 ||
                            fromIndex >= control.options.length ||
                            toIndex >= control.options.length
                          )
                            return control;
                          const options = [...control.options];
                          const [moved] = options.splice(fromIndex, 1);
                          options.splice(toIndex, 0, moved);
                          return { ...control, options };
                        }),
                      }
                ),
              }
        ),
      })),
    addStep: () =>
      change((d) => ({
        ...d,
        steps: [
          ...d.steps,
          {
            id: id(),
            code: '',
            name: i18n.t('wfProcessBuilder.defaults.step', { number: d.steps.length + 1 }),
            order: d.steps.length + 1,
            score: 0,
            autoPassingHours: 0,
            allMandatory: false,
            active: true,
            systemField: false,
            condition: null,
            activities: [],
          },
        ],
      })),
    updateStep: (key, values) =>
      change((d) => ({
        ...d,
        steps: d.steps.map((x) => (x.id === key ? { ...x, ...values } : x)),
      })),
    removeStep: (key) =>
      set((state) => {
        const removedActivityIds = new Set(
          state.document.steps
            .find((step) => step.id === key)
            ?.activities.map((activity) => activity.id) ?? []
        );
        const steps = state.document.steps
          .filter((step) => step.id !== key)
          .map((step, index) => ({ ...step, order: index + 1 }));
        const document = {
          ...state.document,
          steps,
          transitions: state.document.transitions.map((transition) => ({
            ...transition,
            sourceStepId: transition.sourceStepId === key ? '' : transition.sourceStepId,
            targetStepId: transition.targetStepId === key ? '' : transition.targetStepId,
            ...(transition.triggerSource === 'activity' &&
            removedActivityIds.has(transition.triggerId)
              ? { triggerSource: 'none' as const, triggerId: '' }
              : {}),
          })),
        };
        const selectedStepId =
          state.selectedStepId === key ? (steps[0]?.id ?? null) : state.selectedStepId;
        return {
          document,
          selectedStepId,
          selected: selectionForTab(document, state.selected, selectedStepId, state.centerTab),
          dirty: true,
        };
      }),
    addActivity: (stepId, type = 'approval') =>
      change((d) => ({
        ...d,
        steps: d.steps.map((s) =>
          s.id === stepId
            ? {
                ...s,
                activities: [
                  ...s.activities,
                  {
                    id: id(),
                    code: '',
                    name: i18n.t('wfProcessBuilder.defaults.newActivity'),
                    type,
                    activityTypeId: '',
                    performer: '',
                    score: 0,
                    sortOrder: (s.activities.length + 1) * 10,
                    assignmentMode: 'any',
                    active: true,
                    required: true,
                    mandatoryDocs: false,
                    autoPassEnabled: false,
                    autoPassingHours: 0,
                    controls: [],
                    actions: [],
                    validations: [],
                    condition: null,
                    config: { apiMethod: 'GET', apiUrl: '', notifyEmails: '' },
                  },
                ],
              }
            : s
        ),
      })),
    updateActivity: (stepId, key, values) =>
      change((d) => ({
        ...d,
        steps: d.steps.map((s) =>
          s.id === stepId
            ? {
                ...s,
                activities: s.activities.map((a) => (a.id === key ? { ...a, ...values } : a)),
              }
            : s
        ),
      })),
    removeActivity: (stepId, key) =>
      set((state) => {
        const document = {
          ...state.document,
          steps: state.document.steps.map((step) =>
            step.id === stepId
              ? { ...step, activities: step.activities.filter((activity) => activity.id !== key) }
              : step
          ),
          transitions: state.document.transitions.map((transition) =>
            transition.triggerSource === 'activity' && transition.triggerId === key
              ? { ...transition, triggerSource: 'none' as const, triggerId: '' }
              : transition
          ),
        };
        return {
          document,
          selected: selectionForTab(
            document,
            state.selected,
            state.selectedStepId,
            state.centerTab
          ),
          dirty: true,
        };
      }),
    addActivityControl: (stepId, activityId, type = 'text') =>
      change((d) => ({
        ...d,
        steps: d.steps.map((s) =>
          s.id === stepId
            ? {
                ...s,
                activities: s.activities.map((a) =>
                  a.id === activityId
                    ? {
                        ...a,
                        controls: sequenceControls([
                          ...a.controls,
                          {
                            id: id(),
                            code: '',
                            label: i18n.t('wfProcessBuilder.defaults.newField', { lng: 'en' }),
                            labelAR: i18n.t('wfProcessBuilder.defaults.newField', { lng: 'ar' }),
                            labelColor: '#7a4b00',
                            type,
                            controlId: '',
                            sortOrder: a.controls.length + 1,
                            score: 0,
                            required: false,
                            readOnly: false,
                            visible: true,
                            uniqueKey: false,
                            usedAsCriteria: false,
                            canFilter: true,
                            canGroup: true,
                            canSort: true,
                            referenceType: null,
                        fieldRole: 'Dimension',
                        dataType: 'String',
                        defaultAggregation: 'NONE',
                            defaultValue: '',
                            options: [],
                            validations: [],
                            visibilityCondition: null,
                          },
                        ]),
                      }
                    : a
                ),
              }
            : s
        ),
      })),
    updateActivityControl: (stepId, activityId, key, values) =>
      change((d) => ({
        ...d,
        steps: d.steps.map((s) =>
          s.id === stepId
            ? {
                ...s,
                activities: s.activities.map((a) =>
                  a.id === activityId
                    ? {
                        ...a,
                        controls: a.controls.map((c) => (c.id === key ? { ...c, ...values } : c)),
                      }
                    : a
                ),
              }
            : s
        ),
      })),
    removeActivityControl: (stepId, activityId, key) =>
      set((state) => {
        const document = {
          ...state.document,
          steps: state.document.steps.map((step) =>
            step.id === stepId
              ? {
                  ...step,
                  activities: step.activities.map((activity) =>
                    activity.id === activityId
                      ? {
                          ...activity,
                          controls: sequenceControls(
                            activity.controls.filter((control) => control.id !== key)
                          ),
                        }
                      : activity
                  ),
                }
              : step
          ),
        };
        return {
          document,
          selected: selectionForTab(
            document,
            state.selected,
            state.selectedStepId,
            state.centerTab
          ),
          dirty: true,
        };
      }),
    addActivityAction: (stepId, activityId, type = 'approve') =>
      change((d) => ({
        ...d,
        steps: d.steps.map((step) =>
          step.id !== stepId
            ? step
            : {
                ...step,
                activities: step.activities.map((activity) =>
                  activity.id !== activityId
                    ? activity
                    : {
                        ...activity,
                        actions: [
                          ...activity.actions,
                          {
                            id: id(),
                            type,
                            label: i18n.t(`wfProcessBuilder.actionTypes.${type}`),
                            nextStepId: '',
                            condition: null,
                          },
                        ],
                      }
                ),
              }
        ),
      })),
    updateActivityAction: (stepId, activityId, key, values) =>
      change((d) => ({
        ...d,
        steps: d.steps.map((step) =>
          step.id !== stepId
            ? step
            : {
                ...step,
                activities: step.activities.map((activity) =>
                  activity.id !== activityId
                    ? activity
                    : {
                        ...activity,
                        actions: activity.actions.map((action) =>
                          action.id === key ? { ...action, ...values } : action
                        ),
                      }
                ),
              }
        ),
      })),
    removeActivityAction: (stepId, activityId, key) =>
      change((d) => ({
        ...d,
        steps: d.steps.map((step) =>
          step.id !== stepId
            ? step
            : {
                ...step,
                activities: step.activities.map((activity) =>
                  activity.id !== activityId
                    ? activity
                    : {
                        ...activity,
                        actions: activity.actions.filter((action) => action.id !== key),
                      }
                ),
              }
        ),
      })),
    moveStep: (key, direction) =>
      change((d) => {
        const steps = [...d.steps];
        const index = steps.findIndex((x) => x.id === key);
        const target = index + direction;
        if (index < 0 || target < 0 || target >= steps.length) return d;
        [steps[index], steps[target]] = [steps[target], steps[index]];
        return { ...d, steps: steps.map((x, i) => ({ ...x, order: i + 1 })) };
      }),
    reorderSteps: (activeId, overId) =>
      change((d) => {
        const steps = [...d.steps];
        const from = steps.findIndex((x) => x.id === activeId);
        const to = steps.findIndex((x) => x.id === overId);
        if (from < 0 || to < 0 || from === to) return d;
        const [moved] = steps.splice(from, 1);
        steps.splice(to, 0, moved);
        return { ...d, steps: steps.map((x, index) => ({ ...x, order: index + 1 })) };
      }),
    reorderActivities: (stepId, activeId, overId) =>
      change((d) => ({
        ...d,
        steps: d.steps.map((step) => {
          if (step.id !== stepId) return step;
          const activities = [...step.activities];
          const from = activities.findIndex((x) => x.id === activeId);
          const to = activities.findIndex((x) => x.id === overId);
          if (from < 0 || to < 0 || from === to) return step;
          const [moved] = activities.splice(from, 1);
          activities.splice(to, 0, moved);
          return {
            ...step,
            activities: activities.map((activity, index) => ({
              ...activity,
              sortOrder: (index + 1) * 10,
            })),
          };
        }),
      })),
    reorderControls: (stepId, activityId, activeId, overId) =>
      change((d) => ({
        ...d,
        steps: d.steps.map((step) =>
          step.id !== stepId
            ? step
            : {
                ...step,
                activities: step.activities.map((activity) => {
                  if (activity.id !== activityId) return activity;
                  const controls = [...activity.controls];
                  const from = controls.findIndex((x) => x.id === activeId);
                  const to = controls.findIndex((x) => x.id === overId);
                  if (from < 0 || to < 0 || from === to) return activity;
                  const [moved] = controls.splice(from, 1);
                  controls.splice(to, 0, moved);
                  return { ...activity, controls: sequenceControls(controls) };
                }),
              }
        ),
      })),
    addTransition: (trigger) =>
      change((d) => ({
        ...d,
        transitions: [
          ...d.transitions,
          {
            id: id(),
            name: 'New transition',
            sourceStepId: d.steps[0]?.id ?? '',
            targetStepId: d.steps[1]?.id ?? d.steps[0]?.id ?? '',
            variableId: d.variables[0]?.id ?? '',
            operator: '=',
            operatorId: '',
            value: '',
            sortOrder: (d.transitions.length + 1) * 10,
            active: true,
            triggerSource: trigger?.triggerSource ?? 'none',
            triggerId: trigger?.triggerId ?? '',
          },
        ],
      })),
    updateTransition: (key, values) =>
      change((d) => ({
        ...d,
        transitions: d.transitions.map((x) => (x.id === key ? { ...x, ...values } : x)),
      })),
    removeTransition: (key) =>
      change((d) => ({ ...d, transitions: d.transitions.filter((x) => x.id !== key) })),
  };
});
