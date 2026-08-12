import type { WfTransition } from '../../../../types';
import type { Transition, Step, ProcessVariable, RequestControl } from '../../types';
import { uid } from '../uid';

export const mapWfTransitionToLocal = (
    dto: WfTransition,
    variables: ProcessVariable[],
    steps: Step[],
    requestControls: RequestControl[]
): Transition => {
    const localVariable = variables.find(v => v.serverId === dto.variableId);
    const localStep = steps.find(s => s.serverId === dto.stepId);
    const localRequestControl = requestControls.find(c => c.serverId === dto.requestControlId);
    
    let localActivityId: string | undefined;
    if (dto.activityId) {
        for (const s of steps) {
            const match = s.activities.find(a => a.serverId === dto.activityId);
            if (match) {
                localActivityId = match.id;
                break;
            }
        }
    }

    let localActivityControlId: string | undefined;
    if (dto.activityControlId) {
        for (const s of steps) {
            for (const a of s.activities) {
                const match = a.controls.find(c => c.serverId === dto.activityControlId);
                if (match) {
                    localActivityControlId = match.id;
                    break;
                }
            }
            if (localActivityControlId) break;
        }
    }

    return {
        id: uid(),
        serverId: dto.id,
        processId: dto.processId,
        activityId: localActivityId,
        requestControlId: localRequestControl?.id,
        activityControlId: localActivityControlId,
        variableId: localVariable?.id ?? '',
        operatorId: dto.operatorId,
        value: dto.value ?? '',
        stepId: localStep?.id ?? '',
        sortOrder: dto.sortOrder ?? 0,
        isActive: dto.isActive !== false,
        dirty: false,
    };
};

export const mapLocalToWfTransition = (
    t: Transition,
    processId: number,
    variables: ProcessVariable[],
    steps: Step[],
    requestControls: RequestControl[]
): Omit<WfTransition, 'id'> & { id?: number } => {
    const variable = variables.find(v => v.id === t.variableId);
    const step = steps.find(s => s.id === t.stepId);
    const requestControl = requestControls.find(c => c.id === t.requestControlId);

    let activityServerId: number | undefined;
    if (t.activityId) {
        for (const s of steps) {
            const match = s.activities.find(a => a.id === t.activityId);
            if (match) {
                activityServerId = match.serverId;
                break;
            }
        }
    }

    let activityControlServerId: number | undefined;
    if (t.activityControlId) {
        for (const s of steps) {
            for (const a of s.activities) {
                const match = a.controls.find(c => c.id === t.activityControlId);
                if (match) {
                    activityControlServerId = match.serverId;
                    break;
                }
            }
            if (activityControlServerId) break;
        }
    }

    return {
        id: t.serverId,
        processId,
        activityId: activityServerId || null,
        requestControlId: requestControl?.serverId || null,
        activityControlId: activityControlServerId || null,
        variableId: variable?.serverId || 0,
        operatorId: Number(t.operatorId) || 0,
        value: t.value || '',
        stepId: step?.serverId || 0,
        sortOrder: t.sortOrder,
        isActive: t.isActive,
        isDeleted: false,
    } as any;
};
