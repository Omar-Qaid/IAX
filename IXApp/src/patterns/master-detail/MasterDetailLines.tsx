import React from 'react';
import { Box, Stack, Typography } from '@mui/material';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { d365 } from '@shared/constants/enterpriseUiTokens';

/**
 * The lines area of a master-detail page: line action toolbar, keyboard help /
 * saving status, the lines grid and (optionally) the selected line's details.
 */
export function MasterDetailLines({
  toolbar,
  toolbarLabel,
  saving = false,
  help,
  grid,
  details,
}: {
  /** Line actions (Add line, Remove, menus). Rendered as an ARIA toolbar. */
  toolbar?: React.ReactNode;
  toolbarLabel: string;
  /** Shows "Saving..." in place of the keyboard help. */
  saving?: boolean;
  /** Keyboard help; defaults to the shared D365 line-editing keys. */
  help?: React.ReactNode;
  /** Usually a `DataGrid` with inline line editing. */
  grid: React.ReactNode;
  /** Line details (tabs/fast tabs of the selected line). */
  details?: React.ReactNode;
}): React.ReactElement {
  const { t } = useAppTranslation();
  return (
    <Box sx={{ minWidth: 0 }}>
      {toolbar && (
        <Stack
          role="toolbar"
          aria-label={toolbarLabel}
          direction="row"
          spacing={0.5}
          sx={{
            mb: 0.5,
            minHeight: d365.controlHeight,
            overflowX: 'auto',
            whiteSpace: 'nowrap',
            '& .MuiButton-root': { flexShrink: 0, fontSize: d365.fontSize, minWidth: 0, px: 0.75, py: 0.25, minHeight: d365.controlHeight },
          }}
        >
          {toolbar}
        </Stack>
      )}
      <Typography variant="caption" color="text.secondary" role="status" sx={{ display: 'block', minHeight: 22 }}>
        {saving
          ? t('common.saving', 'Saving...')
          : (help ??
            t(
              'masterDetail.linesHelp',
              'Tab / Enter: next cell | Shift: previous | Up / Down: next row | F2: edit | Insert: add | Alt+Delete: remove | Ctrl+F: filter'
            ))}
      </Typography>
      {grid}
      {details && <Box sx={{ mt: 1 }}>{details}</Box>}
    </Box>
  );
}
