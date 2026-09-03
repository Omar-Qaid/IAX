import React from 'react';
import { Box, CircularProgress, Typography, Backdrop, Paper } from '@mui/material';
import type { ReportDesignerProps } from './types';

/**
 * Domain-neutral workspace shell for visual report and process designers.
 *
 * Designed to be reusable across all modules (Workflow print templates, Finance report builders,
 * Organization chart designers, etc.). Supports both slot-based composition (toolbar, sidebar,
 * workspace, properties, footer) and unconstrained child layout composition.
 */
export function ReportDesigner({
  children,
  ariaLabel = 'Report designer',
  className,
  minHeight = 560,
  height = 'min(78vh, 820px)',
  toolbarHeight = 54,
  sidebarWidth = 200,
  propertiesWidth = 260,
  toolbar,
  sidebar,
  properties,
  footer,
  isLoading = false,
  loadingMessage,
  sx,
}: ReportDesignerProps): React.ReactElement {
  const formattedToolbarHeight =
    typeof toolbarHeight === 'number' ? `${toolbarHeight}px` : toolbarHeight;

  const formattedSidebarWidth =
    typeof sidebarWidth === 'number' ? `${sidebarWidth}px` : sidebarWidth;

  const formattedPropertiesWidth =
    typeof propertiesWidth === 'number' ? `${propertiesWidth}px` : propertiesWidth;

  const hasSlots = Boolean(toolbar || sidebar || properties || footer);

  const gridRows = hasSlots
    ? `${toolbar ? formattedToolbarHeight : ''} minmax(0, 1fr) ${footer ? '32px' : ''}`.trim()
    : `${formattedToolbarHeight} minmax(0, 1fr)`;

  return (
    <Box
      className={className}
      role="region"
      aria-label={ariaLabel}
      sx={[
        (theme) => ({
          position: 'relative',
          height,
          minHeight,
          display: 'grid',
          gridTemplateRows: gridRows,
          border: `1px solid ${theme.palette.divider}`,
          bgcolor: theme.palette.mode === 'dark' ? theme.palette.background.paper : '#eef1f4',
          overflow: 'hidden',
          borderRadius: 1,
        }),
        ...(Array.isArray(sx) ? sx : sx ? [sx] : []),
      ]}
    >
      {/* Optional Top Toolbar Slot */}
      {toolbar && (
        <Paper
          square
          elevation={0}
          sx={{
            minHeight: formattedToolbarHeight,
            borderBottom: 1,
            borderColor: 'divider',
            display: 'flex',
            alignItems: 'center',
            zIndex: 1,
          }}
        >
          {toolbar}
        </Paper>
      )}

      {/* Main Workspace Body */}
      {hasSlots ? (
        <Box
          sx={{
            minHeight: 0,
            display: 'grid',
            gridTemplateColumns:
              `${sidebar ? formattedSidebarWidth : ''} minmax(300px, 1fr) ${properties ? formattedPropertiesWidth : ''}`.trim(),
          }}
        >
          {sidebar && (
            <Paper
              square
              variant="outlined"
              sx={{ minWidth: 0, overflow: 'auto', borderWidth: 0, borderInlineEndWidth: 1 }}
            >
              {sidebar}
            </Paper>
          )}

          <Box sx={{ minWidth: 0, overflow: 'auto', p: 2 }}>{children}</Box>

          {properties && (
            <Paper
              square
              variant="outlined"
              sx={{ minWidth: 0, overflow: 'auto', borderWidth: 0, borderInlineStartWidth: 1 }}
            >
              {properties}
            </Paper>
          )}
        </Box>
      ) : (
        children
      )}

      {/* Optional Footer Slot */}
      {footer && (
        <Paper
          square
          elevation={0}
          sx={{
            height: 32,
            borderTop: 1,
            borderColor: 'divider',
            display: 'flex',
            alignItems: 'center',
            px: 1.5,
            bgcolor: 'background.paper',
          }}
        >
          {footer}
        </Paper>
      )}

      {/* Loading Overlay */}
      {isLoading && (
        <Backdrop
          open
          sx={{
            position: 'absolute',
            zIndex: (theme) => theme.zIndex.drawer + 1,
            bgcolor: 'rgba(255, 255, 255, 0.75)',
            display: 'flex',
            flexDirection: 'column',
            gap: 1.5,
          }}
        >
          <CircularProgress size={36} color="primary" />
          {loadingMessage && (
            <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 500 }}>
              {loadingMessage}
            </Typography>
          )}
        </Backdrop>
      )}
    </Box>
  );
}
