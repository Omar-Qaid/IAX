import React, { useMemo } from 'react';
import {
    Box, Typography, Button, Card, CardContent, Divider, Chip, Stack
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useProcessBuilderContext } from '../../context/ProcessBuilderContext';
import { ACTIVITY_TYPES, CONTROL_PALETTE, getActivityIcon, getControlIcon } from '../../utils/palette';
import type { ActivityType, ControlType } from '../../types';

interface ActivitiesPaletteProps {
    addActivity: (stepId: string, type: ActivityType) => void;
    addControl: (stepId: string, activityId: string, type: ControlType) => void;
    addRequestControl: (type: ControlType) => void;
    setCenterTab: (tab: number) => void;
}

export const ActivitiesPalette: React.FC<ActivitiesPaletteProps> = React.memo(({
    addActivity,
    addControl,
    addRequestControl,
    setCenterTab,
}) => {
    const { t } = useTranslation();
    const {
        steps,
        selectedNode: selected,
    } = useProcessBuilderContext();

    const selectedStep = useMemo(
        () => selected.kind === 'step' ? steps.find((s) => s.id === selected.id) : null,
        [selected, steps]
    );

    const selectedActivity = useMemo(() => {
        if (selected.kind !== 'activity') return null;
        return steps.find((s) => s.id === selected.stepId)?.activities.find((a) => a.id === selected.id) ?? null;
    }, [selected, steps]);

    return (
        <Box sx={{ p: 1 }}>
            <Typography variant="caption" sx={{ fontWeight: 700, color: 'text.secondary' }}>
                {t('workflow.activity_types_add', 'ACTIVITY TYPES (click a step, then add)')}
            </Typography>
            <Stack spacing={0.5} sx={{ mt: 1 }}>
                {ACTIVITY_TYPES.map((tItem) => (
                    <Card key={tItem.type} variant="outlined" sx={{ borderRadius: 1 }}>
                        <CardContent sx={{ p: 1.25, '&:last-child': { pb: 1.25 } }}>
                            <Stack direction="row" alignItems="center" spacing={1}>
                                {getActivityIcon(tItem.type)}
                                <Typography variant="body2" sx={{ flexGrow: 1 }}>{tItem.label}</Typography>
                                <Button
                                    size="small"
                                    disabled={!selectedStep}
                                    onClick={() => selectedStep && addActivity(selectedStep.id, tItem.type)}
                                >
                                    {t('common.add', 'Add')}
                                </Button>
                            </Stack>
                        </CardContent>
                    </Card>
                ))}
            </Stack>

            <Divider sx={{ my: 2 }} />
            <Stack direction="row" alignItems="center" sx={{ mb: 1 }}>
                <Typography variant="caption" sx={{ fontWeight: 700, color: 'text.secondary', flexGrow: 1 }}>
                    {t('workflow.controls_allcaps', 'CONTROLS')}
                </Typography>
                <Chip
                    size="small"
                    variant="outlined"
                    color={selectedActivity ? 'info' : 'default'}
                    label={selectedActivity
                        ? `→ ${selectedActivity.name}`
                        : t('workflow.request_form_process_level', 'Request Form (process-level)')}
                />
            </Stack>
            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1 }}>
                {selectedActivity
                    ? t('workflow.adds_to_selected_activity', 'Adds to the selected activity.')
                    : t('workflow.adds_as_process_level', 'Adds as a process-level Request Control. Select an activity to add there instead.')}
            </Typography>
            <Stack spacing={0.5} sx={{ mt: 1 }}>
                {CONTROL_PALETTE.map((c) => (
                    <Card key={c.type} variant="outlined">
                        <CardContent sx={{ p: 1.25, '&:last-child': { pb: 1.25 } }}>
                            <Stack direction="row" alignItems="center" spacing={1}>
                                {getControlIcon(c.type)}
                                <Typography variant="body2" sx={{ flexGrow: 1 }}>{c.label}</Typography>
                                <Button
                                    size="small"
                                    onClick={() => {
                                        if (selectedActivity && selected.kind === 'activity') {
                                            addControl(selected.stepId, selected.id, c.type);
                                        } else {
                                            addRequestControl(c.type);
                                            setCenterTab(4);
                                        }
                                    }}
                                >
                                    {t('common.add', 'Add')}
                                </Button>
                            </Stack>
                        </CardContent>
                    </Card>
                ))}
            </Stack>
        </Box>
    );
});

ActivitiesPalette.displayName = 'ActivitiesPalette';
export default ActivitiesPalette;
