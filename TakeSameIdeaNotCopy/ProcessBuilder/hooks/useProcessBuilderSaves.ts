import { logger } from '../../../../../utils/logger';
import { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { notify } from '../../../../../lib/notify';
import { useTranslation } from 'react-i18next';
import {
    wfVariableService, wfStepService,
    wfActivityService, wfActivityControlService, wfRequestControlService,
    wfTransitionService, wfRequestMappingVariableService, wfActivityMappingVariableService,
    wfRequestControlsValidationService, wfActivityControlsValidationService,
    wfRequestControlsOptionService, wfActivityControlsOptionService
} from '../../../api';
import { queryKeys } from '../../../../../config/query-keys';
import { buildValidationRulesXml } from '../../../utils/validationHelper';
import { useProcessBuilderContext } from '../context/ProcessBuilderContext';
import { useProcessBuilderStore } from '../store/processBuilderStore';
import {
    mapInfoToDto, mapLocalToWfVariable, mapLocalToWfStep,
    mapLocalToWfActivity, mapLocalToWfActivityControl, mapLocalToWfRequestControl,
    findControlIdByType, mapLocalToWfTransition, mapLocalToWfRequestControlsValidation
} from '../utils/ProcessBuilderMappers';
import type { WfActivity, WfProcess, WfControl } from '../../../types';
import type { ProcessVariable, Activity, RequestControl, Transition, DropdownOptionX } from '../types';
import { useNavigate, useParams } from 'react-router-dom';
import { useProcessBuilderMutations } from './useProcessBuilderMutations';
import type { CrudService } from '../../../../../services/crud.service';

/** Control types whose options are stored in the normalized Wf*ControlsOptions tables. */
const OPTION_CONTROL_TYPES = ['dropdown', 'dropdown-manual', 'checkboxlist', 'radiobuttonlist'];

/**
 * Replace-set a control's rows in its normalized option table from the local optionsX:
 * deletes the control's existing options, then inserts the current set. The control's
 * ExtendedProperties XML (written by the control DTO) remains the back-compat copy.
 */
async function syncControlOptions<T extends { id?: number | string }>(
    service: CrudService<T>,
    serverFilterField: string,
    serverId: number,
    type: string,
    optionsX: DropdownOptionX[] | undefined,
    buildRow: (o: DropdownOptionX, index: number) => Omit<T, 'id'>,
): Promise<void> {
    const existing = await service.getPaged(
        1, [], '', [], [{ Field: serverFilterField, Operator: 'equals', Value: String(serverId) }], 1000,
    );
    const existingIds = existing.data.map(o => o.id).filter((id): id is number | string => id != null);
    if (existingIds.length) await service.deleteRange(existingIds);
    if (OPTION_CONTROL_TYPES.includes(type) && (optionsX?.length ?? 0) > 0) {
        await service.createRange((optionsX ?? []).map(buildRow));
    }
}

export const useProcessBuilderSaves = (processId: number | string | undefined, wfControls: WfControl[] = []) => {
    const { i18n, t } = useTranslation();
    const isRtl = i18n.language === 'ar';
    const qc = useQueryClient();
    const navigate = useNavigate();
    const { id: routeId } = useParams<{ id: string }>();

    const {
        processInfo, setProcessInfo,
        variables, setVariables,
        setSteps,
        setRequestControls,
        updateActivity,
        setRequestControlsValidations,
        deletedRequestControlsValidationControlIds
    } = useProcessBuilderContext();

    const [transitionsSavingLocal, setTransitionsSavingLocal] = useState(false);

    const mutations = useProcessBuilderMutations(processId);

    const saveProcessInfoToBackend = async () => {
        if (!processInfo.name || !processInfo.categoryId) {
            notify.error(t('workflow.name_category_required', 'Process Name and Category are required.'));
            return;
        }
        const infoCopy = { ...processInfo };
        if (isRtl && !infoCopy.nameAR) infoCopy.nameAR = infoCopy.name;
        if (!isRtl && !infoCopy.name) infoCopy.name = infoCopy.nameAR;

        try {
            const dto = mapInfoToDto(infoCopy);
            if (processInfo.id) {
                await mutations.updateProcess.mutateAsync({ id: processInfo.id, dto: dto as any });
            } else {
                const created = await mutations.createProcess.mutateAsync({ ...dto } as Omit<WfProcess, 'id'>);
                setProcessInfo((p) => ({ ...p, id: created?.id, code: created?.code || p.code }));
                if (created?.id) {
                    // Copy existing localStorage data from current draft to the new ID prefix to prevent data loss
                    const currentIdKey = routeId || 'new';
                    const oldPrefix = `processBuilder.${currentIdKey}.`;
                    const newPrefix = `processBuilder.${created.id}.`;
                    const sections = ['processInfo', 'variables', 'requestControls', 'steps', 'uiState', 'savedAt'];
                    sections.forEach((sect) => {
                        const val = localStorage.getItem(oldPrefix + sect);
                        if (val) {
                            localStorage.setItem(newPrefix + sect, val);
                        }
                    });
                    navigate(`/workflow/process-builder/${created.id}`, { replace: true });
                }
                return created?.id; // return new ID to caller for redirect
            }
        } catch { /* toast already handled by hook */ }
        return undefined;
    };

    const saveVariablesToBackend = async () => {
        const state = useProcessBuilderStore.getState();
        const { processInfo, variables, deletedVariableServerIds } = state;
        
        if (!processInfo.id) {
            notify.error(t('workflow.save_process_first', 'Save Process Info first'));
            return;
        }
        const dirty = variables.filter((v) => v.dirty);
        const hasDeletions = deletedVariableServerIds && deletedVariableServerIds.length > 0;
        if (dirty.length === 0 && !hasDeletions) {
            notify.success(t('workflow.no_changes', 'No changes to save'));
            return;
        }
        for (const v of dirty) {
            if (isRtl) {
                if (v.nameAR) v.name = v.nameAR;
                if (v.descriptionAR) v.description = v.descriptionAR;
            } else {
                if (v.name) v.nameAR = v.name;
                if (v.description) v.descriptionAR = v.description;
            }
            if (!v.name && v.nameAR) v.name = v.nameAR;
            if (!v.nameAR && v.name) v.nameAR = v.name;
            if (!v.description && v.descriptionAR) v.description = v.descriptionAR;
            if (!v.descriptionAR && v.description) v.descriptionAR = v.description;
            if (!v.name || !v.dataTypeId) {
                notify.error(t('workflow.variable_missing_fields', 'Variable missing Name/DataType'));
                return;
            }
        }
        try {
            if (hasDeletions) {
                await wfVariableService.deleteRange(deletedVariableServerIds);
                useProcessBuilderStore.setState({ deletedVariableServerIds: [] });
                console.log('[Variables Save debug] deleteRange completed for IDs:', deletedVariableServerIds);
            }

            const creates: any[] = [];
            const updates: any[] = [];
            const localToDto: { v: ProcessVariable; dto: any }[] = [];

            for (const v of dirty) {
                const dto = mapLocalToWfVariable(v, Number(processInfo.id) || 0);
                localToDto.push({ v, dto });
                if (v.serverId) updates.push({ ...dto, id: v.serverId });
                else creates.push(dto);
            }

            console.log('[Variables Save debug] Initiating save:', {
                dirtyCount: dirty.length,
                createsCount: creates.length,
                updatesCount: updates.length,
                createsPayload: creates
            });

            let actualNewRecords: any[] = [];
            if (creates.length > 0) {
                const res = await wfVariableService.createRange(creates);
                console.log('[Variables Save debug] createRange response:', res);
                if (Array.isArray(res)) actualNewRecords = res;
                else {
                    console.warn('[Variables Save debug] createRange response was not an array. Falling back to getAll().');
                    const all = await wfVariableService.getAll();
                    actualNewRecords = all.filter((x: any) => x.processId === Number(processInfo.id));
                    console.log('[Variables Save debug] getAll fallback records:', actualNewRecords);
                }
            }
            if (updates.length > 0) {
                await wfVariableService.updateRange(updates);
                console.log('[Variables Save debug] updateRange completed.');
            }

            let newRecordIndex = 0;
            const updated = variables.map((v) => {
                if (!v.dirty) return v;
                if (v.serverId) return { ...v, dirty: false };

                let srvId, cCode;
                if (actualNewRecords.length === creates.length) {
                    const createdRecord: any = actualNewRecords[newRecordIndex++];
                    srvId = typeof createdRecord === 'object' ? (createdRecord?.id ?? createdRecord?.Id) : createdRecord;
                    cCode = typeof createdRecord === 'object' ? (createdRecord?.code ?? createdRecord?.Code) : undefined;
                    console.log(`[Variables Save debug] Positional reconciliation index ${newRecordIndex - 1}:`, {
                        localCode: v.code,
                        localName: v.name,
                        createdRecord,
                        resolvedId: srvId,
                        resolvedCode: cCode
                    });
                } else {
                    const match = actualNewRecords.find((nr: any) => 
                        (nr.code ?? nr.Code) === v.code || 
                        (nr.name ?? nr.Name) === v.name || 
                        (nr.nameAR ?? nr.NameAR) === v.nameAR
                    );
                    srvId = match?.id ?? match?.Id;
                    cCode = match?.code ?? match?.Code;
                    console.log('[Variables Save debug] Property reconciliation match:', {
                        localCode: v.code,
                        localName: v.name,
                        matchRecord: match,
                        resolvedId: srvId,
                        resolvedCode: cCode
                    });
                }
                
                if (srvId) {
                    return { ...v, serverId: srvId, code: cCode || v.code, dirty: false };
                }
                console.warn('[Variables Save debug] Failed to find srvId for variable:', v);
                return v; // Keep it dirty if it wasn't actually saved
            });

            console.log('[Variables Save debug] Final updated variables list:', updated);

            setVariables(updated);
            qc.invalidateQueries({ queryKey: queryKeys.entities.list('WfVariable') });
            
            const stillDirty = updated.some(v => v.dirty);
            if (stillDirty) notify.error(t('workflow.failed_save_variables', 'Failed to save some variables'));
            else notify.success(t('workflow.variables_saved', 'Variables saved'));
        } catch (error) {
            console.error('[Variables Save debug] Exception during saveVariablesToBackend:', error);
            notify.error(t('workflow.failed_save_variables', 'Failed to save variables'));
        }
    };

    const saveStepsToBackend = async () => {
        const { steps: currentSteps, processInfo: currentInfo, deletedStepServerIds } = useProcessBuilderStore.getState();
        if (!currentInfo.id) {
            notify.error(t('workflow.save_process_first', 'Save Process Info first'));
            return;
        }
        const dirty = currentSteps.filter((s) => s.dirty);
        const hasDeletions = deletedStepServerIds && deletedStepServerIds.length > 0;
        if (dirty.length === 0 && !hasDeletions) {
            notify.success(t('workflow.no_changes', 'No changes to save'));
            return;
        }
        for (const s of dirty) {
            if (isRtl) { if (s.nameAR) s.name = s.nameAR; }
            else { if (s.name) s.nameAR = s.name; }
            if (!s.name && s.nameAR) s.name = s.nameAR;
            if (!s.nameAR && s.name) s.nameAR = s.name;
            if (!s.name) {
                notify.error(t('workflow.step_missing_name', 'Step is missing Name'));
                return;
            }
        }
        try {
            if (hasDeletions) {
                await wfStepService.deleteRange(deletedStepServerIds);
                useProcessBuilderStore.setState({ deletedStepServerIds: [] });
            }

            const creates: any[] = [];
            const updates: any[] = [];

            for (const s of dirty) {
                const dto = mapLocalToWfStep(s, Number(currentInfo.id) || 0);
                if (s.serverId) updates.push({ ...dto, id: s.serverId });
                else creates.push(dto);
            }

            let actualNewRecords: any[] = [];
            if (creates.length > 0) {
                const res = await wfStepService.createRange(creates);
                if (Array.isArray(res)) actualNewRecords = res;
                else {
                    const all = await wfStepService.getAll();
                    actualNewRecords = all.filter((x: any) => x.processId === Number(currentInfo.id));
                }
            }
            if (updates.length > 0) await wfStepService.updateRange(updates);

            // Read the latest steps from the store — state may have changed while awaiting
            const latestSteps = useProcessBuilderStore.getState().steps;
            let newRecordIndex = 0;
            const updatedSteps = latestSteps.map((s) => {
                if (!s.dirty) return s;
                if (s.serverId) return { ...s, dirty: false };

                let srvId, cCode;
                if (actualNewRecords.length === creates.length) {
                    const createdRecord: any = actualNewRecords[newRecordIndex++];
                    srvId = typeof createdRecord === 'object' ? (createdRecord?.id ?? createdRecord?.Id) : createdRecord;
                    cCode = typeof createdRecord === 'object' ? (createdRecord?.code ?? createdRecord?.Code) : undefined;
                } else {
                    const match = actualNewRecords.find((nr: any) => 
                        (nr.code ?? nr.Code) === s.code || 
                        (nr.name ?? nr.Name) === s.name || 
                        (nr.nameAR ?? nr.NameAR) === s.nameAR
                    );
                    srvId = match?.id ?? match?.Id;
                    cCode = match?.code ?? match?.Code;
                }
                

                if (srvId) {
                    return { ...s, serverId: srvId, code: cCode || s.code, dirty: false };
                }
                return s; // Keep it dirty if it wasn't actually saved
            });

            setSteps(updatedSteps);
            qc.invalidateQueries({ queryKey: ['WfStep', String(currentInfo.id ?? '')] });
            
            const stillDirty = updatedSteps.some(s => s.dirty);
            if (stillDirty) notify.error(t('workflow.failed_save_steps', 'Failed to save some steps'));
            else notify.success(t('workflow.steps_saved', 'Steps saved'));
        } catch { notify.error(t('workflow.failed_save_steps', 'Failed to save steps')); }
    };

    const saveRequestControlsToBackend = async (wfControls: WfControl[]) => {
        const state = useProcessBuilderStore.getState();
        const { processInfo, requestControls, requestControlsValidations, deletedRequestControlsValidationServerIds, deletedRequestControlServerIds } = state;

        if (!processInfo.id) {
            notify.error(t('workflow.save_process_first', 'Save Process Info first'));
            return;
        }
        const dirtyControls = requestControls.filter((c) => c.dirty);
        // Exclude activity-control validations — those are saved by saveActivityToBackend.
        const dirtyValidations = requestControlsValidations.filter((v) => v.dirty && !v.activityControlId);
        const hasDeletedValidations = deletedRequestControlsValidationServerIds.length > 0;
        const hasDeletedControls = deletedRequestControlServerIds && deletedRequestControlServerIds.length > 0;

        if (dirtyControls.length === 0 && dirtyValidations.length === 0 && !hasDeletedValidations && !hasDeletedControls) {
            notify.success(t('workflow.no_changes', 'No changes to save'));
            return;
        }
        for (let i = 0; i < dirtyControls.length; i++) {
            const c = dirtyControls[i];
            const cid = c.controlId ?? findControlIdByType(c.type, wfControls);
            if (!cid) {
                notify.error(`Row ${i + 1}: no WfControl catalog row found for type "${c.type}".`);
                return;
            }
            if (c.bindVariableId) {
                const variable = state.variables.find(v => v.id === c.bindVariableId);
                if (!variable?.serverId) {
                    notify.error(t('workflow.save_variables_first', 'You must save Variables before saving Controls that bind to them.'));
                    return;
                }
            }
        }
        try {
            if (hasDeletedControls) {
                await wfRequestControlService.deleteRange(deletedRequestControlServerIds);
                useProcessBuilderStore.setState({ deletedRequestControlServerIds: [] });
            }

            const creates: any[] = [];
            const updates: any[] = [];
            const localToDto: { c: RequestControl; dto: any }[] = [];

            for (let i = 0; i < dirtyControls.length; i++) {
                const c = dirtyControls[i];
                const dto = mapLocalToWfRequestControl(c, Number(processInfo.id) || 0, i);
                dto.controlId = c.controlId ?? findControlIdByType(c.type, wfControls) ?? 0;
                dto.validationRules = buildValidationRulesXml(
                    requestControlsValidations.filter(v => v.requestControlId === c.id && !v.activityControlId)
                );

                localToDto.push({ c, dto });
                if (c.serverId) updates.push({ ...dto, id: c.serverId });
                else creates.push(dto);
            }

            let actualNewRecords: any[] = [];
            if (creates.length > 0) {
                const res = await wfRequestControlService.createRange(creates);
                if (Array.isArray(res)) actualNewRecords = res;
                else {
                    const all = await wfRequestControlService.getAll();
                    actualNewRecords = all.filter((x: any) => x.processId === Number(processInfo.id));
                }
            }
            if (updates.length > 0) await wfRequestControlService.updateRange(updates);

            let newRecordIndex = 0;
            const updated = requestControls.map((c) => {
                if (!c.dirty) return c;
                if (c.serverId) return { ...c, dirty: false };
                const matchDto = localToDto.find(x => x.c.id === c.id)?.dto;

                let matchId;
                if (actualNewRecords.length === creates.length) {
                    const createdRecord: any = actualNewRecords[newRecordIndex++];
                    matchId = typeof createdRecord === 'object' ? (createdRecord?.id ?? createdRecord?.Id) : createdRecord;
                } else {
                    const match = actualNewRecords.find((nr: any) => 
                        ((nr.code ?? nr.Code) === matchDto?.code || (nr.name ?? nr.Name) === matchDto?.name) && 
                        (nr.controlId ?? nr.ControlId) === matchDto?.controlId
                    );
                    matchId = match?.id ?? match?.Id;
                }
                if (matchId) {
                    return { ...c, serverId: matchId, controlId: matchDto?.controlId, dirty: false };
                }
                return c; // Keep it dirty if it wasn't actually saved
            });

            setRequestControls(updated);
            qc.invalidateQueries({ queryKey: queryKeys.entities.list('WfRequestControl') });

            // Sync WfRequestControlsOption (replace-set per saved control)
            try {
                for (const c of dirtyControls) {
                    const serverId = updated.find(u => u.id === c.id)?.serverId;
                    if (!serverId) continue;
                    await syncControlOptions(
                        wfRequestControlsOptionService, 'RequestControlId', Number(serverId), c.type, c.optionsX,
                        (o, idx) => ({
                            requestControlId: Number(serverId), value: o.value, nameEn: o.en, nameAr: o.ar,
                            sortOrder: (idx + 1) * 10, isActive: true,
                        }),
                    );
                }
            } catch (err) {
                logger.error('Failed to sync request control options', err);
                notify.error(t('workflow.failed_save_options', 'Failed to save control options'));
            }

            // Sync WfRequestMappingVariables
            try {
                const currentMappings = await wfRequestMappingVariableService.getAll();
                const processMappings = currentMappings.filter((m: any) => 
                    updated.some(uc => uc.serverId === m.requestControlId)
                );

                for (const c of updated) {
                    const existingMapping = processMappings.find((m: any) => m.requestControlId === c.serverId);
                    if (c.bindVariableId) {
                        const variable = variables.find(v => v.id === c.bindVariableId);
                        if (variable?.serverId) {
                            if (existingMapping) {
                                if (existingMapping.variableId !== variable.serverId || !existingMapping.isActive) {
                                    await wfRequestMappingVariableService.update(existingMapping.id!, {
                                        ...existingMapping,
                                        variableId: variable.serverId,
                                        isActive: true
                                    });
                                }
                            } else {
                                await wfRequestMappingVariableService.create({
                                    requestControlId: c.serverId!,
                                    variableId: variable.serverId,
                                    isActive: true
                                });
                            }
                        }
                    } else {
                        if (existingMapping) {
                            await wfRequestMappingVariableService.delete(existingMapping.id!);
                        }
                    }
                }
                qc.invalidateQueries({ queryKey: ['WfRequestMappingVariable', String(processInfo.id ?? '')] });
            } catch (err) {
                logger.error("Failed to sync request mapping variables", err);
            }

            // Sync WfRequestControlsValidation
            try {
                for (const deletedId of deletedRequestControlsValidationServerIds) {
                    await wfRequestControlsValidationService.delete(deletedId);
                }

                const valCreates: any[] = [];
                const valUpdates: any[] = [];
                const valLocalToDto: { v: any; dto: any }[] = [];

                for (const v of dirtyValidations) {
                    const dto = mapLocalToWfRequestControlsValidation(v, updated);
                    if (!dto.requestControlId) {
                        const localCtrl = requestControls.find(rc => rc.id === v.requestControlId);
                        if (localCtrl) {
                            const updatedCtrl = updated.find(uc => uc.id === localCtrl.id);
                            if (updatedCtrl?.serverId) {
                                dto.requestControlId = updatedCtrl.serverId;
                            }
                        }
                    }

                    if (dto.requestControlId) {
                        valLocalToDto.push({ v, dto });
                        if (v.serverId) valUpdates.push({ ...dto, id: v.serverId });
                        else valCreates.push(dto);
                    }
                }

                let actualNewVals: any[] = [];
                if (valCreates.length > 0) {
                    const res = await wfRequestControlsValidationService.createRange(valCreates);
                    if (Array.isArray(res)) actualNewVals = res;
                    else {
                        const all = await wfRequestControlsValidationService.getAll();
                        const controlServerIds = updated.map(uc => uc.serverId).filter(Boolean);
                        actualNewVals = all.filter((x: any) => controlServerIds.includes(x.requestControlId));
                    }
                }
                if (valUpdates.length > 0) await wfRequestControlsValidationService.updateRange(valUpdates);

                let newValIdx = 0;
                const updatedValidations = requestControlsValidations.map(v => {
                    // Activity-control validations are owned by saveActivityToBackend — never touch their state here.
                    if (!v.dirty || v.activityControlId) return v;
                    if (v.serverId) return { ...v, dirty: false };

                    let srvId;
                    if (actualNewVals.length === valCreates.length) { 
                        const createdRecord: any = actualNewVals[newValIdx++];
                        srvId = typeof createdRecord === 'object' ? (createdRecord?.id ?? createdRecord?.Id) : createdRecord;
                    } else {
                        const matchDto = valLocalToDto.find(x => x.v.id === v.id)?.dto;
                        const match = actualNewVals.find((nv: any) => 
                            (nv.requestControlId ?? nv.RequestControlId) === matchDto?.requestControlId && 
                            (nv.validationType ?? nv.ValidationType) === matchDto?.validationType && 
                            (nv.errorMessageAr ?? nv.ErrorMessageAr) === matchDto?.errorMessageAr
                        );
                        srvId = match?.id ?? match?.Id;
                    }
                    if (srvId) {
                        return { ...v, serverId: srvId, dirty: false };
                    }
                    return v; // Keep it dirty if it wasn't actually saved
                });

                setRequestControlsValidations(updatedValidations);
                useProcessBuilderStore.setState({
                    deletedRequestControlsValidationServerIds: [],
                    deletedRequestControlsValidationControlIds: [],
                });
                qc.invalidateQueries({ queryKey: ['WfRequestControlsValidation', String(processInfo.id ?? '')] });

                // Update ValidationRules XML on controls that had validation-only changes
                // (dirty controls already got XML in their DTO above)
                const dirtyControlIds = new Set(dirtyControls.map(c => c.id));
                const xmlUpdateControlIds = new Set([
                    ...dirtyValidations.map(v => v.requestControlId),
                    ...deletedRequestControlsValidationControlIds,
                ]);
                const xmlOnlyControlIds = [...xmlUpdateControlIds].filter(id => !dirtyControlIds.has(id));

                if (xmlOnlyControlIds.length > 0) {
                    const xmlUpdates: any[] = [];
                    for (const localId of xmlOnlyControlIds) {
                        const ctrl = updated.find(c => c.id === localId);
                        if (!ctrl?.serverId) continue;
                        const dto = mapLocalToWfRequestControl(ctrl, Number(processInfo.id) || 0, 0);
                        dto.controlId = ctrl.controlId ?? findControlIdByType(ctrl.type, wfControls) ?? 0;
                        dto.validationRules = buildValidationRulesXml(
                            updatedValidations.filter(v => v.requestControlId === localId && !v.activityControlId)
                        );
                        xmlUpdates.push({ ...dto, id: ctrl.serverId });
                    }
                    if (xmlUpdates.length > 0) await wfRequestControlService.updateRange(xmlUpdates);
                }
            } catch (err) {
                logger.error("Failed to sync request control validations", err);
            }

            notify.success(t('workflow.request_controls_saved', 'Request controls saved'));
        } catch { notify.error(t('workflow.failed_save_req_controls', 'Failed to save request controls')); }
    };

    const saveActivityToBackend = async (stepId: string, a: Activity) => {
        // Read fresh from the store — closure `steps` can be stale in async context
        const step = useProcessBuilderStore.getState().steps.find((s) => s.id === stepId);
        if (!step?.serverId) {
            notify.error(t('workflow.save_step_first', 'Save the Step first'));
            return;
        }

        if (isRtl) {
            if (a.nameAR) a.name = a.nameAR;
        } else {
            if (a.name) a.nameAR = a.name;
        }
        if (!a.name && a.nameAR) a.name = a.nameAR;
        if (!a.nameAR && a.name) a.nameAR = a.name;

        if (!a.name || !a.activityTypeId || !a.performerId) {
            notify.error(t('workflow.activity_req_fields', 'Name, Activity Type and Performer are required'));
            return;
        }

        for (const c of a.controls) {
            if (c.bindVariableId) {
                const variable = useProcessBuilderStore.getState().variables.find(v => v.id === c.bindVariableId);
                if (!variable?.serverId) {
                    notify.error(t('workflow.save_variables_first', 'You must save Variables before saving Controls that bind to them.'));
                    return;
                }
            }
        }

        try {
            let activityServerId: number;

            if (a.serverId) {
                const dto = mapLocalToWfActivity(a, step.serverId);
                await wfActivityService.update(a.serverId, { ...dto, id: a.serverId } as WfActivity);
                activityServerId = a.serverId;
            } else {
                const dto = mapLocalToWfActivity(a, step.serverId);
                const created = await wfActivityService.create(dto as Omit<WfActivity, 'id'>);
                activityServerId = created?.id as number;
                updateActivity(stepId, a.id, { serverId: activityServerId as number, code: created?.code || a.code });
            }

            // Sync controls
            const creates: any[] = [];
            const updates: any[] = [];
            const localToDto: { c: RequestControl; dto: any, resolvedControlId?: number }[] = [];

            for (let ci = 0; ci < a.controls.length; ci++) {
                const c = a.controls[ci];
                const resolvedControlId = c.controlId ?? findControlIdByType(c.type, wfControls);
                const ctrlDto = mapLocalToWfActivityControl(c, activityServerId, Number(processInfo.id), ci);
                if (resolvedControlId) ctrlDto.controlId = resolvedControlId;

                localToDto.push({ c, dto: ctrlDto, resolvedControlId });
                if (c.serverId) updates.push({ ...ctrlDto, id: c.serverId });
                else if (resolvedControlId) creates.push(ctrlDto);
            }

            let actualNewRecords: any[] = [];
            if (creates.length > 0) {
                const res = await wfActivityControlService.createRange(creates);
                if (Array.isArray(res)) actualNewRecords = res;
                else {
                    const all = await wfActivityControlService.getAll();
                    actualNewRecords = all.filter((x: any) => x.activityId === a.serverId);
                }
            }
            if (updates.length > 0) await wfActivityControlService.updateRange(updates);

            let newRecordIndex = 0;
            const updatedControls = a.controls.map((c) => {
                const matchLocal = localToDto.find(x => x.c.id === c.id);
                if (c.serverId) return { ...c, dirty: false, controlId: matchLocal?.resolvedControlId };

                if (matchLocal && !c.serverId && matchLocal.resolvedControlId) {
                    let matchId;
                    if (actualNewRecords.length === creates.length) {
                        const createdRecord: any = actualNewRecords[newRecordIndex++];
                        matchId = typeof createdRecord === 'object' ? (createdRecord?.id ?? createdRecord?.Id) : createdRecord;
                    } else {
                        const match = actualNewRecords.find((nr: any) => 
                            (nr.controlLabel ?? nr.ControlLabel) === matchLocal?.dto.controlLabel && 
                            (nr.controlId ?? nr.ControlId) === matchLocal?.resolvedControlId
                        );
                        matchId = match?.id ?? match?.Id;
                    }
                    if (matchId) {
                        return { ...c, serverId: matchId, controlId: matchLocal.resolvedControlId, dirty: false };
                    }
                }
                return c; // Keep it dirty if it wasn't actually saved
            });

            // Delete controls removed since last save
            for (const deletedCtrlId of (a.deletedControlServerIds ?? [])) {
                await wfActivityControlService.delete(deletedCtrlId);
            }

            updateActivity(stepId, a.id, {
                controls: updatedControls,
                deletedControlServerIds: [],
                dirty: false,
            });

            // Sync WfActivityControlsOption (replace-set per saved control)
            try {
                for (const c of updatedControls) {
                    if (!c.serverId) continue;
                    await syncControlOptions(
                        wfActivityControlsOptionService, 'ActivityControlId', Number(c.serverId), c.type, c.optionsX,
                        (o, idx) => ({
                            activityControlId: Number(c.serverId), value: o.value, nameEn: o.en, nameAr: o.ar,
                            sortOrder: (idx + 1) * 10, isActive: true,
                        }),
                    );
                }
            } catch (err) {
                logger.error('Failed to sync activity control options', err);
                notify.error(t('workflow.failed_save_options', 'Failed to save control options'));
            }

            // Sync WfActivityMappingVariables
            try {
                const currentActMappings = await wfActivityMappingVariableService.getAll();
                const activityMappings = currentActMappings.filter((m: any) =>
                    updatedControls.some(uc => uc.serverId === m.activityControlId)
                );

                for (const c of updatedControls) {
                    const existingMapping = activityMappings.find((m: any) => m.activityControlId === c.serverId);
                    if (c.bindVariableId) {
                        const variable = variables.find(v => v.id === c.bindVariableId);
                        if (variable?.serverId) {
                            if (existingMapping) {
                                if (existingMapping.variableId !== variable.serverId || !existingMapping.isActive) {
                                    await wfActivityMappingVariableService.update(existingMapping.id!, {
                                        ...existingMapping,
                                        variableId: variable.serverId,
                                        isActive: true
                                    });
                                }
                            } else {
                                await wfActivityMappingVariableService.create({
                                    activityControlId: c.serverId!,
                                    variableId: variable.serverId,
                                    isActive: true
                                });
                            }
                        }
                    } else {
                        if (existingMapping) {
                            await wfActivityMappingVariableService.delete(existingMapping.id!);
                        }
                    }
                }
                qc.invalidateQueries({ queryKey: ['WfActivityMappingVariable', String(processInfo.id ?? '')] });
            } catch (err) {
                logger.error("Failed to sync activity mapping variables", err);
            }

            // Sync activity-control-level validations
            try {
                const { requestControlsValidations: allVals, deletedActivityControlsValidationEntries: deletedValEntries } = useProcessBuilderStore.getState();
                const activityCtrlIds = updatedControls.map(c => c.id);
                const actCtrlValidations = allVals.filter(v => v.activityControlId && activityCtrlIds.includes(v.activityControlId));
                const dirtyActCtrlVals = actCtrlValidations.filter(v => v.dirty);
                // Only delete server records that belonged to THIS activity's controls.
                const relevantDeletedIds = deletedValEntries
                    .filter(e => activityCtrlIds.includes(e.activityControlId))
                    .map(e => e.serverId);

                for (const deletedId of relevantDeletedIds) {
                    try { await wfActivityControlsValidationService.delete(deletedId); } catch { /* ignore */ }
                }

                const valCreates: any[] = [];
                const valUpdates: any[] = [];
                const valLocalToDto: { v: any; dto: any }[] = [];

                for (const v of dirtyActCtrlVals) {
                    const ctrl = updatedControls.find(c => c.id === v.activityControlId);
                    if (!ctrl?.serverId) continue;
                    const dto: any = {
                        activityControlId: ctrl.serverId,
                        validationType: v.validationType || 'Required',
                        validationExpression: v.validationExpression || '',
                        operator: v.operator || '',
                        value: v.value || '',
                        maskInput: v.maskInput || '',
                        errorMessageAr: v.errorMessageAr || '',
                        errorMessageEn: v.errorMessageEn || '',
                        severity: v.severity || 'Error',
                        sortOrder: v.sortOrder,
                        isActive: v.isActive,
                    };
                    valLocalToDto.push({ v, dto });
                    if (v.serverId) valUpdates.push({ ...dto, id: v.serverId });
                    else valCreates.push(dto);
                }

                let actualNewVals: any[] = [];
                if (valCreates.length > 0) {
                    const res = await wfActivityControlsValidationService.createRange(valCreates);
                    if (Array.isArray(res)) actualNewVals = res;
                }
                if (valUpdates.length > 0) await wfActivityControlsValidationService.updateRange(valUpdates);

                let newValIdx = 0;
                const { requestControlsValidations: currentVals } = useProcessBuilderStore.getState();
                const updatedVals = currentVals.map(v => {
                    if (!v.activityControlId || !activityCtrlIds.includes(v.activityControlId) || !v.dirty) return v;
                    if (v.serverId) return { ...v, dirty: false };
                    let srvId;
                    if (actualNewVals.length === valCreates.length) {
                        const rec: any = actualNewVals[newValIdx++];
                        srvId = typeof rec === 'object' ? (rec?.id ?? rec?.Id) : rec;
                    }
                    if (srvId) {
                        return { ...v, serverId: srvId, dirty: false };
                    }
                    return v; // Keep it dirty if it wasn't actually saved
                });
                useProcessBuilderStore.setState({
                    requestControlsValidations: updatedVals,
                    // Remove only entries that belonged to this activity's controls.
                    deletedActivityControlsValidationEntries: useProcessBuilderStore.getState().deletedActivityControlsValidationEntries.filter(e => !activityCtrlIds.includes(e.activityControlId)),
                });
                qc.invalidateQueries({ queryKey: ['WfActivityControlsValidation', String(processInfo.id ?? '')] });
            } catch (err) {
                logger.error('Failed to sync activity control validations', err);
            }

            // Sync activity-control-level transitions
            try {
                const { transitions: allTransitions, deletedActivityTransitionEntries: deletedTrEntries } = useProcessBuilderStore.getState();
                const activityCtrlIds = updatedControls.map(c => c.id);
                const actCtrlTransitions = allTransitions.filter(tr => tr.activityControlId && activityCtrlIds.includes(tr.activityControlId));
                const dirtyTr = actCtrlTransitions.filter(tr => tr.dirty);
                // Only delete server records that belonged to THIS activity's controls.
                const relevantDeletedTrIds = deletedTrEntries
                    .filter(e => activityCtrlIds.includes(e.activityControlId))
                    .map(e => e.serverId);

                for (const deletedId of relevantDeletedTrIds) {
                    try { await wfTransitionService.delete(deletedId); } catch { /* ignore */ }
                }

                const trCreates: any[] = [];
                const trUpdates: any[] = [];
                const trLocalToDto: { t: Transition; dto: any }[] = [];

                for (const tr of dirtyTr) {
                    if (!tr.variableId || !tr.operatorId || !tr.stepId) continue;
                    const { steps: stepsNow, variables: varsNow, requestControls: rcNow } = useProcessBuilderStore.getState();
                    const dto = mapLocalToWfTransition(tr, Number(processInfo.id) || 0, varsNow, stepsNow, rcNow);
                    trLocalToDto.push({ t: tr, dto });
                    if (tr.serverId) trUpdates.push({ ...dto, id: tr.serverId });
                    else trCreates.push(dto);
                }

                let actualNewTrs: any[] = [];
                if (trCreates.length > 0) {
                    const res = await wfTransitionService.createRange(trCreates);
                    if (Array.isArray(res)) actualNewTrs = res;
                }
                if (trUpdates.length > 0) await wfTransitionService.updateRange(trUpdates);

                let newTrIdx = 0;
                const { transitions: currentTrs } = useProcessBuilderStore.getState();
                const updatedTrs = currentTrs.map(tr => {
                    if (!tr.activityControlId || !activityCtrlIds.includes(tr.activityControlId) || !tr.dirty) return tr;
                    if (tr.serverId) return { ...tr, dirty: false };
                    let srvId;
                    if (actualNewTrs.length === trCreates.length) {
                        const rec: any = actualNewTrs[newTrIdx++];
                        srvId = typeof rec === 'object' ? (rec?.id ?? rec?.Id) : rec;
                    }
                    if (srvId) {
                        return { ...tr, serverId: srvId, dirty: false };
                    }
                    return tr; // Keep it dirty if it wasn't actually saved
                });
                useProcessBuilderStore.setState({
                    transitions: updatedTrs,
                    // Remove only entries that belonged to this activity's controls.
                    deletedActivityTransitionEntries: useProcessBuilderStore.getState().deletedActivityTransitionEntries.filter(e => !activityCtrlIds.includes(e.activityControlId)),
                });
                qc.invalidateQueries({ queryKey: ['WfTransition', String(processInfo.id ?? '')] });
            } catch (err) {
                logger.error('Failed to sync activity control transitions', err);
            }

            notify.success(a.serverId ? t('workflow.activity_updated', 'Activity updated') : t('workflow.activity_created', 'Activity created'));
            qc.invalidateQueries({ queryKey: ['WfActivity', String(processInfo.id ?? '')] });
            qc.invalidateQueries({ queryKey: ['WfActivityControl', String(processInfo.id ?? '')] });
        } catch { notify.error(t('workflow.failed_save_activity', 'Failed to save activity')); }
    };

    const saveTransitionsToBackend = async () => {
        const state = useProcessBuilderStore.getState();
        const { processInfo, transitions, deletedTransitionServerIds, variables, steps, requestControls } = state;

        if (!processInfo.id) {
            notify.error(t('workflow.save_process_first', 'Save Process Info first'));
            return;
        }
        // Request/process transitions only — activity-control transitions are
        // owned by the activity flow (saveActivityToBackend) and saved there.
        const dirty = transitions.filter((tr) => tr.dirty && !tr.activityControlId);
        if (dirty.length === 0 && deletedTransitionServerIds.length === 0) {
            notify.success(t('workflow.no_changes', 'No changes to save'));
            return;
        }
        for (const tr of dirty) {
            if (!tr.variableId || !tr.operatorId || !tr.stepId) {
                notify.error(t('workflow.transition_missing_fields', 'Transition missing Variable, Operator, or Target Step'));
                return;
            }
            const variable = variables.find(v => v.id === tr.variableId);
            const step = steps.find(s => s.id === tr.stepId);
            if (!variable?.serverId) {
                 notify.error(t('workflow.save_variables_first', 'You must save Variables before saving Transitions that use them.'));
                 return;
            }
            if (!step?.serverId) {
                 notify.error(t('workflow.save_steps_first', 'You must save Steps before saving Transitions that use them.'));
                 return;
            }
        }
        setTransitionsSavingLocal(true);
        try {
            // Delete transitions removed since last save
            for (const deletedId of deletedTransitionServerIds) {
                await wfTransitionService.delete(deletedId);
            }

            const creates: any[] = [];
            const updates: any[] = [];
            const localToDto: { t: Transition; dto: any }[] = [];

            for (const tr of dirty) {
                const dto = mapLocalToWfTransition(tr, Number(processInfo.id) || 0, variables, steps, requestControls);
                localToDto.push({ t: tr, dto });
                if (tr.serverId) updates.push({ ...dto, id: tr.serverId });
                else creates.push(dto);
            }

            let actualNewRecords: any[] = [];
            if (creates.length > 0) {
                const res = await wfTransitionService.createRange(creates);
                if (Array.isArray(res)) actualNewRecords = res;
                else {
                    const all = await wfTransitionService.getAll();
                    actualNewRecords = all.filter((x: any) => x.processId === Number(processInfo.id));
                }
            }
            if (updates.length > 0) await wfTransitionService.updateRange(updates);

            let newRecordIndex = 0;
            const updated = transitions.map((tr) => {
                // Leave activity-control transitions untouched — they're saved by the activity flow.
                if (!tr.dirty || tr.activityControlId) return tr;
                if (tr.serverId) return { ...tr, dirty: false };

                let srvId;
                if (actualNewRecords.length === creates.length) {
                    const createdRecord: any = actualNewRecords[newRecordIndex++];
                    srvId = typeof createdRecord === 'object' ? (createdRecord?.id ?? createdRecord?.Id) : createdRecord;
                } else {
                    const matchDto = localToDto.find(x => x.t.id === tr.id)?.dto;
                    const match = actualNewRecords.find((nr: any) =>
                        (nr.variableId ?? nr.VariableId) === matchDto?.variableId &&
                        (nr.stepId ?? nr.StepId) === matchDto?.stepId &&
                        (nr.value ?? nr.Value) === matchDto?.value
                    );
                    srvId = match?.id ?? match?.Id;
                }
                
                if (srvId) {
                    return { ...tr, serverId: srvId, dirty: false };
                }
                
                return tr; // Keep it dirty if it wasn't actually saved
            });

            useProcessBuilderStore.setState({ transitions: updated, deletedTransitionServerIds: [] });
            qc.invalidateQueries({ queryKey: ['WfTransition', String(processInfo.id ?? '')] });
            
            const stillDirty = updated.some(tr => tr.dirty && !tr.activityControlId);
            if (stillDirty) {
                notify.error(t('workflow.failed_save_transitions', 'Failed to save some transitions. Did you save Variables and Steps first?'));
            } else {
                notify.success(t('workflow.transitions_saved', 'Transitions saved'));
            }
        } catch {
            notify.error(t('workflow.failed_save_transitions', 'Failed to save transitions'));
        } finally {
            setTransitionsSavingLocal(false);
        }
    };

    return {
        ...mutations,
        saveProcessInfoToBackend,
        saveVariablesToBackend,
        saveStepsToBackend,
        saveRequestControlsToBackend,
        saveActivityToBackend,
        saveTransitionsToBackend,
        transitionsSaving: transitionsSavingLocal || mutations.transitionsSaving,
    };
};
