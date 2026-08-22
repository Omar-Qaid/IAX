import React from 'react';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Autocomplete,
  Box,
  Button,
  Chip,
  FormControlLabel,
  MenuItem,
  Stack,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import ExpandMore from '@mui/icons-material/ExpandMore';
import Delete from '@mui/icons-material/Delete';
import DragIndicator from '@mui/icons-material/DragIndicator';
import IconButton from '@mui/material/IconButton';
import { closestCenter, DndContext, PointerSensor, useSensor, useSensors } from '@dnd-kit/core';
import { SortableContext, verticalListSortingStrategy } from '@dnd-kit/sortable';
import { useProcessBuilderStore } from '../store/useProcessBuilderStore';
import { ConditionBuilder } from './ConditionBuilder';
import { controlPalette } from './ProcessBuilderPalette';
import { normalizeTransitionValue, TransitionValueField } from './TransitionValueField';
import type {
  BuilderStep,
  BuilderTransition,
  BuilderValidation,
  BuilderValidationType,
  BuilderVariable,
  BuilderOptionFeatureConfiguration,
} from '../types/processBuilderTypes';
import { processBuilderTokens as tokens } from './processBuilderTokens';
import { DEFAULT_VALIDATION_MESSAGES, validationUsesCustomMessage } from '../validationDefaults';
import { useQuery } from '@tanstack/react-query';
import { wfCategoryApi, type WfCategoryRecord } from '@modules/workflow/api/wfCategoryApi';
import {
  wfActivityTypeApi,
  wfOperatorApi,
  wfPriorityApi,
  wfProcessTypeApi,
} from '@modules/workflow/api/workflowSetupApis';
import { wfPerformerApi } from '@modules/workflow/api/wfPerformerApi';
import { AppLookupGridField } from '@shared/components/fields/AppLookupGridField';
import { AppLookupField } from '@shared/components/fields/AppLookupField';
import { SortableBuilderItem } from './SortableBuilderItem';

const categoryLookupColumns = [
  { field: 'code', header: 'Code', width: 110 },
  { field: 'name', header: 'Name', flex: 1 },
] as const;

const requestOptionControlTypes = new Set(['dropdown-manual', 'checkboxlist', 'radiobuttonlist']);
const emptyOptionFeatures = (): BuilderOptionFeatureConfiguration => ({
  requireFileUpload: false,
  sendAlertMessage: false,
  alertMessage: '',
  performerIds: [],
  showOtherControls: false,
  visibleControlIds: [],
});
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

const transitionOperatorFromLabel = (label: string): BuilderTransition['operator'] => {
  const normalized = label.replace(/\s/g, '').toLocaleLowerCase();
  if (normalized === '!=' || normalized.includes('notequal')) return '!=';
  if (normalized === '>=' || normalized.includes('greaterthanorequal')) return '>=';
  if (normalized === '<=' || normalized.includes('lessthanorequal')) return '<=';
  if (normalized === '>' || normalized.includes('greaterthan')) return '>';
  if (normalized === '<' || normalized.includes('lessthan')) return '<';
  if (normalized.includes('contains')) return 'contains';
  if (normalized.includes('isempty')) return 'isEmpty';
  if (normalized.includes('between')) return 'between';
  return '=';
};

const fetchCategoryPage = async ({
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
  const categories = await wfCategoryApi.list(signal);
  const normalizedSearch = search.trim().toLocaleLowerCase();
  const filtered = normalizedSearch
    ? categories.filter((category) =>
        `${category.code ?? ''} ${category.name ?? ''}`
          .toLocaleLowerCase()
          .includes(normalizedSearch)
      )
    : categories;
  const start = (pageNumber - 1) * pageSize;
  return {
    data: filtered.slice(start, start + pageSize),
    pageNumber,
    totalPages: Math.max(1, Math.ceil(filtered.length / pageSize)),
    totalRecords: filtered.length,
  };
};

const SettingsTitle = ({
  title,
  dirty = false,
  isNew = false,
}: {
  title: string;
  dirty?: boolean;
  isNew?: boolean;
}) => (
  <Stack
    direction="row"
    spacing={1}
    sx={{
      position: 'sticky',
      top: -16,
      zIndex: 2,
      minHeight: 34,
      py: '4px',
      bgcolor: '#fff',
      borderBottom: `1px solid ${tokens.border}`,
      alignItems: 'center',
    }}
  >
    <Typography
      component="h2"
      sx={{ flex: 1, fontSize: tokens.fontSize.heading, fontWeight: 700, color: tokens.text }}
    >
      {title}
    </Typography>
    {isNew && <Chip size="small" label="New record" sx={{ height: 20, bgcolor: '#eeeeee' }} />}
    {dirty && (
      <Chip
        size="small"
        label="Unsaved changes"
        sx={{ bgcolor: '#fff3cd', color: '#7a4b00', border: '1px solid #f0c36d', height: 20 }}
      />
    )}
  </Stack>
);

const settingsGroupSx = {
  display: 'grid',
  gap: 0.5,
  p: 0.75,
  border: `1px solid ${tokens.border}`,
  borderRadius: `${tokens.radius}px`,
  bgcolor: '#f9fafb',
};

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

const settingsSwitchGridSx = {
  ...settingsGroupSx,
  gridTemplateColumns: 'repeat(2, minmax(0, 1fr))',
  gap: '6px 10px',
  '& .MuiFormControlLabel-root': { m: 0, minWidth: 0 },
  '& .MuiFormControlLabel-label': { fontSize: tokens.fontSize.secondary },
  '& .MuiSwitch-root': { width: 24, height: 14, p: 0, mr: '4px' },
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

const switchRowSx = { m: 0, minHeight: 28 };

const sectionSx = {
  boxShadow: 'none',
  border: `1px solid ${tokens.border}`,
  borderRadius: `${tokens.radius}px !important`,
  '&:before': { display: 'none' },
  '& .MuiAccordionSummary-root': { minHeight: 38 },
};
const Section = ({
  title,
  children,
  expanded = false,
}: {
  title: string;
  children: React.ReactNode;
  expanded?: boolean;
}) => (
  <Accordion defaultExpanded={expanded} sx={sectionSx}>
    <AccordionSummary expandIcon={<ExpandMore />}>
      <Typography sx={{ fontSize: tokens.fontSize.body, fontWeight: 700 }}>{title}</Typography>
    </AccordionSummary>
    <AccordionDetails>{children}</AccordionDetails>
  </Accordion>
);

function ValidationRules({
  values,
  onChange,
  disabled = false,
}: {
  values: BuilderValidation[];
  onChange: (values: BuilderValidation[]) => void;
  disabled?: boolean;
}) {
  type ConditionalValidationField = 'value' | 'secondaryValue' | 'operator' | 'expression' | 'mask';
  const fieldsByType: Record<BuilderValidationType, readonly ConditionalValidationField[]> = {
    required: [],
    regex: ['expression'],
    pattern: ['expression'],
    minLength: ['value'],
    maxLength: ['value'],
    exactLength: ['value'],
    length: ['value'],
    minValue: ['value'],
    maxValue: ['value'],
    range: ['value', 'secondaryValue'],
    compare: ['operator', 'value'],
    comparison: ['operator', 'value'],
    crossField: ['operator', 'value'],
    expression: ['expression', 'operator', 'value'],
    custom: ['expression', 'operator', 'value'],
    mask: ['mask'],
    inputMask: ['mask'],
    startsWith: ['value'],
    endsWith: ['value'],
    contains: ['value'],
    fileExtensions: ['value'],
    fileSize: ['value'],
    maxFiles: ['value'],
    minSelected: ['value'],
    maxSelected: ['value'],
    email: [],
    url: [],
    phone: [],
    saudiMobile: ['mask'],
    saudiNationalId: [],
    saudiIban: [],
    taxNumber: [],
    passport: [],
  };
  const normalizeRules = (rules: BuilderValidation[]) => rules.map((rule, index) => ({
    ...rule,
    message: rule.message.trim() || DEFAULT_VALIDATION_MESSAGES[rule.type],
    sortOrder: (index + 1) * 10,
  }));
  const update = (id: string, patch: Partial<BuilderValidation>) =>
    onChange(normalizeRules(values.map((rule) => (rule.id === id ? { ...rule, ...patch } : rule))));
  const validationTypes: ReadonlyArray<{ value: BuilderValidationType; label: string }> = [
    ['required', 'Required'],
    ['minLength', 'Minimum length'],
    ['maxLength', 'Maximum length'],
    ['exactLength', 'Exact length'],
    ['length', 'Length'],
    ['minValue', 'Minimum value'],
    ['maxValue', 'Maximum value'],
    ['range', 'Range'],
    ['regex', 'Regular expression'],
    ['pattern', 'Pattern'],
    ['startsWith', 'Starts with'],
    ['endsWith', 'Ends with'],
    ['contains', 'Contains'],
    ['email', 'Email'],
    ['url', 'URL'],
    ['phone', 'Phone'],
    ['saudiMobile', 'Saudi mobile'],
    ['saudiNationalId', 'Saudi National ID'],
    ['saudiIban', 'Saudi IBAN'],
    ['taxNumber', 'Tax number'],
    ['passport', 'Passport number'],
    ['fileExtensions', 'Allowed file extensions'],
    ['fileSize', 'File size'],
    ['maxFiles', 'Maximum number of files'],
    ['minSelected', 'Minimum selected items'],
    ['maxSelected', 'Maximum selected items'],
    ['compare', 'Compare'],
    ['comparison', 'Comparison'],
    ['expression', 'Expression'],
    ['custom', 'Custom expression'],
    ['crossField', 'Cross-field validation'],
    ['mask', 'Mask'],
    ['inputMask', 'Input mask'],
  ].map(([value, label]) => ({ value: value as BuilderValidationType, label }));
  const changeType = (id: string, type: BuilderValidationType) => {
    const current = values.find((rule) => rule.id === id);
    if (!current) return;
    const visible = new Set(fieldsByType[type]);
    const previouslyVisible = new Set(fieldsByType[current.type] ?? []);
    const message = validationUsesCustomMessage(type) && validationUsesCustomMessage(current.type) && current.message.trim()
      ? current.message
      : DEFAULT_VALIDATION_MESSAGES[type];
    update(id, {
      type,
      message,
      value: visible.has('value') && previouslyVisible.has('value') ? current.value : '',
      secondaryValue:
        (visible.has('secondaryValue') && previouslyVisible.has('secondaryValue')) ||
        (visible.has('expression') && previouslyVisible.has('expression'))
          ? current.secondaryValue
          : '',
      operator:
        visible.has('operator') && previouslyVisible.has('operator') ? current.operator : '',
      mask: visible.has('mask') && previouslyVisible.has('mask') ? current.mask : '',
    });
  };
  const add = () =>
    onChange(normalizeRules([
      ...values,
      {
        id: crypto.randomUUID(),
        type: 'required',
        value: '',
        secondaryValue: '',
        operator: '',
        mask: '',
        message: DEFAULT_VALIDATION_MESSAGES.required,
        severity: 'Error',
        sortOrder: (values.length + 1) * 10,
        active: true,
      },
    ]));
  return (
    <Box sx={{ pt: 1.5, borderTop: '1px solid #e5e7eb' }}>
      <Stack direction="row" sx={{ alignItems: 'center' }}>
        <Typography sx={{ flex: 1, fontSize: tokens.fontSize.body, fontWeight: 600 }}>
          Validation Rules
        </Typography>
        <Button size="small" disabled={disabled} onClick={add}>
          + Add
        </Button>
      </Stack>
      <Stack spacing={1.25} sx={{ mt: 1 }}>
        {values.map((rule) => {
          const visible = new Set(fieldsByType[rule.type] ?? []);
          return (
            <Box
              key={rule.id}
              sx={{
                display: 'grid',
                gridTemplateColumns: 'minmax(0, 1fr) minmax(100px, .75fr)',
                gap: 1,
                p: 1.25,
                border: '1px solid #dfe3ea',
                bgcolor: '#fff',
              }}
            >
              <TextField
                select
                size="small"
                label="Validation Type"
                value={rule.type}
                onChange={(event) =>
                  changeType(rule.id, event.target.value as BuilderValidationType)
                }
              >
                {validationTypes.map((type) => (
                  <MenuItem key={type.value} value={type.value}>
                    {type.label}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                select
                size="small"
                label="Severity"
                value={rule.severity}
                onChange={(event) =>
                  update(rule.id, { severity: event.target.value as BuilderValidation['severity'] })
                }
              >
                {['Error', 'Warning', 'Information'].map((severity) => (
                  <MenuItem key={severity} value={severity}>
                    {severity}
                  </MenuItem>
                ))}
              </TextField>
              {visible.has('value') && (
                <TextField
                  size="small"
                  label={rule.type === 'range' ? 'Minimum value' : 'Value'}
                  value={rule.value}
                  onChange={(event) => update(rule.id, { value: event.target.value })}
                />
              )}
              {visible.has('secondaryValue') && (
                <TextField
                  size="small"
                  label="Maximum value"
                  value={rule.secondaryValue}
                  onChange={(event) => update(rule.id, { secondaryValue: event.target.value })}
                />
              )}
              {visible.has('operator') && (
                <TextField
                  size="small"
                  label="Operator"
                  value={rule.operator}
                  onChange={(event) => update(rule.id, { operator: event.target.value })}
                />
              )}
              {visible.has('expression') && (
                <TextField
                  size="small"
                  label="Validation expression"
                  value={rule.secondaryValue}
                  onChange={(event) => update(rule.id, { secondaryValue: event.target.value })}
                  sx={{ gridColumn: '1 / -1' }}
                />
              )}
              {visible.has('mask') && (
                <TextField
                  size="small"
                  label="Input mask"
                  value={rule.mask}
                  onChange={(event) => update(rule.id, { mask: event.target.value })}
                  sx={{ gridColumn: '1 / -1' }}
                />
              )}
              {validationUsesCustomMessage(rule.type) && <TextField
                required
                size="small"
                label="Error message"
                value={rule.message}
                onChange={(event) => update(rule.id, { message: event.target.value })}
                sx={{ gridColumn: '1 / -1' }}
              />}
              <Stack direction="row" sx={{ gridColumn: '1 / -1', alignItems: 'center', justifyContent: 'space-between' }}>
                <FormControlLabel
                  control={
                    <Switch
                      size="small"
                      sx={compactSwitchSx}
                      checked={rule.active}
                      onChange={(_, active) => update(rule.id, { active })}
                    />
                  }
                  label="Active"
                />
                <IconButton
                  color="error"
                  size="small"
                  aria-label="Delete validation"
                  onClick={() => onChange(normalizeRules(values.filter((item) => item.id !== rule.id)))}
                >
                  <Delete fontSize="small" />
                </IconButton>
              </Stack>
            </Box>
          );
        })}
        {values.length === 0 && (
          <Typography
            color="text.secondary"
            sx={{ py: 2, textAlign: 'center', fontSize: tokens.fontSize.caption }}
          >
            No validation rules yet.
          </Typography>
        )}
      </Stack>
    </Box>
  );
}

function TransitionRules({
  values,
  variables,
  steps,
  onAdd,
  onUpdate,
  onRemove,
}: {
  values: BuilderTransition[];
  variables: BuilderVariable[];
  steps: BuilderStep[];
  onAdd: () => void;
  onUpdate: (id: string, values: Partial<BuilderTransition>) => void;
  onRemove: (id: string) => void;
}) {
  return (
    <Box sx={{ pt: '12px', borderTop: `1px solid ${tokens.border}` }}>
      <Stack direction="row" sx={{ alignItems: 'center' }}>
        <Typography sx={{ flex: 1, fontSize: tokens.fontSize.body, fontWeight: 600 }}>
          Transitions ({values.length})
        </Typography>
        <Button size="small" onClick={onAdd}>
          + Add
        </Button>
      </Stack>
      <Stack spacing="10px" sx={{ mt: '8px' }}>
        {values.map((transition) => {
          const variable = variables.find((item) => item.id === transition.variableId);
          return (
            <Box
              key={transition.id}
              sx={{ p: '10px', border: `1px solid ${tokens.border}`, bgcolor: '#fff' }}
            >
              <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '8px' }}>
                <TextField
                  select
                  size="small"
                  label="Variable"
                  value={transition.variableId}
                  onChange={(event) => {
                    const variableId = event.target.value;
                    const dataType = variables.find((item) => item.id === variableId)?.dataType;
                    onUpdate(transition.id, {
                      variableId,
                      value: normalizeTransitionValue(transition.value, dataType),
                    });
                  }}
                >
                  <MenuItem value="">Variable</MenuItem>
                  {variables.map((variable) => (
                    <MenuItem key={variable.id} value={variable.id}>
                      {variable.name}
                    </MenuItem>
                  ))}
                </TextField>
                <TextField
                  select
                  size="small"
                  label="Operator"
                  value={transition.operator}
                  onChange={(event) =>
                    onUpdate(transition.id, {
                      operator: event.target.value as BuilderTransition['operator'],
                    })
                  }
                >
                  {['=', '!=', '>', '<', '>=', '<=', 'contains', 'isEmpty'].map((operator) => (
                    <MenuItem key={operator} value={operator}>
                      {operator}
                    </MenuItem>
                  ))}
                </TextField>
                <TransitionValueField
                  label="Value"
                  dataType={variable?.dataType}
                  value={transition.value}
                  disabled={transition.operator === 'isEmpty'}
                  onChange={(value) => onUpdate(transition.id, { value })}
                />
                <TextField
                  select
                  size="small"
                  label="Target Step"
                  value={transition.targetStepId}
                  onChange={(event) =>
                    onUpdate(transition.id, { targetStepId: event.target.value })
                  }
                >
                  <MenuItem value="">Target Step</MenuItem>
                  {steps.map((step) => (
                    <MenuItem key={step.id} value={step.id}>
                      {step.name}
                    </MenuItem>
                  ))}
                </TextField>
              </Box>
              <Stack direction="row" sx={{ mt: '8px', alignItems: 'center' }}>
                <FormControlLabel
                  control={
                    <Switch
                      size="small"
                      sx={compactSwitchSx}
                      checked={transition.active}
                      onChange={(_, active) => onUpdate(transition.id, { active })}
                    />
                  }
                  label="Active"
                />
                <Box sx={{ flex: 1 }} />
                <IconButton
                  size="small"
                  color="error"
                  aria-label="Delete transition"
                  onClick={() => onRemove(transition.id)}
                >
                  <Delete />
                </IconButton>
              </Stack>
            </Box>
          );
        })}
        {values.length === 0 && (
          <Typography
            color="text.secondary"
            sx={{ py: '12px', textAlign: 'center', fontSize: tokens.fontSize.caption }}
          >
            No transitions yet.
          </Typography>
        )}
      </Stack>
    </Box>
  );
}

export function ProcessBuilderSettingsPanel() {
  const s = useProcessBuilderStore();
  const d = s.document;
  const selected = s.selected;
  const [activityValidationControlId, setActivityValidationControlId] = React.useState('');
  const [showAllRequestOptions, setShowAllRequestOptions] = React.useState(false);
  const [showAllRequestValidations, setShowAllRequestValidations] = React.useState(false);
  const [showAllRequestTransitions, setShowAllRequestTransitions] = React.useState(false);
  const optionSensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 4 } }));
  const priorities = useQuery({
    queryKey: ['workflow', 'builder-priorities'],
    queryFn: ({ signal }) => wfPriorityApi.list(signal),
  });
  const processTypes = useQuery({
    queryKey: ['workflow', 'builder-process-types'],
    queryFn: ({ signal }) => wfProcessTypeApi.list(signal),
  });
  const activityTypes = useQuery({
    queryKey: ['workflow', 'builder-activity-type-options'],
    queryFn: ({ signal }) => wfActivityTypeApi.list(signal),
  });
  const performers = useQuery({
    queryKey: ['workflow', 'builder-performer-options'],
    queryFn: ({ signal }) => wfPerformerApi.list(signal),
  });
  const operators = useQuery({
    queryKey: ['workflow', 'builder-operator-options'],
    queryFn: ({ signal }) => wfOperatorApi.list(signal),
  });
  const text = (
    label: string,
    value: string | number,
    onChange: (value: string) => void,
    type = 'text'
  ) => (
    <TextField
      fullWidth
      size="small"
      label={label}
      value={value}
      type={type}
      onChange={(e) => onChange(e.target.value)}
    />
  );
  if (selected.kind === 'workspace') {
    const workspaceSettings: Record<number, { title: string; message: string }> = {
      1: { title: 'Variables Settings', message: 'Add or select a variable to edit its settings.' },
      2: {
        title: 'Request Form Settings',
        message: 'Add or select a request field to edit its settings.',
      },
      3: { title: 'Steps Settings', message: 'Add or select a step to edit its settings.' },
      4: {
        title: 'Activities Settings',
        message: 'Add or select an activity to edit its settings.',
      },
      5: {
        title: 'Activity Form Settings',
        message: 'Add or select an activity field to edit its settings.',
      },
      6: {
        title: 'Transitions Settings',
        message: 'Add or select a transition to edit its settings.',
      },
      7: {
        title: 'Diagram Settings',
        message: 'Select a process item in the diagram to edit its settings.',
      },
    };
    const content = workspaceSettings[selected.tab] ?? {
      title: 'Settings',
      message: 'Select an item to edit its settings.',
    };
    return (
      <Stack spacing="8px" sx={{ p: '10px' }}>
        <SettingsTitle title={content.title} dirty={s.dirty} />
        <Typography color="text.secondary" sx={{ fontSize: tokens.fontSize.secondary }}>
          {content.message}
        </Typography>
      </Stack>
    );
  }
  if (selected.kind === 'process')
    return (
      <Stack spacing="8px" sx={{ p: '10px', minHeight: '100%' }}>
        <SettingsTitle title="Process Information" isNew />
        <TextField fullWidth size="small" label="Code" value={d.code} disabled />
        {text('Name', d.name, (name) => s.updateProcess({ name }))}
        <TextField
          fullWidth
          multiline
          minRows={3}
          size="small"
          label="Description"
          value={d.description}
          onChange={(event) => s.updateProcess({ description: event.target.value })}
        />
        <AppLookupGridField<WfCategoryRecord>
          name="categoryId"
          label="Category"
          value={Number(d.categoryId) || null}
          onChange={(categoryId) =>
            s.updateProcess({ categoryId: categoryId == null ? '' : String(categoryId) })
          }
          required
          columns={[...categoryLookupColumns]}
          queryKey={['workflow', 'builder-category-lookup']}
          fetchPage={fetchCategoryPage}
          fetchById={async (categoryId) =>
            wfCategoryApi.getById(Number(categoryId)).catch(() => null)
          }
          valueField="recId"
          labelField="name"
          pageSize={25}
        />
        <TextField
          select
          size="small"
          label="Priority"
          value={d.priorityId ?? ''}
          onChange={(event) => s.updateProcess({ priorityId: event.target.value })}
        >
          <MenuItem value="">Select priority</MenuItem>
          {(priorities.data ?? []).map((priority) => (
            <MenuItem key={priority.recId} value={String(priority.recId)}>
              {priority.code} - {priority.name}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Process Type"
          value={d.processType ?? ''}
          onChange={(event) => s.updateProcess({ processType: event.target.value })}
        >
          <MenuItem value="">Select process type</MenuItem>
          {(processTypes.data ?? []).map((processType) => (
            <MenuItem key={processType.recId} value={String(processType.recId)}>
              {processType.code} - {processType.name}
            </MenuItem>
          ))}
        </TextField>
        {text(
          'Score',
          d.score ?? 100,
          (value) => s.updateProcess({ score: Number(value) }),
          'number'
        )}
        <Box sx={settingsSwitchGridSx}>
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={d.active}
                onChange={(_, active) => s.updateProcess({ active })}
              />
            }
            label="Active"
          />
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={d.canRepeat ?? false}
                onChange={(_, canRepeat) => s.updateProcess({ canRepeat })}
              />
            }
            label="Can Repeat"
          />
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={d.mandatoryDocs ?? false}
                onChange={(_, mandatoryDocs) => s.updateProcess({ mandatoryDocs })}
              />
            }
            label="Mandatory Docs"
          />
        </Box>
        <Box sx={{ pt: '12px', borderTop: `1px solid ${tokens.border}` }}>
          <Stack direction="row" sx={{ alignItems: 'center', minHeight: 28 }}>
            <Typography sx={{ flex: 1, fontSize: tokens.fontSize.body, fontWeight: 600 }}>
              Variables
            </Typography>
            {s.dirty && (
              <Chip
                size="small"
                variant="outlined"
                label="Unsaved"
                sx={{
                  mr: 1,
                  height: 22,
                  color: '#7a4b00',
                  bgcolor: '#fff3cd',
                  borderColor: '#f0c36d',
                }}
              />
            )}
            <Button size="small" onClick={s.addVariable}>
              + Add
            </Button>
          </Stack>
          {d.id === 'new' && (
            <Typography sx={{ py: '8px', color: '#9a4f00', fontSize: tokens.fontSize.caption }}>
              Save the Process first to enable variable creation (ProcessId required).
            </Typography>
          )}
          <Stack spacing="8px">
            {d.variables.map((variable) => (
              <Box
                key={variable.id}
                sx={{ p: '8px', border: `1px solid ${tokens.warning}`, bgcolor: '#fff' }}
              >
                <Box
                  sx={{
                    display: 'grid',
                    gridTemplateColumns: '88px minmax(0, 1fr) 24px',
                    gap: '6px',
                  }}
                >
                  <TextField
                    size="small"
                    label="Code"
                    value={variable.code}
                    placeholder="Managed by number sequence"
                    disabled
                  />
                  <TextField
                    size="small"
                    value={variable.name}
                    onChange={(event) =>
                      s.updateVariable(variable.id, { name: event.target.value })
                    }
                  />
                  <IconButton
                    color="error"
                    size="small"
                    aria-label="Delete variable"
                    onClick={() => s.removeVariable(variable.id)}
                  >
                    <Delete />
                  </IconButton>
                </Box>
                <Box
                  sx={{
                    display: 'grid',
                    gridTemplateColumns: 'minmax(0, 1fr) 24px auto',
                    gap: '6px',
                    mt: '6px',
                    alignItems: 'center',
                  }}
                >
                  <TextField
                    select
                    size="small"
                    label="Data Type"
                    value={variable.dataType}
                    onChange={(event) =>
                      s.updateVariable(variable.id, {
                        dataType: event.target.value as typeof variable.dataType,
                      })
                    }
                  >
                    {['text', 'number', 'boolean', 'date', 'object'].map((value) => (
                      <MenuItem key={value} value={value}>
                        {value}
                      </MenuItem>
                    ))}
                  </TextField>
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
                </Box>
              </Box>
            ))}
          </Stack>
        </Box>
      </Stack>
    );
  if (selected.kind === 'variable') {
    const x = d.variables.find((v) => v.id === selected.id);
    if (!x) return null;
    return (
      <Stack spacing="8px" sx={{ p: '10px' }}>
        <SettingsTitle title="Variable" dirty={s.dirty} isNew />
        <TextField
          size="small"
          label="Code"
          value={x.code}
          placeholder="Managed by number sequence"
          disabled
        />
        {text('Name', x.name, (name) => s.updateVariable(x.id, { name }))}
        {text('Description', x.description, (description) =>
          s.updateVariable(x.id, { description })
        )}
        <TextField
          select
          size="small"
          label="Data type"
          value={x.dataType}
          onChange={(e) =>
            s.updateVariable(x.id, { dataType: e.target.value as typeof x.dataType })
          }
        >
          {['text', 'number', 'boolean', 'date', 'object'].map((v) => (
            <MenuItem key={v} value={v}>
              {v}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          size="small"
          type="number"
          label="Sort order"
          value={x.sortOrder}
          slotProps={{ htmlInput: { min: 0, max: 255, step: 1 } }}
          onChange={(event) => s.updateVariable(x.id, { sortOrder: Number(event.target.value) })}
        />
        <Box sx={settingsSwitchGridSx}>
          <FormControlLabel
            sx={switchRowSx}
            control={
              <Switch
                size="small"
                checked={x.active}
                onChange={(_, active) => s.updateVariable(x.id, { active })}
              />
            }
            label="Active"
          />
        </Box>
      </Stack>
    );
  }
  if (selected.kind === 'step') {
    const x = d.steps.find((v) => v.id === selected.id);
    if (!x) return null;
    return (
      <Stack spacing="8px" sx={{ p: '10px', minHeight: '100%' }}>
        <SettingsTitle title="Step Settings" dirty={s.dirty} />
        <TextField
          size="small"
          label="Step Code"
          value={x.code}
          placeholder="Generated on save"
          disabled
        />
        {text('Step Name *', x.name, (name) => s.updateStep(x.id, { name }))}
        <Box sx={{ display: 'grid', gridTemplateColumns: 'minmax(0, 1fr) minmax(0, 1fr)', gap: 1 }}>
          {text(
            'Order',
            x.order,
            (value) => s.updateStep(x.id, { order: Number(value) }),
            'number'
          )}
          {text(
            'Auto passing hours',
            x.autoPassingHours,
            (value) => s.updateStep(x.id, { autoPassingHours: Number(value) }),
            'number'
          )}
        </Box>
        {text('Score', x.score, (value) => s.updateStep(x.id, { score: Number(value) }), 'number')}
        <Box sx={settingsSwitchGridSx}>
          <FormControlLabel
            sx={switchRowSx}
            control={
              <Switch
                size="small"
                checked={x.allMandatory}
                onChange={(_, allMandatory) => s.updateStep(x.id, { allMandatory })}
              />
            }
            label="All Mandatory"
          />
          <FormControlLabel
            sx={switchRowSx}
            control={
              <Switch
                size="small"
                checked={x.active}
                onChange={(_, active) => s.updateStep(x.id, { active })}
              />
            }
            label="Active Step"
          />
          <FormControlLabel
            sx={switchRowSx}
            control={
              <Switch
                size="small"
                checked={x.systemField}
                onChange={(_, systemField) => s.updateStep(x.id, { systemField })}
              />
            }
            label="System Field"
          />
        </Box>
        <Section title="Step Condition">
          <ConditionBuilder
            value={x.condition}
            variables={d.variables}
            onChange={(condition) => s.updateStep(x.id, { condition })}
          />
        </Section>
        <Button variant="contained" disabled sx={{ alignSelf: 'stretch', mt: 'auto' }}>
          Save Steps to DB
        </Button>
      </Stack>
    );
  }
  if (selected.kind === 'activity') {
    const x = d.steps
      .find((v) => v.id === selected.stepId)
      ?.activities.find((v) => v.id === selected.id);
    if (!x) return null;
    const validationControl =
      x.controls.find((control) => control.id === activityValidationControlId) ?? x.controls[0];
    const activityTransitions = d.transitions.filter(
      (transition) => transition.triggerSource === 'activity' && transition.triggerId === x.id
    );
    return (
      <Stack spacing="8px" sx={{ p: '10px' }}>
        <SettingsTitle title="Activity Settings" dirty={s.dirty} isNew={!/^\d+$/.test(x.id)} />
        <TextField
          size="small"
          label="Activity code"
          value={x.code}
          placeholder="Managed by number sequence"
          disabled
        />
        {text('Activity name', x.name, (name) => s.updateActivity(selected.stepId, x.id, { name }))}
        <Stack spacing={1}>
          <AppLookupField
            name={`settings-performerId-${x.id}`}
            label="Performer"
            value={Number(x.performer) || undefined}
            options={(performers.data ?? []).map((item) => ({
              id: item.recId,
              code: item.code ?? '',
              name: item.name ?? '',
            }))}
            onChange={(value) =>
              s.updateActivity(selected.stepId, x.id, {
                performer: value == null ? '' : String(value),
              })
            }
            required
            displayMode="select"
          />
        </Stack>
        <Stack spacing={1.25}>
          <AppLookupField
            name={`settings-activityTypeId-${x.id}`}
            label="Activity Type"
            value={Number(x.activityTypeId) || undefined}
            options={(activityTypes.data ?? []).map((item) => ({
              id: item.recId,
              code: item.code ?? '',
              name: item.name ?? '',
            }))}
            onChange={(value, option) =>
              s.updateActivity(selected.stepId, x.id, {
                activityTypeId: value == null ? '' : String(value),
                type: option && !Array.isArray(option) ? builderTypeFromLabel(option.name) : x.type,
              })
            }
            required
            displayMode="select"
          />
        </Stack>
        <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1 }}>
          <TextField
            size="small"
            type="number"
            label="Score"
            value={x.score}
            onChange={(event) =>
              s.updateActivity(selected.stepId, x.id, { score: Number(event.target.value) })
            }
          />
          <TextField
            size="small"
            type="number"
            label="Auto passing hours"
            value={x.autoPassingHours}
            disabled={!x.autoPassEnabled}
            onChange={(event) =>
              s.updateActivity(selected.stepId, x.id, {
                autoPassingHours: Number(event.target.value),
              })
            }
          />
        </Box>
        {text('Notification emails', x.config.notifyEmails, (notifyEmails) =>
          s.updateActivity(selected.stepId, x.id, { config: { ...x.config, notifyEmails } })
        )}
        <Box sx={settingsSwitchGridSx}>
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={x.active}
                onChange={(_, active) => s.updateActivity(selected.stepId, x.id, { active })}
              />
            }
            label="Active"
          />
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={x.required}
                onChange={(_, required) => s.updateActivity(selected.stepId, x.id, { required })}
              />
            }
            label="Required"
          />
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={x.autoPassEnabled}
                onChange={(_, autoPassEnabled) =>
                  s.updateActivity(selected.stepId, x.id, { autoPassEnabled })
                }
              />
            }
            label="Auto pass enabled"
          />
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={x.mandatoryDocs}
                onChange={(_, mandatoryDocs) =>
                  s.updateActivity(selected.stepId, x.id, { mandatoryDocs })
                }
              />
            }
            label="Mandatory documents"
          />
        </Box>
        {x.type === 'api' && (
          <Section title="API Action">
            <Stack spacing={1}>
              <TextField
                select
                size="small"
                label="Method"
                value={x.config.apiMethod}
                onChange={(e) =>
                  s.updateActivity(selected.stepId, x.id, {
                    config: { ...x.config, apiMethod: e.target.value as typeof x.config.apiMethod },
                  })
                }
              >
                {['GET', 'POST', 'PUT', 'DELETE'].map((method) => (
                  <MenuItem key={method} value={method}>
                    {method}
                  </MenuItem>
                ))}
              </TextField>
              {text('API URL', x.config.apiUrl, (apiUrl) =>
                s.updateActivity(selected.stepId, x.id, { config: { ...x.config, apiUrl } })
              )}
            </Stack>
          </Section>
        )}
        {x.controls.length > 0 && (
          <TextField
            select
            size="small"
            label="Validation control"
            value={validationControl?.id ?? ''}
            onChange={(event) => setActivityValidationControlId(event.target.value)}
          >
            {x.controls.map((control) => (
              <MenuItem key={control.id} value={control.id}>
                {control.label || control.code || 'Unnamed control'}
              </MenuItem>
            ))}
          </TextField>
        )}
        <ValidationRules
          values={validationControl?.validations ?? []}
          disabled={!validationControl}
          onChange={(validations) => {
            if (validationControl) {
              s.updateActivityControl(selected.stepId, x.id, validationControl.id, { validations });
            }
          }}
        />
        <TransitionRules
          values={activityTransitions}
          variables={d.variables}
          steps={d.steps}
          onAdd={() => s.addTransition({ triggerSource: 'activity', triggerId: x.id })}
          onUpdate={s.updateTransition}
          onRemove={s.removeTransition}
        />
      </Stack>
    );
  }
  if (selected.kind === 'requestControl') {
    const control = d.requestControls.find((x) => x.id === selected.id);
    if (!control) return null;
    const update = (values: Partial<typeof control>) => s.updateRequestControl(control.id, values);
    const controlTransitions = d.transitions.filter(
      (transition) =>
        transition.triggerSource === 'requestControl' && transition.triggerId === control.id
    );
    const validationSummary = control.validations.map((rule) =>
      rule.type.replace(/([A-Z])/g, ' $1').toLocaleLowerCase()
    );
    const transitionSummary = controlTransitions.map((transition) => {
      const variable = d.variables.find((item) => item.id === transition.variableId);
      const target = d.steps.find((step) => step.id === transition.targetStepId);
      const condition = `${variable?.name || 'Variable'} ${transition.operator}${
        transition.operator === 'isEmpty' ? '' : ` ${transition.value || '…'}`
      }`;
      return `${condition} → ${target?.name || 'Unassigned step'}`;
    });
    const optionFeaturesAt = (index: number) => ({
      ...emptyOptionFeatures(),
      ...(control.optionFeatureConfigurations?.[index] ?? {}),
    });
    const updateOptionFeatures = (index: number, patch: Partial<BuilderOptionFeatureConfiguration>) =>
      update({
        optionFeatureConfigurations: control.options.map((_, itemIndex) =>
          itemIndex === index
            ? { ...optionFeaturesAt(itemIndex), ...patch }
            : optionFeaturesAt(itemIndex)
        ),
      });
    if (s.controlSettingsPane === 'options' && requestOptionControlTypes.has(control.type)) {
      return (
        <Stack spacing="8px" sx={{ p: '10px' }}>
          <SettingsTitle title={`Request Control Options (${control.options.length})`} dirty={s.dirty} />
          <Stack spacing="6px" sx={settingsGroupSx}>
            <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
              <Typography sx={{ fontSize: tokens.fontSize.body, fontWeight: 600 }}>
                Options
              </Typography>
              <Button
                size="small"
                onClick={() =>
                  update({
                    options: [...control.options, `Option ${control.options.length + 1}`],
                    optionScores: [...(control.optionScores ?? []), 0],
                    optionFeatureConfigurations: [
                      ...(control.optionFeatureConfigurations ?? control.options.map(() => emptyOptionFeatures())),
                      emptyOptionFeatures(),
                    ],
                  })
                }
              >
                + Add option
              </Button>
            </Stack>
            {control.options.length === 0 && (
              <Typography color="text.secondary" sx={{ fontSize: tokens.fontSize.caption }}>
                Add at least one selectable option.
              </Typography>
            )}
            <DndContext
              sensors={optionSensors}
              collisionDetection={closestCenter}
              onDragEnd={({ active, over }) => {
                if (!over || active.id === over.id) return;
                s.reorderRequestControlOptions(control.id, Number(active.id), Number(over.id));
              }}
            >
              <SortableContext
                items={control.options.map((_, index) => String(index))}
                strategy={verticalListSortingStrategy}
              >
                <Stack spacing="6px">
                  {control.options.map((option, index) => (
                    <SortableBuilderItem key={`${index}-${control.options.length}`} id={String(index)}>
                      {(attributes, listeners) => (
                        <Stack spacing="4px">
                          <Stack direction="row" spacing="4px" sx={{ alignItems: 'center' }}>
                            <Box
                              {...attributes}
                              {...listeners}
                              aria-label={`Reorder option ${index + 1}`}
                              sx={{ display: 'flex', color: tokens.textMuted, cursor: 'grab', touchAction: 'none' }}
                            >
                              <DragIndicator fontSize="small" />
                            </Box>
                            <TextField
                              fullWidth
                              size="small"
                              label={`Option ${index + 1}`}
                              value={option}
                              onChange={(event) =>
                                update({
                                  options: control.options.map((item, itemIndex) =>
                                    itemIndex === index ? event.target.value : item
                                  ),
                                })
                              }
                            />
                            <IconButton
                              size="small"
                              color="error"
                              aria-label={`Remove option ${index + 1}`}
                              onClick={() =>
                                update({
                                  options: control.options.filter((_, itemIndex) => itemIndex !== index),
                                  optionScores: (control.optionScores ?? []).filter((_, itemIndex) => itemIndex !== index),
                                  optionFeatureConfigurations: (control.optionFeatureConfigurations ?? []).filter(
                                    (_, itemIndex) => itemIndex !== index
                                  ),
                                })
                              }
                            >
                              <Delete fontSize="small" />
                            </IconButton>
                          </Stack>
                          <Stack direction="row" spacing="4px" sx={{ ml: '28px', mr: '32px' }}>
                            <TextField
                              fullWidth
                              size="small"
                              type="number"
                              label="Score"
                              value={control.optionScores?.[index] ?? 0}
                              onChange={(event) =>
                                update({
                                  optionScores: control.options.map((_, itemIndex) =>
                                    itemIndex === index
                                      ? Number(event.target.value) || 0
                                      : control.optionScores?.[itemIndex] ?? 0
                                  ),
                                })
                              }
                            />
                            <TextField
                              fullWidth
                              size="small"
                              type="number"
                              label="Sort order"
                              value={index + 1}
                              slotProps={{ input: { readOnly: true } }}
                            />
                          </Stack>
                          <Accordion sx={{ ...sectionSx, ml: '28px', mr: '32px' }}>
                            <AccordionSummary expandIcon={<ExpandMore fontSize="small" />}>
                              <Typography sx={{ fontSize: tokens.fontSize.secondary, fontWeight: 700 }}>
                                Feature Configuration
                              </Typography>
                            </AccordionSummary>
                            <AccordionDetails>
                              <Stack spacing="8px">
                                <Box sx={settingsSwitchGridSx}>
                                  <FormControlLabel
                                    control={
                                      <Switch
                                        size="small"
                                        checked={optionFeaturesAt(index).requireFileUpload}
                                        onChange={(_, requireFileUpload) => updateOptionFeatures(index, { requireFileUpload })}
                                      />
                                    }
                                    label="Require File Upload"
                                  />
                                  <FormControlLabel
                                    control={
                                      <Switch
                                        size="small"
                                        checked={optionFeaturesAt(index).sendAlertMessage}
                                        onChange={(_, sendAlertMessage) => updateOptionFeatures(index, {
                                          sendAlertMessage,
                                          ...(!sendAlertMessage ? { alertMessage: '', performerIds: [] } : {}),
                                        })}
                                      />
                                    }
                                    label="Send Alert Message"
                                  />
                                  <FormControlLabel
                                    control={
                                      <Switch
                                        size="small"
                                        checked={optionFeaturesAt(index).showOtherControls}
                                        onChange={(_, showOtherControls) => updateOptionFeatures(index, {
                                          showOtherControls,
                                          ...(!showOtherControls ? { visibleControlIds: [] } : {}),
                                        })}
                                      />
                                    }
                                    label="Show Other Controls"
                                  />
                                </Box>
                                {optionFeaturesAt(index).sendAlertMessage && (
                                  <Stack spacing="8px">
                                    <TextField
                                      fullWidth
                                      size="small"
                                      label="Alert message"
                                      value={optionFeaturesAt(index).alertMessage}
                                      onChange={(event) => updateOptionFeatures(index, { alertMessage: event.target.value })}
                                    />
                                    <Autocomplete
                                      multiple
                                      size="small"
                                      options={performers.data ?? []}
                                      loading={performers.isLoading}
                                      getOptionLabel={(item) => `${item.code ?? ''} - ${item.name ?? ''}`}
                                      value={(performers.data ?? []).filter((item) =>
                                        optionFeaturesAt(index).performerIds.includes(String(item.recId))
                                      )}
                                      onChange={(_, selectedPerformers) => updateOptionFeatures(index, {
                                        performerIds: selectedPerformers.map((item) => String(item.recId)),
                                      })}
                                      renderInput={(params) => (
                                        <TextField {...params} label="Performers" placeholder="Search performers" />
                                      )}
                                    />
                                  </Stack>
                                )}
                                {optionFeaturesAt(index).showOtherControls && <TextField
                                  fullWidth
                                  select
                                  size="small"
                                  label="Show Other Controls"
                                  value={optionFeaturesAt(index).visibleControlIds}
                                  slotProps={{ select: { multiple: true } }}
                                  onChange={(event) => {
                                    const value: unknown = event.target.value;
                                    updateOptionFeatures(index, {
                                      visibleControlIds: Array.isArray(value)
                                        ? value.map(String)
                                        : String(value).split(',').filter(Boolean),
                                    });
                                  }}
                                >
                                  {d.requestControls.filter((item) => item.id !== control.id).map((item) => (
                                    <MenuItem key={item.id} value={item.id}>
                                      {item.label || item.code || 'Unnamed control'}
                                    </MenuItem>
                                  ))}
                                </TextField>}
                              </Stack>
                            </AccordionDetails>
                          </Accordion>
                        </Stack>
                      )}
                    </SortableBuilderItem>
                  ))}
                </Stack>
              </SortableContext>
            </DndContext>
          </Stack>
        </Stack>
      );
    }
    if (s.controlSettingsPane === 'validation') {
      return (
        <Stack spacing="8px" sx={{ p: '10px' }}>
          <SettingsTitle title={`Request Control Validation (${control.validations.length})`} dirty={s.dirty} />
          <ValidationRules
            values={control.validations}
            onChange={(validations) => update({ validations })}
          />
        </Stack>
      );
    }
    if (s.controlSettingsPane === 'transitions') {
      return (
        <Stack spacing="8px" sx={{ p: '10px' }}>
          <SettingsTitle title={`Request Control Transitions (${controlTransitions.length})`} dirty={s.dirty} />
          <TransitionRules
            values={controlTransitions}
            variables={d.variables}
            steps={d.steps}
            onAdd={() => s.addTransition({ triggerSource: 'requestControl', triggerId: control.id })}
            onUpdate={s.updateTransition}
            onRemove={s.removeTransition}
          />
        </Stack>
      );
    }
    return (
      <Stack spacing="8px" sx={{ p: '10px' }}>
        <SettingsTitle title="Request Control" dirty={s.dirty} isNew />
        {text(
          'Control code',
          `RCTL-${String(d.requestControls.indexOf(control) + 1).padStart(4, '0')}`,
          () => undefined
        )}
        {text('Label', control.label, (label) => update({ label }))}
        <Stack direction="row" spacing="8px">
          <TextField
            fullWidth
            size="small"
            type="number"
            label="Score"
            value={control.score}
            onChange={(event) => update({ score: Number(event.target.value) || 0 })}
          />
          <TextField
            fullWidth
            size="small"
            type="number"
            label="Sort order"
            value={control.sortOrder}
            slotProps={{ input: { readOnly: true } }}
          />
          <TextField
            fullWidth
            select
            size="small"
            label="Width"
            value={control.columnSpan ?? 1}
            onChange={(event) => update({ columnSpan: Number(event.target.value) as 1 | 2 | 3 })}
          >
            <MenuItem value={1}>1 column</MenuItem>
            <MenuItem value={2}>2 columns</MenuItem>
            <MenuItem value={3}>Full row</MenuItem>
          </TextField>
        </Stack>
        {control.type === 'label' && <TextField
          size="small" type="color" label="Note color"
          value={control.labelColor || '#7a4b00'}
          onChange={(event) => update({ labelColor: event.target.value })}
          slotProps={{ inputLabel: { shrink: true }, htmlInput: { 'aria-label': 'Note color' } }}
          sx={{ '& input': { minHeight: 30, p: 0.5, cursor: 'pointer' } }}
        />}
        <Box sx={{ ...settingsGroupSx, display: 'flex', alignItems: 'center', py: 0.5 }}>
          <FormControlLabel
            sx={switchRowSx}
            control={
              <Switch
                size="small"
                sx={compactSwitchSx}
                checked={control.visible}
                onChange={(_, visible) => update({ visible })}
              />
            }
            label="Visible"
          />
        </Box>
        <Stack spacing="6px">
          {requestOptionControlTypes.has(control.type) && (
            <Box sx={{ border: `1px solid ${tokens.border}` }}>
              <Button
                fullWidth
                onClick={() => s.openControlSettings({ kind: 'requestControl', id: control.id }, 'options')}
                sx={{ justifyContent: 'flex-start', textTransform: 'none', px: 1, py: 0.5 }}
              >
                <Typography sx={{ fontSize: tokens.fontSize.body, fontWeight: 700 }}>
                  Options ({control.options.length})
                </Typography>
              </Button>
              <Box sx={{ display: 'flex', alignItems: 'center', flexWrap: 'wrap', gap: 0.5, px: 1, pb: 0.75 }}>
                {(showAllRequestOptions ? control.options : control.options.slice(0, 4)).map((option, index) => (
                  <Chip
                    key={`${option}-${index}`}
                    size="small"
                    variant="outlined"
                    label={option || `Option ${index + 1}`}
                    title={option || `Option ${index + 1}`}
                    sx={{ height: 22 }}
                  />
                ))}
                {control.options.length === 0 && (
                  <Typography sx={{ color: tokens.textMuted, fontSize: tokens.fontSize.caption }}>
                    No options
                  </Typography>
                )}
                {control.options.length > 4 && (
                  <Chip
                    size="small"
                    variant="outlined"
                    clickable
                    label={showAllRequestOptions ? 'Show less' : `Show ${control.options.length - 4} more`}
                    onClick={() => setShowAllRequestOptions((value) => !value)}
                    sx={{ height: 22 }}
                  />
                )}
              </Box>
            </Box>
          )}
          <Box sx={{ border: `1px solid ${tokens.border}` }}>
            <Button
              fullWidth
              onClick={() => s.openControlSettings({ kind: 'requestControl', id: control.id }, 'validation')}
              sx={{ justifyContent: 'flex-start', textTransform: 'none', px: 1, py: 0.5 }}
            >
              <Typography sx={{ fontSize: tokens.fontSize.body, fontWeight: 700 }}>
                Validation ({control.validations.length})
              </Typography>
            </Button>
            <Box sx={{ display: 'flex', alignItems: 'center', flexWrap: 'wrap', gap: 0.5, px: 1, pb: 0.75 }}>
              {(showAllRequestValidations ? validationSummary : validationSummary.slice(0, 4)).map((summary, index) => (
                <Chip
                  key={`${summary}-${index}`}
                  size="small"
                  variant="outlined"
                  label={summary}
                  title={summary}
                  sx={{ height: 22, color: '#7a4b00', bgcolor: '#fff3cd', borderColor: '#f0c36d' }}
                />
              ))}
              {validationSummary.length === 0 && (
                <Typography sx={{ color: tokens.textMuted, fontSize: tokens.fontSize.caption }}>
                  No validation rules
                </Typography>
              )}
              {validationSummary.length > 4 && (
                <Chip
                  size="small"
                  variant="outlined"
                  clickable
                  label={showAllRequestValidations ? 'Show less' : `Show ${validationSummary.length - 4} more`}
                  onClick={() => setShowAllRequestValidations((value) => !value)}
                  sx={{ height: 22, color: '#7a4b00', bgcolor: '#fff3cd', borderColor: '#f0c36d' }}
                />
              )}
            </Box>
          </Box>
          <Box sx={{ border: `1px solid ${tokens.border}` }}>
            <Button
              fullWidth
              onClick={() => s.openControlSettings({ kind: 'requestControl', id: control.id }, 'transitions')}
              sx={{ justifyContent: 'flex-start', textTransform: 'none', px: 1, py: 0.5 }}
            >
              <Typography sx={{ fontSize: tokens.fontSize.body, fontWeight: 700 }}>
                Transitions ({controlTransitions.length})
              </Typography>
            </Button>
            <Stack spacing="2px" sx={{ px: 1, pb: 0.75 }}>
              {(showAllRequestTransitions ? transitionSummary : transitionSummary.slice(0, 4)).map((summary, index) => (
                <Typography
                  key={`${summary}-${index}`}
                  title={summary}
                  sx={{ color: tokens.textMuted, fontSize: tokens.fontSize.caption, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}
                >
                  {summary}
                </Typography>
              ))}
              {transitionSummary.length === 0 && (
                <Typography sx={{ color: tokens.textMuted, fontSize: tokens.fontSize.caption }}>
                  No transitions
                </Typography>
              )}
              {transitionSummary.length > 4 && (
                <Button
                  size="small"
                  onClick={() => setShowAllRequestTransitions((value) => !value)}
                  sx={{ alignSelf: 'flex-start', minWidth: 0, p: 0, textTransform: 'none' }}
                >
                  {showAllRequestTransitions ? 'Show less' : `Show ${transitionSummary.length - 4} more`}
                </Button>
              )}
            </Stack>
          </Box>
        </Stack>
        <Button variant="contained" disabled sx={{ alignSelf: 'stretch' }}>
          Create Request Control
        </Button>
      </Stack>
    );
  }
  const control =
    selected.kind === 'control'
      ? d.steps
          .find((x) => x.id === selected.stepId)
          ?.activities.find((x) => x.id === selected.activityId)
          ?.controls.find((x) => x.id === selected.id)
      : null;
  if (control && selected.kind === 'control') {
    const update = (values: Partial<typeof control>) =>
      s.updateActivityControl(selected.stepId, selected.activityId, control.id, values);
    const controlTransitions = d.transitions.filter(
      (transition) =>
        transition.triggerSource === 'activity' && transition.triggerId === selected.activityId
    );
    if (s.controlSettingsPane === 'validation') {
      return (
        <Stack spacing="8px" sx={{ p: '10px' }}>
          <SettingsTitle title={`Control Validation (${control.validations.length})`} dirty={s.dirty} />
          <ValidationRules
            values={control.validations}
            onChange={(validations) => update({ validations })}
          />
        </Stack>
      );
    }
    if (s.controlSettingsPane === 'transitions') {
      return (
        <Stack spacing="8px" sx={{ p: '10px' }}>
          <SettingsTitle title={`Control Transitions (${controlTransitions.length})`} dirty={s.dirty} />
          <TransitionRules
            values={controlTransitions}
            variables={d.variables}
            steps={d.steps}
            onAdd={() =>
              s.addTransition({ triggerSource: 'activity', triggerId: selected.activityId })
            }
            onUpdate={s.updateTransition}
            onRemove={s.removeTransition}
          />
        </Stack>
      );
    }
    return (
      <Stack spacing="8px" sx={{ p: '10px' }}>
        <SettingsTitle title="Control Settings" dirty={s.dirty} isNew />
        {text('Code', control.code, (code) => update({ code }))}
        {text('Label', control.label, (label) => update({ label }))}
        {text('Arabic label', control.labelAR, (labelAR) => update({ labelAR }))}
        {control.type === 'label' && <TextField
          size="small" type="color" label="Note color"
          value={control.labelColor || '#7a4b00'}
          onChange={(event) => update({ labelColor: event.target.value })}
          slotProps={{ inputLabel: { shrink: true }, htmlInput: { 'aria-label': 'Note color' } }}
          sx={{ '& input': { minHeight: 30, p: 0.5, cursor: 'pointer' } }}
        />}
        <TextField
          select
          size="small"
          label="Control type"
          value={control.type}
          onChange={(e) => update({ type: e.target.value as typeof control.type })}
        >
          {controlPalette.map((item) => (
            <MenuItem key={item.type} value={item.type}>
              {item.label}
            </MenuItem>
          ))}
        </TextField>
        {['dropdown-db', 'dropdown-manual', 'checkboxlist', 'radiobuttonlist'].includes(
          control.type
        ) &&
          text('Options (comma separated)', control.options.join(', '), (value) =>
            update({
              options: value
                .split(',')
                .map((x) => x.trim())
                .filter(Boolean),
            })
          )}
        {text('Default value', control.defaultValue, (defaultValue) => update({ defaultValue }))}
        <Box sx={settingsSwitchGridSx}>
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={control.visible}
                onChange={(_, visible) => update({ visible })}
              />
            }
            label="Visible"
          />
        </Box>
        <Button variant="contained" disabled sx={{ alignSelf: 'stretch' }}>
          Create Control
        </Button>
      </Stack>
    );
  }
  if (selected.kind === 'transition') {
    const x = d.transitions.find((v) => v.id === selected.id);
    if (!x) return null;
    const variable = d.variables.find((item) => item.id === x.variableId);
    const activities = d.steps.flatMap((step) =>
      step.activities.map((activity) => ({ ...activity, stepName: step.name }))
    );
    return (
      <Stack spacing="8px" sx={{ p: '10px' }}>
        <SettingsTitle title="Transition Settings" dirty={s.dirty} />
        <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center' }}>
          <Box sx={{ flex: 1 }}>
            {text('Transition name', x.name, (name) => s.updateTransition(x.id, { name }))}
          </Box>
          <IconButton
            size="small"
            color="error"
            aria-label="Delete transition"
            onClick={() => s.removeTransition(x.id)}
          >
            <Delete />
          </IconButton>
        </Stack>

        <TextField
          select
          fullWidth
          size="small"
          label="Trigger source"
          value={x.triggerSource}
          onChange={(event) =>
            s.updateTransition(x.id, {
              triggerSource: event.target.value as BuilderTransition['triggerSource'],
              triggerId: '',
            })
          }
        >
          <MenuItem value="none">None</MenuItem>
          <MenuItem value="requestControl">Request control</MenuItem>
          <MenuItem value="activity">Activity</MenuItem>
        </TextField>
        {x.triggerSource === 'requestControl' && (
          <TextField
            select
            fullWidth
            size="small"
            label="Request control"
            value={x.triggerId}
            onChange={(event) => s.updateTransition(x.id, { triggerId: event.target.value })}
          >
            {d.requestControls.map((control) => (
              <MenuItem key={control.id} value={control.id}>
                {control.label}
              </MenuItem>
            ))}
          </TextField>
        )}
        {x.triggerSource === 'activity' && (
          <TextField
            select
            fullWidth
            size="small"
            label="Activity"
            value={x.triggerId}
            onChange={(event) => s.updateTransition(x.id, { triggerId: event.target.value })}
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
          fullWidth
          size="small"
          label="Variable"
          value={x.variableId}
          onChange={(event) => {
            const variableId = event.target.value;
            const dataType = d.variables.find((item) => item.id === variableId)?.dataType;
            s.updateTransition(x.id, {
              variableId,
              value: normalizeTransitionValue(x.value, dataType),
            });
          }}
        >
          {d.variables.map((item) => (
            <MenuItem key={item.id} value={item.id}>
              {item.name}
            </MenuItem>
          ))}
        </TextField>
        <Stack spacing={1.25}>
          <AppLookupField
            name={`settings-operatorId-${x.id}`}
            label="Operator"
            value={Number(x.operatorId) || undefined}
            options={(operators.data ?? []).map((item) => ({
              id: item.recId,
              code: item.code ?? '',
              name: item.name ?? '',
            }))}
            onChange={(value, option) =>
              s.updateTransition(x.id, {
                operatorId: value == null ? '' : String(value),
                operator:
                  option && !Array.isArray(option)
                    ? transitionOperatorFromLabel(option.name || option.code)
                    : x.operator,
              })
            }
            required
            displayMode="select"
          />
        </Stack>
        <TransitionValueField
          label="Comparison value"
          dataType={variable?.dataType}
          value={x.value}
          disabled={x.operator === 'isEmpty'}
          onChange={(value) => s.updateTransition(x.id, { value })}
        />
        <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1 }}>
          <TextField
            select
            fullWidth
            size="small"
            label="Target step"
            value={x.targetStepId}
            onChange={(event) => s.updateTransition(x.id, { targetStepId: event.target.value })}
          >
            {d.steps.map((step) => (
              <MenuItem key={step.id} value={step.id}>
                {step.name}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            fullWidth
            size="small"
            type="number"
            label="Sort order"
            value={x.sortOrder}
            slotProps={{ htmlInput: { min: 0, max: 255 } }}
            onChange={(event) =>
              s.updateTransition(x.id, { sortOrder: Number(event.target.value) })
            }
          />
        </Box>

        <Box sx={settingsSwitchGridSx}>
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={x.active}
                onChange={(_, active) => s.updateTransition(x.id, { active })}
              />
            }
            label="Active"
          />
        </Box>
      </Stack>
    );
  }
  return (
    <Typography color="text.secondary" sx={{ p: '10px', fontSize: tokens.fontSize.secondary }}>
      Select an item to edit its properties.
    </Typography>
  );
}
