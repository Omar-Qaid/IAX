import React from 'react';
import { Accordion, AccordionDetails, AccordionSummary, Box, Typography } from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
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
    <Accordion
      defaultExpanded={defaultExpanded}
      expanded={expanded}
      onChange={(_, next) => onExpandedChange?.(next)}
      disableGutters
      elevation={0}
      sx={{
        mb: `${d365.sectionGap}px !important`,
        border: `1px solid ${d365.border}`,
        borderRadius: `${d365.sectionRadius}px !important`,
        bgcolor: d365.surface,
        overflow: 'hidden',
        '&::before': { display: 'none' },
      }}
    >
      <AccordionSummary
        expandIcon={<ExpandMoreIcon sx={{ fontSize: 16, color: '#323130' }} />}
        sx={{
          px: 1,
          minHeight: `${d365.sectionHeaderHeight}px !important`,
          borderBottom: `1px solid ${d365.border}`,
          '& .MuiAccordionSummary-content, & .MuiAccordionSummary-content.Mui-expanded': {
            my: 0.5,
            minWidth: 0,
          },
          '& .MuiAccordionSummary-expandIconWrapper': {
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            border: `1px solid ${d365.border}`,
            borderRadius: `${d365.radius}px`,
            p: '2px',
          },
        }}
      >
        <Typography
          sx={{
            fontFamily: d365.fontFamily,
            fontSize: 14,
            lineHeight: 1.2,
            color: '#111',
            fontWeight: 600,
            flexShrink: 0,
          }}
        >
          {title}
        </Typography>
        {summary && summary.length > 0 && (
          <Box
            sx={{
              display: 'flex',
              gap: 2,
              marginInlineStart: 'auto',
              paddingInlineEnd: 1.5,
              minWidth: 0,
              overflow: 'hidden',
            }}
          >
            {summary.map((item, index) => (
              <Typography
                key={index}
                component="span"
                noWrap
                sx={{ fontSize: d365.fontSize, color: d365.mutedText }}
              >
                {item}
              </Typography>
            ))}
          </Box>
        )}
      </AccordionSummary>
      <AccordionDetails sx={{ p: 1.25 }}>{children}</AccordionDetails>
    </Accordion>
  );
}
