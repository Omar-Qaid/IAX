import React, { useMemo } from 'react';
import {
    Box, Typography, Button, Paper, TextField, FormControlLabel, Switch, IconButton, Chip, Stack
} from '@mui/material';
import { Visibility, DragIndicator, Delete, Save } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { DndContext, closestCenter, useSensors, useSensor, PointerSensor, type DragEndEvent } from '@dnd-kit/core';
import { SortableContext, verticalListSortingStrategy, arrayMove } from '@dnd-kit/sortable';
import { useProcessBuilderContext } from '../../context/ProcessBuilderContext';
import { SortableItem } from '../shared/SortableItem';
import { CONTROL_PALETTE, getControlIcon } from '../../utils/palette';
import { ControlPreview } from '../shared/ControlPreview';
import type { ControlType } from '../../types';

interface ActivityFormTabProps {
    addControl: (stepId: string, activityId: string, type: ControlType) => void;
    saveActivityToBackend: (stepId: string, activity: import('../../types').Activity) => Promise<void>;
}

export const ActivityFormTab: React.FC<ActivityFormTabProps> = React.memo(({
    addControl, saveActivityToBackend
}) => {
    const { i18n, t } = useTranslation();
    const isRtl = i18n.language === 'ar';

    const {
        steps,
        selectedNode: selected,
        setSelectedNode: setSelected,
        updateControl,
        deleteControl,
        setSteps,
        transitions,
    } = useProcessBuilderContext();

    const onControlDragEnd = (targetStepId: string, activityId: string) => (e: DragEndEvent) => {
        const { active, over } = e;
        if (!over || active.id === over.id) return;
        setSteps((prev) => prev.map((s) => {
            if (s.id !== targetStepId) return s;
            return {
                ...s,
                activities: s.activities.map((a) => {
                    if (a.id !== activityId) return a;
                    const oldIdx = a.controls.findIndex((c) => c.id === active.id);
                    const newIdx = a.controls.findIndex((c) => c.id === over.id);
                    return {
                        ...a,
                        controls: arrayMove(a.controls, oldIdx, newIdx).map((c, i) => ({
                            ...c,
                            sortOrder: (i + 1) * 10,
                            dirty: true
                        })),
                        dirty: true
                    };
                })
            };
        }));
    };

    const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));

    const selectedActivity = useMemo(() => {
        if (selected.kind === 'activity') {
            return steps.find((s) => s.id === selected.stepId)?.activities.find((a) => a.id === selected.id) ?? null;
        }
        if (selected.kind === 'control' && 'activityId' in selected) {
            return steps.find((s) => s.id === selected.stepId)?.activities.find((a) => a.id === selected.activityId) ?? null;
        }
        return null;
    }, [selected, steps]);

    const activity = useMemo(() => {
        return selectedActivity ?? steps.flatMap((s) => s.activities)[0];
    }, [selectedActivity, steps]);

    if (!activity) {
        return (
            <Box sx={{ p: 4, textAlign: 'center', color: 'text.secondary' }}>
                <Visibility sx={{ fontSize: 48, opacity: 0.3 }} />
                <Typography sx={{ mb: 2 }}>
                    {t('workflow.select_activity_design', 'Select an activity to design its form')}
                </Typography>
                <Stack spacing={1} alignItems="center">
                    {steps.flatMap((s) =>
                        s.activities.map((a) => (
                            <Button
                                key={a.id}
                                size="small"
                                variant="outlined"
                                onClick={() => setSelected({ kind: 'activity', stepId: s.id, id: a.id })}
                            >
                                {s.name} → {a.name}
                            </Button>
                        ))
                    )}
                    {steps.every((s) => s.activities.length === 0) && (
                        <Typography variant="caption">
                            {t('workflow.add_activity_first_designer', 'Add an activity first in the Designer tab.')}
                        </Typography>
                    )}
                </Stack>
            </Box>
        );
    }

    const stepId = steps.find((s) => s.activities.some((a) => a.id === activity.id))!.id;

    return (
        <Box sx={{ p: 2 }}>
            <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 2 }}>
                <Typography variant="h6">{t('workflow.activity_form', 'Activity Form')} · {activity.name}</Typography>
                {activity.dirty && (
                    <Button
                        variant="contained"
                        color="primary"
                        startIcon={<Save />}
                        onClick={() => saveActivityToBackend(stepId, activity)}
                    >
                        {t('workflow.save_activity_controls', 'Save Activity Controls')}
                    </Button>
                )}
            </Stack>

            <Paper variant="outlined" sx={{ p: 1.5, mb: 2, bgcolor: 'primary.50' }}>
                <Typography variant="caption" sx={{ fontWeight: 700, display: 'block', mb: 1 }}>
                    {t('workflow.add_control_allcaps', 'ADD CONTROL')}
                </Typography>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                    {(['text', 'longtext', 'date', 'dropdown-manual',
                        'checkbox', 'table', 'file', 'employeesearch'] as ControlType[])
                        .map((tItem) => CONTROL_PALETTE.find((p) => p.type === tItem))
                        .filter((p): p is typeof CONTROL_PALETTE[number] => !!p)
                        .map((p) => (
                            <Button
                                key={p.type}
                                size="small"
                                variant="outlined"
                                startIcon={getControlIcon(p.type)}
                                onClick={() => addControl(stepId, activity.id, p.type)}
                                sx={{ textTransform: 'none', whiteSpace: 'nowrap' }}
                            >
                                {p.label}
                            </Button>
                        ))}
                </Box>
            </Paper>

            <DndContext
                sensors={sensors}
                collisionDetection={closestCenter}
                onDragEnd={(e) => onControlDragEnd(stepId, activity.id)(e)}
            >
                <SortableContext items={activity.controls.map((c) => c.id)} strategy={verticalListSortingStrategy}>
                    <Stack spacing={2}>
                        {activity.controls.length === 0 && (
                            <Typography color="text.secondary" sx={{ p: 2, textAlign: 'center' }}>
                                {t('workflow.no_controls_yet_left_panel', 'No controls yet. Add some from the left panel.')}
                            </Typography>
                        )}
                        {activity.controls.map((c) => (
                            <SortableItem key={c.id} id={c.id}>
                                {(handle) => (
                                    <Paper
                                        variant="outlined"
                                        onClick={() => setSelected({ kind: 'control', stepId, activityId: activity.id, id: c.id })}
                                        onFocusCapture={() => setSelected({ kind: 'control', stepId, activityId: activity.id, id: c.id })}
                                        sx={{
                                            p: 1.5,
                                            borderColor: c.dirty ? 'warning.main' : undefined,
                                            boxShadow: c.dirty ? '0 0 4px rgba(237, 108, 2, 0.1)' : undefined,
                                            transition: 'border-color 0.2s, box-shadow 0.2s',
                                            cursor: 'pointer',
                                        }}
                                    >
                                        <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1, flexWrap: 'wrap', gap: 1 }}>
                                            <Box {...handle} sx={{ cursor: 'grab', display: 'flex' }}>
                                                <DragIndicator />
                                            </Box>

                                            <TextField
                                                size="small"
                                                label={t('common.code', 'Code')}
                                                value={c.code ?? ''}
                                                onChange={(e) => updateControl(stepId, activity.id, c.id, { code: e.target.value })}
                                                sx={{ width: 140 }}
                                            />

                                            {isRtl ? (
                                                <TextField
                                                    size="small"
                                                    label={t('workflow.label_ar', 'Label (AR)')}
                                                    value={c.labelAR ?? ''}
                                                    onChange={(e) => updateControl(stepId, activity.id, c.id, { labelAR: e.target.value, label: e.target.value })}
                                                    sx={{ flexGrow: 1, minWidth: 150 }}
                                                    dir="rtl"
                                                />
                                            ) : (
                                                <TextField
                                                    size="small"
                                                    label={t('workflow.label', 'Label')}
                                                    value={c.label}
                                                    onChange={(e) => updateControl(stepId, activity.id, c.id, { label: e.target.value, labelAR: e.target.value })}
                                                    sx={{ flexGrow: 1, minWidth: 150 }}
                                                />
                                            )}

                                            <Box sx={{ flexGrow: 1, ml: 2, pointerEvents: 'none', opacity: 0.8, maxWidth: 300 }}>
                                                <ControlPreview control={c} />
                                            </Box>

                                            <Button
                                                size="small"
                                                variant="text"
                                                onClick={() => setSelected({ kind: 'control', stepId, activityId: activity.id, id: c.id })}
                                            >
                                                {t('workflow.configure', 'Configure')}
                                            </Button>
                                            <IconButton
                                                size="small"
                                                color="error"
                                                onClick={() => deleteControl(stepId, activity.id, c.id)}
                                            >
                                                <Delete fontSize="small" />
                                            </IconButton>
                                        </Stack>

                                        <Stack direction="row" spacing={1} sx={{ mt: 1, flexWrap: 'wrap', gap: 1 }} alignItems="center">
                                            <FormControlLabel
                                                control={
                                                    <Switch
                                                        size="small"
                                                        checked={!!c.visible}
                                                        onChange={(e) => updateControl(stepId, activity.id, c.id, { visible: e.target.checked })}
                                                        sx={{
                                                            '& .MuiSwitch-switchBase.Mui-checked': { color: '#6366f1' },
                                                            '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { bgcolor: '#6366f1' }
                                                        }}
                                                    />
                                                }
                                                label={<Typography variant="caption">{t('workflow.visible', 'Visible')}</Typography>}
                                            />
                                            {c.required && (
                                                <Chip label={t('workflow.required', 'Required')} size="small" variant="outlined" />
                                            )}
                                            {c.visible && (
                                                <Chip label={t('workflow.visible', 'Visible')} size="small" variant="outlined" />
                                            )}
                                            {c.readOnly && (
                                                <Chip label={t('workflow.read_only', 'Read Only')} size="small" variant="outlined" />
                                            )}
                                            <Chip label={c.type} size="small" variant="outlined" />

                                            {c.dirty && (
                                                <Chip
                                                    label={t('common.unsaved', 'unsaved')}
                                                    size="small"
                                                    color="warning"
                                                    variant="outlined"
                                                    sx={{ height: 18, fontSize: 10, ml: 'auto' }}
                                                />
                                            )}
                                            {transitions.filter(tr => tr.activityControlId === c.id).map(tr => {
                                                const targetStep = steps.find(s => s.id === tr.stepId);
                                                const stepName = targetStep ? (isRtl ? targetStep.nameAR || targetStep.name : targetStep.name) : '';
                                                return stepName ? (
                                                    <Chip
                                                        key={tr.id}
                                                        label={`\u2192 ${stepName}`}
                                                        size="small"
                                                        color="info"
                                                        variant="outlined"
                                                    />
                                                ) : null;
                                            })}
                                        </Stack>
                                    </Paper>
                                )}
                            </SortableItem>
                        ))}
                    </Stack>
                </SortableContext>
            </DndContext>

            {activity.actions.length > 0 && (
                <Box sx={{ mt: 3, pt: 2, borderTop: '1px solid', borderColor: 'divider' }}>
                    <Typography variant="caption" sx={{ fontWeight: 700 }}>{t('workflow.actions_allcaps', 'ACTIONS')}</Typography>
                    <Stack direction="row" spacing={1} sx={{ mt: 1 }}>
                        {activity.actions.map((aAction) => (
                            <Button
                                key={aAction.id}
                                variant="contained"
                                size="small"
                                color={aAction.type === 'approve' ? 'success' : aAction.type === 'reject' ? 'error' : 'warning'}
                            >
                                {aAction.label || aAction.type}
                            </Button>
                        ))}
                    </Stack>
                </Box>
            )}
        </Box>
    );
});

ActivityFormTab.displayName = 'ActivityFormTab';
export default ActivityFormTab;
