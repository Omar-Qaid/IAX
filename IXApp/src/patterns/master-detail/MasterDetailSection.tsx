import React from 'react';
import { Accordion, AccordionDetails, AccordionSummary, Typography } from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';

export function MasterDetailSection({
  title,
  children,
  defaultExpanded = true,
}: {
  title: string;
  children: React.ReactNode;
  defaultExpanded?: boolean;
}): React.ReactElement {
  return (
    <Accordion
      defaultExpanded={defaultExpanded}
      disableGutters
      elevation={0}
      sx={{
        mb: 0.75,
        border: 1,
        borderColor: 'divider',
        borderRadius: '9px !important',
        boxShadow: '0 1px 4px rgba(0,0,0,0.12)',
        overflow: 'hidden',
        '&:before': { display: 'none' },
      }}
    >
      <AccordionSummary
        expandIcon={<ExpandMoreIcon />}
        sx={{
          minHeight: 44,
          px: 1.25,
          '& .MuiAccordionSummary-expandIconWrapper': {
            border: 1,
            borderColor: 'divider',
            borderRadius: 0.75,
            p: 0.25,
          },
          '& .MuiSvgIcon-root': { fontSize: 20 },
        }}
      >
        <Typography variant="body2" sx={{ fontWeight: 600 }}>
          {title}
        </Typography>
      </AccordionSummary>
      <AccordionDetails sx={{ borderTop: 1, borderColor: 'divider', p: 1.25 }}>
        {children}
      </AccordionDetails>
    </Accordion>
  );
}
