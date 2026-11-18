import React, { useId, useState } from 'react';
import {
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Typography,
  Box,
  Chip,
  type SxProps,
  type Theme,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ErrorIcon from '@mui/icons-material/Error';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export interface FastTabProps {
  id?: string;
  title: string;
  summary?: React.ReactNode;
  defaultExpanded?: boolean;
  expanded?: boolean;
  onExpandedChange?: (expanded: boolean) => void;
  appearance?: 'standard' | 'enterprise' | 'legalEntity';
  /** Standard tabs show summaries when collapsed; enterprise tabs show them in both states. */
  summaryWhenCollapsed?: boolean;
  hasError?: boolean;
  required?: boolean;
  sx?: SxProps<Theme>;
  detailsSx?: SxProps<Theme>;
  children: React.ReactNode;
}

export const FastTab: React.FC<FastTabProps> = ({
  id,
  title,
  summary,
  defaultExpanded = true,
  expanded: controlledExpanded,
  onExpandedChange,
  appearance = 'standard',
  summaryWhenCollapsed = appearance === 'standard',
  hasError = false,
  required = false,
  sx,
  detailsSx,
  children,
}) => {
  const { t } = useAppTranslation();
  const generatedId = useId();
  const sectionId = id ?? generatedId;
  const [localExpanded, setLocalExpanded] = useState(defaultExpanded);
  const expanded = controlledExpanded ?? localExpanded;

  return (
    <Accordion
      id={sectionId}
      expanded={expanded}
      onChange={(_, next) => {
        if (controlledExpanded === undefined) setLocalExpanded(next);
        onExpandedChange?.(next);
      }}
      disableGutters
      square={appearance !== 'standard'}
      elevation={appearance === 'standard' ? undefined : 0}
      data-page-section={appearance === 'standard' ? undefined : appearance}
      sx={sx}
    >
      <AccordionSummary
        data-page-section={appearance === 'standard' ? undefined : appearance}
        id={`${sectionId}-header`}
        aria-controls={`${sectionId}-content`}
        expandIcon={<ExpandMoreIcon fontSize="small" />}
      >
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            width: '100%',
            minWidth: 0,
            paddingInlineEnd: 2,
            gap: 1,
          }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, minWidth: 0 }}>
            <Typography
              className="page-section-title"
              variant="subtitle2"
              color={hasError ? 'error.main' : 'text.primary'}
              sx={{ fontWeight: 700, overflowWrap: 'anywhere' }}
            >
              {title}{' '}
              {required && (
                <Box component="span" color="error.main">
                  *
                </Box>
              )}
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
          {summary != null && (!summaryWhenCollapsed || !expanded) && (
            <Box
              className="page-section-summary"
              sx={{
                minWidth: 0,
                maxWidth: appearance === 'standard' ? 300 : '60%',
                whiteSpace: appearance === 'standard' ? 'nowrap' : undefined,
                textOverflow: appearance === 'standard' ? 'ellipsis' : undefined,
                overflow: 'hidden',
                color: 'text.secondary',
                typography: 'caption',
              }}
            >
              {summary}
            </Box>
          )}
        </Box>
      </AccordionSummary>
      <AccordionDetails
        data-page-section={appearance === 'standard' ? undefined : appearance}
        sx={detailsSx}
      >
        {children}
      </AccordionDetails>
    </Accordion>
  );
};
