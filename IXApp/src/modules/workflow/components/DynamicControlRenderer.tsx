import React from 'react';
import {
  Box,
  Button,
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
  IconButton,
} from '@mui/material';
import AddOutlined from '@mui/icons-material/AddOutlined';
import DeleteOutline from '@mui/icons-material/DeleteOutlined';
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
    return Array.isArray(parsed)
      ? parsed.filter((item): item is string => typeof item === 'string')
      : [];
  } catch {
    return [];
  }
};

const tableColumnKey = (value: string, index: number): string =>
  value
    .trim()
    .replace(/[^a-z0-9]+/gi, '_')
    .replace(/^_+|_+$/g, '')
    .toLocaleLowerCase() || `column_${index + 1}`;

export const readTableRows = (value: string): Array<Record<string, string>> => {
  if (!value.trim()) return [];
  try {
    const parsed = JSON.parse(value) as unknown;
    if (!Array.isArray(parsed)) return [];
    return parsed.filter(
      (row): row is Record<string, string> =>
        Boolean(row) && typeof row === 'object' && !Array.isArray(row)
    );
  } catch {
    return [];
  }
};

function OptionLabel({ option }: { option: RenderableOption }) {
  const { t } = useAppTranslation();
  return (
    <Box
      component="span"
      sx={{ display: 'inline-flex', alignItems: 'center', gap: 0.45, minWidth: 0 }}
    >
      <Box component="span" sx={{ overflow: 'hidden', textOverflow: 'ellipsis' }}>
        {option.label}
      </Box>
      {option.sendsNotification && (
        <Tooltip title={t('workflowRequest.sendsNotification')} arrow>
          <NotificationsNoneOutlined color="action" sx={{ fontSize: 15 }} />
        </Tooltip>
      )}
      {option.requiresAttachment && (
        <Tooltip title={t('workflowRequest.attachmentRequired')} arrow>
          <AttachFileOutlined color="action" sx={{ fontSize: 15 }} />
        </Tooltip>
      )}
      {option.revealsControls && (
        <Tooltip title={t('workflowRequest.showsAdditionalFields')} arrow>
          <SubdirectoryArrowRightOutlined color="action" sx={{ fontSize: 15 }} />
        </Tooltip>
      )}
    </Box>
  );
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
    return (
      <Box
        role="note"
        sx={{
          px: 1.25,
          py: 1,
          borderInlineStart: '4px solid',
          borderColor: noteColor,
          bgcolor: 'rgba(245, 158, 11, .08)',
          borderRadius: 0.75,
        }}
      >
        <Typography sx={{ color: noteColor, fontSize: 13, lineHeight: 1.45, fontWeight: 800 }}>
          {control.label}
        </Typography>
      </Box>
    );
  }
  if (type === 'checkbox')
    return (
      <FormControl error={error} required={control.required}>
        <FormControlLabel
          control={
            <Checkbox
              checked={value === 'true'}
              disabled={disabled}
              onChange={(_, checked) => onChange(String(checked))}
            />
          }
          label={
            <Typography component="span" sx={{ fontSize: 'inherit', fontWeight: 700 }}>
              {control.label}
            </Typography>
          }
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
          {options.map((option) => (
            <FormControlLabel
              key={option.value}
              label={<OptionLabel option={option} />}
              control={
                <Checkbox
                  checked={selected.includes(option.value)}
                  disabled={disabled}
                  onChange={(_, checked) => {
                    const next = checked
                      ? [...selected, option.value]
                      : selected.filter((item) => item !== option.value);
                    onChange(JSON.stringify(next));
                  }}
                />
              }
            />
          ))}
        </FormGroup>
        {helperText && <FormHelperText>{helperText}</FormHelperText>}
      </FormControl>
    );
  }
  if (type === 'radio' || type.includes('radiobutton'))
    return (
      <FormControl error={error} required={control.required}>
        <FormLabel sx={{ fontWeight: 700 }}>{control.label}</FormLabel>
        <RadioGroup row value={value} onChange={(event) => onChange(event.target.value)}>
          {options.map((option) => (
            <FormControlLabel
              key={option.value}
              value={option.value}
              label={<OptionLabel option={option} />}
              control={<Radio disabled={disabled} />}
            />
          ))}
        </RadioGroup>
        {helperText && <FormHelperText>{helperText}</FormHelperText>}
      </FormControl>
    );
  if (type === 'file')
    return (
      <FileDropControl
        control={control}
        value={value}
        onChange={onChange}
        onFilesChange={onFilesChange}
        error={error}
        helperText={helperText}
        preview={preview}
      />
    );
  if (type === 'signature')
    return (
      <SignatureControl
        control={control}
        value={value}
        onChange={onChange}
        error={error}
        helperText={helperText}
        preview={preview}
      />
    );
  if (type === 'location')
    return (
      <LocationControl
        control={control}
        value={value}
        onChange={onChange}
        error={error}
        helperText={helperText}
        preview={preview}
      />
    );
  if (type === 'table') {
    const configuredColumns = options.length ? options : [{ value: 'value', label: 'Value' }];
    const columns = configuredColumns.map((option, index) => ({
      key: tableColumnKey(option.value || option.label, index),
      label: option.label || option.value,
    }));
    const rows = readTableRows(value);
    const updateRows = (nextRows: Array<Record<string, string>>) =>
      onChange(JSON.stringify(nextRows));
    return (
      <Box>
        <Typography variant="body2" sx={{ mb: 0.5, fontWeight: 700 }}>
          {control.label}
        </Typography>
        <Box
          sx={{
            overflow: 'auto',
            border: '1px solid',
            borderColor: error ? 'error.main' : 'divider',
          }}
        >
          <Box sx={{ minWidth: Math.max(360, columns.length * 150) }}>
            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: `repeat(${columns.length}, minmax(130px, 1fr)) 34px`,
                bgcolor: '#f3f6f9',
                borderBottom: '1px solid',
                borderColor: 'divider',
              }}
            >
              {columns.map((column) => (
                <Typography
                  key={column.key}
                  sx={{ px: 1, py: 0.65, fontSize: 11, fontWeight: 700 }}
                >
                  {column.label}
                </Typography>
              ))}
              <Box />
            </Box>
            {rows.map((row, rowIndex) => (
              <Box
                key={rowIndex}
                sx={{
                  display: 'grid',
                  gridTemplateColumns: `repeat(${columns.length}, minmax(130px, 1fr)) 34px`,
                  borderBottom: '1px solid',
                  borderColor: 'divider',
                }}
              >
                {columns.map((column) => (
                  <TextField
                    key={column.key}
                    value={row[column.key] ?? ''}
                    size="small"
                    variant="standard"
                    disabled={disabled}
                    onChange={(event) =>
                      updateRows(
                        rows.map((item, itemIndex) =>
                          itemIndex === rowIndex
                            ? { ...item, [column.key]: event.target.value }
                            : item
                        )
                      )
                    }
                    slotProps={{
                      input: { disableUnderline: true },
                      htmlInput: { 'aria-label': `${column.label} ${rowIndex + 1}` },
                    }}
                    sx={{
                      px: 1,
                      boxShadow: 'inset 0 0 0 1px #c7c7c7',
                      '& .MuiInputBase-root': { minHeight: 31, fontSize: 12 },
                    }}
                  />
                ))}
                <IconButton
                  size="small"
                  disabled={disabled}
                  aria-label={`${t('actions.delete')} ${rowIndex + 1}`}
                  onClick={() => updateRows(rows.filter((_, index) => index !== rowIndex))}
                  sx={{ borderRadius: 0, boxShadow: 'inset 0 0 0 1px #c7c7c7' }}
                >
                  <DeleteOutline sx={{ fontSize: 16 }} />
                </IconButton>
              </Box>
            ))}
            {rows.length === 0 ? (
              <Typography color="text.secondary" sx={{ px: 1, py: 1.25, fontSize: 11 }}>
                {t('workflowRequest.tableData')}
              </Typography>
            ) : null}
          </Box>
        </Box>
        <Button
          size="small"
          startIcon={<AddOutlined />}
          disabled={disabled}
          onClick={() =>
            updateRows([...rows, Object.fromEntries(columns.map((column) => [column.key, '']))])
          }
          sx={{ mt: 0.5 }}
        >
          {t('actions.add')}
        </Button>
        {helperText && <FormHelperText error={error}>{helperText}</FormHelperText>}
      </Box>
    );
  }
  const select =
    type === 'select' ||
    type === 'combobox' ||
    type.includes('dropdown') ||
    (type === 'showroom' && options.length > 0);
  const inputType =
    type === 'digits' || type === 'number' || type === 'employeeid'
      ? 'number'
      : type === 'date' || type === 'calendar'
        ? 'date'
        : type === 'time'
          ? 'time'
          : type === 'url'
            ? 'url'
            : type === 'email'
              ? 'email'
              : 'text';
  const multiline = type === 'longtext' || type === 'textarea';
  return (
    <Box sx={{ display: 'grid', gridTemplateColumns: 'minmax(0, 1fr)', gap: 0.45 }}>
      <Typography component="label" sx={{ fontSize: 12.5, lineHeight: 1.2, fontWeight: 700 }}>
        {control.label}
        {control.required ? ' *' : ''}
      </Typography>
      <TextField
        fullWidth
        select={select}
        type={inputType}
        multiline={multiline}
        minRows={multiline ? 4 : undefined}
        size="small"
        placeholder={!select ? control.label : undefined}
        required={control.required}
        disabled={disabled}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        error={error}
        helperText={helperText}
        slotProps={{
          htmlInput: { 'aria-label': control.label },
          ...(type === 'date' || type === 'calendar' || type === 'time'
            ? { inputLabel: { shrink: true } }
            : {}),
        }}
        sx={{
          '& .MuiOutlinedInput-root': {
            borderRadius: '3px',
            bgcolor: disabled ? 'action.disabledBackground' : '#fff',
          },
        }}
      >
        {select
          ? options.map((option) => (
              <MenuItem key={option.value} value={option.value}>
                <OptionLabel option={option} />
              </MenuItem>
            ))
          : null}
      </TextField>
    </Box>
  );
}
