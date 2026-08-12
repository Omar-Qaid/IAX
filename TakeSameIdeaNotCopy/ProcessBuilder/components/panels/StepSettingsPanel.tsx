import React, { useState, useMemo } from 'react';
import {
    Stack, Box, Typography, Chip, FormControlLabel, Switch, Accordion, AccordionSummary, AccordionDetails, Button
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useParams } from 'react-router-dom';
import { DynamicField } from '../../../../../../components/common/DynamicField';
import { ExpandMore, PlaylistAddCheck } from '@mui/icons-material';

import { ConditionBuilder } from '../shared/ConditionBuilder';
import { useProcessBuilderContext } from '../../context/ProcessBuilderContext';
import { useProcessBuilderData } from '../../hooks/useProcessBuilderData';
import { GeneratableTextField } from '../../../../../../components/common/GeneratableTextField';
import { BilingualField } from '../../../../../../components/common/BilingualField';

export const StepSettingsPanel: React.FC = React.memo(() => {
    const { i18n, t } = useTranslation();
    const isRtl = i18n.language === 'ar';
    const { id: routeId } = useParams<{ id: string }>();
    const isEditMode = !!routeId && routeId !== 'new';

    const {
        steps, updateStep, selectedNode, processInfo,
        variables, requestControls
    } = useProcessBuilderContext();

    const { stepsSaving, saveStepsToBackend } = useProcessBuilderData(processInfo.id, isEditMode, true);

    const allFieldNames = useMemo(() => {
        const names: string[] = variables.map((v) => v.name);
        requestControls.forEach((c) => names.push(c.name));
        steps.forEach((s) => s.activities.forEach((a) => a.controls.forEach((c) => names.push(c.name))));
        return names;
    }, [variables, requestControls, steps]);

    const [showErrors, setShowErrors] = useState(false);

    if (selectedNode.kind !== 'step') return null;
    const step = steps.find(s => s.id === selectedNode.id);
    if (!step) return null;

    const errors = showErrors ? {
        code: !step.code?.trim() ? t('common.required', 'Required') : undefined,
        name: !(isRtl ? step.nameAR : step.name)?.trim() ? t('common.required', 'Required') : undefined,
    } : { code: undefined, name: undefined };

    const handleSave = async () => {
        setShowErrors(true);
        if (!step.code?.trim() || !(isRtl ? step.nameAR : step.name)?.trim()) return;
        await saveStepsToBackend();
    };

    return (
        <Stack spacing={2.5} sx={{ p: 2.5 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Typography variant="h6" sx={{ fontWeight: 700, color: 'text.primary' }}>{t('workflow.step_settings', 'Step Settings')}</Typography>
                {step.dirty && <Chip size="small" color="warning" label={t('common.unsaved', 'unsaved')} sx={{ borderRadius: 1, fontWeight: 600 }} />}
            </Box>

            <GeneratableTextField
                label={t('workflow.step_code', 'Step Code')}
                value={step.code ?? ''}
                onChange={(val) => updateStep(step.id, { code: val })}
                sequenceType="WfStep"
                required
                error={!!errors.code}
                helperText={errors.code}
                disabled={!!step.serverId}
            />

            <BilingualField
                labelEN={t('workflow.step_name_en', 'Step Name')}
                labelAR={t('workflow.step_name_ar', 'Step Name (AR)')}
                nameValue={step.name}
                nameARValue={step.nameAR}
                onChange={(name, nameAR) => updateStep(step.id, { name, nameAR })}
                required
                error={!!errors.name}
                errorMessage={errors.name}
            />

            <Stack direction="row" spacing={1.5}>
                <Box sx={{ flex: 1 }}>
                    <DynamicField config={{ name: 'order', label: t('common.sort_order', 'Sort Order'), type: 'number' }} value={step.order}
                        onChange={(val) => updateStep(step.id, { order: Number(val) || 0 })} />
                </Box>
                <Box sx={{ flex: 1 }}>
                    <DynamicField config={{ name: 'score', label: t('common.score', 'Score'), type: 'number' }} value={step.score ?? 0}
                        onChange={(val) => updateStep(step.id, { score: Number(val) || 0 })} />
                </Box>
            </Stack>

            <DynamicField config={{ name: 'autoPassingHrs', label: t('workflow.auto_passing_hrs', 'Auto Passing Hours'), type: 'number' }} value={step.autoPassingHrs ?? 0}
                onChange={(val) => updateStep(step.id, { autoPassingHrs: Number(val) || 0 })} />

            <Box sx={{
                border: '1px solid',
                borderColor: 'divider',
                borderRadius: 2,
                p: 1.5,
                bgcolor: 'background.paper',
                display: 'flex',
                flexDirection: 'column',
                gap: 1,
            }}>
                <FormControlLabel
                    sx={{ m: 0, justifyContent: 'space-between', display: 'flex' }}
                    labelPlacement="start"
                    control={<Switch checked={!!step.allMandatory}
                        onChange={(e) => updateStep(step.id, { allMandatory: e.target.checked })}
                        sx={{
                            '& .MuiSwitch-switchBase.Mui-checked': { color: '#6366f1' },
                            '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { bgcolor: '#6366f1' }
                        }} />}
                    label={<Typography variant="body2" sx={{ fontWeight: 500 }}>{t('workflow.all_mandatory', 'All Mandatory')}</Typography>} />

                <FormControlLabel
                    sx={{ m: 0, justifyContent: 'space-between', display: 'flex' }}
                    labelPlacement="start"
                    control={<Switch checked={step.isActive !== false}
                        onChange={(e) => updateStep(step.id, { isActive: e.target.checked })}
                        sx={{
                            '& .MuiSwitch-switchBase.Mui-checked': { color: '#6366f1' },
                            '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { bgcolor: '#6366f1' }
                        }} />}
                    label={<Typography variant="body2" sx={{ fontWeight: 500 }}>{t('workflow.active_step', 'Active Step')}</Typography>} />

                <FormControlLabel
                    sx={{ m: 0, justifyContent: 'space-between', display: 'flex' }}
                    labelPlacement="start"
                    control={<Switch checked={!!step.sysField}
                        onChange={(e) => updateStep(step.id, { sysField: e.target.checked })}
                        sx={{
                            '& .MuiSwitch-switchBase.Mui-checked': { color: '#6366f1' },
                            '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { bgcolor: '#6366f1' }
                        }} />}
                    label={<Typography variant="body2" sx={{ fontWeight: 500 }}>{t('workflow.system_field', 'System Field')}</Typography>} />
            </Box>

            <Accordion variant="outlined" sx={{ borderRadius: '2px !important', overflow: 'hidden' }}>
                <AccordionSummary expandIcon={<ExpandMore />}>
                    <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>{t('workflow.step_condition', 'Step Condition')}</Typography>
                </AccordionSummary>
                <AccordionDetails sx={{ p: 1.5 }}>
                    <Stack spacing={1}>
                        <ConditionBuilder group={step.condition} onChange={(g) => updateStep(step.id, { condition: g })} fields={allFieldNames} />
                    </Stack>
                </AccordionDetails>
            </Accordion>

            <Button variant="contained" startIcon={<PlaylistAddCheck />}
                disabled={!processInfo.id || stepsSaving}
                onClick={handleSave}
                sx={{
                    borderRadius: 2,
                    py: 1.25,
                    textTransform: 'none',
                    fontWeight: 600,
                    boxShadow: 'none',
                    '&:hover': {
                        boxShadow: 'none',
                    }
                }}>
                {stepsSaving ? t('common.saving_changes', 'Saving Changes…') : t('workflow.save_steps_db', 'Save Steps to DB')}
            </Button>
        </Stack>
    );
});

StepSettingsPanel.displayName = 'StepSettingsPanel';
