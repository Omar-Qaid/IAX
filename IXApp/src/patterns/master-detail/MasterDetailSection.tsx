import React from 'react';
import { Accordion, AccordionDetails, AccordionSummary, Typography } from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';

export function MasterDetailSection({ title, children, defaultExpanded = true }: { title: string; children: React.ReactNode; defaultExpanded?: boolean }): React.ReactElement {
  return <Accordion defaultExpanded={defaultExpanded} disableGutters elevation={0} sx={{ mb: 0.75, border: 1, borderColor: 'divider', borderRadius: '2px !important', '&:before': { display: 'none' } }}>
    <AccordionSummary expandIcon={<ExpandMoreIcon />} sx={{ minHeight: 40 }}><Typography variant="body2" sx={{ fontWeight: 600 }}>{title}</Typography></AccordionSummary>
    <AccordionDetails sx={{ borderTop: 1, borderColor: 'divider', p: 1.25 }}>{children}</AccordionDetails>
  </Accordion>;
}
