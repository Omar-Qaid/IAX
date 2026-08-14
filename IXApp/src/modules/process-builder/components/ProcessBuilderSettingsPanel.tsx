import React from 'react';
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
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
import AccountTree from '@mui/icons-material/AccountTree';
import IconButton from '@mui/material/IconButton';
import { useProcessBuilderStore } from '../store/useProcessBuilderStore';
import { ConditionBuilder } from './ConditionBuilder';
import { activityPalette, controlPalette } from './ProcessBuilderPalette';
import type {
  BuilderStep,
  BuilderTransition,
  BuilderValidation,
  BuilderValidationType,
  BuilderVariable,
} from '../types/processBuilderTypes';
import { processBuilderTokens as tokens } from './processBuilderTokens';

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
    <Typography component="h2" sx={{ flex: 1, fontSize: tokens.fontSize.heading, fontWeight: 700, color: tokens.text }}>
      {title}
    </Typography>
    {isNew && <Chip size="small" label="New record" sx={{ height: 20, bgcolor: '#eeeeee' }} />}
    {dirty && <Chip size="small" label="Unsaved changes" sx={{ bgcolor: '#fff3cd', color: '#7a4b00', border: '1px solid #f0c36d', height: 20 }} />}
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

const switchRowSx = { m: 0, minHeight: 32, justifyContent: 'space-between' };

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
}: {
  values: BuilderValidation[];
  onChange: (values: BuilderValidation[]) => void;
}) {
  const update = (id: string, patch: Partial<BuilderValidation>) =>
    onChange(values.map((rule) => (rule.id === id ? { ...rule, ...patch } : rule)));
  const validationTypes: ReadonlyArray<{ value: BuilderValidationType; label: string }> = [
    ['required', 'Required'],
    ['minLength', 'Minimum length'],
    ['maxLength', 'Maximum length'],
    ['exactLength', 'Exact length'],
    ['minValue', 'Minimum value'],
    ['maxValue', 'Maximum value'],
    ['range', 'Range'],
    ['regex', 'Regular expression'],
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
    ['minSelected', 'Minimum selected items'],
    ['maxSelected', 'Maximum selected items'],
    ['custom', 'Custom expression'],
    ['crossField', 'Cross-field validation'],
  ].map(([value, label]) => ({ value: value as BuilderValidationType, label }));
  const messages: Partial<Record<BuilderValidationType, [string, string]>> = {
    required: ['This field is required.', 'هذا الحقل مطلوب.'],
    email: ['Enter a valid email address.', 'أدخل عنوان بريد إلكتروني صالحًا.'],
    url: ['Enter a valid URL.', 'أدخل رابطًا صالحًا.'],
    saudiMobile: ['Enter a valid Saudi mobile number.', 'أدخل رقم جوال سعودي صالحًا.'],
    saudiNationalId: ['Enter a valid Saudi National ID.', 'أدخل رقم هوية وطنية سعودي صالحًا.'],
    saudiIban: ['Enter a valid Saudi IBAN.', 'أدخل رقم آيبان سعودي صالحًا.'],
  };
  const changeType = (id: string, type: BuilderValidationType) => {
    const [message, messageAR] = messages[type] ?? ['', ''];
    update(id, { type, message, messageAR });
  };
  const add = () =>
    onChange([
      ...values,
      {
        id: crypto.randomUUID(),
        type: 'required',
        value: '',
        secondaryValue: '',
        operator: '',
        mask: '',
        message: messages.required?.[0] ?? '',
        messageAR: messages.required?.[1] ?? '',
        severity: 'Error',
        sortOrder: (values.length + 1) * 10,
        active: true,
      },
    ]);
  return (
    <Box sx={{ pt: 1.5, borderTop: '1px solid #e5e7eb' }}>
      <Stack direction="row" sx={{ alignItems: 'center' }}>
        <Typography sx={{ flex: 1, fontSize: tokens.fontSize.body, fontWeight: 600 }}>Validation Rules</Typography>
        <Button size="small" onClick={add}>
          + Add
        </Button>
      </Stack>
      <Stack spacing={1.25} sx={{ mt: 1 }}>
        {values.map((rule) => (
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
              onChange={(event) => changeType(rule.id, event.target.value as BuilderValidationType)}
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
            <TextField
              size="small"
              label="Expression / Rule Value"
              value={rule.value}
              onChange={(event) => update(rule.id, { value: event.target.value })}
              sx={{ gridColumn: '1 / -1' }}
            />
            <TextField
              size="small"
              type="number"
              label="Sort Order"
              value={rule.sortOrder}
              onChange={(event) => update(rule.id, { sortOrder: Number(event.target.value) })}
            />
            <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
              <FormControlLabel
                control={
                  <Switch
                    size="small"
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
                onClick={() => onChange(values.filter((item) => item.id !== rule.id))}
              >
                <Delete fontSize="small" />
              </IconButton>
            </Stack>
          </Box>
        ))}
        {values.length === 0 && (
          <Typography color="text.secondary" sx={{ py: 2, textAlign: 'center', fontSize: tokens.fontSize.caption }}>
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
        <Button size="small" onClick={onAdd}>+ Add</Button>
      </Stack>
      <Stack spacing="10px" sx={{ mt: '8px' }}>
        {values.map((transition) => (
          <Box
            key={transition.id}
            sx={{ p: '10px', border: `1px solid ${tokens.border}`, bgcolor: '#fff' }}
          >
            <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '8px' }}>
              <TextField select size="small" label="Variable" value={transition.variableId} onChange={(event) => onUpdate(transition.id, { variableId: event.target.value })}>
                <MenuItem value="">Variable</MenuItem>
                {variables.map((variable) => <MenuItem key={variable.id} value={variable.id}>{variable.name}</MenuItem>)}
              </TextField>
              <TextField select size="small" label="Operator" value={transition.operator} onChange={(event) => onUpdate(transition.id, { operator: event.target.value as BuilderTransition['operator'] })}>
                {['=', '!=', '>', '<', '>=', '<=', 'contains', 'isEmpty'].map((operator) => <MenuItem key={operator} value={operator}>{operator}</MenuItem>)}
              </TextField>
              <TextField size="small" label="Value" value={transition.value} onChange={(event) => onUpdate(transition.id, { value: event.target.value })} />
              <TextField select size="small" label="Target Step" value={transition.targetStepId} onChange={(event) => onUpdate(transition.id, { targetStepId: event.target.value })}>
                <MenuItem value="">Target Step</MenuItem>
                {steps.map((step) => <MenuItem key={step.id} value={step.id}>{step.name}</MenuItem>)}
              </TextField>
            </Box>
            <Stack direction="row" sx={{ mt: '8px', alignItems: 'center' }}>
              <FormControlLabel control={<Switch size="small" checked={transition.active} onChange={(_, active) => onUpdate(transition.id, { active })} />} label="Active" />
              <Box sx={{ flex: 1 }} />
              <IconButton size="small" color="error" aria-label="Delete transition" onClick={() => onRemove(transition.id)}><Delete /></IconButton>
            </Stack>
          </Box>
        ))}
        {values.length === 0 && (
          <Typography color="text.secondary" sx={{ py: '12px', textAlign: 'center', fontSize: tokens.fontSize.caption }}>
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
  if (selected.kind === 'process')
    return (
      <Stack spacing="8px" sx={{ p: '10px', minHeight: '100%' }}>
        <SettingsTitle title="Process Information" isNew />
        <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
          <Box sx={{ flex: '0 1 220px', minWidth: 0 }}>
            {text('Code', d.code, (code) => s.updateProcess({ code }))}
          </Box>
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
        </Stack>
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
        <TextField
          select
          size="small"
          label="Category"
          value={d.categoryId ?? ''}
          onChange={(event) => s.updateProcess({ categoryId: event.target.value })}
        >
          <MenuItem value="">None</MenuItem>
          <MenuItem value="1">1</MenuItem>
        </TextField>
        <TextField
          select
          size="small"
          label="Priority"
          value={d.priorityId ?? ''}
          onChange={(event) => s.updateProcess({ priorityId: event.target.value })}
        >
          <MenuItem value="">None</MenuItem>
        </TextField>
        <TextField
          select
          size="small"
          label="Process Type"
          value={d.processType ?? 'Workflow Process'}
          onChange={(event) => s.updateProcess({ processType: event.target.value })}
        >
          <MenuItem value="Workflow Process">Workflow Process</MenuItem>
        </TextField>
        {text(
          'Score',
          d.score ?? 100,
          (value) => s.updateProcess({ score: Number(value) }),
          'number'
        )}
        <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '8px' }}>
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
        <Button
          variant="contained"
          startIcon={<AccountTree />}
          onClick={s.markDraftSaved}
          sx={{ alignSelf: 'stretch', bgcolor: tokens.accent, '&:hover': { bgcolor: tokens.accentHover } }}
        >
          Create Process
        </Button>
        <Box sx={{ pt: '12px', borderTop: `1px solid ${tokens.border}` }}>
          <Stack direction="row" sx={{ alignItems: 'center', minHeight: 28 }}>
            <Typography sx={{ flex: 1, fontSize: tokens.fontSize.body, fontWeight: 600 }}>Variables</Typography>
            {s.dirty && <Chip size="small" variant="outlined" label="Unsaved" sx={{ mr: 1, height: 22, color: '#7a4b00', bgcolor: '#fff3cd', borderColor: '#f0c36d' }} />}
            <Button size="small" onClick={s.addVariable}>+ Add</Button>
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
                <Box sx={{ display: 'grid', gridTemplateColumns: '88px minmax(0, 1fr) 24px', gap: '6px' }}>
                  <TextField size="small" label="Code" value={variable.code} onChange={(event) => s.updateVariable(variable.id, { code: event.target.value })} />
                  <TextField size="small" value={variable.name} onChange={(event) => s.updateVariable(variable.id, { name: event.target.value })} />
                  <IconButton color="error" size="small" aria-label="Delete variable" onClick={() => s.removeVariable(variable.id)}><Delete /></IconButton>
                </Box>
                <Box sx={{ display: 'grid', gridTemplateColumns: 'minmax(0, 1fr) 66px auto', gap: '6px', mt: '6px', alignItems: 'center' }}>
                  <TextField select size="small" label="Data Type" value={variable.dataType} onChange={(event) => s.updateVariable(variable.id, { dataType: event.target.value as typeof variable.dataType })}>
                    {['text', 'number', 'boolean', 'date', 'object'].map((value) => <MenuItem key={value} value={value}>{value}</MenuItem>)}
                  </TextField>
                  <TextField size="small" type="number" value={variable.sortOrder} onChange={(event) => s.updateVariable(variable.id, { sortOrder: Number(event.target.value) })} />
                  <FormControlLabel control={<Switch size="small" checked={variable.active} onChange={(_, active) => s.updateVariable(variable.id, { active })} />} label="Active" />
                </Box>
              </Box>
            ))}
          </Stack>
          <Button fullWidth variant="contained" disabled sx={{ mt: '12px' }}>Save Variables</Button>
        </Box>
      </Stack>
    );
  if (selected.kind === 'variable') {
    const x = d.variables.find((v) => v.id === selected.id);
    if (!x) return null;
    return (
      <Stack spacing="8px" sx={{ p: '10px' }}>
        <SettingsTitle title="Variable" dirty={s.dirty} isNew />
        {text('Code', x.code, (code) => s.updateVariable(x.id, { code }))}
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
          select
          size="small"
          label="Scope"
          value={x.scope}
          onChange={(e) => s.updateVariable(x.id, { scope: e.target.value as typeof x.scope })}
        >
          {['process', 'step', 'activity', 'global'].map((v) => (
            <MenuItem key={v} value={v}>
              {v}
            </MenuItem>
          ))}
        </TextField>
        {text('Default value', x.defaultValue, (defaultValue) =>
          s.updateVariable(x.id, { defaultValue })
        )}
        <Box sx={settingsGroupSx}>
          <FormControlLabel
            labelPlacement="start"
            sx={switchRowSx}
            control={
              <Switch
                size="small"
                checked={x.required}
                onChange={(_, required) => s.updateVariable(x.id, { required })}
              />
            }
            label="Required"
          />
          <FormControlLabel
            labelPlacement="start"
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
    const generatedCode = x.code || `STEP-${String(x.order).padStart(5, '0')}`;
    return (
      <Stack spacing="8px" sx={{ p: '10px', minHeight: '100%' }}>
        <SettingsTitle title="Step Settings" dirty={s.dirty} />
        <TextField size="small" label="Step Code *" value={generatedCode} disabled />
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
        <Box sx={settingsGroupSx}>
          <FormControlLabel
            labelPlacement="start"
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
            labelPlacement="start"
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
            labelPlacement="start"
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
    return (
      <Stack spacing="8px" sx={{ p: '10px' }}>
        <SettingsTitle title="Activity Settings" dirty={s.dirty} isNew />
        {text('Activity code', x.code, (code) => s.updateActivity(selected.stepId, x.id, { code }))}
        {text('Activity name', x.name, (name) => s.updateActivity(selected.stepId, x.id, { name }))}
        <Stack spacing={1.25}>
          <TextField
            select
            size="small"
            label="Type"
            value={x.type}
            onChange={(e) =>
              s.updateActivity(selected.stepId, x.id, { type: e.target.value as typeof x.type })
            }
          >
            {activityPalette.map((item) => (
              <MenuItem key={item.type} value={item.type}>
                {item.label}
              </MenuItem>
            ))}
          </TextField>
          <FormControlLabel
            control={
              <Switch
                checked={x.active}
                onChange={(_, active) => s.updateActivity(selected.stepId, x.id, { active })}
              />
            }
            label="Active"
          />
        </Stack>
        <Stack spacing={1}>
          {text(
            'Auto passing hours',
            x.autoPassingHours,
            (value) =>
              s.updateActivity(selected.stepId, x.id, { autoPassingHours: Number(value) }),
            'number'
          )}
          <FormControlLabel
            control={
              <Switch
                checked={x.autoPassEnabled}
                onChange={(_, autoPassEnabled) =>
                  s.updateActivity(selected.stepId, x.id, { autoPassEnabled })
                }
              />
            }
            label="Auto pass enabled"
          />
        </Stack>
        {text('Notification emails', x.config.notifyEmails, (notifyEmails) =>
          s.updateActivity(selected.stepId, x.id, { config: { ...x.config, notifyEmails } })
        )}
        <Stack spacing={1}>
          <FormControlLabel
            control={
              <Switch
                checked={x.required}
                onChange={(_, required) => s.updateActivity(selected.stepId, x.id, { required })}
              />
            }
            label="Required"
          />
          <FormControlLabel
            control={
              <Switch
                checked={x.mandatoryDocs}
                onChange={(_, mandatoryDocs) =>
                  s.updateActivity(selected.stepId, x.id, { mandatoryDocs })
                }
              />
            }
            label="Mandatory documents"
          />
          <ConditionBuilder
            value={x.condition}
            variables={d.variables}
            onChange={(condition) => s.updateActivity(selected.stepId, x.id, { condition })}
          />
        </Stack>
        <Stack spacing={1}>
          {text('Performer', x.performer, (performer) =>
            s.updateActivity(selected.stepId, x.id, { performer })
          )}
          <TextField
            select
            size="small"
            label="Assignment mode"
            value={x.assignmentMode}
            onChange={(e) =>
              s.updateActivity(selected.stepId, x.id, {
                assignmentMode: e.target.value as typeof x.assignmentMode,
              })
            }
          >
            {['any', 'all', 'round-robin'].map((mode) => (
              <MenuItem key={mode} value={mode}>
                {mode}
              </MenuItem>
            ))}
          </TextField>
        </Stack>
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
      </Stack>
    );
  }
  if (selected.kind === 'requestControl') {
    const control = d.requestControls.find((x) => x.id === selected.id);
    if (!control) return null;
    const update = (values: Partial<typeof control>) => s.updateRequestControl(control.id, values);
    return (
      <Stack spacing="8px" sx={{ p: '10px' }}>
        <SettingsTitle title="Request Control" dirty={s.dirty} isNew />
        {text(
          'Control code',
          `RCTL-${String(d.requestControls.indexOf(control) + 1).padStart(4, '0')}`,
          () => undefined
        )}
        {text('Label', control.label, (label) => update({ label }))}
        {text('Arabic label', control.labelAR, (labelAR) => update({ labelAR }))}
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
        {(control.type === 'dropdown-db' ||
          control.type === 'dropdown-manual' ||
          control.type === 'checkboxlist' ||
          control.type === 'radiobuttonlist') &&
          text('Options', control.options.join(', '), (value) =>
            update({
              options: value
                .split(',')
                .map((x) => x.trim())
                .filter(Boolean),
            })
          )}
        <Box sx={{ ...settingsGroupSx, gridTemplateColumns: '1fr 1fr' }}>
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={control.required}
                onChange={(_, required) => update({ required })}
              />
            }
            label="Mandatory"
          />
          <FormControlLabel control={<Switch size="small" checked={control.uniqueKey} onChange={(_, uniqueKey) => update({ uniqueKey })} />} label="Unique Key" />
          <FormControlLabel control={<Switch size="small" checked={control.usedAsCriteria} onChange={(_, usedAsCriteria) => update({ usedAsCriteria })} />} label="Criteria" />
          <FormControlLabel control={<Switch size="small" checked={control.visible} onChange={(_, visible) => update({ visible })} />} label="Active" />
        </Box>
        <TextField
          select
          size="small"
          label="Binding rule (Variable)"
          value={control.visibilityCondition?.variableId ?? ''}
          onChange={(e) =>
            update({
              visibilityCondition: e.target.value
                ? { variableId: e.target.value, operator: '=', value: '' }
                : null,
            })
          }
        >
          <MenuItem value="">None</MenuItem>
          {d.variables.map((variable) => (
            <MenuItem key={variable.id} value={variable.id}>
              {variable.name} · {variable.dataType}
            </MenuItem>
          ))}
        </TextField>
        <Button variant="contained" disabled sx={{ alignSelf: 'stretch' }}>
          Create Request Control
        </Button>
        <ValidationRules
          values={control.validations}
          onChange={(validations) => update({ validations })}
        />
        <TransitionRules
          values={d.transitions}
          variables={d.variables}
          steps={d.steps}
          onAdd={s.addTransition}
          onUpdate={s.updateTransition}
          onRemove={s.removeTransition}
        />
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
    return (
      <Stack spacing="8px" sx={{ p: '10px' }}>
        <SettingsTitle title="Control Settings" dirty={s.dirty} isNew />
        {text('Code', control.code, (code) => update({ code }))}
        {text('Label', control.label, (label) => update({ label }))}
        {text('Arabic label', control.labelAR, (labelAR) => update({ labelAR }))}
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
        <Box sx={{ ...settingsGroupSx, gridTemplateColumns: '1fr 1fr' }}>
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={control.required}
                onChange={(_, required) => update({ required })}
              />
            }
            label="Required"
          />
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={control.readOnly}
                onChange={(_, readOnly) => update({ readOnly })}
              />
            }
            label="ReadOnly"
          />
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
        <ValidationRules
          values={control.validations}
          onChange={(validations) => update({ validations })}
        />
        <TransitionRules
          values={d.transitions}
          variables={d.variables}
          steps={d.steps}
          onAdd={s.addTransition}
          onUpdate={s.updateTransition}
          onRemove={s.removeTransition}
        />
        <Typography sx={{ fontSize: tokens.fontSize.body, fontWeight: 600 }}>Visibility Rule</Typography>
        <ConditionBuilder
          value={control.visibilityCondition}
          variables={d.variables}
          onChange={(visibilityCondition) => update({ visibilityCondition })}
        />
      </Stack>
    );
  }
  if (selected.kind === 'transition') {
    const x = d.transitions.find((v) => v.id === selected.id);
    if (!x) return null;
    return (
      <Stack spacing="8px" sx={{ p: '10px' }}>
        <SettingsTitle title="Transition Settings" dirty={s.dirty} />
        {text('Name', x.name, (name) => s.updateTransition(x.id, { name }))}
        {text('Value', x.value, (value) => s.updateTransition(x.id, { value }))}
      </Stack>
    );
  }
  return (
    <Typography color="text.secondary" sx={{ p: '10px', fontSize: tokens.fontSize.secondary }}>
      Select an item to edit its properties.
    </Typography>
  );
}
