import React from 'react';
import {
    Box, Typography, Button, Paper, TextField, FormControl,
    InputLabel, Select, MenuItem, FormControlLabel, Switch, IconButton, Chip, Stack, Grid
} from '@mui/material';
import { Add, PlaylistAddCheck, Delete } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { notify } from '../../../../../../lib/notify';
import { useProcessBuilderContext } from '../../context/ProcessBuilderContext';
import type { WfOperator } from '../../../../types';

interface TransitionsTabProps {
    operators: WfOperator[];
    transitionsSaving: boolean;
    saveTransitionsToBackend: () => void;
}

export const TransitionsTab: React.FC<TransitionsTabProps> = React.memo(({
    operators,
    transitionsSaving,
    saveTransitionsToBackend,
}) => {
    const { i18n, t } = useTranslation();
    const isRtl = i18n.language === 'ar';

    const {
        processInfo,
        variables,
        steps,
        requestControls,
        transitions,
        addTransition,
        updateTransition,
        deleteTransition,
        setSelectedNode: setSelected,
    } = useProcessBuilderContext();

    const allActivities = React.useMemo(() => {
        return steps.flatMap(s =>
            s.activities.map(a => ({
                id: a.id,
                name: a.name,
                nameAR: a.nameAR,
                stepName: s.name,
                stepNameAR: s.nameAR,
            }))
        );
    }, [steps]);

    // Request/process transitions only. Activity-control transitions live in
    // their own flow (the Activity Control settings panel) and are intentionally
    // excluded here so the two flows stay independent.
    const processTransitions = React.useMemo(
        () => transitions.filter(tr => !tr.activityControlId),
        [transitions],
    );

    const handleSave = () => {
        for (const tr of processTransitions) {
            if (!tr.variableId) {
                notify.error(t('workflow.validation.empty_transition_variable', 'Transition variable cannot be empty.'));
                return;
            }
            if (tr.operatorId === '') {
                notify.error(t('workflow.validation.empty_transition_operator', 'Transition operator cannot be empty.'));
                return;
            }
            if (!tr.stepId) {
                notify.error(t('workflow.validation.empty_transition_step', 'Transition target step cannot be empty.'));
                return;
            }
        }
        saveTransitionsToBackend();
    };

    return (
        <Box sx={{ p: 2, width: '100%', maxWidth: '100%', boxSizing: 'border-box', minWidth: 0 }}>
            <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2, flexWrap: 'wrap' }}>
                <Typography variant="h6" sx={{ flexGrow: 1 }}>
                    {t('workflow.process_transitions', 'Process Transitions')}
                </Typography>
                {processTransitions.some((t) => t.dirty) && (
                    <Chip size="small" color="warning" label={t('common.unsaved', 'unsaved')} />
                )}
                <Button startIcon={<Add />} variant="outlined" size="small" onClick={addTransition}>
                    {t('workflow.add_transition', 'Add Transition')}
                </Button>
                <Button
                    variant="contained"
                    size="small"
                    startIcon={<PlaylistAddCheck />}
                    disabled={!processInfo.id || transitionsSaving}
                    onClick={handleSave}
                >
                    {transitionsSaving ? t('common.saving', 'Saving…') : t('workflow.save_transitions', 'Save Transitions')}
                </Button>
            </Stack>
            {!processInfo.id && (
                <Typography variant="caption" color="warning.main" sx={{ display: 'block', mb: 1 }}>
                    {t('workflow.save_process_first_transitions', 'Save the Process first to enable transitions (ProcessId required).')}
                </Typography>
            )}

            {processTransitions.length === 0 ? (
                <Paper variant="outlined" sx={{ p: 4, textAlign: 'center' }}>
                    <Typography color="text.secondary">
                        {t('workflow.no_transitions_yet', 'No transitions yet. Click "Add Transition" to create one.')}
                    </Typography>
                </Paper>
            ) : (
                <Stack spacing={2}>
                    {processTransitions.map((tr) => {
                        const triggerType = tr.requestControlId ? 'requestControl' : tr.activityId ? 'activity' : 'none';

                        const selectedControl = tr.requestControlId
                            ? requestControls.find(c => c.id === tr.requestControlId || (c.serverId && String(c.serverId) === String(tr.requestControlId)))
                            : undefined;

                        const isClosedControl = selectedControl && [
                            'dropdown',
                            'dropdownlist',
                            'combobox',
                            'dropdown-manual',
                            'checkbox',
                            'checkboxlist',
                            'radiobuttonlist'
                        ].includes(selectedControl.type);

                        const handleTriggerTypeChange = (val: string) => {
                            if (val === 'none') {
                                updateTransition(tr.id, { requestControlId: undefined, activityId: undefined });
                            } else if (val === 'requestControl') {
                                updateTransition(tr.id, { requestControlId: requestControls[0]?.id || '', activityId: undefined });
                            } else if (val === 'activity') {
                                updateTransition(tr.id, { requestControlId: undefined, activityId: allActivities[0]?.id || '' });
                            }
                        };

                        return (
                            <Paper
                                key={tr.id}
                                variant="outlined"
                                sx={{ p: 2, cursor: 'pointer', borderColor: tr.dirty ? 'warning.light' : 'divider' }}
                                onClick={() => setSelected({ kind: 'transition', id: tr.id })}
                                onFocusCapture={() => setSelected({ kind: 'transition', id: tr.id })}
                            >
                                <Grid container spacing={2} alignItems="center">
                                    {/* Variable Selection */}
                                    <Grid size={{ xs: 12, sm: 3 }}>
                                        <FormControl size="small" fullWidth onClick={(e) => e.stopPropagation()}>
                                            <InputLabel>{t('workflow.variable', 'Variable')}</InputLabel>
                                            <Select
                                                label={t('workflow.variable', 'Variable')}
                                                value={tr.variableId}
                                                onChange={(e) => updateTransition(tr.id, { variableId: e.target.value })}
                                            >
                                                <MenuItem value="">—</MenuItem>
                                                {variables.map((v) => (
                                                    <MenuItem key={v.id} value={v.id}>
                                                        {v.code ? `${v.code} - ` : ''}{isRtl ? v.nameAR || v.name : v.name}
                                                    </MenuItem>
                                                ))}
                                            </Select>
                                        </FormControl>
                                    </Grid>

                                    {/* Operator Selection */}
                                    <Grid size={{ xs: 12, sm: 2 }}>
                                        <FormControl size="small" fullWidth onClick={(e) => e.stopPropagation()}>
                                            <InputLabel>{t('workflow.operator', 'Operator')}</InputLabel>
                                            <Select
                                                label={t('workflow.operator', 'Operator')}
                                                value={tr.operatorId}
                                                onChange={(e) => updateTransition(tr.id, { operatorId: e.target.value as number })}
                                            >
                                                <MenuItem value="">—</MenuItem>
                                                {operators.map((op) => (
                                                    <MenuItem key={op.id} value={op.id}>
                                                        {isRtl ? op.nameAR || op.name : op.name}
                                                    </MenuItem>
                                                ))}
                                            </Select>
                                        </FormControl>
                                    </Grid>

                                    {/* Comparison Value */}
                                    <Grid size={{ xs: 12, sm: 2 }}>
                                        {isClosedControl ? (
                                            <FormControl size="small" fullWidth onClick={(e) => e.stopPropagation()}>
                                                <InputLabel>{t('common.value', 'Value')}</InputLabel>
                                                <Select
                                                    label={t('common.value', 'Value')}
                                                    value={tr.value}
                                                    onChange={(e) => updateTransition(tr.id, { value: e.target.value })}
                                                >
                                                    <MenuItem value="">—</MenuItem>
                                                    {selectedControl.type === 'checkbox' ? [
                                                        <MenuItem key="true" value="true">{t('common.yes', 'Yes')}</MenuItem>,
                                                        <MenuItem key="false" value="false">{t('common.no', 'No')}</MenuItem>
                                                    ] : (
                                                        (selectedControl.optionsX || []).map((opt, idx) => (
                                                            <MenuItem key={idx} value={opt.value}>
                                                                {isRtl ? opt.ar || opt.en || opt.value : opt.en || opt.ar || opt.value}
                                                            </MenuItem>
                                                        ))
                                                    )}
                                                </Select>
                                            </FormControl>
                                        ) : (
                                            <TextField
                                                size="small"
                                                fullWidth
                                                label={t('common.value', 'Value')}
                                                value={tr.value}
                                                onClick={(e) => e.stopPropagation()}
                                                onChange={(e) => updateTransition(tr.id, { value: e.target.value })}
                                            />
                                        )}
                                    </Grid>

                                    {/* Target Step */}
                                    <Grid size={{ xs: 12, sm: 3 }}>
                                        <FormControl size="small" fullWidth onClick={(e) => e.stopPropagation()}>
                                            <InputLabel>{t('workflow.target_step', 'Target Step')}</InputLabel>
                                            <Select
                                                label={t('workflow.target_step', 'Target Step')}
                                                value={tr.stepId}
                                                onChange={(e) => updateTransition(tr.id, { stepId: e.target.value })}
                                            >
                                                <MenuItem value="">—</MenuItem>
                                                {steps.map((s) => (
                                                    <MenuItem key={s.id} value={s.id}>
                                                        {isRtl ? s.nameAR || s.name : s.name}
                                                    </MenuItem>
                                                ))}
                                            </Select>
                                        </FormControl>
                                    </Grid>

                                    {/* Active Switch and Delete */}
                                    <Grid size={{ xs: 12, sm: 2 }}>
                                        <Stack direction="row" alignItems="center" justifyContent="flex-end" spacing={1}>
                                            <FormControlLabel
                                                onClick={(e) => e.stopPropagation()}
                                                control={
                                                    <Switch
                                                        size="small"
                                                        checked={tr.isActive}
                                                        onChange={(e) => updateTransition(tr.id, { isActive: e.target.checked })}
                                                    />
                                                }
                                                label={t('common.active', 'Active')}
                                            />
                                            <IconButton
                                                size="small"
                                                color="error"
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    deleteTransition(tr.id);
                                                }}
                                            >
                                                <Delete fontSize="small" />
                                            </IconButton>
                                        </Stack>
                                    </Grid>

                                    {/* Trigger Scoping Row */}
                                    <Grid size={{ xs: 12 }}>
                                        <Grid container spacing={2} alignItems="center" onClick={(e) => e.stopPropagation()}>
                                            <Grid size={{ xs: 12, sm: 3 }}>
                                                <FormControl size="small" fullWidth>
                                                    <InputLabel>{t('workflow.trigger_source', 'Trigger Source')}</InputLabel>
                                                    <Select
                                                        label={t('workflow.trigger_source', 'Trigger Source')}
                                                        value={triggerType}
                                                        onChange={(e) => handleTriggerTypeChange(e.target.value)}
                                                    >
                                                        <MenuItem value="none">{t('common.none', 'None (Always check variable)')}</MenuItem>
                                                        <MenuItem value="requestControl">{t('workflow.request_control', 'Request Control')}</MenuItem>
                                                        <MenuItem value="activity">{t('workflow.activity', 'Activity')}</MenuItem>
                                                    </Select>
                                                </FormControl>
                                            </Grid>

                                            {triggerType === 'requestControl' && (
                                                <Grid size={{ xs: 12, sm: 6 }}>
                                                    <FormControl size="small" fullWidth>
                                                        <InputLabel>{t('workflow.request_control', 'Request Control')}</InputLabel>
                                                        <Select
                                                            label={t('workflow.request_control', 'Request Control')}
                                                            value={tr.requestControlId || ''}
                                                            onChange={(e) => updateTransition(tr.id, { requestControlId: e.target.value })}
                                                        >
                                                            <MenuItem value="">—</MenuItem>
                                                            {requestControls.map((c) => (
                                                                <MenuItem key={c.id} value={c.id}>
                                                                    {c.code ? `${c.code} - ` : ''}{isRtl ? c.labelAR || c.label : c.label}
                                                                </MenuItem>
                                                            ))}
                                                        </Select>
                                                    </FormControl>
                                                </Grid>
                                            )}

                                            {triggerType === 'activity' && (
                                                <Grid size={{ xs: 12, sm: 6 }}>
                                                    <FormControl size="small" fullWidth>
                                                        <InputLabel>{t('workflow.activity', 'Activity')}</InputLabel>
                                                        <Select
                                                            label={t('workflow.activity', 'Activity')}
                                                            value={tr.activityId || ''}
                                                            onChange={(e) => updateTransition(tr.id, { activityId: e.target.value })}
                                                        >
                                                            <MenuItem value="">—</MenuItem>
                                                            {allActivities.map((a) => (
                                                                <MenuItem key={a.id} value={a.id}>
                                                                    [{isRtl ? a.stepNameAR || a.stepName : a.stepName}] {isRtl ? a.nameAR || a.name : a.name}
                                                                </MenuItem>
                                                            ))}
                                                        </Select>
                                                    </FormControl>
                                                </Grid>
                                            )}
                                        </Grid>
                                    </Grid>
                                </Grid>
                            </Paper>
                        );
                    })}
                </Stack>
            )}
        </Box>
    );
});

TransitionsTab.displayName = 'TransitionsTab';
export default TransitionsTab;
