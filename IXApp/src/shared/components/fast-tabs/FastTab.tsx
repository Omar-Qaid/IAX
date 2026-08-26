import React, { useState } from 'react';
import { Accordion, AccordionSummary, AccordionDetails, Typography, Box, Chip } from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ErrorIcon from '@mui/icons-material/Error';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export interface FastTabProps {
  id: string;
  title: string;
  summary?: string;
  defaultExpanded?: boolean;
  hasError?: boolean;
  required?: boolean;
  children: React.ReactNode;
}

export const FastTab: React.FC<FastTabProps> = ({
  title,
  summary,
  defaultExpanded = true,
  hasError = false,
  required = false,
  children,
}) => {
  const { t } = useAppTranslation();
  const [expanded, setExpanded] = useState(defaultExpanded);

  return (
    <Accordion expanded={expanded} onChange={() => setExpanded(!expanded)} disableGutters>
      <AccordionSummary expandIcon={<ExpandMoreIcon fontSize="small" />}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%', paddingInlineEnd: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Typography variant="subtitle2" color={hasError ? 'error.main' : 'text.primary'} sx={{ fontWeight: 700 }}>
              {title} {required && <Box component="span" color="error.main">*</Box>}
            </Typography>
            {hasError && (
              <Chip
                icon={<ErrorIcon fontSize="small" />}
                label={t('common.error_occurred')}
                size="small"
                color="error"
                sx={{ height: 20, fontSize: '0.65rem' }}
              />
            )}
          </Box>
          {summary && !expanded && (
            <Typography variant="caption" color="text.secondary" noWrap sx={{ maxWidth: 300 }}>
              {summary}
            </Typography>
          )}
        </Box>
      </AccordionSummary>
      <AccordionDetails>{children}</AccordionDetails>
    </Accordion>
  );
};
