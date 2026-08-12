import { useQuery } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import {
    wfProcessService, wfPriorityService, wfDataTypeService,
    wfStepService, wfActivityService, wfActivityControlService,
    wfActivityTypeService, wfPerformerService, wfRequestControlService,
    wfVariableService, wfControlService, wfTransitionService, wfOperatorService,
    wfRequestMappingVariableService, wfActivityMappingVariableService, wfRequestControlsValidationService
} from '../../../api';
import { queryKeys } from '../../../../../config/query-keys';
import type { WfProcess, WfPriority, WfDataType, WfStep, WfActivity, WfActivityControl, WfActivityType, WfPerformer, WfRequestControl, WfTransition, WfOperator, WfRequestMappingVariable, WfActivityMappingVariable, WfRequestControlsValidation } from '../../../types';


export const useProcessBuilderQueries = (processId: number | string | undefined, isEditMode: boolean) => {
    const { id: routeId } = useParams<{ id: string }>();

    const processQuery = useQuery<WfProcess>({
        queryKey: queryKeys.entities.detail('WfProcess', routeId!),
        queryFn: () => wfProcessService.getById(routeId!),
        enabled: isEditMode && !!routeId,
        staleTime: Infinity,
        refetchOnWindowFocus: false,
    });

    const prioritiesQuery = useQuery<WfPriority[]>({
        queryKey: queryKeys.entities.list('WfPriority') as any,
        queryFn: () => wfPriorityService.getAll(),
        staleTime: 60_000,
    });

    const dataTypesQuery = useQuery<WfDataType[]>({
        queryKey: queryKeys.entities.list('WfDataType') as any,
        queryFn: () => wfDataTypeService.getAll(),
        staleTime: 60_000,
    });

    const activityTypesQuery = useQuery<WfActivityType[]>({
        queryKey: queryKeys.entities.list('WfActivityType') as any,
        queryFn: () => wfActivityTypeService.getAll(),
        staleTime: 60_000,
    });

    const performersQuery = useQuery<WfPerformer[]>({
        queryKey: queryKeys.entities.list('WfPerformer') as any,
        queryFn: () => wfPerformerService.getAll(),
        staleTime: 60_000,
    });

    const stepsQuery = useQuery<WfStep[]>({
        queryKey: ['WfStep', String(processId ?? '')] as any,
        queryFn: () => wfStepService.getAll(),
        enabled: !!processId,
        staleTime: 60_000,
        select: (rows) => rows.filter((s) => s.processId === processId),
    });

    const activitiesQuery = useQuery<WfActivity[]>({
        queryKey: ['WfActivity', String(processId ?? '')] as any,
        queryFn: () => wfActivityService.getAll(),
        enabled: !!processId,
        staleTime: 60_000,
    });

    const activityControlsQuery = useQuery<WfActivityControl[]>({
        queryKey: ['WfActivityControl', String(processId ?? '')] as any,
        queryFn: () => wfActivityControlService.getAll(),
        enabled: !!processId,
        staleTime: 60_000,
    });

    const requestControlsQuery = useQuery<WfRequestControl[]>({
        queryKey: ['WfRequestControl', String(processId ?? '')] as any,
        queryFn: () => wfRequestControlService.getAll(),
        enabled: !!processId,
        staleTime: 60_000,
        select: (rows) => rows.filter((c) => c.processId === processId),
    });

    const variablesQuery = useQuery<import('../../../types').WfVariable[]>({
        queryKey: queryKeys.entities.list('WfVariable', String(processId ?? '')) as any,
        queryFn: () => wfVariableService.getAll(),
        enabled: !!processId,
        staleTime: 60_000,
        select: (rows) => rows.filter((v) => v.processId === processId),
    });

    const wfControlsQuery = useQuery<import('../../../types').WfControl[]>({
        queryKey: queryKeys.entities.list('WfControl') as any,
        queryFn: () => wfControlService.getAll(),
        staleTime: 60_000,
    });

    const transitionsQuery = useQuery<WfTransition[]>({
        queryKey: ['WfTransition', String(processId ?? '')] as any,
        queryFn: () => wfTransitionService.getAll(),
        enabled: !!processId,
        staleTime: 60_000,
        select: (rows) => rows.filter((t) => t.processId === processId),
    });

    const operatorsQuery = useQuery<WfOperator[]>({
        queryKey: queryKeys.entities.list('WfOperator') as any,
        queryFn: () => wfOperatorService.getAll(),
        staleTime: 60_000,
    });

    const requestMappingVariablesQuery = useQuery<WfRequestMappingVariable[]>({
        queryKey: ['WfRequestMappingVariable', String(processId ?? '')] as any,
        queryFn: () => wfRequestMappingVariableService.getAll(),
        enabled: !!processId,
        staleTime: 60_000,
    });

    const activityMappingVariablesQuery = useQuery<WfActivityMappingVariable[]>({
        queryKey: ['WfActivityMappingVariable', String(processId ?? '')] as any,
        queryFn: () => wfActivityMappingVariableService.getAll(),
        enabled: !!processId,
        staleTime: 60_000,
    });

    const requestControlsValidationsQuery = useQuery<WfRequestControlsValidation[]>({
        queryKey: ['WfRequestControlsValidation', String(processId ?? '')] as any,
        queryFn: () => wfRequestControlsValidationService.getAll(),
        enabled: !!processId,
        staleTime: 60_000,
    });

    return {
        loadedProcess: processQuery.data,
        loadingProcess: processQuery.isLoading,
        priorities: prioritiesQuery.data || [],
        dataTypes: dataTypesQuery.data || [],
        activityTypes: activityTypesQuery.data || [],
        performers: performersQuery.data || [],
        wfControls: wfControlsQuery.data || [],
        loadedSteps: stepsQuery.data,
        loadedActivities: activitiesQuery.data,
        loadedActivityControls: activityControlsQuery.data,
        loadedRequestControls: requestControlsQuery.data,
        loadedVariables: variablesQuery.data,
        loadedTransitions: transitionsQuery.data,
        operators: operatorsQuery.data || [],
        loadedRequestMappingVariables: requestMappingVariablesQuery.data,
        loadedActivityMappingVariables: activityMappingVariablesQuery.data,
        loadedRequestControlsValidations: requestControlsValidationsQuery.data,
    };
};
