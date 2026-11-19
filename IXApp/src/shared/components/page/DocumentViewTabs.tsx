import React from 'react';
import { Tab, Tabs } from '@mui/material';

export interface DocumentViewTab {
  value: string;
  label: string;
}

/** Lines / Header switch under the title (D365 details page view tabs). */
export function DocumentViewTabs({
  tabs,
  value,
  onChange,
  ariaLabel,
  panelId = 'master-detail-panel',
}: {
  tabs: DocumentViewTab[];
  value: string;
  onChange: (value: string) => void;
  ariaLabel: string;
  /** ID of the element the tabs control. */
  panelId?: string;
}): React.ReactElement {
  return (
    <Tabs
      value={value}
      onChange={(_, next: string) => onChange(next)}
      aria-label={ariaLabel}
      sx={{
        mb: 3,
        minHeight: 36,
        '& .MuiTab-root': { minHeight: 36, minWidth: 48, px: 1, fontSize: 13 },
      }}
    >
      {tabs.map((tab) => (
        <Tab
          key={tab.value}
          value={tab.value}
          label={tab.label}
          id={`${panelId}-${tab.value}-tab`}
          aria-controls={panelId}
        />
      ))}
    </Tabs>
  );
}
