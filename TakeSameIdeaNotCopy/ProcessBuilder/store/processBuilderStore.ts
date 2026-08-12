import { create } from 'zustand';
import type { ProcessInfo, Step, RequestControl, ProcessVariable, SelectedNode, Transition, RequestControlsValidation } from '../types';
import { numberSequenceService } from '../../../../system/number-sequences/api/numberSequenceService';
import { uid } from '../utils/uid';

export const defaultProcessInfo: ProcessInfo = {
    code: '',
    name: '',
    nameAR: '',
    description: '',
    descriptionAR: '',
    categoryId: '',
    priorityId: '',
    processType: 'Process',
    score: 100,
    canRepeat: false,
    mandatoryDocs: false,
    isActive: true,
    isDeleted: false,
};

export interface ProcessBuilderState {
    processInfo: ProcessInfo;
    variables: ProcessVariable[];
    requestControls: RequestControl[];
    steps: Step[];
    selectedNode: SelectedNode;
    leftTab: number;
    centerTab: number;
    expandedSteps: Record<string, boolean>;
    transitions: Transition[];
    /** Request/process transition server-ids removed since last save (request flow). */
    deletedTransitionServerIds: number[];
    /** Activity-control transition entries removed since last save (activity flow). Tupled with activityControlId so saves can scope to the current activity. */
    deletedActivityTransitionEntries: { serverId: number; activityControlId: string }[];
    requestControlsValidations: RequestControlsValidation[];
    /** Request-control validation server-ids removed since last save (request flow). */
    deletedRequestControlsValidationServerIds: number[];
    /** Local requestControlIds whose validations were deleted — used to rebuild XML after save. */
    deletedRequestControlsValidationControlIds: string[];
    /** Activity-control validation entries removed since last save (activity flow). Tupled with activityControlId so saves can scope to the current activity. */
    deletedActivityControlsValidationEntries: { serverId: number; activityControlId: string }[];
    deletedVariableServerIds: number[];
    deletedStepServerIds: number[];
    deletedRequestControlServerIds: number[];

    setProcessInfo: (updater: ProcessInfo | ((prev: ProcessInfo) => ProcessInfo)) => void;
    setVariables: (updater: ProcessVariable[] | ((prev: ProcessVariable[]) => ProcessVariable[])) => void;
    setRequestControls: (updater: RequestControl[] | ((prev: RequestControl[]) => RequestControl[])) => void;
    setSteps: (updater: Step[] | ((prev: Step[]) => Step[])) => void;
    setSelectedNode: (updater: SelectedNode | ((prev: SelectedNode) => SelectedNode)) => void;
    setLeftTab: (updater: number | ((prev: number) => number)) => void;
    setCenterTab: (updater: number | ((prev: number) => number)) => void;
    setExpandedSteps: (updater: Record<string, boolean> | ((prev: Record<string, boolean>) => Record<string, boolean>)) => void;
    setTransitions: (updater: Transition[] | ((prev: Transition[]) => Transition[])) => void;
    setRequestControlsValidations: (updater: RequestControlsValidation[] | ((prev: RequestControlsValidation[]) => RequestControlsValidation[])) => void;

    updateStep: (stepId: string, patch: Partial<Step>) => void;
    deleteStep: (stepId: string) => void;
    addStep: () => void;

    addVariable: () => void;
    updateVariable: (id: string, patch: Partial<ProcessVariable>) => void;
    deleteVariable: (id: string) => void;

    addRequestControl: (type: import('../types').ControlType, seqCode?: string, resolvedControlId?: number) => void;
    updateRequestControl: (id: string, patch: Partial<RequestControl>) => void;
    deleteRequestControl: (id: string) => void;

    addActivity: (stepId: string, type: import('../types').ActivityType, seqCode?: string) => void;
    updateActivity: (stepId: string, id: string, patch: Partial<import('../types').Activity>) => void;
    deleteActivity: (stepId: string, id: string) => void;

    addControl: (stepId: string, activityId: string, type: import('../types').ControlType, seqCode?: string) => void;
    updateControl: (stepId: string, activityId: string, id: string, patch: Partial<RequestControl>) => void;
    deleteControl: (stepId: string, activityId: string, id: string, serverId?: number) => void;

    addTransition: () => void;
    updateTransition: (id: string, patch: Partial<Transition>) => void;
    deleteTransition: (id: string) => void;

    addRequestControlsValidation: (requestControlId: string, initial?: Partial<RequestControlsValidation>) => void;
    addActivityControlsValidation: (activityControlId: string, initial?: Partial<RequestControlsValidation>) => void;
    updateRequestControlsValidation: (id: string, patch: Partial<RequestControlsValidation>) => void;
    deleteRequestControlsValidation: (id: string) => void;
}

export const useProcessBuilderStore = create<ProcessBuilderState>((set) => ({
    processInfo: defaultProcessInfo,
    variables: [],
    requestControls: [],
    steps: [{
        id: uid(),
        name: 'Step 1',
        order: 1,
        status: 'pending',
        assignedUsers: '',
        assignedRoles: '',
        activities: [],
    }],
    selectedNode: { kind: 'process' },
    leftTab: 0,
    centerTab: 0,
    expandedSteps: {},
    transitions: [],
    deletedTransitionServerIds: [],
    deletedActivityTransitionEntries: [],
    requestControlsValidations: [],
    deletedRequestControlsValidationServerIds: [],
    deletedRequestControlsValidationControlIds: [],
    deletedActivityControlsValidationEntries: [],
    deletedVariableServerIds: [],
    deletedStepServerIds: [],
    deletedRequestControlServerIds: [],

    setProcessInfo: (updater) => set((state) => ({ processInfo: typeof updater === 'function' ? updater(state.processInfo) : updater })),
    setVariables: (updater) => set((state) => ({ variables: typeof updater === 'function' ? updater(state.variables) : updater })),
    setRequestControls: (updater) => set((state) => ({ requestControls: typeof updater === 'function' ? updater(state.requestControls) : updater })),
    setSteps: (updater) => set((state) => ({ steps: typeof updater === 'function' ? updater(state.steps) : updater })),
    setSelectedNode: (updater) => set((state) => ({ selectedNode: typeof updater === 'function' ? updater(state.selectedNode) : updater })),
    setLeftTab: (updater) => set((state) => ({ leftTab: typeof updater === 'function' ? updater(state.leftTab) : updater })),
    setCenterTab: (updater) => set((state) => ({ centerTab: typeof updater === 'function' ? updater(state.centerTab) : updater })),
    setExpandedSteps: (updater) => set((state) => ({ expandedSteps: typeof updater === 'function' ? updater(state.expandedSteps) : updater })),
    setTransitions: (updater) => set((state) => ({ transitions: typeof updater === 'function' ? updater(state.transitions) : updater })),
    setRequestControlsValidations: (updater) => set((state) => ({ requestControlsValidations: typeof updater === 'function' ? updater(state.requestControlsValidations) : updater })),

    updateStep: (id, patch) => set((state) => ({
        steps: state.steps.map((s) => (s.id === id ? { ...s, dirty: true, ...patch } : s))
    })),

    deleteStep: (stepId) => set((state) => {
        const match = state.steps.find((s) => s.id === stepId);
        const newSteps = state.steps.filter((s) => s.id !== stepId);
        const newSelected = state.selectedNode.kind === 'step' && state.selectedNode.id === stepId ? { kind: 'process' as const } : state.selectedNode;
        if (match?.serverId) {
            return {
                steps: newSteps,
                selectedNode: newSelected,
                deletedStepServerIds: [...state.deletedStepServerIds, match.serverId]
            };
        }
        return { steps: newSteps, selectedNode: newSelected };
    }),

    addStep: () => {
        const newId = uid();
        set((state) => {
            const s: Step = {
                id: newId, code: '', name: `Step ${state.steps.length + 1}`, order: state.steps.length + 1,
                status: 'pending', assignedUsers: '', assignedRoles: '', activities: [], dirty: true,
            };
            return {
                steps: [...state.steps, s],
                expandedSteps: { ...state.expandedSteps, [newId]: true },
                selectedNode: { kind: 'step', id: newId }
            };
        });
        numberSequenceService.next('WfStep').then(seq => {
            if (seq && seq.code) {
                set(state => ({ steps: state.steps.map(v => v.id === newId ? { ...v, code: seq.code! } : v) }));
            }
        }).catch(() => { });
    },

    addVariable: () => {
        const newId = uid();
        set((state) => {
            const v: ProcessVariable = {
                id: newId, code: '', name: `Var ${state.variables.length + 1}`, nameAR: '',
                description: '', descriptionAR: '', dataTypeId: '', sortOrder: (state.variables.length + 1) * 10,
                isActive: true, required: false, scope: 'process', dataType: 'string', defaultValue: '', dirty: true,
            };
            return {
                variables: [...state.variables, v],
                selectedNode: { kind: 'variable', id: newId }
            };
        });
        numberSequenceService.next('WfVariable').then(seq => {
            if (seq && seq.code) {
                set(state => ({ variables: state.variables.map(v => v.id === newId ? { ...v, code: seq.code! } : v) }));
            }
        }).catch(() => { });
    },

    updateVariable: (id, patch) => set((state) => ({
        variables: state.variables.map((v) => (v.id === id ? { ...v, dirty: true, ...patch } : v))
    })),

    deleteVariable: (id) => set((state) => {
        const match = state.variables.find((v) => v.id === id);
        const newVariables = state.variables.filter((v) => v.id !== id).map((v, i) => ({ ...v, sortOrder: (i + 1) * 10, dirty: true }));
        const newSelected = state.selectedNode.kind === 'variable' && state.selectedNode.id === id ? { kind: 'process' as const } : state.selectedNode;
        if (match?.serverId) {
            return {
                variables: newVariables,
                selectedNode: newSelected,
                deletedVariableServerIds: [...state.deletedVariableServerIds, match.serverId]
            };
        }
        return { variables: newVariables, selectedNode: newSelected };
    }),

    addRequestControl: (type, seqCode, resolvedControlId) => {
        const newId = uid();
        set((state) => {
            const c: RequestControl = {
                id: newId, code: seqCode ?? '', label: `New ${type}`, labelAR: '', name: `req_field_${state.requestControls.length + 1}`,
                type, controlId: resolvedControlId, required: false, readOnly: false, visible: true,
                mandatory: false, uniqueKey: false, usedAsCriteria: false, score: 0, sortOrder: (state.requestControls.length + 1) * 10,
                defaultValue: '', options: type === 'dropdown' ? ['Option 1', 'Option 2'] : undefined,
                optionsX: (type === 'dropdown' || type === 'dropdown-manual' || type === 'checkboxlist' || type === 'radiobuttonlist') ? [
                    { ar: 'نعم', en: 'Yes', value: 'نعم' }, { ar: 'لا', en: 'No', value: 'لا' },
                ] : undefined,
                gridRows: (type === 'grid' || type === 'table') ? [['اسم الصنف', 'كود الصنف', 'الكمية'], ['', '', ''], ['', '', '']] : undefined,
                color: type === 'color' ? '#ff0000' : undefined, validations: [], dirty: true,
            };
            return {
                requestControls: [...state.requestControls, c],
                selectedNode: { kind: 'requestControl', id: newId }
            };
        });
        numberSequenceService.next('WfRequestControl').then(seq => {
            if (seq && seq.code) {
                set(state => ({ requestControls: state.requestControls.map(v => v.id === newId ? { ...v, code: seq.code! } : v) }));
            }
        }).catch(() => { });
    },

    updateRequestControl: (id, patch) => set((state) => ({
        requestControls: state.requestControls.map((c) => (c.id === id ? { ...c, dirty: true, ...patch } : c))
    })),

    deleteRequestControl: (id) => set((state) => {
        const match = state.requestControls.find((x) => x.id === id);
        const newControls = state.requestControls.filter((x) => x.id !== id).map((x, i) => ({ ...x, sortOrder: (i + 1) * 10, dirty: true }));
        const newSelected = state.selectedNode.kind === 'requestControl' && state.selectedNode.id === id ? { kind: 'process' as const } : state.selectedNode;
        if (match?.serverId) {
            return {
                requestControls: newControls,
                selectedNode: newSelected,
                deletedRequestControlServerIds: [...state.deletedRequestControlServerIds, match.serverId]
            };
        }
        return { requestControls: newControls, selectedNode: newSelected };
    }),

    addActivity: (stepId, type, seqCode) => {
        const newId = uid();
        set((state) => {
            const a: import('../types').Activity = {
                id: newId, code: seqCode ?? '', name: `${type} activity`, nameAR: '', type, isActive: true,
                activityTypeId: '', performerId: '', score: 0, sysNotificationTemplateId: '', alertingBySystem: false, alertingByEmail: false, alertingBySms: false, alertingByWhatsApp: false,
                showPreviousSteps: false, showPreviousDocs: false, mandatoryDocs: false, autoPassEnabled: false, autoPassingHrs: 0,
                dirty: true, controls: [], actions: [], validations: [],
                assignedUsers: '', assignedRoles: '', assignmentMode: 'any', config: {},
            };
            return {
                steps: state.steps.map((s) => s.id !== stepId ? s : { ...s, activities: [...s.activities, a] }),
                selectedNode: { kind: 'activity', stepId, id: a.id }
            };
        });
    },

    updateActivity: (stepId, id, patch) => set((state) => ({
        steps: state.steps.map((s) => s.id !== stepId ? s : { ...s, activities: s.activities.map((a) => (a.id === id ? { ...a, dirty: true, ...patch } : a)) })
    })),

    deleteActivity: (stepId, id) => set((state) => {
        const newSteps = state.steps.map((s) => s.id !== stepId ? s : { ...s, activities: s.activities.filter((x) => x.id !== id) });
        const newSelected = state.selectedNode.kind === 'activity' && state.selectedNode.id === id ? { kind: 'step' as const, id: stepId } : state.selectedNode;
        return { steps: newSteps, selectedNode: newSelected };
    }),

    addControl: (stepId, activityId, type, seqCode) => {
        const newId = uid();
        set((state) => {
            const step = state.steps.find((s) => s.id === stepId);
            const activity = step?.activities.find((a) => a.id === activityId);
            if (!activity) return state;
            const c: RequestControl = {
                id: newId, code: seqCode ?? '', label: `New ${type}`, name: `field_${activity.controls.length + 1}`,
                type, required: false, readOnly: false, visible: true, defaultValue: '',
                options: type === 'dropdown' ? ['Option 1', 'Option 2'] : undefined, validations: [], dirty: true,
            };
            return {
                steps: state.steps.map((s) => s.id !== stepId ? s : {
                    ...s, activities: s.activities.map((a) => a.id !== activityId ? a : {
                        ...a, controls: [...a.controls, c], dirty: true
                    })
                }),
                selectedNode: { kind: 'control', stepId, activityId, id: newId }
            };
        });
        numberSequenceService.next('WfActivityControl').then(seq => {
            if (seq && seq.code) {
                set(state => ({
                    steps: state.steps.map(s => s.id === stepId ? {
                        ...s, activities: s.activities.map(a => a.id === activityId ? {
                            ...a, controls: a.controls.map(c => c.id === newId ? { ...c, code: seq.code! } : c)
                        } : a)
                    } : s)
                }));
            }
        }).catch(() => { });
    },

    updateControl: (stepId, activityId, id, patch) => set((state) => ({
        steps: state.steps.map((s) => s.id !== stepId ? s : {
            ...s, activities: s.activities.map((a) => a.id !== activityId ? a : {
                ...a, controls: a.controls.map((c) => (c.id === id ? { ...c, dirty: true, ...patch } : c)), dirty: true
            })
        })
    })),

    deleteControl: (stepId, activityId, id, serverId) => set((state) => {
        const newSteps = state.steps.map((s) => s.id !== stepId ? s : {
            ...s, activities: s.activities.map((a) => a.id !== activityId ? a : {
                ...a, controls: a.controls.filter((c) => c.id !== id),
                deletedControlServerIds: serverId ? [...(a.deletedControlServerIds ?? []), serverId] : a.deletedControlServerIds, dirty: true
            })
        });
        const newSelected = state.selectedNode.kind === 'control' && state.selectedNode.id === id ? { kind: 'activity' as const, stepId, id: activityId } : state.selectedNode;
        return { steps: newSteps, selectedNode: newSelected };
    }),

    addTransition: () => {
        const newId = uid();
        set((state) => {
            const t: Transition = {
                id: newId,
                processId: state.processInfo.id,
                variableId: '',
                operatorId: '',
                value: '',
                stepId: '',
                sortOrder: (state.transitions.length + 1) * 10,
                isActive: true,
                dirty: true,
            };
            return {
                transitions: [...state.transitions, t],
                selectedNode: { kind: 'transition', id: newId }
            };
        });
    },

    updateTransition: (id, patch) => set((state) => ({
        transitions: state.transitions.map((t) => (t.id === id ? { ...t, dirty: true, ...patch } : t))
    })),

    deleteTransition: (id) => set((state) => {
        const match = state.transitions.find(t => t.id === id);
        const isActivityScope = !!match?.activityControlId;
        // Reindex sortOrder only within the same flow, so deleting a request
        // transition never dirties activity-control transitions (and vice-versa).
        let idx = 0;
        const newTransitions = state.transitions
            .filter((t) => t.id !== id)
            .map((t) => {
                if (!!t.activityControlId !== isActivityScope) return t;
                idx += 1;
                return { ...t, sortOrder: idx * 10, dirty: true };
            });
        const newSelected = state.selectedNode.kind === 'transition' && state.selectedNode.id === id ? { kind: 'process' as const } : state.selectedNode;
        if (!match?.serverId) {
            return { transitions: newTransitions, selectedNode: newSelected };
        }
        // Route the removed server-id to the correct flow's deletion bucket.
        return isActivityScope
            ? { transitions: newTransitions, selectedNode: newSelected, deletedActivityTransitionEntries: [...state.deletedActivityTransitionEntries, { serverId: match.serverId, activityControlId: match.activityControlId! }] }
            : { transitions: newTransitions, selectedNode: newSelected, deletedTransitionServerIds: [...state.deletedTransitionServerIds, match.serverId] };
    }),

    addRequestControlsValidation: (requestControlId, initial) => {
        const newId = uid();
        set((state) => {
            const count = state.requestControlsValidations.filter(x => x.requestControlId === requestControlId).length;
            const v: RequestControlsValidation = {
                id: newId,
                requestControlId,
                validationType: 'Required',
                validationExpression: '',
                operator: '',
                value: '',
                maskInput: '',
                errorMessageAr: '',
                errorMessageEn: '',
                severity: 'Error',
                sortOrder: (count + 1) * 10,
                isActive: true,
                dirty: true,
                ...initial,
            };
            return {
                requestControlsValidations: [...state.requestControlsValidations, v]
            };
        });
    },

    addActivityControlsValidation: (activityControlId, initial) => {
        const newId = uid();
        set((state) => {
            const count = state.requestControlsValidations.filter(x => x.activityControlId === activityControlId).length;
            const v: RequestControlsValidation = {
                id: newId,
                requestControlId: '',
                activityControlId,
                validationType: 'Required',
                validationExpression: '',
                operator: '',
                value: '',
                maskInput: '',
                errorMessageAr: '',
                errorMessageEn: '',
                severity: 'Error',
                sortOrder: (count + 1) * 10,
                isActive: true,
                dirty: true,
                ...initial,
            };
            return {
                requestControlsValidations: [...state.requestControlsValidations, v]
            };
        });
    },

    updateRequestControlsValidation: (id, patch) => set((state) => ({
        requestControlsValidations: state.requestControlsValidations.map((v) => (v.id === id ? { ...v, dirty: true, ...patch } : v))
    })),

    deleteRequestControlsValidation: (id) => set((state) => {
        const match = state.requestControlsValidations.find(v => v.id === id);
        const newValidations = state.requestControlsValidations.filter((v) => v.id !== id);
        if (!match?.serverId) {
            return { requestControlsValidations: newValidations };
        }
        // Route the removed server-id to the correct flow's deletion bucket.
        return match.activityControlId
            ? { requestControlsValidations: newValidations, deletedActivityControlsValidationEntries: [...state.deletedActivityControlsValidationEntries, { serverId: match.serverId, activityControlId: match.activityControlId }] }
            : {
                requestControlsValidations: newValidations,
                deletedRequestControlsValidationServerIds: [...state.deletedRequestControlsValidationServerIds, match.serverId],
                deletedRequestControlsValidationControlIds: [...state.deletedRequestControlsValidationControlIds, match.requestControlId],
            };
    })
}));
