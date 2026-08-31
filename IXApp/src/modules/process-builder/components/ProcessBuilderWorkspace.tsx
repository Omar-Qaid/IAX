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
import { useProcessBuilderStore } from '../store/useProcessBuilderStore';
import { ControlPreview } from './ControlPreview';
import { controlPalette, getControlTypeLabel } from './ProcessBuilderPalette';
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
import { AppLookupGridField } from '@shared/components/fields/AppLookupGridField';
import { AppLookupField } from '@shared/components/fields/AppLookupField';
import { useQuery } from '@tanstack/react-query';
import { wfActivityTypeApi, wfOperatorApi } from '@modules/workflow/api/workflowSetupApis';
import { wfPerformerApi } from '@modules/workflow/api/wfPerformerApi';
import type { WorkflowMasterRecord } from '@modules/workflow/api/workflowMasterApi';
import type { BuilderControlType } from '../types/processBuilderTypes';
import { normalizeTransitionValue, TransitionValueField } from './TransitionValueField';
import { useAppTranslation } from '@core/localization/useAppTranslation';

const requestOptionControlTypes = new Set<BuilderControlType>([
  'dropdown-manual',
  'checkboxlist',
  'radiobuttonlist',
  'table',
]);

const compactSwitchSx = {
  width: 24,
  height: 14,
  p: 0,
  '& .MuiSwitch-switchBase': {
    p: '2px',
    '&.Mui-checked': { transform: 'translateX(10px)' },
  },
  '& .MuiSwitch-thumb': { width: 10, height: 10 },
  '& .MuiSwitch-track': { borderRadius: 7, bgcolor: '#cbd5e1', opacity: 1 },
  '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': {
    bgcolor: 'primary.main',
    opacity: 1,
  },
};

const activityLookupColumns = [
  { field: 'code', header: 'Code', width: 110 },
  { field: 'name', header: 'Name', flex: 1 },
] as const;
const activityLookupPage =
  (load: (signal?: AbortSignal) => Promise<WorkflowMasterRecord[]>) =>
  async ({
    pageNumber,
    pageSize,
    search,
    signal,
  }: {
    pageNumber: number;
    pageSize: number;
    search: string;
    signal?: AbortSignal;
  }) => {
    const records = await load(signal);
    const query = search.trim().toLocaleLowerCase();
    const filtered = query
      ? records.filter((record) =>
          `${record.code ?? ''} ${record.name ?? ''}`.toLocaleLowerCase().includes(query)
        )
      : records;
    const start = (pageNumber - 1) * pageSize;
    return {
      data: filtered.slice(start, start + pageSize),
      pageNumber,
      totalPages: Math.max(1, Math.ceil(filtered.length / pageSize)),
      totalRecords: filtered.length,
    };
  };
const fetchPerformerPage = activityLookupPage(wfPerformerApi.list);
const fetchOperatorPage = activityLookupPage(wfOperatorApi.list);
const builderTypeFromLabel = (
  label: string
): 'approval' | 'review' | 'data-entry' | 'api' | 'notification' => {
  const normalized = label.replace(/[^a-z0-9]/gi, '').toLocaleLowerCase();
  if (normalized.includes('dataentry')) return 'data-entry';
  if (normalized.includes('notification')) return 'notification';
  if (normalized.includes('review')) return 'review';
  if (normalized.includes('api')) return 'api';
  return 'approval';
};
const transitionOperatorFromLabel = (
  label: string
): '=' | '!=' | '>' | '<' | '>=' | '<=' | 'contains' | 'isEmpty' | 'between' => {
  const value = label.trim().toLocaleLowerCase();
  if (value === '<>' || value === 'neq') return '!=';
  if (value === 'gt') return '>';
  if (value === 'lt') return '<';
  if (value === 'gte') return '>=';
  if (value === 'lte') return '<=';
  if (value === 'between') return 'between';
  return ['=', '!=', '>', '<', '>=', '<=', 'contains', 'isEmpty', 'between'].includes(label)
    ? (label as '=' | '!=' | '>' | '<' | '>=' | '<=' | 'contains' | 'isEmpty' | 'between')
    : '=';
};

function UnsavedStatus({ compact = false }: { compact?: boolean }) {
  const { t } = useAppTranslation();
  return (
    <Chip
      size="small"
      variant="outlined"
      label={
        compact ? t('wfProcessBuilder.status.unsaved') : t('wfProcessBuilder.status.unsavedChanges')
      }
      aria-label={
        compact ? t('wfProcessBuilder.status.unsaved') : t('wfProcessBuilder.status.unsavedChanges')
      }
      sx={{
        height: compact ? 22 : 24,
        color: '#7a4b00',
        bgcolor: '#fff3cd',
        borderColor: '#f0c36d',
      }}
    />
  );
}

const stickyWorkspaceHeaderSx = {
  position: 'sticky',
  top: `${tokens.tabsHeight}px`,
  zIndex: 1,
  mx: { xs: '-8px', sm: '-10px' },
  px: { xs: '8px', sm: '10px' },
  py: '6px',
  minHeight: 44,
  bgcolor: tokens.canvas,
  borderBottom: `1px solid ${tokens.border}`,
};

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
      sx={{
        ...stickyWorkspaceHeaderSx,
        alignItems: { xs: 'stretch', sm: 'center' },
      }}
    >
      <Box sx={{ flex: 1, minWidth: 0 }} title={summary}>
        <Typography component="h2" sx={{ fontSize: tokens.fontSize.heading, fontWeight: 700 }}>
          {title}
        </Typography>
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
  const { t } = useAppTranslation();
  const s = useProcessBuilderStore();
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));
  const dragStep = ({ active, over }: DragEndEvent) => {
    if (over && active.id !== over.id) s.reorderSteps(String(active.id), String(over.id));
  };
  return (
    <Stack spacing="14px">
      <WorkspaceHeader
        title={t('wfProcessBuilder.designer.title')}
        summary={t('wfProcessBuilder.designer.summary', {
          steps: s.document.steps.length,
          activities: s.document.steps.reduce((count, step) => count + step.activities.length, 0),
        })}
        action={
          <Button variant="outlined" startIcon={<Add />} onClick={s.addStep}>
            {t('wfProcessBuilder.actions.addStep')}
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
                        aria-label={t('wfProcessBuilder.actions.dragItem', { name: step.name })}
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
                            ? t('wfProcessBuilder.status.pending')
                            : step.active
                              ? t('wfProcessBuilder.status.active')
                              : t('wfProcessBuilder.status.inactive')
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
                        slotProps={{
                          htmlInput: {
                            'aria-label': t('wfProcessBuilder.settings.fields.stepName'),
                          },
                        }}
                        sx={{ flex: '1 1 300px' }}
                      />
                      <Tooltip
                        title={t('wfProcessBuilder.actions.moveItemUp', { name: step.name })}
                      >
                        <span>
                          <IconButton
                            size="small"
                            disabled={index === 0}
                            aria-label={t('wfProcessBuilder.actions.moveItemUp', {
                              name: step.name,
                            })}
                            onClick={(event) => {
                              event.stopPropagation();
                              s.moveStep(step.id, -1);
                            }}
                          >
                            <ArrowUpward />
                          </IconButton>
                        </span>
                      </Tooltip>
                      <Tooltip
                        title={t('wfProcessBuilder.actions.moveItemDown', { name: step.name })}
                      >
                        <span>
                          <IconButton
                            size="small"
                            disabled={index === s.document.steps.length - 1}
                            aria-label={t('wfProcessBuilder.actions.moveItemDown', {
                              name: step.name,
                            })}
                            onClick={(event) => {
                              event.stopPropagation();
                              s.moveStep(step.id, 1);
                            }}
                          >
                            <ArrowDownward />
                          </IconButton>
                        </span>
                      </Tooltip>
                      <Button size="small" onClick={() => s.select({ kind: 'step', id: step.id })}>
                        {t('wfProcessBuilder.actions.configure')}
                      </Button>
                      <Tooltip
                        title={t('wfProcessBuilder.actions.deleteItem', { name: step.name })}
                      >
                        <IconButton
                          color="error"
                          size="small"
                          aria-label={t('wfProcessBuilder.actions.deleteItem', { name: step.name })}
                          onClick={(event) => {
                            event.stopPropagation();
                            if (
                              step.activities.length > 0 &&
                              !window.confirm(
                                t('wfProcessBuilder.actions.deleteStepConfirm', {
                                  name: step.name,
                                  count: step.activities.length,
                                })
                              )
                            )
                              return;
                            s.removeStep(step.id);
                          }}
                        >
                          <Delete fontSize="small" />
                        </IconButton>
                      </Tooltip>
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
                          <Typography sx={{ flex: 1, fontSize: tokens.fontSize.body }}>
                            {activity.name}
                          </Typography>
                          <Button
                            size="small"
                            aria-label={`${t('wfProcessBuilder.actions.configure')} ${activity.name}`}
                            onClick={(event) => {
                              event.stopPropagation();
                              s.select({ kind: 'activity', stepId: step.id, id: activity.id });
                            }}
                          >
                            {t('wfProcessBuilder.actions.configure')}
                          </Button>
                          <Chip
                            size="small"
                            label={t('wfProcessBuilder.structure.controlCount', {
                              count: activity.controls.length,
                            })}
                          />
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
export function VariablesWorkspace({
  onSave,
  saving = false,
  manualCode = false,
}: {
  onSave?: () => void;
  saving?: boolean;
  manualCode?: boolean;
}) {
  const { t } = useAppTranslation();
  const s = useProcessBuilderStore();
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));
  const drag = ({ active, over }: DragEndEvent) => {
    if (over && active.id !== over.id) s.reorderVariables(String(active.id), String(over.id));
  };
  return (
    <Stack spacing="16px">
      <WorkspaceHeader
        title={t('wfProcessBuilder.workspace.variablesTitle')}
        summary={t('wfProcessBuilder.workspace.variablesSummary', {
          total: s.document.variables.length,
          active: s.document.variables.filter((variable) => variable.active).length,
        })}
        dirty={s.document.id === 'new' || s.dirty}
        action={
          <Stack direction="row" spacing={1}>
            <Button variant="outlined" startIcon={<Add />} onClick={s.addVariable}>
              {t('wfProcessBuilder.actions.addVariable')}
            </Button>
            <Button
              variant="contained"
              disabled={s.document.id === 'new' || saving || !onSave}
              onClick={onSave}
            >
              {saving
                ? t('wfProcessBuilder.actions.saving')
                : t('wfProcessBuilder.actions.saveVariables')}
            </Button>
          </Stack>
        }
      />
      {s.document.id === 'new' && (
        <Typography sx={{ color: '#9a4f00', fontSize: tokens.fontSize.secondary }}>
          {t('wfProcessBuilder.workspace.saveVariablesFirst')}
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
                      <Chip
                        size="small"
                        label={`#${variable.sortOrder}`}
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
                        label={t('wfProcessBuilder.settings.fields.code')}
                        value={variable.code}
                        placeholder={
                          manualCode
                            ? t('wfProcessBuilder.settings.enterCode')
                            : t('wfProcessBuilder.settings.generatedCode')
                        }
                        disabled={!manualCode || /^\d+$/.test(variable.id)}
                        required={manualCode && !/^\d+$/.test(variable.id)}
                        onChange={(event) =>
                          s.updateVariable(variable.id, { code: event.target.value })
                        }
                        sx={{ width: 130 }}
                      />
                      <TextField
                        size="small"
                        label={t('wfProcessBuilder.settings.fields.variableName')}
                        value={
                          variable.name === 'New variable'
                            ? t('wfProcessBuilder.structure.newVariable')
                            : variable.name
                        }
                        onChange={(event) =>
                          s.updateVariable(variable.id, { name: event.target.value })
                        }
                        sx={{ flex: '1 1 240px' }}
                      />
                      <Tooltip
                        title={t('wfProcessBuilder.actions.deleteItem', { name: variable.name })}
                      >
                        <IconButton
                          color="error"
                          size="small"
                          aria-label={t('wfProcessBuilder.actions.deleteItem', {
                            name: variable.name,
                          })}
                          onClick={(event) => {
                            event.stopPropagation();
                            s.removeVariable(variable.id);
                          }}
                        >
                          <Delete fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Stack>
                    <Box
                      sx={{
                        mt: 1.5,
                        marginInlineStart: { xs: 0, md: '48px' },
                        pt: 1.5,
                        borderTop: `1px solid ${tokens.border}`,
                      }}
                    >
                      <Box
                        sx={{
                          display: 'grid',
                          gridTemplateColumns: {
                            xs: '1fr',
                            sm: 'minmax(150px, 1fr) 110px',
                          },
                          gap: 1,
                        }}
                      >
                        <TextField
                          select
                          size="small"
                          label={t('wfProcessBuilder.settings.fields.dataType')}
                          value={variable.dataType}
                          onChange={(event) =>
                            s.updateVariable(variable.id, {
                              dataType: event.target.value as typeof variable.dataType,
                            })
                          }
                        >
                          {['text', 'number', 'boolean', 'date', 'object'].map((type) => (
                            <MenuItem key={type} value={type}>
                              {t(`wfProcessBuilder.dataTypes.${type}`)}
                            </MenuItem>
                          ))}
                        </TextField>
                        <FormControlLabel
                          control={
                            <Switch
                              size="small"
                              checked={variable.active}
                              onChange={(_, active) => s.updateVariable(variable.id, { active })}
                            />
                          }
                          label={t('common.active')}
                          sx={{ m: 0, alignSelf: 'center' }}
                        />
                        <TextField
                          size="small"
                          label={t('wfProcessBuilder.settings.fields.description')}
                          value={variable.description}
                          onChange={(event) =>
                            s.updateVariable(variable.id, { description: event.target.value })
                          }
                          sx={{ gridColumn: '1 / -1' }}
                        />
                      </Box>
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
export function StepsWorkspace({
  onSave,
  saving = false,
  manualCode = false,
}: {
  onSave?: () => void;
  saving?: boolean;
  manualCode?: boolean;
}) {
  const { t } = useAppTranslation();
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
        title={t('wfProcessBuilder.workspace.workflowSteps')}
        summary={t('wfProcessBuilder.workspace.stepsSummary', {
          steps: s.document.steps.length,
          active: activeSteps,
          activities: activityCount,
        })}
        dirty={s.document.id === 'new' || s.dirty}
        action={
          <Stack direction="row" spacing={1}>
            <Button variant="outlined" startIcon={<Add />} onClick={s.addStep}>
              {t('wfProcessBuilder.actions.addStep')}
            </Button>
            <Button
              variant="contained"
              disabled={s.document.id === 'new' || saving || !onSave}
              onClick={onSave}
            >
              {saving
                ? t('wfProcessBuilder.actions.saving')
                : t('wfProcessBuilder.actions.saveSteps')}
            </Button>
          </Stack>
        }
      />
      {s.document.id === 'new' && (
        <Typography sx={{ color: '#9a4f00', fontSize: tokens.fontSize.secondary }}>
          {t('wfProcessBuilder.workspace.saveStepsFirst')}
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
                        label={t('wfProcessBuilder.settings.fields.code')}
                        value={step.code}
                        placeholder={
                          manualCode
                            ? t('wfProcessBuilder.settings.enterCode')
                            : t('wfProcessBuilder.settings.generatedCode')
                        }
                        disabled={!manualCode || /^\d+$/.test(step.id)}
                        required={manualCode && !/^\d+$/.test(step.id)}
                        onChange={(event) => s.updateStep(step.id, { code: event.target.value })}
                        sx={{ flex: '0 1 170px' }}
                      />
                      <TextField
                        size="small"
                        label={t('wfProcessBuilder.settings.fields.stepName')}
                        value={step.name}
                        onChange={(event) => s.updateStep(step.id, { name: event.target.value })}
                        sx={{ flex: '1 1 240px' }}
                      />
                      <Button
                        size="small"
                        variant="outlined"
                        onClick={(event) => {
                          event.stopPropagation();
                          s.select({ kind: 'step', id: step.id });
                          s.setCenterTab(4);
                        }}
                      >
                        {t('wfProcessBuilder.tabs.activities')}
                      </Button>
                      <Button
                        size="small"
                        onClick={(event) => {
                          event.stopPropagation();
                          s.select({ kind: 'step', id: step.id });
                        }}
                      >
                        {t('wfProcessBuilder.actions.configure')}
                      </Button>
                      <Tooltip
                        title={t('wfProcessBuilder.actions.deleteItem', { name: step.name })}
                      >
                        <IconButton
                          color="error"
                          size="small"
                          aria-label={t('wfProcessBuilder.actions.deleteItem', { name: step.name })}
                          onClick={(event) => {
                            event.stopPropagation();
                            s.removeStep(step.id);
                          }}
                        >
                          <Delete fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Stack>
                    <Box sx={{ mt: '4px', marginInlineStart: { xs: 0, md: '48px' } }}>
                      <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: 'wrap' }}>
                        <TextField
                          size="small"
                          type="number"
                          label={t('wfProcessBuilder.settings.fields.score')}
                          value={step.score}
                          onChange={(event) =>
                            s.updateStep(step.id, { score: Number(event.target.value) })
                          }
                          sx={{ width: 75 }}
                        />
                        <TextField
                          size="small"
                          type="number"
                          label={t('wfProcessBuilder.settings.fields.autoPassingHours')}
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
                          label={t('wfProcessBuilder.settings.fields.mandatory')}
                        />
                        <FormControlLabel
                          control={
                            <Switch
                              size="small"
                              checked={step.active}
                              onChange={(_, active) => s.updateStep(step.id, { active })}
                            />
                          }
                          label={t('common.active')}
                        />
                        <FormControlLabel
                          control={
                            <Switch
                              size="small"
                              checked={step.systemField}
                              onChange={(_, systemField) => s.updateStep(step.id, { systemField })}
                            />
                          }
                          label={t('wfProcessBuilder.settings.fields.system')}
                        />
                        {s.dirty && <UnsavedStatus compact />}
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
          {t('wfProcessBuilder.workspace.noSteps')}
        </Typography>
      )}
    </Stack>
  );
}
export function ActivitiesWorkspace({
  onSave,
  saving = false,
  manualCode = false,
}: {
  onSave?: () => void;
  saving?: boolean;
  manualCode?: boolean;
}) {
  const { t } = useAppTranslation();
  const s = useProcessBuilderStore();
  const activityTypes = useQuery({
    queryKey: ['workflow', 'builder-activity-type-options'],
    queryFn: ({ signal }) => wfActivityTypeApi.list(signal),
  });
  const activityTypeOptions = (activityTypes.data ?? []).map((item) => ({
    id: item.recId,
    code: item.code ?? '',
    name: item.name ?? '',
  }));
  const node = s.selected;
  const step =
    node.kind === 'step'
      ? s.document.steps.find((x) => x.id === node.id)
      : node.kind === 'activity'
        ? s.document.steps.find((x) => x.id === node.stepId)
        : node.kind === 'control'
          ? s.document.steps.find((x) => x.id === node.stepId)
          : (s.document.steps.find((x) => x.id === s.selectedStepId) ?? s.document.steps[0]);
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
        sx={{
          ...stickyWorkspaceHeaderSx,
          alignItems: { xs: 'stretch', sm: 'center' },
        }}
      >
        <Typography
          component="h2"
          sx={{ flex: 1, fontSize: tokens.fontSize.heading, fontWeight: 700 }}
        >
          {t('wfProcessBuilder.workspace.activitiesForStep', {
            name: step?.name ?? t('wfProcessBuilder.workspace.selectStep'),
          })}
        </Typography>
        {s.dirty && <UnsavedStatus />}
        <Button
          variant="outlined"
          size="small"
          startIcon={<Add />}
          disabled={!step}
          onClick={() => step && s.addActivity(step.id)}
        >
          {t('wfProcessBuilder.workspace.addActivity')}
        </Button>
        <Button
          variant="contained"
          size="small"
          disabled={s.document.id === 'new' || saving || !onSave}
          onClick={onSave}
        >
          {saving
            ? t('wfProcessBuilder.actions.saving')
            : t('wfProcessBuilder.actions.saveActivities')}
        </Button>
        <TextField
          select
          size="small"
          label={t('wfProcessBuilder.settings.fields.stepName')}
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
      <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={dragActivity}>
        <SortableContext
          items={step?.activities.map((activity) => activity.id) ?? []}
          strategy={verticalListSortingStrategy}
        >
          <Stack spacing={1}>
            {step?.activities.map((activity) => {
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
                        label={t('wfProcessBuilder.settings.fields.code')}
                        value={activity.code}
                        placeholder={
                          manualCode
                            ? t('wfProcessBuilder.settings.enterCode')
                            : t('wfProcessBuilder.settings.generatedCode')
                        }
                        disabled={!manualCode || /^\d+$/.test(activity.id)}
                        required={manualCode && !/^\d+$/.test(activity.id)}
                        onChange={(event) =>
                          s.updateActivity(step.id, activity.id, { code: event.target.value })
                        }
                      />
                      <TextField
                        size="small"
                        label={t('wfProcessBuilder.settings.fields.activityName')}
                        value={
                          activity.name === 'New activity'
                            ? t('wfProcessBuilder.structure.newActivity')
                            : activity.name
                        }
                        onChange={(e) =>
                          s.updateActivity(step.id, activity.id, { name: e.target.value })
                        }
                      />
                      <TextField
                        size="small"
                        type="number"
                        label={t('wfProcessBuilder.settings.fields.score')}
                        value={activity.score}
                        onChange={(event) =>
                          s.updateActivity(step.id, activity.id, {
                            score: Number(event.target.value),
                          })
                        }
                        sx={{ width: 82 }}
                      />
                      <Chip
                        size="small"
                        label={t('wfProcessBuilder.workspace.controls', {
                          count: activity.controls.length,
                        })}
                      />
                      <Button
                        size="small"
                        onClick={() => {
                          s.select({ kind: 'activity', stepId: step.id, id: activity.id });
                          s.setCenterTab(5);
                        }}
                      >
                        {t('wfProcessBuilder.workspace.editForm')}
                      </Button>
                      <Button
                        size="small"
                        onClick={() => s.addActivityControl(step.id, activity.id)}
                      >
                        {t('wfProcessBuilder.actions.configure')}
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
                        <Box
                          sx={{
                            width: '100%',
                            display: 'grid',
                            gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, minmax(0, 1fr))' },
                            gap: 1,
                          }}
                        >
                          <AppLookupField
                            name={`activityTypeId-${activity.id}`}
                            label={t('wfProcessBuilder.settings.fields.activityType')}
                            value={Number(activity.activityTypeId) || undefined}
                            options={activityTypeOptions}
                            onChange={(value, option) =>
                              s.updateActivity(step.id, activity.id, {
                                activityTypeId: value == null ? '' : String(value),
                                type:
                                  option && !Array.isArray(option)
                                    ? builderTypeFromLabel(option.name)
                                    : activity.type,
                              })
                            }
                            required
                            displayMode="select"
                          />
                          <AppLookupGridField<WorkflowMasterRecord>
                            name={`performerId-${activity.id}`}
                            label={t('wfProcessBuilder.settings.fields.performer')}
                            value={Number(activity.performer) || null}
                            onChange={(value) =>
                              s.updateActivity(step.id, activity.id, {
                                performer: value == null ? '' : String(value),
                              })
                            }
                            required
                            columns={[...activityLookupColumns]}
                            queryKey={['workflow', 'builder-performer-lookup']}
                            fetchPage={fetchPerformerPage}
                            fetchById={async (value) =>
                              (await wfPerformerApi.list()).find(
                                (item) => item.recId === Number(value)
                              ) ?? null
                            }
                            valueField="recId"
                            labelField="name"
                            pageSize={25}
                          />
                        </Box>
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
                          label={t('wfProcessBuilder.settings.mandatoryDocuments')}
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
                          label={t('common.active')}
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
                          label={t('wfProcessBuilder.settings.fields.required')}
                        />
                        {s.dirty && <UnsavedStatus compact />}
                        <Button
                          color="error"
                          size="small"
                          aria-label={t('wfProcessBuilder.actions.deleteItem', {
                            name: activity.name,
                          })}
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
                {t('wfProcessBuilder.workspace.addActivityHelp')}
              </Typography>
            )}
          </Stack>
        </SortableContext>
      </DndContext>
    </Stack>
  );
}
export function RequestFormWorkspace({
  onSave,
  saving = false,
  manualCode = false,
}: {
  onSave?: () => void;
  saving?: boolean;
  manualCode?: boolean;
}) {
  const { t } = useAppTranslation();
  const s = useProcessBuilderStore();
  const palette = controlPalette;
  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 5 } }));
  const dragControl = ({ active, over }: DragEndEvent) => {
    if (over && active.id !== over.id) s.reorderRequestControls(String(active.id), String(over.id));
  };
  return (
    <Stack spacing="14px">
      <WorkspaceHeader
        title={t('wfProcessBuilder.workspace.requestForm')}
        summary={t('wfProcessBuilder.workspace.processControls', {
          count: s.document.requestControls.length,
        })}
        dirty={s.document.id === 'new' || s.dirty}
        action={
          <Button
            variant="contained"
            disabled={s.document.id === 'new' || saving || !onSave}
            onClick={onSave}
          >
            {saving
              ? t('wfProcessBuilder.actions.saving')
              : t('wfProcessBuilder.workspace.saveRequestControls')}
          </Button>
        }
      />
      {s.document.id === 'new' && (
        <Typography sx={{ color: '#9a4f00', fontSize: tokens.fontSize.secondary }}>
          {t('wfProcessBuilder.workspace.saveProcessFirst')}
        </Typography>
      )}
      <Box sx={{ ...workspaceCardSx(), minHeight: 60, mt: '-6px' }}>
        <Typography sx={{ mb: 1, fontSize: tokens.fontSize.secondary, fontWeight: 700 }}>
          {t('wfProcessBuilder.workspace.addControl')}
        </Typography>
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
              {getControlTypeLabel(t, item.type)}
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
            {s.document.requestControls.map((control) => {
              const selected = s.selected.kind === 'requestControl' && s.selected.id === control.id;
              const controlTransitions = s.document.transitions.filter(
                (transition) =>
                  transition.triggerSource === 'requestControl' &&
                  transition.triggerId === control.id
              );
              const openSettings = (
                event: React.MouseEvent,
                pane: 'configure' | 'options' | 'validation' | 'transitions'
              ) => {
                event.stopPropagation();
                s.openControlSettings({ kind: 'requestControl', id: control.id }, pane);
              };
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
                        label={t('wfProcessBuilder.settings.fields.code')}
                        value={control.code}
                        placeholder={
                          manualCode
                            ? t('wfProcessBuilder.settings.enterCode')
                            : t('wfProcessBuilder.settings.generatedCode')
                        }
                        disabled={!manualCode || /^\d+$/.test(control.id)}
                        required={manualCode && !/^\d+$/.test(control.id)}
                        onChange={(event) =>
                          s.updateRequestControl(control.id, { code: event.target.value })
                        }
                      />
                      <TextField
                        size="small"
                        label={t('wfProcessBuilder.settings.fields.label')}
                        value={control.label}
                        onChange={(e) =>
                          s.updateRequestControl(control.id, { label: e.target.value })
                        }
                      />
                      <Box sx={{ px: 1 }}>
                        <ControlPreview control={control} />
                      </Box>
                      <Button size="small" onClick={(event) => openSettings(event, 'configure')}>
                        {t('wfProcessBuilder.actions.configure')}
                      </Button>
                      <Button
                        color="error"
                        size="small"
                        aria-label={t('wfProcessBuilder.actions.deleteItem', {
                          name: control.label,
                        })}
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
                          sx={{ m: 0 }}
                          control={
                            <Switch
                              size="small"
                              sx={compactSwitchSx}
                              checked={control.visible}
                              onChange={(_, visible) =>
                                s.updateRequestControl(control.id, { visible })
                              }
                            />
                          }
                          label={t('wfProcessBuilder.settings.fields.visible')}
                        />
                        {requestOptionControlTypes.has(control.type) && (
                          <Button size="small" onClick={(event) => openSettings(event, 'options')}>
                            {t('wfProcessBuilder.settings.optionsCount', {
                              count: control.options.length,
                            })}
                          </Button>
                        )}
                        <Button size="small" onClick={(event) => openSettings(event, 'validation')}>
                          {t('wfProcessBuilder.settings.validationCount', {
                            count: control.validations.length,
                          })}
                        </Button>
                        <Button
                          size="small"
                          onClick={(event) => openSettings(event, 'transitions')}
                        >
                          {t('wfProcessBuilder.settings.transitionsCount', {
                            count: controlTransitions.length,
                          })}
                        </Button>
                        {s.dirty && <UnsavedStatus compact />}
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
export function ActivityFormWorkspace({
  onSave,
  saving = false,
}: {
  onSave?: () => void;
  saving?: boolean;
}) {
  const { t } = useAppTranslation();
  const s = useProcessBuilderStore();
  const activityOptions = s.document.steps.flatMap((step) =>
    step.activities.map((activity) => ({ activity, stepId: step.id, stepName: step.name }))
  );
  const paletteOrder = [
    'text',
    'longtext',
    'date',
    'dropdown-manual',
    'checkbox',
    'checkboxlist',
    'radiobuttonlist',
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
          title={t('wfProcessBuilder.workspace.activityForm')}
          summary={t('wfProcessBuilder.workspace.selectActivitySummary')}
        />
        <Box
          role="region"
          aria-label={t('wfProcessBuilder.workspace.activityEmptyState')}
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
            {t('wfProcessBuilder.workspace.selectActivityTitle')}
          </Typography>
          <Typography sx={{ mt: '8px', color: tokens.textMuted, fontSize: tokens.fontSize.body }}>
            {t('wfProcessBuilder.workspace.selectActivityHelp')}
          </Typography>
          <Button variant="outlined" sx={{ mt: '16px' }} onClick={() => s.setCenterTab(4)}>
            {t('wfProcessBuilder.workspace.openActivities')}
          </Button>
          {s.document.steps.some((step) => step.activities.length > 0) && (
            <Stack spacing="6px" sx={{ mt: '16px', alignItems: 'center' }}>
              {s.document.steps.flatMap((step) =>
                step.activities.map((item) => (
                  <Button
                    key={item.id}
                    onClick={() => s.select({ kind: 'activity', stepId: step.id, id: item.id })}
                  >
                    {item.name}
                  </Button>
                ))
              )}
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
        sx={{
          ...stickyWorkspaceHeaderSx,
          alignItems: { xs: 'stretch', sm: 'center' },
        }}
      >
        <Typography
          component="h2"
          sx={{ flex: 1, fontSize: tokens.fontSize.heading, fontWeight: 700 }}
        >
          {t('wfProcessBuilder.workspace.activityFormNamed', { name: activity.name })}
        </Typography>
        {s.dirty && <UnsavedStatus compact />}
        <Button
          variant="contained"
          disabled={s.document.id === 'new' || saving || !onSave}
          onClick={onSave}
        >
          {saving
            ? t('wfProcessBuilder.actions.saving')
            : t('wfProcessBuilder.workspace.saveActivityControls')}
        </Button>
        <TextField
          select
          size="small"
          label={t('wfProcessBuilder.workspace.activity')}
          value={activity.id}
          onChange={(event) => {
            const option = activityOptions.find((item) => item.activity.id === event.target.value);
            if (option)
              s.select({ kind: 'activity', stepId: option.stepId, id: option.activity.id });
          }}
          sx={{ width: { xs: '100%', sm: 240 } }}
        >
          {activityOptions.map((option) => (
            <MenuItem key={`${option.stepId}-${option.activity.id}`} value={option.activity.id}>
              {option.stepName} · {option.activity.name}
            </MenuItem>
          ))}
        </TextField>
      </Stack>
      <Box sx={{ ...workspaceCardSx(), minHeight: 60 }}>
        <Typography sx={{ mb: 1, fontSize: tokens.fontSize.secondary, fontWeight: 700 }}>
          {t('wfProcessBuilder.workspace.addControl')}
        </Typography>
        <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: 'wrap' }}>
          {palette.map((item) => (
            <Button
              key={item.type}
              size="small"
              variant="outlined"
              startIcon={item.icon}
              onClick={() => s.addActivityControl(stepId, activity.id, item.type)}
            >
              {getControlTypeLabel(t, item.type)}
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
                    <TextField
                      size="small"
                      label={t('wfProcessBuilder.settings.fields.code')}
                      value={control.code}
                      disabled
                    />
                    <TextField
                      size="small"
                      label={t('wfProcessBuilder.settings.fields.label')}
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
                      onClick={(event) => {
                        event.stopPropagation();
                        s.openControlSettings(
                          { kind: 'control', stepId, activityId: activity.id, id: control.id },
                          'configure'
                        );
                      }}
                    >
                      {t('wfProcessBuilder.actions.configure')}
                    </Button>
                    <Button
                      color="error"
                      size="small"
                      aria-label={t('wfProcessBuilder.actions.deleteItem', { name: control.label })}
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
                      <Chip size="small" label={getControlTypeLabel(t, control.type)} />
                      <Chip
                        size="small"
                        variant="outlined"
                        label={
                          control.visible
                            ? t('wfProcessBuilder.settings.fields.visible')
                            : t('wfProcessBuilder.settings.hidden')
                        }
                      />
                      <Button
                        size="small"
                        onClick={(event) => {
                          event.stopPropagation();
                          s.openControlSettings(
                            { kind: 'control', stepId, activityId: activity.id, id: control.id },
                            'validation'
                          );
                        }}
                      >
                        {t('wfProcessBuilder.settings.validationCount', {
                          count: control.validations.length,
                        })}
                      </Button>
                      <Button
                        size="small"
                        onClick={(event) => {
                          event.stopPropagation();
                          s.openControlSettings(
                            { kind: 'control', stepId, activityId: activity.id, id: control.id },
                            'transitions'
                          );
                        }}
                      >
                        {t('wfProcessBuilder.settings.transitionsCount', {
                          count: s.document.transitions.filter(
                            (transition) =>
                              transition.triggerSource === 'activity' &&
                              transition.triggerId === activity.id
                          ).length,
                        })}
                      </Button>
                    </Box>
                    {requestOptionControlTypes.has(control.type) && (
                      <Box
                        onClick={(event) => event.stopPropagation()}
                        sx={{
                          gridColumn: { xs: '2', md: '2 / -1' },
                          p: 1.25,
                          border: `1px solid ${tokens.border}`,
                          bgcolor: '#f8fafc',
                        }}
                      >
                        <Stack direction="row" spacing={1} sx={{ alignItems: 'center', mb: 1 }}>
                          <Typography
                            sx={{ flex: 1, fontSize: tokens.fontSize.secondary, fontWeight: 700 }}
                          >
                            {t('wfProcessBuilder.settings.options')}
                          </Typography>
                          <Button
                            size="small"
                            startIcon={<Add fontSize="small" />}
                            aria-label={t('wfProcessBuilder.settings.addOptionTo', {
                              name: control.label,
                            })}
                            onClick={() =>
                              s.updateActivityControl(stepId, activity.id, control.id, {
                                options: [
                                  ...control.options,
                                  t('wfProcessBuilder.settings.optionNumber', {
                                    number: control.options.length + 1,
                                  }),
                                ],
                              })
                            }
                          >
                            {t('wfProcessBuilder.settings.addOption')}
                          </Button>
                        </Stack>
                        {control.options.length === 0 ? (
                          <Typography
                            sx={{ color: tokens.textMuted, fontSize: tokens.fontSize.secondary }}
                          >
                            {t('wfProcessBuilder.settings.addSelectableOption')}
                          </Typography>
                        ) : (
                          <DndContext
                            sensors={sensors}
                            collisionDetection={closestCenter}
                            onDragEnd={({ active, over }) => {
                              if (!over || active.id === over.id) return;
                              s.reorderActivityControlOptions(
                                stepId,
                                activity.id,
                                control.id,
                                Number(active.id),
                                Number(over.id)
                              );
                            }}
                          >
                            <SortableContext
                              items={control.options.map((_, index) => String(index))}
                              strategy={verticalListSortingStrategy}
                            >
                              <Stack spacing={1}>
                                {control.options.map((option, optionIndex) => (
                                  <SortableBuilderItem key={optionIndex} id={String(optionIndex)}>
                                    {(optionAttributes, optionListeners) => (
                                      <Stack
                                        direction="row"
                                        spacing={1}
                                        sx={{ alignItems: 'center' }}
                                      >
                                        <Tooltip
                                          title={t('wfProcessBuilder.settings.dragToReorder')}
                                        >
                                          <Box
                                            {...optionAttributes}
                                            {...optionListeners}
                                            aria-label={t(
                                              'wfProcessBuilder.settings.reorderOptionFor',
                                              { number: optionIndex + 1, name: control.label }
                                            )}
                                            sx={{
                                              display: 'flex',
                                              color: tokens.textMuted,
                                              cursor: 'grab',
                                              touchAction: 'none',
                                            }}
                                          >
                                            <DragIndicator fontSize="small" />
                                          </Box>
                                        </Tooltip>
                                        <TextField
                                          fullWidth
                                          size="small"
                                          label={t('wfProcessBuilder.settings.optionNumber', {
                                            number: optionIndex + 1,
                                          })}
                                          value={option}
                                          slotProps={{
                                            htmlInput: {
                                              'aria-label': t(
                                                'wfProcessBuilder.settings.optionFor',
                                                { number: optionIndex + 1, name: control.label }
                                              ),
                                            },
                                          }}
                                          onChange={(event) => {
                                            const options = [...control.options];
                                            options[optionIndex] = event.target.value;
                                            s.updateActivityControl(
                                              stepId,
                                              activity.id,
                                              control.id,
                                              { options }
                                            );
                                          }}
                                        />
                                        <IconButton
                                          size="small"
                                          color="error"
                                          aria-label={t(
                                            'wfProcessBuilder.settings.removeOptionFrom',
                                            { number: optionIndex + 1, name: control.label }
                                          )}
                                          onClick={() =>
                                            s.updateActivityControl(
                                              stepId,
                                              activity.id,
                                              control.id,
                                              {
                                                options: control.options.filter(
                                                  (_, index) => index !== optionIndex
                                                ),
                                              }
                                            )
                                          }
                                        >
                                          <Delete fontSize="small" />
                                        </IconButton>
                                      </Stack>
                                    )}
                                  </SortableBuilderItem>
                                ))}
                              </Stack>
                            </SortableContext>
                          </DndContext>
                        )}
                      </Box>
                    )}
                  </Box>
                )}
              </SortableBuilderItem>
            ))}
          </Stack>
        </SortableContext>
      </DndContext>
      <Box sx={{ ...workspaceCardSx(), mt: 0.5 }}>
        <Stack direction="row" sx={{ alignItems: 'center' }}>
          <Typography sx={{ flex: 1, fontSize: tokens.fontSize.body, fontWeight: 600 }}>
            {t('wfProcessBuilder.settings.actionsTitle')}
          </Typography>
          {(['approve', 'reject', 'return', 'escalate'] as const).map((type) => (
            <Button
              key={type}
              size="small"
              onClick={() => s.addActivityAction(stepId, activity.id, type)}
            >
              + {t(`wfProcessBuilder.actionTypes.${type}`)}
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
                label={t('wfProcessBuilder.settings.fields.action')}
                value={action.type}
                onChange={(event) =>
                  s.updateActivityAction(stepId, activity.id, action.id, {
                    type: event.target.value as typeof action.type,
                  })
                }
              >
                {['approve', 'reject', 'return', 'escalate'].map((type) => (
                  <MenuItem key={type} value={type}>
                    {t(`wfProcessBuilder.actionTypes.${type}`)}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                size="small"
                label={t('wfProcessBuilder.settings.fields.label')}
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
                label={t('wfProcessBuilder.settings.fields.nextStep')}
                value={action.nextStepId}
                onChange={(event) =>
                  s.updateActivityAction(stepId, activity.id, action.id, {
                    nextStepId: event.target.value,
                  })
                }
              >
                <MenuItem value="">{t('wfProcessBuilder.settings.endProcess')}</MenuItem>
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
                aria-label={t('wfProcessBuilder.actions.deleteItem', { name: action.label })}
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
  const { t } = useAppTranslation();
  const s = useProcessBuilderStore();
  const d = s.document;
  return (
    <Stack spacing={1.25}>
      <WorkspaceHeader
        title={t('wfProcessBuilder.tabsExtended.diagram')}
        summary={t('wfProcessBuilder.workspace.diagramSummary', {
          steps: d.steps.length,
          transitions: d.transitions.length,
        })}
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
            label={t('wfProcessBuilder.workspace.requestFlow')}
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
            {t('wfProcessBuilder.start')}
          </Box>
          <Box sx={{ height: 22, borderInlineStart: '2px solid #635bff' }} />
          <Box
            onClick={() => {
              s.select({ kind: 'process' });
              s.setCenterTab(2);
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
            <Typography sx={{ fontWeight: 800 }}>
              {t('wfProcessBuilder.tabsExtended.requestForm')}
            </Typography>
            <Typography variant="caption">
              {t('wfProcessBuilder.workspace.controls', { count: d.requestControls.length })}
            </Typography>
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
                  {step.activities.map((activity) => activity.name).join(' · ') ||
                    t('wfProcessBuilder.workspace.noActivities')}
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
            {t('wfProcessBuilder.workspace.processPath')}
          </Typography>
          <Typography variant="caption">
            <Box component="span" sx={{ color: '#f59e0b' }}>
              ●
            </Box>{' '}
            {t('wfProcessBuilder.workspace.conditionalTransition')}
          </Typography>
        </Stack>
      </Box>
    </Stack>
  );
}
export function TransitionsWorkspace({
  onSave,
  saving = false,
}: {
  onSave?: () => void;
  saving?: boolean;
}) {
  const { t } = useAppTranslation();
  const s = useProcessBuilderStore();
  const activities = s.document.steps.flatMap((step) =>
    step.activities.map((activity) => ({ ...activity, stepName: step.name }))
  );
  return (
    <Stack spacing="12px">
      <WorkspaceHeader
        title={t('wfProcessBuilder.workspace.transitionsTitle')}
        summary={t('wfProcessBuilder.workspace.transitionsSummary', {
          count: s.document.transitions.length,
        })}
        dirty={s.dirty}
        action={
          <Stack direction="row" spacing={1}>
            <Button
              variant="outlined"
              startIcon={<Add />}
              disabled={s.document.steps.length < 1}
              onClick={() => s.addTransition()}
            >
              {t('wfProcessBuilder.actions.newTransition')}
            </Button>
            <Button
              variant="contained"
              disabled={s.document.id === 'new' || saving || !onSave}
              onClick={onSave}
            >
              {saving
                ? t('wfProcessBuilder.actions.saving')
                : t('wfProcessBuilder.actions.saveTransitions')}
            </Button>
          </Stack>
        }
      />
      {s.document.transitions.map((x) => {
        const variable = s.document.variables.find((item) => item.id === x.variableId);
        return (
          <Box
            key={x.id}
            onClick={() => s.select({ kind: 'transition', id: x.id })}
            sx={workspaceCardSx(s.selected.kind === 'transition' && s.selected.id === x.id)}
          >
            <Stack
              direction="row"
              spacing={1}
              useFlexGap
              sx={{ alignItems: 'center', flexWrap: 'wrap' }}
            >
              <Box sx={{ display: 'flex', color: tokens.textMuted }} aria-hidden="true">
                <DragIndicator fontSize="small" />
              </Box>
              <Chip
                size="small"
                label={`#${x.sortOrder}`}
                sx={{
                  minWidth: 28,
                  height: 28,
                  borderRadius: '50%',
                  bgcolor: tokens.accent,
                  color: '#fff',
                  '& .MuiChip-label': { px: 0.5 },
                }}
              />
              <TextField
                size="small"
                label={t('wfProcessBuilder.settings.fields.transitionName')}
                value={x.name}
                onChange={(event) => s.updateTransition(x.id, { name: event.target.value })}
                sx={{ flex: '1 1 260px' }}
              />
              <Tooltip title={t('wfProcessBuilder.actions.deleteItem', { name: x.name })}>
                <IconButton
                  color="error"
                  size="small"
                  aria-label={t('wfProcessBuilder.actions.deleteItem', { name: x.name })}
                  onClick={(event) => {
                    event.stopPropagation();
                    s.removeTransition(x.id);
                  }}
                >
                  <Delete fontSize="small" />
                </IconButton>
              </Tooltip>
            </Stack>
            <Box
              sx={{
                mt: 1.5,
                marginInlineStart: { xs: 0, md: '52px' },
                pt: 1.5,
                borderTop: `1px solid ${tokens.border}`,
                display: 'grid',
                gridTemplateColumns: {
                  xs: '1fr',
                  sm: 'repeat(2, minmax(150px, 1fr))',
                  lg: 'repeat(3, minmax(150px, 1fr))',
                },
                gap: 1,
              }}
            >
              <TextField
                select
                size="small"
                label={t('wfProcessBuilder.settings.fields.triggerSource')}
                value={x.triggerSource}
                onChange={(e) =>
                  s.updateTransition(x.id, {
                    triggerSource: e.target.value as typeof x.triggerSource,
                    triggerId: '',
                  })
                }
              >
                {['none', 'requestControl', 'activity'].map((source) => (
                  <MenuItem key={source} value={source}>
                    {t(`wfProcessBuilder.triggerSources.${source}`)}
                  </MenuItem>
                ))}
              </TextField>
              {x.triggerSource === 'requestControl' && (
                <TextField
                  select
                  size="small"
                  label={t('wfProcessBuilder.settings.fields.requestControl')}
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
              {x.triggerSource === 'activity' && (
                <TextField
                  select
                  size="small"
                  label={t('wfProcessBuilder.settings.fields.activity')}
                  value={x.triggerId}
                  onChange={(e) => s.updateTransition(x.id, { triggerId: e.target.value })}
                >
                  {activities.map((activity) => (
                    <MenuItem key={activity.id} value={activity.id}>
                      {activity.stepName} · {activity.name}
                    </MenuItem>
                  ))}
                </TextField>
              )}
              <TextField
                select
                size="small"
                label={t('wfProcessBuilder.settings.fields.variable')}
                value={x.variableId}
                onChange={(e) => {
                  const variableId = e.target.value;
                  const dataType = s.document.variables.find(
                    (item) => item.id === variableId
                  )?.dataType;
                  s.updateTransition(x.id, {
                    variableId,
                    value: normalizeTransitionValue(x.value, dataType),
                  });
                }}
              >
                {s.document.variables.map((item) => (
                  <MenuItem key={item.id} value={item.id}>
                    {item.name}
                  </MenuItem>
                ))}
              </TextField>
              <AppLookupGridField<WorkflowMasterRecord>
                name={`operatorId-${x.id}`}
                label={t('wfProcessBuilder.settings.fields.operator')}
                value={Number(x.operatorId) || null}
                onChange={(value, row) =>
                  s.updateTransition(x.id, {
                    operatorId: value == null ? '' : String(value),
                    operator: row
                      ? transitionOperatorFromLabel(row.name ?? row.code ?? '')
                      : x.operator,
                  })
                }
                required
                columns={[...activityLookupColumns]}
                queryKey={['workflow', 'builder-operator-lookup']}
                fetchPage={fetchOperatorPage}
                fetchById={async (value) =>
                  (await wfOperatorApi.list()).find((item) => item.recId === Number(value)) ?? null
                }
                valueField="recId"
                labelField="name"
                pageSize={25}
              />
              <TransitionValueField
                dataType={variable?.dataType}
                value={x.value}
                disabled={x.operator === 'isEmpty'}
                onChange={(value) => s.updateTransition(x.id, { value })}
              />
              <TextField
                select
                size="small"
                label={t('wfProcessBuilder.settings.fields.targetStep')}
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
                label={t('wfProcessBuilder.settings.fields.sortOrder')}
                value={x.sortOrder}
                onChange={(e) => s.updateTransition(x.id, { sortOrder: Number(e.target.value) })}
              />
              <FormControlLabel
                control={
                  <Switch
                    size="small"
                    sx={compactSwitchSx}
                    checked={x.active}
                    onChange={(_, active) => s.updateTransition(x.id, { active })}
                  />
                }
                label={t('common.active')}
                sx={{ m: 0, alignSelf: 'center' }}
              />
            </Box>
          </Box>
        );
      })}
    </Stack>
  );
}
