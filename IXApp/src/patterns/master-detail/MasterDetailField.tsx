import React from 'react';
import { Box, Typography } from '@mui/material';
import { d365 } from '@shared/constants/enterpriseUiTokens';
import { EditableViewField } from '@shared/components/fields/EditableViewField';
import { LookupDisplayField } from '@shared/components/lookups/LookupDisplayField';
import type { MasterDetailFieldProps } from './types';

/**
 * One header field, D365 style. View mode: label + read-only value (pencil to
 * edit). Master data (`masterRoute` / `previewEntity`): open-details icon on the
 * label, blue value and hover record card. Edit mode: the page's editor.
 */
export function MasterDetailField({
  label,
  value,
  editor,
  editing = false,
  numeric = false,
  onEdit,
  masterRoute,
  previewEntity,
  previewKey,
  previewOption,
  lookup = false,
}: MasterDetailFieldProps): React.ReactElement {
  if (editing && editor) return <Box sx={{ minWidth: 0 }}>{editor}</Box>;
  const text =
    value == null
      ? ''
      : typeof value === 'string' || typeof value === 'number'
        ? String(value)
        : value;
  if ((masterRoute || previewEntity) && typeof text === 'string')
    return (
      <LookupDisplayField
        label={label}
        value={text}
        masterRoute={masterRoute}
        previewEntity={previewEntity}
        previewKey={previewKey}
        previewOption={previewOption}
        onEdit={onEdit}
      />
    );
  return (
    <Box sx={{ minWidth: 0 }}>
      <Typography
        sx={{
          display: 'block',
          mb: '6px',
          fontSize: d365.labelFontSize,
          lineHeight: 1.2,
          color: 'text.primary',
        }}
      >
        {label}
      </Typography>
      {typeof text === 'string' ? (
        <EditableViewField
          label={label}
          value={text}
          numeric={numeric}
          lookup={lookup}
          onEdit={onEdit}
        />
      ) : (
        text
      )}
    </Box>
  );
}

/** A titled group of header fields (D365 header panel column: DELIVERY, REFERENCES…). */
export function MasterDetailFieldGroup({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}): React.ReactElement {
  return (
    <Box
      role="group"
      aria-label={title}
      sx={{ minWidth: 0, display: 'grid', alignContent: 'start', gap: 1.25 }}
    >
      <Typography
        sx={{
          fontSize: d365.labelFontSize,
          fontWeight: 700,
          lineHeight: 1.5,
          textTransform: 'uppercase',
        }}
      >
        {title}
      </Typography>
      {children}
    </Box>
  );
}

/** Responsive columns of field groups (wraps on narrow screens). */
export function MasterDetailFieldGroups({
  children,
  minColumnWidth = 180,
}: {
  children: React.ReactNode;
  minColumnWidth?: number;
}): React.ReactElement {
  return (
    <Box
      sx={{
        display: 'grid',
        gridTemplateColumns: `repeat(auto-fit, minmax(${minColumnWidth}px, 1fr))`,
        gap: 3,
      }}
    >
      {children}
    </Box>
  );
}
