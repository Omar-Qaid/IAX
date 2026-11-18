import React, { useState } from 'react';
import { FastTab } from '@shared/components/fast-tabs/FastTab';
import { Box, Button, MenuItem, TextField, Typography } from '@mui/material';
import { AppActionDrawer } from '@shared/components/dialogs/AppActionDrawer';

export interface DocumentTotalsField {
  id: string;
  label: string;
  value: React.ReactNode;
  emphasized?: boolean;
}

export interface DocumentTotalsColumn {
  id: string;
  title?: string;
  fields: readonly DocumentTotalsField[];
}

export interface DocumentTotalsSection {
  id: string;
  title: string;
  columns: readonly DocumentTotalsColumn[];
  defaultExpanded?: boolean;
}

export interface DocumentTotalsSelectionOption {
  value: string;
  label: string;
}

export interface DocumentTotalsDrawerProps {
  open: boolean;
  onClose: () => void;
  title: string;
  viewLabel?: string;
  calculationBasisLabel?: string;
  selectionLabel?: string;
  selectionOptions?: readonly DocumentTotalsSelectionOption[];
  sections: readonly DocumentTotalsSection[];
  okLabel?: string;
  width?: number;
}

export function DocumentTotalsDrawer({
  open,
  onClose,
  title,
  viewLabel = 'Standard view',
  calculationBasisLabel = 'Calculation basis',
  selectionLabel = 'Selection',
  selectionOptions = [{ value: 'all', label: 'All' }],
  sections,
  okLabel = 'OK',
  width = 520,
}: DocumentTotalsDrawerProps): React.ReactElement {
  const [selection, setSelection] = useState(selectionOptions[0]?.value ?? 'all');

  return (
    <AppActionDrawer
      open={open}
      onClose={onClose}
      title={title}
      width={width}
      actions={
        <Button variant="contained" size="small" onClick={onClose}>
          {okLabel}
        </Button>
      }
    >
      <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1.5 }}>
        {viewLabel}
      </Typography>
      <FastTab
        title={calculationBasisLabel}
        appearance="enterprise"
        detailsSx={{ px: 1, pt: 1, pb: 2 }}
      >
        <TextField
          select
          size="small"
          label={selectionLabel}
          value={selection}
          onChange={(event) => setSelection(event.target.value)}
          sx={{ width: 190 }}
        >
          {selectionOptions.map((option) => (
            <MenuItem key={option.value} value={option.value}>
              {option.label}
            </MenuItem>
          ))}
        </TextField>
      </FastTab>
      {sections.map((section) => (
        <FastTab
          key={section.id}
          title={section.title}
          appearance="enterprise"
          defaultExpanded={section.defaultExpanded ?? true}
          detailsSx={{ px: 1, pt: 1, pb: 2 }}
        >
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: {
                xs: '1fr',
                sm: `repeat(${Math.max(section.columns.length, 1)}, minmax(0, 1fr))`,
              },
              gap: 3,
            }}
          >
            {section.columns.map((column) => (
              <Box key={column.id} sx={{ display: 'grid', alignContent: 'start', gap: 1.15 }}>
                {column.title && (
                  <Typography variant="overline" sx={{ fontSize: 10, fontWeight: 700 }}>
                    {column.title}
                  </Typography>
                )}
                {column.fields.map((field) => (
                  <Box key={field.id}>
                    <Typography variant="caption" color="text.secondary">
                      {field.label}
                    </Typography>
                    <Box
                      sx={{
                        minHeight: field.emphasized ? 38 : 30,
                        px: 1,
                        py: 0.5,
                        border: 1,
                        borderColor: 'text.secondary',
                        borderRadius: 0.5,
                        bgcolor: 'action.hover',
                        textAlign: 'end',
                        fontSize: field.emphasized ? 22 : 14,
                        lineHeight: field.emphasized ? 1.25 : 1.35,
                        fontVariantNumeric: 'tabular-nums',
                      }}
                    >
                      {field.value}
                    </Box>
                  </Box>
                ))}
              </Box>
            ))}
          </Box>
        </FastTab>
      ))}
    </AppActionDrawer>
  );
}
