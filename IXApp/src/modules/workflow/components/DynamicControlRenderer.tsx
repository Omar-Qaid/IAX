import React from 'react';
import {
  Box,
  Checkbox,
  FormControl,
  FormControlLabel,
  FormGroup,
  FormHelperText,
  FormLabel,
  MenuItem,
  Radio,
  RadioGroup,
  TextField,
  Typography,
  Tooltip,
} from '@mui/material';
import AttachFileOutlined from '@mui/icons-material/AttachFileOutlined';
import NotificationsNoneOutlined from '@mui/icons-material/NotificationsNoneOutlined';
import SubdirectoryArrowRightOutlined from '@mui/icons-material/SubdirectoryArrowRightOutlined';
import { FileDropControl, LocationControl, SignatureControl } from './DynamicSpecialControls';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { RenderableControl, RenderableOption } from './dynamicControlTypes';
export type {
  RenderableControl,
  RenderableOption,
  RenderableValidation,
} from './dynamicControlTypes';

export const normalizeDynamicControlType = (value: string): string =>
  value.replace(/[^a-z0-9]/gi, '').toLocaleLowerCase();

export const readMultiValue = (value: string): string[] => {
  if (!value) return [];
  try {
    const parsed = JSON.parse(value) as unknown;
    return Array.isArray(parsed) ? parsed.filter((item): item is string => typeof item === 'string') : [];
  } catch { return []; }
};

function OptionLabel({ option }: { option: RenderableOption }) {
  const { t } = useAppTranslation();
  return <Box component="span" sx={{ display: 'inline-flex', alignItems: 'center', gap: 0.45, minWidth: 0 }}>
    <Box component="span" sx={{ overflow: 'hidden', textOverflow: 'ellipsis' }}>{option.label}</Box>
    {option.sendsNotification && <Tooltip title={t('workflowRequest.sendsNotification')} arrow><NotificationsNoneOutlined color="action" sx={{ fontSize: 15 }} /></Tooltip>}
    {option.requiresAttachment && <Tooltip title={t('workflowRequest.attachmentRequired')} arrow><AttachFileOutlined color="action" sx={{ fontSize: 15 }} /></Tooltip>}
    {option.revealsControls && <Tooltip title={t('workflowRequest.showsAdditionalFields')} arrow><SubdirectoryArrowRightOutlined color="action" sx={{ fontSize: 15 }} /></Tooltip>}
  </Box>;
}

export function DynamicControlRenderer({
  control,
  value,
  onChange,
  error,
  helperText,
  preview = false,
  onFilesChange,
}: {
  control: RenderableControl;
  value: string;
  onChange: (value: string) => void;
  error?: boolean;
  helperText?: string;
  preview?: boolean;
  onFilesChange?: (files: File[]) => void;
}): React.ReactElement {
  const { t } = useAppTranslation();
  const type = normalizeDynamicControlType(control.controlType);
  const options = control.options ?? [];
  const disabled = Boolean(control.readOnly || preview);
  if (type === 'label') {
    const noteColor = control.labelColor || '#7a4b00';
    return <Box role="note" sx={{ px: 1.25, py: 1, borderInlineStart: '4px solid', borderColor: noteColor, bgcolor: 'rgba(245, 158, 11, .08)', borderRadius: 0.75 }}>
      <Typography sx={{ color: noteColor, fontSize: 13, lineHeight: 1.45, fontWeight: 800 }}>{control.label}</Typography>
    </Box>;
  }
  if (type === 'checkbox') return (
    <FormControl error={error} required={control.required}>
      <FormControlLabel
        control={<Checkbox checked={value === 'true'} disabled={disabled} onChange={(_, checked) => onChange(String(checked))} />}
        label={<Typography component="span" sx={{ fontSize: 'inherit', fontWeight: 700 }}>{control.label}</Typography>}
      />
      {helperText && <FormHelperText>{helperText}</FormHelperText>}
    </FormControl>
  );
  if (type.includes('checkboxlist')) {
    const selected = readMultiValue(value);
    return (
      <FormControl error={error} required={control.required}>
        <FormLabel sx={{ fontWeight: 700 }}>{control.label}</FormLabel>
        <FormGroup row>
          {options.map((option) => <FormControlLabel key={option.value} label={<OptionLabel option={option} />} control={
            <Checkbox checked={selected.includes(option.value)} disabled={disabled} onChange={(_, checked) => {
              const next = checked ? [...selected, option.value] : selected.filter((item) => item !== option.value);
              onChange(JSON.stringify(next));
            }} />
          } />)}
        </FormGroup>
        {helperText && <FormHelperText>{helperText}</FormHelperText>}
      </FormControl>
    );
  }
  if (type === 'radio' || type.includes('radiobutton')) return (
    <FormControl error={error} required={control.required}>
      <FormLabel sx={{ fontWeight: 700 }}>{control.label}</FormLabel>
      <RadioGroup row value={value} onChange={(event) => onChange(event.target.value)}>
        {options.map((option) => <FormControlLabel key={option.value} value={option.value} label={<OptionLabel option={option} />} control={<Radio disabled={disabled} />} />)}
      </RadioGroup>
      {helperText && <FormHelperText>{helperText}</FormHelperText>}
    </FormControl>
  );
  if (type === 'file') return <FileDropControl control={control} value={value} onChange={onChange} onFilesChange={onFilesChange} error={error} helperText={helperText} preview={preview} />;
  if (type === 'signature') return <SignatureControl control={control} value={value} onChange={onChange} error={error} helperText={helperText} preview={preview} />;
  if (type === 'location') return <LocationControl control={control} value={value} onChange={onChange} error={error} helperText={helperText} preview={preview} />;
  if (type === 'table') return (
    <Box>
      <Typography variant="body2" sx={{ mb: 0.5, fontWeight: 700 }}>{control.label}</Typography>
      <Box sx={{ minHeight: 72, maxHeight: 240, overflow: 'auto', border: '1px dashed', borderColor: error ? 'error.main' : 'divider', display: 'grid', placeItems: 'center', color: 'text.secondary' }}>{t('workflowRequest.tableData')}</Box>
      {helperText && <FormHelperText error={error}>{helperText}</FormHelperText>}
    </Box>
  );
  const select = type === 'select' || type === 'combobox' || type.includes('dropdown') ||
    (type === 'showroom' && options.length > 0);
  const inputType = type === 'digits' || type === 'number' || type === 'employeeid' ? 'number'
    : type === 'date' || type === 'calendar' ? 'date' : type === 'time' ? 'time'
      : type === 'url' ? 'url' : type === 'email' ? 'email' : 'text';
  const multiline = type === 'longtext' || type === 'textarea';
  return (
    <Box sx={{ display: 'grid', gridTemplateColumns: 'minmax(0, 1fr)', gap: 0.45 }}>
      <Typography component="label" sx={{ fontSize: 12.5, lineHeight: 1.2, fontWeight: 700 }}>{control.label}{control.required ? ' *' : ''}</Typography>
      <TextField
        fullWidth select={select} type={inputType} multiline={multiline} minRows={multiline ? 4 : undefined}
        size="small" placeholder={!select ? control.label : undefined} required={control.required} disabled={disabled}
        value={value} onChange={(event) => onChange(event.target.value)} error={error} helperText={helperText}
        slotProps={{ htmlInput: { 'aria-label': control.label }, ...(type === 'date' || type === 'calendar' || type === 'time' ? { inputLabel: { shrink: true } } : {}) }}
        sx={{ '& .MuiOutlinedInput-root': { borderRadius: '3px', bgcolor: disabled ? 'action.disabledBackground' : '#fff' } }}
      >
        {select ? options.map((option) => <MenuItem key={option.value} value={option.value}><OptionLabel option={option} /></MenuItem>) : null}
      </TextField>
    </Box>
  );
}
