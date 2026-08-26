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
import { useAppTranslation } from '@core/localization/useAppTranslation';
export function ProcessBuilderTreePanel() {
  const { t, isRtl } = useAppTranslation();
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
  const variableName = (name: string) => name === 'New variable' ? t('wfProcessBuilder.structure.newVariable') : name;
  const activityName = (name: string) => name === 'New activity' ? t('wfProcessBuilder.structure.newActivity') : name;
  const stepName = (name: string, order: number) => /^Step \d+$/.test(name) ? t('wfProcessBuilder.structure.defaultStep', { number: order }) : name;
  const activityType = (type: string) => t(`wfProcessBuilder.activityTypes.${type === 'data-entry' ? 'dataEntry' : type}`, { defaultValue: type });
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
    marginInlineStart: '22px',
    paddingInlineStart: '10px',
    borderInlineStart: '1px solid #cbd5e1',
  };
  return (
    <List dense disablePadding aria-label={t('wfProcessBuilder.structure.title')} sx={{ px: '8px', py: '6px' }}>
      <ListItemButton
        selected={active('process')}
        sx={itemSx}
        onClick={() => select({ kind: 'process' })}
      >
        <ExpandMore sx={{ marginInlineEnd: '4px', fontSize: 18, color: tokens.textMuted }} />
        <AccountTree sx={{ marginInlineEnd: '8px', fontSize: 20, color: tokens.textMuted }} />
        <Tooltip title={`${d.name} · ${d.code}`} placement={isRtl ? 'left' : 'right'}>
          <ListItemText
            primary={d.name || t('wfProcessBuilder.structure.untitled')}
            secondary={`${d.code || t('wfProcessBuilder.structure.draft')} · ${d.active ? t('wfProcessBuilder.status.active') : t('wfProcessBuilder.status.inactive')} · ${t('wfProcessBuilder.structure.stepCount', { count: d.steps.length })}`}
          />
        </Tooltip>
      </ListItemButton>
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        <Typography component="div" sx={{ ...sectionSx, flex: 1 }}>{t('wfProcessBuilder.structure.variables')} <Chip size="small" label={d.variables.length} sx={{ marginInlineStart: 0.5, height: 18 }} /></Typography>
        <Tooltip title={t('wfProcessBuilder.actions.addVariable')}>
          <IconButton size="small" aria-label={t('wfProcessBuilder.actions.addVariable')} onClick={addVariable} sx={{ marginInlineEnd: 1 }}>
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
            <Tooltip title={`${variableName(v.name)} · ${t(`wfProcessBuilder.dataTypes.${v.dataType}`)}`} placement={isRtl ? 'left' : 'right'}>
              <ListItemText primary={variableName(v.name)} secondary={`${v.code || t('wfProcessBuilder.structure.draft')} · ${t(`wfProcessBuilder.dataTypes.${v.dataType}`)} · #${v.sortOrder} · ${v.active ? t('wfProcessBuilder.status.active') : t('wfProcessBuilder.status.inactive')}`} />
            </Tooltip>
          </ListItemButton>
        ))}
      </Box>
      <Typography component="div" sx={sectionSx}>{t('wfProcessBuilder.structure.requestControls')} <Chip size="small" label={d.requestControls.length} sx={{ marginInlineStart: 0.5, height: 18 }} /></Typography>
      <Box sx={branchSx}>
        {d.requestControls.map((c) => (
          <ListItemButton
            key={c.id}
            selected={active('requestControl', c.id)}
            sx={itemSx}
            onClick={() => select({ kind: 'requestControl', id: c.id })}
          >
            <Tooltip title={c.label} placement={isRtl ? 'left' : 'right'}>
              <ListItemText primary={c.label} secondary={`${c.code || t('wfProcessBuilder.structure.draft')} · #${c.sortOrder}`} />
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
        {t('wfProcessBuilder.actions.openRequestForm')}
      </Button>
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        <Typography component="div" sx={{ ...sectionSx, flex: 1 }}>{t('wfProcessBuilder.structure.steps')} <Chip size="small" label={d.steps.length} sx={{ marginInlineStart: 0.5, height: 18 }} /></Typography>
        <Tooltip title={t('wfProcessBuilder.actions.addStepShort')}>
          <IconButton size="small" aria-label={t('wfProcessBuilder.actions.addStepShort')} onClick={addStep} sx={{ marginInlineEnd: 1 }}>
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
                      sx={{ display: 'flex', marginInlineEnd: 0.25, cursor: 'grab' }}
                    >
                      <DragIndicator fontSize="small" />
                    </Box>
                    <IconButton
                      size="small"
                      aria-label={t(expanded[s.id] ? 'wfProcessBuilder.structure.collapseItem' : 'wfProcessBuilder.structure.expandItem', { name: stepName(s.name, s.order) })}
                      onClick={(event) => {
                        event.stopPropagation();
                        setExpanded((value) => ({ ...value, [s.id]: !value[s.id] }));
                      }}
                      sx={{ p: 0.25, marginInlineEnd: 0.25 }}
                    >
                      {expanded[s.id] ? (
                        <ExpandMore fontSize="small" />
                      ) : (
                        <ChevronRight fontSize="small" sx={{ transform: isRtl ? 'scaleX(-1)' : 'none' }} />
                      )}
                    </IconButton>
                    <Tooltip title={`${s.order}. ${stepName(s.name, s.order)} · ${t('wfProcessBuilder.structure.activityCount', { count: s.activities.length })}`} placement={isRtl ? 'left' : 'right'}>
                      <ListItemText primary={`${s.order}. ${stepName(s.name, s.order)}`} secondary={`${s.code || t('wfProcessBuilder.structure.draft')} · ${t('wfProcessBuilder.structure.activityCount', { count: s.activities.length })} · ${s.active ? t('wfProcessBuilder.status.active') : t('wfProcessBuilder.status.inactive')}`} />
                    </Tooltip>
                  </ListItemButton>
                  {expanded[s.id] && (
                    <Box sx={{ marginInlineStart: '20px', paddingInlineStart: '10px', borderInlineStart: '1px solid #cbd5e1' }}>
                    {s.activities.map((a) => (
                      <Box key={a.id}>
                        <ListItemButton
                          selected={active('activity', a.id)}
                          sx={itemSx}
                          onClick={() => select({ kind: 'activity', stepId: s.id, id: a.id })}
                        >
                          <Tooltip title={`${activityName(a.name)} · ${activityType(a.type)} · ${t('wfProcessBuilder.structure.controlCount', { count: a.controls.length })}`} placement={isRtl ? 'left' : 'right'}>
                            <ListItemText primary={activityName(a.name)} secondary={`${a.code || t('wfProcessBuilder.structure.draft')} · ${activityType(a.type)} · ${t('wfProcessBuilder.structure.controlCount', { count: a.controls.length })} · ${a.active ? t('wfProcessBuilder.status.active') : t('wfProcessBuilder.status.inactive')}`} />
                          </Tooltip>
                        </ListItemButton>
                        <Box sx={{ marginInlineStart: '20px', paddingInlineStart: '10px', borderInlineStart: '1px solid #dbe2ea' }}>
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
                            <Tooltip title={`${control.label} · ${control.type}`} placement={isRtl ? 'left' : 'right'}>
                              <ListItemText primary={control.label} secondary={`${control.code || t('wfProcessBuilder.structure.draft')} · ${control.type} · #${control.sortOrder}`} />
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
