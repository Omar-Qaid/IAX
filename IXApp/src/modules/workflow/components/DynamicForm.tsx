import React from 'react';
import { Alert, Box, Button, CircularProgress, Paper, Stack, Typography } from '@mui/material';
import AssignmentOutlined from '@mui/icons-material/AssignmentOutlined';
import { useQuery } from '@tanstack/react-query';
import { useNotifications } from '@shared/hooks/useNotifications';
import { documentApi } from '@shared/components/documents/documentApi';
import { documentTableIds } from '@shared/components/documents/recordTableIds';
import {
  dynamicRequestFormApi,
  type DynamicRequestCondition,
  type DynamicRequestControl,
  type DynamicRequestOption,
  type DynamicRequestValidation,
} from '../api/dynamicRequestFormApi';
import { DynamicControlRenderer, readMultiValue } from './DynamicControlRenderer';
import { readFileMetadata } from './DynamicSpecialControls';
import { useAppTranslation } from '@core/localization/useAppTranslation';

type Values = Record<number, string>;
type Errors = Record<number, string>;
const normalized = (value: string) => value.replace(/[^a-z0-9]/gi, '').toLocaleLowerCase();
const empty = (value: string) => !value.trim() || value === '[]';
const selectedValues = (value: string) => value.trimStart().startsWith('[') ? readMultiValue(value) : value ? [value] : [];
const selectedOptions = (control: DynamicRequestControl, values: Values): DynamicRequestOption[] => {
  const selected = selectedValues(values[control.requestControlId] ?? control.defaultValue ?? '');
  return control.options.filter((option) => selected.includes(option.value));
};
const compareCondition = (condition: DynamicRequestCondition | null, values: Values): boolean => {
  if (!condition) return true;
  const actual = values[condition.sourceControlId] ?? '';
  const expected = condition.value ?? '';
  const left = Number(actual); const right = Number(expected);
  switch (condition.operator) {
    case '!=': case '<>': return actual.toLocaleLowerCase() !== expected.toLocaleLowerCase();
    case '>': return Number.isFinite(left) && Number.isFinite(right) && left > right;
    case '<': return Number.isFinite(left) && Number.isFinite(right) && left < right;
    case '>=': return Number.isFinite(left) && Number.isFinite(right) && left >= right;
    case '<=': return Number.isFinite(left) && Number.isFinite(right) && left <= right;
    case 'contains': return selectedValues(actual).some((item) => item.toLocaleLowerCase() === expected.toLocaleLowerCase()) || actual.toLocaleLowerCase().includes(expected.toLocaleLowerCase());
    case 'isEmpty': return empty(actual);
    default: return actual.toLocaleLowerCase() === expected.toLocaleLowerCase() || selectedValues(actual).some((item) => item.toLocaleLowerCase() === expected.toLocaleLowerCase());
  }
};
const compareValues = (left: string, right: string, operator: string): boolean => {
  const leftNumber = Number(left); const rightNumber = Number(right);
  if (left.trim() && right.trim() && Number.isFinite(leftNumber) && Number.isFinite(rightNumber)) {
    switch (operator) {
      case '>': return leftNumber > rightNumber;
      case '<': return leftNumber < rightNumber;
      case '>=': return leftNumber >= rightNumber;
      case '<=': return leftNumber <= rightNumber;
      case '!=': case '<>': return leftNumber !== rightNumber;
      default: return leftNumber === rightNumber;
    }
  }
  const equal = left.trim().replace(/^['"]|['"]$/g, '').toLocaleLowerCase() ===
    right.trim().replace(/^['"]|['"]$/g, '').toLocaleLowerCase();
  return operator === '!=' || operator === '<>' ? !equal : equal;
};
const replaceValueTokens = (expression: string, currentValue: string, controls: DynamicRequestControl[], values: Values) => {
  let result = expression.replaceAll('{value}', currentValue);
  for (const control of controls) {
    const fieldValue = values[control.requestControlId] ?? control.defaultValue ?? '';
    result = result.replaceAll(`{${control.code}}`, fieldValue)
      .replaceAll(`{${control.requestControlId}}`, fieldValue);
  }
  return result;
};
const configuredRuleValid = (rule: DynamicRequestValidation, value: string, controls: DynamicRequestControl[], values: Values) => {
  const expression = replaceValueTokens(rule.expression ?? '', value, controls, values);
  if (expression) {
    for (const operator of ['>=', '<=', '!=', '==', '=', '>', '<']) {
      const index = expression.indexOf(operator);
      if (index >= 0) return compareValues(expression.slice(0, index), expression.slice(index + operator.length), operator);
    }
    return false;
  }
  const operand = replaceValueTokens(rule.value ?? '', value, controls, values);
  return compareValues(value, operand, rule.operator ?? '=');
};
const ruleValid = (rule: DynamicRequestValidation, value: string, controls: DynamicRequestControl[], values: Values): boolean => {
  const type = normalized(rule.type); const operand = rule.value ?? rule.expression ?? '';
  if (empty(value) && type !== 'required') return true;
  switch (type) {
    case 'required': return !empty(value);
    case 'minlength': return value.length >= Number(operand);
    case 'maxlength': return value.length <= Number(operand);
    case 'exactlength': case 'length': return value.length === Number(operand);
    case 'minvalue': return Number(value) >= Number(operand);
    case 'maxvalue': return Number(value) <= Number(operand);
    case 'range': return Number(value) >= Number(rule.value) && Number(value) <= Number(rule.expression);
    case 'regex': case 'pattern': try { return new RegExp(rule.expression ?? '').test(value); } catch { return false; }
    case 'email': return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(value);
    case 'url': try { const url = new URL(value); return ['http:', 'https:'].includes(url.protocol); } catch { return false; }
    case 'phone': return /^[+0-9\s()-]{7,20}$/.test(value);
    case 'saudimobile': return /^(?:\+966|00966|966|0)?5\d{8}$/.test(value);
    case 'saudinationalid': return /^[12]\d{9}$/.test(value);
    case 'saudiiban': return /^SA\d{22}$/.test(value.replace(/\s/g, ''));
    case 'taxnumber': return /^\d{15}$/.test(value);
    case 'passport': return /^[A-Za-z0-9]{6,12}$/.test(value);
    case 'startswith': return value.toLocaleLowerCase().startsWith(operand.toLocaleLowerCase());
    case 'endswith': return value.toLocaleLowerCase().endsWith(operand.toLocaleLowerCase());
    case 'contains': return value.toLocaleLowerCase().includes(operand.toLocaleLowerCase());
    case 'fileextensions': case 'fileextension': case 'allowedextensions': case 'allowedtypes': {
      const allowed = operand.split(',').map((item) => item.trim().toLocaleLowerCase().replace(/^\./, '')).filter(Boolean);
      return readFileMetadata(value).every((file) => allowed.includes(file.name.split('.').pop()?.toLocaleLowerCase() ?? '') || allowed.includes(file.type.toLocaleLowerCase()));
    }
    case 'filesize': case 'maxfilesize': {
      const match = operand.trim().match(/^(\d+(?:\.\d+)?)\s*(b|kb|mb|gb)?$/i); if (!match) return false;
      const unit = (match[2] ?? 'mb').toLocaleLowerCase(); const limit = Number(match[1]) * ({ b: 1, kb: 1024, mb: 1024 ** 2, gb: 1024 ** 3 }[unit] ?? 1024 ** 2);
      return readFileMetadata(value).every((file) => file.size <= limit);
    }
    case 'minselected': return (readFileMetadata(value).length || selectedValues(value).length) >= Number(operand);
    case 'maxselected': case 'maxfiles': return (readFileMetadata(value).length || selectedValues(value).length) <= Number(operand);
    case 'compare': case 'comparison': case 'crossfield': case 'expression':
    case 'custom': case 'customexpression': case 'conditional':
      return configuredRuleValid(rule, value, controls, values);
    default: return true;
  }
};
const initialValues = (controls: DynamicRequestControl[]): Values => Object.fromEntries(
  controls.map((control) => [control.requestControlId, control.defaultValue ?? (normalized(control.controlType) === 'checkbox' ? 'false' : '')])
);
export interface DynamicFormHandle { submit: () => void }
export interface DynamicFormStatus { score: number; saving: boolean; canSubmit: boolean; requestId: number | null }
export const DynamicForm = React.forwardRef<DynamicFormHandle, { processId: number; requestFiles?: File[]; showActions?: boolean; onStatusChange?: (status: DynamicFormStatus) => void }>(function DynamicForm({ processId, requestFiles = [], showActions = true, onStatusChange }, ref): React.ReactElement {
  const { t } = useAppTranslation();
  const { notifyError, notifySuccess } = useNotifications();
  const definition = useQuery({
    queryKey: ['workflow', 'dynamic-request-form', processId],
    queryFn: ({ signal }) => dynamicRequestFormApi.getDefinition(processId, signal),
    enabled: Number.isSafeInteger(processId) && processId > 0,
  });
  const [values, setValues] = React.useState<Values>({});
  const [errors, setErrors] = React.useState<Errors>({});
  const [optionFeatureValues, setOptionFeatureValues] = React.useState<Record<string, string>>({});
  const [controlFiles, setControlFiles] = React.useState<Record<number, File[]>>({});
  const [optionFiles, setOptionFiles] = React.useState<Record<string, File[]>>({});
  const [formError, setFormError] = React.useState('');
  const [saving, setSaving] = React.useState(false);
  const [savedRequestId, setSavedRequestId] = React.useState<number | null>(null);

  React.useEffect(() => {
    if (!definition.data) return;
    setErrors({}); setFormError(''); setOptionFeatureValues({}); setControlFiles({}); setOptionFiles({}); setSavedRequestId(null);
    setValues(initialValues(definition.data.controls));
  }, [definition.data]);

  const sortedControls = React.useMemo(() => [...(definition.data?.controls ?? [])].sort((left, right) =>
    (left.sortOrder ?? Number.MAX_SAFE_INTEGER) - (right.sortOrder ?? Number.MAX_SAFE_INTEGER) ||
    left.requestControlId - right.requestControlId
  ), [definition.data?.controls]);
  const visibleControls = React.useMemo(() => {
    const controls = sortedControls;
    const optionControlledIds = new Set(controls.flatMap((control) =>
      control.options.flatMap((option) => option.featureConfiguration?.showOtherControls
        ? option.featureConfiguration.visibleControlIds
        : [])
    ));
    const visibleIds = new Set(controls
      .filter((control) => !control.visibilityCondition && !optionControlledIds.has(control.requestControlId))
      .map((control) => control.requestControlId));
    let changed = true;
    while (changed) {
      changed = false;
      for (const control of controls) {
        const condition = control.visibilityCondition;
        if (condition && !visibleIds.has(control.requestControlId) && visibleIds.has(condition.sourceControlId) && compareCondition(condition, values)) {
          visibleIds.add(control.requestControlId); changed = true;
        }
      }
      for (const control of controls.filter((item) => visibleIds.has(item.requestControlId))) {
        for (const option of selectedOptions(control, values)) {
          const features = option.featureConfiguration;
          for (const id of features?.showOtherControls ? features.visibleControlIds : []) {
            if (!visibleIds.has(id)) { visibleIds.add(id); changed = true; }
          }
        }
      }
    }
    return controls.filter((control) => visibleIds.has(control.requestControlId));
  }, [sortedControls, values]);
  const score = React.useMemo(() => visibleControls.filter((control) => normalized(control.controlType) !== 'label').reduce((total, control) => {
    const value = values[control.requestControlId] ?? control.defaultValue ?? '';
    if (control.options.length) {
      const selected = selectedValues(value);
      return total + control.options.filter((option) => selected.includes(option.value)).reduce((sum, option) => sum + option.score, 0);
    }
    return empty(value) || normalized(control.controlType) === 'checkbox' && value !== 'true' ? total : total + control.score;
  }, 0), [values, visibleControls]);
  const inputControls = React.useMemo(
    () => visibleControls.filter((control) => normalized(control.controlType) !== 'label'),
    [visibleControls]
  );
  const inlineChildIds = React.useMemo(() => new Set(visibleControls.flatMap((control) =>
    selectedOptions(control, values).flatMap((option) =>
      option.featureConfiguration?.showOtherControls
        ? option.featureConfiguration.visibleControlIds
        : []
    )
  )), [values, visibleControls]);
  const topLevelControls = React.useMemo(
    () => visibleControls.filter((control) => !inlineChildIds.has(control.requestControlId)),
    [inlineChildIds, visibleControls]
  );
  const controlRows = React.useMemo(() => {
    const rows: DynamicRequestControl[][] = [];
    let row: DynamicRequestControl[] = [];
    let used = 0;
    for (const control of topLevelControls) {
      const span = Math.min(3, Math.max(1, control.columnSpan || 1));
      if (row.length > 0 && used + span > 3) { rows.push(row); row = []; used = 0; }
      row.push(control); used += span;
      if (used === 3) { rows.push(row); row = []; used = 0; }
    }
    if (row.length > 0) rows.push(row);
    return rows;
  }, [topLevelControls]);
  const validate = (): boolean => {
    const next: Errors = {};
    for (const control of inputControls) {
      const value = values[control.requestControlId] ?? '';
      if (control.required && empty(value)) next[control.requestControlId] = t('validation.required', { field: control.label });
      const missingFeatureFile = selectedOptions(control, values).find((option) =>
        option.featureConfiguration?.requireFileUpload &&
        empty(optionFeatureValues[`${control.requestControlId}:${option.optionId}:files`] ?? '')
      );
      if (missingFeatureFile) next[control.requestControlId] = t('workflowRequest.fileRequired', { field: missingFeatureFile.label });
      const failed = control.validations.find((rule) =>
        rule.severity.toLocaleLowerCase() === 'error' &&
        !ruleValid(rule, value, definition.data?.controls ?? [], values));
      if (failed) next[control.requestControlId] = failed.errorMessage;
    }
    setErrors(next);
    return Object.keys(next).length === 0;
  };
  const submit = async () => {
    setFormError('');
    if (!validate()) return;
    setSaving(true);
    try {
      const result = await dynamicRequestFormApi.submit({
        processId,
        values: inputControls.map((control) => ({ requestControlId: control.requestControlId, value: values[control.requestControlId] ?? control.defaultValue ?? '' })),
        optionFeatureValues: inputControls.flatMap((control) => selectedOptions(control, values)
          .filter((option) => option.featureConfiguration?.requireFileUpload)
          .map((option) => ({
            optionId: option.optionId,
            fileValue: optionFeatureValues[`${control.requestControlId}:${option.optionId}:files`] ?? '',
          }))),
      });
      const uploads: Promise<unknown>[] = [];
      const attachmentOwners = result.attachmentOwners ?? [];
      for (const file of requestFiles) uploads.push(documentApi.create(documentTableIds.wfRequest, result.requestId, {
        typeId: 'File', name: file.name, notes: t('workflowRequest.attachmentNote'), url: '', file,
      }));
      for (const [requestControlIdText, files] of Object.entries(controlFiles)) {
        const requestControlId = Number(requestControlIdText);
        const owner = attachmentOwners.find((item) => item.requestControlId === requestControlId && item.optionId == null);
        if (!owner) continue;
        for (const file of files) uploads.push(documentApi.create(documentTableIds.wfRequestDetail, owner.detailRecId, {
          typeId: 'File', name: file.name, notes: t('workflowRequest.controlAttachmentNote', { id: requestControlId }), url: '', file,
        }));
      }
      for (const [key, files] of Object.entries(optionFiles)) {
        const [requestControlId, optionId] = key.split(':').map(Number);
        const owner = attachmentOwners.find((item) => item.requestControlId === requestControlId && item.optionId === optionId);
        if (!owner) continue;
        for (const file of files) uploads.push(documentApi.create(documentTableIds.wfRequestDetail, owner.detailRecId, {
          typeId: 'File', name: file.name, notes: t('workflowRequest.optionAttachmentNote', { id: optionId }), url: '', file,
        }));
      }
      const uploadResults = await Promise.allSettled(uploads);
      const failedUploads = uploadResults.filter((item) => item.status === 'rejected').length;
      setSavedRequestId(result.requestId);
      notifySuccess(t('workflowRequest.submitted', { request: result.code ?? result.requestId, score: result.score }));
      if (failedUploads > 0) notifyError(t('workflowRequest.uploadsFailed', { count: failedUploads }));
    } catch (reason) {
      const message = reason instanceof Error ? reason.message : String(reason);
      setFormError(message); notifyError(message);
    } finally { setSaving(false); }
  };
  const submitRef = React.useRef(submit);
  submitRef.current = submit;
  React.useImperativeHandle(ref, () => ({ submit: () => { void submitRef.current(); } }), []);
  React.useEffect(() => {
    onStatusChange?.({ score, saving, canSubmit: inputControls.length > 0 && savedRequestId == null, requestId: savedRequestId });
  }, [inputControls.length, onStatusChange, savedRequestId, saving, score]);

  if (definition.isLoading) return <Box sx={{ py: 6, display: 'grid', placeItems: 'center' }}><CircularProgress aria-label={t('workflowRequest.loadingForm')} /></Box>;
  if (definition.isError) return <Alert severity="error">{definition.error instanceof Error ? definition.error.message : t('workflowRequest.loadFailed')}</Alert>;
  if (!definition.data) return <Alert severity="warning">{t('workflowRequest.noDefinition')}</Alert>;
  const childControlsFor = (option: DynamicRequestOption) => option.featureConfiguration?.showOtherControls
    ? option.featureConfiguration.visibleControlIds
      .map((id) => visibleControls.find((item) => item.requestControlId === id))
      .filter((item): item is DynamicRequestControl => Boolean(item))
    : [];
  const usesFullDependencyRow = (option: DynamicRequestOption) => {
    const children = childControlsFor(option);
    return children.length >= 2 || children.some((child) => ['table', 'longtext', 'textarea'].includes(normalized(child.controlType)));
  };
  const renderDependency = (control: DynamicRequestControl, option: DynamicRequestOption, ancestors: Set<number>, fullRow: boolean): React.ReactNode => {
    const features = option.featureConfiguration;
    if (!features) return null;
    const featureKey = `${control.requestControlId}:${option.optionId}`;
    const childControls = childControlsFor(option);
    if (!features.sendAlertMessage && !features.requireFileUpload && childControls.length === 0) return null;
    return <Stack key={featureKey} role="group" aria-label={t('workflowRequest.additionalFields', { option: option.label })} spacing={0.7} sx={{ mt: fullRow ? 0.75 : 0.65, marginInlineStart: fullRow ? 0 : { xs: 0.25, sm: 0.75 }, p: fullRow ? 1 : 0.65, borderInlineStart: '3px solid', borderColor: 'primary.light', borderRadius: 0.6, bgcolor: 'rgba(0, 91, 161, .035)' }}>
      {fullRow && <Typography sx={{ fontSize: 12.5, fontWeight: 750 }}>{t('workflowRequest.optionDetails', { option: option.label })}</Typography>}
      {features.sendAlertMessage && features.alertMessage && <Alert severity="warning" sx={{ py: 0, '& .MuiAlert-icon': { py: 0.35 }, '& .MuiAlert-message': { py: 0.35, fontSize: 11.5 } }}>{features.alertMessage}</Alert>}
      {childControls.length > 0 && <Box sx={{ display: 'grid', gridTemplateColumns: fullRow ? { xs: '1fr', md: 'repeat(2, minmax(0, 1fr))', lg: 'repeat(3, minmax(0, 1fr))' } : '1fr', gap: 0.8, alignItems: 'start' }}>
        {childControls.map((child) => <Box key={child.requestControlId} sx={{ minWidth: 0 }}>{renderControl(child, ancestors, false)}</Box>)}
      </Box>}
      {features.requireFileUpload && <DynamicControlRenderer
        control={{ label: t('workflowRequest.supportingDocument'), hideLabel: true, compact: true, required: true, controlType: 'file' }}
        value={optionFeatureValues[`${featureKey}:files`] ?? ''}
        onChange={(value) => setOptionFeatureValues((current) => ({ ...current, [`${featureKey}:files`]: value }))}
        onFilesChange={(files) => setOptionFiles((current) => ({ ...current, [featureKey]: files }))}
        preview={savedRequestId != null}
      />}
    </Stack>;
  };
  const renderControl = (control: DynamicRequestControl, ancestors = new Set<number>(), externalizeFullRows = false): React.ReactNode => {
    if (ancestors.has(control.requestControlId)) return null;
    const nextAncestors = new Set(ancestors).add(control.requestControlId);
    const activeOptions = selectedOptions(control, values);
    return <Box key={control.requestControlId} sx={{ minWidth: 0 }}>
    <DynamicControlRenderer control={{
      label: control.label, labelColor: control.labelColor, controlType: control.controlType, required: control.required,
      readOnly: control.readOnly, defaultValue: control.defaultValue,
      options: control.options.map((option) => ({
        value: option.value,
        label: option.label,
        sendsNotification: option.featureConfiguration?.sendAlertMessage,
        requiresAttachment: option.featureConfiguration?.requireFileUpload,
        revealsControls: option.featureConfiguration?.showOtherControls && option.featureConfiguration.visibleControlIds.length > 0,
      })),
      validations: control.validations,
    }} value={values[control.requestControlId] ?? ''} onChange={(value) => {
      setValues((current) => ({ ...current, [control.requestControlId]: value }));
      setErrors((current) => { const next = { ...current }; delete next[control.requestControlId]; return next; });
    }} onFilesChange={(files) => setControlFiles((current) => ({ ...current, [control.requestControlId]: files }))}
      preview={savedRequestId != null} error={Boolean(errors[control.requestControlId])} helperText={errors[control.requestControlId]} />
    {activeOptions.filter((option) => !externalizeFullRows || !usesFullDependencyRow(option)).map((option) => renderDependency(control, option, nextAncestors, false))}
  </Box>;
  };
  return (
    <Box sx={{ width: '100%', minWidth: 0, minHeight: 0, height: 'auto', flex: '0 0 auto', alignSelf: 'flex-start', display: 'flex', flexDirection: 'column', gap: 1.15 }}>
      {visibleControls.length === 0 ? <Alert severity="info">{t('workflowRequest.noActiveControls')}</Alert> : <Stack spacing={1.25} sx={{ width: '100%', maxWidth: 'none', minWidth: 0, minHeight: 0, height: 'auto', flex: '0 0 auto', alignSelf: 'flex-start' }}>
        <Paper variant="outlined" sx={{ width: '100%', minWidth: 0, minHeight: 0, height: 'auto', alignSelf: 'flex-start', p: { xs: 1.1, sm: 1.35 }, borderColor: '#d5d7dc', borderRadius: 1, boxShadow: '0 2px 7px rgba(32,42,64,.10)' }}>
          <Box data-testid="dynamic-form-grid" sx={{ display: 'grid', gap: 1.35 }}>
            {controlRows.map((row, rowIndex) => <Box key={rowIndex} data-testid="dynamic-form-row" sx={{ minWidth: 0 }}>
              <Box sx={{ display: 'grid', gridTemplateColumns: { xs: 'minmax(0, 1fr)', md: 'repeat(2, minmax(0, 1fr))', lg: 'repeat(3, minmax(0, 1fr))' }, columnGap: 1.25, rowGap: 1.35, alignItems: 'start' }}>
                {row.map((control) => <Box key={control.requestControlId} data-control-id={control.requestControlId} sx={{ minWidth: 0, gridColumn: { xs: 'span 1', md: `span ${Math.min(2, Math.max(1, control.columnSpan || 1))}`, lg: `span ${Math.min(3, Math.max(1, control.columnSpan || 1))}` } }}>
                  {renderControl(control, new Set<number>(), true)}
                </Box>)}
              </Box>
              {row.flatMap((control) => selectedOptions(control, values).filter(usesFullDependencyRow).map((option) => renderDependency(control, option, new Set([control.requestControlId]), true)))}
            </Box>)}
          </Box>
        </Paper>
      </Stack>}
      {formError && <Alert severity="error">{formError}</Alert>}
      {showActions && <Paper square variant="outlined" sx={{ position: 'sticky', bottom: 0, zIndex: 5, mt: 'auto', px: { xs: 1.25, sm: 2.5 }, py: 0.8, mx: { xs: -0.25, sm: -0.5 }, bgcolor: 'rgba(255,255,255,.98)', backdropFilter: 'blur(8px)', boxShadow: '0 -2px 8px rgba(32,42,64,.12)' }}>
        <Stack direction="row" spacing={1.25} sx={{ alignItems: 'center', justifyContent: { xs: 'flex-start', sm: 'flex-end' } }}>
          <Button variant="contained" disabled={saving || inputControls.length === 0} onClick={() => void submit()} sx={{ minWidth: { xs: 128, sm: 190 }, height: { xs: 38, sm: 46 }, whiteSpace: 'nowrap', borderRadius: { xs: 1, sm: 0.5 }, fontSize: { xs: 12, sm: 15 } }}>{saving ? t('workflowRequest.submitting') : t('workflowRequest.submit')}</Button>
          <Box sx={{ width: 54, height: 54, flexShrink: 0, borderRadius: '50%', bgcolor: 'primary.main', color: 'primary.contrastText', display: { xs: 'grid', sm: 'none' }, placeItems: 'center', boxShadow: '0 2px 7px rgba(0,91,161,.30)' }}><Stack spacing={0} sx={{ alignItems: 'center' }}><Typography sx={{ fontSize: 18, lineHeight: 1, fontWeight: 800 }}>{score}</Typography><AssignmentOutlined sx={{ fontSize: 13, mt: 0.25 }} /></Stack></Box>
          <Box sx={{ minWidth: 0, px: { sm: 1.25 }, py: { sm: 0.7 }, border: { sm: '1px solid' }, borderColor: { sm: 'divider' }, borderRadius: { sm: 1 }, bgcolor: { sm: '#f8f8f8' } }}><Typography sx={{ fontSize: { xs: 11.5, sm: 13 }, lineHeight: 1.15, fontWeight: 750 }}>{t('workflowRequest.requestScore', { score })}</Typography><Typography color="text.secondary" sx={{ fontSize: { xs: 10.5, sm: 11.5 }, lineHeight: 1.1 }}>{t('workflowRequest.scoreHelp')}</Typography></Box>
        </Stack>
      </Paper>}
    </Box>
  );
});
