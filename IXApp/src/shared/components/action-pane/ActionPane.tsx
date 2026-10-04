import React, { createContext, useContext, useMemo, useState } from 'react';
import { Paper, Box, ClickAwayListener } from '@mui/material';
import { d365 } from '@shared/constants/enterpriseUiTokens';

export interface ActionPaneProps {
  children: React.ReactNode;
  variant?: 'default' | 'flat';
  endActions?: React.ReactNode;
  expandedContent?: React.ReactNode;
  onClickAway?: () => void;
}

export interface ActionPaneRibbonDescriptor {
  id: string;
  render: (pinned: boolean, onPinnedChange: (pinned: boolean) => void) => React.ReactNode;
  persistenceKey?: string;
}

interface ActionPaneRibbonContextValue {
  activeId: string | null;
  pinned: boolean;
  toggleRibbon: (descriptor: ActionPaneRibbonDescriptor) => void;
  restoreRibbon: (descriptor: ActionPaneRibbonDescriptor) => void;
}

const ActionPaneRibbonContext = createContext<ActionPaneRibbonContextValue | null>(null);

export const useActionPaneRibbon = (): ActionPaneRibbonContextValue | null =>
  useContext(ActionPaneRibbonContext);

export const ActionPane: React.FC<ActionPaneProps> = ({
  children,
  variant = 'flat',
  endActions,
  expandedContent,
  onClickAway,
}) => {
  const [ribbon, setRibbon] = useState<ActionPaneRibbonDescriptor | null>(null);
  const [ribbonPinned, setRibbonPinned] = useState(false);

  const changeRibbonPinned = (pinned: boolean) => {
    setRibbonPinned(pinned);
    if (ribbon?.persistenceKey) {
      try {
        window.localStorage.setItem(ribbon.persistenceKey, String(pinned));
      } catch {
        // Storage can be unavailable in restricted browser contexts.
      }
    }
    if (!pinned) setRibbon(null);
  };

  const contextValue = useMemo<ActionPaneRibbonContextValue>(
    () => ({
      activeId: ribbon?.id ?? null,
      pinned: ribbonPinned,
      toggleRibbon: (descriptor) => {
        if (ribbon?.id === descriptor.id && !ribbonPinned) {
          setRibbon(null);
          return;
        }
        if (ribbon && ribbon.id !== descriptor.id) {
          if (ribbon.persistenceKey) {
            try {
              window.localStorage.setItem(ribbon.persistenceKey, 'false');
            } catch {
              // Storage can be unavailable in restricted browser contexts.
            }
          }
          setRibbonPinned(false);
        }
        setRibbon(descriptor);
      },
      restoreRibbon: (descriptor) => {
        setRibbon(descriptor);
        setRibbonPinned(true);
      },
    }),
    [ribbon, ribbonPinned]
  );

  const internalExpandedContent = ribbon?.render(ribbonPinned, changeRibbonPinned);
  const visibleExpandedContent = internalExpandedContent ?? expandedContent;
  const handleClickAway = () => {
    if (ribbon && !ribbonPinned) setRibbon(null);
    else onClickAway?.();
  };

  const pane = (
    <Box sx={{ maxWidth: '100%' }}>
      <Paper
        elevation={0}
        sx={{
          p: '3px 6px',
          minHeight: d365.toolbarHeight,
          boxSizing: 'border-box',
          mx: 0,
          mt: 0,
          mb: visibleExpandedContent ? '3px' : '6px',
          borderRadius: variant === 'flat' ? '9px' : 1,
          border: (t) => `1px solid ${t.palette.divider}`,
          bgcolor:
            variant === 'flat'
              ? 'background.paper'
              : (t) => (t.palette.mode === 'light' ? '#f8f9fa' : '#222222'),
          boxShadow: variant === 'flat' ? '0 2px 7px rgba(0,0,0,0.16)' : 'none',
          overflow: 'hidden',
          WebkitOverflowScrolling: 'touch',
          whiteSpace: 'nowrap',
        }}
      >
        <Box sx={{ display: 'flex', alignItems: 'center', overflowX: 'auto', overflowY: 'hidden' }}>
          <Box
            sx={{ display: 'flex', alignItems: 'center', minWidth: 'max-content', flexShrink: 0 }}
          >
            <ActionPaneRibbonContext.Provider value={contextValue}>
              {children}
            </ActionPaneRibbonContext.Provider>
          </Box>
          {endActions && (
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                gap: 0.25,
                marginInlineStart: 'auto',
                paddingInlineStart: 1.5,
                position: 'sticky',
                insetInlineEnd: 0,
                flexShrink: 0,
                bgcolor: 'inherit',
              }}
            >
              {endActions}
            </Box>
          )}
        </Box>
      </Paper>
      {visibleExpandedContent && (
        <Paper
          elevation={0}
          sx={{
            mb: '6px',
            border: (theme) => `1px solid ${theme.palette.divider}`,
            borderRadius: variant === 'flat' ? '9px' : 1,
            boxShadow: variant === 'flat' ? '0 2px 7px rgba(0,0,0,0.16)' : 'none',
            overflow: 'hidden',
          }}
        >
          {visibleExpandedContent}
        </Paper>
      )}
    </Box>
  );

  return ribbon || onClickAway ? (
    <ClickAwayListener onClickAway={handleClickAway}>{pane}</ClickAwayListener>
  ) : (
    pane
  );
};
