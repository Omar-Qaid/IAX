import React from 'react';
import {
  Box,
  Button,
  Chip,
  FormControlLabel,
  IconButton,
  MenuItem,
  Stack,
  Switch,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import Add from '@mui/icons-material/Add';
import ArrowDownward from '@mui/icons-material/ArrowDownward';
import ArrowUpward from '@mui/icons-material/ArrowUpward';
import DragIndicator from '@mui/icons-material/DragIndicator';
import Delete from '@mui/icons-material/Delete';
import CalendarMonth from '@mui/icons-material/CalendarMonth';
import CheckBox from '@mui/icons-material/CheckBox';
import CloudUpload from '@mui/icons-material/CloudUpload';
import PersonSearch from '@mui/icons-material/PersonSearch';
import TableChart from '@mui/icons-material/TableChart';
import TextFields from '@mui/icons-material/TextFields';
import { BuilderItemCard } from './BuilderItemCard';
import { useProcessBuilderStore } from '../store/useProcessBuilderStore';
import { ControlPreview } from './ControlPreview';
import { activityPalette, controlPalette } from './ProcessBuilderPalette';
import {
  closestCenter,
  DndContext,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
} from '@dnd-kit/core';
import { SortableContext, verticalListSortingStrategy } from '@dnd-kit/sortable';
import { SortableBuilderItem } from './SortableBuilderItem';
import { processBuilderTokens as tokens } from './processBuilderTokens';

function UnsavedStatus({ compact = false }: { compact?: boolean }) {
  return (
    <Chip
      size="small"
      variant="outlined"
      label={compact ? 'Unsaved' : 'Unsaved changes'}
      aria-label={compact ? 'Unsaved item' : 'Workspace has unsaved changes'}
      sx={{ height: compact ? 22 : 24, color: '#7a4b00', bgcolor: '#fff3cd', borderColor: '#f0c36d' }}
    />
  );
}

function WorkspaceHeader({
  title,
  summary,
  dirty,
  action,
}: {
  title: string;
  summary: string;
  dirty?: boolean;
  action?: React.ReactNode;
}) {
  return (
    <Stack
      direction={{ xs: 'column', sm: 'row' }}
      spacing={1}
      sx={{ alignItems: { xs: 'stretch', sm: 'center' }, minHeight: 32 }}
    >
      <Box sx={{ flex: 1, minWidth: 0 }} title={summary}>
        <Typography component="h2" sx={{ fontSize: tokens.fontSize.heading, fontWeight: 700 }}>{title}</Typography>
      </Box>
      {dirty && <UnsavedStatus />}
      {action}
    </Stack>
  );
}

const workspaceCardSx = (selected = false) => ({
  p: '8px 10px',
  border: '1px solid',
  borderColor: selected ? tokens.warning : tokens.border,
  borderRadius: `${tokens.radius}px`,
  bgcolor: '#fff',
  boxShadow: 'none',
  transition: 'border-color 120ms ease',
  '&:hover': { borderColor: selected ? tokens.warning : tokens.borderStrong },
  '&:focus-within': { borderColor: tokens.accent },
});

export function DesignerWorkspace() {
  const s = useProcessBuilderStore();
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));
  const dragStep = ({ active, over }: DragEndEvent) => {
    if (over && active.id !== over.id) s.reorderSteps(String(active.id), String(over.id));
  };
  return (
    <Stack spacing="14px">
      <WorkspaceHeader
        title="Workflow Designer"
        summary={`${s.document.steps.length} steps · ${s.document.steps.reduce((count, step) => count + step.activities.length, 0)} activities · build and reorder the process`}
        action={
          <Button variant="outlined" startIcon={<Add />} onClick={s.addStep}>
            Add Step
          </Button>
        }
      />
      <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={dragStep}>
        <SortableContext
          items={s.document.steps.map((step) => step.id)}
          strategy={verticalListSortingStrategy}
        >
          <Stack spacing="16px">
            {s.document.steps.map((step, index) => (
              <SortableBuilderItem key={step.id} id={step.id}>
                {(attributes, listeners) => (
                  <Box
                    onClick={() => s.select({ kind: 'step', id: step.id })}
                    sx={{
                      ...workspaceCardSx(s.selected.kind === 'step' && s.selected.id === step.id),
                      minHeight: 92,
                    }}
                  >
                    <Stack
                      direction="row"
                      spacing="14px"
                      useFlexGap
                      sx={{ alignItems: 'center', flexWrap: 'wrap' }}
                    >
                      <Box
                        {...attributes}
                        {...listeners}
                        role="button"
                        tabIndex={0}
                        aria-label={`Drag ${step.name}`}
                        sx={{ display: 'flex', cursor: 'grab' }}
                      >
                        <DragIndicator />
                      </Box>
                      <Chip
                        size="small"
                        label={`#${step.order}`}
                        sx={{
                          width: 24,
                          height: 24,
                          borderRadius: '50%',
                          bgcolor: tokens.accent,
                          color: '#fff',
                          '& .MuiChip-label': { px: 0 },
                        }}
                      />
                      <Chip
                        size="small"
                        label={
                          s.document.id === 'new' || s.dirty
                            ? 'Pending'
                            : step.active
                              ? 'Active'
                              : 'Inactive'
                        }
                        sx={{
                          height: 24,
                          borderRadius: 12,
                          bgcolor:
                            s.document.id === 'new' || s.dirty
                              ? tokens.warning
                              : step.active
                                ? tokens.success
                                : '#e0e0e0',
                          color: s.dirty || step.active ? '#fff' : '#64748b',
                        }}
                      />
                      <TextField
                        size="small"
                        value={step.name}
                        onChange={(event) => s.updateStep(step.id, { name: event.target.value })}
                        slotProps={{ htmlInput: { 'aria-label': `Step ${step.order} name` } }}
                        sx={{ flex: '1 1 300px' }}
                      />
                      <Tooltip title="Move step up">
                        <span>
                          <IconButton size="small" disabled={index === 0} aria-label={`Move ${step.name} up`} onClick={(event) => { event.stopPropagation(); s.moveStep(step.id, -1); }}>
                            <ArrowUpward />
                          </IconButton>
                        </span>
                      </Tooltip>
                      <Tooltip title="Move step down">
                        <span>
                          <IconButton size="small" disabled={index === s.document.steps.length - 1} aria-label={`Move ${step.name} down`} onClick={(event) => { event.stopPropagation(); s.moveStep(step.id, 1); }}>
                            <ArrowDownward />
                          </IconButton>
                        </span>
                      </Tooltip>
                      <Button size="small" onClick={() => s.select({ kind: 'step', id: step.id })}>
                        Configure
                      </Button>
                      <Tooltip title={`Delete ${step.name}`}>
                        <IconButton
                          color="error"
                          size="small"
                          aria-label={`Delete ${step.name}`}
                          onClick={(event) => {
                            event.stopPropagation();
                            if (step.activities.length > 0 && !window.confirm(`Delete ${step.name} and its ${step.activities.length} activities?`)) return;
                            s.removeStep(step.id);
                          }}
                        >
                          <Delete fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Stack>
                    <Stack
                      direction="row"
                      spacing="12px"
                      useFlexGap
                      sx={{ pt: '12px', flexWrap: 'wrap' }}
                    >
                      {activityPalette.map((item) => (
                        <Button
                          key={item.type}
                          size="small"
                          variant="outlined"
                          startIcon={item.icon}
                          onClick={() => s.addActivity(step.id, item.type)}
                        >
                          {item.label}
                        </Button>
                      ))}
                    </Stack>
                    <Stack spacing={0.75}>
                      {step.activities.map((activity) => (
                        <Box
                          key={activity.id}
                          onClick={() =>
                            s.select({ kind: 'activity', stepId: step.id, id: activity.id })
                          }
                          sx={{
                            display: 'flex',
                            alignItems: 'center',
                            flexWrap: 'wrap',
                            gap: 1,
                            p: 1,
                            bgcolor: '#fff',
                            border: '1px solid',
                            borderColor:
                              s.selected.kind === 'activity' && s.selected.id === activity.id
                                ? tokens.warning
                                : tokens.border,
                            borderRadius: `${tokens.radius}px`,
                            cursor: 'pointer',
                          }}
                        >
                          <Typography sx={{ flex: 1, fontSize: tokens.fontSize.body }}>{activity.name}</Typography>
                          <Chip size="small" variant="outlined" label={activity.type} />
                          <Chip size="small" label={`${activity.controls.length} controls`} />
                        </Box>
                      ))}
                    </Stack>
                  </Box>
                )}
              </SortableBuilderItem>
            ))}
          </Stack>
        </SortableContext>
      </DndContext>
    </Stack>
  );
}
export function VariablesWorkspace() {
  const s = useProcessBuilderStore();
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));
  const drag = ({ active, over }: DragEndEvent) => {
    if (over && active.id !== over.id) s.reorderVariables(String(active.id), String(over.id));
  };
  return (
    <Stack spacing="16px">
      <WorkspaceHeader
        title="Variables"
        summary={`${s.document.variables.length} variables · ${s.document.variables.filter((variable) => variable.active).length} active`}
        dirty={s.document.id === 'new' || s.dirty}
        action={
          <Stack direction="row" spacing={1}>
            <Button variant="outlined" startIcon={<Add />} onClick={s.addVariable}>
              Add Variable
            </Button>
            <Button variant="contained" disabled>
              Save Variables
            </Button>
          </Stack>
        }
      />
      {s.document.id === 'new' && (
        <Typography sx={{ color: '#9a4f00', fontSize: tokens.fontSize.secondary }}>
          Save the Process first to enable variable creation (ProcessId required).
        </Typography>
      )}
      <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={drag}>
        <SortableContext
          items={s.document.variables.map((variable) => variable.id)}
          strategy={verticalListSortingStrategy}
        >
          <Stack spacing="14px">
            {s.document.variables.map((variable) => (
              <SortableBuilderItem key={variable.id} id={variable.id}>
                {(attributes, listeners) => (
                  <Box
                    onClick={() => s.select({ kind: 'variable', id: variable.id })}
                    sx={workspaceCardSx(
                      s.selected.kind === 'variable' && s.selected.id === variable.id
                    )}
                  >
                    <Stack
                      direction="row"
                      spacing={1}
                      useFlexGap
                      sx={{ alignItems: 'center', flexWrap: 'wrap' }}
                    >
                      <Box
                        {...attributes}
                        {...listeners}
                        sx={{ display: 'flex', cursor: 'grab', color: tokens.textMuted }}
                      >
                        <DragIndicator fontSize="small" />
                      </Box>
                      <TextField
                        size="small"
                        label="Variable name"
                        value={variable.name}
                        onChange={(event) =>
                          s.updateVariable(variable.id, { name: event.target.value })
                        }
                        sx={{ flex: '1 1 240px' }}
                      />
                      <TextField
                        size="small"
                        label="Code"
                        value={variable.code}
                        onChange={(event) =>
                          s.updateVariable(variable.id, { code: event.target.value })
                        }
                        sx={{ width: 130 }}
                      />
                      <Chip size="small" variant="outlined" label={variable.dataType} />
                      <Chip
                        size="small"
                        label={variable.active ? 'Active' : 'Inactive'}
                        color={variable.active ? 'success' : 'default'}
                      />
                      <Tooltip title={`Delete ${variable.name}`}>
                        <IconButton
                          color="error"
                          size="small"
                          aria-label={`Delete ${variable.name}`}
                          onClick={(event) => {
                            event.stopPropagation();
                            s.removeVariable(variable.id);
                          }}
                        >
                          <Delete fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Stack>
                    <Box sx={{ mt: 1.5, pt: 1.5, borderTop: `1px solid ${tokens.border}` }}>
                      <Box
                        sx={{
                          display: 'grid',
                          gridTemplateColumns: {
                            xs: '1fr',
                            sm: 'minmax(180px, 1fr) minmax(150px, .7fr) 100px',
                          },
                          gap: 1,
                        }}
                      >
                        <TextField
                          size="small"
                          label="Arabic name"
                          value={variable.nameAR}
                          onChange={(event) =>
                            s.updateVariable(variable.id, { nameAR: event.target.value })
                          }
                          slotProps={{ htmlInput: { dir: 'rtl' } }}
                        />
                        <TextField
                          select
                          size="small"
                          label="Data type"
                          value={variable.dataType}
                          onChange={(event) =>
                            s.updateVariable(variable.id, {
                              dataType: event.target.value as typeof variable.dataType,
                            })
                          }
                        >
                          {['text', 'number', 'boolean', 'date', 'object'].map((type) => (
                            <MenuItem key={type} value={type}>
                              {type}
                            </MenuItem>
                          ))}
                        </TextField>
                        <TextField
                          size="small"
                          type="number"
                          label="Sort"
                          value={variable.sortOrder}
                          onChange={(event) =>
                            s.updateVariable(variable.id, { sortOrder: Number(event.target.value) })
                          }
                        />
                      </Box>
                      <Box
                        sx={{
                          display: 'grid',
                          gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' },
                          gap: 1,
                          mt: 1,
                        }}
                      >
                        <TextField
                          size="small"
                          label="Description"
                          value={variable.description}
                          onChange={(event) =>
                            s.updateVariable(variable.id, { description: event.target.value })
                          }
                        />
                        <TextField
                          size="small"
                          label="Description (AR)"
                          value={variable.descriptionAR}
                          onChange={(event) =>
                            s.updateVariable(variable.id, { descriptionAR: event.target.value })
                          }
                          slotProps={{ htmlInput: { dir: 'rtl' } }}
                        />
                      </Box>
                      <Stack
                        direction="row"
                        spacing={2}
                        useFlexGap
                        sx={{ mt: 1, flexWrap: 'wrap' }}
                      >
                        <FormControlLabel
                          control={
                            <Switch
                              size="small"
                              checked={variable.required}
                              onChange={(_, required) =>
                                s.updateVariable(variable.id, { required })
                              }
                            />
                          }
                          label="Required"
                        />
                        <FormControlLabel
                          control={
                            <Switch
                              size="small"
                              checked={variable.active}
                              onChange={(_, active) => s.updateVariable(variable.id, { active })}
                            />
                          }
                          label="Active"
                        />
                      </Stack>
                    </Box>
                  </Box>
                )}
              </SortableBuilderItem>
            ))}
          </Stack>
        </SortableContext>
      </DndContext>
    </Stack>
  );
}
export function StepsWorkspace() {
  const s = useProcessBuilderStore();
  const activeSteps = s.document.steps.filter((step) => step.active).length;
  const activityCount = s.document.steps.reduce((count, step) => count + step.activities.length, 0);
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));
  const drag = ({ active, over }: DragEndEvent) => {
    if (over && active.id !== over.id) s.reorderSteps(String(active.id), String(over.id));
  };
  return (
    <Stack spacing="12px">
      <WorkspaceHeader
        title="Workflow Steps"
        summary={`${s.document.steps.length} steps · ${activeSteps} active · ${activityCount} activities`}
        dirty={s.document.id === 'new' || s.dirty}
        action={
          <Stack direction="row" spacing={1}>
            <Button variant="outlined" startIcon={<Add />} onClick={s.addStep}>
              Add Step
            </Button>
            <Button variant="contained" disabled>
              Save Steps
            </Button>
          </Stack>
        }
      />
      {s.document.id === 'new' && (
        <Typography sx={{ color: '#9a4f00', fontSize: tokens.fontSize.secondary }}>
          Save the Process first to enable steps (ProcessId required).
        </Typography>
      )}
      <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={drag}>
        <SortableContext
          items={s.document.steps.map((step) => step.id)}
          strategy={verticalListSortingStrategy}
        >
          <Stack spacing="12px">
            {s.document.steps.map((step) => (
              <SortableBuilderItem key={step.id} id={step.id}>
                {(attributes, listeners) => (
                  <Box
                    onClick={() => s.select({ kind: 'step', id: step.id })}
                    sx={workspaceCardSx(s.selected.kind === 'step' && s.selected.id === step.id)}
                  >
                    <Stack
                      direction="row"
                      spacing={1}
                      useFlexGap
                      sx={{ alignItems: 'center', flexWrap: 'wrap' }}
                    >
                      <Box
                        {...attributes}
                        {...listeners}
                        sx={{ display: 'flex', cursor: 'grab', color: tokens.textMuted }}
                      >
                        <DragIndicator fontSize="small" />
                      </Box>
                      <Chip
                        size="small"
                        label={`#${step.order}`}
                        sx={{
                          width: 24,
                          height: 24,
                          borderRadius: '50%',
                          bgcolor: tokens.accent,
                          color: '#fff',
                          '& .MuiChip-label': { px: 0 },
                        }}
                      />
                      <TextField
                        size="small"
                        label="Code"
                        value={step.code}
                        onChange={(event) => s.updateStep(step.id, { code: event.target.value })}
                        sx={{ flex: '0 1 170px' }}
                      />
                      <TextField
                        size="small"
                        label="Step name"
                        value={step.name}
                        onChange={(event) => s.updateStep(step.id, { name: event.target.value })}
                        sx={{ flex: '1 1 240px' }}
                      />
                      <Chip
                        size="small"
                        variant="outlined"
                        label={`${step.activities.length} act.`}
                      />
                      <Button
                        size="small"
                        variant="outlined"
                        onClick={(event) => {
                          event.stopPropagation();
                          s.select({ kind: 'step', id: step.id });
                          s.setCenterTab(3);
                        }}
                      >
                        Activities
                      </Button>
                      <Button
                        size="small"
                        onClick={(event) => {
                          event.stopPropagation();
                          s.select({ kind: 'step', id: step.id });
                        }}
                      >
                        Configure
                      </Button>
                      <Tooltip title={`Delete ${step.name}`}>
                        <IconButton
                          color="error"
                          size="small"
                          aria-label={`Delete ${step.name}`}
                          onClick={(event) => {
                            event.stopPropagation();
                            s.removeStep(step.id);
                          }}
                        >
                          <Delete fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Stack>
                    <Box sx={{ mt: '4px' }}>
                      <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: 'wrap' }}>
                        <TextField
                          size="small"
                          type="number"
                          label="Score"
                          value={step.score}
                          onChange={(event) =>
                            s.updateStep(step.id, { score: Number(event.target.value) })
                          }
                          sx={{ width: 75 }}
                        />
                        <TextField
                          size="small"
                          type="number"
                          label="Auto passing hours"
                          value={step.autoPassingHours}
                          onChange={(event) =>
                            s.updateStep(step.id, { autoPassingHours: Number(event.target.value) })
                          }
                          sx={{ width: 133 }}
                        />
                        <FormControlLabel
                          control={
                            <Switch
                              size="small"
                              checked={step.allMandatory}
                              onChange={(_, allMandatory) =>
                                s.updateStep(step.id, { allMandatory })
                              }
                            />
                          }
                          label="Mandatory"
                        />
                        <FormControlLabel
                          control={
                            <Switch
                              size="small"
                              checked={step.active}
                              onChange={(_, active) => s.updateStep(step.id, { active })}
                            />
                          }
                          label="Active"
                        />
                        <FormControlLabel
                          control={
                            <Switch
                              size="small"
                              checked={step.systemField}
                              onChange={(_, systemField) => s.updateStep(step.id, { systemField })}
                            />
                          }
                          label="System"
                        />
                        <UnsavedStatus compact />
                      </Stack>
                    </Box>
                  </Box>
                )}
              </SortableBuilderItem>
            ))}
          </Stack>
        </SortableContext>
      </DndContext>
      {s.document.steps.length === 0 && (
        <Typography color="text.secondary" sx={{ py: 5, textAlign: 'center' }}>
          No steps. Add the first step to begin the workflow.
        </Typography>
      )}
    </Stack>
  );
}
export function ActivitiesWorkspace() {
  const s = useProcessBuilderStore();
  const node = s.selected;
  const step =
    node.kind === 'step'
      ? s.document.steps.find((x) => x.id === node.id)
      : node.kind === 'activity'
        ? s.document.steps.find((x) => x.id === node.stepId)
        : s.document.steps[0];
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));
  const dragActivity = ({ active, over }: DragEndEvent) => {
    if (step && over && active.id !== over.id)
      s.reorderActivities(step.id, String(active.id), String(over.id));
  };
  return (
    <Stack spacing="16px">
      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        spacing={1}
        sx={{ alignItems: { sm: 'center' } }}
      >
        <Typography component="h2" sx={{ flex: 1, fontSize: tokens.fontSize.heading, fontWeight: 700 }}>
          Activities · {step?.name ?? 'Select step'}
        </Typography>
        {s.dirty && <UnsavedStatus />}
        <Button variant="outlined" size="small">
          Save Activities
        </Button>
        <TextField
          select
          size="small"
          label="Step"
          value={step?.id ?? ''}
          onChange={(e) => s.select({ kind: 'step', id: e.target.value })}
          sx={{ width: { xs: '100%', sm: 180 } }}
        >
          {s.document.steps.map((x) => (
            <MenuItem key={x.id} value={x.id}>
              {x.name}
            </MenuItem>
          ))}
        </TextField>
      </Stack>
      <Box sx={workspaceCardSx()}>
        <Typography sx={{ mb: 1, fontSize: tokens.fontSize.secondary, fontWeight: 700 }}>ADD ACTIVITY</Typography>
        <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: 'wrap' }}>
          {activityPalette.map((item) => (
            <Button
              key={item.type}
              variant="outlined"
              size="small"
              startIcon={item.icon}
              disabled={!step}
              onClick={() => step && s.addActivity(step.id, item.type)}
              sx={{ color: tokens.accent, borderColor: tokens.accent }}
            >
              {item.label}
            </Button>
          ))}
        </Stack>
      </Box>
      <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={dragActivity}>
        <SortableContext
          items={step?.activities.map((activity) => activity.id) ?? []}
          strategy={verticalListSortingStrategy}
        >
          <Stack spacing={1}>
            {step?.activities.map((activity, index) => {
              const selected = s.selected.kind === 'activity' && s.selected.id === activity.id;
              return (
                <SortableBuilderItem key={activity.id} id={activity.id}>
                  {(attributes, listeners) => (
                    <Box
                      onClick={() =>
                        s.select({ kind: 'activity', stepId: step.id, id: activity.id })
                      }
                      sx={{
                        ...workspaceCardSx(selected),
                        display: 'grid',
                        gridTemplateColumns: {
                          xs: '28px 1fr',
                          md: '28px 115px minmax(220px,1fr) auto auto auto auto',
                        },
                        gap: 1,
                        alignItems: 'center',
                      }}
                    >
                      <Box
                        {...attributes}
                        {...listeners}
                        sx={{ display: 'flex', cursor: 'grab', color: '#111827' }}
                      >
                        <DragIndicator />
                      </Box>
                      <TextField
                        size="small"
                        label="Code"
                        value={activity.code || `ACT-${String(index + 1).padStart(5, '0')}`}
                        disabled
                      />
                      <TextField
                        size="small"
                        label="Activity Name"
                        value={activity.name}
                        onChange={(e) =>
                          s.updateActivity(step.id, activity.id, { name: e.target.value })
                        }
                      />
                      <Chip size="small" variant="outlined" label={activity.type} />
                      <Chip size="small" label={`${activity.controls.length} controls`} />
                      <Button
                        size="small"
                        onClick={() => {
                          s.select({ kind: 'activity', stepId: step.id, id: activity.id });
                          s.setCenterTab(5);
                        }}
                      >
                        Edit Form
                      </Button>
                      <Button
                        size="small"
                        onClick={() => s.addActivityControl(step.id, activity.id)}
                      >
                        Configure
                      </Button>
                      <Box
                        sx={{
                          gridColumn: { xs: '2', md: '2 / -1' },
                          display: 'flex',
                          alignItems: 'center',
                          gap: 1.5,
                          flexWrap: 'wrap',
                        }}
                      >
                        <TextField
                          select
                          size="small"
                          label="Activity Type"
                          value={activity.type}
                          onChange={(e) =>
                            s.updateActivity(step.id, activity.id, {
                              type: e.target.value as typeof activity.type,
                            })
                          }
                          sx={{ width: 150 }}
                        >
                          {activityPalette.map((item) => (
                            <MenuItem key={item.type} value={item.type}>
                              {item.label}
                            </MenuItem>
                          ))}
                        </TextField>
                        <TextField
                          size="small"
                          label="Performer"
                          value={activity.performer}
                          onChange={(e) =>
                            s.updateActivity(step.id, activity.id, { performer: e.target.value })
                          }
                          sx={{ width: 150 }}
                        />
                        <FormControlLabel
                          control={
                            <Switch
                              size="small"
                              checked={activity.mandatoryDocs}
                              onChange={(_, mandatoryDocs) =>
                                s.updateActivity(step.id, activity.id, { mandatoryDocs })
                              }
                            />
                          }
                          label="Mandatory Docs"
                        />
                        <FormControlLabel
                          control={
                            <Switch
                              size="small"
                              checked={activity.active}
                              onChange={(_, active) =>
                                s.updateActivity(step.id, activity.id, { active })
                              }
                            />
                          }
                          label="Active"
                        />
                        <FormControlLabel
                          control={
                            <Switch
                              size="small"
                              checked={activity.required}
                              onChange={(_, required) =>
                                s.updateActivity(step.id, activity.id, { required })
                              }
                            />
                          }
                          label="Required"
                        />
                        <UnsavedStatus compact />
                        <Button
                          color="error"
                          size="small"
                          aria-label={`Delete ${activity.name}`}
                          onClick={(event) => {
                            event.stopPropagation();
                            s.removeActivity(step.id, activity.id);
                          }}
                        >
                          <Delete fontSize="small" />
                        </Button>
                      </Box>
                    </Box>
                  )}
                </SortableBuilderItem>
              );
            })}
            {step && step.activities.length === 0 && (
              <Typography color="text.secondary">
                Add an activity using the options above.
              </Typography>
            )}
          </Stack>
        </SortableContext>
      </DndContext>
    </Stack>
  );
}
export function RequestFormWorkspace() {
  const s = useProcessBuilderStore();
  const paletteOrder = [
    'text',
    'longtext',
    'date',
    'dropdown-manual',
    'checkbox',
    'table',
    'file',
    'employeesearch',
  ];
  const palette = controlPalette
    .filter((item) => paletteOrder.includes(item.type))
    .sort((left, right) => paletteOrder.indexOf(left.type) - paletteOrder.indexOf(right.type));
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));
  const dragControl = ({ active, over }: DragEndEvent) => {
    if (over && active.id !== over.id) s.reorderRequestControls(String(active.id), String(over.id));
  };
  return (
    <Stack spacing="14px">
      <WorkspaceHeader
        title="Request Form (Process-level controls)"
        summary={`${s.document.requestControls.length} process-level controls`}
        dirty={s.document.id === 'new' || s.dirty}
        action={
          <Button variant="contained" disabled>
            Save Request Controls
          </Button>
        }
      />
      {s.document.id === 'new' && (
        <Typography sx={{ color: '#9a4f00', fontSize: tokens.fontSize.secondary }}>
          Save the Process first to enable request controls (ProcessId required).
        </Typography>
      )}
      <Box sx={{ ...workspaceCardSx(), minHeight: 60, mt: '-6px' }}>
        <Typography sx={{ mb: 1, fontSize: tokens.fontSize.secondary, fontWeight: 700 }}>ADD CONTROL</Typography>
        <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: 'wrap' }}>
          {palette.map((item) => (
            <Button
              key={item.label}
              variant="outlined"
              size="small"
              startIcon={item.icon}
              onClick={() => s.addRequestControl(item.type)}
              sx={{
                minHeight: 32,
                color: '#475569',
                borderColor: '#d9dee8',
                borderRadius: `${tokens.radius}px`,
                textTransform: 'none',
                px: 1.5,
              }}
            >
              {item.type === 'dropdown-manual'
                ? 'Drop Down List (Fill Manually)'
                : item.type === 'employeesearch'
                  ? 'EmployeeSearch'
                  : item.label}
            </Button>
          ))}
        </Stack>
      </Box>
      <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={dragControl}>
        <SortableContext
          items={s.document.requestControls.map((control) => control.id)}
          strategy={verticalListSortingStrategy}
        >
          <Stack spacing={1}>
            {s.document.requestControls.map((control, index) => {
              const selected = s.selected.kind === 'requestControl' && s.selected.id === control.id;
              return (
                <SortableBuilderItem key={control.id} id={control.id}>
                  {(attributes, listeners) => (
                    <Box
                      onClick={() => s.select({ kind: 'requestControl', id: control.id })}
                      sx={{
                        ...workspaceCardSx(selected),
                        display: 'grid',
                        gridTemplateColumns: {
                          xs: '28px 1fr',
                          md: '28px 125px minmax(220px,1fr) 270px auto auto',
                        },
                        gap: 1,
                        alignItems: 'center',
                      }}
                    >
                      <Box
                        {...attributes}
                        {...listeners}
                        sx={{ display: 'flex', color: '#111827', cursor: 'grab' }}
                      >
                        <DragIndicator />
                      </Box>
                      <TextField
                        size="small"
                        label="Code"
                        value={`RCTL-${String(index + 1).padStart(4, '0')}`}
                        disabled
                      />
                      <TextField
                        size="small"
                        label="Label"
                        value={control.label}
                        onChange={(e) =>
                          s.updateRequestControl(control.id, { label: e.target.value })
                        }
                      />
                      {control.type === 'checkbox' ? (
                        <Box sx={{ px: 1 }}>
                          <ControlPreview control={control} />
                        </Box>
                      ) : (
                        <TextField
                          size="small"
                          label="Preview"
                          value={control.label}
                          multiline={control.label.length > 24}
                          minRows={control.label.length > 24 ? 2 : 1}
                          disabled
                        />
                      )}
                      <Button
                        size="small"
                        onClick={() => s.select({ kind: 'requestControl', id: control.id })}
                      >
                        Configure
                      </Button>
                      <Button
                        color="error"
                        size="small"
                        aria-label={`Delete ${control.label}`}
                        onClick={(event) => {
                          event.stopPropagation();
                          s.removeRequestControl(control.id);
                        }}
                      >
                        <Delete fontSize="small" />
                      </Button>
                      <Box
                        sx={{
                          gridColumn: { xs: '2', md: '2 / -1' },
                          display: 'flex',
                          alignItems: 'center',
                          gap: 1.5,
                          flexWrap: 'wrap',
                        }}
                      >
                        <FormControlLabel
                          control={
                            <Switch
                              size="small"
                              checked={control.required}
                              onChange={(_, required) =>
                                s.updateRequestControl(control.id, { required })
                              }
                            />
                          }
                          label="Required"
                        />
                        <FormControlLabel
                          control={
                            <Switch
                              size="small"
                              checked={!control.readOnly}
                              onChange={(_, visible) =>
                                s.updateRequestControl(control.id, { readOnly: !visible })
                              }
                            />
                          }
                          label="Visible"
                        />
                        <FormControlLabel
                          control={
                            <Switch
                              size="small"
                              checked={control.readOnly}
                              onChange={(_, readOnly) =>
                                s.updateRequestControl(control.id, { readOnly })
                              }
                            />
                          }
                          label="Read Only"
                        />
                        <Chip size="small" variant="outlined" label={control.type} />
                        <UnsavedStatus compact />
                      </Box>
                    </Box>
                  )}
                </SortableBuilderItem>
              );
            })}
          </Stack>
        </SortableContext>
      </DndContext>
    </Stack>
  );
}
export function ActivityFormWorkspace() {
  const s = useProcessBuilderStore();
  const paletteOrder = [
    'text',
    'longtext',
    'date',
    'dropdown-manual',
    'checkbox',
    'table',
    'file',
    'employeesearch',
  ];
  const palette = controlPalette
    .filter((item) => paletteOrder.includes(item.type))
    .sort((left, right) => paletteOrder.indexOf(left.type) - paletteOrder.indexOf(right.type));
  const node = s.selected;
  const stepId = node.kind === 'activity' || node.kind === 'control' ? node.stepId : '';
  const activityId =
    node.kind === 'activity' ? node.id : node.kind === 'control' ? node.activityId : '';
  const activity = s.document.steps
    .find((step) => step.id === stepId)
    ?.activities.find((item) => item.id === activityId);
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));
  if (!activity)
    return (
      <Stack spacing="14px">
        <WorkspaceHeader
          title="Activity Form"
          summary="Select an activity to design its controls and actions"
        />
        <Box
          role="region"
          aria-label="Activity form empty state"
          sx={{
            maxWidth: 560,
            mx: 'auto',
            mt: '48px !important',
            p: '28px',
            textAlign: 'center',
            border: `1px dashed ${tokens.borderStrong}`,
            bgcolor: '#fff',
          }}
        >
          <Typography sx={{ fontSize: tokens.fontSize.heading, fontWeight: 700 }}>
            Select an activity to design its form
          </Typography>
          <Typography sx={{ mt: '8px', color: tokens.textMuted, fontSize: tokens.fontSize.body }}>
            Add an activity first, or choose an existing activity from the process tree.
          </Typography>
          <Button variant="outlined" sx={{ mt: '16px' }} onClick={() => s.setCenterTab(3)}>
            Open Activities
          </Button>
          {s.document.steps.some((step) => step.activities.length > 0) && (
            <Stack spacing="6px" sx={{ mt: '16px', alignItems: 'center' }}>
              {s.document.steps.flatMap((step) => step.activities.map((item) => (
                <Button key={item.id} onClick={() => s.select({ kind: 'activity', stepId: step.id, id: item.id })}>
                  {item.name}
                </Button>
              )))}
            </Stack>
          )}
        </Box>
      </Stack>
    );
  const dragControl = ({ active, over }: DragEndEvent) => {
    if (over && active.id !== over.id)
      s.reorderControls(stepId, activity.id, String(active.id), String(over.id));
  };
  return (
    <Stack spacing="14px">
      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        spacing={1}
        sx={{ alignItems: { xs: 'stretch', sm: 'center' }, minHeight: 44 }}
      >
        <Typography component="h2" sx={{ flex: 1, fontSize: tokens.fontSize.heading, fontWeight: 700 }}>
          Activity Form · {activity.name}
        </Typography>
        {s.dirty && <UnsavedStatus compact />}
        <Button variant="contained">Save Activity Controls</Button>
      </Stack>
      <Box sx={{ ...workspaceCardSx(), minHeight: 60 }}>
        <Typography sx={{ mb: 1, fontSize: tokens.fontSize.secondary, fontWeight: 700 }}>ADD CONTROL</Typography>
        <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: 'wrap' }}>
          {palette.map((item) => (
            <Button
              key={item.type}
              size="small"
              variant="outlined"
              startIcon={item.icon}
              onClick={() => s.addActivityControl(stepId, activity.id, item.type)}
            >
              {item.type === 'dropdown-manual'
                ? 'Drop Down List (Fill Manually)'
                : item.type === 'employeesearch'
                  ? 'EmployeeSearch'
                  : item.label}
            </Button>
          ))}
        </Stack>
      </Box>
      <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={dragControl}>
        <SortableContext
          items={activity.controls.map((control) => control.id)}
          strategy={verticalListSortingStrategy}
        >
          <Stack spacing={1}>
            {activity.controls.map((control) => (
              <SortableBuilderItem key={control.id} id={control.id}>
                {(attributes, listeners) => (
                  <Box
                    sx={{
                      ...workspaceCardSx(
                        s.selected.kind === 'control' && s.selected.id === control.id
                      ),
                      display: 'grid',
                      gridTemplateColumns: {
                        xs: '28px 1fr',
                        md: '28px 120px minmax(220px,1fr) minmax(200px,.8fr) auto auto',
                      },
                      gap: 1,
                      alignItems: 'center',
                    }}
                  >
                    <Box {...attributes} {...listeners} sx={{ display: 'flex', cursor: 'grab' }}>
                      <DragIndicator />
                    </Box>
                    <TextField size="small" label="Code" value={control.code} disabled />
                    <TextField
                      size="small"
                      label="Label"
                      value={control.label}
                      onChange={(event) =>
                        s.updateActivityControl(stepId, activity.id, control.id, {
                          label: event.target.value,
                        })
                      }
                    />
                    <Box sx={{ pointerEvents: 'none' }}>
                      <ControlPreview control={control} />
                    </Box>
                    <Button
                      size="small"
                      onClick={() =>
                        s.select({
                          kind: 'control',
                          stepId,
                          activityId: activity.id,
                          id: control.id,
                        })
                      }
                    >
                      Configure
                    </Button>
                    <Button
                      color="error"
                      size="small"
                      aria-label={`Delete ${control.label}`}
                      onClick={() => s.removeActivityControl(stepId, activity.id, control.id)}
                    >
                      <Delete />
                    </Button>
                    <Box
                      sx={{
                        gridColumn: { xs: '2', md: '2 / -1' },
                        display: 'flex',
                        gap: 1,
                        alignItems: 'center',
                        flexWrap: 'wrap',
                      }}
                    >
                      <Chip size="small" label={control.type} />
                      <Chip
                        size="small"
                        variant="outlined"
                        label={control.required ? 'Required' : 'Optional'}
                      />
                      <Chip
                        size="small"
                        variant="outlined"
                        label={control.visible ? 'Visible' : 'Hidden'}
                      />
                      {control.readOnly && (
                        <Chip size="small" variant="outlined" label="Read only" />
                      )}
                    </Box>
                  </Box>
                )}
              </SortableBuilderItem>
            ))}
          </Stack>
        </SortableContext>
      </DndContext>
      <Box sx={{ ...workspaceCardSx(), mt: 0.5 }}>
        <Stack direction="row" sx={{ alignItems: 'center' }}>
          <Typography sx={{ flex: 1, fontSize: tokens.fontSize.body, fontWeight: 600 }}>Actions</Typography>
          {(['approve', 'reject', 'return', 'escalate'] as const).map((type) => (
            <Button
              key={type}
              size="small"
              onClick={() => s.addActivityAction(stepId, activity.id, type)}
            >
              + {type}
            </Button>
          ))}
        </Stack>
        <Stack spacing={1} sx={{ mt: 1 }}>
          {activity.actions.map((action) => (
            <Box
              key={action.id}
              sx={{
                display: 'grid',
                gridTemplateColumns: '150px 1fr 180px auto',
                gap: 1,
                alignItems: 'center',
              }}
            >
              <TextField
                select
                size="small"
                label="Action"
                value={action.type}
                onChange={(event) =>
                  s.updateActivityAction(stepId, activity.id, action.id, {
                    type: event.target.value as typeof action.type,
                  })
                }
              >
                {['approve', 'reject', 'return', 'escalate'].map((type) => (
                  <MenuItem key={type} value={type}>
                    {type}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                size="small"
                label="Label"
                value={action.label}
                onChange={(event) =>
                  s.updateActivityAction(stepId, activity.id, action.id, {
                    label: event.target.value,
                  })
                }
              />
              <TextField
                select
                size="small"
                label="Next step"
                value={action.nextStepId}
                onChange={(event) =>
                  s.updateActivityAction(stepId, activity.id, action.id, {
                    nextStepId: event.target.value,
                  })
                }
              >
                <MenuItem value="">End process</MenuItem>
                {s.document.steps
                  .filter((step) => step.id !== stepId)
                  .map((step) => (
                    <MenuItem key={step.id} value={step.id}>
                      {step.name}
                    </MenuItem>
                  ))}
              </TextField>
              <Button
                color="error"
                aria-label={`Delete ${action.label}`}
                onClick={() => s.removeActivityAction(stepId, activity.id, action.id)}
              >
                <Delete />
              </Button>
            </Box>
          ))}
        </Stack>
      </Box>
    </Stack>
  );
}
export function DiagramWorkspace() {
  const s = useProcessBuilderStore();
  const d = s.document;
  return (
    <Stack spacing={1.25}>
      <WorkspaceHeader
        title="Diagram"
        summary={`${d.steps.length} steps · ${d.transitions.length} transitions · visual process flow`}
        dirty={s.dirty}
      />
      <Box
        sx={{
          minWidth: 520,
          minHeight: 500,
          p: 2,
          borderRadius: `${tokens.radius}px`,
          backgroundColor: '#f8f9fb',
          backgroundImage: 'radial-gradient(#d8dde7 1px, transparent 1px)',
          backgroundSize: '16px 16px',
        }}
      >
        <Stack sx={{ alignItems: 'center' }}>
          <Chip
            label="Request flow"
            size="small"
            sx={{ alignSelf: 'flex-start', bgcolor: '#eef0ff', color: '#635bff' }}
          />
          <Box
            sx={{
              px: 3,
              py: 1,
              border: '2px solid #10b981',
              borderRadius: 8,
              bgcolor: '#ecfdf5',
              fontWeight: 700,
            }}
          >
            Start
          </Box>
          <Box sx={{ height: 22, borderInlineStart: '2px solid #635bff' }} />
          <Box
            onClick={() => {
              s.select({ kind: 'process' });
              s.setCenterTab(4);
            }}
            sx={{
              width: 280,
              p: 1.5,
              border: '2px solid #635bff',
              borderRadius: `${tokens.radius}px`,
              bgcolor: '#fff',
              textAlign: 'center',
              cursor: 'pointer',
            }}
          >
            <Typography sx={{ fontWeight: 800 }}>Request Form</Typography>
            <Typography variant="caption">{d.requestControls.length} controls</Typography>
          </Box>
          {d.steps.map((step) => (
            <React.Fragment key={step.id}>
              <Box sx={{ height: 26, borderInlineStart: '2px solid #635bff' }} />
              {d.transitions
                .filter((transition) => transition.targetStepId === step.id)
                .map((transition) => (
                  <Chip
                    key={transition.id}
                    size="small"
                    label={`${transition.operator} ${transition.value || '…'}`}
                    sx={{ mb: 0.5, bgcolor: '#fff7ed', color: '#c2410c' }}
                  />
                ))}
              <Box
                onClick={() => s.select({ kind: 'step', id: step.id })}
                sx={{
                  width: 300,
                  p: 1.5,
                  border: '2px solid',
                  borderColor:
                    s.selected.kind === 'step' && s.selected.id === step.id ? '#635bff' : '#a5b4fc',
                  borderRadius: `${tokens.radius}px`,
                  bgcolor: '#fff',
                  textAlign: 'center',
                  cursor: 'pointer',
                }}
              >
                <Stack direction="row" sx={{ justifyContent: 'center', gap: 0.75 }}>
                  <Chip size="small" label={`#${step.order}`} />
                  <Typography sx={{ fontWeight: 800 }}>{step.name}</Typography>
                </Stack>
                <Typography variant="caption">
                  {step.activities.map((activity) => activity.name).join(' · ') || 'No activities'}
                </Typography>
              </Box>
            </React.Fragment>
          ))}
        </Stack>
        <Stack direction="row" spacing={2} sx={{ mt: 2 }}>
          <Typography variant="caption">
            <Box component="span" sx={{ color: '#635bff' }}>
              ━
            </Box>{' '}
            Process path
          </Typography>
          <Typography variant="caption">
            <Box component="span" sx={{ color: '#f59e0b' }}>
              ●
            </Box>{' '}
            Conditional transition
          </Typography>
        </Stack>
      </Box>
    </Stack>
  );
}
export function TransitionsWorkspace() {
  const s = useProcessBuilderStore();
  const activityControls = s.document.steps.flatMap((step) =>
    step.activities.flatMap((activity) =>
      activity.controls.map((control) => ({ ...control, activityName: activity.name }))
    )
  );
  return (
    <Stack spacing="12px">
      <WorkspaceHeader
        title="Conditional Transitions"
        summary={`${s.document.transitions.length} transitions · route the process using configured conditions`}
        dirty={s.dirty}
        action={
          <Button
            variant="outlined"
            startIcon={<Add />}
            disabled={s.document.steps.length < 2}
            onClick={s.addTransition}
          >
            New transition
          </Button>
        }
      />
      {s.document.transitions.map((x) => {
        const variable = s.document.variables.find((item) => item.id === x.variableId);
        return (
          <BuilderItemCard
            key={x.id}
            title={x.name}
            subtitle={`${x.operator} ${x.value}`}
            selected={s.selected.kind === 'transition' && s.selected.id === x.id}
            onSelect={() => s.select({ kind: 'transition', id: x.id })}
            onDelete={() => s.removeTransition(x.id)}
          >
            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: { xs: '1fr', md: 'repeat(3, minmax(150px, 1fr))' },
                gap: 1,
                mt: 1,
              }}
            >
              <TextField
                select
                size="small"
                label="Trigger source"
                value={x.triggerSource}
                onChange={(e) =>
                  s.updateTransition(x.id, {
                    triggerSource: e.target.value as typeof x.triggerSource,
                    triggerId: '',
                  })
                }
              >
                {['none', 'requestControl', 'activityControl'].map((source) => (
                  <MenuItem key={source} value={source}>
                    {source}
                  </MenuItem>
                ))}
              </TextField>
              {x.triggerSource === 'requestControl' && (
                <TextField
                  select
                  size="small"
                  label="Request control"
                  value={x.triggerId}
                  onChange={(e) => s.updateTransition(x.id, { triggerId: e.target.value })}
                >
                  {s.document.requestControls.map((control) => (
                    <MenuItem key={control.id} value={control.id}>
                      {control.label}
                    </MenuItem>
                  ))}
                </TextField>
              )}
              {x.triggerSource === 'activityControl' && (
                <TextField
                  select
                  size="small"
                  label="Activity control"
                  value={x.triggerId}
                  onChange={(e) => s.updateTransition(x.id, { triggerId: e.target.value })}
                >
                  {activityControls.map((control) => (
                    <MenuItem key={control.id} value={control.id}>
                      {control.activityName} · {control.label}
                    </MenuItem>
                  ))}
                </TextField>
              )}
              <TextField
                select
                size="small"
                label="Variable"
                value={x.variableId}
                onChange={(e) => s.updateTransition(x.id, { variableId: e.target.value })}
              >
                {s.document.variables.map((item) => (
                  <MenuItem key={item.id} value={item.id}>
                    {item.name}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                select
                size="small"
                label="Operator"
                value={x.operator}
                onChange={(e) =>
                  s.updateTransition(x.id, { operator: e.target.value as typeof x.operator })
                }
              >
                {['=', '!=', '>', '<', '>=', '<=', 'contains', 'isEmpty'].map((operator) => (
                  <MenuItem key={operator} value={operator}>
                    {operator}
                  </MenuItem>
                ))}
              </TextField>
              {variable?.dataType === 'boolean' ? (
                <TextField
                  select
                  size="small"
                  label="Comparison value"
                  value={x.value}
                  onChange={(e) => s.updateTransition(x.id, { value: e.target.value })}
                >
                  <MenuItem value="true">Yes</MenuItem>
                  <MenuItem value="false">No</MenuItem>
                </TextField>
              ) : (
                <TextField
                  size="small"
                  label="Comparison value"
                  value={x.value}
                  onChange={(e) => s.updateTransition(x.id, { value: e.target.value })}
                />
              )}
              <TextField
                select
                size="small"
                label="From step"
                value={x.sourceStepId}
                onChange={(e) => s.updateTransition(x.id, { sourceStepId: e.target.value })}
              >
                {s.document.steps.map((step) => (
                  <MenuItem key={step.id} value={step.id}>
                    {step.name}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                select
                size="small"
                label="Target step"
                value={x.targetStepId}
                onChange={(e) => s.updateTransition(x.id, { targetStepId: e.target.value })}
              >
                {s.document.steps.map((step) => (
                  <MenuItem key={step.id} value={step.id}>
                    {step.name}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                size="small"
                type="number"
                label="Sort order"
                value={x.sortOrder}
                onChange={(e) => s.updateTransition(x.id, { sortOrder: Number(e.target.value) })}
              />
              <FormControlLabel
                control={
                  <Switch
                    checked={x.active}
                    onChange={(_, active) => s.updateTransition(x.id, { active })}
                  />
                }
                label="Active"
              />
            </Box>
          </BuilderItemCard>
        );
      })}
    </Stack>
  );
}
