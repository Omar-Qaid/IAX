import React from 'react';
import { Box, Typography } from '@mui/material';
import { FastTab } from '@shared/components/fast-tabs/FastTab';
import { d365 } from '@shared/constants/enterpriseUiTokens';

interface MasterDetailSectionProps {
  title: string;
  children: React.ReactNode;
  defaultExpanded?: boolean;
  /** Controlled expansion (with `onExpandedChange`). */
  expanded?: boolean;
  onExpandedChange?: (expanded: boolean) => void;
  /**
   * D365 FastTab summary: key values shown in the header line, e.g. the customer
   * and currency, so a collapsed section still tells what it holds.
   */
  summary?: React.ReactNode[];
}

/** D365 FastTab: collapsible bordered section with title, summary and content. */
export function MasterDetailSection({
  title,
  children,
  defaultExpanded = true,
  expanded,
  onExpandedChange,
  summary,
}: MasterDetailSectionProps): React.ReactElement {
  return (
    <FastTab
      title={title}
      appearance="enterprise"
      defaultExpanded={defaultExpanded}
      expanded={expanded}
      onExpandedChange={onExpandedChange}
      summary={
        summary && summary.length > 0 ? (
          <Box sx={{ display: 'flex', gap: 2, minWidth: 0, overflow: 'hidden' }}>
            {summary.map((item, index) => (
              <Typography
                key={index}
                component="span"
                noWrap
                sx={{ fontSize: d365.fontSize, color: 'text.secondary' }}
              >
                {item}
              </Typography>
            ))}
          </Box>
        ) : undefined
      }
    >
      {children}
    </FastTab>
  );
}
