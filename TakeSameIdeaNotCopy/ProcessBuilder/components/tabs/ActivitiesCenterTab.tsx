import React, { useMemo, useState, useEffect, useCallback } from 'react';
import {
    Box, Typography, Button, Paper, FormControl,
    InputLabel, Select, MenuItem, Chip, Stack
} from '@mui/material';
import { PlaylistAddCheck } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { notify } from '../../../../../../lib/notify';
import { DndContext, closestCenter, type DragEndEvent, useSensors, useSensor, PointerSensor } from '@dnd-kit/core';
import { SortableContext, verticalListSortingStrategy, arrayMove } from '@dnd-kit/sortable';
import { useProcessBuilderContext } from '../../context/ProcessBuilderContext';
import { ACTIVITY_TYPES, getActivityIcon } from '../../utils/palette';
import type { ActivityType } from '../../types';
import type { WfActivityType, WfPerformer } from '../../../../types';

interface ActivitiesCenterTabProps {
    activityTypes: WfActivityType[];
    performers: WfPerformer[];
    addActivity: (stepId: string, type: ActivityType) => void;
    saveActivityToBackend: (stepId: string, a: import('../../types').Activity) => Promise<void>;
}

import { ActivitySortableItem } from './ActivitySortableItem';

export const ActivitiesCenterTab: React.FC<ActivitiesCenterTabProps> = React.memo(({
    activityTypes,
    performers,
    addActivity,
    saveActivityToBackend,
}) => {
    const { t } = useTranslation();

    const {
        steps,
        setSteps,
        selectedNode: selected,
        setSelectedNode: setSelected,
        updateActivity,
        deleteActivity,
        setCenterTab,
    } = useProcessBuilderContext();

    const [saving, setSaving] = useState(false);
    const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));

    // ── Persistent step selection ──────────────────────────────────────────
    // Use a dedicated localStorage key so the tab always remembers the chosen
    // step, independent of the global selectedNode (which can point to anything).
    const STEP_STORAGE_KEY = 'activitiesTab.selectedStepId';

    const [selectedStepId, setSelectedStepId] = useState<string>(() => {
        try { return localStorage.getItem(STEP_STORAGE_KEY) ?? ''; } catch { return ''; }
    });

    const persistStepId = useCallback((id: string) => {
        setSelectedStepId(id);
        const step = steps.find(s => s.id === id);
        const stableRef = step ? (step.code || step.name || step.id) : id;
        try { localStorage.setItem(STEP_STORAGE_KEY, stableRef); } catch { /* quota */ }
    }, [steps]);

    // Auto-sync from global selectedNode when it points at a step or activity
    useEffect(() => {
        if (selected.kind === 'step') {
            persistStepId(selected.id);
        } else if (selected.kind === 'activity' && selected.stepId) {
            persistStepId(selected.stepId);
        } else if (selected.kind === 'control' && 'stepId' in selected) {
            persistStepId((selected as any).stepId);
        }
    }, [selected, persistStepId]);

    // Derived: the actual Step object from our persisted ID
    const targetStep = useMemo(() => {
        if (!selectedStepId) return undefined;
        let match = steps.find((s) => s.id === selectedStepId);
        if (!match) {
            match = steps.find((s) => s.code === selectedStepId || s.name === selectedStepId);
        }
        return match;
    }, [selectedStepId, steps]);

    const onActivityDragEnd = (e: DragEndEvent) => {
        if (!targetStep) return;
        const { active, over } = e;
        if (!over || active.id === over.id) return;
        setSteps((prev) => prev.map((s) => {
            if (s.id !== targetStep.id) return s;
            const oldIdx = s.activities.findIndex((a) => a.id === active.id);
            const newIdx = s.activities.findIndex((a) => a.id === over.id);
            return {
                ...s,
                activities: arrayMove(s.activities, oldIdx, newIdx).map((a) => ({ ...a, dirty: true }))
            };
        }));
    };

    const handleSave = async () => {
        if (!targetStep) return;
        const dirty = targetStep.activities.filter(a => a.dirty);
        if (dirty.length === 0) {
            notify.success(t('workflow.no_changes', 'No changes to save'));
            return;
        }

        // Prefill check and validations
        for (const a of dirty) {
            if (!(a.name || '').trim()) {
                notify.error(t('workflow.validation.empty_activity_name', `Activity name cannot be empty.`));
                return;
            }
            if (!a.activityTypeId) {
                notify.error(t('workflow.validation.missing_activity_type', `Please select an Activity Type for "${a.name}".`));
                return;
            }
            if (!a.performerId) {
                notify.error(t('workflow.validation.missing_performer', `Please select a Performer for "${a.name}".`));
                return;
            }
        }

        setSaving(true);
        try {
            for (const a of dirty) {
                await saveActivityToBackend(targetStep.id, a);
            }
        } finally {
            setSaving(false);
        }
    };

    return (
        <Box sx={{ p: 2, width: '100%', maxWidth: '100%', boxSizing: 'border-box', minWidth: 0 }}>
            <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2, flexWrap: 'wrap' }}>
                <Typography variant="h6" sx={{ flexGrow: 1 }}>
                    {t('workflow.activities', 'Activities')} {targetStep ? `· ${targetStep.name}` : ''}
                </Typography>
                {targetStep && targetStep.activities.some((a) => a.dirty) && (
                    <Chip size="small" color="warning" label={t('workflow.unsaved_changes', 'unsaved changes')} />
                )}
                <Button
                    variant="contained"
                    size="small"
                    startIcon={<PlaylistAddCheck />}
                    disabled={!targetStep || saving}
                    onClick={handleSave}
                >
                    {saving ? t('common.saving', 'Saving…') : t('workflow.save_activities', 'Save Activities')}
                </Button>
                <FormControl size="small" sx={{ minWidth: 200 }}>
                    <InputLabel>{t('workflow.step_label', 'Step')}</InputLabel>
                    <Select
                        label={t('workflow.step_label', 'Step')}
                        value={targetStep?.id ?? ''}
                        onChange={(e) => {
                            const stepId = String(e.target.value);
                            persistStepId(stepId);
                            setSelected({ kind: 'step', id: stepId });
                        }}
                    >
                        {steps.map((s) => (
                            <MenuItem key={s.id} value={s.id}>{s.name}</MenuItem>
                        ))}
                    </Select>
                </FormControl>
            </Stack>

            {!targetStep ? (
                <Paper variant="outlined" sx={{ p: 4, textAlign: 'center' }}>
                    <Typography color="text.secondary">
                        {t('workflow.select_step_manage_activities', 'Select a step to manage its activities.')}
                    </Typography>
                </Paper>
            ) : (
                <>
                    <Paper variant="outlined" sx={{ p: 1.5, mb: 2, bgcolor: 'primary.50' }}>
                        <Typography variant="caption" sx={{ fontWeight: 700, display: 'block', mb: 1 }}>
                            {t('workflow.add_activity_allcaps', 'ADD ACTIVITY')}
                        </Typography>
                        <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap', gap: 1 }}>
                            {ACTIVITY_TYPES.map((tItem) => (
                                <Button
                                    key={tItem.type}
                                    size="small"
                                    variant="outlined"
                                    startIcon={getActivityIcon(tItem.type)}
                                    onClick={() => addActivity(targetStep.id, tItem.type)}
                                >
                                    {tItem.label}
                                </Button>
                            ))}
                        </Stack>
                    </Paper>

                    {targetStep.activities.length === 0 ? (
                        <Paper variant="outlined" sx={{ p: 4, textAlign: 'center' }}>
                            <Typography color="text.secondary">
                                {t('workflow.no_activities_yet', 'No activities yet for this step.')}
                            </Typography>
                        </Paper>
                    ) : (
                        <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={onActivityDragEnd}>
                            <SortableContext items={targetStep.activities.map((a) => a.id)} strategy={verticalListSortingStrategy}>
                                <Stack spacing={1.5}>
                                    {targetStep.activities.map((a) => (
                                        <ActivitySortableItem
                                            key={a.id}
                                            activity={a}
                                            targetStepId={targetStep.id}
                                            activityTypes={activityTypes}
                                            performers={performers}
                                            updateActivity={updateActivity}
                                            deleteActivity={deleteActivity}
                                            setSelected={setSelected}
                                            setCenterTab={setCenterTab}
                                        />
                                    ))}
                                </Stack>
                            </SortableContext>
                        </DndContext>
                    )}
                </>
            )}
        </Box>
    );
});

ActivitiesCenterTab.displayName = 'ActivitiesCenterTab';
export default ActivitiesCenterTab;
