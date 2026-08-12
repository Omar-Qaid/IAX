import React, { memo } from 'react';
import {
    Box, Typography, Button, Paper, TextField, FormControl,
    InputLabel, Select, MenuItem, FormControlLabel, Switch, IconButton, Chip, Stack
} from '@mui/material';
import { DragIndicator, Delete } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { SortableItem } from '../shared/SortableItem';
import { getActivityIcon } from '../../utils/palette';
import type { WfActivityType, WfPerformer } from '../../../../types';
import type { Activity } from '../../types';

interface ActivitySortableItemProps {
    activity: Activity;
    targetStepId: string;
    activityTypes: WfActivityType[];
    performers: WfPerformer[];
    updateActivity: (stepId: string, activityId: string, updates: Partial<Activity>) => void;
    deleteActivity: (stepId: string, activityId: string) => void;
    setSelected: (node: { kind: 'activity'; stepId: string; id: string }) => void;
    setCenterTab: (tabIndex: number) => void;
}

export const ActivitySortableItem: React.FC<ActivitySortableItemProps> = memo(({
    activity: a,
    targetStepId,
    activityTypes,
    performers,
    updateActivity,
    deleteActivity,
    setSelected,
    setCenterTab
}) => {
    const { i18n, t } = useTranslation();
    const isRtl = i18n.language === 'ar';

    return (
        <SortableItem id={a.id}>
            {(handle) => (
                <Paper
                    variant="outlined"
                    onClick={() => setSelected({ kind: 'activity', stepId: targetStepId, id: a.id })}
                    onFocusCapture={() => setSelected({ kind: 'activity', stepId: targetStepId, id: a.id })}
                    sx={{
                        p: 1.5,
                        borderColor: a.dirty ? 'warning.main' : undefined,
                        boxShadow: a.dirty ? '0 0 4px rgba(237, 108, 2, 0.1)' : undefined,
                        transition: 'border-color 0.2s, box-shadow 0.2s',
                        cursor: 'pointer',
                    }}
                >
                    <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1, flexWrap: 'wrap', gap: 1 }}>
                        <Box {...handle} sx={{ cursor: 'grab', display: 'flex' }}>
                            <DragIndicator />
                        </Box>
                        {getActivityIcon(a.type)}

                        <TextField
                            size="small"
                            label={t('common.code', 'Code')}
                            value={a.code ?? ''}
                            onChange={(e) => updateActivity(targetStepId, a.id, { code: e.target.value })}
                            sx={{ width: 110 }}
                        />
                        {isRtl ? (
                            <TextField
                                size="small"
                                label={t('common.name_ar', 'Name (AR)')}
                                value={a.nameAR ?? ''}
                                onChange={(e) => updateActivity(targetStepId, a.id, { nameAR: e.target.value, name: e.target.value })}
                                sx={{ flexGrow: 1, minWidth: 150 }}
                                dir="rtl"
                            />
                        ) : (
                            <TextField
                                size="small"
                                label={t('workflow.activity_name', 'Activity Name')}
                                value={a.name}
                                onChange={(e) => updateActivity(targetStepId, a.id, { name: e.target.value, nameAR: e.target.value })}
                                sx={{ flexGrow: 1, minWidth: 150 }}
                            />
                        )}

                        <Chip label={a.type} size="small" variant="outlined" />
                        <Chip label={`${a.controls.length} ${t('workflow.controls_short', 'controls')}`} size="small" />

                        <Button
                            size="small"
                            variant="text"
                            onClick={() => {
                                setSelected({ kind: 'activity', stepId: targetStepId, id: a.id });
                                setCenterTab(5);
                            }}
                        >
                            {t('workflow.edit_form', 'Edit Form')}
                        </Button>
                        <Button
                            size="small"
                            variant="text"
                            onClick={() => {
                                setSelected({ kind: 'activity', stepId: targetStepId, id: a.id });
                            }}
                        >
                            {t('workflow.configure', 'Configure')}
                        </Button>
                        <IconButton
                            size="small"
                            color="error"
                            onClick={() => deleteActivity(targetStepId, a.id)}
                        >
                            <Delete fontSize="small" />
                        </IconButton>
                    </Stack>
                    <Stack direction="row" spacing={1} sx={{ mt: 1, flexWrap: 'wrap', gap: 1 }} alignItems="center">
                        <FormControl size="small" sx={{ minWidth: 150 }} error={!a.activityTypeId}>
                            <InputLabel sx={{ fontSize: '0.75rem', mt: -0.5 }}>
                                {t('workflow.activity_type_required', 'Activity Type *')}
                            </InputLabel>
                            <Select
                                label={t('workflow.activity_type_required', 'Activity Type *')}
                                value={a.activityTypeId ?? ''}
                                onChange={(e) => updateActivity(targetStepId, a.id, { activityTypeId: e.target.value as number })}
                                sx={{ height: 32, fontSize: '0.8rem' }}
                            >
                                <MenuItem value=""><em>{t('common.none', 'None')}</em></MenuItem>
                                {activityTypes.map(tItem => <MenuItem key={tItem.id} value={tItem.id}>{tItem.name}</MenuItem>)}
                            </Select>
                        </FormControl>
                        <FormControl size="small" sx={{ minWidth: 150 }} error={!a.performerId}>
                            <InputLabel sx={{ fontSize: '0.75rem', mt: -0.5 }}>
                                {t('workflow.performer_required', 'Performer *')}
                            </InputLabel>
                            <Select
                                label={t('workflow.performer_required', 'Performer *')}
                                value={a.performerId ?? ''}
                                onChange={(e) => updateActivity(targetStepId, a.id, { performerId: e.target.value as number })}
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
                                    onChange={(e) => updateActivity(targetStepId, a.id, { mandatoryDocs: e.target.checked })}
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
                                    onChange={(e) => updateActivity(targetStepId, a.id, { isActive: e.target.checked })}
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
                                    onChange={(e) => updateActivity(targetStepId, a.id, { isRequired: e.target.checked })}
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
    );
});

ActivitySortableItem.displayName = 'ActivitySortableItem';
