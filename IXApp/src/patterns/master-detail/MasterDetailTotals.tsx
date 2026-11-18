import React from 'react';
import { Box, Typography } from '@mui/material';
import { d365 } from '@shared/constants/enterpriseUiTokens';

export interface MasterDetailTotal {
  id: string;
  label: string;
  value: React.ReactNode;
  /** Highlights the document total. */
  emphasis?: boolean;
}

/** Document totals row (D365 "Totals" FastTab): label above a right-aligned amount. */
export function MasterDetailTotals({
  items,
}: {
  items: readonly MasterDetailTotal[];
}): React.ReactElement {
  return (
    <Box component="dl" sx={{ m: 0, display: 'flex', flexWrap: 'wrap', gap: 4 }}>
      {items.map((item) => (
        <Box key={item.id} sx={{ minWidth: 120 }}>
          <Typography
            component="dt"
            sx={{ fontSize: d365.labelFontSize + 1, color: d365.mutedText, mb: 0.5 }}
          >
            {item.label}
          </Typography>
          <Typography
            component="dd"
            sx={{
              m: 0,
              fontSize: d365.fontSize,
              textAlign: 'end',
              fontWeight: item.emphasis ? 600 : 400,
              color: d365.text,
            }}
          >
            {item.value}
          </Typography>
        </Box>
      ))}
    </Box>
  );
}
