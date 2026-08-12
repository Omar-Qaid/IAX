import React from 'react';
import {
    Box, Typography, Button, Paper, TextField, FormControlLabel, Switch, IconButton, Chip, Stack
} from '@mui/material';
import { AccountTree, DragIndicator, Delete } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { GeneratableTextField } from '../../../../../../components/common/GeneratableTextField';
import { notify } from '../../../../../../lib/notify';
import { DndContext, closestCenter, type DragEndEvent, useSensors, useSensor, PointerSensor } from '@dnd-kit/core';
import { SortableContext, verticalListSortingStrategy, arrayMove } from '@dnd-kit/sortable';
import { useProcessBuilderContext } from '../../context/ProcessBuilderContext';
import { SortableItem } from '../shared/SortableItem';
import { CONTROL_PALETTE, getControlIcon } from '../../utils/palette';
import { ControlPreview } from '../shared/ControlPreview';
import type { ControlType } from '../../types';

interface RequestFormTabProps {
    requestControlsSaving: boolean;
    saveRequestControlsToBackend: () => void;
    addRequestControl: (type: ControlType) => void;
}

export const RequestFormTab: React.FC<RequestFormTabProps> = React.memo(({
    requestControlsSaving,
    saveRequestControlsToBackend,
    addRequestControl,
}) => {
    const { i18n, t } = useTranslation();
    const isRtl = i18n.language === 'ar';

    const {
        processInfo,
        requestControls,
        setRequestControls,
        updateRequestControl,
        deleteRequestControl,
        setSelectedNode: setSelected,
        transitions,
        steps,
    } = useProcessBuilderContext();

    const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));

    const onRequestControlDragEnd = (e: DragEndEvent) => {
        const { active, over } = e;
        if (!over || active.id === over.id) return;
        setRequestControls((prev) => {
            const o = prev.findIndex((c) => c.id === active.id);
            const n = prev.findIndex((c) => c.id === over.id);
            return arrayMove(prev, o, n).map((c, i) => ({
                ...c,
                sortOrder: (i + 1) * 10,
                dirty: true,
            }));
        });
    };

    const handleSave = () => {
        for (const c of requestControls) {
            if (!(c.code || '').trim()) {
                notify.error(t('workflow.validation.empty_control_code', 'Control Code cannot be empty.'));
                return;
            }
        }
        saveRequestControlsToBackend();
    };

    return (
        <Box sx={{ p: 2, width: '100%', maxWidth: '100%', boxSizing: 'border-box', minWidth: 0 }}>
            <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2, flexWrap: 'wrap' }}>
                <Typography variant="h6" sx={{ flexGrow: 1 }}>
                    {t('workflow.request_form_title', 'Request Form (Process-level controls)')}
                </Typography>
                {requestControls.some((c) => c.dirty) && (
                    <Chip size="small" color="warning" label={t('common.unsaved', 'unsaved')} />
                )}
                <Button
                    variant="contained"
                    size="small"
                    startIcon={<AccountTree />}
                    disabled={!processInfo.id || requestControlsSaving}
                    onClick={handleSave}
                >
                    {requestControlsSaving ? t('common.saving', 'Saving…') : t('workflow.save_request_controls', 'Save Request Controls')}
                </Button>
            </Stack>
            {!processInfo.id && (
                <Typography variant="caption" color="warning.main" sx={{ display: 'block', mb: 1 }}>
                    {t('workflow.save_process_first_request_controls', 'Save the Process first to enable request controls (ProcessId required).')}
                </Typography>
            )}

            <Paper variant="outlined" sx={{ p: 1.5, mb: 2, bgcolor: 'primary.50' }}>
                <Stack direction="row" alignItems="center" sx={{ mb: 1 }}>
                    <Typography variant="caption" sx={{ fontWeight: 700, flexGrow: 1 }}>
                        {t('workflow.add_control_allcaps', 'ADD CONTROL')}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                        {t('workflow.add_control_subtitle', 'Most used · full palette in left sidebar')}
                    </Typography>
                </Stack>
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
                                onClick={() => addRequestControl(p.type)}
                                sx={{ textTransform: 'none', whiteSpace: 'nowrap' }}
                            >
                                {p.label}
                            </Button>
                        ))}
                </Box>
            </Paper>

            <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={onRequestControlDragEnd}>
                <SortableContext items={requestControls.map((c) => c.id)} strategy={verticalListSortingStrategy}>
                    <Stack spacing={2}>
                        {requestControls.length === 0 && (
                            <Typography color="text.secondary" sx={{ p: 2, textAlign: 'center' }}>
                                {t('workflow.no_request_controls_yet', 'No request controls yet. Add some above.')}
                            </Typography>
                        )}
                        {requestControls.map((c) => (
                            <SortableItem key={c.id} id={c.id}>
                                {(handle) => (
                                    <Paper
                                        variant="outlined"
                                        onClick={() => setSelected({ kind: 'requestControl', id: c.id })}
                                        onFocusCapture={() => setSelected({ kind: 'requestControl', id: c.id })}
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


                                            <Box sx={{ width: 180 }}>
                                                <GeneratableTextField
                                                    label={t('common.code', 'Code')}
                                                    value={c.code ?? ''}
                                                    onChange={(val) => updateRequestControl(c.id, { code: val })}
                                                    sequenceType="WfRequestControl"
                                                />
                                            </Box>

                                            {isRtl ? (
                                                <TextField
                                                    size="small"
                                                    label={t('workflow.label_ar', 'Label (AR)')}
                                                    value={c.labelAR ?? ''}
                                                    onChange={(e) => updateRequestControl(c.id, { labelAR: e.target.value, label: e.target.value })}
                                                    sx={{ flexGrow: 1, minWidth: 150 }}
                                                    dir="rtl"
                                                />
                                            ) : (
                                                <TextField
                                                    size="small"
                                                    label={t('workflow.label', 'Label')}
                                                    value={c.label}
                                                    onChange={(e) => updateRequestControl(c.id, { label: e.target.value, labelAR: e.target.value })}
                                                    sx={{ flexGrow: 1, minWidth: 150 }}
                                                />
                                            )}

                                            <Box sx={{ flexGrow: 1, ml: 2, pointerEvents: 'none', opacity: 0.8, maxWidth: 300 }}>
                                                <ControlPreview control={c} />
                                            </Box>

                                            <Button size="small" variant="text" onClick={() => setSelected({ kind: 'requestControl', id: c.id })}>
                                                {t('workflow.configure', 'Configure')}
                                            </Button>
                                            <IconButton size="small" color="error" onClick={() => deleteRequestControl(c.id)}>
                                                <Delete fontSize="small" />
                                            </IconButton>
                                        </Stack>

                                        <Stack direction="row" spacing={1} sx={{ mt: 1, flexWrap: 'wrap', gap: 1 }} alignItems="center">
                                            <FormControlLabel
                                                control={
                                                    <Switch
                                                        size="small"
                                                        checked={!!c.visible}
                                                        onChange={(e) => updateRequestControl(c.id, { visible: e.target.checked })}
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
                                            {transitions.filter(tr => tr.requestControlId === c.id).map(tr => {
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
        </Box>
    );
});

RequestFormTab.displayName = 'RequestFormTab';
export default RequestFormTab;
