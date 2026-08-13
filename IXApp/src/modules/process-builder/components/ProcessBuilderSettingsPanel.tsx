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
import IconButton from '@mui/material/IconButton';
import { useProcessBuilderStore } from '../store/useProcessBuilderStore';
import { ConditionBuilder } from './ConditionBuilder';
import { activityPalette, controlPalette } from './ProcessBuilderPalette';
import type { BuilderValidation, BuilderValidationType } from '../types/processBuilderTypes';
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
  <Stack direction="row" spacing={1} sx={{ minHeight: 24, alignItems: 'center' }}>
    <Typography sx={{ flex: 1, fontSize: 12, fontWeight: 600, color: tokens.text }}>
      {title}
    </Typography>
    {isNew && <Chip size="small" label="New" sx={{ height: 24, bgcolor: '#eeeeee' }} />}
    {dirty && <Chip size="small" label="unsaved" sx={{ bgcolor: tokens.warning, height: 24 }} />}
  </Stack>
);

const settingsGroupSx = {
  display: 'grid',
  gap: 0.5,
  p: 1.25,
  border: `1px solid ${tokens.border}`,
  borderRadius: `${tokens.radius}px`,
  bgcolor: '#f9fafb',
};

const switchRowSx = { m: 0, minHeight: 42, justifyContent: 'space-between' };

const sectionSx = {
  boxShadow: 'none',
  border: `1px solid ${tokens.border}`,
  borderRadius: `${tokens.radius}px !important`,
  '&:before': { display: 'none' },
  '& .MuiAccordionSummary-root': { minHeight: 48 },
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
      <Typography sx={{ fontWeight: 700 }}>{title}</Typography>
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
        <Typography sx={{ flex: 1, fontSize: 10, fontWeight: 600 }}>Validation Rules</Typography>
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
            {(rule.type === 'range' || rule.type === 'crossField') && (
              <TextField
                size="small"
                label="Secondary Value"
                value={rule.secondaryValue}
                onChange={(event) => update(rule.id, { secondaryValue: event.target.value })}
                sx={{ gridColumn: '1 / -1' }}
              />
            )}
            {(rule.type === 'custom' || rule.type === 'crossField') && (
              <TextField
                size="small"
                label="Operator"
                value={rule.operator}
                onChange={(event) => update(rule.id, { operator: event.target.value })}
              />
            )}
            {(rule.type === 'phone' ||
              rule.type === 'saudiMobile' ||
              rule.type === 'saudiNationalId' ||
              rule.type === 'saudiIban') && (
              <TextField
                size="small"
                label="Input Mask"
                value={rule.mask}
                onChange={(event) => update(rule.id, { mask: event.target.value })}
              />
            )}
            <TextField
              size="small"
              label="Error Message (EN)"
              value={rule.message}
              onChange={(event) => update(rule.id, { message: event.target.value })}
              sx={{ gridColumn: '1 / -1' }}
            />
            <TextField
              size="small"
              label="Error Message (AR)"
              value={rule.messageAR}
              onChange={(event) => update(rule.id, { messageAR: event.target.value })}
              sx={{ gridColumn: '1 / -1' }}
              slotProps={{ htmlInput: { dir: 'rtl' } }}
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
          <Typography color="text.secondary" sx={{ py: 2, textAlign: 'center', fontSize: 8 }}>
            No validation rules yet.
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
      <Stack spacing="14px" sx={{ p: '16px' }}>
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
        {text('Arabic name', d.nameAR, (nameAR) => s.updateProcess({ nameAR }))}
      </Stack>
    );
  if (selected.kind === 'variable') {
    const x = d.variables.find((v) => v.id === selected.id);
    if (!x) return null;
    return (
      <Stack spacing="14px" sx={{ p: '16px' }}>
        <SettingsTitle title="Variable" dirty={s.dirty} isNew />
        {text('Code', x.code, (code) => s.updateVariable(x.id, { code }))}
        {text('Name', x.name, (name) => s.updateVariable(x.id, { name }))}
        {text('Name (AR)', x.nameAR, (nameAR) => s.updateVariable(x.id, { nameAR }))}
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
      <Stack spacing="14px" sx={{ p: '16px', minHeight: '100%' }}>
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
        {text('Step Name (AR)', x.nameAR, (nameAR) => s.updateStep(x.id, { nameAR }))}
      </Stack>
    );
  }
  if (selected.kind === 'activity') {
    const x = d.steps
      .find((v) => v.id === selected.stepId)
      ?.activities.find((v) => v.id === selected.id);
    if (!x) return null;
    return (
      <Stack spacing="14px" sx={{ p: '16px' }}>
        <SettingsTitle title="Activity Settings" dirty={s.dirty} isNew />
        {text('Activity code', x.code, (code) => s.updateActivity(selected.stepId, x.id, { code }))}
        {text('Activity name', x.name, (name) => s.updateActivity(selected.stepId, x.id, { name }))}
        <Section title="General Information" expanded>
          <Stack spacing={1.25}>
            {text('Arabic name', x.nameAR, (nameAR) =>
              s.updateActivity(selected.stepId, x.id, { nameAR })
            )}
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
        </Section>
        <Section title="Performance & SLA">
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
        </Section>
        <Section title="Notification & Alerts">
          {text('Notification emails', x.config.notifyEmails, (notifyEmails) =>
            s.updateActivity(selected.stepId, x.id, { config: { ...x.config, notifyEmails } })
          )}
        </Section>
        <Section title="Behavioral Rules">
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
        </Section>
        <Section title="Assignment Configuration" expanded>
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
        </Section>
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
      <Stack spacing="16px" sx={{ p: '16px' }}>
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
          <FormControlLabel
            control={
              <Switch
                size="small"
                checked={!control.readOnly}
                onChange={(_, visible) => update({ readOnly: !visible })}
              />
            }
            label="Active"
          />
          <FormControlLabel control={<Switch size="small" checked={false} />} label="Unique key" />
          <FormControlLabel control={<Switch size="small" checked={false} />} label="Criteria" />
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
        <Box sx={{ pt: 1.5, borderTop: '1px solid #e5e7eb' }}>
          <Stack direction="row" sx={{ alignItems: 'center' }}>
            <Typography sx={{ flex: 1, fontSize: 10, fontWeight: 600 }}>
              Transitions ({d.transitions.length})
            </Typography>
            <Button
              size="small"
              onClick={() => {
                s.addTransition();
                s.setCenterTab(7);
              }}
            >
              + Add
            </Button>
          </Stack>
          {d.transitions.length === 0 && (
            <Typography color="text.secondary" sx={{ py: 2, textAlign: 'center', fontSize: 8 }}>
              No transitions yet.
            </Typography>
          )}
        </Box>
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
      <Stack spacing="14px" sx={{ p: '16px' }}>
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
        <Box sx={settingsGroupSx}>
          <FormControlLabel
            labelPlacement="start"
            sx={switchRowSx}
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
            labelPlacement="start"
            sx={switchRowSx}
            control={
              <Switch
                size="small"
                checked={control.visible}
                onChange={(_, visible) => update({ visible })}
              />
            }
            label="Visible"
          />
          <FormControlLabel
            labelPlacement="start"
            sx={switchRowSx}
            control={
              <Switch
                size="small"
                checked={control.readOnly}
                onChange={(_, readOnly) => update({ readOnly })}
              />
            }
            label="Read only"
          />
        </Box>
        <Button variant="contained" disabled sx={{ alignSelf: 'stretch' }}>
          Create Control
        </Button>
        <ValidationRules
          values={control.validations}
          onChange={(validations) => update({ validations })}
        />
        <Typography sx={{ fontSize: 9, fontWeight: 600 }}>Visibility Rule</Typography>
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
      <Stack spacing="14px" sx={{ p: '16px' }}>
        <SettingsTitle title="Transition Settings" dirty={s.dirty} />
        {text('Name', x.name, (name) => s.updateTransition(x.id, { name }))}
        {text('Value', x.value, (value) => s.updateTransition(x.id, { value }))}
      </Stack>
    );
  }
  return (
    <Typography color="text.secondary" sx={{ p: '16px', fontSize: 9 }}>
      Select an item to edit its properties.
    </Typography>
  );
}
