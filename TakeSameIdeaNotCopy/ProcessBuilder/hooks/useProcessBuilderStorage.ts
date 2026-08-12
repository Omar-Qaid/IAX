/**
 * useProcessBuilderStorage
 * ─────────────────────────────────────────────────────────────────────────────
 * Centralises ALL localStorage persistence for the Process Builder.
 *
 * Responsibilities
 *  - Hydrate context state from localStorage on first render   (new-process mode)
 *  - Hydrate UI state (tabs, selected node) for both new and edit mode
 *  - Auto-save data sections with a debounce                   (new-process mode)
 *  - Persist UI state immediately on every relevant state change
 *  - Defer selected-node restoration until server steps arrive (edit mode)
 *  - Expose helpers: markSaved, saveSection, resetStorage, buildPayload
 *
 * Usage
 *  Call this hook once inside ProcessBuilderPageContent, AFTER calling
 *  useProcessBuilderContext() to obtain the setters.
 */

import { useState, useEffect, useCallback, useRef } from 'react';
import type { SelectedNode, Step, ProcessInfo, ProcessVariable, RequestControl, Transition } from '../types';
import { uid } from '../utils/uid';
import { defaultProcessInfo } from '../context/ProcessBuilderContext';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface UiState {
    selectedNode: SelectedNode;
    leftTab: number;
    centerTab: number;
    expandedSteps: Record<string, boolean>;
}

export interface StorageSetters {
    setProcessInfo: React.Dispatch<React.SetStateAction<ProcessInfo>>;
    setVariables: React.Dispatch<React.SetStateAction<ProcessVariable[]>>;
    setRequestControls: React.Dispatch<React.SetStateAction<RequestControl[]>>;
    setSteps: React.Dispatch<React.SetStateAction<Step[]>>;
    setSelectedNode: React.Dispatch<React.SetStateAction<SelectedNode>>;
    setLeftTab: React.Dispatch<React.SetStateAction<number>>;
    setCenterTab: React.Dispatch<React.SetStateAction<number>>;
    setExpandedSteps: React.Dispatch<React.SetStateAction<Record<string, boolean>>>;
    setTransitions: React.Dispatch<React.SetStateAction<Transition[]>>;
}

export interface StorageData {
    processInfo: ProcessInfo;
    variables: ProcessVariable[];
    requestControls: RequestControl[];
    steps: Step[];
    selectedNode: SelectedNode;
    leftTab: number;
    centerTab: number;
    expandedSteps: Record<string, boolean>;
    transitions: Transition[];
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

/** Validate a SelectedNode against the loaded steps before restoring it. */
function validateNode(node: SelectedNode, steps: Step[]): boolean {
    switch (node.kind) {
        case 'process':
        case 'variable':
        case 'requestControl':
        case 'transition':
            return true;
        case 'step':
            return steps.some((s) => s.id === node.id);
        case 'activity': {
            const step = steps.find((s) => s.id === node.stepId);
            return step?.activities.some((a) => a.id === node.id) ?? false;
        }
        case 'control': {
            const step = steps.find((s) => s.id === node.stepId);
            const act = step?.activities.find((a) => a.id === node.activityId);
            return act?.controls.some((c) => c.id === node.id) ?? false;
        }
        default:
            return false;
    }
}

/** Strip server-only fields that should not be rehydrated into a fresh session. */
function stripServerIds(steps: Step[]): Step[] {
    return steps.map((s) => {
        const step = { ...s };
        delete (step as any).serverId;
        step.activities = step.activities.map((a) => {
            const act = { ...a };
            delete (act as any).serverId;
            act.controls = act.controls.map((c) => {
                const ctrl = { ...c };
                delete (ctrl as any).serverId;
                return ctrl;
            });
            return act;
        });
        return step;
    });
}

// ─── Hook ─────────────────────────────────────────────────────────────────────

export function useProcessBuilderStorage(
    routeId: string | undefined,
    isEditMode: boolean,
    data: StorageData,
    setters: StorageSetters,
) {
    // ── Key helpers ───────────────────────────────────────────────────────────
    const prefix = `processBuilder.${routeId ?? 'new'}.`;
    const key = useCallback((section: string) => `${prefix}${section}`, [prefix]);

    // ── Internal state ────────────────────────────────────────────────────────
    const [savedAt, setSavedAt] = useState<Record<string, string>>({});
    const [hydrated, setHydrated] = useState(false);
    /**
     * Edit mode only: store the node from localStorage until server steps
     * arrive. We then validate and apply it.
     */
    const [pendingNode, setPendingNode] = useState<SelectedNode | null>(null);

    // Keep setters in a ref so effects don't need them in deps arrays
    const settersRef = useRef(setters);
    settersRef.current = setters;

    // ── Read savedAt timestamps ───────────────────────────────────────────────
    const readSavedAt = useCallback(() => {
        try {
            const raw = localStorage.getItem(key('savedAt'));
            if (raw) setSavedAt(JSON.parse(raw));
        } catch { /* ignore */ }
    }, [key]);

    // ── Hydration (runs once on mount) ────────────────────────────────────────
    useEffect(() => {
        const s = settersRef.current;

        if (!isEditMode) {
            // ── New-process mode: restore all data + UI from localStorage ────
            try {
                // processInfo
                const piRaw = localStorage.getItem(key('processInfo'));
                if (piRaw) {
                    const parsed = JSON.parse(piRaw).data as ProcessInfo | undefined;
                    if (parsed) delete (parsed as any).id;
                    s.setProcessInfo(parsed ?? defaultProcessInfo);
                }

                // variables
                const vRaw = localStorage.getItem(key('variables'));
                if (vRaw) {
                    const parsed = JSON.parse(vRaw).data as ProcessVariable[] | undefined;
                    if (Array.isArray(parsed)) parsed.forEach(v => delete (v as any).serverId);
                    s.setVariables(Array.isArray(parsed) ? parsed : []);
                }

                // requestControls
                const rcRaw = localStorage.getItem(key('requestControls'));
                if (rcRaw) {
                    const parsed = JSON.parse(rcRaw).data as RequestControl[] | undefined;
                    if (Array.isArray(parsed)) parsed.forEach(c => delete (c as any).serverId);
                    s.setRequestControls(Array.isArray(parsed) ? parsed : []);
                }

                // transitions
                const tRaw = localStorage.getItem(key('transitions'));
                if (tRaw) {
                    const parsed = JSON.parse(tRaw).data as Transition[] | undefined;
                    if (Array.isArray(parsed)) parsed.forEach(t => delete (t as any).serverId);
                    s.setTransitions(Array.isArray(parsed) ? parsed : []);
                }

                // steps
                let restoredSteps: Step[] | null = null;
                const stRaw = localStorage.getItem(key('steps'));
                if (stRaw) {
                    const parsed = JSON.parse(stRaw).data;
                    if (Array.isArray(parsed)) {
                        restoredSteps = stripServerIds(parsed as Step[]);
                    }
                    s.setSteps(
                        restoredSteps ?? [{
                            id: uid(), name: 'Step 1', order: 1,
                            status: 'pending', assignedUsers: '', assignedRoles: '', activities: [],
                        }]
                    );
                }

                // UI state — restore tabs + selectedNode (validated against restored steps)
                const uiRaw = localStorage.getItem(key('uiState'));
                if (uiRaw) {
                    const ui = JSON.parse(uiRaw) as Partial<UiState>;
                    if (typeof ui.leftTab === 'number')    s.setLeftTab(ui.leftTab);
                    if (typeof ui.centerTab === 'number')  s.setCenterTab(ui.centerTab);
                    if (ui.expandedSteps)                  s.setExpandedSteps(ui.expandedSteps);
                    if (ui.selectedNode) {
                        const ok = validateNode(ui.selectedNode, restoredSteps ?? []);
                        if (ok) s.setSelectedNode(ui.selectedNode);
                    }
                }
            } catch { /* ignore corrupt storage */ }

        } else {
            // ── Edit mode: tabs only now; defer node until server data loads ──
            try {
                const uiRaw = localStorage.getItem(key('uiState'));
                if (uiRaw) {
                    const ui = JSON.parse(uiRaw) as Partial<UiState>;
                    if (typeof ui.leftTab === 'number')    s.setLeftTab(ui.leftTab);
                    if (typeof ui.centerTab === 'number')  s.setCenterTab(ui.centerTab);
                    if (ui.expandedSteps)                  s.setExpandedSteps(ui.expandedSteps);
                    if (ui.selectedNode)                   setPendingNode(ui.selectedNode);
                }
            } catch { /* ignore */ }
        }

        readSavedAt();
        setHydrated(true);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // ── Edit mode: apply pendingNode once server steps arrive ─────────────────
    useEffect(() => {
        if (!pendingNode || data.steps.length === 0) return;
        if (validateNode(pendingNode, data.steps)) {
            settersRef.current.setSelectedNode(pendingNode);
            setPendingNode(null);
        }
    }, [data.steps, pendingNode]);

    // ── Write helpers ─────────────────────────────────────────────────────────

    const markSaved = useCallback((section: string) => {
        setSavedAt((prev) => {
            const next = { ...prev, [section]: new Date().toISOString() };
            try { localStorage.setItem(key('savedAt'), JSON.stringify(next)); } catch { /* quota */ }
            return next;
        });
    }, [key]);

    const saveSection = useCallback((section: string, payload: unknown) => {
        try {
            localStorage.setItem(key(section), JSON.stringify({
                data: payload,
                savedAt: new Date().toISOString(),
            }));
            markSaved(section);
        } catch { /* quota */ }
    }, [key, markSaved]);

    // ── Auto-save data sections (new-process mode only, debounced) ────────────
    const useAutoSave = (section: string, payload: unknown, delay = 600) => {
         
        useEffect(() => {
            if (!hydrated || isEditMode) return;
            const timer = setTimeout(() => saveSection(section, payload), delay);
            return () => clearTimeout(timer);
            // eslint-disable-next-line react-hooks/exhaustive-deps
        }, [hydrated, JSON.stringify(payload)]);
    };

    useAutoSave('processInfo',      data.processInfo);
    useAutoSave('variables',        data.variables);
    useAutoSave('requestControls',  data.requestControls);
    useAutoSave('steps',            data.steps);
    useAutoSave('transitions',      data.transitions);

    // ── Persist UI state on every relevant change ─────────────────────────────
    useEffect(() => {
        if (!hydrated) return;
        try {
            localStorage.setItem(key('uiState'), JSON.stringify({
                selectedNode:  data.selectedNode,
                leftTab:       data.leftTab,
                centerTab:     data.centerTab,
                expandedSteps: data.expandedSteps,
            } satisfies UiState));
        } catch { /* quota */ }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [hydrated, data.selectedNode, data.leftTab, data.centerTab, data.expandedSteps]);

    // ── Reset ─────────────────────────────────────────────────────────────────
    const resetStorage = useCallback(() => {
        Object.keys(localStorage)
            .filter((k) => k.startsWith(prefix))
            .forEach((k) => localStorage.removeItem(k));
        setSavedAt({});
    }, [prefix]);

    // ── Export / build payload ────────────────────────────────────────────────
    const buildPayload = useCallback(() => ({
        processInfo:     data.processInfo,
        variables:       data.variables,
        requestControls: data.requestControls,
        steps:           data.steps,
        transitions:     data.transitions,
        sectionSavedAt:  savedAt,
    }), [data, savedAt]);

    return {
        hydrated,
        savedAt,
        saveSection,
        markSaved,
        resetStorage,
        buildPayload,
    };
}
