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
import { controlPalette, getControlTypeLabel } from './ProcessBuilderPalette';
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
import { useAppTranslation } from '@core/localization/useAppTranslation';

const categoryLookupColumns = [
  { field: 'code', header: 'Code', width: 110 },
  { field: 'name', header: 'Name', flex: 1 },
] as const;

const requestOptionControlTypes = new Set([
  'dropdown-manual',
  'checkboxlist',
  'radiobuttonlist',
  'table',
]);
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
}) => {
  const { t } = useAppTranslation();
  return (
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
      {isNew && (
        <Chip
          size="small"
          label={t('wfProcessBuilder.status.newRecord')}
          sx={{ height: 20, bgcolor: '#eeeeee' }}
        />
      )}
      {dirty && (
        <Chip
          size="small"
          label={t('wfProcessBuilder.status.unsavedChanges')}
          sx={{ bgcolor: '#fff3cd', color: '#7a4b00', border: '1px solid #f0c36d', height: 20 }}
        />
      )}
    </Stack>
  );
};

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
  '& .MuiSwitch-root': { width: 24, height: 14, p: 0, marginInlineEnd: '4px' },
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
  const { t } = useAppTranslation();
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
  const normalizeRules = (rules: BuilderValidation[]) =>
    rules.map((rule, index) => ({
      ...rule,
      message: rule.message.trim() || DEFAULT_VALIDATION_MESSAGES[rule.type],
      sortOrder: (index + 1) * 10,
    }));
  const update = (id: string, patch: Partial<BuilderValidation>) =>
    onChange(normalizeRules(values.map((rule) => (rule.id === id ? { ...rule, ...patch } : rule))));
  const validationTypes: readonly BuilderValidationType[] = [
    'required',
    'minLength',
    'maxLength',
    'exactLength',
    'length',
    'minValue',
    'maxValue',
    'range',
    'regex',
    'pattern',
    'startsWith',
    'endsWith',
    'contains',
    'email',
    'url',
    'phone',
    'saudiMobile',
    'saudiNationalId',
    'saudiIban',
    'taxNumber',
    'passport',
    'fileExtensions',
    'fileSize',
    'maxFiles',
    'minSelected',
    'maxSelected',
    'compare',
    'comparison',
    'expression',
    'custom',
    'crossField',
    'mask',
    'inputMask',
  ];
  const changeType = (id: string, type: BuilderValidationType) => {
    const current = values.find((rule) => rule.id === id);
    if (!current) return;
    const visible = new Set(fieldsByType[type]);
    const previouslyVisible = new Set(fieldsByType[current.type] ?? []);
    const message =
      validationUsesCustomMessage(type) &&
      validationUsesCustomMessage(current.type) &&
      current.message.trim()
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
    onChange(
      normalizeRules([
        ...values,
        {
          id: crypto.randomUUID(),
          type: 'required',
          value: '',
          secondaryValue: '',
          operator: '',
          mask: '',
          message: DEFAULT_VALIDATION_MESSAGES.required,
          messageAlias: '',
          severity: 'Error',
          sortOrder: (values.length + 1) * 10,
          active: true,
        },
      ])
    );
  return (
    <Box sx={{ pt: 1.5, borderTop: '1px solid #e5e7eb' }}>
      <Stack direction="row" sx={{ alignItems: 'center' }}>
        <Typography sx={{ flex: 1, fontSize: tokens.fontSize.body, fontWeight: 600 }}>
          {t('wfProcessBuilder.settings.validationRules')}
        </Typography>
        <Button size="small" disabled={disabled} onClick={add}>
          + {t('wfProcessBuilder.actions.add')}
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
                label={t('wfProcessBuilder.settings.validationType')}
                value={rule.type}
                onChange={(event) =>
                  changeType(rule.id, event.target.value as BuilderValidationType)
                }
              >
                {validationTypes.map((type) => (
                  <MenuItem key={type} value={type}>
                    {t(`wfProcessBuilder.settings.validationTypes.${type}`)}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                select
                size="small"
                label={t('wfProcessBuilder.settings.severity')}
                value={rule.severity}
                onChange={(event) =>
                  update(rule.id, { severity: event.target.value as BuilderValidation['severity'] })
                }
              >
                {['Error', 'Warning', 'Information'].map((severity) => (
                  <MenuItem key={severity} value={severity}>
                    {t(`wfProcessBuilder.settings.severityValues.${severity}`)}
                  </MenuItem>
                ))}
              </TextField>
              {visible.has('value') && (
                <TextField
                  size="small"
                  label={
                    rule.type === 'range'
                      ? t('wfProcessBuilder.settings.minimumValue')
                      : t('wfProcessBuilder.settings.value')
                  }
                  value={rule.value}
                  onChange={(event) => update(rule.id, { value: event.target.value })}
                />
              )}
              {visible.has('secondaryValue') && (
                <TextField
                  size="small"
                  label={t('wfProcessBuilder.settings.maximumValue')}
                  value={rule.secondaryValue}
                  onChange={(event) => update(rule.id, { secondaryValue: event.target.value })}
                />
              )}
              {visible.has('operator') && (
                <TextField
                  size="small"
                  label={t('wfProcessBuilder.settings.fields.operator')}
                  value={rule.operator}
                  onChange={(event) => update(rule.id, { operator: event.target.value })}
                />
              )}
              {visible.has('expression') && (
                <TextField
                  size="small"
                  label={t('wfProcessBuilder.settings.validationExpression')}
                  value={rule.secondaryValue}
                  onChange={(event) => update(rule.id, { secondaryValue: event.target.value })}
                  sx={{ gridColumn: '1 / -1' }}
                />
              )}
              {visible.has('mask') && (
                <TextField
                  size="small"
                  label={t('wfProcessBuilder.settings.inputMask')}
                  value={rule.mask}
                  onChange={(event) => update(rule.id, { mask: event.target.value })}
                  sx={{ gridColumn: '1 / -1' }}
                />
              )}
              {validationUsesCustomMessage(rule.type) && (
                <>
                  <TextField
                    required
                    size="small"
                    label={t('wfProcessBuilder.settings.errorMessage')}
                    value={rule.message}
                    onChange={(event) => update(rule.id, { message: event.target.value })}
                    sx={{ gridColumn: '1 / -1' }}
                  />
                  <TextField
                    size="small"
                    label={t('wfProcessBuilder.settings.errorMessageAlias')}
                    value={rule.messageAlias ?? ''}
                    onChange={(event) => update(rule.id, { messageAlias: event.target.value })}
                    sx={{ gridColumn: '1 / -1' }}
                  />
                </>
              )}
              <Stack
                direction="row"
                sx={{ gridColumn: '1 / -1', alignItems: 'center', justifyContent: 'space-between' }}
              >
                <FormControlLabel
                  control={
                    <Switch
                      size="small"
                      sx={compactSwitchSx}
                      checked={rule.active}
                      onChange={(_, active) => update(rule.id, { active })}
                    />
                  }
                  label={t('common.active')}
                />
                <IconButton
                  color="error"
                  size="small"
                  aria-label={t('wfProcessBuilder.settings.deleteValidation')}
                  onClick={() =>
                    onChange(normalizeRules(values.filter((item) => item.id !== rule.id)))
                  }
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
            {t('wfProcessBuilder.settings.noValidationRules')}
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
  const { t } = useAppTranslation();
  return (
    <Box sx={{ pt: '12px', borderTop: `1px solid ${tokens.border}` }}>
      <Stack direction="row" sx={{ alignItems: 'center' }}>
        <Typography sx={{ flex: 1, fontSize: tokens.fontSize.body, fontWeight: 600 }}>
          {t('wfProcessBuilder.settings.transitionsCount', { count: values.length })}
        </Typography>
        <Button size="small" onClick={onAdd}>
          + {t('wfProcessBuilder.actions.add')}
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
                  label={t('wfProcessBuilder.settings.fields.variable')}
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
                  <MenuItem value="">{t('wfProcessBuilder.settings.fields.variable')}</MenuItem>
                  {variables.map((variable) => (
                    <MenuItem key={variable.id} value={variable.id}>
                      {variable.name}
                    </MenuItem>
                  ))}
                </TextField>
                <TextField
                  select
                  size="small"
                  label={t('wfProcessBuilder.settings.fields.operator')}
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
                  label={t('wfProcessBuilder.settings.value')}
                  dataType={variable?.dataType}
                  value={transition.value}
                  disabled={transition.operator === 'isEmpty'}
                  onChange={(value) => onUpdate(transition.id, { value })}
                />
                <TextField
                  select
                  size="small"
                  label={t('wfProcessBuilder.settings.fields.targetStep')}
                  value={transition.targetStepId}
                  onChange={(event) =>
                    onUpdate(transition.id, { targetStepId: event.target.value })
                  }
                >
                  <MenuItem value="">{t('wfProcessBuilder.settings.fields.targetStep')}</MenuItem>
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
                  label={t('common.active')}
                />
                <Box sx={{ flex: 1 }} />
                <IconButton
                  size="small"
                  color="error"
                  aria-label={t('wfProcessBuilder.settings.deleteTransition')}
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
            {t('wfProcessBuilder.settings.noTransitions')}
          </Typography>
        )}
      </Stack>
    </Box>
  );
}

export function ProcessBuilderSettingsPanel() {
  const { t } = useAppTranslation();
  const s = useProcessBuilderStore();
  const d = s.document;
  const selected = s.selected;
  const [activityValidationControlId, setActivityValidationControlId] = React.useState('');
  const [showAllRequestOptions, setShowAllRequestOptions] = React.useState(false);
  const [showAllRequestValidations, setShowAllRequestValidations] = React.useState(false);
  const [showAllRequestTransitions, setShowAllRequestTransitions] = React.useState(false);
  const optionSensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 4 } })
  );
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
      1: {
        title: t('wfProcessBuilder.settings.workspaceTitles.variables'),
        message: t('wfProcessBuilder.settings.workspaceHelp.variables'),
      },
      2: {
        title: t('wfProcessBuilder.settings.workspaceTitles.requestForm'),
        message: t('wfProcessBuilder.settings.workspaceHelp.requestForm'),
      },
      3: {
        title: t('wfProcessBuilder.settings.workspaceTitles.steps'),
        message: t('wfProcessBuilder.settings.workspaceHelp.steps'),
      },
      4: {
        title: t('wfProcessBuilder.settings.workspaceTitles.activities'),
        message: t('wfProcessBuilder.settings.workspaceHelp.activities'),
      },
      5: {
        title: t('wfProcessBuilder.settings.workspaceTitles.activityForm'),
        message: t('wfProcessBuilder.settings.workspaceHelp.activityForm'),
      },
      6: {
        title: t('wfProcessBuilder.settings.workspaceTitles.transitions'),
        message: t('wfProcessBuilder.settings.workspaceHelp.transitions'),
      },
      7: {
        title: t('wfProcessBuilder.settings.workspaceTitles.diagram'),
        message: t('wfProcessBuilder.settings.workspaceHelp.diagram'),
      },
    };
    const content = workspaceSettings[selected.tab] ?? {
      title: t('wfProcessBuilder.settings.title'),
      message: t('wfProcessBuilder.settings.workspaceHelp.default'),
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
        <SettingsTitle title={t('wfProcessBuilder.settings.processInformation')} isNew />
        <TextField
          fullWidth
          size="small"
          label={t('wfProcess.fields.code')}
          value={d.code}
          disabled
        />
        {text(t('wfProcess.fields.name'), d.name, (name) => s.updateProcess({ name }))}
        <TextField
          fullWidth
          multiline
          minRows={3}
          size="small"
          label={t('wfProcess.fields.description')}
          value={d.description}
          onChange={(event) => s.updateProcess({ description: event.target.value })}
        />
        <AppLookupGridField<WfCategoryRecord>
          name="categoryId"
          label={t('wfProcess.fields.category')}
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
          label={t('wfProcess.fields.priority')}
          value={d.priorityId ?? ''}
          onChange={(event) => s.updateProcess({ priorityId: event.target.value })}
        >
          <MenuItem value="">{t('wfProcessBuilder.settings.selectPriority')}</MenuItem>
          {(priorities.data ?? []).map((priority) => (
            <MenuItem key={priority.recId} value={String(priority.recId)}>
              {priority.code} - {priority.name}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label={t('wfProcess.fields.processType')}
          value={d.processType ?? ''}
          onChange={(event) => s.updateProcess({ processType: event.target.value })}
        >
          <MenuItem value="">{t('wfProcessBuilder.settings.selectProcessType')}</MenuItem>
          {(processTypes.data ?? []).map((processType) => (
            <MenuItem key={processType.recId} value={String(processType.recId)}>
              {processType.code} - {processType.name}
            </MenuItem>
          ))}
        </TextField>
        {text(
          t('wfProcess.fields.score'),
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
            label={t('common.active')}
          />
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={d.canRepeat ?? false}
                onChange={(_, canRepeat) => s.updateProcess({ canRepeat })}
              />
            }
            label={t('wfProcess.fields.canRepeat')}
          />
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={d.mandatoryDocs ?? false}
                onChange={(_, mandatoryDocs) => s.updateProcess({ mandatoryDocs })}
              />
            }
            label={t('wfProcess.fields.mandatoryDocs')}
          />
        </Box>
        <Box sx={{ pt: '12px', borderTop: `1px solid ${tokens.border}` }}>
          <Stack direction="row" sx={{ alignItems: 'center', minHeight: 28 }}>
            <Typography sx={{ flex: 1, fontSize: tokens.fontSize.body, fontWeight: 600 }}>
              {t('wfProcessBuilder.tabs.variables')}
            </Typography>
            {s.dirty && (
              <Chip
                size="small"
                variant="outlined"
                label={t('wfProcessBuilder.status.unsaved')}
                sx={{
                  marginInlineEnd: 1,
                  height: 22,
                  color: '#7a4b00',
                  bgcolor: '#fff3cd',
                  borderColor: '#f0c36d',
                }}
              />
            )}
            <Button size="small" onClick={s.addVariable}>
              + {t('wfProcessBuilder.actions.add')}
            </Button>
          </Stack>
          {d.id === 'new' && (
            <Typography sx={{ py: '8px', color: '#9a4f00', fontSize: tokens.fontSize.caption }}>
              {t('wfProcessBuilder.workspace.saveVariablesFirst')}
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
                    label={t('wfProcessBuilder.settings.fields.code')}
                    value={variable.code}
                    placeholder={t('wfProcessBuilder.settings.managedCode')}
                    disabled
                  />
                  <TextField
                    size="small"
                    value={
                      variable.name === 'New variable'
                        ? t('wfProcessBuilder.structure.newVariable')
                        : variable.name
                    }
                    onChange={(event) =>
                      s.updateVariable(variable.id, { name: event.target.value })
                    }
                  />
                  <IconButton
                    color="error"
                    size="small"
                    aria-label={t('wfProcessBuilder.settings.deleteVariable')}
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
                    label={t('wfProcessBuilder.settings.fields.dataType')}
                    value={variable.dataType}
                    onChange={(event) =>
                      s.updateVariable(variable.id, {
                        dataType: event.target.value as typeof variable.dataType,
                      })
                    }
                  >
                    {['text', 'number', 'boolean', 'date', 'object'].map((value) => (
                      <MenuItem key={value} value={value}>
                        {t(`wfProcessBuilder.dataTypes.${value}`)}
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
                    label={t('common.active')}
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
        <SettingsTitle title={t('wfProcessBuilder.settings.variable')} dirty={s.dirty} isNew />
        <TextField
          size="small"
          label={t('wfProcessBuilder.settings.fields.code')}
          value={x.code}
          placeholder={t('wfProcessBuilder.settings.managedCode')}
          disabled
        />
        {text(
          t('wfProcessBuilder.settings.fields.name'),
          x.name === 'New variable' ? t('wfProcessBuilder.structure.newVariable') : x.name,
          (name) => s.updateVariable(x.id, { name })
        )}
        {text(t('wfProcessBuilder.settings.fields.description'), x.description, (description) =>
          s.updateVariable(x.id, { description })
        )}
        <TextField
          select
          size="small"
          label={t('wfProcessBuilder.settings.fields.dataType')}
          value={x.dataType}
          onChange={(e) =>
            s.updateVariable(x.id, { dataType: e.target.value as typeof x.dataType })
          }
        >
          {['text', 'number', 'boolean', 'date', 'object'].map((v) => (
            <MenuItem key={v} value={v}>
              {t(`wfProcessBuilder.dataTypes.${v}`)}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          size="small"
          type="number"
          label={t('wfProcessBuilder.settings.fields.sortOrder')}
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
            label={t('common.active')}
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
        <SettingsTitle title={t('wfProcessBuilder.settings.stepSettings')} dirty={s.dirty} />
        <TextField
          size="small"
          label={t('wfProcessBuilder.settings.fields.code')}
          value={x.code}
          placeholder={t('wfProcessBuilder.settings.generatedCode')}
          disabled
        />
        {text(`${t('wfProcessBuilder.settings.fields.stepName')} *`, x.name, (name) =>
          s.updateStep(x.id, { name })
        )}
        <Box sx={{ display: 'grid', gridTemplateColumns: 'minmax(0, 1fr) minmax(0, 1fr)', gap: 1 }}>
          {text(
            t('wfProcessBuilder.settings.fields.order'),
            x.order,
            (value) => s.updateStep(x.id, { order: Number(value) }),
            'number'
          )}
          {text(
            t('wfProcessBuilder.settings.fields.autoPassingHours'),
            x.autoPassingHours,
            (value) => s.updateStep(x.id, { autoPassingHours: Number(value) }),
            'number'
          )}
        </Box>
        {text(
          t('wfProcessBuilder.settings.fields.score'),
          x.score,
          (value) => s.updateStep(x.id, { score: Number(value) }),
          'number'
        )}
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
            label={t('wfProcessBuilder.settings.fields.mandatory')}
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
            label={t('common.active')}
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
            label={t('wfProcessBuilder.settings.fields.system')}
          />
        </Box>
        <Section title={t('wfProcessBuilder.settings.stepCondition')}>
          <ConditionBuilder
            value={x.condition}
            variables={d.variables}
            onChange={(condition) => s.updateStep(x.id, { condition })}
          />
        </Section>
        <Button variant="contained" disabled sx={{ alignSelf: 'stretch', mt: 'auto' }}>
          {t('wfProcessBuilder.actions.saveSteps')}
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
        <SettingsTitle
          title={t('wfProcessBuilder.settings.activitySettings')}
          dirty={s.dirty}
          isNew={!/^\d+$/.test(x.id)}
        />
        <TextField
          size="small"
          label={t('wfProcessBuilder.settings.fields.code')}
          value={x.code}
          placeholder={t('wfProcessBuilder.settings.managedCode')}
          disabled
        />
        {text(
          t('wfProcessBuilder.settings.activityName'),
          x.name === 'New activity' ? t('wfProcessBuilder.structure.newActivity') : x.name,
          (name) => s.updateActivity(selected.stepId, x.id, { name })
        )}
        <Stack spacing={1}>
          <AppLookupField
            name={`settings-performerId-${x.id}`}
            label={t('wfProcessBuilder.settings.fields.performer')}
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
            label={t('wfProcessBuilder.settings.fields.activityType')}
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
            label={t('wfProcessBuilder.settings.fields.score')}
            value={x.score}
            onChange={(event) =>
              s.updateActivity(selected.stepId, x.id, { score: Number(event.target.value) })
            }
          />
          <TextField
            size="small"
            type="number"
            label={t('wfProcessBuilder.settings.fields.autoPassingHours')}
            value={x.autoPassingHours}
            disabled={!x.autoPassEnabled}
            onChange={(event) =>
              s.updateActivity(selected.stepId, x.id, {
                autoPassingHours: Number(event.target.value),
              })
            }
          />
        </Box>
        {text(
          t('wfProcessBuilder.settings.notificationEmails'),
          x.config.notifyEmails,
          (notifyEmails) =>
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
            label={t('common.active')}
          />
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={x.required}
                onChange={(_, required) => s.updateActivity(selected.stepId, x.id, { required })}
              />
            }
            label={t('wfProcessBuilder.settings.fields.required')}
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
            label={t('wfProcessBuilder.settings.autoPassEnabled')}
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
            label={t('wfProcessBuilder.settings.mandatoryDocuments')}
          />
        </Box>
        {x.type === 'api' && (
          <Section title={t('wfProcessBuilder.settings.apiAction')}>
            <Stack spacing={1}>
              <TextField
                select
                size="small"
                label={t('wfProcessBuilder.settings.method')}
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
              {text(t('wfProcessBuilder.settings.apiUrl'), x.config.apiUrl, (apiUrl) =>
                s.updateActivity(selected.stepId, x.id, { config: { ...x.config, apiUrl } })
              )}
            </Stack>
          </Section>
        )}
        {x.controls.length > 0 && (
          <TextField
            select
            size="small"
            label={t('wfProcessBuilder.settings.validationControl')}
            value={validationControl?.id ?? ''}
            onChange={(event) => setActivityValidationControlId(event.target.value)}
          >
            {x.controls.map((control) => (
              <MenuItem key={control.id} value={control.id}>
                {control.label || control.code || t('wfProcessBuilder.settings.unnamedControl')}
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
      t(`wfProcessBuilder.settings.validationTypes.${rule.type}`, {
        defaultValue: rule.type.replace(/([A-Z])/g, ' $1').toLocaleLowerCase(),
      })
    );
    const transitionSummary = controlTransitions.map((transition) => {
      const variable = d.variables.find((item) => item.id === transition.variableId);
      const target = d.steps.find((step) => step.id === transition.targetStepId);
      const condition = `${variable?.name || t('wfProcessBuilder.settings.fields.variable')} ${transition.operator}${
        transition.operator === 'isEmpty' ? '' : ` ${transition.value || '…'}`
      }`;
      return `${condition} → ${target?.name || t('wfProcessBuilder.settings.unassignedStep')}`;
    });
    const optionFeaturesAt = (index: number) => ({
      ...emptyOptionFeatures(),
      ...(control.optionFeatureConfigurations?.[index] ?? {}),
    });
    const updateOptionFeatures = (
      index: number,
      patch: Partial<BuilderOptionFeatureConfiguration>
    ) =>
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
          <SettingsTitle
            title={t(
              control.type === 'table'
                ? 'wfProcessBuilder.settings.requestControlColumns'
                : 'wfProcessBuilder.settings.requestControlOptions',
              {
                count: control.options.length,
              }
            )}
            dirty={s.dirty}
          />
          <Stack spacing="6px" sx={settingsGroupSx}>
            <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
              <Typography sx={{ fontSize: tokens.fontSize.body, fontWeight: 600 }}>
                {t(
                  control.type === 'table'
                    ? 'wfProcessBuilder.settings.tableColumns'
                    : 'wfProcessBuilder.settings.options'
                )}
              </Typography>
              <Button
                size="small"
                onClick={() =>
                  update({
                    options: [
                      ...control.options,
                      t(
                        control.type === 'table'
                          ? 'wfProcessBuilder.settings.columnNumber'
                          : 'wfProcessBuilder.settings.optionNumber',
                        {
                          number: control.options.length + 1,
                        }
                      ),
                    ],
                    optionAliases: [...(control.optionAliases ?? control.options.map(() => '')), ''],
                    optionScores: [...(control.optionScores ?? []), 0],
                    optionFeatureConfigurations: [
                      ...(control.optionFeatureConfigurations ??
                        control.options.map(() => emptyOptionFeatures())),
                      emptyOptionFeatures(),
                    ],
                  })
                }
              >
                +{' '}
                {t(
                  control.type === 'table'
                    ? 'wfProcessBuilder.settings.addColumn'
                    : 'wfProcessBuilder.settings.addOption'
                )}
              </Button>
            </Stack>
            {control.options.length === 0 && (
              <Typography color="text.secondary" sx={{ fontSize: tokens.fontSize.caption }}>
                {t(
                  control.type === 'table'
                    ? 'wfProcessBuilder.settings.addTableColumn'
                    : 'wfProcessBuilder.settings.addSelectableOption'
                )}
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
                    <SortableBuilderItem
                      key={`${index}-${control.options.length}`}
                      id={String(index)}
                    >
                      {(attributes, listeners) => (
                        <Stack spacing="4px">
                          <Stack direction="row" spacing="4px" sx={{ alignItems: 'center' }}>
                            <Box
                              {...attributes}
                              {...listeners}
                              aria-label={t('wfProcessBuilder.settings.reorderOption', {
                                number: index + 1,
                              })}
                              sx={{
                                display: 'flex',
                                color: tokens.textMuted,
                                cursor: 'grab',
                                touchAction: 'none',
                              }}
                            >
                              <DragIndicator fontSize="small" />
                            </Box>
                            <TextField
                              fullWidth
                              size="small"
                              label={t(
                                control.type === 'table'
                                  ? 'wfProcessBuilder.settings.columnNumber'
                                  : 'wfProcessBuilder.settings.optionNumber',
                                {
                                  number: index + 1,
                                }
                              )}
                              value={option}
                              onChange={(event) =>
                                update({
                                  options: control.options.map((item, itemIndex) =>
                                    itemIndex === index ? event.target.value : item
                                  ),
                                })
                              }
                            />
                            <TextField
                              fullWidth
                              size="small"
                              label={t('wfProcessBuilder.settings.optionAlias')}
                              value={control.optionAliases?.[index] ?? ''}
                              onChange={(event) =>
                                update({
                                  optionAliases: control.options.map((_, itemIndex) =>
                                    itemIndex === index
                                      ? event.target.value
                                      : (control.optionAliases?.[itemIndex] ?? '')
                                  ),
                                })
                              }
                            />
                            <IconButton
                              size="small"
                              color="error"
                              aria-label={t('wfProcessBuilder.settings.removeOption', {
                                number: index + 1,
                              })}
                              onClick={() =>
                                update({
                                  options: control.options.filter(
                                    (_, itemIndex) => itemIndex !== index
                                  ),
                                  optionAliases: (control.optionAliases ?? []).filter(
                                    (_, itemIndex) => itemIndex !== index
                                  ),
                                  optionScores: (control.optionScores ?? []).filter(
                                    (_, itemIndex) => itemIndex !== index
                                  ),
                                  optionFeatureConfigurations: (
                                    control.optionFeatureConfigurations ?? []
                                  ).filter((_, itemIndex) => itemIndex !== index),
                                })
                              }
                            >
                              <Delete fontSize="small" />
                            </IconButton>
                          </Stack>
                          <Stack
                            direction="row"
                            spacing="4px"
                            sx={{ marginInlineStart: '28px', marginInlineEnd: '32px' }}
                          >
                            <TextField
                              fullWidth
                              size="small"
                              type="number"
                              label={t('wfProcessBuilder.settings.fields.score')}
                              value={control.optionScores?.[index] ?? 0}
                              onChange={(event) =>
                                update({
                                  optionScores: control.options.map((_, itemIndex) =>
                                    itemIndex === index
                                      ? Number(event.target.value) || 0
                                      : (control.optionScores?.[itemIndex] ?? 0)
                                  ),
                                })
                              }
                            />
                            <TextField
                              fullWidth
                              size="small"
                              type="number"
                              label={t('wfProcessBuilder.settings.fields.sortOrder')}
                              value={index + 1}
                              slotProps={{ input: { readOnly: true } }}
                            />
                          </Stack>
                          <Accordion
                            sx={{
                              ...sectionSx,
                              marginInlineStart: '28px',
                              marginInlineEnd: '32px',
                            }}
                          >
                            <AccordionSummary expandIcon={<ExpandMore fontSize="small" />}>
                              <Typography
                                sx={{ fontSize: tokens.fontSize.secondary, fontWeight: 700 }}
                              >
                                {t('wfProcessBuilder.settings.featureConfiguration')}
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
                                        onChange={(_, requireFileUpload) =>
                                          updateOptionFeatures(index, { requireFileUpload })
                                        }
                                      />
                                    }
                                    label={t('wfProcessBuilder.settings.requireFileUpload')}
                                  />
                                  <FormControlLabel
                                    control={
                                      <Switch
                                        size="small"
                                        checked={optionFeaturesAt(index).sendAlertMessage}
                                        onChange={(_, sendAlertMessage) =>
                                          updateOptionFeatures(index, {
                                            sendAlertMessage,
                                            ...(!sendAlertMessage
                                              ? { alertMessage: '', performerIds: [] }
                                              : {}),
                                          })
                                        }
                                      />
                                    }
                                    label={t('wfProcessBuilder.settings.sendAlertMessage')}
                                  />
                                  <FormControlLabel
                                    control={
                                      <Switch
                                        size="small"
                                        checked={optionFeaturesAt(index).showOtherControls}
                                        onChange={(_, showOtherControls) =>
                                          updateOptionFeatures(index, {
                                            showOtherControls,
                                            ...(!showOtherControls
                                              ? { visibleControlIds: [] }
                                              : {}),
                                          })
                                        }
                                      />
                                    }
                                    label={t('wfProcessBuilder.settings.showOtherControls')}
                                  />
                                </Box>
                                {optionFeaturesAt(index).sendAlertMessage && (
                                  <Stack spacing="8px">
                                    <TextField
                                      fullWidth
                                      size="small"
                                      label={t('wfProcessBuilder.settings.alertMessage')}
                                      value={optionFeaturesAt(index).alertMessage}
                                      onChange={(event) =>
                                        updateOptionFeatures(index, {
                                          alertMessage: event.target.value,
                                        })
                                      }
                                    />
                                    <Autocomplete
                                      multiple
                                      size="small"
                                      options={performers.data ?? []}
                                      loading={performers.isLoading}
                                      getOptionLabel={(item) =>
                                        `${item.code ?? ''} - ${item.name ?? ''}`
                                      }
                                      value={(performers.data ?? []).filter((item) =>
                                        optionFeaturesAt(index).performerIds.includes(
                                          String(item.recId)
                                        )
                                      )}
                                      onChange={(_, selectedPerformers) =>
                                        updateOptionFeatures(index, {
                                          performerIds: selectedPerformers.map((item) =>
                                            String(item.recId)
                                          ),
                                        })
                                      }
                                      renderInput={(params) => (
                                        <TextField
                                          {...params}
                                          label={t('wfProcessBuilder.settings.performers')}
                                          placeholder={t(
                                            'wfProcessBuilder.settings.searchPerformers'
                                          )}
                                        />
                                      )}
                                    />
                                  </Stack>
                                )}
                                {optionFeaturesAt(index).showOtherControls && (
                                  <TextField
                                    fullWidth
                                    select
                                    size="small"
                                    label={t('wfProcessBuilder.settings.showOtherControls')}
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
                                    {d.requestControls
                                      .filter((item) => item.id !== control.id)
                                      .map((item) => (
                                        <MenuItem key={item.id} value={item.id}>
                                          {item.label ||
                                            item.code ||
                                            t('wfProcessBuilder.settings.unnamedControl')}
                                        </MenuItem>
                                      ))}
                                  </TextField>
                                )}
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
          <SettingsTitle
            title={t('wfProcessBuilder.settings.requestControlValidation', {
              count: control.validations.length,
            })}
            dirty={s.dirty}
          />
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
          <SettingsTitle
            title={t('wfProcessBuilder.settings.requestControlTransitions', {
              count: controlTransitions.length,
            })}
            dirty={s.dirty}
          />
          <TransitionRules
            values={controlTransitions}
            variables={d.variables}
            steps={d.steps}
            onAdd={() =>
              s.addTransition({ triggerSource: 'requestControl', triggerId: control.id })
            }
            onUpdate={s.updateTransition}
            onRemove={s.removeTransition}
          />
        </Stack>
      );
    }
    return (
      <Stack spacing="8px" sx={{ p: '10px' }}>
        <SettingsTitle
          title={t('wfProcessBuilder.settings.requestControl')}
          dirty={s.dirty}
          isNew
        />
        {text(
          t('wfProcessBuilder.settings.controlCode'),
          `RCTL-${String(d.requestControls.indexOf(control) + 1).padStart(4, '0')}`,
          () => undefined
        )}
        {text(t('wfProcessBuilder.settings.fields.label'), control.label, (label) =>
          update({ label })
        )}
        <Stack direction="row" spacing="8px">
          <TextField
            fullWidth
            size="small"
            type="number"
            label={t('wfProcessBuilder.settings.fields.score')}
            value={control.score}
            onChange={(event) => update({ score: Number(event.target.value) || 0 })}
          />
          <TextField
            fullWidth
            size="small"
            type="number"
            label={t('wfProcessBuilder.settings.fields.sortOrder')}
            value={control.sortOrder}
            slotProps={{ input: { readOnly: true } }}
          />
          <TextField
            fullWidth
            select
            size="small"
            label={t('wfProcessBuilder.settings.width')}
            value={control.columnSpan ?? 1}
            onChange={(event) => update({ columnSpan: Number(event.target.value) as 1 | 2 | 3 })}
          >
            <MenuItem value={1}>{t('wfProcessBuilder.settings.columns.one')}</MenuItem>
            <MenuItem value={2}>{t('wfProcessBuilder.settings.columns.two')}</MenuItem>
            <MenuItem value={3}>{t('wfProcessBuilder.settings.columns.full')}</MenuItem>
          </TextField>
        </Stack>
        {control.type === 'label' && (
          <TextField
            size="small"
            type="color"
            label={t('wfProcessBuilder.settings.noteColor')}
            value={control.labelColor || '#7a4b00'}
            onChange={(event) => update({ labelColor: event.target.value })}
            slotProps={{
              inputLabel: { shrink: true },
              htmlInput: { 'aria-label': t('wfProcessBuilder.settings.noteColorAria') },
            }}
            sx={{ '& input': { minHeight: 30, p: 0.5, cursor: 'pointer' } }}
          />
        )}
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
            label={t('wfProcessBuilder.settings.fields.visible')}
          />
        </Box>
        <Stack spacing="6px">
          {requestOptionControlTypes.has(control.type) && (
            <Box sx={{ border: `1px solid ${tokens.border}` }}>
              <Button
                fullWidth
                onClick={() =>
                  s.openControlSettings({ kind: 'requestControl', id: control.id }, 'options')
                }
                sx={{ justifyContent: 'flex-start', textTransform: 'none', px: 1, py: 0.5 }}
              >
                <Typography sx={{ fontSize: tokens.fontSize.body, fontWeight: 700 }}>
                  {t('wfProcessBuilder.settings.optionsCount', { count: control.options.length })}
                </Typography>
              </Button>
              <Box
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  flexWrap: 'wrap',
                  gap: 0.5,
                  px: 1,
                  pb: 0.75,
                }}
              >
                {(showAllRequestOptions ? control.options : control.options.slice(0, 4)).map(
                  (option, index) => (
                    <Chip
                      key={`${option}-${index}`}
                      size="small"
                      variant="outlined"
                      label={
                        option || t('wfProcessBuilder.settings.optionNumber', { number: index + 1 })
                      }
                      title={
                        option || t('wfProcessBuilder.settings.optionNumber', { number: index + 1 })
                      }
                      sx={{ height: 22 }}
                    />
                  )
                )}
                {control.options.length === 0 && (
                  <Typography sx={{ color: tokens.textMuted, fontSize: tokens.fontSize.caption }}>
                    {t('wfProcessBuilder.settings.noOptions')}
                  </Typography>
                )}
                {control.options.length > 4 && (
                  <Chip
                    size="small"
                    variant="outlined"
                    clickable
                    label={
                      showAllRequestOptions
                        ? t('wfProcessBuilder.settings.showLess')
                        : t('wfProcessBuilder.settings.showMore', {
                            count: control.options.length - 4,
                          })
                    }
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
              onClick={() =>
                s.openControlSettings({ kind: 'requestControl', id: control.id }, 'validation')
              }
              sx={{ justifyContent: 'flex-start', textTransform: 'none', px: 1, py: 0.5 }}
            >
              <Typography sx={{ fontSize: tokens.fontSize.body, fontWeight: 700 }}>
                {t('wfProcessBuilder.settings.validationCount', {
                  count: control.validations.length,
                })}
              </Typography>
            </Button>
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                flexWrap: 'wrap',
                gap: 0.5,
                px: 1,
                pb: 0.75,
              }}
            >
              {(showAllRequestValidations ? validationSummary : validationSummary.slice(0, 4)).map(
                (summary, index) => (
                  <Chip
                    key={`${summary}-${index}`}
                    size="small"
                    variant="outlined"
                    label={summary}
                    title={summary}
                    sx={{
                      height: 22,
                      color: '#7a4b00',
                      bgcolor: '#fff3cd',
                      borderColor: '#f0c36d',
                    }}
                  />
                )
              )}
              {validationSummary.length === 0 && (
                <Typography sx={{ color: tokens.textMuted, fontSize: tokens.fontSize.caption }}>
                  {t('wfProcessBuilder.settings.noValidationRules')}
                </Typography>
              )}
              {validationSummary.length > 4 && (
                <Chip
                  size="small"
                  variant="outlined"
                  clickable
                  label={
                    showAllRequestValidations
                      ? t('wfProcessBuilder.settings.showLess')
                      : t('wfProcessBuilder.settings.showMore', {
                          count: validationSummary.length - 4,
                        })
                  }
                  onClick={() => setShowAllRequestValidations((value) => !value)}
                  sx={{ height: 22, color: '#7a4b00', bgcolor: '#fff3cd', borderColor: '#f0c36d' }}
                />
              )}
            </Box>
          </Box>
          <Box sx={{ border: `1px solid ${tokens.border}` }}>
            <Button
              fullWidth
              onClick={() =>
                s.openControlSettings({ kind: 'requestControl', id: control.id }, 'transitions')
              }
              sx={{ justifyContent: 'flex-start', textTransform: 'none', px: 1, py: 0.5 }}
            >
              <Typography sx={{ fontSize: tokens.fontSize.body, fontWeight: 700 }}>
                {t('wfProcessBuilder.settings.transitionsCount', {
                  count: controlTransitions.length,
                })}
              </Typography>
            </Button>
            <Stack spacing="2px" sx={{ px: 1, pb: 0.75 }}>
              {(showAllRequestTransitions ? transitionSummary : transitionSummary.slice(0, 4)).map(
                (summary, index) => (
                  <Typography
                    key={`${summary}-${index}`}
                    title={summary}
                    sx={{
                      color: tokens.textMuted,
                      fontSize: tokens.fontSize.caption,
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap',
                    }}
                  >
                    {summary}
                  </Typography>
                )
              )}
              {transitionSummary.length === 0 && (
                <Typography sx={{ color: tokens.textMuted, fontSize: tokens.fontSize.caption }}>
                  {t('wfProcessBuilder.settings.noTransitions')}
                </Typography>
              )}
              {transitionSummary.length > 4 && (
                <Button
                  size="small"
                  onClick={() => setShowAllRequestTransitions((value) => !value)}
                  sx={{ alignSelf: 'flex-start', minWidth: 0, p: 0, textTransform: 'none' }}
                >
                  {showAllRequestTransitions
                    ? t('wfProcessBuilder.settings.showLess')
                    : t('wfProcessBuilder.settings.showMore', {
                        count: transitionSummary.length - 4,
                      })}
                </Button>
              )}
            </Stack>
          </Box>
        </Stack>
        <Button variant="contained" disabled sx={{ alignSelf: 'stretch' }}>
          {t('wfProcessBuilder.settings.createRequestControl')}
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
          <SettingsTitle
            title={t('wfProcessBuilder.settings.controlValidation', {
              count: control.validations.length,
            })}
            dirty={s.dirty}
          />
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
          <SettingsTitle
            title={t('wfProcessBuilder.settings.controlTransitions', {
              count: controlTransitions.length,
            })}
            dirty={s.dirty}
          />
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
        <SettingsTitle
          title={t('wfProcessBuilder.settings.controlSettings')}
          dirty={s.dirty}
          isNew
        />
        {text(t('wfProcessBuilder.settings.fields.code'), control.code, (code) => update({ code }))}
        {text(t('wfProcessBuilder.settings.fields.label'), control.label, (label) =>
          update({ label })
        )}
        {text(t('wfProcessBuilder.settings.arabicLabel'), control.labelAR, (labelAR) =>
          update({ labelAR })
        )}
        {control.type === 'label' && (
          <TextField
            size="small"
            type="color"
            label={t('wfProcessBuilder.settings.noteColor')}
            value={control.labelColor || '#7a4b00'}
            onChange={(event) => update({ labelColor: event.target.value })}
            slotProps={{
              inputLabel: { shrink: true },
              htmlInput: { 'aria-label': t('wfProcessBuilder.settings.noteColorAria') },
            }}
            sx={{ '& input': { minHeight: 30, p: 0.5, cursor: 'pointer' } }}
          />
        )}
        <TextField
          select
          size="small"
          label={t('wfProcessBuilder.settings.controlType')}
          value={control.type}
          onChange={(e) => update({ type: e.target.value as typeof control.type })}
        >
          {controlPalette.map((item) => (
            <MenuItem key={item.type} value={item.type}>
              {getControlTypeLabel(t, item.type)}
            </MenuItem>
          ))}
        </TextField>
        {['dropdown-db', 'dropdown-manual', 'checkboxlist', 'radiobuttonlist'].includes(
          control.type
        ) &&
          text(
            t('wfProcessBuilder.settings.commaSeparatedOptions'),
            control.options.join(', '),
            (value) =>
              update({
                options: value
                  .split(',')
                  .map((x) => x.trim())
                  .filter(Boolean),
              })
          )}
        {text(t('wfProcessBuilder.settings.defaultValue'), control.defaultValue, (defaultValue) =>
          update({ defaultValue })
        )}
        <Box sx={settingsSwitchGridSx}>
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={control.visible}
                onChange={(_, visible) => update({ visible })}
              />
            }
            label={t('wfProcessBuilder.settings.fields.visible')}
          />
        </Box>
        <Button variant="contained" disabled sx={{ alignSelf: 'stretch' }}>
          {t('wfProcessBuilder.settings.createControl')}
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
        <SettingsTitle title={t('wfProcessBuilder.settings.transitionSettings')} dirty={s.dirty} />
        <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center' }}>
          <Box sx={{ flex: 1 }}>
            {text(t('wfProcessBuilder.settings.fields.transitionName'), x.name, (name) =>
              s.updateTransition(x.id, { name })
            )}
          </Box>
          <IconButton
            size="small"
            color="error"
            aria-label={t('wfProcessBuilder.settings.deleteTransition')}
            onClick={() => s.removeTransition(x.id)}
          >
            <Delete />
          </IconButton>
        </Stack>

        <TextField
          select
          fullWidth
          size="small"
          label={t('wfProcessBuilder.settings.fields.triggerSource')}
          value={x.triggerSource}
          onChange={(event) =>
            s.updateTransition(x.id, {
              triggerSource: event.target.value as BuilderTransition['triggerSource'],
              triggerId: '',
            })
          }
        >
          <MenuItem value="none">{t('wfProcessBuilder.triggerSources.none')}</MenuItem>
          <MenuItem value="requestControl">
            {t('wfProcessBuilder.triggerSources.requestControl')}
          </MenuItem>
          <MenuItem value="activity">{t('wfProcessBuilder.triggerSources.activity')}</MenuItem>
        </TextField>
        {x.triggerSource === 'requestControl' && (
          <TextField
            select
            fullWidth
            size="small"
            label={t('wfProcessBuilder.settings.fields.requestControl')}
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
            label={t('wfProcessBuilder.settings.fields.activity')}
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
          label={t('wfProcessBuilder.settings.fields.variable')}
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
            label={t('wfProcessBuilder.settings.fields.operator')}
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
          label={t('wfProcessBuilder.settings.comparisonValue')}
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
            label={t('wfProcessBuilder.settings.fields.targetStep')}
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
            label={t('wfProcessBuilder.settings.fields.sortOrder')}
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
            label={t('common.active')}
          />
        </Box>
      </Stack>
    );
  }
  return (
    <Typography color="text.secondary" sx={{ p: '10px', fontSize: tokens.fontSize.secondary }}>
      {t('wfProcessBuilder.settings.selectItemToEdit')}
    </Typography>
  );
}
