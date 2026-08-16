import React from 'react';
import {
  Box,
  Button,
  Chip,
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
  React.useEffect(() => {
    setExpanded((current) => Object.fromEntries(
      d.steps.map((step) => [step.id, current[step.id] ?? true])
    ));
  }, [d.steps]);
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));
  const onDragEnd = ({ active: drag, over }: DragEndEvent) => {
    if (over && drag.id !== over.id) reorderSteps(String(drag.id), String(over.id));
  };
  const active = (kind: string, id?: string) =>
    selected.kind === kind && (!id || ('id' in selected && selected.id === id));
  const itemSx = {
    mx: 0,
    my: 0,
    px: '8px',
    py: '3px',
    borderRadius: `${tokens.radius}px`,
    minHeight: 36,
    '&.Mui-selected': { bgcolor: tokens.accentSoft, color: tokens.accent },
    '&.Mui-selected:hover': { bgcolor: tokens.accentSoft },
    '&:hover': { bgcolor: '#f8fafc' },
    '&:focus-visible': { boxShadow: tokens.focusRing },
    '& .MuiListItemText-primary': { fontSize: tokens.fontSize.body, fontWeight: 500, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
    '& .MuiListItemText-secondary': { fontSize: tokens.fontSize.caption, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
  };
  const sectionSx = {
    px: '8px',
    pt: '12px',
    pb: '4px',
    fontSize: tokens.fontSize.secondary,
    fontWeight: 600,
    color: tokens.textMuted,
  };
  const branchSx = {
    ml: '22px',
    pl: '10px',
    borderLeft: '1px solid #cbd5e1',
  };
  return (
    <List dense disablePadding aria-label="Process structure" sx={{ px: '8px', py: '6px' }}>
      <ListItemButton
        selected={active('process')}
        sx={itemSx}
        onClick={() => select({ kind: 'process' })}
      >
        <ExpandMore sx={{ mr: '4px', fontSize: 18, color: tokens.textMuted }} />
        <AccountTree sx={{ mr: '8px', fontSize: 20, color: tokens.textMuted }} />
        <Tooltip title={`${d.name} · ${d.code}`} placement="right">
          <ListItemText
            primary={d.name || 'Untitled process'}
            secondary={`${d.code || 'Draft'} · ${d.active ? 'Active' : 'Inactive'} · ${d.steps.length} steps`}
          />
        </Tooltip>
      </ListItemButton>
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        <Typography component="div" sx={{ ...sectionSx, flex: 1 }}>VARIABLES <Chip size="small" label={d.variables.length} sx={{ ml: 0.5, height: 18 }} /></Typography>
        <Tooltip title="Add variable">
          <IconButton size="small" aria-label="Add variable" onClick={addVariable} sx={{ mr: 1 }}>
            <Add fontSize="small" />
          </IconButton>
        </Tooltip>
      </Box>
      <Box sx={branchSx}>
        {d.variables.map((v) => (
          <ListItemButton
            key={v.id}
            selected={active('variable', v.id)}
            sx={itemSx}
            onClick={() => select({ kind: 'variable', id: v.id })}
          >
            <Tooltip title={`${v.name} · ${v.dataType}`} placement="right">
              <ListItemText primary={v.name} secondary={`${v.code || 'Draft'} · ${v.dataType} · #${v.sortOrder} · ${v.active ? 'Active' : 'Inactive'}`} />
            </Tooltip>
          </ListItemButton>
        ))}
      </Box>
      <Typography component="div" sx={sectionSx}>REQUEST CONTROLS <Chip size="small" label={d.requestControls.length} sx={{ ml: 0.5, height: 18 }} /></Typography>
      <Box sx={branchSx}>
        {d.requestControls.map((c) => (
          <ListItemButton
            key={c.id}
            selected={active('requestControl', c.id)}
            sx={itemSx}
            onClick={() => select({ kind: 'requestControl', id: c.id })}
          >
            <Tooltip title={`${c.label} · ${c.type}`} placement="right">
              <ListItemText primary={c.label} secondary={`${c.code || 'Draft'} · ${c.type} · #${c.sortOrder} · ${c.required ? 'Required' : 'Optional'}`} />
            </Tooltip>
          </ListItemButton>
        ))}
      </Box>
      <Button
        size="small"
        startIcon={<Add />}
        onClick={() => setCenterTab(2)}
        sx={{ width: '100%', my: '4px', justifyContent: 'center', textTransform: 'none' }}
      >
        Open Request Form
      </Button>
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        <Typography component="div" sx={{ ...sectionSx, flex: 1 }}>STEPS <Chip size="small" label={d.steps.length} sx={{ ml: 0.5, height: 18 }} /></Typography>
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
          <Box sx={branchSx}>
          {d.steps.map((s) => (
            <SortableBuilderItem key={s.id} id={s.id}>
              {(attributes, listeners) => (
                <Box>
                  <ListItemButton
                    selected={active('step', s.id)}
                    sx={itemSx}
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
                    <Tooltip title={`${s.order}. ${s.name} · ${s.activities.length} activities`} placement="right">
                      <ListItemText primary={`${s.order}. ${s.name}`} secondary={`${s.code || 'Draft'} · ${s.activities.length} activities · ${s.active ? 'Active' : 'Inactive'}`} />
                    </Tooltip>
                  </ListItemButton>
                  {expanded[s.id] && (
                    <Box sx={{ ml: '20px', pl: '10px', borderLeft: '1px solid #cbd5e1' }}>
                    {s.activities.map((a) => (
                      <Box key={a.id}>
                        <ListItemButton
                          selected={active('activity', a.id)}
                          sx={itemSx}
                          onClick={() => select({ kind: 'activity', stepId: s.id, id: a.id })}
                        >
                          <Tooltip title={`${a.name} · ${a.type} · ${a.controls.length} controls`} placement="right">
                            <ListItemText primary={a.name} secondary={`${a.code || 'Draft'} · ${a.type} · ${a.controls.length} controls · ${a.active ? 'Active' : 'Inactive'}`} />
                          </Tooltip>
                        </ListItemButton>
                        <Box sx={{ ml: '20px', pl: '10px', borderLeft: '1px solid #dbe2ea' }}>
                        {a.controls.map((control) => (
                          <ListItemButton
                            key={control.id}
                            selected={active('control', control.id)}
                            sx={{ ...itemSx, minHeight: 34 }}
                            onClick={() =>
                              select({
                                kind: 'control',
                                stepId: s.id,
                                activityId: a.id,
                                id: control.id,
                              })
                            }
                          >
                            <Tooltip title={`${control.label} · ${control.type}`} placement="right">
                              <ListItemText primary={control.label} secondary={`${control.code || 'Draft'} · ${control.type} · ${control.required ? 'Required' : 'Optional'} · #${control.sortOrder}`} />
                            </Tooltip>
                          </ListItemButton>
                        ))}
                        </Box>
                      </Box>
                    ))}
                    </Box>
                  )}
                </Box>
              )}
            </SortableBuilderItem>
          ))}
          </Box>
        </SortableContext>
      </DndContext>
    </List>
  );
}
