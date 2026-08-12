import { create } from 'zustand';
import type { BuilderActivity, BuilderActivityAction, BuilderControl, BuilderNode, BuilderStep, BuilderTransition, BuilderVariable, ProcessBuilderDocument } from '../types/processBuilderTypes';

const id = () => crypto.randomUUID();
export const createProcessBuilderDocument = (builderId = 'new'): ProcessBuilderDocument => ({
  id: builderId, code: builderId === 'new' ? 'PB-DRAFT' : `PB-${builderId}`, name: 'New approval process', nameAR: 'عملية اعتماد جديدة', description: 'Configure the request, steps, activities, and routing conditions.', active: true,
  variables: [{ id: id(), code: 'AMOUNT', name: 'Request amount', nameAR: 'قيمة الطلب', description: '', descriptionAR: '', dataType: 'number', sortOrder: 10, required: true, active: true, scope: 'process', defaultValue: '0' }],
  requestControls: [{ id: id(), code: 'RCTL-0001', label: 'Request title', labelAR: 'عنوان الطلب', type: 'text', required: true, readOnly: false, visible: true, uniqueKey: false, usedAsCriteria: false, defaultValue: '', options: [], validations: [], visibilityCondition: null }],
  steps: [{ id: id(), code: 'STEP-00001', name: 'Manager review', nameAR: 'مراجعة المدير', order: 1, score: 0, autoPassingHours: 0, allMandatory: true, active: true, systemField: false, condition: null, activities: [] }], transitions: [],
});

interface State {
  document: ProcessBuilderDocument;
  selected: BuilderNode;
  leftTab: number;
  centerTab: number;
  dirty: boolean;
  markDraftSaved: () => void;
  initialize: (document: ProcessBuilderDocument) => void;
  select: (node: BuilderNode) => void;
  setLeftTab: (value: number) => void;
  setCenterTab: (value: number) => void;
  updateProcess: (values: Partial<ProcessBuilderDocument>) => void;
  addVariable: () => void;
  updateVariable: (id: string, values: Partial<BuilderVariable>) => void;
  removeVariable: (id: string) => void;
  reorderVariables: (activeId: string, overId: string) => void;
  addRequestControl: (type?: BuilderControl['type']) => void;
  updateRequestControl: (id: string, values: Partial<BuilderControl>) => void;
  removeRequestControl: (id: string) => void;
  reorderRequestControls: (activeId: string, overId: string) => void;
  addStep: () => void;
  updateStep: (id: string, values: Partial<BuilderStep>) => void;
  removeStep: (id: string) => void;
  addActivity: (stepId: string, type?: BuilderActivity['type']) => void;
  updateActivity: (stepId: string, id: string, values: Partial<BuilderActivity>) => void;
  removeActivity: (stepId: string, id: string) => void;
  addActivityControl: (stepId: string, activityId: string, type?: BuilderControl['type']) => void;
  updateActivityControl: (stepId: string, activityId: string, id: string, values: Partial<BuilderControl>) => void;
  removeActivityControl: (stepId: string, activityId: string, id: string) => void;
  addActivityAction: (stepId: string, activityId: string, type?: BuilderActivityAction['type']) => void;
  updateActivityAction: (stepId: string, activityId: string, id: string, values: Partial<BuilderActivityAction>) => void;
  removeActivityAction: (stepId: string, activityId: string, id: string) => void;
  moveStep: (id: string, direction: -1 | 1) => void;
  reorderSteps: (activeId: string, overId: string) => void;
  reorderActivities: (stepId: string, activeId: string, overId: string) => void;
  reorderControls: (stepId: string, activityId: string, activeId: string, overId: string) => void;
  addTransition: () => void;
  updateTransition: (id: string, values: Partial<BuilderTransition>) => void;
  removeTransition: (id: string) => void;
}

export const useProcessBuilderStore = create<State>((set) => {
  const change = (mutate: (document: ProcessBuilderDocument) => ProcessBuilderDocument) => set((state) => ({ document: mutate(state.document), dirty: true }));
  return {
    document: createProcessBuilderDocument(), selected: { kind: 'process' }, leftTab: 0, centerTab: 0, dirty: false,
    initialize: (document) => set({ document, selected: { kind: 'process' }, leftTab: 0, centerTab: 0, dirty: false }), markDraftSaved: () => set({ dirty: false }),
    select: (selected) => set({ selected }), setLeftTab: (leftTab) => set({ leftTab }), setCenterTab: (centerTab) => set({ centerTab }),
    updateProcess: (values) => change((document) => ({ ...document, ...values })),
    addVariable: () => change((d) => ({ ...d, variables: [...d.variables, { id: id(), code: `VAR${d.variables.length + 1}`, name: 'New variable', nameAR: 'متغير جديد', description: '', descriptionAR: '', dataType: 'text', sortOrder: (d.variables.length + 1) * 10, required: false, active: true, scope: 'process', defaultValue: '' }] })),
    updateVariable: (key, values) => change((d) => ({ ...d, variables: d.variables.map((x) => x.id === key ? { ...x, ...values } : x) })),
    removeVariable: (key) => change((d) => ({ ...d, variables: d.variables.filter((x) => x.id !== key) })),
    reorderVariables: (activeId, overId) => change((d) => { const variables = [...d.variables]; const from = variables.findIndex((x) => x.id === activeId); const to = variables.findIndex((x) => x.id === overId); if (from < 0 || to < 0 || from === to) return d; const [moved] = variables.splice(from, 1); variables.splice(to, 0, moved); return { ...d, variables: variables.map((variable, index) => ({ ...variable, sortOrder: (index + 1) * 10 })) }; }),
    addRequestControl: (type = 'text') => change((d) => ({ ...d, requestControls: [...d.requestControls, { id: id(), code: `RCTL-${String(d.requestControls.length + 1).padStart(4, '0')}`, label: 'New field', labelAR: 'حقل جديد', type, required: false, readOnly: false, visible: true, uniqueKey: false, usedAsCriteria: false, defaultValue: '', options: [], validations: [], visibilityCondition: null }] })),
    updateRequestControl: (key, values) => change((d) => ({ ...d, requestControls: d.requestControls.map((x) => x.id === key ? { ...x, ...values } : x) })),
    removeRequestControl: (key) => change((d) => ({ ...d, requestControls: d.requestControls.filter((x) => x.id !== key) })),
    reorderRequestControls: (activeId, overId) => change((d) => { const controls = [...d.requestControls]; const from = controls.findIndex((x) => x.id === activeId); const to = controls.findIndex((x) => x.id === overId); if (from < 0 || to < 0 || from === to) return d; const [moved] = controls.splice(from, 1); controls.splice(to, 0, moved); return { ...d, requestControls: controls.map((control, index) => ({ ...control, code: `RCTL-${String(index + 1).padStart(4, '0')}` })) }; }),
    addStep: () => change((d) => ({ ...d, steps: [...d.steps, { id: id(), code: `STEP-${String(d.steps.length + 1).padStart(5, '0')}`, name: `Step ${d.steps.length + 1}`, nameAR: `الخطوة ${d.steps.length + 1}`, order: d.steps.length + 1, score: 0, autoPassingHours: 0, allMandatory: false, active: true, systemField: false, condition: null, activities: [] }] })),
    updateStep: (key, values) => change((d) => ({ ...d, steps: d.steps.map((x) => x.id === key ? { ...x, ...values } : x) })),
    removeStep: (key) => change((d) => ({ ...d, steps: d.steps.filter((x) => x.id !== key).map((x, i) => ({ ...x, order: i + 1 })) })),
    addActivity: (stepId, type = 'approval') => change((d) => ({ ...d, steps: d.steps.map((s) => s.id === stepId ? { ...s, activities: [...s.activities, { id: id(), code: `ACT-${String(s.activities.length + 1).padStart(5, '0')}`, name: 'New activity', nameAR: 'نشاط جديد', type, performer: '', assignmentMode: 'any', active: true, required: true, mandatoryDocs: false, autoPassEnabled: false, autoPassingHours: 0, controls: [], actions: [], validations: [], condition: null, config: { apiMethod: 'GET', apiUrl: '', notifyEmails: '' } }] } : s) })),
    updateActivity: (stepId, key, values) => change((d) => ({ ...d, steps: d.steps.map((s) => s.id === stepId ? { ...s, activities: s.activities.map((a) => a.id === key ? { ...a, ...values } : a) } : s) })),
    removeActivity: (stepId, key) => change((d) => ({ ...d, steps: d.steps.map((s) => s.id === stepId ? { ...s, activities: s.activities.filter((a) => a.id !== key) } : s) })),
    addActivityControl: (stepId, activityId, type = 'text') => change((d) => ({ ...d, steps: d.steps.map((s) => s.id === stepId ? { ...s, activities: s.activities.map((a) => a.id === activityId ? { ...a, controls: [...a.controls, { id: id(), code: `CTRL-${String(a.controls.length + 1).padStart(4, '0')}`, label: 'New field', labelAR: 'حقل جديد', type, required: false, readOnly: false, visible: true, uniqueKey: false, usedAsCriteria: false, defaultValue: '', options: [], validations: [], visibilityCondition: null }] } : a) } : s) })),
    updateActivityControl: (stepId, activityId, key, values) => change((d) => ({ ...d, steps: d.steps.map((s) => s.id === stepId ? { ...s, activities: s.activities.map((a) => a.id === activityId ? { ...a, controls: a.controls.map((c) => c.id === key ? { ...c, ...values } : c) } : a) } : s) })),
    removeActivityControl: (stepId, activityId, key) => change((d) => ({ ...d, steps: d.steps.map((s) => s.id === stepId ? { ...s, activities: s.activities.map((a) => a.id === activityId ? { ...a, controls: a.controls.filter((c) => c.id !== key) } : a) } : s) })),
    addActivityAction: (stepId, activityId, type = 'approve') => change((d) => ({ ...d, steps: d.steps.map((step) => step.id !== stepId ? step : { ...step, activities: step.activities.map((activity) => activity.id !== activityId ? activity : { ...activity, actions: [...activity.actions, { id: id(), type, label: type[0].toUpperCase() + type.slice(1), nextStepId: '', condition: null }] }) }) })),
    updateActivityAction: (stepId, activityId, key, values) => change((d) => ({ ...d, steps: d.steps.map((step) => step.id !== stepId ? step : { ...step, activities: step.activities.map((activity) => activity.id !== activityId ? activity : { ...activity, actions: activity.actions.map((action) => action.id === key ? { ...action, ...values } : action) }) }) })),
    removeActivityAction: (stepId, activityId, key) => change((d) => ({ ...d, steps: d.steps.map((step) => step.id !== stepId ? step : { ...step, activities: step.activities.map((activity) => activity.id !== activityId ? activity : { ...activity, actions: activity.actions.filter((action) => action.id !== key) }) }) })),
    moveStep: (key, direction) => change((d) => { const steps = [...d.steps]; const index = steps.findIndex((x) => x.id === key); const target = index + direction; if (index < 0 || target < 0 || target >= steps.length) return d; [steps[index], steps[target]] = [steps[target], steps[index]]; return { ...d, steps: steps.map((x, i) => ({ ...x, order: i + 1 })) }; }),
    reorderSteps: (activeId, overId) => change((d) => { const steps = [...d.steps]; const from = steps.findIndex((x) => x.id === activeId); const to = steps.findIndex((x) => x.id === overId); if (from < 0 || to < 0 || from === to) return d; const [moved] = steps.splice(from, 1); steps.splice(to, 0, moved); return { ...d, steps: steps.map((x, index) => ({ ...x, order: index + 1 })) }; }),
    reorderActivities: (stepId, activeId, overId) => change((d) => ({ ...d, steps: d.steps.map((step) => { if (step.id !== stepId) return step; const activities = [...step.activities]; const from = activities.findIndex((x) => x.id === activeId); const to = activities.findIndex((x) => x.id === overId); if (from < 0 || to < 0 || from === to) return step; const [moved] = activities.splice(from, 1); activities.splice(to, 0, moved); return { ...step, activities }; }) })),
    reorderControls: (stepId, activityId, activeId, overId) => change((d) => ({ ...d, steps: d.steps.map((step) => step.id !== stepId ? step : { ...step, activities: step.activities.map((activity) => { if (activity.id !== activityId) return activity; const controls = [...activity.controls]; const from = controls.findIndex((x) => x.id === activeId); const to = controls.findIndex((x) => x.id === overId); if (from < 0 || to < 0 || from === to) return activity; const [moved] = controls.splice(from, 1); controls.splice(to, 0, moved); return { ...activity, controls }; }) }) })),
    addTransition: () => change((d) => ({ ...d, transitions: [...d.transitions, { id: id(), name: 'New transition', sourceStepId: d.steps[0]?.id ?? '', targetStepId: d.steps[1]?.id ?? '', variableId: d.variables[0]?.id ?? '', operator: '=', value: '', sortOrder: (d.transitions.length + 1) * 10, active: true, triggerSource: 'none', triggerId: '' }] })),
    updateTransition: (key, values) => change((d) => ({ ...d, transitions: d.transitions.map((x) => x.id === key ? { ...x, ...values } : x) })),
    removeTransition: (key) => change((d) => ({ ...d, transitions: d.transitions.filter((x) => x.id !== key) })),
  };
});
