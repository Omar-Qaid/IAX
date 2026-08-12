import React, { useState } from 'react';
import {
    Box, Typography, Button, Paper, TextField, FormControlLabel, Switch, IconButton, Chip, Stack
} from '@mui/material';
import { Add, PlaylistAddCheck, DragIndicator, Delete } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { GeneratableTextField } from '../../../../../../components/common/GeneratableTextField';
import { DndContext, closestCenter, type DragEndEvent, useSensors, useSensor, PointerSensor } from '@dnd-kit/core';
import { SortableContext, verticalListSortingStrategy, arrayMove } from '@dnd-kit/sortable';
import { useProcessBuilderContext } from '../../context/ProcessBuilderContext';
import { SortableItem } from '../shared/SortableItem';

interface StepsTabProps {
    stepsSaving: boolean;
    saveStepsToBackend: () => void;
    setCenterTab: (tab: number) => void;
}

export const StepsTab: React.FC<StepsTabProps> = React.memo(({
    stepsSaving,
    saveStepsToBackend,
    setCenterTab,
}) => {
    const { i18n, t } = useTranslation();
    const isRtl = i18n.language === 'ar';

    const {
        processInfo,
        steps,
        setSteps,
        addStep,
        updateStep,
        deleteStep,
        setSelectedNode: setSelected,
    } = useProcessBuilderContext();

    const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));
    const [showErrors, setShowErrors] = useState(false);

    const onStepDragEnd = (e: DragEndEvent) => {
        const { active, over } = e;
        if (!over || active.id === over.id) return;
        setSteps((prev) => {
            const oldIdx = prev.findIndex((s) => s.id === active.id);
            const newIdx = prev.findIndex((s) => s.id === over.id);
            return arrayMove(prev, oldIdx, newIdx).map((s, i) => ({ ...s, order: i + 1, dirty: true }));
        });
    };

    const handleSave = () => {
        setShowErrors(true);
        const anyEmpty = steps.some(s => !(isRtl ? s.nameAR : s.name)?.trim());
        if (anyEmpty) return;
        saveStepsToBackend();
    };

    return (
        <Box sx={{ p: 2, width: '100%', maxWidth: '100%', boxSizing: 'border-box', minWidth: 0 }}>
            <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2, flexWrap: 'wrap' }}>
                <Typography variant="h6" sx={{ flexGrow: 1 }}>
                    {t('workflow.workflow_steps', 'Workflow Steps')}
                </Typography>
                {steps.some((s) => s.dirty) && (
                    <Chip size="small" color="warning" label={t('workflow.unsaved_changes', 'unsaved changes')} />
                )}
                <Button startIcon={<Add />} variant="outlined" size="small" onClick={addStep}>
                    {t('workflow.add_step', 'Add Step')}
                </Button>
                <Button
                    variant="contained"
                    size="small"
                    startIcon={<PlaylistAddCheck />}
                    disabled={!processInfo.id || stepsSaving}
                    onClick={handleSave}
                >
                    {stepsSaving ? t('common.saving', 'Saving…') : t('workflow.save_steps', 'Save Steps')}
                </Button>
            </Stack>
            {!processInfo.id && (
                <Typography variant="caption" color="warning.main" sx={{ display: 'block', mb: 1 }}>
                    {t('workflow.save_process_first_steps', 'Save the Process first to enable steps (ProcessId required).')}
                </Typography>
            )}

            {steps.length === 0 ? (
                <Paper variant="outlined" sx={{ p: 4, textAlign: 'center' }}>
                    <Typography color="text.secondary">
                        {t('workflow.no_steps_yet', 'No steps yet. Click "Add Step" to create one.')}
                    </Typography>
                </Paper>
            ) : (
                <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={onStepDragEnd}>
                    <SortableContext items={steps.map((s) => s.id)} strategy={verticalListSortingStrategy}>
                        <Stack spacing={1.5}>
                            {steps.map((step, idx) => (
                                <SortableItem key={step.id} id={step.id}>
                                    {(handle) => (
                                        <Paper
                                            variant="outlined"
                                            onClick={() => setSelected({ kind: 'step', id: step.id })}
                                            onFocusCapture={() => setSelected({ kind: 'step', id: step.id })}
                                            sx={{
                                                p: 1.5,
                                                borderColor: step.dirty ? 'warning.main' : undefined,
                                                boxShadow: step.dirty ? '0 0 4px rgba(237, 108, 2, 0.1)' : undefined,
                                                transition: 'border-color 0.2s, box-shadow 0.2s',
                                                cursor: 'pointer',
                                            }}
                                        >
                                            <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1, flexWrap: 'wrap', gap: 1 }}>
                                                <Box {...handle} sx={{ cursor: 'grab', display: 'flex' }}>
                                                    <DragIndicator />
                                                </Box>
                                                <Chip
                                                    label={`#${step.order ?? idx + 1}`}
                                                    size="small"
                                                    color="primary"
                                                    sx={{ fontWeight: 600 }}
                                                />

                                                <Box sx={{ width: 180 }}>
                                                    <GeneratableTextField
                                                        label={t('common.code', 'Code')}
                                                        value={step.code ?? ''}
                                                        onChange={(val) => updateStep(step.id, { code: val })}
                                                        sequenceType="WfStep"
                                                    />
                                                </Box>

                                                {isRtl ? (
                                                    <TextField
                                                        size="small"
                                                        label={`${t('common.name_ar', 'Name (AR)')} *`}
                                                        value={step.nameAR ?? ''}
                                                        onChange={(e) => updateStep(step.id, { nameAR: e.target.value, name: e.target.value })}
                                                        error={showErrors && !step.nameAR?.trim()}
                                                        helperText={showErrors && !step.nameAR?.trim() ? t('common.required', 'Required') : undefined}
                                                        sx={{ flexGrow: 1, minWidth: 150 }}
                                                        dir="rtl"
                                                    />
                                                ) : (
                                                    <TextField
                                                        size="small"
                                                        label={`${t('workflow.step_name', 'Step Name')} *`}
                                                        value={step.name}
                                                        onChange={(e) => updateStep(step.id, { name: e.target.value, nameAR: e.target.value })}
                                                        error={showErrors && !step.name?.trim()}
                                                        helperText={showErrors && !step.name?.trim() ? t('common.required', 'Required') : undefined}
                                                        sx={{ flexGrow: 1, minWidth: 150 }}
                                                    />
                                                )}

                                                <Chip
                                                    label={`${step.activities.length} ${t('workflow.act_short', 'act.')}`}
                                                    size="small"
                                                    variant="outlined"
                                                />

                                                <Button
                                                    size="small"
                                                    variant="text"
                                                    onClick={() => { setSelected({ kind: 'step', id: step.id }); setCenterTab(3); }}
                                                >
                                                    {t('workflow.activities', 'Activities')}
                                                </Button>
                                                <Button
                                                    size="small"
                                                    variant="text"
                                                    onClick={() => { setSelected({ kind: 'step', id: step.id }); }}
                                                >
                                                    {t('workflow.configure', 'Configure')}
                                                </Button>
                                                <IconButton size="small" color="error" onClick={() => deleteStep(step.id)}>
                                                    <Delete fontSize="small" />
                                                </IconButton>
                                            </Stack>
                                            <Stack direction="row" spacing={1} sx={{ mt: 1, flexWrap: 'wrap', gap: 1 }} alignItems="center">
                                                <TextField
                                                    size="small"
                                                    type="number"
                                                    label={t('workflow.score', 'Score')}
                                                    value={step.score ?? 0}
                                                    onChange={(e) => updateStep(step.id, { score: Number(e.target.value) || 0 })}
                                                    sx={{ width: 80 }}
                                                />
                                                <TextField
                                                    size="small"
                                                    type="number"
                                                    label={t('workflow.auto_passing_hrs', 'Auto Passing (hrs)')}
                                                    value={step.autoPassingHrs ?? 0}
                                                    onChange={(e) => updateStep(step.id, { autoPassingHrs: Number(e.target.value) || 0 })}
                                                    sx={{ width: 140 }}
                                                />
                                                <FormControlLabel
                                                    control={
                                                        <Switch
                                                            size="small"
                                                            checked={!!step.allMandatory}
                                                            onChange={(e) => updateStep(step.id, { allMandatory: e.target.checked })}
                                                            sx={{
                                                                '& .MuiSwitch-switchBase.Mui-checked': { color: '#6366f1' },
                                                                '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { bgcolor: '#6366f1' }
                                                            }}
                                                        />
                                                    }
                                                    label={<Typography variant="caption">{t('workflow.all_mandatory', 'All Mandatory')}</Typography>}
                                                />
                                                <FormControlLabel
                                                    control={
                                                        <Switch
                                                            size="small"
                                                            checked={step.isActive !== false}
                                                            onChange={(e) => updateStep(step.id, { isActive: e.target.checked })}
                                                            sx={{
                                                                '& .MuiSwitch-switchBase.Mui-checked': { color: '#6366f1' },
                                                                '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { bgcolor: '#6366f1' }
                                                            }}
                                                        />
                                                    }
                                                    label={<Typography variant="caption">{t('common.active', 'Active')}</Typography>}
                                                />
                                                <FormControlLabel
                                                    control={
                                                        <Switch
                                                            size="small"
                                                            checked={!!step.sysField}
                                                            onChange={(e) => updateStep(step.id, { sysField: e.target.checked })}
                                                            sx={{
                                                                '& .MuiSwitch-switchBase.Mui-checked': { color: '#6366f1' },
                                                                '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { bgcolor: '#6366f1' }
                                                            }}
                                                        />
                                                    }
                                                    label={<Typography variant="caption">{t('workflow.sys_field', 'Sys Field')}</Typography>}
                                                />
                                                {step.dirty && (
                                                    <Chip
                                                        label={t('common.unsaved', 'unsaved')}
                                                        size="small"
                                                        color="warning"
                                                        variant="outlined"
                                                        sx={{ height: 18, fontSize: 10, ml: 'auto' }}
                                                    />
                                                )}
                                            </Stack>
                                        </Paper>
                                    )}
                                </SortableItem>
                            ))}
                        </Stack>
                    </SortableContext>
                </DndContext>
            )}
        </Box>
    );
});

StepsTab.displayName = 'StepsTab';
export default StepsTab;
