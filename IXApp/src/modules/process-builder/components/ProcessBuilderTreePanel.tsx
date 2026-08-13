import React from 'react';
import {
  Box,
  Button,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  Tooltip,
  Typography,
} from '@mui/material';
import Add from '@mui/icons-material/Add';
import AccountTree from '@mui/icons-material/AccountTree';
import DragIndicator from '@mui/icons-material/DragIndicator';
import ChevronRight from '@mui/icons-material/ChevronRight';
import ExpandMore from '@mui/icons-material/ExpandMore';
import {
  closestCenter,
  DndContext,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
} from '@dnd-kit/core';
import { SortableContext, verticalListSortingStrategy } from '@dnd-kit/sortable';
import { useProcessBuilderStore } from '../store/useProcessBuilderStore';
import { processBuilderTokens as tokens } from './processBuilderTokens';
import { SortableBuilderItem } from './SortableBuilderItem';
export function ProcessBuilderTreePanel() {
  const {
    document: d,
    selected,
    select,
    addVariable,
    addStep,
    reorderSteps,
    setCenterTab,
  } = useProcessBuilderStore();
  const [expanded, setExpanded] = React.useState<Record<string, boolean>>(() =>
    Object.fromEntries(d.steps.map((step) => [step.id, true]))
  );
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));
  const onDragEnd = ({ active: drag, over }: DragEndEvent) => {
    if (over && drag.id !== over.id) reorderSteps(String(drag.id), String(over.id));
  };
  const active = (kind: string, id?: string) =>
    selected.kind === kind && (!id || ('id' in selected && selected.id === id));
  const itemSx = {
    mx: '8px',
    my: 0.25,
    borderRadius: 0,
    minHeight: 38,
    '&.Mui-selected': { bgcolor: tokens.accentSoft, color: tokens.accent },
    '&.Mui-selected:hover': { bgcolor: tokens.accentSoft },
    '& .MuiListItemText-primary': { fontSize: 10, overflow: 'hidden', textOverflow: 'ellipsis' },
    '& .MuiListItemText-secondary': { fontSize: 8 },
  };
  const sectionSx = {
    px: '24px',
    pt: '18px',
    pb: '8px',
    fontSize: 9,
    fontWeight: 600,
    color: tokens.textMuted,
  };
  return (
    <List dense disablePadding aria-label="Process structure" sx={{ py: 1 }}>
      <ListItemButton
        selected={active('process')}
        sx={itemSx}
        onClick={() => select({ kind: 'process' })}
      >
        <AccountTree sx={{ mr: '12px', color: tokens.textMuted }} />
        <ListItemText primary={d.name} secondary={d.code} />
      </ListItemButton>
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        <Typography sx={{ ...sectionSx, flex: 1 }}>VARIABLES</Typography>
        <Tooltip title="Add variable">
          <IconButton size="small" aria-label="Add variable" onClick={addVariable} sx={{ mr: 1 }}>
            <Add fontSize="small" />
          </IconButton>
        </Tooltip>
      </Box>
      {d.variables.map((v) => (
        <ListItemButton
          key={v.id}
          selected={active('variable', v.id)}
          sx={{ ...itemSx, pl: '32px' }}
          onClick={() => select({ kind: 'variable', id: v.id })}
        >
          <ListItemText primary={v.name} secondary={v.dataType} />
        </ListItemButton>
      ))}
      <Typography sx={sectionSx}>REQUEST CONTROLS</Typography>
      {d.requestControls.map((c) => (
        <ListItemButton
          key={c.id}
          selected={active('requestControl', c.id)}
          sx={{ ...itemSx, pl: '32px' }}
          onClick={() => select({ kind: 'requestControl', id: c.id })}
        >
          <ListItemText primary={c.label} secondary={c.type} />
        </ListItemButton>
      ))}
      <Button
        size="small"
        startIcon={<Add />}
        onClick={() => setCenterTab(4)}
        sx={{ width: '100%', my: '4px', justifyContent: 'center', textTransform: 'none' }}
      >
        Open Request Form
      </Button>
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        <Typography sx={{ ...sectionSx, flex: 1 }}>STEPS</Typography>
        <Tooltip title="Add step">
          <IconButton size="small" aria-label="Add step" onClick={addStep} sx={{ mr: 1 }}>
            <Add fontSize="small" />
          </IconButton>
        </Tooltip>
      </Box>
      <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={onDragEnd}>
        <SortableContext
          items={d.steps.map((step) => step.id)}
          strategy={verticalListSortingStrategy}
        >
          {d.steps.map((s) => (
            <SortableBuilderItem key={s.id} id={s.id}>
              {(attributes, listeners) => (
                <Box>
                  <ListItemButton
                    selected={active('step', s.id)}
                    sx={{ ...itemSx, pl: '34px' }}
                    onClick={() => select({ kind: 'step', id: s.id })}
                  >
                    <Box
                      component="span"
                      {...attributes}
                      {...listeners}
                      sx={{ display: 'flex', mr: 0.25, cursor: 'grab' }}
                    >
                      <DragIndicator fontSize="small" />
                    </Box>
                    <IconButton
                      size="small"
                      aria-label={`${expanded[s.id] ? 'Collapse' : 'Expand'} ${s.name}`}
                      onClick={(event) => {
                        event.stopPropagation();
                        setExpanded((value) => ({ ...value, [s.id]: !value[s.id] }));
                      }}
                      sx={{ p: 0.25, mr: 0.25 }}
                    >
                      {expanded[s.id] ? (
                        <ExpandMore fontSize="small" />
                      ) : (
                        <ChevronRight fontSize="small" />
                      )}
                    </IconButton>
                    <ListItemText
                      primary={`${s.order}. ${s.name}`}
                      secondary={`${s.activities.length} activities`}
                    />
                  </ListItemButton>
                  {expanded[s.id] &&
                    s.activities.map((a) => (
                      <Box key={a.id}>
                        <ListItemButton
                          selected={active('activity', a.id)}
                          sx={{ ...itemSx, ml: 3, pl: 2 }}
                          onClick={() => select({ kind: 'activity', stepId: s.id, id: a.id })}
                        >
                          <ListItemText
                            primary={a.name}
                            secondary={`${a.type} · ${a.controls.length} controls`}
                          />
                        </ListItemButton>
                        {a.controls.map((control) => (
                          <ListItemButton
                            key={control.id}
                            selected={active('control', control.id)}
                            sx={{ ...itemSx, ml: 5, pl: 2 }}
                            onClick={() =>
                              select({
                                kind: 'control',
                                stepId: s.id,
                                activityId: a.id,
                                id: control.id,
                              })
                            }
                          >
                            <ListItemText primary={control.label} secondary={control.type} />
                          </ListItemButton>
                        ))}
                      </Box>
                    ))}
                </Box>
              )}
            </SortableBuilderItem>
          ))}
        </SortableContext>
      </DndContext>
    </List>
  );
}
