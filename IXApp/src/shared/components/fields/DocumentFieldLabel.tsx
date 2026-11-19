import React from 'react';
import { Typography } from '@mui/material';
import { d365 } from '@shared/constants/enterpriseUiTokens';

/** Same plain-caption and lookup-label sizing used by sales-order header fields. */
export function DocumentFieldLabel({
  children,
  lookup = false,
  title,
}: {
  children: React.ReactNode;
  lookup?: boolean;
  title?: string;
}): React.ReactElement {
  return lookup ? (
    <Typography
      title={title}
      sx={{
        display: 'block',
        mb: '6px',
        fontSize: d365.labelFontSize,
        lineHeight: 1.2,
        color: 'text.primary',
      }}
    >
      {children}
    </Typography>
  ) : (
    <Typography title={title} variant="caption" color="text.secondary">
      {children}
    </Typography>
  );
}
