import React from 'react';
import { Box, Typography } from '@mui/material';
import { d365 } from '@shared/constants/enterpriseUiTokens';
import { EditableViewField } from '@shared/components/fields/EditableViewField';
import { LookupNavigationLabel } from './LookupNavigationLabel';
import { RecordPreview, type RecordPreviewOption } from '@shared/components/record-preview';

interface LookupDisplayFieldProps {
  label: string;
  value: string | number | null | undefined;
  masterRoute?: string;
  /** Registered master-data entity: hovering the value shows its record preview. */
  previewEntity?: string;
  /** Key of the referenced record (defaults to the shown value). */
  previewKey?: string | number | null;
  /** Option already held for the value: the card shows it without loading. */
  previewOption?: RecordPreviewOption | null;
  /** Pencil on the value that switches the form to edit mode. */
  onEdit?: () => void;
}

export function LookupDisplayField({
  label,
  value,
  masterRoute,
  previewEntity,
  previewKey,
  previewOption,
  onEdit,
}: LookupDisplayFieldProps): React.ReactElement {
  return (
    <Box sx={{ minWidth: 0 }}>
      {label && (
        <Typography sx={{ display: 'block', mb: '6px', fontSize: d365.labelFontSize, lineHeight: 1.2, color: 'text.primary' }}>
          <LookupNavigationLabel label={label} masterRoute={masterRoute} />
        </Typography>
      )}
      <RecordPreview entity={previewEntity} masterRoute={masterRoute} recordKey={previewKey ?? value} option={previewOption}>
        <EditableViewField label={label} value={value} lookup onEdit={onEdit} />
      </RecordPreview>
    </Box>
  );
}
