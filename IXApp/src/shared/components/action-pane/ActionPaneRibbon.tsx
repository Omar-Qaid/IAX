import React from 'react';
import PushPinOutlinedIcon from '@mui/icons-material/PushPinOutlined';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import { Box, Button, IconButton, Typography } from '@mui/material';

export interface ActionPaneRibbonAction {
  id: string;
  label: string;
  disabled?: boolean;
  onClick?: () => void;
}

export interface ActionPaneRibbonGroup {
  id: string;
  label: string;
  actions: readonly ActionPaneRibbonAction[];
}

export interface ActionPaneRibbonProps {
  groups: readonly ActionPaneRibbonGroup[];
  pinned?: boolean;
  onPinnedChange?: (pinned: boolean) => void;
  pinLabel?: string;
  unpinLabel?: string;
}

export function ActionPaneRibbon({
  groups,
  pinned = false,
  onPinnedChange,
  pinLabel = 'Pin action pane',
  unpinLabel = 'Unpin action pane',
}: ActionPaneRibbonProps): React.ReactElement {
  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'stretch',
        minHeight: 82,
        px: 0.75,
        pt: 1.25,
        pb: 0.75,
        overflowX: 'auto',
        position: 'relative',
        bgcolor: 'background.paper',
      }}
    >
      {groups.map((group) => (
        <Box
          key={group.id}
          sx={{
            minWidth: 106,
            px: 1.25,
            borderInlineEnd: (theme) => `1px solid ${theme.palette.divider}`,
          }}
        >
          <Typography
            variant="caption"
            sx={{ display: 'block', mb: 0.35, fontSize: 11, fontWeight: 600, textAlign: 'center' }}
          >
            {group.label}
          </Typography>
          <Box sx={{ display: 'grid', alignContent: 'start' }}>
            {group.actions.map((action) => (
              <Button
                key={action.id}
                disabled={action.disabled}
                onClick={action.onClick}
                size="small"
                variant="text"
                sx={{
                  justifyContent: 'flex-start',
                  minWidth: 0,
                  minHeight: 23,
                  px: 0.5,
                  py: 0,
                  color: 'text.primary',
                  fontSize: 12,
                  fontWeight: 400,
                  lineHeight: 1.2,
                  textTransform: 'none',
                  whiteSpace: 'nowrap',
                  border: '1px solid transparent',
                  '&:focus-visible': { borderColor: 'primary.main' },
                }}
              >
                {action.label}
              </Button>
            ))}
          </Box>
        </Box>
      ))}
      {onPinnedChange && (
        <IconButton
          aria-label={pinned ? unpinLabel : pinLabel}
          aria-pressed={pinned}
          onClick={() => onPinnedChange(!pinned)}
          size="small"
          sx={{ position: 'sticky', alignSelf: 'flex-end', insetInlineEnd: 0, ml: 'auto' }}
        >
          {pinned ? (
            <KeyboardArrowUpIcon sx={{ fontSize: 18 }} />
          ) : (
            <PushPinOutlinedIcon sx={{ fontSize: 15 }} />
          )}
        </IconButton>
      )}
    </Box>
  );
}
