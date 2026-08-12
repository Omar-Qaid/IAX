import { wfProcessService, wfStepService, wfVariableService, wfRequestControlService, wfTransitionService } from '../../../api';
import { queryKeys } from '../../../../../config/query-keys';
import type { WfProcess, WfStep, WfVariable, WfRequestControl, WfTransition } from '../../../types';
import { useEntityMutation } from '../../../../../hooks/useEntityMutation';

export const useProcessBuilderMutations = (processId: number | string | undefined) => {
    // Process Info Mutations
    const createProcess = useEntityMutation({
        mutationFn: (dto: Omit<WfProcess, 'id'>) => wfProcessService.create(dto),
        invalidateKeys: [queryKeys.entities.list('WfProcess') as any],
        successMessage: 'Process created',
        errorMessage: 'Failed to create process',
    });

    const updateProcess = useEntityMutation({
        mutationFn: ({ id, dto }: { id: number | string; dto: WfProcess }) =>
            wfProcessService.update(id, dto),
        invalidateKeys: processId ? [queryKeys.entities.detail('WfProcess', String(processId)) as any] : [],
        successMessage: 'Process updated',
        errorMessage: 'Failed to update process',
    });

    // Step Mutations
    const createStep = useEntityMutation({
        mutationFn: (dto: Omit<WfStep, 'id'>) => wfStepService.create(dto),
        invalidateKeys: [['WfStep', String(processId ?? '')]],
        errorMessage: 'Failed to create step',
    });

    const updateStep = useEntityMutation({
        mutationFn: ({ id, dto }: { id: number; dto: WfStep }) => wfStepService.update(id, dto),
        invalidateKeys: [['WfStep', String(processId ?? '')]],
        errorMessage: 'Failed to update step',
    });

    const deleteStep = useEntityMutation({
        mutationFn: (id: number) => wfStepService.delete(id),
        invalidateKeys: [['WfStep', String(processId ?? '')]],
        errorMessage: 'Failed to delete step',
    });

    // Variable Mutations
    const createVariable = useEntityMutation({
        mutationFn: (dto: Omit<WfVariable, 'id'>) => wfVariableService.create(dto),
        invalidateKeys: [queryKeys.entities.list('WfVariable') as any],
        errorMessage: 'Failed to create variable',
    });

    const updateVariable = useEntityMutation({
        mutationFn: ({ id, dto }: { id: number; dto: WfVariable }) => wfVariableService.update(id, dto),
        invalidateKeys: [queryKeys.entities.list('WfVariable') as any],
        errorMessage: 'Failed to update variable',
    });

    const deleteVariable = useEntityMutation({
        mutationFn: (id: number) => wfVariableService.delete(id),
        invalidateKeys: [queryKeys.entities.list('WfVariable') as any],
        errorMessage: 'Failed to delete variable',
    });

    // Request Control Mutations
    const createRequestControl = useEntityMutation({
        mutationFn: (dto: Omit<WfRequestControl, 'id'>) => wfRequestControlService.create(dto),
        invalidateKeys: [queryKeys.entities.list('WfRequestControl') as any],
        errorMessage: 'Failed to create request control',
    });

    const updateRequestControl = useEntityMutation({
        mutationFn: ({ id, dto }: { id: number; dto: WfRequestControl }) => wfRequestControlService.update(id, dto),
        invalidateKeys: [queryKeys.entities.list('WfRequestControl') as any],
        errorMessage: 'Failed to update request control',
    });

    const deleteRequestControl = useEntityMutation({
        mutationFn: (id: number) => wfRequestControlService.delete(id),
        invalidateKeys: [queryKeys.entities.list('WfRequestControl') as any],
        errorMessage: 'Failed to delete request control',
    });

    // Transition Mutations
    const createTransition = useEntityMutation({
        mutationFn: (dto: Omit<WfTransition, 'id'>) => wfTransitionService.create(dto),
        invalidateKeys: [['WfTransition', String(processId ?? '')]],
        errorMessage: 'Failed to create transition',
    });

    const updateTransition = useEntityMutation({
        mutationFn: ({ id, dto }: { id: number; dto: WfTransition }) => wfTransitionService.update(id, dto),
        invalidateKeys: [['WfTransition', String(processId ?? '')]],
        errorMessage: 'Failed to update transition',
    });

    const deleteTransition = useEntityMutation({
        mutationFn: (id: number) => wfTransitionService.delete(id),
        invalidateKeys: [['WfTransition', String(processId ?? '')]],
        errorMessage: 'Failed to delete transition',
    });

    return {
        createProcess, updateProcess,
        createStep, updateStep, deleteStep,
        createVariable, updateVariable, deleteVariable,
        createRequestControl, updateRequestControl, deleteRequestControl,
        createTransition, updateTransition, deleteTransition,
        
        // Status checks
        processSaving: createProcess.isPending || updateProcess.isPending,
        stepsSaving: createStep.isPending || updateStep.isPending || deleteStep.isPending,
        variablesSaving: createVariable.isPending || updateVariable.isPending || deleteVariable.isPending,
        requestControlsSaving: createRequestControl.isPending || updateRequestControl.isPending || deleteRequestControl.isPending,
        transitionsSaving: createTransition.isPending || updateTransition.isPending || deleteTransition.isPending,
    };
};
