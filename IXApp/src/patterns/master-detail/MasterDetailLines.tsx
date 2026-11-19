import React from 'react';
import { Box, Button, Stack, Typography } from '@mui/material';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { d365 } from '@shared/constants/enterpriseUiTokens';

/**
 * Grid settings of a lines grid (sales order look): compact rows, bordered
 * cells, no own toolbar/footer (the lines toolbar and Search drive it).
 * Spread into the page's `DataGrid`: `<DataGrid {...MASTER_DETAIL_LINES_GRID_PROPS} … />`.
 */
export const MASTER_DETAIL_LINES_GRID_PROPS = {
  height: 232,
  rowHeight: 24,
  headerHeight: 27,
  hideFooter: true,
  hideColumnMenu: false,
  showColumnBorders: true,
  showCellBorders: true,
  hideAddRowButton: true,
  hideToolbar: true,
  hideSidebarTabs: true,
  hideInlineEditActions: true,
  editRowHighlight: 'none',
} as const;

/** One line action in the toolbar (Add line, Remove...). */
export interface MasterDetailLineAction {
  id: string;
  label: string;
  icon?: React.ReactNode;
  disabled?: boolean;
  onClick: () => void;
}

/**
 * The lines area of a master-detail page: line action toolbar, keyboard help /
 * saving status, the lines grid and (optionally) the selected line's details.
 */
export function MasterDetailLines({
  actions,
  toolbar,
  toolbarLabel,
  saving = false,
  help,
  grid,
  details,
}: {
  /** Standard line actions, rendered as toolbar buttons before `toolbar`. */
  actions?: readonly MasterDetailLineAction[];
  /** Extra toolbar content (menus...). Rendered with `actions` as an ARIA toolbar. */
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
      {(toolbar || Boolean(actions?.length)) && (
        <Stack
          role="toolbar"
          aria-label={toolbarLabel}
          direction="row"
          spacing={0.5}
          sx={{
            mb: 0.5,
            minHeight: d365.fieldControlHeight,
            overflowX: 'auto',
            whiteSpace: 'nowrap',
            '& .MuiButton-root': {
              flexShrink: 0,
              fontSize: d365.fontSize,
              minWidth: 0,
              px: 0.75,
              py: 0.25,
              minHeight: d365.fieldControlHeight,
            },
          }}
        >
          {actions?.map((action) => (
            <Button
              key={action.id}
              size="small"
              startIcon={action.icon}
              disabled={action.disabled}
              onClick={action.onClick}
            >
              {action.label}
            </Button>
          ))}
          {toolbar}
        </Stack>
      )}
      <Typography
        variant="caption"
        color="text.secondary"
        role="status"
        sx={{ display: 'block', minHeight: 22 }}
      >
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
