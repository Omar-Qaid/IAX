import React, { useState } from 'react';
import {
    Box, Typography, Button, Paper, TextField, FormControl,
    InputLabel, Select, MenuItem, FormControlLabel, Switch, IconButton, Chip, Stack
} from '@mui/material';
import { Add, DragIndicator, Delete } from '@mui/icons-material';
import { GeneratableTextField } from '../../../../../../components/common/GeneratableTextField';
import { useTranslation } from 'react-i18next';
import {
    DndContext, closestCenter, DragOverlay, useSensors, useSensor, PointerSensor,
    type DragStartEvent, type DragEndEvent
} from '@dnd-kit/core';
import { SortableContext, verticalListSortingStrategy, arrayMove } from '@dnd-kit/sortable';
import { useProcessBuilderContext } from '../../context/ProcessBuilderContext';
import { SortableItem } from '../shared/SortableItem';
import { ACTIVITY_TYPES, getActivityIcon } from '../../utils/palette';
import type { ActivityType } from '../../types';
import type { WfActivityType, WfPerformer } from '../../../../types';

interface DesignerTabProps {
    addStep: () => void;
    addActivity: (stepId: string, type: ActivityType) => void;
    activityTypes: WfActivityType[];
    performers: WfPerformer[];
}

export const DesignerTab: React.FC<DesignerTabProps> = React.memo(({
    addStep,
    addActivity,
    activityTypes,
    performers,
}) => {
    const { i18n, t } = useTranslation();
    const isRtl = i18n.language === 'ar';

    const {
        steps,
        setSteps,
        setSelectedNode: setSelected,
        updateStep,
        deleteStep,
        updateActivity,
        deleteActivity,
        setCenterTab,
    } = useProcessBuilderContext();

    const [activeStepId, setActiveStepId] = useState<string | null>(null);
    const [activeActivityId, setActiveActivityId] = useState<string | null>(null);

    const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));

    const handleStepDragStart = (e: DragStartEvent) => {
        setActiveStepId(String(e.active.id));
    };

    const handleStepDragEnd = (e: DragEndEvent) => {
        setActiveStepId(null);
        const { active, over } = e;
        if (!over || active.id === over.id) return;
        setSteps((prev) => {
            const oldIdx = prev.findIndex((s) => s.id === active.id);
            const newIdx = prev.findIndex((s) => s.id === over.id);
            return arrayMove(prev, oldIdx, newIdx).map((s, i) => ({ ...s, order: i + 1, dirty: true }));
        });
    };

    const handleActivityDragStart = (e: DragStartEvent) => {
        setActiveActivityId(String(e.active.id));
    };

    const handleActivityDragEnd = (stepId: string) => (e: DragEndEvent) => {
        setActiveActivityId(null);
        const { active, over } = e;
        if (!over || active.id === over.id) return;
        setSteps((prev) => prev.map((s) => {
            if (s.id !== stepId) return s;
            const oldIdx = s.activities.findIndex((a) => a.id === active.id);
            const newIdx = s.activities.findIndex((a) => a.id === over.id);
            return {
                ...s,
                activities: arrayMove(s.activities, oldIdx, newIdx).map((a) => ({ ...a, dirty: true }))
            };
        }));
    };

    // Render step layout preview inside DragOverlay
    const activeStep = steps.find(s => s.id === activeStepId);
    const activeActivity = steps.flatMap(s => s.activities).find(a => a.id === activeActivityId);

    return (
        <Box sx={{ p: 2, width: '100%', maxWidth: '100%', boxSizing: 'border-box', minWidth: 0 }}>
            <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
                <Typography variant="h6">{t('workflow.workflow_designer', 'Workflow Designer')}</Typography>
                <Box sx={{ flexGrow: 1 }} />
                <Button startIcon={<Add />} variant="outlined" size="small" onClick={addStep}>
                    {t('workflow.add_step', 'Add Step')}
                </Button>
            </Stack>

            <DndContext
                sensors={sensors}
                collisionDetection={closestCenter}
                onDragStart={handleStepDragStart}
                onDragEnd={handleStepDragEnd}
            >
                <SortableContext items={steps.map((s) => s.id)} strategy={verticalListSortingStrategy}>
                    <Stack spacing={2}>
                        {steps.map((step, idx) => (
                            <SortableItem key={step.id} id={step.id}>
                                {(handle) => (
                                    <Paper
                                        variant="outlined"
                                        sx={{
                                            p: 2,
                                            boxShadow: '0 4px 12px rgba(0,0,0,0.02)',
                                            borderRadius: 1.5,
                                            transition: 'transform 0.15s, box-shadow 0.15s',
                                            '&:hover': {
                                                boxShadow: '0 6px 16px rgba(0,0,0,0.04)',
                                            }
                                        }}
                                    >
                                        <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1.5, flexWrap: 'wrap', gap: 1 }}>
                                            <Box {...handle} sx={{ cursor: 'grab', display: 'flex' }}>
                                                <DragIndicator />
                                            </Box>
                                            <Chip label={`#${step.order ?? idx + 1}`} size="small" color="primary" sx={{ fontWeight: 600 }} />
                                            <Chip
                                                label={step.status}
                                                size="small"
                                                color={step.status === 'completed' ? 'success'
                                                    : step.status === 'active' ? 'info'
                                                    : step.status === 'skipped' ? 'default' : 'warning'}
                                                sx={{ textTransform: 'capitalize' }}
                                            />
                                            <TextField
                                                size="small"
                                                value={step.name}
                                                onChange={(e) => updateStep(step.id, { name: e.target.value })}
                                                sx={{ flexGrow: 1 }}
                                            />
                                            <Button size="small" variant="text" onClick={() => setSelected({ kind: 'step', id: step.id })}>
                                                {t('workflow.configure', 'Configure')}
                                            </Button>
                                            <IconButton size="small" color="error" onClick={() => deleteStep(step.id)}>
                                                <Delete fontSize="small" />
                                            </IconButton>
                                        </Stack>

                                        <Stack direction="row" spacing={1} sx={{ mb: 1.5, flexWrap: 'wrap', gap: 0.5 }}>
                                            {ACTIVITY_TYPES.map((tItem) => (
                                                <Button
                                                    key={tItem.type}
                                                    size="small"
                                                    variant="outlined"
                                                    startIcon={getActivityIcon(tItem.type)}
                                                    onClick={() => addActivity(step.id, tItem.type)}
                                                    sx={{ height: 28, textTransform: 'none', fontSize: '0.75rem' }}
                                                >
                                                    {tItem.label}
                                                </Button>
                                            ))}
                                        </Stack>

                                        <DndContext
                                            sensors={sensors}
                                            collisionDetection={closestCenter}
                                            onDragStart={handleActivityDragStart}
                                            onDragEnd={handleActivityDragEnd(step.id)}
                                        >
                                            <SortableContext items={step.activities.map((a) => a.id)} strategy={verticalListSortingStrategy}>
                                                <Stack spacing={1}>
                                                    {step.activities.map((a) => (
                                                        <SortableItem key={a.id} id={a.id}>
                                                            {(actHandle) => (
                                                                <Paper
                                                                    sx={{
                                                                        p: 1.5,
                                                                        bgcolor: 'grey.50',
                                                                        borderColor: a.dirty ? 'warning.main' : undefined,
                                                                        boxShadow: a.dirty ? '0 0 4px rgba(237, 108, 2, 0.1)' : undefined,
                                                                        transition: 'border-color 0.2s, box-shadow 0.2s'
                                                                    }}
                                                                    variant="outlined"
                                                                >
                                                                    <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1, flexWrap: 'wrap', gap: 1 }}>
                                                                        <Box {...actHandle} sx={{ cursor: 'grab', display: 'flex' }}>
                                                                            <DragIndicator />
                                                                        </Box>
                                                                        {getActivityIcon(a.type)}
                                                                        <Box sx={{ width: 180 }}>
                                                                            <GeneratableTextField
                                                                                label={t('common.code', 'Code')}
                                                                                value={a.code ?? ''}
                                                                                onChange={(val) => updateActivity(step.id, a.id, { code: val })}
                                                                                sequenceType="WfActivity"
                                                                            />
                                                                        </Box>
                                                                        {isRtl ? (
                                                                            <TextField
                                                                                size="small"
                                                                                label={t('common.name_ar', 'Name (AR)')}
                                                                                value={a.nameAR ?? ''}
                                                                                onChange={(e) => updateActivity(step.id, a.id, { nameAR: e.target.value, name: e.target.value })}
                                                                                sx={{ flexGrow: 1, minWidth: 150 }}
                                                                                dir="rtl"
                                                                            />
                                                                        ) : (
                                                                            <TextField
                                                                                size="small"
                                                                                label={t('workflow.activity_name', 'Activity Name')}
                                                                                value={a.name}
                                                                                onChange={(e) => updateActivity(step.id, a.id, { name: e.target.value, nameAR: e.target.value })}
                                                                                sx={{ flexGrow: 1, minWidth: 150 }}
                                                                            />
                                                                        )}
                                                                        
                                                                        <Chip label={a.type} size="small" variant="outlined" />
                                                                        <Chip label={`${a.controls.length} ${t('workflow.controls_short', 'controls')}`} size="small" />
                                                                        
                                                                        <Button
                                                                            size="small"
                                                                            variant="text"
                                                                            onClick={() => {
                                                                                setSelected({ kind: 'activity', stepId: step.id, id: a.id });
                                                                                setCenterTab(5);
                                                                            }}
                                                                        >
                                                                            {t('workflow.edit_form', 'Edit Form')}
                                                                        </Button>
                                                                        <Button
                                                                            size="small"
                                                                            variant="text"
                                                                            onClick={() => {
                                                                                setSelected({ kind: 'activity', stepId: step.id, id: a.id });
                                                                            }}
                                                                        >
                                                                            {t('workflow.configure', 'Configure')}
                                                                        </Button>
                                                                        <IconButton
                                                                            size="small"
                                                                            color="error"
                                                                            onClick={() => deleteActivity(step.id, a.id)}
                                                                        >
                                                                            <Delete fontSize="small" />
                                                                        </IconButton>
                                                                    </Stack>
                                                                    <Stack direction="row" spacing={1} sx={{ mt: 1, flexWrap: 'wrap', gap: 1 }} alignItems="center">
                                                                        <FormControl size="small" sx={{ minWidth: 150 }}>
                                                                            <InputLabel sx={{ fontSize: '0.75rem', mt: -0.5 }}>
                                                                                {t('workflow.activity_type_required', 'Activity Type *')}
                                                                            </InputLabel>
                                                                            <Select
                                                                                label={t('workflow.activity_type_required', 'Activity Type *')}
                                                                                value={a.activityTypeId ?? ''}
                                                                                onChange={(e) => updateActivity(step.id, a.id, { activityTypeId: e.target.value as number })}
                                                                                sx={{ height: 32, fontSize: '0.8rem' }}
                                                                            >
                                                                                <MenuItem value=""><em>{t('common.none', 'None')}</em></MenuItem>
                                                                                {activityTypes.map(tItem => <MenuItem key={tItem.id} value={tItem.id}>{tItem.name}</MenuItem>)}
                                                                            </Select>
                                                                        </FormControl>
                                                                        <FormControl size="small" sx={{ minWidth: 150 }}>
                                                                            <InputLabel sx={{ fontSize: '0.75rem', mt: -0.5 }}>
                                                                                {t('workflow.performer_required', 'Performer *')}
                                                                            </InputLabel>
                                                                            <Select
                                                                                label={t('workflow.performer_required', 'Performer *')}
                                                                                value={a.performerId ?? ''}
                                                                                onChange={(e) => updateActivity(step.id, a.id, { performerId: e.target.value as number })}
                                                                                sx={{ height: 32, fontSize: '0.8rem' }}
                                                                            >
                                                                                <MenuItem value=""><em>{t('common.none', 'None')}</em></MenuItem>
                                                                                {performers.map(p => <MenuItem key={p.id} value={p.id}>{p.name}</MenuItem>)}
                                                                            </Select>
                                                                        </FormControl>
                                                                        <FormControlLabel
                                                                            control={
                                                                                <Switch
                                                                                    size="small"
                                                                                    checked={!!a.mandatoryDocs}
                                                                                    onChange={(e) => updateActivity(step.id, a.id, { mandatoryDocs: e.target.checked })}
                                                                                    sx={{
                                                                                        '& .MuiSwitch-switchBase.Mui-checked': { color: '#6366f1' },
                                                                                        '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { bgcolor: '#6366f1' }
                                                                                    }}
                                                                                />
                                                                            }
                                                                            label={<Typography variant="caption">{t('workflow.mandatory_docs', 'Mandatory Docs')}</Typography>}
                                                                        />
                                                                        <FormControlLabel
                                                                            control={
                                                                                <Switch
                                                                                    size="small"
                                                                                    checked={a.isActive !== false}
                                                                                    onChange={(e) => updateActivity(step.id, a.id, { isActive: e.target.checked })}
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
                                                                                    checked={!!a.isRequired}
                                                                                    onChange={(e) => updateActivity(step.id, a.id, { isRequired: e.target.checked })}
                                                                                    sx={{
                                                                                        '& .MuiSwitch-switchBase.Mui-checked': { color: '#6366f1' },
                                                                                        '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { bgcolor: '#6366f1' }
                                                                                    }}
                                                                                />
                                                                            }
                                                                            label={<Typography variant="caption">{t('workflow.required', 'Required')}</Typography>}
                                                                        />
                                                                        {a.dirty && (
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
                                            <DragOverlay>
                                                {activeActivityId ? (
                                                    <Paper sx={{ p: 1.5, opacity: 0.8, bgcolor: 'primary.50', border: '2px dashed #6366f1' }} variant="outlined">
                                                        <Typography variant="body2">{activeActivity?.name ?? 'Dragging activity…'}</Typography>
                                                    </Paper>
                                                ) : null}
                                            </DragOverlay>
                                        </DndContext>
                                    </Paper>
                                )}
                            </SortableItem>
                        ))}
                    </Stack>
                </SortableContext>
                <DragOverlay>
                    {activeStepId ? (
                        <Paper sx={{ p: 3, opacity: 0.7, bgcolor: 'primary.50', border: '2px dashed #6366f1', boxShadow: '0 8px 24px rgba(99,102,241,0.15)' }}>
                            <Typography variant="h6" color="primary">{activeStep?.name ?? 'Dragging step…'}</Typography>
                        </Paper>
                    ) : null}
                </DragOverlay>
            </DndContext>
        </Box>
    );
});

DesignerTab.displayName = 'DesignerTab';
export default DesignerTab;
