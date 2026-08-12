import React, { useState, useCallback } from 'react';
import {
    Stack, Typography, Chip, TextField, FormControlLabel, Switch,
    FormControl, InputLabel, Select, MenuItem, Button, Divider, Paper, IconButton, Box,
    FormHelperText,
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useParams } from 'react-router-dom';
import { DynamicField } from '../../../../../../components/common/DynamicField';
import { Add, Delete, AccountTree } from '@mui/icons-material';
import { FormGridLookupField } from '../../../../../../components/common/GridLookup/FormGridLookup';
import { queryKeys } from '../../../../../../config/query-keys';
import { wfCategoryService } from '../../../../api';
import type { WfCategory } from '../../../../types';
import { useProcessBuilderContext } from '../../context/ProcessBuilderContext';
import { useProcessBuilderData } from '../../hooks/useProcessBuilderData';
import { inferDataType } from '../../utils/ProcessBuilderMappers';
import { GeneratableTextField } from '../../../../../../components/common/GeneratableTextField';
import { BilingualField } from '../../../../../../components/common/BilingualField';

export const ProcessSettingsPanel: React.FC = React.memo(() => {
    const { i18n, t } = useTranslation();
    const isRtl = i18n.language === 'ar';
    const { id: routeId } = useParams<{ id: string }>();
    const isEditMode = !!routeId && routeId !== 'new';

    const {
        processInfo, setProcessInfo,
        variables, addVariable, updateVariable, deleteVariable
    } = useProcessBuilderContext();

    const {
        loadingProcess, priorities, dataTypes, processSaving, variablesSaving, saveProcessInfoToBackend, saveVariablesToBackend
    } = useProcessBuilderData(processInfo.id, isEditMode, true);

    const piSaving = processSaving;

    const setPI = (patch: Partial<typeof processInfo>) => setProcessInfo(prev => ({ ...prev, ...patch }));

    // ── Inline validation ─────────────────────────────────────────────────────
    // Errors are only shown after the first save attempt.
    const [showErrors, setShowErrors] = useState(false);
    const [showVariableErrors, setShowVariableErrors] = useState(false);

    const errors = showErrors ? {
        code:       !processInfo.code?.trim()     ? t('common.required', 'Required') : undefined,
        name:       !(isRtl ? processInfo.nameAR : processInfo.name)?.trim()
                        ? t('common.required', 'Required') : undefined,
        categoryId: !processInfo.categoryId       ? t('common.required', 'Required') : undefined,
    } : { code: undefined, name: undefined, categoryId: undefined };

    const handleSaveVariables = useCallback(async () => {
        setShowVariableErrors(true);
        const allValid = variables.every(v => v.name?.trim() && v.dataTypeId);
        if (!allValid) return;
        await saveVariablesToBackend();
    }, [variables, saveVariablesToBackend]);

    const handleSave = useCallback(async () => {
        setShowErrors(true);
        const codeOk     = !!processInfo.code?.trim();
        const nameOk     = !!(isRtl ? processInfo.nameAR : processInfo.name)?.trim();
        const categoryOk = !!processInfo.categoryId;
        if (!codeOk || !nameOk || !categoryOk) {
            return;
        }
        await saveProcessInfoToBackend();
    }, [processInfo, isRtl, saveProcessInfoToBackend]);

    return (
        <Stack spacing={2} sx={{ p: 2 }}>
            <Stack direction="row" alignItems="center" spacing={1}>
                <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>{t('workflow.process_information', 'Process Information')}</Typography>
                {processInfo.id ? (
                    <Chip size="small" color="success" label={`ID: ${processInfo.id}`} sx={{ borderRadius: 1.5, fontWeight: 600 }} />
                ) : (
                    <Chip size="small" label={t('common.new', 'New')} sx={{ borderRadius: 1.5, fontWeight: 600 }} />
                )}
            </Stack>

            <Stack direction="row" spacing={1}>
                <Box sx={{ flex: 1 }}>
                    <GeneratableTextField
                        label={t('common.code')}
                        value={processInfo.code ?? ''}
                        onChange={(val) => setPI({ code: val })}
                        sequenceType="WfProcess"
                        required
                        error={!!errors.code}
                        helperText={errors.code}
                        disabled={!!processInfo.id}
                    />
                </Box>
                <FormControlLabel
                    control={<Switch checked={processInfo.isActive}
                        onChange={(e) => setPI({ isActive: e.target.checked })} />}
                    label={t('common.active', 'Active')} />
            </Stack>

            <BilingualField
                labelEN={t('common.name')}
                labelAR={t('common.name_ar')}
                nameValue={processInfo.name}
                nameARValue={processInfo.nameAR}
                onChange={(name, nameAR) => setPI({ name, nameAR })}
                required
                error={!!errors.name}
                errorMessage={errors.name}
            />

            <BilingualField
                labelEN={t('common.description', 'Description')}
                labelAR={t('common.description_ar', 'Description (AR)')}
                nameValue={processInfo.description}
                nameARValue={processInfo.descriptionAR}
                onChange={(description, descriptionAR) => setPI({ description, descriptionAR })}
                multiline
                rows={3}
            />

            <FormGridLookupField<WfCategory>
                value={processInfo.categoryId || null}
                onChange={(val) => setPI({ categoryId: (val as number) ?? '' })}
                columns={[
                    { field: 'code', header: 'common.code', width: 110 },
                    { field: 'name', header: 'common.name', flex: 1 },
                    { field: 'nameAR', header: 'common.name_ar', flex: 1 },
                ]}
                queryKey={queryKeys.entities.list('WfCategory') as any}
                fetchPage={({ pageNumber, pageSize, search, signal }) =>
                    wfCategoryService.getPaged(pageNumber, [], search, [], [], pageSize, signal)
                }
                fetchById={(id) => wfCategoryService.getById(id as number)}
                placeholder={t('workflow.category', 'Category')}
                size="small"
                required
                errorMessage={errors.categoryId}
            />

            <FormControl size="small" sx={{ '& .MuiOutlinedInput-root': { borderRadius: 1.5 } }}>
                <InputLabel>{t('workflow.priority', 'Priority')}</InputLabel>
                <Select label={t('workflow.priority', 'Priority')} value={processInfo.priorityId}
                    onChange={(e) => setPI({ priorityId: e.target.value as number | '' })}>
                    <MenuItem value=""><em>({t('common.none', 'none')})</em></MenuItem>
                    {priorities.map((p) =>
                        <MenuItem key={p.id} value={p.id}>{isRtl ? p.nameAR || p.name : p.name} ({p.code})</MenuItem>)}
                </Select>
            </FormControl>

            <FormControl size="small" sx={{ '& .MuiOutlinedInput-root': { borderRadius: 1.5 } }}>
                <InputLabel>{t('workflow.process_type', 'Process Type')}</InputLabel>
                <Select label={t('workflow.process_type', 'Process Type')} value={processInfo.processType}
                    onChange={(e) => setPI({ processType: e.target.value })}>
                    <MenuItem value="Process">{t('workflow.process', 'Process')}</MenuItem>
                    <MenuItem value="Evaluation">{t('workflow.evaluation', 'Evaluation')}</MenuItem>
                    <MenuItem value="Visit">{t('workflow.visit', 'Visit')}</MenuItem>
                </Select>
            </FormControl>

            <DynamicField config={{ name: 'score', label: t('workflow.score_weight', 'Score / Weight'), type: 'number' }} value={processInfo.score}
                onChange={(val: string | number) => setPI({ score: Number(val) || 0 })} />

            <Stack direction="row" spacing={2}>
                <FormControlLabel
                    control={<Switch checked={processInfo.canRepeat}
                        onChange={(e) => setPI({ canRepeat: e.target.checked })} />}
                    label={t('workflow.can_repeat', 'Can Repeat')} />
                <FormControlLabel
                    control={<Switch checked={processInfo.mandatoryDocs}
                        onChange={(e) => setPI({ mandatoryDocs: e.target.checked })} />}
                    label={t('workflow.mandatory_docs', 'Mandatory Docs')} />
            </Stack>

            <Button variant="contained" size="medium" startIcon={<AccountTree />}
                disabled={piSaving || loadingProcess}
                onClick={handleSave}
                sx={{ borderRadius: 1.5, py: 1, textTransform: 'none', fontWeight: 600 }}>
                {piSaving ? t('common.saving', 'Saving…') : processInfo.id ? t('workflow.update_process', 'Update Process') : t('workflow.create_process', 'Create Process')}
            </Button>

            <Divider />
            <Stack direction="row" alignItems="center">
                <Typography variant="subtitle2" sx={{ flexGrow: 1, fontWeight: 700 }}>
                    {t('workflow.variables', 'Variables')} {variables.some((v) => v.dirty) &&
                        <Chip size="small" color="warning" label={t('common.unsaved', 'unsaved')} sx={{ ml: 1, height: 18, borderRadius: 1 }} />}
                </Typography>
                <Button size="small" startIcon={<Add />} disabled={!processInfo.id}
                    onClick={addVariable} sx={{ textTransform: 'none', fontWeight: 600 }}>{t('common.add', 'Add')}</Button>
            </Stack>
            {!processInfo.id && (
                <Typography variant="caption" color="warning.main">
                    {t('workflow.save_process_for_variables', 'Save the Process first to enable variable creation (ProcessId required).')}
                </Typography>
            )}
            <Stack spacing={1}>
                {variables.map((v) => (
                    <Paper key={v.id} variant="outlined" sx={{
                        p: 1, borderColor: v.dirty ? 'warning.main' : undefined,
                        borderRadius: 1.5
                    }}>
                        <Stack direction="row" alignItems="center" spacing={0.5}>
                            <TextField size="small" placeholder={t('common.code')} value={v.code}
                                onChange={(e) => updateVariable(v.id, { code: e.target.value })}
                                sx={{ flex: 1, '& .MuiOutlinedInput-root': { borderRadius: 1 } }} />
                            <TextField size="small" placeholder={`${t('common.name')} *`} value={v.name}
                                onChange={(e) => updateVariable(v.id, { name: e.target.value })}
                                error={showVariableErrors && !v.name?.trim()}
                                helperText={showVariableErrors && !v.name?.trim() ? t('common.required', 'Required') : undefined}
                                sx={{ flex: 2, '& .MuiOutlinedInput-root': { borderRadius: 1 } }} />
                            <IconButton size="small" color="error" onClick={() => deleteVariable(v.id)}>
                                <Delete fontSize="small" />
                            </IconButton>
                        </Stack>
                        <Stack direction="row" spacing={1} alignItems="center" sx={{ mt: 1 }}>
                            <FormControl size="small" error={showVariableErrors && !v.dataTypeId} sx={{ flex: 2, '& .MuiOutlinedInput-root': { borderRadius: 1 } }}>
                                <Select displayEmpty value={v.dataTypeId}
                                    onChange={(e) => {
                                        const id = e.target.value as number | '';
                                        updateVariable(v.id, { dataTypeId: id, dataType: inferDataType(id, dataTypes) as any });
                                    }}>
                                    <MenuItem value=""><em>{t('workflow.data_type', 'Data type *')}</em></MenuItem>
                                    {dataTypes.map((d) =>
                                        <MenuItem key={d.id} value={d.id}>{isRtl ? d.nameAR || d.name : d.name} ({d.code})</MenuItem>)}
                                </Select>
                                {showVariableErrors && !v.dataTypeId && <FormHelperText>{t('common.required', 'Required')}</FormHelperText>}
                            </FormControl>
                            <TextField size="small" type="number" placeholder={t('common.sort_order', 'Sort')} value={v.sortOrder}
                                onChange={(e) => updateVariable(v.id, { sortOrder: Number(e.target.value) || 0 })}
                                sx={{ width: 70, '& .MuiOutlinedInput-root': { borderRadius: 1 } }} />
                            <FormControlLabel
                                control={<Switch size="small" checked={v.isActive}
                                    onChange={(e) => updateVariable(v.id, { isActive: e.target.checked })} />}
                                label={t('common.active', 'Active')} sx={{ '& .MuiTypography-root': { fontSize: '0.8rem' } }} />
                        </Stack>
                    </Paper>
                ))}
            </Stack>
            <Button variant="contained" startIcon={<AccountTree />}
                disabled={!processInfo.id || variablesSaving}
                onClick={handleSaveVariables}
                sx={{ borderRadius: 1.5, textTransform: 'none', fontWeight: 600, py: 1 }}>
                {variablesSaving ? t('common.saving', 'Saving…') : t('workflow.save_variables', 'Save Variables')}
            </Button>
        </Stack>
    );
});

ProcessSettingsPanel.displayName = 'ProcessSettingsPanel';
