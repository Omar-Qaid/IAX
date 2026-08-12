import { useEffect, useRef } from 'react';
import { useProcessBuilderQueries } from './useProcessBuilderQueries';
import { useProcessBuilderContext } from '../context/ProcessBuilderContext';
import { useProcessBuilderSaves } from './useProcessBuilderSaves';
import {
    mapDtoToInfo, mapWfStepToLocal, mapWfActivityToLocal, mapWfActivityControlToLocal,
    mapWfVariableToLocal, mapWfRequestControlToLocal, mapWfTransitionToLocal,
    mapWfRequestControlsValidationToLocal
} from '../utils/ProcessBuilderMappers';
import { uid } from '../utils/uid';

export const useProcessBuilderData = (processId: number | string | undefined, isEditMode: boolean, disableHydration: boolean = false) => {
    const {
        processInfo, setProcessInfo,
        setSteps, setVariables, setRequestControls,
        setTransitions, variables, requestControls, steps,
        setRequestControlsValidations
    } = useProcessBuilderContext();

    const queries = useProcessBuilderQueries(processId, isEditMode);
    const saves = useProcessBuilderSaves(processId, queries.wfControls);

    // Keep track of which entities have been successfully hydrated for the current processId
    const hydratedRef = useRef<{
        processId: string | number | undefined;
        steps: boolean;
        variables: boolean;
        requestControls: boolean;
        transitions: boolean;
        requestControlsValidations: boolean;
    }>({
        processId: undefined,
        steps: false,
        variables: false,
        requestControls: false,
        transitions: false,
        requestControlsValidations: false,
    });

    // Reset hydration tracking flags if the active processId changes
    useEffect(() => {
        hydratedRef.current = {
            processId,
            steps: false,
            variables: false,
            requestControls: false,
            transitions: false,
            requestControlsValidations: false,
        };
    }, [processId]);

    // Hydrate ProcessInfo
    useEffect(() => {
        if (disableHydration) return;
        if (queries.loadedProcess && processInfo.id !== queries.loadedProcess.id) {
            setProcessInfo(mapDtoToInfo(queries.loadedProcess));
        }
    }, [queries.loadedProcess, processInfo.id, setProcessInfo, disableHydration]);

    // Hydrate Steps, Activities, Activity Controls (Only once per process load)
    // Note: no longer waits for `variables` context state — uses raw query data directly
    // so steps always get their serverId as soon as the server responses arrive.
    useEffect(() => {
        if (disableHydration) return;
        if (!hydratedRef.current.steps && queries.loadedSteps && queries.loadedActivities && queries.loadedActivityControls && queries.activityTypes && queries.loadedActivityMappingVariables) {
            // Map variables from query data directly (avoids depending on async context state).
            const freshVars = (queries.loadedVariables && queries.dataTypes)
                ? queries.loadedVariables.map(v => mapWfVariableToLocal(v, queries.dataTypes!))
                : variables;

            const mappedSteps = queries.loadedSteps.map((s) => {
                const step = mapWfStepToLocal(s);
                const stepActs = (queries.loadedActivities || [])
                    .filter((a) => a.stepId === s.id)
                    .sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0));

                step.activities = stepActs.map((dto) => {
                    const activity = mapWfActivityToLocal(dto, queries.activityTypes);
                    if (dto.id != null) {
                        const rows = (queries.loadedActivityControls || [])
                            .filter((c) => c.activityId === dto.id)
                            .sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0));
                        if (rows.length > 0) {
                            activity.controls = rows.map(c => {
                                const localCtrl = mapWfActivityControlToLocal(c);
                                const mapping = (queries.loadedActivityMappingVariables || []).find(m => m.activityControlId === c.id && m.isActive);
                                if (mapping) {
                                    const localV = freshVars.find(v => v.serverId === mapping.variableId);
                                    if (localV) {
                                        localCtrl.bindVariableId = localV.id;
                                    }
                                }
                                return localCtrl;
                            });
                        }
                    }
                    return activity;
                });
                return step;
            });
            setSteps(prev => {
                if (mappedSteps.length === 0) {
                    return prev.length ? prev.map(s => ({ ...s, dirty: true })) : [{ id: uid(), name: 'Step 1', order: 1, status: 'pending', assignedUsers: '', assignedRoles: '', activities: [], dirty: true }];
                }
                const dirtySteps = prev.filter(s => s.dirty);
                let merged = mappedSteps.map(ls => {
                    const dirtyMatch = dirtySteps.find(ds => ds.id === ls.id || ds.serverId === ls.serverId);
                    return dirtyMatch ? dirtyMatch : ls;
                });
                const addedLocally = dirtySteps.filter(ds => !ds.serverId && !merged.find(m => m.id === ds.id));
                merged = [...merged, ...addedLocally];
                return merged;
            });
            hydratedRef.current.steps = true;
        }
    }, [queries.loadedSteps, queries.loadedActivities, queries.loadedActivityControls, queries.activityTypes, queries.loadedActivityMappingVariables, queries.loadedVariables, queries.dataTypes, variables, setSteps, disableHydration]);

    // Hydrate Variables (Only once per process load)
    useEffect(() => {
        if (disableHydration) return;
        if (!hydratedRef.current.variables && queries.loadedVariables && queries.dataTypes) {
            setVariables(prev => {
                const dirtyVars = prev.filter(v => v.dirty);
                const loadedVars = queries.loadedVariables!.map(v => mapWfVariableToLocal(v, queries.dataTypes!));
                const merged = loadedVars.map(ls => {
                    const dirtyMatch = dirtyVars.find(ds => ds.id === ls.id || ds.serverId === ls.serverId);
                    return dirtyMatch ? dirtyMatch : ls;
                });
                const addedLocally = dirtyVars.filter(ds => !ds.serverId && !merged.find(m => m.id === ds.id));
                return [...merged, ...addedLocally];
            });
            hydratedRef.current.variables = true;
        }
    }, [queries.loadedVariables, queries.dataTypes, setVariables, disableHydration]);

    // Hydrate Request Controls (Only once per process load)
    useEffect(() => {
        if (disableHydration) return;
        if (!hydratedRef.current.requestControls && queries.loadedRequestControls && queries.wfControls && queries.loadedRequestMappingVariables && variables.length > 0) {
            setRequestControls(prev => {
                const dirtyControls = prev.filter(c => c.dirty);
                const loadedControls = queries.loadedRequestControls!.map(c => {
                    const localCtrl = mapWfRequestControlToLocal(c, queries.wfControls!);
                    const mapping = (queries.loadedRequestMappingVariables || []).find(m => m.requestControlId === c.id && m.isActive);
                    if (mapping) {
                        const localV = variables.find(v => v.serverId === mapping.variableId);
                        if (localV) {
                            localCtrl.bindVariableId = localV.id;
                        }
                    }
                    return localCtrl;
                });
                const merged = loadedControls.map(ls => {
                    const dirtyMatch = dirtyControls.find(dc => dc.id === ls.id || dc.serverId === ls.serverId);
                    return dirtyMatch ? dirtyMatch : ls;
                });
                const addedLocally = dirtyControls.filter(dc => !dc.serverId && !merged.find(m => m.id === dc.id));
                return [...merged, ...addedLocally];
            });
            hydratedRef.current.requestControls = true;
        }
    }, [queries.loadedRequestControls, queries.wfControls, queries.loadedRequestMappingVariables, variables, setRequestControls, disableHydration]);

    // Hydrate Transitions (Only once per process load)
    useEffect(() => {
        if (disableHydration) return;
        if (!hydratedRef.current.transitions && queries.loadedTransitions && variables.length > 0 && steps.length > 0 && requestControls.length > 0) {
            setTransitions(prev => {
                const dirtyTrans = prev.filter(t => t.dirty);
                const loadedTrans = queries.loadedTransitions!.map(t => 
                    mapWfTransitionToLocal(t, variables, steps, requestControls)
                );
                const merged = loadedTrans.map(ls => {
                    const dirtyMatch = dirtyTrans.find(dt => dt.id === ls.id || dt.serverId === ls.serverId);
                    return dirtyMatch ? dirtyMatch : ls;
                });
                const addedLocally = dirtyTrans.filter(dt => !dt.serverId && !merged.find(m => m.id === dt.id));
                return [...merged, ...addedLocally];
            });
            hydratedRef.current.transitions = true;
        }
    }, [queries.loadedTransitions, variables, steps, requestControls, setTransitions, disableHydration]);

    // Hydrate Request Controls Validations (Only once per process load)
    useEffect(() => {
        if (disableHydration) return;
        if (!hydratedRef.current.requestControlsValidations && queries.loadedRequestControlsValidations && requestControls.length > 0) {
            setRequestControlsValidations(prev => {
                const dirtyValidations = prev.filter(v => v.dirty);
                const requestControlServerIds = requestControls.map(rc => rc.serverId).filter(Boolean);
                const relevantValidations = (queries.loadedRequestControlsValidations || []).filter(v => 
                    requestControlServerIds.includes(v.requestControlId)
                );
                
                const loadedValidations = relevantValidations.map(v => 
                    mapWfRequestControlsValidationToLocal(v, requestControls)
                );
                
                const merged = loadedValidations.map(ls => {
                    const dirtyMatch = dirtyValidations.find(dv => dv.id === ls.id || dv.serverId === ls.serverId);
                    return dirtyMatch ? dirtyMatch : ls;
                });
                const addedLocally = dirtyValidations.filter(dv => !dv.serverId && !merged.find(m => m.id === dv.id));
                return [...merged, ...addedLocally];
            });
            hydratedRef.current.requestControlsValidations = true;
        }
    }, [queries.loadedRequestControlsValidations, requestControls, setRequestControlsValidations, disableHydration]);

    return { ...queries, ...saves };
};
