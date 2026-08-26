import React, { useEffect, useRef, useState } from 'react';
import { Accordion, AccordionDetails, AccordionSummary, Alert, Box, Button, CircularProgress, Dialog, DialogActions, DialogContent, IconButton, MenuItem, TextField, Typography } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import HelpOutlineIcon from '@mui/icons-material/Help';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import { deepEqual } from '@shared/utils/deepEqual';

export type FastTabValue = string | number | boolean;
export interface FastTabOption { value: string; label: string }
export interface FastTabField {
  name: string;
  label: string;
  type?: 'text' | 'select' | 'multiline';
  required?: boolean;
  disabled?: boolean;
  options?: FastTabOption[];
  width?: number | string;
  rows?: number;
}
export interface FastTabSection { id: string; title: string; fields: FastTabField[]; defaultExpanded?: boolean; summary?: React.ReactNode }

export type FastTabsSubmitMode = 'save' | 'save-and-open';

export interface FastTabsDialogProps<TValues extends Record<string, FastTabValue>> {
  open: boolean;
  title: string;
  viewLabel: string;
  sections: FastTabSection[];
  initialValues: () => TValues;
  resetKey?: string | number;
  validate?: (values: TValues) => Record<string, string> | Promise<Record<string, string>>;
  onSubmit: (values: TValues, mode: FastTabsSubmitMode) => void | Promise<void>;
  saveLabel: string;
  saveAndOpenLabel?: string;
  cancelLabel: string;
  closeLabel?: string;
  helpLabel?: string;
  placement?: 'center' | 'top-start';
  canSave?: boolean;
  onCancel: () => void;
}

export function FastTabsDialog<TValues extends Record<string, FastTabValue>>({ open, title, sections, initialValues, resetKey, validate, onSubmit, saveLabel, saveAndOpenLabel, cancelLabel, closeLabel = 'Close', helpLabel = 'Help', placement = 'center', canSave = true, onCancel }: FastTabsDialogProps<TValues>): React.ReactElement {
  const initialValuesRef = useRef(initialValues);
  useEffect(() => { initialValuesRef.current = initialValues; }, [initialValues]);
  const [values, setValues] = useState<TValues>(() => initialValues());
  const [pristineValues, setPristineValues] = useState<TValues>(() => initialValues());
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitError, setSubmitError] = useState('');
  const [saving, setSaving] = useState(false);
  useEffect(() => { if (open) { const next = initialValuesRef.current(); setValues(next); setPristineValues(next); setErrors({}); setSubmitError(''); setSaving(false); } }, [open, resetKey]);
  const dirty = !deepEqual(values, pristineValues);
  useUnsavedChanges(open && dirty);
  const changeValue = (name: string, value: FastTabValue) => {
    setValues((current) => ({ ...current, [name]: value }));
    setErrors((current) => { const next = { ...current }; delete next[name]; return next; });
  };
  const submit = async (mode: FastTabsSubmitMode) => {
    const nextErrors = await validate?.(values) ?? {};
    setErrors(nextErrors);
    if (Object.keys(nextErrors).length) return;
    setSaving(true); setSubmitError('');
    try { await onSubmit(values, mode); }
    catch (reason: unknown) { setSubmitError(reason instanceof Error ? reason.message : String(reason)); }
    finally { setSaving(false); }
  };
  const topStart = placement === 'top-start';
  return <Dialog open={open} onClose={saving ? undefined : onCancel} maxWidth={false} fullWidth sx={topStart ? { '& .MuiDialog-container': { alignItems: 'flex-start', justifyContent: (theme) => theme.direction === 'rtl' ? 'flex-end' : 'flex-start' } } : undefined} slotProps={{ paper: { sx: (theme) => ({ width: { xs: '100vw', sm: 600 }, maxWidth: topStart ? '100vw' : 'calc(100vw - 24px)', height: { xs: '100dvh', sm: 850 }, maxHeight: topStart ? '100dvh' : 'calc(100dvh - 24px)', borderRadius: 0, m: topStart ? 0 : { xs: 0, sm: 1.5 }, direction: theme.direction, textAlign: 'start' }) } }}>
    <Box sx={{ px: { xs: 1.5, sm: 3 }, pt: 1, pb: 1.75, flexShrink: 0 }}>
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 0.25 }}>
        <IconButton size="small" aria-label={helpLabel}><HelpOutlineIcon sx={{ fontSize: 18 }} /></IconButton>
        <IconButton size="small" aria-label={closeLabel} disabled={saving} onClick={onCancel}><CloseIcon sx={{ fontSize: 18 }} /></IconButton>
      </Box>
      <Typography component="h2" sx={{ fontSize: '1.1rem', fontWeight: 600 }}>{title}</Typography>
    </Box>
    <DialogContent sx={{ px: { xs: 1.5, sm: 3 }, py: 0, overflowY: 'auto', overflowX: 'hidden', '&::-webkit-scrollbar': { width: 8 }, '&::-webkit-scrollbar-thumb': { bgcolor: '#999', borderRadius: 4 } }}>
      {submitError && <Alert severity="error" sx={{ mb: 1 }}>{submitError}</Alert>}
      {sections.map((section) => <Accordion key={section.id} defaultExpanded={section.defaultExpanded ?? true} disableGutters elevation={0} sx={{ '&::before': { display: 'none' }, borderRadius: '0 !important', borderTop: 1, borderColor: 'divider' }}>
        <AccordionSummary expandIcon={<ExpandMoreIcon sx={{ fontSize: 17 }} />} sx={{ px: 1.25, minHeight: 44, '&.Mui-expanded': { minHeight: 44 }, '& .MuiAccordionSummary-content, & .MuiAccordionSummary-content.Mui-expanded': { my: 0.75 }, '& .MuiAccordionSummary-expandIconWrapper': { border: 1, borderColor: 'divider', borderRadius: 0.5, p: 0.25 } }}>
          <Typography sx={{ fontSize: '0.875rem', fontWeight: 600 }}>{section.title}</Typography><Box sx={{ marginInlineStart: 'auto', marginInlineEnd: 1 }}>{section.summary}</Box>
        </AccordionSummary>
        <AccordionDetails sx={{ px: 1.25, pt: 0.75, pb: 1.5, borderTop: 1, borderColor: 'divider' }}>
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, columnGap: 5.75, rowGap: 1.05 }}>
            {section.fields.map((field) => <Box key={field.name} sx={{ minWidth: 0 }}>
              <Typography sx={{ mb: 0.2, display: 'flex', gap: 0.5, fontSize: '0.6875rem', color: 'text.secondary' }}>{field.label}{field.required && <Box component="span" sx={{ marginInlineStart: 'auto', color: 'error.main', fontSize: '0.9rem' }}>*</Box>}</Typography>
              <TextField select={field.type === 'select'} multiline={field.type === 'multiline'} rows={field.type === 'multiline' ? (field.rows ?? 4) : undefined} value={values[field.name] ?? ''} disabled={saving || field.disabled} error={Boolean(errors[field.name])} helperText={errors[field.name]} slotProps={{ input: { 'aria-label': field.label, 'aria-required': field.required } }} onChange={(event) => changeValue(field.name, event.target.value)} sx={{ width: { xs: '100%', sm: field.width ?? '100%' }, maxWidth: '100%', '& .MuiInputBase-root': { minHeight: field.type === 'multiline' ? 112 : 29, borderRadius: 0.5, fontSize: '0.75rem' }, '& .MuiInputBase-input': { px: 0.75, py: 0.5 }, '& .MuiFormHelperText-root': { mx: 0, fontSize: '0.65rem' } }}>
                {(field.options ?? []).map((option) => <MenuItem key={option.value} value={option.value} sx={{ fontSize: '0.78rem' }}>{option.label}</MenuItem>)}
              </TextField>
            </Box>)}
          </Box>
        </AccordionDetails>
      </Accordion>)}
    </DialogContent>
    <DialogActions sx={{ px: 2, py: 1.25, borderTop: 1, borderColor: 'divider', flexShrink: 0 }}>
      <Button variant="contained" size="small" disabled={!canSave || saving} startIcon={saving ? <CircularProgress size={13} /> : undefined} onClick={() => void submit('save')}>{saveLabel}</Button>
      {saveAndOpenLabel && <Button variant="outlined" size="small" disabled={!canSave || saving} endIcon={<ExpandMoreIcon />} onClick={() => void submit('save-and-open')}>{saveAndOpenLabel}</Button>}
      <Button variant="outlined" size="small" disabled={saving} onClick={onCancel}>{cancelLabel}</Button>
    </DialogActions>
  </Dialog>;
}
