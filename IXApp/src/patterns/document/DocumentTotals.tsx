import type { ReactNode } from 'react';
import { Box, Divider, Stack, Typography } from '@mui/material';

export interface DocumentTotal { id: string; label: string; value: ReactNode; emphasized?: boolean }
export interface DocumentTotalsProps { title?: string; totals: DocumentTotal[] }
export function DocumentTotals({ title, totals }: DocumentTotalsProps) {
  return <Stack component="section" aria-label={title} spacing={1}>
    {title && <Typography variant="subtitle1" component="h2">{title}</Typography>}
    {totals.map((total, index) => <Box key={total.id}>
      {index > 0 && total.emphasized && <Divider sx={{ mb: 1 }} />}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 2 }}>
        <Typography variant="body2" sx={{ fontWeight: total.emphasized ? 700 : 400 }}>{total.label}</Typography>
        <Typography variant="body2" sx={{ fontWeight: total.emphasized ? 700 : 500 }}>{total.value}</Typography>
      </Box>
    </Box>)}
  </Stack>;
}
