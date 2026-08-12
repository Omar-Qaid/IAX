import React from 'react';
import {
    Box, Typography, IconButton, ListItemButton, ListItemText, ListItemIcon, Button, Chip, Stack
} from '@mui/material';
import {
    Add, AccountTree, DragIndicator, ExpandMore, ChevronRight
} from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { DndContext, closestCenter, type DragEndEvent, useSensors, useSensor, PointerSensor } from '@dnd-kit/core';
import { SortableContext, verticalListSortingStrategy, arrayMove } from '@dnd-kit/sortable';
import { useProcessBuilderContext } from '../../context/ProcessBuilderContext';
import { SortableItem } from '../shared/SortableItem';
import { getActivityIcon } from '../../utils/palette';

interface ProcessTreeProps {
    setCenterTab: (tab: number) => void;
}

export const ProcessTree: React.FC<ProcessTreeProps> = React.memo(({ setCenterTab }) => {
    const { t } = useTranslation();
    const {
        processInfo,
        variables,
        requestControls,
        steps,
        setSteps,
        selectedNode: selected,
        setSelectedNode: setSelected,
        expandedSteps,
        setExpandedSteps,
        addVariable,
        addStep,
    } = useProcessBuilderContext();

    const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));

    const onStepDragEnd = (e: DragEndEvent) => {
        const { active, over } = e;
        if (!over || active.id === over.id) return;
        setSteps((prev) => {
            const oldIdx = prev.findIndex((s) => s.id === active.id);
            const newIdx = prev.findIndex((s) => s.id === over.id);
            return arrayMove(prev, oldIdx, newIdx).map((s, i) => ({ ...s, order: i + 1, dirty: true }));
        });
    };

    return (
        <Box sx={{ p: 1 }}>
            <ListItemButton
                selected={selected.kind === 'process'}
                onClick={() => setSelected({ kind: 'process' })}
                dense
            >
                <ListItemIcon sx={{ minWidth: 32 }}>
                    <AccountTree fontSize="small" />
                </ListItemIcon>
                <ListItemText
                    primary={processInfo.name || t('workflow.new_process', 'New Process')}
                    secondary={processInfo.code || '—'}
                />
            </ListItemButton>

            <Box sx={{ ml: 2, mt: 1 }}>
                <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ pr: 1 }}>
                    <Typography variant="caption" sx={{ fontWeight: 700, color: 'text.secondary' }}>
                        {t('workflow.variables_allcaps', 'VARIABLES')}
                    </Typography>
                    <IconButton size="small" onClick={addVariable}>
                        <Add fontSize="small" />
                    </IconButton>
                </Stack>
                {variables.map((v) => (
                    <ListItemButton
                        key={v.id}
                        dense
                        selected={selected.kind === 'variable' && selected.id === v.id}
                        onClick={() => setSelected({ kind: 'variable', id: v.id })}
                    >
                        <ListItemText
                            primary={v.name}
                            secondary={v.dataType}
                            primaryTypographyProps={{ fontSize: 12 }}
                            secondaryTypographyProps={{ fontSize: 10 }}
                        />
                    </ListItemButton>
                ))}

                <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ pr: 1, mt: 1 }}>
                    <Typography variant="caption" sx={{ fontWeight: 700, color: 'text.secondary' }}>
                        {t('workflow.request_controls_allcaps', 'REQUEST CONTROLS')}
                    </Typography>
                </Stack>
                {requestControls.map((c) => (
                    <ListItemButton
                        key={c.id}
                        dense
                        selected={selected.kind === 'requestControl' && selected.id === c.id}
                        onClick={() => setSelected({ kind: 'requestControl', id: c.id })}
                    >
                        <ListItemText
                            primary={c.label || c.name}
                            secondary={c.type}
                            primaryTypographyProps={{ fontSize: 12 }}
                            secondaryTypographyProps={{ fontSize: 10 }}
                        />
                    </ListItemButton>
                ))}
                <Box sx={{ pl: 1, pr: 1, pb: 1 }}>
                    <Button
                        size="small"
                        fullWidth
                        startIcon={<Add />}
                        onClick={() => { setCenterTab(4); }}
                    >
                        {t('workflow.open_request_form', 'Open Request Form')}
                    </Button>
                </Box>

                <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ pr: 1, mt: 1 }}>
                    <Typography variant="caption" sx={{ fontWeight: 700, color: 'text.secondary' }}>
                        {t('workflow.steps_allcaps', 'STEPS')}
                    </Typography>
                    <IconButton size="small" onClick={addStep}>
                        <Add fontSize="small" />
                    </IconButton>
                </Stack>
                <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={onStepDragEnd}>
                    <SortableContext items={steps.map((s) => s.id)} strategy={verticalListSortingStrategy}>
                        {steps.map((s) => (
                            <SortableItem key={s.id} id={s.id}>
                                {(handle) => (
                                    <Box>
                                        <ListItemButton
                                            selected={
                                                (selected.kind === 'step' && selected.id === s.id) ||
                                                (selected.kind === 'activity' && selected.stepId === s.id)
                                            }
                                            onClick={() => setSelected({ kind: 'step', id: s.id })}
                                            dense
                                        >
                                            <Box {...handle} sx={{ cursor: 'grab', mr: 0.5, display: 'flex' }}>
                                                <DragIndicator fontSize="small" sx={{ color: 'text.disabled' }} />
                                            </Box>
                                            <IconButton
                                                size="small"
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    setExpandedSteps((p) => ({ ...p, [s.id]: !p[s.id] }));
                                                }}
                                            >
                                                {expandedSteps[s.id] ? <ExpandMore fontSize="small" /> : <ChevronRight fontSize="small" />}
                                            </IconButton>
                                            <ListItemText primary={s.name} primaryTypographyProps={{ fontSize: 13 }} />
                                            <Chip label={s.activities.length} size="small" sx={{ height: 18, fontSize: 10 }} />
                                        </ListItemButton>
                                        {expandedSteps[s.id] && (
                                            <Box sx={{ ml: 4 }}>
                                                {s.activities.map((a) => (
                                                    <ListItemButton
                                                        key={a.id}
                                                        dense
                                                        selected={selected.kind === 'activity' && selected.id === a.id}
                                                        onClick={() => setSelected({ kind: 'activity', stepId: s.id, id: a.id })}
                                                    >
                                                        <ListItemIcon sx={{ minWidth: 28 }}>
                                                            {getActivityIcon(a.type)}
                                                        </ListItemIcon>
                                                        <ListItemText primary={a.name} primaryTypographyProps={{ fontSize: 12 }} />
                                                    </ListItemButton>
                                                ))}
                                            </Box>
                                        )}
                                    </Box>
                                )}
                            </SortableItem>
                        ))}
                    </SortableContext>
                </DndContext>
            </Box>
        </Box>
    );
});

ProcessTree.displayName = 'ProcessTree';
