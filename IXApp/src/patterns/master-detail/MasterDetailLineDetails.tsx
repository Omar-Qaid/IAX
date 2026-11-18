import React from 'react';
import { Box, Tab, Tabs, Typography } from '@mui/material';
import { d365 } from '@shared/constants/enterpriseUiTokens';

export interface MasterDetailLineDetailsTab {
  value: string;
  label: string;
}

/**
 * Details of the selected line, under the lines grid (D365 "Line details"):
 * scrollable tabs, one tab panel, and a hint when no line is selected.
 * The page renders the fields of the active tab as children.
 */
export function MasterDetailLineDetails({
  ariaLabel,
  tabs,
  value,
  onChange,
  hasLine,
  emptyLabel,
  children,
}: {
  ariaLabel: string;
  tabs: readonly MasterDetailLineDetailsTab[];
  value: string;
  onChange: (value: string) => void;
  /** False shows `emptyLabel` instead of the tab content. */
  hasLine: boolean;
  emptyLabel: string;
  children?: React.ReactNode;
}): React.ReactElement {
  const id = React.useId();
  const tabId = (tab: string) => `${id}-tab-${tab}`;
  return (
    <Box sx={{ minWidth: 0 }}>
      <Tabs
        value={value}
        onChange={(_, next: string) => onChange(next)}
        variant="scrollable"
        scrollButtons="auto"
        aria-label={ariaLabel}
        sx={{
          mb: 2,
          minHeight: 34,
          '& .MuiTab-root': { fontSize: d365.fontSize, minHeight: 34, minWidth: 0, px: 1.25 },
        }}
      >
        {tabs.map((tab) => (
          <Tab
            key={tab.value}
            value={tab.value}
            label={tab.label}
            id={tabId(tab.value)}
            aria-controls={`${id}-panel`}
          />
        ))}
      </Tabs>
      <Box role="tabpanel" id={`${id}-panel`} aria-labelledby={tabId(value)}>
        {hasLine ? (
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fit, minmax(160px, 1fr))',
              gap: 2,
            }}
          >
            {children}
          </Box>
        ) : (
          <Typography sx={{ fontSize: d365.fontSize, color: 'text.secondary', minHeight: 100 }}>
            {emptyLabel}
          </Typography>
        )}
      </Box>
    </Box>
  );
}
