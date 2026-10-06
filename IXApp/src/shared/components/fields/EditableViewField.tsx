import React from 'react';
import { Box, IconButton, Typography } from '@mui/material';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import { d365 } from '@shared/constants/enterpriseUiTokens';
import { hasLookupValue } from '@shared/components/lookups/lookupValueStyle';

interface EditableViewFieldProps {
  value: string | number | boolean | null | undefined;
  label: string;
  numeric?: boolean;
  lookup?: boolean;
  disabled?: boolean;
  onEdit?: () => void;
}

export function EditableViewField({ value, label, numeric = false, lookup = false, disabled = false, onEdit }: EditableViewFieldProps): React.ReactElement {
  return (
    <Box sx={{
      width: '100%', minHeight: d365.controlHeight, display: 'flex', alignItems: 'center', position: 'relative',
      borderBottom: `1px solid ${d365.darkBorder}`,
      borderRadius: 0,
      bgcolor: 'transparent',
      px: 0.5, overflow: 'hidden',
      '&:hover .field-edit-action, &:focus-within .field-edit-action': { opacity: 1 },
    }}>
      <Typography noWrap sx={{ flex: 1, minWidth: 0, textAlign: numeric ? 'end' : 'start', paddingInlineEnd: onEdit && !disabled ? '18px' : 0,
        fontFamily: d365.fontFamily, fontSize: d365.fontSize,
        color: lookup && hasLookupValue(value) ? d365.primary : 'inherit',
        textDecoration: lookup && hasLookupValue(value) ? 'underline' : undefined,
      }}>
        {String(value ?? '')}
      </Typography>
      {onEdit && !disabled && (
        <IconButton
          className="field-edit-action"
          size="small"
          aria-label={`Edit ${label}`}
          onClick={onEdit}
          sx={{ position: 'absolute', insetInlineEnd: 2, top: '50%', transform: 'translateY(-50%)', opacity: 0, transition: 'opacity 120ms ease', color: d365.primary, p: 0.25 }}
        >
          <EditOutlinedIcon sx={{ fontSize: 13 }} />
        </IconButton>
      )}
    </Box>
  );
}
